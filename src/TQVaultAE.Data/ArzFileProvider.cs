using System.Buffers.Binary;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using Microsoft.Extensions.Logging;
using TQVaultAE.Config;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Logs;

namespace TQVaultAE.Data;


public class ArzFileProvider : IArzFileProvider
{
	private readonly ILogger Log;
	private readonly ITQDataService TQData;
	private readonly IRecordInfoProvider infoProv;
	private readonly IFileDataService FileData;
	private readonly UserSettings USettings;


	/// <summary>
	/// Initializes a new instance of the ArzFile class.
	/// </summary>
	public ArzFileProvider(ILogger<ArzFileProvider> log, IRecordInfoProvider recordInfoProvider, ITQDataService tQData, UserSettings uSettings, IFileDataService fileData)
	{
		this.Log = log;
		this.TQData = tQData;
		this.infoProv = recordInfoProvider;
		this.FileData = fileData;
		USettings = uSettings;
	}

	/// <summary>
	/// Reads the ARZ file.
	/// </summary>
	/// <returns>true on success</returns>
	public bool Read(ArzFile file)
	{
		StreamWriter outStream = null;

		if (USettings.DatabaseDebugLevel > 2)
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
			int[] header = new int[6];
			for (int i = 0; i < 6; ++i)
			{
				header[i] = this.FileData.Read<int>(file.FileName, i * 4);
				if (outStream != null)
					outStream.WriteLine("Header[{0}] = {1:n0} (0x{1:X})", i, header[i]);
			}

			int recordTableStart = header[1];
			int recordTableSize = header[2];
			int recordTableCount = header[3];
			int stringTableStart = header[4];
			int stringTableSize = header[5];

			this.ReadStringTable(file, stringTableStart, stringTableSize, outStream);
			this.ReadRecordTable(file, recordTableStart, recordTableSize, recordTableCount, outStream);
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
	public DBRecordCollection GetItem(ArzFile file, RecordId recordId)
	{
		if (recordId is null) return null;

		RecordInfo rawRecord;
		if (file.RecordInfo.ContainsKey(recordId))
			rawRecord = file.RecordInfo[recordId];
		else
			// record not found
			return null;

		return infoProv.Decompress(file, rawRecord);
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
	public DBRecordCollection GetRecordNotCached(ArzFile file, RecordId recordId)
		=> infoProv.Decompress(file, file.RecordInfo[recordId]);


	/// <summary>
	/// Reads the whole string table into memory using ReadOnlySpan for zero-copy parsing.
	/// </summary>
	/// <remarks>
	/// string Table Format
	/// first 4 bytes is the number of entries
	/// then
	/// one string followed by another...
	/// </remarks>
	/// <param name="pos">position within the file.</param>
	/// <param name="outStream">output StreamWriter.</param>
	private void ReadStringTable(ArzFile file, int pos, int size, StreamWriter outStream)
	{
		var data = this.FileData.GetMemory(file.FileName, pos, size);
		ReadOnlySpan<byte> span = data.Span;

		int numstrings = BinaryPrimitives.ReadInt32LittleEndian(span);
		file.Strings = new string[numstrings];

		if (outStream != null)
			outStream.WriteLine("stringTable located at 0x{1:X} numstrings= {0:n0}", numstrings, pos);

		int offset = sizeof(int);
		for (int i = 0; i < numstrings; ++i)
		{
			file.Strings[i] = TQData.ReadCString(span, ref offset);

			if (outStream != null)
				outStream.WriteLine("{0},{1}", i, file.Strings[i]);
		}
	}

	/// <summary>
	/// Reads the entire record table into memory using ReadOnlySpan for zero-copy parsing.
	/// </summary>
	/// <param name="pos">position within the file.</param>
	/// <param name="size">size of the record table in bytes</param>
	/// <param name="numEntries">number of entries in the file.</param>
	/// <param name="outStream">output StreamWriter.</param>
	private void ReadRecordTable(ArzFile file, int pos, int size, int numEntries, StreamWriter outStream)
	{
		var data = this.FileData.GetMemory(file.FileName, pos, size);
		ReadOnlySpan<byte> span = data.Span;

		if (outStream != null)
			outStream.WriteLine("RecordTable located at 0x{0:X}", pos);

		int offset = 0;
		for (int i = 0; i < numEntries; ++i)
		{
			RecordInfo recordInfo = new RecordInfo();

			infoProv.Decode(recordInfo, span, ref offset, 24, file);

			file.RecordInfo.Add(recordInfo.ID, recordInfo);

			if (outStream != null)
				outStream.WriteLine("{0},{1},{2}", i, recordInfo.ID, recordInfo.RecordType);
		}
	}
}