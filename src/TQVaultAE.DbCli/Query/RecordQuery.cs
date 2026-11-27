//-----------------------------------------------------------------------
// <copyright file="RecordQuery.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using TQVaultAE.DbCli.Core;

namespace TQVaultAE.DbCli.Query;

/// <summary>
/// Record filters. Table filters (<see cref="Ids"/>, <see cref="Classes"/>) run on the record table before decompression;
/// value filters run on decoded records (on <see cref="IArzDatabase.ReadMany"/> worker threads).
/// Repeated id/class/template/name/tag patterns are OR-ed; different filter kinds and where clauses are AND-ed.
/// </summary>
internal sealed class RecordQuery
{
	/// <summary>Record path patterns (path-like).</summary>
	public List<Pattern> Ids { get; } = [];

	/// <summary>Class patterns.</summary>
	public List<Pattern> Classes { get; } = [];

	/// <summary>templateName patterns (path-like).</summary>
	public List<Pattern> Templates { get; } = [];

	/// <summary>Display name patterns.</summary>
	public List<Pattern> Names { get; } = [];

	/// <summary>Name tag patterns.</summary>
	public List<Pattern> Tags { get; } = [];

	/// <summary>Field predicates.</summary>
	public List<WhereClause> Where { get; } = [];

	/// <summary>Case-insensitive substring searched in every string value.</summary>
	public string? Text { get; set; }

	/// <summary>Some filter needs decoded records.</summary>
	public bool HasValueFilters => this.Templates.Count > 0 || this.Names.Count > 0 || this.Tags.Count > 0 || this.Where.Count > 0 || !string.IsNullOrEmpty(this.Text);

	/// <summary>Some filter needs a text database (display names).</summary>
	public bool NeedsText => this.Names.Count > 0 || this.Where.Exists(w => w.UsesDisplayName);

	/// <summary>Literal record id when the query is a single exact id (enables an indexed lookup).</summary>
	public string? SingleLiteralId => this.Ids.Count == 1 && this.Ids[0].Literal is { } l ? l : null;

	/// <summary>Record table filter (no decompression).</summary>
	public bool MatchesInfo(ArzRecordInfo info) {
		if (this.Classes.Count > 0 && !AnyMatch(this.Classes, info.Class))
			return false;
		if (this.Ids.Count > 0) {
			var id = info.Id.Replace('\\', '/');
			bool ok = false;
			foreach (var p in this.Ids) {
				if (p.IsMatchNormalized(id)) {
					ok = true;
					break;
				}
			}
			if (!ok)
				return false;
		}
		return true;
	}

	/// <summary>Value filter on a decoded record.</summary>
	public bool MatchesRecord(ArzRecord record, IRecordNamer? namer) {
		if (this.Templates.Count > 0 && !AnyMatch(this.Templates, record.Get("templateName")?.Values is [string t, ..] ? t : null))
			return false;
		if (this.Tags.Count > 0 && (namer is null || !AnyMatch(this.Tags, namer.NameTag(record))))
			return false;
		foreach (var w in this.Where)
			if (!w.Evaluate(record, namer))
				return false;
		if (!string.IsNullOrEmpty(this.Text) && !ContainsText(record, this.Text))
			return false;
		// Display name last: it is the most expensive check.
		if (this.Names.Count > 0 && (namer is null || !AnyMatch(this.Names, namer.DisplayName(record))))
			return false;
		return true;
	}

	/// <summary>Record table entries passing the table filters, in table order.</summary>
	public IEnumerable<ArzRecordInfo> Prefilter(IArzDatabase db) {
		if (this.SingleLiteralId is { } literal) {
			var info = db.Find(literal);
			if (info is not null && this.MatchesInfo(info))
				yield return info;
			yield break;
		}

		var records = db.Records;
		if (this.Ids.Count == 0 && this.Classes.Count == 0) {
			for (int i = 0; i < records.Count; i++)
				yield return records[i];
			yield break;
		}

		for (int i = 0; i < records.Count; i++)
			if (this.MatchesInfo(records[i]))
				yield return records[i];
	}

	private static bool AnyMatch(List<Pattern> patterns, string? value) {
		if (value is null)
			return false;
		foreach (var p in patterns)
			if (p.IsMatch(value))
				return true;
		return false;
	}

	private static bool ContainsText(ArzRecord record, string text) {
		foreach (var v in record.Variables) {
			if (v.Type != ArzValueType.String)
				continue;
			foreach (var o in v.Values)
				if (o is string s && s.Contains(text, StringComparison.OrdinalIgnoreCase))
					return true;
		}
		return false;
	}
}
