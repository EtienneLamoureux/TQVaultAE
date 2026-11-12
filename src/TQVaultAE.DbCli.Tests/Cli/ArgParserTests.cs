using AwesomeAssertions;
using TQVaultAE.DbCli.Commands;

namespace TQVaultAE.Tests.DbCli.Cli;

public class ArgParserTests
{
	private static readonly OptionSpec[] Globals = [
		new("game", 'g', "path", "Game folder."),
		new("quiet", 'q', null, "Quiet."),
	];

	private static readonly CommandSpec[] Commands = [
		new("records", "Search.", [
			new("class", 'c', "glob", "Class.", Repeatable: true),
			new("limit", null, "N", "Limit."),
			new("count", null, null, "Count."),
			new("resolve", 'r', null, "Resolve."),
			new("fields", 'f', "list", "Fields.", Repeatable: true),
		], [], []),
		new("record", "One.", [new("format", null, "fmt", "Format.")], [new PositionalSpec("id", "Id.")], []),
	];

	private static ParsedArgs Parse(params string[] args) => ArgParser.Parse(args, Commands, Globals);

	[Fact]
	public void Parse_InlineAndSeparateValues_AreEquivalent() {
		var a = Parse("records", "--limit=5", "--class", "ItemRelic");

		a.Command.Should().Be("records");
		a.GetInt("limit", 0).Should().Be(5);
		a.Get("class").Should().Be("ItemRelic");
	}

	[Fact]
	public void Parse_RepeatableOption_AccumulatesValuesInOrder() {
		var a = Parse("records", "-c", "A*", "--class=B*", "-cC*");

		a.GetAll("class").Should().Equal("A*", "B*", "C*");
	}

	[Fact]
	public void Parse_NonRepeatableOptionTwice_IsUsageError() {
		var act = () => Parse("records", "--limit", "1", "--limit", "2");

		act.Should().Throw<UsageException>().WithMessage("*--limit*more than once*");
	}

	[Fact]
	public void Parse_UnknownOption_SuggestsClosestName() {
		var act = () => Parse("records", "--clas", "X");

		act.Should().Throw<UsageException>().WithMessage("*Did you mean '--class'*").Which.Command.Should().Be("records");
	}

	[Fact]
	public void Parse_UnknownCommand_SuggestsClosestCommand() {
		var act = () => Parse("recrods");

		act.Should().Throw<UsageException>().WithMessage("*Did you mean 'records'*");
	}

	[Fact]
	public void Parse_GlobalOptions_AcceptedBeforeAndAfterCommand() {
		var a = Parse("--game", "C:/TQ", "records", "-q");

		a.Get("game").Should().Be("C:/TQ");
		a.Has("quiet").Should().BeTrue();
	}

	[Fact]
	public void Parse_CombinedShortFlags_SetEachFlag() {
		var a = Parse("records", "-qr");

		a.Has("quiet").Should().BeTrue();
		a.Has("resolve").Should().BeTrue();
	}

	[Fact]
	public void Parse_DoubleDash_TreatsFollowingTokensAsPositionals() {
		var a = Parse("record", "--", "--weird-id.dbr");

		a.Positionals.Should().Equal("--weird-id.dbr");
	}

	[Fact]
	public void Parse_OptionValueStartingWithDash_IsTakenAsValue() {
		var a = Parse("records", "--class", "-x");

		a.Get("class").Should().Be("-x");
	}

	[Fact]
	public void Parse_MissingValueAtEnd_IsUsageError() {
		var act = () => Parse("records", "--limit");

		act.Should().Throw<UsageException>().WithMessage("*--limit requires a value*");
	}

	[Fact]
	public void Parse_FlagWithInlineValue_IsUsageError() {
		var act = () => Parse("records", "--count=yes");

		act.Should().Throw<UsageException>().WithMessage("*does not take a value*");
	}

	[Fact]
	public void Parse_MissingRequiredPositional_IsUsageError() {
		var act = () => Parse("record");

		act.Should().Throw<UsageException>().WithMessage("*<id>*");
	}

	[Fact]
	public void Parse_ExtraPositional_IsUsageError() {
		var act = () => Parse("record", "a.dbr", "b.dbr");

		act.Should().Throw<UsageException>().WithMessage("*'b.dbr'*");
	}

	[Fact]
	public void Parse_HelpFlag_SkipsPositionalValidation() {
		var a = Parse("record", "--help");

		a.HelpRequested.Should().BeTrue();
	}

	[Fact]
	public void GetList_SplitsCommasAcrossRepeats_AndDropsEmptyItems() {
		var a = Parse("records", "--fields", "id, class,", "-f", "itemLevel");

		a.GetList("fields").Should().Equal("id", "class", "itemLevel");
	}

	[Theory]
	[InlineData("abc")]
	[InlineData("-1")]
	public void GetInt_OutOfRangeOrNotANumber_IsUsageError(string value) {
		var a = Parse("records", "--limit", value);

		var act = () => a.GetInt("limit", 0, min: 0);

		act.Should().Throw<UsageException>();
	}

	[Fact]
	public void GetChoice_IsCaseInsensitive_AndRejectsUnknown() {
		var ok = Parse("record", "x", "--format", "JSON");
		ok.GetChoice("format", "dbr", "json", "dbr").Should().Be("json");

		var bad = Parse("record", "x", "--format", "xml");
		var act = () => bad.GetChoice("format", "dbr", "json", "dbr");
		act.Should().Throw<UsageException>().WithMessage("*json, dbr*");
	}

	[Fact]
	public void Help_ListsCommandAndGlobalOptions() {
		var help = ArgParser.Help("tqdb", Commands[0], Globals);

		help.Should().Contain("Usage: tqdb records").And.Contain("--class <glob>").And.Contain("(repeatable)").And.Contain("--game <path>");
	}

	[Fact]
	public void Program_UsageError_ReturnsExitCode2() {
		var err = new StringWriter();

		int code = TQVaultAE.DbCli.Program.Run(["records", "--nope"], err, CancellationToken.None);

		code.Should().Be(ExitCodes.Usage);
		err.ToString().Should().Contain("Unknown option '--nope'");
	}
}
