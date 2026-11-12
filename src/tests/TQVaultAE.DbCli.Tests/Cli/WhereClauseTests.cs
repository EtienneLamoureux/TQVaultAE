using AwesomeAssertions;
using TQVaultAE.DbCli.Commands;
using TQVaultAE.DbCli.Query;

namespace TQVaultAE.Tests.DbCli.Cli;

public class WhereClauseTests
{
	private static readonly ArzRecord Sword = V.Record(
		@"records\item\equipmentweapon\sword\c10_sword01.dbr",
		V.Int("itemLevel", 40),
		V.Float("offensivePhysicalMin", 12.5f),
		V.Float("characterAttackSpeed", 0.1f),
		V.Int("levelRequirement", 0),
		V.Str("itemNameTag", "tagItemHookedBlade"),
		V.Str("lootTable", @"records\a.dbr", @"records\b.dbr"),
		V.Int("weights", 5, 50, 500),
		V.Bool("hidePrefixName", true),
		V.Str("description", ""),
		V.Str("quality", "Rare"));

	private static bool Eval(string expression, IRecordNamer? namer = null) => WhereClause.Parse(expression).Evaluate(Sword, namer);

	[Theory]
	[InlineData("itemLevel>39", true)]
	[InlineData("itemLevel>40", false)]
	[InlineData("itemLevel>=40", true)]
	[InlineData("itemLevel<=40", true)]
	[InlineData("itemLevel<40", false)]
	[InlineData("itemLevel>9", true)] // numeric, not "40" < "9" as strings
	[InlineData("offensivePhysicalMin>12.4", true)]
	public void NumericOperators_CompareNumerically(string expression, bool expected) {
		Eval(expression).Should().Be(expected);
	}

	[Fact]
	public void Equality_OnFloat_ToleratesSinglePrecision() {
		// 0.1f widened to double is 0.10000000149..., still equal to "0.1".
		Eval("characterAttackSpeed=0.1").Should().BeTrue();
	}

	[Fact]
	public void FieldNames_AreCaseInsensitive() {
		Eval("ITEMLEVEL=40").Should().BeTrue();
	}

	[Theory]
	[InlineData("weights=50", true)]
	[InlineData("weights>400", true)]
	[InlineData("weights=7", false)]
	[InlineData("weights!=50", false)] // != means no value equals
	[InlineData("weights!=7", true)]
	public void MultiValueFields_MatchWhenAnyValueMatches(string expression, bool expected) {
		Eval(expression).Should().Be(expected);
	}

	[Fact]
	public void NotEqual_OnAbsentField_IsTrue_WhileOtherOperatorsAreFalse() {
		Eval("missingField!=1").Should().BeTrue();
		Eval("missingField=1").Should().BeFalse();
		Eval("missingField<1").Should().BeFalse();
	}

	[Theory]
	[InlineData("itemLevel?", true)]
	[InlineData("levelRequirement?", false)] // zero
	[InlineData("description?", false)] // empty string
	[InlineData("hidePrefixName?", true)]
	[InlineData("missingField?", false)]
	[InlineData("!levelRequirement?", true)]
	public void Exists_RequiresNonZeroValue(string expression, bool expected) {
		Eval(expression).Should().Be(expected);
	}

	[Fact]
	public void Like_UsesPathInsensitiveGlob() {
		Eval("lootTable~records/b.*").Should().BeTrue();
		Eval("itemNameTag~*hooked*").Should().BeTrue();
		Eval("itemNameTag~/^tagItem/").Should().BeTrue();
	}

	[Fact]
	public void Equality_OnStrings_IgnoresCaseAndSeparators() {
		Eval("quality=rare").Should().BeTrue();
		Eval("lootTable=records/a.dbr").Should().BeTrue();
	}

	[Fact]
	public void StringOrdering_FallsBackToOrdinalCompare() {
		Eval("quality>Common").Should().BeTrue();
		Eval("quality<Epic").Should().BeFalse();
	}

	[Fact]
	public void FieldGlob_MatchesAnyVariable() {
		Eval("offensive*Min>10").Should().BeTrue();
		Eval("offensive*Max?").Should().BeFalse();
	}

	[Fact]
	public void PseudoFields_IdAndDisplayName_AreAvailable() {
		var namer = new FakeNamer(new() { ["tagItemHookedBlade"] = "Hooked Blade" });

		Eval("id~*sword01.dbr").Should().BeTrue();
		Eval("displayName=hooked blade", namer).Should().BeTrue();
		WhereClause.Parse("displayName=x").UsesDisplayName.Should().BeTrue();
	}

	[Theory]
	[InlineData("noOperator")]
	[InlineData("=5")]
	[InlineData("level>")]
	[InlineData("level?extra")]
	public void Parse_InvalidExpression_IsUsageError(string expression) {
		var act = () => WhereClause.Parse(expression);

		act.Should().Throw<UsageException>();
	}
}
