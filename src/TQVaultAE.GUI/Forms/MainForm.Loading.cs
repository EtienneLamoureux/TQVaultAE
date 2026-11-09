using Microsoft.Extensions.Logging;
using System.Globalization;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Results;
using TQVaultAE.GUI.Dialogs;
using TQVaultAE.GUI.Infrastructure;
using TQVaultAE.Presentation;
using TQVaultAE.Services;
using System.ComponentModel;
using TQVaultAE.Application;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.Logs;

namespace TQVaultAE.GUI.Forms;

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
		=> LoadAllService.CountSteps(this.CharactersOfActiveMap().Length, GamePathResolver.GetVaultList().Length);

	/// <summary>
	/// The listed characters played with the active map: the only ones LoadAllFiles can read with the right database.
	/// </summary>
	private PlayerSave[] CharactersOfActiveMap()
		=> [.. this.comboBoxCharacter.Items.OfType<PlayerSave>().Where(this.gameMapService.UsesActiveMap)];

	/// <summary>
	/// Loads all of the players, stashes, and vaults.
	/// Used for the searching function.
	/// </summary>
	/// <param name="progress">Receives one step per loaded file.</param>
	private void LoadAllFiles(IProgress<int> progress)
	{
		// Check to see if we failed the last time we tried loading all of the files.
		// If we did fail then turn it off and skip it.
		if (this.loadAllService.LastRunFailed
			&& this.Services.MessageBoxes.Confirm(Resources.MainFormDisableLoadAllFiles, Resources.MainFormDisableLoadAllFilesCaption, MessageBoxIcon.Information))
		{
			this.loadAllService.DisableLoadAll();
			return;
		}

		var result = this.loadAllService.LoadAll(
			this.CharactersOfActiveMap()
			, GamePathResolver.GetVaultList()
			, progress);

		// Display errors
		foreach (var error in result.Errors)
		{
			string msg = string.Format(CultureInfo.CurrentUICulture, "{0}\n{1}\n{2}", Resources.MainFormPlayerReadError, error.File, error.Error.Message);
			this.Services.MessageBoxes.Inform(msg, Resources.GlobalError, MessageBoxIcon.Error);
		}
	}

	/// <summary>
	/// Attempts to save all modified files.
	/// </summary>
	/// <returns>true if players have been modified</returns>
	private bool SaveAllModifiedFiles()
	{
		var result = this.saveOrchestrator.Save(SaveRequest.FilesOnly, this.AskSaveDecision);
		this.ShowSavedNotification(result);
		return result.PlayersSaved;
	}

	/// <summary>
	/// Shows the time of the last save when a character, vault or stash was saved.
	/// </summary>
	private void ShowSavedNotification(SaveResult result)
	{
		if (!result.AnythingSaved)
			return;

		var saved = string.Format(Resources.SavedAtNotification, DateTime.Now);
		this.NotificationText.Text = saved;
		this.toolTip.SetToolTip(this.saveButton, saved);
	}

	/// <summary>
	/// Logs a file error of a save and asks the user: Abort rethrows, Retry writes that file again, Ignore leaves it unsaved
	/// and goes on with the next files.
	/// </summary>
	private SaveDecision AskSaveDecision(SaveError error)
	{
		string title = string.Format(CultureInfo.CurrentCulture, Resources.MainFormSaveError, error.Name);
		Log.LogError(error.Exception, "{Message}", title);
		return this.Services.MessageBoxes.Ask(Log.FormatException(error.Exception), title, MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Exclamation) switch
		{
			DialogResult.Abort => SaveDecision.Abort,
			DialogResult.Retry => SaveDecision.Retry,
			_ => SaveDecision.Ignore,
		};
	}

	#endregion

	#region SplashScreen & Tooltip

	/// <summary>
	/// Handler for closing the splash screen
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">FormClosedEventArgs data</param>
	private void SplashScreenClosed(object? sender, FormClosedEventArgs e)
	{
		if (this.resourcesLoaded)
		{
			if (!this.loadingComplete)
				this.loadCancellation.Cancel();

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
	/// Cancelled when the splash screen is closed before the loading completes.
	/// </summary>
	private readonly CancellationTokenSource loadCancellation = new();

	/// <summary>
	/// Loads the resources off the UI thread, advancing the splash screen, then shows them.
	/// </summary>
	private async Task LoadResourcesAsync()
	{
		// Created on the UI thread, so the splash screen is updated there.
		var progress = new Progress<int>(_ => this.splashScreen.IncrementValue());
		var token = this.loadCancellation.Token;
		try
		{
			await Task.Run(() => this.LoadResources(progress, token));
		}
		catch (OperationCanceledException) when (token.IsCancellationRequested)
		{
			if (!this.resourcesLoaded)
				System.Windows.Forms.Application.Exit();
			return;
		}
		catch (Exception ex)
		{
			Log.LogError(ex, "resourcesLoaded = {ResourcesLoaded}", this.resourcesLoaded);

			if (this.Services.MessageBoxes.Confirm(string.Concat(ex.Message, Resources.Form1BadLanguage), Resources.Form1ErrorLoadingResources, MessageBoxIcon.Exclamation)
			) System.Windows.Forms.Application.Restart();
			else
				System.Windows.Forms.Application.Exit();
			return;
		}

		this.ResourcesLoaded();
	}

	/// <summary>
	/// Loads the resources. Runs off the UI thread.
	/// </summary>
	/// <param name="progress">Receives one step per loaded resource.</param>
	/// <param name="token">Cancelled when the splash screen is closed.</param>
	private void LoadResources(IProgress<int> progress, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();

		var args = this.commandLineArgs;
		args.ThrowIfInvalid();

		this.resourcesLoaded = true;
		progress.Report(1);

		if (USettings.LoadAllFiles)
			this.LoadAllFiles(progress);
	}

	/// <summary>
	/// Shows the loaded resources: stashes, last character and vault.
	/// </summary>
	private void ResourcesLoaded()
	{
		this.loadingComplete = true;
		this.Enabled = true;

		// The characters may have been loaded by LoadAllFiles.
		this.comboBoxCharacter.RefreshItems();

		// Prepare the search data now, so the first search does not have to,
		// then the bag button icons, so the bag button settings open at once.
		var iconService = this.Services.IconService;
		_ = this.Services.ItemDatabaseService.WarmUpAsync()
			.ContinueWith(_ => iconService.GetIconDatabaseAsync(), TaskScheduler.Default);

		this.LoadSharedStashes();
		this.WatchSharedStashFiles();

		var characters = this.comboBoxCharacter.Items.OfType<PlayerSave>().ToArray();
		var selection = StartupSelection.Resolve(
			USettings
			, characters.Select(ps => ps.Key)
			, this.vaultService.VaultExists
			, this.commandLineArgs);

		// Load last character here if selected
		if (selection.CharacterKey is not null)
			this.comboBoxCharacter.SelectedItem = characters.First(ps => ps.Key == selection.CharacterKey);

		this.vaultListComboBox.SelectedItem = selection.VaultName;

		// Finally load Vault
		this.LoadVault(selection.VaultName, false);

		this.splashScreen.UpdateText();
		this.splashScreen.ShowMainForm = true;

		// Allows skipping of title screen with setting
		if (selection.SkipTitle)
		{
			// The command line player is a character key (IT/User/HeroA) or the start of a name in the list.
			if (characters.FirstOrDefault(ps => ps.Key.Equals(selection.AutomaticPlayer, StringComparison.OrdinalIgnoreCase)) is { } player)
				this.comboBoxCharacter.SelectedItem = player;
			else if (this.comboBoxCharacter.FindString(selection.AutomaticPlayer) is var index and not -1)
				this.comboBoxCharacter.SelectedIndex = index;

			this.splashScreen.CloseForm();
		}
	}

	#endregion

	#region Settings Dialog

	/// <summary>
	/// Updates configuration settings
	/// </summary>
	private void SaveConfiguration()
	{
		var configuration = this.CurrentSessionConfiguration();
		this.configChanged = this.settingsPersistenceService.SaveConfiguration(configuration.VaultName, configuration.CharacterName, configuration.Zoom, configuration.ConfigChanged);
	}

	/// <summary>
	/// Gets the state of the session to store in the user settings: shown vault, selected character, zoom.
	/// </summary>
	private SessionConfiguration CurrentSessionConfiguration()
	{
		string? vaultName = this.vaultListComboBox.SelectedItem is not null ? this.vaultListComboBox.SelectedItemText() : null;

		// The key, not the name: a main campaign and a custom map character can have the same name.
		string? characterName = this.comboBoxCharacter.SelectedItem is PlayerSave ps
			? ps.Key
			: this.comboBoxCharacter.SelectedItem?.ToString();

		// No character is selected
		if (characterName == Resources.MainFormSelectCharacter)
			characterName = string.Empty;

		return new SessionConfiguration(vaultName, characterName, UIService.Zoom, this.configChanged);
	}

	/// <summary>
	/// Handler for clicking the configure button.
	/// Shows the Settings Dialog.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void ConfigureButtonClick(object? sender, EventArgs e)
	{
		SettingsDialog settingsDialog = new SettingsDialog(this);
		bool restart = false;
		using (ZoomScope.Enter())
			settingsDialog.Scale(new SizeF(UIService.Zoom, UIService.Zoom));

		if (settingsDialog.ShowDialog() == DialogResult.OK && settingsDialog.ConfigurationChanged)
		{
			if (settingsDialog.VaultPathChanged)
			{
				GamePathResolver.TQVaultSaveFolder = settingsDialog.VaultPath;
				this.vaultService.UpdateVaultPath(settingsDialog.VaultPath);
				this.GetVaultList(true);
			}

			if (settingsDialog.EnableTQVaultSoundsChanged)
			{
				var soundSrv = this.Services.SoundService;
				soundSrv.InitAllPlayers();
			}

			var restartReason = settingsDialog.RestartReason;

			if (restartReason != RestartReason.None)
			{
				var (title, message) = restartReason switch
				{
					RestartReason.Settings => (Resources.MainFormSettingsChanged, Resources.MainFormSettingsChangedMsg),
					RestartReason.GamePath => (Resources.MainFormPathsChanged, Resources.MainFormPathsChangedMsg),
					_ => (Resources.MainFormLanguageChanged, Resources.MainFormLanguageChangedMsg),
				};

				restart = this.Services.MessageBoxes.Confirm(message, title);
			}

			if (settingsDialog.ItemBGColorOpacityChanged || settingsDialog.EnableItemRequirementRestrictionChanged)
				this.Refresh();

			if (settingsDialog.EnableCharacterEditChanged)
				stashPanel?.UpdatePlayerInfo();

			this.configChanged = true;
			this.SaveConfiguration();


			AdjustMenuButtonVisibility();

			if (restart)
			{
				if (this.DoCloseStuff())
					System.Windows.Forms.Application.Restart();
			}
		}
	}

	#endregion
}
