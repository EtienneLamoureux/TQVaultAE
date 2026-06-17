using AwesomeAssertions;
using System.Drawing;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Application;

public class ItemMoveResultTests
{
	[Fact]
	public void Succeeded_SetsSuccessToTrue()
	{
		// Arrange
		var item = new Item();
		var sack = new SackCollection();
		var position = new Point(5, 3);

		// Act
		var result = ItemMoveResult.Succeeded(item, sack, position);

		// Assert
		result.Success.Should().BeTrue();
	}

	[Fact]
	public void Succeeded_SetsCorrectItem()
	{
		// Arrange
		var item = new Item();
		var sack = new SackCollection();
		var position = new Point(5, 3);

		// Act
		var result = ItemMoveResult.Succeeded(item, sack, position);

		// Assert
		result.Item.Should().BeSameAs(item);
	}

	[Fact]
	public void Succeeded_SetsCorrectDestinationSack()
	{
		// Arrange
		var item = new Item();
		var sack = new SackCollection();
		var position = new Point(5, 3);

		// Act
		var result = ItemMoveResult.Succeeded(item, sack, position);

		// Assert
		result.DestinationSack.Should().BeSameAs(sack);
	}

	[Fact]
	public void Succeeded_SetsCorrectNewPosition()
	{
		// Arrange
		var item = new Item();
		var sack = new SackCollection();
		var position = new Point(5, 3);

		// Act
		var result = ItemMoveResult.Succeeded(item, sack, position);

		// Assert
		result.NewPosition.Should().Be(position);
	}

	[Fact]
	public void Succeeded_SetsErrorMessageToNull()
	{
		// Arrange
		var item = new Item();
		var sack = new SackCollection();
		var position = new Point(5, 3);

		// Act
		var result = ItemMoveResult.Succeeded(item, sack, position);

		// Assert
		result.ErrorMessage.Should().BeNull();
	}

	[Fact]
	public void Failed_SetsSuccessToFalse()
	{
		// Act
		var result = ItemMoveResult.Failed("Test error");

		// Assert
		result.Success.Should().BeFalse();
	}

	[Fact]
	public void Failed_SetsCorrectErrorMessage()
	{
		// Arrange
		var errorMessage = "Test error message";

		// Act
		var result = ItemMoveResult.Failed(errorMessage);

		// Assert
		result.ErrorMessage.Should().Be(errorMessage);
	}

	[Fact]
	public void Failed_SetsNewPositionToNegativeOne()
	{
		// Act
		var result = ItemMoveResult.Failed("Test error");

		// Assert
		result.NewPosition.Should().Be(new Point(-1, -1));
	}

	[Fact]
	public void Failed_WithNullItem_SetsItemToNull()
	{
		// Act
		var result = ItemMoveResult.Failed("Test error", null);

		// Assert
		result.Item.Should().BeNull();
	}

	[Fact]
	public void Failed_WithItem_SetsCorrectItem()
	{
		// Arrange
		var item = new Item();

		// Act
		var result = ItemMoveResult.Failed("Test error", item);

		// Assert
		result.Item.Should().BeSameAs(item);
	}

	[Fact]
	public void Failed_WithNullDestinationSack_SetsToNull()
	{
		// Act
		var result = ItemMoveResult.Failed("Test error", null, null);

		// Assert
		result.DestinationSack.Should().BeNull();
	}

	[Fact]
	public void Failed_WithDestinationSack_SetsCorrectSack()
	{
		// Arrange
		var sack = new SackCollection();

		// Act
		var result = ItemMoveResult.Failed("Test error", null, sack);

		// Assert
		result.DestinationSack.Should().BeSameAs(sack);
	}

	[Fact]
	public void Failed_WithAllParameters_SetsAllCorrectly()
	{
		// Arrange
		var item = new Item();
		var sack = new SackCollection();
		var errorMessage = "Full test error";

		// Act
		var result = ItemMoveResult.Failed(errorMessage, item, sack);

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorMessage.Should().Be(errorMessage);
		result.Item.Should().BeSameAs(item);
		result.DestinationSack.Should().BeSameAs(sack);
		result.NewPosition.Should().Be(new Point(-1, -1));
	}
}
