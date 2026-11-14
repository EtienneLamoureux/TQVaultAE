using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using Microsoft.Extensions.Logging;
using TQVaultAE.Config;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GameFiles;

namespace TQVaultAE.Data;

public class ArcFileProvider(ILogger<ArcFileProvider> log, IPathIO pathIO, IDirectoryIO directoryIO, IFileIO fileIO, UserSettings uSettings) : IArcFileProvider
{
	private readonly ILogger Log = log;
	private readonly IPathIO PathIO = pathIO;
	private readonly IDirectoryIO DirectoryIO = directoryIO;
	private readonly IFileIO FileIO = fileIO;
	private readonly UserSettings USettings = uSettings;

	#region ArcFile Public Methods

	/// <summary>
	/// Reads the ARC file table of contents to determine if the file is readable.
	/// </summary>
	/// <returns>True if able to read the ToC</returns>
	public bool Read(ArcFile file)
	{
		try
		{
			if (!file.FileHasBeenRead)
				this.ReadARCToC(file);

			return file.DirectoryEntries.Count != 0;
		}
		catch (IOException exception)
		{
			this.Log.LogError(exception, "Failed to read the table of contents of {ArcFile}", file.FileName);
			return false;
		}
	}

	/// <summary>
	/// Writes a record to a file.
	/// </summary>
	/// <param name="baseFolder">string holding the base folder path</param>
	/// <param name="record">Record we are writing</param>
	/// <param name="destinationFileName">Filename for the new file.</param>
	public void Write(ArcFile file, string baseFolder, RecordId record, string destinationFileName)
	{
		try
		{
			if (!file.FileHasBeenRead)
				this.ReadARCToC(file);

			if (!file.DirectoryEntries.TryGetValue(record, out ArcEntry? entry))
				return;

			string destination = baseFolder;
			if (!destination.EndsWith("\\", StringComparison.OrdinalIgnoreCase))
				destination = string.Concat(destination, "\\");

			this.WriteEntry(file, entry, string.Concat(destination, destinationFileName));
		}
		catch (IOException exception)
		{
			this.Log.LogError(exception, "Failed to extract record {Record} of {ArcFile} to {Destination} in {Folder}"
				, record.Raw, file.FileName, destinationFileName, baseFolder);
			return;
		}
	}

	/// <summary>
	/// Reads data from an ARC file and puts it into a Byte array (or NULL if not found)
	/// </summary>
	/// <param name="dataId">The string ID for the data which we are retieving.</param>
	/// <returns>Returns byte array of the data corresponding to the string ID.</returns>
	public byte[]? GetData(ArcFile file, RecordId dataId)
	{
		if (USettings.ARCFileDebugLevel > 0)
			Log.LogDebug("ARCFile.GetData({DataId})", dataId);

		if (!file.FileHasBeenRead)
			this.ReadARCToC(file);

		if (file.DirectoryEntries.Count == 0)
		{
			if (USettings.ARCFileDebugLevel > 1)
				Log.LogDebug("Error - Could not read {FileName}", file.FileName);

			return null;
		}

		if (USettings.ARCFileDebugLevel > 1)
			Log.LogDebug("Normalized dataID = {DataId}", dataId);

		int firstPathDelim = dataId.Normalized.IndexOf('\\');
		if (firstPathDelim != -1)
			dataId = dataId.Normalized.Substring(firstPathDelim + 1);

		if (!file.DirectoryEntries.TryGetValue(dataId, out ArcEntry? directoryEntry))
		{
			if (USettings.ARCFileDebugLevel > 1)
				Log.LogDebug("Error - {DataId} not found.", dataId);

			return null;
		}

		byte[] data = file.Archive!.ReadAll(directoryEntry);

		if (USettings.ARCFileDebugLevel > 0)
			Log.LogDebug("Exiting ARCFile.GetData()");

		return data;
	}

	/// <summary>
	/// Extracts the decoded ARC file contents into a folder.
	/// </summary>
	/// <param name="destination">Destination folder for the files.</param>
	/// <returns>true if successful, false on error.</returns>
	public bool ExtractArcFile(ArcFile file, string destination)
	{
		try
		{
			if (USettings.ARCFileDebugLevel > 0)
				Log.LogDebug("ARCFile.ReadARCFile()");

			if (!file.FileHasBeenRead)
				this.ReadARCToC(file);

			foreach (ArcEntry dirEntry in file.DirectoryEntries.Values)
			{
				string filename = destination;
				if (!filename.EndsWith('\\'))
					filename = string.Concat(filename, "\\");

				filename = string.Concat(filename, dirEntry.Name);

				if (USettings.ARCFileDebugLevel > 1)
					Log.LogDebug("Creating File - {Filename}", filename);

				this.WriteEntry(file, dirEntry, filename);
			}

			if (USettings.ARCFileDebugLevel > 0)
				Log.LogDebug("Exiting ARCFile.ReadARCFile()");

			return true;
		}
		catch (IOException exception)
		{
			Log.LogError(exception, "ARCFile.ReadARCFile() - Error reading arcfile");
			return false;
		}
	}

	#endregion ArcFile Public Methods

	/// <summary>
	/// Writes the content of an entry to a file, creating its folder (the entry may sit in a sub folder of the archive).
	/// </summary>
	private void WriteEntry(ArcFile file, ArcEntry entry, string path)
	{
		var folder = this.PathIO.GetDirectoryName(path);
		if (!this.DirectoryIO.Exists(folder))
			this.DirectoryIO.CreateDirectory(folder);

		this.FileIO.WriteAllBytes(path, file.Archive!.ReadAll(entry));
	}

	#region ArcFile Private Methods

	/// <summary>
	/// Read the table of contents of the ARC file through the shared <see cref="IArcArchive"/> reader.
	/// </summary>
	/// <remarks>An unreadable archive leaves <see cref="ArcFile.DirectoryEntries"/> empty.</remarks>
	public void ReadARCToC(ArcFile file)
	{
		file.FileHasBeenRead = true;

		IArcArchive archive;
		try
		{
			archive = ArcArchiveFactory.Open(file.FileName);
		}
		catch (InvalidDataException exception)
		{
			// Empty or truncated archives (e.g. XPack4\Underground.arc) have no readable entries.
			if (USettings.ARCFileDebugLevel > 0)
				Log.LogDebug(exception, "ARCFile.ReadARCToC({File}) - not a readable archive", file.FileName);

			file.DirectoryEntries = [];
			return;
		}

		file.Archive?.Dispose();
		file.Archive = archive;

		var entries = new Dictionary<RecordId, ArcEntry>(archive.Entries.Count);
		foreach (var entry in archive.Entries)
			entries.TryAdd(entry.Name, entry);

		file.DirectoryEntries = entries;

		if (USettings.ARCFileDebugLevel > 0)
			Log.LogDebug("ARCFile.ReadARCToC({File}) - {Count} entries", file.FileName, entries.Count);
	}

	#endregion ArcFile Private Methods
}
