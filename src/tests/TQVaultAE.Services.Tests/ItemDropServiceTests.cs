using System.Drawing;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

/// <summary>
/// <see cref="ItemDropService"/>: an item dropped with the mouse on a sack of a vault, a player or a stash.
/// </summary>
public class ItemDropServiceTests
{
	private static readonly Point Cell = new(5, 2);

	private readonly Mock<IPlayerService> _playerService = new();
	private readonly Mock<IItemDatabaseService> _itemDatabaseService = new();
	private readonly ItemDropService _service;

	public ItemDropServiceTests()
	{
		this._playerService.Setup(p => p.IsSuitableForCurrentPlayer(It.IsAny<Item>())).Returns(true);
		this._service = new ItemDropService(
			new ItemMovementService(new Mock<ILogger<ItemMovementService>>().Object)
			, new ItemStackingService(new Mock<IItemProvider>().Object)
			, this._itemDatabaseService.Object
			, this._playerService.Object);
	}

	#region Helpers

	private static Item MakeItem(string itemClass, string id = "records/item/thing.dbr")
	{
		var record = new DBRecordCollection(RecordId.Create(id), itemClass);
		var cls = new Variable("Class", VariableDataType.StringVar, 1);
		cls[0] = itemClass;
		record.Set(cls);
		return new Item { BaseItemId = RecordId.Create(id), baseItemInfo = new Info(record), Width = 1, Height = 1 };
	}

	private static Item Potions(int count)
	{
		var potions = MakeItem(Item.ICLASS_POTIONHEALTH, "records/item/potion.dbr");
		potions.StackSize = count;
		return potions;
	}

	private static ContainerPlace VaultPlace() => new()
	{
		SackType = SackType.Vault,
		Path = """C:\Vaults\Main.vault.json""",
		Name = "Main",
		SackNumber = 3,
	};

	private void PlayerCannotUse(Item item)
		=> this._playerService.Setup(p => p.IsSuitableForCurrentPlayer(item)).Returns(false);

	#endregion

	#region Decide

	[Theory]
	[InlineData(SackType.Vault, null, DropDecision.Drop)]
	[InlineData(SackType.Stash, StashType.TransferStash, DropDecision.Drop)]
	[InlineData(SackType.Stash, StashType.RelicVaultStash, DropDecision.Refused)]
	public void Decide_ASword_DependsOnTheStashType(SackType sackType, StashType? stashType, DropDecision expected)
	{
		var sword = MakeItem(Item.ICLASS_SWORD);

		this._service.Decide(sword, null, sackType, stashType, canCancelDrag: true).Should().Be(expected);
	}

	[Theory]
	[InlineData(Item.ICLASS_RELIC)]
	[InlineData(Item.ICLASS_CHARM)]
	[InlineData(Item.ICLASS_ARTIFACT)]
	[InlineData(Item.ICLASS_FORMULA)]
	public void Decide_RelicVault_TakesRelicsCharmsArtifactsAndFormulae(string itemClass)
	{
		var item = MakeItem(itemClass);

		this._service.Decide(item, null, SackType.Stash, StashType.RelicVaultStash, canCancelDrag: true)
			.Should().Be(DropDecision.Drop);
	}

	[Theory]
	[InlineData(SackType.Player, DropDecision.Refused)]
	[InlineData(SackType.Equipment, DropDecision.Refused)]
	[InlineData(SackType.Vault, DropDecision.Drop)]
	[InlineData(SackType.Stash, DropDecision.Drop)]
	[InlineData(SackType.Trash, DropDecision.Drop)]
	public void Decide_ItemTheCharacterCannotUse_RefusedOnlyByThePlayerAndEquipment(SackType sackType, DropDecision expected)
	{
		var item = MakeItem(Item.ICLASS_SWORD);
		this.PlayerCannotUse(item);

		this._service.Decide(item, null, sackType, null, canCancelDrag: true).Should().Be(expected);
	}

	[Fact]
	public void Decide_StashRuleComesFirst_ThePlayerIsNotAsked()
	{
		var sword = MakeItem(Item.ICLASS_SWORD);

		this._service.Decide(sword, null, SackType.Stash, StashType.RelicVaultStash, canCancelDrag: true);

		this._playerService.Verify(p => p.IsSuitableForCurrentPlayer(It.IsAny<Item>()), Times.Never);
	}

	[Fact]
	public void Decide_OnItself_PutsItBack()
	{
		var item = MakeItem(Item.ICLASS_RING);

		this._service.Decide(item, item, SackType.Vault, null, canCancelDrag: true).Should().Be(DropDecision.PutBack);
	}

	[Fact]
	public void Decide_OnItselfWhenTheDragCannotBeCancelled_Drops()
	{
		// A new or modified item has no place to go back to.
		var item = MakeItem(Item.ICLASS_RING);

		this._service.Decide(item, item, SackType.Vault, null, canCancelDrag: false).Should().Be(DropDecision.Drop);
	}

	[Fact]
	public void Decide_OnItselfButRefused_RefusalWins()
	{
		var item = MakeItem(Item.ICLASS_SWORD);
		this.PlayerCannotUse(item);

		this._service.Decide(item, item, SackType.Player, null, canCancelDrag: true).Should().Be(DropDecision.Refused);
	}

	[Fact]
	public void Decide_OnAnotherItem_Drops()
	{
		var item = MakeItem(Item.ICLASS_RING);
		var other = MakeItem(Item.ICLASS_RING);

		this._service.Decide(item, other, SackType.Vault, null, canCancelDrag: true).Should().Be(DropDecision.Drop);
	}

	#endregion

	#region Drop

	[Fact]
	public void Drop_OnAnEmptyCell_PlacesTheItemAndRegistersIt()
	{
		var sack = new SackCollection();
		var ring = MakeItem(Item.ICLASS_RING);
		var place = VaultPlace();

		var result = this._service.Drop(ring, null, sack, Cell, place, disableAutoStacking: false);

		result.Should().Be(new ItemDropResult(false, ring, null));
		sack.Should().ContainSingle().Which.Should().BeSameAs(ring);
		ring.Location.Should().Be(Cell);
		ring.Place.Should().Be(place);
		this._itemDatabaseService.Verify(d => d.TryAddItemToDatabase(ring), Times.Once);
	}

	[Fact]
	public void Drop_KeepsThePlaceInstanceOfTheItem()
	{
		// The search result of the item reads the same instance.
		var ring = MakeItem(Item.ICLASS_RING);
		var placeBefore = ring.Place;

		this._service.Drop(ring, null, new SackCollection(), Cell, VaultPlace(), disableAutoStacking: false);

		ring.Place.Should().BeSameAs(placeBefore);
	}

	[Fact]
	public void Drop_OnAnotherItem_PlacesItAndHandsTheOtherBack()
	{
		var sack = new SackCollection();
		var other = MakeItem(Item.ICLASS_AMULET);
		sack.AddItem(other);
		var ring = MakeItem(Item.ICLASS_RING);

		var result = this._service.Drop(ring, other, sack, Cell, VaultPlace(), disableAutoStacking: false);

		result.Should().Be(new ItemDropResult(false, ring, other));
		sack.Should().Contain(ring);
	}

	[Fact]
	public void Drop_PotionsOnTheSamePotions_StacksThemAndDoesNotPlace()
	{
		var sack = new SackCollection();
		var inSack = Potions(3);
		inSack.Location = Cell;
		sack.AddItem(inSack);
		var dragged = Potions(2);

		var result = this._service.Drop(dragged, inSack, sack, Cell, VaultPlace(), disableAutoStacking: false);

		result.Stacked.Should().BeTrue();
		result.ItemUnderUs.Should().BeNull();
		inSack.StackSize.Should().Be(5);
		sack.Should().ContainSingle().Which.Should().BeSameAs(inSack);
		this._itemDatabaseService.Verify(d => d.TryAddItemToDatabase(It.IsAny<Item>()), Times.Never);
	}

	[Fact]
	public void Drop_PotionsWithStackingTurnedOff_PlacesThemAndPicksTheOthersUp()
	{
		var sack = new SackCollection();
		var inSack = Potions(3);
		sack.AddItem(inSack);
		var dragged = Potions(2);

		var result = this._service.Drop(dragged, inSack, sack, Cell, VaultPlace(), disableAutoStacking: true);

		result.Should().Be(new ItemDropResult(false, dragged, inSack));
		inSack.StackSize.Should().Be(3);
		sack.Should().HaveCount(2);
		this._itemDatabaseService.Verify(d => d.TryAddItemToDatabase(dragged), Times.Once);
	}

	#endregion

	#region Equip

	/// <summary>An equipment sack: one item per slot, empty items in the slots not given.</summary>
	private static SackCollection Equipment(params (int Slot, Item Item)[] equipped)
	{
		var sack = new SackCollection { SackType = SackType.Equipment, slots = 12 };
		for (int slot = 0; slot < 12; slot++)
		{
			var item = equipped.FirstOrDefault(e => e.Slot == slot).Item ?? new Item { BaseItemId = RecordId.Empty };
			item.Location = SackCollection.GetEquipmentLocationOffset(slot);
			sack.AddItem(item);
		}

		return sack;
	}

	private static ContainerPlace EquipmentPlace() => new()
	{
		SackType = SackType.Equipment,
		Path = """C:\Saves\_HeroA\Player.chr""",
		Name = "HeroA",
		SackNumber = BagIdConstants.BAGID_EQUIPMENTPANEL,
	};

	[Fact]
	public void Equip_OnAnEquippedRing_PutsItInTheSlotRegistersItAndHandsTheOldOneBack()
	{
		var oldRing = MakeItem(Item.ICLASS_RING, "records/item/oldring.dbr");
		var sack = Equipment((EquipmentRules.Ring1Slot, oldRing));
		var ring = MakeItem(Item.ICLASS_RING);
		var place = EquipmentPlace();

		var result = this._service.Equip(ring, oldRing, sack, EquipmentRules.Ring1Slot, oldRing.Location, place, disableAutoStacking: false);

		result.Should().Be(new ItemDropResult(false, ring, oldRing));
		sack.GetItem(EquipmentRules.Ring1Slot).Should().BeSameAs(ring);
		ring.Location.Should().Be(SackCollection.GetEquipmentLocationOffset(EquipmentRules.Ring1Slot));
		ring.Place.Should().Be(place);
		this._itemDatabaseService.Verify(d => d.TryAddItemToDatabase(ring), Times.Once);
	}

	[Fact]
	public void Equip_OneHandedOnTheShadowSlotOfATwoHanded_HandsTheTwoHandedBack()
	{
		var bow = MakeItem(Item.ICLASS_BOW, "records/item/bow.dbr");
		var sack = Equipment((EquipmentRules.Shield1Slot, bow));
		var sword = MakeItem(Item.ICLASS_SWORD, "records/item/sword.dbr");

		var result = this._service.Equip(sword, bow, sack, EquipmentRules.Weapon1Slot, Cell, EquipmentPlace(), disableAutoStacking: false);

		result.Stacked.Should().BeFalse();
		result.ItemUnderUs!.BaseItemId.Should().Be(bow.BaseItemId);
		sack.GetItem(EquipmentRules.Weapon1Slot).Should().BeSameAs(sword);
		RecordId.IsNullOrEmpty(sack.GetItem(EquipmentRules.Shield1Slot).BaseItemId).Should().BeTrue();
	}

	#endregion
}
