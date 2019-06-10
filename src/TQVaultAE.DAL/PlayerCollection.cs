//-----------------------------------------------------------------------
// <copyright file="PlayerCollection.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultData
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;

	/// <summary>
	/// Loads, decodes, encodes and saves a Titan Quest player file.
	/// </summary>
	public class PlayerCollection : IEnumerable<SackCollection>
	{
		/// <summary>
		/// Static array holding the byte pattern for the beginning of a block in the player file.
		/// </summary>
		private static byte[] beginBlockPattern = { 0x0B, 0x00, 0x00, 0x00, 0x62, 0x65, 0x67, 0x69, 0x6E, 0x5F, 0x62, 0x6C, 0x6F, 0x63, 0x6B };

		/// <summary>
		/// Static array holding the byte pattern for the end of a block in the player file.
		/// </summary>
		private static byte[] endBlockPattern = { 0x09, 0x00, 0x00, 0x00, 0x65, 0x6E, 0x64, 0x5F, 0x62, 0x6C, 0x6F, 0x63, 0x6B };

		/// <summary>
		/// String holding the player name
		/// </summary>
		private string playerName;

		/// <summary>
		/// Byte array holding the raw data from the file.
		/// </summary>
		private byte[] rawData;

		/// <summary>
		/// Position of the item block within the file.
		/// </summary>
		private int itemBlockStart;

		/// <summary>
		/// Position of the end of the item block within the file.
		/// </summary>
		private int itemBlockEnd;

		/// <summary>
		/// Position of the equipment block within the file.
		/// </summary>
		private int equipmentBlockStart;

		/// <summary>
		/// Position of the end of the equipment block within the file.
		/// </summary>
		private int equipmentBlockEnd;

		/// <summary>
		/// Number of sacks that this file holds
		/// </summary>
		private int numberOfSacks;

		/// <summary>
		/// Holds the currently focused sack
		/// </summary>
		private int currentlyFocusedSackNumber;

		/// <summary>
		/// Holds the currently selected sack
		/// </summary>
		private int currentlySelectedSackNumber;

		/// <summary>
		/// Holds the equipmentCtrlIOStreamVersion tag in the file.
		/// </summary>
		private int equipmentCtrlIOStreamVersion;

		/// <summary>
		/// Array of the sacks
		/// </summary>
		private SackCollection[] sacks;

		/// <summary>
		/// Initializes a new instance of the PlayerCollection class.
		/// </summary>
		/// <param name="playerName">Name of the player</param>
		/// <param name="playerFile">filename of the player file</param>
		public PlayerCollection(string playerName, string playerFile)
		{
			this.PlayerFile = playerFile;
			this.PlayerName = playerName;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this file is a vault
		/// </summary>
		public bool IsVault { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this file is an immortal throne file
		/// </summary>
		public bool IsImmortalThrone { get; set; }

		/// <summary>
		/// Gets the equipment sack for this file.
		/// </summary>
		public SackCollection EquipmentSack { get; private set; }

		/// <summary>
		/// Holds playerInfo
		/// </summary>
		public PlayerInfo PlayerInfo { get; private set; }

		/// <summary>
		/// Gets the player file name
		/// </summary>
		public string PlayerFile { get; private set; }

		/// <summary>
		/// Gets a value indicating whether this file has been modified.
		/// </summary>
		public bool IsModified
		{
			get
			{
				// look through each sack and see if the sack has been modified
				if (this.sacks != null)
				{
					foreach (SackCollection sack in this.sacks)
					{
						if (sack.IsModified)
						{
							return true;
						}
					}
				}

				if (this.EquipmentSack != null)
				{
					if (this.EquipmentSack.IsModified)
					{
						return true;
					}
				}

				if (this.PlayerInfo != null)
				{
					if (this.PlayerInfo.Modified)
					{
						return (true);
					}
				}

				return false;
			}
		}

		/// <summary>
		/// Gets the player name
		/// </summary>
		public string PlayerName
		{
			get
			{
				if (!this.IsVault && this.IsImmortalThrone)
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
		/// Gets the number of sacks in this file
		/// </summary>
		public int NumberOfSacks
		{
			get
			{
				if (this.sacks == null)
				{
					return 0;
				}

				return this.sacks.Length;
			}
		}

		/// <summary>
		/// Enumerator block to iterate all of the sacks in the Player
		/// </summary>
		/// <returns>Each Sack in the sack array.</returns>
		public IEnumerator<SackCollection> GetEnumerator()
		{
			if (this.sacks == null)
			{
				yield break;
			}
			foreach (SackCollection sack in this.sacks)
			{
				yield return sack;
			}
		}


		public void CommitPlayerInfo(PlayerInfo playerInfo)
		{
			if (this.PlayerInfo == null) return;
			if (playerInfo == null) return;
			if (rawData == null || rawData.Length < 1) return;

			var writer = new PlayerInfoWriter();
			//validate the current data against the raw file
			//there should be no changes 
			writer.Validate(this.PlayerInfo, this.rawData);
			this.PlayerInfo.CurrentLevel = playerInfo.CurrentLevel;
			this.PlayerInfo.CurrentXP = playerInfo.CurrentXP;
			this.PlayerInfo.DifficultyUnlocked = playerInfo.DifficultyUnlocked;
			this.PlayerInfo.AttributesPoints = playerInfo.AttributesPoints;
			this.PlayerInfo.SkillPoints = playerInfo.SkillPoints;
			this.PlayerInfo.BaseStrength = playerInfo.BaseStrength;
			this.PlayerInfo.BaseDexterity = playerInfo.BaseDexterity;
			this.PlayerInfo.BaseIntelligence = playerInfo.BaseIntelligence;
			this.PlayerInfo.BaseHealth = playerInfo.BaseHealth;
			this.PlayerInfo.BaseMana = playerInfo.BaseMana;
			this.PlayerInfo.Money = playerInfo.Money;
			//commit the player changes to the raw file
			writer.Commit(this.PlayerInfo, this.rawData);
		}

		/// <summary>
		/// Non Generic enumerator interface.
		/// </summary>
		/// <returns>Generic interface implementation.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		/// <summary>
		/// Creates empty sacks within the file.
		/// </summary>
		/// <param name="numberOfSacks">Number of sacks to create</param>
		public void CreateEmptySacks(int numberOfSacks)
		{
			this.sacks = new SackCollection[numberOfSacks];
			this.numberOfSacks = numberOfSacks;

			for (int i = 0; i < numberOfSacks; ++i)
			{
				this.sacks[i] = new SackCollection();
				this.sacks[i].IsModified = false;
			}
		}


		/// <summary>
		/// Attempts to save the file.
		/// </summary>
		/// <param name="fileName">Name of the file to save</param>
		public void Save(string fileName)
		{
			byte[] data = this.Encode();

			using (FileStream outStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
			{
				outStream.Write(data, 0, data.Length);
			}
		}

		/// <summary>
		/// Converts the live data back into the raw binary data format.
		/// </summary>
		/// <returns>Byte Array holding the converted binary data</returns>
		public byte[] Encode()
		{
			// We need to encode the item data to a memory stream
			// then splice it back in place of the old item data
			byte[] rawItemData = this.EncodeItemData();

			if (this.IsVault)
			{
				// vaults do not have all the other crapola.
				return rawItemData;
			}

			// Added to support equipment panel
			byte[] rawEquipmentData = this.EncodeEquipmentData();

			// now make an array big enough to hold everything
			byte[] ans = new byte[this.itemBlockStart + rawItemData.Length + (this.equipmentBlockStart - this.itemBlockEnd) +
				rawEquipmentData.Length + (this.rawData.Length - this.equipmentBlockEnd)];

			// Now copy all the data into this array
			Array.Copy(this.rawData, 0, ans, 0, this.itemBlockStart);
			Array.Copy(rawItemData, 0, ans, this.itemBlockStart, rawItemData.Length);
			Array.Copy(this.rawData, this.itemBlockEnd, ans, this.itemBlockStart + rawItemData.Length, this.equipmentBlockStart - this.itemBlockEnd);
			Array.Copy(rawEquipmentData, 0, ans, this.itemBlockStart + rawItemData.Length + this.equipmentBlockStart - this.itemBlockEnd, rawEquipmentData.Length);
			Array.Copy(
				this.rawData,
				this.equipmentBlockEnd,
				ans,
				this.itemBlockStart + rawItemData.Length + this.equipmentBlockStart - this.itemBlockEnd + rawEquipmentData.Length,
				this.rawData.Length - this.equipmentBlockEnd);

			return ans;
		}

		/// <summary>
		/// Gets a sack from the instance
		/// </summary>
		/// <param name="sackNumber">Number of the sack we are retrieving</param>
		/// <returns>Sack instace for the corresponding sack number</returns>
		public SackCollection GetSack(int sackNumber)
		{
			if (this.sacks == null || this.sacks.Length <= sackNumber)
			{
				return null;
			}

			return this.sacks[sackNumber];
		}

		/// <summary>
		/// Moves a sack within the instance.  Used for renumbering the sacks.
		/// </summary>
		/// <param name="source">source sack number</param>
		/// <param name="destination">destination sack number</param>
		/// <returns>true if successful</returns>
		public bool MoveSack(int source, int destination)
		{
			// Do a little bit of error handling
			if (this.sacks == null ||
					destination < 0 || destination > this.sacks.Length ||
					source < 0 || source > this.sacks.Length || source == destination)
			{
				return false;
			}

			List<SackCollection> tmp = new List<SackCollection>(this.sacks.Length);

			// Copy the whole array first.
			foreach (SackCollection sack in this.sacks)
			{
				tmp.Add(sack);
			}

			// Now we can shuffle things around
			tmp.RemoveAt(source);
			tmp.Insert(destination, this.sacks[source]);
			this.sacks[source].IsModified = true;
			this.sacks[destination].IsModified = true;

			tmp.CopyTo(this.sacks);

			return true;
		}

		/// <summary>
		/// Copies a sack within the instance
		/// </summary>
		/// <param name="source">source sack number</param>
		/// <param name="destination">desintation sack number</param>
		/// <returns>true if successful</returns>
		public bool CopySack(int source, int destination)
		{
			// Do a little bit of error handling
			if (this.sacks == null ||
					destination < 0 || destination > this.sacks.Length ||
					source < 0 || source > this.sacks.Length || source == destination)
			{
				return false;
			}

			SackCollection newSack = this.sacks[source].Duplicate();

			if (newSack != null)
			{
				this.sacks[destination] = newSack;
				return true;
			}

			return false;
		}

		/// <summary>
		/// Attempts to load a player file
		/// </summary>
		public void LoadFile()
		{
			using (FileStream fileStream = new FileStream(this.PlayerFile, FileMode.Open, FileAccess.Read))
			{
				using (BinaryReader reader = new BinaryReader(fileStream))
				{
					// Just suck the entire file into memory
					this.rawData = reader.ReadBytes((int)fileStream.Length);
				}
			}

			try
			{
				// Now Parse the file
				this.ParseRawData();
			}
			catch (ArgumentException)
			{
				throw;
			}
		}

		/// <summary>
		/// Parses the raw binary data for use within TQVault
		/// </summary>
		private void ParseRawData()
		{
			// First create a memory stream so we can decode the binary data as needed.
			using (MemoryStream stream = new MemoryStream(this.rawData, false))
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
					bool foundItems = this.IsVault;
					bool foundEquipment = this.IsVault;

					while ((!foundItems || !foundEquipment) && (currentOffset = this.FindNextBlockDelim(currentOffset)) != -1)
					{
						if (this.rawData[currentOffset] == beginBlockPattern[0])
						{
							// begin block
							++blockNestLevel;
							currentOffset += beginBlockPattern.Length;

							// skip past the 4 bytes of noise after begin_block
							currentOffset += 4;

							// Seek our stream to the correct position
							stream.Seek(currentOffset, SeekOrigin.Begin);

							// Now get the string for this block
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
							else if (!this.IsVault && playerReader.Match(blockName))
							{
								playerReader.Record(blockName,currentOffset);
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
							this.ParseItemBlock(itemOffset, reader);
						}
						catch (ArgumentException exception)
						{
							if (!TQDebug.DebugEnabled)
							{
								TQDebug.DebugEnabled = true;
							}

							TQDebug.DebugWriteLine(string.Format(CultureInfo.InvariantCulture, "Error parsing player file Item Block - '{0}'", this.PlayerName));
							TQDebug.DebugWriteLine(exception.ToString());
							throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Error parsing player file Item Block- '{0}'", this.PlayerName), exception);
						}

						try
						{
							string outfile = string.Concat(Path.Combine(TQData.TQVaultSaveFolder, this.PlayerName), " Export.txt");
							using (StreamWriter outStream = new StreamWriter(outfile, false))
							{
								outStream.WriteLine("Number of Sacks = {0}", this.numberOfSacks);

								int sackNumber = 0;
								if (this.sacks != null)
								{
									foreach (SackCollection sack in this.sacks)
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
												params1[1] = item.ToString();
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
							if (!TQDebug.DebugEnabled)
							{
								TQDebug.DebugEnabled = true;
							}

							TQDebug.DebugWriteLine(string.Format(
								CultureInfo.InvariantCulture,
								"Error writing Export file - '{0}'",
								string.Concat(Path.Combine(TQData.TQVaultSaveFolder, this.PlayerName), " Export.txt")));
							TQDebug.DebugWriteLine(exception.ToString());
						}
					}

					// Process the equipment block
					if (foundEquipment && !this.IsVault)
					{
						try
						{
							this.ParseEquipmentBlock(equipmentOffset, reader);
						}
						catch (ArgumentException exception)
						{
							if (!TQDebug.DebugEnabled)
							{
								TQDebug.DebugEnabled = true;
							}

							TQDebug.DebugWriteLine(string.Format(CultureInfo.InvariantCulture, "Error parsing player file Equipment Block - '{0}'", this.PlayerName));
							TQDebug.DebugWriteLine(exception.ToString());
							throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Error parsing player file Equipment Block - '{0}'", this.PlayerName), exception);
						}

						try
						{
							string outfile = string.Concat(Path.Combine(TQData.TQVaultSaveFolder, this.PlayerName), " Equipment Export.txt");
							using (StreamWriter outStream = new StreamWriter(outfile, false))
							{
								if (!this.EquipmentSack.IsEmpty)
								{
									int itemNumber = 0;
									foreach (Item item in this.EquipmentSack)
									{
										object[] params1 = new object[20];

										params1[0] = itemNumber;
										params1[1] = item.ToString();
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
							if (!TQDebug.DebugEnabled)
							{
								TQDebug.DebugEnabled = true;
							}

							TQDebug.DebugWriteLine(string.Format(
								CultureInfo.InvariantCulture,
								"Error writing Export file - '{0}'",
								string.Concat(Path.Combine(TQData.TQVaultSaveFolder, this.PlayerName), " Equipment Export.txt")));

							TQDebug.DebugWriteLine(exception.ToString());
						}
					}

					if(playerReader.FoundPlayerInfo && !this.IsVault)
					{
						try
						{
							playerReader.Read(reader);
							this.PlayerInfo = playerReader.GetPlayerInfo();

						}
						catch (ArgumentException exception)
						{
							if (!TQDebug.DebugEnabled)
							{
								TQDebug.DebugEnabled = true;
							}

							TQDebug.DebugWriteLine(string.Format(CultureInfo.InvariantCulture, "Error parsing player file player info Block - '{0}'", this.PlayerName));
							TQDebug.DebugWriteLine(exception.ToString());
							throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Error parsing player player info Block - '{0}'", this.PlayerName), exception);
						}
					}
				}
			}
		}

		/// <summary>
		/// Encodes the live item data into raw binary format
		/// </summary>
		/// <returns>byte array holding the converted binary data</returns>
		private byte[] EncodeItemData()
		{
			int dataLength;
			byte[] data;

			// Encode the item data into a memory stream
			using (MemoryStream writeStream = new MemoryStream(2048))
			{
				using (BinaryWriter writer = new BinaryWriter(writeStream))
				{
					TQData.WriteCString(writer, "numberOfSacks");
					writer.Write(this.numberOfSacks);

					TQData.WriteCString(writer, "currentlyFocusedSackNumber");
					writer.Write(this.currentlyFocusedSackNumber);

					TQData.WriteCString(writer, "currentlySelectedSackNumber");
					writer.Write(this.currentlySelectedSackNumber);

					for (int i = 0; i < this.numberOfSacks; ++i)
					{
						// SackType should already be set at this point
						this.sacks[i].Encode(writer);
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
		private void ParseItemBlock(int offset, BinaryReader reader)
		{
			try
			{
				this.itemBlockStart = offset;

				reader.BaseStream.Seek(offset, SeekOrigin.Begin);

				TQData.ValidateNextString("numberOfSacks", reader);
				this.numberOfSacks = reader.ReadInt32();

				TQData.ValidateNextString("currentlyFocusedSackNumber", reader);
				this.currentlyFocusedSackNumber = reader.ReadInt32();

				TQData.ValidateNextString("currentlySelectedSackNumber", reader);
				this.currentlySelectedSackNumber = reader.ReadInt32();

				this.sacks = new SackCollection[this.numberOfSacks];

				for (int i = 0; i < this.numberOfSacks; ++i)
				{
					this.sacks[i] = new SackCollection();
					this.sacks[i].SackType = SackType.Sack;
					this.sacks[i].IsImmortalThrone = this.IsImmortalThrone;
					this.sacks[i].Parse(reader);
				}

				this.itemBlockEnd = (int)reader.BaseStream.Position;
			}
			catch (ArgumentException)
			{
				// The ValidateNextString Method can throw an ArgumentException.
				// We just pass it along at this point.
				throw;
			}
		}

		/// <summary>
		/// Encodes the live equipment data into raw binary
		/// </summary>
		/// <returns>byte array holding the converted binary data</returns>
		private byte[] EncodeEquipmentData()
		{
			int dataLength;
			byte[] data;

			// Encode the item data into a memory stream
			using (MemoryStream writeStream = new MemoryStream(2048))
			{
				using (BinaryWriter writer = new BinaryWriter(writeStream))
				{
					if (this.IsImmortalThrone)
					{
						TQData.WriteCString(writer, "equipmentCtrlIOStreamVersion");
						writer.Write(this.equipmentCtrlIOStreamVersion);
					}

					this.EquipmentSack.Encode(writer);

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
		private void ParseEquipmentBlock(int offset, BinaryReader reader)
		{
			try
			{
				this.equipmentBlockStart = offset;

				reader.BaseStream.Seek(offset, SeekOrigin.Begin);

				if (this.IsImmortalThrone)
				{
					TQData.ValidateNextString("equipmentCtrlIOStreamVersion", reader);
					this.equipmentCtrlIOStreamVersion = reader.ReadInt32();
				}

				this.EquipmentSack = new SackCollection();
				this.EquipmentSack.SackType = SackType.Equipment;
				this.EquipmentSack.IsImmortalThrone = this.IsImmortalThrone;
				this.EquipmentSack.Parse(reader);

				this.equipmentBlockEnd = (int)reader.BaseStream.Position;
			}
			catch (ArgumentException)
			{
				throw;
			}
		}

		/// <summary>
		/// Looks for the next begin_block or end_block.
		/// </summary>
		/// <param name="start">offset where we are starting our search</param>
		/// <returns>Returns the index of the first char indicating the block delimiter or -1 if none is found.</returns>
		private int FindNextBlockDelim(int start)
		{
			int beginMatch = 0;
			int endMatch = 0;

			for (int i = start; i < this.rawData.Length; ++i)
			{
				if (this.rawData[i] == beginBlockPattern[beginMatch])
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
					if (this.rawData[i] == beginBlockPattern[beginMatch])
					{
						++beginMatch;
					}
				}

				if (this.rawData[i] == endBlockPattern[endMatch])
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
					if (this.rawData[i] == endBlockPattern[endMatch])
					{
						++endMatch;
					}
				}
			}

			return -1;
		}
	}
}