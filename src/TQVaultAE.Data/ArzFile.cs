//-----------------------------------------------------------------------
// <copyright file="ArzFile.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.Data
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.IO.Compression;
	using TQVaultAE.Config;
	using TQVaultAE.Entities;
	using TQVaultAE.Logs;

	/// <summary>
	/// Class for decoding Titan Quest ARZ files.
	/// </summary>
	public class ArzFile
	{
		private readonly log4net.ILog Log = null;

		/// <summary>
		/// Name of the ARZ file.
		/// </summary>
		private string fileName;

		/// <summary>
		/// String table
		/// </summary>
		private string[] strings;

		/// <summary>
		/// RecordInfo keyed by their ID
		/// </summary>
		private Dictionary<string, RecordInfo> recordInfo;

		/// <summary>
		/// DBRecords cached as they get requested
		/// </summary>
		private Dictionary<string, DBRecordCollection> cache;

		/// <summary>
		/// Holds the keys for the recordInfo Dictionary
		/// </summary>
		private string[] keys;

		/// <summary>
		/// Initializes a new instance of the ArzFile class.
		/// </summary>
		/// <param name="fileName">name of the ARZ file.</param>
		public ArzFile(string fileName)
		{
			this.Log = Logger.Get(this);

			this.fileName = fileName;
			this.cache = new Dictionary<string, DBRecordCollection>();
		}

		/// <summary>
		/// Gets the number of DBRecords
		/// </summary>
		public int Count
		{
			get
			{
				return this.recordInfo.Count;
			}
		}

		/// <summary>
		/// Gets the list of keys from the recordInfo dictionary.
		/// </summary>
		/// <returns>string array holding the sorted list</returns>
		public string[] GetKeyTable()
		{
			if (this.keys == null || this.keys.Length == 0)
			{
				this.BuildKeyTable();
			}

			return (string[])this.keys.Clone();
		}

		/// <summary>
		/// Reads the ARZ file.
		/// </summary>
		/// <returns>true on success</returns>
		public bool Read()
		{
			StreamWriter outStream = null;

			if (TQDebug.DatabaseDebugLevel > 2)
			{
				outStream = new StreamWriter("arzOut.txt", false);
			}

			try
			{
				// ARZ header file format
				//
				// 0x000000 int32
				// 0x000004 int32 start of dbRecord table
				// 0x000008 int32 size in bytes of dbRecord table
				// 0x00000c int32 numEntries in dbRecord table
				// 0x000010 int32 start of string table
				// 0x000014 int32 size in bytes of string table
				using (FileStream instream = new FileStream(this.fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
				using (BinaryReader reader = new BinaryReader(instream))
				{
					try
					{
						int[] header = new int[6];

						for (int i = 0; i < 6; ++i)
						{
							header[i] = reader.ReadInt32();
							if (outStream != null)
							{
								outStream.WriteLine("Header[{0}] = {1:n0} (0x{1:X})", i, header[i]);
							}
						}

						int firstTableStart = header[1];
						int firstTableCount = header[3];
						int secondTableStart = header[4];

						this.ReadStringTable(secondTableStart, reader, outStream);
						this.ReadRecordTable(firstTableStart, firstTableCount, reader, outStream);

						// 4 final int32's from file
						// first int32 is numstrings in the stringtable
						// second int32 is something ;)
						// 3rd and 4th are crap (timestamps maybe?)
						for (int i = 0; i < 4; ++i)
						{
							int val = reader.ReadInt32();
							if (outStream != null)
							{
								outStream.WriteLine("{0:n0} 0x{0:X}", val);
							}
						}
					}
					catch (IOException ex)
					{
						Log.ErrorException(ex);
						throw;
					}
				}
			}
			catch (IOException exception)
			{
				Log.ErrorException(exception);
				return false;
			}
			finally
			{
				if (outStream != null)
				{
					outStream.Close();
				}
			}

			return true;
		}

		/// <summary>
		/// Gets the DBRecord for a particular ID.
		/// </summary>
		/// <param name="recordId">string ID of the record will be normalized internally</param>
		/// <returns>DBRecord corresponding to the string ID.</returns>
		public DBRecordCollection GetItem(string recordId)
		{
			if (string.IsNullOrEmpty(recordId)) return null;

			DBRecordCollection databaseRecord;

			recordId = TQData.NormalizeRecordPath(recordId);

			if (cache.ContainsKey(recordId))
			{
				databaseRecord = this.cache[recordId];
			}
			else
			{
				RecordInfo rawRecord;
				if (this.recordInfo.ContainsKey(recordId))
				{
					rawRecord = this.recordInfo[recordId];
				}
				else
				{
					// record not found
					return null;
				}

				databaseRecord = rawRecord.Decompress(this);
				this.cache.Add(recordId, databaseRecord);
			}

			return databaseRecord;
		}

		/// <summary>
		/// Gets a database record without adding it to the cache.
		/// </summary>
		/// <remarks>
		/// The Item property caches the DBRecords, which is great when you are only using a few 100 (1000?) records and are requesting
		/// them many times.  Not great if you are looping through all the records as it eats alot of memory.  This method will create
		/// the record on the fly if it is not in the cache so when you are done with it, it can be reclaimed by the garbage collector.
		/// Great for when you want to loop through all the records for some reason.  It will take longer, but use less memory.
		/// </remarks>
		/// <param name="recordId">String ID of the record.  Will be normalized internally.</param>
		/// <returns>Decompressed RecordInfo record</returns>
		public DBRecordCollection GetRecordNotCached(string recordId)
		{
			recordId = TQData.NormalizeRecordPath(recordId);

			try
			{
				// If it is already in the cache no need not to use it
				return this.cache[recordId];
			}
			catch (KeyNotFoundException ex)
			{
				Log.Debug("record not found first attempt", ex);
				try
				{
					return this.recordInfo[recordId].Decompress(this);
				}
				catch (KeyNotFoundException exx)
				{
					Log.Debug("record not found second attempt", exx);
					return null;
				}
			}
		}

		/// <summary>
		/// Builds a list of the keys for this file.  Used to help build the tree structure.
		/// </summary>
		private void BuildKeyTable()
		{
			if (this.recordInfo == null || this.recordInfo.Count == 0)
			{
				return;
			}

			int index = 0;
			this.keys = new string[this.recordInfo.Count];
			foreach (string recordID in this.recordInfo.Keys)
			{
				this.keys[index] = recordID;
				index++;
			}

			Array.Sort(this.keys);
		}

		/// <summary>
		/// Retrieves a string from the string table.
		/// </summary>
		/// <param name="index">Offset in the string table.</param>
		/// <returns>string from the string table</returns>
		private string Getstring(int index)
		{
			return this.strings[index];
		}

		/// <summary>
		/// Reads the whole string table into memory from a stream.
		/// </summary>
		/// <remarks>
		/// string Table Format
		/// first 4 bytes is the number of entries
		/// then
		/// one string followed by another...
		/// </remarks>
		/// <param name="pos">position within the file.</param>
		/// <param name="reader">input BinaryReader</param>
		/// <param name="outStream">output StreamWriter.</param>
		private void ReadStringTable(int pos, BinaryReader reader, StreamWriter outStream)
		{
			reader.BaseStream.Seek(pos, SeekOrigin.Begin);
			int numstrings = reader.ReadInt32();

			this.strings = new string[numstrings];

			if (outStream != null)
			{
				outStream.WriteLine("stringTable located at 0x{1:X} numstrings= {0:n0}", numstrings, pos);
			}

			for (int i = 0; i < numstrings; ++i)
			{
				this.strings[i] = TQData.ReadCString(reader);

				if (outStream != null)
				{
					outStream.WriteLine("{0},{1}", i, this.strings[i]);
				}
			}
		}

		/// <summary>
		/// Reads the entire record table into memory from a stream.
		/// </summary>
		/// <param name="pos">position within the file.</param>
		/// <param name="numEntries">number of entries in the file.</param>
		/// <param name="reader">input BinaryReader</param>
		/// <param name="outStream">output StreamWriter.</param>
		private void ReadRecordTable(int pos, int numEntries, BinaryReader reader, StreamWriter outStream)
		{
			this.recordInfo = new Dictionary<string, RecordInfo>((int)Math.Round(numEntries * 1.2));
			reader.BaseStream.Seek(pos, SeekOrigin.Begin);

			if (outStream != null)
			{
				outStream.WriteLine("RecordTable located at 0x{0:X}", pos);
			}

			for (int i = 0; i < numEntries; ++i)
			{
				RecordInfo recordInfo = new RecordInfo();
				recordInfo.Decode(reader, 24, this); // 24 is the offset of where all record data begins

				this.recordInfo.Add(TQData.NormalizeRecordPath(recordInfo.ID), recordInfo);

				// output this record
				if (outStream != null)
				{
					outStream.WriteLine("{0},{1},{2}", i, recordInfo.ID, recordInfo.RecordType);
				}
			}
		}

		#region RecordInfo

		/// <summary>
		/// Class to encapsulate the actual record information from the file.
		/// Also does the decoding of the raw data.
		/// </summary>
		private class RecordInfo
		{
			private readonly log4net.ILog Log = null;

			/// <summary>
			/// Offset in the file for this record.
			/// </summary>
			private int offset;

			/// <summary>
			/// String index of ID
			/// </summary>
			private int idStringIndex;

			/// <summary>
			/// Initializes a new instance of the RecordInfo class.
			/// </summary>
			public RecordInfo()
			{
				this.Log = Logger.Get(this);

				this.idStringIndex = -1;
				this.RecordType = string.Empty;
			}

			/// <summary>
			/// Gets the string ID
			/// </summary>
			public string ID { get; private set; }

			/// <summary>
			/// Gets the Record type.
			/// </summary>
			public string RecordType { get; private set; }

			/// <summary>
			/// Decodes the ARZ file.
			/// </summary>
			/// <param name="inReader">input BinaryReader</param>
			/// <param name="baseOffset">Offset in the file.</param>
			/// <param name="arzFile">ArzFile instance which we are operating.</param>
			public void Decode(BinaryReader inReader, int baseOffset, ArzFile arzFile)
			{
				// Record Entry Format
				// 0x0000 int32 stringEntryID (dbr filename)
				// 0x0004 int32 string length
				// 0x0008 string (record type)
				// 0x00?? int32 offset
				// 0x00?? int32 length in bytes
				// 0x00?? int32 timestamp?
				// 0x00?? int32 timestamp?
				this.idStringIndex = inReader.ReadInt32();
				this.RecordType = TQData.ReadCString(inReader);
				this.offset = inReader.ReadInt32() + baseOffset;

				// Compressed size
				// We throw it away and just advance the offset in the file.
				inReader.ReadInt32();

				// Crap1 - timestamp?
				// We throw it away and just advance the offset in the file.
				inReader.ReadInt32();

				// Crap2 - timestamp?
				// We throw it away and just advance the offset in the file.
				inReader.ReadInt32();

				// Get the ID string
				this.ID = arzFile.Getstring(this.idStringIndex);
			}

			/// <summary>
			/// Decompresses an individual record.
			/// </summary>
			/// <param name="arzFile">ARZ file which we are decompressing.</param>
			/// <returns>decompressed DBRecord.</returns>
			public DBRecordCollection Decompress(ArzFile arzFile)
			{
				// record variables have this format:
				// 0x00 int16 specifies data type:
				//      0x0000 = int - data will be an int32
				//      0x0001 = float - data will be a Single
				//      0x0002 = string - data will be an int32 that is index into string table
				//      0x0003 = bool - data will be an int32
				// 0x02 int16 specifies number of values (usually 1, but sometimes more (for arrays)
				// 0x04 int32 key string ID (the id into the string table for this variable name
				// 0x08 data value
				byte[] data = this.DecompressBytes(arzFile);

				int numberOfDWords = data.Length / 4;

				if (data.Length % 4 != 0)
				{
					var ex = new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Error while parsing arz record {0}, data Length = {1} which is not a multiple of 4", this.ID, (int)data.Length));
					Log.ErrorException(ex);
					throw ex;
				}

				DBRecordCollection record = new DBRecordCollection(this.ID, this.RecordType);

				// Create a memory stream to read the binary data
				using (BinaryReader inReader = new BinaryReader(new MemoryStream(data, false)))
				{
					int i = 0;
					while (i < numberOfDWords)
					{
						short dataType = inReader.ReadInt16();
						short valCount = inReader.ReadInt16();
						int variableID = inReader.ReadInt32();
						string variableName = arzFile.Getstring(variableID);

						if (variableName == null)
						{
							var ex = new ArgumentNullException(string.Format(CultureInfo.InvariantCulture, "Error while parsing arz record {0}, variable is NULL", this.ID));
							Log.ErrorFormat(CultureInfo.InvariantCulture, "Error in ARZFile - {0}", arzFile.fileName);
							Log.ErrorException(ex);
							throw ex;
						}

						if (dataType < 0 || dataType > 3)
						{
							var ex = new ArgumentOutOfRangeException(string.Format(CultureInfo.InvariantCulture, "Error while parsing arz record {0}, variable {1}, bad dataType {2}", this.ID, variableName, dataType));
							Log.ErrorFormat(CultureInfo.InvariantCulture, "Error in ARZFile - {0}", arzFile.fileName);
							Log.ErrorException(ex);
							throw ex;
						}

						Variable v = new Variable(variableName, (VariableDataType)dataType, valCount);

						if (valCount < 1)
						{
							var ex = new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Error while parsing arz record {0}, variable {1}, bad valCount {2}", this.ID, variableName, valCount));
							Log.ErrorFormat(CultureInfo.InvariantCulture, "Error in ARZFile - {0}", arzFile.fileName);
							Log.ErrorException(ex);
							throw ex;
						}

						// increment our dword count
						i += 2 + valCount;

						for (int j = 0; j < valCount; ++j)
						{
							switch (v.DataType)
							{
								case VariableDataType.Integer:
								case VariableDataType.Boolean:
									{
										int val = inReader.ReadInt32();
										v[j] = val;
										break;
									}

								case VariableDataType.Float:
									{
										float val = inReader.ReadSingle();
										v[j] = val;
										break;
									}

								case VariableDataType.StringVar:
									{
										int id = inReader.ReadInt32();
										string val = arzFile.Getstring(id);
										if (val == null)
										{
											val = string.Empty;
										}
										else
										{
											val = val.Trim();
										}

										v[j] = val;
										break;
									}

								default:
									{
										int val = inReader.ReadInt32();
										v[j] = val;
										break;
									}
							}
						}

						record.Set(v);
					}
				}

				return record;
			}

			/// <summary>
			/// Decompresses the ARZ file into an array of bytes.
			/// </summary>
			/// <param name="arzFile">ArzFile which we are decompressing.</param>
			/// <returns>Returns a byte array containing the raw data.</returns>
			private byte[] DecompressBytes(ArzFile arzFile)
			{
				if (arzFile == null)
				{
					throw new ArgumentNullException("arzFile", "arzFile is null.");
				}

				// Read in the compressed data and decompress it, storing the results in a memorystream
				using (FileStream arzStream = new FileStream(arzFile.fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					arzStream.Seek(this.offset, SeekOrigin.Begin);

					// Ignore the zlib compression method.
					arzStream.ReadByte();

					// Ignore the zlib compression flags.
					arzStream.ReadByte();

					// Create a deflate stream.
					using (DeflateStream deflate = new DeflateStream(arzStream, CompressionMode.Decompress))
					{
						// Create a memorystream to hold the decompressed data
						using (MemoryStream outStream = new MemoryStream())
						{
							// Now decompress
							byte[] buffer = new byte[1024];
							int len;
							while ((len = deflate.Read(buffer, 0, 1024)) > 0)
							{
								outStream.Write(buffer, 0, len);
							}

							// Return the decompressed data
							return outStream.ToArray();
						}
					}
				}
			}
		}

		#endregion RecordInfo
	}
}
