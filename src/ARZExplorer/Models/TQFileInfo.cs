using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace ArzExplorer.Models;

internal class TQFileInfo
{

	/// <summary>
	/// The static instance of the arcFile we are working on.
	/// </summary>
	internal ArcFile ARCFile;

	/// <summary>
	/// The static instance of the arzFile we are working on.
	/// </summary>
	internal ArzFile ARZFile;

	/// <summary>
	/// Gets the type of file that is open.
	/// </summary>
	internal CompressedFileType FileType = CompressedFileType.Unknown;

	/// <summary>
	/// Name of the source file
	/// </summary>
	internal string SourceFile;

	RecordId _SourceFileId;
	internal RecordId SourceFileId
	{
		get
		{
			if (_SourceFileId is null && !string.IsNullOrWhiteSpace(SourceFile))
				_SourceFileId = SourceFile.ToRecordId();
			return _SourceFileId;
		}
	}

	/// <summary>
	/// Destination directory path for extracted files.
	/// </summary>
	internal string DestDirectory;

	/// <summary>
	/// File name for a single extracted file.
	/// </summary>
	internal string DestFile;

	/// <summary>
	/// Holds the current record being viewed.
	/// </summary>
	internal DBRecordCollection Records;

}

