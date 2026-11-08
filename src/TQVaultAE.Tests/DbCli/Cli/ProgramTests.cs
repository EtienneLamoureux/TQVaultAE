using AwesomeAssertions;
using TQVaultAE.DbCli;

namespace TQVaultAE.Tests.DbCli.Cli;

/// <summary>Help and version go to <see cref="Console.Out"/>: these tests swap it, so they run alone.</summary>
[CollectionDefinition(nameof(ConsoleOutCollection), DisableParallelization = true)]
public sealed class ConsoleOutCollection;

[Collection(nameof(ConsoleOutCollection))]
public sealed class ProgramTests : IDisposable
{
	private readonly FakeGameInstall _game = new();

	public void Dispose() => this._game.Dispose();

	private static (int Code, string Out, string Error) RunCapturingOut(params string[] args)
	{
		var original = Console.Out;
		var output = new StringWriter();
		var error = new StringWriter();
		Console.SetOut(output);
		try
		{
			int code = Program.Run(args, error, CancellationToken.None);
			return (code, output.ToString(), error.ToString());
		}
		finally
		{
			Console.SetOut(original);
		}
	}

	[Fact]
	public void Run_WithoutCommand_PrintsOverviewAndExitsWith2()
	{
		var (code, output, _) = RunCapturingOut();

		code.Should().Be(2);
		output.Should().Contain("Usage: tqdb <command> [options]").And.Contain("extract");
	}

	[Theory]
	[InlineData("--help")]
	[InlineData("help")]
	public void Run_Help_PrintsOverviewAndExitsWith0(string arg)
	{
		var (code, output, _) = RunCapturingOut(arg);

		code.Should().Be(0);
		output.Should().Contain("Usage: tqdb <command> [options]");
	}

	[Theory]
	[InlineData("help", "extract")]
	[InlineData("extract", "--help")]
	[InlineData("extract", "-h")]
	public void Run_CommandHelp_PrintsItsOptionsWithoutNeedingTheGame(string first, string second)
	{
		var (code, output, error) = RunCapturingOut("--game", "Z:\\no-game-here", first, second);

		code.Should().Be(0);
		output.Should().Contain("Usage: tqdb extract").And.Contain("--skip-existing");
		error.Should().BeEmpty();
	}

	[Fact]
	public void Run_HelpOfUnknownCommand_IsUsageErrorWithSuggestion()
	{
		var (code, output, error) = RunCapturingOut("help", "recrods");

		code.Should().Be(2);
		output.Should().BeEmpty();
		error.Should().Contain("Unknown command 'recrods'.").And.Contain("records").And.Contain("Run 'tqdb --help' for usage.");
	}

	[Fact]
	public void Run_Version_PrintsToolAndVersion()
	{
		var (code, output, _) = RunCapturingOut("--version");

		code.Should().Be(0);
		output.Should().MatchRegex(@"^tqdb \d+\.\d+\.\d+\.\d+\r?\n$");
	}

	[Fact]
	public void Run_UnknownCommand_IsUsageError()
	{
		var error = new StringWriter();

		int code = Program.Run(["recrods"], error, CancellationToken.None);

		code.Should().Be(2);
		error.ToString().Should().Contain("Run 'tqdb --help' for usage.");
	}

	[Fact]
	public void Run_GameFolderWithoutDatabase_ExitsWith3WithoutSuggestingGameOption()
	{
		var folder = Path.Combine(this._game.Root, "Resources");
		var error = new StringWriter();

		int code = Program.Run(["info", "--game", folder], error, CancellationToken.None);

		code.Should().Be(3);
		error.ToString().Should().Contain($"'{folder}' is not a Titan Quest install").And.NotContain("Use --game");
	}

	[Theory]
	[InlineData("0")]
	[InlineData("2000")]
	public void Run_ThreadsOutOfRange_IsUsageError(string threads)
	{
		var (code, error) = this._game.Run("info", "--threads", threads);

		code.Should().Be(2);
		error.Should().Contain("--threads");
	}

	[Fact]
	public void Run_Cancelled_ExitsWith130()
	{
		using var cts = new CancellationTokenSource();
		cts.Cancel();

		var (code, error) = this._game.Run(cts.Token, "files", "--count", "--out", this._game.OutPath("n.txt"));

		code.Should().Be(130);
		error.Should().Contain("tqdb: cancelled.");
	}

	[Fact]
	public void Run_CorruptDatabase_ExitsWith1AndNamesTheError()
	{
		File.WriteAllBytes(Path.Combine(this._game.Root, "Database", "database.arz"), [1, 2, 3]);

		var (code, error) = this._game.Run("record", FakeGameInstall.SwordA);

		code.Should().Be(1);
		error.Should().StartWith("tqdb: ").And.Contain("Exception: ");
	}
}
