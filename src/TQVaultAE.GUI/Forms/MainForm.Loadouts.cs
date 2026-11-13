using System.Globalization;
using TQVaultAE.Application;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Components.Character;
using TQVaultAE.GUI.Dialogs;
using TQVaultAE.GUI.Tooltips;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Forms;

/// <summary>
/// The loadouts of the shown character (saved equipment sets), asked from the loadout bar under the statistics panel.
/// </summary>
public partial class MainForm
{
	private void LoadoutActionCallback(object? sender, LoadoutBarEventArgs e)
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
			case LoadoutBarAction.Load when e.Name is not null:
				this.LoadLoadout(e.Name);
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
			this.UIService.NotifyUser(string.Format(CultureInfo.CurrentCulture, Resources.LoadoutReadFailed, loadouts.CharacterName), TQColor.Red);
			return null;
		}

		if (player.IsReadOnly)
		{
			this.UIService.NotifyUser(string.Format(CultureInfo.CurrentCulture, Resources.CharacterReadOnlyRefused, loadouts.CharacterName), TQColor.Red);
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
	{
		while (true)
		{
			var name = InputDialog.Show(Resources.LoadoutNamePrompt, Resources.LoadoutTitle, proposed).Trim();
			if (name.Length == 0)
				return null;

			var error = this.Services.Loadouts.ValidateName(loadouts, name, current);
			if (error == LoadoutNameError.None)
				return name;

			this.Services.MessageBoxes.Inform(this.LoadoutNameMessage(loadouts, name, error), Resources.LoadoutTitle, MessageBoxIcon.Error);
			proposed = name;
		}
	}

	private string LoadoutNameMessage(LoadoutSet loadouts, string name, LoadoutNameError error)
		=> error == LoadoutNameError.AlreadyUsed
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

	/// <summary>An item held from the equipment would be left out of the recorded or loaded gear: it goes back first.</summary>
	private void PutBackHeldItem()
	{
		if (this.DragInfo.IsActive && this.DragInfo.CanBeCanceled)
			this.DragInfo.Cancel();
	}

	/// <summary>Shows the outcome of a change: an error box, or the loadouts again with <paramref name="select"/> selected.</summary>
	private bool ShowLoadoutChange(LoadoutSet loadouts, LoadoutChange change, string? select, string? name = null)
	{
		if (change.NameError != LoadoutNameError.None)
			this.Services.MessageBoxes.Inform(this.LoadoutNameMessage(loadouts, name ?? string.Empty, change.NameError), Resources.LoadoutTitle, MessageBoxIcon.Error);
		else if (change.Error is not null)
			this.Services.MessageBoxes.Inform(string.Format(CultureInfo.CurrentCulture, Resources.LoadoutWriteFailed, loadouts.CharacterName, change.Error.Message), Resources.LoadoutTitle, MessageBoxIcon.Error);

		this.stashPanel.RefreshLoadouts(change.IsSuccess ? select : null);
		return change.IsSuccess;
	}

	/// <summary>Records the worn equipment as a new loadout.</summary>
	/// <param name="name">The name; asked when null.</param>
	internal bool AddLoadout(string? name = null)
	{
		if (this.ChangeableLoadouts() is not var (player, loadouts))
			return false;

		name ??= this.AskLoadoutName(loadouts, string.Empty);
		if (name is null)
			return false;

		this.PutBackHeldItem();
		return this.ShowLoadoutChange(loadouts, this.Services.Loadouts.Add(loadouts, name, player), name.Trim(), name);
	}

	/// <summary>Records the worn equipment over a loadout, once confirmed.</summary>
	/// <param name="name">The loadout.</param>
	internal bool UpdateLoadout(string name)
	{
		if (this.ChangeableLoadouts() is not var (player, loadouts) || loadouts.Find(name) is not { } loadout)
			return false;

		if (!this.Services.MessageBoxes.Confirm(string.Format(CultureInfo.CurrentCulture, Resources.LoadoutUpdateConfirm, loadout.Name), Resources.LoadoutTitle, MessageBoxIcon.Question, defaultNo: true)
			|| !this.GoOnThoughGameRuns())
			return false;

		this.PutBackHeldItem();
		return this.ShowLoadoutChange(loadouts, this.Services.Loadouts.Update(loadouts, loadout.Name, player), loadout.Name);
	}

	/// <summary>Renames a loadout.</summary>
	/// <param name="name">The loadout.</param>
	/// <param name="newName">The new name; asked when null.</param>
	internal bool RenameLoadout(string name, string? newName = null)
	{
		if (this.ChangeableLoadouts() is not var (_, loadouts) || loadouts.Find(name) is not { } loadout)
			return false;

		newName ??= this.AskLoadoutName(loadouts, loadout.Name, loadout.Name);
		if (newName is null)
			return false;

		return this.ShowLoadoutChange(loadouts, this.Services.Loadouts.Rename(loadouts, loadout.Name, newName), newName.Trim(), newName);
	}

	/// <summary>Copies a loadout under a new name.</summary>
	/// <param name="name">The loadout.</param>
	/// <param name="newName">The copy's name; asked when null.</param>
	internal bool CopyLoadout(string name, string? newName = null)
	{
		if (this.ChangeableLoadouts() is not var (_, loadouts) || loadouts.Find(name) is not { } loadout)
			return false;

		newName ??= this.AskLoadoutName(loadouts, loadout.Name);
		if (newName is null)
			return false;

		return this.ShowLoadoutChange(loadouts, this.Services.Loadouts.Copy(loadouts, loadout.Name, newName), newName.Trim(), newName);
	}

	/// <summary>
	/// Wears a loadout: the pieces taken off go to the bag shown in the vault panel; without room for all of them, nothing
	/// changes and the user is told. Asks first when the game runs.
	/// </summary>
	/// <param name="name">The loadout.</param>
	internal bool LoadLoadout(string name)
	{
		if (this.ChangeableLoadouts() is not var (player, loadouts) || loadouts.Find(name) is not { } loadout)
			return false;

		if (this.Services.Settings.PlayerReadonly)
		{
			this.UIService.NotifyUser(string.Format(CultureInfo.CurrentCulture, Resources.LoadoutEquipmentLocked, Resources.SettingsPlayerReadonly), TQColor.Red);
			return false;
		}

		int bagNumber = this.vaultPanel.CurrentBag;
		if (this.vaultPanel.Player is not { } vault || vault.GetSack(bagNumber) is not { } bag)
		{
			this.UIService.NotifyUser(Resources.LoadoutNoVault, TQColor.Red);
			return false;
		}

		if (!this.GoOnThoughGameRuns())
			return false;

		this.PutBackHeldItem();
		var bagSize = this.vaultPanel.SackPanel.SackSize;
		var place = this.Services.ItemMovementService.PlaceIn(vault, SackType.Vault, bagNumber);
		var result = this.Services.Loadouts.Load(loadouts, loadout.Name, player, bag, bagSize, place);
		switch (result.Outcome)
		{
			case LoadoutLoadOutcome.NoRoom:
				this.UIService.NotifyUser(string.Format(CultureInfo.CurrentCulture, Resources.LoadoutNoRoom, bagNumber + 1, vault.PlayerName, result.ToBag), TQColor.Red);
				return false;
			case LoadoutLoadOutcome.AlreadyWorn:
				this.UIService.NotifyUser(string.Format(CultureInfo.CurrentCulture, Resources.LoadoutAlreadyWorn, loadout.Name));
				return false;
			case LoadoutLoadOutcome.Refused:
				this.UIService.NotifyUser(string.Format(CultureInfo.CurrentCulture, Resources.CharacterReadOnlyRefused, loadouts.CharacterName), TQColor.Red);
				return false;
		}

		// The pieces stored take the vault's map rules, as a dropped item; then the panels show the new state.
		this.vaultPanel.AdoptVaultMod(bag, result.Stored, bagSize);
		BagButtonTooltip.InvalidateCache(bag);
		this.vaultPanel.Refresh();
		this.stashPanel.EquipmentReplaced();
		this.UIService.NotifyUser(string.Format(CultureInfo.CurrentCulture, Resources.LoadoutLoaded, loadout.Name, result.ToBag, bagNumber + 1, vault.PlayerName, result.Copied));
		return true;
	}

	/// <summary>Deletes a loadout, once confirmed.</summary>
	/// <param name="name">The loadout.</param>
	internal bool DeleteLoadout(string name)
	{
		if (this.ChangeableLoadouts() is not var (_, loadouts) || loadouts.Find(name) is not { } loadout)
			return false;

		if (!this.Services.MessageBoxes.Confirm(string.Format(CultureInfo.CurrentCulture, Resources.LoadoutDeleteConfirm, loadout.Name), Resources.LoadoutTitle, MessageBoxIcon.Question, defaultNo: true))
			return false;

		return this.ShowLoadoutChange(loadouts, this.Services.Loadouts.Delete(loadouts, loadout.Name), null);
	}
}
