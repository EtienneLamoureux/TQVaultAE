//-----------------------------------------------------------------------
// <copyright file="InfoCommand.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Text.Json;
using TQVaultAE.DbCli.Export;

namespace TQVaultAE.DbCli.Commands;

/// <summary><c>tqdb info</c>: install overview.</summary>
internal sealed class InfoCommand : ICommand
{
	/// <inheritdoc/>
	public CommandSpec Spec { get; } = new(
		"info",
		"Show the game install, database statistics, archives and text languages.",
		[
			new("top", null, "N", "Number of classes in the histogram (default 20, 0 = all)."),
			new("format", null, "fmt", "text | json (default text)."),
			new("out", 'o', "path", "Output file, default stdout."),
		],
		[],
		["tqdb info", "tqdb info --top 0 --format json"]);

	/// <summary>Class histogram sorted by count desc, then name.</summary>
	internal static List<KeyValuePair<string, int>> ClassHistogram(IReadOnlyList<ArzRecordInfo> records) {
		var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
		foreach (var r in records) {
			var key = r.Class.Length == 0 ? "(none)" : r.Class;
			counts[key] = counts.GetValueOrDefault(key) + 1;
		}
		var list = counts.ToList();
		list.Sort(static (a, b) => a.Value != b.Value ? b.Value.CompareTo(a.Value) : string.Compare(a.Key, b.Key, StringComparison.OrdinalIgnoreCase));
		return list;
	}

	/// <inheritdoc/>
	public int Execute(ParsedArgs args, CliContext context) {
		int top = args.GetInt("top", 20, 0);
		var outPath = args.Get("out");
		var format = Formats.Resolve(args.Get("format"), outPath, OutputFormat.Text, OutputFormat.Text, OutputFormat.Json);

		var install = context.Install;
		var db = context.Database;
		var histogram = ClassHistogram(db.Records);
		var shownClasses = top == 0 ? histogram : histogram.Take(top).ToList();

		var archives = context.OpenArchives(install.Archives);
		long archiveBytes = 0, entryCount = 0;
		foreach (var p in install.Archives)
			archiveBytes += new FileInfo(p).Length;
		foreach (var a in archives)
			entryCount += a.Entries.Count;

		var languages = new List<string>();
		var textDir = Path.Combine(install.Root, "Text");
		if (Directory.Exists(textDir)) {
			foreach (var f in Directory.EnumerateFiles(textDir, "Text_*.arc")) {
				var stem = Path.GetFileNameWithoutExtension(f);
				languages.Add(stem[(stem.IndexOf('_') + 1)..].ToUpperInvariant());
			}
			languages.Sort(StringComparer.Ordinal);
		}

		using var stream = CliContext.OpenOutput(outPath);
		if (format == OutputFormat.Json) {
			using var w = new Utf8JsonWriter(stream, Formats.JsonOptions(indented: true));
			w.WriteStartObject();
			w.WriteString("gameRoot", install.Root);
			w.WriteString("database", install.Database);
			w.WriteNumber("databaseSize", new FileInfo(install.Database).Length);
			w.WriteNumber("records", db.Records.Count);
			w.WriteNumber("classes", histogram.Count);
			w.WriteStartObject("classHistogram");
			foreach (var kv in shownClasses)
				w.WriteNumber(kv.Key, kv.Value);
			w.WriteEndObject();
			w.WriteNumber("archiveCount", install.Archives.Count);
			w.WriteNumber("archiveSize", archiveBytes);
			w.WriteNumber("archiveEntries", entryCount);
			w.WriteStartArray("archives");
			foreach (var a in archives) {
				w.WriteStartObject();
				w.WriteString("key", context.ArchiveKey(a.Path));
				w.WriteString("path", a.Path);
				w.WriteNumber("entries", a.Entries.Count);
				w.WriteNumber("size", new FileInfo(a.Path).Length);
				w.WriteEndObject();
			}
			w.WriteEndArray();
			w.WriteStartArray("languages");
			foreach (var l in languages)
				w.WriteStringValue(l);
			w.WriteEndArray();
			w.WriteEndObject();
			w.Flush();
			stream.WriteByte((byte)'\n');
			return ExitCodes.Ok;
		}

		using var t = new StreamWriter(stream, Formats.Utf8, Formats.BufferSize, leaveOpen: true);
		t.WriteLine($"Game root     {install.Root}");
		t.WriteLine($"Database      {install.Database} ({ExtractCommand.FormatBytes(new FileInfo(install.Database).Length)})");
		t.WriteLine($"Records       {db.Records.Count} in {histogram.Count} classes");
		t.WriteLine($"Archives      {install.Archives.Count} ({ExtractCommand.FormatBytes(archiveBytes)}, {entryCount} entries)");
		t.WriteLine($"Languages     {(languages.Count == 0 ? "(none)" : string.Join(' ', languages))}");
		t.WriteLine();
		t.WriteLine(top == 0 ? "Classes:" : $"Top {shownClasses.Count} classes:");
		int width = shownClasses.Count == 0 ? 0 : shownClasses.Max(kv => kv.Key.Length);
		foreach (var kv in shownClasses)
			t.WriteLine($"  {kv.Key.PadRight(width)}  {kv.Value,7}");
		t.Flush();
		return ExitCodes.Ok;
	}
}
