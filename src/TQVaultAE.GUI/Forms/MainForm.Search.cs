using Microsoft.Extensions.Logging;
using System.Globalization;
using TQVaultAE.Application;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Dialogs;
using TQVaultAE.GUI.Models;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Forms;

public partial class MainForm
{
	/// <summary>
	/// Handler for clicking the search button on the form.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void SearchButtonClick(object? sender, EventArgs e)
	{
		this.OpenSearchDialog();
	}

	/// <summary>
	/// Search dialog kept between openings: building its thousands of category entries is the slow part,
	/// so it is created once and only rebuilt when the items change. Disposed with the main form.
	/// </summary>
	private SearchDialogAdvanced? _searchDialog;

	/// <summary>Whether the search dialog is disposed with the main form; it may be made again (zoom change).</summary>
	private bool _searchDialogDisposedOnClose;

	/// <summary>
	/// Opens the search dialog and shows its results.
	/// </summary>
	private void OpenSearchDialog()
	{
		// A dialog made at another zoom is made again at this one, scaled once from its design like the other dialogs.
		if (this._searchDialog is { IsDisposed: false } stale && stale.ScaledZoom != UIService.Zoom)
		{
			stale.Dispose();
			this._searchDialog = null;
		}

		if (this._searchDialog is null || this._searchDialog.IsDisposed)
		{
			var watch = System.Diagnostics.Stopwatch.StartNew();
			this._searchDialog = this.Services.Forms.Create<SearchDialogAdvanced>();
			this._searchDialog.ApplyZoom(UIService.Zoom);
			this.Log.LogInformation("Search dialog created in {Elapsed} ms", watch.ElapsedMilliseconds);
			if (!this._searchDialogDisposedOnClose)
			{
				this._searchDialogDisposedOnClose = true;
				this.FormClosed += (_, _) => this._searchDialog?.Dispose();
			}
		}

		var searchDialog = this._searchDialog;
		var result = searchDialog.ShowDialog();

		if (result == DialogResult.OK && searchDialog.QueryResults.Length != 0)
			this.DisplayResults(null, searchDialog.QueryResults);
	}

	/// <summary>
	/// Handler for keypresses within the search text box.
	/// Used to handle the resizing hot keys.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">KeyEventArgs data</param>
	private void SearchTextBoxKeyDown(object? sender, KeyEventArgs e)
	{
		if (e.KeyData == (Keys.Control | Keys.Add) || e.KeyData == (Keys.Control | Keys.Oemplus))
		{
			this.ResizeFormCallback(this, new ResizeEventArgs(0.1F));
		}

		if (e.KeyData == (Keys.Control | Keys.Subtract) || e.KeyData == (Keys.Control | Keys.OemMinus))
		{
			this.ResizeFormCallback(this, new ResizeEventArgs(-0.1F));
		}

		if (e.KeyData == (Keys.Control | Keys.Home))
		{
			this.ResizeFormCallback(this, new ResizeEventArgs(1.0F));
		}
	}

	private void DisplayResults(string? searchString, IEnumerable<SearchResult> results)
	{
		if (results is null || !results.Any())
		{
			this.Services.MessageBoxes.Inform(string.Format(Resources.MainFormNoItemsFound, searchString), Resources.MainFormNoItemsFound2, MessageBoxIcon.Information);
			return;
		}

		// Display a dialog with the results.
		ResultsDialog dlg = this.Services.Forms.Create<ResultsDialog>();
		dlg.ResultChanged += new ResultsDialog.EventHandler<ResultChangedEventArgs>(this.SelectResult);
		dlg.ResultsList.Clear();
		dlg.ResultsList.AddRange(results);
		dlg.SearchString = searchString ?? string.Empty;
		dlg.Show();
	}

	/// <summary>
	/// Selects the item highlighted in the results list.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">ResultChangedEventArgs data</param>
	private void SelectResult(object? sender, ResultChangedEventArgs e)
	{
		SearchResult selectedResult = e.Result;
		if (selectedResult is null || selectedResult.FriendlyNames is null) return;

		this.ClearAllItemsSelectedCallback(this, new SackPanelEventArgs(null, null));

		var location = SearchResultNavigation.Resolve(selectedResult);

		if (location.Panel == ResultPanelKind.Vault)
		{
			// Switch to the selected vault
			this.vaultListComboBox.SelectedItem = selectedResult.ContainerName;

			// A vault no longer listed (deleted, or open in the secondary panel): do not select a cell of another vault.
			if (!string.Equals(this.vaultPanel.Player?.PlayerName, selectedResult.ContainerName, StringComparison.OrdinalIgnoreCase))
				return;

			this.vaultPanel.CurrentBag = location.Bag;
			this.vaultPanel.SackPanel.SelectItem(location.Cell);
			return;
		}

		if (location.NeedsCharacter)
		{
			// The character by its exact name: a prefix search would pick "HeroB Two" for "HeroB".
			var save = this.comboBoxCharacter.Items.OfType<PlayerSave>()
				.FirstOrDefault(ps => string.Equals(ps.Name, selectedResult.ContainerName, StringComparison.OrdinalIgnoreCase));
			if (save is null)
				return;

			this.LeaveSecondaryVault();

			// Update the selection list and load the character.
			this.comboBoxCharacter.SelectedItem = save;
		}
		else if (location.Panel == ResultPanelKind.SharedStash)
		{
			// The stash panel shows the shared stashes of the shown character's map: another map's transfer stash or
			// relic vault needs a character played with that map, selected like a character's result.
			var selected = this.comboBoxCharacter.SelectedItem as PlayerSave;
			var save = this.gameMapService.CharacterShowing(selectedResult.Container, this.ListedCharacters(), selected);
			if (save is null)
			{
				this.UIService.NotifyUser(string.Format(CultureInfo.CurrentCulture, Resources.SearchSharedStashNoCharacter, selectedResult.ContainerName), TQColor.Red);
				return;
			}

			// The player panel shows the stashes: leave the secondary vault.
			this.LeaveSecondaryVault();

			if (save != selected)
				this.comboBoxCharacter.SelectedItem = save;

			// Bail when the character could not be loaded: there is no stash panel then.
			if (this.stashPanel.SackPanel is null)
				return;
		}
		else
		{
			return;
		}

		switch (location.Panel)
		{
			case ResultPanelKind.PlayerMainSack:
				this.playerPanel.SackPanel.SelectItem(location.Cell);
				break;

			case ResultPanelKind.PlayerBag:
				this.playerPanel.CurrentBag = location.Bag;
				this.playerPanel.BagSackPanel.SelectItem(location.Cell);
				break;

			default:
				// Bail if we are attempting to highlight something in the stash panel and the stash does not exist.
				if (this.stashPanel.SackPanel is null)
					return;

				this.stashPanel.CurrentBag = location.Bag;
				this.stashPanel.SackPanel?.SelectItem(location.Cell);
				break;
		}
	}

	/// <summary>
	/// Shows the player panel again when the secondary vault is shown in its place.
	/// </summary>
	private void LeaveSecondaryVault()
	{
		if (this.showSecondaryVault)
		{
			this.showSecondaryVault = !this.showSecondaryVault;
			this.UpdateTopPanel();
		}
	}



}
