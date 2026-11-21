//-----------------------------------------------------------------------
// <copyright file="RecordOutput.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using TQVaultAE.DbCli.Export;

namespace TQVaultAE.DbCli.Commands;

/// <summary>Opens the record sink for a format and <c>--out</c> target.</summary>
internal sealed class RecordOutput : IDisposable
{
	private readonly Stream? _stream;
	private readonly TextWriter? _text;

	private RecordOutput(IRecordSink sink, Stream? stream, TextWriter? text) {
		this.Sink = sink;
		this._stream = stream;
		this._text = text;
	}

	/// <summary>The sink.</summary>
	public IRecordSink Sink { get; }

	/// <summary>
	/// Opens the sink. For dbr, <paramref name="outPath"/> is a directory (one file per record) unless it ends with .dbr/.txt.
	/// </summary>
	public static RecordOutput Open(OutputFormat format, string? outPath, RecordSinkOptions options, bool multipleRecords, bool indented, bool singleObject = false) {
		switch (format) {
			case OutputFormat.Json:
			case OutputFormat.Jsonl: {
				var stream = CliContext.OpenOutput(outPath);
				return new RecordOutput(new JsonRecordSink(stream, format == OutputFormat.Jsonl, options, indented, singleObject), stream, null);
			}
			case OutputFormat.Tsv: {
				var text = CliContext.OpenTextOutput(outPath);
				return new RecordOutput(new TsvRecordSink(text, options), null, text);
			}
			case OutputFormat.Dbr: {
				bool singleFile = outPath is null || outPath == "-"
					|| outPath.EndsWith(".dbr", StringComparison.OrdinalIgnoreCase) || outPath.EndsWith(".txt", StringComparison.OrdinalIgnoreCase);
				if (!singleFile)
					return new RecordOutput(DbrRecordSink.ToDirectory(outPath!), null, null);
				var text = CliContext.OpenTextOutput(outPath);
				return new RecordOutput(DbrRecordSink.ToStream(text, multipleRecords), null, text);
			}
			default:
				throw new UsageException($"Format '{Formats.Name(format)}' is not supported for records.");
		}
	}

	/// <inheritdoc/>
	public void Dispose() {
		this.Sink.Dispose();
		this._text?.Dispose();
		this._stream?.Dispose();
	}
}
