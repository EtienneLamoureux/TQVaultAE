using Microsoft.Extensions.Logging;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Session;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Config;
using TQVaultAE.Application.Results;

namespace TQVaultAE.Services;

public class StashService(
	ILogger<StashService> log,
	SessionContext userContext,
	IItemDatabaseService itemDatabaseService,
	IStashProvider stashProvider,
	IGamePathService gamePathResolver,
	IGameFileService iGameFileService,
	UserSettings userSettings,
	IPathIO pathIO,
	IGameMapService gameMapService,
	ICharacterReadOnlyService characterReadOnlyService,
	IItemProvider itemProvider) : IStashService
{
	private readonly IGameMapService GameMapService = gameMapService;
	private readonly ICharacterReadOnlyService CharacterReadOnly = characterReadOnlyService;
	private readonly IItemProvider ItemProvider = itemProvider;

	/// <summary>Reads with a custom map's database (empty for the base game) until disposed.</summary>
	private IDisposable ReadWithMap(string? mapFolder)
		=> this.GameMapService.ReadWithMap(mapFolder);

	/// <summary>The items of a stash just read keep its map, which they were read with.</summary>
	private static void Inherit(Stash stash, string? mod)
	{
		foreach (var item in stash.Sack ?? [])
			item.SetContainerMod(mod);
	}

	/// <summary>A shared stash's name in the search: a mod's names its mod, as each map has its own.</summary>
	private string SharedStashName(string name, string map)
		=> this.GameMapService.ModNameOfMap(map) is { } mod ? $"{name} [{ModIdentity.NameOf(mod)}]" : name;

	private readonly ILogger Log = log;
	private readonly SessionContext userContext = userContext;
	private readonly IItemDatabaseService ItemDatabaseService = itemDatabaseService;
	private readonly IStashProvider StashProvider = stashProvider;
	private readonly IGamePathService GamePathResolver = gamePathResolver;
	private readonly IGameFileService GameFileService = iGameFileService;
	private readonly UserSettings UserSettings = userSettings;
	private readonly IPathIO PathIO = pathIO;

	/// <summary>
	/// Loads a player stash.
	/// </summary>
	/// <param name="selectedSave">Player save information.</param>
	/// <param name="fromFileWatcher">When <c>true</c> called from <see cref="FileSystemWatcher.Changed"/></param>
	/// <returns>Stash load result.</returns>
	public StashLoadResult LoadPlayerStash(PlayerSave? selectedSave, bool fromFileWatcher = false)
	{
		var result = new StashLoadResult();

		if (string.IsNullOrWhiteSpace(selectedSave?.Name))
			return result;

		// From the save's own folder: main campaign and custom map characters are listed together.
		result.StashFile = this.GamePathResolver.StashFileIn(this.PathIO, selectedSave.Folder);

		Stash addStash(string k)
		{
			var stash = new Stash(selectedSave.Name, k);
			try
			{
				// Read with the character's own map; its items inherit that map.
				using (this.GameMapService.ReadWith(selectedSave))
					stash.StashFound = StashProvider.LoadFile(stash);

				Inherit(stash, this.GameMapService.ModNameFor(selectedSave));
				if ((stash.StashFound ?? false) && stash.Sack is not null)
					stash.Sack.StashType = StashType.PlayerStash;

				// The stash of a read-only character is read-only with it.
				stash.IsReadOnly = this.CharacterReadOnly.IsReadOnly(selectedSave.Folder);

				// Add stash items to the search database
				this.ItemDatabaseService.IndexStash(stash, k, selectedSave.Name, StashType.PlayerStash);
			}
			catch (ArgumentException argumentException)
			{
				stash.ArgumentException = argumentException;
			}
			return stash;
		}

		result.Stash = this.userContext.Stashes.LoadOrReload(result.StashFile, addStash, fromFileWatcher, this.ItemDatabaseService);

		return result;
	}

	/// <summary>
	/// What tells the transfer stash and the relic vault apart: their name, their file and their stash type.
	/// </summary>
	private sealed record SharedStashKind(StashType Type, string Name, Func<string> ActiveMapFile, Func<string, string> FileOfMap);

	private SharedStashKind KindOf(StashType type) => type == StashType.TransferStash
		? new(StashType.TransferStash, SharedStashNames.TransferStash, () => this.GamePathResolver.TransferStashFileFullPath, this.GamePathResolver.TransferStashFileFullPathFor)
		: new(StashType.RelicVaultStash, SharedStashNames.RelicVault, () => this.GamePathResolver.RelicVaultStashFileFullPath, this.GamePathResolver.RelicVaultStashFileFullPathFor);

	/// <summary>
	/// Loads the transfer stash for immortal throne.
	/// </summary>
	/// <param name="fromFileWatcher">When <c>true</c> called from <see cref="FileSystemWatcher.Changed"/></param>
	/// <returns>Stash load result.</returns>
	public StashLoadResult LoadTransferStash(bool fromFileWatcher = false, string? mapFolder = null)
		=> this.LoadSharedStash(this.KindOf(StashType.TransferStash), fromFileWatcher, mapFolder);

	/// <summary>
	/// Loads the relic vault stash.
	/// </summary>
	/// <param name="fromFileWatcher">When <c>true</c> called from <see cref="FileSystemWatcher.Changed"/></param>
	/// <returns>Stash load result.</returns>
	public StashLoadResult LoadRelicVaultStash(bool fromFileWatcher = false, string? mapFolder = null)
		=> this.LoadSharedStash(this.KindOf(StashType.RelicVaultStash), fromFileWatcher, mapFolder);

	/// <summary>
	/// Loads a stash shared by the characters of a map: the shown character's unless <paramref name="mapFolder"/> is
	/// given.
	/// </summary>
	private StashLoadResult LoadSharedStash(SharedStashKind kind, bool fromFileWatcher, string? mapFolder)
	{
		var result = new StashLoadResult();

		string map = GameMaps.Normalize(mapFolder ?? this.GameMapService.ActiveSharedStashMap);
		var data = this.GameMapService.SharedStashDataOf(map);
		// The shown character's files follow the active shared stash map (IGamePathService.MapName).
		result.StashFile = mapFolder is null ? kind.ActiveMapFile() : kind.FileOfMap(map);

		Stash addStash(string k)
		{
			var stash = new Stash(kind.Name, k);
			try
			{
				// Read with its map's data (the base game's files: the shown character's); its items inherit that map.
				using (this.ReadWithMap(data.Map))
					stash.StashFound = StashProvider.LoadFile(stash);

				Inherit(stash, data.Mod);
				this.userContext.SharedStashReadWith[k] = data;
				if ((stash.StashFound ?? false) && stash.Sack is not null)
					stash.Sack.StashType = kind.Type;

				// Add the stash items to the search database
				this.ItemDatabaseService.IndexStash(stash, k, this.SharedStashName(kind.Name, map), kind.Type);
			}
			catch (ArgumentException argumentException)
			{
				stash.ArgumentException = argumentException;
			}

			return stash;
		}

		// Read with other data before (the base game's files, shown with a character of another mod): read again; with
		// changes to save, its items only, where they lie.
		bool otherData = this.userContext.SharedStashReadWith.TryGetValue(result.StashFile, out var readWith) && !readWith.SameAs(data);
		if (otherData && !fromFileWatcher && this.userContext.Stashes.TryGetLoaded(result.StashFile, out var modified) && modified.IsModified)
		{
			this.ReadItemsWith(modified, data, this.SharedStashName(kind.Name, map), kind.Type);
			result.Stash = modified;
		}
		else
			result.Stash = this.userContext.Stashes.LoadOrReload(result.StashFile, addStash, fromFileWatcher || otherData, this.ItemDatabaseService);

		result.Stash.IsImmortalThrone = true;

		return result;
	}

	/// <summary>
	/// Reads the items of a loaded shared stash again with other data, and puts them in the search again.
	/// </summary>
	private void ReadItemsWith(Stash stash, SharedStashData data, string name, StashType type)
	{
		foreach (var item in stash.Sack ?? [])
		{
			if (item.SetContainerMod(data.Mod))
				this.ItemProvider.GetDBData(item);
		}

		this.userContext.SharedStashReadWith[stash.StashFile] = data;
		this.ItemDatabaseService.RemoveContainerFromDatabase(stash.StashFile);
		this.ItemDatabaseService.IndexStash(stash, stash.StashFile, name, type);
	}

	/// <summary>
	/// A Titan Quest original character has no player, transfer or relic vault stash.
	/// </summary>
	private static bool HasStashes(PlayerSave? selectedSave)
		=> selectedSave is null || selectedSave.IsImmortalThrone;

	/// <inheritdoc/>
	public StashLoadResult? LoadCharacterStash(PlayerSave selectedSave, bool fromFileWatcher = false)
		=> HasStashes(selectedSave) ? this.LoadPlayerStash(selectedSave, fromFileWatcher) : null;

	/// <inheritdoc/>
	public StashLoadResult? LoadTransferStashFor(PlayerSave? selectedSave, bool fromFileWatcher = false)
		=> HasStashes(selectedSave) ? this.LoadTransferStash(fromFileWatcher) : null;

	/// <inheritdoc/>
	public StashLoadResult? LoadRelicVaultStashFor(PlayerSave? selectedSave, bool fromFileWatcher = false)
		=> HasStashes(selectedSave) ? this.LoadRelicVaultStash(fromFileWatcher) : null;

	/// <inheritdoc/>
	public string SharedStashFile(StashType type, string mapFolder)
		=> this.KindOf(type).FileOfMap(mapFolder);

	/// <inheritdoc/>
	public StashLoadResult? ReloadSharedStash(StashType type, string mapFolder)
	{
		var kind = this.KindOf(type);

		// A file the session never read has nothing to refresh: it is read when first shown.
		if (!this.userContext.Stashes.ContainsKey(kind.FileOfMap(mapFolder)))
			return null;

		return this.LoadSharedStash(kind, true, mapFolder);
	}

	/// <inheritdoc/>
	public IReadOnlyList<(string File, Stash Stash)> GetModifiedStashes()
		=> [.. this.userContext.Stashes.LoadedEntries()
			.Select(kvp => (File: kvp.Key, Stash: kvp.Value))
			.Where(s => s.Stash is not null && s.Stash.IsModified && !this.SkipReadOnly(s.Stash.IsReadOnly, s.File))];

	/// <summary>
	/// The stash of a read-only character is never saved, even modified (nothing should modify it): its read-only
	/// attribute stays.
	/// </summary>
	private bool SkipReadOnly(bool isReadOnly, string file)
	{
		if (isReadOnly)
			this.Log.LogWarning("The read-only stash file {File} has changes: it is not saved", file);

		return isReadOnly;
	}

	/// <inheritdoc/>
	public void SaveModifiedStash(string file, Stash stash)
	{
		if (!this.UserSettings.DisableLegacyBackup)
			this.GameFileService.BackupFile(stash.PlayerName, file);

		this.StashProvider.Save(stash, file);
		stash.Saved();
	}

	/// <inheritdoc />
	public int ClampBagId(int bagId)
		=> Math.Clamp(bagId, BagIdConstants.BAGID_EQUIPMENTPANEL, BagIdConstants.BAGID_RELICVAULTSTASH);

	/// <inheritdoc />
	public SackCollection? GetBagSack(int bagId, PlayerCollection? player, Stash? transferStash, Stash? playerStash, Stash? relicVaultStash)
	{
		static SackCollection? SackOf(Stash? stash)
			=> stash is { NumberOfSacks: > 0 } ? stash.Sack : null;

		return this.ClampBagId(bagId) switch
		{
			BagIdConstants.BAGID_EQUIPMENTPANEL => player?.EquipmentSack,
			BagIdConstants.BAGID_TRANSFERSTASH => SackOf(transferStash),
			BagIdConstants.BAGID_PLAYERSTASH => SackOf(playerStash),
			_ => SackOf(relicVaultStash),
		};
	}

	/// <summary>
	/// The player stash is centered as if it were at least this many cells high.
	/// </summary>
	private const int PlayerStashMinCenteringHeight = 15;

	/// <inheritdoc />
	public StashBag? GetBag(int bagId, PlayerCollection? player, Stash? transferStash, Stash? playerStash, Stash? relicVaultStash)
	{
		static StashBag? BagOf(int id, Stash? stash, int minCenteringHeight = 0)
			=> stash is null
				? null
				: new StashBag(id, stash.Sack, stash.Width, stash.Height, Math.Max(minCenteringHeight, stash.Height));

		int id = this.ClampBagId(bagId);
		return id switch
		{
			BagIdConstants.BAGID_EQUIPMENTPANEL => new StashBag(id, player?.EquipmentSack, 0, 0, 0),
			BagIdConstants.BAGID_TRANSFERSTASH => BagOf(id, transferStash),
			BagIdConstants.BAGID_PLAYERSTASH => BagOf(id, playerStash, PlayerStashMinCenteringHeight),
			_ => BagOf(id, relicVaultStash),
		};
	}
}
