//-----------------------------------------------------------------------
// <copyright file="WhereClause.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using TQVaultAE.DbCli.Commands;
using TQVaultAE.DbCli.Core;

namespace TQVaultAE.DbCli.Query;

/// <summary>Comparison operator of a <see cref="WhereClause"/>.</summary>
internal enum WhereOp
{
	Eq,
	Ne,
	Gt,
	Ge,
	Lt,
	Le,
	Like,
	Exists,
}

/// <summary>
/// Field predicate: <c>field=v</c>, <c>field!=v</c>, <c>field&gt;n</c>, <c>field&gt;=n</c>, <c>field&lt;n</c>, <c>field&lt;=n</c>,
/// <c>field~glob</c> (or <c>/regex/</c>), <c>field?</c> (exists and non-zero); a leading <c>!</c> negates.
/// Field names are case-insensitive and may be globs (any matching variable). Multi-value fields match when any value matches
/// (<c>!=</c> is the negation of <c>=</c>: no value equals). Pseudo fields: <c>id</c>, <c>displayName</c>, <c>nameTag</c>.
/// </summary>
internal sealed class WhereClause
{
	private static readonly System.Buffers.SearchValues<char> OperatorChars = System.Buffers.SearchValues.Create("=!<>~?");
	private readonly Pattern? _fieldPattern;
	private readonly Pattern? _like;
	private readonly double _number;
	private readonly bool _isNumber;

	private WhereClause(string source, string field, WhereOp op, string value, bool negate) {
		this.Source = source;
		this.Field = field;
		this.Op = op;
		this.Value = value;
		this.Negate = negate;
		if (Pattern.HasWildcards(field))
			this._fieldPattern = Pattern.Parse(field);
		if (op == WhereOp.Like)
			this._like = Pattern.Parse(value, pathLike: true);
		this._isNumber = Values.TryParseNumber(value, out this._number);
		if (op is WhereOp.Gt or WhereOp.Ge or WhereOp.Lt or WhereOp.Le && value.Length == 0)
			throw new UsageException($"--where '{source}': missing value after the operator.");
	}

	/// <summary>Expression as given.</summary>
	public string Source { get; }

	/// <summary>Field name or glob.</summary>
	public string Field { get; }

	/// <summary>Operator.</summary>
	public WhereOp Op { get; }

	/// <summary>Right-hand side (empty for <see cref="WhereOp.Exists"/>).</summary>
	public string Value { get; }

	/// <summary>Leading '!'.</summary>
	public bool Negate { get; }

	/// <summary>Clause needs the display name.</summary>
	public bool UsesDisplayName => string.Equals(this.Field, "displayName", StringComparison.OrdinalIgnoreCase);

	/// <summary>Parses an expression; throws <see cref="UsageException"/> on syntax errors.</summary>
	public static WhereClause Parse(string expression) {
		var text = expression.Trim();
		bool negate = false;
		if (text.StartsWith('!')) {
			negate = true;
			text = text[1..].TrimStart();
		}

		int opAt = text.AsSpan().IndexOfAny(OperatorChars);
		if (opAt <= 0)
			throw new UsageException($"--where '{expression}': expected <field><op><value> with op one of = != > >= < <= ~ or a trailing ?.");

		var field = text[..opAt].Trim();
		var rest = text.AsSpan(opAt);
		WhereOp op;
		int len;
		if (rest.StartsWith("!="))
			(op, len) = (WhereOp.Ne, 2);
		else if (rest.StartsWith(">="))
			(op, len) = (WhereOp.Ge, 2);
		else if (rest.StartsWith("<="))
			(op, len) = (WhereOp.Le, 2);
		else if (rest[0] == '=')
			(op, len) = (rest.Length > 1 && rest[1] == '=' ? (WhereOp.Eq, 2) : (WhereOp.Eq, 1));
		else if (rest[0] == '>')
			(op, len) = (WhereOp.Gt, 1);
		else if (rest[0] == '<')
			(op, len) = (WhereOp.Lt, 1);
		else if (rest[0] == '~')
			(op, len) = (WhereOp.Like, 1);
		else if (rest[0] == '?')
			(op, len) = (WhereOp.Exists, 1);
		else
			throw new UsageException($"--where '{expression}': unknown operator.");

		var value = rest[len..].Trim().ToString();
		if (op == WhereOp.Exists && value.Length > 0)
			throw new UsageException($"--where '{expression}': nothing may follow '?'.");
		return new WhereClause(expression, field, op, value, negate);
	}

	/// <summary>Evaluates the clause against a decoded record.</summary>
	public bool Evaluate(ArzRecord record, IRecordNamer? namer) {
		bool found = false;
		bool matched = false;

		if (this._fieldPattern is not null) {
			foreach (var v in record.Variables) {
				if (!this._fieldPattern.IsMatch(v.Name))
					continue;
				found = true;
				if (this.MatchesAny(v.Values)) {
					matched = true;
					break;
				}
			}
		}
		else {
			var variable = record.Get(this.Field);
			if (variable is not null) {
				found = true;
				matched = this.MatchesAny(variable.Values);
			}
			else {
				var pseudo = this.PseudoValue(record, namer);
				if (pseudo is not null) {
					found = true;
					matched = this.MatchesAny([pseudo]);
				}
			}
		}

		bool result = this.Op == WhereOp.Ne ? !found || !matched : found && matched;
		return result != this.Negate;
	}

	private string? PseudoValue(ArzRecord record, IRecordNamer? namer) {
		if (string.Equals(this.Field, "id", StringComparison.OrdinalIgnoreCase))
			return record.Info.Id;
		if (string.Equals(this.Field, "class", StringComparison.OrdinalIgnoreCase))
			return record.Info.Class;
		if (namer is null)
			return null;
		if (string.Equals(this.Field, "displayName", StringComparison.OrdinalIgnoreCase))
			return namer.DisplayName(record);
		if (string.Equals(this.Field, "nameTag", StringComparison.OrdinalIgnoreCase))
			return namer.NameTag(record);
		return null;
	}

	/// <summary>For <see cref="WhereOp.Ne"/> "matches" means "some value equals" (the result is then inverted).</summary>
	private bool MatchesAny(object[] values) {
		foreach (var v in values)
			if (this.MatchesOne(v))
				return true;
		return false;
	}

	private bool MatchesOne(object value) {
		switch (this.Op) {
			case WhereOp.Exists:
				return Values.IsNonZero(value);
			case WhereOp.Like:
				return this._like!.IsMatch(Values.ToText(value));
			case WhereOp.Eq:
			case WhereOp.Ne:
				return this.Equal(value);
			default:
				int cmp = this.Compare(value);
				return this.Op switch {
					WhereOp.Gt => cmp > 0,
					WhereOp.Ge => cmp >= 0,
					WhereOp.Lt => cmp < 0,
					_ => cmp <= 0,
				};
		}
	}

	private bool Equal(object value) {
		if (this._isNumber && value is not string && Values.TryNumber(value, out var n))
			return value is float or double
				? Math.Abs(n - this._number) <= 1e-6 * Math.Max(1.0, Math.Abs(this._number))
				: n == this._number;
		if (this._isNumber && value is string s && Values.TryParseNumber(s, out var sn))
			return sn == this._number;
		return Values.PathInsensitiveEquals(Values.ToText(value), this.Value);
	}

	/// <summary>Sign of (value - right-hand side): numeric when both sides are numbers, else ordinal case-insensitive.</summary>
	private int Compare(object value) {
		if (this._isNumber && Values.TryNumber(value, out var n))
			return n.CompareTo(this._number);
		return string.Compare(Values.ToText(value), this.Value, StringComparison.OrdinalIgnoreCase);
	}

	/// <inheritdoc/>
	public override string ToString() => this.Source;
}
