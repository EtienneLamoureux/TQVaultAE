//-----------------------------------------------------------------------
// <copyright file="ArzFile.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.Data
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using TQVaultAE.Config;
	using TQVaultAE.Domain.Contracts.Providers;
	using TQVaultAE.Domain.Contracts.Services;
	using TQVaultAE.Domain.Entities;
	using TQVaultAE.Domain.Helpers;
	using TQVaultAE.Logs;

	/// <summary>
	/// Class for decoding Titan Quest ARZ files.
	/// </summary>
	public class ArzFileProvider : IArzFileProvider
	{
		private readonly log4net.ILog Log = null;
		private readonly ITQDataService TQData;
		private readonly IRecordInfoProvider infoProv;


		/// <summary>
		/// Initializes a new instance of the ArzFile class.
		/// </summary>
		public ArzFileProvider(ILogger<ArzFileProvider> log, IRecordInfoProvider recordInfoProvider, ITQDataService tQData)
		{
			this.Log = log.Logger;
			this.TQData = tQData;
			this.infoProv = recordInfoProvider;
		}


		/// <summary>
		/// Gets the list of keys from the recordInfo dictionary.
		/// </summary>
		/// <returns>string array holding the sorted list</returns>
		public string[] GetKeyTable(ArzFile file)
		{
			if (file.Keys == null || file.Keys.Length == 0)
				this.BuildKeyTable(file);

			return (string[])file.Keys.Clone();
		}

		/// <summary>
		/// Reads the ARZ file.
		/// </summary>
		/// <returns>true on success</returns>
		public bool Read(ArzFile file)
		{
			StreamWriter outStream = null;

			if (TQDebug.DatabaseDebugLevel > 2)
				outStream = new StreamWriter("arzOut.txt", false);

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
				using (FileStream instream = new FileStream(file.FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
				using (BinaryReader reader = new BinaryReader(instream))
				{
					try
					{
						int[] header = new int[6];

						for (int i = 0; i < 6; ++i)
						{
							header[i] = reader.ReadInt32();
							if (outStream != null)
								outStream.WriteLine("Header[{0}] = {1:n0} (0x{1:X})", i, header[i]);
						}

						int firstTableStart = header[1];
						int firstTableCount = header[3];
						int secondTableStart = header[4];

						this.ReadStringTable(file, secondTableStart, reader, outStream);
						this.ReadRecordTable(file, firstTableStart, firstTableCount, reader, outStream);

						// 4 final int32's from file
						// first int32 is numstrings in the stringtable
						// second int32 is something ;)
						// 3rd and 4th are crap (timestamps maybe?)
						for (int i = 0; i < 4; ++i)
						{
							int val = reader.ReadInt32();
							if (outStream != null)
								outStream.WriteLine("{0:n0} 0x{0:X}", val);
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
		public DBRecordCollection GetItem(ArzFile file, string recordId)
		{
			if (string.IsNullOrEmpty(recordId)) return null;

			recordId = TQData.NormalizeRecordPath(recordId);

			return file.Cache.GetOrAddAtomic(recordId, k =>
			{
				RecordInfo rawRecord;
				if (file.RecordInfo.ContainsKey(k))
					rawRecord = file.RecordInfo[k].Value;
				else
					// record not found
					return null;

				return infoProv.Decompress(file, rawRecord);
			});
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
		public DBRecordCollection GetRecordNotCached(ArzFile file, string recordId)
		{
			recordId = TQData.NormalizeRecordPath(recordId);
			return file.Cache.GetOrAddAtomic(recordId, k => infoProv.Decompress(file, file.RecordInfo[k].Value));
		}

		/// <summary>
		/// Builds a list of the keys for this file.  Used to help build the tree structure.
		/// </summary>
		private void BuildKeyTable(ArzFile file)
		{
			if (file.RecordInfo == null || file.RecordInfo.Count == 0)
				return;

			int index = 0;
			file.Keys = new string[file.RecordInfo.Count];
			foreach (string recordID in file.RecordInfo.Keys)
			{
				file.Keys[index] = recordID;
				index++;
			}

			Array.Sort(file.Keys);
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
		private void ReadStringTable(ArzFile file, int pos, BinaryReader reader, StreamWriter outStream)
		{
			reader.BaseStream.Seek(pos, SeekOrigin.Begin);
			int numstrings = reader.ReadInt32();

			file.Strings = new string[numstrings];

			if (outStream != null)
				outStream.WriteLine("stringTable located at 0x{1:X} numstrings= {0:n0}", numstrings, pos);

			for (int i = 0; i < numstrings; ++i)
			{
				file.Strings[i] = TQData.ReadCString(reader);

				if (outStream != null)
					outStream.WriteLine("{0},{1}", i, file.Strings[i]);
			}
		}

		/// <summary>
		/// Reads the entire record table into memory from a stream.
		/// </summary>
		/// <param name="pos">position within the file.</param>
		/// <param name="numEntries">number of entries in the file.</param>
		/// <param name="reader">input BinaryReader</param>
		/// <param name="outStream">output StreamWriter.</param>
		private void ReadRecordTable(ArzFile file, int pos, int numEntries, BinaryReader reader, StreamWriter outStream)
		{
			reader.BaseStream.Seek(pos, SeekOrigin.Begin);

			if (outStream != null)
				outStream.WriteLine("RecordTable located at 0x{0:X}", pos);

			for (int i = 0; i < numEntries; ++i)
			{
				RecordInfo recordInfo = new RecordInfo();

				infoProv.Decode(recordInfo, reader, 24, file); // 24 is the offset of where all record data begins

				file.RecordInfo.TryAdd(TQData.NormalizeRecordPath(recordInfo.ID), new Lazy<RecordInfo>(() => { return recordInfo; }));

				// output this record
				if (outStream != null)
					outStream.WriteLine("{0},{1},{2}", i, recordInfo.ID, recordInfo.RecordType);
			}
		}
	}
}
