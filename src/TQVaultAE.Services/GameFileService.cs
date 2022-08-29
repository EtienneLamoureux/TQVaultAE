using System;
using Microsoft.VisualBasic.Devices;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using TQVaultAE.Domain.Contracts.Services;
using TQVaultAE.Presentation;
using Medallion.Shell;
using TQVaultAE.Domain.Helpers;
using System.Text.RegularExpressions;
using System.ComponentModel;
using Medallion.Shell.Streams;
using System.Diagnostics;

namespace TQVaultAE.Services;

/// <summary>
/// IGameFileService implementation
/// </summary>
public class GameFileService : IGameFileService
{


	private const StringComparison noCase = StringComparison.OrdinalIgnoreCase;
	protected readonly ILogger Log;
	protected readonly IGamePathService GamePathService;
	protected readonly IUIService UIService;

	public GameFileService(ILogger<GameFileService> log, IGamePathService iGamePathService, IUIService iUIService)
	{
		this.Log = log;
		this.GamePathService = iGamePathService;
		this.UIService = iUIService;
	}

	public bool GitClone()
	{
		var repoUrl = Config.UserSettings.Default.GitBackupRepository;
		string errMess = string.Format(Resources.GitUnableToClone, repoUrl);
		Directory.CreateDirectory(GamePathService.LocalGitRepositoryDirectory);

		var clone = Command.Run("git"
			, new[] { "clone", repoUrl, GamePathService.LocalGitRepositoryDirectory }
			, opt => { opt.WorkingDirectory(GamePathService.LocalGitRepositoryDirectory); }
		);

		return HandleExecuteOut(clone, errMess, out var outStd, out var errStd);
	}

	public bool GitAddCommitTagAndPush()
	{
		if (Config.UserSettings.Default.GitBackupEnabled && GamePathService.LocalGitRepositoryGitDirExist)
		{
			var repoUrl = Config.UserSettings.Default.GitBackupRepository;
			string errMess = string.Format(Resources.GitUnableToPush, repoUrl);
			Action<Shell.Options> options = (opt) => opt.WorkingDirectory(GamePathService.LocalGitRepositoryDirectory);

			Command status, stageAll, commit, tagVersion, tagLatest, push, pushTags;
			status = stageAll = commit = tagVersion = tagLatest = push = pushTags = null;

			try
			{
				/*
				 * Reporting synchronously via event starts good but break at some point, job is done though.
				 * 
				 * IProgress : Should be the new way of reporting progress. 
				 * It report progress on time and is not blocking, but the Form is delaying result processing and flush them all AFTER the job. 
				 * 
				 * BackgroundWorker.ReportProgress() : should work for Winform 
				 * but Command.Wait() inside BackgroundWorker.DoWork() simply brutaly kill the app, like a BSOD for app. LOL
				 * 
				 * I really tried.
				 * hguy
				 */

				// Is there anything new to commit ?
				status = Command.Run("git", new[] { "status", "-suall" }, options);
				var statusSuccess = HandleExecuteOut(status, errMess, out var statusoutStd, out var statuserrStd);
				if (statusSuccess && statusoutStd.Count > 0)
				{
					stageAll = Command.Run("git", new[] { "stage", "*" }, options);
					if (HandleExecuteOut(stageAll, errMess, out var stageAlloutStd, out var stageAllerrStd))
					{
						commit = Command.Run("git", new[] { "commit", "-m", @"TQVaultAE update!" }, options);
						if (HandleExecuteOut(commit, errMess, out var commitoutStd, out var commiterrStd))
						{
							var tag = DateTime.Now.ToString("yy.MM.dd.HHmmss");// Use date for versioning

							tagVersion = Command.Run("git", new[] { "tag", tag, }, options);
							if (HandleExecuteOut(tagVersion, errMess, out var tagVersionoutStd, out var tagVersionerrStd))
							{
								tagLatest = Command.Run("git", new[] { "tag", "-f", "latest" }, options);
								if (HandleExecuteOut(tagLatest, errMess, out var tagLatestoutStd, out var tagLatesterrStd))
								{
									using (push = Command.Run("git", new[] { "push", "-v", "--progress" } // You need "--progress" to capture percentage in StandardError
										, (opt) =>
										{
											opt.WorkingDirectory(GamePathService.LocalGitRepositoryDirectory);
											//opt.DisposeOnExit(false); // needed for RedirectStandardError = true
											//opt.StartInfo(si => { si.RedirectStandardError = true; }); // needed for ConsumeStandardOutputAsync
										})
									)
									{
										// Hook console output verbosity

										// --- Method StandardError direct reading
										//var tsk = Task.Run(() => ConsumeStandardOutputAsync(push.StandardError));
										//push.Wait();
										//return push.Result.Success;

										// --- Method BindingList event model
										//BindingList<string> pushoutStd = new(), pusherrStd = new();
										//pusherrStd.ListChanged += PusherrStd_ListChanged;
										//var res = HandleExecuteRef(push, errMess, ref pushoutStd, ref pusherrStd);
										//return res;

										if (HandleExecuteOut(push, errMess, out var pushoutStd, out var pusherrStd))
										{
											pushTags = Command.Run("git", new[] { "push", "origin", "latest", "-f" }, options);
											if (HandleExecuteOut(pushTags, errMess, out var pushTagsoutStd, out var pushTagserrStd))
											{
												pushTags = Command.Run("git", new[] { "push", "origin", tag }, options);
												return HandleExecuteOut(pushTags, errMess, out pushTagsoutStd, out pushTagserrStd);
											}
										}
									}
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				errMess += Environment.NewLine + ex.Message;
				this.Log.LogError(ex, errMess);
				this.UIService.ShowError(errMess, Buttons: ShowMessageButtons.OK);
			}
		}
		return false;
	}

	#region MedallionShell output hook

	private void PusherrStd_ListChanged(object sender, ListChangedEventArgs e)
	{
		var lst = sender as BindingList<string>;

		if (e.ListChangedType == ListChangedType.ItemAdded)
		{
			var line = lst[e.NewIndex];
			if (line is not null && PercentProgressRegEx.Match(line) is { Success: true } m)
			{
				var num = m.Groups["Num"].Value;
				Debug.WriteLine("Num : " + num);
				var percent = int.Parse(num);
				//UIService.ProgressBarReport("Git Push - " + line, percent);
				//GitAddCommitTagAndPushProgress.Report(("Git Push : " + line, percent));// Do report in time but everything is flushed after work.
			}
		}
	}

	private async void ConsumeStandardOutputAsync(ProcessStreamReader output)
	{
		// From https://github.com/madelson/MedallionShell/issues/23
		string line;
		while ((line = await output.ReadLineAsync().ConfigureAwait(false)) != null)
		{
			Console.WriteLine(line);
			/* Will match
			Counting objects: 100% (2173/2173), done.
			Compressing objects: 100% (2117/2117), done.
			Writing objects: 100% (2172/2172), 38.08 MiB | 2.23 MiB/s, done.
			remote: Resolving deltas: 100% (1886/1886), done.
			*/
			if (line is not null && PercentProgressRegEx.Match(line) is { Success: true } m)
			{
				var percent = int.Parse(m.Groups["Num"].Value);
				//UIService.ProgressBarReport("Git Push", percent);
			}
		}
	}

	static Regex PercentProgressRegEx = new Regex(@"(?<Num>\d{1,3})%", RegexOptions.Compiled);

	#endregion

	public void GitRepositorySetup()
	{
		if (Config.UserSettings.Default.GitBackupEnabled)
		{

			bool overrideFiles = false;

			var RepoTQPath = Path.Combine(GamePathService.LocalGitRepositoryDirectory, GamePathService.SaveDirNameTQ, GamePathService.SaveDataDirName);
			var RepoTQITPath = Path.Combine(GamePathService.LocalGitRepositoryDirectory, GamePathService.SaveDirNameTQIT, GamePathService.SaveDataDirName);
			bool RepoTQPathExists = false, RepoTQITPathExists = false;

			var TQPathSaveData = Path.Combine(GamePathService.SaveFolderTQ, GamePathService.SaveDataDirName);
			var TQITPathSaveData = Path.Combine(GamePathService.SaveFolderTQIT, GamePathService.SaveDataDirName);
			var TQPathSaveDataExists = Directory.Exists(TQPathSaveData);
			var TQITPathSaveDataExists = Directory.Exists(TQITPathSaveData);

			// does Vault Dir has any vault file ?
			var vaultfiles = GamePathService.GetVaultList();

			// Cloned already
			if (GamePathService.LocalGitRepositoryGitDirExist)
			{
				// pull player save files changes from github 
				// Untracked files will stay there, remote deleted tracked character will be deleted localy, remote updated will be localy updated here too.
				if (GitPull())
				{
					// Are they here after pull
					RepoTQPathExists = Directory.Exists(RepoTQPath);
					RepoTQITPathExists = Directory.Exists(RepoTQITPath);

					// Do you want to refresh your vault and local Save with last git save ?
					if (vaultfiles.Any())
					{
						overrideFiles = DoYouWantToReplaceLocalVault();
					}

					// new remote vault files will be deployed, localy updated will be overrided if needed, else leave in place
					CopyVaultFilesFromRepoToVaultData(overrideFiles);

					if (Config.UserSettings.Default.GitBackupPlayerSavesEnabled)
					{
						//		Do you want to replace your local TQ character save with git repository version ? Are you sure ?
						if (RepoTQPathExists || RepoTQITPathExists)
						{
							overrideFiles = DoYouWantToReplaceLocalCharacterSave();

							//			if Yes - Copy Repo Player files to local SaveData override file
							//			if No - Copy Repo Player files to local SaveData ignore file

							if (RepoTQPathExists)
							{
								CopySaveDataFromRepoToMyGames(RepoTQPath, TQPathSaveData, overrideFiles);
							}

							if (RepoTQITPathExists)
							{
								CopySaveDataFromRepoToMyGames(RepoTQITPath, TQITPathSaveData, overrideFiles);
							}
						}
					}
				}
				return;
			}

			// clone it
			var repoSuccess = GitClone();
			if (repoSuccess)
			{
				// Are they here after Clone
				RepoTQPathExists = Directory.Exists(RepoTQPath);
				RepoTQITPathExists = Directory.Exists(RepoTQITPath);

				overrideFiles = false;
				if (vaultfiles.Any())
				{
					overrideFiles = DoYouWantToReplaceLocalVault();
				}
				CopyVaultFilesFromRepoToVaultData(overrideFiles);

				// Now Local vault files are refreshed from git repo

				//		Remove $Repo TQVaultData
				//			$Repo\TQVaultData
				this.RemoveRepoTQVaultData();

				//		Make Junction into $Repo for the TQVault directory
				this.LinkRepoTQVaultData();

				if (Config.UserSettings.Default.GitBackupPlayerSavesEnabled)
				{
					//		Do you want to replace your local TQ character save with git repository version ? Are you sure ?
					if (RepoTQPathExists || RepoTQITPathExists)
					{
						overrideFiles = DoYouWantToReplaceLocalCharacterSave();

						//			if Yes - Copy Repo Player files to local SaveData override file
						//			if No - Copy Repo Player files to local SaveData ignore file

						if (RepoTQPathExists)
						{
							CopySaveDataFromRepoToMyGames(RepoTQPath, TQPathSaveData, overrideFiles);
						}

						if (RepoTQITPathExists)
						{
							CopySaveDataFromRepoToMyGames(RepoTQITPath, TQITPathSaveData, overrideFiles);
						}
					}

					// Now Local game directories are refreshed from git repo

					// Clean repo and put inside NTFS Junctions to local game directories.
					// to make sure that git automaticaly sees the changes, save disk space and file copy time

					//		Remove $Repo SaveData 
					//			$Repo\Titan Quest\SaveData
					//			$Repo\Titan Quest - Immortal Throne\SaveData
					this.RemoveRepoSaveData(RepoTQPathExists, RepoTQITPathExists);

					//		Make Junction into $Repo for the 2 SaveData directories
					this.LinkRepoSaveData(
						TQPathSaveData, TQPathSaveDataExists, RepoTQPath
						, TQITPathSaveData, TQITPathSaveDataExists, RepoTQITPath
					);
				}
			}
		}
	}


	#region This is specific to OS FileSystem

	protected virtual void CopyVaultFilesFromRepoToVaultData(bool overrideFiles)
	{
		throw new NotImplementedException();
	}

	protected virtual void CopySaveDataFromRepoToMyGames(string repoTQPath, string tQPathSaveData, bool overrideFiles)
	{
		throw new NotImplementedException();
	}

	protected virtual bool RemoveRepoSaveData(bool repoTQPathExists, bool repoTQITPathExists)
	{
		throw new NotImplementedException();
	}

	protected virtual bool LinkRepoSaveData(string tQPathSaveData, bool tQPathSaveDataExists, string repoTQPath, string tQITPathSaveData, bool tQITPathSaveDataExists, string repoTQITPath)
	{
		throw new NotImplementedException();
	}

	protected virtual bool LinkRepoTQVaultData()
	{
		throw new NotImplementedException();
	}

	protected virtual bool RemoveRepoTQVaultData()
	{
		throw new NotImplementedException();
	}

	#endregion

	private bool GitPull()
	{
		if (GamePathService.LocalGitRepositoryGitDirExist)
		{
			var repoUrl = Config.UserSettings.Default.GitBackupRepository;
			string errMess = string.Format(Resources.GitUnableToPull, repoUrl);

			var pull = Command.Run("git", new[] { "pull", repoUrl }, opt => { opt.WorkingDirectory(GamePathService.LocalGitRepositoryDirectory); });
			return HandleExecuteOut(pull, errMess, out var pulloutStd, out var pullerrStd);
		}
		return false;
	}

	protected bool HandleExecuteOut(Command cmd, string errMess, out BindingList<string> outputLines, out BindingList<string> errorLines, bool pipeOutputStandard = true)
	{
		outputLines = new BindingList<string>();
		errorLines = new BindingList<string>();

		return HandleExecuteRef(cmd, errMess, ref outputLines, ref errorLines, pipeOutputStandard);
	}

	protected bool HandleExecuteRef(Command cmd, string errMess, ref BindingList<string> outputLines, ref BindingList<string> errorLines, bool pipeOutputStandard = true)
	{
		string errLog = string.Empty;
		try
		{
			if (pipeOutputStandard)
			{
				cmd.StandardOutput.PipeToAsync(outputLines);
				cmd.StandardError.PipeToAsync(errorLines);
			}
			cmd.Wait();

			if (!cmd.Result.Success)
				errLog = errorLines.JoinString(Environment.NewLine);

			return cmd.Result.Success;
		}
		catch (Exception ex)
		{
			this.Log.LogError(ex, errMess);
			errLog = ex.Message;
		}
		finally
		{
			if (errLog != string.Empty)
			{
				errMess += Environment.NewLine + errLog;
				this.Log.LogError(errMess);
				this.UIService.ShowError(errMess, Buttons: ShowMessageButtons.OK);
			}
		}
		return false;
	}

	private bool DoYouWantToReplaceLocalCharacterSave()
	{
		bool replaceAnswerOk = false, sureAnswer = true, overrideFiles = false;
		replaceAnswerOk = UIService.ShowWarning(Resources.GitCloneDoYouWantToReplaceLocalCharacterSave).IsOK;
		sureAnswer = true;
		if (replaceAnswerOk)
			sureAnswer = UIService.ShowWarning(Resources.GlobalConfirm).IsOK;

		overrideFiles = replaceAnswerOk && sureAnswer;
		return overrideFiles;
	}

	private bool DoYouWantToReplaceLocalVault()
	{
		bool replaceAnswerOk = false, sureAnswer = true, overrideFiles = false;
		replaceAnswerOk = UIService.ShowWarning(Resources.GitCloneDoYouWantToReplaceLocalVault).IsOK;
		sureAnswer = true;
		if (replaceAnswerOk)
			sureAnswer = UIService.ShowWarning(Resources.GlobalConfirm).IsOK;

		overrideFiles = replaceAnswerOk && sureAnswer;
		return overrideFiles;
	}



	/// <summary>
	/// Backs up the file to the backup folder.
	/// </summary>
	/// <param name="prefix">prefix of the backup file</param>
	/// <param name="file">file name to backup</param>
	/// <returns>Returns the name of the backup file, or NULL if file does not exist</returns>
	public string BackupFile(string prefix, string file)
	{
		if (File.Exists(file))
		{
			// only backup if it exists!
			string backupFile = GamePathService.ConvertFilePathToBackupPath(prefix, file);

			File.Copy(file, backupFile);

			// Added by VillageIdiot
			// Backup the file pairs for the player stash files.
			if (Path.GetFileName(file).ToUpperInvariant() == GamePathService.PlayerStashFileNameB.ToUpperInvariant())
			{
				string dxgfile = Path.ChangeExtension(file, ".dxg");

				if (File.Exists(dxgfile))
				{
					// only backup if it exists!
					backupFile = GamePathService.ConvertFilePathToBackupPath(prefix, dxgfile);

					File.Copy(dxgfile, backupFile);
				}
			}

			return backupFile;
		}
		else
			return null;
	}

	/// <summary>
	/// Duplicate player save files
	/// </summary>
	/// <param name="playerSaveDirectory"></param>
	/// <param name="newname"></param>
	/// <returns>new directory path</returns>
	public string DuplicateCharacterFiles(string playerSaveDirectory, string newname)
	{
		var baseFolder = Path.GetDirectoryName(playerSaveDirectory);
		var newFolder = Path.Combine(baseFolder, $"_{newname}");
		var newPlayerFile = Path.Combine(newFolder, GamePathService.PlayerSaveFileName);

		var playerFile = Path.Combine(playerSaveDirectory, GamePathService.PlayerSaveFileName);
		var stashFileB = Path.Combine(playerSaveDirectory, GamePathService.PlayerStashFileNameB);
		var stashFileG = Path.Combine(playerSaveDirectory, GamePathService.PlayerStashFileNameG);
		var settingsFile = Path.Combine(playerSaveDirectory, GamePathService.PlayerSettingsFileName);

		Directory.CreateDirectory(newFolder);
		File.Copy(playerFile, newPlayerFile);
		if (File.Exists(stashFileB)) File.Copy(stashFileB, Path.Combine(newFolder, GamePathService.PlayerStashFileNameB));
		if (File.Exists(stashFileG)) File.Copy(stashFileG, Path.Combine(newFolder, GamePathService.PlayerStashFileNameG));
		if (File.Exists(settingsFile)) File.Copy(settingsFile, Path.Combine(newFolder, GamePathService.PlayerSettingsFileName));

		// Copy Progression
		// Easyest way of doing that (why VB has all the easy stuff?)
		new Computer().FileSystem.CopyDirectory(
			Path.Combine(playerSaveDirectory, "Levels_World_World01.map")
			, Path.Combine(newFolder, "Levels_World_World01.map")
		);

		return newFolder;
	}

	/// <summary>
	/// TQ has an annoying habit of throwing away your char in preference
	/// for the Backup folder if it exists if it thinks your char is not valid.
	/// We need to move that folder away so TQ won't find it.
	/// </summary>
	/// <param name="playerFile">Name of the player file to backup</param>
	public void BackupStupidPlayerBackupFolder(string playerFile)
	{
		string playerFolder = Path.GetDirectoryName(playerFile);
		string backupFolder = Path.Combine(playerFolder, "Backup");
		if (Directory.Exists(backupFolder))
		{
			// we need to move it.
			string newFolder = Path.Combine(playerFolder, "Backup-moved by TQVault");
			if (Directory.Exists(newFolder))
			{
				try
				{
					// It already exists--we need to remove it
					Directory.Delete(newFolder, true);
				}
				catch (Exception)
				{
					int fn = 1;
					while (Directory.Exists(String.Format("{0}({1})", newFolder, fn)))
					{
						fn++;
					}
					newFolder = String.Format("{0}({1})", newFolder, fn);
				}
			}

			Directory.Move(backupFolder, newFolder);
		}
	}
}