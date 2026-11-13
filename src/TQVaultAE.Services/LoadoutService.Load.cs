using System.Drawing;
using Microsoft.Extensions.Logging;
using TQVaultAE.Application;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Services;

public partial class LoadoutService
{
	/// <inheritdoc/>
	public LoadoutLoadResult Load(LoadoutSet loadouts, string name, PlayerCollection player, SackCollection bag, Size bagSize, ContainerPlace bagPlace)
	{
		if (loadouts.Find(name) is not { } loadout
			|| player.EquipmentSack is not { } equipment
			|| player.IsReadOnly || equipment.IsReadOnly || bag.IsReadOnly)
			return new LoadoutLoadResult(LoadoutLoadOutcome.Refused);

		var worn = equipment.ToList();
		var plan = LoadoutPlan.Make(worn, loadout.Pieces);
		if (plan.ChangesNothing && player.UseAlternate == loadout.UseAlternate)
			return new LoadoutLoadResult(LoadoutLoadOutcome.AlreadyWorn);

		// Only the pieces that leave the equipment need room; the whole load is refused without it.
		var leaving = plan.LeavingSlots.Select(slot => worn[slot]).ToList();
		if (!this.HasRoom(bag, bagSize, leaving))
			return new LoadoutLoadResult(LoadoutLoadOutcome.NoRoom, ToBag: leaving.Count);

		var next = plan.Steps.Select(step => step.Action switch
		{
			LoadoutSlotAction.Keep => worn[step.Slot],
			LoadoutSlotAction.Move => worn[step.FromSlot],
			LoadoutSlotAction.Copy => CopyIn(loadout.Pieces[step.Slot]!, worn[step.Slot]),
			_ => ItemIdentity.IsEmpty(worn[step.Slot]) ? worn[step.Slot] : worn[step.Slot].MakeEmptyItem(),
		}).ToList();

		// The equipment's place in the search, for the items put in.
		var equipmentPlace = worn[0].Place;
		for (int slot = 0; slot < next.Count; slot++)
		{
			var item = next[slot];
			item.Location = SackCollection.GetEquipmentLocationOffset(slot);
			if (ReferenceEquals(item, worn[slot]))
				continue;

			equipment.RemoveAtItem(slot);
			equipment.InsertItem(slot, item);

			// A slot's empty item replaced leaves the search; a copy or a new empty item enters it.
			if (ItemIdentity.IsEmpty(worn[slot]) && !next.Contains(worn[slot]))
				this.ItemDatabaseService.RemoveItemFromDatabase(worn[slot]);
			if (!worn.Contains(item))
				this.ItemDatabaseService.AddItemToDatabase(item, equipmentPlace.Path, equipmentPlace.Name, equipmentPlace.SackNumber, SackType.Equipment);
		}

		// The pieces taken off, in the bag at the spots the room check found (same order, same search).
		foreach (var item in leaving)
		{
			if (!this.ItemMovementService.TryPlace(item, bag, bagSize, bagPlace))
				this.Log.LogError("Loadout {Name}: no room for {Item} after the room check", loadout.Name, item.BaseItemId);
		}

		player.UseAlternate = loadout.UseAlternate;
		equipment.IsModified = true;

		this.Log.LogInformation("Loadout {Name} worn by {Character}: {Stored} piece(s) stored, {Moved} moved, {Copied} copied in",
			loadout.Name, loadouts.CharacterName, leaving.Count, plan.MoveCount, plan.CopyCount);
		return new LoadoutLoadResult(LoadoutLoadOutcome.Loaded, leaving.Count, plan.MoveCount, plan.CopyCount) { Stored = leaving };
	}

	/// <summary>
	/// Whether every item fits in the bag, placed one after the other at the first free spot as
	/// <see cref="Application.Contracts.Services.IItemMovementService.TryPlace"/> places them.
	/// </summary>
	private bool HasRoom(SackCollection bag, Size bagSize, List<Item> items)
	{
		if (items.Count == 0)
			return true;

		// Stand-ins of the same cells: the real bag is not touched.
		var trial = new SackCollection { SackType = bag.SackType, StashType = bag.StashType };
		static Item StandIn(Item of, int x, int y) => new() { BaseItemId = of.BaseItemId, PositionX = x, PositionY = y, Width = of.Width, Height = of.Height };
		foreach (var item in bag)
			trial.AddItem(StandIn(item, item.PositionX, item.PositionY));

		foreach (var item in items)
		{
			var at = this.ItemMovementService.FindOpenCells(trial, item.Width, item.Height, bagSize.Width, bagSize.Height);
			if (at.X < 0)
				return false;
			trial.AddItem(StandIn(item, at.X, at.Y));
		}

		return true;
	}

	/// <summary>A copy of a loadout piece for a slot, with the block values of the item it replaces.</summary>
	private static Item CopyIn(Item piece, Item replaced)
	{
		var copy = piece.Clone();
		copy.beginBlockCrap1 = replaced.beginBlockCrap1;
		copy.beginBlockCrap2 = replaced.beginBlockCrap2;
		copy.endBlockCrap1 = replaced.endBlockCrap1;
		copy.endBlockCrap2 = replaced.endBlockCrap2;
		copy.InVault = false;
		copy.Place = new ContainerPlace();
		return copy;
	}
}
