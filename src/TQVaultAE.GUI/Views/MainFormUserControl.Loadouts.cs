using System.Globalization;
using TQVaultAE.Application.Results;
using TQVaultAE.Application.Session;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Components.Character;
using TQVaultAE.GUI.Components.Panels;
using TQVaultAE.GUI.Dialogs;
using TQVaultAE.GUI.Tooltips;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Views;

/// <summary>
/// The loadouts of the shown character (saved equipment sets), asked from the loadout bar under the statistics panel.
/// </summary>
public partial class MainFormUserControl
{
	private void LoadoutActionCallback(object? sender, ListBarEventArgs<LoadoutBarAction> e)
	{
		switch (e.Action)
		{
			case LoadoutBarAction.Add:
				this.AddLoadout();
				break;
			case LoadoutBarAction.Update when e.Name is not null:
				this.UpdateLoadout(e.Name);
				break;
			case LoadoutBarAction.Rename when e.Name is not null:
				this.RenameLoadout(e.Name);
				break;
			case LoadoutBarAction.Copy when e.Name is not null:
				this.CopyLoadout(e.Name);
				break;
			case LoadoutBarAction.Delete when e.Name is not null:
				this.DeleteLoadout(e.Name);
				break;
			case LoadoutBarAction.Swap when e.Name is not null:
				this.SwapLoadout(e.Name);
				break;
			case LoadoutBarAction.EmptyToInventory or LoadoutBarAction.EmptyToStash or LoadoutBarAction.EmptyToVault when e.Name is not null:
				this.EmptyLoadout(e.Name, e.Action);
				break;
		}
	}

	/// <summary>The shown character and its loadouts; null (told to the user) when they cannot change.</summary>
	private (PlayerCollection Player, LoadoutSet Loadouts)? ChangeableLoadouts()
	{
		if (this.stashPanel.Player is not { } player
			|| this.Services.Loadouts.Find(Path.GetDirectoryName(player.PlayerFile) ?? string.Empty) is not { } loadouts)
			return null;

		if (loadouts.ReadFailed)
		{
			this.Services.UIService.NotifyUser(string.Format(CultureInfo.CurrentCulture, Resources.LoadoutReadFailed, loadouts.CharacterName), TQColor.Red);
			return null;
		}

		if (player.IsReadOnly)
		{
			this.Services.UIService.NotifyUser(string.Format(CultureInfo.CurrentCulture, Resources.CharacterReadOnlyRefused, loadouts.CharacterName), TQColor.Red);
			return null;
		}

		return (player, loadouts);
	}

	/// <summary>
	/// Asks a loadout name until it is valid or the dialog is cancelled, as Duplicate Character does.
	/// </summary>
	/// <param name="loadouts">The character's loadouts.</param>
	/// <param name="proposed">The name shown first.</param>
	/// <param name="current">The loadout being renamed.</param>
	/// <returns>The name, or null when cancelled.</returns>
	private string? AskLoadoutName(LoadoutSet loadouts, string proposed, string? current = null)
		=> InputDialog.AskSetName(this.Services, Resources.LoadoutNamePrompt, Resources.LoadoutTitle, proposed
			, name => this.Services.Loadouts.ValidateName(loadouts, name, current)
			, (name, error) => this.LoadoutNameMessage(loadouts, name, error));

	private string LoadoutNameMessage(LoadoutSet loadouts, string name, SetNameError error)
		=> error == SetNameError.AlreadyUsed
			? string.Format(CultureInfo.CurrentCulture, Resources.LoadoutNameUsed, loadouts.CharacterName, loadouts.Find(name)?.Name ?? name)
			: Resources.LoadoutNameEmpty;

	/// <summary>
	/// True when the game is not running, or the user goes on although it is: the game keeps the character it plays in
	/// memory and its next save replaces the changes.
	/// </summary>
	private bool GoOnThoughGameRuns()
		=> !this.IsGameRunning()
			|| this.Services.MessageBoxes.Confirm(Resources.LoadoutGameRunning, Resources.LoadoutTitle, MessageBoxIcon.Warning, defaultNo: true);

	private bool IsGameRunning()
	{
#if DEBUG
		if (Automation.AutomationTools.GameRunningOverride is { } running)
			return running;
#endif
		return this.Services.GameProcess.IsGameRunning();
	}

	/// <summary>An item held from the equipment would be left out of the recorded or swapped gear: it goes back first.</summary>
	private void PutBackHeldItem()
	{
		if (this.DragInfo.IsActive && this.DragInfo.CanBeCanceled)
			this.DragInfo.Cancel();
	}

	/// <summary>Shows the outcome of a change: an error box, or the loadouts again with <paramref name="select"/> selected.</summary>
	private bool ShowLoadoutChange(LoadoutSet loadouts, LoadoutChange change, string? select, string? name = null)
	{
		if (change.NameError != SetNameError.None)
			this.Services.MessageBoxes.Inform(this.LoadoutNameMessage(loadouts, name ?? string.Empty, change.NameError), Resources.LoadoutTitle, MessageBoxIcon.Error);
		else if (change.Error is not null)
			this.Services.MessageBoxes.Inform(string.Format(CultureInfo.CurrentCulture, Resources.LoadoutWriteFailed, loadouts.CharacterName, change.Error.Message), Resources.LoadoutTitle, MessageBoxIcon.Error);

		this.stashPanel.RefreshLoadouts(change.IsSuccess ? select : null);
		return change.IsSuccess;
	}

	/// <summary>Adds an empty loadout, under the name asked.</summary>
	private bool AddLoadout()
	{
		if (this.ChangeableLoadouts() is not var (_, loadouts))
			return false;

		var name = this.AskLoadoutName(loadouts, string.Empty);
		if (name is null)
			return false;

		this.PutBackHeldItem();
		return this.ShowLoadoutChange(loadouts, this.Services.Loadouts.Add(loadouts, name), name.Trim(), name);
	}

	/// <summary>Whether the Allow Item Copying setting refuses Update and Copy (told to the user): they write copies.</summary>
	private bool LoadoutCopyRefused()
	{
		if (StashPanel.LoadoutCopyLocked(this.Services.Settings.AllowItemCopy) is not { } reason)
			return false;

		this.Services.UIService.NotifyUser(reason, TQColor.Red);
		return true;
	}

	/// <summary>Merges the worn equipment into a loadout (as copies), once confirmed.</summary>
	/// <param name="name">The loadout.</param>
	private bool UpdateLoadout(string name)
	{
		if (this.LoadoutCopyRefused() || this.ChangeableLoadouts() is not var (player, loadouts) || loadouts.Find(name) is not { } loadout)
			return false;

		if (!this.Services.MessageBoxes.Confirm(string.Format(CultureInfo.CurrentCulture, Resources.LoadoutUpdateConfirm, loadout.Name), Resources.LoadoutTitle, MessageBoxIcon.Question, defaultNo: true)
			|| !this.GoOnThoughGameRuns())
			return false;

		this.PutBackHeldItem();
		return this.ShowLoadoutChange(loadouts, this.Services.Loadouts.Update(loadouts, loadout.Name, player), loadout.Name);
	}

	/// <summary>Renames a loadout, to the name asked.</summary>
	/// <param name="name">The loadout.</param>
	private bool RenameLoadout(string name)
	{
		if (this.ChangeableLoadouts() is not var (_, loadouts) || loadouts.Find(name) is not { } loadout)
			return false;

		var newName = this.AskLoadoutName(loadouts, loadout.Name, loadout.Name);
		if (newName is null)
			return false;

		return this.ShowLoadoutChange(loadouts, this.Services.Loadouts.Rename(loadouts, loadout.Name, newName), newName.Trim(), newName);
	}

	/// <summary>Copies a loadout under the name asked.</summary>
	/// <param name="name">The loadout.</param>
	private bool CopyLoadout(string name)
	{
		if (this.LoadoutCopyRefused() || this.ChangeableLoadouts() is not var (_, loadouts) || loadouts.Find(name) is not { } loadout)
			return false;

		var newName = this.AskLoadoutName(loadouts, loadout.Name);
		if (newName is null)
			return false;

		return this.ShowLoadoutChange(loadouts, this.Services.Loadouts.Copy(loadouts, loadout.Name, newName), newName.Trim(), newName);
	}

	/// <summary>
	/// Gear swap: the loadout's pieces go on the character and the worn ones take their place in the loadout, none copied.
	/// Refused for a read-only character or with the PlayerReadonly setting; asks first when the game runs.
	/// </summary>
	/// <param name="name">The loadout.</param>
	private bool SwapLoadout(string name)
	{
		if (this.ChangeableLoadouts() is not var (player, loadouts) || loadouts.Find(name) is not { } loadout)
			return false;

		if (this.Services.Settings.PlayerReadonly)
		{
			this.Services.UIService.NotifyUser(string.Format(CultureInfo.CurrentCulture, Resources.LoadoutEquipmentLocked, Resources.SettingsPlayerReadonly), TQColor.Red);
			return false;
		}

		if (!this.GoOnThoughGameRuns())
			return false;

		this.PutBackHeldItem();
		var result = this.Services.Loadouts.Swap(loadouts, loadout.Name, player);
		switch (result.Outcome)
		{
			case LoadoutSwapOutcome.NothingToSwap:
				this.Services.UIService.NotifyUser(string.Format(CultureInfo.CurrentCulture, Resources.LoadoutSwapNothing, loadout.Name));
				return false;
			case LoadoutSwapOutcome.Refused:
				this.Services.UIService.NotifyUser(string.Format(CultureInfo.CurrentCulture, Resources.CharacterReadOnlyRefused, loadouts.CharacterName), TQColor.Red);
				return false;
		}

		// Both panels show the new pieces: the equipment (statistics, marks) and the loadout panel.
		this.ShownEquipmentReplaced();
		this.stashPanel.RefreshLoadouts(loadout.Name);
		this.Services.UIService.NotifyUser(string.Format(CultureInfo.CurrentCulture, Resources.LoadoutSwapped, loadout.Name, result.PutOn, result.TakenOff));
		return true;
	}

	/// <summary>A bag receiving loadout pieces: its panel, sack, size in cells, place, and its name for the status line.</summary>
	private sealed record LoadoutBag(VaultPanel Panel, SackCollection Sack, Size Size, ContainerPlace Place, string Name);

	/// <summary>The bag shown in the vault panel, or null without a vault.</summary>
	private LoadoutBag? ShownVaultBag()
	{
		int bagNumber = this.vaultPanel.CurrentBag;
		if (this.vaultPanel.Player is not { } vault || vault.GetSack(bagNumber) is not { } bag)
			return null;

		return new LoadoutBag(this.vaultPanel, bag, this.vaultPanel.SackPanel.SackSize
			, this.Services.ItemMovementService.PlaceIn(vault, SackType.Vault, bagNumber)
			, string.Format(CultureInfo.CurrentCulture, Resources.LoadoutBagOf, bagNumber + 1, vault.PlayerName));
	}

	/// <summary>
	/// The bag an Empty loadout entry sends the pieces to, or null (told to the user): the character's inventory (refused
	/// with the PlayerReadonly setting), the stash as an auto-move finds it (the transfer stash while the equipment shows),
	/// or the bag of the vault shown.
	/// </summary>
	private LoadoutBag? LoadoutEmptyTarget(PlayerCollection player, LoadoutBarAction target)
	{
		switch (target)
		{
			case LoadoutBarAction.EmptyToInventory:
				if (this.Services.Settings.PlayerReadonly)
				{
					this.Services.UIService.NotifyUser(string.Format(CultureInfo.CurrentCulture, Resources.LoadoutInventoryLocked, Resources.SettingsPlayerReadonly), TQColor.Red);
					return null;
				}

				// The main bag (or the bag shown without one), as the auto-move to the player panel.
				var inventoryPanel = this.playerPanel.InventoryTarget;
				return player.GetSack(inventoryPanel.CurrentSack) is { } inventory
					? new LoadoutBag(this.playerPanel, inventory, inventoryPanel.SackSize
						, this.Services.ItemMovementService.PlaceIn(player, SackType.Player, inventoryPanel.CurrentSack)
						, string.Format(CultureInfo.CurrentCulture, Resources.LoadoutInventoryOf, player.PlayerName))
					: null;

			case LoadoutBarAction.EmptyToStash:
				var stashTarget = AutoMovePlanner.StashTarget(this.stashPanel.CurrentBag, this.stashPanel.Stash is not null
					, this.stashPanel.TransferStash is not null, this.stashPanel.RelicVaultStash is not null);
				if (!stashTarget.IsAvailable || this.stashPanel.StashOf(stashTarget.Bag) is not { Sack: { } sack } stash)
				{
					this.Services.UIService.NotifyUser(Resources.LoadoutNoStash, TQColor.Red);
					return null;
				}

				return new LoadoutBag(this.stashPanel, sack, new Size(stash.Width, stash.Height)
					, this.Services.ItemMovementService.PlaceIn(stash, (StashType)stashTarget.Bag), StashPanel.BagName(stashTarget.Bag));

			default:
				if (this.ShownVaultBag() is { } vaultBag)
					return vaultBag;

				this.Services.UIService.NotifyUser(Resources.LoadoutNoVaultForPieces, TQColor.Red);
				return null;
		}
	}

	/// <summary>
	/// Empty loadout: moves the loadout's pieces (none copied) to the inventory, the stash or the vault bag shown; a piece
	/// without room stays in the loadout and the user is told. The bag and the loadouts are saved as after a move by hand.
	/// </summary>
	/// <param name="name">The loadout.</param>
	/// <param name="target">The menu entry: <see cref="LoadoutBarAction.EmptyToInventory"/>, <see cref="LoadoutBarAction.EmptyToStash"/>
	/// or <see cref="LoadoutBarAction.EmptyToVault"/>.</param>
	private bool EmptyLoadout(string name, LoadoutBarAction target)
	{
		if (this.ChangeableLoadouts() is not var (player, loadouts) || loadouts.Find(name) is not { } loadout
			|| this.LoadoutEmptyTarget(player, target) is not { } bag)
			return false;

		this.PutBackHeldItem();
		var result = this.Services.Loadouts.Empty(loadouts, loadout.Name, bag.Sack, bag.Size, bag.Place);
		switch (result.Outcome)
		{
			case LoadoutEmptyOutcome.NothingToEmpty:
				this.Services.UIService.NotifyUser(string.Format(CultureInfo.CurrentCulture, Resources.LoadoutEmptyNothing, loadout.Name));
				return false;
			case LoadoutEmptyOutcome.NoRoom:
				this.Services.UIService.NotifyUser(string.Format(CultureInfo.CurrentCulture, Resources.LoadoutEmptyNoRoom, bag.Name, result.Left), TQColor.Red);
				return false;
			case LoadoutEmptyOutcome.Refused:
				this.Services.UIService.NotifyUser(string.Format(CultureInfo.CurrentCulture, Resources.LoadoutEmptyRefused, bag.Name), TQColor.Red);
				return false;
		}

		// The pieces take the map rules of their new place, as dropped items; then the bag and the loadout panel show it.
		bag.Panel.AdoptVaultMod(bag.Sack, result.Moved, bag.Size);
		BagButtonTooltip.InvalidateCache(bag.Sack);
		bag.Panel.Refresh();
		this.stashPanel.RefreshLoadouts(loadout.Name);
		if (result.Outcome == LoadoutEmptyOutcome.PartlyEmptied)
			this.Services.UIService.NotifyUser(string.Format(CultureInfo.CurrentCulture, Resources.LoadoutEmptyPartly, loadout.Name, result.Pieces, bag.Name, result.Left), TQColor.Red);
		else
			this.Services.UIService.NotifyUser(string.Format(CultureInfo.CurrentCulture, Resources.LoadoutEmptied, loadout.Name, result.Pieces, bag.Name));
		return true;
	}

	/// <summary>Deletes a loadout, once confirmed.</summary>
	/// <param name="name">The loadout.</param>
	private bool DeleteLoadout(string name)
	{
		if (this.ChangeableLoadouts() is not var (_, loadouts) || loadouts.Find(name) is not { } loadout)
			return false;

		if (!this.Services.MessageBoxes.Confirm(string.Format(CultureInfo.CurrentCulture, Resources.LoadoutDeleteConfirm, loadout.Name), Resources.LoadoutTitle, MessageBoxIcon.Question, defaultNo: true))
			return false;

		return this.ShowLoadoutChange(loadouts, this.Services.Loadouts.Delete(loadouts, loadout.Name), null);
	}
}
