using Microsoft.Extensions.Logging;
using TQVaultAE.Application.Session;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Infrastructure;

namespace TQVaultAE.GUI.Views;

/// <summary>
/// Hot reload: the game save files of the listed characters and the shared stash files are watched, and read again
/// once the game has written them.
/// </summary>
/// <remarks>
/// While the main window shows another view in place of the main content, the main content sleeps: the reloads of the
/// character files changed meanwhile wait for its return (the character editor works on the character as loaded).
/// </remarks>
public partial class MainFormUserControl
{
	/// <summary>Coalesces the change events of the game save files (player, player stash, caravan and relic stash).</summary>
	private FileReloadDebouncer? fileReloadDebouncer;

	private FileReloadDebouncer FileReloadDebouncer => this.fileReloadDebouncer ??= new FileReloadDebouncer(this.Log);

	/// <summary>Reloads of character files changed while the main content slept, scheduled again when it wakes up.</summary>
	private readonly Dictionary<string, Action> deferredReloads = new(StringComparer.OrdinalIgnoreCase);

	/// <summary>True while the main window shows another view in place of the main content.</summary>
	private bool asleep;

	/// <summary>
	/// The watchers of the shared stash files (transfer stash and relic vault) of every map in use, by file.
	/// </summary>
	private readonly Dictionary<string, FileSystemWatcher> sharedStashWatchers = new(StringComparer.OrdinalIgnoreCase);

	/// <summary>
	/// The character save files found, each watched when hot reload is on.
	/// </summary>
	internal PlayerSave[] WatchedCharacterSaves()
	{
		var characters = this.Services.PlayerService.GetPlayerSaveList();

		if (this.Services.Settings.EnableHotReload)
		{
			var paths = this.Services.GamePathService;
			foreach (var ps in characters)
			{
				ps.PlayerSaveWatcher = this.CreateCharacterFileWatcher(ps.Folder, paths.PlayerSaveFileName);
				ps.PlayerStashWatcher = this.CreateCharacterFileWatcher(ps.Folder, paths.PlayerStashFileNameB);
			}
		}

		return characters;
	}

	private FileSystemWatcher CreateCharacterFileWatcher(string folder, string filename)
	{
		// The game writes the save file when the inventory is opened and again when it is closed: both events look alike.
		var fw = new FileSystemWatcher();
		fw.BeginInit();
		fw.Path = folder;
		fw.Filter = filename;
		fw.IncludeSubdirectories = false;
		fw.NotifyFilter = NotifyFilters.Attributes;// Attributes triggers after file update
		fw.SynchronizingObject = this;
		fw.Changed += this.CharacterFile_Changed;
		fw.EnableRaisingEvents = true;
		fw.EndInit();
		return fw;
	}

	// Raised on the UI thread (SynchronizingObject = this)
	private void CharacterFile_Changed(object? sender, FileSystemEventArgs e)
	{
		var paths = this.Services.GamePathService;
		if (e.ChangeType != WatcherChangeTypes.Changed
			|| !(string.Equals(e.Name, paths.PlayerSaveFileName, StringComparison.OrdinalIgnoreCase)
				|| string.Equals(e.Name, paths.PlayerStashFileNameB, StringComparison.OrdinalIgnoreCase))
		) return;

		var folder = ((FileSystemWatcher)sender!).Path;
		var fileName = e.Name;

		this.ScheduleCharacterReload(e.FullPath, () => this.ReloadCharacterFile(folder, fileName));
	}

	/// <summary>
	/// Schedules the reload of a changed character file; while the main content sleeps, the reload waits for its return.
	/// </summary>
	private void ScheduleCharacterReload(string path, Action reload)
		=> this.FileReloadDebouncer.Schedule(path, () =>
		{
			if (!this.asleep)
			{
				this.Log.LogDebug("Hot reload of {File}", path);
				reload();
			}
			else
			{
				this.Log.LogDebug("Hot reload of {File} kept until the main content is shown again", path);
				this.deferredReloads[path] = reload;
			}
		});

	/// <summary>The main window shows another view in place of the main content: the character reloads wait.</summary>
	internal void Sleep() => this.asleep = true;

	/// <summary>
	/// The main content is shown again: the character files changed meanwhile are reloaded as if they had just changed.
	/// </summary>
	internal void Wake()
	{
		this.asleep = false;

		var reloads = this.deferredReloads.ToList();
		this.deferredReloads.Clear();
		foreach (var (path, reload) in reloads)
			this.ScheduleCharacterReload(path, reload);
	}

	/// <summary>No hot reload may land any more: the files are about to be saved and the window to close.</summary>
	internal void StopHotReload()
	{
		this.fileReloadDebouncer?.Dispose();
		this.fileReloadDebouncer = null;
	}

	/// <summary>
	/// The maps whose shared stashes (transfer stash, relic vault) are read: the base game and the map of every listed
	/// Immortal Throne character.
	/// </summary>
	internal List<string> SharedStashMaps()
		=> SharedStashes.Maps(this.ListedCharacters().Where(c => c.IsImmortalThrone).Select(this.Services.GameMapService.MapFor));

	/// <summary>
	/// Watches the shared stash files of every map in use when hot reload is on: the base game's, those of the maps of
	/// the listed characters, and the active map's. Watchers already set are kept.
	/// </summary>
	internal void WatchSharedStashFiles()
	{
		if (!this.Services.Settings.EnableHotReload)
			return;

		var mapService = this.Services.GameMapService;
		var maps = this.SharedStashMaps();
		if (!maps.Any(m => GameMaps.Same(m, mapService.ActiveMap)))
			maps.Add(mapService.ActiveMap);

		foreach (var map in maps)
		{
			foreach (var type in new[] { StashType.TransferStash, StashType.RelicVaultStash })
			{
				string file = this.Services.StashService.SharedStashFile(type, map);
				string? folder = this.Services.PathIO.GetDirectoryName(file);
				if (this.sharedStashWatchers.ContainsKey(file) || folder is null || !Directory.Exists(folder))
					continue;

				var watcher = new FileSystemWatcher(folder, this.Services.PathIO.GetFileName(file))
				{
					NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime,
					SynchronizingObject = this,
				};
				string watchedMap = map;
				watcher.Changed += (_, e) => this.SharedStashFile_Changed(e, type, watchedMap);
				watcher.EnableRaisingEvents = true;
				this.sharedStashWatchers[file] = watcher;
				// The view's container disposes it with the view.
				this.components?.Add(watcher);
			}
		}
	}

	/// <summary>
	/// Reads a shared stash again after the game wrote it: in the search, and in the stash panel when it is the active
	/// map's. Raised on the UI thread (SynchronizingObject = this).
	/// </summary>
	private void SharedStashFile_Changed(FileSystemEventArgs e, StashType type, string map)
	{
		if (e.ChangeType != WatcherChangeTypes.Changed) return;

		this.FileReloadDebouncer.Schedule(e.FullPath, () => this.ReloadSharedStash(type, map));
	}
}
