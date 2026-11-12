//-----------------------------------------------------------------------
// <copyright file="Database.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.RegularExpressions;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Config;
using TQVaultAE.GameFiles;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Data;

/// <summary>
/// Reads a Titan Quest database file.
/// </summary>
public partial class Database : IDatabase
{

	private const StringComparison noCase = StringComparison.OrdinalIgnoreCase;

	#region Record Class Names

	internal const string RCLASS_LOOTRANDOMIZERTABLE = "LootRandomizerTable";
	internal const string RCLASS_LOOTRANDOMIZER = "LootRandomizer";
	internal const string RCLASS_LOOTITEMTABLE_FIXEDWEIGHT = "LootItemTable_FixedWeight";
	internal const string RCLASS_LOOTITEMTABLE_DYNWEIGHT = "LootItemTable_DynWeight";

	#endregion

	private readonly ILogger Log;
	private readonly IFileIO FileIO;
	private readonly IDirectoryIO DirectoryIO;
	private readonly IPathIO PathIO;
	private readonly UserSettings USettings;

	#region Database Fields

	/// <summary>
	/// Dictionary of all database info records
	/// </summary>
	protected readonly LazyConcurrentDictionary<RecordId, Info?> infoDB = new();

	/// <summary>
	/// Dictionary of all text database entries
	/// </summary>
	protected readonly ConcurrentDictionary<string, string> textDB = new();

	/// <summary>
	/// Dictionary of all associated arc files in the database.
	/// </summary>
	protected readonly LazyConcurrentDictionary<string, ArcFile?> arcFiles = new();

	/// <summary>
	/// Dictionary of all records dataset loaded from the database.
	/// </summary>
	protected readonly LazyConcurrentDictionary<RecordId, byte[]?> resourcesData = new();
	private readonly LazyConcurrentDictionary<RecordId, (int Width, int Height)?> textureSizes = new();

	/// <summary>
	/// Dictionary of all record collections loaded from the database.
	/// </summary>
	protected readonly LazyConcurrentDictionary<RecordId, DBRecordCollection?> dbRecordCollections = new();

	/// <summary>
	/// Game language to support setting language in UI
	/// </summary>
	protected string? gameLanguage;

	protected readonly IArcFileProvider arcProv;
	protected readonly IArzFileProvider arzProv;
	protected readonly IItemAttributeProvider ItemAttributeProvider;
	protected readonly IGamePathService GamePathResolver;
	protected readonly ITQDataService TQData;

	#endregion Database Fields

	/// <summary>
	/// Initializes a new instance of Database class.
	/// </summary>
	public Database(
		ILogger<Database> log
		, IArcFileProvider arcFileProvider
		, IArzFileProvider arzFileProvider
		, IItemAttributeProvider itemAttributeProvider
		, IGamePathService gamePathResolver
		, ITQDataService tQData
		, IFileIO fileIO
		, IDirectoryIO directoryIO
		, IPathIO pathIO
		, UserSettings uSettings
	)
	{
		this.Log = log;
		USettings = uSettings;
		this.AutoDetectLanguage = uSettings.AutoDetectLanguage;
		this.TQLanguage = uSettings.TQLanguage;
		this.arcProv = arcFileProvider;
		this.arzProv = arzFileProvider;
		this.ItemAttributeProvider = itemAttributeProvider;
		this.GamePathResolver = gamePathResolver;
		this.TQData = tQData;
		this.FileIO = fileIO;
		this.DirectoryIO = directoryIO;
		this.PathIO = pathIO;
	}

	#region Database Properties

	ReadOnlyDictionary<RecordId, DBRecordCollection>? _LootRandomizerTableList;
	public ReadOnlyDictionary<RecordId, DBRecordCollection> AllLootRandomizerTable
	{
		get
		{
			if (_LootRandomizerTableList is null)
				_LootRandomizerTableList = ReadAllLootRandomizerTable();

			return _LootRandomizerTableList;
		}
	}

	ReadOnlyDictionary<RecordId, LootRandomizerItem>? _LootRandomizerList;
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
	private ReadOnlyDictionary<RecordId, ReadOnlyCollection<AffixTableMapItem>>? ItemAffixTableMap;

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
	public ArzFile? ArzFile { get; private set; }

	/// <summary>
	/// Gets the instance of the Immortal Throne Database ArzFile.
	/// </summary>
	public ArzFile? ArzFileIT { get; private set; }

	/// <summary>
	/// Gets the instance of a custom map Database ArzFile.
	/// </summary>
	public ArzFile? ArzFileMod { get; private set; }

	/// <summary>
	/// The custom map (mod) folder whose data this database adds over the game's; empty for the base game.
	/// Set before <see cref="LoadDBFile"/>, else taken from <see cref="IGamePathService.MapName"/> when it loads: each
	/// instance then keeps its own map, so databases of several maps can live side by side.
	/// </summary>
	public string? CustomMapFolder { get; set; }

	/// <inheritdoc/>
	public string DataScope => this.CustomMapFolder ?? string.Empty;

	/// <summary>
	/// True when this database reads a custom map's data.
	/// </summary>
	private bool IsCustomMap => !string.IsNullOrEmpty(this.MapFolder);

	/// <summary>
	/// <see cref="CustomMapFolder"/>, or the map set at startup while it is not set yet.
	/// </summary>
	private string MapFolder => this.CustomMapFolder
		?? (this.GamePathResolver.IsCustom ? this.GamePathResolver.MapName : string.Empty);

	[GeneratedRegex(@"(?i)language\s*=\s*(""(?<Language>[^""]+)""|(?<Language>[^\r\n]*))[\r\n]")]
	private static partial Regex GameLanguageRegEx();

	/// <summary>
	/// Gets the game language setting as a an English DisplayName.
	/// </summary>
	/// <remarks>Changed to property by VillageIdiot to support changing of Language in UI</remarks>
	public string? GameLanguage
	{
		get
		{
			// Added by VillageIdiot
			// Check if the user configured the language
			if (this.gameLanguage is null)
			{
				if (!this.AutoDetectLanguage)
					this.gameLanguage = this.TQLanguage;
			}

			// Try to read the language from the settings file
			if (string.IsNullOrEmpty(this.gameLanguage))
			{
				try
				{
					// Immortal Throne and the Anniversary Edition write their settings in the Immortal Throne folder:
					// a Titan Quest one is left by the original game and may give another language.
					var (optionsFile, otherOptionsFile) = GamePathResolver.GameType == GameType.TQ
						? (GamePathResolver.SettingsFileTQ, GamePathResolver.SettingsFileTQIT)
						: (GamePathResolver.SettingsFileTQIT, GamePathResolver.SettingsFileTQ);
					if (!this.FileIO.Exists(optionsFile))
						optionsFile = otherOptionsFile;
					if (this.FileIO.Exists(optionsFile))
					{
						var fileContent = this.FileIO.ReadAllText(optionsFile);
						var match = GameLanguageRegEx().Match(fileContent);
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
					this.Log.LogError(exception, "Failed to read the game language from {SettingsFile} or {SettingsFileIT}"
						, GamePathResolver.SettingsFileTQ, GamePathResolver.SettingsFileTQIT);
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

		if (USettings.DatabaseDebugLevel > 0)
			Log.LogDebug("Database.ExtractARCFile('{ArcFileName}', '{Destination}')", arcFileName, destination);

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

		if (USettings.DatabaseDebugLevel > 1)
			Log.LogDebug("Extraction Result = {Result}", result);

		if (USettings.DatabaseDebugLevel > 0)
			Log.LogDebug("Exiting Database.ReadARCFile()");

		return result;
	}

	#endregion Database Public Static Methods

	/// <summary>
	/// Gets the Infor for a specific item id.
	/// </summary>
	/// <param name="itemId">Item ID which we are looking up.  Will be normalized privately.</param>
	/// <returns>Returns Infor for item ID and NULL if not found.</returns>
	public Info? GetInfo(RecordId itemId)
	{
		if (itemId is null)
			return null;

		return this.infoDB.GetOrAddAtomic(itemId, k =>
		{
			DBRecordCollection? record = GetRecordFromFile(k);

			if (record is null)
				return null;

			return new Info(record);
		});
	}

	/// <summary>
	/// Uses the text database to convert the tag to a name in the localized language.
	/// The tag is normalized to upper case privately.
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
		ItemAttributesData? data = ItemAttributeProvider.GetAttributeData(itemAttribute);
		if (data is null)
		{
			this.Log.LogDebug("Attribute unknown : {ItemAttribute}", itemAttribute);
			return string.Concat("?", itemAttribute, "?");
		}

		string attributeTextTag = ItemAttributeProvider.GetAttributeTextTag(data);
		if (string.IsNullOrEmpty(attributeTextTag))
		{
			this.Log.LogDebug("Attribute unknown : {ItemAttribute}", itemAttribute);
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
	public void LoadDBFile()
	{
		this.CustomMapFolder ??= this.MapFolder;
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
			(Priority: 1, ArzFile: this.ArzFileIT is not null && this.ArzFileIT != this.ArzFile ? this.ArzFileIT : null),
			(Priority: 2, ArzFile: this.ArzFile)
		}
		.Where(db => db.ArzFile is not null)
		.SelectMany(db =>
			db.ArzFile!.RecordInfo.Where(r =>
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
		.Where(r => r.DBRecords is not null)
		.ToDictionary(r => r.RecordInfoKey, r => r.DBRecords!);

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
			(Priority: 1, ArzFile: this.ArzFileIT is not null && this.ArzFileIT != this.ArzFile ? this.ArzFileIT : null),
			(Priority: 2, ArzFile: this.ArzFile)
		}
		.Where(db => db.ArzFile is not null)
		.SelectMany(db =>
			db.ArzFile!.RecordInfo.Where(r => r.Value.RecordType.Equals(RCLASS_LOOTRANDOMIZER, noCase))
			.Select(ri => (RecordInfo: ri, db.Priority))
		)
		.Select(r =>
			{
				// Listed in the record table, so it can be read.
				var rec = GetRecordFromFile(r.RecordInfo.Key)!;

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
		var data = new[]
			{
				this.ArzFileMod,
				this.ArzFileIT is not null && this.ArzFileIT != this.ArzFile ? this.ArzFileIT : null,
				this.ArzFile
			}
			.Where(db => db is not null)
			.SelectMany(db => db!.RecordInfo
				.Where(r =>
					r.Value.RecordType.Equals(RCLASS_LOOTITEMTABLE_FIXEDWEIGHT, noCase)
					|| r.Value.RecordType.Equals(RCLASS_LOOTITEMTABLE_DYNWEIGHT, noCase)
				)
			)
			.Select(r =>
			{
				// Listed in the record table, so it can be read.
				var rec = GetRecordFromFile(r.Key)!;
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
				Dictionary<string, (RecordId brokenRandomizerName, RecordId prefixRandomizerName, RecordId suffixRandomizerName)> dico = [];
				foreach (var variable in rec)
				{
					var varname = "brokenRandomizerName";
					if (variable.Name.StartsWith(varname, noCase))
					{
						var brokenTable = variable.GetString(0);
						if (!string.IsNullOrWhiteSpace(brokenTable))
						{
							var num = variable.Name.Substring(varname.Length);

							if (dico.TryGetValue(num, out var val))
							{// ValueTuple are not ref
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

							if (dico.TryGetValue(num, out var val))
							{// ValueTuple are not ref
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

							if (dico.TryGetValue(num, out var val))
							{// ValueTuple are not ref
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

	public ReadOnlyCollection<AffixTableMapItem>? GetItemAffixTableMap(RecordId itemId)
	{
		return ItemAffixTableMap is not null && ItemAffixTableMap.TryGetValue(itemId, out var affixmap) ? affixmap : null;
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
	public DBRecordCollection? GetRecordFromFile(RecordId itemId)
	{
		var cachedDBRecordCollection = this.dbRecordCollections.GetOrAddAtomic(itemId, key =>
		{

			if (this.ArzFileMod is not null)
			{
				DBRecordCollection? recordMod = arzProv.GetItem(this.ArzFileMod, key);
				if (recordMod is not null)
				{
					// Custom Map records have highest precedence.
					return recordMod;
				}
			}

			if (this.ArzFileIT is not null)
			{
				// see if it's in IT ARZ file
				DBRecordCollection? recordIT = arzProv.GetItem(this.ArzFileIT, key);
				if (recordIT is not null)
				{
					// IT file takes precedence over TQ.
					return recordIT;
				}
			}

			return this.ArzFile is null ? null : arzProv.GetItem(this.ArzFile, key);
		});

		return cachedDBRecordCollection;
	}


#if DEBUG
	/// <summary>
	/// Debug-only test hook of the automation endpoint: resources read from here instead of the game files, uncached
	/// (null: missing), to check how an item is shown when its picture is missing or cannot be decoded.
	/// </summary>
	public static System.Collections.Concurrent.ConcurrentDictionary<RecordId, byte[]?> ResourceFaults { get; } = new();

#endif
	/// <summary>
	/// Gets a resource from the database using the resource Id.
	/// Modified by VillageIdiot to support loading resources from a custom map folder.
	/// </summary>
	/// <param name="resourceId">Resource which we are fetching</param>
	/// <returns>Retruns a byte array of the resource.</returns>
	public byte[]? LoadResource(RecordId resourceId)
	{
		if (RecordId.IsNullOrEmpty(resourceId))
			return null;

		if (USettings.DatabaseDebugLevel > 0)
			Log.LogDebug("Database.LoadResource({ResourceId})", resourceId);

		if (USettings.DatabaseDebugLevel > 1)
			Log.LogDebug(" Normalized({ResourceId})", resourceId);

#if DEBUG
		if (ResourceFaults.TryGetValue(resourceId, out byte[]? fault))
			return fault;
#endif

		byte[]? cachedArcFileData = this.resourcesData.GetOrAddAtomic(resourceId, this.ReadResource);

		if (USettings.DatabaseDebugLevel > 0)
			Log.LogDebug("Exiting Database.LoadResource()");

		return cachedArcFileData;
	}

	/// <inheritdoc/>
	public (int Width, int Height)? GetTextureSize(RecordId resourceId)
	{
		if (RecordId.IsNullOrEmpty(resourceId))
			return null;

#if DEBUG
		if (ResourceFaults.TryGetValue(resourceId, out byte[]? fault))
			return fault is not null && TexHeader.TryReadSize(fault, out int faultWidth, out int faultHeight) ? (faultWidth, faultHeight) : null;
#endif

		// Only the size is kept: the texture data is cached when the picture is drawn (LoadResource).
		return this.textureSizes.GetOrAddAtomic(resourceId, key =>
		{
			var data = this.resourcesData.TryGetValue(key, out var cached) ? cached.Value : this.ReadResource(key);
			return data is not null && TexHeader.TryReadSize(data, out int width, out int height)
				? (width, height)
				: null;
		});
	}

	/// <summary>
	/// Reads a resource from the game files or the custom map folder, without caching it.
	/// </summary>
	private byte[]? ReadResource(RecordId key)
	{
		var resourceIdSplited = key.Normalized.Split('\\', StringSplitOptions.RemoveEmptyEntries);// hguy : easier to understand than substring everywhere

		// not a proper resourceID.
		if (resourceIdSplited.Length == 1)
			return null;

		// First we need to figure out the correct file to
		// open, by grabbing it off the front of the resourceID

		string arcFile; bool isDLC = false;
		string rootFolder;
		byte[]? arcFileData = null;
		string arcFileBase = resourceIdSplited.First();

		if (USettings.DatabaseDebugLevel > 1)
			Log.LogDebug("arcFileBase = {ArcFileBase}", arcFileBase);

		// Added by VillageIdiot
		// Check's mod folder for's image resource.
		if (this.IsCustomMap)
		{
			if (USettings.DatabaseDebugLevel > 1)
				Log.LogDebug("Checking Custom Resources.");

			arcFileData = this.ReadCustomMapResource(key);

			if (USettings.DatabaseDebugLevel > 1 && arcFileData is not null)
				Log.LogDebug(@"Custom resource found ""{resourceId}""", key);
		}

		// We either didn't load the resource or didn't find what we were looking for so check the normal game resources.
		if (arcFileData is null)
		{
			// See if this guy is from Immortal Throne expansion pack.
			if (USettings.DatabaseDebugLevel > 1)
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

			if (this.FileIO.Exists(arcFile))
				arcFileData = this.ReadARCFile(arcFile, key);

			if (USettings.DatabaseDebugLevel > 0 && arcFileData is null)
				Log.LogError(@"Resource not found ""{resourceId}"" into ""{arcFile}""", key, arcFile);
		}

		#region Fallback

		// Added by VillageIdiot
		// Maybe the arc file is in the XPack folder even though the record does not state it.
		// Also could be that it says xpack in the record but the file is in the root.
		if (arcFileData is null)
		{
			rootFolder = this.PathIO.Combine(GamePathResolver.GamePathTQIT, "Resources", "XPack");
			arcFile = this.PathIO.Combine(rootFolder, this.PathIO.ChangeExtension(arcFileBase, ".arc"));

			if (this.FileIO.Exists(arcFile))
				arcFileData = this.ReadARCFile(arcFile, key);

			if (USettings.DatabaseDebugLevel > 1 && arcFileData is not null)
				Log.LogError(@"Resource misplaced ""{resourceId}"" into ""{arcFile}""", key, arcFile);
		}

		// Now, let's check if the item is in Ragnarok DLC
		if (arcFileData is null && GamePathResolver.IsRagnarokInstalled)
		{
			rootFolder = this.PathIO.Combine(GamePathResolver.GamePathTQIT, "Resources", "XPack2");
			arcFile = this.PathIO.Combine(rootFolder, this.PathIO.ChangeExtension(arcFileBase, ".arc"));

			if (this.FileIO.Exists(arcFile))
				arcFileData = this.ReadARCFile(arcFile, key);

			if (USettings.DatabaseDebugLevel > 1 && arcFileData is not null)
				Log.LogError(@"Resource misplaced ""{resourceId}"" into ""{arcFile}""", key, arcFile);
		}

		if (arcFileData is null && GamePathResolver.IsAtlantisInstalled)
		{
			rootFolder = this.PathIO.Combine(GamePathResolver.GamePathTQIT, "Resources", "XPack3");
			arcFile = this.PathIO.Combine(rootFolder, this.PathIO.ChangeExtension(arcFileBase, ".arc"));

			if (this.FileIO.Exists(arcFile))
				arcFileData = this.ReadARCFile(arcFile, key);

			if (USettings.DatabaseDebugLevel > 1 && arcFileData is not null)
				Log.LogError(@"Resource misplaced ""{resourceId}"" into ""{arcFile}""", key, arcFile);
		}

		if (arcFileData is null && GamePathResolver.IsEmbersInstalled)
		{
			rootFolder = this.PathIO.Combine(GamePathResolver.GamePathTQIT, "Resources", "XPack4");
			arcFile = this.PathIO.Combine(rootFolder, this.PathIO.ChangeExtension(arcFileBase, ".arc"));

			if (this.FileIO.Exists(arcFile))
				arcFileData = this.ReadARCFile(arcFile, key);

			if (USettings.DatabaseDebugLevel > 1 && arcFileData is not null)
				Log.LogError(@"Resource misplaced ""{resourceId}"" into ""{arcFile}""", key, arcFile);
		}

		if (arcFileData is null)
		{
			// We are either vanilla TQ or have not found our resource yet.
			// from the original TQ folder
			if (USettings.DatabaseDebugLevel > 1)
				Log.LogDebug("Checking TQ Resources.");

			rootFolder = GamePathResolver.GamePathTQ;
			rootFolder = this.PathIO.Combine(rootFolder, "Resources");

			arcFile = this.PathIO.Combine(rootFolder, this.PathIO.ChangeExtension(arcFileBase, ".arc"));

			if (this.FileIO.Exists(arcFile))
				arcFileData = this.ReadARCFile(arcFile, key);

			if (USettings.DatabaseDebugLevel > 0 && arcFileData is null)
				Log.LogError(@"Resource unknown ""{resourceId}""", key);
		}

		#endregion

		return arcFileData;
	}


	#endregion Database Public Methods

	#region Database Private Methods


	/// <summary>
	/// Reads data from an ARC file and puts it into a Byte array
	/// </summary>
	/// <param name="arcFileName">Name of the arc file.</param>
	/// <param name="dataId">Id of data which we are getting from the arc file</param>
	/// <returns>Byte array of the data from the arc file.</returns>
	private byte[]? ReadARCFile(string arcFileName, RecordId dataId)
	{
		// See if we have this arcfile already and if not create it.
		try
		{
			if (USettings.DatabaseDebugLevel > 0)
				Log.LogDebug("Database.ReadARCFile('{ArcFileName}', '{DataId}')", arcFileName, dataId);

			ArcFile? arcFile = ReadARCFile(arcFileName);

			if (arcFile is null)
				return null;

			// Now retrieve the data
			byte[]? ans = arcProv.GetData(arcFile, dataId);

			if (USettings.DatabaseDebugLevel > 0)
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
	public ArcFile? ReadARCFile(string arcFileName)
	{
		// See if we have this arcfile already and if not create it.
		ArcFile? arcFile = this.arcFiles.GetOrAddAtomic(arcFileName, k =>
		{
			if (!this.FileIO.Exists(k))
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
	private string? FigureDBFileToUse(string rootFolder)
	{
		string baseFile = this.PathIO.Combine(rootFolder, "Text_");
		string suffix = ".arc";

		// Added explicit set to null though may not be needed
		string? cultureID = null;

		// Moved this declaration since the first use is inside of the loop.
		string? filename = null;

		// First see if we can use the game setting
		string? gameLanguage = this.GameLanguage;
		if (USettings.DatabaseDebugLevel > 1)
		{
			Log.LogDebug("gameLanguage = {GameLanguage}", gameLanguage ?? "NULL");
			Log.LogDebug("baseFile = {BaseFile}", baseFile);
		}

		if (gameLanguage is not null)
		{
			cultureID = GameLanguageFileCode(gameLanguage);

			if (USettings.DatabaseDebugLevel > 1)
				Log.LogDebug("cultureID = {CultureID}", cultureID);

			// Moved this inital check for the file into the loop
			// and added a check to verify that we actually have a cultureID
			if (cultureID is not null)
			{
				filename = string.Concat(baseFile, cultureID, suffix);
				if (USettings.DatabaseDebugLevel > 1)
				{
					Log.LogDebug("Detected cultureID from gameLanguage");
					Log.LogDebug("filename = {Filename}", filename);
				}

				if (this.FileIO.Exists(filename))
				{
					if (USettings.DatabaseDebugLevel > 0)
						Log.LogDebug("Exiting Database.FigureDBFileToUse()");

					return filename;
				}
			}
		}

		// try to use the default culture for the OS
		cultureID = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
		if (USettings.DatabaseDebugLevel > 1)
		{
			Log.LogDebug("Using cultureID from OS");
			Log.LogDebug("cultureID = {CultureID}", cultureID);
		}

		// Added a check to verify that we actually have a cultureID
		// though it may not be needed
		if (cultureID is not null)
		{
			filename = string.Concat(baseFile, cultureID, suffix);
			if (USettings.DatabaseDebugLevel > 1)
				Log.LogDebug("filename = {Filename}", filename);

			if (this.FileIO.Exists(filename))
			{
				if (USettings.DatabaseDebugLevel > 0)
					Log.LogDebug("Exiting Database.FigureDBFileToUse()");

				return filename;
			}
		}

		// Now just try EN
		cultureID = "EN";
		filename = string.Concat(baseFile, cultureID, suffix);
		if (USettings.DatabaseDebugLevel > 1)
		{
			Log.LogDebug("Forcing English Language");
			Log.LogDebug("cultureID = {CultureID}", cultureID);
			Log.LogDebug("filename = {Filename}", filename);
		}

		if (this.FileIO.Exists(filename))
		{
			if (USettings.DatabaseDebugLevel > 0)
				Log.LogDebug("Database.Exiting FigureDBFileToUse()");

			return filename;
		}

		// Now just see if we can find anything.
		if (USettings.DatabaseDebugLevel > 0)
			Log.LogDebug("Detection Failed - searching for files");

		string[] files = DirectoryIO.GetFiles(rootFolder, "Text_??.arc");

		// Added check that files is not null.
		if (files is not null && files.Length > 0)
		{
			if (USettings.DatabaseDebugLevel > 1)
			{
				Log.LogDebug("Found some files");
				Log.LogDebug("filename = {File}", files[0]);
			}

			if (USettings.DatabaseDebugLevel > 0)
				Log.LogDebug("Exiting Database.FigureDBFileToUse()");

			return files[0];
		}

		if (USettings.DatabaseDebugLevel > 0)
		{
			Log.LogDebug("Failed to determine Language file!");
			Log.LogDebug("Exiting Database.FigureDBFileToUse()");
		}

		return null;
	}

	/// <summary>
	/// Code of a game language in the text file names (<c>Text_FR.arc</c>): the ISO code, except CZ for Czech, BR for
	/// Portuguese and CH for Chinese.
	/// </summary>
	/// <param name="gameLanguage">English or native name of the language, as in the game settings.</param>
	/// <returns>The upper case code, or null for an unknown language.</returns>
	internal static string? GameLanguageFileCode(string gameLanguage)
	{
		foreach (CultureInfo cultureInfo in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
		{
			if (string.Equals(cultureInfo.EnglishName, gameLanguage, StringComparison.OrdinalIgnoreCase)
				|| string.Equals(cultureInfo.DisplayName, gameLanguage, StringComparison.OrdinalIgnoreCase))
			{
				return cultureInfo.TwoLetterISOLanguageName.ToUpperInvariant() switch
				{
					"CS" => "CZ",
					"PT" => "BR",
					"ZH" => "CH",
					var code => code,
				};
			}
		}

		return null;
	}

	/// <summary>
	/// Loads the strings of a custom map: <c>resources\text_xx.arc</c> in the game language, else
	/// <c>resources\text.arc</c>, else the loose <c>resources\text\modstrings.txt</c> of a mod shipped unpacked.
	/// </summary>
	private void LoadCustomMapText()
	{
		string resources = this.PathIO.Combine(this.MapFolder, "resources");
		string? code = this.GameLanguage is { } language ? GameLanguageFileCode(language) : null;
		string[] archives = code is null ? ["text.arc"] : [$"text_{code}.arc", "text.arc"];

		foreach (var archive in archives)
		{
			string databaseFile = this.PathIO.Combine(resources, archive);
			if (!this.FileIO.Exists(databaseFile))
				continue;

			if (USettings.DatabaseDebugLevel > 1)
				Log.LogDebug("Custom map text file {DatabaseFile}", databaseFile);

			this.ParseTextDB(databaseFile, "text\\modstrings.txt");
			return;
		}

		string looseFile = this.PathIO.Combine(resources, "text", "modstrings.txt");
		if (this.FileIO.Exists(looseFile))
			this.ParseTextData(this.FileIO.ReadAllBytes(looseFile));
	}

	/// <summary>
	/// Reads a resource of the custom map, in one of the layouts mods use, in this order:
	/// <list type="bullet">
	/// <item>the archive named after the first segment: <c>resources\Items.arc</c> for <c>Items\x.tex</c>,
	/// <c>resources\XPack2.arc</c> for <c>XPack2\Quests\x.qst</c> (entry <c>Quests\x.qst</c>);</item>
	/// <item>the game's expansion layout: <c>resources\xpack\Item.arc</c> for <c>XPack\Item\x.tex</c> (entry <c>x.tex</c>);</item>
	/// <item>a loose file, for a mod shipped unpacked: <c>resources\Items\x.tex</c>.</item>
	/// </list>
	/// </summary>
	/// <returns>The data, or null when the custom map does not have the resource.</returns>
	private byte[]? ReadCustomMapResource(RecordId resourceId)
	{
		var tokens = resourceId.TokensRaw;
		string resources = this.PathIO.Combine(this.MapFolder, "resources");

		byte[]? data = this.ReadARCFile(this.PathIO.Combine(resources, tokens[0] + ".arc"), resourceId);

		if (data is null && GameArchives.IsExpansionResource(resourceId) && tokens.Count > 2)
			data = this.ReadARCFile(this.PathIO.Combine(resources, tokens[0], tokens[1] + ".arc"), tokens.Skip(1).JoinString("\\"));

		if (data is null)
		{
			string looseFile = this.PathIO.Combine([resources, .. tokens]);
			if (this.FileIO.Exists(looseFile))
				data = this.FileIO.ReadAllBytes(looseFile);
		}

		return data;
	}

	/// <summary>
	/// Loads the Text database
	/// </summary>
	private void LoadTextDB()
	{
		if (USettings.DatabaseDebugLevel > 0)
			Log.LogDebug("Database.LoadTextDB()");

		string rootFolder;
		string? databaseFile;
		if (GamePathResolver.GameType == GameType.TQAE)
		{
			rootFolder = this.PathIO.Combine(GamePathResolver.GamePathTQIT, "Text");

			if (!DirectoryIO.Exists(rootFolder))
				return;

			databaseFile = this.FigureDBFileToUse(rootFolder);

			if (databaseFile is null)
				return;

			ParseTextBase(databaseFile);
			ParseTextIT(databaseFile);
		}
		else if (GamePathResolver.GameType == GameType.TQIT)
		{
			/*
			 * Steam Version split text files in /Text and /Resources,
			 * Disk install with patches use only /Resources
			 * Disk install without patches use TQ/Text and TQIT/Resources
			 */

			rootFolder = this.PathIO.Combine(GamePathResolver.GamePathTQIT, "Text");
			var tqItTextDirExists = DirectoryIO.Exists(rootFolder);

			if (tqItTextDirExists) // Steam Version
			{
				databaseFile = this.FigureDBFileToUse(rootFolder);

				if (databaseFile is not null)
					ParseTextBase(databaseFile);
			}
			else
			{
				rootFolder = this.PathIO.Combine(GamePathResolver.GamePathTQ, "Text"); // Read text from TQ dir
				var tqTextDirExists = DirectoryIO.Exists(rootFolder);

				if (tqTextDirExists)
				{
					databaseFile = this.FigureDBFileToUse(rootFolder);

					if (databaseFile is not null)
						ParseTextBase(databaseFile);
				}
			}

			// Resources
			rootFolder = this.PathIO.Combine(GamePathResolver.GamePathTQIT, "Resources");

			databaseFile = this.FigureDBFileToUse(rootFolder);

			if (databaseFile is null)
				return;

			ParseTextBase(databaseFile); // TQIT patched use only Resources for that
			ParseTextIT(databaseFile);
		}
		else // TQ
		{
			// from the original TQ folder
			rootFolder = this.PathIO.Combine(GamePathResolver.GamePathTQ, "Text");

			if (!DirectoryIO.Exists(rootFolder))
				return;

			databaseFile = this.FigureDBFileToUse(rootFolder);

			if (databaseFile is null)
				return;

			ParseTextBase(databaseFile);
		}

		if (GamePathResolver.IsRagnarokInstalled)
		{
			this.ParseTextDB(databaseFile, "text\\x2commonequipment.txt");
			this.ParseTextDB(databaseFile, "text\\x2uniqueequipment.txt");
			this.ParseTextDB(databaseFile, "text\\x2quest.txt");
			// skills before ui, as in the archive: x2ui.txt redefines 22 Runemaster tags with the reworked texts, over the
			// old ones of x2skills.txt (one still carries a developer note, "PBAoE - should self-target automatically").
			this.ParseTextDB(databaseFile, "text\\x2skills.txt");
			this.ParseTextDB(databaseFile, "text\\x2ui.txt");
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

		// For loading custom map text database.
		if (this.IsCustomMap)
			this.LoadCustomMapText();

		// Added this check to see if anything was loaded.
		if (textDB.IsEmpty)
		{
			if (USettings.DatabaseDebugLevel > 0)
				Log.LogDebug("Exception - Could not load Text DB.");

			throw new FileLoadException("Could not load Text DB.");
		}

		if (USettings.DatabaseDebugLevel > 0)
			Log.LogDebug("Exiting Database.LoadTextDB()");
	}

	private void ParseTextIT(string databaseFile)
	{
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
	}

	private void ParseTextBase(string databaseFile)
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
	}

	[GeneratedRegex(@"^(?<Tag>\[\w+\])(?<Label>[^\\[]+)|^\[(?<Label>[^\]]+)\]$")]
	private static partial Regex ParseTextDBRegEx();
	/// <summary>
	/// Parses the text database to put the entries into a hash table.
	/// </summary>
	/// <param name="databaseFile">Database file name (arc file)</param>
	/// <param name="filename">Name of the text DB file within the arc file</param>
	private void ParseTextDB(string databaseFile, string filename)
	{
		if (USettings.DatabaseDebugLevel > 0)
			Log.LogDebug("Database.ParseTextDB({DatabaseFile}, {Filename})", databaseFile, filename);

		byte[]? data = this.ReadARCFile(databaseFile, filename);

		if (data is null)
		{
			// Changed for mod support.  Sometimes the text file has more entries than just the x or non-x prefix files.
			if (USettings.DatabaseDebugLevel > 0)
				Log.LogDebug("Error in ARC File: {DatabaseFile} does not contain an entry for '{Filename}'", databaseFile, filename);

			return;
		}

		this.ParseTextData(data);
	}

	/// <summary>
	/// Puts the entries of a text file into the text database.
	/// </summary>
	private void ParseTextData(byte[] data)
	{
		// The game files are UTF-16 with a BOM, but some (x2 quests, mod strings) are cp1252 without one.
		foreach (var (tag, text) in TextDatabaseFactory.ReadEntries(data))
		{
			string label = text;

			// Now for the foreign languages there is a bunch of crap in here so the proper version of the adjective can be used with the proper
			// noun form.  I don' want to code all that so this next code will just take the first version of the adjective and then
			// throw away all the metadata.

			// hguy : one expression to rule them all
			if (ParseTextDBRegEx().Match(label) is { Success: true } match)
				label = match.Groups["Label"].Value.Trim();

			// A tag already in the db is replaced: the last file loaded wins.
			this.textDB[tag.ToUpperInvariant()] = label;
		}

		if (USettings.DatabaseDebugLevel > 0)
			Log.LogDebug("Exiting Database.ParseTextDB()");
	}

	/// <summary>
	/// Loads a database arz file.
	/// </summary>
	private void LoadARZFile()
	{
		if (USettings.DatabaseDebugLevel > 0)
			Log.LogDebug("Database.LoadARZFile()");

		// from the original TQ folder
		string file = this.PathIO.Combine(GamePathResolver.GamePathTQ, "Database", "database.arz");

		this.ArzFile = new ArzFile(file);
		arzProv.Read(this.ArzFile);

		if (USettings.DatabaseDebugLevel > 1)
		{
			Log.LogDebug("Load Titan Quest database arz file");
			Log.LogDebug("file = {File}", file);
		}

		this.ArzFileIT = this.ArzFile;

		// now Immortal Throne expansion pack
		if (!string.IsNullOrWhiteSpace(GamePathResolver.GamePathTQIT) && GamePathResolver.GamePathAreDifferent)
		{
			file = this.PathIO.Combine(GamePathResolver.GamePathTQIT, "Database", "database.arz");

			this.ArzFileIT = new ArzFile(file);
			arzProv.Read(this.ArzFileIT);

			if (USettings.DatabaseDebugLevel > 1)
			{
				Log.LogDebug("Load Titan Quest database arz file");
				Log.LogDebug("file = {File}", file);
			}
		}

		// Added to load a custom map database file.
		if (this.IsCustomMap)
		{
			file = this.PathIO.Combine(this.MapFolder, "database", $"{this.PathIO.GetFileName(this.MapFolder)}.arz");

			if (USettings.DatabaseDebugLevel > 1)
			{
				Log.LogDebug("Load Custom Map database arz file");
				Log.LogDebug("file = {File}", file);
			}

			if (this.FileIO.Exists(file))
			{
				this.ArzFileMod = new ArzFile(file);
				arzProv.Read(this.ArzFileMod);
			}
			else
				this.ArzFileMod = null;
		}

		if (USettings.DatabaseDebugLevel > 0)
			Log.LogDebug("Exiting Database.LoadARZFile()");
	}

	#endregion Database Private Methods

}