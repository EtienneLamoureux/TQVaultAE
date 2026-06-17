using Microsoft.Extensions.Logging;
using System.Buffers.Binary;
using System.Globalization;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Logs;

namespace TQVaultAE.Data;

public class RecordInfoProvider : IRecordInfoProvider
{
	private readonly ILogger Log;
	private readonly ITQDataService TQData;
	private readonly IFileDataService FileData;
	private readonly IDecompressionService Decompression;

	/// <summary>
	/// Initializes a new instance of the RecordInfo class.
	/// </summary>
	public RecordInfoProvider(ILogger<RecordInfoProvider> log, ITQDataService tQData, IFileDataService fileData, IDecompressionService decompression)
	{
		this.Log = log;
		this.TQData = tQData;
		this.FileData = fileData;
		this.Decompression = decompression;
	}

	/// <summary>
	/// Decodes the ARZ file.
	/// </summary>
	/// <param name="inReader">input BinaryReader</param>
	/// <param name="baseOffset">Offset in the file.</param>
	/// <param name="arzFile">ArzFile instance which we are operating.</param>
	public void Decode(RecordInfo info, BinaryReader inReader, int baseOffset, ArzFile arzFile)
	{
		// Record Entry Format
		// 0x0000 int32 stringEntryID (dbr filename)
		// 0x0004 int32 string length
		// 0x0008 string (record type)
		// 0x00?? int32 offset
		// 0x00?? int32 length in bytes
		// 0x00?? int32 timestamp?
		// 0x00?? int32 timestamp?
		info.IdStringIndex = inReader.ReadInt32();
		info.RecordType = TQData.ReadCString(inReader);
		info.Offset = inReader.ReadInt32() + baseOffset;

		// Compressed size
		// We throw it away and just advance the offset in the file.
		info.CompressedSize = inReader.ReadInt32();

		// Crap1 - timestamp?
		// We throw it away and just advance the offset in the file.
		inReader.ReadInt32();

		// Crap2 - timestamp?
		// We throw it away and just advance the offset in the file.
		inReader.ReadInt32();

		// Get the ID string
		info.ID = arzFile.Getstring(info.IdStringIndex);
	}

	/// <summary>
	/// Decodes the ARZ file using ReadOnlySpan for zero-copy parsing.
	/// Enables bounds-check elimination in high-frequency parsing paths.
	/// </summary>
	/// <param name="info">RecordInfo to populate</param>
	/// <param name="data">ReadOnlySpan of binary data</param>
	/// <param name="offset">Offset that will be advanced by the method</param>
	/// <param name="baseOffset">Base offset to add to the record offset</param>
	/// <param name="arzFile">ArzFile instance which we are operating.</param>
	public void Decode(RecordInfo info, ReadOnlySpan<byte> data, ref int offset, int baseOffset, ArzFile arzFile)
	{
		// Record Entry Format
		// 0x0000 int32 stringEntryID (dbr filename)
		// 0x0004 int32 string length
		// 0x0008 string (record type)
		// 0x00?? int32 offset
		// 0x00?? int32 length in bytes (compressed size)
		// 0x00?? int32 timestamp?
		// 0x00?? int32 timestamp?
		info.IdStringIndex = BinaryPrimitives.ReadInt32LittleEndian(data.Slice(offset));
		offset += sizeof(int);

		info.RecordType = TQData.ReadCString(data, ref offset);

		info.Offset = BinaryPrimitives.ReadInt32LittleEndian(data.Slice(offset)) + baseOffset;
		offset += sizeof(int);

		// Compressed size
		info.CompressedSize = BinaryPrimitives.ReadInt32LittleEndian(data.Slice(offset));
		offset += sizeof(int);

		// Crap1 - timestamp? (We throw it away)
		// Crap2 - timestamp? (We throw it away)
		offset += sizeof(int) * 2;

		info.ID = arzFile.Getstring(info.IdStringIndex);
	}

	/// <summary>
	/// Decompresses an individual record.
	/// </summary>
	/// <param name="arzFile">ARZ file which we are decompressing.</param>
	/// <returns>decompressed DBRecord.</returns>
	public DBRecordCollection Decompress(ArzFile arzFile, RecordInfo info)
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
		byte[] data = this.DecompressBytes(arzFile, info);

		int numberOfDWords = data.Length / 4;

		if (data.Length % 4 != 0)
		{
			var ex = new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Error while parsing arz record {0}, data Length = {1} which is not a multiple of 4", info.ID, (int)data.Length));
			Log.ErrorException(ex);
			throw ex;
		}

		DBRecordCollection record = new DBRecordCollection(info.ID, info.RecordType);

		// Use ReadOnlySpan for bounds-check elimination
		var dataSpan = new ReadOnlySpan<byte>(data);
		int offset = 0;
		int i = 0;
		while (i < numberOfDWords)
		{
			// Read dataType (int16)
			short dataType = BinaryPrimitives.ReadInt16LittleEndian(dataSpan.Slice(offset));
			offset += 2;

			// Read valCount (int16)
			short valCount = BinaryPrimitives.ReadInt16LittleEndian(dataSpan.Slice(offset));
			offset += 2;

			// Read variableID (int32)
			int variableID = BinaryPrimitives.ReadInt32LittleEndian(dataSpan.Slice(offset));
			offset += 4;

			string variableName = arzFile.Getstring(variableID);

			if (variableName == null)
			{
				var ex = new ArgumentNullException(string.Format("Error while parsing arz record {0}, variable is NULL", info.ID));
				Log.LogError("Error in ARZFile - {0}", arzFile.FileName);
				Log.ErrorException(ex);
				throw ex;
			}

			if (dataType < 0 || dataType > 3)
			{
				var ex = new ArgumentOutOfRangeException(string.Format("Error while parsing arz record {0}, variable {1}, bad dataType {2}", info.ID, variableName, dataType));
				Log.LogError("Error in ARZFile - {0}", arzFile.FileName);
				Log.ErrorException(ex);
				throw ex;
			}

			Variable v = new Variable(variableName, (VariableDataType)dataType, valCount);

			if (valCount < 1)
			{
				var ex = new ArgumentException(string.Format("Error while parsing arz record {0}, variable {1}, bad valCount {2}", info.ID, variableName, valCount));
				Log.LogError("Error in ARZFile - {0}", arzFile.FileName);
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
						v[j] = BinaryPrimitives.ReadInt32LittleEndian(dataSpan.Slice(offset));
						offset += 4;
						break;
					case VariableDataType.Float:
						{
							// Convert int32 bits to float using BitConverter
							int intBits = BinaryPrimitives.ReadInt32LittleEndian(dataSpan.Slice(offset));
							byte[] bytes = BitConverter.GetBytes(intBits);
							v[j] = BitConverter.ToSingle(bytes, 0);
							offset += 4;
						}
						break;
					case VariableDataType.StringVar:
						int id = BinaryPrimitives.ReadInt32LittleEndian(dataSpan.Slice(offset));
						offset += 4;
						v[j] = arzFile.Getstring(id)?.Trim() ?? string.Empty;
						break;
					default:
						v[j] = BinaryPrimitives.ReadInt32LittleEndian(dataSpan.Slice(offset));
						offset += 4;
						break;
				}
			}

			record.Set(v);
		}

		return record;
	}

	/// <summary>
	/// Decompresses the ARZ file into an array of bytes.
	/// </summary>
	/// <param name="arzFile">ArzFile which we are decompressing.</param>
	/// <returns>Returns a byte array containing the raw data.</returns>
	private byte[] DecompressBytes(ArzFile arzFile, RecordInfo info)
	{
		if (arzFile == null)
			throw new ArgumentNullException("arzFile", "arzFile is null.");

		if (info.CompressedSize <= 0)
			return Array.Empty<byte>();

		var compressedData = this.FileData.GetReadOnlySpan(arzFile.FileName, info.Offset + 2, info.CompressedSize - 2);
		if (compressedData.Length > 0)
		{
			return this.Decompression.DecompressZlib(compressedData);
		}

		return Array.Empty<byte>();
	}
}