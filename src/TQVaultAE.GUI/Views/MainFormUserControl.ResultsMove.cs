using TQVaultAE.Application;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Components.Panels;
using TQVaultAE.GUI.Tooltips;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Views;

/// <summary>
/// The containers the results window moves items to (the stashes shown with the selected character, the vaults), and the
/// panels shown again after a move or an undo.
/// </summary>
public partial class MainFormUserControl
{
	/// <summary>
	/// A stash shown with the selected character, as a target: the transfer stash and the relic vault of its map, or its
	/// own stash. Null without a character or without that stash.
	/// </summary>
	internal ResultsMoveTarget? ResultsMoveStashTarget(StashType type)
		=> this.stashPanel.Player is null
			? null
			: this.Services.ResultsMove.StashTarget(type switch
			{
				StashType.TransferStash => this.stashPanel.TransferStash,
				StashType.RelicVaultStash => this.stashPanel.RelicVaultStash,
				_ => this.stashPanel.Stash,
			}, type);

	/// <summary>The vaults the main content knows, by name.</summary>
	internal IReadOnlyList<string> ResultsMoveVaults()
		=> [.. this.KnownVaultNames.Order(StringComparer.CurrentCultureIgnoreCase)];

	/// <summary>
	/// A vault by its name: the one a vault panel shows, else the one already read, else read now (the vault list and
	/// panels stay as they are). Null when it cannot be read.
	/// </summary>
	internal PlayerCollection? ResultsMoveVault(string name)
	{
		foreach (var shown in new[] { this.vaultPanel.Player, this.secondaryVaultPanel.Player })
			if (shown is not null && string.Equals(shown.PlayerName, name, StringComparison.OrdinalIgnoreCase))
				return shown;

		var result = this.Services.VaultService.LoadVault(name);
		if ((result.ArgumentException as Exception ?? result.Error) is { } error)
			this.Services.MessageBoxes.Inform(LoadMessages.ReadFailure(result.Filename, error.Message), Resources.GlobalError, MessageBoxIcon.Error);

		return result.Vault;
	}

	/// <summary>A vault bag as a target, or null.</summary>
	internal ResultsMoveTarget? ResultsMoveVaultTarget(PlayerCollection vault, int bag)
		=> this.Services.ResultsMove.VaultTarget(vault, bag, this.vaultPanel.SackPanel.SackSize);

	/// <summary>
	/// Before a move or an undo: the item held with the mouse goes back, as before a loadout load. False when it cannot
	/// (a new or modified item), with a message.
	/// </summary>
	internal bool ReadyForResultsMove()
	{
		this.PutBackHeldItem();
		if (!this.DragInfo.IsActive)
			return true;

		this.Services.UIService.NotifyUser(Resources.ResultsMoveHeldItem, TQColor.Red);
		return false;
	}

	/// <summary>
	/// After a move or an undo: the items read again get new tool tips and search names, the bags changed new bag tool
	/// tips, the worn equipment its bonuses, the loadout bar its pieces, and the panels are drawn again.
	/// </summary>
	internal void ResultsMoved(ResultsMoveOutcome outcome)
	{
		Item[] readAgain = [.. outcome.ReadAgain, .. outcome.KeptMap];
		if (readAgain.Length > 0)
		{
			ItemCaches.Invalidate(readAgain, this.Services.ItemText);
			this.Services.ItemDatabaseService.RefreshItems(readAgain);
		}

		BagButtonTooltip.InvalidateCache([.. outcome.ChangedSacks]);
		this.vaultPanel.SackPanel.NotifyKeptMap(outcome.KeptMap);

		var shown = this.stashPanel.Player;
		if (shown is not null && outcome.Wearers.Contains(shown))
		{
			this.stashPanel.EquipmentPanel.GetGearStatBonus();
			this.stashPanel.EquipmentReplaced();
		}

		if (shown is not null && outcome.LoadoutsChanged.Any(l => string.Equals(Path.GetDirectoryName(l.FilePath), Path.GetDirectoryName(shown.PlayerFile), StringComparison.OrdinalIgnoreCase)))
			this.stashPanel.RefreshLoadouts();

		this.vaultPanel.Refresh();
		this.secondaryVaultPanel.Refresh();
		this.playerPanel.Refresh();
		this.stashPanel.Refresh();
	}
}
