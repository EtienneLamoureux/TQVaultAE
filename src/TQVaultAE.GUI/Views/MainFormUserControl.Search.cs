using System.Globalization;
using TQVaultAE.Application;
using TQVaultAE.Application.Results;
using TQVaultAE.Application.Search;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Models.Search;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Views;

/// <summary>
/// The jump to a search result: the main content shows the result's container, bag and item.
/// </summary>
public partial class MainFormUserControl
{
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
				// No panel shows a loadout's pieces: the equipment tab with the loadout selected in its bar.
				this.stashPanel.CurrentBag = location.Bag;
				this.stashPanel.RefreshLoadouts(selectedResult.LoadoutName);
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
