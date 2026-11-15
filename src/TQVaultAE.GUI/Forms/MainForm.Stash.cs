﻿using System.Globalization;
using TQVaultAE.Application.Session;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Components.Panels;
using TQVaultAE.GUI.Components;
using TQVaultAE.GUI.Models;
using TQVaultAE.Presentation;
using Microsoft.Extensions.Logging;
using TQVaultAE.Application.Results;

namespace TQVaultAE.GUI.Forms;

public partial class MainForm
{
	/// <summary>
	/// The watchers of the shared stash files (transfer stash and relic vault) of every map in use, by file.
	/// </summary>
	private readonly Dictionary<string, FileSystemWatcher> sharedStashWatchers = new(StringComparer.OrdinalIgnoreCase);

	/// <summary>
	/// Watches the shared stash files of every map in use when hot reload is on: the base game's, those of the maps of
	/// the listed characters, and the active map's. Watchers already set are kept.
	/// </summary>
	private void WatchSharedStashFiles()
	{
		if (!USettings.EnableHotReload)
			return;

		var maps = this.SharedStashMaps();
		if (!maps.Any(m => GameMaps.Same(m, this.gameMapService.ActiveMap)))
			maps.Add(this.gameMapService.ActiveMap);

		foreach (var map in maps)
		{
			foreach (var type in new[] { StashType.TransferStash, StashType.RelicVaultStash })
			{
				string file = this.stashService.SharedStashFile(type, map);
				string? folder = PathIO.GetDirectoryName(file);
				if (this.sharedStashWatchers.ContainsKey(file) || folder is null || !Directory.Exists(folder))
					continue;

				var watcher = new FileSystemWatcher(folder, PathIO.GetFileName(file))
				{
					NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime,
					SynchronizingObject = this,
				};
				string watchedMap = map;
				watcher.Changed += (_, e) => this.SharedStashFile_Changed(e, type, watchedMap);
				watcher.EnableRaisingEvents = true;
				this.sharedStashWatchers[file] = watcher;
				// The form's container disposes it with the form.
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

		this.FileReloadDebouncer.Schedule(e.FullPath, () => this.mainView.ReloadSharedStash(type, map));
	}
}
