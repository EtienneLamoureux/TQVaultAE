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
	public void ItemColumnLines_GrantedSkillNameKept_SoItsStatsAlignUnderIt()
	{
		TooltipLine[] item = [TooltipLine.Of("Blood Gem"), TooltipLine.Of("Grants Skill :"), TooltipLine.Of("{^E}Blood Boil"), TooltipLine.Of("6.0 Meter Radius")];

		var column = ItemComparisonContentBuilder.ItemColumnLines(item, ["Blood Boil"]);
		column.Select(l => l.Text).Should().Equal("Blood Gem", "{^E}Blood Boil", "6.0 Meter Radius");

		var groups = new[] { Group(StatGroupKind.GrantedSkill, "Blood Boil", Diff("+6.0 Meter Radius", StatTrend.Higher, KeyOf("6.0 Meter Radius"))) };
		ItemComparisonContentBuilder.AlignedDifferenceLines(column, groups)[2].Text.Should().Be("+6.0 Meter Radius");
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
	public void Options_FromAndToTheSettings_OnlyAChangeNeedsASave()
	{
		var settings = new TQVaultAE.Config.UserSettings();
		ItemComparisonOptions.From(settings.ItemCompare).Should().Be(new ItemComparisonOptions(AlignRows: true, HideUnchanged: false));

		new ItemComparisonOptions().ApplyTo(settings).Should().BeFalse("nothing changed");
		new ItemComparisonOptions(AlignRows: false, HideUnchanged: true).ApplyTo(settings).Should().BeTrue();
		ItemComparisonOptions.From(settings.ItemCompare).Should().Be(new ItemComparisonOptions(AlignRows: false, HideUnchanged: true));
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

	#region Sources

	private static readonly string Marker = TQColor.DarkGray.ColorTag();

	/// <summary>"+5 Pierce" against "35%": 20% base and 15% from a relic on this item, 30% base on the other.</summary>
	private static StatDifference PierceWithSources(StatTrend trend = StatTrend.Higher, string text = "+5% Pierce Resistance")
		=> Diff(text, trend, KeyOf("35% Pierce Resistance")) with
		{
			Sources =
			[
				new StatSourceDifference(StatSource.Base, "-10% Pierce Resistance", StatTrend.Lower, "20% Pierce Resistance"),
				new StatSourceDifference(StatSource.Relic1, "+15% Pierce Resistance", StatTrend.Higher, null),
			],
		};

	private static string SourceLine(StatSource source, string text)
		=> StatLine.Indent + string.Format(System.Globalization.CultureInfo.CurrentCulture, Resources.ItemComparisonSourceLine, ItemComparisonContentBuilder.SourceLabel(source), text);

	[Fact]
	public void DifferenceLines_SourcesCollapsed_TheTotalAloneWithAClosedMarker()
	{
		var lines = ItemComparisonContentBuilder.DifferenceLines([Group(StatGroupKind.Attributes, null, PierceWithSources(), Diff("+1 Strength", StatTrend.Higher))]);

		lines.Select(l => l.Text).Should().Equal($"+5% Pierce Resistance  {Marker}{ItemComparisonContentBuilder.SourcesClosedMarker}", "+1 Strength");
		lines.Select(l => l.ToggleKey).Should().Equal(KeyOf("35% Pierce Resistance"), null);
		lines.Should().OnlyContain(l => !l.Detail);
		lines[0].Color.Should().Be(TQColor.Green, "the total keeps its color");
	}

	[Fact]
	public void DifferenceLines_SourcesShown_OneMutedLinePerSourceInItsColor()
	{
		var lines = ItemComparisonContentBuilder.DifferenceLines([Group(StatGroupKind.Attributes, null, PierceWithSources())], sourcesShown: _ => true);

		lines.Select(l => (l.Text, l.Color, l.Detail)).Should().Equal(
			($"+5% Pierce Resistance  {Marker}{ItemComparisonContentBuilder.SourcesOpenMarker}", TQColor.Green, false),
			(SourceLine(StatSource.Base, "-10% Pierce Resistance"), TQColor.Red, true),
			(SourceLine(StatSource.Relic1, "+15% Pierce Resistance"), TQColor.Green, true));
	}

	[Fact]
	public void AlignedColumns_SourcesShown_RowsUnderTheStatInBothColumns_DashForAMissingSource()
	{
		TooltipLine[] item = [TooltipLine.Of("Sword"), TooltipLine.Of("35% Pierce Resistance"), TooltipLine.Of("+5 Strength")];
		var groups = new[] { Group(StatGroupKind.Attributes, null, PierceWithSources(), Diff("+1 Strength", StatTrend.Higher, KeyOf("+5 Strength"))) };

		var closed = ItemComparisonContentBuilder.AlignedColumns(item, groups);
		var open = ItemComparisonContentBuilder.AlignedColumns(item, groups, sourcesShown: _ => true);

		closed.Item.Select(l => l.Text).Should().Equal("Sword", "35% Pierce Resistance", "+5 Strength");
		closed.Item[1].ToggleKey.Should().Be(KeyOf("35% Pierce Resistance"), "a click on the item line opens the sources too");
		open.Item.Select(l => l.Text).Should().Equal("Sword", "35% Pierce Resistance",
			SourceLine(StatSource.Base, "20% Pierce Resistance"), SourceLine(StatSource.Relic1, ItemComparisonContentBuilder.NoSourceText), "+5 Strength");
		open.Item.Select(l => l.Detail).Should().Equal(false, false, true, true, false);
		open.Differences.Select(l => l.Text).Should().Equal(string.Empty,
			$"+5% Pierce Resistance  {Marker}{ItemComparisonContentBuilder.SourcesOpenMarker}",
			SourceLine(StatSource.Base, "-10% Pierce Resistance"), SourceLine(StatSource.Relic1, "+15% Pierce Resistance"), "+1 Strength");
	}

	[Fact]
	public void AlignedColumns_IndentedStat_ItsSourcesOneIndentDeeper()
	{
		TooltipLine[] item = [TooltipLine.Of("10% Chance of:"), TooltipLine.Of("    +5 Strength")];
		var key = StatLine.Parse("    +5 Strength", StatLine.Parse("10% Chance of:")!.Template)!.Key;
		var stat = Diff($"{StatLine.Indent}+2 Strength", StatTrend.Higher, key) with
		{
			Sources = [new(StatSource.Base, "    +1 Strength", StatTrend.Higher, "    +3 Strength"), new(StatSource.Prefix, "    +1 Strength", StatTrend.Higher, "    +2 Strength")],
		};

		var open = ItemComparisonContentBuilder.AlignedColumns(item, [Group(StatGroupKind.Attributes, null, stat)], sourcesShown: _ => true);

		open.Item[2].Text.Should().Be($"{StatLine.Indent}{SourceLine(StatSource.Base, "+3 Strength")}");
	}

	[Theory]
	[InlineData(false, false)]
	[InlineData(true, true)]
	public void HideUnchanged_SameTotalFromOtherSources_KeptOnlyWhenTheSourcesShow(bool shown, bool kept)
	{
		var lines = ItemComparisonContentBuilder.DifferenceLines(
			[Group(StatGroupKind.Attributes, null, PierceWithSources(StatTrend.Unchanged, "35% Pierce Resistance"))], hideUnchanged: true, sourcesShown: _ => shown);

		lines.Any(l => l.Text.StartsWith("35% Pierce Resistance", StringComparison.Ordinal)).Should().Be(kept);
	}

	[Fact]
	public void Options_SourcesShown_FollowsShowSources_AClickedStatGoesTheOtherWay()
	{
		var stat = PierceWithSources();
		var clicked = new HashSet<string> { stat.Key };

		new ItemComparisonOptions().SourcesShown(stat).Should().BeFalse();
		new ItemComparisonOptions().SourcesShown(stat, clicked).Should().BeTrue();
		new ItemComparisonOptions(ShowSources: true).SourcesShown(stat).Should().BeTrue();
		new ItemComparisonOptions(ShowSources: true).SourcesShown(stat, clicked).Should().BeFalse();
		new ItemComparisonOptions(ShowSources: true).SourcesShown(Diff("+1 Strength", StatTrend.Higher)).Should().BeFalse("one source only");
	}

	[Fact]
	public void Options_ShowSources_SavedAndReadBack()
	{
		var settings = new TQVaultAE.Config.UserSettings();

		new ItemComparisonOptions(ShowSources: true).ApplyTo(settings).Should().BeTrue();
		settings.ItemCompare.ShowSources.Should().BeTrue();
		ItemComparisonOptions.From(settings.ItemCompare).Should().Be(new ItemComparisonOptions(ShowSources: true));
		new ItemComparisonOptions(ShowSources: true).ApplyTo(settings).Should().BeFalse("nothing changed");
	}

	[Fact]
	public void Build_RelicOnOneItem_CollapsedTotalsAsWithoutSources_ClickedStatOpensInAllFourColumns()
	{
		var translation = new Mock<ITranslationService>();
		var first = new ToFriendlyNameResult(new Item()) { ClassicAttributes = ["20% Pierce Resistance", "+5 Strength"], Relic1Attributes = ["15% Pierce Resistance"] };
		var second = new ToFriendlyNameResult(new Item()) { ClassicAttributes = ["35% Pierce Resistance", "+4 Strength"] };
		var comparison = new ItemComparison(first, second, ItemStatComparer.Compare(ItemStatSheet.From(first), ItemStatSheet.From(second)));
		var builder = new ItemComparisonContentBuilder(translation.Object);

		var closed = builder.Build(comparison);
		var open = builder.Build(comparison, toggled: new HashSet<string> { KeyOf("35% Pierce Resistance") });

		// Collapsed: the same totals, the stat with sources only gets its marker.
		closed.FirstDifferences.Where(l => l.Kind == TooltipLineKind.Text).Select(l => l.Text).Should().Equal(
			$"35% Pierce Resistance  {Marker}{ItemComparisonContentBuilder.SourcesClosedMarker}", "+1 Strength");
		closed.FirstDifferences.Should().OnlyContain(l => !l.Detail);

		// Opened: two rows more on each side, level with each other.
		open.First.Should().HaveCount(closed.First.Count + 2);
		open.Second.Should().HaveCount(closed.Second.Count + 2);
		open.FirstDifferences.Should().HaveCount(open.First.Count);
		open.SecondDifferences.Should().HaveCount(open.Second.Count);
		int row = open.Second.ToList().FindIndex(l => l.Text == "35% Pierce Resistance");
		open.Second[row + 2].Text.Should().Be(SourceLine(StatSource.Relic1, ItemComparisonContentBuilder.NoSourceText));
		open.SecondDifferences[row + 2].Text.Should().Be(SourceLine(StatSource.Relic1, "-15% Pierce Resistance"));
		open.SecondDifferences[row + 2].Color.Should().Be(TQColor.Red);
	}

	#endregion
}
