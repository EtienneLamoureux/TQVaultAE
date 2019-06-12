//-----------------------------------------------------------------------
// <copyright file="Item.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.Data
{
	using ExpressionEvaluator;
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Drawing;
	using System.Globalization;
	using System.IO;
	using TQVaultAE.Entities;
	using TQVaultAE.Logs;



	/// <summary>
	/// Class for holding item information
	/// </summary>
	public static class ItemProvider
	{
		private static readonly log4net.ILog Log = Logger.Get(typeof(ItemProvider));

		#region Must be a flat prop

		/// <summary>
		/// Gets the artifact/charm/relic bonus loot table
		/// returns null if the item is not an artifact/charm/relic or does not contain a charm/relic
		/// </summary>
		public static LootTableCollection BonusTable(Item itm)
		{
			if (itm.baseItemInfo == null)
			{
				return null;
			}

			string lootTableID = null;
			if (itm.IsRelic)
			{
				lootTableID = itm.baseItemInfo.GetString("bonusTableName");
			}
			else if (itm.HasRelic)
			{
				if (itm.RelicInfo != null)
				{
					lootTableID = itm.RelicInfo.GetString("bonusTableName");
				}
			}
			else if (itm.IsArtifact)
			{
				// for artifacts we need to find the formulae that was used to create the artifact.  sucks to be us
				// The formulas seem to always be in the arcaneformulae subfolder with a _formula on the end
				// of the filename
				string folder = Path.GetDirectoryName(itm.BaseItemId);
				folder = Path.Combine(folder, "arcaneformulae");
				string file = Path.GetFileNameWithoutExtension(itm.BaseItemId);

				// Damn it, IL did not keep the filename consistent on Kingslayer (Sands of Kronos)
				if (file.ToUpperInvariant() == "E_GA_SANDOFKRONOS")
				{
					file = file.Insert(9, "s");
				}

				file = string.Concat(file, "_formula");
				file = Path.Combine(folder, file);
				file = Path.ChangeExtension(file, Path.GetExtension(itm.BaseItemId));

				// Now lookup itm record.
				DBRecordCollection record = Database.DB.GetRecordFromFile(file);
				if (record != null)
				{
					lootTableID = record.GetString("artifactBonusTableName", 0);
				}
			}

			if (lootTableID != null && lootTableID.Length > 0)
			{
				return new LootTableCollection(lootTableID);
			}

			return null;
		}

		#endregion

		#region Item Public Methods

		#region Item Public Static Methods





		#endregion Item Public Static Methods



		/// <summary>
		/// Removes the relic from this item
		/// </summary>
		/// <returns>Returns the removed relic as a new Item, if the item has two relics, 
		/// only the first one is returned and the second one is also removed</returns>
		public static Item RemoveRelic(Item itm)
		{
			if (!itm.HasRelic)
			{
				return null;
			}

			Item newRelic = itm.MakeEmptyCopy(itm.relicID);
			GetDBData(newRelic);
			newRelic.RelicBonusId = itm.RelicBonusId;
			newRelic.RelicBonusInfo = itm.RelicBonusInfo;

			// Now clear out our relic data
			itm.relicID = string.Empty;
			itm.relic2ID = string.Empty;
			itm.RelicBonusId = string.Empty;
			itm.RelicBonus2Id = string.Empty;
			itm.Var1 = 0;
			itm.Var2 = Item.var2Default;
			itm.RelicInfo = null;
			itm.RelicBonusInfo = null;
			itm.Relic2Info = null;
			itm.RelicBonus2Info = null;

			itm.MarkModified();

			return newRelic;
		}

		/// <summary>
		/// Create an artifact from its formulae
		/// </summary>
		/// <returns>A new artifact</returns>
		public static Item CraftArtifact(Item itm)
		{
			if (itm.IsFormulae && itm.baseItemInfo != null)
			{
				string artifactID = itm.baseItemInfo.GetString("artifactName");
				Item newArtifact = itm.MakeEmptyCopy(artifactID);
				GetDBData(newArtifact);

				itm.MarkModified();

				return newArtifact;
			}
			return null;
		}





		public static SortedList<string, Variable> GetRequirementVariables(Item itm)
		{
			if (itm.requirementsList != null)
			{
				return itm.requirementsList;
			}

			itm.requirementsList = new SortedList<string, Variable>();
			if (itm.baseItemInfo != null)
			{
				GetRequirementsFromRecord(itm.requirementsList, Database.DB.GetRecordFromFile(itm.BaseItemId));
				GetDynamicRequirementsFromRecord(itm, itm.requirementsList, itm.baseItemInfo);
			}

			if (itm.prefixInfo != null)
			{
				GetRequirementsFromRecord(itm.requirementsList, Database.DB.GetRecordFromFile(itm.prefixID));
			}

			if (itm.suffixInfo != null)
			{
				GetRequirementsFromRecord(itm.requirementsList, Database.DB.GetRecordFromFile(itm.suffixID));
			}

			if (itm.RelicInfo != null)
			{
				GetRequirementsFromRecord(itm.requirementsList, Database.DB.GetRecordFromFile(itm.relicID));
			}

			// Add Artifact level requirement to formula
			if (itm.IsFormulae && itm.baseItemInfo != null)
			{
				string artifactID = itm.baseItemInfo.GetString("artifactName");
				GetRequirementsFromRecord(itm.requirementsList, Database.DB.GetRecordFromFile(artifactID));
			}

			return itm.requirementsList;
		}




		/// <summary>
		/// Gets the itemID's of all the items in the set.
		/// </summary>
		/// <param name="includeName">Flag to include the set name in the returned array</param>
		/// <returns>Returns a string array containing the remaining set items or null if the item is not part of a set.</returns>
		public static string[] GetSetItems(Item itm, bool includeName)
		{
			if (itm.baseItemInfo == null)
			{
				return null;
			}

			string setID = itm.baseItemInfo.GetString("itemSetName");
			if (string.IsNullOrEmpty(setID))
			{
				return null;
			}

			// Get the set record
			DBRecordCollection setRecord = Database.DB.GetRecordFromFile(setID);
			if (setRecord == null)
			{
				return null;
			}

			string[] ans = setRecord.GetAllStrings("setMembers");
			if (ans == null || ans.Length == 0)
			{
				return null;
			}

			// Added by VillageIdiot to support set Name
			if (includeName)
			{
				string[] setitems = new string[ans.Length + 1];
				setitems[0] = setRecord.GetString("setName", 0);
				ans.CopyTo(setitems, 1);
				return setitems;
			}
			else
			{
				return ans;
			}
		}


		/// <summary>
		/// Encodes an item into the save file format
		/// </summary>
		/// <param name="writer">BinaryWriter instance</param>
		public static void Encode(Item itm, BinaryWriter writer)
		{
			int itemCount = itm.StackSize;
			int cx = itm.PositionX;
			int cy = itm.PositionY;
			int cseed = itm.Seed;

			if (itm.ContainerType != SackType.Sack && itm.ContainerType != SackType.Player)
			{
				// Equipment, Stashes, Vaults
				if (itm.ContainerType == SackType.Stash)
				{
					TQData.WriteCString(writer, "stackCount");
					writer.Write(itemCount - 1);
				}

				TQData.WriteCString(writer, "begin_block");
				writer.Write(itm.beginBlockCrap2);

				TQData.WriteCString(writer, "baseName");
				TQData.WriteCString(writer, itm.BaseItemId);

				TQData.WriteCString(writer, "prefixName");
				TQData.WriteCString(writer, itm.prefixID);

				TQData.WriteCString(writer, "suffixName");
				TQData.WriteCString(writer, itm.suffixID);

				TQData.WriteCString(writer, "relicName");
				TQData.WriteCString(writer, itm.relicID);

				TQData.WriteCString(writer, "relicBonus");
				TQData.WriteCString(writer, itm.RelicBonusId);

				TQData.WriteCString(writer, "seed");
				writer.Write(cseed);

				TQData.WriteCString(writer, "var1");
				writer.Write(itm.Var1);

				if (itm.atlantis)
				{
					TQData.WriteCString(writer, "relicName2");
					TQData.WriteCString(writer, itm.relic2ID);

					TQData.WriteCString(writer, "relicBonus2");
					TQData.WriteCString(writer, itm.RelicBonus2Id);

					TQData.WriteCString(writer, "var2");
					writer.Write(itm.Var2);
				}
				TQData.WriteCString(writer, "end_block");
				writer.Write(itm.endBlockCrap2);

				if (itm.ContainerType == SackType.Stash)
				{
					TQData.WriteCString(writer, "xOffset");
					writer.Write(Convert.ToSingle(cx, CultureInfo.InvariantCulture));

					TQData.WriteCString(writer, "yOffset");
					writer.Write(Convert.ToSingle(cy, CultureInfo.InvariantCulture));
				}
			}
			else
			{
				// itm is a sack
				// enter a while() loop so we can print out each potion in the stack if it is a potion stack
				while (true)
				{
					TQData.WriteCString(writer, "begin_block");
					writer.Write(itm.beginBlockCrap1);

					TQData.WriteCString(writer, "begin_block");
					writer.Write(itm.beginBlockCrap2);

					TQData.WriteCString(writer, "baseName");
					TQData.WriteCString(writer, itm.BaseItemId);

					TQData.WriteCString(writer, "prefixName");
					TQData.WriteCString(writer, itm.prefixID);

					TQData.WriteCString(writer, "suffixName");
					TQData.WriteCString(writer, itm.suffixID);

					TQData.WriteCString(writer, "relicName");
					TQData.WriteCString(writer, itm.relicID);

					TQData.WriteCString(writer, "relicBonus");
					TQData.WriteCString(writer, itm.RelicBonusId);

					TQData.WriteCString(writer, "seed");
					writer.Write(cseed);

					TQData.WriteCString(writer, "var1");
					writer.Write(itm.Var1);

					if (itm.atlantis)
					{
						TQData.WriteCString(writer, "relicName2");
						TQData.WriteCString(writer, itm.relic2ID);

						TQData.WriteCString(writer, "relicBonus2");
						TQData.WriteCString(writer, itm.RelicBonus2Id);

						TQData.WriteCString(writer, "var2");
						writer.Write(itm.Var2);
					}
					TQData.WriteCString(writer, "end_block");
					writer.Write(itm.endBlockCrap2);

					TQData.WriteCString(writer, "pointX");
					writer.Write(cx);

					TQData.WriteCString(writer, "pointY");
					writer.Write(cy);

					TQData.WriteCString(writer, "end_block");
					writer.Write(itm.endBlockCrap1);

					if (!itm.DoesStack)
					{
						return;
					}

					if (itemCount <= 1)
					{
						return;
					}

					// we have more items in the stack to write().
					--itemCount;
					cx = -1;
					cy = -1;
					cseed = Item.GenerateSeed(); // get a new seed for the next potion
				}
			}
		}

		/// <summary>
		/// Parses an item from the save file format
		/// </summary>
		/// <param name="reader">BinaryReader instance</param>
		public static void Parse(Item itm, BinaryReader reader)
		{
			try
			{
				if (itm.ContainerType == SackType.Stash)
				{
					TQData.ValidateNextString("stackCount", reader);
					itm.beginBlockCrap1 = reader.ReadInt32();
				}
				else if (itm.ContainerType == SackType.Sack || itm.ContainerType == SackType.Player)
				{
					TQData.ValidateNextString("begin_block", reader); // make sure we just read a new block
					itm.beginBlockCrap1 = reader.ReadInt32();
				}

				TQData.ValidateNextString("begin_block", reader); // make sure we just read a new block
				itm.beginBlockCrap2 = reader.ReadInt32();

				TQData.ValidateNextString("baseName", reader);
				itm.BaseItemId = TQData.ReadCString(reader);

				TQData.ValidateNextString("prefixName", reader);
				itm.prefixID = TQData.ReadCString(reader);

				TQData.ValidateNextString("suffixName", reader);
				itm.suffixID = TQData.ReadCString(reader);

				TQData.ValidateNextString("relicName", reader);
				itm.relicID = TQData.ReadCString(reader);

				TQData.ValidateNextString("relicBonus", reader);
				itm.RelicBonusId = TQData.ReadCString(reader);

				TQData.ValidateNextString("seed", reader);
				itm.Seed = reader.ReadInt32();

				TQData.ValidateNextString("var1", reader);
				itm.Var1 = reader.ReadInt32();

				if (TQData.MatchNextString("relicName2", reader))
				{
					TQData.ValidateNextString("relicName2", reader);
					itm.relic2ID = TQData.ReadCString(reader);
					itm.atlantis = true;
				}

				if (itm.atlantis)
				{
					TQData.ValidateNextString("relicBonus2", reader);
					itm.RelicBonus2Id = TQData.ReadCString(reader);

					TQData.ValidateNextString("var2", reader);
					itm.Var2 = reader.ReadInt32();
				}
				else
				{
					itm.relic2ID = string.Empty;
					itm.RelicBonus2Id = string.Empty;
					itm.Var2 = Item.var2Default;
				}

				TQData.ValidateNextString("end_block", reader);
				itm.endBlockCrap2 = reader.ReadInt32();

				if (itm.ContainerType == SackType.Stash)
				{
					TQData.ValidateNextString("xOffset", reader);
					itm.PositionX = Convert.ToInt32(reader.ReadSingle(), CultureInfo.InvariantCulture);

					TQData.ValidateNextString("yOffset", reader);
					itm.PositionY = Convert.ToInt32(reader.ReadSingle(), CultureInfo.InvariantCulture);
				}
				else if (itm.ContainerType == SackType.Equipment)
				{
					// Initially set the coordinates to (0, 0)
					itm.PositionX = 0;
					itm.PositionY = 0;
				}
				else
				{
					TQData.ValidateNextString("pointX", reader);
					itm.PositionX = reader.ReadInt32();

					TQData.ValidateNextString("pointY", reader);
					itm.PositionY = reader.ReadInt32();

					TQData.ValidateNextString("end_block", reader);
					itm.endBlockCrap1 = reader.ReadInt32();
				}

				GetDBData(itm);

				if (itm.ContainerType == SackType.Stash)
				{
					itm.StackSize = itm.beginBlockCrap1 + 1;
				}
				else
				{
					itm.StackSize = 1;
				}
			}
			catch (ArgumentException ex)
			{
				// The ValidateNextString Method can throw an ArgumentException.
				// We just pass it along at itm point.
				Log.Debug("ValidateNextString() fail !", ex);
				throw;
			}
		}

		/// <summary>
		/// Pulls data out of the TQ item database for this item.
		/// </summary>
		public static void GetDBData(Item itm)
		{
			if (TQDebug.ItemDebugLevel > 0)
			{
				Log.DebugFormat(CultureInfo.InvariantCulture, "Item.GetDBData ()   baseItemID = {0}", itm.BaseItemId);
			}

			itm.BaseItemId = CheckExtension(itm.BaseItemId);
			itm.baseItemInfo = Database.DB.GetInfo(itm.BaseItemId);

			itm.prefixID = CheckExtension(itm.prefixID);
			itm.suffixID = CheckExtension(itm.suffixID);

			if (TQDebug.ItemDebugLevel > 1)
			{
				Log.DebugFormat(CultureInfo.InvariantCulture, "prefixID = {0}", itm.prefixID);
				Log.DebugFormat(CultureInfo.InvariantCulture, "suffixID = {0}", itm.suffixID);
			}

			itm.prefixInfo = Database.DB.GetInfo(itm.prefixID);
			itm.suffixInfo = Database.DB.GetInfo(itm.suffixID);
			itm.relicID = CheckExtension(itm.relicID);
			itm.RelicBonusId = CheckExtension(itm.RelicBonusId);

			if (TQDebug.ItemDebugLevel > 1)
			{
				Log.DebugFormat(CultureInfo.InvariantCulture, "relicID = {0}", itm.relicID);
				Log.DebugFormat(CultureInfo.InvariantCulture, "relicBonusID = {0}", itm.RelicBonusId);
			}

			itm.RelicInfo = Database.DB.GetInfo(itm.relicID);
			itm.RelicBonusInfo = Database.DB.GetInfo(itm.RelicBonusId);

			itm.Relic2Info = Database.DB.GetInfo(itm.relic2ID);
			itm.RelicBonus2Info = Database.DB.GetInfo(itm.RelicBonus2Id);

			if (TQDebug.ItemDebugLevel > 1)
			{
				Log.DebugFormat(CultureInfo.InvariantCulture, "'{0}' baseItemInfo is {1} null", ToFriendlyName(itm), (itm.baseItemInfo == null) ? string.Empty : "NOT");
			}

			// Get the bitmaps we need
			if (itm.baseItemInfo != null)
			{
				if (itm.IsRelic && !itm.IsRelicComplete)
				{
					itm.ItemBitmap = Database.DB.LoadBitmap(itm.baseItemInfo.ShardBitmap);
					if (TQDebug.ItemDebugLevel > 1)
					{
						Log.DebugFormat(CultureInfo.InvariantCulture, "Loaded shardbitmap ({0})", itm.baseItemInfo.ShardBitmap);
					}
				}
				else
				{
					itm.ItemBitmap = Database.DB.LoadBitmap(itm.baseItemInfo.Bitmap);
					if (TQDebug.ItemDebugLevel > 1)
					{
						Log.DebugFormat(CultureInfo.InvariantCulture, "Loaded regular bitmap ({0})", itm.baseItemInfo.Bitmap);
					}
				}
			}
			else
			{
				// Added by VillageIdiot
				// Try showing something so unknown items are not invisible.
				itm.ItemBitmap = Database.DB.LoadBitmap("DefaultBitmap");
				if (TQDebug.ItemDebugLevel > 1)
				{
					Log.Debug("Try loading (DefaultBitmap)");
				}
			}

			// Changed by VillageIdiot
			// Moved outside of BaseItemInfo conditional since there are now 2 conditions
			if (itm.ItemBitmap != null)
			{
				if (TQDebug.ItemDebugLevel > 1)
				{
					Log.DebugFormat(CultureInfo.InvariantCulture
						, "size = {0}x{1} (unitsize={2})"
						, itm.ItemBitmap.Width
						, itm.ItemBitmap.Height
						, Database.DB.ItemUnitSize
					);
				}

				itm.Width = Convert.ToInt32((float)itm.ItemBitmap.Width * Database.DB.Scale / (float)Database.DB.ItemUnitSize);
				itm.Height = Convert.ToInt32((float)itm.ItemBitmap.Height * Database.DB.Scale / (float)Database.DB.ItemUnitSize);
			}
			else
			{
				if (TQDebug.ItemDebugLevel > 1)
				{
					Log.Debug("bitmap is null");
				}

				itm.Width = 1;
				itm.Height = 1;
			}

			if (TQDebug.ItemDebugLevel > 0)
			{
				Log.Debug("Exiting Item.GetDBData ()");
			}
		}

		#endregion Item Public Methods

		#region Item Private Methods

		#region Item Private Static Methods



		/// <summary>
		/// Holds all of the keys which we are filtering
		/// </summary>
		/// <param name="key">key which we are checking whether or not it gets filtered.</param>
		/// <returns>true if key is present in this list</returns>
		public static bool FilterKey(string key)
		{
			string keyUpper = key.ToUpperInvariant();
			string[] notWanted =
			{
				"MAXTRANSPARENCY",
				"SCALE",
				"CASTSSHADOWS",
				"MARKETADJUSTMENTPERCENT",
				"LOOTRANDOMIZERCOST",
				"LOOTRANDOMIZERJITTER",
				"ACTORHEIGHT",
				"ACTORRADIUS",
				"SHADOWBIAS",
				"ITEMLEVEL",
				"ITEMCOST",
				"COMPLETEDRELICLEVEL",
				"CHARACTERBASEATTACKSPEED",
				"HIDESUFFIXNAME",
				"HIDEPREFIXNAME",
				"AMULET",
				"RING",
				"HELMET",
				"GREAVES",
				"ARMBAND",
				"BODYARMOR",
				"BOW",
				"SPEAR",
				"STAFF",
				"MACE",
				"SWORD",
				"RANGEDONEHAND",
				"AXE",
				"SHIELD",
				"BRACELET",
				"AMULET",
				"RING",
				"BLOCKABSORPTION",
				"ITEMCOSTSCALEPERCENT", // Added by VillageIdiot
				"ITEMSKILLLEVEL", // Added by VillageIdiot
				"USEDELAYTIME", // Added by VillageIdiot
				"CAMERASHAKEAMPLITUDE", // Added by VillageIdiot
				"SKILLMAXLEVEL", // Added by VillageIdiot
				"SKILLCOOLDOWNTIME", // Added by VillageIdiot
				"EXPANSIONTIME", // Added by VillageIdiot
				"SKILLTIER", // Added by VillageIdiot
				"CAMERASHAKEDURATIONSECS", // Added by VillageIdiot
				"SKILLULTIMATELEVEL", // Added by VillageIdiot
				"SKILLCONNECTIONSPACING", // Added by VillageIdiot
				"PETBURSTSPAWN", // Added by VillageIdiot
				"PETLIMIT", // Added by VillageIdiot
				"ISPETDISPLAYABLE", // Added by VillageIdiot
				"SPAWNOBJECTSTIMETOLIVE", // Added by VillageIdiot
				"SKILLPROJECTILENUMBER", // Added by VillageIdiot
				"SKILLMASTERYLEVELREQUIRED", // Added by VillageIdiot
				"EXCLUDERACIALDAMAGE", // Added by VillageIdiot
				"SKILLWEAPONTINTRED", // Added by VillageIdiot
				"SKILLWEAPONTINTGREEN", // Added by VillageIdiot
				"SKILLWEAPONTINTBLUE", // Added by VillageIdiot
				"DEBUFSKILL", // Added by VillageIdiot
				"HIDEFROMUI", // Added by VillageIdiot
				"INSTANTCAST", // Added by VillageIdiot
				"WAVEENDWIDTH", // Added by VillageIdiot
				"WAVEDISTANCE",  // Added by VillageIdiot
				"WAVEDEPTH", // Added by VillageIdiot
				"WAVESTARTWIDTH", // Added by VillageIdiot
				"RAGDOLLAMPLIFICATION", // Added by VillageIdiot
				"WAVETIME", // Added by VillageIdiot
				"SPARKGAP", // Added by VillageIdiot
				"SPARKCHANCE", // Added by VillageIdiot
				"PROJECTILEUSESALLDAMAGE", // Added by VillageIdiot
				"DROPOFFSET", // Added by VillageIdiot
				"DROPHEIGHT", // Added by VillageIdiot
				"NUMPROJECTILES", // Added by VillageIdiot
				"SWORD", // Added by VillageIdiot
				"AXE", // Added by VillageIdiot
				"SPEAR", // Added by VillageIdiot
				"MACE", // Added by VillageIdiot
				"QUEST", // Added by VillageIdiot
				"CANNOTPICKUPMULTIPLE" // Added by VillageIdiot
			};

			if (Array.IndexOf(notWanted, keyUpper) != -1)
			{
				return true;
			}

			if (keyUpper.EndsWith("SOUND", StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}

			if (keyUpper.EndsWith("MESH", StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}

			if (keyUpper.StartsWith("BODYMASK", StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Holds all of the requirements which we are filtering
		/// </summary>
		/// <param name="key">key which we are checking whether or not it gets filtered.</param>
		/// <returns>true if key is present in this list</returns>
		public static bool FilterRequirements(string key)
		{
			string[] notWanted =
			{
				"LEVELREQUIREMENT",
				"INTELLIGENCEREQUIREMENT",
				"DEXTERITYREQUIREMENT",
				"STRENGTHREQUIREMENT",
			};

			if (Array.IndexOf(notWanted, key.ToUpperInvariant()) != -1)
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Gets s string containing the prefix of the item class for use in the requirements equation.
		/// </summary>
		/// <param name="itemClass">string containing the item class</param>
		/// <returns>string containing the prefix of the item class for use in the requirements equation</returns>
		private static string GetRequirementEquationPrefix(string itemClass)
		{
			switch (itemClass.ToUpperInvariant())
			{
				case "ARMORPROTECTIVE_HEAD":
					return "head";

				case "ARMORPROTECTIVE_FOREARM":
					return "forearm";

				case "ARMORPROTECTIVE_LOWERBODY":
					return "lowerBody";

				case "ARMORPROTECTIVE_UPPERBODY":
					return "upperBody";

				case "ARMORJEWELRY_BRACELET":
					return "bracelet";

				case "ARMORJEWELRY_RING":
					return "ring";

				case "ARMORJEWELRY_AMULET":
					return "amulet";

				case "WEAPONHUNTING_BOW":
					return "bow";

				case "WEAPONHUNTING_SPEAR":
					return "spear";

				case "WEAPONHUNTING_RANGEDONEHAND":
					return "bow";

				case "WEAPONMELEE_AXE":
					return "axe";

				case "WEAPONMELEE_SWORD":
					return "sword";

				case "WEAPONMELEE_MACE":
					return "mace";

				case "WEAPONMAGICAL_STAFF":
					return "staff";

				case "WEAPONARMOR_SHIELD":
					return "shield";

				default:
					return "none";
			}
		}


		/// <summary>
		/// Gets whether or not the variable contains a value which we are filtering.
		/// </summary>
		/// <param name="variable">Variable which we are checking.</param>
		/// <param name="allowStrings">Flag indicating whether or not we are allowing strings to show up</param>
		/// <returns>true if the variable contains a value which is filtered.</returns>
		public static bool FilterValue(Variable variable, bool allowStrings)
		{
			for (int i = 0; i < variable.NumberOfValues; i++)
			{
				switch (variable.DataType)
				{
					case VariableDataType.Integer:
						if (variable.GetInt32(i) != 0)
						{
							return false;
						}

						break;

					case VariableDataType.Float:
						if (variable.GetSingle(i) != 0.0)
						{
							return false;
						}

						break;

					case VariableDataType.StringVar:
						if ((allowStrings || variable.Name.ToUpperInvariant().Equals("CHARACTERBASEATTACKSPEEDTAG") ||
							variable.Name.ToUpperInvariant().Equals("ITEMSKILLNAME") || // Added by VillageIdiot for Granted skills
							variable.Name.ToUpperInvariant().Equals("SKILLNAME") || // Added by VillageIdiot for scroll skills
							variable.Name.ToUpperInvariant().Equals("PETBONUSNAME") || // Added by VillageIdiot for pet bonuses
							ItemAttributeProvider.IsReagent(variable.Name)) &&
							(variable.GetString(i).Length > 0))
						{
							return false;
						}

						break;

					case VariableDataType.Boolean:
						if (variable.GetInt32(i) != 0)
						{
							return false;
						}

						break;
				}
			}

			return true;
		}

		/// <summary>
		/// Checks to see if the id ends with .dbr and adds it if not.
		/// Sometimes the .dbr extension is not written into the item
		/// </summary>
		/// <param name="itemId">item id to be checked</param>
		/// <returns>string containing itemId with a .dbr extension.</returns>
		private static string CheckExtension(string itemId)
		{
			if (itemId == null)
			{
				return null;
			}

			if (itemId.Length < 4)
			{
				return itemId;
			}

			if (Path.GetExtension(itemId).ToUpperInvariant().Equals(".DBR"))
			{
				return itemId;
			}
			else
			{
				return string.Concat(itemId, ".dbr");
			}
		}



		/// <summary>
		/// Gets the item requirements from the database record
		/// </summary>
		/// <param name="requirements">SortedList of requirements</param>
		/// <param name="record">database record</param>
		private static void GetRequirementsFromRecord(SortedList<string, Variable> requirements, DBRecordCollection record)
		{
			if (TQDebug.ItemDebugLevel > 0)
			{
				Log.DebugFormat(CultureInfo.InvariantCulture, "Item.GetDynamicRequirementsFromRecord({0}, {1})", requirements, record);
			}

			if (record == null)
			{
				if (TQDebug.ItemDebugLevel > 0)
				{
					Log.Debug("Error - record was null.");
				}

				return;
			}

			if (TQDebug.ItemDebugLevel > 1)
			{
				Log.Debug(record.Id);
			}

			foreach (Variable variable in record)
			{
				if (TQDebug.ItemDebugLevel > 2)
				{
					Log.Debug(variable.Name);
				}

				if (FilterValue(variable, false))
				{
					continue;
				}

				if (!FilterRequirements(variable.Name))
				{
					continue;
				}

				string value = variable.ToStringValue();
				string key = variable.Name.Replace("Requirement", string.Empty);

				// Upper-case the first char of key
				key = string.Concat(key.Substring(0, 1).ToUpper(System.Globalization.CultureInfo.InvariantCulture), key.Substring(1));

				// Level needs to be LevelRequirement bah
				if (key.Equals("Level"))
				{
					key = "LevelRequirement";
				}

				if (requirements.ContainsKey(key))
				{
					Variable oldVariable = (Variable)requirements[key];

					// Changed by VillageIdiot
					// Comparison was failing when level difference was too high
					// (single digit vs multi-digit)
					if (value.Contains(",") || oldVariable.Name.Contains(","))
					{
						// Just in case there is something with multiple values
						// Keep the original code
						if (string.Compare(value, oldVariable.ToStringValue(), StringComparison.OrdinalIgnoreCase) <= 0)
						{
							continue;
						}

						requirements.Remove(key);
					}
					else
					{
						if (variable.GetInt32(0) <= oldVariable.GetInt32(0))
						{
							continue;
						}

						requirements.Remove(key);
					}
				}

				if (TQDebug.ItemDebugLevel > 1)
				{
					Log.DebugFormat("Added Requirement {0}={1}", key, value);
				}

				requirements.Add(key, variable);
			}

			if (TQDebug.ItemDebugLevel > 0)
			{
				Log.Debug("Exiting Item.GetDynamicRequirementsFromRecord()");
			}
		}



		#endregion Item Private Static Methods

		/// <summary>
		/// Gets the dynamic requirements from a database record.
		/// </summary>
		/// <param name="requirements">SortedList of requirements</param>
		/// <param name="itemInfo">ItemInfo for the item</param>
		private static void GetDynamicRequirementsFromRecord(Item itm, SortedList<string, Variable> requirements, Info itemInfo)
		{
			if (TQDebug.ItemDebugLevel > 0)
			{
				Log.DebugFormat(CultureInfo.InvariantCulture, "Item.GetDynamicRequirementsFromRecord({0}, {1})", requirements, itemInfo);
			}

			DBRecordCollection record = Database.DB.GetRecordFromFile(itemInfo.ItemId);
			if (record == null)
			{
				return;
			}

			string itemLevelTag = "itemLevel";
			Variable lvl = record[itemLevelTag];
			if (lvl == null)
			{
				return;
			}

			string itemLevel = lvl.ToStringValue();
			string itemCostID = itemInfo.GetString("itemCostName");

			record = Database.DB.GetRecordFromFile(itemCostID);
			if (record == null)
			{
				record = Database.DB.GetRecordFromFile("records/game/itemcost.dbr");
				if (record == null)
				{
					return;
				}
			}

			if (TQDebug.ItemDebugLevel > 1)
			{
				Log.Debug(record.Id);
			}

			string prefix = GetRequirementEquationPrefix(itemInfo.ItemClass);
			foreach (Variable variable in record)
			{
				if (string.Compare(variable.Name, 0, prefix, 0, prefix.Length, StringComparison.OrdinalIgnoreCase) != 0)
				{
					continue;
				}

				if (TQDebug.ItemDebugLevel > 2)
				{
					Log.Debug(variable.Name);
				}

				if (FilterValue(variable, true))
				{
					// our equation is a string, so we want also strings
					return;
				}

				string key = variable.Name.Replace(prefix, string.Empty);
				key = key.Replace("Equation", string.Empty);
				key = key.Replace(key[0], char.ToUpperInvariant(key[0]));

				// We need to ignore the cost equations.
				// Shields have costs so they will cause an overflow.
				if (key.Equals("Cost"))
				{
					continue;
				}

				var variableKey = key.ToLowerInvariant();
				if (variableKey == "level" || variableKey == "strength" || variableKey == "dexterity" || variableKey == "intelligence")
				{
					variableKey += "Requirement";
				}

				// Level needs to be LevelRequirement bah
				if (key.Equals("Level"))
				{
					key = "LevelRequirement";
				}

				string value = variable.ToStringValue().Replace(itemLevelTag, itemLevel);

				// Added by VillageIdiot
				// Changed to reflect Total Attribut count
				value = value.Replace("totalAttCount", Convert.ToString(itm.attributeCount, CultureInfo.InvariantCulture));

				Expression expression = ExpressionEvaluate.CreateExpression(value);

				// Changed by Bman to fix random overflow crashes
				Variable ans = new Variable(variableKey, VariableDataType.Integer, 1);

				// Changed by VillageIdiot to fix random overflow crashes.
				double tempVal = Math.Ceiling(Convert.ToDouble(expression.Evaluate(), CultureInfo.InvariantCulture));

				int intVal = 0;
				try
				{
					intVal = Convert.ToInt32(tempVal, CultureInfo.InvariantCulture);
				}
				catch (OverflowException)
				{
					intVal = 0;
				}

				ans[0] = intVal;

				value = ans.ToStringValue();
				if (requirements.ContainsKey(key))
				{
					if (string.Compare(value, ((Variable)requirements[key]).ToStringValue(), StringComparison.OrdinalIgnoreCase) <= 0)
					{
						return;
					}

					requirements.Remove(key);
				}

				if (TQDebug.ItemDebugLevel > 2)
				{
					Log.DebugFormat(CultureInfo.InvariantCulture, "Added Requirement {0}={1}", key, value);
				}

				requirements.Add(key, ans);
			}

			if (TQDebug.ItemDebugLevel > 0)
			{
				Log.Debug("Exiting Item.GetDynamicRequirementsFromRecord()");
			}
		}



		/// <summary>
		/// Gets the level of a triggered skill
		/// </summary>
		/// <param name="record">DBRecord for the triggered skill</param>
		/// <param name="recordId">string containing the record id</param>
		/// <param name="varNum">variable number which we are looking up since there can be multiple values</param>
		/// <returns>int containing the skill level</returns>
		public static int GetTriggeredSkillLevel(Item itm, DBRecordCollection record, string recordId, int varNum)
		{
			DBRecordCollection baseItem = Database.DB.GetRecordFromFile(itm.baseItemInfo.ItemId);

			// Check to see if it's a Buff Skill
			if (baseItem.GetString("itemSkillAutoController", 0) != null)
			{
				int level = baseItem.GetInt32("itemSkillLevel", 0);
				if (record.GetString("Class", 0).ToUpperInvariant().StartsWith("SKILLBUFF", StringComparison.OrdinalIgnoreCase))
				{
					DBRecordCollection skill = Database.DB.GetRecordFromFile(itm.baseItemInfo.GetString("itemSkillName"));
					if (skill != null && skill.GetString("buffSkillName", 0) == recordId)
					{
						// Use the level from the Base Item.
						varNum = Math.Max(level, 1) - 1;
					}
				}
				else if (baseItem.GetString("itemSkillName", 0) == recordId)
				{
					varNum = Math.Max(level, 1) - 1;
				}
			}

			return varNum;
		}

		/// <summary>
		/// Pet skills can also have multiple values so we attempt to decode it here
		/// unfortunately the level is not in the skill record so we need to look up
		/// the level from the pet record.
		/// </summary>
		/// <param name="record">DBRecord of the skill</param>
		/// <param name="recordId">string of the record id</param>
		/// <param name="varNum">Which variable we are using since there can be multiple values.</param>
		/// <returns>int containing the pet skill level</returns>
		public static int GetPetSkillLevel(Item itm, DBRecordCollection record, string recordId, int varNum)
		{
			// Check to see if itm really is a skill
			if (record.GetString("Class", 0).ToUpperInvariant().StartsWith("SKILL_ATTACK", StringComparison.OrdinalIgnoreCase))
			{
				// Check to see if itm item creates a pet
				DBRecordCollection petSkill = Database.DB.GetRecordFromFile(itm.baseItemInfo.GetString("skillName"));
				string petID = petSkill.GetString("spawnObjects", 0);
				if (!string.IsNullOrEmpty(petID))
				{
					DBRecordCollection petRecord = Database.DB.GetRecordFromFile(petID);
					int foundSkillOffset = 0;
					for (int skillOffset = 0; skillOffset < 17; skillOffset++)
					{
						// There are upto 17 skills
						// Find the skill in the skill tree so that we can get the level
						if (petRecord.GetString(string.Concat("skillName", skillOffset), 0) == recordId)
						{
							break;
						}

						foundSkillOffset++;
					}

					int level = petRecord.GetInt32(string.Concat("skillLevel", foundSkillOffset), 0);
					varNum = Math.Max(level, 1) - 1;
				}
			}

			return varNum;
		}


		#endregion Item Private Methods


		/// <summary>
		/// Formats a string based on the formatspec
		/// </summary>
		/// <param name="formatSpec">format specification</param>
		/// <param name="parameter1">first parameter</param>
		/// <param name="parameter2">second parameter</param>
		/// <param name="parameter3">third parameter</param>
		/// <returns>formatted string.</returns>
		public static string Format(string formatSpec, object parameter1, object parameter2 = null, object parameter3 = null)
		{
			try
			{
				return string.Format(CultureInfo.CurrentCulture, formatSpec, parameter1, parameter2, parameter3);
			}
			catch (ArgumentException)
			{
				string parameters = string.Format(
					CultureInfo.InvariantCulture,
					"\", '{0}', '{1}', '{2}'>",
					parameter1 == null ? "NULL" : parameter1,
					parameter2 == null ? "NULL" : parameter2,
					parameter3 == null ? "NULL" : parameter3);

				string error = string.Concat("FormatErr(\"", formatSpec, parameters);

				Logger.Log.Debug(error);

				return error;
			}
		}

		/// <summary>
		/// Gets a string containing the item name and attributes.
		/// </summary>
		/// <param name="basicInfoOnly">Flag indicating whether or not to return only basic info</param>
		/// <param name="relicInfoOnly">Flag indicating whether or not to return only relic info</param>
		/// /// <param name="secondRelic">Flag indicating whether or not to return second relic info</param>
		/// <returns>A string containing the item name and attributes</returns>
		public static string ToFriendlyName(Item itm, bool basicInfoOnly = false, bool relicInfoOnly = false, bool secondRelic = false)
		{
			string[] parameters = new string[16];
			int parameterCount = 0;
			Info relicInfoTarget = secondRelic ? itm.Relic2Info : itm.RelicInfo;
			string relicIdTarget = secondRelic ? itm.relic2ID : itm.relicID;
			string relicBonusTarget = secondRelic ? itm.RelicBonus2Id : itm.RelicBonusId;

			if (!relicInfoOnly)
			{
				if (!itm.IsRelic && !string.IsNullOrEmpty(itm.prefixID))
				{
					if (itm.prefixInfo != null)
					{
						parameters[parameterCount] = Database.DB.GetFriendlyName(itm.prefixInfo.DescriptionTag);
						if (string.IsNullOrEmpty(parameters[parameterCount]))
						{
							parameters[parameterCount] = itm.prefixID;
						}
					}
					else
					{
						parameters[parameterCount] = itm.prefixID;
					}

					++parameterCount;
				}

				if (itm.baseItemInfo == null)
				{
					parameters[parameterCount++] = itm.BaseItemId;
				}
				else
				{
					// style quality description
					if (!string.IsNullOrEmpty(itm.baseItemInfo.StyleTag))
					{
						if (!itm.IsPotion && !itm.IsRelic && !itm.IsScroll && !itm.IsParchment && !itm.IsQuestItem)
						{
							parameters[parameterCount] = Database.DB.GetFriendlyName(itm.baseItemInfo.StyleTag);
							if (string.IsNullOrEmpty(parameters[parameterCount]))
							{
								parameters[parameterCount] = itm.baseItemInfo.StyleTag;
							}

							++parameterCount;
						}
					}

					if (!string.IsNullOrEmpty(itm.baseItemInfo.QualityTag))
					{
						parameters[parameterCount] = Database.DB.GetFriendlyName(itm.baseItemInfo.QualityTag);
						if (string.IsNullOrEmpty(parameters[parameterCount]))
						{
							parameters[parameterCount] = itm.baseItemInfo.QualityTag;
						}

						++parameterCount;
					}

					parameters[parameterCount] = Database.DB.GetFriendlyName(itm.baseItemInfo.DescriptionTag);
					if (string.IsNullOrEmpty(parameters[parameterCount]))
					{
						parameters[parameterCount] = itm.BaseItemId;
					}

					++parameterCount;

					if (!basicInfoOnly && itm.IsRelic)
					{
						// Add the number of charms in the set acquired.
						if (itm.IsRelicComplete)
						{
							if (!string.IsNullOrEmpty(relicBonusTarget))
							{
								string bonus = Path.GetFileNameWithoutExtension(TQData.NormalizeRecordPath(relicBonusTarget));
								string tag = "tagRelicBonus";
								if (itm.IsCharm)
								{
									tag = "tagAnimalPartcompleteBonus";
								}

								string bonusTitle = Database.DB.GetFriendlyName(tag);
								if (string.IsNullOrEmpty(bonusTitle))
								{
									bonusTitle = "Completion Bonus: ";
								}

								parameters[parameterCount] = string.Format(CultureInfo.CurrentCulture, "({0} {1})", bonusTitle, bonus);
							}
							else
							{
								string tag = "tagRelicComplete";
								if (itm.IsCharm)
								{
									tag = "tagAnimalPartComplete";
								}

								string completed = Database.DB.GetFriendlyName(tag);
								if (string.IsNullOrEmpty(completed))
								{
									completed = "Completed";
								}

								parameters[parameterCount] = string.Concat("(", completed, ")");
							}
						}
						else
						{
							string tag1 = "tagRelicShard";
							string tag2 = "tagRelicRatio";
							if (itm.IsCharm)
							{
								tag1 = "tagAnimalPart";
								tag2 = "tagAnimalPartRatio";
							}

							string type = Database.DB.GetFriendlyName(tag1);
							if (string.IsNullOrEmpty(type))
							{
								type = "Relic";
							}

							string formatSpec = Database.DB.GetFriendlyName(tag2);
							if (string.IsNullOrEmpty(formatSpec))
							{
								formatSpec = "{0} - {1} / {2}";
							}
							else
							{
								formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
							}

							parameters[parameterCount] = string.Concat("(", Format(formatSpec, type, itm.Number, itm.baseItemInfo.CompletedRelicLevel), ")");
						}

						++parameterCount;
					}
					else if (!basicInfoOnly && itm.IsArtifact)
					{
						// Added by VillageIdiot
						// Add Artifact completion bonus
						if (!string.IsNullOrEmpty(itm.RelicBonusId))
						{
							string bonus = Path.GetFileNameWithoutExtension(TQData.NormalizeRecordPath(itm.RelicBonusId));
							string tag = "xtagArtifactBonus";
							string bonusTitle = Database.DB.GetFriendlyName(tag);
							if (string.IsNullOrEmpty(bonusTitle))
							{
								bonusTitle = "Completion Bonus: ";
							}

							parameters[parameterCount] = string.Format(CultureInfo.CurrentCulture, "({0} {1})", bonusTitle, bonus);
						}

						++parameterCount;
					}
					else if (itm.DoesStack)
					{
						// display the # potions
						if (itm.Number > 1)
						{
							parameters[parameterCount] = string.Format(CultureInfo.CurrentCulture, "({0:n0})", itm.Number);
							++parameterCount;
						}
					}
				}

				if (!itm.IsRelic && itm.suffixID.Length > 0)
				{
					if (itm.suffixInfo != null)
					{
						parameters[parameterCount] = Database.DB.GetFriendlyName(itm.suffixInfo.DescriptionTag);
						if (string.IsNullOrEmpty(parameters[parameterCount]))
						{
							parameters[parameterCount] = itm.suffixID;
						}
					}
					else
					{
						parameters[parameterCount] = itm.suffixID;
					}

					++parameterCount;
				}
			}

			if (!basicInfoOnly && itm.HasRelic)
			{
				if (!relicInfoOnly)
				{
					parameters[parameterCount++] = Item.ItemWith;
				}

				if (relicInfoTarget != null)
				{
					parameters[parameterCount] = Database.DB.GetFriendlyName(relicInfoTarget.DescriptionTag);
					if (string.IsNullOrEmpty(parameters[parameterCount]))
					{
						parameters[parameterCount] = relicIdTarget;
					}
				}
				else
				{
					parameters[parameterCount] = relicIdTarget;
				}

				++parameterCount;

				// Add the number of charms in the set acquired.
				if (!relicInfoOnly)
				{
					if (relicInfoTarget != null)
					{
						if (itm.Var1 >= relicInfoTarget.CompletedRelicLevel)
						{
							if (!relicInfoOnly && !string.IsNullOrEmpty(relicBonusTarget))
							{
								string bonus = Path.GetFileNameWithoutExtension(TQData.NormalizeRecordPath(relicBonusTarget));
								parameters[parameterCount] = string.Format(CultureInfo.CurrentCulture, Item.ItemRelicBonus, bonus);
							}
							else
							{
								parameters[parameterCount] = Item.ItemRelicCompleted;
							}
						}
						else
						{
							parameters[parameterCount] = string.Format(CultureInfo.CurrentCulture, "({0:n0}/{1:n0})", Math.Max(1, itm.Var1), relicInfoTarget.CompletedRelicLevel);
						}
					}
					else
					{
						parameters[parameterCount] = string.Format(CultureInfo.CurrentCulture, "({0:n0}/??)", itm.Var1);
					}

					++parameterCount;
				}
			}

			if (!relicInfoOnly && itm.IsQuestItem)
			{
				parameters[parameterCount++] = Item.ItemQuest;
			}

			if (!basicInfoOnly && !relicInfoOnly)
			{
				if (itm.IsImmortalThrone)
				{
					parameters[parameterCount++] = "(IT)";
				}
				else if (itm.IsRagnarok)
				{
					parameters[parameterCount++] = "(RAG)";
				}
				else if (itm.IsAtlantis)
				{
					parameters[parameterCount++] = "(ATL)";
				}
			}

			// Now combine it all with spaces between
			return string.Join(" ", parameters, 0, parameterCount);
		}
	}
}
