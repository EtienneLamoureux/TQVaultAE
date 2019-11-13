//-----------------------------------------------------------------------
// <copyright file="PlayerCollection.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.Data
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using TQVaultAE.Domain.Contracts.Providers;
	using TQVaultAE.Domain.Contracts.Services;
	using TQVaultAE.Domain.Entities;
	using TQVaultAE.Logs;

	/// <summary>
	/// Loads, decodes, encodes and saves a Titan Quest player file.
	/// </summary>
	public class PlayerCollectionProvider : IPlayerCollectionProvider
	{
		private readonly log4net.ILog Log;
		private readonly IItemProvider ItemProvider;
		private readonly ISackCollectionProvider SackCollectionProvider;
		private readonly IGamePathService GamePathResolver;
		private readonly ITQDataService TQData;

		/// <summary>
		/// array holding the byte pattern for the beginning of a block in the player file.
		/// </summary>
		public byte[] beginBlockPattern = { 0x0B, 0x00, 0x00, 0x00, 0x62, 0x65, 0x67, 0x69, 0x6E, 0x5F, 0x62, 0x6C, 0x6F, 0x63, 0x6B };

		/// <summary>
		/// array holding the byte pattern for the end of a block in the player file.
		/// </summary>
		public byte[] endBlockPattern = { 0x09, 0x00, 0x00, 0x00, 0x65, 0x6E, 0x64, 0x5F, 0x62, 0x6C, 0x6F, 0x63, 0x6B };

		public PlayerCollectionProvider(ILogger<PlayerCollectionProvider> log, IItemProvider itemProvider, ISackCollectionProvider sackCollectionProvider, IGamePathService gamePathResolver, ITQDataService tQData)
		{
			this.Log = log.Logger;
			this.ItemProvider = itemProvider;
			this.SackCollectionProvider = sackCollectionProvider;
			this.GamePathResolver = gamePathResolver;
			this.TQData = tQData;
		}

		public void CommitPlayerInfo(PlayerCollection pc, PlayerInfo playerInfo)
		{
			if (pc.PlayerInfo == null) return;
			if (playerInfo == null) return;
			if (pc.rawData == null || !pc.rawData.Any()) return;

			pc.PlayerInfo.CurrentLevel = playerInfo.CurrentLevel;
			pc.PlayerInfo.MasteriesAllowed = playerInfo.MasteriesAllowed;
			pc.PlayerInfo.MasteriesResetRequiered = playerInfo.MasteriesResetRequiered;
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
			Commit(pc);
		}


		/// <summary>
		/// Commits the player info changes to the player.chr file
		/// </summary>
		/// <param name="playerInfo"></param>
		/// <param name="playerFileRawData"></param>
		public void Commit(PlayerCollection pc)
		{
			TQData.WriteIntAfter(pc.rawData, "playerLevel", pc.PlayerInfo.CurrentLevel);

			TQData.WriteIntAfter(pc.rawData, "money", pc.PlayerInfo.Money);

			TQData.WriteIntAfter(pc.rawData, "masteriesAllowed", pc.PlayerInfo.MasteriesAllowed);

			// first "temp" / first "block"
			var difficultyUnlocked = TQData.WriteIntAfter(pc.rawData, "temp", pc.PlayerInfo.DifficultyUnlocked);

			TQData.WriteIntAfter(pc.rawData, "currentStats.charLevel", pc.PlayerInfo.CurrentLevel);

			TQData.WriteIntAfter(pc.rawData, "currentStats.experiencePoints", pc.PlayerInfo.CurrentXP);

			TQData.WriteIntAfter(pc.rawData, "modifierPoints", pc.PlayerInfo.AttributesPoints);

			TQData.WriteIntAfter(pc.rawData, "skillPoints", pc.PlayerInfo.SkillPoints);

			var baseStrength = TQData.WriteFloatAfter(pc.rawData, "temp", pc.PlayerInfo.BaseStrength, difficultyUnlocked.nextOffset);
			var baseDexterity = TQData.WriteFloatAfter(pc.rawData, "temp", pc.PlayerInfo.BaseDexterity, baseStrength.nextOffset);
			var baseIntelligence = TQData.WriteFloatAfter(pc.rawData, "temp", pc.PlayerInfo.BaseIntelligence, baseDexterity.nextOffset);
			var baseHealth = TQData.WriteFloatAfter(pc.rawData, "temp", pc.PlayerInfo.BaseHealth, baseIntelligence.nextOffset);
			var baseMana = TQData.WriteFloatAfter(pc.rawData, "temp", pc.PlayerInfo.BaseMana, baseHealth.nextOffset);

			if (pc.PlayerInfo.MasteriesAllowed_OldValue.HasValue && pc.PlayerInfo.MasteriesAllowed < pc.PlayerInfo.MasteriesAllowed_OldValue
				|| pc.PlayerInfo.MasteriesResetRequiered)
			{
				pc.PlayerInfo.ResetMasteries();

				#region Override skill lines block after reset

				// Find skill section boundaries
				var firstblock = TQData.ReadIntAfter(pc.rawData, "begin_block");
				var secondblock = TQData.ReadIntAfter(pc.rawData, "begin_block", firstblock.nextOffset);
				var max = TQData.ReadIntAfter(pc.rawData, "max", secondblock.nextOffset);// Boundary top
				var end_block = TQData.ReadIntAfter(pc.rawData, "end_block", max.nextOffset);
				var masteriesAllowed = TQData.ReadIntAfter(pc.rawData, "masteriesAllowed", max.nextOffset);// Boundary bottom

				// Split file
				var startfile = pc.rawData.Take(max.nextOffset).ToArray();
				var endfile = pc.rawData.Skip(masteriesAllowed.indexOf - 4).ToArray(); // -4 include key name length

				// make binary section
				var section = pc.PlayerInfo.SkillRecordList.SelectMany(s => s.ToBinary(secondblock.valueAsInt, end_block.valueAsInt)).ToArray();

				// put pieces back together
				pc.rawData = new[] {
					startfile,
					section,
					endfile,
				}.SelectMany(a => a).ToArray();

				// Adjust "max" value
				var found = TQData.WriteIntAfter(pc.rawData, "max", pc.PlayerInfo.SkillRecordList.Count, max.indexOf);

				// Adjust "skillPoints"
				var skillpointsToRestore = pc.PlayerInfo.ReleasedSkillPoints;

				if (skillpointsToRestore > 0)
					TQData.WriteIntAfter(pc.rawData, "skillPoints", pc.PlayerInfo.SkillPoints + skillpointsToRestore);

				#endregion
			}

			//if this value is set to true, the TQVaultAE program will know save the player.chr file
			pc.PlayerInfo.Modified = true;
		}

		/// <summary>
		/// Attempts to save the file.
		/// </summary>
		/// <param name="fileName">Name of the file to save</param>
		public void Save(PlayerCollection pc, string fileName)
		{
			byte[] data = Encode(pc);
			File.WriteAllBytes(fileName, data);
		}

		/// <summary>
		/// Converts the live data back into the raw binary data format.
		/// </summary>
		/// <returns>Byte Array holding the converted binary data</returns>
		public byte[] Encode(PlayerCollection pc)
		{
			// We need to encode the item data to a memory stream
			// then splice it back in place of the old item data
			byte[] rawItemData = EncodeItemData(pc);

			// vaults do not have all the other crapola.
			if (pc.IsVault)
				return rawItemData;

			// Added to support equipment panel
			byte[] rawEquipmentData = EncodeEquipmentData(pc);

			// May have change with skill reset so i get the new offsets
			var itemBlockStart = TQData.BinaryFindKey(pc.rawData, "numberOfSacks");
			var itemBlockStartOffset = itemBlockStart.indexOf - 4;// -4 to include key name length
			var itemBlockEnd = TQData.BinaryFindEndBlockOf(pc.rawData, "numberOfSacks");
			var itemBlockEndOffset = itemBlockEnd.indexOf - 4;

			var equipmentBlockStart = TQData.ReadIntAfter(pc.rawData, "useAlternate");
			var equipmentBlockStartOffset = equipmentBlockStart.nextOffset;
			var equipmentBlockEnd = TQData.BinaryFindEndBlockOf(pc.rawData, "useAlternate");
			var equipmentBlockEndOffset = equipmentBlockEnd.nextOffset;

			var ans = new[] {
				// Begining of the file
				pc.rawData.Take(itemBlockStartOffset).ToArray(),

				// new item segment
				rawItemData,

				// In between segment
				new ArraySegment<byte>(pc.rawData, itemBlockEndOffset, equipmentBlockStartOffset - itemBlockEndOffset).ToArray(),

				// new equipment segment
				rawEquipmentData,

				// End of file
				pc.rawData.Skip(equipmentBlockEndOffset).ToArray(),
			}.SelectMany(b => b).ToArray();

			return ans;
		}



		/// <summary>
		/// Attempts to load a player file
		/// </summary>
		public void LoadFile(PlayerCollection pc)
		{
			try
			{
				pc.rawData = File.ReadAllBytes(pc.PlayerFile);
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
		public int FindNextBlockDelim(PlayerCollection pc, int start)
		{
			int beginMatch = 0;
			int endMatch = 0;

			for (int i = start; i < pc.rawData.Length; ++i)
			{
				if (pc.rawData[i] == beginBlockPattern[beginMatch])
				{
					++beginMatch;
					if (beginMatch == beginBlockPattern.Length)
						return i + 1 - beginMatch;
				}
				else if (beginMatch > 0)
				{
					beginMatch = 0;

					// Test again to see if we are starting a new match
					if (pc.rawData[i] == beginBlockPattern[beginMatch])
						++beginMatch;
				}

				if (pc.rawData[i] == endBlockPattern[endMatch])
				{
					++endMatch;
					if (endMatch == endBlockPattern.Length)
						return i + 1 - endMatch;
				}
				else if (endMatch > 0)
				{
					endMatch = 0;

					// Test again to see if we are starting a new match
					if (pc.rawData[i] == endBlockPattern[endMatch])
						++endMatch;
				}
			}

			return -1;
		}

		/// <summary>
		/// Parses the raw binary data for use within TQVault
		/// </summary>
		private void ParseRawData(PlayerCollection pc)
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

						}
						else
						{
							// end block
							--blockNestLevel;
							currentOffset += endBlockPattern.Length;
						}
					}

					if (foundItems)
					{
						try
						{
							ParseItemBlock(pc, itemOffset, reader);
						}
						catch (ArgumentException exception)
						{
							var ex = new ArgumentException($"Error parsing player file Item Block- '{pc.PlayerName}'", exception);
							Log.ErrorException(ex);
							throw ex;
						}

						try
						{
							string outfile = string.Concat(Path.Combine(GamePathResolver.TQVaultSaveFolder, pc.PlayerName), " Export.txt");
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
												params1[1] = ItemProvider.GetFriendlyNames(item).FullNameBagTooltip;
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
							Log.ErrorFormat(exception, "Error writing Export file - '{0}' Export.txt"
								, Path.Combine(GamePathResolver.TQVaultSaveFolder, pc.PlayerName)
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
							var ex = new ArgumentException($"Error parsing player file Equipment Block - '{pc.PlayerName}'", exception);
							Log.ErrorException(ex);
							throw ex;
						}

						try
						{
							string outfile = string.Concat(Path.Combine(GamePathResolver.TQVaultSaveFolder, pc.PlayerName), " Equipment Export.txt");
							using (StreamWriter outStream = new StreamWriter(outfile, false))
							{
								if (!pc.EquipmentSack.IsEmpty)
								{
									int itemNumber = 0;
									foreach (Item item in pc.EquipmentSack)
									{
										object[] params1 = new object[20];

										params1[0] = itemNumber;
										params1[1] = ItemProvider.GetFriendlyNames(item).FullNameBagTooltip;
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
							Log.ErrorFormat(exception, "Error writing Export file - '{0}' Equipment Export.txt"
								, Path.Combine(GamePathResolver.TQVaultSaveFolder, pc.PlayerName)
							);
						}
					}

					if (pc.IsPlayer)
					{
						try
						{
							pc.PlayerInfo = ReadPlayerInfo(pc);
						}
						catch (ArgumentException ex)
						{
							var exx = new ArgumentException($"Error parsing player player info Block - '{pc.PlayerName}'", ex);
							Log.ErrorException(exx);
							throw exx;
						}
					}
				}
			}
		}

		/// <summary>
		/// Find character data in player.chr file
		/// </summary>
		public PlayerInfo ReadPlayerInfo(PlayerCollection pc)
		{
			var pi = new PlayerInfo();
			pi.Modified = false;

			var headerVersion = TQData.ReadIntAfter(pc.rawData, "headerVersion");
			pi.HeaderVersion = headerVersion.valueAsInt;

			var playerCharacterClass = TQData.ReadCStringAfter(pc.rawData, "playerCharacterClass");
			pi.PlayerCharacterClass = playerCharacterClass.valueAsString;

			var playerClassTag = TQData.ReadCStringAfter(pc.rawData, "playerClassTag");
			pi.Class = playerClassTag.valueAsString;

			var money = TQData.ReadIntAfter(pc.rawData, "money", playerClassTag.nextOffset);
			pi.Money = money.valueAsInt;

			var masteriesAllowed = TQData.ReadIntAfter(pc.rawData, "masteriesAllowed", money.nextOffset);
			pi.MasteriesAllowed = masteriesAllowed.valueAsInt;

			var difficultyUnlocked = TQData.ReadIntAfter(pc.rawData, "temp");// first "temp" / first "block"
			pi.DifficultyUnlocked = difficultyUnlocked.valueAsInt;

			var hasBeenInGame = TQData.ReadIntAfter(pc.rawData, "hasBeenInGame", difficultyUnlocked.nextOffset);
			pi.HasBeenInGame = hasBeenInGame.valueAsInt;

			var currentStatscharLevel = TQData.ReadIntAfter(pc.rawData, "currentStats.charLevel", hasBeenInGame.nextOffset);
			pi.CurrentLevel = currentStatscharLevel.valueAsInt;

			var currentStatsexperiencePoints = TQData.ReadIntAfter(pc.rawData, "currentStats.experiencePoints", currentStatscharLevel.nextOffset);
			pi.CurrentXP = currentStatsexperiencePoints.valueAsInt;

			var modifierPoints = TQData.ReadIntAfter(pc.rawData, "modifierPoints", currentStatsexperiencePoints.nextOffset);
			pi.AttributesPoints = modifierPoints.valueAsInt;

			var skillPoints = TQData.ReadIntAfter(pc.rawData, "skillPoints", modifierPoints.nextOffset);
			pi.SkillPoints = skillPoints.valueAsInt;

			var baseStrength = TQData.ReadFloatAfter(pc.rawData, "temp", difficultyUnlocked.nextOffset);// first "temp" after first one (difficultyUnlocked)
			pi.BaseStrength = Convert.ToInt32(baseStrength.valueAsFloat);

			var baseDexterity = TQData.ReadFloatAfter(pc.rawData, "temp", baseStrength.nextOffset);// second "temp" after first one
			pi.BaseDexterity = Convert.ToInt32(baseDexterity.valueAsFloat);

			var baseIntelligence = TQData.ReadFloatAfter(pc.rawData, "temp", baseDexterity.nextOffset);// third "temp" after first one
			pi.BaseIntelligence = Convert.ToInt32(baseIntelligence.valueAsFloat);

			var baseHealth = TQData.ReadFloatAfter(pc.rawData, "temp", baseIntelligence.nextOffset);// fourth "temp" after first one
			pi.BaseHealth = Convert.ToInt32(baseHealth.valueAsFloat);

			var baseMana = TQData.ReadFloatAfter(pc.rawData, "temp", baseHealth.nextOffset);// fifth "temp" after first one
			pi.BaseMana = Convert.ToInt32(baseMana.valueAsFloat);

			var playTimeInSeconds = TQData.ReadIntAfter(pc.rawData, "playTimeInSeconds", baseMana.nextOffset);
			pi.PlayTimeInSeconds = playTimeInSeconds.valueAsInt;

			var numberOfDeaths = TQData.ReadIntAfter(pc.rawData, "numberOfDeaths", playTimeInSeconds.nextOffset);
			pi.NumberOfDeaths = numberOfDeaths.valueAsInt;

			var numberOfKills = TQData.ReadIntAfter(pc.rawData, "numberOfKills", numberOfDeaths.nextOffset);
			pi.NumberOfKills = numberOfKills.valueAsInt;

			var experienceFromKills = TQData.ReadIntAfter(pc.rawData, "experienceFromKills", numberOfKills.nextOffset);
			pi.ExperienceFromKills = experienceFromKills.valueAsInt;

			var healthPotionsUsed = TQData.ReadIntAfter(pc.rawData, "healthPotionsUsed", experienceFromKills.nextOffset);
			pi.HealthPotionsUsed = healthPotionsUsed.valueAsInt;

			var manaPotionsUsed = TQData.ReadIntAfter(pc.rawData, "manaPotionsUsed", healthPotionsUsed.nextOffset);
			pi.ManaPotionsUsed = manaPotionsUsed.valueAsInt;

			var maxLevel = TQData.ReadIntAfter(pc.rawData, "maxLevel", manaPotionsUsed.nextOffset);
			pi.MaxLevel = maxLevel.valueAsInt;

			var numHitsReceived = TQData.ReadIntAfter(pc.rawData, "numHitsReceived", maxLevel.nextOffset);
			pi.NumHitsReceived = numHitsReceived.valueAsInt;

			var numHitsInflicted = TQData.ReadIntAfter(pc.rawData, "numHitsInflicted", numHitsReceived.nextOffset);
			pi.NumHitsInflicted = numHitsInflicted.valueAsInt;

			var greatestDamageInflicted = TQData.ReadIntAfter(pc.rawData, "greatestDamageInflicted", numHitsInflicted.nextOffset);
			pi.GreatestDamageInflicted = greatestDamageInflicted.valueAsInt;

			var greatestMonsterKilledName = TQData.ReadUnicodeStringAfter(pc.rawData, "(*greatestMonsterKilledName)[i]", greatestDamageInflicted.nextOffset);
			pi.GreatestMonster = greatestMonsterKilledName.valueAsString;

			var greatestMonsterKilledLevel = TQData.ReadIntAfter(pc.rawData, "(*greatestMonsterKilledLevel)[i]", greatestMonsterKilledName.nextOffset);
			pi.GreatestMonsterKilledLevel = greatestMonsterKilledLevel.valueAsInt;

			var greatestMonsterKilledLifeAndMana = TQData.ReadIntAfter(pc.rawData, "(*greatestMonsterKilledLifeAndMana)[i]", greatestMonsterKilledLevel.nextOffset);
			pi.GreatestMonsterKilledLifeAndMana = greatestMonsterKilledLevel.valueAsInt;

			var greatestMonsterKilledName2 = TQData.ReadUnicodeStringAfter(pc.rawData, "(*greatestMonsterKilledName)[i]", greatestMonsterKilledLifeAndMana.nextOffset);

			var greatestMonsterKilledLevel2 = TQData.ReadIntAfter(pc.rawData, "(*greatestMonsterKilledLevel)[i]", greatestMonsterKilledName2.nextOffset);

			var greatestMonsterKilledLifeAndMana2 = TQData.ReadIntAfter(pc.rawData, "(*greatestMonsterKilledLifeAndMana)[i]", greatestMonsterKilledLevel2.nextOffset);

			var greatestMonsterKilledName3 = TQData.ReadUnicodeStringAfter(pc.rawData, "(*greatestMonsterKilledName)[i]", greatestMonsterKilledLifeAndMana2.nextOffset);

			var greatestMonsterKilledLevel3 = TQData.ReadIntAfter(pc.rawData, "(*greatestMonsterKilledLevel)[i]", greatestMonsterKilledName3.nextOffset);

			var greatestMonsterKilledLifeAndMana3 = TQData.ReadIntAfter(pc.rawData, "(*greatestMonsterKilledLifeAndMana)[i]", greatestMonsterKilledLevel3.nextOffset);

			var criticalHitsInflicted = TQData.ReadIntAfter(pc.rawData, "criticalHitsInflicted", greatestMonsterKilledLifeAndMana3.nextOffset);
			pi.CriticalHitsInflicted = criticalHitsInflicted.valueAsInt;

			var criticalHitsReceived = TQData.ReadIntAfter(pc.rawData, "criticalHitsReceived", criticalHitsInflicted.nextOffset);
			pi.CriticalHitsReceived = criticalHitsReceived.valueAsInt;

			// Parse skills collection
			var firstblock = TQData.BinaryFindKey(pc.rawData, "begin_block");
			var secondblock = TQData.BinaryFindKey(pc.rawData, "begin_block", firstblock.nextOffset);
			var max = TQData.ReadIntAfter(pc.rawData, "max", secondblock.nextOffset);
			// Loop of sub block
			int nextOffset = max.nextOffset;
			for (var i = 0; i < max.valueAsInt; i++)
			{
				var begin_block = TQData.ReadIntAfter(pc.rawData, "begin_block", nextOffset);
				var skillName = TQData.ReadCStringAfter(pc.rawData, "skillName", nextOffset);
				var skillLevel = TQData.ReadIntAfter(pc.rawData, "skillLevel", nextOffset);
				var skillEnabled = TQData.ReadIntAfter(pc.rawData, "skillEnabled", nextOffset);
				var skillSubLevel = TQData.ReadIntAfter(pc.rawData, "skillSubLevel", nextOffset);
				var skillActive = TQData.ReadIntAfter(pc.rawData, "skillActive", nextOffset);
				var skillTransition = TQData.ReadIntAfter(pc.rawData, "skillTransition", nextOffset);
				pi.SkillRecordList.Add(new SkillRecord()
				{
					skillActive = skillActive.valueAsInt,
					skillEnabled = skillEnabled.valueAsInt,
					skillLevel = skillLevel.valueAsInt,
					skillName = skillName.valueAsString,
					skillSubLevel = skillSubLevel.valueAsInt,
					skillTransition = skillTransition.valueAsInt,
				});
				nextOffset = skillTransition.nextOffset;
			}

			return pi;
		}

		/// <summary>
		/// Encodes the live item data into raw binary format
		/// </summary>
		/// <returns>byte array holding the converted binary data</returns>
		private byte[] EncodeItemData(PlayerCollection pc)
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
				return data;

			byte[] realData = new byte[dataLength];
			Array.Copy(data, realData, dataLength);
			return realData;
		}

		/// <summary>
		/// Parses the item block
		/// </summary>
		/// <param name="offset">offset in the player file</param>
		/// <param name="reader">BinaryReader instance</param>
		private void ParseItemBlock(PlayerCollection pc, int offset, BinaryReader reader)
		{
			try
			{
				//pc.itemBlockStart = offset;

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

				//pc.itemBlockEnd = (int)reader.BaseStream.Position;

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
		private byte[] EncodeEquipmentData(PlayerCollection pc)
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
				return data;

			byte[] realData = new byte[dataLength];
			Array.Copy(data, realData, dataLength);
			return realData;
		}


		/// <summary>
		/// Parses the binary equipment block data
		/// </summary>
		/// <param name="offset">offset of the block within the player file.</param>
		/// <param name="reader">BinaryReader instance</param>
		private void ParseEquipmentBlock(PlayerCollection pc, int offset, BinaryReader reader)
		{
			try
			{
				//pc.equipmentBlockStart = offset;

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

				//pc.equipmentBlockEnd = (int)reader.BaseStream.Position;

			}
			catch (ArgumentException ex)
			{
				Log.Error($"ParseEquipmentBlock fail ! offset={offset}", ex);
				throw;
			}
		}

	}
}