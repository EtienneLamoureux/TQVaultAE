using System.IO.MemoryMappedFiles;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;
using TQVaultAE.Config;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Logs;

namespace TQVaultAE.Data;

public class ArcFileProvider : IArcFileProvider
{
	private readonly ILogger Log;
	private readonly IPathIO PathIO;
	private readonly IDirectoryIO DirectoryIO;
	private readonly IFileDataService FileData;
	private readonly IDecompressionService Decompression;
	private readonly UserSettings USettings;

	/// <summary>
	/// Ctr
	/// </summary>
	/// <param name="fileName">File Name of the ARC file to be read.</param>
	public ArcFileProvider(ILogger<ArcFileProvider> log, IPathIO pathIO, IDirectoryIO directoryIO, UserSettings uSettings, IFileDataService fileData, IDecompressionService decompression)
	{
		this.Log = log;
		this.PathIO = pathIO;
		this.DirectoryIO = directoryIO;
		this.FileData = fileData;
		this.Decompression = decompression;
		this.USettings = uSettings;
	}

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

			return file.DirectoryEntries.Any();
		}
		catch (IOException exception)
		{
			Log.ErrorException(exception);
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

			string dataID = string.Concat(this.PathIO.GetFileNameWithoutExtension(file.FileName), "\\", record.Raw);
			byte[] data = this.GetData(file, dataID);
			if (data == null)
				return;

			string destination = baseFolder;
			if (!destination.EndsWith("\\", StringComparison.OrdinalIgnoreCase))
				destination = string.Concat(destination, "\\");

			destination = string.Concat(destination, destinationFileName);

			// If there is a sub directory in the arc file then we need to create it.
			var destDirName = PathIO.GetDirectoryName(destination);
			if (!DirectoryIO.Exists(destDirName))
				DirectoryIO.CreateDirectory(destDirName);

			using (FileStream outStream = new FileStream(destination, FileMode.Create, FileAccess.Write))
			{
				outStream.Write(data, 0, data.Length);
			}
		}
		catch (IOException exception)
		{
			Log.ErrorException(exception);
			return;
		}
	}

	/// <summary>
	/// Reads data from an ARC file and puts it into a Byte array (or NULL if not found)
	/// </summary>
	/// <param name="dataId">The string ID for the data which we are retieving.</param>
	/// <returns>Returns byte array of the data corresponding to the string ID.</returns>
	public byte[] GetData(ArcFile file, RecordId dataId)
	{
		if (USettings.ARCFileDebugLevel > 0)
			Log.LogDebug("ARCFile.GetData({0})", dataId);

		if (!file.FileHasBeenRead)
			this.ReadARCToC(file);

		if (!file.DirectoryEntries.Any())
		{
			if (USettings.ARCFileDebugLevel > 1)
				Log.LogDebug("Error - Could not read {0}", file.FileName);

			return null;
		}

		if (USettings.ARCFileDebugLevel > 1)
			Log.LogDebug("Normalized dataID = {0}", dataId);

		int firstPathDelim = dataId.Normalized.IndexOf('\\');
		if (firstPathDelim != -1)
			dataId = dataId.Normalized.Substring(firstPathDelim + 1);

		ArcDirEntry directoryEntry;

		if (file.DirectoryEntries.ContainsKey(dataId))
			directoryEntry = file.DirectoryEntries[dataId];
		else
		{
			if (USettings.ARCFileDebugLevel > 1)
				Log.LogDebug("Error - {0} not found.", dataId);

			return null;
		}

		byte[] data = new byte[directoryEntry.RealSize];
		int startPosition = 0;

		if ((directoryEntry.StorageType == 1) && (directoryEntry.CompressedSize == directoryEntry.RealSize))
		{
			if (USettings.ARCFileDebugLevel > 1)
			{
				Log.LogDebug("Offset={0}  Size={1}"
					, directoryEntry.FileOffset
					, directoryEntry.RealSize
				);
			}

			var rawData = this.FileData.GetReadOnlySpan(file.FileName, directoryEntry.FileOffset, directoryEntry.RealSize);
			rawData.CopyTo(data);
		}
		else
		{
			foreach (ArcPartEntry partEntry in directoryEntry.Parts)
			{
				var compressedData = this.FileData.GetMemory(file.FileName, partEntry.FileOffset + 2, partEntry.CompressedSize - 2);
				var decompressed = this.Decompression.DecompressZlib(compressedData);
				Buffer.BlockCopy(decompressed, 0, data, startPosition, decompressed.Length);
				startPosition += decompressed.Length;
			}
		}

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

			foreach (ArcDirEntry dirEntry in file.DirectoryEntries.Values)
			{
				RecordId dataID = string.Concat(
					this.PathIO.GetFileNameWithoutExtension(file.FileName), "\\"
					, dirEntry.FileName.Raw
				);

				if (USettings.ARCFileDebugLevel > 1)
				{
					Log.LogDebug($"Directory Filename = {dirEntry.FileName}");
					Log.LogDebug($"dataID = {dataID}");
				}

				byte[] data = this.GetData(file, dataID);

				string filename = destination;
				if (!filename.EndsWith("\\", StringComparison.Ordinal))
					filename = string.Concat(filename, "\\");

				filename = string.Concat(filename, dirEntry.FileName);

				// If there is a sub directory in the arc file then we need to create it.
				var dirName = PathIO.GetDirectoryName(filename);
				if (!DirectoryIO.Exists(dirName))
					DirectoryIO.CreateDirectory(dirName);

				if (USettings.ARCFileDebugLevel > 1)
					Log.LogDebug($"Creating File - {filename}");

				using (FileStream outStream = new FileStream(filename, FileMode.Create, FileAccess.Write))
				{
					outStream.Write(data, 0, data.Length);
				}
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

	#region ArcFile Private Methods

	/// <summary>
	/// Read the table of contents of the ARC file
	/// </summary>
	public void ReadARCToC(ArcFile file) => ReadARCToC_NEW(file);

	public void ReadARCToC_OLD(ArcFile file)
	{
		// Format of an ARC file
		// 0x08 - 4 bytes = # of files
		// 0x0C - 4 bytes = # of parts
		// 0x18 - 4 bytes = offset to directory structure
		//
		// Format of directory structure
		// 4-byte int = offset in file where this part begins
		// 4-byte int = size of compressed part
		// 4-byte int = size of uncompressed part
		// these triplets repeat for each part in the arc file
		// After these triplets are a bunch of null-terminated strings
		// which are the sub filenames.
		// After the subfilenames comes the subfile data:
		// 4-byte int = 3 == indicates start of subfile item  (maybe compressed flag??)
		//          1 == maybe uncompressed flag??
		// 4-byte int = offset in file where first part of this subfile begins
		// 4-byte int = compressed size of this file
		// 4-byte int = uncompressed size of this file
		// 4-byte crap
		// 4-byte crap
		// 4-byte crap
		// 4-byte int = numParts this file uses
		// 4-byte int = part# of first part for this file (starting at 0).
		// 4-byte int = length of filename string
		// 4-byte int = offset in directory structure for filename
		file.FileHasBeenRead = true;

		if (USettings.ARCFileDebugLevel > 0)
			Log.LogDebug("ARCFile.ReadARCToC({0})", file.FileName);

		try
		{
			using (FileStream arcFile = new FileStream(file.FileName, FileMode.Open, FileAccess.Read))
			{
				using (BinaryReader reader = new BinaryReader(arcFile))
				{
					if (USettings.ARCFileDebugLevel > 1)
						Log.LogDebug("File Length={0}", arcFile.Length);

					// check the file header
					byte first;
					if ((first = reader.ReadByte()) != 0x41)
						return;

					if ((first = reader.ReadByte()) != 0x52)
						return;

					if ((first = reader.ReadByte()) != 0x43)
						return;

					if (arcFile.Length < 0x21)
						return;

					reader.BaseStream.Seek(0x08, SeekOrigin.Begin);
					int numEntries = reader.ReadInt32();
					int numParts = reader.ReadInt32();

					if (USettings.ARCFileDebugLevel > 1)
						Log.LogDebug("numEntries={0}, numParts={1}", numEntries, numParts);

					ArcPartEntry[] parts = new ArcPartEntry[numParts];
					ArcDirEntry[] records = new ArcDirEntry[numEntries];

					if (USettings.ARCFileDebugLevel > 2)
						Log.LogDebug("Seeking to tocOffset location");

					reader.BaseStream.Seek(0x18, SeekOrigin.Begin);
					int tocOffset = reader.ReadInt32();

					if (USettings.ARCFileDebugLevel > 1)
						Log.LogDebug("tocOffset = {0}", tocOffset);

					// Make sure all 3 entries exist for the toc entry.
					if (arcFile.Length < (tocOffset + 12))
						return;

					// Read in all of the part data
					reader.BaseStream.Seek(tocOffset, SeekOrigin.Begin);
					int i;
					for (i = 0; i < numParts; ++i)
					{
						parts[i] = new ArcPartEntry();
						parts[i].FileOffset = reader.ReadInt32();
						parts[i].CompressedSize = reader.ReadInt32();
						parts[i].RealSize = reader.ReadInt32();

						if (USettings.ARCFileDebugLevel > 2)
						{
							Log.LogDebug("parts[{0}]", i);
							Log.LogDebug("  fileOffset={0}, compressedSize={1}, realSize={2}"
								, parts[i].FileOffset
								, parts[i].CompressedSize
								, parts[i].RealSize
							);
						}
					}

					// Now record this offset so we can come back and read in the filenames
					// after we have read in the file records
					int fileNamesOffset = (int)arcFile.Position;

					// Now seek to the location where the file record data is
					// This offset is from the end of the file.
					int fileRecordOffset = 44 * numEntries;

					if (USettings.ARCFileDebugLevel > 1)
					{
						Log.LogDebug("fileNamesOffset = {0}.  Seeking to {1} to read file record data."
							, fileNamesOffset
							, fileRecordOffset
						);
					}

					arcFile.Seek(-1 * fileRecordOffset, SeekOrigin.End);
					for (i = 0; i < numEntries; ++i)
					{
						records[i] = new ArcDirEntry();

						// storageType = 3 - compressed / 1- non compressed
						int storageType = reader.ReadInt32();

						if (USettings.ARCFileDebugLevel > 2)
							Log.LogDebug("StorageType={0}", storageType);

						// Added by VillageIdiot to support stored types
						records[i].StorageType = storageType;
						records[i].FileOffset = reader.ReadInt32();
						records[i].CompressedSize = reader.ReadInt32();
						records[i].RealSize = reader.ReadInt32();
						int crap = reader.ReadInt32(); // crap
						if (USettings.ARCFileDebugLevel > 2)
							Log.LogDebug("Crap2={0}", crap);

						crap = reader.ReadInt32(); // crap
						if (USettings.ARCFileDebugLevel > 2)
							Log.LogDebug("Crap3={0}", crap);

						crap = reader.ReadInt32(); // crap
						if (USettings.ARCFileDebugLevel > 2)
							Log.LogDebug("Crap4={0}", crap);

						int numberOfParts = reader.ReadInt32();
						if (numberOfParts < 1)
						{
							records[i].Parts = null;
							if (USettings.ARCFileDebugLevel > 2)
								Log.LogDebug("File {0} is not compressed.", i);
						}
						else
							records[i].Parts = new ArcPartEntry[numberOfParts];

						int firstPart = reader.ReadInt32();
						crap = reader.ReadInt32(); // filename length
						if (USettings.ARCFileDebugLevel > 2)
							Log.LogDebug("Filename Length={0}", crap);

						crap = reader.ReadInt32(); // filename offset
						if (USettings.ARCFileDebugLevel > 2)
						{
							Log.LogDebug("Filename Offset={0}", crap);

							Log.LogDebug("record[{0}]", i);
							Log.LogDebug("  offset={0} compressedSize={1} realSize={2}",
								records[i].FileOffset,
								records[i].CompressedSize,
								records[i].RealSize);

							if (storageType != 1 && records[i].IsActive)
							{
								Log.LogDebug("  numParts={0} firstPart={1} lastPart={2}",
									records[i].Parts.Length,
									firstPart,
									firstPart + records[i].Parts.Length - 1);
							}
							else
								Log.LogDebug("  INACTIVE firstPart={0}", firstPart);
						}

						if (storageType != 1 && records[i].IsActive)
						{
							for (int ip = 0; ip < records[i].Parts.Length; ++ip)
								records[i].Parts[ip] = parts[ip + firstPart];
						}
					}

					// Now read in the record names
					arcFile.Seek(fileNamesOffset, SeekOrigin.Begin);
					Span<byte> buffer = stackalloc byte[2048];
					for (i = 0; i < numEntries; ++i)
					{
						// only Active files have a filename entry
						if (records[i].IsActive)
						{
							// For each string, read bytes until I hit a 0x00 byte.
							if (USettings.ARCFileDebugLevel > 2)
								Log.LogDebug("Reading entry name {0:n0}", i);

							int bufferSize = 0;

							while ((buffer[bufferSize++] = reader.ReadByte()) != 0x00)
							{
								if (buffer[bufferSize - 1] == 0x03)
								{
									// File is null?
									arcFile.Seek(-1, SeekOrigin.Current); // backup
									bufferSize--;
									buffer[bufferSize] = 0x00;
									if (USettings.ARCFileDebugLevel > 2)
										Log.LogDebug("Null file - inactive?");

									break;
								}

								if (bufferSize >= buffer.Length)
								{
									Log.LogDebug("ARCFile.ReadARCToC() Error - Buffer size of 2048 has been exceeded.");
									if (USettings.ARCFileDebugLevel > 2)
									{
										var content = buffer.Slice(0, bufferSize).ToArray().Select(b => string.Format(CultureInfo.InvariantCulture, "0x{0:X}", b)).ToArray();
										Log.LogDebug($"Buffer contents:{Environment.NewLine}{string.Join(string.Empty, content)}{Environment.NewLine}{string.Empty}");
									}
								}
							}

							if (USettings.ARCFileDebugLevel > 2)
							{
								Log.LogDebug("Read {0:n0} bytes for name.  Converting to string.", bufferSize);
							}

							string newfile;
							if (bufferSize >= 1)
							{
								// Now convert the buffer to a string using stack-allocated span
								newfile = Encoding.ASCII.GetString(buffer.Slice(0, bufferSize - 1));
							}
							else
								newfile = string.Format(CultureInfo.InvariantCulture, "Null File {0}", i);

							records[i].FileName = newfile;

							if (USettings.ARCFileDebugLevel > 2)
								Log.LogDebug("Name {0:n0} = '{1}'", i, records[i].FileName);
						}
					}

					// Now convert the array of records into a Dictionary.
					var dictionary = new Dictionary<RecordId, ArcDirEntry>(numEntries);

					if (USettings.ARCFileDebugLevel > 1)
						Log.LogDebug("Creating Dictionary");

					for (i = 0; i < numEntries; ++i)
					{
						if (records[i].IsActive)
							dictionary.Add(records[i].FileName, records[i]);
					}

					file.DirectoryEntries = dictionary;

					if (USettings.ARCFileDebugLevel > 0)
						Log.LogDebug("Exiting ARCFile.ReadARCToC()");
				}
			}
		}
		catch (IOException exception)
		{
			Log.LogError(exception, "ARCFile.ReadARCToC() - Error reading arcfile");
		}
	}

	#endregion ArcFile Private Methods

	/// <summary>
	/// Read the table of contents of the ARC file using MemoryMappedFile and SpanHelper.
	/// This is a modernized version that uses memory-mapped file access for efficient random access
	/// and span-based binary parsing to reduce allocations.
	/// </summary>
	/// <param name="file">The ArcFile to read.</param>
	/// <returns>True if the read was successful.</returns>
	public bool ReadARCToC_NEW(ArcFile file)
	{
		/*
		 * Format of an ARC file
		 * 0x00-0x02: "ARC" header (0x41, 0x52, 0x43)
		 * 0x08 - 4 bytes = # of files (numEntries)
		 * 0x0C - 4 bytes = # of parts (numParts)
		 * 0x18 - 4 bytes = offset to directory structure (tocOffset)
		 *
		 * Format of directory structure (at tocOffset):
		 * Part entries: numParts * 12 bytes each:
		 *   4-byte int = offset in file where this part begins
		 *   4-byte int = size of compressed part
		 *   4-byte int = size of uncompressed part
		 *   These triplets repeat for each part in the arc file
		 *
		 * After the part triplets are a bunch of null-terminated strings
		 * which are the sub filenames.
		 *
		 * Then file record entries: numEntries * 44 bytes each (seek from end of file):
		 *   4-byte int = storageType (3 = compressed, 1 = non-compressed)
		 *   4-byte int = offset in file where first part of this subfile begins
		 *   4-byte int = compressed size of this file
		 *   4-byte int = uncompressed size of this file
		 *   4-byte crap (timestamp?)
		 *   4-byte crap (timestamp?)
		 *   4-byte crap (timestamp?)
		 *   4-byte int = numParts this file uses
		 *   4-byte int = part# of first part for this file (starting at 0)
		 *   4-byte int = length of filename string
		 *   4-byte int = offset in directory structure for filename
		 *
		 * Then at fileNamesOffset: null-terminated ASCII filenames
		*/

		file.FileHasBeenRead = true;

		if (USettings.ARCFileDebugLevel > 0)
			Log.LogDebug("ARCFile.ReadARCToC_NEW({0}) - Starting", file.FileName);

		// Use MemoryMappedFile for efficient random access
		using var mmf = MemoryMappedFile.CreateFromFile(file.FileName, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
		using var accessor = mmf.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);

		// Use actual file size, not accessor.Capacity, to ensure consistent behavior across platforms
		// accessor.Capacity may include memory-mapped metadata on some platforms
		long fileLength = new FileInfo(file.FileName).Length;

		if (USettings.ARCFileDebugLevel > 0)
			Log.LogDebug("ARCFile.ReadARCToC_NEW({0}) - fileLength={1} (mmf capacity={2})", file.FileName, fileLength, accessor.Capacity);

		// Validate minimum file length
		if (fileLength < 0x21)
		{
			if (USettings.ARCFileDebugLevel > 0)
				Log.LogDebug("ARCFile.ReadARCToC_NEW({0}) - FAIL: fileLength {1} < 0x21", file.FileName, fileLength);

			return false;
		}

		// Read entire file header into span for efficient parsing
		var headerSpan = new byte[0x21];
		accessor.ReadArray(0, headerSpan, 0, headerSpan.Length);

		// Check "ARC" header at bytes 0, 1, 2
		if (headerSpan[0] != 0x41 || headerSpan[1] != 0x52 || headerSpan[2] != 0x43)
		{
			if (USettings.ARCFileDebugLevel > 0)
				Log.LogDebug("ARCFile.ReadARCToC_NEW({0}) - FAIL: Invalid ARC header", file.FileName);

			return false;
		}

		if (USettings.ARCFileDebugLevel > 0)
			Log.LogDebug("File Length={0}", fileLength);

		// Read header values using SpanHelper
		int numEntries = SpanHelper.ReadInt32LittleEndian(headerSpan, 0x08);
		int numParts = SpanHelper.ReadInt32LittleEndian(headerSpan, 0x0C);
		int tocOffset = SpanHelper.ReadInt32LittleEndian(headerSpan, 0x18);

		if (USettings.ARCFileDebugLevel > 0)
		{
			Log.LogDebug("ARCFile.ReadARCToC_NEW({0}) - numEntries={1}, numParts={2}, tocOffset={3}", file.FileName, numEntries, numParts, tocOffset);
			Log.LogDebug("numEntries={0}, numParts={1}, tocOffset={2}", numEntries, numParts, tocOffset);
		}

		// Validate tocOffset - only needed if we have parts to read
		// When numParts == 0, tocOffset can legitimately be at or past end of file
		if (numParts > 0 && fileLength < tocOffset + 12)
		{
			if (USettings.ARCFileDebugLevel > 0)
				Log.LogDebug("ARCFile.ReadARCToC_NEW({0}) - FAIL: fileLength {1} < tocOffset + 12 ({2})", file.FileName, fileLength, tocOffset + 12);

			return false;
		}

		// Create arrays for parts and records
		var parts = new ArcPartEntry[numParts];
		var records = new ArcDirEntry[numEntries];

		// Read part entries (12 bytes each: fileOffset, compressedSize, realSize)
		// Read into a single span for the entire parts region
		var partsRegionSize = numParts * 12;
		var partsSpan = new byte[partsRegionSize];
		accessor.ReadArray(tocOffset, partsSpan, 0, partsRegionSize);

		if (USettings.ARCFileDebugLevel > 0)
			Log.LogDebug("ARCFile.ReadARCToC_NEW({0}) - Read {1} parts, partsRegionSize={2}", file.FileName, numParts, partsRegionSize);

		for (int i = 0; i < numParts; i++)
		{
			parts[i] = new ArcPartEntry();
			var partEntry = SpanHelper.ReadArcPartEntry(partsSpan, i * 12);
			parts[i].FileOffset = partEntry.FileOffset;
			parts[i].CompressedSize = partEntry.CompressedSize;
			parts[i].RealSize = partEntry.RealSize;

			if (USettings.ARCFileDebugLevel > 2)
				Log.LogDebug("parts[{0}]: fileOffset={1}, compressedSize={2}, realSize={3}", i, parts[i].FileOffset, parts[i].CompressedSize, parts[i].RealSize);
		}

		// Calculate fileNamesOffset (current position after reading parts)
		int fileNamesOffset = tocOffset + (numParts * 12);

		// Calculate where file record data starts (44 bytes per entry, from end)
		int fileRecordOffsetBytes = 44 * numEntries;
		long fileRecordStart = fileLength - fileRecordOffsetBytes;

		if (USettings.ARCFileDebugLevel > 0)
			Log.LogDebug("ARCFile.ReadARCToC_NEW({0}) - fileNamesOffset={1}, fileRecordOffsetBytes={2}, fileRecordStart={3}", file.FileName, fileNamesOffset, fileRecordOffsetBytes, fileRecordStart);

		// Read all file records into a single span (only if there are entries and position is valid)
		var recordsSpan = new byte[fileRecordOffsetBytes];
		if (numEntries > 0 && fileRecordStart < fileLength)
			accessor.ReadArray(fileRecordStart, recordsSpan, 0, fileRecordOffsetBytes);

		// Read file record entries
		for (int i = 0; i < numEntries; i++)
		{
			int baseOffset = i * 44;
			records[i] = new ArcDirEntry();

			var dirEntry = SpanHelper.ReadArcDirEntry(recordsSpan, baseOffset);

			records[i].StorageType = dirEntry.StorageType;
			records[i].FileOffset = dirEntry.FileOffset;
			records[i].CompressedSize = dirEntry.CompressedSize;
			records[i].RealSize = dirEntry.RealSize;

			int numberOfParts = dirEntry.NumberOfParts;
			int firstPart = dirEntry.FirstPart;

			if (numberOfParts < 1)
			{
				records[i].Parts = null;

				if (USettings.ARCFileDebugLevel > 2)
					Log.LogDebug("File {0} is not compressed.", i);
			}
			else
			{
				// Validate that firstPart + numberOfParts is within bounds before allocating
				// OLD algorithm doesn't bounds-check, so we need to be lenient to match its behavior
				if (firstPart < 0 || firstPart + numberOfParts > numParts)
				{
					// Invalid part range - mark as inactive to match OLD behavior
					Log.LogWarning("ARCFile.ReadARCToC_NEW({0}) - record[{1}] has invalid part range: firstPart={2}, numberOfParts={3}, numParts={4}",
						file.FileName, i, firstPart, numberOfParts, numParts);

					records[i].Parts = null;
				}
				else
				{
					records[i].Parts = new ArcPartEntry[numberOfParts];
					// Link parts to this record
					for (int ip = 0; ip < numberOfParts; ip++)
					{
						int partIndex = ip + firstPart;
						records[i].Parts[ip] = parts[partIndex];
					}
				}
			}

			if (USettings.ARCFileDebugLevel > 2)
			{
				Log.LogDebug("record[{0}]: storageType={1}, offset={2}, compressedSize={3}, realSize={4}, numParts={5}, firstPart={6}",
					i, dirEntry.StorageType, records[i].FileOffset, records[i].CompressedSize, records[i].RealSize, numberOfParts, firstPart);
			}

			// Log first 10 entries IsActive status
			if (i < 10)
			{
				if (USettings.ARCFileDebugLevel > 2)
				{
					Log.LogDebug("ARCFile.ReadARCToC_NEW({0}) - Record[{1}]: storageType={2}, Parts={3}, IsActive={4}",
						file.FileName, i, records[i].StorageType, records[i].Parts == null ? "null" : records[i].Parts.Length.ToString(), records[i].IsActive);
				}
			}
		}

		if (USettings.ARCFileDebugLevel > 0)
			Log.LogDebug("ARCFile.ReadARCToC_NEW({0}) - Processed {1} records", file.FileName, numEntries);

		// Read filenames region using efficient span parsing
		// fileNamesOffset to fileRecordStart
		int filenameRegionSize = (int)(fileRecordStart - fileNamesOffset);

		if (USettings.ARCFileDebugLevel > 0)
			Log.LogDebug("ARCFile.ReadARCToC_NEW({0}) - filenameRegionSize={1}, fileNamesOffset={2}, fileRecordStart={3}", file.FileName, filenameRegionSize, fileNamesOffset, fileRecordStart);

		if (filenameRegionSize > 0 && fileNamesOffset >= 0 && fileNamesOffset < fileLength)
		{
			if (USettings.ARCFileDebugLevel > 1)
				Log.LogDebug("ARCFile.ReadARCToC_NEW({0}) - Reading filename region at offset {1}, size {2}", file.FileName, fileNamesOffset, filenameRegionSize);

			var filenameBuffer = new byte[filenameRegionSize];
			accessor.ReadArray(fileNamesOffset, filenameBuffer, 0, filenameRegionSize);

			// Log first 50 bytes of filename region for debugging
			var firstBytes = BitConverter.ToString(filenameBuffer.Take(Math.Min(50, filenameBuffer.Length)).ToArray());

			if (USettings.ARCFileDebugLevel > 1)
				Log.LogDebug("ARCFile.ReadARCToC_NEW({0}) - First 50 filename bytes: {1}", file.FileName, firstBytes);

			var filenameSpan = filenameBuffer.AsSpan();

			int currentOffset = 0;
			int activeEntriesFound = 0;
			for (int i = 0; i < numEntries; i++)
			{
				// Only Active files have a filename entry
				if (records[i].IsActive)
				{
					activeEntriesFound++;

					if (USettings.ARCFileDebugLevel > 2)
						Log.LogDebug("Reading entry name {0:n0}", i);

					// Use SpanHelper for null-terminated string reading with 0x03 marker handling
					string? filename = SpanHelper.ReadArcNullTerminatedString(filenameSpan, currentOffset, 2048, out int bytesConsumed);

					if (filename is not null && filename.Length > 0)
					{
						records[i].FileName = filename;
					}
					else
					{
						// Inactive or null file - use placeholder
						records[i].FileName = $"Null File {i}";

						if (USettings.ARCFileDebugLevel > 2)
							Log.LogWarning("ARCFile.ReadARCToC_NEW({0}) - Entry {1} got placeholder name (filename=null or empty, bytesConsumed={2})", file.FileName, i, bytesConsumed);
					}

					currentOffset += bytesConsumed;

					if (USettings.ARCFileDebugLevel > 2)
						Log.LogDebug("Name {0} = '{1}'", i, records[i].FileName);
				}
				else
				{
					if (USettings.ARCFileDebugLevel > 2)
						Log.LogDebug("Entry {0} is inactive, skipping filename", i);
				}
			}

			if (USettings.ARCFileDebugLevel > 1)
				Log.LogDebug("ARCFile.ReadARCToC_NEW({0}) - Found {1} active entries during filename parsing", file.FileName, activeEntriesFound);
		}
		else
		{
			if (USettings.ARCFileDebugLevel > 1)
				Log.LogDebug("ARCFile.ReadARCToC_NEW({0}) - SKIP filename region: filenameRegionSize={1}, fileNamesOffset={2}, fileLength={3}",
					file.FileName, filenameRegionSize, fileNamesOffset, fileLength);
		}

		// Build dictionary from active records
		var dictionary = new Dictionary<RecordId, ArcDirEntry>(numEntries);
		int activeCount = 0;
		for (int i = 0; i < numEntries; i++)
		{
			if (records[i].IsActive && records[i].FileName is not null)
			{
				dictionary.Add(records[i].FileName, records[i]);
				activeCount++;
			}
		}

		if (USettings.ARCFileDebugLevel > 0)
			Log.LogDebug("ARCFile.ReadARCToC_NEW({0}) - Built dictionary with {1} active entries out of {2} total", file.FileName, activeCount, numEntries);

		file.DirectoryEntries = dictionary;

		if (USettings.ARCFileDebugLevel > 0)
			Log.LogDebug("Exiting ARCFile.ReadARCToC_NEW()");

		return true;
	}
}
