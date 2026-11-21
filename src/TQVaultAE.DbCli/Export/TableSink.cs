//-----------------------------------------------------------------------
// <copyright file="TableSink.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Globalization;
using System.Text.Json;
using TQVaultAE.DbCli.Query;

namespace TQVaultAE.DbCli.Export;

/// <summary>Streaming writer of flat rows (text tags, archive entries) as json, jsonl or tsv.</summary>
internal sealed class TableSink : IDisposable
{
	private readonly string[] _columns;
	private readonly OutputFormat _format;
	private readonly Stream _stream;
	private readonly Utf8JsonWriter? _json;
	private readonly TextWriter? _tsv;
	private bool _started;

	public TableSink(Stream stream, OutputFormat format, params string[] columns) {
		if (format is not (OutputFormat.Json or OutputFormat.Jsonl or OutputFormat.Tsv))
			throw new ArgumentOutOfRangeException(nameof(format), format, "Tables support json, jsonl and tsv.");
		this._stream = stream;
		this._format = format;
		this._columns = columns;
		if (format == OutputFormat.Tsv)
			this._tsv = new StreamWriter(stream, Formats.Utf8, Formats.BufferSize, leaveOpen: true);
		else
			this._json = new Utf8JsonWriter(stream, Formats.JsonOptions(format == OutputFormat.Json, multipleRoots: format == OutputFormat.Jsonl));
	}

	/// <summary>Rows written.</summary>
	public int Written { get; private set; }

	/// <summary>Writes a row; cells map to columns by position (string, numbers, bool, DateTime or null).</summary>
	public void Row(params ReadOnlySpan<object?> cells) {
		this.Start();
		this.Written++;
		if (this._tsv is not null) {
			for (int i = 0; i < cells.Length; i++) {
				if (i > 0)
					this._tsv.Write('\t');
				Tsv.WriteCell(this._tsv, CellText(cells[i]));
			}
			this._tsv.Write('\n');
			return;
		}

		var w = this._json!;
		w.WriteStartObject();
		for (int i = 0; i < cells.Length && i < this._columns.Length; i++) {
			w.WritePropertyName(this._columns[i]);
			if (cells[i] is DateTime dt)
				w.WriteStringValue(dt.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture));
			else
				JsonRecordSink.WriteValue(w, cells[i]);
		}
		w.WriteEndObject();
		if (this._format == OutputFormat.Jsonl) {
			w.Flush();
			w.Reset(); // next root value without a ',' separator
			this._stream.WriteByte((byte)'\n');
		}
		else if (w.BytesPending > Formats.BufferSize) {
			w.Flush();
		}
	}

	private void Start() {
		if (this._started)
			return;
		this._started = true;
		if (this._tsv is not null)
			Tsv.WriteRow(this._tsv, this._columns);
		else if (this._format == OutputFormat.Json)
			this._json!.WriteStartArray();
	}

	private static string CellText(object? value) => value switch {
		DateTime dt => dt.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
		_ => Values.ToText(value),
	};

	/// <summary>Closes the document and flushes.</summary>
	public void Complete() {
		this.Start();
		if (this._json is not null) {
			if (this._format == OutputFormat.Json) {
				this._json.WriteEndArray();
				this._json.Flush();
				this._stream.WriteByte((byte)'\n');
			}
			this._json.Flush();
		}
		this._tsv?.Flush();
		this._stream.Flush();
	}

	/// <inheritdoc/>
	public void Dispose() {
		this._json?.Dispose();
		this._tsv?.Dispose();
	}
}
