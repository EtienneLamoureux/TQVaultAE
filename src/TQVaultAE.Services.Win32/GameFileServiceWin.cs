using Microsoft.Extensions.Logging;
using System.IO;
using TQVaultAE.Domain.Contracts.Services;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Presentation;
using Medallion.Shell;
using System.Linq;
using Microsoft.VisualBasic.Devices;
using Microsoft.VisualBasic.MyServices;

namespace TQVaultAE.Services.Win32
{
	/// <summary>
	/// Win32 implementation of <see cref="GameFileService"/>
	/// </summary>
	public class GameFileServiceWin : GameFileService
	{
		private readonly FileSystemProxy FS;

		public GameFileServiceWin(ILogger<GameFileServiceWin> log, IGamePathService iGamePathService, IUIService iUIService)
			: base(log, iGamePathService, iUIService)
		{
			FS = new Computer().FileSystem;
		}

		protected override void CopySaveDataFromRepoToMyGames(string RepoTQPath, string TQPathSaveData, bool overrideFiles)
		{
			// Copy only if it's not a junction because if it is, the "git pull" did the job.

			// check if it's a junction already
			var LocalGitRepoDir = GamePathService.LocalGitRepositoryDirectory;
			var SaveDataParent = Path.GetDirectoryName(RepoTQPath);
			var dirfilter = Path.Combine(SaveDataParent, @$"*{SAVEDATA_DIRNAME}*");
			string errMess = string.Format(Resources.UnableToExecute, "DIR /A " + dirfilter);
			var dir = Command.Run("CMD.EXE", "/C", "DIR", "/A", dirfilter);
			if (HandleExecuteOut(dir, errMess, out var diroutStd, out var direrrStd))
			{
				var diroutSdtStr = diroutStd.JoinString(" ");
				var isAJunction = diroutSdtStr.Contains("<JUNCTION>");
				var isADIR = diroutSdtStr.Contains("<DIR>");

				if (isAJunction && overrideFiles)
				{
					var filesToRestore = Path.Combine(RepoTQPath, "*");
					// New save files (untracked) will not be deleted (you can only restore files that are tracked Deleted/Modified)
					errMess = string.Format(Resources.UnableToExecute, @"git.exe restore """ + filesToRestore + @"""");
					var restore = Command.Run("git", new[] { "restore", filesToRestore }, (opt) => { opt.WorkingDirectory(LocalGitRepoDir); });
					HandleExecuteOut(restore, errMess, out var restoreoutStd, out var restoreerrStd);
				}
				else if (isAJunction && !overrideFiles)
				{
					// Git Pull did the job already
				}
				else if (isADIR)
				{
					// First time after the "git clone"

					//			if Yes - Copy Repo files to My Games override file
					//			if No - Copy Repo files to My Games ignore if existing file
					FS.CopyDirectory(RepoTQPath, TQPathSaveData, overrideFiles);
				}
			}
		}

		protected override void CopyVaultFilesFromRepoToVaultData(bool overrideFiles)
		{
			// Copy only if it's not a junction because if it is, the "git pull" did the job.

			// check if it's a junction already
			var LocalGitRepoDir = GamePathService.LocalGitRepositoryDirectory;
			var repoVaultFilesPath = Path.Combine(LocalGitRepoDir, VAULTFILES_REPO_DIRNAME);
			if (Directory.Exists(repoVaultFilesPath))
			{
				var dirfilter = Path.Combine(LocalGitRepoDir, @$"*{VAULTFILES_REPO_DIRNAME}*");
				string errMess = string.Format(Resources.UnableToExecute, "DIR /A " + dirfilter);
				var dir = Command.Run("CMD.EXE", "/C", "DIR", "/A", dirfilter);
				if (HandleExecuteOut(dir, errMess, out var diroutStd, out var direrrStd))
				{
					var diroutSdtStr = diroutStd.JoinString(" ");
					var isAJunction = diroutSdtStr.Contains("<JUNCTION>");
					var isADIR = diroutSdtStr.Contains("<DIR>");

					if (isAJunction && overrideFiles)
					{
						var filesToRestore = Path.Combine(repoVaultFilesPath, "*");// New vault (untracked) will not be deleted (you can only restore files that are tracked Deleted/Modified)

						errMess = string.Format(Resources.UnableToExecute, @"git.exe restore """ + filesToRestore + @"""");
						var restore = Command.Run("git", new[] { "restore", filesToRestore }, (opt) => { opt.WorkingDirectory(LocalGitRepoDir); });
						HandleExecuteOut(restore, errMess, out var restoreoutStd, out var restoreerrStd);
					}
					else if (isAJunction && !overrideFiles)
					{
						// Git Pull did the job already
					}
					else if (isADIR)
					{
						// First time after the "git clone"

						//			if Yes - Copy Repo Vault files to TQVaultData (Vault Directory) override file
						//			if No - Copy Repo Vault files to TQVaultData (Vault Directory) ignore if existing file
						if (Directory.Exists(repoVaultFilesPath))
						{
							var repoVaultFilesJson = Directory.GetFiles(repoVaultFilesPath, '*' + GamePathService.VaultFileNameExtensionJson);
							var repoVaultFilesClassic = Directory.GetFiles(repoVaultFilesPath, '*' + GamePathService.VaultFileNameExtensionOld);
							foreach (var vault in new[] { repoVaultFilesJson, repoVaultFilesClassic }.SelectMany(f => f))
							{
								var destName = Path.Combine(GamePathService.TQVaultSaveFolder, Path.GetFileName(vault));

								if (!overrideFiles && File.Exists(destName))
									continue;

								File.Copy(vault, destName, overrideFiles);
							}
						}
					}
				}
			}
		}
		protected override bool RemoveRepoTQVaultData()
		{
			// check if it's a junction already
			var dirfilter = Path.Combine(GamePathService.LocalGitRepositoryDirectory, @$"*{VAULTFILES_REPO_DIRNAME}*");
			string errMess = string.Format(Resources.UnableToExecute, "DIR /A " + dirfilter);
			var dir = Command.Run("CMD.EXE", "/C", "DIR", "/A", dirfilter);
			if (HandleExecuteOut(dir, errMess, out var diroutStd, out var direrrStd))
			{
				var saveDataPath = Path.Combine(GamePathService.LocalGitRepositoryDirectory, VAULTFILES_REPO_DIRNAME);
				return SafelyRemoveJunction(diroutStd.JoinString(" "), saveDataPath);
			}
			return false;
		}

		/// <summary>
		/// Make Junction into $Repo for the TQVault directory
		/// </summary>
		/// <returns></returns>
		protected override bool LinkRepoTQVaultData()
		{
			var junctionPath = Path.Combine(GamePathService.LocalGitRepositoryDirectory, VAULTFILES_REPO_DIRNAME);
			string sourcePath = GamePathService.TQVaultSaveFolder;
			string errMess = string.Format(Resources.UnableToExecute, string.Format("MKLINK /J {0} {1}", junctionPath, GamePathService.TQVaultSaveFolder));

			var mklink = Command.Run("CMD.EXE", "/C", "MKLINK", "/J", junctionPath, sourcePath);
			return HandleExecuteOut(mklink, errMess, out var mklinkoutStd, out var mklinkerrStd);
		}

		/// <summary>
		/// Make Junction into $Repo for the 2 SaveData directories
		/// </summary>
		protected override bool LinkRepoSaveData(
			string TQPathSaveData, bool TQPathSaveDataExists, string RepoTQPath
			, string TQITPathSaveData, bool TQITPathSaveDataExists, string RepoTQITPath
		)
		{
			bool TQSuccess = false, TQITSuccess = false;

			if (TQPathSaveDataExists)
			{
				var junctionPathParent = Path.Combine(GamePathService.LocalGitRepositoryDirectory, SAVE_DIRNAME_TQ);
				Directory.CreateDirectory(junctionPathParent);
				string errMess = string.Format(Resources.UnableToExecute, string.Format("MKLINK /J {0} {1}", RepoTQPath, TQPathSaveData));
				var mklink = Command.Run("CMD.EXE", "/C", "MKLINK", "/J", RepoTQPath, TQPathSaveData);
				TQSuccess = HandleExecuteOut(mklink, errMess, out var mklinkoutStd, out var mklinkerrStd);
			}

			if (TQITPathSaveDataExists)
			{
				var junctionPathParent = Path.Combine(GamePathService.LocalGitRepositoryDirectory, SAVE_DIRNAME_TQIT);
				Directory.CreateDirectory(junctionPathParent);
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
				var TQPath = Path.Combine(GamePathService.LocalGitRepositoryDirectory, SAVE_DIRNAME_TQ);
				TQSuccess = SafelyRemoveRepoSaveData(TQPath);
			}
			if (RepoTQITPathExists)
			{
				var TQITPath = Path.Combine(GamePathService.LocalGitRepositoryDirectory, SAVE_DIRNAME_TQIT);
				TQITSuccess = SafelyRemoveRepoSaveData(TQITPath);
			}
			return TQSuccess || TQITSuccess;

			bool SafelyRemoveRepoSaveData(string tqpath)
			{
				// check if it's a junction already
				var dirfilter = Path.Combine(tqpath, @$"*{SAVEDATA_DIRNAME}*");
				string errMess = string.Format(Resources.UnableToExecute, "DIR /A \"" + dirfilter + '"');
				var dir = Command.Run("CMD.EXE", "/C", "DIR", "/A", dirfilter);
				if (HandleExecuteOut(dir, errMess, out var diroutStd, out var direrrStd))
				{
					var junctionPath = Path.Combine(tqpath, SAVEDATA_DIRNAME);

					return SafelyRemoveJunction(diroutStd.JoinString(" "), junctionPath);
				}
				return false; // DIR failed ? hard to believe
			}
		}

		private bool SafelyRemoveJunction(string commandDirStdOutput, string junctionPath)
		{
			// Is a junction already
			if (commandDirStdOutput.Contains("<JUNCTION>"))
			{
				// Detach the junction
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
}
