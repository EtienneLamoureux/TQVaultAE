//-----------------------------------------------------------------------
// <copyright file="RecordSearch.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using TQVaultAE.DbCli.Core;

namespace TQVaultAE.DbCli.Query;

/// <summary>What a search must compute.</summary>
/// <param name="Decode">Records must be decompressed (value filters, projected variables, dbr output...).</param>
/// <param name="DisplayName">Compute display names for output.</param>
/// <param name="NameTag">Compute name tags for output.</param>
/// <param name="Sort">Optional ordering (buffers the matches).</param>
/// <param name="Limit">Maximum results, 0 = unlimited.</param>
/// <param name="Follow">Fields whose .dbr references are inlined one level.</param>
internal sealed record SearchPlan(bool Decode, bool DisplayName = false, bool NameTag = false, SortSpec? Sort = null, int Limit = 0, IReadOnlyList<string>? Follow = null);

/// <summary>Runs a <see cref="RecordQuery"/>: table prefilter, parallel decode + value filter, names, sort, limit, follow.</summary>
internal static class RecordSearch
{
	/// <summary>Builds the plan for a query, projection and options.</summary>
	public static SearchPlan Plan(RecordQuery query, Projection projection, bool fullRecords, SortSpec? sort, int limit, IReadOnlyList<string>? follow) {
		bool decode = fullRecords || query.HasValueFilters || projection.NeedsRecord || (sort?.NeedsRecord ?? false) || follow is { Count: > 0 };
		return new SearchPlan(
			decode,
			projection.Pseudo.Contains(PseudoColumn.DisplayName) || sort?.Pseudo == PseudoColumn.DisplayName,
			projection.Pseudo.Contains(PseudoColumn.NameTag) || sort?.Pseudo == PseudoColumn.NameTag,
			sort,
			limit,
			follow);
	}

	/// <summary>Counts matches; decodes only when value filters exist.</summary>
	public static int Count(IArzDatabase db, RecordQuery query, IRecordNamer? namer, int maxDegreeOfParallelism, CancellationToken cancellationToken) {
		int count = 0;
		if (!query.HasValueFilters) {
			foreach (var _ in query.Prefilter(db))
				count++;
			return count;
		}
		foreach (var _ in db.ReadMany(query.Prefilter(db), r => query.MatchesRecord(r, namer), ordered: false, maxDegreeOfParallelism, cancellationToken))
			count++;
		return count;
	}

	/// <summary>Streams matching records (buffered only when sorting).</summary>
	public static IEnumerable<RecordView> Run(IArzDatabase db, RecordQuery query, SearchPlan plan, IRecordNamer? namer, int maxDegreeOfParallelism, CancellationToken cancellationToken) {
		var views = Enumerate(db, query, plan, namer, maxDegreeOfParallelism, cancellationToken);
		if (plan.Sort is not null) {
			var all = new List<RecordView>();
			foreach (var v in views)
				all.Add(v);
			all.Sort(plan.Sort);
			views = all;
		}

		int n = 0;
		foreach (var v in views) {
			if (plan.Limit > 0 && n >= plan.Limit)
				yield break;
			n++;
			if (plan.Follow is { Count: > 0 })
				Follow(db, v, plan.Follow, plan, namer);
			yield return v;
		}
	}

	private static IEnumerable<RecordView> Enumerate(IArzDatabase db, RecordQuery query, SearchPlan plan, IRecordNamer? namer, int dop, CancellationToken ct) {
		if (!plan.Decode) {
			foreach (var info in query.Prefilter(db))
				yield return new RecordView(info, null);
			yield break;
		}

		Func<ArzRecord, bool>? predicate = query.HasValueFilters ? r => query.MatchesRecord(r, namer) : null;
		foreach (var record in db.ReadMany(query.Prefilter(db), predicate, ordered: true, dop, ct))
			yield return Decorate(new RecordView(record.Info, record), plan, namer);
	}

	private static RecordView Decorate(RecordView view, SearchPlan plan, IRecordNamer? namer) {
		if (namer is not null && view.Record is not null) {
			if (plan.DisplayName)
				view.DisplayName = namer.DisplayName(view.Record);
			if (plan.NameTag)
				view.NameTag = namer.NameTag(view.Record);
		}
		return view;
	}

	/// <summary>Inlines records referenced by <paramref name="fields"/> (globs allowed) one level deep; unknown references are skipped.</summary>
	public static void Follow(IArzDatabase db, RecordView view, IReadOnlyList<string> fields, SearchPlan plan, IRecordNamer? namer) {
		if (view.Record is null)
			return;
		var patterns = new List<Pattern>(fields.Count);
		foreach (var f in fields)
			patterns.Add(Pattern.Parse(f));

		foreach (var v in view.Record.Variables) {
			if (v.Type != ArzValueType.String || !MatchesAny(patterns, v.Name))
				continue;
			List<RecordView>? linked = null;
			foreach (var value in v.Values) {
				if (!Values.IsRecordReference(value))
					continue;
				var info = db.Find((string)value);
				if (info is null)
					continue;
				linked ??= [];
				linked.Add(Decorate(new RecordView(info, db.Read(info)), plan with { DisplayName = true, NameTag = false }, namer));
			}
			if (linked is not null)
				(view.Linked ??= []).Add((v.Name, linked));
		}
	}

	private static bool MatchesAny(List<Pattern> patterns, string name) {
		foreach (var p in patterns)
			if (p.IsMatch(name))
				return true;
		return false;
	}
}
