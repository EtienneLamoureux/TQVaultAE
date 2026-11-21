//-----------------------------------------------------------------------
// <copyright file="RecordView.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using TQVaultAE.DbCli.Commands;
using TQVaultAE.DbCli.Core;

namespace TQVaultAE.DbCli.Query;

/// <summary>A record as handed to writers: table entry, optional decoded record, computed names, followed links.</summary>
internal sealed class RecordView(ArzRecordInfo info, ArzRecord? record)
{
	/// <summary>Table entry.</summary>
	public ArzRecordInfo Info { get; } = info;

	/// <summary>Decoded record, <see langword="null"/> when only table columns were needed.</summary>
	public ArzRecord? Record { get; } = record;

	/// <summary>Resolved display name (when projected).</summary>
	public string? DisplayName { get; set; }

	/// <summary>Name tag (when projected).</summary>
	public string? NameTag { get; set; }

	/// <summary>Records inlined by <c>--follow</c>, keyed by the referencing field.</summary>
	public List<(string Field, List<RecordView> Records)>? Linked { get; set; }

	/// <summary>Value of a pseudo column.</summary>
	public string? PseudoValue(PseudoColumn c) => c switch {
		PseudoColumn.Id => this.Info.Id,
		PseudoColumn.Class => this.Info.Class,
		PseudoColumn.DisplayName => this.DisplayName,
		PseudoColumn.NameTag => this.NameTag,
		_ => null,
	};
}

/// <summary><c>--sort field[:desc]</c>: numeric when both keys are numbers, else ordinal case-insensitive; missing keys last.</summary>
internal sealed class SortSpec : IComparer<RecordView>
{
	private SortSpec(string field, bool descending) {
		this.Field = field;
		this.Descending = descending;
		this.Pseudo = Projection.ParsePseudo(field);
	}

	/// <summary>Sort key.</summary>
	public string Field { get; }

	/// <summary>Descending order.</summary>
	public bool Descending { get; }

	/// <summary>Key is a pseudo column.</summary>
	public PseudoColumn Pseudo { get; }

	/// <summary>Key needs decoded records.</summary>
	public bool NeedsRecord => this.Pseudo is PseudoColumn.None or PseudoColumn.DisplayName or PseudoColumn.NameTag;

	/// <summary>Parses <c>field</c>, <c>field:asc</c> or <c>field:desc</c>.</summary>
	public static SortSpec Parse(string text) {
		var parts = text.Split(':', 2, StringSplitOptions.TrimEntries);
		if (parts[0].Length == 0)
			throw new UsageException($"--sort '{text}': missing field name.");
		bool desc = false;
		if (parts.Length == 2) {
			desc = parts[1].ToLowerInvariant() switch {
				"desc" or "d" => true,
				"asc" or "a" or "" => false,
				_ => throw new UsageException($"--sort '{text}': direction must be asc or desc."),
			};
		}
		return new SortSpec(parts[0], desc);
	}

	private object? Key(RecordView v) {
		if (this.Pseudo != PseudoColumn.None)
			return v.PseudoValue(this.Pseudo);
		var variable = v.Record?.Get(this.Field);
		return variable is { Values.Length: > 0 } ? variable.Values[0] : null;
	}

	/// <inheritdoc/>
	public int Compare(RecordView? x, RecordView? y) {
		if (x is null || y is null)
			return x is null ? (y is null ? 0 : 1) : -1;
		var a = this.Key(x);
		var b = this.Key(y);
		int cmp;
		if (a is null || b is null) {
			cmp = a is null ? (b is null ? 0 : 1) : -1; // missing last, whatever the direction
		}
		else {
			cmp = Values.TryNumber(a, out var na) && Values.TryNumber(b, out var nb)
				? na.CompareTo(nb)
				: string.Compare(Values.ToText(a), Values.ToText(b), StringComparison.OrdinalIgnoreCase);
			if (this.Descending)
				cmp = -cmp;
		}
		// Stable: table order breaks ties.
		return cmp != 0 ? cmp : x.Info.Index.CompareTo(y.Info.Index);
	}
}
