using System.Drawing;
using AwesomeAssertions;
using TQVaultAE.Application.Character;

namespace TQVaultAE.Tests.Application;

/// <summary>
/// The character's inventory sizes read from gameengine (<see cref="GameBalance.InventoryMainCells"/>,
/// <see cref="GameBalance.InventoryBagCells"/>): the game's, an inventory mod's, the game's when the fields are missing.
/// </summary>
public sealed class GameBalanceInventoryTests
{
	private static GameBalance Engine(int mainX, int mainY, int bagX, int bagY) => new()
	{
		Engine = new Dictionary<string, IReadOnlyList<double>>(StringComparer.OrdinalIgnoreCase)
		{
			["UICharWindowInventorySack0DimsX"] = [mainX],
			["UICharWindowInventorySack0DimsY"] = [mainY],
			["UICharWindowInventorySack1DimsX"] = [bagX],
			["UICharWindowInventorySack1DimsY"] = [bagY],
		},
	};

	[Theory]
	[InlineData(384, 160, 256, 160, 12, 5, 8, 5)] // the game
	[InlineData(0, 0, 640, 288, 0, 0, 20, 9)] // More Space XL: the main inventory moved into 20 x 9 bags
	[InlineData(0, 0, 384, 640, 0, 0, 12, 20)] // More Space XXL
	public void InventoryCells_AreTheDimensionsIn32PixelCells(int mainX, int mainY, int bagX, int bagY, int mainW, int mainH, int bagW, int bagH)
	{
		var balance = Engine(mainX, mainY, bagX, bagY);

		balance.InventoryMainCells.Should().Be(new Size(mainW, mainH));
		balance.InventoryBagCells.Should().Be(new Size(bagW, bagH));
	}

	[Fact]
	public void InventoryCells_WithoutTheFields_AreTheGames()
	{
		var balance = new GameBalance();

		balance.InventoryMainCells.Should().Be(new Size(12, 5));
		balance.InventoryBagCells.Should().Be(new Size(8, 5));
	}
}
