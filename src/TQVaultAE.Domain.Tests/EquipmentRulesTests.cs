using System.Drawing;
using AwesomeAssertions;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Domain;

/// <summary>
/// <see cref="EquipmentRules"/>: which item goes in which equipment slot, and the weapon swaps of a drop.
/// </summary>
public class EquipmentRulesTests
{
	private const int Head = EquipmentRules.HeadSlot;
	private const int Neck = EquipmentRules.NeckSlot;
	private const int Body = EquipmentRules.BodySlot;
	private const int Legs = EquipmentRules.LegsSlot;
	private const int Arms = EquipmentRules.ArmsSlot;
	private const int Ring1 = EquipmentRules.Ring1Slot;
	private const int Ring2 = EquipmentRules.Ring2Slot;
	private const int Weapon1 = EquipmentRules.Weapon1Slot;
	private const int Shield1 = EquipmentRules.Shield1Slot;
	private const int Weapon2 = EquipmentRules.Weapon2Slot;
	private const int Shield2 = EquipmentRules.Shield2Slot;
	private const int Artifact = EquipmentRules.ArtifactSlot;

	#region Fixtures

	private static Item ItemOf(string itemClass, string name)
	{
		var id = RecordId.Create($"records/item/{name}.dbr");
		var record = new DBRecordCollection(id, itemClass);
		var cls = new Variable("Class", VariableDataType.StringVar, 1);
		cls[0] = itemClass;
		record.Set(cls);
		return new Item { BaseItemId = id, baseItemInfo = new Info(record), Width = 2, Height = 4 };
	}

	private static Item Sword(string name = "sword") => ItemOf(Item.ICLASS_SWORD, name);

	private static Item Shield(string name = "shield") => ItemOf(Item.ICLASS_SHIELD, name);

	private static Item Bow(string name = "bow") => ItemOf(Item.ICLASS_BOW, name);

	private static Item Staff(string name = "staff") => ItemOf(Item.ICLASS_STAFF, name);

	private static Item EmptyItem() => new() { BaseItemId = RecordId.Empty };

	/// <summary>An equipment sack: one item per slot, empty items in the slots not given.</summary>
	private static SackCollection Equipment(params (int Slot, Item Item)[] equipped)
	{
		var sack = new SackCollection { SackType = SackType.Equipment, slots = 12 };
		for (int slot = 0; slot < 12; slot++)
		{
			Item item = equipped.FirstOrDefault(e => e.Slot == slot).Item ?? EmptyItem();
			item.Location = SackCollection.GetEquipmentLocationOffset(slot);
			sack.AddItem(item);
		}

		sack.IsModified = false;
		return sack;
	}

	private static bool IsEmpty(Item item) => RecordId.IsNullOrEmpty(item.BaseItemId);

	#endregion

	#region CanEquip

	public static TheoryData<string, int[]> SlotsByItemClass => new()
	{
		{ Item.ICLASS_HEAD, [Head] },
		{ Item.ICLASS_AMULET, [Neck] },
		{ Item.ICLASS_UPPERBODY, [Body] },
		{ Item.ICLASS_LOWERBODY, [Legs] },
		{ Item.ICLASS_FOREARM, [Arms] },
		{ Item.ICLASS_RING, [Ring1, Ring2] },
		{ Item.ICLASS_SWORD, [Weapon1, Shield1, Weapon2, Shield2] },
		{ Item.ICLASS_AXE, [Weapon1, Shield1, Weapon2, Shield2] },
		{ Item.ICLASS_MACE, [Weapon1, Shield1, Weapon2, Shield2] },
		{ Item.ICLASS_SPEAR, [Weapon1, Shield1, Weapon2, Shield2] },
		{ Item.ICLASS_RANGEDONEHAND, [Weapon1, Shield1, Weapon2, Shield2] },
		{ Item.ICLASS_SHIELD, [Weapon1, Shield1, Weapon2, Shield2] },
		{ Item.ICLASS_BOW, [Weapon1, Shield1, Weapon2, Shield2] },
		{ Item.ICLASS_STAFF, [Weapon1, Shield1, Weapon2, Shield2] },
		{ Item.ICLASS_ARTIFACT, [Artifact] },
		{ Item.ICLASS_FORMULA, [] },
		{ Item.ICLASS_RELIC, [] },
		{ Item.ICLASS_CHARM, [] },
		{ Item.ICLASS_POTIONHEALTH, [] },
		{ Item.ICLASS_SCROLL, [] },
		{ Item.ICLASS_QUESTITEM, [] },
		{ Item.ICLASS_DYE, [] },
	};

	[Theory]
	[MemberData(nameof(SlotsByItemClass))]
	public void CanEquip_OnAnEmptyEquipment_AcceptsEachClassInItsSlotsOnly(string itemClass, int[] allowedSlots)
	{
		var sack = Equipment();
		var item = ItemOf(itemClass, "item");

		var accepted = Enumerable.Range(0, 12).Where(slot => EquipmentRules.CanEquip(sack, item, slot));

		accepted.Should().BeEquivalentTo(allowedSlots);
	}

	[Theory]
	[InlineData(-1)]
	[InlineData(12)]
	[InlineData(13)]
	public void CanEquip_OutsideTheSlots_Refuses(int slot)
	{
		var sack = Equipment();

		EquipmentRules.CanEquip(sack, Sword(), slot).Should().BeFalse();
	}

	[Fact]
	public void CanEquip_NoItem_Refuses()
		=> EquipmentRules.CanEquip(Equipment(), null, Weapon1).Should().BeFalse();

	[Theory]
	[InlineData(Weapon1)]
	[InlineData(Shield1)]
	public void CanEquip_OneHandedWeapon_GoesOnAFullWeaponPair(int slot)
	{
		var sack = Equipment((Weapon1, Sword("held")), (Shield1, Shield()));

		EquipmentRules.CanEquip(sack, Sword(), slot).Should().BeTrue();
	}

	[Theory]
	[InlineData(Weapon1)]
	[InlineData(Shield1)]
	public void CanEquip_TwoHandedWeapon_RefusedWhenBothSlotsOfThePairAreTaken(int slot)
	{
		var sack = Equipment((Weapon1, Sword()), (Shield1, Shield()));

		EquipmentRules.CanEquip(sack, Bow(), slot).Should().BeFalse();
	}

	[Theory]
	[InlineData(Weapon1, Weapon1)]
	[InlineData(Weapon1, Shield1)]
	[InlineData(Shield1, Weapon1)]
	[InlineData(Shield1, Shield1)]
	public void CanEquip_TwoHandedWeapon_AcceptedWhenOneSlotOfThePairIsFree(int takenSlot, int dropSlot)
	{
		var sack = Equipment((takenSlot, Sword()));

		EquipmentRules.CanEquip(sack, Staff(), dropSlot).Should().BeTrue();
	}

	[Theory]
	[InlineData(Weapon2)]
	[InlineData(Shield2)]
	public void CanEquip_TwoHandedWeapon_ChecksTheSecondPairOnItsOwn(int slot)
	{
		// The first set is full, the second empty: only the second accepts.
		var fullFirstSet = Equipment((Weapon1, Sword()), (Shield1, Shield()));
		var fullSecondSet = Equipment((Weapon2, Sword()), (Shield2, Shield()));

		EquipmentRules.CanEquip(fullFirstSet, Bow(), slot).Should().BeTrue();
		EquipmentRules.CanEquip(fullSecondSet, Bow(), slot).Should().BeFalse();
	}

	[Fact]
	public void CanEquip_TwoHandedWeapon_AcceptedOnAnEquippedTwoHandedWeapon()
	{
		// The shadow slot of the equipped two-handed weapon is empty.
		var sack = Equipment((Shield1, Bow("equipped")));

		EquipmentRules.CanEquip(sack, Staff(), Weapon1).Should().BeTrue();
		EquipmentRules.CanEquip(sack, Staff(), Shield1).Should().BeTrue();
	}

	#endregion

	#region Slots and shadow slots

	[Theory]
	[InlineData(0, Shield1)]
	[InlineData(1, Weapon1)]
	[InlineData(2, Shield2)]
	[InlineData(3, Weapon2)]
	public void GetShadowSlotItem_ReturnsTheItemOfTheOtherSlotOfThePair(int weaponSlot, int expectedSlot)
	{
		var sack = Equipment((Weapon1, Sword("w1")), (Shield1, Shield("s1")), (Weapon2, Sword("w2")), (Shield2, Shield("s2")));

		EquipmentRules.GetShadowSlotItem(sack, weaponSlot).Should().BeSameAs(sack.GetItem(expectedSlot));
	}

	[Theory]
	[InlineData(-1)]
	[InlineData(4)]
	[InlineData(-7)]
	public void GetShadowSlotItem_NotAWeaponSlot_ReturnsNull(int weaponSlot)
		=> EquipmentRules.GetShadowSlotItem(Equipment(), weaponSlot).Should().BeNull();

	[Fact]
	public void FindSlot_FindsEverySlotFromTheItemPosition()
	{
		var sack = Equipment();

		Enumerable.Range(0, 12).Select(slot => EquipmentRules.FindSlot(sack, sack.GetItem(slot)))
			.Should().Equal(Enumerable.Range(0, 12));
	}

	[Fact]
	public void FindSlot_NotTheEquipmentOrNoItem_ReturnsMinusOne()
	{
		var equipment = Equipment();
		var playerSack = new SackCollection { SackType = SackType.Player, slots = 12 };
		var notInASlot = Sword();
		notInASlot.Location = new Point(0, 0);

		EquipmentRules.FindSlot(playerSack, equipment.GetItem(Head)).Should().Be(-1);
		EquipmentRules.FindSlot(null, equipment.GetItem(Head)).Should().Be(-1);
		EquipmentRules.FindSlot(equipment, null).Should().Be(-1);
		EquipmentRules.FindSlot(equipment, notInASlot).Should().Be(-1);
	}

	[Theory]
	[InlineData(4, 0, Head)]
	[InlineData(5, 1, Head)]
	[InlineData(5, 3, Neck)]
	[InlineData(4, 7, Body)]
	[InlineData(5, 10, Legs)]
	[InlineData(8, 7, Arms)]
	[InlineData(4, 12, Ring1)]
	[InlineData(5, 12, Ring2)]
	[InlineData(1, 0, Weapon1)]
	[InlineData(2, 4, Weapon1)]
	[InlineData(7, 0, Shield1)]
	[InlineData(8, 4, Shield1)]
	[InlineData(1, 9, Weapon2)]
	[InlineData(8, 13, Shield2)]
	[InlineData(1, 6, Artifact)]
	[InlineData(0, 0, -1)]
	[InlineData(3, 0, -1)]
	public void SlotAt_ReturnsTheSlotCoveringTheCell(int x, int y, int expected)
		=> EquipmentRules.SlotAt(new Point(x, y), 12).Should().Be(expected);

	[Fact]
	public void SlotUnder_ReturnsTheFirstSlotTouched()
	{
		// Touches the head (4,0 2x2) and the neck (4,3).
		EquipmentRules.SlotUnder(new Rectangle(4, 1, 1, 3), 12).Should().Be(Head);
		EquipmentRules.SlotUnder(new Rectangle(6, 0, 1, 2), 12).Should().Be(-1);
		EquipmentRules.SlotUnder(new Rectangle(7, 3, 2, 2), 12).Should().Be(Shield1);
	}

	#endregion

	#region TakeOut

	[Fact]
	public void TakeOut_EquippedItem_LeavesAnEmptyItemInItsSlot()
	{
		var sword = Sword();
		var sack = Equipment((Weapon1, sword));

		EquipmentRules.TakeOut(sack, sword);

		sack.Count.Should().Be(12);
		sack.Should().NotContain(sword);
		IsEmpty(sack.GetItem(Weapon1)).Should().BeTrue();
		sack.GetItem(Weapon1).Location.Should().Be(SackCollection.GetEquipmentLocationOffset(Weapon1));
		sack.IsModified.Should().BeTrue();
	}

	[Fact]
	public void TakeOut_ItemOfAnotherSack_RemovesIt()
	{
		var sword = Sword();
		var sack = new SackCollection { SackType = SackType.Vault };
		sack.AddItem(sword);

		EquipmentRules.TakeOut(sack, sword);

		sack.Count.Should().Be(0);
	}

	[Fact]
	public void TakeOut_SwapCopyAtTheEndOfTheEquipment_RemovesIt()
	{
		// The copy picked up by a weapon swap is added at the end of the sack, out of any slot.
		var sack = Equipment();
		var copy = Sword();
		copy.Location = new Point(-1, -1);
		sack.AddItem(copy);

		EquipmentRules.TakeOut(sack, copy);

		sack.Count.Should().Be(12);
		sack.Should().NotContain(copy);
	}

	#endregion

	#region Equip: one-handed items

	[Theory]
	[InlineData(Head, Item.ICLASS_HEAD)]
	[InlineData(Ring2, Item.ICLASS_RING)]
	[InlineData(Artifact, Item.ICLASS_ARTIFACT)]
	[InlineData(Weapon1, Item.ICLASS_SWORD)]
	[InlineData(Shield2, Item.ICLASS_SHIELD)]
	public void Equip_OnAnEmptySlot_PutsTheItemThereAndPicksNothingUp(int slot, string itemClass)
	{
		var sack = Equipment();
		var item = ItemOf(itemClass, "new");

		var pickedUp = EquipmentRules.Equip(sack, item, slot, null);

		pickedUp.Should().BeNull();
		sack.Count.Should().Be(12);
		sack.GetItem(slot).Should().BeSameAs(item);
		item.Location.Should().Be(SackCollection.GetEquipmentLocationOffset(slot));
	}

	[Fact]
	public void Equip_OnAnEquippedItem_SwapsThemAndPicksTheOldOneUp()
	{
		var oldHelm = ItemOf(Item.ICLASS_HEAD, "old");
		var newHelm = ItemOf(Item.ICLASS_HEAD, "new");
		var sack = Equipment((Head, oldHelm));

		var pickedUp = EquipmentRules.Equip(sack, newHelm, Head, oldHelm);

		pickedUp.Should().BeSameAs(oldHelm);
		sack.GetItem(Head).Should().BeSameAs(newHelm);
		sack.Count.Should().Be(12);
		sack.Should().NotContain(oldHelm);
	}

	[Fact]
	public void Equip_OneHandedOnTheWeaponSlotOfAnotherOneHanded_KeepsTheShield()
	{
		var oldSword = Sword("old");
		var shield = Shield();
		var sack = Equipment((Weapon1, oldSword), (Shield1, shield));
		var newSword = Sword("new");

		var pickedUp = EquipmentRules.Equip(sack, newSword, Weapon1, oldSword);

		pickedUp.Should().BeSameAs(oldSword);
		sack.GetItem(Weapon1).Should().BeSameAs(newSword);
		sack.GetItem(Shield1).Should().BeSameAs(shield);
		sack.Count.Should().Be(12);
	}

	[Theory]
	[InlineData(Weapon1, Shield1)]
	[InlineData(Weapon2, Shield2)]
	public void Equip_OneHandedOnTheShadowSlotOfATwoHanded_PicksTheTwoHandedUp(int dropSlot, int twoHandedSlot)
	{
		var bow = Bow();
		var sack = Equipment((twoHandedSlot, bow));
		var sword = Sword();

		// The panel gives the two-handed weapon as the item under an empty shadow slot.
		var pickedUp = EquipmentRules.Equip(sack, sword, dropSlot, bow);

		pickedUp.Should().NotBeNull().And.NotBeSameAs(bow);
		pickedUp!.BaseItemId.Should().Be(bow.BaseItemId);
		pickedUp.Seed.Should().Be(bow.Seed);
		sack.GetItem(dropSlot).Should().BeSameAs(sword);
		IsEmpty(sack.GetItem(twoHandedSlot)).Should().BeTrue();
		sack.GetItem(twoHandedSlot).Location.Should().Be(SackCollection.GetEquipmentLocationOffset(twoHandedSlot));

		// The copy is the 13th item until it is placed elsewhere.
		sack.Count.Should().Be(13);
		sack.GetItem(12).Should().BeSameAs(pickedUp);
	}

	[Fact]
	public void Equip_ShieldOnTheTwoHandedWeapon_SwapsThem()
	{
		var bow = Bow();
		var sack = Equipment((Shield1, bow));
		var shield = Shield();

		var pickedUp = EquipmentRules.Equip(sack, shield, Shield1, bow);

		pickedUp.Should().BeSameAs(bow);
		sack.GetItem(Shield1).Should().BeSameAs(shield);
		IsEmpty(sack.GetItem(Weapon1)).Should().BeTrue();
		sack.Count.Should().Be(12);
	}

	#endregion

	#region Equip: two-handed weapons

	[Theory]
	[InlineData(Weapon1, Shield1)]
	[InlineData(Shield1, Shield1)]
	[InlineData(Weapon2, Shield2)]
	[InlineData(Shield2, Shield2)]
	public void Equip_TwoHandedOnAnEmptyPair_GoesInTheSecondSlot(int dropSlot, int expectedSlot)
	{
		var sack = Equipment();
		var bow = Bow();

		var pickedUp = EquipmentRules.Equip(sack, bow, dropSlot, null);

		pickedUp.Should().BeNull();
		sack.GetItem(expectedSlot).Should().BeSameAs(bow);
		bow.Location.Should().Be(SackCollection.GetEquipmentLocationOffset(expectedSlot));
		IsEmpty(sack.GetItem(expectedSlot - 1)).Should().BeTrue();
		sack.Count.Should().Be(12);
	}

	[Fact]
	public void Equip_TwoHandedOnTheOneHandedOfThePair_PicksTheOneHandedUp()
	{
		var sword = Sword();
		var sack = Equipment((Weapon1, sword));
		var bow = Bow();

		var pickedUp = EquipmentRules.Equip(sack, bow, Weapon1, sword);

		pickedUp!.BaseItemId.Should().Be(sword.BaseItemId);
		sack.GetItem(12).Should().BeSameAs(pickedUp);
		IsEmpty(sack.GetItem(Weapon1)).Should().BeTrue();
		sack.GetItem(Shield1).Should().BeSameAs(bow);
		sack.Count.Should().Be(13);
	}

	[Fact]
	public void Equip_TwoHandedOnTheEmptyWeaponSlotNextToAShield_PicksTheShieldUp()
	{
		var shield = Shield();
		var sack = Equipment((Shield1, shield));
		var bow = Bow();

		// Nothing under the empty weapon slot: the shield of the pair is picked up.
		var pickedUp = EquipmentRules.Equip(sack, bow, Weapon1, sack.GetItem(Weapon1));

		pickedUp!.BaseItemId.Should().Be(shield.BaseItemId);
		sack.GetItem(Shield1).Should().BeSameAs(bow);
		IsEmpty(sack.GetItem(Weapon1)).Should().BeTrue();
		sack.Count.Should().Be(13);
		sack.GetItem(12).Should().BeSameAs(pickedUp);
	}

	[Fact]
	public void Equip_TwoHandedOnTheShieldSlotNextToAOneHanded_PicksTheOneHandedUp()
	{
		var sword = Sword();
		var sack = Equipment((Weapon1, sword));
		var staff = Staff();

		var pickedUp = EquipmentRules.Equip(sack, staff, Shield1, null);

		pickedUp!.BaseItemId.Should().Be(sword.BaseItemId);
		sack.GetItem(Shield1).Should().BeSameAs(staff);
		IsEmpty(sack.GetItem(Weapon1)).Should().BeTrue();
		sack.Count.Should().Be(13);
	}

	[Fact]
	public void Equip_TwoHandedOnAShield_PicksTheShieldUp()
	{
		var shield = Shield();
		var sack = Equipment((Shield1, shield));
		var staff = Staff();

		var pickedUp = EquipmentRules.Equip(sack, staff, Shield1, shield);

		pickedUp!.BaseItemId.Should().Be(shield.BaseItemId);
		pickedUp.Should().NotBeSameAs(shield);
		sack.GetItem(Shield1).Should().BeSameAs(staff);
		IsEmpty(sack.GetItem(Weapon1)).Should().BeTrue();
		sack.Count.Should().Be(13);
	}

	[Theory]
	[InlineData(Weapon1)]
	[InlineData(Shield1)]
	public void Equip_TwoHandedOnAnotherTwoHanded_SwapsThem(int dropSlot)
	{
		var bow = Bow();
		var sack = Equipment((Shield1, bow));
		var staff = Staff();

		// Over the empty shadow slot, the panel gives the two-handed weapon as the item under us.
		var pickedUp = EquipmentRules.Equip(sack, staff, dropSlot, bow);

		pickedUp!.BaseItemId.Should().Be(bow.BaseItemId);
		sack.GetItem(Shield1).Should().BeSameAs(staff);
		IsEmpty(sack.GetItem(Weapon1)).Should().BeTrue();
		sack.Should().NotContain(bow);
		sack.Count.Should().Be(13);
	}

	[Fact]
	public void Equip_TwoHandedOnTheSecondSet_LeavesTheFirstSetAlone()
	{
		var sword = Sword("set1");
		var shield = Shield("set1");
		var offhand = Sword("set2");
		var sack = Equipment((Weapon1, sword), (Shield1, shield), (Weapon2, offhand));
		var bow = Bow();

		var pickedUp = EquipmentRules.Equip(sack, bow, Weapon2, offhand);

		pickedUp!.BaseItemId.Should().Be(offhand.BaseItemId);
		sack.GetItem(Weapon1).Should().BeSameAs(sword);
		sack.GetItem(Shield1).Should().BeSameAs(shield);
		IsEmpty(sack.GetItem(Weapon2)).Should().BeTrue();
		sack.GetItem(Shield2).Should().BeSameAs(bow);
	}

	[Fact]
	public void Equip_EmptyItemsPutInTheSlots_KeepTheBlockDataOfTheDroppedItem()
	{
		var sword = Sword();
		var sack = Equipment((Weapon1, sword));
		var bow = Bow();
		bow.beginBlockCrap1 = 11;
		bow.endBlockCrap2 = 22;

		EquipmentRules.Equip(sack, bow, Weapon1, sword);

		sack.GetItem(Weapon1).beginBlockCrap1.Should().Be(11);
		sack.GetItem(Weapon1).endBlockCrap2.Should().Be(22);
	}

	#endregion
}
