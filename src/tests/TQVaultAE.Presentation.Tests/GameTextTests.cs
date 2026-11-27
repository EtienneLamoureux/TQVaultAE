using System.Globalization;
using AwesomeAssertions;
using TQVaultAE.Presentation;

namespace TQVaultAE.Tests.Presentation;

public class GameTextTests
{
	[Theory]
	[InlineData(0, "0:0:0")]
	[InlineData(59, "0:0:0")]
	[InlineData(3 * 3600 + 25 * 60 + 10, "0:3:25")]
	[InlineData(2 * 86400 + 5 * 3600 + 7 * 60, "2:5:7")]
	public void PlayTime_IsDaysHoursMinutesAsTheGameSplitsIt(int seconds, string expected)
		=> GameText.PlayTime("{%d0}:{%d1}:{%d2}", seconds).Should().Be(expected);

	[Fact]
	public void Format_FillsByIndex_WithThousandsForT()
	{
		var thousands = CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator;

		GameText.Format("Level : {%d0}", 30).Should().Be("Level : 30");
		GameText.Format("XP : {%t0}", 4499991).Should().Be($"XP : 4{thousands}499{thousands}991");
		GameText.Format("{%d1} of {%d0}", 3, 7).Should().Be("7 of 3");
		GameText.Format("{%d2} missing", 1).Should().Be("{%d2} missing");
	}

	[Theory]
	[InlineData("{%+.0f0}% Damage", 10, "+10% Damage")]
	[InlineData("{+%d0} to all Skills", 4, "+4 to all Skills")]
	[InlineData("{+%d0} to all Skills", -1, "-1 to all Skills")]
	public void Format_SignFlag_BeforeOrAfterThePercent(string format, int value, string expected)
		=> GameText.Format(format, value).Should().Be(expected);

	[Theory]
	[InlineData("{%d0} Unused Skill Points", 3, "3 Unused Skill Points")]
	[InlineData("Points : {%i0}", 12, "Points : 12")]
	[InlineData("No Skill Points Available", 0, "No Skill Points Available")]
	public void Format_SkillPaneTexts(string format, int value, string expected)
		=> GameText.Format(format, value).Should().Be(expected);

	[Fact]
	public void Format_TextValues_AreWrittenAsTheyAre()
		=> GameText.Format("This skill requires the {%s1} skill at {%d0}.", 4, "Battle Rage")
			.Should().Be("This skill requires the Battle Rage skill at 4.");

	[Theory]
	[InlineData(45, "+45% Fire Damage")]
	[InlineData(0, "+0% Fire Damage")]
	[InlineData(-10, "-10% Fire Damage")]
	public void Format_PlusFlag_WritesTheSignOfPositiveValues(long value, string expected)
		=> GameText.Format("{%+.0f0}% Fire Damage", value).Should().Be(expected);
}
