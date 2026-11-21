//-----------------------------------------------------------------------
// <copyright file="Projection.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using TQVaultAE.DbCli.Core;

namespace TQVaultAE.DbCli.Query;

/// <summary>Pseudo column (not a database variable).</summary>
internal enum PseudoColumn
{
	None,
	Id,
	Class,
	DisplayName,
	NameTag,
}

/// <summary>
/// Output column selection (<c>--fields</c>). Items: pseudo columns <c>id</c>, <c>class</c>, <c>displayName</c>, <c>nameTag</c>;
/// variable names (case-insensitive); globs over variable names; <c>*</c> for every variable.
/// </summary>
internal sealed class Projection
{
	/// <summary>Default columns: id, class, displayName.</summary>
	public static readonly IReadOnlyList<string> DefaultItems = ["id", "class", "displayName"];

	private readonly List<Pattern> _globs = [];

	private Projection(IReadOnlyList<string> items) {
		foreach (var raw in items) {
			var item = raw.Trim();
			if (item.Length == 0)
				continue;
			var pseudo = ParsePseudo(item);
			if (pseudo != PseudoColumn.None) {
				if (!this.Pseudo.Contains(pseudo))
					this.Pseudo.Add(pseudo);
				this.Columns.Add(new ProjectionColumn(PseudoName(pseudo), pseudo));
				continue;
			}
			if (item == "*") {
				this.All = true;
				continue;
			}
			if (Pattern.HasWildcards(item)) {
				this._globs.Add(Pattern.Parse(item));
				continue;
			}
			if (!this.Fields.Exists(f => string.Equals(f, item, StringComparison.OrdinalIgnoreCase))) {
				this.Fields.Add(item);
				this.Columns.Add(new ProjectionColumn(item, PseudoColumn.None));
			}
		}
	}

	/// <summary>Requested pseudo columns, in order.</summary>
	public List<PseudoColumn> Pseudo { get; } = [];

	/// <summary>Exact variable names, in order.</summary>
	public List<string> Fields { get; } = [];

	/// <summary>Fixed columns (pseudo + exact fields) in the user's order.</summary>
	public List<ProjectionColumn> Columns { get; } = [];

	/// <summary><c>*</c> was requested.</summary>
	public bool All { get; }

	/// <summary>Variable set depends on the record (<c>*</c> or globs): tabular output switches to long format.</summary>
	public bool IsDynamic => this.All || this._globs.Count > 0;

	/// <summary>Any database variable is requested (records must be decoded).</summary>
	public bool HasVariables => this.IsDynamic || this.Fields.Count > 0;

	/// <summary>Needs the record's decoded variables, beyond the record table (id, class).</summary>
	public bool NeedsRecord => this.HasVariables || this.Pseudo.Contains(PseudoColumn.DisplayName) || this.Pseudo.Contains(PseudoColumn.NameTag);

	/// <summary>Builds a projection; empty input means <see cref="DefaultItems"/>.</summary>
	public static Projection Parse(IReadOnlyList<string> items) => new(items.Count == 0 ? DefaultItems : items);

	/// <summary>Pseudo column name, or <see cref="PseudoColumn.None"/>.</summary>
	public static PseudoColumn ParsePseudo(string name) => name.ToLowerInvariant() switch {
		"id" => PseudoColumn.Id,
		"class" => PseudoColumn.Class,
		"displayname" => PseudoColumn.DisplayName,
		"nametag" => PseudoColumn.NameTag,
		_ => PseudoColumn.None,
	};

	/// <summary>Canonical output name of a pseudo column.</summary>
	public static string PseudoName(PseudoColumn c) => c switch {
		PseudoColumn.Id => "id",
		PseudoColumn.Class => "class",
		PseudoColumn.DisplayName => "displayName",
		PseudoColumn.NameTag => "nameTag",
		_ => string.Empty,
	};

	/// <summary>Projected variables of a record, in record order for dynamic projections, else in requested order (absent ones skipped).</summary>
	public void Select(ArzRecord record, List<ArzVariable> into) {
		into.Clear();
		if (this.IsDynamic) {
			foreach (var v in record.Variables)
				if (this.All || this.Contains(v.Name))
					into.Add(v);
			return;
		}
		foreach (var f in this.Fields) {
			var v = record.Get(f);
			if (v is not null)
				into.Add(v);
		}
	}

	private bool Contains(string name) {
		foreach (var f in this.Fields)
			if (string.Equals(f, name, StringComparison.OrdinalIgnoreCase))
				return true;
		foreach (var g in this._globs)
			if (g.IsMatch(name))
				return true;
		return false;
	}
}

/// <summary>One fixed output column.</summary>
internal sealed record ProjectionColumn(string Name, PseudoColumn Pseudo);
