using Microsoft.Extensions.Logging;
using TQVaultAE.Application.Results;
using TQVaultAE.Application.Session;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Services;

public partial class LoadoutService
{
	/// <inheritdoc/>
	public LoadoutSwapResult Swap(LoadoutSet loadouts, string name, PlayerCollection player)
	{
		if (loadouts.ReadFailed
			|| loadouts.Find(name) is not { } loadout
			|| player.EquipmentSack is not { } equipment
			|| player.IsReadOnly || equipment.IsReadOnly)
			return new LoadoutSwapResult(LoadoutSwapOutcome.Refused);

		int slots = equipment.NumberOfSlots;
		var worn = equipment.Take(slots).ToList();
		bool anyPiece = Enumerable.Range(0, Math.Max(slots, loadout.Pieces.Count))
			.Any(slot => PieceAt(loadout.Pieces, slot) is not null || (slot < worn.Count && !ItemKey.IsEmptyItem(worn[slot])));
		if (!anyPiece && player.UseAlternate == loadout.UseAlternate)
			return new LoadoutSwapResult(LoadoutSwapOutcome.NothingToSwap);

		// The equipment's place in the search, for the pieces put on (as a load gives them).
		var equipmentPlace = worn[0].Place;

		// The slots past the character's equipment (an artifact for a character of the base game) keep their piece.
		var toLoadout = new Item?[Math.Max(slots, loadout.Pieces.Count)];
		for (int slot = slots; slot < loadout.Pieces.Count; slot++)
			toLoadout[slot] = loadout.Pieces[slot];

		var putOn = new List<Item>();
		int takenOff = 0;
		for (int slot = 0; slot < slots; slot++)
		{
			var wornItem = worn[slot];
			var piece = PieceAt(loadout.Pieces, slot);
			bool wornEmpty = ItemKey.IsEmptyItem(wornItem);
			if (wornEmpty && piece is null)
				continue;

			// The slot of the equipment gets the piece, or an empty item; the save keeps the slot's block values.
			Item next;
			if (piece is not null)
			{
				piece.beginBlockCrap1 = wornItem.beginBlockCrap1;
				piece.beginBlockCrap2 = wornItem.beginBlockCrap2;
				piece.endBlockCrap1 = wornItem.endBlockCrap1;
				piece.endBlockCrap2 = wornItem.endBlockCrap2;
				next = piece;
				putOn.Add(piece);
			}
			else
				next = wornItem.MakeEmptyItem();

			next.SetContainerMod(loadouts.Mod);
			next.Location = SackCollection.GetEquipmentLocationOffset(slot);
			equipment.RemoveAtItem(slot);
			equipment.InsertItem(slot, next);

			if (wornEmpty)
				this.ItemDatabaseService.RemoveItemFromDatabase(wornItem);
			else
			{
				// The worn piece takes the slot in the loadout, read with the character's map as the loadout's pieces.
				wornItem.SetContainerMod(loadouts.Mod);
				toLoadout[slot] = wornItem;
				takenOff++;
			}

			if (piece is null)
				this.ItemDatabaseService.AddItemToDatabase(next, equipmentPlace.Path, equipmentPlace.Name, equipmentPlace.SackNumber, SackType.Equipment);
		}

		var swapped = loadout with { Pieces = toLoadout, UseAlternate = player.UseAlternate };
		loadouts.Loadouts = [.. loadouts.Loadouts.Select(l => l == loadout ? swapped : l)];
		loadouts.IsModified = true;
		player.UseAlternate = loadout.UseAlternate;
		equipment.IsModified = true;

		// The loadout's entries first (its pieces now worn leave it), then the pieces put on enter as equipment.
		this.Changed(loadouts);
		foreach (var item in putOn)
			this.ItemDatabaseService.AddItemToDatabase(item, equipmentPlace.Path, equipmentPlace.Name, equipmentPlace.SackNumber, SackType.Equipment);

		this.Log.LogInformation("Gear swap of {Character} with loadout {Name}: {PutOn} piece(s) put on, {TakenOff} taken off into the loadout",
			loadouts.CharacterName, loadout.Name, putOn.Count, takenOff);
		return new LoadoutSwapResult(LoadoutSwapOutcome.Swapped, putOn.Count, takenOff);
	}
}
