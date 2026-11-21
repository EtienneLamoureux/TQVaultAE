using System.Drawing;
using AwesomeAssertions;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Domain;

/// <summary>
/// <see cref="LoadoutComparison"/>: the worn equipment against a loadout, slot for slot, and where each slot's mark is
/// drawn.
/// </summary>
public class LoadoutComparisonTests
{
	private const int Slots = 12;

	private static Item Piece(int seed) => ItemKeyTests.Piece(seed);

	private static Item Empty() => new() { BaseItemId = RecordId.Empty };

	/// <summary>Worn equipment: empty items in the slots not given, as the save has them.</summary>
	private static Item?[] Worn(params (int Slot, Item Item)[] items)
	{
		var worn = Enumerable.Range(0, Slots).Select(_ => (Item?)Empty()).ToArray();
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
	public void CompareSlot_IdenticalCopy_IsSame()
		=> LoadoutComparison.CompareSlot(Piece(7), Piece(7).Clone()).Should().Be(LoadoutSlotMatch.Same);

	[Fact]
	public void CompareSlot_OtherSeed_IsDifferent()
		=> LoadoutComparison.CompareSlot(Piece(7), Piece(8)).Should().Be(LoadoutSlotMatch.Different);

	[Fact]
	public void CompareSlot_EmptyOnOneSide_IsOneEmpty()
	{
		LoadoutComparison.CompareSlot(Piece(7), null).Should().Be(LoadoutSlotMatch.OneEmpty);
		LoadoutComparison.CompareSlot(Piece(7), Empty()).Should().Be(LoadoutSlotMatch.OneEmpty);
		LoadoutComparison.CompareSlot(Empty(), Piece(7)).Should().Be(LoadoutSlotMatch.OneEmpty);
		LoadoutComparison.CompareSlot(null, Piece(7)).Should().Be(LoadoutSlotMatch.OneEmpty);
	}

	[Fact]
	public void CompareSlot_EmptyOnBothSides_IsBothEmpty()
	{
		LoadoutComparison.CompareSlot(Empty(), null).Should().Be(LoadoutSlotMatch.BothEmpty);
		LoadoutComparison.CompareSlot(null, Empty()).Should().Be(LoadoutSlotMatch.BothEmpty);
	}

	[Fact]
	public void Compare_GivesOneMarkPerSlotNotEmptyOnBothSides_InSlotOrder()
	{
		var helm = Piece(1);
		var marks = LoadoutComparison.Compare(
			Worn((EquipmentRules.HeadSlot, helm), (EquipmentRules.BodySlot, Piece(2)), (EquipmentRules.Ring1Slot, Piece(3))),
			Loadout((EquipmentRules.HeadSlot, helm), (EquipmentRules.BodySlot, Piece(4)), (EquipmentRules.ArtifactSlot, Piece(5))));

		marks.Select(m => (m.Slot, m.Match)).Should().Equal(
			(EquipmentRules.HeadSlot, LoadoutSlotMatch.Same),
			(EquipmentRules.BodySlot, LoadoutSlotMatch.Different),
			(EquipmentRules.Ring1Slot, LoadoutSlotMatch.OneEmpty),
			(EquipmentRules.ArtifactSlot, LoadoutSlotMatch.OneEmpty));
		marks.Select(m => m.CanCompare).Should().Equal(true, true, false, false);
		marks.Should().OnlyContain(m => m.Bounds == LoadoutComparison.MarkBounds(m.Slot));
	}

	[Fact]
	public void Compare_SwappedRings_AreDifferentSlotForSlot()
	{
		var ringA = Piece(1);
		var ringB = Piece(2);

		var marks = LoadoutComparison.Compare(
			Worn((EquipmentRules.Ring1Slot, ringB), (EquipmentRules.Ring2Slot, ringA)),
			Loadout((EquipmentRules.Ring1Slot, ringA), (EquipmentRules.Ring2Slot, ringB)));

		marks.Should().OnlyContain(m => m.Match == LoadoutSlotMatch.Different).And.HaveCount(2);
	}

	[Fact]
	public void Compare_ShorterLoadout_TakesTheMissingSlotsAsEmpty()
	{
		var marks = LoadoutComparison.Compare(Worn((EquipmentRules.ArtifactSlot, Piece(1))), [Piece(2)]);

		marks.Select(m => (m.Slot, m.Match)).Should().Equal(
			(EquipmentRules.HeadSlot, LoadoutSlotMatch.OneEmpty),
			(EquipmentRules.ArtifactSlot, LoadoutSlotMatch.OneEmpty));
	}

	[Fact]
	public void Compare_SameEquipment_AllSame()
	{
		var worn = Worn(Enumerable.Range(0, Slots).Select(slot => (slot, Piece(slot + 1))).ToArray());

		LoadoutComparison.Compare(worn, Loadout(Enumerable.Range(0, Slots).Select(slot => (slot, worn[slot]!)).ToArray()))
			.Should().HaveCount(Slots).And.OnlyContain(m => m.Match == LoadoutSlotMatch.Same);
	}

	public static TheoryData<int, string> Sides => new()
	{
		{ EquipmentRules.Weapon1Slot, "left" },
		{ EquipmentRules.Weapon2Slot, "left" },
		{ EquipmentRules.ArtifactSlot, "left" },
		{ EquipmentRules.Shield1Slot, "right" },
		{ EquipmentRules.Shield2Slot, "right" },
		{ EquipmentRules.ArmsSlot, "right" },
		{ EquipmentRules.HeadSlot, "below" },
		{ EquipmentRules.NeckSlot, "below" },
		{ EquipmentRules.BodySlot, "below" },
		{ EquipmentRules.LegsSlot, "below" },
		{ EquipmentRules.Ring1Slot, "below" },
		{ EquipmentRules.Ring2Slot, "below" },
	};

	[Theory]
	[MemberData(nameof(Sides))]
	public void MarkBounds_IsBesideItsSlot(int slot, string side)
	{
		var mark = LoadoutComparison.MarkBounds(slot);
		var cells = SlotCells(slot);

		mark.Width.Should().Be(LoadoutComparison.MarkSize);
		mark.Height.Should().Be(LoadoutComparison.MarkSize);
		switch (side)
		{
			case "left":
				mark.Right.Should().BeLessThanOrEqualTo(cells.Left);
				mark.Top.Should().Be(cells.Top);
				break;
			case "right":
				mark.Left.Should().BeGreaterThanOrEqualTo(cells.Right);
				mark.Top.Should().Be(cells.Top);
				break;
			default:
				mark.Top.Should().BeGreaterThan(cells.Bottom);
				(mark.Left + (mark.Width / 2F)).Should().Be(cells.Left + (cells.Width / 2F));
				break;
		}
	}

	[Fact]
	public void MarkBounds_StayInThePanelInTheGaps_ClearOfEverySlotFrame()
	{
		// The slot frames are drawn a little outside their cells.
		const float frame = 0.1F;
		var panel = new RectangleF(0, 0, 10, 14);
		var marks = Enumerable.Range(0, Slots).Select(LoadoutComparison.MarkBounds).ToList();

		foreach (var mark in marks)
		{
			panel.Contains(mark).Should().BeTrue($"{mark} is in the panel");
			foreach (int slot in Enumerable.Range(0, Slots))
			{
				var cells = SlotCells(slot);
				cells.Inflate(frame, frame);
				cells.IntersectsWith(mark).Should().BeFalse($"{mark} is clear of slot {slot}");
			}
		}

		for (int i = 0; i < marks.Count; i++)
			for (int j = i + 1; j < marks.Count; j++)
				marks[i].IntersectsWith(marks[j]).Should().BeFalse($"the marks of slots {i} and {j} do not overlap");
	}

	[Fact]
	public void MarkBounds_NotASlot_Throws()
	{
		var act = () => LoadoutComparison.MarkBounds(Slots);
		act.Should().Throw<ArgumentOutOfRangeException>();
	}

	private static RectangleF SlotCells(int slot)
	{
		var location = SackCollection.GetEquipmentLocationOffset(slot);
		return SackCollection.IsWeaponSlot(slot)
			? new RectangleF(SackCollection.GetWeaponLocationOffset(location.Y), SackCollection.WeaponLocationSize)
			: new RectangleF(location, SackCollection.GetEquipmentLocationSize(slot));
	}
}
