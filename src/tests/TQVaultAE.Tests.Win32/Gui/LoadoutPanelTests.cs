using System.Drawing;
using AwesomeAssertions;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Components.Panels;

namespace TQVaultAE.Tests.Win32.Gui;

/// <summary>
/// <see cref="LoadoutPanel.BuildSack"/>: the equipment sack the loadout panel shows, on which the equipment rules move the
/// pieces in and out (moved, never copied).
/// </summary>
public sealed class LoadoutPanelTests
{
	private const string CharacterMod = "SomeMod|custommaps";

	private static Item MakeItem(string itemClass, string id)
	{
		var record = new DBRecordCollection(RecordId.Create(id), itemClass);
		var cls = new Variable("Class", VariableDataType.StringVar, 1);
		cls[0] = itemClass;
		record.Set(cls);
		return new Item { BaseItemId = RecordId.Create(id), baseItemInfo = new Info(record), Width = 1, Height = 1, PositionX = -1, PositionY = -1 };
	}

	/// <summary>A loadout with pieces in the given slots, as read from the file (no cell).</summary>
	private static Loadout LoadoutOf(params (int Slot, Item Item)[] pieces)
	{
		var slots = new Item?[12];
		foreach (var (slot, item) in pieces)
			slots[slot] = item;
		return new Loadout("Fire set", false, slots);
	}

	[Fact]
	public void BuildSack_PutsThePiecesThemselvesInTheirSlotsAndEmptyItemsElsewhere()
	{
		var helm = MakeItem(Item.ICLASS_HEAD, "records/item/helm.dbr");
		var ring = MakeItem(Item.ICLASS_RING, "records/item/ring.dbr");

		var sack = LoadoutPanel.BuildSack(LoadoutOf((EquipmentRules.HeadSlot, helm), (EquipmentRules.Ring2Slot, ring)), 12, CharacterMod);

		sack.Should().HaveCount(12);
		sack.SackType.Should().Be(SackType.Equipment, "the equipment rules work on it");
		sack.IsModified.Should().BeFalse();
		sack.GetItem(EquipmentRules.HeadSlot).Should().BeSameAs(helm, "the loadout's own items, not copies");
		sack.GetItem(EquipmentRules.Ring2Slot).Should().BeSameAs(ring);
		EquipmentRules.FindSlot(sack, helm).Should().Be(EquipmentRules.HeadSlot);
		EquipmentRules.FindSlot(sack, ring).Should().Be(EquipmentRules.Ring2Slot);
		var empty = sack.GetItem(EquipmentRules.BodySlot);
		empty.IsEmpty.Should().BeTrue();
		empty.ContainerMod.Should().Be(CharacterMod, "an empty slot holds an item of the character's map");
	}

	[Fact]
	public void BuildSack_ACharacterOfTheBaseGame_HasNoArtifactSlot()
		=> LoadoutPanel.BuildSack(LoadoutOf(), 11, null).Should().HaveCount(11);

	[Fact]
	public void PieceTakenOut_LeavesAnEmptySlot()
	{
		// A piece dragged out of the loadout: the drop elsewhere takes it out of its slot.
		var helm = MakeItem(Item.ICLASS_HEAD, "records/item/helm.dbr");
		var sack = LoadoutPanel.BuildSack(LoadoutOf((EquipmentRules.HeadSlot, helm)), 12, null);

		EquipmentRules.TakeOut(sack, helm);

		sack.Should().HaveCount(12);
		sack.GetItem(EquipmentRules.HeadSlot).IsEmpty.Should().BeTrue();
		sack.Should().NotContain(helm);
	}

	[Fact]
	public void PieceDropped_OnlyInASlotOfItsClass()
	{
		var sack = LoadoutPanel.BuildSack(LoadoutOf(), 12, null);
		var helm = MakeItem(Item.ICLASS_HEAD, "records/item/helm.dbr");

		EquipmentRules.CanEquip(sack, helm, EquipmentRules.HeadSlot).Should().BeTrue();
		EquipmentRules.CanEquip(sack, helm, EquipmentRules.Weapon1Slot).Should().BeFalse();
	}

	[Fact]
	public void TwoHandedWeaponDropped_TakesBothHandsAndHandsTheShieldBack()
	{
		var sword = MakeItem(Item.ICLASS_SWORD, "records/item/sword.dbr");
		var shield = MakeItem(Item.ICLASS_SHIELD, "records/item/shield.dbr");
		var sack = LoadoutPanel.BuildSack(LoadoutOf((EquipmentRules.Weapon1Slot, sword), (EquipmentRules.Shield1Slot, shield)), 12, null);
		var bow = MakeItem(Item.ICLASS_BOW, "records/item/bow.dbr");

		// A two-handed weapon needs one free hand of the pair: both are taken here.
		EquipmentRules.CanEquip(sack, bow, EquipmentRules.Weapon1Slot).Should().BeFalse();

		EquipmentRules.TakeOut(sack, sword);
		EquipmentRules.CanEquip(sack, bow, EquipmentRules.Weapon1Slot).Should().BeTrue();
		var handedBack = EquipmentRules.Equip(sack, bow, EquipmentRules.Weapon1Slot, null);

		sack.GetItem(EquipmentRules.Shield1Slot).Should().BeSameAs(bow, "a two-handed weapon is kept in the second slot of its pair");
		sack.GetItem(EquipmentRules.Weapon1Slot).IsEmpty.Should().BeTrue();
		handedBack!.BaseItemId.Should().Be(shield.BaseItemId, "the shield goes to the hand");
		sack.Take(sack.NumberOfSlots).Should().NotContain(i => i.BaseItemId == shield.BaseItemId, "the slots no longer hold it");
	}

	[Fact]
	public void ButtonCells_SitInTheGapsOfTheStone_OverNoSlot()
	{
		var (back, previous, next) = LoadoutPanel.ButtonCells();
		var grid = new RectangleF(PointF.Empty, LoadoutPanel.SlotCells);

		foreach (var button in new[] { back, previous, next })
		{
			grid.Contains(button).Should().BeTrue($"{button} stays on the stone of the slots");
			for (int slot = 0; slot <= EquipmentRules.ArtifactSlot; slot++)
			{
				var (location, size) = LoadoutComparison.SlotCells(slot);
				new RectangleF(location, size).IntersectsWith(button).Should().BeFalse($"{button} leaves slot {slot} free");
			}
		}

		back.Top.Should().BeLessThan(previous.Top, "Back is in the upper corner, the turn arrows in the lower ones");
		previous.Top.Should().Be(next.Top);
		previous.Left.Should().BeLessThan(LoadoutComparison.MiddleColumnFirst);
		next.Left.Should().BeGreaterThan(LoadoutComparison.MiddleColumnLast + 1);
	}
}
