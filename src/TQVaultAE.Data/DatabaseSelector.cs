using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Data;

/// <summary>
/// The application's <see cref="IDatabase"/>: forwards every call to the database of the active custom map, one
/// <see cref="Database"/> per map, created and loaded on first use and kept for the session.
/// </summary>
/// <param name="log">Logger.</param>
/// <param name="createDatabase">Creates a database, not loaded yet.</param>
public sealed class DatabaseSelector(ILogger<DatabaseSelector> log, Func<Database> createDatabase) : IDatabase, IDatabaseMaps
{
	private readonly ILogger Log = log;
	private readonly Func<Database> CreateDatabase = createDatabase;
	private readonly Dictionary<string, Database> _databases = new(StringComparer.OrdinalIgnoreCase);
	private readonly Lock _lock = new();

	private Database? _active;
	private bool? _autoDetectLanguage;
	private string? _tqLanguage;

	/// <summary>
	/// The active database. Before the first <see cref="LoadDBFile"/> or <see cref="Activate"/>, a database not loaded yet.
	/// </summary>
	private Database Active
	{
		get
		{
			if (this._active is null)
			{
				lock (this._lock)
					this._active ??= this.CreateDatabase();
			}

			return this._active;
		}
	}

	#region IDatabaseMaps

	/// <inheritdoc/>
	public string ActiveMapFolder => this.Active.CustomMapFolder ?? string.Empty;

	/// <inheritdoc/>
	public IReadOnlyCollection<string> LoadedMapFolders
	{
		get
		{
			lock (this._lock)
				return [.. this._databases.Keys];
		}
	}

	/// <inheritdoc/>
	public bool Activate(string? mapFolder)
	{
		string key = Normalize(mapFolder);

		lock (this._lock)
		{
			if (this._active is not null && this._databases.TryGetValue(key, out var loaded) && ReferenceEquals(loaded, this._active))
				return false;

			if (!this._databases.TryGetValue(key, out var database))
			{
				database = this.CreateDatabase();
				database.CustomMapFolder = key;
				this.ApplyLanguage(database);
				this.Log.LogInformation("Loading the game database for {Map}", key.Length == 0 ? "the base game" : key);
				database.LoadDBFile();
				this._databases[key] = database;
			}

			this._active = database;
			return true;
		}
	}

	private static string Normalize(string? mapFolder) => GameMaps.Normalize(mapFolder);

	private void ApplyLanguage(Database database)
	{
		if (this._autoDetectLanguage is { } autoDetect)
			database.AutoDetectLanguage = autoDetect;
		if (this._tqLanguage is not null)
			database.TQLanguage = this._tqLanguage;
	}

	#endregion

	#region IDatabase

	/// <summary>
	/// Loads the database of the map set at startup (<c>IGamePathService.MapName</c>) and makes it the active one.
	/// </summary>
	public void LoadDBFile()
	{
		lock (this._lock)
		{
			var database = this.Active;
			database.LoadDBFile(); // takes the startup map
			this._databases[Normalize(database.CustomMapFolder)] = database;
		}
	}

	public ReadOnlyDictionary<RecordId, LootRandomizerItem> AllLootRandomizer => this.Active.AllLootRandomizer;

	public ReadOnlyDictionary<RecordId, DBRecordCollection> AllLootRandomizerTable => this.Active.AllLootRandomizerTable;

	public ReadOnlyCollection<AffixTableMapItem>? GetItemAffixTableMap(RecordId itemId) => this.Active.GetItemAffixTableMap(itemId);

	public ArzFile? ArzFile => this.Active.ArzFile;

	public ArzFile? ArzFileIT => this.Active.ArzFileIT;

	public ArzFile? ArzFileMod => this.Active.ArzFileMod;

	public bool AutoDetectLanguage
	{
		get => this.Active.AutoDetectLanguage;
		set
		{
			this._autoDetectLanguage = value;
			lock (this._lock)
			{
				foreach (var database in this._databases.Values.Append(this.Active))
					database.AutoDetectLanguage = value;
			}
		}
	}

	public string? GameLanguage => this.Active.GameLanguage;

	public string TQLanguage
	{
		get => this.Active.TQLanguage;
		set
		{
			this._tqLanguage = value;
			lock (this._lock)
			{
				foreach (var database in this._databases.Values.Append(this.Active))
					database.TQLanguage = value;
			}
		}
	}

	public bool ExtractArcFile(string arcFileName, string destination) => this.Active.ExtractArcFile(arcFileName, destination);

	public string GetFriendlyName(string tagId) => this.Active.GetFriendlyName(tagId);

	public Info? GetInfo(RecordId itemId) => this.Active.GetInfo(itemId);

	public string GetItemAttributeFriendlyText(string itemAttribute, bool addVariable = true) => this.Active.GetItemAttributeFriendlyText(itemAttribute, addVariable);

	public DBRecordCollection? GetRecordFromFile(RecordId itemId) => this.Active.GetRecordFromFile(itemId);

	public byte[]? LoadResource(RecordId resourceId) => this.Active.LoadResource(resourceId);

	public (int Width, int Height)? GetTextureSize(RecordId resourceId) => this.Active.GetTextureSize(resourceId);

	public string VariableToStringNice(Variable variable) => this.Active.VariableToStringNice(variable);

	public ArcFile? ReadARCFile(string arcFileName) => this.Active.ReadARCFile(arcFileName);

	#endregion
}
