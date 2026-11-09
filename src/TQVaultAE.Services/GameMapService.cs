using System.Globalization;
using Microsoft.Extensions.Logging;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Config;
using TQVaultAE.Presentation;

namespace TQVaultAE.Services;

/// <inheritdoc cref="IGameMapService"/>
public class GameMapService(
	ILogger<GameMapService> log
	, IDatabaseMaps databaseMaps
	, IGamePathService gamePathService
	, IModLinkService modLinkService
	, UserSettings userSettings
	, CommandLineArgs commandLineArgs
	, IEnumerable<IGameDataCache> gameDataCaches
) : IGameMapService
{
	private readonly ILogger Log = log;
	private readonly IDatabaseMaps DatabaseMaps = databaseMaps;
	private readonly IGamePathService GamePathService = gamePathService;
	private readonly IModLinkService ModLinkService = modLinkService;
	private readonly UserSettings USettings = userSettings;
	private readonly CommandLineArgs CommandLineArgs = commandLineArgs;
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
	public string DefaultCustomMap
		=> GameMaps.Normalize(this.CommandLineArgs.HasMapName
			? this.CommandLineArgs.MapName
			: this.USettings.ModEnabled ? this.USettings.CustomMap : null);

	/// <inheritdoc/>
	public CharacterMod ModOf(PlayerSave save)
		=> this.ModLinkService.Resolve(save.Folder, save.IsCustom);

	/// <inheritdoc/>
	public string MapFor(PlayerSave save)
		=> this.MapFor(this.ModOf(save));

	private string MapFor(CharacterMod mod) => mod.State switch
	{
		CharacterModState.BaseGame => string.Empty,
		CharacterModState.Linked => GameMaps.Normalize(mod.Mod!.Folder),
		_ => this.DefaultCustomMap,
	};

	/// <inheritdoc/>
	public bool UsesActiveMap(PlayerSave save)
		=> GameMaps.Same(this.MapFor(save), this.ActiveMap);

	/// <inheritdoc/>
	public bool ActivateFor(PlayerSave save)
	{
		this.Refresh(save);
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
		=> this._loadedWith[save.Folder] = this.ActiveMap;

	/// <inheritdoc/>
	public bool LoadedWithOtherMap(PlayerSave save)
		=> this._loadedWith.TryGetValue(save.Folder, out var map) && !GameMaps.Same(map, this.ActiveMap);

	/// <inheritdoc/>
	public void Refresh(PlayerSave save)
	{
		var mod = this.ModOf(save);
		save.CustomMap = this.MapFor(mod);
		save.ModLabel = mod.State switch
		{
			CharacterModState.Linked => mod.Mod!.Name,
			CharacterModState.LinkedModMissing => string.Format(CultureInfo.CurrentCulture, Resources.ModLinkMissingLabel, mod.Link!.Mod),
			CharacterModState.Unlinked => Resources.ModLinkUnlinkedLabel,
			_ => null,
		};
	}
}
