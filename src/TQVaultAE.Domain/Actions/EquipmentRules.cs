using System.Drawing;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Domain.Actions;

/// <summary>
/// Rules of the equipment sack: which item goes in which slot, and the weapon swaps.
/// </summary>
/// <remarks>
/// The equipment sack always holds one item per slot, in slot order: an empty slot holds an empty item (no base item
/// id). A two-handed weapon is kept in the second slot of its weapon pair (8 or 10); the other slot of the pair, its
/// shadow slot, holds an empty item.
/// </remarks>
public static class EquipmentRules
{
	/// <summary>Head slot.</summary>
	public const int HeadSlot = 0;

	/// <summary>Neck slot.</summary>
	public const int NeckSlot = 1;

	/// <summary>Body slot.</summary>
	public const int BodySlot = 2;

	/// <summary>Legs slot.</summary>
	public const int LegsSlot = 3;

	/// <summary>Arms slot.</summary>
	public const int ArmsSlot = 4;

	/// <summary>First ring slot.</summary>
	public const int Ring1Slot = 5;

	/// <summary>Second ring slot.</summary>
	public const int Ring2Slot = 6;

	/// <summary>Weapon slot of the first weapon set; also the first weapon slot.</summary>
	public const int Weapon1Slot = 7;

	/// <summary>Shield slot of the first weapon set, where its two-handed weapon is kept.</summary>
	public const int Shield1Slot = 8;

	/// <summary>Weapon slot of the second weapon set.</summary>
	public const int Weapon2Slot = 9;

	/// <summary>Shield slot of the second weapon set, where its two-handed weapon is kept.</summary>
	public const int Shield2Slot = 10;

	/// <summary>Artifact slot.</summary>
	public const int ArtifactSlot = 11;

	/// <summary>
	/// Merges the worn pieces into a loadout, slot by slot: a worn piece replaces the loadout's, an empty slot of the
	/// character keeps the loadout's piece.
	/// </summary>
	/// <remarks>
	/// A weapon pair that would hold a two-handed weapon beside another piece (a worn bow and the loadout's sword in its
	/// shadow slot, or the reverse) is taken whole from the worn gear. Slots past <paramref name="worn"/> keep the
	/// loadout's piece.
	/// </remarks>
	/// <param name="loadout">The loadout's pieces, slot by slot; null for an empty slot.</param>
	/// <param name="worn">The worn pieces, slot by slot; null for an empty slot.</param>
	/// <returns>The merged pieces: items of <paramref name="worn"/> or of <paramref name="loadout"/>.</returns>
	public static Item?[] MergeWorn(IReadOnlyList<Item?> loadout, IReadOnlyList<Item?> worn)
	{
		static Item? At(IReadOnlyList<Item?> pieces, int slot)
			=> slot < pieces.Count && pieces[slot] is { } piece && !Item.IsNullOrEmpty(piece) ? piece : null;

		var merged = new Item?[Math.Max(loadout.Count, worn.Count)];
		for (int slot = 0; slot < merged.Length; slot++)
			merged[slot] = At(worn, slot) ?? At(loadout, slot);

		foreach (var (first, second) in new[] { (Weapon1Slot, Shield1Slot), (Weapon2Slot, Shield2Slot) })
		{
			if (second >= merged.Length)
				continue;

			bool twoHandedBeside = (merged[second]?.Is2HWeapon == true && merged[first] is not null)
				|| (merged[first]?.Is2HWeapon == true && merged[second] is not null);
			if (twoHandedBeside)
			{
				merged[first] = At(worn, first);
				merged[second] = At(worn, second);
			}
		}

		return merged;
	}

	/// <summary>
	/// Tells whether an item may go in an equipment slot.
	/// </summary>
	/// <remarks>
	/// Any weapon or shield goes in any of the four weapon slots; a two-handed weapon also needs one free slot in the
	/// pair, the dropped one or the other.
	/// </remarks>
	/// <param name="sack">The equipment sack.</param>
	/// <param name="item">The item to equip.</param>
	/// <param name="slot">The slot.</param>
	public static bool CanEquip(SackCollection sack, Item? item, int slot)
	{
		if (item is null || slot < 0 || slot >= sack.NumberOfSlots)
			return false;

		switch (slot)
		{
			case HeadSlot:
				return item.IsHelm;

			case NeckSlot:
				return item.IsAmulet;

			case BodySlot:
				return item.IsTorsoArmor;

			case LegsSlot:
				return item.IsGreave;

			case ArmsSlot:
				return item.IsBracer;

			case Ring1Slot:
			case Ring2Slot:
				return item.IsRing;

			case Weapon1Slot:
			case Shield1Slot:
			case Weapon2Slot:
			case Shield2Slot:
				if (!item.IsWeaponShield)
					return false;

				if (!item.Is2HWeapon)
					return true;

				// A two-handed weapon needs at least one free slot in the pair.
				return Item.IsNullOrEmpty(sack.GetItem(OtherSlotOfPair(slot))) || Item.IsNullOrEmpty(sack.GetItem(slot));

			case ArtifactSlot:
				return item.IsArtifact;

			default:
				return false;
		}
	}

	/// <summary>
	/// Gets the item in the other slot of a weapon pair.
	/// </summary>
	/// <param name="sack">The equipment sack.</param>
	/// <param name="weaponSlot">The weapon slot, 0 to 3 (equipment slot minus <see cref="Weapon1Slot"/>).</param>
	/// <returns>The item of the other slot of the pair, or <see langword="null"/> when not a weapon slot.</returns>
	public static Item? GetShadowSlotItem(SackCollection sack, int weaponSlot)
	{
		int slot = weaponSlot + Weapon1Slot;
		if (!SackCollection.IsWeaponSlot(slot))
			return null;

		return sack.GetItem(OtherSlotOfPair(slot));
	}

	/// <summary>
	/// Finds the equipment slot an item is in, from its position.
	/// </summary>
	/// <param name="sack">The sack.</param>
	/// <param name="item">The item.</param>
	/// <returns>The slot, or -1 when the sack is not the equipment or the item is in no slot.</returns>
	public static int FindSlot(SackCollection? sack, Item? item)
	{
		if (sack is null || sack.SackType != SackType.Equipment || item is null)
			return -1;

		Point position = item.IsInWeaponSlot
			? SackCollection.GetWeaponLocationOffset(item.PositionY)
			: item.Location;

		return SlotAt(position, sack.NumberOfSlots);
	}

	/// <summary>
	/// Finds the equipment slot covering a cell of the equipment panel.
	/// </summary>
	/// <param name="cell">The cell.</param>
	/// <param name="numberOfSlots">The number of slots of the equipment sack.</param>
	/// <returns>The slot, or -1.</returns>
	public static int SlotAt(Point cell, int numberOfSlots)
	{
		for (int slot = 0; slot < numberOfSlots; ++slot)
		{
			if (SlotCells(slot).Contains(cell))
				return slot;
		}

		return -1;
	}

	/// <summary>
	/// Finds the first equipment slot touched by cells of the equipment panel.
	/// </summary>
	/// <param name="cells">The cells.</param>
	/// <param name="numberOfSlots">The number of slots of the equipment sack.</param>
	/// <returns>The slot, or -1.</returns>
	public static int SlotUnder(Rectangle cells, int numberOfSlots)
	{
		for (int slot = 0; slot < numberOfSlots; ++slot)
		{
			if (cells.IntersectsWith(SlotCells(slot)))
				return slot;
		}

		return -1;
	}

	/// <summary>
	/// Gets the cells of an equipment slot in the equipment panel.
	/// </summary>
	/// <param name="slot">The slot.</param>
	public static Rectangle SlotCells(int slot)
	{
		Point upperLeft = SackCollection.GetEquipmentLocationOffset(slot);
		if (SackCollection.IsWeaponSlot(slot))
			return new Rectangle(SackCollection.GetWeaponLocationOffset(upperLeft.Y), SackCollection.WeaponLocationSize);

		return new Rectangle(upperLeft, SackCollection.GetEquipmentLocationSize(slot));
	}

	/// <summary>
	/// Takes an item out of its sack: an equipped item leaves an empty item in its slot.
	/// </summary>
	/// <param name="sack">The sack of the item.</param>
	/// <param name="item">The item.</param>
	public static void TakeOut(SackCollection sack, Item item)
	{
		int slot = FindSlot(sack, item);
		if (slot == -1)
		{
			sack.RemoveItem(item);
			return;
		}

		sack.RemoveAtItem(slot);
		Item empty = item.MakeEmptyItem();
		empty.Location = SackCollection.GetEquipmentLocationOffset(slot);
		sack.InsertItem(slot, empty);
	}

	/// <summary>
	/// Puts an item in an equipment slot, freeing the slots a two-handed weapon needs.
	/// </summary>
	/// <remarks>
	/// <list type="bullet">
	/// <item>A two-handed weapon goes in the second slot of the pair; the item under it, or else the item in the
	/// other slot of the pair, is picked up.</item>
	/// <item>A one-handed item dropped on the shadow slot of a two-handed weapon picks the two-handed weapon up.</item>
	/// <item>Otherwise the item under the drop is picked up.</item>
	/// </list>
	/// An item picked up from a slot other than the dropped one is a copy, added at the end of the sack, and its slot
	/// gets an empty item. Call <see cref="CanEquip"/> first.
	/// </remarks>
	/// <param name="sack">The equipment sack.</param>
	/// <param name="item">The item to equip, already taken out of its source sack.</param>
	/// <param name="slot">The slot of the drop.</param>
	/// <param name="itemUnderUs">The item under the drop: for an empty shadow slot, its two-handed weapon.</param>
	/// <returns>The item to pick up next, possibly an empty item or <see langword="null"/>.</returns>
	public static Item? Equip(SackCollection sack, Item item, int slot, Item? itemUnderUs)
	{
		int secondSlot = ((slot + 1) / 2) * 2;
		if (item.Is2HWeapon)
		{
			if (slot != secondSlot)
			{
				// Dropped on the first slot of the pair.
				if (itemUnderUs is not null && !Item.IsNullOrEmpty(itemUnderUs))
				{
					// For a two-handed weapon, the item under us is the one in the second slot.
					itemUnderUs = PickUpCopy(sack, itemUnderUs.Is2HWeapon ? secondSlot : slot);
					PutEmptyItem(sack, item, slot);
				}
				else if (!Item.IsNullOrEmpty(sack.GetItem(secondSlot)))
				{
					itemUnderUs = PickUpCopy(sack, secondSlot);
					PutEmptyItem(sack, item, secondSlot);
				}

				slot = secondSlot;
			}
			else
			{
				// Dropped on the second slot, the first slot of the pair may hold something.
				int firstSlot = secondSlot - 1;
				if (itemUnderUs is not null && !Item.IsNullOrEmpty(itemUnderUs))
				{
					itemUnderUs = PickUpCopy(sack, slot);
					PutEmptyItem(sack, item, slot);
				}
				else if (!Item.IsNullOrEmpty(sack.GetItem(firstSlot)))
				{
					itemUnderUs = PickUpCopy(sack, firstSlot);
					PutEmptyItem(sack, item, firstSlot);
				}
			}
		}

		// An item dropped on the shadow slot of a two-handed weapon picks the weapon up.
		if (SackCollection.IsWeaponSlot(slot) && slot != secondSlot && sack.GetItem(secondSlot).Is2HWeapon)
		{
			itemUnderUs = PickUpCopy(sack, secondSlot);
			PutEmptyItem(sack, item, secondSlot);
		}

		item.Location = SackCollection.GetEquipmentLocationOffset(slot);

		// The equipment sack holds one item per slot: remove and insert.
		sack.RemoveAtItem(slot);
		sack.InsertItem(slot, item);

		return itemUnderUs;
	}

	/// <summary>The other slot of a weapon pair: 7 and 8, 9 and 10.</summary>
	private static int OtherSlotOfPair(int slot) => (slot % 2) == 1 ? slot + 1 : slot - 1;

	/// <summary>Copies the item of a slot at the end of the sack, to be picked up.</summary>
	private static Item PickUpCopy(SackCollection sack, int slot)
	{
		sack.AddItem(sack.GetItem(slot).Duplicate(true));
		return sack.GetItem(sack.Count - 1);
	}

	private static void PutEmptyItem(SackCollection sack, Item model, int slot)
	{
		Item empty = model.MakeEmptyItem();
		empty.Location = SackCollection.GetEquipmentLocationOffset(slot);
		sack.RemoveAtItem(slot);
		sack.InsertItem(slot, empty);
	}
}
