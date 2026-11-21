using System.Globalization;
using TQVaultAE.Application.Results;
using TQVaultAE.Application.Search;
using TQVaultAE.Application.Session;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Components.Panels;
using TQVaultAE.GUI.Dialogs;
using TQVaultAE.GUI.Models.Search;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Views;

/// <summary>
/// The search results of the main content: the results window, and the jump to a result, where the main content shows
/// the result's container, bag and item.
/// </summary>
public partial class MainFormUserControl : IItemCopySearchHost
{
	/// <summary>
	/// Shows the items matching <paramref name="item"/> for the item menu's <i>Search</i> in the results window, the item
	/// first, marked as the source; a click on a result shows it like an Advanced Search result.
	/// </summary>
	/// <param name="item">The item clicked.</param>
	/// <param name="kind">The search.</param>
	public void ShowItemCopies(Item item, CopySearchKind kind)
	{
		var results = this.Services.ItemDatabaseService.FindCopies(item, kind);
		if (results.Count == 0)
			return;

		var source = results[0];
		string title = ItemCopySearchText.Title(kind, source.ItemName, results.Count - 1);
		this.ShowResults(null, results, title, source);
	}

	/// <summary>
	/// Shows <paramref name="results"/> in a results window over the main window, or says that nothing was found; a click
	/// on a result shows it in the main content, and its <i>Move To</i> acts on the main content's containers.
	/// </summary>
	internal void ShowResults(string? searchString, IEnumerable<SearchResult> results, string? title = null, SearchResult? source = null)
	{
		if (results is null || !results.Any())
		{
			this.Services.MessageBoxes.Inform(string.Format(Resources.MainFormNoItemsFound, searchString), Resources.MainFormNoItemsFound2, MessageBoxIcon.Information);
			return;
		}

		var dialog = this.Services.Forms.Create<ResultsDialog>();
		dialog.MoveHost = this;
		dialog.ResultChanged += (_, e) => this.ShowSearchResult(e.Result);
		dialog.ResultsList.Clear();
		dialog.ResultsList.AddRange(results);
		dialog.SearchString = searchString ?? string.Empty;
		dialog.Title = title;
		dialog.SourceResult = source;
		dialog.Show();
	}

	/// <summary>
	/// Shows a search result: selects its vault or character (which loads it), its bag and its item.
	/// </summary>
	internal void ShowSearchResult(SearchResult selectedResult)
	{
		if (selectedResult is null || selectedResult.FriendlyNames is null) return;

		this.ClearAllItemsSelected();

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
			var save = this.Services.GameMapService.CharacterShowing(selectedResult.Container, this.ListedCharacters(), selected);
			if (save is null)
			{
				this.Services.UIService.NotifyUser(string.Format(CultureInfo.CurrentCulture, Resources.SearchSharedStashNoCharacter, SharedStashText.Localize(selectedResult.ContainerName, selectedResult.StashType)), TQColor.Red);
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

			case ResultPanelKind.Loadout:
				// The equipment tab with the loadout selected in its bar and shown in the loadout panel.
				this.stashPanel.CurrentBag = location.Bag;
				this.stashPanel.ShowLoadoutPanel(selectedResult.LoadoutName);
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
			this.ShowSecondaryVault(false);
	}
}
