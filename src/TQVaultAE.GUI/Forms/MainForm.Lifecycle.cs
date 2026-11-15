using Microsoft.Extensions.Logging;
using TQVaultAE.Application.KeyBinds;
using TQVaultAE.Application.Results;
using TQVaultAE.GUI.Infrastructure;
using TQVaultAE.GUI.Models;
using TQVaultAE.Presentation;
using TQVaultAE.GUI.Components.Scaling;

namespace TQVaultAE.GUI.Forms;

/// <summary>
/// Main Dialog class
/// </summary>
public partial class MainForm
{
	#region Mainform Events

	/// <summary>
	/// Handler for closing the main form
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">FormClosingEventArgs data</param>
	private void MainFormClosing(object? sender, FormClosingEventArgs e)
	{
		// Application.Exit is only called when loading failed or after a fatal error: nothing to save then.
		// The former Closing event was not raised in that case.
		if (e.CloseReason == CloseReason.ApplicationExitCall)
			return;

		// Settings or character editor shown: their edits are dropped, after a question when the user closes the window.
		if (!this.LeaveShownView(confirm: e.CloseReason == CloseReason.UserClosing))
		{
			e.Cancel = true;
			return;
		}

		e.Cancel = !this.DoCloseStuff();
	}

	/// <summary>
	/// Shows things that you may want to know before a close.
	/// Like holding an item
	/// </summary>
	/// <returns>TRUE if none of the conditions exist or the user selected to ignore the message</returns>
	private bool DoCloseStuff()
	{
		bool ok = false;
		try
		{
			// Make sure we are not dragging anything
			if (this.mainView.DragInfo.IsActive)
			{
				this.Services.MessageBoxes.Inform(Resources.MainFormHoldingItem, Resources.MainFormHoldingItem2);
				return false;
			}

			// No hot reload may land while files are saved and the form goes away.
			this.mainView.StopHotReload();

			// Files, then configuration (added by VillageIdiot), then git backup.
			var request = new SaveRequest(
				Files: true
				, Configuration: this.CurrentSessionConfiguration()
				, GitBackup: true);

			var result = this.saveOrchestrator.Save(request, this.AskSaveDecision);
			this.ShowSavedNotification(result);

			ok = true;
		}
		catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
		{
			// The user chose Abort after a file error: the window stays open.
			Log.LogError(exception, "Save files failed !");
			this.Services.MessageBoxes.ShowException(exception, icon: MessageBoxIcon.None);
		}

		return ok;
	}

	/// <summary>
	/// Handler for loading the main form
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private async void MainFormLoad(object? sender, EventArgs e)
	{
		StartupTiming.Mark(this.Log, "main form load");

		// async void event handler: nothing may escape it.
		try
		{
			await this.StartLoadingAsync();
		}
		catch (Exception ex)
		{
			Log.LogError(ex, "Starting the main form failed (loadingComplete = {LoadingComplete})", this.loadingComplete);
			this.Services.MessageBoxes.ShowException(ex, Resources.Form1ErrorLoadingResources, MessageBoxIcon.Error);

			// The form stays disabled until the loading completes: it cannot be used then.
			if (!this.loadingComplete)
				System.Windows.Forms.Application.Exit();
		}
	}

	/// <summary>
	/// Shows the splash screen and loads the resources (the git backup is synced when the form is created).
	/// </summary>
	private async Task StartLoadingAsync()
	{
		this.splashScreen = this.Services.Forms.Create<SplashScreenForm>();
		this.splashScreen.MaximumValue = 1;
		this.splashScreen.FormClosed += new FormClosedEventHandler(this.SplashScreenClosed);

		if (USettings.LoadAllFiles)
			this.splashScreen.MaximumValue += LoadAllFilesTotal();

		this.splashScreen.Show();
		this.splashScreen.Update();
		this.splashScreen.BringToFront();
		StartupTiming.Mark(this.Log, "splash shown");

		await this.LoadResourcesAsync();
	}

	/// <summary>
	/// Handler for showing the main form.
	/// Used to switch focus to the search text box.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void MainFormShown(object? sender, EventArgs e)
	{
		this.mainView.FocusHighlightSearch(dock: true);
	}

	/// <summary>
	/// Handler for moving the mouse wheel.
	/// Used to scroll through the vault list.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">MouseEventArgs data</param>
	private void MainFormMouseWheel(object? sender, MouseEventArgs e)
	{
		// The vault list is hidden with the main content.
		if (this.shownView is not null)
			return;

		this.mainView.ScrollVaultList(e.Delta);
	}

	/// <summary>
	/// Key Handler for the main form, which sees every key first (<see cref="Form.KeyPreview"/>): the keys of the window
	/// (search, zoom, clipboard, forge), from the key bind map. The keys of the item panels are read by the panels.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">KeyEventArgs data</param>
	private void MainFormKeyDown(object? sender, KeyEventArgs e)
	{
		// The keys of the main content sleep while another view is shown.
		if (this.shownView is not null)
			return;

		var gesture = KeyBindInput.FromKeys(e.KeyData);
		var action = this.Services.KeyBinds.Find(
			gesture
			, KeyBindAction.CopyToClipboard
			, KeyBindAction.PasteFromClipboard
			, KeyBindAction.Search
			, KeyBindAction.ZoomIn
			, KeyBindAction.ZoomOut
			, KeyBindAction.ZoomReset
			, KeyBindAction.Forge);

		// The forge keys only while the forge is shown, and not in a text box, where the key is typed.
		if (action == KeyBindAction.Forge && (!this.mainView.ForgeShown || this.FocusedControl() is TextBoxBase))
			action = null;

		if (action is not null)
			Log.LogDebug("Key {Gesture}: {Action}", gesture, action);

		switch (action)
		{
			// The clipboard keys also reach the focused control: a text box still copies and pastes its text.
			case KeyBindAction.CopyToClipboard:
				this.mainView.CopySelectionToClipboard();
				return;
			case KeyBindAction.PasteFromClipboard:
				_ = this.mainView.PasteFromClipboardAsync();
				return;
			case KeyBindAction.Search:
				this.OpenSearchDialog();
				break;
			case KeyBindAction.ZoomIn:
				this.ResizeFormCallback(this, new ResizeEventArgs(0.1F));
				break;
			case KeyBindAction.ZoomOut:
				this.ResizeFormCallback(this, new ResizeEventArgs(-0.1F));
				break;
			case KeyBindAction.ZoomReset:
				this.ResizeFormCallback(this, new ResizeEventArgs(1.0F));
				break;
			case KeyBindAction.Forge:
				this.mainView.ForgeFromShortcut();
				break;
			default:
				return;
		}

		// Read once: the panels and the focused control do not run it again.
		e.Handled = true;
		e.SuppressKeyPress = true;
	}

	/// <summary>The control that has the focus, inside the user controls holding it.</summary>
	private Control? FocusedControl()
	{
		Control? control = this.ActiveControl;
		while (control is ContainerControl { ActiveControl: { } inner })
			control = inner;

		return control;
	}


	/// <summary>
	/// Handler for the exit button.
	/// Closes the main form
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void ExitButtonClick(object? sender, EventArgs e) => this.Close();

	#endregion

	#region About

	/// <summary>
	/// Handler for clicking the about button.
	/// Shows the about dialog box.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void AboutButtonClick(object? sender, EventArgs e)
	{
		AboutBox dlg = this.Services.Forms.Create<AboutBox>();
		dlg.ShowDialog();
	}

	#endregion

}
