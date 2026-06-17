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
		var result = _service.FindOpenCells(null, 2, 2, 10, 10);

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
		var result = _service.CanPlaceItem(null, item, 10, 10);

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
		var result = _service.CanPlaceItem(sack, null, 10, 10);

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
		var result = _service.IsItemValidForStashType(null, StashType.PlayerStash);

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
		var result = _service.MoveItem(null, null, destSack, new Point(0, 0));

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
		var result = _service.MoveItem(item, null, null, new Point(0, 0));

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
		var result = _service.MoveItem(item, null, destSack, new Point(-1, 0));

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

	/// <summary>
	/// Test UpdateItemLocation with null item does nothing
	/// </summary>
	[Fact]
	public void UpdateItemLocation_WithNullItem_DoesNothing()
	{
		// Arrange
		var player = new PlayerCollection("TestPlayer", "/path/to/player");

		// Act & Assert - should not throw
		_service.UpdateItemLocation(null, player, SackType.Player, 0);
	}

	/// <summary>
	/// Test UpdateItemLocation with null player does nothing
	/// </summary>
	[Fact]
	public void UpdateItemLocation_WithNullPlayer_DoesNothing()
	{
		// Arrange
		var item = CreateItem("Item1", 0, 0, 1, 1);

		// Act & Assert - should not throw
		_service.UpdateItemLocation(item, null, SackType.Player, 0);
	}

	/// <summary>
	/// Test UpdateItemLocation updates equipment sack type correctly
	/// </summary>
	[Fact]
	public void UpdateItemLocation_EquipmentSack_UpdatesSackNumber()
	{
		// Arrange
		var item = CreateItem("Item1", 0, 0, 1, 1);
		var player = new PlayerCollection("TestPlayer", "/path/to/player");

		// Act
		_service.UpdateItemLocation(item, player, SackType.Equipment, 5);

		// Assert - Equipment panel always uses BAGID_EQUIPMENTPANEL (0)
		item.Place.SackNumber.Should().Be(0);
		item.Place.SackType.Should().Be(SackType.Equipment);
	}

	/// <summary>
	/// Test UpdateItemLocation updates vault sack type correctly
	/// </summary>
	[Fact]
	public void UpdateItemLocation_VaultSack_UpdatesCorrectly()
	{
		// Arrange
		var item = CreateItem("Item1", 0, 0, 1, 1);
		var player = new PlayerCollection("Vault1", "/path/to/vault") { IsVault = true };

		// Act
		_service.UpdateItemLocation(item, player, SackType.Vault, 3);

		// Assert
		item.Place.SackNumber.Should().Be(3);
		item.Place.SackType.Should().Be(SackType.Vault);
	}

	/// <summary>
	/// Test UpdateItemLocation with custom container name
	/// </summary>
	[Fact]
	public void UpdateItemLocation_WithContainerName_UsesCustomName()
	{
		// Arrange
		var item = CreateItem("Item1", 0, 0, 1, 1);
		var player = new PlayerCollection("TestPlayer", "/path/to/player");

		// Act
		_service.UpdateItemLocation(item, player, SackType.Player, 0, null, "Custom Name");

		// Assert
		item.Place.Name.Should().Be("Custom Name");
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
