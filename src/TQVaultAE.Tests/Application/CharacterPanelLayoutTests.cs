using System.Globalization;
using AwesomeAssertions;
using TQVaultAE.Application;

namespace TQVaultAE.Tests.Application;

public class CharacterPanelLayoutTests
{
	[Theory]
	[InlineData(50, 0, 0F, 50)]
	[InlineData(50, 10, 0F, 60)]
	[InlineData(300, 20, 10F, 352)]
	public void AttributeTotal_AddsFlatBonusesThenPercentages(int baseValue, int flat, float percent, int expected)
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

	[Fact]
	public void ColorRuns_SplitsAtColourCodes()
		=> CharacterPanelLayout.ColorRuns("{^y}2{^w} Points Available").Should().Equal(
			new GameTextRun("2", 'y'),
			new GameTextRun(" Points Available", 'w'));

	[Fact]
	public void ColorRuns_WithoutCodes_IsOneUncolouredRun()
		=> CharacterPanelLayout.ColorRuns("Health").Should().Equal(new GameTextRun("Health", null));
}
