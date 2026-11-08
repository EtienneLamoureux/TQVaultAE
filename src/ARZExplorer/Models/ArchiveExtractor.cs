using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Domain.Entities;

namespace ArzExplorer.Models;

/// <summary>
/// How an extraction ended.
/// </summary>
internal enum ExtractionOutcome
{
	Completed,
	Cancelled,
	Failed,
}

/// <summary>
/// Result of <see cref="ArchiveExtractor.ExtractAll"/>.
/// </summary>
/// <param name="Outcome">How the extraction ended.</param>
/// <param name="Extracted">Number of entries written.</param>
/// <param name="Error">Exception that stopped a failed extraction.</param>
internal sealed record ExtractionResult(ExtractionOutcome Outcome, int Extracted, Exception? Error = null);

/// <summary>
/// Writes the records of an open ARZ file, or the entries of an open ARC file, to disk.
/// </summary>
public sealed class ArchiveExtractor(IArcFileProvider arcProvider, IArzFileProvider arzProvider, IDBRecordCollectionProvider recordWriter)
{
	private readonly IArcFileProvider _arcProvider = arcProvider;
	private readonly IArzFileProvider _arzProvider = arzProvider;
	private readonly IDBRecordCollectionProvider _recordWriter = recordWriter;

	/// <summary>
	/// Number of records or entries of the file.
	/// </summary>
	internal static int Count(TQFileInfo file)
		=> file.FileType == CompressedFileType.ArcFile ? file.Arc.Count : file.Arz.Count;

	/// <summary>
	/// Writes every record or entry of the file under <paramref name="baseFolder"/>, keeping their folders.
	/// </summary>
	/// <param name="file">Open ARZ or ARC file.</param>
	/// <param name="baseFolder">Destination folder.</param>
	/// <param name="onEntry">Called with each record or entry before it is written.</param>
	/// <param name="onWritten">Called after each record or entry is written.</param>
	/// <param name="cancellationToken">Stops the extraction after the entry being written.</param>
	/// <returns>The outcome; a failure carries its exception instead of throwing it.</returns>
	internal ExtractionResult ExtractAll(TQFileInfo file, string baseFolder, Action<RecordId> onEntry, Action onWritten, CancellationToken cancellationToken)
	{
		int extracted = 0;
		try
		{
			var keys = file.FileType == CompressedFileType.ArcFile ? file.Arc.Keys : file.Arz.Keys;
			foreach (var recordId in keys)
			{
				if (cancellationToken.IsCancellationRequested)
					return new ExtractionResult(ExtractionOutcome.Cancelled, extracted);

				onEntry(recordId);

				if (file.FileType == CompressedFileType.ArcFile)
					this._arcProvider.Write(file.Arc, baseFolder, recordId, recordId.Normalized);
				else
					this._recordWriter.Write(this._arzProvider.GetRecordNotCached(file.Arz, recordId), baseFolder);

				extracted++;
				onWritten();
			}
		}
		catch (Exception ex)
		{
			return new ExtractionResult(ExtractionOutcome.Failed, extracted, ex);
		}

		return cancellationToken.IsCancellationRequested
			? new ExtractionResult(ExtractionOutcome.Cancelled, extracted)
			: new ExtractionResult(ExtractionOutcome.Completed, extracted);
	}

	/// <summary>
	/// Writes the selected record or entry to a file.
	/// </summary>
	/// <param name="file">Open ARZ or ARC file.</param>
	/// <param name="treeKey">Key of the selected tree node.</param>
	/// <param name="destinationPath">Full path of the file to write.</param>
	internal void ExtractOne(TQFileInfo file, RecordId treeKey, string destinationPath)
	{
		var folder = Path.GetDirectoryName(destinationPath) ?? string.Empty;
		var fileName = Path.GetFileName(destinationPath);

		if (file.FileType == CompressedFileType.ArzFile)
			this._recordWriter.Write(this._arzProvider.GetRecordNotCached(file.Arz, treeKey), folder, fileName);
		else if (file.FileType == CompressedFileType.ArcFile)
			this._arcProvider.Write(file.Arc, folder, ArcPaths.EntryId(treeKey), fileName);
	}
}
