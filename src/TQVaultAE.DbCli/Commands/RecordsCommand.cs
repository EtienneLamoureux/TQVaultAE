//-----------------------------------------------------------------------
// <copyright file="RecordsCommand.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using TQVaultAE.DbCli.Export;
using TQVaultAE.DbCli.Query;

namespace TQVaultAE.DbCli.Commands;

/// <summary><c>tqdb records</c>: search records.</summary>
internal sealed class RecordsCommand : ICommand
{
	/// <summary>Filter options shared with other record commands.</summary>
	internal static readonly OptionSpec[] FilterOptions = [
		new("id", 'i', "glob|/re/", "Record path ('/' or '\\', case-insensitive).", Repeatable: true),
		new("class", 'c', "glob", "Record class (record table: filtered before decompression).", Repeatable: true),
		new("template", null, "glob|/re/", "templateName value.", Repeatable: true),
		new("name", 'n', "glob|/re/", "Resolved display name (see --lang).", Repeatable: true),
		new("tag", null, "glob|/re/", "Name tag itself, e.g. tagItemHookedBlade.", Repeatable: true),
		new("where", 'w', "expr", "Field predicate: f=v f!=v f>n f>=n f<n f<=n f~glob f? (exists, non-zero); '!' negates.", Repeatable: true),
		new("text", 't', "substring", "Case-insensitive substring in any string value."),
		new("lang", 'l', "code", "Language of display names and --resolve (default EN)."),
	];

	/// <inheritdoc/>
	public CommandSpec Spec { get; } = new(
		"records",
		"Search records of database.arz.",
		[
			.. FilterOptions,
			new("fields", 'f', "list", "Columns: id,class,displayName,nameTag, variable names or globs; * = all variables (default id,class,displayName).", Repeatable: true),
			new("resolve", 'r', null, "Add resolved text for tag-valued string fields."),
			new("follow", null, "field", "Inline records referenced by this field (glob allowed), one level.", Repeatable: true),
			new("sort", 's', "field[:desc]", "Sort by a column or variable."),
			new("limit", null, "N", "Maximum number of records."),
			CliContext.CountOption,
			new("format", null, "fmt", "json | jsonl | tsv | dbr (default tsv, or from the --out extension)."),
			new("compact", null, null, "Non-indented JSON."),
			new("out", 'o', "path", "Output file (dbr: directory, one .dbr per record), default stdout."),
		],
		[],
		[
			"tqdb records --class ItemRelic --fields id,displayName,completedRelicLevel",
			"tqdb records --id \"records/item/equipmentweapon/sword/*\" --where \"itemLevel>=40\" --sort itemLevel:desc --limit 20",
			"tqdb records --name \"*hooked*\" --fields \"*\" --format json",
			"tqdb records --where \"offensive*Min?\" --count",
			"tqdb records --class LootRandomizer --format dbr --out ./dbr",
		]) {
		Description = "Id and class filters run on the record table; other filters decode matching records in parallel.\n"
			+ "Repeated --id/--class/--template/--name/--tag are OR-ed; different filters and --where clauses are AND-ed.\n"
			+ "Globs are anchored (* ? [abc]); /regex/ is an unanchored, case-insensitive search.\n"
			+ "TSV with '*' or field globs switches to long format (one row per variable).",
	};

	/// <summary>Builds the query from filter options.</summary>
	internal static RecordQuery BuildQuery(ParsedArgs args) {
		var q = new RecordQuery();
		foreach (var v in args.GetAll("id"))
			q.Ids.Add(Pattern.Parse(v, pathLike: true));
		foreach (var v in args.GetAll("class"))
			q.Classes.Add(Pattern.Parse(v));
		foreach (var v in args.GetAll("template"))
			q.Templates.Add(Pattern.Parse(v, pathLike: true));
		foreach (var v in args.GetAll("name"))
			q.Names.Add(Pattern.Parse(v));
		foreach (var v in args.GetAll("tag"))
			q.Tags.Add(Pattern.Parse(v));
		foreach (var v in args.GetAll("where"))
			q.Where.Add(WhereClause.Parse(v));
		q.Text = args.Get("text");
		return q;
	}

	/// <inheritdoc/>
	public int Execute(ParsedArgs args, CliContext context) {
		var query = BuildQuery(args);
		var projection = Projection.Parse(args.GetList("fields"));
		var sort = args.Get("sort") is { } s ? SortSpec.Parse(s) : null;
		int limit = args.GetInt("limit", 0, 0);
		var follow = args.GetList("follow");
		var lang = args.Get("lang") ?? "EN";
		bool resolve = args.Has("resolve");
		var outPath = args.Get("out");
		var format = Formats.Resolve(args.Get("format"), outPath, OutputFormat.Tsv, OutputFormat.Json, OutputFormat.Jsonl, OutputFormat.Tsv, OutputFormat.Dbr);

		var db = context.Database;

		bool needsText = query.NeedsText || resolve || follow.Count > 0
			|| projection.Pseudo.Contains(PseudoColumn.DisplayName) || sort?.Pseudo == PseudoColumn.DisplayName;
		bool needsTags = query.Tags.Count > 0 || projection.Pseudo.Contains(PseudoColumn.NameTag) || sort?.Pseudo == PseudoColumn.NameTag;
		var namer = needsText || needsTags ? NamerOrTagOnly(context, lang, needsText) : null;

		if (args.Has("count")) {
			int count = RecordSearch.Count(db, query, namer, context.Threads, context.CancellationToken);
			CliContext.WriteCount(outPath, count, limit);
			return ExitCodes.Ok;
		}

		var plan = RecordSearch.Plan(query, projection, format == OutputFormat.Dbr, sort, limit, follow);
		using var output = RecordOutput.Open(format, outPath, new RecordSinkOptions(projection, resolve ? namer : null), multipleRecords: true, indented: !args.Has("compact"));
		foreach (var view in RecordSearch.Run(db, query, plan, namer, context.Threads, context.CancellationToken))
			output.Sink.Write(view);
		output.Sink.Complete();
		context.Info($"{output.Sink.Written} record(s).");
		return ExitCodes.Ok;
	}

	/// <summary>Display names need the text database; name tags alone do not.</summary>
	internal static IRecordNamer NamerOrTagOnly(CliContext context, string lang, bool needsText) => needsText ? context.Namer(lang) : TagOnlyNamer.Instance;

	/// <summary>Namer that only extracts name tags (no text database loaded).</summary>
	private sealed class TagOnlyNamer : IRecordNamer
	{
		public static readonly TagOnlyNamer Instance = new();

		public string? NameTag(ArzRecord record) => Core.DisplayNames.NameTag(record);

		public string? DisplayName(ArzRecord record) => null;

		public string? ResolveTag(string? tag) => null;
	}
}
