//-----------------------------------------------------------------------
// <copyright file="ArcFile.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.Data
{
	using Microsoft.Extensions.Logging;
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.IO.Compression;
	using System.Linq;
	using System.Text;
	using TQVaultAE.Config;
	using TQVaultAE.Domain.Contracts.Providers;
	using TQVaultAE.Domain.Contracts.Services;
	using TQVaultAE.Domain.Entities;
	using TQVaultAE.Domain.Helpers;
	using TQVaultAE.Logs;

	/// <summary>
	/// Reads and decodes a Titan Quest ARC file.
	/// </summary>
	public class ArcFileProvider : IArcFileProvider
	{
		private readonly ILogger Log = null;
		private readonly ITQDataService TQData;

		/// <summary>
		/// Ctr
		/// </summary>
		/// <param name="fileName">File Name of the ARC file to be read.</param>
		public ArcFileProvider(ILogger<ArcFileProvider> log, ITQDataService tQData)
		{
			this.Log = log;
			this.TQData = tQData;
		}

		/// <summary>
		/// Gets the sorted list of directoryEntries.
		/// </summary>
		/// <returns>string array holding the sorted list</returns>
		public RecordId[] GetKeyTable(ArcFile file)
		{
			if (file.Keys == null || file.Keys.Length == 0)
				this.BuildKeyTable(file);

			return (RecordId[])file.Keys.Clone();
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

				return file.DirectoryEntries != null;
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

				string dataID = string.Concat(Path.GetFileNameWithoutExtension(file.FileName), "\\", record.Raw);
				byte[] data = this.GetData(file, dataID);
				if (data == null)
					return;

				string destination = baseFolder;
				if (!destination.EndsWith("\\", StringComparison.OrdinalIgnoreCase))
					destination = string.Concat(destination, "\\");

				destination = string.Concat(destination, destinationFileName);

				// If there is a sub directory in the arc file then we need to create it.
				if (!Directory.Exists(Path.GetDirectoryName(destination)))
					Directory.CreateDirectory(Path.GetDirectoryName(destination));

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
			if (TQDebug.ArcFileDebugLevel > 0)
				Log.LogDebug("ARCFile.GetData({0})", dataId);

			if (!file.FileHasBeenRead)
				this.ReadARCToC(file);

			if (file.DirectoryEntries == null)
			{
				if (TQDebug.ArcFileDebugLevel > 1)
					Log.LogDebug("Error - Could not read {0}", file.FileName);

				// could not read the file
				return null;
			}

			if (TQDebug.ArcFileDebugLevel > 1)
				Log.LogDebug("Normalized dataID = {0}", dataId);

			// Find our file in the toc.
			// First strip off the leading folder since it is just the ARC name
			int firstPathDelim = dataId.Normalized.IndexOf('\\');
			if (firstPathDelim != -1)
				dataId = dataId.Normalized.Substring(firstPathDelim + 1);

			// Now see if this file is in the toc.
			ArcDirEntry directoryEntry;

			if (file.DirectoryEntries.ContainsKey(dataId))
				directoryEntry = file.DirectoryEntries[dataId];
			else
			{
				// record not found
				if (TQDebug.ArcFileDebugLevel > 1)
					Log.LogDebug("Error - {0} not found.", dataId);

				return null;
			}

			// Now open the ARC file and read in the record.
			using (FileStream arcFile = new FileStream(file.FileName, FileMode.Open, FileAccess.Read))
			{
				// Allocate memory for the uncompressed data
				byte[] data = new byte[directoryEntry.RealSize];

				// Now process each part of this record
				int startPosition = 0;

				// First see if the data was just stored without compression.
				if ((directoryEntry.StorageType == 1) && (directoryEntry.CompressedSize == directoryEntry.RealSize))
				{
					if (TQDebug.ArcFileDebugLevel > 1)
					{
						Log.LogDebug("Offset={0}  Size={1}"
							, directoryEntry.FileOffset
							, directoryEntry.RealSize
						);
					}

					arcFile.Seek(directoryEntry.FileOffset, SeekOrigin.Begin);
					arcFile.Read(data, 0, directoryEntry.RealSize);
				}
				else
				{
					// The data was compressed so we attempt to decompress it.
					foreach (ArcPartEntry partEntry in directoryEntry.Parts)
					{
						// seek to the part we want
						arcFile.Seek(partEntry.FileOffset, SeekOrigin.Begin);

						// Ignore the zlib compression method.
						arcFile.ReadByte();

						// Ignore the zlib compression flags.
						arcFile.ReadByte();

						// Create a deflate stream.
						using (DeflateStream deflate = new DeflateStream(arcFile, CompressionMode.Decompress, true))
						{
							int bytesRead;
							int partLength = 0;
							while ((bytesRead = deflate.Read(data, startPosition, data.Length - startPosition)) > 0)
							{
								startPosition += bytesRead;
								partLength += bytesRead;

								// break out of the read loop if we have processed this part completely.
								if (partLength >= partEntry.RealSize)
									break;
							}
						}
					}
				}

				if (TQDebug.ArcFileDebugLevel > 0)
					Log.LogDebug("Exiting ARCFile.GetData()");

				return data;
			}
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
				if (TQDebug.ArcFileDebugLevel > 0)
					Log.LogDebug("ARCFile.ReadARCFile()");

				if (!file.FileHasBeenRead)
					this.ReadARCToC(file);

				foreach (ArcDirEntry dirEntry in file.DirectoryEntries.Values)
				{
					RecordId dataID = string.Concat(
						Path.GetFileNameWithoutExtension(file.FileName), "\\"
						, dirEntry.FileName.Raw
					);

					if (TQDebug.ArcFileDebugLevel > 1)
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
					if (!Directory.Exists(Path.GetDirectoryName(filename)))
						Directory.CreateDirectory(Path.GetDirectoryName(filename));

					if (TQDebug.ArcFileDebugLevel > 1)
						Log.LogDebug($"Creating File - {filename}");

					using (FileStream outStream = new FileStream(filename, FileMode.Create, FileAccess.Write))
					{
						outStream.Write(data, 0, data.Length);
					}
				}

				if (TQDebug.ArcFileDebugLevel > 0)
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
		/// Builds a sorted list of entries in the directoryEntries dictionary.  Used to build a tree structure of the names.
		/// </summary>
		private void BuildKeyTable(ArcFile file)
		{
			if (file.DirectoryEntries == null || file.DirectoryEntries.Count == 0)
				return;

			int index = 0;
			file.Keys = new RecordId[file.DirectoryEntries.Count];
			foreach (RecordId filename in file.DirectoryEntries.Keys)
			{
				file.Keys[index] = filename;
				index++;
			}

			Array.Sort(file.Keys);
		}

		/// <summary>
		/// Read the table of contents of the ARC file
		/// </summary>
		public void ReadARCToC(ArcFile file)
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

			if (TQDebug.ArcFileDebugLevel > 0)
				Log.LogDebug("ARCFile.ReadARCToC({0})", file.FileName);

			try
			{
				using (FileStream arcFile = new FileStream(file.FileName, FileMode.Open, FileAccess.Read))
				{
					using (BinaryReader reader = new BinaryReader(arcFile))
					{
						if (TQDebug.ArcFileDebugLevel > 1)
							Log.LogDebug("File Length={0}", arcFile.Length);

						// check the file header
						if (reader.ReadByte() != 0x41)
							return;

						if (reader.ReadByte() != 0x52)
							return;

						if (reader.ReadByte() != 0x43)
							return;

						if (arcFile.Length < 0x21)
							return;

						reader.BaseStream.Seek(0x08, SeekOrigin.Begin);
						int numEntries = reader.ReadInt32();
						int numParts = reader.ReadInt32();

						if (TQDebug.ArcFileDebugLevel > 1)
							Log.LogDebug("numEntries={0}, numParts={1}", numEntries, numParts);

						ArcPartEntry[] parts = new ArcPartEntry[numParts];
						ArcDirEntry[] records = new ArcDirEntry[numEntries];

						if (TQDebug.ArcFileDebugLevel > 2)
							Log.LogDebug("Seeking to tocOffset location");

						reader.BaseStream.Seek(0x18, SeekOrigin.Begin);
						int tocOffset = reader.ReadInt32();

						if (TQDebug.ArcFileDebugLevel > 1)
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

							if (TQDebug.ArcFileDebugLevel > 2)
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

						if (TQDebug.ArcFileDebugLevel > 1)
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

							if (TQDebug.ArcFileDebugLevel > 2)
								Log.LogDebug("StorageType={0}", storageType);

							// Added by VillageIdiot to support stored types
							records[i].StorageType = storageType;
							records[i].FileOffset = reader.ReadInt32();
							records[i].CompressedSize = reader.ReadInt32();
							records[i].RealSize = reader.ReadInt32();
							int crap = reader.ReadInt32(); // crap
							if (TQDebug.ArcFileDebugLevel > 2)
								Log.LogDebug("Crap2={0}", crap);

							crap = reader.ReadInt32(); // crap
							if (TQDebug.ArcFileDebugLevel > 2)
								Log.LogDebug("Crap3={0}", crap);

							crap = reader.ReadInt32(); // crap
							if (TQDebug.ArcFileDebugLevel > 2)
								Log.LogDebug("Crap4={0}", crap);

							int numberOfParts = reader.ReadInt32();
							if (numberOfParts < 1)
							{
								records[i].Parts = null;
								if (TQDebug.ArcFileDebugLevel > 2)
									Log.LogDebug("File {0} is not compressed.", i);
							}
							else
								records[i].Parts = new ArcPartEntry[numberOfParts];

							int firstPart = reader.ReadInt32();
							crap = reader.ReadInt32(); // filename length
							if (TQDebug.ArcFileDebugLevel > 2)
								Log.LogDebug("Filename Length={0}", crap);

							crap = reader.ReadInt32(); // filename offset
							if (TQDebug.ArcFileDebugLevel > 2)
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
						byte[] buffer = new byte[2048];
						ASCIIEncoding ascii = new ASCIIEncoding();
						for (i = 0; i < numEntries; ++i)
						{
							// only Active files have a filename entry
							if (records[i].IsActive)
							{
								// For each string, read bytes until I hit a 0x00 byte.
								if (TQDebug.ArcFileDebugLevel > 2)
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
										if (TQDebug.ArcFileDebugLevel > 2)
											Log.LogDebug("Null file - inactive?");

										break;
									}

									if (bufferSize >= buffer.Length)
									{
										Log.LogDebug("ARCFile.ReadARCToC() Error - Buffer size of 2048 has been exceeded.");
										if (TQDebug.ArcFileDebugLevel > 2)
										{
											var content = buffer.Select(b => string.Format(CultureInfo.InvariantCulture, "0x{0:X}", b)).ToArray();
											Log.LogDebug($"Buffer contents:{Environment.NewLine}{string.Join(string.Empty, content)}{Environment.NewLine}{string.Empty}");
										}
									}
								}

								if (TQDebug.ArcFileDebugLevel > 2)
								{
									Log.LogDebug("Read {0:n0} bytes for name.  Converting to string.", bufferSize);
								}

								string newfile;
								if (bufferSize >= 1)
								{
									// Now convert the buffer to a string
									char[] chars = new char[ascii.GetCharCount(buffer, 0, bufferSize - 1)];
									ascii.GetChars(buffer, 0, bufferSize - 1, chars, 0);
									newfile = new string(chars);
								}
								else
									newfile = string.Format(CultureInfo.InvariantCulture, "Null File {0}", i);

								records[i].FileName = newfile;

								if (TQDebug.ArcFileDebugLevel > 2)
									Log.LogDebug("Name {0:n0} = '{1}'", i, records[i].FileName);
							}
						}

						// Now convert the array of records into a Dictionary.
						var dictionary = new Dictionary<RecordId, ArcDirEntry>(numEntries);

						if (TQDebug.ArcFileDebugLevel > 1)
							Log.LogDebug("Creating Dictionary");

						for (i = 0; i < numEntries; ++i)
						{
							if (records[i].IsActive)
								dictionary.Add(records[i].FileName, records[i]);
						}

						file.DirectoryEntries = dictionary;

						if (TQDebug.ArcFileDebugLevel > 0)
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
	}
}