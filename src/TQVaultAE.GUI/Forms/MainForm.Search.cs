using Microsoft.Extensions.Logging;
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

	/// <summary>
	/// Opens the search dialog and shows its results.
	/// </summary>
	private void OpenSearchDialog()
	{
		if (this._searchDialog is null || this._searchDialog.IsDisposed)
		{
			var watch = System.Diagnostics.Stopwatch.StartNew();
			this._searchDialog = this.Services.Forms.Create<SearchDialogAdvanced>();
			this.Log.LogInformation("Search dialog created in {Elapsed} ms", watch.ElapsedMilliseconds);
			this.FormClosed += (_, _) => this._searchDialog?.Dispose();
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

		// The transfer and relic stashes are shared by every character: no character to switch to.
		bool sharedStash = selectedResult.StashType is StashType.TransferStash or StashType.RelicVaultStash;

		if (selectedResult.SackType == SackType.Vault)
		{
			// Switch to the selected vault
			this.vaultListComboBox.SelectedItem = selectedResult.ContainerName;

			// A vault no longer listed (deleted, or open in the secondary panel): do not select a cell of another vault.
			if (!string.Equals(this.vaultPanel.Player?.PlayerName, selectedResult.ContainerName, StringComparison.OrdinalIgnoreCase))
				return;

			this.vaultPanel.CurrentBag = selectedResult.SackNumber;
			this.vaultPanel.SackPanel.SelectItem(selectedResult.FriendlyNames.Item.Location);
		}
		else if (!sharedStash && selectedResult.SackType is SackType.Player or SackType.Equipment or SackType.Stash)
		{
			// The character by its exact name: a prefix search would pick "HeroB Two" for "HeroB".
			var save = this.comboBoxCharacter.Items.OfType<PlayerSave>()
				.FirstOrDefault(ps => string.Equals(ps.Name, selectedResult.ContainerName, StringComparison.OrdinalIgnoreCase));
			if (save is null)
				return;

			// Switch to the selected player
			if (this.showSecondaryVault)
			{
				this.showSecondaryVault = !this.showSecondaryVault;
				this.UpdateTopPanel();
			}

			// Update the selection list and load the character.
			this.comboBoxCharacter.SelectedItem = save;

			// Bail if we are attempting to highlight something in the stash panel and the stash does not exist.
			if (this.stashPanel.SackPanel is null && selectedResult.SackType != SackType.Player)
				return;

			if (selectedResult.SackType == SackType.Player)
			{
				// Highlight the item if it's in the player inventory.
				if (selectedResult.SackNumber == 0)
					this.playerPanel.SackPanel.SelectItem(selectedResult.FriendlyNames.Item.Location);
				else
				{
					this.playerPanel.CurrentBag = selectedResult.SackNumber - 1;
					this.playerPanel.BagSackPanel.SelectItem(selectedResult.FriendlyNames.Item.Location);
				}
			}
			else
			{
				this.stashPanel.CurrentBag = selectedResult.SackNumber;
				this.stashPanel.SackPanel?.SelectItem(selectedResult.FriendlyNames.Item.Location);
			}
		}
		else if (sharedStash)
		{
			// Bail when no character is loaded: there is no stash panel then.
			if (this.stashPanel.SackPanel is null)
				return;

			// The player panel shows the stashes: leave the secondary vault.
			if (this.showSecondaryVault)
			{
				this.showSecondaryVault = !this.showSecondaryVault;
				this.UpdateTopPanel();
			}

			// Switch to the Stash bag
			this.stashPanel.CurrentBag = selectedResult.SackNumber;
			this.stashPanel.SackPanel?.SelectItem(selectedResult.FriendlyNames.Item.Location);
		}
	}



}
