using System.Globalization;
using Microsoft.Extensions.Logging;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Presentation;

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
			? mod.Mod?.Name ?? mod.Link?.Mod
			: null;

	/// <inheritdoc/>
	public string? ModNameOfMap(string? mapFolder)
	{
		string map = GameMaps.Normalize(mapFolder);
		return map.Length == 0 ? null : System.IO.Path.GetFileName(System.IO.Path.TrimEndingDirectorySeparator(map));
	}

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
	public bool UsesActiveMap(PlayerSave save)
		=> GameMaps.Same(this.MapFor(save), this.ActiveMap);

	/// <inheritdoc/>
	public bool ActivateFor(PlayerSave save)
	{
		this.Refresh(save);
		this.ActiveCharacterMod = this.ModNameFor(save);
		return this.Activate(save.CustomMap);
	}

	/// <inheritdoc/>
	public bool Activate(string? mapFolder)
	{
		string map = GameMaps.Normalize(mapFolder);
		bool changed = this.DatabaseMaps.Activate(map);

		// The stash files and the custom map paths follow the active map.
		this.GamePathService.MapName = map.Length == 0 ? "main" : map;

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
		save.ModLabel = mod.State switch
		{
			CharacterModState.Linked => mod.Mod!.Name,
			CharacterModState.LinkedModMissing => string.Format(CultureInfo.CurrentCulture, Resources.ModLinkMissingLabel, mod.Link!.Mod),
			CharacterModState.Unlinked => Resources.ModLinkUnlinkedLabel,
			_ => null,
		};
	}
}
