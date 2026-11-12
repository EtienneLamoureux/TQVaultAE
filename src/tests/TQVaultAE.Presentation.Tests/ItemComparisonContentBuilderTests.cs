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

	private static StatDifference Diff(string text, StatTrend trend, string key = "", StatPresence presence = StatPresence.Both, bool lowerIsBetter = false)
		=> new(text, trend, presence, key, lowerIsBetter);

	private static string KeyOf(string line) => StatLine.Parse(line)!.Key;

	[Theory]
	[InlineData(StatTrend.Higher, false, TQColor.Green)]
	[InlineData(StatTrend.Lower, false, TQColor.Red)]
	[InlineData(StatTrend.Higher, true, TQColor.Red)]
	[InlineData(StatTrend.Lower, true, TQColor.Green)]
	[InlineData(StatTrend.Mixed, false, TQColor.Yellow)]
	[InlineData(StatTrend.Mixed, true, TQColor.Yellow)]
	[InlineData(StatTrend.Unchanged, false, TQColor.DarkGray)]
	[InlineData(StatTrend.Unchanged, true, TQColor.DarkGray)]
	public void ColorOf_BetterIsGreen_LowerIsBetterReversed_UnchangedIsGray(StatTrend trend, bool lowerIsBetter, TQColor expected)
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
	public void DifferenceLines_LowerIsBetterStat_ColoredTheOtherWay()
		=> ItemComparisonContentBuilder.DifferenceLines([Group(StatGroupKind.Attributes, null, Diff("-5% Recharge", StatTrend.Lower, lowerIsBetter: true))])
			.Single().Color.Should().Be(TQColor.Green);

	[Fact]
	public void DifferenceLines_NothingDiffers_SaysSo()
		=> ItemComparisonContentBuilder.DifferenceLines([]).Select(l => l.Text)
			.Should().Equal(Resources.ItemComparisonNoDifference);

	[Fact]
	public void DifferenceLines_UnchangedStats_GrayOrHidden()
	{
		StatDifferenceGroup[] groups =
		[
			Group(StatGroupKind.Attributes, null, Diff("+12 Strength", StatTrend.Higher), Diff("42 Health", StatTrend.Unchanged)),
			Group(StatGroupKind.Requirements, null, Diff("Required Level: 40", StatTrend.Unchanged)),
		];

		ItemComparisonContentBuilder.DifferenceLines(groups).Select(l => (l.Text, l.Color)).Should().Equal(
			("+12 Strength", TQColor.Green),
			("42 Health", TQColor.DarkGray),
			("", null),
			("Required Level: 40", TQColor.DarkGray));

		// Hidden: the group left empty goes with its delimiter.
		ItemComparisonContentBuilder.DifferenceLines(groups, hideUnchanged: true).Select(l => l.Text).Should().Equal("+12 Strength");
	}

	[Fact]
	public void DifferenceLines_OnlyUnchangedStats_SaysNoDifferenceFirst()
		=> ItemComparisonContentBuilder.DifferenceLines([Group(StatGroupKind.Attributes, null, Diff("42 Health", StatTrend.Unchanged))])
			.Select(l => l.Text).Should().Equal(Resources.ItemComparisonNoDifference, "", "42 Health");

	[Fact]
	public void AlignedDifferenceLines_EachDifferenceLevelWithItsItemLine_TheRestAfter()
	{
		TooltipLine[] item =
		[
			TooltipLine.Of("Sword of Ares", bold: true),
			TooltipLine.Of("A blade of legend"),
			TooltipLine.Delimiter,
			TooltipLine.Of("+42 Strength"),
			TooltipLine.Of("25% Chance of:"),
			TooltipLine.Of("    +10% Attack Speed"),
			TooltipLine.Of("+10% Attack Speed"),
		];
		var groups = new[]
		{
			Group(StatGroupKind.Attributes, null
				, Diff("+10% Attack Speed", StatTrend.Higher, KeyOf("+10% Attack Speed"))
				, Diff("12 Strength", StatTrend.Unchanged, KeyOf("+42 Strength"))
				, Diff("-15% Pierce Resistance", StatTrend.Lower, KeyOf("15% Pierce Resistance"), StatPresence.OnlyOther)),
		};

		var lines = ItemComparisonContentBuilder.AlignedDifferenceLines(item, groups);

		lines.Select(l => (l.Kind, l.Text)).Should().Equal(
			(TooltipLineKind.Spacer, ""),
			(TooltipLineKind.Spacer, ""),
			(TooltipLineKind.Delimiter, ""),
			(TooltipLineKind.Text, "12 Strength"),
			(TooltipLineKind.Spacer, ""),
			(TooltipLineKind.Spacer, ""),
			(TooltipLineKind.Text, "+10% Attack Speed"),
			(TooltipLineKind.Delimiter, ""),
			(TooltipLineKind.Text, "-15% Pierce Resistance"));

		ItemComparisonContentBuilder.AlignedDifferenceLines(item, groups, hideUnchanged: true)[3].Kind.Should().Be(TooltipLineKind.Spacer);
	}

	[Fact]
	public void AlignedDifferenceLines_SkillStatsLookedForUnderTheSkillName()
	{
		TooltipLine[] item =
		[
			TooltipLine.Of("6.0 Meter Radius"),
			TooltipLine.Of("Grants Skill : Blood Boil"),
			TooltipLine.Of("6.0 Meter Radius"),
		];
		var groups = new[] { Group(StatGroupKind.GrantedSkill, "Blood Boil", Diff("+1.0 Meter Radius", StatTrend.Higher, KeyOf("6.0 Meter Radius"))) };

		ItemComparisonContentBuilder.AlignedDifferenceLines(item, groups).Select(l => l.Text)
			.Should().Equal("", "", "+1.0 Meter Radius");
	}

	[Fact]
	public void AlignedDifferenceLines_SkillNameNotInTheColumn_SkillStatsFollowTheRows()
	{
		// Without the skill's name row, its stats must not land on the item's own lines of the same stat.
		TooltipLine[] item = [TooltipLine.Of("Shield"), TooltipLine.Of("+100 Armor")];
		var groups = new[] { Group(StatGroupKind.GrantedSkill, "Stone Skin", Diff("+100 Armor", StatTrend.Higher, KeyOf("+100 Armor"))) };

		ItemComparisonContentBuilder.AlignedDifferenceLines(item, groups).Select(l => (l.Kind, l.Text)).Should().Equal(
			(TooltipLineKind.Spacer, ""),
			(TooltipLineKind.Spacer, ""),
			(TooltipLineKind.Delimiter, ""),
			(TooltipLineKind.Text, "Stone Skin"),
			(TooltipLineKind.Text, "+100 Armor"));
	}

	[Fact]
	public void ItemColumnLines_TextOnlyLinesHidden_NameNumbersHeadingsAndWarningsKept()
	{
		TooltipLine[] item =
		[
			TooltipLine.Of("Chiyou's Fall", bold: true),
			TooltipLine.Of("304 Damage"),
			TooltipLine.Of("Speed: Fast"),
			TooltipLine.Spacer,
			TooltipLine.Of("A shield forged in the east"),
			TooltipLine.Of("Bonus to all Pets:"),
			TooltipLine.Of("    +10% Attack Speed"),
			TooltipLine.Of("Grants Skill :"),
			TooltipLine.Of("Stone Skin"),
			TooltipLine.Delimiter,
			TooltipLine.Of("Chiyou's Embrace"),
			TooltipLine.Of("    Chiyou's Spite"),
			TooltipLine.Delimiter,
			TooltipLine.WarningOf("Incomplete relic"),
			TooltipLine.Of("Required Player Level: 64"),
			TooltipLine.Spacer,
		];

		ItemComparisonContentBuilder.ItemColumnLines(item).Select(l => (l.Kind, l.Text)).Should().Equal(
			(TooltipLineKind.Text, "Chiyou's Fall"),
			(TooltipLineKind.Text, "304 Damage"),
			(TooltipLineKind.Spacer, ""),
			(TooltipLineKind.Text, "Bonus to all Pets:"),
			(TooltipLineKind.Text, "    +10% Attack Speed"),
			(TooltipLineKind.Delimiter, ""),
			(TooltipLineKind.Warning, "Incomplete relic"),
			(TooltipLineKind.Text, "Required Player Level: 64"));
	}

	[Fact]
	public void Build_ItemColumnsWithoutTextOnlyLines_DifferencesStillLevelWithTheirStat()
	{
		var translation = new Mock<ITranslationService>();
		var first = new ToFriendlyNameResult(new Item()) { ClassicAttributes = ["+42 Strength", "Speed: Fast", "A legend"] };
		var second = new ToFriendlyNameResult(new Item()) { ClassicAttributes = ["+30 Strength"] };
		var stats = ItemStatComparer.Compare(ItemStatSheet.From(first), ItemStatSheet.From(second));

		var content = new ItemComparisonContentBuilder(translation.Object).Build(new ItemComparison(first, second, stats));

		content.First.Select(l => l.Text).Should().NotContain(["Speed: Fast", "A legend"]);
		content.FirstDifferences[content.First.ToList().FindIndex(l => l.Text == "+42 Strength")].Text.Should().Be("+12 Strength");
	}

	[Fact]
	public void AlignedDifferenceLines_NothingDiffers_SaysSoOnTheFirstRow()
		=> ItemComparisonContentBuilder.AlignedDifferenceLines([TooltipLine.Of("Ring"), TooltipLine.Of("+5 Strength")]
				, [Group(StatGroupKind.Attributes, null, Diff("5 Strength", StatTrend.Unchanged, KeyOf("+5 Strength")))])
			.Select(l => l.Text).Should().Equal(Resources.ItemComparisonNoDifference, "5 Strength");

	[Fact]
	public void Build_ItemColumnsAreTheClassicTooltips_DifferencesOnTheOutside()
	{
		var translation = new Mock<ITranslationService>();
		var tooltip = new ItemTooltipContentBuilder(translation.Object);
		var first = new ToFriendlyNameResult(new Item()) { ClassicAttributes = ["+42 Strength"], ItemSeed = "Seed 1" };
		var second = new ToFriendlyNameResult(new Item()) { ClassicAttributes = ["+30 Strength"], ItemSeed = "Seed 2" };
		var stats = ItemStatComparer.Compare(ItemStatSheet.From(first), ItemStatSheet.From(second));
		var builder = new ItemComparisonContentBuilder(translation.Object);

		var content = builder.Build(new ItemComparison(first, second, stats), new ItemComparisonOptions(AlignRows: false));

		content.Aligned.Should().BeFalse();
		content.First.Should().BeEquivalentTo(ItemComparisonContentBuilder.ItemColumnLines(tooltip.Build(first, ItemTooltipView.Classic).Lines));
		content.Second.Should().BeEquivalentTo(ItemComparisonContentBuilder.ItemColumnLines(tooltip.Build(second, ItemTooltipView.Classic).Lines));
		content.FirstDifferences.Select(l => l.Text).Should().Equal("+12 Strength");
		content.SecondDifferences.Select(l => l.Text).Should().Equal("-12 Strength");

		var aligned = builder.Build(new ItemComparison(first, second, stats));
		aligned.Aligned.Should().BeTrue();
		aligned.FirstDifferences.Should().HaveCount(aligned.First.Count);
		aligned.FirstDifferences[aligned.First.ToList().FindIndex(l => l.Text == "+42 Strength")].Text.Should().Be("+12 Strength");
	}
}
