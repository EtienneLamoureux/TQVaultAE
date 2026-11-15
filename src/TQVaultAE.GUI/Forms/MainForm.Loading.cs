using Microsoft.Extensions.Logging;
using System.Globalization;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Results;
using TQVaultAE.Application.Session;
using TQVaultAE.GUI.Dialogs;
using TQVaultAE.GUI.Infrastructure;
using TQVaultAE.Presentation;
using TQVaultAE.Services;
using System.ComponentModel;
using TQVaultAE.GUI.Components.Scaling;

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
		=> ILoadAllService.CountSteps(this.mainView.ListedCharacters().Length, GamePathResolver.GetVaultList().Length, this.mainView.SharedStashMaps().Count);

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
			this.mainView.ListedCharacters()
			, GamePathResolver.GetVaultList()
			, progress
			, this.mainView.SharedStashMaps());

		// Display errors
		foreach (var error in result.Errors)
		{
			this.Services.MessageBoxes.Inform(LoadMessages.ReadFailure(error.File, error.Error.Message), Resources.GlobalError, MessageBoxIcon.Error);
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
	/// Tells the user that a git backup step failed, when one did.
	/// </summary>
	private void ShowGitBackupError(GitBackupError? error)
	{
		if (error is not null)
			this.UIService.ShowError(GitBackupText.Message(error), Buttons: ShowMessageButtons.OK);
	}

	/// <summary>
	/// Tells the user that the git backup failed, then shows the time of the last save when a character, vault or
	/// stash was saved.
	/// </summary>
	private void ShowSavedNotification(SaveResult result)
	{
		this.ShowGitBackupError(result.GitError);

		if (!result.AnythingSaved)
			return;

		var saved = string.Format(Resources.SavedAtNotification, DateTime.Now);
		this.mainView.ShowNotification(saved);
		this.toolTip.SetToolTip(this.saveButton, saved);
	}

	/// <summary>
	/// Logs a file error of a save and asks the user: Abort rethrows, Retry writes that file again, Ignore leaves it unsaved
	/// and goes on with the next files.
	/// </summary>
	private SaveDecision AskSaveDecision(SaveError error)
	{
		string? name = error.Name is null ? null : SharedStashText.Localize(error.Name, error.StashType);
		string title = string.Format(CultureInfo.CurrentCulture, Resources.MainFormSaveError, name);
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
		this.MoveToOpeningScreen();
		this.Activate();
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
		this.mainView.RefreshCharacters();

		// Prepare the search data now, so the first search does not have to,
		// then the bag button icons, so the bag button settings open at once.
		var iconService = this.Services.IconService;
		_ = this.Services.ItemDatabaseService.WarmUpAsync()
			.ContinueWith(_ => iconService.GetIconDatabaseAsync(), TaskScheduler.Default);

		this.mainView.LoadSharedStashes();
		this.mainView.WatchSharedStashFiles();

		var characters = this.mainView.ListedCharacters();

		// The databases of the mods the characters are linked to load in the background (already done by LoadAllFiles),
		// so that selecting a modded character does not wait for its mod's database on the UI thread.
		_ = this.gameMapService.PreloadMapsAsync(characters);

		var selection = StartupSelection.Resolve(
			USettings
			, characters.Select(ps => ps.Key)
			, this.vaultService.VaultExists
			, this.commandLineArgs);

		// Load last character here if selected
		if (selection.CharacterKey is not null)
			this.mainView.SelectCharacter(characters.First(ps => ps.Key == selection.CharacterKey));

		this.mainView.SelectVault(selection.VaultName);

		this.splashScreen.UpdateText();
		this.splashScreen.ShowMainForm = true;

		// Allows skipping of title screen with setting
		if (selection.SkipTitle)
		{
			// The command line player is a character key (IT/User/HeroA) or the start of a name in the list.
			this.mainView.SelectAutomaticPlayer(selection.AutomaticPlayer);

			this.splashScreen.CloseForm();
		}
	}

	#endregion
}
