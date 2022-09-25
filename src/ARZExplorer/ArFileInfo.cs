using TQVaultAE.Domain.Entities;

namespace ArzExplorer;

internal class ArFileInfo
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
	internal string sourceFile;

	/// <summary>
	/// Destination directory path for extracted files.
	/// </summary>
	internal string destDirectory;

	/// <summary>
	/// File name for a single extracted file.
	/// </summary>
	internal string destFile;

	/// <summary>
	/// Holds the current record being viewed.
	/// </summary>
	internal DBRecordCollection record;

}

