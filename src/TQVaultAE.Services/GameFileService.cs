using Microsoft.Extensions.Logging;
using TQVaultAE.Application.Session;
using TQVaultAE.Config;
using TQVaultAE.Domain.Helpers;
using System.IO.Compression;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Results;
using TQVaultAE.Services.GitBackup;

namespace TQVaultAE.Services;

/// <summary>
/// IGameFileService implementation
/// </summary>
public class GameFileService(ILogger<GameFileService> log, IGamePathService iGamePathService, IFileIO fileIO, IPathIO pathIO, IDirectoryIO directoryIO, UserSettings userSettings) : IGameFileService
{
	private readonly ILogger Log = log;
	private readonly IGamePathService GamePathService = iGamePathService;
	private readonly IFileIO FileIO = fileIO;
	private readonly IPathIO PathIO = pathIO;
	private readonly IDirectoryIO DirectoryIO = directoryIO;

	private readonly UserSettings UserSettings = userSettings;

	public GitBackupError? GitRepositorySetup()
	{
		if (!this.UserSettings.GitBackupEnabled)
			return null;

		var sync = this.CreateGitBackupSync();
		return this.RunGitBackup(sync.IsCloned ? GitBackupStep.Pull : GitBackupStep.Clone, sync.Synchronize);
	}

	public GitPushResult GitAddCommitTagAndPush()
	{
		if (!this.UserSettings.GitBackupEnabled || !GamePathService.LocalGitRepositoryGitDirExist)
			return new(false);

		var pushed = false;
		var error = this.RunGitBackup(GitBackupStep.Push, () => pushed = this.CreateGitBackupSync().CommitAndPush());
		return new(pushed, error);
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
	/// Runs a git backup step and returns what went wrong, so that a backup problem never stops the app.
	/// </summary>
	/// <param name="defaultStep">The step reported when the failing git command is not known.</param>
	/// <returns>The step that failed, or null.</returns>
	private GitBackupError? RunGitBackup(GitBackupStep defaultStep, Action step)
	{
		var repoUrl = this.UserSettings.GitBackupRepository;
		try
		{
			step();
			return null;
		}
		catch (Exception ex)
		{
			var failed = ex switch
			{
				GitCommandException { Command: "clone" } => GitBackupStep.Clone,
				GitCommandException { Command: "fetch" } => GitBackupStep.Fetch,
				GitCommandException { Command: "push" } => GitBackupStep.Push,
				_ => defaultStep,
			};
			this.Log.LogError(ex, "Git backup: {Step} of {Repository} failed", failed, repoUrl);
			return new GitBackupError(failed, repoUrl, ex);
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
		DirectoryIO.CreateDirectory(newFolder);

		// Every file of the character that exists: its stash, its loadouts (copies of the pieces, like its gear) and its
		// mod link, so the copy plays with the same mod. The player file is copied even if missing, which fails.
		foreach (var file in this.GamePathService.CharacterFiles())
		{
			var source = file.In(this.PathIO, playerSaveDirectory);
			if (file.Name == this.GamePathService.PlayerSaveFileName || this.FileIO.Exists(source))
				this.FileIO.Copy(source, file.In(this.PathIO, newFolder));
		}

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

	/// <inheritdoc/>
	public void ToggleArchive(PlayerSave ps, bool archive)
	{
		ArgumentNullException.ThrowIfNull(ps);

		if (ps.IsArchived == archive)
			return;

		// The archive folder is made at the first archive.
		if (archive)
		{
			var archiveFolder = this.GamePathService.GetBaseCharacterFolder(ps.IsImmortalThrone, ps.IsCustom, true);
			if (!this.DirectoryIO.Exists(archiveFolder))
				this.DirectoryIO.CreateDirectory(archiveFolder);
		}

		var newFolder = this.GamePathService.ArchiveTogglePath(ps.Folder);

		// No hot reload while the folder moves; a failed move leaves the save and its watchers as they were.
		using (ps.PauseWatching())
		{
			this.DirectoryIO.Move(ps.Folder, newFolder);
			ps.MovedTo(newFolder, archive);
		}
	}
}