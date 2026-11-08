//-----------------------------------------------------------------------
// <copyright file="TextCommand.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using TQVaultAE.DbCli.Core;
using TQVaultAE.DbCli.Export;
using TQVaultAE.DbCli.Query;

namespace TQVaultAE.DbCli.Commands;

/// <summary><c>tqdb text</c>: search localization tags (tag ⇄ text).</summary>
internal sealed class TextCommand : ICommand
{
	/// <inheritdoc/>
	public CommandSpec Spec { get; } = new(
		"text",
		"Search localization tags (tag code <-> display text).",
		[
			new("key", 'k', "glob|/re/", "Tag name, e.g. tagItem*.", Repeatable: true),
			new("value", 'v', "glob|/re/", "Text, e.g. \"*hooked*\".", Repeatable: true),
			new("lang", 'l', "code", "Language (default EN)."),
			new("limit", null, "N", "Maximum number of rows."),
			new("count", null, null, "Print only the number of matches."),
			new("format", null, "fmt", "json | jsonl | tsv (default tsv, or from the --out extension)."),
			new("out", 'o', "path", "Output file, default stdout."),
		],
		[],
		[
			"tqdb text --key tagItemHookedBlade",
			"tqdb text --value \"*of the Hunt*\" --lang FR",
			"tqdb text --key \"/^tagSkill/\" --format json --out skills.json",
		]) {
		Description = "Rows are sorted by tag. Repeated --key or --value patterns are OR-ed; --key and --value together are AND-ed.",
	};

	/// <summary>Matching (tag, text) pairs sorted by tag.</summary>
	internal static List<KeyValuePair<string, string>> Search(ITextDatabase text, IReadOnlyList<Pattern> keys, IReadOnlyList<Pattern> values) {
		var hits = new List<KeyValuePair<string, string>>();
		foreach (var kv in text.Entries) {
			if (keys.Count > 0 && !AnyMatch(keys, kv.Key))
				continue;
			if (values.Count > 0 && !AnyMatch(values, kv.Value))
				continue;
			hits.Add(kv);
		}
		hits.Sort(static (a, b) => string.Compare(a.Key, b.Key, StringComparison.OrdinalIgnoreCase));
		return hits;
	}

	/// <summary>The tag as stored in the text files (the lookup ignores case).</summary>
	private static string GameSpelling(ITextDatabase text, string tag) {
		foreach (var key in text.Entries.Keys)
			if (string.Equals(key, tag, StringComparison.OrdinalIgnoreCase))
				return key;
		return tag;
	}

	private static bool AnyMatch(IReadOnlyList<Pattern> patterns, string value) {
		foreach (var p in patterns)
			if (p.IsMatch(value))
				return true;
		return false;
	}

	/// <inheritdoc/>
	public int Execute(ParsedArgs args, CliContext context) {
		var keys = args.GetAll("key").Select(k => Pattern.Parse(k)).ToList();
		var values = args.GetAll("value").Select(v => Pattern.Parse(v)).ToList();
		int limit = args.GetInt("limit", 0, 0);
		var outPath = args.Get("out");
		var format = Formats.Resolve(args.Get("format"), outPath, OutputFormat.Tsv, OutputFormat.Json, OutputFormat.Jsonl, OutputFormat.Tsv);
		var text = context.Text(args.Get("lang") ?? "EN");

		// Exact single key: direct lookup; the tag is written as the game spells it, not as typed.
		List<KeyValuePair<string, string>> hits;
		if (keys.Count == 1 && keys[0].Literal is { } literal && values.Count == 0)
			hits = text.Resolve(literal) is { } t ? [new(GameSpelling(text, literal), t)] : [];
		else
			hits = Search(text, keys, values);

		if (args.Has("count")) {
			using var w = CliContext.OpenTextOutput(outPath);
			w.WriteLine(limit > 0 ? Math.Min(limit, hits.Count) : hits.Count);
			return ExitCodes.Ok;
		}

		using var stream = CliContext.OpenOutput(outPath);
		using var sink = new TableSink(stream, format, "tag", "text");
		int n = 0;
		foreach (var kv in hits) {
			if (limit > 0 && n++ >= limit)
				break;
			sink.Row(kv.Key, kv.Value);
		}
		sink.Complete();
		context.Info($"{sink.Written} tag(s).");
		return ExitCodes.Ok;
	}
}
