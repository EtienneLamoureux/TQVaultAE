//-----------------------------------------------------------------------
// <copyright file="Item.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.Data
{
	using Microsoft.Extensions.Logging;
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Text.RegularExpressions;
	using TQVaultAE.Config;
	using TQVaultAE.Domain.Contracts.Providers;
	using TQVaultAE.Domain.Contracts.Services;
	using TQVaultAE.Domain.Entities;
	using TQVaultAE.Domain.Helpers;
	using TQVaultAE.Domain.Results;



	/// <summary>
	/// Class for holding item information
	/// </summary>
	public class ItemProvider : IItemProvider
	{
		private readonly ILogger Log;
		private readonly IDatabase Database;
		private readonly ILootTableCollectionProvider LootTableCollectionProvider;
		private readonly IItemAttributeProvider ItemAttributeProvider;
		private readonly ITQDataService TQData;
		private readonly ITranslationService TranslationService;
		private readonly LazyConcurrentDictionary<(Item Item, FriendlyNamesExtraScopes? Scope, bool FilterExtra), ToFriendlyNameResult> FriendlyNamesCache = new LazyConcurrentDictionary<(Item, FriendlyNamesExtraScopes?, bool), ToFriendlyNameResult>();

		public ItemProvider(
			ILogger<ItemProvider> log
			, IDatabase database
			, ILootTableCollectionProvider lootTableCollectionProvider
			, IItemAttributeProvider itemAttributeProvider
			, ITQDataService tQData
			, ITranslationService translationService
		)
		{
			this.Log = log;
			this.Database = database;
			this.LootTableCollectionProvider = lootTableCollectionProvider;
			this.ItemAttributeProvider = itemAttributeProvider;
			this.TQData = tQData;
			this.TranslationService = translationService;
		}

		public bool InvalidateFriendlyNamesCache(params Item[] items)
		{
			items = items.Where(i => i != null).ToArray();
			var keylist = this.FriendlyNamesCache.Where(i => items.Contains(i.Key.Item)).Select(i => i.Key).ToList();
			keylist.ForEach(k => this.FriendlyNamesCache.TryRemove(k, out var outVal));
			return keylist.Any();
		}

		#region Must be a flat prop

		/// <summary>
		/// Gets the artifact/charm/relic bonus loot table
		/// returns null if the item is not an artifact/charm/relic or does not contain a charm/relic
		/// </summary>
		public LootTableCollection BonusTable(Item itm)
		{
			if (itm.baseItemInfo == null)
				return null;

			string lootTableID = null;
			if (itm.IsRelic)
				lootTableID = itm.baseItemInfo.GetString("bonusTableName");
			else if (itm.HasRelicSlot1)
			{
				if (itm.RelicInfo != null)
					lootTableID = itm.RelicInfo.GetString("bonusTableName");
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
					file = file.Insert(9, "s");

				file = string.Concat(file, "_formula");
				file = Path.Combine(folder, file);
				file = Path.ChangeExtension(file, Path.GetExtension(itm.BaseItemId));

				// Now lookup itm record.
				DBRecordCollection record = Database.GetRecordFromFile(file);
				if (record != null)
					lootTableID = record.GetString("artifactBonusTableName", 0);
			}

			if (lootTableID != null && lootTableID.Length > 0)
				return LootTableCollectionProvider.LoadTable(lootTableID);

			return null;
		}

		#endregion

		#region Item Public Methods

		/// <summary>
		/// Removes the relic from this item
		/// </summary>
		/// <returns>Returns the removed relic as a new Item, if the item has two relics, 
		/// only the first one is returned and the second one is also removed</returns>
		public Item RemoveRelic(Item itm)
		{
			if (!itm.HasRelicSlot1)
				return null;

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

			itm.IsModified = true;

			return newRelic;
		}

		/// <summary>
		/// Create an artifact from its formulae
		/// </summary>
		/// <returns>A new artifact</returns>
		public Item CraftArtifact(Item itm)
		{
			if (itm.IsFormulae && itm.baseItemInfo != null)
			{
				string artifactID = itm.baseItemInfo.GetString("artifactName");
				Item newArtifact = itm.MakeEmptyCopy(artifactID);
				GetDBData(newArtifact);

				itm.IsModified = true;

				return newArtifact;
			}
			return null;
		}

		public SortedList<string, Variable> GetRequirementVariables(Item itm)
		{
			var RequirementVariables = new SortedList<string, Variable>();
			if (itm.baseItemInfo != null)
			{
				GetRequirementsFromRecord(RequirementVariables, Database.GetRecordFromFile(itm.BaseItemId));
				GetDynamicRequirementsFromRecord(itm, RequirementVariables, itm.baseItemInfo);
			}

			if (itm.prefixInfo != null)
				GetRequirementsFromRecord(RequirementVariables, Database.GetRecordFromFile(itm.prefixID));

			if (itm.suffixInfo != null)
				GetRequirementsFromRecord(RequirementVariables, Database.GetRecordFromFile(itm.suffixID));

			if (itm.RelicInfo != null)
				GetRequirementsFromRecord(RequirementVariables, Database.GetRecordFromFile(itm.relicID));

			if (itm.Relic2Info != null)
				GetRequirementsFromRecord(RequirementVariables, Database.GetRecordFromFile(itm.relic2ID));

			// Add Artifact level requirement to formula
			if (itm.IsFormulae && itm.baseItemInfo != null)
			{
				string artifactID = itm.baseItemInfo.GetString("artifactName");
				GetRequirementsFromRecord(RequirementVariables, Database.GetRecordFromFile(artifactID));
			}

			return RequirementVariables;
		}

		/// <summary>
		/// Gets the itemID's of all the items in the set.
		/// </summary>
		/// <param name="includeName">Flag to include the set name in the returned array</param>
		/// <returns>Returns a string array containing the remaining set items or null if the item is not part of a set.</returns>
		public string[] GetSetItems(Item itm, bool includeName)
		{
			if (itm.baseItemInfo == null)
				return null;

			string setID = itm.baseItemInfo.GetString("itemSetName");
			if (string.IsNullOrEmpty(setID))
				return null;

			// Get the set record
			DBRecordCollection setRecord = Database.GetRecordFromFile(setID);
			if (setRecord == null)
				return null;

			string[] ans = setRecord.GetAllStrings("setMembers");
			if (ans == null || ans.Length == 0)
				return null;

			// Added by VillageIdiot to support set Name
			if (includeName)
			{
				string[] setitems = new string[ans.Length + 1];
				setitems[0] = setRecord.GetString("setName", 0);
				ans.CopyTo(setitems, 1);
				return setitems;
			}
			else
				return ans;
		}


		/// <summary>
		/// Encodes an item into the save file format
		/// </summary>
		/// <param name="writer">BinaryWriter instance</param>
		public void Encode(Item itm, BinaryWriter writer)
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
						return;

					if (itemCount <= 1)
						return;

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
		public void Parse(Item itm, BinaryReader reader)
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
					itm.StackSize = itm.beginBlockCrap1 + 1;
				else
					itm.StackSize = 1;
			}
			catch (ArgumentException ex)
			{
				// The ValidateNextString Method can throw an ArgumentException.
				// We just pass it along at itm point.
				Log.LogDebug(ex, "ValidateNextString() fail !");
				throw;
			}
		}

		/// <summary>
		/// Pulls data out of the TQ item database for this item.
		/// </summary>
		public void GetDBData(Item itm)
		{
			if (TQDebug.ItemDebugLevel > 0)
				Log.LogDebug("Item.GetDBData ()   baseItemID = {0}", itm.BaseItemId);

			itm.BaseItemId = CheckExtension(itm.BaseItemId);
			itm.baseItemInfo = Database.GetInfo(itm.BaseItemId);

			itm.prefixID = CheckExtension(itm.prefixID);
			itm.suffixID = CheckExtension(itm.suffixID);

			if (TQDebug.ItemDebugLevel > 1)
			{
				Log.LogDebug("prefixID = {0}", itm.prefixID);
				Log.LogDebug("suffixID = {0}", itm.suffixID);
			}

			itm.prefixInfo = Database.GetInfo(itm.prefixID);
			itm.suffixInfo = Database.GetInfo(itm.suffixID);
			itm.relicID = CheckExtension(itm.relicID);
			itm.RelicBonusId = CheckExtension(itm.RelicBonusId);

			if (TQDebug.ItemDebugLevel > 1)
			{
				Log.LogDebug("relicID = {0}", itm.relicID);
				Log.LogDebug("relicBonusID = {0}", itm.RelicBonusId);
			}

			itm.RelicInfo = Database.GetInfo(itm.relicID);
			itm.RelicBonusInfo = Database.GetInfo(itm.RelicBonusId);

			itm.Relic2Info = Database.GetInfo(itm.relic2ID);
			itm.RelicBonus2Info = Database.GetInfo(itm.RelicBonus2Id);

			if (TQDebug.ItemDebugLevel > 1)
				Log.LogDebug("'{0}' baseItemInfo is {1} null"
					, GetFriendlyNames(itm).FullNameBagTooltip
					, (itm.baseItemInfo == null) ? string.Empty : "NOT"
				);

			// Extract image raw data
			if (itm.baseItemInfo != null)
			{
				if (itm.IsRelic && !itm.IsRelicComplete)
				{
					itm.TexImageResourceId = itm.baseItemInfo.ShardBitmap;
					itm.TexImage = Database.LoadResource(itm.TexImageResourceId);
					if (TQDebug.ItemDebugLevel > 1)
						Log.LogDebug("Loaded shardbitmap ({0})", itm.baseItemInfo.ShardBitmap);
				}
				else
				{
					itm.TexImageResourceId = itm.baseItemInfo.Bitmap;
					itm.TexImage = Database.LoadResource(itm.TexImageResourceId);
					if (TQDebug.ItemDebugLevel > 1)
						Log.LogDebug("Loaded regular bitmap ({0})", itm.baseItemInfo.Bitmap);
				}
			}
			else
			{
				// Added by VillageIdiot
				// Try showing something so unknown items are not invisible.
				itm.TexImageResourceId = "DefaultBitmap";
				itm.TexImage = Database.LoadResource(itm.TexImageResourceId);
				if (TQDebug.ItemDebugLevel > 1)
					Log.LogDebug("Try loading (DefaultBitmap)");
			}

			if (TQDebug.ItemDebugLevel > 0)
				Log.LogDebug("Exiting Item.GetDBData ()");
		}

		#endregion Item Public Methods

		#region Item Private Methods

		#region Item Private Methods

		/// <summary>
		/// Holds all of the keys which we are filtering
		/// </summary>
		/// <param name="key">key which we are checking whether or not it gets filtered.</param>
		/// <returns>true if key is present in this list</returns>
		public bool FilterKey(string key)
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
				"CANNOTPICKUPMULTIPLE", // Added by VillageIdiot
				"BONUSLIFEPERCENT",
				"BONUSLIFEPOINTS",
				"BONUSMANAPERCENT",
				"BONUSMANAPOINTS",
				"DISPLAYASQUESTITEM",  // New tags from the latest expansions.
				"ACTORSCALE",
				"ACTORSCALETIME"
			};

			return (Array.IndexOf(notWanted, keyUpper) != -1
				|| keyUpper.EndsWith("SOUND", StringComparison.OrdinalIgnoreCase)
				|| keyUpper.EndsWith("MESH", StringComparison.OrdinalIgnoreCase)
				|| keyUpper.StartsWith("BODYMASK", StringComparison.OrdinalIgnoreCase)
			);
		}

		/// <summary>
		/// Holds all of the requirements which we are filtering
		/// </summary>
		/// <param name="key">key which we are checking whether or not it gets filtered.</param>
		/// <returns>true if key is present in this list</returns>
		public bool FilterRequirements(string key)
		{
			string[] notWanted =
			{
				"LEVELREQUIREMENT",
				"INTELLIGENCEREQUIREMENT",
				"DEXTERITYREQUIREMENT",
				"STRENGTHREQUIREMENT",
			};

			return Array.IndexOf(notWanted, key.ToUpperInvariant()) != -1;
		}

		internal static ReadOnlyCollection<(string ItemClass, string RequirementEquationPrefix)> ItemClassMap = new[]
		{
			("ARMORPROTECTIVE_HEAD", "head"),
			("ARMORPROTECTIVE_FOREARM", "forearm"),
			("ARMORPROTECTIVE_LOWERBODY", "lowerBody"),
			("ARMORPROTECTIVE_UPPERBODY", "upperBody"),
			("ARMORJEWELRY_BRACELET", "bracelet"),
			("ARMORJEWELRY_RING", "ring"),
			("ARMORJEWELRY_AMULET", "amulet"),
			("WEAPONHUNTING_BOW", "bow"),
			("WEAPONHUNTING_SPEAR", "spear"),
			("WEAPONHUNTING_RANGEDONEHAND", "bow"),
			("WEAPONMELEE_AXE", "axe"),
			("WEAPONMELEE_SWORD", "sword"),
			("WEAPONMELEE_MACE", "mace"),
			("WEAPONMAGICAL_STAFF", "staff"),
			("WEAPONARMOR_SHIELD", "shield"),
		}.ToList().AsReadOnly();

		/// <summary>
		/// Gets s string containing the prefix of the item class for use in the requirements equation.
		/// </summary>
		/// <param name="itemClass">string containing the item class</param>
		/// <returns>string containing the prefix of the item class for use in the requirements equation</returns>
		private string GetRequirementEquationPrefix(string itemClass)
		{
			var itemClassUI = itemClass.ToUpperInvariant();
			var map = ItemClassMap.Where(m => m.ItemClass == itemClassUI).Select(m => m.RequirementEquationPrefix);
			return map.Any() ? map.First() : "none";
		}


		/// <summary>
		/// Gets whether or not the variable contains a value which we are filtering.
		/// </summary>
		/// <param name="variable">Variable which we are checking.</param>
		/// <param name="allowStrings">Flag indicating whether or not we are allowing strings to show up</param>
		/// <returns>true if the variable contains a value which is filtered.</returns>
		public bool FilterValue(Variable variable, bool allowStrings)
		{
			for (int i = 0; i < variable.NumberOfValues; i++)
			{
				switch (variable.DataType)
				{
					case VariableDataType.Integer:
						if (variable.GetInt32(i) != 0)
							return false;
						break;

					case VariableDataType.Float:
						if (variable.GetSingle(i) != 0.0)
							return false;
						break;

					case VariableDataType.StringVar:
						if ((
								allowStrings
								|| variable.Name.ToUpperInvariant().Equals("CHARACTERBASEATTACKSPEEDTAG")
								|| variable.Name.ToUpperInvariant().Equals("ITEMSKILLNAME") // Added by VillageIdiot for Granted skills
								|| variable.Name.ToUpperInvariant().Equals("SKILLNAME") // Added by VillageIdiot for scroll skills
								|| variable.Name.ToUpperInvariant().Equals("PETBONUSNAME") // Added by VillageIdiot for pet bonuses
								|| ItemAttributeProvider.IsReagent(variable.Name)
							) && variable.GetString(i).Length > 0
						)
						{
							return false;
						}
						break;

					case VariableDataType.Boolean:
						if (variable.GetInt32(i) != 0)
							return false;
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
		private string CheckExtension(string itemId)
		{
			if (itemId == null)
				return null;

			if (itemId.Length < 4)
				return itemId;

			if (Path.GetExtension(itemId).ToUpperInvariant().Equals(".DBR"))
				return itemId;
			else
				return string.Concat(itemId, ".dbr");
		}



		/// <summary>
		/// Gets the item requirements from the database record
		/// </summary>
		/// <param name="requirements">SortedList of requirements</param>
		/// <param name="record">database record</param>
		private void GetRequirementsFromRecord(SortedList<string, Variable> requirements, DBRecordCollection record)
		{
			if (TQDebug.ItemDebugLevel > 0)
				Log.LogDebug("Item.GetDynamicRequirementsFromRecord({0}, {1})", requirements, record);

			if (record == null)
			{
				if (TQDebug.ItemDebugLevel > 0)
					Log.LogDebug("Error - record was null.");

				return;
			}

			if (TQDebug.ItemDebugLevel > 1)
				Log.LogDebug(record.Id);

			foreach (Variable variable in record)
			{
				if (TQDebug.ItemDebugLevel > 2)
					Log.LogDebug(variable.Name);

				if (FilterValue(variable, false))
					continue;

				if (!FilterRequirements(variable.Name))
					continue;

				string value = variable.ToStringValue();
				string key = variable.Name.Replace("Requirement", string.Empty);

				// Upper-case the first char of key
				key = string.Concat(key.Substring(0, 1).ToUpper(System.Globalization.CultureInfo.InvariantCulture), key.Substring(1));

				// Level needs to be LevelRequirement bah
				if (key.Equals("Level"))
					key = "LevelRequirement";

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
							continue;

						requirements.Remove(key);
					}
					else
					{
						if (variable.GetInt32(0) <= oldVariable.GetInt32(0))
							continue;

						requirements.Remove(key);
					}
				}

				if (TQDebug.ItemDebugLevel > 1)
					Log.LogDebug("Added Requirement {0}={1}", key, value);

				requirements.Add(key, variable);
			}

			if (TQDebug.ItemDebugLevel > 0)
				Log.LogDebug("Exiting Item.GetDynamicRequirementsFromRecord()");
		}



		#endregion Item Private Methods

		/// <summary>
		/// Gets the dynamic requirements from a database record.
		/// </summary>
		/// <param name="requirements">SortedList of requirements</param>
		/// <param name="itemInfo">ItemInfo for the item</param>
		private void GetDynamicRequirementsFromRecord(Item itm, SortedList<string, Variable> requirements, Info itemInfo)
		{
			if (TQDebug.ItemDebugLevel > 0)
				Log.LogDebug("Item.GetDynamicRequirementsFromRecord({0}, {1})", requirements, itemInfo);

			DBRecordCollection record = Database.GetRecordFromFile(itemInfo.ItemId);
			if (record == null)
				return;

			string itemLevelTag = "itemLevel";
			Variable lvl = record[itemLevelTag];
			if (lvl == null)
				return;

			string itemLevel = lvl.ToStringValue();
			string itemCostID = itemInfo.GetString("itemCostName");

			record = Database.GetRecordFromFile(itemCostID);
			if (record == null)
			{
				record = Database.GetRecordFromFile("records/game/itemcost.dbr");
				if (record == null)
					return;
			}

			if (TQDebug.ItemDebugLevel > 1)
				Log.LogDebug(record.Id);

			string prefix = GetRequirementEquationPrefix(itemInfo.ItemClass);
			foreach (Variable variable in record)
			{
				if (string.Compare(variable.Name, 0, prefix, 0, prefix.Length, StringComparison.OrdinalIgnoreCase) != 0)
					continue;

				if (TQDebug.ItemDebugLevel > 2)
					Log.LogDebug(variable.Name);

				if (FilterValue(variable, true))
					// our equation is a string, so we want also strings
					return;

				string key = variable.Name.Replace(prefix, string.Empty);
				key = key.Replace("Equation", string.Empty);
				key = key.Replace(key[0], char.ToUpperInvariant(key[0]));

				// We need to ignore the cost equations.
				// Shields have costs so they will cause an overflow.
				if (key.Equals("Cost"))
					continue;

				var variableKey = key.ToLowerInvariant();
				if (variableKey == "level" || variableKey == "strength" || variableKey == "dexterity" || variableKey == "intelligence")
					variableKey += "Requirement";

				// Level needs to be LevelRequirement bah
				if (key.Equals("Level"))
					key = "LevelRequirement";

				string value = variable.ToStringValue().Replace(itemLevelTag, itemLevel);

				// Added by VillageIdiot
				// Changed to reflect Total Attribut count
				value = value.Replace("totalAttCount", Convert.ToString(itm.attributeCount, CultureInfo.InvariantCulture));

				// Changed by Bman to fix random overflow crashes
				Variable ans = new Variable(variableKey, VariableDataType.Integer, 1);

				// Changed by VillageIdiot to fix random overflow crashes.
				double tempVal = Math.Ceiling(value.Eval<double>());

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
						return;

					requirements.Remove(key);
				}

				if (TQDebug.ItemDebugLevel > 2)
					Log.LogDebug("Added Requirement {0}={1}", key, value);

				requirements.Add(key, ans);
			}

			if (TQDebug.ItemDebugLevel > 0)
				Log.LogDebug("Exiting Item.GetDynamicRequirementsFromRecord()");
		}



		/// <summary>
		/// Gets the level of a triggered skill
		/// </summary>
		/// <param name="record">DBRecord for the triggered skill</param>
		/// <param name="recordId">string containing the record id</param>
		/// <param name="varNum">variable number which we are looking up since there can be multiple values</param>
		/// <returns>int containing the skill level</returns>
		public int GetTriggeredSkillLevel(Item itm, DBRecordCollection record, string recordId, int varNum)
		{
			DBRecordCollection baseItem = Database.GetRecordFromFile(itm.baseItemInfo.ItemId);

			// Check to see if it's a Buff Skill
			if (baseItem.GetString("itemSkillAutoController", 0) != null)
			{
				int level = baseItem.GetInt32("itemSkillLevel", 0);
				if (record.GetString("Class", 0).ToUpperInvariant().StartsWith("SKILLBUFF", StringComparison.OrdinalIgnoreCase))
				{
					DBRecordCollection skill = Database.GetRecordFromFile(itm.baseItemInfo.GetString("itemSkillName"));
					if (skill != null && skill.GetString("buffSkillName", 0) == recordId)
						// Use the level from the Base Item.
						varNum = Math.Max(level, 1) - 1;
				}
				else if (baseItem.GetString("itemSkillName", 0) == recordId)
					varNum = Math.Max(level, 1) - 1;
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
		public int GetPetSkillLevel(Item itm, DBRecordCollection record, string recordId, int varNum)
		{
			// Check to see if itm really is a skill
			if (record.GetString("Class", 0).ToUpperInvariant().StartsWith("SKILL_ATTACK", StringComparison.OrdinalIgnoreCase))
			{
				// Check to see if itm item creates a pet
				DBRecordCollection petSkill = Database.GetRecordFromFile(itm.baseItemInfo.GetString("skillName"));
				string petID = petSkill.GetString("spawnObjects", 0);
				if (!string.IsNullOrEmpty(petID))
				{
					DBRecordCollection petRecord = Database.GetRecordFromFile(petID);
					int foundSkillOffset = 0;
					for (int skillOffset = 0; skillOffset < 17; skillOffset++)
					{
						// There are upto 17 skills
						// Find the skill in the skill tree so that we can get the level
						if (petRecord.GetString(string.Concat("skillName", skillOffset), 0) == recordId)
							break;

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
		public string Format(string formatSpec, object parameter1, object parameter2 = null, object parameter3 = null)
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

				Log.LogDebug(error);

				return error;
			}
		}

		/// <summary>
		/// Gets the item name and attributes.
		/// </summary>
		/// <param name="itm"></param>
		/// <param name="scopes">Extra data scopes as a bitmask</param>
		/// <param name="filterExtra">filter extra properties</param>
		/// <returns>An object containing the item name and attributes</returns>
		public ToFriendlyNameResult GetFriendlyNames(Item itm, FriendlyNamesExtraScopes? scopes = null, bool filterExtra = true)
		{
			var key = (itm, scopes, filterExtra);
			return FriendlyNamesCache.GetOrAddAtomic(key, k =>
			{

				var res = new ToFriendlyNameResult(k.Item);
				k.Item.CurrentFriendlyNameResult = res;

				#region Minimal Info (ButtonBag tooltip + Common item properties)

				// Item Seed
				res.ItemSeed = string.Format(CultureInfo.CurrentCulture, this.TranslationService.ItemSeed, k.Item.Seed, (k.Item.Seed != 0) ? (k.Item.Seed / (float)Int16.MaxValue) : 0.0f);
				res.ItemQuest = this.TranslationService.ItemQuest;

				#region Prefix translation

				if (!k.Item.IsRelic && !string.IsNullOrEmpty(k.Item.prefixID))
				{
					res.PrefixInfoDescription = k.Item.prefixID;
					if (k.Item.prefixInfo != null)
					{
						if (TranslationService.TryTranslateXTag(k.Item.prefixInfo.DescriptionTag, out var desc))
							res.PrefixInfoDescription = desc;
					}
				}

				#endregion

				#region Base Item translation

				// Load common relic translations if item is relic related by any means
				if (k.Item.IsRelic || k.Item.HasRelicSlot1 || k.Item.HasRelicSlot2 || k.Item.RelicInfo != null || k.Item.Relic2Info != null)
				{
					res.ItemWith = this.TranslationService.ItemWith;

					if (k.Item.RelicInfo != null)
						TranslationService.TryTranslateXTag(k.Item.RelicInfo.DescriptionTag, out res.RelicInfo1Description);

					if (k.Item.Relic2Info != null)
						TranslationService.TryTranslateXTag(k.Item.Relic2Info.DescriptionTag, out res.RelicInfo2Description);

					var labelCompleted = "Completed";
					if (!TranslationService.TryTranslateXTag("tagAnimalPartComplete", out res.AnimalPartComplete))
						res.AnimalPartComplete = labelCompleted;

					if (!TranslationService.TryTranslateXTag("tagRelicComplete", out res.RelicComplete))
						res.RelicComplete = labelCompleted;

					var labelPartcomplete = "Completion Bonus: ";
					if (!TranslationService.TryTranslateXTag("tagAnimalPartcompleteBonus", out res.AnimalPartCompleteBonus))
						res.AnimalPartCompleteBonus = labelPartcomplete;

					if (!TranslationService.TryTranslateXTag("tagRelicBonus", out res.RelicBonus))
						res.RelicBonus = labelPartcomplete;

					var labelRelic = "Relic";
					if (!TranslationService.TryTranslateXTag("tagAnimalPart", out res.AnimalPart))
						res.AnimalPart = labelRelic;

					if (!TranslationService.TryTranslateXTag("tagRelicShard", out res.RelicShard))
						res.RelicShard = labelRelic;

					var labelRelicPattern = "{0} - {1} / {2}";
					if (TranslationService.TryTranslateXTag("tagAnimalPartRatio", out res.AnimalPartRatio))
						res.AnimalPartRatio = ItemAttributeProvider.ConvertFormat(res.AnimalPartRatio);
					else
						res.AnimalPartRatio = labelRelicPattern;

					if (TranslationService.TryTranslateXTag("tagRelicRatio", out res.RelicRatio))
						res.RelicRatio = ItemAttributeProvider.ConvertFormat(res.RelicRatio);
					else
						res.RelicRatio = labelRelicPattern;
				}

				res.BaseItemId = k.Item.BaseItemId;

				// Set Rarity translation
				res.BaseItemRarity = TranslationService.Translate(k.Item.ItemStyle);

				if (k.Item.baseItemInfo != null)
				{
					// style quality description
					if (!string.IsNullOrEmpty(k.Item.baseItemInfo.StyleTag))
					{
						if (!k.Item.IsPotion && !k.Item.IsRelic && !k.Item.IsScroll && !k.Item.IsParchment && !k.Item.IsQuestItem)
						{
							if (!TranslationService.TryTranslateXTag(k.Item.baseItemInfo.StyleTag, out res.BaseItemInfoStyle))
								res.BaseItemInfoStyle = k.Item.baseItemInfo.StyleTag;
						}
					}

					if (!string.IsNullOrEmpty(k.Item.baseItemInfo.QualityTag))
					{
						if (!TranslationService.TryTranslateXTag(k.Item.baseItemInfo.QualityTag, out res.BaseItemInfoQuality))
							res.BaseItemInfoQuality = k.Item.baseItemInfo.QualityTag;
					}

					if (!TranslationService.TryTranslateXTag(k.Item.baseItemInfo.DescriptionTag, out res.BaseItemInfoDescription))
						res.BaseItemInfoDescription = k.Item.BaseItemId;

					res.BaseItemInfoClass = TranslationService.TranslateXTag(k.Item.ItemClassTagName);

					res.BaseItemInfoRecords = Database.GetRecordFromFile(k.Item.BaseItemId);

					if (k.Item.IsRelic)
					{
						// Add the number of charms in the set acquired.
						if (k.Item.IsRelicComplete)
						{
							if (k.Item.IsCharm)
							{
								res.RelicCompletionFormat = res.AnimalPartComplete;
								res.RelicBonusTitle = res.AnimalPartCompleteBonus;
							}
							else
							{
								res.RelicCompletionFormat = res.RelicComplete;
								res.RelicBonusTitle = res.RelicBonus;
							}

							if (!string.IsNullOrEmpty(k.Item.RelicBonusId))
							{
								res.RelicBonusFileName = k.Item.RelicBonusId.PrettyFileName();
								res.RelicBonusPattern = "{0} {1}";
								res.RelicBonusFormat = string.Format(CultureInfo.CurrentCulture, res.RelicBonusPattern
									, res.RelicBonusTitle
									, TQColor.Yellow.ColorTag() + res.RelicBonusFileName
								);
							}
							else
							{
								res.RelicBonusPattern = "{0}";
								res.RelicBonusFormat = string.Format(CultureInfo.CurrentCulture, res.RelicBonusPattern, res.RelicBonusTitle);
							}
						}
						else
						{
							if (k.Item.IsCharm)
							{
								res.RelicClass = res.AnimalPart;
								res.RelicPattern = res.AnimalPartRatio;
							}
							else
							{
								res.RelicClass = res.RelicShard;
								res.RelicPattern = res.RelicRatio;
							}

							res.RelicCompletionFormat = Format(res.RelicPattern, res.RelicClass, k.Item.Number, k.Item.baseItemInfo.CompletedRelicLevel);
							res.RelicBonusFormat = res.RelicCompletionFormat;
						}

					}
					else if (k.Item.IsArtifact)
					{
						// Add Artifact completion bonus
						if (!string.IsNullOrEmpty(k.Item.RelicBonusId))
						{
							var RelicBonusIdExt = Path.GetFileNameWithoutExtension(TQData.NormalizeRecordPath(k.Item.RelicBonusId));
							res.ArtifactBonus = TranslationService.TranslateXTag("xtagArtifactBonus");
							res.ArtifactBonusFormat = string.Format(CultureInfo.CurrentCulture, "({0} {1})", res.ArtifactBonus, RelicBonusIdExt);
						}

						// Show Artifact Class (Lesser / Greater / Divine).
						string artifactClassification = k.Item.baseItemInfo.GetString("artifactClassification").ToUpperInvariant();
						res.ArtifactClass = TranslateArtifactClassification(artifactClassification);

					}
					else if (k.Item.IsFormulae)
					{
						// Added to show recipe type for Formulae
						if (!TranslationService.TryTranslateXTag("xtagArtifactRecipe", out res.ArtifactRecipe))
							res.ArtifactRecipe = "Recipe";

						// Get Reagents format
						if (TranslationService.TryTranslateXTag("xtagArtifactReagents", out res.ArtifactReagents))
							res.ArtifactReagents = ItemAttributeProvider.ConvertFormat(res.ArtifactReagents);
						else
							res.ArtifactReagents = "Required Reagents  ({0}/{1})";


						// it looks like the formulae reagents is hard coded at 3
						res.FormulaeFormat = Format(res.ArtifactReagents, (object)0, 3);
						res.FormulaeFormat = $"{TQColor.Orange.ColorTag()}{res.FormulaeFormat}";

					}
					else if (k.Item.DoesStack)
					{
						// display the # potions
						if (k.Item.Number > 1)
							res.NumberFormat = string.Format(CultureInfo.CurrentCulture, "({0:n0})", k.Item.Number);
					}
				}

				#endregion

				#region Suffix translation

				if (!k.Item.IsRelic && !string.IsNullOrWhiteSpace(k.Item.suffixID))
				{
					if (k.Item.suffixInfo != null)
					{
						if (!TranslationService.TryTranslateXTag(k.Item.suffixInfo.DescriptionTag, out res.SuffixInfoDescription))
							res.SuffixInfoDescription = k.Item.suffixID;
					}
					else
						res.SuffixInfoDescription = k.Item.suffixID;
				}

				#endregion

				#region flavor text

				// Removed Scroll flavor text since it gets printed by the skill effect code
				if ((k.Item.IsPotion || k.Item.IsRelic || k.Item.IsScroll || k.Item.IsParchment || k.Item.IsQuestItem) && !string.IsNullOrWhiteSpace(k.Item.baseItemInfo?.StyleTag))
				{
					if (TranslationService.TryTranslateXTag(k.Item.baseItemInfo.StyleTag, out var flavor))
					{
						var ft = StringHelper.WrapWords(flavor, 40);
						res.FlavorText = ft.ToArray();
					}
				}

				#endregion

				#endregion

				List<string> results = new List<string>();

				if (k.Scope?.HasFlag(FriendlyNamesExtraScopes.PrefixAttributes) ?? false)
				{
					if (k.Item.prefixInfo != null)
						res.PrefixInfoRecords = Database.GetRecordFromFile(k.Item.prefixID);

					if (res.PrefixInfoRecords?.Any() ?? false)
						GetAttributesFromRecord(k.Item, res.PrefixInfoRecords, k.FilterExtra, k.Item.prefixID, results);

					res.PrefixAttributes = results.ToArray();
				}

				if (k.Scope?.HasFlag(FriendlyNamesExtraScopes.SuffixAttributes) ?? false)
				{
					results.Clear();

					if (k.Item.suffixInfo != null)
						res.SuffixInfoRecords = Database.GetRecordFromFile(k.Item.suffixID);

					if (res.SuffixInfoRecords?.Any() ?? false)
						GetAttributesFromRecord(k.Item, res.SuffixInfoRecords, k.FilterExtra, k.Item.suffixID, results);

					res.SuffixAttributes = results.ToArray();
				}

				if (k.Scope?.HasFlag(FriendlyNamesExtraScopes.BaseAttributes) ?? false)
				{
					results.Clear();

					// res.baseItemInfoRecords should be already loaded
					if (res.BaseItemInfoRecords?.Any() ?? false)
						GetAttributesFromRecord(k.Item, res.BaseItemInfoRecords, k.FilterExtra, k.Item.BaseItemId, results);
				}

				res.BaseAttributes = results.ToArray();

				if (k.Scope?.HasFlag(FriendlyNamesExtraScopes.RelicAttributes) ?? false)
				{
					var tmp = new List<string>();

					if (k.Item.RelicInfo != null)
						res.RelicInfoRecords = Database.GetRecordFromFile(k.Item.relicID);

					if (res.RelicInfoRecords?.Any() ?? false)
						GetAttributesFromRecord(k.Item, res.RelicInfoRecords, k.FilterExtra, k.Item.relicID, tmp);

					res.Relic1Attributes = tmp.ToArray();
				}

				if (k.Scope?.HasFlag(FriendlyNamesExtraScopes.Relic2Attributes) ?? false)
				{
					var tmp = new List<string>();

					if (k.Item.Relic2Info != null)
						res.Relic2InfoRecords = Database.GetRecordFromFile(k.Item.relic2ID);

					if (res.Relic2InfoRecords?.Any() ?? false)
						GetAttributesFromRecord(k.Item, res.Relic2InfoRecords, k.FilterExtra, k.Item.relic2ID, tmp);

					res.Relic2Attributes = tmp.ToArray();
				}

				if (k.Scope?.HasFlag(FriendlyNamesExtraScopes.ItemSet) ?? false)
					res.ItemSet = GetItemSetString(k.Item);

				if (k.Scope?.HasFlag(FriendlyNamesExtraScopes.Requirements) ?? false)
				{
					var reqs = GetRequirements(k.Item);
					res.Requirements = reqs.Requirements;
					res.RequirementVariables = reqs.RequirementVariables;
				}

				#region Extra Attributes for specific types

				// Shows Artifact stats for the formula
				if (k.Item.IsFormulae && k.Item.baseItemInfo != null && (k.Scope?.HasFlag(FriendlyNamesExtraScopes.BaseAttributes) ?? false))
				{
					string artifactID = k.Item.baseItemInfo.GetString("artifactName");

					if (!string.IsNullOrWhiteSpace(artifactID))
					{
						List<string> tmp = new List<string>();
						res.FormulaeArtifactRecords = Database.GetRecordFromFile(artifactID);

						// Display the name of the Artifact
						if (!TranslationService.TryTranslateXTag(res.FormulaeArtifactRecords.GetString("description", 0), out res.FormulaeArtifactName))
							res.FormulaeArtifactName = "?Unknown Artifact Name?";

						// Class
						string artifactClassification = res.FormulaeArtifactRecords.GetString("artifactClassification", 0).ToUpperInvariant();
						res.FormulaeArtifactClass = TranslateArtifactClassification(artifactClassification);

						// Attributes
						GetAttributesFromRecord(k.Item, res.FormulaeArtifactRecords, true, artifactID, tmp);
						res.FormulaeArtifactAttributes = tmp.ToArray();
					}
				}

				// Show the completion bonus. // TODO is it possible to have 2 relics on one artifact ?
				if (k.Item.RelicBonusInfo != null
					&& (k.Scope?.HasFlag(FriendlyNamesExtraScopes.RelicAttributes) ?? false)
					&& (k.Item.IsArtifact // Artifact completion bonus
						|| k.Item.IsRelic // Relic completion bonus
						|| k.Item.HasRelicSlot1
					)
				)
				{
					var tmp = new List<string>();

					res.RelicBonus1InfoRecords = Database.GetRecordFromFile(k.Item.RelicBonusId);

					if (res.RelicBonus1InfoRecords?.Any() ?? false)
						GetAttributesFromRecord(k.Item, res.RelicBonus1InfoRecords, k.FilterExtra, k.Item.RelicBonusId, tmp);

					res.RelicBonus1Attributes = tmp.ToArray();
				}

				// Show the Relic2 completion bonus.
				if (k.Item.HasRelicSlot2 && k.Item.RelicBonus2Info != null && (k.Scope?.HasFlag(FriendlyNamesExtraScopes.Relic2Attributes) ?? false))
				{
					var tmp = new List<string>();
					res.RelicBonus2InfoRecords = Database.GetRecordFromFile(k.Item.RelicBonus2Id);

					if (res.RelicBonus2InfoRecords?.Any() ?? false)
						GetAttributesFromRecord(k.Item, res.RelicBonus2InfoRecords, k.FilterExtra, k.Item.RelicBonus2Id, tmp);

					res.RelicBonus2Attributes = tmp.ToArray();
				}

				#endregion

				k.Item.CurrentFriendlyNameResult.TmpAttrib.Clear();
				k.Item.CurrentFriendlyNameResult = null;
				return res;

			});

			#region Local Helper

			string TranslateArtifactClassification(string artifactClassificationKey)
			{
				string tag = string.Empty;
				string resartifactClass = string.Empty;
				if (artifactClassificationKey == null)
					tag = null;
				else if (artifactClassificationKey == "LESSER")
					tag = "xtagArtifactClass01";
				else if (artifactClassificationKey == "GREATER")
					tag = "xtagArtifactClass02";
				else if (artifactClassificationKey == "DIVINE")
					tag = "xtagArtifactClass03";
				else
					tag = null;

				if (tag != null)
					resartifactClass = TranslationService.TranslateXTag(tag);
				else
					resartifactClass = "Unknown Artifact Class";

				return string.IsNullOrWhiteSpace(resartifactClass) ? "Unknown Artifact Class" : resartifactClass;
			}

			#endregion
		}

		/// <summary>
		/// Shows the items in a set for the set items
		/// </summary>
		/// <returns>string containing the set items</returns>
		public string[] GetItemSetString(Item itm)
		{
			List<string> results = new List<string>();
			string[] setMembers = this.GetSetItems(itm, true);
			if (setMembers != null)
			{
				var isfirst = true;
				foreach (string memb in setMembers)
				{
					string name = string.Empty;

					// Changed by VillageIdiot
					// The first entry is now the set name
					if (isfirst)
					{
						name = TranslationService.TranslateXTag(memb);
						results.Add($"{ItemStyle.Rare.TQColor().ColorTag()}{name}");
						isfirst = false;
					}
					else
					{
						name = "?? Missing database info ??";
						Info info = Database.GetInfo(memb);

						if (info != null)
							name = TranslationService.TranslateXTag(info.DescriptionTag);

						results.Add($"{ItemStyle.Common.TQColor().ColorTag()}    {name}");
					}
				}
			}

			return results.ToArray();
		}

		/// <summary>
		/// Gets the item's attributes from the database record.
		/// </summary>
		/// <remarks>
		/// Changed by VillageIdiot
		/// Added option to NOT translate the attributes to strings
		/// </remarks>
		/// <param name="record">DBRecord for the database record</param>
		/// <param name="filtering">whether or not we are filtering strings</param>
		/// <param name="recordId">string containing the database record id</param>
		/// <param name="results">List for the results</param>
		/// <param name="convertStrings">flag on whether we convert attributes to strings.</param>
		private void GetAttributesFromRecord(Item itm, DBRecordCollection record, bool filtering, string recordId, List<string> results, bool convertStrings = true)
		{
			if (TQDebug.ItemDebugLevel > 0)
			{
				Log.LogDebug("Item.GetAttributesFromRecord({0}, {1}, {2}, {3}, {4})"
					, record, filtering, recordId, results, convertStrings
				);
			}

			// First get a list of attributes, grouped by effect.
			Dictionary<string, List<Variable>> attrByEffect = new Dictionary<string, List<Variable>>();
			if (record == null)
			{
				if (TQDebug.ItemDebugLevel > 0)
					Log.LogDebug("Error - record was null.");

				results.Add("<unknown>");
				return;
			}

			if (TQDebug.ItemDebugLevel > 1)
				Log.LogDebug(record.Id);

			// Added by Village Idiot
			// To keep track of groups so they are not counted twice
			List<string> countedGroups = new List<string>();

			foreach (Variable variable in record)
			{
				if (TQDebug.ItemDebugLevel > 2)
					Log.LogDebug(variable.Name);


				if (this.FilterValue(variable, !filtering))
					continue;

				if (filtering && this.FilterKey(variable.Name))
					continue;

				if (filtering && this.FilterRequirements(variable.Name))
					continue;

				ItemAttributesData data = ItemAttributeProvider.GetAttributeData(variable.Name);
				if (data == null)
				{
					// unknown attribute
					if (TQDebug.ItemDebugLevel > 2)
						Log.LogDebug("Unknown Attribute");

					data = new ItemAttributesData(ItemAttributesEffectType.Other, variable.Name, variable.Name, string.Empty, 0);
				}

				string effectGroup;

				// Changed by VillageIdiot to group DamageQualifiers together.
				if (data.EffectType == ItemAttributesEffectType.DamageQualifierEffect)
					effectGroup = string.Concat(data.EffectType.ToString(), ":", "DamageQualifier");
				else
					effectGroup = string.Concat(data.EffectType.ToString(), ":", data.Effect);

				// Find or create the attrList for itm effect
				List<Variable> attrList;
				if (!attrByEffect.TryGetValue(effectGroup, out attrList))
				{
					attrList = new List<Variable>();
					attrByEffect[effectGroup] = attrList;
				}

				// Add itm guy to the attrList
				attrList.Add(variable);

				// Added by VillageIdiot
				// Set number of attributes parameter for level calculation
				// Filter relics and relic bonuses
				if (recordId != itm.relic2ID && recordId != itm.RelicBonus2Id && recordId != itm.relicID && recordId != itm.RelicBonusId && !itm.isAttributeCounted)
				{
					// Added test to see if itm has already been done
					if (!countedGroups.Contains(effectGroup))
					{
						string normalizedVariableName = variable.Name.ToUpperInvariant();
						if (!normalizedVariableName.Contains("CHANCE") && !normalizedVariableName.Contains("DURATION"))
						{
							// Filter Attribute chance and duration tags
							// Filter base attributes
							if (normalizedVariableName != "CHARACTERBASEATTACKSPEEDTAG" &&
								normalizedVariableName != "OFFENSIVEPHYSICALMIN" &&
								normalizedVariableName != "OFFENSIVEPHYSICALMAX" &&
								normalizedVariableName != "DEFENSIVEPROTECTION" &&
								normalizedVariableName != "DEFENSIVEBLOCK" &&
								normalizedVariableName != "BLOCKRECOVERYTIME" &&
								normalizedVariableName != "OFFENSIVEGLOBALCHANCE" &&
								normalizedVariableName != "RETALIATIONGLOBALCHANCE" &&
								normalizedVariableName != "OFFENSIVEPIERCERATIOMIN")
							{
								// Chance of effects are still messed up.
								if (normalizedVariableName.StartsWith("AUGMENTSKILLLEVEL", StringComparison.OrdinalIgnoreCase))
								{
									// Add value of augment skill level to count instead of incrementing
									itm.attributeCount += variable.GetInt32(0);
									countedGroups.Add(effectGroup);
								}
								else
								{
									++itm.attributeCount;
									countedGroups.Add(effectGroup);
								}
							}
						}
					}
				}
			}

			// Added by VillageIdiot
			// Some attributes have been counted so set the flag so we do not count them again
			if (itm.attributeCount != 0)
				itm.isAttributeCounted = true;

			// Now we have all our attributes grouped by effect.  Now lets sort them
			List<Variable>[] attrArray = new List<Variable>[attrByEffect.Count];
			attrByEffect.Values.CopyTo(attrArray, 0);
			Array.Sort(attrArray, new ItemAttributeListCompare(itm.IsArmor || itm.IsShield, this.ItemAttributeProvider));

			// Now for the global params, we need to check to see if they are XOR or all.
			// We do itm by checking the effect just after the global param.
			for (int i = 0; i < attrArray.Length; ++i)
			{
				List<Variable> attrList = attrArray[i];

				if (ItemAttributeProvider.AttributeGroupIs(new Collection<Variable>(attrList), "offensiveGlobalChance") ||
					ItemAttributeProvider.AttributeGroupIs(new Collection<Variable>(attrList), "retaliationGlobalChance"))
				{
					// check the next effect group
					int j = i + 1;
					if (j < attrArray.Length)
					{
						List<Variable> next = attrArray[j];
						if (!ItemAttributeProvider.AttributeGroupHas(new Collection<Variable>(next), "Global"))
						{
							// itm is a spurious globalChance entry.  Let's add 2 null entries to signal it should be ignored
							attrList.Add(null);
							attrList.Add(null);
						}
						else if (ItemAttributeProvider.AttributeGroupHas(new Collection<Variable>(next), "XOR"))
						{
							// Yes it is global and is also XOR
							// flag our current attribute as XOR
							// We do itm by adding a second NULL entry to the list.  Its a hack but it works
							attrList.Add(null);
						}
					}
					else
					{
						// itm is a spurious globalChance entry.  Let's add 2 null entries to signal it should be ignored
						attrList.Add(null);
						attrList.Add(null);
					}
				}
			}

			foreach (List<Variable> attrList in attrArray)
			{
				// Used to sort out the Damage Qualifier effects.
				if (ItemAttributeProvider.AttributeGroupIs(new Collection<Variable>(attrList), ItemAttributesEffectType.DamageQualifierEffect))
					attrList.Sort(new ItemAttributeSubListCompare(this.ItemAttributeProvider));

				if (!convertStrings)
					ConvertBareAttributeListToString(attrList, results);
				else
					ConvertAttributeListToString(itm, record, attrList, recordId, results);
			}

			if (TQDebug.ItemDebugLevel > 0)
				Log.LogDebug("Exiting Item.GetAttributesFromRecord()");
		}

		/// <summary>
		/// Converts the item's attribute list to a string
		/// </summary>
		/// <param name="record">DBRecord for the item</param>
		/// <param name="attributeList">ArrayList containing the attributes list</param>
		/// <param name="recordId">string containing the record id</param>
		/// <param name="results">List containing the results</param>
		private void ConvertAttributeListToString(Item itm, DBRecordCollection record, List<Variable> attributeList, string recordId, List<string> results)
		{
			if (TQDebug.ItemDebugLevel > 0)
			{
				Log.LogDebug("Item.ConvertAttrListToString ({0}, {1}, {2}, {3})"
					, record, attributeList, recordId, results
				);
			}

			// see what kind of effects are in this list
			Variable variable = (Variable)attributeList[0];
			ItemAttributesData data = ItemAttributeProvider.GetAttributeData(variable.Name);
			if (data == null)
			{
				// unknown attribute
				if (TQDebug.ItemDebugLevel > 0)
					Log.LogDebug("Error - Unknown Attribute.");

				data = new ItemAttributesData(ItemAttributesEffectType.Other, variable.Name, variable.Name, string.Empty, 0);
			}

			if (TQDebug.ItemDebugLevel > 0)
				Log.LogDebug("Exiting Item.ConvertAttrListToString ()");

			ConvertOffenseAttributesToString(itm, record, attributeList, data, recordId, results);
			return;
		}

		/// <summary>
		/// Gets a formatted range amount
		/// </summary>
		/// <param name="data">ItemAttributesData data</param>
		/// <param name="varNum">variable number to look up</param>
		/// <param name="minVar">minVar variable</param>
		/// <param name="maxVar">maxVar variable</param>
		/// <param name="label">label string</param>
		/// <param name="labelColor">label color</param>
		/// <returns>formatted range string</returns>
		private string GetAmountRange(Item itm, ItemAttributesData data, int varNum, Variable minVar, Variable maxVar, ref string label, TQColor? labelColor, Variable minDurVar = null)
		{
			// Added by VillageIdiot : check to see if min and max are the same
			TQColor? color = null;
			string amount = null;

			Variable min = null;
			Variable max = null;

			if (minVar != null)
				min = minVar.Clone();

			if (maxVar != null)
				max = maxVar.Clone();

			// sweet we have a range
			string tag = "DamageRangeFormat";
			if (data.Effect.EndsWith("Stun", StringComparison.OrdinalIgnoreCase)
				|| data.Effect.EndsWith("Freeze", StringComparison.OrdinalIgnoreCase)
				|| data.Effect.EndsWith("Petrify", StringComparison.OrdinalIgnoreCase)
				|| data.Effect.EndsWith("Trap", StringComparison.OrdinalIgnoreCase)
				|| data.Effect.EndsWith("Convert", StringComparison.OrdinalIgnoreCase)
				|| data.Effect.EndsWith("Fear", StringComparison.OrdinalIgnoreCase)
				|| data.Effect.EndsWith("Confusion", StringComparison.OrdinalIgnoreCase)
				|| data.Effect.EndsWith("Disruption", StringComparison.OrdinalIgnoreCase)
			)
			{
				tag = "DamageInfluenceRangeFormat";
			}
			else if (data.Effect.Equals("defensiveBlock"))
				tag = "DefenseBlock";

			if (!TranslationService.TryTranslateXTag(tag, out var formatSpec))
			{
				formatSpec = "{0}..{1}";
				color = ItemStyle.Legendary.TQColor();
			}
			else
			{
				if (TQDebug.ItemDebugLevel > 2)
					Log.LogDebug("Item.formatspec (range) = " + formatSpec);

				formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
			}

			if (label.IndexOf('{') >= 0)
			{
				// the label has formatting codes.  Use it to format the amount
				formatSpec = label;
				label = null;
				color = labelColor;
			}

			// Added by VillageIdiot
			// Adjust for itemScalePercent
			if (minDurVar != null)
			{
				min[Math.Min(min.NumberOfValues - 1, varNum)] = (float)min[Math.Min(min.NumberOfValues - 1, varNum)] * (float)minDurVar[minDurVar.NumberOfValues - 1] * itm.itemScalePercent;
				max[Math.Min(max.NumberOfValues - 1, varNum)] = (float)max[Math.Min(max.NumberOfValues - 1, varNum)] * (float)minDurVar[minDurVar.NumberOfValues - 1] * itm.itemScalePercent;
			}
			else
			{
				min[Math.Min(min.NumberOfValues - 1, varNum)] = (float)min[Math.Min(min.NumberOfValues - 1, varNum)] * itm.itemScalePercent;
				max[Math.Min(max.NumberOfValues - 1, varNum)] = (float)max[Math.Min(max.NumberOfValues - 1, varNum)] * itm.itemScalePercent;
			}

			amount = this.Format(formatSpec, min[Math.Min(min.NumberOfValues - 1, varNum)], max[Math.Min(max.NumberOfValues - 1, varNum)]);
			return color.HasValue ? $"{color?.ColorTag()}{amount}" : amount;
		}

		/// <summary>
		/// Gets a formatted single amount
		/// </summary>
		/// <param name="data">ItemAttributesData data</param>
		/// <param name="varNum">variable number to look up</param>
		/// <param name="minVar">minVar variable</param>
		/// <param name="maxVar">maxVar variable</param>
		/// <param name="label">label string</param>
		/// <param name="labelColor">label color</param>
		/// <param name="minDurVar">Duration of Damage</param>
		/// <returns>formatted single amount string</returns>
		private string GetAmountSingle(Item itm, ItemAttributesData data, int varNum, Variable minVar, Variable maxVar, ref string label, TQColor? labelColor, Variable minDurVar = null)
		{
			TQColor? color = null;
			string amount = null;

			string tag = "DamageSingleFormat";
			if (data.Effect.EndsWith("Stun", StringComparison.OrdinalIgnoreCase)
				|| data.Effect.EndsWith("Freeze", StringComparison.OrdinalIgnoreCase)
				|| data.Effect.EndsWith("Petrify", StringComparison.OrdinalIgnoreCase)
				|| data.Effect.EndsWith("Trap", StringComparison.OrdinalIgnoreCase)
				|| data.Effect.EndsWith("Convert", StringComparison.OrdinalIgnoreCase)
				|| data.Effect.EndsWith("Fear", StringComparison.OrdinalIgnoreCase)
				|| data.Effect.EndsWith("Confusion", StringComparison.OrdinalIgnoreCase)
				|| data.Effect.EndsWith("Disruption", StringComparison.OrdinalIgnoreCase)
			)
			{
				tag = "DamageInfluenceSingleFormat";
			}

			if (!TranslationService.TryTranslateXTag(tag, out var formatSpec))
			{
				formatSpec = "{0}";
				color = ItemStyle.Legendary.TQColor();
			}
			else
			{
				if (TQDebug.ItemDebugLevel > 2)
					Log.LogDebug("Item.formatspec (single) = " + formatSpec);

				formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
			}

			if (label.IndexOf('{') >= 0)
			{
				// the label has formatting codes.  Use it to format the amount
				formatSpec = label;
				label = null;
				color = labelColor;
			}

			Variable currentVariable = null;

			if (minVar != null)
				currentVariable = minVar.Clone();
			else if (maxVar != null)
				currentVariable = maxVar.Clone();

			if (currentVariable != null)
			{
				// Adjust for itemScalePercent
				// only for floats
				var curvar = currentVariable[Math.Min(currentVariable.NumberOfValues - 1, varNum)];
				if (currentVariable.DataType == VariableDataType.Float)
				{
					if (minDurVar != null)
					{
						curvar = (float)curvar * (float)minDurVar[minDurVar.NumberOfValues - 1] * itm.itemScalePercent;
					}
					else
					{
						curvar = (float)curvar * itm.itemScalePercent;
					}
					currentVariable[Math.Min(currentVariable.NumberOfValues - 1, varNum)] = curvar;

					// Fix#246, double signed result on negative value Ex : string.Format("{0:+#0} d'intelligence", -10) by removing format sign.
					// Fix "Dotted decimal mask" matching Ex : {0:#0.0} Health Regeneration per second
					formatSpec = Regex.Replace(formatSpec
						, @"(?<Prefix>\{(\d):)(?<Sign>[+-])(?<Suffix>#([\d\.]+)})"
						, new MatchEvaluator((Match m) =>
						{
							var Prefix = m.Groups["Prefix"].Value;
							var Sign = m.Groups["Sign"].Value;
							var Suffix = m.Groups["Suffix"].Value;
							var val = (float)curvar;

							if ((Sign == "+" && val < 0) || (Sign == "-" && val >= 0))
								return $"{Prefix}{Suffix}";

							return m.Value;
						})
					);
				}

				amount = this.Format(formatSpec, curvar);
			}

			return color.HasValue ? $"{color?.ColorTag()}{amount}" : amount;
		}

		/// <summary>
		/// Gets the formatted duration range values
		/// </summary>
		/// <param name="varNum">offset number of the variable value that we are using</param>
		/// <param name="minDurVar">minimum duration variable</param>
		/// <param name="maxDurVar">maximum duration variable</param>
		/// <returns>formatted duration string</returns>
		private string GetDurationRange(int varNum, Variable minDurVar, Variable maxDurVar)
		{
			string duration = null;
			TQColor? color = null;

			if (!TranslationService.TryTranslateXTag("DamageRangeFormatTime", out var formatSpec))
			{
				formatSpec = "for {0}..{1} seconds";
				color = ItemStyle.Legendary.TQColor();
			}
			else
			{
				if (TQDebug.ItemDebugLevel > 2)
					Log.LogDebug("Item.formatspec (time range) = " + formatSpec);

				formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
			}

			duration = this.Format(formatSpec, minDurVar[Math.Min(minDurVar.NumberOfValues - 1, varNum)], maxDurVar[Math.Min(maxDurVar.NumberOfValues - 1, varNum)]);

			return color.HasValue ? $"{color?.ColorTag()}{duration}" : duration;
		}

		/// <summary>
		/// Gets the formatted duration single value
		/// </summary>
		/// <param name="varNum">offset number of the variable value that we are using</param>
		/// <param name="minDurVar">minimum duration variable</param>
		/// <param name="maxDurVar">maximum duration variable</param>
		/// <returns>formatted duration string</returns>
		private string GetDurationSingle(int varNum, Variable minDurVar, Variable maxDurVar)
		{
			string duration = null;
			TQColor? color = null;

			if (!TranslationService.TryTranslateXTag("DamageSingleFormatTime", out var formatSpec))
			{
				formatSpec = "{0}";
				color = ItemStyle.Legendary.TQColor();
			}
			else
			{
				if (TQDebug.ItemDebugLevel > 2)
					Log.LogDebug("Item.formatspec (time single) = " + formatSpec);

				formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
			}

			Variable durationVariable = minDurVar;
			if (durationVariable == null)
				durationVariable = maxDurVar;

			if (durationVariable != null)
			{
				duration = this.Format(formatSpec, durationVariable[Math.Min(durationVariable.NumberOfValues - 1, varNum)]);
				duration = $"{color?.ColorTag()}{duration}";
			}

			return duration;
		}

		/// <summary>
		/// Gets the formatted damage ratio string
		/// </summary>
		/// <param name="varNum">offset number of the variable value that we are using</param>
		/// <param name="damageRatioData">ItemAttributesData for the damage ratio</param>
		/// <param name="damageRatioVar">Damage Ratio variable</param>
		/// <returns>formatted damage ratio string</returns>
		private string GetDamageRatio(int varNum, ItemAttributesData damageRatioData, Variable damageRatioVar)
		{
			string damageRatio = null;
			TQColor? color = null;
			string formatSpec = null;

			string tag = string.Concat("Damage", damageRatioData.FullAttribute.Substring(9, damageRatioData.FullAttribute.Length - 20), "Ratio");

			if (!TranslationService.TryTranslateXTag(tag, out formatSpec))
			{
				formatSpec = string.Concat("{0:f1}% ?", damageRatioData.FullAttribute, "?");
				color = ItemStyle.Legendary.TQColor();
			}
			else
			{
				if (TQDebug.ItemDebugLevel > 2)
					Log.LogDebug("Item.formatspec (percent) = " + formatSpec);

				formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
			}

			damageRatio = this.Format(formatSpec, damageRatioVar[Math.Min(damageRatioVar.NumberOfValues - 1, varNum)]);

			return $"{color?.ColorTag()}{damageRatio}";
		}

		/// <summary>
		/// Gets formatted chance string
		/// </summary>
		/// <param name="varNum">offset number of the variable value that we are using</param>
		/// <param name="chanceVar">chance variable</param>
		/// <returns>formatted chance string.</returns>
		private string GetChance(int varNum, Variable chanceVar)
		{
			string chance = null;
			TQColor? color = null;

			if (!TranslationService.TryTranslateXTag("ChanceOfTag", out var formatSpec))
			{
				formatSpec = "?{%.1f0}% Chance of?";
				color = ItemStyle.Legendary.TQColor();
			}

			if (TQDebug.ItemDebugLevel > 2)
				Log.LogDebug("Item.formatspec (chance) = " + formatSpec);

			formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
			if (chanceVar != null)
			{
				chance = this.Format(formatSpec, chanceVar[Math.Min(chanceVar.NumberOfValues - 1, varNum)]);
				chance = $"{color?.ColorTag()}{chance}";
			}

			return chance;
		}


		/// <summary>
		/// Gets formatted modifier string
		/// </summary>
		/// <param name="data">ItemAttributesData for the attribute</param>
		/// <param name="varNum">offset number of the variable value that we are using</param>
		/// <param name="modifierData">ItemAttributesData for the modifier</param>
		/// <param name="modifierVar">modifier variable</param>
		/// <returns>formatted modifier string</returns>
		private string GetModifier(ItemAttributesData data, int varNum, ItemAttributesData modifierData, Variable modifierVar)
		{
			string modifier = null;
			TQColor? color = null;
			string formatSpec = null;

			string tag = ItemAttributeProvider.GetAttributeTextTag(data);
			if (string.IsNullOrEmpty(tag))
			{
				formatSpec = string.Concat("{0:f1}% ?", modifierData.FullAttribute, "?");
				color = ItemStyle.Legendary.TQColor();
			}
			else
			{
				if (!TranslationService.TryTranslateXTag(tag, out formatSpec))
				{
					formatSpec = string.Concat("{0:f1}% ?", modifierData.FullAttribute, "?");
					color = ItemStyle.Legendary.TQColor();
				}
				else
				{
					if (TQDebug.ItemDebugLevel > 2)
						Log.LogDebug("Item.formatspec (percent) = " + formatSpec);

					formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
				}

			}

			modifier = Format(formatSpec, modifierVar[Math.Min(modifierVar.NumberOfValues - 1, varNum)]);

			return $"{color?.ColorTag()}{modifier}";
		}

		/// <summary>
		/// Gets formatted duration modifier string
		/// </summary>
		/// <param name="varNum">offset number of the variable value that we are using</param>
		/// <param name="durationModifierVar">duration modifier variable</param>
		/// <returns>formatted duration modifier string</returns>
		private string GetDurationModifier(int varNum, Variable durationModifierVar)
		{
			string durationModifier = null;
			TQColor? color = null;

			if (!TranslationService.TryTranslateXTag("ImprovedTimeFormat", out var formatSpec))
			{
				formatSpec = "?with {0:f0}% Improved Duration?";
				color = ItemStyle.Legendary.TQColor();
			}
			else
			{
				if (TQDebug.ItemDebugLevel > 2)
					Log.LogDebug("Item.formatspec (improved time) = " + formatSpec);

				formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
			}

			durationModifier = Format(formatSpec, durationModifierVar[Math.Min(durationModifierVar.NumberOfValues - 1, varNum)]);

			return $"{color?.ColorTag()}{durationModifier}";
		}

		/// <summary>
		/// Gets a formatted chance modifier string
		/// </summary>
		/// <param name="varNum">offset number of the variable value that we are using</param>
		/// <param name="modifierChanceVar">Chance modifier variable</param>
		/// <returns>formatted chance modifier string</returns>
		private string GetChanceModifier(int varNum, Variable modifierChanceVar)
		{
			string modifierChance = null;
			TQColor? color = null;

			if (!TranslationService.TryTranslateXTag("ChanceOfTag", out var formatSpec))
			{
				formatSpec = "?{%.1f0}% Chance of?";
				color = ItemStyle.Legendary.TQColor();
			}
			else
			{
				if (TQDebug.ItemDebugLevel > 2)
					Log.LogDebug("Item.formatspec (chance) = " + formatSpec);

				formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
			}

			modifierChance = this.Format(formatSpec, modifierChanceVar[Math.Min(modifierChanceVar.NumberOfValues - 1, varNum)]);

			return $"{color?.ColorTag()}{modifierChance}";
		}

		/// <summary>
		/// Gets the global chance string
		/// </summary>
		/// <param name="attributeList">Arraylist containing the attributes</param>
		/// <param name="varNum">offset number of the variable value that we are using</param>
		/// <param name="v">variable structure</param>
		/// <param name="font">font string</param>
		/// <returns>formatted global chance string</returns>
		private string GetGlobalChance(List<Variable> attributeList, int varNum, Variable v, ref TQColor? font)
		{
			string line;
			string tag = "GlobalPercentChanceOfAllTag";

			// use our hack to determine if it was XOR or not.
			if (attributeList.Count > 2)
			{
				// Spurious global chance indicator.  Do not use
				line = string.Empty;
			}
			else
			{
				if (attributeList.Count > 1)
					tag = "GlobalPercentChanceOfOneTag";

				if (!TranslationService.TryTranslateXTag(tag, out var formatSpec))
				{
					formatSpec = string.Format(CultureInfo.CurrentCulture, "{0:f1}% ?{0}?", tag);
					font = ItemStyle.Legendary.TQColor();
				}
				else
				{
					if (TQDebug.ItemDebugLevel > 2)
						Log.LogDebug("Item.formatspec (chance of one) = " + formatSpec);

					formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
					font = ItemStyle.Epic.TQColor();
				}

				line = Format(formatSpec, v[System.Math.Min(v.NumberOfValues - 1, varNum)]);
			}

			return line;
		}


		/// <summary>
		/// Gets the formatted racial bonus string(s)
		/// </summary>
		/// <param name="record">DBRecord of the databse record</param>
		/// <param name="results">List containing the results</param>
		/// <param name="varNum">offset number of the variable value that we are using</param>
		/// <param name="isGlobal">Flag to signal global parameters</param>
		/// <param name="globalIndent">global indent string</param>
		/// <param name="v">variable structure</param>
		/// <param name="d">ItemAttributesData structure</param>
		/// <param name="line">line string</param>
		/// <param name="color">display font string</param>
		/// <returns>formatted string of racial bonus(es)  adds to the results if there are multiple.</returns>
		private string GetRacialBonus(DBRecordCollection record, Item itm, List<string> results, int varNum, bool isGlobal, string globalIndent, Variable v, ItemAttributesData d, string line, ref TQColor? color)
		{
			// Added by VillageIdiot
			// Updated to accept multiple racial bonuses in record
			string[] races = record.GetAllStrings("racialBonusRace");
			if (races != null)
			{
				for (int j = 0; j < races.Length; ++j)
				{

					if (!TranslationService.TryTranslateXTag($"racialBonusRace{races[j]}", out var finalRace))
					{
						finalRace = races[j];

						if (TQDebug.DebugEnabled)
							Log.LogDebug("missing racialBonusRace={0}", finalRace);
					}

					string formatTag = string.Concat(d.FullAttribute.Substring(0, 1).ToUpperInvariant(), d.FullAttribute.Substring(1));

					if (!TranslationService.TryTranslateXTag(formatTag, out var formatSpec))
						formatSpec = string.Concat(formatTag, " {0} {1}");
					else
					{
						if (TQDebug.ItemDebugLevel > 2)
							Log.LogDebug("Item.formatspec (race bonus) = " + formatSpec);

						formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
					}


					if (line != null)
					{
						if (d.Variable.Length > 0)
						{
							string s = $"{ItemStyle.Legendary.TQColor().ColorTag()}{d.Variable}";
							line = string.Concat(line, s);
						}
						else
						{
							// There are multiple lines to the attribute so the color tag needs to be added.
							string s = $"{ItemStyle.Epic.TQColor().ColorTag()}";
							line = string.Concat(s, line);
						}

						if (isGlobal)
							line = string.Concat(globalIndent, line);

						results.Add(line);
						itm.CurrentFriendlyNameResult.TmpAttrib.Add(line);
					}

					line = Format(formatSpec, v[Math.Min(v.NumberOfValues - 1, varNum)], finalRace);
					color = ItemStyle.Epic.TQColor();
				}
			}

			return line;
		}



		/// <summary>
		/// Gets the + to all skills string
		/// </summary>
		/// <param name="variableNumber">offset number of the variable value that we are using</param>
		/// <param name="variable">variable structure</param>
		/// <param name="color">display font string</param>
		/// <returns>formatted string for + to all skills</returns>
		private string GetAugmentAllLevel(int variableNumber, Variable variable, ref TQColor? color)
		{
			string tag = "ItemAllSkillIncrement";

			if (!TranslationService.TryTranslateXTag(tag, out var formatSpec))
			{
				formatSpec = "?+{0} to all skills?";
				color = ItemStyle.Legendary.TQColor();
			}
			else
			{
				if (TQDebug.ItemDebugLevel > 2)
					Log.LogDebug("Item.formatspec (augment level) = " + formatSpec);

				formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
				color = ItemStyle.Epic.TQColor();
			}

			return Format(formatSpec, variable[System.Math.Min(variable.NumberOfValues - 1, variableNumber)]);
		}

		/// <summary>
		/// Get + to a Mastery string
		/// </summary>
		/// <param name="record">DBRecord database record</param>
		/// <param name="variable">variable structure</param>
		/// <param name="attributeData">ItemAttributesData structure</param>
		/// <param name="font">display font string</param>
		/// <returns>formatted string with the + to mastery</returns>
		private string GetAugmentMasteryLevel(DBRecordCollection record, Variable variable, ItemAttributesData attributeData, ref TQColor? font)
		{
			string augmentNumber = attributeData.FullAttribute.Substring(19, 1);
			string skillRecordKey = string.Concat("augmentMasteryName", augmentNumber);
			string skillRecordID = record.GetString(skillRecordKey, 0);

			if (string.IsNullOrEmpty(skillRecordID))
				skillRecordID = skillRecordKey;

			string skillName = null;
			DBRecordCollection skillRecord = Database.GetRecordFromFile(skillRecordID);
			if (skillRecord != null)
			{
				string nameTag = skillRecord.GetString("skillDisplayName", 0);

				if (!string.IsNullOrEmpty(nameTag))
					TranslationService.TryTranslateXTag(nameTag, out skillName);
			}

			if (string.IsNullOrEmpty(skillName))
			{
				skillName = Path.GetFileNameWithoutExtension(skillRecordID);
				font = ItemStyle.Legendary.TQColor();
			}

			// now get the formatSpec
			if (!TranslationService.TryTranslateXTag("ItemMasteryIncrement", out var formatSpec))
			{
				formatSpec = "?+{0} to skills in {1}?";
				if (font == null)
					font = ItemStyle.Legendary.TQColor();
			}
			else
			{
				if (TQDebug.ItemDebugLevel > 2)
					Log.LogDebug("Item.formatspec (augment mastery) = " + formatSpec);

				formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
				if (!font.HasValue)
					font = ItemStyle.Epic.TQColor();
			}

			return Format(formatSpec, variable[0], skillName);
		}


		/// <summary>
		/// Gets a formatted + to skill string
		/// </summary>
		/// <param name="record">DBRecord database record</param>
		/// <param name="variable">variable structure</param>
		/// <param name="attributeData">ItemAttributesData structure</param>
		/// <param name="line">line of text</param>
		/// <param name="font">display font string</param>
		/// <returns>formatted string containing + to skill</returns>
		private string GetAugmentSkillLevel(DBRecordCollection record, Variable variable, ItemAttributesData attributeData, string line, ref TQColor? font)
		{
			string augmentSkillNumber = attributeData.FullAttribute.Substring(17, 1);
			string skillRecordKey = string.Concat("augmentSkillName", augmentSkillNumber);
			string skillRecordID = record.GetString(skillRecordKey, 0);

			if (!string.IsNullOrEmpty(skillRecordID))
			{
				string skillName = null;
				string nameTag = null;
				DBRecordCollection skillRecord = Database.GetRecordFromFile(skillRecordID);
				if (skillRecord != null)
				{
					// Changed by VillageIdiot
					// for augmenting buff skills
					string buffSkillName = skillRecord.GetString("buffSkillName", 0);
					if (string.IsNullOrEmpty(buffSkillName))
					{
						// Not a buff so look up the name
						nameTag = skillRecord.GetString("skillDisplayName", 0);
						if (!string.IsNullOrEmpty(nameTag))
							TranslationService.TryTranslateXTag(nameTag, out skillName);
						else
						{
							// Added by VillageIdiot
							// Check to see if this is a pet skill
							nameTag = skillRecord.GetString("Class", 0);
							if (nameTag.Contains("PetModifier"))
							{
								string petSkillID = skillRecord.GetString("petSkillName", 0);
								DBRecordCollection petSkillRecord = Database.GetRecordFromFile(petSkillID);
								if (petSkillRecord != null)
								{
									string petNameTag = petSkillRecord.GetString("skillDisplayName", 0);
									if (!string.IsNullOrEmpty(petNameTag))
										TranslationService.TryTranslateXTag(petNameTag, out skillName);
								}
							}
						}
					}
					else
					{
						// This is a buff skill
						DBRecordCollection buffSkillRecord = Database.GetRecordFromFile(buffSkillName);
						if (buffSkillRecord != null)
						{
							nameTag = buffSkillRecord.GetString("skillDisplayName", 0);
							if (!string.IsNullOrEmpty(nameTag))
								TranslationService.TryTranslateXTag(nameTag, out skillName);
						}
					}
				}

				if (string.IsNullOrEmpty(skillName))
					skillName = Path.GetFileNameWithoutExtension(skillRecordID);

				// now get the formatSpec
				if (!TranslationService.TryTranslateXTag("ItemSkillIncrement", out var formatSpec))
				{
					formatSpec = "?+{0} to skill {1}?";
					font = ItemStyle.Legendary.TQColor();
				}
				else
				{
					if (TQDebug.ItemDebugLevel > 2)
						Log.LogDebug("Item.formatspec (item skill) = " + formatSpec);

					formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
					font = ItemStyle.Epic.TQColor();
				}

				line = this.Format(formatSpec, variable[0], skillName);
			}

			return line;
		}

		/// <summary>
		/// Gets the formatted formulae string(s)
		/// </summary>
		/// <param name="results">results list</param>
		/// <param name="variable">variable structure</param>
		/// <param name="attributeData">ItemAttributesData structure</param>
		/// <param name="line">line of text</param>
		/// <param name="font">display font string</param>
		/// <returns>formatted formulae string</returns>
		private string GetFormulae(List<string> results, Variable variable, ItemAttributesData attributeData, string line, ref TQColor? font)
		{
			// Special case for formulae reagents
			if (attributeData.FullAttribute.StartsWith("reagent", StringComparison.OrdinalIgnoreCase))
			{
				DBRecordCollection reagentRecord = Database.GetRecordFromFile(variable.GetString(0));
				if (reagentRecord != null)
				{
					string nameTag = reagentRecord.GetString("description", 0);
					if (!string.IsNullOrEmpty(nameTag))
					{
						string reagentName = TranslationService.TranslateXTag(nameTag);
						string formatSpec = "{0}";
						font = ItemStyle.Common.TQColor();
						line = Format(formatSpec, reagentName);
					}
				}
			}
			else if (attributeData.FullAttribute.Equals("artifactCreationCost"))
			{
				if (!TranslationService.TryTranslateXTag("xtagArtifactCost", out var formatSpec))
					formatSpec = "Gold Cost: {0}";
				else
					formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);

				if (TQDebug.ItemDebugLevel > 2)
					Log.LogDebug("Item.formatspec (Artifact cost) = " + formatSpec);

				font = ItemStyle.Rare.TQColor();
				results.Add(string.Empty);
				line = Format(formatSpec, string.Format(CultureInfo.CurrentCulture, "{0:N0}", variable[0]));
			}

			return line;
		}

		/// <summary>
		/// Gets a formatted string of the granted skill
		/// </summary>
		/// <param name="record">DBRecord database record</param>
		/// <param name="results">results list</param>
		/// <param name="variable">variable structure</param>
		/// <param name="line">line of text</param>
		/// <param name="font">display font string</param>
		/// <returns>formatted granted skill string.</returns>
		private string GetGrantedSkill(DBRecordCollection record, Item itm, List<string> results, Variable variable, string line, ref TQColor? font)
		{
			// Added by VillageIdiot
			// Special case for granted skills
			DBRecordCollection skillRecord = Database.GetRecordFromFile(variable.GetString(0));
			if (skillRecord != null)
			{
				// Add a blank line and then the Grants Skill text
				results.Add(string.Empty);
				font = ItemStyle.Mundane.TQColor();

				if (!TranslationService.TryTranslateXTag("tagItemGrantSkill", out var skillTag))
					skillTag = "Grants Skill :";

				var value = $"{font?.ColorTag()}{skillTag}";
				results.Add(value);
				itm.CurrentFriendlyNameResult.TmpAttrib.Add(value);

				string skillName = null;
				string nameTag = null;

				// Changed by VillageIdiot
				// Let's actually test if there is a buff skill
				string buffSkillName = skillRecord.GetString("buffSkillName", 0);
				if (string.IsNullOrEmpty(buffSkillName))
				{
					nameTag = skillRecord.GetString("skillDisplayName", 0);
					if (!string.IsNullOrEmpty(nameTag))
					{
						if (!TranslationService.TryTranslateXTag(nameTag, out skillName))
							skillName = Path.GetFileNameWithoutExtension(variable.GetString(0));
					}
				}
				else
				{
					// This is a buff skill
					DBRecordCollection buffSkillRecord = Database.GetRecordFromFile(buffSkillName);
					if (buffSkillRecord != null)
					{
						nameTag = buffSkillRecord.GetString("skillDisplayName", 0);
						if (!string.IsNullOrEmpty(nameTag))
						{
							if (!TranslationService.TryTranslateXTag(nameTag, out skillName))
								skillName = Path.GetFileNameWithoutExtension(variable.GetString(0));
						}
					}
				}

				// Added by VillageIdiot to support skill activation text
				string triggerType = null;
				string activationTag = null;
				string activationText = null;
				string autoController = record.GetString("itemSkillAutoController", 0);
				if (!string.IsNullOrEmpty(autoController))
				{
					DBRecordCollection autoControllerRecord = Database.GetRecordFromFile(autoController);
					if (autoControllerRecord != null)
						triggerType = autoControllerRecord.GetString("triggerType", 0);
				}

				// Convert TriggerType into text tag
				if (!string.IsNullOrEmpty(triggerType))
				{
					switch (triggerType.ToUpperInvariant())
					{
						case "LOWHEALTH":
							// Activated on low health
							activationTag = "xtagAutoSkillCondition01";
							break;

						case "LOWMANA":
							// Activated on low energy
							activationTag = "xtagAutoSkillCondition02";
							break;

						case "HITBYENEMY":
							// Activated upon taking damage
							activationTag = "xtagAutoSkillCondition03";
							break;

						case "HITBYMELEE":
							// Activated upon taking melee damage
							activationTag = "xtagAutoSkillCondition04";
							break;

						case "HITBYPROJECTILE":
							// Activated upon taking ranged damage
							activationTag = "xtagAutoSkillCondition05";
							break;

						case "CASTBUFF":
							// Activated upon casting a buff
							activationTag = "xtagAutoSkillCondition06";
							break;

						case "ATTACKENEMY":
							// Activated on attack
							activationTag = "xtagAutoSkillCondition07";
							break;

						case "ONEQUIP":
							// Activated when equipped
							activationTag = "xtagAutoSkillCondition08";
							break;

						default:
							activationTag = string.Empty;
							break;
					}
				}

				if (!string.IsNullOrEmpty(activationTag))
					TranslationService.TryTranslateXTag(activationTag, out activationText);
				else
					activationText = string.Empty;

				if (string.IsNullOrEmpty(activationText))
					font = ItemStyle.Epic.TQColor();
				else
					font = ItemStyle.Mundane.TQColor();

				line = Format("{0} {1}", skillName, activationText);
			}

			return line;
		}


		/// <summary>
		/// Gets the pet bonus string
		/// </summary>
		/// <param name="color">display font string</param>
		/// <returns>formatted pet bonus name</returns>
		private string GetPetBonusName(ref TQColor? color)
		{
			string tag = "xtagPetBonusNameAllPets";
			if (!TranslationService.TryTranslateXTag(tag, out var formatSpec))
			{
				formatSpec = "?Bonus to All Pets:?";
				color = ItemStyle.Legendary.TQColor();
			}
			else
			{
				if (TQDebug.ItemDebugLevel > 2)
					Log.LogDebug("Item.formatspec (pet bonus) = " + formatSpec);

				formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
				color = ItemStyle.Relic.TQColor();
			}

			return formatSpec;
		}


		/// <summary>
		/// Gets the skill effects string
		/// </summary>
		/// <param name="baseAttributeData">ItemAttributesData structure of the base attribute</param>
		/// <param name="variableNumber">offset number of the variable value that we are using</param>
		/// <param name="variable">variable structure</param>
		/// <param name="currentAttributeData">ItemAttributesData structure of the current attribute</param>
		/// <param name="line">line of text</param>
		/// <param name="color">display font string</param>
		/// <returns>formatted skill effect string</returns>
		private string GetSkillEffect(ItemAttributesData baseAttributeData, int variableNumber, Variable variable, ItemAttributesData currentAttributeData, string line, ref TQColor? color)
		{
			string labelTag = ItemAttributeProvider.GetAttributeTextTag(baseAttributeData);
			if (string.IsNullOrEmpty(labelTag))
			{
				labelTag = string.Concat("?", baseAttributeData.FullAttribute, "?");
				color = ItemStyle.Legendary.TQColor();
			}

			if (!TranslationService.TryTranslateXTag(labelTag, out var label))
			{
				label = string.Concat("?", labelTag, "?");
				color = ItemStyle.Legendary.TQColor();
			}

			if (TQDebug.ItemDebugLevel > 2)
				Log.LogDebug("Item.label (scroll) = " + label);

			label = ItemAttributeProvider.ConvertFormat(label);

			// Find the extra format tag for those that take 2 parameters.
			string formatSpecTag = null;
			string formatSpec = null;
			if (currentAttributeData.FullAttribute.EndsWith("Cost", StringComparison.OrdinalIgnoreCase))
				formatSpecTag = "SkillIntFormat";
			else if (currentAttributeData.FullAttribute.EndsWith("Level", StringComparison.OrdinalIgnoreCase))
				formatSpecTag = "SkillIntFormat";
			else if (currentAttributeData.FullAttribute.EndsWith("Duration", StringComparison.OrdinalIgnoreCase))
				formatSpecTag = "SkillSecondFormat";
			else if (currentAttributeData.FullAttribute.EndsWith("Radius", StringComparison.OrdinalIgnoreCase))
				formatSpecTag = "SkillDistanceFormat";

			if (!string.IsNullOrEmpty(formatSpecTag))
			{
				if (!TranslationService.TryTranslateXTag(formatSpecTag, out formatSpec))
				{
					formatSpec = "?{0} {1}?";
					color = ItemStyle.Legendary.TQColor();
				}
				else
				{
					if (TQDebug.ItemDebugLevel > 2)
						Log.LogDebug("Item.formatspec (2 parameter) = " + formatSpec);

					formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
					color = ItemStyle.Epic.TQColor();
				}
			}

			if (string.IsNullOrEmpty(formatSpecTag))
			{
				color = ItemStyle.Epic.TQColor();
				line = Format(label, variable[Math.Min(variable.NumberOfValues - 1, variableNumber)]);
			}
			else
			{
				line = Format(formatSpec, variable[Math.Min(variable.NumberOfValues - 1, variableNumber)], label);
			}

			return line;
		}

		/// <summary>
		/// Gets a raw attribute string
		/// </summary>
		/// <param name="attributeData">ItemAttributesData structure</param>
		/// <param name="variableNumber">offset number of the variable value that we are using</param>
		/// <param name="variable">variable structure</param>
		/// <param name="color">display font string</param>
		/// <returns>formatted raw attribute string</returns>
		private string GetRawAttribute(ItemAttributesData attributeData, int variableNumber, Variable variable, ref TQColor? color)
		{
			string line = null;

			string labelTag = ItemAttributeProvider.GetAttributeTextTag(attributeData);
			if (string.IsNullOrWhiteSpace(labelTag))
			{
				labelTag = string.Concat("?", attributeData.FullAttribute, "?");
				color = ItemStyle.Legendary.TQColor();
			}

			if (!TranslationService.TryTranslateXTag(labelTag, out var label))
			{
				label = string.Concat("?", labelTag, "?");
				color = ItemStyle.Legendary.TQColor();
			}

			label = ItemAttributeProvider.ConvertFormat(label);
			if (label.IndexOf('{') >= 0)
			{
				// we have a format string.  try using it.
				line = Format(label, variable[Math.Min(variable.NumberOfValues - 1, variableNumber)]);
				if (!color.HasValue)
					color = ItemStyle.Epic.TQColor();
			}
			else
			{
				// no format string.
				line = Database.VariableToStringNice(variable);
			}

			if (!color.HasValue)
				color = ItemStyle.Legendary.TQColor(); // make these unknowns stand out

			return line;
		}

		/// <summary>
		/// Converts the item's offensice attributes to a string
		/// </summary>
		/// <param name="record">DBRecord of the database record</param>
		/// <param name="attributeList">ArrayList containing the attribute list</param>
		/// <param name="data">ItemAttributesData for the item</param>
		/// <param name="recordId">string containing the record id</param>
		/// <param name="results">List containing the results</param>
		private void ConvertOffenseAttributesToString(Item itm, DBRecordCollection record, List<Variable> attributeList, ItemAttributesData data, string recordId, List<string> results)
		{
			if (TQDebug.ItemDebugLevel > 0)
			{
				Log.LogDebug("Item.ConvertOffenseAttrToString({0}, {1}, {2}, {3}, {4})"
					, record, attributeList, data, recordId, results
				);
			}

			// If we are a relic, then sometimes there are multiple values per variable depending on how many pieces we have.
			// Let's determine which variable we want in these cases.
			int variableNumber = 0;
			if (itm.IsRelic && recordId == itm.BaseItemId)
				variableNumber = itm.Number - 1;
			else if (itm.HasRelicSlot1 && recordId == itm.relicID)
				variableNumber = Math.Max(itm.Var1, 1) - 1;
			else if (itm.HasRelicSlot2 && recordId == itm.relic2ID)
				variableNumber = Math.Max(itm.Var2, 1) - 1;

			// Pet skills can also have multiple values so we attempt to decode it here
			if (itm.IsScroll || itm.IsRelic)
				variableNumber = GetPetSkillLevel(itm, record, recordId, variableNumber);

			// Triggered skills can have also multiple values so we need to decode it here
			if (record.GetString("Class", 0).ToUpperInvariant().StartsWith("SKILL", StringComparison.OrdinalIgnoreCase))
				variableNumber = GetTriggeredSkillLevel(itm, record, recordId, variableNumber);

			// See what variables we have
			ItemAttributesData minData = null;
			ItemAttributesData maxData = null;
			ItemAttributesData minDurData = null;
			ItemAttributesData maxDurData = null;
			ItemAttributesData chanceData = null;
			ItemAttributesData modifierData = null;
			ItemAttributesData durationModifierData = null;
			ItemAttributesData modifierChanceData = null;
			ItemAttributesData damageRatioData = null;  // Added by VillageIdiot

			Variable minVar = null;
			Variable maxVar = null;
			Variable minDurVar = null;
			Variable maxDurVar = null;
			Variable chanceVar = null;
			Variable modifierVar = null;
			Variable durationModifierVar = null;
			Variable modifierChanceVar = null;
			Variable damageRatioVar = null;  // Added by VillageIdiot

			bool isGlobal = ItemAttributeProvider.AttributeGroupHas(new Collection<Variable>(attributeList), "Global");
			string globalIndent = new string(' ', 4);

			foreach (Variable variable in attributeList)
			{
				if (variable == null)
					continue;

				ItemAttributesData attributeData = ItemAttributeProvider.GetAttributeData(variable.Name);
				if (attributeData == null)
				{
					// unknown attribute
					attributeData = new ItemAttributesData(ItemAttributesEffectType.Other, variable.Name, variable.Name, string.Empty, 0);
				}

				switch (attributeData.Variable.ToUpperInvariant())
				{
					case "MIN":
						minData = attributeData;
						minVar = variable;
						break;


					case "MAX":
						maxData = attributeData;
						maxVar = variable;
						break;


					case "DURATIONMIN":
						minDurData = attributeData;
						minDurVar = variable;
						break;


					case "DURATIONMAX":
						maxDurData = attributeData;
						maxDurVar = variable;
						break;


					case "CHANCE":
						chanceData = attributeData;
						chanceVar = variable;
						break;


					case "MODIFIER":
						modifierData = attributeData;
						modifierVar = variable;
						break;


					case "MODIFIERCHANCE":
						modifierChanceData = attributeData;
						modifierChanceVar = variable;
						break;


					case "DURATIONMODIFIER":
						durationModifierData = attributeData;
						durationModifierVar = variable;
						break;


					case "DRAINMIN":
						// Added by VillageIdiot
						minData = attributeData;
						minVar = variable;
						break;


					case "DRAINMAX":
						// Added by VillageIdiot
						maxData = attributeData;
						maxVar = variable;
						break;


					case "DAMAGERATIO":
						// Added by VillageIdiot
						damageRatioData = attributeData;
						damageRatioVar = variable;
						break;

				}
			}

			// Figure out the label string
			string labelTag = null;
			TQColor? labelColor = null;
			string label = GetLabelAndColorFromTag(itm, data, recordId, ref labelTag, ref labelColor);

			if (TQDebug.ItemDebugLevel > 1)
			{
				Log.LogDebug("Full attribute = " + data.FullAttribute);
				Log.LogDebug("Item.label = " + label);
			}

			label = ItemAttributeProvider.ConvertFormat(label);

			// Figure out the Amount string
			string amount = null;
			if (minData != null
				&& maxData != null
				&& minVar.GetSingle(Math.Min(minVar.NumberOfValues - 1, variableNumber)) != maxVar.GetSingle(Math.Min(maxVar.NumberOfValues - 1, variableNumber))
			)
			{
				if (minDurVar != null)
					amount = GetAmountRange(itm, data, variableNumber, minVar, maxVar, ref label, labelColor, minDurVar);
				else
					amount = GetAmountRange(itm, data, variableNumber, minVar, maxVar, ref label, labelColor);
			}
			else
			{
				if (minDurVar != null)
					amount = GetAmountSingle(itm, data, variableNumber, minVar, maxVar, ref label, labelColor, minDurVar);
				else
					amount = GetAmountSingle(itm, data, variableNumber, minVar, maxVar, ref label, labelColor);
			}

			// Figure out the duration string
			string duration = null;
			//If we have both minDurData and maxDurData we also need to check if the actual Values of minDurVar and maxDurVar are actually different
			float minDurVarValue = -1;
			float maxDurVarValue = -1;

			if (minDurData != null)
				minDurVarValue = (float)minDurVar[minDurVar.NumberOfValues - 1];

			if (maxDurData != null)
				maxDurVarValue = (float)maxDurVar[maxDurVar.NumberOfValues - 1];
			if (minDurData != null && maxDurData != null && minDurVarValue != maxDurVarValue)
				duration = GetDurationRange(variableNumber, minDurVar, maxDurVar);
			else
				duration = GetDurationSingle(variableNumber, minDurVar, maxDurVar);

			// Figure out the Damage Ratio string
			string damageRatio = null;
			if (damageRatioData != null)
				damageRatio = GetDamageRatio(variableNumber, damageRatioData, damageRatioVar);

			// Figure out the chance string
			string chance = null;
			if (chanceData != null)
				chance = GetChance(variableNumber, chanceVar);

			// Display the chance + label + Amount + Duration + DamageRatio
			string[] strarray = new string[5];
			int numberOfStrings = 0;
			if (!string.IsNullOrEmpty(label))
			{
				if (!label.HasColorPrefix())
					label = $"{labelColor?.ColorTag()}{label}";
			}

			if (!string.IsNullOrEmpty(chance))
				strarray[numberOfStrings++] = chance;

			if (!string.IsNullOrEmpty(amount))
				strarray[numberOfStrings++] = amount;

			if (!string.IsNullOrEmpty(label))
				strarray[numberOfStrings++] = label;

			if (!string.IsNullOrEmpty(duration))
				strarray[numberOfStrings++] = duration;

			if (!string.IsNullOrEmpty(damageRatio))
				// Added by VillageIdiot
				strarray[numberOfStrings++] = damageRatio;

			if (!string.IsNullOrEmpty(amount) || !string.IsNullOrEmpty(duration))
			{
				string amountOrDurationText = string.Join(" ", strarray, 0, numberOfStrings);

				// Figure out what color to use
				TQColor? fontColor = null;
				if (!isGlobal
					&& (string.IsNullOrEmpty(chance) || data.Effect.Equals("defensiveBlock"))
					&& recordId == itm.BaseItemId
					&& string.IsNullOrEmpty(duration)
					&& !string.IsNullOrEmpty(amount)
				)
				{
					if (itm.IsWeapon)
					{
						if (data.Effect.Equals("offensivePierceRatio")
							|| data.Effect.Equals("offensivePhysical")
							|| data.Effect.Equals("offensiveBaseFire")
							|| data.Effect.Equals("offensiveBaseCold")
							|| data.Effect.Equals("offensiveBaseLightning")
							|| data.Effect.Equals("offensiveBaseLife")
						)
						{
							// mundane effect
							fontColor = ItemStyle.Mundane.TQColor();
						}
					}

					if (itm.IsShield)
					{
						if (data.Effect.Equals("defensiveBlock")
							|| data.Effect.Equals("blockRecoveryTime")
							|| data.Effect.Equals("offensivePhysical")
						)
						{
							fontColor = ItemStyle.Mundane.TQColor();
						}
					}
				}

				// magical effect
				if (!fontColor.HasValue)
					fontColor = ItemStyle.Epic.TQColor();

				if (!amountOrDurationText.HasColorPrefix())
					amountOrDurationText = $"{fontColor?.ColorTag()}{amountOrDurationText}";

				if (isGlobal)
					amountOrDurationText = amountOrDurationText.InsertAfterColorPrefix(globalIndent);

				results.Add(amountOrDurationText);
				itm.CurrentFriendlyNameResult.TmpAttrib.Add(amountOrDurationText);
			}
			else
			{
				// null these out to indicate they did not get used
				amount = null;
				duration = null;
				chance = null;
			}

			// now see if we have a modifier
			string modifier = null;
			if (modifierData != null)
				modifier = GetModifier(data, variableNumber, modifierData, modifierVar);

			string durationModifier = null;
			if (durationModifierData != null)
				durationModifier = GetDurationModifier(variableNumber, durationModifierVar);

			string modifierChance = null;
			if (modifierChanceData != null)
				modifierChance = GetChanceModifier(variableNumber, modifierChanceVar);

			numberOfStrings = 0;
			if (!string.IsNullOrEmpty(modifierChance))
				strarray[numberOfStrings++] = modifierChance;

			if (!string.IsNullOrEmpty(modifier))
				strarray[numberOfStrings++] = modifier;

			if (!string.IsNullOrEmpty(durationModifier))
				strarray[numberOfStrings++] = durationModifier;

			if (!string.IsNullOrEmpty(modifier))
			{
				string modifierText = string.Join(" ", strarray, 0, numberOfStrings);

				if (isGlobal)
					modifierText = string.Concat(globalIndent, modifierText);

				if (!modifierText.HasColorPrefix())
					modifierText = $"{ItemStyle.Epic.TQColor().ColorTag()}{modifierText}";

				results.Add(modifierText);
				itm.CurrentFriendlyNameResult.TmpAttrib.Add(modifierText);
			}
			else
			{
				modifier = null;
				modifierChance = null;
				durationModifier = null;
			}

			// Added so we only show the title once for the group.
			bool displayDamageQualifierTitle = true;

			// Now display any other variables we did not see here.
			foreach (Variable variable in attributeList)
			{
				if (variable == null)
					continue;

				ItemAttributesData attributeData = ItemAttributeProvider.GetAttributeData(variable.Name);

				if (attributeData == null)
					// unknown attribute
					attributeData = new ItemAttributesData(ItemAttributesEffectType.Other, variable.Name, variable.Name, string.Empty, 0);

				string normalizedAttributeVariable = attributeData.Variable.ToUpperInvariant();
				if (
					!(
						amount != null
						&& (normalizedAttributeVariable == "MIN"
							|| normalizedAttributeVariable == "MAX"
							|| normalizedAttributeVariable == "DRAINMIN"
							|| attributeData.Variable == "DRAINMAX"
						)
					)
					&& !(duration != null && (normalizedAttributeVariable == "DURATIONMIN" || normalizedAttributeVariable == "DURATIONMAX"))
					&& !(chance != null && normalizedAttributeVariable == "CHANCE")
					&& !(modifier != null && normalizedAttributeVariable == "MODIFIER")
					&& !(durationModifier != null && normalizedAttributeVariable == "DURATIONMODIFIER")
					&& !(modifierChance != null && normalizedAttributeVariable == "MODIFIERCHANCE")
					&& !(damageRatio != null && normalizedAttributeVariable == "DAMAGERATIO")
					&& normalizedAttributeVariable != "GLOBAL"
					&& !(normalizedAttributeVariable == "XOR" && isGlobal)
				)
				{
					string line = null;
					TQColor? color = null;
					string normalizedFullAttribute = attributeData.FullAttribute.ToUpperInvariant();
					if (normalizedFullAttribute == "CHARACTERBASEATTACKSPEEDTAG")
					{
						// only display itm tag if we are a basic weapon
						if (itm.IsWeapon && recordId == itm.BaseItemId)
						{
							color = ItemStyle.Mundane.TQColor();
							line = TranslationService.TranslateXTag(variable.GetString(0));
						}
						else
							line = string.Empty;
					}
					else if (normalizedFullAttribute.EndsWith("GLOBALCHANCE", StringComparison.OrdinalIgnoreCase))
						line = GetGlobalChance(attributeList, variableNumber, variable, ref color);
					else if (normalizedFullAttribute.StartsWith("RACIALBONUS", StringComparison.OrdinalIgnoreCase))
						line = GetRacialBonus(record, itm, results, variableNumber, isGlobal, globalIndent, variable, attributeData, line, ref color);
					else if (normalizedFullAttribute == "AUGMENTALLLEVEL")
						line = GetAugmentAllLevel(variableNumber, variable, ref color);
					else if (normalizedFullAttribute.StartsWith("AUGMENTMASTERYLEVEL", StringComparison.OrdinalIgnoreCase))
						line = GetAugmentMasteryLevel(record, variable, attributeData, ref color);
					else if (normalizedFullAttribute.StartsWith("AUGMENTSKILLLEVEL", StringComparison.OrdinalIgnoreCase))
						line = GetAugmentSkillLevel(record, variable, attributeData, line, ref color);
					else if (itm.IsFormulae && recordId == itm.BaseItemId)
						line = GetFormulae(results, variable, attributeData, line, ref color);
					else if (normalizedFullAttribute == "ITEMSKILLNAME")
						line = GetGrantedSkill(record, itm, results, variable, line, ref color);

					// Added by VillageIdiot
					// Shows the header text for the pet bonus
					if (normalizedFullAttribute == "PETBONUSNAME")
						line = StringHelper.TQNewLineTag + GetPetBonusName(ref color);

					// Added by VillageIdiot
					// Set the scale percent here
					if (recordId == itm.BaseItemId && normalizedFullAttribute == "ATTRIBUTESCALEPERCENT" && itm.itemScalePercent == 1.00)
					{
						itm.itemScalePercent += variable.GetSingle(0) / 100;

						// Set line to nothing so we do not see the tag text.
						line = string.Empty;
					}
					else if (normalizedFullAttribute == "SKILLNAME")
					{
						// Added by VillageIdiot
						// itm is for Scroll effects which get decoded in the skill code below
						// Set line to nothing so we do not see the tag text.
						line = string.Empty;
					}
					else if (attributeData.EffectType == ItemAttributesEffectType.SkillEffect)
					{
						line = GetSkillEffect(data, variableNumber, variable, attributeData, line, ref color);
					}
					else if (normalizedFullAttribute.EndsWith("DAMAGEQUALIFIER", StringComparison.OrdinalIgnoreCase))
					{
						// Added by VillageIdiot
						// for Damage Absorption

						// Get the qualifier title
						if (!TranslationService.TryTranslateXTag("tagDamageAbsorptionTitle", out var title))
							title = "Protects Against :";

						// We really only want to show the title once for the group.
						if (displayDamageQualifierTitle)
						{
							results.Add(title);
							itm.CurrentFriendlyNameResult.TmpAttrib.Add(title);
							displayDamageQualifierTitle = false;
						}

						// Show the damage type
						string damageTag = attributeData.FullAttribute.Remove(attributeData.FullAttribute.Length - 15);
						damageTag = string.Concat(damageTag.Substring(0, 1).ToUpperInvariant(), damageTag.Substring(1));
						TranslationService.TryTranslateXTag(string.Concat("tagQualifyingDamage", damageTag), out var damageType);

						if (!TranslationService.TryTranslateXTag("formatQualifyingDamage", out var formatSpec))
							formatSpec = "{0}";
						else
						{
							if (TQDebug.ItemDebugLevel > 2)
								Log.LogDebug("Item.formatspec (Damage type) = " + formatSpec);

							formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
						}

						color = ItemStyle.Mundane.TQColor();
						line = Format(formatSpec, damageType);
					}

					// We have no line so just show the raw attribute
					if (line == null)
						line = GetRawAttribute(data, variableNumber, variable, ref color);

					// Start finalizing the line of text
					string itemSkillAutoController = null;
					if (line.Length > 0)
					{
						if (attributeData.Variable.Length > 0)
							line = string.Concat(
								line
								, ' '
								, $"{ItemStyle.Legendary.TQColor().ColorTag()}{attributeData.Variable}"
							);

						// Add another special case for skill name formatting
						// if it's an activated skill
						if (normalizedFullAttribute == "ITEMSKILLNAME")
						{
							itemSkillAutoController = record.GetString("itemSkillAutoController", 0);
							if (!string.IsNullOrEmpty(itemSkillAutoController))
							{
								// Granted Skill name BOLD. TODO must implement extended ColorTag {^(?<Color>.)(?<Style>.)} to support this.
								//line = string.Concat("<b>", line, "</b>");
							}
						}

						line = $"{color?.ColorTag()}{line}";

						if (isGlobal)
							line = line.InsertAfterColorPrefix(globalIndent);

						// Indent formulae reagents
						if (itm.IsFormulae && normalizedFullAttribute.StartsWith("REAGENT", StringComparison.OrdinalIgnoreCase))
							line = line.InsertAfterColorPrefix(globalIndent);

						results.Add(line);
						itm.CurrentFriendlyNameResult.TmpAttrib.Add(line);
					}

					// Added by VillageIdiot
					// itm a special case for pet bonuses
					if (normalizedFullAttribute == "PETBONUSNAME")
					{
						string petBonusID = record.GetString("petBonusName", 0);
						DBRecordCollection petBonusRecord = Database.GetRecordFromFile(petBonusID);
						if (petBonusRecord != null)
						{
							var tmp = new List<string>();
							GetAttributesFromRecord(itm, petBonusRecord, true, petBonusID, tmp);
							results.AddRange(tmp.Select(s => s.InsertAfterColorPrefix(globalIndent)));
							results.Add(string.Empty);
						}
					}

					// Added by VillageIdiot
					// Another special case for skill description and effects of activated skills
					if (normalizedFullAttribute == "ITEMSKILLNAME" || (itm.IsScroll && normalizedFullAttribute == "SKILLNAME"))
						GetSkillDescriptionAndEffects(itm, record, results, variable, line);
				}
			}

			if (TQDebug.ItemDebugLevel > 0)
				Log.LogDebug("Exiting Item.ConvertOffenseAttrToString()");
		}

		/// <summary>
		/// Adds the formatted skill description and effects for granted skills to the results list
		/// </summary>
		/// <param name="record">DBRecord databse record</param>
		/// <param name="results">results list</param>
		/// <param name="variable">variable structure</param>
		/// <param name="line">line of text</param>
		private void GetSkillDescriptionAndEffects(Item itm, DBRecordCollection record, List<string> results, Variable variable, string line)
		{
			string autoController = record.GetString("itemSkillAutoController", 0);
			if (!string.IsNullOrEmpty(autoController) || itm.IsScroll)
			{
				DBRecordCollection skillRecord = Database.GetRecordFromFile(variable.GetString(0));

				// Changed by VillageIdiot
				// Get title from the last line
				// Remove the HTML formatting and use for word wrapping
				string lastline = string.Empty;
				if (!itm.IsScroll)
					lastline = autoController ?? string.Empty;

				// Set the minimum column width to 30
				// Also takes care of scrolls
				int lineLength = lastline.Length;
				if (lineLength < 30)
					lineLength = 30;

				// Show the description text first
				if (skillRecord != null)
				{
					string buffSkillName = skillRecord.GetString("buffSkillName", 0);

					if (!itm.IsScroll)
					{
						// Skip scrolls since they are handled elsewhere with the flavor text
						string descriptionTag, skillDescription;
						Collection<string> skillDescriptionList;

						// Changed by VillageIdiot
						// Let's actually test if it's a buff skill
						if (string.IsNullOrEmpty(buffSkillName))
						{
							descriptionTag = skillRecord.GetString("skillBaseDescription", 0);
							if (descriptionTag.Length != 0)
							{
								if (TranslationService.TryTranslateXTag(descriptionTag, out skillDescription))
								{
									skillDescriptionList = StringHelper.WrapWords(skillDescription, lineLength);

									foreach (string skillDescriptionFromList in skillDescriptionList)
									{
										var value = $"{ItemStyle.Mundane.TQColor().ColorTag()}    {skillDescriptionFromList}";
										results.Add(value);
										itm.CurrentFriendlyNameResult.TmpAttrib.Add(value);
									}

									// Show granted skill level
									if (Config.Settings.Default.ShowSkillLevel)
									{
										if (!TranslationService.TryTranslateXTag("MenuLevel", out var formatSpec))
											formatSpec = "Level:   {0}";
										else
											formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);

										int skillLevel = record.GetInt32("itemSkillLevel", 0);
										if (skillLevel > 0)
										{
											line = Format(formatSpec, skillLevel);
											var value = $"{ItemStyle.Mundane.TQColor().ColorTag()}{line}";
											results.Add(value);
											itm.CurrentFriendlyNameResult.TmpAttrib.Add(value);
										}
									}
								}
							}
						}
						else
						{
							// itm skill is a buff
							DBRecordCollection buffSkillRecord = Database.GetRecordFromFile(buffSkillName);
							if (buffSkillRecord != null)
							{
								descriptionTag = buffSkillRecord.GetString("skillBaseDescription", 0);
								if (!string.IsNullOrEmpty(descriptionTag))
								{
									skillDescription = TranslationService.TranslateXTag(descriptionTag);
									skillDescriptionList = StringHelper.WrapWords(skillDescription, lineLength);
									foreach (string skillDescriptionFromList in skillDescriptionList)
									{
										var value = $"{ItemStyle.Mundane.TQColor().ColorTag()}    {skillDescriptionFromList}";
										results.Add(value);
										itm.CurrentFriendlyNameResult.TmpAttrib.Add(value);
									}

									// Show granted skill level
									if (Config.Settings.Default.ShowSkillLevel)
									{
										if (!TranslationService.TryTranslateXTag("MenuLevel", out var formatSpec))
											formatSpec = "Level:   {0}";
										else
											formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);

										int skillLevel = record.GetInt32("itemSkillLevel", 0);
										if (skillLevel > 0)
										{
											line = Format(formatSpec, skillLevel);
											var value = $"{ItemStyle.Mundane.TQColor().ColorTag()}    {line}";
											results.Add(value);
											itm.CurrentFriendlyNameResult.TmpAttrib.Add(value);
										}
									}
								}
							}
						}
					}

					// Clear out effects for unnamed skills, unless it's a buff or a scroll.
					if (skillRecord.GetString("skillDisplayName", 0).Length == 0 && string.IsNullOrEmpty(buffSkillName) && !itm.IsScroll)
						skillRecord = null;

					// Added by VillageIdiot
					// Adjust for the flavor text of scrolls
					if (skillRecord != null && !itm.IsScroll)
						results.Add(string.Empty);

					// Added by VillageIdiot
					// Add the skill effects
					if (skillRecord != null)
					{
						// itm is a summon
						if (skillRecord.GetString("Class", 0).ToUpperInvariant().Equals("SKILL_SPAWNPET"))
							ConvertPetStats(itm, skillRecord, results);
						else
						{
							// Skill Effects
							if (!string.IsNullOrEmpty(buffSkillName))
								GetAttributesFromRecord(itm, Database.GetRecordFromFile(buffSkillName), true, buffSkillName, results);
							else
								GetAttributesFromRecord(itm, skillRecord, true, variable.GetString(0), results);
						}
					}
				}
			}
		}


		/// <summary>
		/// Used for showing the pet statistics
		/// </summary>
		/// <param name="skillRecord">DBRecord of the skill</param>
		/// <param name="results">List containing the results</param>
		private void ConvertPetStats(Item itm, DBRecordCollection skillRecord, List<string> results)
		{
			string formatSpec, petLine;
			int summonLimit = skillRecord.GetInt32("petLimit", 0);
			if (summonLimit > 1)
			{
				if (!TranslationService.TryTranslateXTag("SkillPetLimit", out formatSpec))
					formatSpec = "{0} Summon Limit";
				else
					formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);

				petLine = string.Format(CultureInfo.CurrentCulture, formatSpec, summonLimit.ToString(CultureInfo.CurrentCulture));
				var value = $"{ItemStyle.Mundane.TQColor().ColorTag()}{petLine}";
				results.Add(value);
				itm.CurrentFriendlyNameResult.TmpAttrib.Add(value);
			}

			DBRecordCollection petRecord = Database.GetRecordFromFile(skillRecord.GetString("spawnObjects", 0));
			if (petRecord != null)
			{
				// Print out Pet attributes
				if (!TranslationService.TryTranslateXTag("SkillPetDescriptionHeading", out formatSpec))
					formatSpec = "{0} Attributes:";
				else
					formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);

				string petNameTag = petRecord.GetString("description", 0);
				string petName = TranslationService.TranslateXTag(petNameTag);
				float value = 0.0F;

				petLine = string.Format(CultureInfo.CurrentCulture, formatSpec, petName);
				var valueStr = $"{ItemStyle.Mundane.TQColor().ColorTag()}{petLine}";
				results.Add(valueStr);
				itm.CurrentFriendlyNameResult.TmpAttrib.Add(valueStr);

				// Time to live
				if (!TranslationService.TryTranslateXTag("tagSkillPetTimeToLive", out formatSpec))
					formatSpec = "Life Time {0} Seconds";
				else
					formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);

				petLine = string.Format(CultureInfo.CurrentCulture, formatSpec, skillRecord.GetSingle("spawnObjectsTimeToLive", 0));
				valueStr = $"{ItemStyle.Mundane.TQColor().ColorTag()}{petLine}";
				results.Add(valueStr);
				itm.CurrentFriendlyNameResult.TmpAttrib.Add(valueStr);

				// Health
				value = petRecord.GetSingle("characterLife", 0);
				if (value != 0.0F)
				{
					if (!TranslationService.TryTranslateXTag("SkillPetDescriptionHealth", out formatSpec))
						formatSpec = "{0}  Health";
					else
						formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);

					petLine = string.Format(CultureInfo.CurrentCulture, formatSpec, value);
					valueStr = $"{ItemStyle.Mundane.TQColor().ColorTag()}{petLine}";
					results.Add(valueStr);
					itm.CurrentFriendlyNameResult.TmpAttrib.Add(valueStr);
				}

				// Energy
				value = petRecord.GetSingle("characterMana", 0);
				if (value != 0.0F)
				{
					if (!TranslationService.TryTranslateXTag("SkillPetDescriptionMana", out formatSpec))
						formatSpec = "{0}  Energy";
					else
						formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);

					petLine = string.Format(CultureInfo.CurrentCulture, formatSpec, value);
					valueStr = $"{ItemStyle.Mundane.TQColor().ColorTag()}{petLine}";
					results.Add(valueStr);
					itm.CurrentFriendlyNameResult.TmpAttrib.Add(valueStr);
				}

				// Add abilities text
				results.Add(string.Empty);
				if (!TranslationService.TryTranslateXTag("tagSkillPetAbilities", out formatSpec))
					formatSpec = "{0} Abilities:";
				else
					formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);

				petLine = string.Format(CultureInfo.CurrentCulture, formatSpec, petName);
				valueStr = $"{ItemStyle.Mundane.TQColor().ColorTag()}{petLine}";
				results.Add(valueStr);
				itm.CurrentFriendlyNameResult.TmpAttrib.Add(valueStr);

				// Show Physical Damage
				value = petRecord.GetSingle("handHitDamageMin", 0);
				float value2 = petRecord.GetSingle("handHitDamageMax", 0);

				if (value > 1.0F || value2 > 2.0F)
				{
					if (value2 == 0.0F || value == value2)
					{
						if (!TranslationService.TryTranslateXTag("SkillPetDescriptionDamageMinOnly", out formatSpec))
							formatSpec = "{0}  Damage";
						else
							formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);

						petLine = string.Format(CultureInfo.CurrentCulture, formatSpec, value);
						valueStr = $"{ItemStyle.Mundane.TQColor().ColorTag()}{petLine}";
						results.Add(valueStr);
						itm.CurrentFriendlyNameResult.TmpAttrib.Add(valueStr);
					}
					else
					{
						if (!TranslationService.TryTranslateXTag("SkillPetDescriptionDamageMinMax", out formatSpec))
							formatSpec = "{0} - {1}  Damage";
						else
							formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);

						petLine = string.Format(CultureInfo.CurrentCulture, formatSpec, value, value2);
						valueStr = $"{ItemStyle.Mundane.TQColor().ColorTag()}{petLine}";
						results.Add(valueStr);
						itm.CurrentFriendlyNameResult.TmpAttrib.Add(valueStr);
					}
				}

				// Show the pet's skills
				string[] skills = new string[17];  // Upto 17 skills in the tree
				int[] skillLevels = new int[17];
				int numSkills = 0;
				string petskillName;
				int petskillLevel;

				for (int i = 0; i < skills.Length; i++)
				{
					petskillName = petRecord.GetString(string.Concat("skillName", i), 0);
					if (string.IsNullOrEmpty(petskillName))
						continue;

					skills[numSkills] = petskillName;
					petskillLevel = petRecord.GetInt32(string.Concat("skillLevel", i), 0);
					if (petskillLevel < 1)
						petskillLevel = 1;

					skillLevels[numSkills] = petskillLevel;
					numSkills++;
				}

				for (int i = 0; i < numSkills; i++)
				{
					if (skills[i] != null && !skills[i].ToLower().StartsWith("records"))
						continue;

					DBRecordCollection skillRecord1 = Database.GetRecordFromFile(skills[i]);
					DBRecordCollection record = null;
					string skillClass = skillRecord1.GetString("Class", 0);

					// Skip passive skills
					if (skillClass.ToUpperInvariant() == "SKILL_PASSIVE")
						continue;

					string skillNameTag = null;
					string skillName = null;
					string recordID = null;
					string buffSkillName = skillRecord1.GetString("buffSkillName", 0);

					if (string.IsNullOrEmpty(buffSkillName))
					{
						record = skillRecord1;
						recordID = skills[i];
						skillNameTag = skillRecord.GetString("skillDisplayName", 0);
						if (!string.IsNullOrWhiteSpace(skillNameTag))
							TranslationService.TryTranslateXTag(skillNameTag, out skillName);
					}
					else
					{
						// This is a buff skill
						DBRecordCollection buffSkillRecord = Database.GetRecordFromFile(buffSkillName);
						if (buffSkillRecord != null)
						{
							record = buffSkillRecord;
							recordID = buffSkillName;
							skillNameTag = buffSkillRecord.GetString("skillDisplayName", 0);
							if (!string.IsNullOrWhiteSpace(skillNameTag))
								TranslationService.TryTranslateXTag(skillNameTag, out skillName);
						}
					}

					if (string.IsNullOrWhiteSpace(skillName))
						valueStr = $"{ItemStyle.Legendary.TQColor().ColorTag()}{skillNameTag}";
					else
						valueStr = $"{ItemStyle.Mundane.TQColor().ColorTag()}{skillName}";

					results.Add(valueStr);
					itm.CurrentFriendlyNameResult.TmpAttrib.Add(valueStr);

					GetAttributesFromRecord(itm, record, true, recordID, results);
					results.Add(string.Empty);
				}
			}
		}


		/// <summary>
		/// Gets the item label from the tag
		/// </summary>
		/// <param name="data">ItemAttributesData structure for the attribute</param>
		/// <param name="recordId">string containing the database record id</param>
		/// <param name="labelTag">the label tag</param>
		/// <param name="labelColor">the label color which gets modified here</param>
		/// <returns>string containing the label.</returns>
		private string GetLabelAndColorFromTag(Item itm, ItemAttributesData data, string recordId, ref string labelTag, ref TQColor? labelColor)
		{
			labelTag = ItemAttributeProvider.GetAttributeTextTag(data);
			string label, TrailingNL = string.Empty;

			if (string.IsNullOrEmpty(labelTag))
			{
				labelTag = string.Concat("?", data.FullAttribute, "?");
				labelColor = ItemStyle.Legendary.TQColor();
			}

			// if itm is an Armor effect and we are not armor, then change it to bonus
			if (labelTag.ToUpperInvariant().Equals("DEFENSEABSORPTIONPROTECTION"))
			{
				if (!itm.IsArmor || recordId != itm.BaseItemId)
				{
					labelTag = "DefenseAbsorptionProtectionBonus";
					labelColor = ItemStyle.Epic.TQColor();
				}
				else
				{
					// regular armor attribute is not magical
					labelColor = ItemStyle.Mundane.TQColor();
					// Add trailing '\n' + space for regular armor pieces to force empty row in tooltip (only if is first attribute)
					if (!itm.CurrentFriendlyNameResult.TmpAttrib.Any())
						TrailingNL = StringHelper.TQNewLineTag + ' ';
				}
			}

			if (!TranslationService.TryTranslateXTag(labelTag, out label))
			{
				label = string.Concat("?", labelTag, "?");
				labelColor = ItemStyle.Legendary.TQColor();
			}

			return $"{label}{TrailingNL}";
		}


		/// <summary>
		/// For displaying raw attribute data
		/// </summary>
		/// <param name="attributeList">ArrayList containing the arributes</param>
		/// <param name="results">List containing the attribute strings.</param>
		private void ConvertBareAttributeListToString(List<Variable> attributeList, List<string> results)
		{
			foreach (Variable variable in attributeList)
			{
				if (variable != null)
					results.Add(variable.ToString());
			}
		}

		/// <summary>
		/// Gets the item's requirements
		/// </summary>
		/// <returns>A string containing the items requirements</returns>
		public (string[] Requirements, SortedList<string, Variable> RequirementVariables) GetRequirements(Item itm)
		{
			SortedList<string, Variable> requirementVariables = GetRequirementVariables(itm);

			// Get the format string to use to list a requirement
			if (!TranslationService.TryTranslateXTag("MeetsRequirement", out var requirementFormat))
				requirementFormat = "?Required? {0}: {1:f0}";
			else
				requirementFormat = ItemAttributeProvider.ConvertFormat(requirementFormat);

			// Now combine it all with spaces between
			List<string> requirements = new List<string>();
			foreach (KeyValuePair<string, Variable> kvp in requirementVariables)
			{
				if (TQDebug.ItemDebugLevel > 1)
					Log.LogDebug("Retrieving requirement {0}={1} (type={2})", kvp.Key, kvp.Value, kvp.Value.GetType().ToString());

				Variable variable = kvp.Value;

				// Format the requirement
				string requirementsText;
				if (variable.NumberOfValues > 1 || variable.DataType == VariableDataType.StringVar)
				{
					// reqs should only have 1 entry and should be a number type.  We must punt on itm one
					requirementsText = string.Concat(kvp.Key, ": ", variable.ToStringValue());
				}
				else
				{
					// get the name of itm requirement
					if (!TranslationService.TryTranslateXTag(kvp.Key, out var reqName))
						reqName = string.Concat("?", kvp.Key, "?");


					// Now apply the format string
					requirementsText = Format(requirementFormat, reqName, variable[0]);
				}

				// Changed by VillageIdiot - Change requirement text to Grey
				requirements.Add(requirementsText);
			}

			return (requirements.ToArray(), requirementVariables);
		}
	}
}

