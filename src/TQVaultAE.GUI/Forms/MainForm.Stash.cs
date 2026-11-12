﻿using System.Globalization;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Components.Panels;
using TQVaultAE.GUI.Components;
using TQVaultAE.GUI.Models;
using TQVaultAE.Presentation;
using Microsoft.Extensions.Logging;
using TQVaultAE.Application;
using TQVaultAE.Application.Results;

namespace TQVaultAE.GUI.Forms;

public partial class MainForm
{
	/// <summary>
	/// Creates the stash panel
	/// </summary>
	private void CreateStashPanel()
	{
		// size params are width, height
		Size panelSize = new Size(17, 16);

		this.stashPanel = new StashPanel(this.DragInfo, panelSize, this.Services);

		// New location in bottom right of the Main Form.
		// Align to playerPanel

		this.stashPanel.DrawAsGroupBox = false;

		this.stashPanel.OnNewItemHighlighted += new EventHandler<SackPanelEventArgs>(this.NewItemHighlightedCallback);
		this.stashPanel.OnAutoMoveItem += new EventHandler<SackPanelEventArgs>(this.AutoMoveItemCallback);
		this.stashPanel.OnItemSelected += new EventHandler<SackPanelEventArgs>(this.ItemSelectedCallback);
		this.stashPanel.OnClearAllItemsSelected += new EventHandler<SackPanelEventArgs>(this.ClearAllItemsSelectedCallback);

		this.flowLayoutPanelRightPanels.Controls.Add(this.stashPanel);
		this.stashPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top;
		this.stashPanel.Margin = new Padding(0);
	}

	/// <summary>
	/// Loads and shows the transfer and relic vault stashes, unless the selected character is a Titan Quest original one.
	/// </summary>
	private void LoadSharedStashes()
	{
		var selectedSave = this.comboBoxCharacter.SelectedItem as PlayerSave;
		this.ShowTransferStash(this.stashService.LoadTransferStashFor(selectedSave), false);
		this.ShowRelicVaultStash(this.stashService.LoadRelicVaultStashFor(selectedSave), false);
	}

	/// <summary>
	/// Shows the loaded transfer stash: its messages, then the stash in the panel (unless reloaded by the file watcher).
	/// </summary>
	/// <param name="result">The load; null when the character has no transfer stash, which clears the panel.</param>
	private void ShowTransferStash(StashLoadResult? result, bool fromFileWatcher)
	{
		if (result is null)
		{
			this.stashPanel.TransferStash = null;
			return;
		}

		this.ShowLoaded(
			LoadMessages.ForTransferStash(result)
			, result.StashFile
			, Resources.MainFormStashReadError
			, () =>
			{
				if (!fromFileWatcher)
					this.stashPanel.TransferStash = result.Stash;
			}
			, () => this.stashPanel.TransferStash = null);
	}

	/// <summary>
	/// Shows the loaded relic vault stash: its messages, then the stash in the panel (unless reloaded by the file watcher).
	/// </summary>
	/// <param name="result">The load; null when the character has no relic vault stash, which clears the panel.</param>
	private void ShowRelicVaultStash(StashLoadResult? result, bool fromFileWatcher)
	{
		if (result is null)
		{
			this.stashPanel.RelicVaultStash = null;
			return;
		}

		this.ShowLoaded(
			LoadMessages.ForRelicVaultStash(result)
			, result.StashFile
			, Resources.MainFormStashReadError
			, () =>
			{
				if (!fromFileWatcher)
					this.stashPanel.RelicVaultStash = result.Stash;
			}
			, () => this.stashPanel.RelicVaultStash = null);
	}

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

		this.FileReloadDebouncer.Schedule(e.FullPath, () =>
		{
			var stashResult = this.stashService.ReloadSharedStash(type, map);
			if (stashResult is null)
				return;

			// The panel shows the active map's shared stashes, unless the character shown has none (Titan Quest original).
			var selected = this.comboBoxCharacter.SelectedItem as PlayerSave;
			bool shown = GameMaps.Same(map, this.gameMapService.ActiveMap) && (selected is null || selected.IsImmortalThrone);
			if (type == StashType.TransferStash)
			{
				this.ShowTransferStash(stashResult, true);
				if (shown)
					this.stashPanel.TransferStash = stashResult.Stash;
			}
			else
			{
				this.ShowRelicVaultStash(stashResult, true);
				if (shown)
					this.stashPanel.RelicVaultStash = stashResult.Stash;
			}
		});
	}
}
