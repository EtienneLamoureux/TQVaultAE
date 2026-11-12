using AwesomeAssertions;
using TQVaultAE.Domain.Comparison;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Results;

namespace TQVaultAE.Tests.Domain;

/// <summary>
/// <see cref="ItemStatSheet"/> and <see cref="ItemStatComparer"/>: the stats of two items compared stat by stat.
/// </summary>
public class ItemStatComparerTests
{
	#region Fixtures

	private static ToFriendlyNameResult Texts(string[] attributes, string[]? requirements = null) => new(new Item())
	{
		ClassicAttributes = attributes,
		Requirements = requirements ?? [],
	};

	private static ItemStatComparison Compare(ToFriendlyNameResult first, ToFriendlyNameResult second)
		=> ItemStatComparer.Compare(ItemStatSheet.From(first), ItemStatSheet.From(second));

	private static IEnumerable<string> LinesOf(IReadOnlyList<StatDifferenceGroup> groups)
		=> groups.SelectMany(g => g.Lines).Select(l => l.Text);

	/// <summary>The groups with their differing lines only, the groups left empty dropped.</summary>
	private static List<StatDifferenceGroup> Changed(IReadOnlyList<StatDifferenceGroup> groups)
		=> [.. groups.Select(g => g with { Lines = [.. g.Lines.Where(l => l.Trend != StatTrend.Unchanged)] }).Where(g => g.Lines.Count != 0)];

	private static GrantedSkill Skill(string id, string name, string level, params string[] attributes)
		=> new(RecordId.Create(id), name, 1, level, null, null, [], attributes);

	#endregion

	[Fact]
	public void Compare_SameStat_ShowsTheDifferenceBothWays()
	{
		var result = Compare(Texts(["+42 Strength"]), Texts(["+30 Strength"]));

		LinesOf(result.FirstMinusSecond).Should().Equal("+12 Strength");
		LinesOf(result.SecondMinusFirst).Should().Equal("-12 Strength");
		result.FirstMinusSecond[0].Lines[0].Trend.Should().Be(StatTrend.Higher);
		result.SecondMinusFirst[0].Lines[0].Trend.Should().Be(StatTrend.Lower);
	}

	[Fact]
	public void Compare_EqualStats_AreUnchanged_WithTheirOwnValues()
	{
		var result = Compare(Texts(["+42 Strength", "20.0% Chance of 10 Damage"]), Texts(["20.0% Chance of 10 Damage", "+42 Strength"]));

		result.FirstMinusSecond.Single().Lines.Select(l => (l.Text, l.Trend, l.Presence)).Should().Equal(
			("+42 Strength", StatTrend.Unchanged, StatPresence.Both),
			("20.0% Chance of 10 Damage", StatTrend.Unchanged, StatPresence.Both));
		result.SecondMinusFirst.Single().Lines.Should().OnlyContain(l => l.Trend == StatTrend.Unchanged);
	}

	[Fact]
	public void Compare_StatOnOneItemOnly_IsFullyPlusOnItsSideAndFullyMinusOnTheOther()
	{
		var result = Compare(Texts(["+200 Health"]), Texts(["15% Pierce Resistance"]));

		var first = result.FirstMinusSecond.Single().Lines;
		first.Select(l => (l.Text, l.Presence)).Should().Equal(
			("+200 Health", StatPresence.OnlyThis),
			("-15% Pierce Resistance", StatPresence.OnlyOther));

		LinesOf(result.SecondMinusFirst).Should().Equal("+15% Pierce Resistance", "-200 Health");
	}

	[Fact]
	public void Compare_TextOnlyLines_AreLeftOut()
	{
		// Lore, titles and texts without a number ("Attack Speed: Fast") are not compared nor shown.
		var result = Compare(Texts(["Attack Speed: Fast", "A blade forged by Hephaestus", "+5 Strength"]), Texts(["Attack Speed: Very Fast"]));

		LinesOf(result.FirstMinusSecond).Should().Equal("+5 Strength");
		LinesOf(result.SecondMinusFirst).Should().Equal("-5 Strength");
	}

	[Fact]
	public void From_TextOnlyLine_StillHeadsTheIndentedLinesUnderIt()
	{
		var sheet = ItemStatSheet.From(Texts(["Bonus to all Pets:", "    +10% Attack Speed", "+10% Attack Speed"]));

		sheet.Groups.Single().Stats.Select(s => s.Key).Should().Equal(
			$"Bonus to all Pets:\n{StatLine.Slot}% Attack Speed",
			$"{StatLine.Slot}% Attack Speed");
	}

	[Fact]
	public void Compare_StatOnOneItemOnly_TrendFollowsTheSignOfItsNumbers()
	{
		var result = Compare(Texts(["-10% Recharge"]), Texts(["+5 Strength"]));

		result.FirstMinusSecond.Single().Lines.Select(l => (l.Text, l.Trend)).Should().Equal(
			("-10% Recharge", StatTrend.Lower),
			("-5 Strength", StatTrend.Lower));
		result.SecondMinusFirst.Single().Lines.Select(l => (l.Text, l.Trend)).Should().Equal(
			("+5 Strength", StatTrend.Higher),
			("+10% Recharge", StatTrend.Higher));
	}

	[Fact]
	public void Compare_LowerIsBetterStats_AreMarked_ByTheSignatureOfTheirLabel()
	{
		var lowerIsBetter = LowerIsBetterStats.Signatures(["-{%.0f0}% Recharge", "^a{%.1f0} Second(s) Recharge"]);
		var first = Texts(["-10% Recharge", "+5 Strength"]);
		first.GrantedSkills = [Skill("records/skills/boil.dbr", "Blood Boil", "Level: 1", "12.0 Second(s) Recharge")];
		var second = Texts(["-15% Recharge", "+3 Strength"]);
		second.GrantedSkills = [Skill("records/skills/boil.dbr", "Blood Boil", "Level: 1", "10.0 Second(s) Recharge")];

		var groups = Changed(ItemStatComparer.Compare(ItemStatSheet.From(first), ItemStatSheet.From(second), lowerIsBetter).FirstMinusSecond);

		groups.SelectMany(g => g.Lines).Select(l => (l.Text, l.Trend, l.LowerIsBetter)).Should().Equal(
			("+5% Recharge", StatTrend.Higher, true),
			("+2 Strength", StatTrend.Higher, false),
			("+2.0 Second(s) Recharge", StatTrend.Higher, true));
	}

	[Fact]
	public void Compare_Requirements_AreLowerIsBetterLines()
		=> Compare(Texts([], ["Required Strength: 650"]), Texts([], ["Required Strength: 600"]))
			.FirstMinusSecond.Single().Lines.Single().LowerIsBetter.Should().BeTrue();

	[Theory]
	[InlineData("^a{%.1f0} Second(s) Recharge", "1.5 Second(s) Recharge")]
	[InlineData("-{%.0f0}% Strength Requirement for all Weapons", "-15% Strength Requirement for all Weapons")]
	[InlineData("{%+.0f0}% less Energy Reserved", "+5% less Energy Reserved")]
	[InlineData("Energy Cost", "45 Energy Cost")]
	public void Signature_SameForTheLabelAndTheDisplayedLine(string label, string line)
		=> LowerIsBetterStats.Signature(label).Should().Be(LowerIsBetterStats.Signature(StatLine.Parse(line)!.Template));

	[Fact]
	public void Signature_DiffersForAnotherStat()
		=> LowerIsBetterStats.Signature("-{%.0f0}% Recharge").Should().NotBe(LowerIsBetterStats.Signature("^a{%.1f0} Second(s) Recharge"));

	[Fact]
	public void Compare_SeveralNumbers_EachDiffersAndTheTrendCanBeMixed()
	{
		var result = Compare(
			Texts(["20.0% Chance of 111 Bleeding Damage Retaliation over 3.0 Seconds"]),
			Texts(["25.0% Chance of 100 Bleeding Damage Retaliation over 3.0 Seconds"]));

		var line = result.FirstMinusSecond.Single().Lines.Single();
		line.Text.Should().Be("-5.0% Chance of +11 Bleeding Damage Retaliation over 3.0 Seconds");
		line.Trend.Should().Be(StatTrend.Mixed);
	}

	[Fact]
	public void Compare_DecimalsOfEitherItem_AreKept()
		=> LinesOf(Compare(Texts(["+1 Health Regeneration per second"]), Texts(["+0.5 Health Regeneration per second"])).FirstMinusSecond)
			.Should().Equal("+0.5 Health Regeneration per second");

	[Fact]
	public void From_RelicAndCompletionBonus_AddToTheItemsOwnStat()
	{
		var withRelic = Texts(["30% Stun Resistance"]);
		withRelic.Relic1Attributes = ["35% Stun Resistance"];
		withRelic.RelicBonus1Attributes = ["50% Stun Resistance"];

		var result = Compare(withRelic, Texts(["30% Stun Resistance"]));

		LinesOf(result.FirstMinusSecond).Should().Equal("+85% Stun Resistance");
	}

	[Fact]
	public void From_SecondRelicAndItsBonus_AreCounted()
	{
		var twoRelics = Texts([]);
		twoRelics.Relic2Attributes = ["+10 Strength"];
		twoRelics.RelicBonus2Attributes = ["+5 Strength"];

		LinesOf(Compare(twoRelics, Texts([])).FirstMinusSecond).Should().Equal("+15 Strength");
	}

	[Fact]
	public void From_WithoutClassicOrder_UsesBasePrefixAndSuffixAttributes()
	{
		var detailed = new ToFriendlyNameResult(new Item())
		{
			BaseAttributes = ["+10 Strength"],
			PrefixAttributes = ["+5 Strength"],
			SuffixAttributes = ["+20 Health"],
		};

		LinesOf(Compare(detailed, Texts([])).FirstMinusSecond).Should().Equal("+15 Strength", "+20 Health");
	}

	[Fact]
	public void Compare_ChanceGroupEffects_AreNotMatchedWithTheItemsOwnStat()
	{
		var result = Compare(
			Texts(["25% Chance of:", "    +10% Attack Speed"]),
			Texts(["+10% Attack Speed"]));

		LinesOf(result.FirstMinusSecond).Should().Equal("+25% Chance of:", "    +10% Attack Speed", "-10% Attack Speed");
	}

	[Fact]
	public void Compare_Requirements_AreAGroupWhereLowerIsBetter_ShownLast()
	{
		var first = Texts(["+42 Strength"], ["Required Strength: 650"]);
		var second = Texts(["+30 Strength"], ["Required Strength: 600"]);

		var groups = Compare(first, second).FirstMinusSecond;

		groups.Select(g => (g.Kind, g.LowerIsBetter)).Should().Equal(
			(StatGroupKind.Attributes, false),
			(StatGroupKind.Requirements, true));
		groups[1].Lines.Single().Text.Should().Be("Required Strength: +50");
	}

	[Fact]
	public void Compare_GrantedSkills_SameSkillComparedApartFromTheAttributes()
	{
		var first = Texts(["1170 Bleeding Damage over 2.0 Seconds"]);
		first.GrantedSkills = [Skill("records/skills/bloodboil.dbr", "Blood Boil", "Level: 2", "1170 Bleeding Damage over 2.0 Seconds")];
		var second = Texts(["1170 Bleeding Damage over 2.0 Seconds"]);
		second.GrantedSkills = [Skill("records/skills/bloodboil.dbr", "Blood Boil", "Level: 1", "1000 Bleeding Damage over 2.0 Seconds")];

		var group = Changed(Compare(first, second).FirstMinusSecond).Single();

		group.Kind.Should().Be(StatGroupKind.GrantedSkill);
		group.Title.Should().Be("Blood Boil");
		group.Lines.Select(l => l.Text).Should().Equal("Level: +1", "+170 Bleeding Damage over 2.0 Seconds");
	}

	[Fact]
	public void From_SkillNamedInTheAttributes_OnlyInItsOwnGroup()
	{
		var granting = Texts(["+10% Attack Speed", "", "Grants Skill :", "{^E}Venomous Mist"]);
		granting.GrantedSkills = [Skill("records/skills/mist.dbr", "Venomous Mist", "Level: 3", "-50 Energy Cost")];

		var sheet = ItemStatSheet.From(granting);

		sheet.Groups.Select(g => g.Stats.Count).Should().Equal(1, 2);
		sheet.Groups[0].Stats.Single().Template.Should().Be($"{StatLine.Slot}% Attack Speed");
	}

	[Fact]
	public void Compare_SkillOnlyTheOtherItemGrants_IsFullyMinusBeforeTheRequirements()
	{
		var first = Texts(["+42 Strength"], ["Required Player Level: 40"]);
		var second = Texts(["+42 Strength"], ["Required Player Level: 40"]);
		second.GrantedSkills = [Skill("records/skills/boil.dbr", "Blood Boil", "Level: 1", "6.0 Meter Radius")];

		var groups = Changed(Compare(first, second).FirstMinusSecond);

		groups.Should().ContainSingle();
		groups[0].Title.Should().Be("Blood Boil");
		groups[0].Lines.Select(l => l.Text).Should().Equal("Level: -1", "-6.0 Meter Radius");
		groups[0].Lines.Should().OnlyContain(l => l.Presence == StatPresence.OnlyOther);
	}

	[Fact]
	public void Compare_SkillOnlyTheOtherItemGrants_GoesBeforeDifferingRequirements()
	{
		var first = Texts([], ["Required Player Level: 40"]);
		var second = Texts([], ["Required Player Level: 30"]);
		second.GrantedSkills = [Skill("records/skills/boil.dbr", "Blood Boil", "Level: 1")];

		Compare(first, second).FirstMinusSecond.Select(g => g.Kind)
			.Should().Equal(StatGroupKind.GrantedSkill, StatGroupKind.Requirements);
	}
}
