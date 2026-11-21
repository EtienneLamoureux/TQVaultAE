using Microsoft.Extensions.Logging;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Session;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Services;

/// <inheritdoc cref="IGameMapService"/>
public class GameMapService(
	ILogger<GameMapService> log
	, IDatabaseMaps databaseMaps
	, IGamePathService gamePathService
	, IModLinkService modLinkService
	, IEnumerable<IGameDataCache> gameDataCaches
) : IGameMapService
{
	private readonly ILogger Log = log;
	private readonly IDatabaseMaps DatabaseMaps = databaseMaps;
	private readonly IGamePathService GamePathService = gamePathService;
	private readonly IModLinkService ModLinkService = modLinkService;
	private readonly IReadOnlyList<IGameDataCache> GameDataCaches = [.. gameDataCaches];

	/// <summary>
	/// The map each character's files were read with, by save folder.
	/// </summary>
	private readonly System.Collections.Concurrent.ConcurrentDictionary<string, string> _loadedWith = new(StringComparer.OrdinalIgnoreCase);

	/// <inheritdoc/>
	public event EventHandler? ActiveMapChanged;

	/// <inheritdoc/>
	public string ActiveMap => GameMaps.Normalize(this.DatabaseMaps.ActiveMapFolder);

	/// <inheritdoc/>
	public CharacterMod ModOf(PlayerSave save)
		=> this.ModLinkService.Resolve(save.Folder, save.IsCustom);

	/// <inheritdoc/>
	public string MapFor(PlayerSave save)
		=> MapFor(this.ModOf(save));

	/// <summary>
	/// Only a link to an installed mod gives a custom map: the other characters use the base game data.
	/// </summary>
	private static string MapFor(CharacterMod mod)
		=> mod.State == CharacterModState.Linked ? GameMaps.Normalize(mod.Mod!.Folder) : string.Empty;

	/// <inheritdoc/>
	public string? ModNameFor(PlayerSave save)
		=> this.ModOf(save) is { State: CharacterModState.Linked or CharacterModState.LinkedModMissing } mod
			? mod.Mod?.Id ?? ModIdentity.Compose(mod.Link?.Mod, mod.Link?.Source)
			: null;

	/// <inheritdoc/>
	public string? ModNameOfMap(string? mapFolder) => this.ModLinkService.ModIdOfMap(mapFolder);

	/// <inheritdoc/>
	public string? ActiveCharacterMod { get; private set; }

	/// <inheritdoc/>
	public IDisposable ReadWith(PlayerSave save)
		=> this.DatabaseMaps.Use(this.MapFor(save));

	/// <inheritdoc/>
	public IDisposable ReadWithMap(string? mapFolder)
		=> this.DatabaseMaps.Use(GameMaps.Normalize(mapFolder));

	/// <inheritdoc/>
	public Task PreloadMapsAsync(IEnumerable<PlayerSave> saves)
	{
		var list = saves.ToList();
		return Task.Run(() =>
		{
			// Reading the link files is cheap; the base game is loaded at startup already.
			var maps = list.Select(this.MapFor).Where(map => map.Length != 0).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
			foreach (var map in maps)
			{
				try
				{
					this.DatabaseMaps.Preload(map);
				}
				catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException)
				{
					this.Log.LogWarning(ex, "Preloading the game database for {Map} failed", map);
				}
			}
		});
	}

	/// <inheritdoc/>
	public PlayerSave? CharacterShowing(string sharedStashFile, IEnumerable<PlayerSave> characters, PlayerSave? selected)
	{
		bool Shows(PlayerSave save)
		{
			// A Titan Quest original character has no shared stash.
			if (!save.IsImmortalThrone)
				return false;

			string map = this.SharedStashMapFor(save);
			return SameFile(this.GamePathService.TransferStashFileFullPathFor(map), sharedStashFile)
				|| SameFile(this.GamePathService.RelicVaultStashFileFullPathFor(map), sharedStashFile);
		}

		// The base game's shared stashes are read with the shown character's data: one read with the data they have now
		// shows them as the search found them.
		bool SameData(PlayerSave save)
			=> this.SharedStashMapFor(save).Length != 0 || GameMaps.Same(this.MapFor(save), this._baseStashData.Map);

		if (selected is not null && Shows(selected))
			return selected;

		var showing = characters.Where(Shows).ToList();
		return showing.FirstOrDefault(SameData) ?? showing.FirstOrDefault();
	}

	private static bool SameFile(string a, string b)
		=> string.Equals(System.IO.Path.GetFullPath(a), System.IO.Path.GetFullPath(b), StringComparison.OrdinalIgnoreCase);

	/// <inheritdoc/>
	public string SharedStashMapFor(PlayerSave save)
		=> save.IsCustom ? this.MapFor(save) : string.Empty;

	/// <inheritdoc/>
	public string ActiveSharedStashMap { get; private set; } = string.Empty;

	/// <summary>
	/// The data the base game's shared stash files are read with: the last shown character using them.
	/// </summary>
	private SharedStashData _baseStashData = new(string.Empty, null);

	/// <inheritdoc/>
	public SharedStashData SharedStashDataOf(string? sharedStashMap)
	{
		string map = GameMaps.Normalize(sharedStashMap);
		return map.Length == 0 ? this._baseStashData : new SharedStashData(map, this.ModNameOfMap(map));
	}

	/// <inheritdoc/>
	public bool ActivateFor(PlayerSave save)
	{
		this.Refresh(save);
		this.ActiveCharacterMod = this.ModNameFor(save);

		string stashMap = this.SharedStashMapFor(save);
		bool stashChanged = !GameMaps.Same(stashMap, this.ActiveSharedStashMap);
		if (stashMap.Length == 0)
		{
			// The base game's shared stashes take the data of the character shown with them: a bounce mod's.
			var data = new SharedStashData(save.CustomMap, this.ActiveCharacterMod);
			stashChanged |= !this._baseStashData.SameAs(data);
			this._baseStashData = data;
		}

		bool changed = this.Activate(save.CustomMap, stashMap);
		if (stashChanged && !changed)
			this.ActiveMapChanged?.Invoke(this, EventArgs.Empty);

		return changed || stashChanged;
	}

	/// <inheritdoc/>
	public bool Activate(string? mapFolder)
		=> this.Activate(mapFolder, GameMaps.Normalize(mapFolder));

	/// <summary>
	/// Makes a map's database the active one, and a map's shared stash files the active ones.
	/// </summary>
	private bool Activate(string? mapFolder, string sharedStashMap)
	{
		string map = GameMaps.Normalize(mapFolder);
		bool changed = this.DatabaseMaps.Activate(map);

		// The shared stash files follow the shown character: a main campaign one keeps the base game's (SaveData\Sys).
		this.ActiveSharedStashMap = sharedStashMap;
		this.GamePathService.MapName = sharedStashMap.Length == 0 ? "main" : sharedStashMap;

		if (!changed)
			return false;

		this.Log.LogInformation("Active map: {Map}", map.Length == 0 ? "base game" : map);
		foreach (var cache in this.GameDataCaches)
			cache.ClearGameDataCache();

		this.ActiveMapChanged?.Invoke(this, EventArgs.Empty);
		return true;
	}

	/// <inheritdoc/>
	public void MarkLoaded(PlayerSave save)
		=> this._loadedWith[save.Folder] = this.MapFor(save);

	/// <inheritdoc/>
	public bool LoadedWithOtherMap(PlayerSave save)
		=> this._loadedWith.TryGetValue(save.Folder, out var map) && !GameMaps.Same(map, this.MapFor(save));

	/// <inheritdoc/>
	public void Refresh(PlayerSave save)
	{
		var mod = this.ModOf(save);
		save.CustomMap = MapFor(mod);
		save.ModState = mod.State;
		save.ModName = mod.Mod?.Name ?? mod.Link?.Mod;
	}
}
