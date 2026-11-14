using System.Drawing;
using AwesomeAssertions;
using Moq;
using TQVaultAE.Application;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Services;

/// <summary>
/// <see cref="TQVaultAE.Services.LoadoutService.Load"/>: identical pieces stay worn, the others are copied in, the pieces
/// taken off go to the bag, and nothing changes without room for them.
/// </summary>
public sealed partial class LoadoutServiceTests
{
	private static readonly ContainerPlace BagPlace = new() { Path = "vault.json", Name = "VaultA", SackNumber = 2, SackType = SackType.Vault };

	/// <summary>An empty vault bag, or one whose cells are all taken but <paramref name="freeCells"/> (1x2 slots, left to right).</summary>
	private static SackCollection Bag(Size size, int? freeSlots = null)
	{
		var bag = new SackCollection { SackType = SackType.Vault };
		if (freeSlots is { } free)
		{
			// Fill the bag with 1x2 items, leaving the last `free` columns of the top row pair empty.
			for (int x = 0; x < size.Width; x++)
				for (int y = 0; y < size.Height; y += 2)
				{
					if (y == 0 && x >= size.Width - free)
						continue;
					bag.AddItem(new Item { BaseItemId = RecordId.Create("records/item/filler.dbr"), PositionX = x, PositionY = y, Width = 1, Height = 2 });
				}
		}

		bag.IsModified = false;
		return bag;
	}

	/// <summary>Worn equipment as read from the save: places set, nothing modified.</summary>
	private static PlayerCollection Worn(bool useAlternate, params (int Slot, Item Item)[] pieces)
	{
		var player = Wearing(useAlternate, pieces);
		foreach (var item in player.EquipmentSack!)
			item.Place = new ContainerPlace { Path = "Player.chr", Name = "HeroA", SackType = SackType.Equipment };
		player.EquipmentSack!.IsModified = false;
		return player;
	}

	private LoadoutSet WithLoadout(string name, bool useAlternate, params (int Slot, Item Item)[] pieces)
	{
		var set = this.Read();
		this._service.Add(set, name, Wearing(useAlternate, pieces)).IsSuccess.Should().BeTrue();
		return set;
	}

	[Fact]
	public void Load_IdenticalPiecesStayAndTheOthersAreCopiedIn_TheReplacedPieceGoesToTheBag()
	{
		var helm = Piece("helm", 1);
		var ring = Piece("ring", 2);
		var newTorso = Piece("torso", 3);
		var set = this.WithLoadout("A", false, (0, helm), (EquipmentRules.Ring1Slot, ring), (2, Piece("torso", 4)));
		var player = Worn(false, (0, helm), (EquipmentRules.Ring1Slot, ring), (2, newTorso));
		var bag = Bag(new Size(4, 4));

		var result = this._service.Load(set, "A", player, bag, new Size(4, 4), BagPlace);

		result.Outcome.Should().Be(LoadoutLoadOutcome.Loaded);
		result.ToBag.Should().Be(1);
		result.Copied.Should().Be(1);
		var equipment = player.EquipmentSack!;
		equipment.GetItem(0).Should().BeSameAs(helm, "an identical piece is not replaced by a copy");
		equipment.GetItem(EquipmentRules.Ring1Slot).Should().BeSameAs(ring);
		equipment.GetItem(2).Seed.Should().Be(4);
		equipment.GetItem(2).Location.Should().Be(SackCollection.GetEquipmentLocationOffset(2));
		equipment.IsModified.Should().BeTrue();
		bag.Should().ContainSingle().Which.Should().BeSameAs(newTorso);
		newTorso.Place.Name.Should().Be("VaultA", "the search follows the piece into the bag");
		result.Stored.Should().Equal(newTorso);
		this._database.Verify(d => d.AddItemToDatabase(equipment.GetItem(2), "Player.chr", "HeroA", 0, SackType.Equipment), Times.Once);
	}

	[Fact]
	public void LoadReplacing_DeletesTheWornPiecesTheLoadoutDoesNotUse_AndTheyLeaveTheSearch()
	{
		var helm = Piece("helm", 1);
		var wornTorso = Piece("torso", 3);
		var wornRing = Piece("ring", 5);
		var set = this.WithLoadout("A", false, (0, helm), (2, Piece("torso", 4)));
		var player = Worn(false, (0, helm), (2, wornTorso), (EquipmentRules.Ring1Slot, wornRing));

		var result = this._service.LoadReplacing(set, "A", player);

		result.Outcome.Should().Be(LoadoutLoadOutcome.Loaded);
		result.Replaced.Should().Equal(wornTorso, wornRing);
		result.Stored.Should().BeEmpty();
		result.ToBag.Should().Be(0);
		result.Copied.Should().Be(1);
		var equipment = player.EquipmentSack!;
		equipment.GetItem(0).Should().BeSameAs(helm);
		equipment.GetItem(2).Seed.Should().Be(4);
		ItemKey.IsEmptyItem(equipment.GetItem(EquipmentRules.Ring1Slot)).Should().BeTrue();
		equipment.Should().NotContain(wornTorso).And.NotContain(wornRing);
		this._database.Verify(d => d.RemoveItemFromDatabase(wornTorso), Times.Once);
		this._database.Verify(d => d.RemoveItemFromDatabase(wornRing), Times.Once);
	}

	[Fact]
	public void TakenOff_ListsTheWornPiecesALoadWouldTakeOff_InSlotOrder()
	{
		var helm = Piece("helm", 1);
		var wornTorso = Piece("torso", 3);
		var wornRing = Piece("ring", 5);
		var set = this.WithLoadout("A", false, (0, helm), (2, Piece("torso", 4)));
		var player = Worn(false, (EquipmentRules.Ring1Slot, wornRing), (0, helm), (2, wornTorso));

		this._service.TakenOff(set, "A", player).Should().Equal(wornTorso, wornRing);
		this._service.TakenOff(set, "Missing", player).Should().BeEmpty();
		player.EquipmentSack!.IsModified.Should().BeFalse("listing changes nothing");
	}

	[Fact]
	public void Load_RingsAndWeaponSetsSwapped_MovesThePiecesWithoutCopyNorBag()
	{
		var ringA = Piece("ringa", 1);
		var ringB = Piece("ringb", 2);
		var sword = Piece("sword", 3);
		var set = this.WithLoadout("A", true, (EquipmentRules.Ring1Slot, ringA), (EquipmentRules.Ring2Slot, ringB), (EquipmentRules.Weapon1Slot, sword));
		var player = Worn(false, (EquipmentRules.Ring1Slot, ringB), (EquipmentRules.Ring2Slot, ringA), (EquipmentRules.Weapon2Slot, sword));
		var bag = Bag(new Size(4, 4));

		var result = this._service.Load(set, "A", player, bag, new Size(4, 4), BagPlace);

		result.Outcome.Should().Be(LoadoutLoadOutcome.Loaded);
		result.Moved.Should().Be(3);
		result.Copied.Should().Be(0);
		var equipment = player.EquipmentSack!;
		equipment.GetItem(EquipmentRules.Ring1Slot).Should().BeSameAs(ringA);
		equipment.GetItem(EquipmentRules.Ring2Slot).Should().BeSameAs(ringB);
		equipment.GetItem(EquipmentRules.Weapon1Slot).Should().BeSameAs(sword);
		sword.Location.Should().Be(SackCollection.GetEquipmentLocationOffset(EquipmentRules.Weapon1Slot));
		ItemKey.IsEmptyItem(equipment.GetItem(EquipmentRules.Weapon2Slot)).Should().BeTrue();
		player.UseAlternate.Should().BeTrue("the loadout's weapon set is put in hand");
		bag.Should().BeEmpty();
	}

	[Fact]
	public void Load_NotEnoughRoomInTheBag_ChangesNothing()
	{
		var set = this.WithLoadout("A", false, (0, Piece("helm", 1)), (2, Piece("torso", 2)));
		var wornHelm = Piece("helm", 9);
		var wornTorso = Piece("torso", 8);
		var player = Worn(false, (0, wornHelm), (2, wornTorso));
		var bag = Bag(new Size(4, 4), freeSlots: 1);
		var before = bag.ToList();

		var result = this._service.Load(set, "A", player, bag, new Size(4, 4), BagPlace);

		result.Outcome.Should().Be(LoadoutLoadOutcome.NoRoom);
		result.ToBag.Should().Be(2);
		bag.Should().Equal(before);
		bag.IsModified.Should().BeFalse();
		player.EquipmentSack!.GetItem(0).Should().BeSameAs(wornHelm);
		player.EquipmentSack.GetItem(2).Should().BeSameAs(wornTorso);
		player.EquipmentSack.IsModified.Should().BeFalse();
		this._database.Verify(d => d.AddItemToDatabase(It.IsAny<Item>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<SackType>(), It.IsAny<StashType?>()), Times.Never);
	}

	[Fact]
	public void Load_RoomForThePiecesThatMoveOnly_IsEnough()
	{
		// Eleven pieces stay; one leaves: one free slot is room enough.
		var pieces = Enumerable.Range(0, 12).Select(slot => (slot, Piece($"item{slot}", slot + 1))).ToArray();
		var set = this.WithLoadout("A", false, [.. pieces.Take(11), (11, Piece("artifact", 100))]);
		var player = Worn(false, pieces);
		var bag = Bag(new Size(4, 4), freeSlots: 1);

		var result = this._service.Load(set, "A", player, bag, new Size(4, 4), BagPlace);

		result.Outcome.Should().Be(LoadoutLoadOutcome.Loaded);
		result.ToBag.Should().Be(1);
	}

	[Fact]
	public void Load_LoadoutAlreadyWorn_ChangesNothing()
	{
		var helm = Piece("helm", 1);
		var set = this.WithLoadout("A", true, (0, helm));
		var player = Worn(true, (0, helm));

		var result = this._service.Load(set, "A", player, Bag(new Size(4, 4)), new Size(4, 4), BagPlace);

		result.Outcome.Should().Be(LoadoutLoadOutcome.AlreadyWorn);
		player.EquipmentSack!.IsModified.Should().BeFalse();
	}

	[Fact]
	public void Load_OnlyTheWeaponSetDiffers_PutsItInHand()
	{
		var helm = Piece("helm", 1);
		var set = this.WithLoadout("A", true, (0, helm));
		var player = Worn(false, (0, helm));

		var result = this._service.Load(set, "A", player, Bag(new Size(4, 4)), new Size(4, 4), BagPlace);

		result.Outcome.Should().Be(LoadoutLoadOutcome.Loaded);
		player.UseAlternate.Should().BeTrue();
		player.EquipmentSack!.IsModified.Should().BeTrue("the save must write the weapon set in hand");
	}

	[Fact]
	public void Load_ReadOnlyCharacter_IsRefused()
	{
		var set = this.WithLoadout("A", false, (0, Piece("helm", 1)));
		var player = Worn(false, (0, Piece("helm", 2)));
		player.IsReadOnly = true;

		this._service.Load(set, "A", player, Bag(new Size(4, 4)), new Size(4, 4), BagPlace).Outcome.Should().Be(LoadoutLoadOutcome.Refused);
		player.EquipmentSack!.GetItem(0).Seed.Should().Be(2);
	}

	[Fact]
	public void Load_CopyTakesTheBlockValuesOfTheItemItReplaces()
	{
		var set = this.WithLoadout("A", false, (0, Piece("helm", 1)));
		var player = Worn(false);
		var empty = player.EquipmentSack!.GetItem(0);
		empty.beginBlockCrap1 = 11;
		empty.endBlockCrap2 = 22;

		this._service.Load(set, "A", player, Bag(new Size(4, 4)), new Size(4, 4), BagPlace);

		var copy = player.EquipmentSack.GetItem(0);
		copy.Seed.Should().Be(1);
		copy.beginBlockCrap1.Should().Be(11);
		copy.endBlockCrap2.Should().Be(22);
		copy.Should().NotBeSameAs(set.Loadouts[0].Pieces[0], "the loadout keeps its own piece");
		this._database.Verify(d => d.RemoveItemFromDatabase(empty), Times.Once);
	}
}
