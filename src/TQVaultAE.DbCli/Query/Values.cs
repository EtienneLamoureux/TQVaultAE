//-----------------------------------------------------------------------
// <copyright file="Values.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Globalization;

namespace TQVaultAE.DbCli.Query;

/// <summary>Conversions of decoded variable values (int, float, string, bool).</summary>
internal static class Values
{
	/// <summary>Numeric view of a value: ints, floats, bools (1/0) and numeric strings.</summary>
	public static bool TryNumber(object? value, out double number) {
		switch (value) {
			case int i:
				number = i;
				return true;
			case float f:
				number = f;
				return true;
			case double d:
				number = d;
				return true;
			case long l:
				number = l;
				return true;
			case bool b:
				number = b ? 1 : 0;
				return true;
			case string s:
				return TryParseNumber(s, out number);
			default:
				number = 0;
				return false;
		}
	}

	/// <summary>Invariant number parse (integers, decimals, exponent).</summary>
	public static bool TryParseNumber(string? s, out double number) {
		number = 0;
		if (string.IsNullOrWhiteSpace(s))
			return false;
		return double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out number) && double.IsFinite(number);
	}

	/// <summary>Value is non-zero, non-empty or true.</summary>
	public static bool IsNonZero(object? value) => value switch {
		null => false,
		string s => !string.IsNullOrWhiteSpace(s),
		bool b => b,
		_ => TryNumber(value, out var n) && n != 0,
	};

	/// <summary>Short round-trip text (TSV, comparisons, globs). Bools are 1/0 as in .dbr files.</summary>
	public static string ToText(object? value) => value switch {
		null => string.Empty,
		string s => s,
		bool b => b ? "1" : "0",
		float f => f.ToString("R", CultureInfo.InvariantCulture),
		double d => d.ToString("R", CultureInfo.InvariantCulture),
		IFormattable fmt => fmt.ToString(null, CultureInfo.InvariantCulture),
		_ => value.ToString() ?? string.Empty,
	};

	/// <summary>String equality ignoring case and treating '/' and '\' as equal.</summary>
	public static bool PathInsensitiveEquals(ReadOnlySpan<char> a, ReadOnlySpan<char> b) {
		if (a.Length != b.Length)
			return false;
		for (int i = 0; i < a.Length; i++) {
			char x = a[i], y = b[i];
			if (x == y)
				continue;
			if ((x is '/' or '\\') && (y is '/' or '\\'))
				continue;
			if (char.ToUpperInvariant(x) != char.ToUpperInvariant(y))
				return false;
		}
		return true;
	}

	/// <summary>Value is a record reference (string ending with .dbr).</summary>
	public static bool IsRecordReference(object? value) => value is string s && s.EndsWith(".dbr", StringComparison.OrdinalIgnoreCase);
}
