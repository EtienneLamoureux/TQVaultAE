//-----------------------------------------------------------------------
// <copyright file="RecordSinks.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Text.Json;
using TQVaultAE.DbCli.Core;
using TQVaultAE.DbCli.Query;

namespace TQVaultAE.DbCli.Export;

/// <summary>Streaming record output.</summary>
internal interface IRecordSink : IDisposable
{
	/// <summary>Records written so far.</summary>
	int Written { get; }

	/// <summary>Writes one record.</summary>
	void Write(RecordView view);

	/// <summary>Terminates the document (closing bracket...) and flushes.</summary>
	void Complete();
}

/// <summary>Writer settings shared by record sinks.</summary>
/// <param name="Projection">Columns of the top-level records.</param>
/// <param name="Resolver">Adds resolved text for tag-valued string fields when not <see langword="null"/> (<c>--resolve</c>).</param>
internal sealed record RecordSinkOptions(Projection Projection, IRecordNamer? Resolver = null)
{
	/// <summary>Projection used for records inlined by <c>--follow</c>: every field.</summary>
	public static readonly Projection LinkedProjection = Projection.Parse(["id", "class", "displayName", "*"]);
}

/// <summary>
/// JSON array (<c>json</c>) or one object per line (<c>jsonl</c>):
/// <c>{"id":..,"class":..,"displayName":..,"fields":{"name": value | [values]},"resolved":{..},"linked":{"field":[..]}}</c>.
/// </summary>
internal sealed class JsonRecordSink : IRecordSink
{
	private readonly Stream _stream;
	private readonly bool _array;
	private readonly RecordSinkOptions _options;
	private readonly Utf8JsonWriter _writer;
	private readonly List<ArzVariable> _scratch = [];
	private bool _started;

	/// <param name="stream">Destination.</param>
	/// <param name="lines">jsonl: one compact object per line.</param>
	/// <param name="options">Projection and resolver.</param>
	/// <param name="indented">Indent (json only).</param>
	/// <param name="singleObject">json without the enclosing array (exactly one record expected, e.g. <c>tqdb record</c>).</param>
	public JsonRecordSink(Stream stream, bool lines, RecordSinkOptions options, bool indented = true, bool singleObject = false) {
		this._stream = stream;
		this._array = !lines && !singleObject;
		this._options = options;
		this._writer = new Utf8JsonWriter(stream, Formats.JsonOptions(indented && !lines, multipleRoots: !this._array));
	}

	/// <inheritdoc/>
	public int Written { get; private set; }

	/// <inheritdoc/>
	public void Write(RecordView view) {
		if (this._array && !this._started) {
			this._writer.WriteStartArray();
			this._started = true;
		}

		WriteRecord(this._writer, view, this._options.Projection, this._options.Resolver, this._scratch);
		this.Written++;

		if (!this._array) {
			this._writer.Flush();
			this._writer.Reset(); // next root value without a ',' separator
			this._stream.WriteByte((byte)'\n');
		}
		else if (this._writer.BytesPending > Formats.BufferSize) {
			this._writer.Flush();
		}
	}

	/// <inheritdoc/>
	public void Complete() {
		if (this._array) {
			if (!this._started)
				this._writer.WriteStartArray();
			this._writer.WriteEndArray();
			this._writer.Flush();
			this._stream.WriteByte((byte)'\n');
		}
		this._writer.Flush();
		this._stream.Flush();
	}

	/// <summary>Writes one record object.</summary>
	internal static void WriteRecord(Utf8JsonWriter w, RecordView view, Projection projection, IRecordNamer? resolver, List<ArzVariable> scratch) {
		w.WriteStartObject();
		w.WriteString("id", view.Info.Id);
		w.WriteString("class", view.Info.Class);
		foreach (var pseudo in projection.Pseudo) {
			if (pseudo is PseudoColumn.Id or PseudoColumn.Class)
				continue;
			var value = view.PseudoValue(pseudo);
			if (value is null)
				w.WriteNull(Projection.PseudoName(pseudo));
			else
				w.WriteString(Projection.PseudoName(pseudo), value);
		}

		if (view.Record is not null && projection.HasVariables) {
			projection.Select(view.Record, scratch);
			w.WriteStartObject("fields");
			foreach (var v in scratch) {
				w.WritePropertyName(v.Name);
				WriteValues(w, v.Values);
			}
			w.WriteEndObject();

			if (resolver is not null)
				WriteResolved(w, scratch, resolver);
		}

		if (view.Linked is { Count: > 0 }) {
			w.WriteStartObject("linked");
			var nested = new List<ArzVariable>();
			foreach (var (field, records) in view.Linked) {
				w.WriteStartArray(field);
				foreach (var r in records)
					WriteRecord(w, r, RecordSinkOptions.LinkedProjection, resolver, nested);
				w.WriteEndArray();
			}
			w.WriteEndObject();
		}

		w.WriteEndObject();
	}

	private static void WriteResolved(Utf8JsonWriter w, List<ArzVariable> variables, IRecordNamer resolver) {
		bool open = false;
		foreach (var v in variables) {
			if (v.Type != ArzValueType.String || v.Values.Length == 0)
				continue;

			if (v.Values.Length == 1) {
				var text = resolver.ResolveTag(v.Values[0] as string);
				if (text is null)
					continue;
				Open(w, ref open);
				w.WriteString(v.Name, text);
				continue;
			}

			bool any = false;
			foreach (var o in v.Values)
				if (resolver.ResolveTag(o as string) is not null) {
					any = true;
					break;
				}
			if (!any)
				continue;
			Open(w, ref open);
			w.WriteStartArray(v.Name);
			foreach (var o in v.Values) {
				var text = resolver.ResolveTag(o as string);
				if (text is null)
					w.WriteNullValue();
				else
					w.WriteStringValue(text);
			}
			w.WriteEndArray();
		}
		if (open)
			w.WriteEndObject();

		static void Open(Utf8JsonWriter w, ref bool open) {
			if (!open) {
				w.WriteStartObject("resolved");
				open = true;
			}
		}
	}

	/// <summary>Single value → scalar, several → array.</summary>
	internal static void WriteValues(Utf8JsonWriter w, object[] values) {
		if (values.Length == 1) {
			WriteValue(w, values[0]);
			return;
		}
		w.WriteStartArray();
		foreach (var o in values)
			WriteValue(w, o);
		w.WriteEndArray();
	}

	/// <summary>Typed JSON value; non-finite floats are written as strings.</summary>
	internal static void WriteValue(Utf8JsonWriter w, object? value) {
		switch (value) {
			case null:
				w.WriteNullValue();
				break;
			case int i:
				w.WriteNumberValue(i);
				break;
			case long l:
				w.WriteNumberValue(l);
				break;
			case float f when float.IsFinite(f):
				w.WriteNumberValue(f);
				break;
			case double d when double.IsFinite(d):
				w.WriteNumberValue(d);
				break;
			case bool b:
				w.WriteBooleanValue(b);
				break;
			case string s:
				w.WriteStringValue(s);
				break;
			default:
				w.WriteStringValue(Values.ToText(value));
				break;
		}
	}

	/// <inheritdoc/>
	public void Dispose() => this._writer.Dispose();
}

/// <summary>
/// TSV. Fixed projections: one row per record, header = projected columns (plus <c>field:text</c> columns with <c>--resolve</c>).
/// Dynamic projections (<c>*</c> or globs): long format, one row per variable: id, class, field, type, value[, text].
/// Multi-values are joined by ';'.
/// </summary>
internal sealed class TsvRecordSink : IRecordSink
{
	private readonly TextWriter _writer;
	private readonly RecordSinkOptions _options;
	private readonly List<ArzVariable> _scratch = [];
	private readonly bool _header;
	private bool _headerWritten;

	public TsvRecordSink(TextWriter writer, RecordSinkOptions options, bool header = true) {
		this._writer = writer;
		this._options = options;
		this._header = header;
	}

	/// <inheritdoc/>
	public int Written { get; private set; }

	private bool Long => this._options.Projection.IsDynamic;

	/// <inheritdoc/>
	public void Write(RecordView view) {
		this.EnsureHeader();
		if (this.Long)
			this.WriteLong(view);
		else
			this.WriteWide(view);
		this.Written++;
	}

	private void EnsureHeader() {
		if (this._headerWritten || !this._header)
			return;
		this._headerWritten = true;
		var cols = new List<string?>();
		if (this.Long) {
			cols.AddRange(["id", "class"]);
			foreach (var p in this._options.Projection.Pseudo)
				if (p is PseudoColumn.DisplayName or PseudoColumn.NameTag)
					cols.Add(Projection.PseudoName(p));
			cols.AddRange(["field", "type", "value"]);
			if (this._options.Resolver is not null)
				cols.Add("text");
		}
		else {
			foreach (var c in this._options.Projection.Columns) {
				cols.Add(c.Name);
				if (c.Pseudo == PseudoColumn.None && this._options.Resolver is not null)
					cols.Add(c.Name + ":text");
			}
		}
		Tsv.WriteRow(this._writer, cols);
	}

	private void WriteWide(RecordView view) {
		bool first = true;
		foreach (var c in this._options.Projection.Columns) {
			if (!first)
				this._writer.Write('\t');
			first = false;

			if (c.Pseudo != PseudoColumn.None) {
				Tsv.WriteCell(this._writer, view.PseudoValue(c.Pseudo));
				continue;
			}

			var v = view.Record?.Get(c.Name);
			if (v is not null)
				Tsv.WriteValues(this._writer, v.Values);
			if (this._options.Resolver is not null) {
				this._writer.Write('\t');
				if (v is not null)
					this.WriteResolvedCell(v);
			}
		}
		this._writer.Write('\n');
	}

	private void WriteLong(RecordView view) {
		if (view.Record is null)
			return;
		this._options.Projection.Select(view.Record, this._scratch);
		foreach (var v in this._scratch) {
			Tsv.WriteCell(this._writer, view.Info.Id);
			this._writer.Write('\t');
			Tsv.WriteCell(this._writer, view.Info.Class);
			foreach (var p in this._options.Projection.Pseudo) {
				if (p is PseudoColumn.DisplayName or PseudoColumn.NameTag) {
					this._writer.Write('\t');
					Tsv.WriteCell(this._writer, view.PseudoValue(p));
				}
			}
			this._writer.Write('\t');
			Tsv.WriteCell(this._writer, v.Name);
			this._writer.Write('\t');
			this._writer.Write(TypeName(v.Type));
			this._writer.Write('\t');
			Tsv.WriteValues(this._writer, v.Values);
			if (this._options.Resolver is not null) {
				this._writer.Write('\t');
				this.WriteResolvedCell(v);
			}
			this._writer.Write('\n');
		}
	}

	private void WriteResolvedCell(ArzVariable v) {
		if (v.Type != ArzValueType.String)
			return;
		for (int i = 0; i < v.Values.Length; i++) {
			if (i > 0)
				this._writer.Write(';');
			Tsv.WriteCell(this._writer, this._options.Resolver!.ResolveTag(v.Values[i] as string));
		}
	}

	/// <summary>Lower-case type name.</summary>
	internal static string TypeName(ArzValueType t) => t switch {
		ArzValueType.Int => "int",
		ArzValueType.Float => "float",
		ArzValueType.String => "string",
		ArzValueType.Bool => "bool",
		_ => "unknown",
	};

	/// <inheritdoc/>
	public void Complete() {
		this.EnsureHeader();
		this._writer.Flush();
	}

	/// <inheritdoc/>
	public void Dispose() {
	}
}

/// <summary>
/// ARZExplorer-style .dbr text: one <c>name,v1;v2,</c> line per variable (floats with 6 decimals, bools 1/0).
/// To a directory: one file per record mirroring its path. To a stream: each record is preceded by <c>// &lt;id&gt;</c>
/// when more than one record may be written. Records inlined by <c>--follow</c> are written as additional records.
/// </summary>
internal sealed class DbrRecordSink : IRecordSink
{
	private readonly TextWriter? _stream;
	private readonly string? _directory;
	private readonly bool _separators;
	private readonly HashSet<int> _written = [];

	private DbrRecordSink(TextWriter? stream, string? directory, bool separators) {
		this._stream = stream;
		this._directory = directory;
		this._separators = separators;
	}

	/// <summary>Writes records to a single text stream.</summary>
	public static DbrRecordSink ToStream(TextWriter writer, bool separators) => new(writer, null, separators);

	/// <summary>Writes one .dbr file per record under <paramref name="directory"/>.</summary>
	public static DbrRecordSink ToDirectory(string directory) => new(null, directory, false);

	/// <inheritdoc/>
	public int Written { get; private set; }

	/// <inheritdoc/>
	public void Write(RecordView view) {
		this.WriteOne(view);
		if (view.Linked is null)
			return;
		foreach (var (_, records) in view.Linked)
			foreach (var r in records)
				this.WriteOne(r);
	}

	private void WriteOne(RecordView view) {
		if (view.Record is null || !this._written.Add(view.Info.Index))
			return;
		this.Written++;

		if (this._stream is not null) {
			if (this._separators) {
				if (this.Written > 1)
					this._stream.WriteLine();
				this._stream.Write("// ");
				this._stream.WriteLine(view.Info.Id);
			}
			WriteBody(this._stream, view.Record);
			return;
		}

		var path = PathSanitizer.Combine(this._directory!, view.Info.Id);
		Directory.CreateDirectory(Path.GetDirectoryName(path)!);
		using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 1 << 14);
		using var w = new StreamWriter(fs, Formats.Dbr);
		WriteBody(w, view.Record);
	}

	/// <summary>Writes the variable lines of a record.</summary>
	internal static void WriteBody(TextWriter w, ArzRecord record) {
		foreach (var v in record.Variables) {
			DbrText.Write(w, v.Name, v.Values);
			w.WriteLine();
		}
	}

	/// <inheritdoc/>
	public void Complete() => this._stream?.Flush();

	/// <inheritdoc/>
	public void Dispose() {
	}
}
