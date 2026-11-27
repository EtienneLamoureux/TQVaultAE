//-----------------------------------------------------------------------
// <copyright file="Tsv.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using TQVaultAE.DbCli.Query;

namespace TQVaultAE.DbCli.Export;

/// <summary>
/// TSV cell writing: tab, CR and LF are written as the two-character sequences <c>\t</c>, <c>\r</c>, <c>\n</c>.
/// Backslashes are kept as is (record ids are backslash paths and must stay readable).
/// </summary>
internal static class Tsv
{
	/// <summary>Writes one escaped cell.</summary>
	public static void WriteCell(TextWriter w, ReadOnlySpan<char> value) {
		int start = 0;
		for (int i = 0; i < value.Length; i++) {
			char c = value[i];
			string? esc = c switch {
				'\t' => "\\t",
				'\n' => "\\n",
				'\r' => "\\r",
				_ => null,
			};
			if (esc is null)
				continue;
			w.Write(value[start..i]);
			w.Write(esc);
			start = i + 1;
		}
		w.Write(value[start..]);
	}

	/// <summary>Writes values joined by ';', each escaped.</summary>
	public static void WriteValues(TextWriter w, object[] values) {
		for (int i = 0; i < values.Length; i++) {
			if (i > 0)
				w.Write(';');
			WriteCell(w, Values.ToText(values[i]));
		}
	}

	/// <summary>Writes a header or data row of plain strings.</summary>
	public static void WriteRow(TextWriter w, IReadOnlyList<string?> cells) {
		for (int i = 0; i < cells.Count; i++) {
			if (i > 0)
				w.Write('\t');
			WriteCell(w, cells[i]);
		}
		w.Write('\n');
	}
}
