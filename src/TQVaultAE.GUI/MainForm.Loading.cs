using Microsoft.Extensions.Logging;
using System.Globalization;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Results;
using TQVaultAE.Presentation;
using TQVaultAE.Services;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using TQVaultAE.Application;
using TQVaultAE.GUI.Models;

namespace TQVaultAE.GUI;

/// <summary>
/// Main Dialog class
/// </summary>
public partial class MainForm
{
	#region Files


	/// <summary>
	/// Counts the number of files which LoadAllFiles will load.  Used to set the max value of the progress bar.
	/// </summary>
	/// <returns>Total number of files that LoadAllFiles() will load.</returns>
	private int LoadAllFilesTotal()
	{
		int numIT = GamePathResolver.GetCharacterList().Count();
		numIT = numIT * 2;// Assuming that there is 1 stash file per character
		int numVaults = GamePathResolver.GetVaultList().Count();
		return Math.Max(0, numIT + numVaults - 1);
	}

	/// <summary>
	/// Loads all of the players, stashes, and vaults.
	/// Shows a progress dialog.
	/// Used for the searching function.
	/// </summary>
	private void LoadAllFiles()
	{
		// Check to see if we failed the last time we tried loading all of the files.
		// If we did fail then turn it off and skip it.
		if (!USettings.LoadAllFilesCompleted)
		{
			if (MessageBox.Show(
				Resources.MainFormDisableLoadAllFiles,
				Resources.MainFormDisableLoadAllFilesCaption,
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Information,
				MessageBoxDefaultButton.Button1,
				RightToLeftOptions) == DialogResult.Yes)
			{
				USettings.LoadAllFilesCompleted = true;
				USettings.LoadAllFiles = false;
				USettings.Save();
				return;
			}
		}

		string[] vaults = GamePathResolver.GetVaultList();
		var charactersIT = this.comboBoxCharacter.Items.OfType<PlayerSave>().ToArray();

		// Since this takes a while, show a progress dialog box.
		int total = charactersIT.Length + vaults.Length - 1;

		if (total > 0)
		{
			// We were successful last time so we reset the flag for this attempt.
			USettings.LoadAllFilesCompleted = false;
			USettings.Save();
		}
		else
			return;

		Stopwatch stopWatch = new Stopwatch();
		stopWatch.Start();

		// Load all of the Immortal Throne player files and stashes.
		var bagPlayer = new ConcurrentBag<PlayerLoadResult>();
		var bagPlayerStashes = new ConcurrentBag<StashLoadResult>();
		var bagVault = new ConcurrentBag<VaultLoadResult>();

		var lambdacharactersIT = charactersIT.Select(c => (Action)(() =>
		{
			// Get the player
			var result = this.playerService.LoadPlayer(c);
			bagPlayer.Add(result);
			this.backgroundWorkerLoadAllFiles.ReportProgress(1);
		})).ToArray();

		var lambdacharacterStashes = charactersIT.Select(c => (Action)(() =>
		{
			// Get the player's stash
			var result = this.stashService.LoadPlayerStash(c);
			bagPlayerStashes.Add(result);
			this.backgroundWorkerLoadAllFiles.ReportProgress(1);
		})).ToArray();

		var lambdaVault = vaults.Select(c => (Action)(() =>
		{
			// Load all of the vaults.
			var result = this.vaultService.LoadVault(c);
			bagVault.Add(result);
			this.backgroundWorkerLoadAllFiles.ReportProgress(1);
		})).ToArray();

		Parallel.Invoke(lambdacharactersIT.Concat(lambdacharacterStashes).Concat(lambdaVault).ToArray());// Parallel loading

		// Dispay errors
		bagPlayer.Where(p => p.Player.ArgumentException != null).ToList()
			.ForEach(result =>
			{
				string msg = string.Format(CultureInfo.CurrentUICulture, "{0}\n{1}\n{2}", Resources.MainFormPlayerReadError, result.PlayerFile, result.Player.ArgumentException.Message);
				MessageBox.Show(msg, Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);
			});
		bagPlayerStashes.Where(p => p.Stash.ArgumentException != null).ToList()
			.ForEach(result =>
			{
				string msg = string.Format(CultureInfo.CurrentUICulture, "{0}\n{1}\n{2}", Resources.MainFormPlayerReadError, result.StashFile, result.Stash.ArgumentException.Message);
				MessageBox.Show(msg, Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);
			});
		bagVault.Where(p => p.ArgumentException != null).ToList()
			.ForEach(result =>
			{
				string msg = string.Format(CultureInfo.CurrentUICulture, "{0}\n{1}\n{2}", Resources.MainFormPlayerReadError, result.Filename, result.ArgumentException.Message);
				MessageBox.Show(msg, Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);
			});

		this.comboBoxCharacter.RefreshItems();

		stopWatch.Stop();
		// Get the elapsed time as a TimeSpan value.
		TimeSpan ts = stopWatch.Elapsed;

		// Format and display the TimeSpan value.
		Log.LogInformation("LoadTime {0:00}:{1:00}:{2:00}.{3:00}",
			ts.Hours, ts.Minutes, ts.Seconds,
			ts.Milliseconds / 10);

		// We made it so set the flag to indicate we were successful.
		USettings.LoadAllFilesCompleted = true;
		USettings.Save();
	}

	/// <summary>
	/// Attempts to save all modified files.
	/// </summary>
	/// <returns>true if players have been modified</returns>
	private bool SaveAllModifiedFiles()
	{
		bool playersModified = this.SaveAllModifiedPlayers();
		bool vaultsModified = this.SaveAllModifiedVaults();
		bool stashesModified = this.SaveAllModifiedStashes();

		// Notification Last Save
		if (playersModified || vaultsModified || stashesModified)
		{
			var saved = string.Format(Resources.SavedAtNotification, DateTime.Now);
			this.NotificationText.Text = saved;
			this.toolTip.SetToolTip(this.saveButton, saved);
		}

		return playersModified;
	}

	#endregion

	#region SplashScreen & Tooltip

	/// <summary>
	/// Handler for closing the splash screen
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">FormClosedEventArgs data</param>
	private void SplashScreenClosed(object sender, FormClosedEventArgs e)
	{
		if (this.resourcesLoaded)
		{
			if (!this.loadingComplete)
				this.backgroundWorkerLoadAllFiles.CancelAsync();

			this.ShowMainForm();
		}
		else
			System.Windows.Forms.Application.Exit();
	}

	/// <summary>
	/// Starts the fade in of the main form.
	/// </summary>
	private void ShowMainForm()
	{
		this.fadeInTimer.Start();
		this.ShowInTaskbar = true;
		this.Enabled = true;
		this.Show();
		this.Activate();
	}


	/// <summary>
	/// Tooltip callback
	/// </summary>
	/// <param name="windowHandle">handle of the main window form</param>
	/// <returns>tooltip string</returns>
	private void ToolTipCallback(int windowHandle)
	{
		this.vaultPanel.ToolTipCallback(windowHandle);
		this.playerPanel.ToolTipCallback(windowHandle);
		this.stashPanel.ToolTipCallback(windowHandle);
		this.secondaryVaultPanel.ToolTipCallback(windowHandle);

		// Changed by VillageIdiot
		// If we are dragging something around, clear the tooltip and text box.
		if (this.DragInfo.IsActive)
			this.NotificationText.Text = string.Empty;
	}

	#endregion

	#region Game Resources

	/// <summary>
	/// Loads the resources.
	/// </summary>
	/// <param name="worker">Background worker</param>
	/// <param name="e">DoWorkEventArgs data</param>
	/// <returns>true when resource loading has completed successfully</returns>
	private bool LoadResources(BackgroundWorker worker, DoWorkEventArgs e)
	{
		// Abort the operation if the user has canceled.
		// Note that a call to CancelAsync may have set
		// CancellationPending to true just after the
		// last invocation of this method exits, so this
		// code will not have the opportunity to set the
		// DoWorkEventArgs.Cancel flag to true. This means
		// that RunWorkerCompletedEventArgs.Cancelled will
		// not be set to true in your RunWorkerCompleted
		// event handler. This is a race condition.
		if (worker.CancellationPending)
		{
			e.Cancel = true;
			return this.resourcesLoaded;
		}
		else
		{
			CommandLineArgs args = new CommandLineArgs();

			// Check to see if we loaded something from the command line.
			if (args.HasMapName)
				GamePathResolver.MapName = args.MapName;

			this.resourcesLoaded = true;
			this.backgroundWorkerLoadAllFiles.ReportProgress(1);

			if (USettings.LoadAllFiles)
				this.LoadAllFiles();

			// Notify the form that the resources are loaded.
			return true;
		}
	}

	/// <summary>
	/// Background worker call to load the resources.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">DoWorkEventArgs data</param>
	private void BackgroundWorkerLoadAllFiles_DoWork(object sender, DoWorkEventArgs e)
	{
		// Get the BackgroundWorker that raised this event.
		BackgroundWorker worker = sender as BackgroundWorker;

		// Assign the result of the resource loader
		// to the Result property of the DoWorkEventArgs
		// object. This is will be available to the
		// RunWorkerCompleted eventhandler.
		e.Result = this.LoadResources(worker, e);
	}

	/// <summary>
	/// Background worker has finished
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">RunWorkerCompletedEventArgs data</param>
	private void BackgroundWorkerLoadAllFiles_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		// First, handle the case where an exception was thrown.
		if (e.Error != null)
		{
			Log.LogError(e.Error, $"resourcesLoaded = {this.resourcesLoaded}");

			if (MessageBox.Show(
				string.Concat(e.Error.Message, Resources.Form1BadLanguage)
				, Resources.Form1ErrorLoadingResources
				, MessageBoxButtons.YesNo
				, MessageBoxIcon.Exclamation
				, MessageBoxDefaultButton.Button1
				, RightToLeftOptions) == DialogResult.Yes
			) System.Windows.Forms.Application.Restart();
			else
				System.Windows.Forms.Application.Exit();
		}
		else if (e.Cancelled && !this.resourcesLoaded)
			System.Windows.Forms.Application.Exit();
		else if (e.Result.Equals(true))
		{
			this.loadingComplete = true;
			this.Enabled = true;

			this.LoadTransferStash();
			this.LoadRelicVaultStash();

			if (USettings.EnableHotReload)
			{
				var relicPath = GamePathResolver.RelicVaultStashFileFullPath;
				var transferPath = GamePathResolver.TransferStashFileFullPath;

				this.fileSystemWatcherRelicStash.EnableRaisingEvents = false;
				if (FileIO.Exists(relicPath))
				{
					this.fileSystemWatcherRelicStash.Path = PathIO.GetDirectoryName(relicPath);
					this.fileSystemWatcherRelicStash.Filter = PathIO.GetFileName(relicPath);
					this.fileSystemWatcherRelicStash.EnableRaisingEvents = true;
				}

				this.fileSystemWatcherTransferStash.EnableRaisingEvents = false;
				if (FileIO.Exists(transferPath))
				{
					this.fileSystemWatcherTransferStash.Path = PathIO.GetDirectoryName(transferPath);
					this.fileSystemWatcherTransferStash.Filter = PathIO.GetFileName(transferPath);
					this.fileSystemWatcherTransferStash.EnableRaisingEvents = true;
				}
			}

			// Load last character here if selected
			if (USettings.LoadLastCharacter)
			{
				var lastPlayerSave = this.comboBoxCharacter.Items.OfType<PlayerSave>()
					.FirstOrDefault(ps => ps.Name == USettings.LastCharacterName);

				if (lastPlayerSave != null)
					this.comboBoxCharacter.SelectedItem = lastPlayerSave;
			}

			string currentVault = VaultService.MAINVAULT;

			// See if we should load the last loaded vault
			if (USettings.LoadLastVault)
			{
				currentVault = USettings.LastVaultName;

				// Make sure there is something in the config file to load else load the Main Vault
				// We do not want to create new here.
				if (string.IsNullOrEmpty(currentVault) || !FileIO.Exists(GamePathResolver.GetVaultFile(currentVault)))
					currentVault = VaultService.MAINVAULT;
			}

			this.vaultListComboBox.SelectedItem = currentVault;

			// Finally load Vault
			this.LoadVault(currentVault, false);

			this.splashScreen.UpdateText();
			this.splashScreen.ShowMainForm = true;

			CommandLineArgs args = new CommandLineArgs();

			// Allows skipping of title screen with setting
			if (args.IsAutomatic || USettings.SkipTitle == true)
			{
				string player = args.Player;
				int index = this.comboBoxCharacter.FindString(player);
				if (index != -1)
					this.comboBoxCharacter.SelectedIndex = index;

				this.splashScreen.CloseForm();
			}
		}
		else
		{
			Log.LogError(e.Error, $"resourcesLoaded = {this.resourcesLoaded}");
			// If for some reason the loading failed, but there was no error raised.
			MessageBox.Show(
				Resources.Form1ErrorLoadingResources,
				Resources.Form1ErrorLoadingResources,
				MessageBoxButtons.OK,
				MessageBoxIcon.Exclamation,
				MessageBoxDefaultButton.Button1,
				RightToLeftOptions);
			System.Windows.Forms.Application.Exit();
		}
	}

	/// <summary>
	/// Handler for updating the splash screen progress bar.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">ProgressChangedEventArgs data</param>
	private void BackgroundWorkerLoadAllFiles_ProgressChanged(object sender, ProgressChangedEventArgs e)
		=> this.splashScreen.IncrementValue();

	#endregion

	#region Settings Dialog

	/// <summary>
	/// Updates configuration settings
	/// </summary>
	private void SaveConfiguration()
	{
		// Update last loaded vault
		if (USettings.LoadLastVault)
		{
			// Changed by VillageIdiot
			// Now check to see if the value is changed since the Main Vault would never auto load
			if (this.vaultListComboBox.SelectedItem != null && this.vaultListComboBox.SelectedItem.ToString().ToUpperInvariant() != USettings.LastVaultName.ToUpperInvariant())
			{
				USettings.LastVaultName = this.vaultListComboBox.SelectedItem.ToString();
				this.configChanged = true;
			}
		}

		// Update last loaded character
		if (USettings.LoadLastCharacter)
		{
			string name = this.comboBoxCharacter.SelectedItem.ToString();
			var ps = this.comboBoxCharacter.SelectedItem as PlayerSave;

			if (ps is not null) name = ps.Name;

			if (name.ToUpperInvariant() != USettings.LastCharacterName.ToUpperInvariant())
			{
				// Clear the value if no character is selected
				if (name == Resources.MainFormSelectCharacter)
					name = string.Empty;

				USettings.LastCharacterName = name;

				this.configChanged = true;
			}
		}

		// Update custom map settings
		if (USettings.ModEnabled)
			this.configChanged = true;

		// Clear out the key if we are autodetecting.
		if (USettings.AutoDetectLanguage)
			USettings.TQLanguage = string.Empty;

		// Clear out the settings if auto detecting.
		if (USettings.AutoDetectGamePath)
		{
			USettings.TQITPath = string.Empty;
			USettings.TQPath = string.Empty;
		}

		if (UIService.Scale != 1.0F)
		{
			USettings.Scale = UIService.Scale;
			this.configChanged = true;
		}

		if (this.configChanged)
			USettings.Save();
	}

	/// <summary>
	/// Handler for clicking the configure button.
	/// Shows the Settings Dialog.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void ConfigureButtonClick(object sender, EventArgs e)
	{
		SettingsDialog settingsDialog = new SettingsDialog(this);
		DialogResult result = DialogResult.Cancel;
		settingsDialog.Scale(new SizeF(UIService.Scale, UIService.Scale));

		string title = string.Empty;
		string message = string.Empty;

		if (settingsDialog.ShowDialog() == DialogResult.OK && settingsDialog.ConfigurationChanged)
		{
			if (settingsDialog.PlayerFilterChanged)
			{
				this.comboBoxCharacter.SelectedItem = Resources.MainFormSelectCharacter;
				if (this.playerPanel.Player != null)
					this.playerPanel.Player = null;

				this.GetPlayerList();
			}

			if (settingsDialog.VaultPathChanged)
			{
				GamePathResolver.TQVaultSaveFolder = settingsDialog.VaultPath;
				this.vaultService.UpdateVaultPath(settingsDialog.VaultPath);
				this.GetVaultList(true);
			}

			if (settingsDialog.EnableTQVaultSoundsChanged)
			{
				var soundSrv = this.ServiceProvider.GetService<ISoundService>();
				soundSrv.InitAllPlayers();
			}

			if (settingsDialog.LanguageChanged || settingsDialog.GamePathChanged || settingsDialog.CustomMapsChanged || settingsDialog.UISettingChanged)
			{
				if ((settingsDialog.GamePathChanged && settingsDialog.LanguageChanged) || settingsDialog.UISettingChanged)
				{
					title = Resources.MainFormSettingsChanged;
					message = Resources.MainFormSettingsChangedMsg;
				}
				else if (settingsDialog.GamePathChanged)
				{
					title = Resources.MainFormPathsChanged;
					message = Resources.MainFormPathsChangedMsg;
				}
				else if (settingsDialog.CustomMapsChanged)
				{
					title = Resources.MainFormMapsChanged;
					message = Resources.MainFormMapsChangedMsg;
				}
				else
				{
					title = Resources.MainFormLanguageChanged;
					message = Resources.MainFormLanguageChangedMsg;
				}

				result = MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, RightToLeftOptions);
			}

			if (settingsDialog.ItemBGColorOpacityChanged || settingsDialog.EnableItemRequirementRestrictionChanged)
				this.Refresh();

			if (settingsDialog.EnableCharacterEditChanged)
				stashPanel?.UpdatePlayerInfo();

			this.configChanged = true;
			this.SaveConfiguration();


			AdjustMenuButtonVisibility();

			if (result == DialogResult.Yes)
			{
				if (this.DoCloseStuff())
					System.Windows.Forms.Application.Restart();
			}
		}
	}

	#endregion
}
