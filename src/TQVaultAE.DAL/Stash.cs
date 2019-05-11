//-----------------------------------------------------------------------
// <copyright file="Stash.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.DAL
{
	using System;
	using System.Globalization;
	using System.IO;
	using TQVaultAE.Logging;

	/// <summary>
	/// Class for handling the stash file
	/// </summary>
	public class Stash
	{
		private readonly log4net.ILog Log = null;

		/// <summary>
		/// Defines the raw data buffer size
		/// </summary>
		private const int BUFFERSIZE = 1024;

		/// <summary>
		/// CRC32 hash table.  Used for calculating the file CRC
		/// </summary>
		private static uint[] crc32Table =
		{
			0x00000000, 0x77073096, 0xee0e612c, 0x990951ba, 0x076dc419, 0x706af48f,
			0xe963a535, 0x9e6495a3, 0x0edb8832, 0x79dcb8a4, 0xe0d5e91e, 0x97d2d988,
			0x09b64c2b, 0x7eb17cbd, 0xe7b82d07, 0x90bf1d91, 0x1db71064, 0x6ab020f2,
			0xf3b97148, 0x84be41de, 0x1adad47d, 0x6ddde4eb, 0xf4d4b551, 0x83d385c7,
			0x136c9856, 0x646ba8c0, 0xfd62f97a, 0x8a65c9ec, 0x14015c4f, 0x63066cd9,
			0xfa0f3d63, 0x8d080df5, 0x3b6e20c8, 0x4c69105e, 0xd56041e4, 0xa2677172,
			0x3c03e4d1, 0x4b04d447, 0xd20d85fd, 0xa50ab56b, 0x35b5a8fa, 0x42b2986c,
			0xdbbbc9d6, 0xacbcf940, 0x32d86ce3, 0x45df5c75, 0xdcd60dcf, 0xabd13d59,
			0x26d930ac, 0x51de003a, 0xc8d75180, 0xbfd06116, 0x21b4f4b5, 0x56b3c423,
			0xcfba9599, 0xb8bda50f, 0x2802b89e, 0x5f058808, 0xc60cd9b2, 0xb10be924,
			0x2f6f7c87, 0x58684c11, 0xc1611dab, 0xb6662d3d, 0x76dc4190, 0x01db7106,
			0x98d220bc, 0xefd5102a, 0x71b18589, 0x06b6b51f, 0x9fbfe4a5, 0xe8b8d433,
			0x7807c9a2, 0x0f00f934, 0x9609a88e, 0xe10e9818, 0x7f6a0dbb, 0x086d3d2d,
			0x91646c97, 0xe6635c01, 0x6b6b51f4, 0x1c6c6162, 0x856530d8, 0xf262004e,
			0x6c0695ed, 0x1b01a57b, 0x8208f4c1, 0xf50fc457, 0x65b0d9c6, 0x12b7e950,
			0x8bbeb8ea, 0xfcb9887c, 0x62dd1ddf, 0x15da2d49, 0x8cd37cf3, 0xfbd44c65,
			0x4db26158, 0x3ab551ce, 0xa3bc0074, 0xd4bb30e2, 0x4adfa541, 0x3dd895d7,
			0xa4d1c46d, 0xd3d6f4fb, 0x4369e96a, 0x346ed9fc, 0xad678846, 0xda60b8d0,
			0x44042d73, 0x33031de5, 0xaa0a4c5f, 0xdd0d7cc9, 0x5005713c, 0x270241aa,
			0xbe0b1010, 0xc90c2086, 0x5768b525, 0x206f85b3, 0xb966d409, 0xce61e49f,
			0x5edef90e, 0x29d9c998, 0xb0d09822, 0xc7d7a8b4, 0x59b33d17, 0x2eb40d81,
			0xb7bd5c3b, 0xc0ba6cad, 0xedb88320, 0x9abfb3b6, 0x03b6e20c, 0x74b1d29a,
			0xead54739, 0x9dd277af, 0x04db2615, 0x73dc1683, 0xe3630b12, 0x94643b84,
			0x0d6d6a3e, 0x7a6a5aa8, 0xe40ecf0b, 0x9309ff9d, 0x0a00ae27, 0x7d079eb1,
			0xf00f9344, 0x8708a3d2, 0x1e01f268, 0x6906c2fe, 0xf762575d, 0x806567cb,
			0x196c3671, 0x6e6b06e7, 0xfed41b76, 0x89d32be0, 0x10da7a5a, 0x67dd4acc,
			0xf9b9df6f, 0x8ebeeff9, 0x17b7be43, 0x60b08ed5, 0xd6d6a3e8, 0xa1d1937e,
			0x38d8c2c4, 0x4fdff252, 0xd1bb67f1, 0xa6bc5767, 0x3fb506dd, 0x48b2364b,
			0xd80d2bda, 0xaf0a1b4c, 0x36034af6, 0x41047a60, 0xdf60efc3, 0xa867df55,
			0x316e8eef, 0x4669be79, 0xcb61b38c, 0xbc66831a, 0x256fd2a0, 0x5268e236,
			0xcc0c7795, 0xbb0b4703, 0x220216b9, 0x5505262f, 0xc5ba3bbe, 0xb2bd0b28,
			0x2bb45a92, 0x5cb36a04, 0xc2d7ffa7, 0xb5d0cf31, 0x2cd99e8b, 0x5bdeae1d,
			0x9b64c2b0, 0xec63f226, 0x756aa39c, 0x026d930a, 0x9c0906a9, 0xeb0e363f,
			0x72076785, 0x05005713, 0x95bf4a82, 0xe2b87a14, 0x7bb12bae, 0x0cb61b38,
			0x92d28e9b, 0xe5d5be0d, 0x7cdcefb7, 0x0bdbdf21, 0x86d3d2d4, 0xf1d4e242,
			0x68ddb3f8, 0x1fda836e, 0x81be16cd, 0xf6b9265b, 0x6fb077e1, 0x18b74777,
			0x88085ae6, 0xff0f6a70, 0x66063bca, 0x11010b5c, 0x8f659eff, 0xf862ae69,
			0x616bffd3, 0x166ccf45, 0xa00ae278, 0xd70dd2ee, 0x4e048354, 0x3903b3c2,
			0xa7672661, 0xd06016f7, 0x4969474d, 0x3e6e77db, 0xaed16a4a, 0xd9d65adc,
			0x40df0b66, 0x37d83bf0, 0xa9bcae53, 0xdebb9ec5, 0x47b2cf7f, 0x30b5ffe9,
			0xbdbdf21c, 0xcabac28a, 0x53b39330, 0x24b4a3a6, 0xbad03605, 0xcdd70693,
			0x54de5729, 0x23d967bf, 0xb3667a2e, 0xc4614ab8, 0x5d681b02, 0x2a6f2b94,
			0xb40bbe37, 0xc30c8ea1, 0x5a05df1b, 0x2d02ef8d,
		};

		/// <summary>
		/// Player name associated with this stash file.
		/// </summary>
		private string playerName;

		/// <summary>
		/// Raw file data
		/// </summary>
		private byte[] rawData;

		/// <summary>
		/// Binary marker for begin block
		/// </summary>
		private int beginBlockCrap;

		/// <summary>
		/// The number of sacks in this stash file
		/// </summary>
		private int numberOfSacks;

		/// <summary>
		/// Stash file version
		/// </summary>
		private int stashVersion;

		/// <summary>
		/// Raw data holding the name.
		/// Changed to raw data to support extended characters
		/// </summary>
		private byte[] name;

		/// <summary>
		/// Sack instance for this file
		/// </summary>
		private SackCollection sack;

		/// <summary>
		/// Initializes a new instance of the Stash class.
		/// </summary>
		/// <param name="playerName">Name of the player</param>
		/// <param name="stashFile">name of the stash file</param>
		public Stash(string playerName, string stashFile)
		{
			this.Log = Logger.Get(this);

			this.StashFile = stashFile;
			this.PlayerName = playerName;
			this.IsImmortalThrone = true;
			this.numberOfSacks = 2;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this is from Immortal Throne
		/// </summary>
		/// <remarks>
		/// This really should always be true since stashes are not supported without Immortal Throne.
		/// </remarks>
		public bool IsImmortalThrone { get; set; }

		/// <summary>
		/// Gets a value indicating whether this file has been modified
		/// </summary>
		public bool IsModified
		{
			get
			{
				if (this.sack != null)
				{
					if (this.sack.IsModified)
					{
						return true;
					}
				}

				return false;
			}
		}

		/// <summary>
		/// Gets the height of the stash sack
		/// </summary>
		public int Height { get; private set; }

		/// <summary>
		/// Gets the width of the stash sack
		/// </summary>
		public int Width { get; private set; }

		/// <summary>
		/// Gets the player name associated with this stash
		/// </summary>
		public string PlayerName
		{
			get
			{
				if (this.IsImmortalThrone)
				{
					return string.Concat(this.playerName, " - Immortal Throne");
				}
				else
				{
					return this.playerName;
				}
			}

			private set
			{
				this.playerName = value;
			}
		}

		/// <summary>
		/// Gets the stash file name
		/// </summary>
		public string StashFile { get; private set; }

		/// <summary>
		/// Gets the number of sack contained in this stash
		/// </summary>
		public int NumberOfSacks
		{
			get
			{
				if (this.sack == null)
				{
					return 0;
				}

				return this.numberOfSacks;
			}
		}

		/// <summary>
		/// Gets the current sack instance
		/// </summary>
		public SackCollection Sack
		{
			get
			{
				return this.sack;
			}
		}

		/// <summary>
		/// Creates an empty sack
		/// </summary>
		public void CreateEmptySack()
		{
			this.sack = new SackCollection();
			this.sack.IsModified = false;
		}

		/// <summary>
		/// Saves the stash file
		/// </summary>
		/// <param name="fileName">file name of this stash file</param>
		public void Save(string fileName)
		{
			byte[] data = this.Encode();

			data = CalculateCRC(data);

			using (FileStream outStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
			{
				outStream.Write(data, 0, data.Length);
			}

			// Save the corresponding dxg file
			data = EncodeBackupFile(data);

			// Now calculate the CRC for the dxg file
			data = CalculateCRC(data);

			string dxgFilename = Path.ChangeExtension(fileName, ".dxg");
			using (FileStream dxgOutStream = new FileStream(dxgFilename, FileMode.Create, FileAccess.Write))
			{
				dxgOutStream.Write(data, 0, data.Length);
			}
		}

		/// <summary>
		/// Converts the live data back into the raw binary data format
		/// </summary>
		/// <returns>byte array holding the raw data</returns>
		public byte[] Encode()
		{
			// We need to encode the item data to a memory stream
			return this.EncodeItemData();
		}

		/// <summary>
		/// Loads a stash file
		/// </summary>
		/// <returns>false if the file does not exist otherwise true.</returns>
		public bool LoadFile()
		{
			if (!File.Exists(this.StashFile))
			{
				return false;
			}

			using (FileStream file = new FileStream(this.StashFile, FileMode.Open, FileAccess.Read))
			{
				using (BinaryReader reader = new BinaryReader(file))
				{
					// Just suck the entire file into memory
					this.rawData = reader.ReadBytes((int)file.Length);
				}
			}

			try
			{
				// Now Parse the file
				this.ParseRawData();
			}
			catch (ArgumentException ex)
			{
				Log.Error("ParseRawData fail !", ex);
				throw;
			}

			return true;
		}

		/// <summary>
		/// Changes the file name extension in the raw file data to .dxg
		/// </summary>
		/// <param name="data">the raw file data</param>
		/// <returns>raw file data with the updated file name extension</returns>
		private static byte[] EncodeBackupFile(byte[] data)
		{
			// Find the length of the filename string.
			// It's an Int32 starting at offset 52 in the file.
			int offset = data[52] + (256 * data[53]) + (65536 * data[54]) + (16777216 * data[55]);

			// Adjust for the location of the filename string - 1
			// which is at offset 56.  Subtract 1 to get the last letter of the filename.
			offset += 55;

			// look for the 'b' in .dxb
			if (data[offset] == 98 || data[offset] == 66)
			{
				// and change it to 'g'
				data[offset] = Convert.ToByte(103, CultureInfo.InvariantCulture);
			}

			// zero out the checksum
			for (int i = 0; i < 4; i++)
			{
				data[i] = Convert.ToByte(0, CultureInfo.InvariantCulture);
			}

			return data;
		}

		/// <summary>
		/// Calculates the CRC32 of the raw data
		/// </summary>
		/// <param name="data">raw file data we are calculating</param>
		/// <returns>raw file data with the crc calculated and inserted into the proper field</returns>
		private static byte[] CalculateCRC(byte[] data)
		{
			using (BinaryReader reader = new BinaryReader(new MemoryStream(data, false)))
			{
				uint crc32Result = 0;
				byte[] buffer = new byte[BUFFERSIZE];
				int readSize = BUFFERSIZE;

				int count = reader.Read(buffer, 0, readSize);
				while (count > 0)
				{
					for (int i = 0; i < count; i++)
					{
						crc32Result = (crc32Result >> 8) ^ crc32Table[buffer[i] ^ (crc32Result & 0x000000FF)];
					}

					count = reader.Read(buffer, 0, readSize);
				}

				// Put the data into the stream
				data[3] = Convert.ToByte((crc32Result & 0xFF000000) >> 24, CultureInfo.InvariantCulture);
				data[2] = Convert.ToByte((crc32Result & 0x00FF0000) >> 16, CultureInfo.InvariantCulture);
				data[1] = Convert.ToByte((crc32Result & 0x0000FF00) >> 8, CultureInfo.InvariantCulture);
				data[0] = Convert.ToByte(crc32Result & 0x000000FF, CultureInfo.InvariantCulture);
			}

			return data;
		}

		/// <summary>
		/// Parses the raw data and converts to internal data.
		/// </summary>
		private void ParseRawData()
		{
			// First create a memory stream so we can decode the binary data as needed.
			using (BinaryReader reader = new BinaryReader(new MemoryStream(this.rawData, false)))
			{
				int offset = 0;
				try
				{
					this.ParseItemBlock(offset, reader);
				}
				catch (ArgumentException)
				{
					throw;
				}

				try
				{
					string outfile = string.Concat(Path.Combine(TQData.TQVaultSaveFolder, this.PlayerName), " Export.txt");
					using (StreamWriter outStream = new StreamWriter(outfile, false))
					{
						outStream.WriteLine("Number of Sacks = {0}", this.numberOfSacks);

						if (!this.sack.IsEmpty)
						{
							outStream.WriteLine();
							outStream.WriteLine("SACK 0");

							int itemNumber = 0;
							foreach (Item item in this.sack)
							{
								object[] params1 = new object[20];

								params1[0] = itemNumber;
								params1[1] = item.ToString();
								params1[2] = item.PositionX;
								params1[3] = item.PositionY;
								params1[4] = item.Seed;

								outStream.WriteLine("  {0,5:n0} {1}", params1);
								itemNumber++;
							}
						}
					}
				}
				catch (IOException exception)
				{
					Log.ErrorFormat(exception, "Error Exporting - '{0} Export.txt'", Path.Combine(TQData.TQVaultSaveFolder, this.PlayerName));
				}
			}
		}

		/// <summary>
		/// Encodes the internal item data back into raw data
		/// </summary>
		/// <returns>raw data for the item data</returns>
		private byte[] EncodeItemData()
		{
			int dataLength;
			byte[] data;

			// Encode the item data into a memory stream
			using (MemoryStream writeStream = new MemoryStream(2048))
			{
				using (BinaryWriter writer = new BinaryWriter(writeStream))
				{
					// Write zero into the checksum value
					writer.Write(Convert.ToInt32(0, CultureInfo.InvariantCulture));

					TQData.WriteCString(writer, "begin_block");
					writer.Write(this.beginBlockCrap);

					TQData.WriteCString(writer, "stashVersion");
					writer.Write(this.stashVersion);

					TQData.WriteCString(writer, "fName");

					// Changed to raw data to support extended characters
					writer.Write(this.name.Length);
					writer.Write(this.name);

					TQData.WriteCString(writer, "sackWidth");
					writer.Write(this.Width);

					TQData.WriteCString(writer, "sackHeight");
					writer.Write(this.Height);

					// SackType should already be set at this point
					this.sack.Encode(writer);
					dataLength = (int)writeStream.Length;
				}

				// now just return the buffer we wrote to.
				data = writeStream.GetBuffer();
			}

			// The problem is that data[] may be bigger than the amount of data in it.
			// We need to resize the array
			if (dataLength == data.Length)
			{
				return data;
			}

			byte[] realData = new byte[dataLength];
			Array.Copy(data, realData, dataLength);
			return realData;
		}

		/// <summary>
		/// Parses an item block within the file and coverts raw item data into internal item data
		/// </summary>
		/// <param name="fileOffset">Offset into the file</param>
		/// <param name="reader">BinaryReader instance</param>
		private void ParseItemBlock(int fileOffset, BinaryReader reader)
		{
			try
			{
				reader.BaseStream.Seek(fileOffset, SeekOrigin.Begin);
				reader.ReadInt32();

				TQData.ValidateNextString("begin_block", reader);
				this.beginBlockCrap = reader.ReadInt32();

				TQData.ValidateNextString("stashVersion", reader);
				this.stashVersion = reader.ReadInt32();

				TQData.ValidateNextString("fName", reader);

				// Changed to raw data to support extended characters
				int stringLength = reader.ReadInt32();
				this.name = reader.ReadBytes(stringLength);

				TQData.ValidateNextString("sackWidth", reader);
				this.Width = reader.ReadInt32();

				TQData.ValidateNextString("sackHeight", reader);
				this.Height = reader.ReadInt32();

				this.numberOfSacks = 1;
				this.sack = new SackCollection();
				this.sack.SackType = SackType.Stash;
				this.sack.IsImmortalThrone = true;
				this.sack.Parse(reader);
			}
			catch (ArgumentException)
			{
				// The ValidateNextString Method can throw an ArgumentException.
				// We just pass it along at this point.
				throw;
			}
		}
	}
}