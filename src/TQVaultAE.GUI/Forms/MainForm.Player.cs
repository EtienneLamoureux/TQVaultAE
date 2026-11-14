using System.Globalization;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Components.Panels;
using TQVaultAE.GUI.Components;
using TQVaultAE.GUI.Dialogs;
using TQVaultAE.GUI.Infrastructure;
using TQVaultAE.GUI.Models;
using TQVaultAE.Presentation;
using Microsoft.Extensions.Logging;
using TQVaultAE.Application;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Results;

namespace TQVaultAE.GUI.Forms;

public partial class MainForm
{
	private IReadOnlyCollection<string>? _watchedPlayerFiles = null;
	private IEnumerable<string> WatchedPlayerFiles
	{
		get
		{
			if (_watchedPlayerFiles is null)
				_watchedPlayerFiles = new List<string> { this.GamePathResolver.PlayerSaveFileName, this.GamePathResolver.PlayerStashFileNameB }.AsReadOnly();

			return _watchedPlayerFiles;
		}
	}

	/// <summary>
	/// Gets the list of available player files, watched when hot reload is on.
	/// </summary>
	private PlayerSave[] GetPlayerList()
	{
		var characters = this.playerService.GetPlayerSaveList();

		// Init FileWatcher
		if (base.USettings.EnableHotReload)
		{
			foreach (var ps in characters)
			{
				ps.PlayerSaveWatcher = CreateFileWatcher(ps.Folder, this.GamePathResolver.PlayerSaveFileName);
				ps.PlayerStashWatcher = CreateFileWatcher(ps.Folder, this.GamePathResolver.PlayerStashFileNameB);
			}
		}

		return characters;
	}

	/// <summary>
	/// Coalesces the change events of the game save files (player, player stash, caravan and relic stash).
	/// </summary>
	private FileReloadDebouncer? _fileReloadDebouncer;
	private FileReloadDebouncer FileReloadDebouncer => this._fileReloadDebouncer ??= new FileReloadDebouncer(this.Log);

	// Raised on the UI thread (SynchronizingObject = this)
	private void PlayerSaveFile_Changed(object? sender, FileSystemEventArgs e)
	{
		if (e.ChangeType != WatcherChangeTypes.Changed
			|| !this.WatchedPlayerFiles.Any(f => f.Equals(e.Name, StringComparison.OrdinalIgnoreCase))
		) return;

		var fw = (FileSystemWatcher)sender!;
		var folder = fw.Path;
		var fileName = e.Name;

		this.ScheduleReload(e.FullPath, () => this.mainView.ReloadCharacterFile(folder, fileName));
	}


	private FileSystemWatcher CreateFileWatcher(string folder, string filename)
	{
		// the game write save file when inventory is opened and again when it's closed. We can't distinguish between both events.
		var fw = new FileSystemWatcher();
		fw.BeginInit();
		fw.Path = folder;
		fw.Filter = filename;
		fw.IncludeSubdirectories = false;
		fw.NotifyFilter = NotifyFilters.Attributes;// Attributes triggers after file update
		fw.SynchronizingObject = this;
		fw.Changed += new FileSystemEventHandler(this.PlayerSaveFile_Changed);
		fw.EnableRaisingEvents = true;
		fw.EndInit();
		return fw;
	}



}
