using AwesomeAssertions;
using Moq;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Comparison;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Results;
using TQVaultAE.Presentation;

namespace TQVaultAE.Tests.Presentation;

/// <summary>
/// <see cref="ItemComparisonContentBuilder"/>: the four columns of the item comparison.
/// </summary>
public class ItemComparisonContentBuilderTests
{
	private static StatDifferenceGroup Group(StatGroupKind kind, string? title, params StatDifference[] lines)
		=> new(kind, title, kind == StatGroupKind.Requirements, lines);

	private static StatDifference Diff(string text, StatTrend trend) => new(text, trend, StatPresence.Both);

	[Theory]
	[InlineData(StatTrend.Higher, false, TQColor.Green)]
	[InlineData(StatTrend.Lower, false, TQColor.Red)]
	[InlineData(StatTrend.Higher, true, TQColor.Red)]
	[InlineData(StatTrend.Lower, true, TQColor.Green)]
	[InlineData(StatTrend.Mixed, false, TQColor.Yellow)]
	[InlineData(StatTrend.Mixed, true, TQColor.Yellow)]
	public void ColorOf_BetterIsGreen_LowerRequirementsAreBetter(StatTrend trend, bool lowerIsBetter, TQColor expected)
		=> ItemComparisonContentBuilder.ColorOf(trend, lowerIsBetter).Should().Be(expected);

	[Fact]
	public void DifferenceLines_GroupsSeparated_SkillUnderItsName()
	{
		var lines = ItemComparisonContentBuilder.DifferenceLines(
		[
			Group(StatGroupKind.Attributes, null, Diff("+12 Strength", StatTrend.Higher)),
			Group(StatGroupKind.GrantedSkill, "Blood Boil", Diff("Level: -1", StatTrend.Lower)),
			Group(StatGroupKind.Requirements, null, Diff("Required Strength: +50", StatTrend.Higher)),
		]);

		lines.Select(l => (l.Kind, l.Text, l.Color, l.Bold)).Should().Equal(
			(TooltipLineKind.Text, "+12 Strength", TQColor.Green, false),
			(TooltipLineKind.Delimiter, "", null, false),
			(TooltipLineKind.Text, "Blood Boil", ItemStyle.Epic.TQColor(), true),
			(TooltipLineKind.Text, "Level: -1", TQColor.Red, false),
			(TooltipLineKind.Delimiter, "", null, false),
			(TooltipLineKind.Text, "Required Strength: +50", TQColor.Red, false));
	}

	[Fact]
	public void DifferenceLines_NothingDiffers_SaysSo()
		=> ItemComparisonContentBuilder.DifferenceLines([]).Select(l => l.Text)
			.Should().Equal(Resources.ItemComparisonNoDifference);

	[Fact]
	public void Build_ItemColumnsAreTheClassicTooltips_DifferencesOnTheOutside()
	{
		var translation = new Mock<ITranslationService>();
		var tooltip = new ItemTooltipContentBuilder(translation.Object);
		var first = new ToFriendlyNameResult(new Item()) { ClassicAttributes = ["+42 Strength"], ItemSeed = "Seed 1" };
		var second = new ToFriendlyNameResult(new Item()) { ClassicAttributes = ["+30 Strength"], ItemSeed = "Seed 2" };
		var stats = ItemStatComparer.Compare(ItemStatSheet.From(first), ItemStatSheet.From(second));

		var content = new ItemComparisonContentBuilder(translation.Object).Build(new ItemComparison(first, second, stats));

		content.First.Should().BeEquivalentTo(tooltip.Build(first, ItemTooltipView.Classic).Lines);
		content.Second.Should().BeEquivalentTo(tooltip.Build(second, ItemTooltipView.Classic).Lines);
		content.FirstDifferences.Select(l => l.Text).Should().Equal("+12 Strength");
		content.SecondDifferences.Select(l => l.Text).Should().Equal("-12 Strength");
	}
}
