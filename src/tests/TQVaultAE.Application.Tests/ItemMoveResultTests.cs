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
	public void Failed_SetsSuccessToFalse()
	{
		// Act
		var result = ItemMoveResult.Failed("Test error");

		// Assert
		result.Success.Should().BeFalse();
	}

	[Fact]
	public void Failed_SetsNewPositionToNegativeOne()
	{
		// Act
		var result = ItemMoveResult.Failed("Test error");

		// Assert
		result.NewPosition.Should().Be(new Point(-1, -1));
	}
}
