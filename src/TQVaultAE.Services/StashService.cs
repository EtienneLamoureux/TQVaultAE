using Microsoft.Extensions.Logging;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Presentation;
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
	IGameMapService? gameMapService = null,
	ICharacterReadOnlyService? characterReadOnlyService = null) : IStashService
{
	private readonly IGameMapService? GameMapService = gameMapService;
	private readonly ICharacterReadOnlyService? CharacterReadOnly = characterReadOnlyService;

	/// <summary>Reads with a custom map's database (empty for the base game) until disposed.</summary>
	private IDisposable ReadWithMap(string? mapFolder)
		=> this.GameMapService?.ReadWithMap(mapFolder) ?? NoScope.Instance;

	/// <summary>The items of a stash just read inherit its map.</summary>
	private static void Inherit(Stash stash, string? mod)
	{
		foreach (var item in stash.Sack ?? [])
			item.InheritMod(mod);
	}

	/// <summary>A shared stash's name in the search: a mod's names its mod, as each map has its own.</summary>
	private string SharedStashName(string name, string map)
		=> this.GameMapService?.ModNameOfMap(map) is { } mod ? $"{name} [{ModIdentity.NameOf(mod)}]" : name;

	/// <summary>The map whose shared stash files are read: the given one, else the active one.</summary>
	private string MapOrActive(string? mapFolder)
		=> mapFolder ?? this.GameMapService?.ActiveMap ?? string.Empty;

	private readonly ILogger Log = log;
	private readonly SessionContext userContext = userContext;
	private readonly IItemDatabaseService ItemDatabaseService = itemDatabaseService;
	private readonly IStashProvider StashProvider = stashProvider;
	private readonly IGamePathService GamePathResolver = gamePathResolver;
	private readonly IGameFileService GameFileService = iGameFileService;
	private readonly UserSettings UserSettings = userSettings;

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
		result.StashFile = Path.Combine(selectedSave.Folder, GamePathResolver.PlayerStashFileNameB);

		Stash addStash(string k)
		{
			var stash = new Stash(selectedSave.Name, k);
			try
			{
				// Read with the character's own map; its items inherit that map.
				using (this.GameMapService?.ReadWith(selectedSave) ?? NoScope.Instance)
					stash.StashFound = StashProvider.LoadFile(stash);

				Inherit(stash, this.GameMapService?.ModNameFor(selectedSave));
				if ((stash.StashFound ?? false) && stash.Sack is not null)
					stash.Sack.StashType = StashType.PlayerStash;

				// The stash of a read-only character is read-only with it.
				stash.IsReadOnly = this.CharacterReadOnly?.IsReadOnly(selectedSave.Folder) ?? false;

				// Add stash items to the search database
				if (stash.Sack is not null)
				{
					foreach (var item in stash.Sack)
						this.ItemDatabaseService.AddItemToDatabase(item, k, selectedSave.Name, BagIdConstants.BAGID_PLAYERSTASH, SackType.Stash, StashType.PlayerStash);
				}
			}
			catch (ArgumentException argumentException)
			{
				stash.ArgumentException = argumentException;
			}
			return stash;
		}

		Stash updateStash(string k, Stash oldValue)
		{
			// no logic with oldValue
			// The reloaded items replace those of the old file in the search.
			this.ItemDatabaseService.RemoveContainerFromDatabase(k);
			return addStash(k);
		}

		result.Stash = fromFileWatcher
			? userContext.Stashes.AddOrUpdateAtomic(result.StashFile, addStash, updateStash)
			: userContext.Stashes.GetOrAddAtomic(result.StashFile, addStash);

		return result;
	}

	/// <summary>
	/// Loads the transfer stash for immortal throne.
	/// </summary>
	/// <param name="fromFileWatcher">When <c>true</c> called from <see cref="FileSystemWatcher.Changed"/></param>
	/// <returns>Stash load result.</returns>
	public StashLoadResult LoadTransferStash(bool fromFileWatcher = false, string? mapFolder = null)
	{
		var result = new StashLoadResult();

		string map = this.MapOrActive(mapFolder);
		result.StashFile = mapFolder is null ? GamePathResolver.TransferStashFileFullPath : GamePathResolver.TransferStashFileFullPathFor(map);

		Stash addStash(string k)
		{
			var stash = new Stash(Resources.GlobalTransferStash, result.StashFile);
			try
			{
				// Read with the map whose file it is; its items inherit that map.
				using (this.ReadWithMap(map))
					stash.StashFound = StashProvider.LoadFile(stash);

				Inherit(stash, this.GameMapService?.ModNameOfMap(map));
				if ((stash.StashFound ?? false) && stash.Sack is not null)
					stash.Sack.StashType = StashType.TransferStash;

				// Add transfer stash items to the search database
				if (stash.Sack is not null)
				{
					foreach (var item in stash.Sack)
						this.ItemDatabaseService.AddItemToDatabase(item, k, this.SharedStashName(Resources.GlobalTransferStash, map), BagIdConstants.BAGID_TRANSFERSTASH, SackType.Stash, StashType.TransferStash);
				}
			}
			catch (ArgumentException argumentException)
			{
				stash.ArgumentException = argumentException;
			}
			return stash;
		}

		Stash updateStash(string k, Stash oldValue)
		{
			// The reloaded items replace those of the old file in the search.
			this.ItemDatabaseService.RemoveContainerFromDatabase(k);
			return addStash(k);
		}

		var resultStash = fromFileWatcher
			? userContext.Stashes.AddOrUpdateAtomic(result.StashFile, addStash, updateStash)
			: userContext.Stashes.GetOrAddAtomic(result.StashFile, addStash);

		result.Stash = resultStash;
		result.Stash.IsImmortalThrone = true;

		return result;
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

	/// <summary>
	/// Loads the relic vault stash.
	/// </summary>
	/// <param name="fromFileWatcher">When <c>true</c> called from <see cref="FileSystemWatcher.Changed"/></param>
	/// <returns>Stash load result.</returns>
	public StashLoadResult LoadRelicVaultStash(bool fromFileWatcher = false, string? mapFolder = null)
	{
		var result = new StashLoadResult();

		string map = this.MapOrActive(mapFolder);
		result.StashFile = mapFolder is null ? GamePathResolver.RelicVaultStashFileFullPath : GamePathResolver.RelicVaultStashFileFullPathFor(map);

		Stash addStash(string k)
		{
			var stash = new Stash(Resources.GlobalRelicVaultStash, k);

			try
			{
				// Read with the map whose file it is; its items inherit that map.
				using (this.ReadWithMap(map))
					stash.StashFound = StashProvider.LoadFile(stash);

				Inherit(stash, this.GameMapService?.ModNameOfMap(map));
				if ((stash.StashFound ?? false) && stash.Sack is not null)
					stash.Sack.StashType = StashType.RelicVaultStash;

				// Add relic vault stash items to the search database
				if (stash.Sack is not null)
				{
					foreach (var item in stash.Sack)
						this.ItemDatabaseService.AddItemToDatabase(item, k, this.SharedStashName(Resources.GlobalRelicVaultStash, map), BagIdConstants.BAGID_RELICVAULTSTASH, SackType.Stash, StashType.RelicVaultStash);
				}
			}
			catch (ArgumentException argumentException)
			{
				stash.ArgumentException = argumentException;
			}

			return stash;
		}

		Stash updateStash(string k, Stash oldValue)
		{
			// The reloaded items replace those of the old file in the search.
			this.ItemDatabaseService.RemoveContainerFromDatabase(k);
			return addStash(k);
		}

		// Get the relic vault stash
		var resultStash = fromFileWatcher
			? userContext.Stashes.AddOrUpdateAtomic(result.StashFile, addStash, updateStash)
			: userContext.Stashes.GetOrAddAtomic(result.StashFile, addStash);

		result.Stash = resultStash;
		result.Stash.IsImmortalThrone = true;

		return result;
	}

	/// <inheritdoc/>
	public string SharedStashFile(StashType type, string mapFolder)
		=> type == StashType.TransferStash
			? this.GamePathResolver.TransferStashFileFullPathFor(mapFolder)
			: this.GamePathResolver.RelicVaultStashFileFullPathFor(mapFolder);

	/// <inheritdoc/>
	public StashLoadResult? ReloadSharedStash(StashType type, string mapFolder)
	{
		// A file the session never read has nothing to refresh: it is read when first shown.
		if (!this.userContext.Stashes.ContainsKey(this.SharedStashFile(type, mapFolder)))
			return null;

		return type == StashType.TransferStash
			? this.LoadTransferStash(true, mapFolder)
			: this.LoadRelicVaultStash(true, mapFolder);
	}

	/// <inheritdoc/>
	public IReadOnlyList<(string File, Stash Stash)> GetModifiedStashes()
		=> [.. this.userContext.Stashes
			.Where(kvp => kvp.Value.IsValueCreated)
			.Select(kvp => (File: kvp.Key, Stash: kvp.Value.Value))
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
