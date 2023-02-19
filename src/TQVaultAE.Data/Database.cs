//-----------------------------------------------------------------------
// <copyright file="Database.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TQVaultAE.Config;
using TQVaultAE.Domain.Contracts.Providers;
using TQVaultAE.Domain.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Logs;

namespace TQVaultAE.Data;

/// <summary>
/// Reads a Titan Quest database file.
/// </summary>
public class Database : IDatabase
{

	private const StringComparison noCase = StringComparison.OrdinalIgnoreCase;

	#region Record Class Names

	internal const string RCLASS_LOOTRANDOMIZERTABLE = "LootRandomizerTable";
	internal const string RCLASS_LOOTRANDOMIZER = "LootRandomizer";
	internal const string RCLASS_LOOTITEMTABLE_FIXEDWEIGHT = "LootItemTable_FixedWeight";
	internal const string RCLASS_LOOTITEMTABLE_DYNWEIGHT = "LootItemTable_DynWeight";

	#endregion

	private readonly ILogger Log = null;

	#region Database Fields

	/// <summary>
	/// Dictionary of all database info records
	/// </summary>
	private LazyConcurrentDictionary<RecordId, Info> infoDB = new();

	/// <summary>
	/// Dictionary of all text database entries
	/// </summary>
	private ConcurrentDictionary<string, string> textDB = new();

	/// <summary>
	/// Dictionary of all associated arc files in the database.
	/// </summary>
	private LazyConcurrentDictionary<string, ArcFile> arcFiles = new();

	/// <summary>
	/// Dictionary of all records dataset loaded from the database.
	/// </summary>
	private LazyConcurrentDictionary<RecordId, byte[]> resourcesData = new();

	/// <summary>
	/// Dictionary of all record collections loaded from the database.
	/// </summary>
	private LazyConcurrentDictionary<RecordId, DBRecordCollection> dbRecordCollections = new();

	/// <summary>
	/// Game language to support setting language in UI
	/// </summary>
	private string gameLanguage;

	private readonly IArcFileProvider arcProv;
	private readonly IArzFileProvider arzProv;
	private readonly IItemAttributeProvider ItemAttributeProvider;
	private readonly IGamePathService GamePathResolver;
	private readonly ITQDataService TQData;

	#endregion Database Fields

	/// <summary>
	/// Initializes a new instance of the Database class.
	/// </summary>
	public Database(
		ILogger<Database> log
		, IArcFileProvider arcFileProvider
		, IArzFileProvider arzFileProvider
		, IItemAttributeProvider itemAttributeProvider
		, IGamePathService gamePathResolver
		, ITQDataService tQData
	)
	{
		this.Log = log;
		this.AutoDetectLanguage = Config.UserSettings.Default.AutoDetectLanguage;
		this.TQLanguage = Config.UserSettings.Default.TQLanguage;
		this.arcProv = arcFileProvider;
		this.arzProv = arzFileProvider;
		this.ItemAttributeProvider = itemAttributeProvider;
		this.GamePathResolver = gamePathResolver;
		this.TQData = tQData;
		this.LoadDBFile();
	}

	#region Database Properties

	ReadOnlyDictionary<RecordId, DBRecordCollection> _LootRandomizerTableList;
	public ReadOnlyDictionary<RecordId, DBRecordCollection> AllLootRandomizerTable
	{
		get
		{
			if (_LootRandomizerTableList is null)
				_LootRandomizerTableList = ReadAllLootRandomizerTable();

			return _LootRandomizerTableList;
		}
	}

	ReadOnlyDictionary<RecordId, LootRandomizerItem> _LootRandomizerList;
	public ReadOnlyDictionary<RecordId, LootRandomizerItem> AllLootRandomizer
	{
		get
		{
			if (_LootRandomizerList is null)
				_LootRandomizerList = ReadAllLootRandomizer();

			return _LootRandomizerList;
		}
	}


	/// <summary>
	/// Mapping between ItemId and Affixes LootTable
	/// </summary>
	private static ReadOnlyDictionary<RecordId, ReadOnlyCollection<AffixTableMapItem>> ItemAffixTableMap;

	/// <summary>
	/// Gets or sets a value indicating whether the game language is being auto detected.
	/// </summary>
	public bool AutoDetectLanguage { get; set; }

	/// <summary>
	/// Gets or sets the game language from the config file.
	/// </summary>
	public string TQLanguage { get; set; }


	/// <summary>
	/// Gets the instance of the Titan Quest Database ArzFile.
	/// </summary>
	public ArzFile ArzFile { get; private set; }

	/// <summary>
	/// Gets the instance of the Immortal Throne Database ArzFile.
	/// </summary>
	public ArzFile ArzFileIT { get; private set; }

	/// <summary>
	/// Gets the instance of a custom map Database ArzFile.
	/// </summary>
	public ArzFile ArzFileMod { get; private set; }

	/// <summary>
	/// Gets the game language setting as a an English DisplayName.
	/// </summary>
	/// <remarks>Changed to property by VillageIdiot to support changing of Language in UI</remarks>
	public string GameLanguage
	{
		get
		{
			// Added by VillageIdiot
			// Check if the user configured the language
			if (this.gameLanguage == null)
			{
				if (!this.AutoDetectLanguage)
					this.gameLanguage = this.TQLanguage;
			}

			// Try to read the language from the settings file
			if (string.IsNullOrEmpty(this.gameLanguage))
			{
				try
				{
					string optionsFile = GamePathResolver.SettingsFileTQ;
					if (!File.Exists(optionsFile))
					{
						// Try IT Folder if there is no settings file in TQ Folder
						optionsFile = GamePathResolver.SettingsFileTQIT;
					}
					if (File.Exists(optionsFile))
					{
						var fileContent = File.ReadAllText(optionsFile);
						var match = Regex.Match(fileContent, @"(?i)language\s*=\s*(""(?<Language>[^""]+)""|(?<Language>[^\r\n]*))[\r\n]");
						if (match.Success)
						{
							this.gameLanguage = match.Groups["Language"].Value.ToUpperInvariant();
							return this.gameLanguage;
						}

						return null;
					}

					return null;
				}
				catch (IOException exception)
				{
					Log.ErrorException(exception);
					return null;
				}
			}

			// Added by VillageIdiot
			// We have something so we need to return it
			// This was added to support setting the language in the config file
			return this.gameLanguage;
		}
	}

	#endregion Database Properties

	#region Database Public Methods

	#region Database Public Static Methods

	/// <summary>
	/// Used to Extract an ARC file into the destination directory.
	/// The ARC file will not be added to the cache.
	/// </summary>
	/// <remarks>Added by VillageIdiot</remarks>
	/// <param name="arcFileName">Name of the arc file</param>
	/// <param name="destination">Destination path for extracted data</param>
	/// <returns>Returns true on success otherwise false</returns>
	public bool ExtractArcFile(string arcFileName, string destination)
	{
		bool result = false;

		if (TQDebug.DatabaseDebugLevel > 0)
			Log.LogDebug("Database.ExtractARCFile('{0}', '{1}')", arcFileName, destination);

		try
		{
			ArcFile arcFile = new ArcFile(arcFileName);

			// Extract the files
			result = arcProv.ExtractArcFile(arcFile, destination);
		}
		catch (IOException exception)
		{
			Log.LogError(exception, "Exception occurred");
			result = false;
		}

		if (TQDebug.DatabaseDebugLevel > 1)
			Log.LogDebug("Extraction Result = {0}", result);

		if (TQDebug.DatabaseDebugLevel > 0)
			Log.LogDebug("Exiting Database.ReadARCFile()");

		return result;
	}

	#endregion Database Public Static Methods

	/// <summary>
	/// Gets the Infor for a specific item id.
	/// </summary>
	/// <param name="itemId">Item ID which we are looking up.  Will be normalized internally.</param>
	/// <returns>Returns Infor for item ID and NULL if not found.</returns>
	public Info GetInfo(RecordId itemId)
	{
		if (itemId is null)
			return null;

		return this.infoDB.GetOrAddAtomic(itemId, k =>
		{
			DBRecordCollection record = GetRecordFromFile(k);

			if (record == null)
				return null;

			return new Info(record);
		});
	}

	/// <summary>
	/// Uses the text database to convert the tag to a name in the localized language.
	/// The tag is normalized to upper case internally.
	/// </summary>
	/// <param name="tagId">Tag to be looked up in the text database normalized to upper case.</param>
	/// <returns>Returns localized string, empty string if it cannot find a string or "?ErrorName?" in case of uncaught exception.</returns>
	public string GetFriendlyName(string tagId)
		=> this.textDB.TryGetValue(tagId.ToUpperInvariant(), out var text) ? text : string.Empty;

	/// <summary>
	/// Gets the formatted string for the variable attribute.
	/// </summary>
	/// <param name="variable">variable for which we are making a nice string.</param>
	/// <returns>Formatted string in the format of:  Attribute: value</returns>
	public string VariableToStringNice(Variable variable)
		=> $"{this.GetItemAttributeFriendlyText(variable.Name)}: {variable.ToStringValue()}";

	/// <summary>
	/// Converts the item attribute to a name in the localized language
	/// </summary>
	/// <param name="itemAttribute">Item attribure to be looked up.</param>
	/// <param name="addVariable">Flag for whether the variable is added to the text string.</param>
	/// <returns>Returns localized item attribute</returns>
	public string GetItemAttributeFriendlyText(string itemAttribute, bool addVariable = true)
	{
		ItemAttributesData data = ItemAttributeProvider.GetAttributeData(itemAttribute);
		if (data == null)
		{
			this.Log.LogDebug($"Attribute unknown : {itemAttribute}");
			return string.Concat("?", itemAttribute, "?");
		}

		string attributeTextTag = ItemAttributeProvider.GetAttributeTextTag(data);
		if (string.IsNullOrEmpty(attributeTextTag))
		{
			this.Log.LogDebug($"Attribute unknown : {itemAttribute}");
			return string.Concat("?", itemAttribute, "?");
		}

		string textFromTag = this.GetFriendlyName(attributeTextTag);
		if (string.IsNullOrEmpty(textFromTag))
		{
			textFromTag = string.Concat("ATTR<", itemAttribute, "> TAG<");
			textFromTag = string.Concat(textFromTag, attributeTextTag, ">");
		}

		if (addVariable && data.Variable.Length > 0)
			textFromTag = string.Concat(textFromTag, " ", data.Variable);

		return textFromTag;
	}

	/// <summary>
	/// Load our data from the db file.
	/// </summary>
	private void LoadDBFile()
	{
		this.LoadTextDB();
		this.LoadARZFile();
		this.BuildItemAffixTableMap();
	}

	/// <summary>
	/// Extract all loot randomizer table (Affix effect infos)
	/// </summary>
	/// <returns></returns>
	private ReadOnlyDictionary<RecordId, DBRecordCollection> ReadAllLootRandomizerTable()
	{
		// Load all available loot randomizer table
		var lootRandomizerTableList = new[] {
			(Priority: 0, ArzFile: this.ArzFileMod),
			(Priority: 1, ArzFile: this.ArzFile)
		}
		.Where(db => db.ArzFile is not null)
		.SelectMany(db =>
			db.ArzFile.RecordInfo.Where(r =>
				r.Value.RecordType.Equals(RCLASS_LOOTRANDOMIZERTABLE, noCase)
				&& !r.Key.IsOld // Remove old loot table
			).Select(ri => (RecordInfo: ri, db.Priority))
		)
		.Select(r =>
		{
			var DBRecords = GetRecordFromFile(r.RecordInfo.Key);

			return (RecordInfoKey: r.RecordInfo.Key, DBRecords, r.Priority);
		})
		.GroupBy(r => r.RecordInfoKey)
		.Select(grp =>
			(
				RecordInfoKey: grp.Key,
				grp.OrderBy(v => v.Priority).First().DBRecords // Promote Mod records over base game
			)
		)
		.ToDictionary(r => r.RecordInfoKey, r => r.DBRecords);

		return new ReadOnlyDictionary<RecordId, DBRecordCollection>(lootRandomizerTableList);
	}


	/// <summary>
	/// Extract all LootRandomizer
	/// </summary>
	/// <returns></returns>
	private ReadOnlyDictionary<RecordId, LootRandomizerItem> ReadAllLootRandomizer()
	{
		// Load all available loot randomizer
		var lootRandomizerList = new[] {
			(Priority: 0, ArzFile: this.ArzFileMod),
			(Priority: 1, ArzFile: this.ArzFile)
		}
		.Where(db => db.ArzFile is not null)
		.SelectMany(db =>
			db.ArzFile.RecordInfo.Where(r => r.Value.RecordType.Equals(RCLASS_LOOTRANDOMIZER, noCase))
			.Select(ri => (RecordInfo: ri, db.Priority))
		)
		.Select(r =>
			{
				var rec = GetRecordFromFile(r.RecordInfo.Key);

				string Tag = rec.GetString(Variable.KEY_LOOTRANDNAME, 0);
				int Cost = rec.GetInt32(Variable.KEY_LOOTRANDCOST, 0);
				int LevelRequirement = rec.GetInt32(Variable.KEY_LEVELREQ, 0);
				string ItemClass = rec.GetString(Variable.KEY_ITEMCLASS, 0);
				string FileDescription = rec.GetString(Variable.KEY_FILEDESC, 0);

				var val = new LootRandomizerItem(
					r.RecordInfo.Key
					, Tag
					, Cost
					, LevelRequirement
					, ItemClass
					, FileDescription
					, r.RecordInfo.Key.PrettyFileName
				);

				return (RecordInfoKey: r.RecordInfo.Key, LootRandomizerItem: val, r.Priority);
			}
		)
		.GroupBy(r => r.RecordInfoKey)
		.Select(grp =>
			(
				RecordInfoKey: grp.Key,
				grp.OrderBy(v => v.Priority).First().LootRandomizerItem // Promote Mod records over base game
			)
		)
		.ToDictionary(r => r.RecordInfoKey, r => r.LootRandomizerItem);

		return new ReadOnlyDictionary<RecordId, LootRandomizerItem>(lootRandomizerList);
	}

	#region ItemAffixTableMap


	/// <summary>
	/// Create a map between Items and affix loot tables
	/// </summary>
	private void BuildItemAffixTableMap()
	{
		// Load all available loot table
		var data = new[] { this.ArzFileMod, this.ArzFile }
			.Where(db => db is not null)
			.SelectMany(db => db.RecordInfo
				.Where(r =>
					r.Value.RecordType.Equals(RCLASS_LOOTITEMTABLE_FIXEDWEIGHT, noCase)
					|| r.Value.RecordType.Equals(RCLASS_LOOTITEMTABLE_DYNWEIGHT, noCase)
				)
			)
			.Select(r =>
			{
				var rec = GetRecordFromFile(r.Key);
				var lootNames = new List<RecordId>();
				var records = new List<(RecordId brokenTable, RecordId prefixTable, RecordId suffixTable, List<RecordId> lootNames)>();

				// Read loot names
				switch (r.Value.RecordType)
				{
					case RCLASS_LOOTITEMTABLE_FIXEDWEIGHT:
						foreach (var variable in rec)
						{
							if (variable.Name.StartsWith("lootName", noCase))
							{
								var lootName = variable.GetString(0);

								if (!string.IsNullOrWhiteSpace(lootName))
									lootNames.Add(lootName);
							}
						}
						break;
					default: // LootItemTable_DynWeight
						var itemNames = rec.GetAllStrings("itemNames")?.Where(ina => !string.IsNullOrWhiteSpace(ina));
						if (itemNames?.Any() ?? false)
							lootNames.AddRange(itemNames.Select(ina => ina.ToRecordId()));
						break;
				}

				// Read loot tables
				Dictionary<string, (RecordId brokenRandomizerName, RecordId prefixRandomizerName, RecordId suffixRandomizerName)> dico = new();
				foreach (var variable in rec)
				{
					var varname = "brokenRandomizerName";
					if (variable.Name.StartsWith(varname, noCase))
					{
						var brokenTable = variable.GetString(0);
						if (!string.IsNullOrWhiteSpace(brokenTable))
						{
							var num = variable.Name.Substring(varname.Length);

							if (dico.Keys.Contains(num))
							{// ValueTuple are not ref
								var val = dico[num];
								val.brokenRandomizerName = brokenTable;
								dico[num] = val;
							}
							else
								dico.Add(num, (brokenTable, RecordId.Empty, RecordId.Empty));
						}
						continue;
					}

					varname = "prefixRandomizerName";
					if (variable.Name.StartsWith(varname, noCase))
					{
						var prefixTable = variable.GetString(0);
						if (!string.IsNullOrWhiteSpace(prefixTable))
						{
							var num = variable.Name.Substring(varname.Length);

							if (dico.Keys.Contains(num))
							{// ValueTuple are not ref
								var val = dico[num];
								val.prefixRandomizerName = prefixTable;
								dico[num] = val;
							}
							else
								dico.Add(num, (RecordId.Empty, prefixTable, RecordId.Empty));
						}
						continue;
					}

					varname = "suffixRandomizerName";
					if (variable.Name.StartsWith(varname, noCase))
					{
						var suffixTable = variable.GetString(0);
						if (!string.IsNullOrWhiteSpace(suffixTable))
						{
							var num = variable.Name.Substring(varname.Length);

							if (dico.Keys.Contains(num))
							{// ValueTuple are not ref
								var val = dico[num];
								val.suffixRandomizerName = suffixTable;
								dico[num] = val;
							}
							else
								dico.Add(num, (RecordId.Empty, RecordId.Empty, suffixTable));
						}
					}
				}

				// Avoid useless records
				if (lootNames.Count > 0 && dico.Count > 0)
				{
					records.AddRange(dico.Select(kv =>
						(
							kv.Value.brokenRandomizerName,
							kv.Value.prefixRandomizerName,
							kv.Value.suffixRandomizerName,
							lootNames
						)
					));
				}

				return records;
			})
			.SelectMany(i => i)
			.SelectMany(itemAffix => itemAffix.lootNames
				.Select(itemId => new
				{
					itemId = itemId,
					itemAffix.suffixTable,
					itemAffix.prefixTable,
					itemAffix.brokenTable
				})
			) // Flatten
			.GroupBy(i => i.itemId)
			.ToDictionary(i =>
				i.Key
				, j => j
					.Select(k => new AffixTableMapItem(k.brokenTable, k.prefixTable, k.suffixTable)).Distinct()
					.ToList().AsReadOnly()
			);

		ItemAffixTableMap = new ReadOnlyDictionary<RecordId, ReadOnlyCollection<AffixTableMapItem>>(data);
	}

	public ReadOnlyCollection<AffixTableMapItem> GetItemAffixTableMap(RecordId itemId)
	{
		var affixmap = ItemAffixTableMap.SingleOrDefault(i => i.Key == itemId);
		if (affixmap.Key is null) return null;

		return affixmap.Value;
	}

	#endregion

	/// <summary>
	/// Gets a DBRecord for the specified item ID string.
	/// </summary>
	/// <remarks>
	/// Changed by VillageIdiot
	/// Changed search order so that IT records have precedence of TQ records.
	/// Add Custom Map database.  Custom Map records have precedence over IT records.
	/// </remarks>
	/// <param name="itemId">Item Id which we are looking up</param>
	/// <returns>Returns the DBRecord for the item Id</returns>
	public DBRecordCollection GetRecordFromFile(RecordId itemId)
	{
		var cachedDBRecordCollection = this.dbRecordCollections.GetOrAddAtomic(itemId, key =>
		{

			if (this.ArzFileMod != null)
			{
				DBRecordCollection recordMod = arzProv.GetItem(this.ArzFileMod, key);
				if (recordMod != null)
				{
					// Custom Map records have highest precedence.
					return recordMod;
				}
			}

			if (this.ArzFileIT != null)
			{
				// see if it's in IT ARZ file
				DBRecordCollection recordIT = arzProv.GetItem(this.ArzFileIT, key);
				if (recordIT != null)
				{
					// IT file takes precedence over TQ.
					return recordIT;
				}
			}

			return arzProv.GetItem(ArzFile, key);
		});

		return cachedDBRecordCollection;
	}


	/// <summary>
	/// Gets a resource from the database using the resource Id.
	/// Modified by VillageIdiot to support loading resources from a custom map folder.
	/// </summary>
	/// <param name="resourceId">Resource which we are fetching</param>
	/// <returns>Retruns a byte array of the resource.</returns>
	public byte[] LoadResource(RecordId resourceId)
	{
		if (RecordId.IsNullOrEmpty(resourceId))
			return null;

		if (TQDebug.DatabaseDebugLevel > 0)
			Log.LogDebug("Database.LoadResource({0})", resourceId);

		if (TQDebug.DatabaseDebugLevel > 1)
			Log.LogDebug(" Normalized({0})", resourceId);

		byte[] cachedArcFileData = this.resourcesData.GetOrAddAtomic(resourceId, key =>
		{
			var resourceIdSplited = key.Normalized.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);// hguy : easier to understand than substring everywhere

			// not a proper resourceID.
			if (resourceIdSplited.Length == 1)
				return null;

			// First we need to figure out the correct file to
			// open, by grabbing it off the front of the resourceID

			string arcFile; bool isDLC = false;
			string rootFolder;
			byte[] arcFileData = null;
			string arcFileBase = resourceIdSplited.First();

			if (TQDebug.DatabaseDebugLevel > 1)
				Log.LogDebug("arcFileBase = {0}", arcFileBase);

			// Added by VillageIdiot
			// Check the mod folder for the image resource.
			if (GamePathResolver.IsCustom)
			{
				if (TQDebug.DatabaseDebugLevel > 1)
					Log.LogDebug("Checking Custom Resources.");

				rootFolder = Path.Combine(GamePathResolver.MapName, "resources");

				arcFile = Path.Combine(rootFolder, Path.ChangeExtension(arcFileBase, ".arc"));
				arcFileData = this.ReadARCFile(arcFile, key);

				if (TQDebug.DatabaseDebugLevel > 1 && arcFileData is not null)
					Log.LogDebug(@"Custom resource found ""{resourceId}"" into ""{arcFile}""", key, arcFile);
			}

			// We either didn't load the resource or didn't find what we were looking for so check the normal game resources.
			if (arcFileData == null)
			{
				// See if this guy is from Immortal Throne expansion pack.
				if (TQDebug.DatabaseDebugLevel > 1)
					Log.LogDebug("Checking IT Resources.");

				(arcFile, isDLC) = this.GamePathResolver.ResolveArcFileName(key);
				if (isDLC)
				{
					// not a proper resourceID.
					if (resourceIdSplited.Length == 2)
						return null;

					arcFileBase = resourceIdSplited[1];
					key = resourceIdSplited.Skip(1).JoinString("\\");
				}

				arcFileData = this.ReadARCFile(arcFile, key);

				if (TQDebug.DatabaseDebugLevel > 0 && arcFileData is null)
					Log.LogError(@"Resource not found ""{resourceId}"" into ""{arcFile}""", key, arcFile);
			}

			#region Fallback : It looks like we never go there

			// Added by VillageIdiot
			// Maybe the arc file is in the XPack folder even though the record does not state it.
			// Also could be that it says xpack in the record but the file is in the root.
			if (arcFileData == null)
			{
				rootFolder = Path.Combine(GamePathResolver.GamePathTQIT, "Resources", "XPack");
				arcFile = Path.Combine(rootFolder, Path.ChangeExtension(arcFileBase, ".arc"));
				arcFileData = this.ReadARCFile(arcFile, key);

				if (TQDebug.DatabaseDebugLevel > 1 && arcFileData is not null)
					Log.LogError(@"Resource misplaced ""{resourceId}"" into ""{arcFile}""", key, arcFile);
			}

			// Now, let's check if the item is in Ragnarok DLC
			if (arcFileData == null && GamePathResolver.IsRagnarokInstalled)
			{
				rootFolder = Path.Combine(GamePathResolver.GamePathTQIT, "Resources", "XPack2");
				arcFile = Path.Combine(rootFolder, Path.ChangeExtension(arcFileBase, ".arc"));
				arcFileData = this.ReadARCFile(arcFile, key);

				if (TQDebug.DatabaseDebugLevel > 1 && arcFileData is not null)
					Log.LogError(@"Resource misplaced ""{resourceId}"" into ""{arcFile}""", key, arcFile);
			}

			if (arcFileData == null && GamePathResolver.IsAtlantisInstalled)
			{
				rootFolder = Path.Combine(GamePathResolver.GamePathTQIT, "Resources", "XPack3");
				arcFile = Path.Combine(rootFolder, Path.ChangeExtension(arcFileBase, ".arc"));
				arcFileData = this.ReadARCFile(arcFile, key);

				if (TQDebug.DatabaseDebugLevel > 1 && arcFileData is not null)
					Log.LogError(@"Resource misplaced ""{resourceId}"" into ""{arcFile}""", key, arcFile);
			}

			if (arcFileData == null && GamePathResolver.IsEmbersInstalled)
			{
				rootFolder = Path.Combine(GamePathResolver.GamePathTQIT, "Resources", "XPack4");
				arcFile = Path.Combine(rootFolder, Path.ChangeExtension(arcFileBase, ".arc"));
				arcFileData = this.ReadARCFile(arcFile, key);

				if (TQDebug.DatabaseDebugLevel > 1 && arcFileData is not null)
					Log.LogError(@"Resource misplaced ""{resourceId}"" into ""{arcFile}""", key, arcFile);
			}

			if (arcFileData == null)
			{
				// We are either vanilla TQ or have not found our resource yet.
				// from the original TQ folder
				if (TQDebug.DatabaseDebugLevel > 1)
					Log.LogDebug("Checking TQ Resources.");

				rootFolder = GamePathResolver.GamePathTQ;
				rootFolder = Path.Combine(rootFolder, "Resources");

				arcFile = Path.Combine(rootFolder, Path.ChangeExtension(arcFileBase, ".arc"));
				arcFileData = this.ReadARCFile(arcFile, key);

				if (TQDebug.DatabaseDebugLevel > 0 && arcFileData is null)
					Log.LogError(@"Resource unknown ""{resourceId}""", key);
			}

			#endregion

			return arcFileData;
		});

		if (TQDebug.DatabaseDebugLevel > 0)
			Log.LogDebug("Exiting Database.LoadResource()");

		return cachedArcFileData;
	}


	#endregion Database Public Methods

	#region Database Private Methods


	/// <summary>
	/// Reads data from an ARC file and puts it into a Byte array
	/// </summary>
	/// <param name="arcFileName">Name of the arc file.</param>
	/// <param name="dataId">Id of data which we are getting from the arc file</param>
	/// <returns>Byte array of the data from the arc file.</returns>
	private byte[] ReadARCFile(string arcFileName, RecordId dataId)
	{
		// See if we have this arcfile already and if not create it.
		try
		{
			if (TQDebug.DatabaseDebugLevel > 0)
				Log.LogDebug("Database.ReadARCFile('{0}', '{1}')", arcFileName, dataId);

			ArcFile arcFile = ReadARCFile(arcFileName);

			if (arcFile is null)
				return null;

			// Now retrieve the data
			byte[] ans = arcProv.GetData(arcFile, dataId);

			if (TQDebug.DatabaseDebugLevel > 0)
				Log.LogDebug("Exiting Database.ReadARCFile()");

			return ans;
		}
		catch (Exception e)
		{
			Log.LogError(e, "Exception occurred");
			throw;
		}
	}

	/// <summary>
	/// Read ARC file
	/// </summary>
	/// <param name="arcFileName"></param>
	/// <returns></returns>
	public ArcFile ReadARCFile(string arcFileName)
	{
		// See if we have this arcfile already and if not create it.
		ArcFile arcFile = this.arcFiles.GetOrAddAtomic(arcFileName, k =>
		{
			if (!File.Exists(k))
				return null;

			var file = new ArcFile(k);
			arcProv.ReadARCToC(file);// Heavy lifting in GetOrAddAtomic
			return file;
		});

		return arcFile;
	}

	/// <summary>
	/// Tries to determine the name of the text database file.
	/// This is based on the game language and the UI language.
	/// Will use English if all else fails.
	/// </summary>
	/// <param name="isImmortalThrone">Signals whether we are looking for Immortal Throne files or vanilla Titan Quest files.</param>
	/// <returns>Path to the text db file</returns>
	private string FigureDBFileToUse(bool isImmortalThrone)
	{
		if (TQDebug.DatabaseDebugLevel > 0)
			Log.LogDebug("Database.FigureDBFileToUse({0})", isImmortalThrone);

		string rootFolder;
		if (isImmortalThrone)
		{
			if (GamePathResolver.GamePathTQIT.Contains("Anniversary"))
				rootFolder = Path.Combine(GamePathResolver.GamePathTQIT, "Text");
			else
				rootFolder = Path.Combine(GamePathResolver.GamePathTQIT, "Resources");

			if (TQDebug.DatabaseDebugLevel > 1)
			{
				Log.LogDebug("Detecting Immortal Throne text files");
				Log.LogDebug("rootFolder = {0}", rootFolder);
			}
		}
		else
		{
			// from the original TQ folder
			rootFolder = Path.Combine(GamePathResolver.GamePathTQ, "Text");

			if (TQDebug.DatabaseDebugLevel > 1)
			{
				Log.LogDebug("Detecting Titan Quest text files");
				Log.LogDebug("rootFolder = {0}", rootFolder);
			}
		}

		// make sure the damn directory exists
		if (!Directory.Exists(rootFolder))
		{
			if (TQDebug.DatabaseDebugLevel > 0)
				Log.LogDebug("Error - Root Folder does not exist");

			return null; // silently fail
		}

		string baseFile = Path.Combine(rootFolder, "Text_");
		string suffix = ".arc";

		// Added explicit set to null though may not be needed
		string cultureID = null;

		// Moved this declaration since the first use is inside of the loop.
		string filename = null;

		// First see if we can use the game setting
		string gameLanguage = this.GameLanguage;
		if (TQDebug.DatabaseDebugLevel > 1)
		{
			Log.LogDebug("gameLanguage = {0}", gameLanguage == null ? "NULL" : gameLanguage);
			Log.LogDebug("baseFile = {0}", baseFile);
		}

		if (gameLanguage != null)
		{
			// Try this method of getting the culture
			if (TQDebug.DatabaseDebugLevel > 2)
				Log.LogDebug("Try looking up cultureID");

			foreach (CultureInfo cultureInfo in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
			{
				if (TQDebug.DatabaseDebugLevel > 2)
					Log.LogDebug("Trying {0}", cultureInfo.EnglishName.ToUpperInvariant());

				if (cultureInfo.EnglishName.ToUpperInvariant().Equals(gameLanguage.ToUpperInvariant()) || cultureInfo.DisplayName.ToUpperInvariant().Equals(gameLanguage.ToUpperInvariant()))
				{
					cultureID = cultureInfo.TwoLetterISOLanguageName;
					break;
				}
			}

			// Titan Quest doesn't use the ISO language code for some languages
			// Added null check to fix exception when there is no culture found.
			if (cultureID != null)
			{
				if (cultureID.ToUpperInvariant() == "CS")
				{
					// Force Czech to use CZ instead of CS for the 2 letter code.
					cultureID = "CZ";
				}
				else if (cultureID.ToUpperInvariant() == "PT")
				{
					// Force brazilian portuguese to use BR instead of PT
					cultureID = "BR";
				}
				else if (cultureID.ToUpperInvariant() == "ZH")
				{
					// Force chinese to use CH instead of ZH
					cultureID = "CH";
				}
			}

			if (TQDebug.DatabaseDebugLevel > 1)
				Log.LogDebug("cultureID = {0}", cultureID);

			// Moved this inital check for the file into the loop
			// and added a check to verify that we actually have a cultureID
			if (cultureID != null)
			{
				filename = string.Concat(baseFile, cultureID, suffix);
				if (TQDebug.DatabaseDebugLevel > 1)
				{
					Log.LogDebug("Detected cultureID from gameLanguage");
					Log.LogDebug("filename = {0}", filename);
				}

				if (File.Exists(filename))
				{
					if (TQDebug.DatabaseDebugLevel > 0)
						Log.LogDebug("Exiting Database.FigureDBFileToUse()");

					return filename;
				}
			}
		}

		// try to use the default culture for the OS
		cultureID = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
		if (TQDebug.DatabaseDebugLevel > 1)
		{
			Log.LogDebug("Using cultureID from OS");
			Log.LogDebug("cultureID = {0}", cultureID);
		}

		// Added a check to verify that we actually have a cultureID
		// though it may not be needed
		if (cultureID != null)
		{
			filename = string.Concat(baseFile, cultureID, suffix);
			if (TQDebug.DatabaseDebugLevel > 1)
				Log.LogDebug("filename = {0}", filename);

			if (File.Exists(filename))
			{
				if (TQDebug.DatabaseDebugLevel > 0)
					Log.LogDebug("Exiting Database.FigureDBFileToUse()");

				return filename;
			}
		}

		// Now just try EN
		cultureID = "EN";
		filename = string.Concat(baseFile, cultureID, suffix);
		if (TQDebug.DatabaseDebugLevel > 1)
		{
			Log.LogDebug("Forcing English Language");
			Log.LogDebug("cultureID = {0}", cultureID);
			Log.LogDebug("filename = {0}", filename);
		}

		if (File.Exists(filename))
		{
			if (TQDebug.DatabaseDebugLevel > 0)
				Log.LogDebug("Database.Exiting FigureDBFileToUse()");

			return filename;
		}

		// Now just see if we can find anything.
		if (TQDebug.DatabaseDebugLevel > 0)
			Log.LogDebug("Detection Failed - searching for files");

		string[] files = Directory.GetFiles(rootFolder, "Text_??.arc");

		// Added check that files is not null.
		if (files != null && files.Length > 0)
		{
			if (TQDebug.DatabaseDebugLevel > 1)
			{
				Log.LogDebug("Found some files");
				Log.LogDebug("filename = {0}", files[0]);
			}

			if (TQDebug.DatabaseDebugLevel > 0)
				Log.LogDebug("Exiting Database.FigureDBFileToUse()");

			return files[0];
		}

		if (TQDebug.DatabaseDebugLevel > 0)
		{
			Log.LogDebug("Failed to determine Language file!");
			Log.LogDebug("Exiting Database.FigureDBFileToUse()");
		}

		return null;
	}

	/// <summary>
	/// Loads the Text database
	/// </summary>
	private void LoadTextDB()
	{
		if (TQDebug.DatabaseDebugLevel > 0)
			Log.LogDebug("Database.LoadTextDB()");

		string databaseFile = this.FigureDBFileToUse(false);
		if (TQDebug.DatabaseDebugLevel > 1)
		{
			Log.LogDebug("Find Titan Quest text file");
			Log.LogDebug("dbFile = {0}", databaseFile);
		}

		if (!string.IsNullOrEmpty(databaseFile))
		{
			string fileName = Path.GetFileNameWithoutExtension(databaseFile);
		}

		if (databaseFile != null)
		{
			// Try to suck what we want into memory and then parse it.
			this.ParseTextDB(databaseFile, "text\\commonequipment.txt");
			this.ParseTextDB(databaseFile, "text\\uniqueequipment.txt");
			this.ParseTextDB(databaseFile, "text\\quest.txt");
			this.ParseTextDB(databaseFile, "text\\ui.txt");
			this.ParseTextDB(databaseFile, "text\\skills.txt");
			this.ParseTextDB(databaseFile, "text\\monsters.txt"); // Added by VillageIdiot
			this.ParseTextDB(databaseFile, "text\\menu.txt"); // Added by VillageIdiot
			this.ParseTextDB(databaseFile, "text\\tutorial.txt");

			// Immortal Throne data
			this.ParseTextDB(databaseFile, "text\\xcommonequipment.txt");
			this.ParseTextDB(databaseFile, "text\\xuniqueequipment.txt");
			this.ParseTextDB(databaseFile, "text\\xquest.txt");
			this.ParseTextDB(databaseFile, "text\\xui.txt");
			this.ParseTextDB(databaseFile, "text\\xskills.txt");
			this.ParseTextDB(databaseFile, "text\\xmonsters.txt"); // Added by VillageIdiot
			this.ParseTextDB(databaseFile, "text\\xmenu.txt"); // Added by VillageIdiot
			this.ParseTextDB(databaseFile, "text\\xnpc.txt"); // Added by VillageIdiot
			this.ParseTextDB(databaseFile, "text\\modstrings.txt"); // Added by VillageIdiot
			this.ParseTextDB(databaseFile, "text\\xtutorial.txt"); // Added by hguy

			if (GamePathResolver.IsRagnarokInstalled)
			{
				this.ParseTextDB(databaseFile, "text\\x2commonequipment.txt");
				this.ParseTextDB(databaseFile, "text\\x2uniqueequipment.txt");
				this.ParseTextDB(databaseFile, "text\\x2quest.txt");
				this.ParseTextDB(databaseFile, "text\\x2ui.txt");
				this.ParseTextDB(databaseFile, "text\\x2skills.txt");
				this.ParseTextDB(databaseFile, "text\\x2monsters.txt"); // Added by VillageIdiot
				this.ParseTextDB(databaseFile, "text\\x2menu.txt"); // Added by VillageIdiot
				this.ParseTextDB(databaseFile, "text\\x2npc.txt"); // Added by VillageIdiot
			}

			if (GamePathResolver.IsAtlantisInstalled)
			{
				this.ParseTextDB(databaseFile, "text\\x3basegame_nonvoiced.txt");
				this.ParseTextDB(databaseFile, "text\\x3items_nonvoiced.txt");
				this.ParseTextDB(databaseFile, "text\\x3mainquest_nonvoiced.txt");
				this.ParseTextDB(databaseFile, "text\\x3misctags_nonvoiced.txt");
				this.ParseTextDB(databaseFile, "text\\x3sidequests_nonvoiced.txt");
			}

			if (GamePathResolver.IsEmbersInstalled)
			{
				this.ParseTextDB(databaseFile, "text\\x4basegame_nonvoiced.txt");
				this.ParseTextDB(databaseFile, "text\\x4items_nonvoiced.txt");
				this.ParseTextDB(databaseFile, "text\\x4mainquest_nonvoiced.txt");
				this.ParseTextDB(databaseFile, "text\\x4misctags_nonvoiced.txt");
				this.ParseTextDB(databaseFile, "text\\x4nametags_nonvoiced.txt");
				this.ParseTextDB(databaseFile, "text\\x4sidequests_nonvoiced.txt");
			}
		}

		// For loading custom map text database.
		if (GamePathResolver.IsCustom)
		{
			databaseFile = Path.Combine(GamePathResolver.MapName, "resources", "text.arc");

			if (TQDebug.DatabaseDebugLevel > 1)
			{
				Log.LogDebug("Find Custom Map text file");
				Log.LogDebug("dbFile = {0}", databaseFile);
			}

			if (databaseFile != null)
				this.ParseTextDB(databaseFile, "text\\modstrings.txt");
		}

		// Added this check to see if anything was loaded.
		if (this.textDB.Count == 0)
		{
			if (TQDebug.DatabaseDebugLevel > 0)
				Log.LogDebug("Exception - Could not load Text DB.");

			throw new FileLoadException("Could not load Text DB.");
		}

		if (TQDebug.DatabaseDebugLevel > 0)
			Log.LogDebug("Exiting Database.LoadTextDB()");
	}

	static Regex ParseTextDBRegEx = new Regex(@"^(?<Tag>\[\w+\])(?<Label>[^\\[]+)|^\[(?<Label>[^\]]+)\]$", RegexOptions.Compiled);
	/// <summary>
	/// Parses the text database to put the entries into a hash table.
	/// </summary>
	/// <param name="databaseFile">Database file name (arc file)</param>
	/// <param name="filename">Name of the text DB file within the arc file</param>
	private void ParseTextDB(string databaseFile, string filename)
	{
		if (TQDebug.DatabaseDebugLevel > 0)
			Log.LogDebug("Database.ParseTextDB({0}, {1})", databaseFile, filename);

		byte[] data = this.ReadARCFile(databaseFile, filename);

		if (data == null)
		{
			// Changed for mod support.  Sometimes the text file has more entries than just the x or non-x prefix files.
			if (TQDebug.DatabaseDebugLevel > 0)
				Log.LogDebug("Error in ARC File: {0} does not contain an entry for '{1}'", databaseFile, filename);

			return;
		}

		// now read it like a text file
		// Changed to system default encoding since there might be extended ascii (or something else) in the text db.
		using (StreamReader reader = new StreamReader(new MemoryStream(data), Encoding.Default))
		{
			char delimiter = '=';
			string line;
			while ((line = reader.ReadLine()) != null)
			{
				line = line.Trim();

				// delete short lines
				if (line.Length < 2)
					continue;

				// comment line
				if (line.StartsWith("//", StringComparison.Ordinal))
					continue;

				// split on the equal sign
				string[] fields = line.Split(delimiter);

				// bad line
				if (fields.Length < 2)
					continue;

				string label = fields[1].Trim();

				// Now for the foreign languages there is a bunch of crap in here so the proper version of the adjective can be used with the proper
				// noun form.  I don' want to code all that so this next code will just take the first version of the adjective and then
				// throw away all the metadata.

				// hguy : one expression to rule them all 
				if (ParseTextDBRegEx.Match(label) is { Success: true } match)
					label = match.Groups["Label"].Value.Trim();

				// If this field is already in the db, then replace it
				string key = fields[0].Trim().ToUpperInvariant();

				//if (!this.textDB.TryAdd(key, label))
				//	Log.LogDebug(@"TextDB Overlap ! Try to override ""{key}"" = ""{oldvalue}"" with ""{newvalue}"" !", key, this.textDB[key], label);

				this.textDB.AddOrUpdate(key, label, (k, v) => label);// Override with the new "label"
			}
		}

		if (TQDebug.DatabaseDebugLevel > 0)
			Log.LogDebug("Exiting Database.ParseTextDB()");
	}

	/// <summary>
	/// Loads a database arz file.
	/// </summary>
	private void LoadARZFile()
	{
		if (TQDebug.DatabaseDebugLevel > 0)
			Log.LogDebug("Database.LoadARZFile()");

		// from the original TQ folder
		string file = Path.Combine(GamePathResolver.GamePathTQ, "Database", "database.arz");

		if (TQDebug.DatabaseDebugLevel > 1)
		{
			Log.LogDebug("Load Titan Quest database arz file");
			Log.LogDebug("file = {0}", file);
		}

		this.ArzFile = new ArzFile(file);
		arzProv.Read(this.ArzFile);

		// now Immortal Throne expansion pack
		this.ArzFileIT = this.ArzFile;

		// Added to load a custom map database file.
		if (GamePathResolver.IsCustom)
		{
			file = Path.Combine(GamePathResolver.MapName, "database", $"{Path.GetFileName(GamePathResolver.MapName)}.arz");

			if (TQDebug.DatabaseDebugLevel > 1)
			{
				Log.LogDebug("Load Custom Map database arz file");
				Log.LogDebug("file = {0}", file);
			}

			if (File.Exists(file))
			{
				this.ArzFileMod = new ArzFile(file);
				arzProv.Read(this.ArzFileMod);
			}
			else
				this.ArzFileMod = null;
		}

		if (TQDebug.DatabaseDebugLevel > 0)
			Log.LogDebug("Exiting Database.LoadARZFile()");
	}

	#endregion Database Private Methods

}