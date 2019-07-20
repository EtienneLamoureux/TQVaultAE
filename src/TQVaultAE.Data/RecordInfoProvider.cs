using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using TQVaultAE.Domain.Contracts.Providers;
using TQVaultAE.Domain.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Logs;

namespace TQVaultAE.Data
{

	/// <summary>
	/// Class to encapsulate the actual record information from the file.
	/// Also does the decoding of the raw data.
	/// </summary>
	public class RecordInfoProvider : IRecordInfoProvider
	{
		private readonly log4net.ILog Log = null;
		private readonly ITQDataService TQData;

		/// <summary>
		/// Initializes a new instance of the RecordInfo class.
		/// </summary>
		public RecordInfoProvider(ILogger<RecordInfoProvider> log, ITQDataService tQData)
		{
			this.Log = log.Logger;
			this.TQData = tQData;
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
			inReader.ReadInt32();

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
						var ex = new ArgumentNullException(string.Format(CultureInfo.InvariantCulture, "Error while parsing arz record {0}, variable is NULL", info.ID));
						Log.ErrorFormat(CultureInfo.InvariantCulture, "Error in ARZFile - {0}", arzFile.FileName);
						Log.ErrorException(ex);
						throw ex;
					}

					if (dataType < 0 || dataType > 3)
					{
						var ex = new ArgumentOutOfRangeException(string.Format(CultureInfo.InvariantCulture, "Error while parsing arz record {0}, variable {1}, bad dataType {2}", info.ID, variableName, dataType));
						Log.ErrorFormat(CultureInfo.InvariantCulture, "Error in ARZFile - {0}", arzFile.FileName);
						Log.ErrorException(ex);
						throw ex;
					}

					Variable v = new Variable(variableName, (VariableDataType)dataType, valCount);

					if (valCount < 1)
					{
						var ex = new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Error while parsing arz record {0}, variable {1}, bad valCount {2}", info.ID, variableName, valCount));
						Log.ErrorFormat(CultureInfo.InvariantCulture, "Error in ARZFile - {0}", arzFile.FileName);
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
								v[j] = inReader.ReadInt32();
								break;
							case VariableDataType.Float:
								v[j] = inReader.ReadSingle();
								break;
							case VariableDataType.StringVar:
								int id = inReader.ReadInt32();
								v[j] = arzFile.Getstring(id)?.Trim() ?? string.Empty;
								break;
							default:
								v[j] = inReader.ReadInt32();
								break;
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
		private byte[] DecompressBytes(ArzFile arzFile, RecordInfo info)
		{
			if (arzFile == null)
				throw new ArgumentNullException("arzFile", "arzFile is null.");

			// Read in the compressed data and decompress it, storing the results in a memorystream
			using (FileStream arzStream = new FileStream(arzFile.FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				arzStream.Seek(info.Offset, SeekOrigin.Begin);

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

}
