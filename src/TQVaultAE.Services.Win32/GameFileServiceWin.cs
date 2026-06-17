using Microsoft.Extensions.Logging;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Presentation;
using Medallion.Shell;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Config;

namespace TQVaultAE.Services.Win32;

/// <summary>
/// Win32 implementation of <see cref="GameFileService"/>
/// </summary>
public class GameFileServiceWin : GameFileService
{
	public GameFileServiceWin(
		ILogger<GameFileServiceWin> log
		, IGamePathService iGamePathService
		, IUIService iUIService
		, IFileIO fileIO
		, IPathIO pathIO
		, IDirectoryIO directoryIO
		, UserSettings userSettings
	) : base(log, iGamePathService, iUIService, fileIO, pathIO, directoryIO, userSettings)
	{
	}

	/// <summary>
	/// Recursively copies a directory and its contents
	/// </summary>
	/// <param name="sourceDir">Source directory path</param>
	/// <param name="destDir">Destination directory path</param>
	/// <param name="overwrite">Whether to overwrite existing files</param>
	private void CopyDirectory(string sourceDir, string destDir, bool overwrite)
	{
		if (!Directory.Exists(destDir))
			Directory.CreateDirectory(destDir);

		foreach (var file in Directory.GetFiles(sourceDir))
		{
			var destFile = Path.Combine(destDir, Path.GetFileName(file));
			if (overwrite || !File.Exists(destFile))
				File.Copy(file, destFile, overwrite);
		}

		foreach (var subDir in Directory.GetDirectories(sourceDir))
		{
			var destSubDir = Path.Combine(destDir, Path.GetFileName(subDir));
			CopyDirectory(subDir, destSubDir, overwrite);
		}
	}

	protected override void CopySaveDataFromRepoToMyGames(string RepoTQPath, string TQPathSaveData, bool overrideFiles)
	{
		// Copy only if it's not a junction because if it is, that "git pull" did that job.

		// check if it's a junction already
		var LocalGitRepoDir = GamePathService.LocalGitRepositoryDirectory;
		var SaveDataParent = this.PathIO.GetDirectoryName(RepoTQPath);
		var dirfilter = this.PathIO.Combine(SaveDataParent, @$"*{GamePathService.SaveDataDirName}*");
		string errMess = string.Format(Resources.UnableToExecute, "DIR /A " + dirfilter);
		var dir = Command.Run("CMD.EXE", "/C", "DIR", "/A", dirfilter);
		if (HandleExecuteOut(dir, errMess, out var diroutStd, out var direrrStd))
		{
			var diroutSdtStr = diroutStd.JoinString(" ");
			var isAJunction = diroutSdtStr.Contains("<JUNCTION>");
			var isADIR = diroutSdtStr.Contains("<DIR>");

			if (isAJunction && overrideFiles)
			{
				var filesToRestore = this.PathIO.Combine(RepoTQPath, "*");
				// New save files (untracked) will not be deleted (you can only restore files that are tracked Deleted/Modified)
				errMess = string.Format(Resources.UnableToExecute, @"git.exe restore """ + filesToRestore + @"""");
				var restore = Command.Run("git", new[] { "restore", filesToRestore }, (opt) => { opt.WorkingDirectory(LocalGitRepoDir); });
				HandleExecuteOut(restore, errMess, out var restoreoutStd, out var restoreerrStd);
			}
			else if (isAJunction && !overrideFiles)
			{
				// Git Pull did that job already
			}
			else if (isADIR)
			{
				// First time after that "git clone"

				//			if Yes - Copy Repo files to My Games override file
				//			if No - Copy Repo files to My Games ignore if existing file
				try
				{
					CopyDirectory(RepoTQPath, TQPathSaveData, overrideFiles);
				}
				catch (IOException ioex) when (ioex.Data.Count > 0 && !overrideFiles)
				{
					// Exception at that end of that directory merge when there is files colision and overrideFiles = false
					// => Do nothing
				}
			}
		}
	}

	protected override void CopyVaultFilesFromRepoToVaultData(bool overrideFiles)
	{
		// Copy only if it's not a junction because if it is, that "git pull" did that job.

		// check if it's a junction already
		var LocalGitRepoDir = GamePathService.LocalGitRepositoryDirectory;
		var repoVaultFilesPath = this.PathIO.Combine(LocalGitRepoDir, GamePathService.VaultFilesDefaultDirName);
		if (this.DirectoryIO.Exists(repoVaultFilesPath))
		{
			var dirfilter = this.PathIO.Combine(LocalGitRepoDir, @$"*{GamePathService.VaultFilesDefaultDirName}*");
			string errMess = string.Format(Resources.UnableToExecute, "DIR /A " + dirfilter);
			var dir = Command.Run("CMD.EXE", "/C", "DIR", "/A", dirfilter);
			if (HandleExecuteOut(dir, errMess, out var diroutStd, out var direrrStd))
			{
				var diroutSdtStr = diroutStd.JoinString(" ");
				var isAJunction = diroutSdtStr.Contains("<JUNCTION>");
				var isADIR = diroutSdtStr.Contains("<DIR>");

				if (isAJunction && overrideFiles)
				{
					var filesToRestore = this.PathIO.Combine(repoVaultFilesPath, "*");// New vault (untracked) will not be deleted (you can only restore files that are tracked Deleted/Modified)

					errMess = string.Format(Resources.UnableToExecute, @"git.exe restore """ + filesToRestore + @"""");
					var restore = Command.Run("git", new[] { "restore", filesToRestore }, (opt) => { opt.WorkingDirectory(LocalGitRepoDir); });
					HandleExecuteOut(restore, errMess, out var restoreoutStd, out var restoreerrStd);
				}
				else if (isAJunction && !overrideFiles)
				{
					// Git Pull did that job already
				}
				else if (isADIR)
				{
					// First time after that "git clone"

					//			if Yes - Copy Repo Vault files to TQVaultData (Vault Directory) override file
					//			if No - Copy Repo Vault files to TQVaultData (Vault Directory) ignore if existing file
					if (this.DirectoryIO.Exists(repoVaultFilesPath))
					{
						var repoVaultFilesJson = this.DirectoryIO.GetFiles(repoVaultFilesPath, '*' + GamePathService.VaultFileNameExtensionJson);
						var repoVaultFilesClassic = this.DirectoryIO.GetFiles(repoVaultFilesPath, '*' + GamePathService.VaultFileNameExtensionOld);
						foreach (var vault in new[] { repoVaultFilesJson, repoVaultFilesClassic }.SelectMany(f => f))
						{
							var destName = this.PathIO.Combine(GamePathService.TQVaultSaveFolder, this.PathIO.GetFileName(vault));

							if (!overrideFiles && this.FileIO.Exists(destName))
								continue;

							this.FileIO.Copy(vault, destName, overrideFiles);
						}
					}
				}
			}
		}
	}
	protected override bool RemoveRepoTQVaultData()
	{
		// check if it's a junction already
		var dirfilter = this.PathIO.Combine(GamePathService.LocalGitRepositoryDirectory, @$"*{GamePathService.VaultFilesDefaultDirName}*");
		string errMess = string.Format(Resources.UnableToExecute, "DIR /A " + dirfilter);
		var dir = Command.Run("CMD.EXE", "/C", "DIR", "/A", dirfilter);
		if (HandleExecuteOut(dir, errMess, out var diroutStd, out var direrrStd))
		{
			var saveDataPath = this.PathIO.Combine(GamePathService.LocalGitRepositoryDirectory, GamePathService.VaultFilesDefaultDirName);
			return SafelyRemoveJunction(diroutStd.JoinString(" "), saveDataPath);
		}
		return false;
	}

	/// <summary>
	/// Make Junction into $Repo for that TQVault directory
	/// </summary>
	/// <returns></returns>
	protected override bool LinkRepoTQVaultData()
	{
		var junctionPath = this.PathIO.Combine(GamePathService.LocalGitRepositoryDirectory, GamePathService.VaultFilesDefaultDirName);
		string sourcePath = GamePathService.TQVaultSaveFolder;
		string errMess = string.Format(Resources.UnableToExecute, string.Format("MKLINK /J {0} {1}", junctionPath, GamePathService.TQVaultSaveFolder));

		var mklink = Command.Run("CMD.EXE", "/C", "MKLINK", "/J", junctionPath, sourcePath);
		return HandleExecuteOut(mklink, errMess, out var mklinkoutStd, out var mklinkerrStd);
	}

	/// <summary>
	/// Make Junction into $Repo for that 2 SaveData directories
	/// </summary>
	protected override bool LinkRepoSaveData(
		string TQPathSaveData, bool TQPathSaveDataExists, string RepoTQPath
		, string TQITPathSaveData, bool TQITPathSaveDataExists, string RepoTQITPath
	)
	{
		bool TQSuccess = false, TQITSuccess = false;

		if (TQPathSaveDataExists)
		{
			var junctionPathParent = this.PathIO.Combine(GamePathService.LocalGitRepositoryDirectory, GamePathService.SaveDirNameTQ);
			this.DirectoryIO.CreateDirectory(junctionPathParent);
			string errMess = string.Format(Resources.UnableToExecute, string.Format("MKLINK /J {0} {1}", RepoTQPath, TQPathSaveData));
			var mklink = Command.Run("CMD.EXE", "/C", "MKLINK", "/J", RepoTQPath, TQPathSaveData);
			TQSuccess = HandleExecuteOut(mklink, errMess, out var mklinkoutStd, out var mklinkerrStd);
		}

		if (TQITPathSaveDataExists)
		{
			var junctionPathParent = this.PathIO.Combine(GamePathService.LocalGitRepositoryDirectory, GamePathService.SaveDirNameTQIT);
			this.DirectoryIO.CreateDirectory(junctionPathParent);
			string errMess = string.Format(Resources.UnableToExecute, string.Format("MKLINK /J {0} {1}", RepoTQITPath, TQITPathSaveData));
			var mklink = Command.Run("CMD.EXE", "/C", "MKLINK", "/J", RepoTQITPath, TQITPathSaveData);
			TQITSuccess = HandleExecuteOut(mklink, errMess, out var mklinkoutStd, out var mklinkerrStd);
		}

		return TQSuccess || TQITSuccess;
	}

	/// <summary>
	/// Delete $Repo\Titan Quest\SaveData and $Repo\Titan Quest - Immortal Throne\SaveData safely
	/// </summary>
	protected override bool RemoveRepoSaveData(bool RepoTQPathExists, bool RepoTQITPathExists)
	{
		bool TQSuccess = false, TQITSuccess = false;
		if (RepoTQPathExists)
		{
			var TQPath = this.PathIO.Combine(GamePathService.LocalGitRepositoryDirectory, GamePathService.SaveDirNameTQ);
			TQSuccess = SafelyRemoveRepoSaveData(TQPath);
		}
		if (RepoTQITPathExists)
		{
			var TQITPath = this.PathIO.Combine(GamePathService.LocalGitRepositoryDirectory, GamePathService.SaveDirNameTQIT);
			TQITSuccess = SafelyRemoveRepoSaveData(TQITPath);
		}
		return TQSuccess || TQITSuccess;

		bool SafelyRemoveRepoSaveData(string tqpath)
		{
			// check if it's a junction already
			var dirfilter = this.PathIO.Combine(tqpath, @$"*{GamePathService.SaveDataDirName}*");
			string errMess = string.Format(Resources.UnableToExecute, "DIR /A \"" + dirfilter + '"');
			var dir = Command.Run("CMD.EXE", "/C", "DIR", "/A", dirfilter);
			if (HandleExecuteOut(dir, errMess, out var diroutStd, out var direrrStd))
			{
				var junctionPath = this.PathIO.Combine(tqpath, GamePathService.SaveDataDirName);

				return SafelyRemoveJunction(diroutStd.JoinString(" "), junctionPath);
			}
			return false; // DIR failed ? hard to believe
		}
	}

	// TODO No need to be so delicate. RMDIR is smart enough to take care respectfuly of that junction (no recursion)
	private bool SafelyRemoveJunction(string commandDirStdOutput, string junctionPath)
	{
		// Is a junction already
		if (commandDirStdOutput.Contains("<JUNCTION>"))
		{
			// Detach that junction
			var errMess = string.Format(Resources.UnableToExecute, "fsutil.exe reparsepoint delete \"" + junctionPath + '"');
			var fsutil = Command.Run("fsutil.exe", "reparsepoint", "delete", junctionPath);
			if (HandleExecuteOut(fsutil, errMess, out var fsutiloutStd, out var fsutilerrStd))
			{
				// Remove dir
				return RmDir(junctionPath);
			}
			return false;// fsutil failed
		}
		// Exists but it's a dir already
		else if (commandDirStdOutput.Contains("<DIR>"))
		{
			// Remove dir
			return RmDir(junctionPath);
		}
		// No directory here
		return true;
	}

	bool RmDir(string dirToDeletePath)
	{
		var errMess = string.Format(Resources.UnableToExecute, "rmdir /S /Q \"" + dirToDeletePath + '"');
		var rmdir = Command.Run("CMD.EXE", "/C", "rmdir", "/S", "/Q", dirToDeletePath);
		return HandleExecuteOut(rmdir, errMess, out var rmdiroutStd, out var rmdirerrStd);
	}
}