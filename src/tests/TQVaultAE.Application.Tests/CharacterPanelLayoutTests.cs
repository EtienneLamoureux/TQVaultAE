using AwesomeAssertions;
using TQVaultAE.Application.Layout;

namespace TQVaultAE.Tests.Application;

public class CharacterPanelLayoutTests
{
	[Theory]
	[InlineData(50, 0, 0, 50)]
	[InlineData(50, 10, 0, 60)]
	[InlineData(300, 20, 10, 352)]
	[InlineData(58, 13.5, 0, 71)] // A character's strength in the game: truncated, not rounded
	[InlineData(50, 5.75, 0, 55)]
	[InlineData(10, -20, 0, 0)]
	[InlineData(100, 0, 15, 115)] // 100 × 1.15 is 114.999... in floating point: the formula keeps it whole
	public void AttributeTotal_AddsFlatBonusesThenPercentages_Truncated(int baseValue, double flat, double percent, int expected)
		=> CharacterPanelLayout.AttributeTotal(baseValue, flat, percent).Should().Be(expected);

	[Theory]
	[InlineData(1, -1, 1)]   // the difficulty unlocked
	[InlineData(2, 0, 0)]    // the one picked in the editor
	[InlineData(3, -1, 2)]   // a save beyond Legendary
	[InlineData(-1, -1, 0)]
	public void ShownDifficulty_PickedElseUnlocked_WithinTheGameDifficulties(int unlocked, int picked, int expected)
		=> CharacterPanelLayout.ShownDifficulty(unlocked, picked).Should().Be(expected);
}
