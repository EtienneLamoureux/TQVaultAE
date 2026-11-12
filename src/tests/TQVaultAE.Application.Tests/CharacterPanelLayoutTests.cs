using System.Globalization;
using AwesomeAssertions;
using TQVaultAE.Application;

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
	public void AttributeTotal_AddsFlatBonusesThenPercentages_Truncated(int baseValue, double flat, double percent, int expected)
		=> CharacterPanelLayout.AttributeTotal(baseValue, flat, percent).Should().Be(expected);

	[Theory]
	[InlineData(0, "0:0:0")]
	[InlineData(59, "0:0:0")]
	[InlineData(3 * 3600 + 25 * 60 + 10, "0:3:25")]
	[InlineData(2 * 86400 + 5 * 3600 + 7 * 60, "2:5:7")]
	public void PlayTime_IsDaysHoursMinutesAsTheGameSplitsIt(int seconds, string expected)
		=> CharacterPanelLayout.PlayTime("{%d0}:{%d1}:{%d2}", seconds).Should().Be(expected);

	[Fact]
	public void FormatGameText_FillsByIndex_WithThousandsForT()
	{
		var thousands = CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator;

		CharacterPanelLayout.FormatGameText("Level : {%d0}", 30).Should().Be("Level : 30");
		CharacterPanelLayout.FormatGameText("XP : {%t0}", 4499991).Should().Be($"XP : 4{thousands}499{thousands}991");
		CharacterPanelLayout.FormatGameText("{%d1} of {%d0}", 3, 7).Should().Be("7 of 3");
		CharacterPanelLayout.FormatGameText("{%d2} missing", 1).Should().Be("{%d2} missing");
	}

	[Theory]
	[InlineData("{%d0} Unused Skill Points", 3, "3 Unused Skill Points")]
	[InlineData("Points : {%i0}", 12, "Points : 12")]
	[InlineData("No Skill Points Available", 0, "No Skill Points Available")]
	public void FormatGameText_SkillPaneTexts(string format, int value, string expected)
		=> CharacterPanelLayout.FormatGameText(format, value).Should().Be(expected);

	[Fact]
	public void FormatGameText_TextValues_AreWrittenAsTheyAre()
		=> CharacterPanelLayout.FormatGameText("This skill requires the {%s1} skill at {%d0}.", 4, "Battle Rage")
			.Should().Be("This skill requires the Battle Rage skill at 4.");

	[Theory]
	[InlineData(1, -1, 1)]   // the difficulty unlocked
	[InlineData(2, 0, 0)]    // the one picked in the editor
	[InlineData(3, -1, 2)]   // a save beyond Legendary
	[InlineData(-1, -1, 0)]
	public void ShownDifficulty_PickedElseUnlocked_WithinTheGameDifficulties(int unlocked, int picked, int expected)
		=> CharacterPanelLayout.ShownDifficulty(unlocked, picked).Should().Be(expected);

	[Theory]
	[InlineData(45, "+45% Fire Damage")]
	[InlineData(0, "+0% Fire Damage")]
	[InlineData(-10, "-10% Fire Damage")]
	public void FormatGameText_PlusFlag_WritesTheSignOfPositiveValues(long value, string expected)
		=> CharacterPanelLayout.FormatGameText("{%+.0f0}% Fire Damage", value).Should().Be(expected);

	[Fact]
	public void ColorRuns_SplitsAtColourCodes()
		=> CharacterPanelLayout.ColorRuns("{^y}2{^w} Points Available").Should().Equal(
			new GameTextRun("2", 'y'),
			new GameTextRun(" Points Available", 'w'));

	[Fact]
	public void ColorRuns_WithoutCodes_IsOneUncolouredRun()
		=> CharacterPanelLayout.ColorRuns("Health").Should().Equal(new GameTextRun("Health", null));
}
