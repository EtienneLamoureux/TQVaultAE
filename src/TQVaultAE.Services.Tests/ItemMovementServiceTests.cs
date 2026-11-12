using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Drawing;
using TQVaultAE.Application;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

/// <summary>
/// Unit tests for ItemMovementService class
/// </summary>
public class ItemMovementServiceTests
{
	private readonly Mock<ILogger<ItemMovementService>> _mockLogger;
	private readonly ItemMovementService _service;

	public ItemMovementServiceTests()
	{
		_mockLogger = new Mock<ILogger<ItemMovementService>>();
		_service = new ItemMovementService(_mockLogger.Object);
	}

	/// <summary>
	/// Test FindOpenCells with null sack returns (-1, -1)
	/// </summary>
	[Fact]
	public void FindOpenCells_WithNullSack_ReturnsInvalidPosition()
	{
		// Act
		var result = _service.FindOpenCells(null!, 2, 2, 10, 10);

		// Assert
		result.X.Should().Be(-1);
		result.Y.Should().Be(-1);
	}

	/// <summary>
	/// Test FindOpenCells with zero width returns (-1, -1)
	/// </summary>
	[Fact]
	public void FindOpenCells_WithZeroWidth_ReturnsInvalidPosition()
	{
		// Arrange
		var sack = new SackCollection();

		// Act
		var result = _service.FindOpenCells(sack, 0, 2, 10, 10);

		// Assert
		result.X.Should().Be(-1);
		result.Y.Should().Be(-1);
	}

	/// <summary>
	/// Test FindOpenCells with zero height returns (-1, -1)
	/// </summary>
	[Fact]
	public void FindOpenCells_WithZeroHeight_ReturnsInvalidPosition()
	{
		// Arrange
		var sack = new SackCollection();

		// Act
		var result = _service.FindOpenCells(sack, 2, 0, 10, 10);

		// Assert
		result.X.Should().Be(-1);
		result.Y.Should().Be(-1);
	}

	/// <summary>
	/// Test FindOpenCells with negative dimensions returns (-1, -1)
	/// </summary>
	[Fact]
	public void FindOpenCells_WithNegativeDimensions_ReturnsInvalidPosition()
	{
		// Arrange
		var sack = new SackCollection();

		// Act
		var result = _service.FindOpenCells(sack, -1, 2, 10, 10);

		// Assert
		result.X.Should().Be(-1);
		result.Y.Should().Be(-1);
	}

	/// <summary>
	/// Test FindOpenCells finds space in empty sack
	/// </summary>
	[Fact]
	public void FindOpenCells_WithEmptySack_ReturnsFirstPosition()
	{
		// Arrange
		var sack = new SackCollection();

		// Act
		var result = _service.FindOpenCells(sack, 2, 2, 10, 10);

		// Assert
		result.X.Should().Be(0);
		result.Y.Should().Be(0);
	}

	/// <summary>
	/// Test FindOpenCells when no space available returns (-1, -1)
	/// </summary>
	[Fact]
	public void FindOpenCells_WhenNoSpace_ReturnsInvalidPosition()
	{
		// Arrange - Create a sack where no 2x2 space is available
		var sack = new SackCollection();
		var existingItem = CreateItem("Item1", 0, 0, 5, 5);
		sack.AddItem(existingItem);

		// Act
		var result = _service.FindOpenCells(sack, 2, 2, 5, 5);

		// Assert - no 2x2 space available in 5x5 sack filled with 5x5 item
		result.X.Should().Be(-1);
		result.Y.Should().Be(-1);
	}

	/// <summary>
	/// Test CanPlaceItem with null sack returns false
	/// </summary>
	[Fact]
	public void CanPlaceItem_WithNullSack_ReturnsFalse()
	{
		// Arrange
		var item = CreateItem("Item1", 0, 0, 1, 1);

		// Act
		var result = _service.CanPlaceItem(null!, item, 10, 10);

		// Assert
		result.Should().BeFalse();
	}

	/// <summary>
	/// Test CanPlaceItem with null item returns false
	/// </summary>
	[Fact]
	public void CanPlaceItem_WithNullItem_ReturnsFalse()
	{
		// Arrange
		var sack = new SackCollection();

		// Act
		var result = _service.CanPlaceItem(sack, null!, 10, 10);

		// Assert
		result.Should().BeFalse();
	}

	/// <summary>
	/// Test CanPlaceItem with valid empty sack returns true
	/// </summary>
	[Fact]
	public void CanPlaceItem_WithEmptySack_ReturnsTrue()
	{
		// Arrange
		var sack = new SackCollection();
		var item = CreateItem("Item1", 0, 0, 2, 2);

		// Act
		var result = _service.CanPlaceItem(sack, item, 10, 10);

		// Assert
		result.Should().BeTrue();
	}

	/// <summary>
	/// Test IsItemValidForStashType with null item returns false
	/// </summary>
	[Fact]
	public void IsItemValidForStashType_WithNullItem_ReturnsFalse()
	{
		// Act
		var result = _service.IsItemValidForStashType(null!, StashType.PlayerStash);

		// Assert
		result.Should().BeFalse();
	}

	/// <summary>
	/// Test IsItemValidForStashType with null stash type returns true
	/// </summary>
	[Fact]
	public void IsItemValidForStashType_WithNullStashType_ReturnsTrue()
	{
		// Arrange
		var item = CreateItem("Item1", 0, 0, 1, 1);

		// Act
		var result = _service.IsItemValidForStashType(item, null);

		// Assert
		result.Should().BeTrue();
	}

	/// <summary>
	/// Test IsItemValidForStashType for RelicVaultStash rejects normal items
	/// </summary>
	[Fact]
	public void IsItemValidForStashType_RelicVaultWithNormalItem_ReturnsFalse()
	{
		// Arrange - Create a regular item (not artifact/relic/charm/formulae)
		var item = CreateItem("WEAPON", 0, 0, 1, 1);

		// Act
		var result = _service.IsItemValidForStashType(item, StashType.RelicVaultStash);

		// Assert - normal items should not be valid for relic vault
		result.Should().BeFalse();
	}

	/// <summary>
	/// Test IsItemValidForStashType for PlayerStash accepts any item
	/// </summary>
	[Fact]
	public void IsItemValidForStashType_PlayerStash_ReturnsTrue()
	{
		// Arrange
		var item = CreateItem("Item1", 0, 0, 1, 1);

		// Act
		var result = _service.IsItemValidForStashType(item, StashType.PlayerStash);

		// Assert
		result.Should().BeTrue();
	}

	/// <summary>
	/// Test MoveItem with null item returns failed result
	/// </summary>
	[Fact]
	public void MoveItem_WithNullItem_ReturnsFailedResult()
	{
		// Arrange
		var destSack = new SackCollection();

		// Act
		var result = _service.MoveItem(null!, null!, destSack, new Point(0, 0));

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorMessage.Should().Be("Item is null");
	}

	/// <summary>
	/// Test MoveItem with null destination returns failed result
	/// </summary>
	[Fact]
	public void MoveItem_WithNullDestination_ReturnsFailedResult()
	{
		// Arrange
		var item = CreateItem("Item1", 0, 0, 1, 1);

		// Act
		var result = _service.MoveItem(item, null!, null!, new Point(0, 0));

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorMessage.Should().Be("Destination sack is null");
	}

	/// <summary>
	/// Test MoveItem with invalid position returns failed result
	/// </summary>
	[Fact]
	public void MoveItem_WithInvalidPosition_ReturnsFailedResult()
	{
		// Arrange
		var item = CreateItem("Item1", 0, 0, 1, 1);
		var destSack = new SackCollection();

		// Act
		var result = _service.MoveItem(item, null!, destSack, new Point(-1, 0));

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorMessage.Should().Be("Invalid destination position");
	}

	/// <summary>
	/// Test MoveItem successfully moves item
	/// </summary>
	[Fact]
	public void MoveItem_WithValidInput_MovesItem()
	{
		// Arrange
		var sourceSack = new SackCollection();
		var destSack = new SackCollection();
		var item = CreateItem("Item1", 0, 0, 1, 1);
		sourceSack.AddItem(item);

		// Act
		var result = _service.MoveItem(item, sourceSack, destSack, new Point(5, 5));

		// Assert
		result.Success.Should().BeTrue();
		item.PositionX.Should().Be(5);
		item.PositionY.Should().Be(5);
		destSack.Count.Should().Be(1);
	}

	[Fact]
	public void FindItem_CellCoveredByItem_ReturnsIt()
	{
		var sack = new SackCollection();
		var item = CreateItem("Item1", 2, 1, 2, 3);
		sack.AddItem(item);

		_service.FindItem(sack, new Point(3, 3)).Should().BeSameAs(item);
		_service.FindItem(sack, new Point(4, 3)).Should().BeNull();
		_service.FindItem(sack, new Point(3, 4)).Should().BeNull();
	}

	[Fact]
	public void FindItem_IgnoredItem_IsSkipped()
	{
		var sack = new SackCollection();
		var dragged = CreateItem("Dragged", 0, 0, 1, 1);
		sack.AddItem(dragged);

		_service.FindItem(sack, new Point(0, 0), dragged).Should().BeNull();
		_service.FindItem(sack, new Point(0, 0)).Should().BeSameAs(dragged);
	}

	[Fact]
	public void FindItem_IgnoredItemOverlapsAnother_ReturnsTheOther()
	{
		var sack = new SackCollection();
		var dragged = CreateItem("Dragged", 0, 0, 2, 2);
		var other = CreateItem("Other", 1, 1, 1, 1);
		sack.AddItem(dragged);
		sack.AddItem(other);

		_service.FindItem(sack, new Point(1, 1), dragged).Should().BeSameAs(other);
	}

	[Fact]
	public void FindItem_ItemWithoutBaseId_IsSkipped()
	{
		var sack = new SackCollection();
		var empty = CreateItem("Empty", 0, 0, 1, 1);
		empty.BaseItemId = RecordId.Empty;
		sack.AddItem(empty);

		_service.FindItem(sack, new Point(0, 0)).Should().BeNull();
	}

	[Fact]
	public void PlaceIn_Equipment_UsesTheEquipmentBag()
	{
		var player = new PlayerCollection("HeroB", "/saves/_HeroB/Player.chr");

		var place = _service.PlaceIn(player, SackType.Equipment, 5);

		place.SackNumber.Should().Be(BagIdConstants.BAGID_EQUIPMENTPANEL);
		place.SackType.Should().Be(SackType.Equipment);
	}

	[Theory]
	[InlineData(SackType.Vault, 3)]
	[InlineData(SackType.Player, 2)]
	public void PlaceIn_VaultOrPlayerSack_UsesTheSackNumber(SackType sackType, int sackNumber)
	{
		var container = new PlayerCollection("Main", "/vaults/Main.vault.json");

		var place = _service.PlaceIn(container, sackType, sackNumber);

		place.SackNumber.Should().Be(sackNumber);
		place.SackType.Should().Be(sackType);
		place.StashType.Should().BeNull();
	}

	[Fact]
	public void PlaceIn_StashWithStashType_UsesTheStashTypeAsSackNumber()
	{
		var player = new PlayerCollection("HeroB", "/saves/_HeroB/Player.chr");

		var place = _service.PlaceIn(player, SackType.Stash, 7, StashType.RelicVaultStash);

		place.SackNumber.Should().Be((int)StashType.RelicVaultStash);
		place.StashType.Should().Be(StashType.RelicVaultStash);
	}

	[Fact]
	public void PlaceIn_StashWithoutStashType_UsesTheSackNumber()
	{
		var player = new PlayerCollection("HeroB", "/saves/_HeroB/Player.chr");

		_service.PlaceIn(player, SackType.Stash, 3).SackNumber.Should().Be(3);
	}

	[Fact]
	public void PlaceIn_NameIsThePlayerName_AsTheSearchDatabaseHasIt()
	{
		// A vault file in a folder starting with '_' would pass for a character folder when named from the path.
		var vault = new PlayerCollection("Main", """/_vaults/Main.vault.json""") { IsVault = true };

		var place = _service.PlaceIn(vault, SackType.Vault, 0);

		place.Name.Should().Be("Main");
		place.Path.Should().Be("""/_vaults/Main.vault.json""");
	}

	[Fact]
	public void TryPlace_FreeCells_PlacesTheItemAtTheFirstColumnGap()
	{
		var sack = new SackCollection();
		sack.AddItem(CreateItem("Blocker", 0, 0, 1, 3));
		var item = CreateItem("Moved", 7, 7, 1, 2);
		var place = new ContainerPlace { Path = "/vaults/Main.vault.json", Name = "Main", SackNumber = 4, SackType = SackType.Vault };

		bool placed = _service.TryPlace(item, sack, new Size(3, 3), place);

		placed.Should().BeTrue();
		item.Location.Should().Be(new Point(1, 0));
		sack.Should().Contain(item);
		item.Place.Should().Be(place);
		item.Place.Should().NotBeSameAs(place, "the item keeps its place instance, shared with its search result");
	}

	[Fact]
	public void TryPlace_NoRoom_LeavesEverythingAsItWas()
	{
		var sack = new SackCollection();
		sack.AddItem(CreateItem("Blocker", 0, 0, 2, 1));
		sack.IsModified = false;
		var item = CreateItem("Moved", 7, 7, 1, 2);
		item.Place.Name = "Source";
		bool called = false;

		bool placed = _service.TryPlace(item, sack, new Size(2, 2), new ContainerPlace { Name = "Dest" }, () => called = true);

		placed.Should().BeFalse();
		called.Should().BeFalse("the drag must stay active so it can be cancelled");
		item.Location.Should().Be(new Point(7, 7));
		item.Place.Name.Should().Be("Source");
		sack.Should().NotContain(item);
		sack.IsModified.Should().BeFalse();
	}

	[Fact]
	public void TryPlace_RelicVaultStash_RefusesOtherItems()
	{
		var sack = new SackCollection { StashType = StashType.RelicVaultStash };
		var sword = CreateItem("records/item/equipmentweapon/sword/sword01.dbr", 0, 0, 1, 1);

		_service.TryPlace(sword, sack, new Size(5, 5), new ContainerPlace()).Should().BeFalse();
		sack.Should().BeEmpty();
	}

	[Fact]
	public void TryPlace_RelicVaultStash_TakesRelics()
	{
		var sack = new SackCollection { StashType = StashType.RelicVaultStash };
		var relic = CreateItem("records/xpack/items/relics/relic01.dbr", 0, 0, 1, 1);

		_service.TryPlace(relic, sack, new Size(5, 5), new ContainerPlace()).Should().BeTrue();
		sack.Should().Contain(relic);
	}

	[Fact]
	public void TryPlace_BeforePlacing_SeesTheOldPosition()
	{
		// An equipped item is taken out of its slot by its position: the callback must run before the item moves.
		var sack = new SackCollection();
		var item = CreateItem("Moved", 4, 2, 1, 1);
		Point? seen = null;
		bool inSack = true;

		_service.TryPlace(item, sack, new Size(3, 3), new ContainerPlace(), () =>
		{
			seen = item.Location;
			inSack = sack.Contains(item);
		}).Should().BeTrue();

		seen.Should().Be(new Point(4, 2));
		inSack.Should().BeFalse();
		item.Location.Should().Be(new Point(0, 0));
	}

	[Fact]
	public void TryPlace_ItemStillInTheSack_DoesNotBlockItself()
	{
		// A 2x2 sack with the item in it: other free cells exist only if the item does not count.
		var sack = new SackCollection();
		var item = CreateItem("Moved", 0, 0, 2, 2);
		sack.AddItem(item);

		_service.TryPlace(item, sack, new Size(2, 2), new ContainerPlace(), () => sack.RemoveItem(item)).Should().BeTrue();

		sack.Count.Should().Be(1);
		item.Location.Should().Be(new Point(0, 0));
	}

	[Fact]
	public void TryPlace_StackableItem_IsPlacedAsANewEntryWithoutMerging()
	{
		// The auto-move never stacks: a potion goes to free cells even next to the same potion.
		var sack = new SackCollection();
		var potion = CreateItem("records/item/potions/potion01.dbr", 0, 0, 1, 1);
		potion.StackSize = 3;
		sack.AddItem(potion);
		var moved = CreateItem("records/item/potions/potion01.dbr", 9, 9, 1, 1);
		moved.StackSize = 2;

		_service.TryPlace(moved, sack, new Size(2, 1), new ContainerPlace()).Should().BeTrue();

		sack.Count.Should().Be(2);
		potion.StackSize.Should().Be(3);
		moved.Location.Should().Be(new Point(1, 0));
	}

	/// <summary>
	/// Helper to create a test item
	/// </summary>
	private static Item CreateItem(string baseId, int x, int y, int width, int height)
	{
		var item = new Item
		{
			PositionX = x,
			PositionY = y,
			Width = width,
			Height = height,
			BaseItemId = RecordId.Create(baseId)
		};
		return item;
	}
}
