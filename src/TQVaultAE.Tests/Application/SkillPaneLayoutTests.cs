using AwesomeAssertions;
using TQVaultAE.Application;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Application;

public class SkillPaneLayoutTests
{
	private static readonly SkillPaneArt Art = new(
		"""InGameUI\Skills\EarthSkillBackground01.tex""", """InGameUI\Skills\EarthPanel01.tex""", 718, 31,
		"""InGameUI\Skills\EarthSkillBar01.tex""", 47, 26, 158, 51, 0, "tagMasteryDescription03", 29, 459);

	[Theory]
	[InlineData(0, 0.0)]
	[InlineData(1, 1.0)]
	[InlineData(4, 2.0)]
	[InlineData(7, 2.5)]
	[InlineData(10, 3.0)]
	[InlineData(28, 5.5)]
	[InlineData(32, 6.0)]
	[InlineData(40, 7.0)]
	public void BarFraction_FillsOneEqualStepPerTier(int masteryLevel, double steps)
		=> SkillPaneLayout.BarFraction(masteryLevel).Should().BeApproximately(steps / 7, 1e-9);

	[Fact]
	public void BarFraction_AboveTheLastMilestone_StaysFull()
		=> SkillPaneLayout.BarFraction(45).Should().Be(1);

	[Theory]
	[InlineData(1, 62)]
	[InlineData(24, 310)]
	[InlineData(40, 434)]
	public void PeekThrough_EndsAtTheBottomOfTheBar_AsHighAsTheFill(int masteryLevel, int height)
	{
		var rect = SkillPaneLayout.PeekThrough(Art, masteryLevel);

		rect.Height.Should().Be(height);
		rect.Bottom.Should().Be(26 + 434 - 1, "the game ends it one pixel above the bottom of the bar");
		rect.Width.Should().Be(SkillPaneLayout.Width);
	}

	[Fact]
	public void PeekThrough_AtTier1_ReachesTheFirstTierLine()
	{
		// The first tier line of the background is at y 397: level 1 must light it, not the one above (335).
		var rect = SkillPaneLayout.PeekThrough(Art, 1);

		rect.Y.Should().BeLessThanOrEqualTo(397).And.BeGreaterThan(335);
	}

	[Fact]
	public void Connectors_StackUpTheColumnLeftOfTheButton_OnOrOffByPoints()
	{
		var button = new SkillPaneButton(228, 403, false, string.Empty,
			["BottomOn", "MiddleOn", "TopOn"], ["BottomOff", "MiddleOff", "TopOff"]);

		SkillPaneLayout.Connectors(button, hasPoints: true).Should().Equal(
			new PanePiece("BottomOn", 208, 393),
			new PanePiece("MiddleOn", 208, 331),
			new PanePiece("TopOn", 208, 269));
		SkillPaneLayout.Connectors(button, hasPoints: false).Select(p => p.Texture).Should().Equal("BottomOff", "MiddleOff", "TopOff");
	}

	[Theory]
	[InlineData(12, 12, 1, 1, false, 0, SkillButtonState.Maxed)]
	[InlineData(0, 12, 10, 16, true, 5, SkillButtonState.TierLocked)]
	[InlineData(0, 8, 20, 16, false, 5, SkillButtonState.ParentLocked)]
	[InlineData(3, 8, 20, 16, true, 0, SkillButtonState.NoPointLeft)]
	[InlineData(3, 8, 20, 16, true, 2, SkillButtonState.CanRaise)]
	[InlineData(0, 8, 9, 16, false, 2, SkillButtonState.TierLocked)]
	public void State_FollowsTheGameOrder(int level, int max, int masteryLevel, int required, bool parentHasPoints, int points, SkillButtonState expected)
		=> SkillPaneLayout.State(level, max, masteryLevel, required, parentHasPoints, points).Should().Be(expected);

	[Theory]
	[InlineData(SkillButtonState.CanRaise, false, """InGameUI\SkillButtonBorderGreen01.tex""")]
	[InlineData(SkillButtonState.TierLocked, true, """InGameUI\SkillButtonBorderRoundGrayOut01.tex""")]
	[InlineData(SkillButtonState.ParentLocked, true, """InGameUI\SkillButtonBorderRoundRed01.tex""")]
	public void StateBorder_NamesTheGameTexture(SkillButtonState state, bool round, string texture)
		=> SkillPaneLayout.StateBorder(state, round).Should().Be(texture);

	[Fact]
	public void LevelText_AddsTheItemLevels_AndIsHiddenWhileTheTierIsClosed()
	{
		SkillPaneLayout.LevelText(5, 2, 12, tierOpen: true).Should().Be("7 / 12");
		SkillPaneLayout.LevelText(0, 0, 8, tierOpen: true).Should().Be("0 / 8");
		SkillPaneLayout.LevelText(0, 2, 8, tierOpen: false).Should().BeNull();
	}

	[Theory]
	[InlineData("{%d0} Unused Skill Points", 3, "3 Unused Skill Points")]
	[InlineData("Points : {%i0}", 12, "Points : 12")]
	[InlineData("No Skill Points Available", 0, "No Skill Points Available")]
	public void FormatGameText_ReplacesTheFirstValue(string format, int value, string expected)
		=> SkillPaneLayout.FormatGameText(format, value).Should().Be(expected);
}
