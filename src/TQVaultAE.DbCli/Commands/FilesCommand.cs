//-----------------------------------------------------------------------
// <copyright file="FilesCommand.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using TQVaultAE.DbCli.Core;
using TQVaultAE.DbCli.Export;
using TQVaultAE.DbCli.Query;

namespace TQVaultAE.DbCli.Commands;

/// <summary><c>tqdb files</c>: list archive entries.</summary>
internal sealed class FilesCommand : ICommand
{
	/// <summary>Selector options shared with <c>extract</c>.</summary>
	internal static readonly OptionSpec[] SelectorOptions = [
		new("archive", 'a', "glob|/re/", "Archive: key (Resources/XPack/Items), file name (Items.arc) or stem (Items).", Repeatable: true),
		new("path", 'p', "glob|/re/", "Entry name inside the archive ('/' or '\\').", Repeatable: true),
		new("resource", null, "glob|/re/", "Resource id as the database references it, e.g. Items\\weapons\\*.tex.", Repeatable: true),
		new("ext", 'e', "list", "Extensions, e.g. tex,mp3,msh.", Repeatable: true),
	];

	/// <inheritdoc/>
	public CommandSpec Spec { get; } = new(
		"files",
		"List entries of the game's .arc archives.",
		[
			.. SelectorOptions,
			new("limit", null, "N", "Maximum number of entries."),
			CliContext.CountOption,
			new("format", null, "fmt", "json | jsonl | tsv (default tsv, or from the --out extension)."),
			new("out", 'o', "path", "Output file, default stdout."),
		],
		[],
		[
			"tqdb files --archive Items --ext tex --limit 20",
			"tqdb files --resource \"Items\\weapons\\*\" --format json",
			"tqdb files --archive \"Audio/*\" --ext mp3,wav --count",
		]) {
		Description = "Columns: archive (path relative to the game root, without extension), name, resourceId, size, compressed, storage (1 stored, 3 zlib), type, lastWrite.",
	};

	/// <summary>Builds the selection from selector options.</summary>
	internal static ArchiveSelection BuildSelection(ParsedArgs args) {
		var sel = new ArchiveSelection();
		foreach (var v in args.GetAll("archive"))
			sel.Archives.Add(Pattern.Parse(v, pathLike: true));
		foreach (var v in args.GetAll("path"))
			sel.Paths.Add(Pattern.Parse(v, pathLike: true));
		foreach (var v in args.GetAll("resource"))
			sel.Resources.Add(Pattern.Parse(v, pathLike: true));
		sel.AddExtensions(args.GetList("ext"));
		return sel;
	}

	/// <summary>Selected archives opened in parallel, with their keys, in install order.</summary>
	internal static List<(string Key, IArcArchive Archive)> OpenSelected(CliContext context, ArchiveSelection selection) {
		var install = context.Install;
		var paths = new List<string>();
		foreach (var p in install.Archives)
			if (selection.MatchesArchive(context.ArchiveKey(p), p))
				paths.Add(p);
		var result = new List<(string, IArcArchive)>(paths.Count);
		foreach (var a in context.OpenArchives(paths))
			result.Add((context.ArchiveKey(a.Path), a));
		return result;
	}

	/// <inheritdoc/>
	public int Execute(ParsedArgs args, CliContext context) {
		var selection = BuildSelection(args);
		int limit = args.GetInt("limit", 0, 0);
		var outPath = args.Get("out");
		var format = Formats.Resolve(args.Get("format"), outPath, OutputFormat.Tsv, OutputFormat.Json, OutputFormat.Jsonl, OutputFormat.Tsv);
		var archives = OpenSelected(context, selection);

		if (args.Has("count")) {
			long count = 0;
			foreach (var (_, a) in archives)
				foreach (var e in a.Entries)
					if (selection.MatchesEntry(e))
						count++;
			CliContext.WriteCount(outPath, count, limit);
			return ExitCodes.Ok;
		}

		using var stream = CliContext.OpenOutput(outPath);
		using var sink = new TableSink(stream, format, "archive", "name", "resourceId", "size", "compressed", "storage", "type", "lastWrite");
		foreach (var (key, a) in archives) {
			foreach (var e in a.Entries) {
				if (!selection.MatchesEntry(e))
					continue;
				if (limit > 0 && sink.Written >= limit)
					break;
				sink.Row(key, e.Name, e.ResourceId, e.Size, e.CompressedSize, e.StorageType, ArchiveSelection.TypeOf(e.Name), e.LastWrite);
			}
			if (limit > 0 && sink.Written >= limit)
				break;
		}
		sink.Complete();
		context.Info($"{sink.Written} entr{(sink.Written == 1 ? "y" : "ies")} in {archives.Count} archive(s).");
		return ExitCodes.Ok;
	}
}
