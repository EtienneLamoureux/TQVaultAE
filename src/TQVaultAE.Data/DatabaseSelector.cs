using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Data;

/// <summary>
/// The application's <see cref="IDatabase"/>: forwards every call to the database of the active custom map, one
/// <see cref="Database"/> per map, created and loaded on first use (or by <see cref="Preload"/>) and kept for the
/// session.
/// </summary>
/// <remarks>
/// A map's database is loaded outside the selector's lock: a load (about half a second for a large mod) only makes the
/// users of that same map wait, never those of a map already loaded.
/// </remarks>
/// <param name="log">Logger.</param>
/// <param name="createDatabase">Creates a database, not loaded yet.</param>
public sealed class DatabaseSelector(ILogger<DatabaseSelector> log, Func<Database> createDatabase) : IDatabase, IDatabaseMaps, IDisposable
{
	private readonly ILogger Log = log;
	private readonly Func<Database> CreateDatabase = createDatabase;
	private readonly Dictionary<string, Lazy<Database>> _databases = new(StringComparer.OrdinalIgnoreCase);
	private readonly Lock _lock = new();

	private Database? _active;

	/// <summary>
	/// The database a <see cref="Use"/> scope gives the current thread.
	/// </summary>
	private readonly ThreadLocal<Database?> _scoped = new();
	private bool? _autoDetectLanguage;
	private string? _tqLanguage;

	/// <summary>
	/// The active database. Before the first <see cref="LoadDBFile"/> or <see cref="Activate"/>, a database not loaded yet.
	/// </summary>
	private Database Active
	{
		get
		{
			if (this._scoped.Value is { } scoped)
				return scoped;

			if (this._active is null)
			{
				lock (this._lock)
					this._active ??= this.CreateDatabase();
			}

			return this._active;
		}
	}

	/// <inheritdoc/>
	public string DataScope => this.Active.DataScope;

	#region IDatabaseMaps

	/// <inheritdoc/>
	public string ActiveMapFolder => (this._active ?? this.Active).CustomMapFolder ?? string.Empty;

	/// <inheritdoc/>
	public IReadOnlyCollection<string> LoadedMapFolders
	{
		get
		{
			lock (this._lock)
				return [.. this._databases.Where(entry => entry.Value.IsValueCreated).Select(entry => entry.Key)];
		}
	}

	/// <inheritdoc/>
	public bool Activate(string? mapFolder)
	{
		var database = this.GetOrLoad(Normalize(mapFolder));

		lock (this._lock)
		{
			if (ReferenceEquals(database, this._active))
				return false;

			this._active = database;
			return true;
		}
	}

	/// <inheritdoc/>
	public IDisposable Use(string? mapFolder)
	{
		var database = this.GetOrLoad(Normalize(mapFolder));

		var previous = this._scoped.Value;
		this._scoped.Value = database;
		return new ScopedDatabase(this, previous);
	}

	/// <inheritdoc/>
	public void Preload(string? mapFolder) => this.GetOrLoad(Normalize(mapFolder));

	/// <summary>
	/// The database of a map, loaded the first time, outside the lock: a thread asking for the same map meanwhile waits
	/// for that load, the others go on. A failed load is forgotten, so the next use tries again.
	/// </summary>
	private Database GetOrLoad(string key)
	{
		Lazy<Database>? entry;
		lock (this._lock)
		{
			if (!this._databases.TryGetValue(key, out entry))
			{
				entry = new Lazy<Database>(() => this.Load(key), LazyThreadSafetyMode.ExecutionAndPublication);
				this._databases[key] = entry;
			}
		}

		try
		{
			return entry.Value;
		}
		catch
		{
			lock (this._lock)
			{
				if (this._databases.TryGetValue(key, out var current) && ReferenceEquals(current, entry))
					this._databases.Remove(key);
			}

			throw;
		}
	}

	private Database Load(string key)
	{
		var database = this.CreateDatabase();
		database.CustomMapFolder = key;
		this.ApplyLanguage(database);
		this.Log.LogInformation("Loading the game database for {Map}", key.Length == 0 ? "the base game" : key);
		long start = System.Diagnostics.Stopwatch.GetTimestamp();
		database.LoadDBFile();
		this.Log.LogInformation("Game database for {Map} loaded in {Milliseconds} ms", key.Length == 0 ? "the base game" : key
			, (long)System.Diagnostics.Stopwatch.GetElapsedTime(start).TotalMilliseconds);
		return database;
	}

	/// <summary>
	/// The databases loaded so far. Call under the lock.
	/// </summary>
	private IEnumerable<Database> LoadedDatabases
		=> this._databases.Values.Where(entry => entry.IsValueCreated).Select(entry => entry.Value);

	/// <inheritdoc/>
	public void Dispose() => this._scoped.Dispose();

	private sealed class ScopedDatabase(DatabaseSelector owner, Database? previous) : IDisposable
	{
		private bool _disposed;

		public void Dispose()
		{
			if (this._disposed)
				return;

			this._disposed = true;
			owner._scoped.Value = previous;
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
			var loaded = new Lazy<Database>(() => database);
			_ = loaded.Value;
			this._databases[Normalize(database.CustomMapFolder)] = loaded;
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
				foreach (var database in this.LoadedDatabases.Append(this.Active))
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
				foreach (var database in this.LoadedDatabases.Append(this.Active))
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
