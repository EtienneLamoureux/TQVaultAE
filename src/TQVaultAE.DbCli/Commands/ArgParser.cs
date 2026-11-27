//-----------------------------------------------------------------------
// <copyright file="ArgParser.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Globalization;
using System.Text;

namespace TQVaultAE.DbCli.Commands;

/// <summary>Command-line usage error (unknown option, missing value...). Maps to exit code 2.</summary>
internal sealed class UsageException(string message, string? command = null) : Exception(message)
{
	/// <summary>Command the error belongs to, used to point at its help.</summary>
	public string? Command { get; } = command;
}

/// <summary>Declares one option.</summary>
/// <param name="Long">Long name without dashes, e.g. <c>class</c>.</param>
/// <param name="Short">Optional one-letter alias.</param>
/// <param name="ValueName">Value placeholder for help; <see langword="null"/> for a flag.</param>
/// <param name="Help">One-line help.</param>
/// <param name="Repeatable">Option may be given several times (values accumulate).</param>
internal sealed record OptionSpec(string Long, char? Short, string? ValueName, string Help, bool Repeatable = false)
{
	/// <summary>Option is a boolean flag (takes no value).</summary>
	public bool IsFlag => this.ValueName is null;
}

/// <summary>Declares one positional argument.</summary>
internal sealed record PositionalSpec(string Name, string Help, bool Required = true, bool Variadic = false);

/// <summary>Declares a sub-command.</summary>
internal sealed record CommandSpec(string Name, string Summary, IReadOnlyList<OptionSpec> Options, IReadOnlyList<PositionalSpec> Positionals, IReadOnlyList<string> Examples)
{
	/// <summary>Longer description printed in the command help.</summary>
	public string? Description { get; init; }
}

/// <summary>Parse result.</summary>
internal sealed class ParsedArgs
{
	private readonly Dictionary<string, List<string>> _values = new(StringComparer.Ordinal);
	private readonly List<string> _positionals = [];

	/// <summary>Selected command name, <see langword="null"/> when none was given.</summary>
	public string? Command { get; internal set; }

	/// <summary>-h/--help was given.</summary>
	public bool HelpRequested { get; internal set; }

	/// <summary>Positional arguments in order.</summary>
	public IReadOnlyList<string> Positionals => this._positionals;

	internal void Add(string name, string value) {
		if (!this._values.TryGetValue(name, out var list))
			this._values[name] = list = [];
		list.Add(value);
	}

	internal void AddPositional(string value) => this._positionals.Add(value);

	internal int CountOf(string name) => this._values.TryGetValue(name, out var l) ? l.Count : 0;

	/// <summary>Option or flag was given.</summary>
	public bool Has(string name) => this._values.ContainsKey(name);

	/// <summary>Last value of an option, <see langword="null"/> when absent.</summary>
	public string? Get(string name) => this._values.TryGetValue(name, out var l) ? l[^1] : null;

	/// <summary>All values of a repeatable option.</summary>
	public IReadOnlyList<string> GetAll(string name) => this._values.TryGetValue(name, out var l) ? l : [];

	/// <summary>All values, each split on ',' (for list options like <c>--fields a,b</c>), empty items dropped.</summary>
	public IReadOnlyList<string> GetList(string name) {
		var result = new List<string>();
		foreach (var v in this.GetAll(name))
			foreach (var part in v.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
				result.Add(part);
		return result;
	}

	/// <summary>Integer option with range check.</summary>
	public int GetInt(string name, int defaultValue, int min = int.MinValue, int max = int.MaxValue) {
		var v = this.Get(name);
		if (v is null)
			return defaultValue;
		if (!int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) || n < min || n > max)
			throw new UsageException($"--{name}: '{v}' is not an integer in [{min}..{max}].", this.Command);
		return n;
	}

	/// <summary>Option restricted to a set of choices (case-insensitive); returns the canonical lower-case choice.</summary>
	public string GetChoice(string name, string defaultValue, params string[] choices) {
		var v = this.Get(name);
		if (v is null)
			return defaultValue;
		foreach (var c in choices)
			if (string.Equals(c, v, StringComparison.OrdinalIgnoreCase))
				return c;
		throw new UsageException($"--{name}: '{v}' is not one of {string.Join(", ", choices)}.", this.Command);
	}
}

/// <summary>Small allocation-light argument parser: sub-command, --long/-s options, --opt=value, repeated options, '--' terminator.</summary>
internal static class ArgParser
{
	/// <summary>Built-in help option, accepted everywhere.</summary>
	public static readonly OptionSpec HelpOption = new("help", 'h', null, "Show help.");

	/// <summary>Parses <paramref name="args"/>. Global options are accepted before and after the command name.</summary>
	public static ParsedArgs Parse(IReadOnlyList<string> args, IReadOnlyList<CommandSpec> commands, IReadOnlyList<OptionSpec> globals) {
		var result = new ParsedArgs();
		CommandSpec? command = null;
		bool optionsEnded = false;

		for (int i = 0; i < args.Count; i++) {
			var arg = args[i];

			if (optionsEnded || arg.Length < 2 || arg[0] != '-' || IsNegativeNumber(arg)) {
				if (command is null && !optionsEnded) {
					command = FindCommand(commands, arg) ?? throw new UsageException($"Unknown command '{arg}'.{Suggest(arg, commands.Select(c => c.Name))}");
					result.Command = command.Name;
				}
				else {
					if (command is null)
						throw new UsageException($"Unexpected argument '{arg}' before the command name.");
					result.AddPositional(arg);
				}
				continue;
			}

			if (arg == "--") {
				optionsEnded = true;
				continue;
			}

			if (arg[1] == '-') {
				// --name or --name=value
				var body = arg.AsSpan(2);
				int eq = body.IndexOf('=');
				var name = (eq < 0 ? body : body[..eq]).ToString();
				string? inline = eq < 0 ? null : body[(eq + 1)..].ToString();
				var spec = FindOption(name, command, globals) ?? throw UnknownOption("--" + name, command, globals);
				i = Consume(spec, inline, args, i, result, command);
			}
			else {
				// -x, -xVALUE, -abc (flags)
				for (int j = 1; j < arg.Length; j++) {
					var spec = FindShort(arg[j], command, globals) ?? throw UnknownOption("-" + arg[j], command, globals);
					if (spec.IsFlag) {
						i = Consume(spec, null, args, i, result, command);
						continue;
					}
					string? inline = j + 1 < arg.Length ? arg[(j + 1)..].TrimStart('=') : null;
					i = Consume(spec, inline, args, i, result, command);
					break;
				}
			}
		}

		if (command is not null && !result.HelpRequested)
			ValidatePositionals(command, result);
		return result;
	}

	private static int Consume(OptionSpec spec, string? inline, IReadOnlyList<string> args, int i, ParsedArgs result, CommandSpec? command) {
		if (ReferenceEquals(spec, HelpOption)) {
			result.HelpRequested = true;
			return i;
		}

		string value;
		if (spec.IsFlag) {
			if (inline is not null)
				throw new UsageException($"Option --{spec.Long} does not take a value.", command?.Name);
			value = "true";
		}
		else if (inline is not null) {
			value = inline;
		}
		else {
			if (i + 1 >= args.Count)
				throw new UsageException($"Option --{spec.Long} requires a value <{spec.ValueName}>.", command?.Name);
			value = args[++i];
		}

		if (!spec.Repeatable && result.CountOf(spec.Long) > 0)
			throw new UsageException($"Option --{spec.Long} given more than once.", command?.Name);
		result.Add(spec.Long, value);
		return i;
	}

	private static void ValidatePositionals(CommandSpec command, ParsedArgs result) {
		int required = 0;
		bool variadic = false;
		foreach (var p in command.Positionals) {
			if (p.Required)
				required++;
			variadic |= p.Variadic;
		}

		if (result.Positionals.Count < required)
			throw new UsageException($"Missing argument <{command.Positionals[result.Positionals.Count].Name}>.", command.Name);
		if (!variadic && result.Positionals.Count > command.Positionals.Count)
			throw new UsageException($"Unexpected argument '{result.Positionals[command.Positionals.Count]}'.", command.Name);
	}

	private static bool IsNegativeNumber(string arg) => arg.Length > 1 && arg[0] == '-' && (char.IsAsciiDigit(arg[1]) || arg[1] == '.');

	private static CommandSpec? FindCommand(IReadOnlyList<CommandSpec> commands, string name) {
		foreach (var c in commands)
			if (string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase))
				return c;
		return null;
	}

	private static OptionSpec? FindOption(string name, CommandSpec? command, IReadOnlyList<OptionSpec> globals) {
		if (name == HelpOption.Long)
			return HelpOption;
		if (command is not null)
			foreach (var o in command.Options)
				if (o.Long == name)
					return o;
		foreach (var o in globals)
			if (o.Long == name)
				return o;
		return null;
	}

	private static OptionSpec? FindShort(char c, CommandSpec? command, IReadOnlyList<OptionSpec> globals) {
		if (c == HelpOption.Short)
			return HelpOption;
		if (command is not null)
			foreach (var o in command.Options)
				if (o.Short == c)
					return o;
		foreach (var o in globals)
			if (o.Short == c)
				return o;
		return null;
	}

	private static UsageException UnknownOption(string token, CommandSpec? command, IReadOnlyList<OptionSpec> globals) {
		var names = globals.Select(o => o.Long);
		if (command is not null)
			names = names.Concat(command.Options.Select(o => o.Long));
		var where = command is null ? string.Empty : $" for '{command.Name}'";
		return new UsageException($"Unknown option '{token}'{where}.{Suggest(token.TrimStart('-'), names, "--")}", command?.Name);
	}

	/// <summary>" Did you mean ...?" suffix for the closest candidate (edit distance ≤ 2), empty otherwise.</summary>
	internal static string Suggest(string input, IEnumerable<string> candidates, string prefix = "") {
		string? best = null;
		int bestDistance = int.MaxValue;
		foreach (var c in candidates) {
			int d = Distance(input.ToLowerInvariant(), c.ToLowerInvariant());
			if (d < bestDistance) {
				bestDistance = d;
				best = c;
			}
		}
		return best is not null && bestDistance <= 2 ? $" Did you mean '{prefix}{best}'?" : string.Empty;
	}

	private static int Distance(string a, string b) {
		var prev = new int[b.Length + 1];
		var cur = new int[b.Length + 1];
		for (int j = 0; j <= b.Length; j++)
			prev[j] = j;
		for (int i = 1; i <= a.Length; i++) {
			cur[0] = i;
			for (int j = 1; j <= b.Length; j++)
				cur[j] = Math.Min(Math.Min(cur[j - 1] + 1, prev[j] + 1), prev[j - 1] + (a[i - 1] == b[j - 1] ? 0 : 1));
			(prev, cur) = (cur, prev);
		}
		return prev[b.Length];
	}

	/// <summary>Top-level help.</summary>
	public static string Overview(string tool, IReadOnlyList<CommandSpec> commands, IReadOnlyList<OptionSpec> globals) {
		var sb = new StringBuilder();
		sb.AppendLine($"{tool} - search and export the Titan Quest AE database and archives");
		sb.AppendLine();
		sb.AppendLine($"Usage: {tool} <command> [options]");
		sb.AppendLine();
		sb.AppendLine("Commands:");
		int width = commands.Max(c => c.Name.Length) + 2;
		foreach (var c in commands)
			sb.Append("  ").Append(c.Name.PadRight(width)).AppendLine(c.Summary);
		sb.AppendLine();
		AppendOptions(sb, "Global options:", globals.Append(HelpOption).ToList());
		sb.AppendLine();
		sb.AppendLine($"Run '{tool} <command> --help' for command options.");
		return sb.ToString();
	}

	/// <summary>Help of one command.</summary>
	public static string Help(string tool, CommandSpec command, IReadOnlyList<OptionSpec> globals) {
		var sb = new StringBuilder();
		sb.Append("Usage: ").Append(tool).Append(' ').Append(command.Name);
		foreach (var p in command.Positionals)
			sb.Append(p.Required ? $" <{p.Name}>" : $" [{p.Name}]").Append(p.Variadic ? "..." : string.Empty);
		if (command.Options.Count > 0)
			sb.Append(" [options]");
		sb.AppendLine();
		sb.AppendLine();
		sb.AppendLine(command.Summary);
		if (command.Description is not null) {
			sb.AppendLine();
			sb.AppendLine(command.Description);
		}

		if (command.Positionals.Count > 0) {
			sb.AppendLine();
			sb.AppendLine("Arguments:");
			foreach (var p in command.Positionals)
				sb.Append("  ").Append(p.Name.PadRight(24)).AppendLine(p.Help);
		}

		if (command.Options.Count > 0) {
			sb.AppendLine();
			AppendOptions(sb, "Options:", command.Options);
		}

		sb.AppendLine();
		AppendOptions(sb, "Global options:", globals.Append(HelpOption).ToList());

		if (command.Examples.Count > 0) {
			sb.AppendLine();
			sb.AppendLine("Examples:");
			foreach (var e in command.Examples)
				sb.Append("  ").AppendLine(e);
		}
		return sb.ToString();
	}

	private static void AppendOptions(StringBuilder sb, string title, IReadOnlyList<OptionSpec> options) {
		sb.AppendLine(title);
		var heads = options.Select(o => (o.Short is null ? "    " : $"-{o.Short}, ") + "--" + o.Long + (o.IsFlag ? string.Empty : $" <{o.ValueName}>")).ToList();
		int width = Math.Max(26, heads.Max(h => h.Length) + 2);
		for (int i = 0; i < options.Count; i++) {
			sb.Append("  ").Append(heads[i].PadRight(width)).Append(options[i].Help);
			if (options[i].Repeatable)
				sb.Append(" (repeatable)");
			sb.AppendLine();
		}
	}
}
