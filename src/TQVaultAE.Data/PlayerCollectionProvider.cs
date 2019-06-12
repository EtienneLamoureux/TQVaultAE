//-----------------------------------------------------------------------
// <copyright file="PlayerCollection.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.Data
{
	using System;
	using System.Globalization;
	using System.IO;
	using TQVaultAE.Entities;
	using TQVaultAE.Logs;

	/// <summary>
	/// Loads, decodes, encodes and saves a Titan Quest player file.
	/// </summary>
	public static class PlayerCollectionProvider
	{
		private static readonly log4net.ILog Log = Logger.Get(typeof(PlayerCollectionProvider));

		/// <summary>
		/// Static array holding the byte pattern for the beginning of a block in the player file.
		/// </summary>
		public static byte[] beginBlockPattern = { 0x0B, 0x00, 0x00, 0x00, 0x62, 0x65, 0x67, 0x69, 0x6E, 0x5F, 0x62, 0x6C, 0x6F, 0x63, 0x6B };

		/// <summary>
		/// Static array holding the byte pattern for the end of a block in the player file.
		/// </summary>
		public static byte[] endBlockPattern = { 0x09, 0x00, 0x00, 0x00, 0x65, 0x6E, 0x64, 0x5F, 0x62, 0x6C, 0x6F, 0x63, 0x6B };

		public static void CommitPlayerInfo(PlayerCollection pc, PlayerInfo playerInfo)
		{
			if (pc.PlayerInfo == null) return;
			if (playerInfo == null) return;
			if (pc.rawData == null || pc.rawData.Length < 1) return;

			var writer = new PlayerInfoWriter();
			//validate the current data against the raw file
			//there should be no changes 
			writer.Validate(pc.PlayerInfo, pc.rawData);
			pc.PlayerInfo.CurrentLevel = playerInfo.CurrentLevel;
			pc.PlayerInfo.CurrentXP = playerInfo.CurrentXP;
			pc.PlayerInfo.DifficultyUnlocked = playerInfo.DifficultyUnlocked;
			pc.PlayerInfo.AttributesPoints = playerInfo.AttributesPoints;
			pc.PlayerInfo.SkillPoints = playerInfo.SkillPoints;
			pc.PlayerInfo.BaseStrength = playerInfo.BaseStrength;
			pc.PlayerInfo.BaseDexterity = playerInfo.BaseDexterity;
			pc.PlayerInfo.BaseIntelligence = playerInfo.BaseIntelligence;
			pc.PlayerInfo.BaseHealth = playerInfo.BaseHealth;
			pc.PlayerInfo.BaseMana = playerInfo.BaseMana;
			pc.PlayerInfo.Money = playerInfo.Money;
			//commit the player changes to the raw file
			writer.Commit(pc.PlayerInfo, pc.rawData);
		}




		/// <summary>
		/// Attempts to save the file.
		/// </summary>
		/// <param name="fileName">Name of the file to save</param>
		public static void Save(PlayerCollection pc, string fileName)
		{
			byte[] data = Encode(pc);

			using (FileStream outStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
			{
				outStream.Write(data, 0, data.Length);
			}
		}

		/// <summary>
		/// Converts the live data back into the raw binary data format.
		/// </summary>
		/// <returns>Byte Array holding the converted binary data</returns>
		public static byte[] Encode(PlayerCollection pc)
		{
			// We need to encode the item data to a memory stream
			// then splice it back in place of the old item data
			byte[] rawItemData = EncodeItemData(pc);

			if (pc.IsVault)
			{
				// vaults do not have all the other crapola.
				return rawItemData;
			}

			// Added to support equipment panel
			byte[] rawEquipmentData = EncodeEquipmentData(pc);

			// now make an array big enough to hold everything
			byte[] ans = new byte[pc.itemBlockStart + rawItemData.Length + (pc.equipmentBlockStart - pc.itemBlockEnd) +
				rawEquipmentData.Length + (pc.rawData.Length - pc.equipmentBlockEnd)];

			// Now copy all the data into pc array
			Array.Copy(pc.rawData, 0, ans, 0, pc.itemBlockStart);
			Array.Copy(rawItemData, 0, ans, pc.itemBlockStart, rawItemData.Length);
			Array.Copy(pc.rawData, pc.itemBlockEnd, ans, pc.itemBlockStart + rawItemData.Length, pc.equipmentBlockStart - pc.itemBlockEnd);
			Array.Copy(rawEquipmentData, 0, ans, pc.itemBlockStart + rawItemData.Length + pc.equipmentBlockStart - pc.itemBlockEnd, rawEquipmentData.Length);
			Array.Copy(
				pc.rawData,
				pc.equipmentBlockEnd,
				ans,
				pc.itemBlockStart + rawItemData.Length + pc.equipmentBlockStart - pc.itemBlockEnd + rawEquipmentData.Length,
				pc.rawData.Length - pc.equipmentBlockEnd);

			return ans;
		}



		/// <summary>
		/// Attempts to load a player file
		/// </summary>
		public static void LoadFile(PlayerCollection pc)
		{
			using (FileStream fileStream = new FileStream(pc.PlayerFile, FileMode.Open, FileAccess.Read))
			{
				using (BinaryReader reader = new BinaryReader(fileStream))
				{
					// Just suck the entire file into memory
					pc.rawData = reader.ReadBytes((int)fileStream.Length);
				}
			}

			try
			{
				// Now Parse the file
				ParseRawData(pc);
			}
			catch (ArgumentException ex)
			{
				Log.Error("ParseRawData() Failed !", ex);
				throw;
			}
		}


		/// <summary>
		/// Looks for the next begin_block or end_block.
		/// </summary>
		/// <param name="start">offset where we are starting our search</param>
		/// <returns>Returns the index of the first char indicating the block delimiter or -1 if none is found.</returns>
		public static int FindNextBlockDelim(PlayerCollection pc, int start)
		{
			int beginMatch = 0;
			int endMatch = 0;

			for (int i = start; i < pc.rawData.Length; ++i)
			{
				if (pc.rawData[i] == beginBlockPattern[beginMatch])
				{
					++beginMatch;
					if (beginMatch == beginBlockPattern.Length)
					{
						return i + 1 - beginMatch;
					}
				}
				else if (beginMatch > 0)
				{
					beginMatch = 0;

					// Test again to see if we are starting a new match
					if (pc.rawData[i] == beginBlockPattern[beginMatch])
					{
						++beginMatch;
					}
				}

				if (pc.rawData[i] == endBlockPattern[endMatch])
				{
					++endMatch;
					if (endMatch == endBlockPattern.Length)
					{
						return i + 1 - endMatch;
					}
				}
				else if (endMatch > 0)
				{
					endMatch = 0;

					// Test again to see if we are starting a new match
					if (pc.rawData[i] == endBlockPattern[endMatch])
					{
						++endMatch;
					}
				}
			}

			return -1;
		}

		/// <summary>
		/// Parses the raw binary data for use within TQVault
		/// </summary>
		private static void ParseRawData(PlayerCollection pc)
		{
			// First create a memory stream so we can decode the binary data as needed.
			using (MemoryStream stream = new MemoryStream(pc.rawData, false))
			{
				using (BinaryReader reader = new BinaryReader(stream))
				{
					// Find the block pairs until we find the block that contains the item data.
					int blockNestLevel = 0;
					int currentOffset = 0;
					int itemOffset = 0;
					int equipmentOffset = 0;
					var playerReader = new PlayerInfoReader();

					// vaults start at the item data with no crap
					bool foundItems = pc.IsVault;
					bool foundEquipment = pc.IsVault;

					while ((!foundItems || !foundEquipment) && (currentOffset = FindNextBlockDelim(pc, currentOffset)) != -1)
					{
						if (pc.rawData[currentOffset] == beginBlockPattern[0])
						{
							// begin block
							++blockNestLevel;
							currentOffset += beginBlockPattern.Length;

							// skip past the 4 bytes of noise after begin_block
							currentOffset += 4;

							// Seek our stream to the correct position
							stream.Seek(currentOffset, SeekOrigin.Begin);

							// Now get the string for pc block
							string blockName = TQData.ReadCString(reader).ToUpperInvariant();

							// Assign loc to our new stream position
							currentOffset = (int)stream.Position;

							// See if we accidentally got a begin_block or end_block
							if (blockName.Equals("BEGIN_BLOCK"))
							{
								blockName = "(NONAME)";
								currentOffset -= beginBlockPattern.Length;
							}
							else if (blockName.Equals("END_BLOCK"))
							{
								blockName = "(NONAME)";
								currentOffset -= endBlockPattern.Length;
							}
							else if (blockName.Equals("ITEMPOSITIONSSAVEDASGRIDCOORDS"))
							{
								currentOffset += 4;
								itemOffset = currentOffset; // skip value for itemPositionsSavedAsGridCoords
								foundItems = true;
							}
							else if (blockName.Equals("USEALTERNATE"))
							{
								currentOffset += 4;
								equipmentOffset = currentOffset; // skip value for useAlternate
								foundEquipment = true;
							}
							else if (!pc.IsVault && playerReader.Match(blockName))
							{
								playerReader.Record(blockName, currentOffset);
							}

							// Print the string with a nesting level indicator
							////string levelString = new string ('-', System.Math.Max(0,blockNestLevel*2-2));
							////out.WriteLine ("{0} {2:n0} '{1}'", levelString, blockName, loc);
						}
						else
						{
							// end block
							--blockNestLevel;
							currentOffset += endBlockPattern.Length;
							////if (blockNestLevel < 0)
							////{
							//// out.WriteLine ("{0:n0} Block Nest Level < 0!!!", loc);
							////}
						}
					}
					////out.WriteLine ("Final Block Level = {0:n0}", blockNestLevel);

					if (foundItems)
					{
						try
						{
							ParseItemBlock(pc, itemOffset, reader);
						}
						catch (ArgumentException exception)
						{
							var ex = new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Error parsing player file Item Block- '{0}'", pc.PlayerName), exception);
							Log.ErrorFormat(CultureInfo.InvariantCulture, "Error parsing player file Item Block - '{0}'", pc.PlayerName);
							Log.ErrorException(exception);
							throw ex;
						}

						try
						{
							string outfile = string.Concat(Path.Combine(TQData.TQVaultSaveFolder, pc.PlayerName), " Export.txt");
							using (StreamWriter outStream = new StreamWriter(outfile, false))
							{
								outStream.WriteLine("Number of Sacks = {0}", pc.numberOfSacks);

								int sackNumber = 0;
								if (pc.sacks != null)
								{
									foreach (SackCollection sack in pc.sacks)
									{
										if (!sack.IsEmpty)
										{
											outStream.WriteLine();
											outStream.WriteLine("SACK {0}", sackNumber);

											int itemNumber = 0;
											foreach (Item item in sack)
											{
												object[] params1 = new object[20];

												params1[0] = itemNumber;
												params1[1] =  ItemProvider.ToFriendlyName(item);
												params1[2] = item.PositionX;
												params1[3] = item.PositionY;
												params1[4] = item.Seed;
												////params1[5] =

												outStream.WriteLine("  {0,5:n0} {1}", params1);
												itemNumber++;
											}
										}

										sackNumber++;
									}
								}
							}
						}
						catch (IOException exception)
						{
							Log.ErrorFormat(exception, "Error writing Export file - '{0}'"
								, string.Concat(Path.Combine(TQData.TQVaultSaveFolder, pc.PlayerName), " Export.txt")
							);
						}
					}

					// Process the equipment block
					if (foundEquipment && !pc.IsVault)
					{
						try
						{
							ParseEquipmentBlock(pc, equipmentOffset, reader);
						}
						catch (ArgumentException exception)
						{
							var ex = new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Error parsing player file Equipment Block - '{0}'", pc.PlayerName), exception);
							Log.ErrorFormat(ex, "Error parsing player file Equipment Block - '{0}'", pc.PlayerName);
							throw ex;
						}

						try
						{
							string outfile = string.Concat(Path.Combine(TQData.TQVaultSaveFolder, pc.PlayerName), " Equipment Export.txt");
							using (StreamWriter outStream = new StreamWriter(outfile, false))
							{
								if (!pc.EquipmentSack.IsEmpty)
								{
									int itemNumber = 0;
									foreach (Item item in pc.EquipmentSack)
									{
										object[] params1 = new object[20];

										params1[0] = itemNumber;
										params1[1] =  ItemProvider.ToFriendlyName(item);
										params1[2] = item.PositionX;
										params1[3] = item.PositionY;
										params1[4] = item.Seed;
										////params1[5] =

										outStream.WriteLine("  {0,5:n0} {1}", params1);
										itemNumber++;
									}
								}
							}
						}
						catch (IOException exception)
						{
							Log.ErrorFormat(exception, "Error writing Export file - '{0}'"
								, string.Concat(Path.Combine(TQData.TQVaultSaveFolder, pc.PlayerName), " Equipment Export.txt")
							);
						}
					}

					if (playerReader.FoundPlayerInfo && !pc.IsVault)
					{
						try
						{
							playerReader.Read(reader);
							pc.PlayerInfo = playerReader.GetPlayerInfo();

						}
						catch (ArgumentException exception)
						{
							if (!TQDebug.DebugEnabled)
							{
								TQDebug.DebugEnabled = true;
							}
							var rethrowex = new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Error parsing player player info Block - '{0}'", pc.PlayerName), exception);
							Log.ErrorException(rethrowex);
							throw rethrowex;
						}
					}
				}
			}
		}

		/// <summary>
		/// Encodes the live item data into raw binary format
		/// </summary>
		/// <returns>byte array holding the converted binary data</returns>
		private static byte[] EncodeItemData(PlayerCollection pc)
		{
			int dataLength;
			byte[] data;

			// Encode the item data into a memory stream
			using (MemoryStream writeStream = new MemoryStream(2048))
			{
				using (BinaryWriter writer = new BinaryWriter(writeStream))
				{
					TQData.WriteCString(writer, "numberOfSacks");
					writer.Write(pc.numberOfSacks);

					TQData.WriteCString(writer, "currentlyFocusedSackNumber");
					writer.Write(pc.currentlyFocusedSackNumber);

					TQData.WriteCString(writer, "currentlySelectedSackNumber");
					writer.Write(pc.currentlySelectedSackNumber);

					for (int i = 0; i < pc.numberOfSacks; ++i)
					{
						// SackType should already be set at pc point
						SackCollectionProvider.Encode(pc.sacks[i], writer);
					}

					dataLength = (int)writeStream.Length;
				}

				// now just return the buffer we wrote to.
				data = writeStream.GetBuffer();
			}

			// The problem is that ans() may be bigger than the amount of data in it.
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
		/// Parses the item block
		/// </summary>
		/// <param name="offset">offset in the player file</param>
		/// <param name="reader">BinaryReader instance</param>
		private static void ParseItemBlock(PlayerCollection pc, int offset, BinaryReader reader)
		{
			try
			{
				pc.itemBlockStart = offset;

				reader.BaseStream.Seek(offset, SeekOrigin.Begin);

				TQData.ValidateNextString("numberOfSacks", reader);
				pc.numberOfSacks = reader.ReadInt32();

				TQData.ValidateNextString("currentlyFocusedSackNumber", reader);
				pc.currentlyFocusedSackNumber = reader.ReadInt32();

				TQData.ValidateNextString("currentlySelectedSackNumber", reader);
				pc.currentlySelectedSackNumber = reader.ReadInt32();

				pc.sacks = new SackCollection[pc.numberOfSacks];

				for (int i = 0; i < pc.numberOfSacks; ++i)
				{
					pc.sacks[i] = new SackCollection();
					pc.sacks[i].SackType = SackType.Sack;
					pc.sacks[i].IsImmortalThrone = pc.IsImmortalThrone;
					SackCollectionProvider.Parse(pc.sacks[i], reader);
				}

				pc.itemBlockEnd = (int)reader.BaseStream.Position;
			}
			catch (ArgumentException ex)
			{
				// The ValidateNextString Method can throw an ArgumentException.
				// We just pass it along at pc point.
				Log.Debug("ValidateNextString fail !", ex);
				throw;
			}
		}

		/// <summary>
		/// Encodes the live equipment data into raw binary
		/// </summary>
		/// <returns>byte array holding the converted binary data</returns>
		private static byte[] EncodeEquipmentData(PlayerCollection pc)
		{
			int dataLength;
			byte[] data;

			// Encode the item data into a memory stream
			using (MemoryStream writeStream = new MemoryStream(2048))
			{
				using (BinaryWriter writer = new BinaryWriter(writeStream))
				{
					if (pc.IsImmortalThrone)
					{
						TQData.WriteCString(writer, "equipmentCtrlIOStreamVersion");
						writer.Write(pc.equipmentCtrlIOStreamVersion);
					}

					SackCollectionProvider.Encode(pc.EquipmentSack, writer);

					dataLength = (int)writeStream.Length;
				}

				// now just return the buffer we wrote to.
				data = writeStream.GetBuffer();
			}

			// The problem is that ans() may be bigger than the amount of data in it.
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
		/// Parses the binary equipment block data
		/// </summary>
		/// <param name="offset">offset of the block within the player file.</param>
		/// <param name="reader">BinaryReader instance</param>
		private static void ParseEquipmentBlock(PlayerCollection pc, int offset, BinaryReader reader)
		{
			try
			{
				pc.equipmentBlockStart = offset;

				reader.BaseStream.Seek(offset, SeekOrigin.Begin);

				if (pc.IsImmortalThrone)
				{
					TQData.ValidateNextString("equipmentCtrlIOStreamVersion", reader);
					pc.equipmentCtrlIOStreamVersion = reader.ReadInt32();
				}

				pc.EquipmentSack = new SackCollection();
				pc.EquipmentSack.SackType = SackType.Equipment;
				pc.EquipmentSack.IsImmortalThrone = pc.IsImmortalThrone;
				SackCollectionProvider.Parse(pc.EquipmentSack, reader);

				pc.equipmentBlockEnd = (int)reader.BaseStream.Position;
			}
			catch (ArgumentException ex)
			{
				Log.Error($"ParseEquipmentBlock fail ! offset={offset}", ex);
				throw;
			}
		}

	}
}