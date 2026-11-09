using Microsoft.Extensions.Logging;
using TQVaultAE.Presentation;
using TQVaultAE.Config;
using TQVaultAE.Domain.Helpers;
using System.ComponentModel;
using System.IO.Compression;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Services.GitBackup;

namespace TQVaultAE.Services;

/// <summary>
/// IGameFileService implementation
/// </summary>
public partial class GameFileService(ILogger<GameFileService> log, IGamePathService iGamePathService, IUserNotifier userNotifier, IFileIO fileIO, IPathIO pathIO, IDirectoryIO directoryIO, UserSettings userSettings) : IGameFileService
{
	protected readonly ILogger Log = log;
	protected readonly IGamePathService GamePathService = iGamePathService;
	protected readonly IUserNotifier UserNotifier = userNotifier;
	protected readonly IFileIO FileIO = fileIO;
	protected readonly IPathIO PathIO = pathIO;
	protected readonly IDirectoryIO DirectoryIO = directoryIO;

	protected readonly UserSettings UserSettings = userSettings;

	public void GitRepositorySetup()
	{
		if (!this.UserSettings.GitBackupEnabled)
			return;

		var sync = this.CreateGitBackupSync();
		this.RunGitBackup(sync.IsCloned ? Resources.GitUnableToPull : Resources.GitUnableToClone, sync.Synchronize);
	}

	public bool GitAddCommitTagAndPush()
	{
		if (!this.UserSettings.GitBackupEnabled || !GamePathService.LocalGitRepositoryGitDirExist)
			return false;

		var pushed = false;
		this.RunGitBackup(Resources.GitUnableToPush, () => pushed = this.CreateGitBackupSync().CommitAndPush());
		return pushed;
	}

	/// <summary>
	/// The folders the git backup holds: the vault files (and their Config folder, not the legacy Backups folder),
	/// and the two SaveData folders when character saves are backed up too.
	/// </summary>
	private GitBackupSync CreateGitBackupSync()
	{
		var jsonExtension = GamePathService.VaultFileNameExtensionJson;
		var oldExtension = GamePathService.VaultFileNameExtensionOld;
		bool IsVaultFile(string relative)
			=> (!relative.Contains('/')
				&& (relative.EndsWith(jsonExtension, StringComparison.OrdinalIgnoreCase) || relative.EndsWith(oldExtension, StringComparison.OrdinalIgnoreCase)))
			|| relative.StartsWith("Config/", StringComparison.OrdinalIgnoreCase);

		var folders = new List<GitBackupFolder>
		{
			new(GamePathService.VaultFilesDefaultDirName, GamePathService.TQVaultSaveFolder, IsVaultFile),
		};
		if (this.UserSettings.GitBackupPlayerSavesEnabled)
		{
			var saveData = GamePathService.SaveDataDirName;
			folders.Add(new($"{GamePathService.SaveDirNameTQ}/{saveData}", this.PathIO.Combine(GamePathService.SaveFolderTQ, saveData), _ => true));
			folders.Add(new($"{GamePathService.SaveDirNameTQIT}/{saveData}", this.PathIO.Combine(GamePathService.SaveFolderTQIT, saveData), _ => true));
		}

		return new GitBackupSync(this.Log, GamePathService.LocalGitRepositoryDirectory, this.UserSettings.GitBackupRepository, folders);
	}

	/// <summary>
	/// Runs a git backup step and shows what went wrong, so that a backup problem never stops the app.
	/// </summary>
	/// <param name="defaultMessage">Format of the message, with the repository URL as argument, when the failing step is not known.</param>
	private void RunGitBackup(string defaultMessage, Action step)
	{
		var repoUrl = this.UserSettings.GitBackupRepository;
		try
		{
			step();
		}
		catch (Exception ex)
		{
			var format = ex switch
			{
				GitCommandException { Command: "clone" } => Resources.GitUnableToClone,
				GitCommandException { Command: "fetch" } => Resources.GitUnableToFetch,
				GitCommandException { Command: "push" } => Resources.GitUnableToPush,
				_ => defaultMessage,
			};
			var detail = ex is Win32Exception
				? $"git: {ex.Message}" // git is not installed, or not on the PATH.
				: ex.Message;
			var errMess = string.Format(format, repoUrl) + Environment.NewLine + detail;
			this.Log.LogError(ex, "{Message}", errMess);
			this.UserNotifier.ShowError(errMess, Buttons: ShowMessageButtons.OK);
		}
	}

	public string? BackupFile(string prefix, string file)
	{
		if (this.FileIO.Exists(file))
		{
			// only backup if it exists!
			string backupFile = GamePathService.ConvertFilePathToBackupPath(prefix, file);

			this.FileIO.Copy(file, backupFile);

			// Added by VillageIdiot
			// Backup the file pairs for the player stash files.
			if (PathIO.GetFileName(file).Equals(GamePathService.PlayerStashFileNameB, StringComparison.OrdinalIgnoreCase))
			{
				string dxgfile = this.PathIO.ChangeExtension(file, ".dxg");

				if (this.FileIO.Exists(dxgfile))
				{
					// only backup if it exists!
					backupFile = GamePathService.ConvertFilePathToBackupPath(prefix, dxgfile);

					this.FileIO.Copy(dxgfile, backupFile);
				}
			}

			return backupFile;
		}
		else
			return null;
	}

	public string GetDuplicateCharacterFolder(string playerSaveDirectory, string newname)
		=> this.PathIO.Combine(this.PathIO.GetDirectoryName(playerSaveDirectory), $"_{newname}");

	public string DuplicateCharacterFiles(string playerSaveDirectory, string newname)
	{
		var newFolder = this.GetDuplicateCharacterFolder(playerSaveDirectory, newname);
		var newPlayerFile = this.PathIO.Combine(newFolder, GamePathService.PlayerSaveFileName);

		var playerFile = this.PathIO.Combine(playerSaveDirectory, GamePathService.PlayerSaveFileName);
		var stashFileB = this.PathIO.Combine(playerSaveDirectory, GamePathService.PlayerStashFileNameB);
		var stashFileG = this.PathIO.Combine(playerSaveDirectory, GamePathService.PlayerStashFileNameG);
		var settingsFile = this.PathIO.Combine(playerSaveDirectory, GamePathService.PlayerSettingsFileName);

		DirectoryIO.CreateDirectory(newFolder);
		this.FileIO.Copy(playerFile, newPlayerFile);
		if (this.FileIO.Exists(stashFileB)) this.FileIO.Copy(stashFileB, this.PathIO.Combine(newFolder, GamePathService.PlayerStashFileNameB));
		if (this.FileIO.Exists(stashFileG)) this.FileIO.Copy(stashFileG, this.PathIO.Combine(newFolder, GamePathService.PlayerStashFileNameG));
		if (this.FileIO.Exists(settingsFile)) this.FileIO.Copy(settingsFile, this.PathIO.Combine(newFolder, GamePathService.PlayerSettingsFileName));

		// Copy Progression directory
		var sourceProgressionDir = this.PathIO.Combine(playerSaveDirectory, "Levels_World_World01.map");
		var destProgressionDir = this.PathIO.Combine(newFolder, "Levels_World_World01.map");
		if (DirectoryIO.Exists(sourceProgressionDir))
		{
			CopyDirectoryRecursive(sourceProgressionDir, destProgressionDir);
		}

		return newFolder;
	}

	private void CopyDirectoryRecursive(string sourceDir, string destDir)
	{
		if (!DirectoryIO.Exists(destDir))
			DirectoryIO.CreateDirectory(destDir);

		foreach (var file in DirectoryIO.GetFiles(sourceDir))
		{
			var destFile = this.PathIO.Combine(destDir, this.PathIO.GetFileName(file));
			this.FileIO.Copy(file, destFile, true);
		}

		foreach (var subDir in DirectoryIO.GetDirectories(sourceDir))
		{
			var destSubDir = this.PathIO.Combine(destDir, this.PathIO.GetFileName(subDir));
			CopyDirectoryRecursive(subDir, destSubDir);
		}
	}

	public void BackupStupidPlayerBackupFolder(string playerFile)
	{
		string playerFolder = this.PathIO.GetDirectoryName(playerFile);
		string backupFolder = this.PathIO.Combine(playerFolder, "Backup");
		string newFolder = this.PathIO.Combine(playerFolder, "Backup-moved by TQVault");
		var existsbackupFolder = DirectoryIO.Exists(backupFolder);
		var existsnewFolder = DirectoryIO.Exists(newFolder);

		if (existsbackupFolder)
		{
			// we need to move it.
			if (existsnewFolder)
			{
				try
				{
					// It already exists--we need to remove it
					DirectoryIO.Delete(newFolder, true);
				}
				catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
				{
					this.Log.LogWarning(ex, "Unable to remove {Folder}, using a numbered folder instead", newFolder);
					int fn = 1;
					while (DirectoryIO.Exists(string.Format("{0}({1})", newFolder, fn)))
					{
						fn++;
					}
					newFolder = string.Format("{0}({1})", newFolder, fn);
				}
			}

			DirectoryIO.Move(backupFolder, newFolder);
		}

		// Change TQVault moved backup to zip to avoid game reading it's content : https://github.com/EtienneLamoureux/TQVaultAE/issues/535
		existsnewFolder = DirectoryIO.Exists(newFolder);
		if (existsnewFolder)
		{
			var zipFile = $"{newFolder}.zip";
			var zipAlreadyExists= FileIO.Exists(zipFile);

			// Delete old zip file
			if (zipAlreadyExists)
			{
				try
				{
					FileIO.Delete(zipFile);
					zipAlreadyExists = false;
				}
				catch (Exception e)
				{
					this.Log.LogWarning(e, "Unable to delete {zipFile}", zipFile);
				}
			}
			
			// Create new one
			if (!zipAlreadyExists)
			{
				try
				{
					ZipFile.CreateFromDirectory(newFolder, zipFile, CompressionLevel.Fastest, false);
					DirectoryIO.Delete(newFolder, true);
				}
				catch (Exception e)
				{
					this.Log.LogWarning(e, "Unable to zip new {zipFile}", zipFile);
				}
			}
		}
	}

	public bool Archive(PlayerSave ps)
	{
		if (ps is null)
			return false;

		if (ps.IsArchived)// Double Check
			return true;

		// Disable filewatchers
		ToggleFileWatchers(ps, false);

		// Move directory
		var oldFolder = ps.Folder;

		// -- Prep arbo
		var archDirNameTarget = this.GamePathService.GetBaseCharacterFolder(ps.IsImmortalThrone, ps.IsCustom, true);
		if (!DirectoryIO.Exists(archDirNameTarget))
			DirectoryIO.CreateDirectory(archDirNameTarget);

		var newFolder = GamePathService.ArchiveTogglePath(oldFolder);

		Exception? ex = null;
		try
		{
			DirectoryIO.Move(oldFolder, newFolder);
		}
		catch (Exception exx)
		{
			ex = exx;
			var errMess = "Error during move directory!";
			this.Log.LogError(ex, "{Message}", errMess);
			this.UserNotifier.ShowError(errMess, ex, Buttons: ShowMessageButtons.OK);
		}
		finally
		{
			// Update PlayerSave
			if (ex is null)
			{
				ps.IsArchived = true;
				ps.Folder = newFolder;

				if (ps.PlayerSaveWatcher is not null)
					ps.PlayerSaveWatcher.Path = newFolder;

				if (ps.PlayerStashWatcher is not null)
					ps.PlayerStashWatcher.Path = newFolder;
			}
			// Restart filewatchers
			ToggleFileWatchers(ps, true);
		}

		return ex is null;
	}

	static void ToggleFileWatchers(PlayerSave ps, bool enable)
	{
		if (ps.PlayerSaveWatcher is not null)
			ps.PlayerSaveWatcher.EnableRaisingEvents = enable;

		if (ps.PlayerStashWatcher is not null)
			ps.PlayerStashWatcher.EnableRaisingEvents = enable;
	}

	public bool Unarchive(PlayerSave ps)
	{
		if (ps is null)
			return false;

		if (!ps.IsArchived)// Double Check
			return true;

		// Disable filewatchers
		ToggleFileWatchers(ps, false);

		// Move directory
		var oldFolder = ps.Folder;// Should include archDirName

		var newFolder = GamePathService.ArchiveTogglePath(oldFolder);

		Exception? ex = null;
		try
		{
			DirectoryIO.Move(oldFolder, newFolder);
		}
		catch (Exception exx)
		{
			ex = exx;
			var errMess = "Error during move directory!";
			this.Log.LogError(ex, "{Message}", errMess);
			this.UserNotifier.ShowError(errMess, ex, Buttons: ShowMessageButtons.OK);
		}
		finally
		{
			// Update PlayerSave
			if (ex is null)
			{
				ps.IsArchived = false;
				ps.Folder = newFolder;

				if (ps.PlayerSaveWatcher is not null)
					ps.PlayerSaveWatcher.Path = newFolder;

				if (ps.PlayerStashWatcher is not null)
					ps.PlayerStashWatcher.Path = newFolder;
			}

			// Restart filewatchers
			ToggleFileWatchers(ps, true);
		}

		return ex is null;
	}
}