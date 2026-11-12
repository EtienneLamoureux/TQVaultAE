using AwesomeAssertions;
using TQVaultAE.DbCli.Commands;
using TQVaultAE.DbCli.Query;

namespace TQVaultAE.Tests.DbCli.Cli;

public class PatternTests
{
	[Theory]
	[InlineData("Item*", "ItemRelic", true)]
	[InlineData("Item*", "itemrelic", true)]
	[InlineData("Item*", "LootItemTable", false)] // anchored
	[InlineData("*Relic", "ItemRelic", true)]
	[InlineData("Item?elic", "ItemRelic", true)]
	[InlineData("a.b", "axb", false)] // regex metacharacters are literal in globs
	[InlineData("[ab]x", "bx", true)]
	[InlineData("[!ab]x", "bx", false)]
	[InlineData("[!ab]x", "cx", true)]
	public void Glob_MatchesWholeValueCaseInsensitively(string glob, string value, bool expected) {
		Pattern.Parse(glob).IsMatch(value).Should().Be(expected);
	}

	[Theory]
	[InlineData("records/item/*", @"records\item\sword.dbr")]
	[InlineData(@"records\item\*", "records/item/sword.dbr")]
	[InlineData("/item/sword/", @"records\item\sword\a.dbr")]
	public void PathLike_TreatsSlashAndBackslashAsEqual(string pattern, string value) {
		Pattern.Parse(pattern, pathLike: true).IsMatch(value).Should().BeTrue();
	}

	[Fact]
	public void Regex_IsUnanchoredSearch() {
		var p = Pattern.Parse("/sword0[1-3]/");

		p.IsMatch("c10_SWORD02.dbr").Should().BeTrue();
		p.IsMatch("c10_sword05.dbr").Should().BeFalse();
		p.Literal.Should().BeNull();
	}

	[Fact]
	public void Glob_WithoutWildcards_ExposesNormalizedLiteralForIndexedLookup() {
		Pattern.Parse(@"records\a.dbr", pathLike: true).Literal.Should().Be("records/a.dbr");
		Pattern.Parse("records/*.dbr", pathLike: true).Literal.Should().BeNull();
	}

	[Fact]
	public void Regex_Invalid_IsUsageError() {
		var act = () => Pattern.Parse("/[unclosed/");

		act.Should().Throw<UsageException>().WithMessage("*Invalid regular expression*");
	}

	[Fact]
	public void IsMatch_Null_NeverMatches() {
		Pattern.Parse("*").IsMatch(null).Should().BeFalse();
	}
}
