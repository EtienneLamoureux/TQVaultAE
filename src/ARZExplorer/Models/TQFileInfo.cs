using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace ArzExplorer.Models;

/// <summary>
/// An opened ARZ or ARC file, and what the window shows or extracts of it.
/// </summary>
internal sealed class TQFileInfo
{
	/// <summary>
	/// The ARC file, when the file is one.
	/// </summary>
	internal ArcFile? ARCFile;

	/// <summary><see cref="ARCFile"/> of an open ARC file.</summary>
	internal ArcFile Arc => this.ARCFile ?? throw new InvalidOperationException("No ARC file is open.");

	/// <summary>
	/// The ARZ file, when the file is one.
	/// </summary>
	internal ArzFile? ARZFile;

	/// <summary><see cref="ARZFile"/> of an open ARZ file.</summary>
	internal ArzFile Arz => this.ARZFile ?? throw new InvalidOperationException("No ARZ file is open.");

	/// <summary>
	/// The type of the file.
	/// </summary>
	internal CompressedFileType FileType = CompressedFileType.Unknown;

	/// <summary>
	/// Name of the source file
	/// </summary>
	internal string SourceFile = string.Empty;

	private RecordId? _sourceFileId;

	/// <summary>
	/// <see cref="SourceFile"/> as a record id, computed once it is set.
	/// </summary>
	internal RecordId SourceFileId
	{
		get
		{
			if (this._sourceFileId is null && !string.IsNullOrWhiteSpace(this.SourceFile))
				this._sourceFileId = this.SourceFile.ToRecordId();
			return this._sourceFileId ?? RecordId.Empty;
		}
	}

	/// <summary>
	/// Destination directory path for extracted files.
	/// </summary>
	internal string? DestDirectory;

	/// <summary>
	/// File name for a single extracted file.
	/// </summary>
	internal string? DestFile;

	/// <summary>
	/// Holds the current record being viewed.
	/// </summary>
	internal DBRecordCollection? Records;
}
