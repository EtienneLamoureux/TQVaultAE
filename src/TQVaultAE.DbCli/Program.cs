//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Reflection;
using TQVaultAE.DbCli.Commands;

namespace TQVaultAE.DbCli;

/// <summary>tqdb entry point.</summary>
internal static class Program
{
	private const string Tool = "tqdb";

	/// <summary>Options accepted by every command.</summary>
	internal static readonly OptionSpec[] GlobalOptions = [
		new("game", 'g', "path", "Game install folder (default: TQ_GAME_PATH, Steam, GOG, well-known folders)."),
		new("quiet", 'q', null, "No progress or informational messages on stderr."),
		new("threads", null, "N", "Worker threads for decoding and archive I/O (default: CPU count)."),
		new("version", null, null, "Print the version."),
	];

	/// <summary>All commands, in help order.</summary>
	internal static ICommand[] CreateCommands() => [
		new InfoCommand(),
		new RecordsCommand(),
		new RecordCommand(),
		new TextCommand(),
		new FilesCommand(),
		new ExtractCommand(),
	];

	private static int Main(string[] args) {
		using var cts = new CancellationTokenSource();
		Console.CancelKeyPress += (_, e) => {
			if (cts.IsCancellationRequested)
				return; // second Ctrl+C: let the process die
			e.Cancel = true;
			cts.Cancel();
		};
		return Run(args, Console.Error, cts.Token);
	}

	/// <summary>Parses and runs; returns the exit code.</summary>
	internal static int Run(string[] args, TextWriter error, CancellationToken cancellationToken) {
		var commands = CreateCommands();
		var specs = commands.Select(c => c.Spec).Append(HelpSpec).ToArray();

		ParsedArgs parsed;
		try {
			parsed = ArgParser.Parse(args, specs, GlobalOptions);
		}
		catch (UsageException ex) {
			error.WriteLine($"{Tool}: {ex.Message}");
			error.WriteLine(ex.Command is null ? $"Run '{Tool} --help' for usage." : $"Run '{Tool} {ex.Command} --help' for usage.");
			return ExitCodes.Usage;
		}

		if (parsed.Has("version")) {
			Console.Out.WriteLine($"{Tool} {Assembly.GetExecutingAssembly().GetName().Version}");
			return ExitCodes.Ok;
		}

		if (parsed.Command is null || parsed.Command == HelpSpec.Name) {
			var topic = parsed.Positionals.Count > 0 ? commands.FirstOrDefault(c => string.Equals(c.Spec.Name, parsed.Positionals[0], StringComparison.OrdinalIgnoreCase)) : null;
			if (topic is null && parsed.Positionals.Count > 0) {
				error.WriteLine($"{Tool}: Unknown command '{parsed.Positionals[0]}'.{ArgParser.Suggest(parsed.Positionals[0], commands.Select(c => c.Spec.Name))}");
				error.WriteLine($"Run '{Tool} --help' for usage.");
				return ExitCodes.Usage;
			}
			Console.Out.Write(topic is null ? ArgParser.Overview(Tool, specs, GlobalOptions) : ArgParser.Help(Tool, topic.Spec, GlobalOptions));
			return parsed.Command is null && !parsed.HelpRequested ? ExitCodes.Usage : ExitCodes.Ok;
		}

		var command = commands.First(c => c.Spec.Name == parsed.Command);
		if (parsed.HelpRequested) {
			Console.Out.Write(ArgParser.Help(Tool, command.Spec, GlobalOptions));
			return ExitCodes.Ok;
		}

		try {
			using var context = new CliContext(parsed, error, cancellationToken);
			return command.Execute(parsed, context);
		}
		catch (UsageException ex) {
			error.WriteLine($"{Tool} {command.Spec.Name}: {ex.Message}");
			error.WriteLine($"Run '{Tool} {command.Spec.Name} --help' for usage.");
			return ExitCodes.Usage;
		}
		catch (NotFoundException ex) {
			error.WriteLine($"{Tool}: {ex.Message}");
			return ExitCodes.NotFound;
		}
		catch (OperationCanceledException) {
			error.WriteLine($"{Tool}: cancelled.");
			return ExitCodes.Cancelled;
		}
		catch (AggregateException ex) when (ex.InnerExceptions.All(e => e is OperationCanceledException)) {
			error.WriteLine($"{Tool}: cancelled.");
			return ExitCodes.Cancelled;
		}
		catch (IOException ex) when (IsBrokenPipe(ex)) {
			return ExitCodes.Ok; // e.g. '| head': the reader went away
		}
		catch (Exception ex) {
			error.WriteLine($"{Tool}: {ex.GetType().Name}: {ex.Message}");
			if (Environment.GetEnvironmentVariable("TQDB_DEBUG") is not null)
				error.WriteLine(ex);
			return ExitCodes.Failure;
		}
	}

	/// <summary>Pseudo command for <c>tqdb help [command]</c>.</summary>
	private static readonly CommandSpec HelpSpec = new("help", "Show help of a command.", [], [new PositionalSpec("command", "Command name.", Required: false)], ["tqdb help records"]);

	private static bool IsBrokenPipe(IOException ex) {
		const int ErrorBrokenPipe = unchecked((int)0x8007006D);
		const int ErrorNoData = unchecked((int)0x800700E8);
		return ex.HResult is ErrorBrokenPipe or ErrorNoData or 32;
	}
}
