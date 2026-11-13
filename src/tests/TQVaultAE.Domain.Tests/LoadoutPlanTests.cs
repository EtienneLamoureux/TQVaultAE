using AwesomeAssertions;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Domain;

/// <summary>
/// <see cref="LoadoutPlan"/>: identical pieces stay worn (moved when in another slot), the others are copied in, the
/// worn pieces the loadout does not use leave.
/// </summary>
public class LoadoutPlanTests
{
	private const int Slots = 12;
	private const int Ring1 = EquipmentRules.Ring1Slot;
	private const int Ring2 = EquipmentRules.Ring2Slot;
	private const int Weapon1 = EquipmentRules.Weapon1Slot;
	private const int Shield1 = EquipmentRules.Shield1Slot;
	private const int Weapon2 = EquipmentRules.Weapon2Slot;
	private const int Shield2 = EquipmentRules.Shield2Slot;

	private static Item Piece(int seed) => ItemIdentityTests.Piece(seed);

	/// <summary>Worn equipment: empty items in the slots not given, as the save has them.</summary>
	private static Item?[] Worn(params (int Slot, Item Item)[] items)
	{
		var worn = Enumerable.Range(0, Slots).Select(_ => (Item?)new Item { BaseItemId = RecordId.Empty }).ToArray();
		foreach (var (slot, item) in items)
			worn[slot] = item;
		return worn;
	}

	/// <summary>Loadout pieces: copies of the given items, null in the other slots.</summary>
	private static Item?[] Loadout(params (int Slot, Item Item)[] items)
	{
		var pieces = new Item?[Slots];
		foreach (var (slot, item) in items)
			pieces[slot] = item.Clone();
		return pieces;
	}

	[Fact]
	public void Make_WornEquipmentIsTheLoadout_ChangesNothing()
	{
		var helm = Piece(1);
		var ring = Piece(2);

		var plan = LoadoutPlan.Make(Worn((0, helm), (Ring1, ring)), Loadout((0, helm), (Ring1, ring)));

		plan.ChangesNothing.Should().BeTrue();
		plan.LeavingSlots.Should().BeEmpty();
	}

	[Fact]
	public void Make_RingsSwapped_MovesThemWithoutCopyNorLeaving()
	{
		var ringA = Piece(1);
		var ringB = Piece(2);

		var plan = LoadoutPlan.Make(Worn((Ring1, ringB), (Ring2, ringA)), Loadout((Ring1, ringA), (Ring2, ringB)));

		plan.Steps[Ring1].Should().Be(new LoadoutSlotStep(Ring1, LoadoutSlotAction.Move, Ring2));
		plan.Steps[Ring2].Should().Be(new LoadoutSlotStep(Ring2, LoadoutSlotAction.Move, Ring1));
		plan.LeavingSlots.Should().BeEmpty();
		plan.CopyCount.Should().Be(0);
		plan.ChangesNothing.Should().BeFalse();
	}

	[Fact]
	public void Make_WeaponSetsSwapped_MovesTheFourWeapons()
	{
		var sword = Piece(1);
		var shield = Piece(2);
		var bow = Piece(3);
		var empty = new Item { BaseItemId = RecordId.Empty };

		// Worn: bow (two-handed, in the second slot of the pair) in set 1, sword and shield in set 2.
		var plan = LoadoutPlan.Make(
			Worn((Weapon1, empty), (Shield1, bow), (Weapon2, sword), (Shield2, shield)),
			Loadout((Weapon1, sword), (Shield1, shield), (Shield2, bow)));

		plan.Steps[Weapon1].Should().Be(new LoadoutSlotStep(Weapon1, LoadoutSlotAction.Move, Weapon2));
		plan.Steps[Shield1].Should().Be(new LoadoutSlotStep(Shield1, LoadoutSlotAction.Move, Shield2));
		plan.Steps[Weapon2].Action.Should().Be(LoadoutSlotAction.Clear, "its sword moved to the first set");
		plan.Steps[Shield2].Should().Be(new LoadoutSlotStep(Shield2, LoadoutSlotAction.Move, Shield1));
		plan.LeavingSlots.Should().BeEmpty();
	}

	[Fact]
	public void Make_SameBaseOtherSeed_CopiesTheLoadoutPieceAndTheWornOneLeaves()
	{
		var helm = Piece(1);
		var otherRoll = Piece(2);

		var plan = LoadoutPlan.Make(Worn((0, otherRoll)), Loadout((0, helm)));

		plan.Steps[0].Action.Should().Be(LoadoutSlotAction.Copy);
		plan.LeavingSlots.Should().Equal(0);
	}

	[Fact]
	public void Make_EmptyLoadoutSlot_ClearsItAndTheWornPieceLeaves()
	{
		var artifact = Piece(1);

		var plan = LoadoutPlan.Make(Worn((EquipmentRules.ArtifactSlot, artifact)), Loadout());

		plan.Steps[EquipmentRules.ArtifactSlot].Action.Should().Be(LoadoutSlotAction.Clear);
		plan.LeavingSlots.Should().Equal(EquipmentRules.ArtifactSlot);
	}

	[Fact]
	public void Make_PieceWornInBothRingSlots_KeepsTheOneInItsSlotAndMovesNoneAway()
	{
		// Two copies of the same ring are worn; the loadout has it in Ring1 only: Ring1 keeps its own, Ring2's leaves.
		var ring = Piece(5);

		var plan = LoadoutPlan.Make(Worn((Ring1, ring), (Ring2, ring.Clone())), Loadout((Ring1, ring)));

		plan.Steps[Ring1].Action.Should().Be(LoadoutSlotAction.Keep);
		plan.LeavingSlots.Should().Equal(Ring2);
	}

	[Fact]
	public void Make_PieceMovedFromASlotTheLoadoutFillsWithACopy_LeavesOnlyTheUnusedPieces()
	{
		var ringA = Piece(1);
		var ringB = Piece(2);
		var ringC = Piece(3);

		// ringA moves from Ring2 to Ring1; Ring2 gets a copy of ringC; ringB (worn in Ring1) is not in the loadout.
		var plan = LoadoutPlan.Make(Worn((Ring1, ringB), (Ring2, ringA)), Loadout((Ring1, ringA), (Ring2, ringC)));

		plan.Steps[Ring1].Should().Be(new LoadoutSlotStep(Ring1, LoadoutSlotAction.Move, Ring2));
		plan.Steps[Ring2].Action.Should().Be(LoadoutSlotAction.Copy);
		plan.LeavingSlots.Should().Equal(Ring1);
		plan.CopyCount.Should().Be(1);
		plan.MoveCount.Should().Be(1);
	}

	[Fact]
	public void Make_LoadoutOfMoreSlotsThanTheEquipment_IgnoresTheExtraSlots()
	{
		// A character of the original game has 11 slots (no artifact).
		var worn = Worn().Take(11).ToArray();

		var plan = LoadoutPlan.Make(worn, Loadout((EquipmentRules.ArtifactSlot, Piece(1))));

		plan.Steps.Should().HaveCount(11);
		plan.ChangesNothing.Should().BeTrue();
	}
}
