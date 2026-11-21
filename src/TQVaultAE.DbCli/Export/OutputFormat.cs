//-----------------------------------------------------------------------
// <copyright file="OutputFormat.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using TQVaultAE.DbCli.Commands;

namespace TQVaultAE.DbCli.Export;

/// <summary>Structured output formats.</summary>
internal enum OutputFormat
{
	Json,
	Jsonl,
	Tsv,
	Dbr,
	Text,
}

/// <summary>Format selection and shared writer settings.</summary>
internal static class Formats
{
	/// <summary>UTF-8 without BOM.</summary>
	public static readonly Encoding Utf8 = new UTF8Encoding(false);

	/// <summary>Buffer size of output streams.</summary>
	public const int BufferSize = 1 << 16;

	/// <summary>JSON writer options; relaxed escaping keeps accented (cp1252) characters readable.</summary>
	public static JsonWriterOptions JsonOptions(bool indented, bool multipleRoots = false) => new() {
		Indented = indented,
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
		SkipValidation = multipleRoots,
	};

	/// <summary>
	/// Resolves <c>--format</c>: explicit value, else inferred from the <c>--out</c> extension, else <paramref name="fallback"/>.
	/// </summary>
	public static OutputFormat Resolve(string? format, string? outPath, OutputFormat fallback, params OutputFormat[] allowed) {
		OutputFormat result;
		if (format is not null) {
			if (!Enum.TryParse(format, ignoreCase: true, out result) || !Enum.IsDefined(result) || int.TryParse(format, out _))
				throw new UsageException($"--format: '{format}' is not one of {string.Join(", ", allowed.Select(Name))}.");
		}
		else {
			result = Path.GetExtension(outPath ?? string.Empty).ToLowerInvariant() switch {
				".json" => OutputFormat.Json,
				".jsonl" or ".ndjson" => OutputFormat.Jsonl,
				".tsv" or ".tab" => OutputFormat.Tsv,
				".dbr" => OutputFormat.Dbr,
				_ => fallback,
			};
			if (Array.IndexOf(allowed, result) < 0)
				result = fallback;
		}

		if (Array.IndexOf(allowed, result) < 0)
			throw new UsageException($"--format: '{Name(result)}' is not supported here; use one of {string.Join(", ", allowed.Select(Name))}.");
		return result;
	}

	/// <summary>Lower-case name.</summary>
	public static string Name(OutputFormat f) => f.ToString().ToLowerInvariant();
}
