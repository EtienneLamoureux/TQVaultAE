//-----------------------------------------------------------------------
// <copyright file="RecordCommand.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using TQVaultAE.DbCli.Export;
using TQVaultAE.DbCli.Query;

namespace TQVaultAE.DbCli.Commands;

/// <summary><c>tqdb record &lt;id&gt;</c>: one record, every field.</summary>
internal sealed class RecordCommand : ICommand
{
	/// <inheritdoc/>
	public CommandSpec Spec { get; } = new(
		"record",
		"Show one record with all its fields.",
		[
			new("format", null, "fmt", "json | jsonl | tsv | dbr (default json, or from the --out extension)."),
			new("fields", 'f', "list", "Variables to show (names or globs), default all.", Repeatable: true),
			new("resolve", 'r', null, "Add resolved text for tag-valued string fields."),
			new("follow", null, "field", "Inline records referenced by this field (glob allowed, e.g. '*'), one level.", Repeatable: true),
			new("lang", 'l', "code", "Language (default EN)."),
			new("compact", null, null, "Non-indented JSON."),
			new("out", 'o', "path", "Output file, default stdout."),
		],
		[new PositionalSpec("id", "Record path, '/' or '\\' separators, case-insensitive.", Required: true, Variadic: true)],
		[
			"tqdb record records/item/equipmentweapon/sword/c10_sword01.dbr",
			"tqdb record records\\xpack\\items\\loottables\\arms\\mixed_arms_n01.dbr --format dbr",
			"tqdb record <id> --follow \"lootName*\" --resolve",
		]);

	/// <inheritdoc/>
	public int Execute(ParsedArgs args, CliContext context) {
		var outPath = args.Get("out");
		var format = Formats.Resolve(args.Get("format"), outPath, OutputFormat.Json, OutputFormat.Json, OutputFormat.Jsonl, OutputFormat.Tsv, OutputFormat.Dbr);
		var lang = args.Get("lang") ?? "EN";
		var fields = args.GetList("fields");
		var follow = args.GetList("follow");
		bool resolve = args.Has("resolve");

		var items = new List<string> { "id", "class", "displayName" };
		if (fields.Count == 0)
			items.Add("*");
		else
			items.AddRange(fields);
		var projection = Projection.Parse(items);

		var db = context.Database;
		var infos = new List<ArzRecordInfo>();
		foreach (var id in args.Positionals) {
			var info = db.Find(id) ?? throw new NotFoundException($"Record '{id}' not found.{SuggestRecord(db, id)}");
			infos.Add(info);
		}

		// Display names are best-effort here: a missing text archive must not hide the record.
		IRecordNamer? namer = null;
		try {
			namer = context.Namer(lang);
		}
		catch (NotFoundException ex) {
			context.Warn(ex.Message);
		}

		var plan = new SearchPlan(Decode: true, DisplayName: namer is not null, Follow: follow);
		using var output = RecordOutput.Open(format, outPath, new RecordSinkOptions(projection, resolve ? namer : null), multipleRecords: infos.Count > 1 || follow.Count > 0, indented: !args.Has("compact"), singleObject: infos.Count == 1);
		foreach (var info in infos) {
			var record = db.Read(info);
			var view = new RecordView(info, record) { DisplayName = namer?.DisplayName(record) };
			if (follow.Count > 0)
				RecordSearch.Follow(db, view, follow, plan, namer);
			output.Sink.Write(view);
		}
		output.Sink.Complete();
		return ExitCodes.Ok;
	}

	private static string SuggestRecord(IArzDatabase db, string id) {
		var file = Path.GetFileName(id.Replace('\\', '/'));
		if (file.Length == 0)
			return string.Empty;
		var hits = new List<string>(3);
		foreach (var r in db.Records) {
			if (r.Id.EndsWith(file, StringComparison.OrdinalIgnoreCase)) {
				hits.Add(r.Id);
				if (hits.Count == 3)
					break;
			}
		}
		return hits.Count == 0 ? string.Empty : " Did you mean: " + string.Join(", ", hits) + "?";
	}
}
