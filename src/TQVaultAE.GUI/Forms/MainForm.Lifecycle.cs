using Microsoft.Extensions.Logging;
using TQVaultAE.Application.Results;
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
	/// Handler for the ResizeEnd event.  Used to scale the internal controls after the window has been resized.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	protected override void ResizeEndCallback(object? sender, EventArgs e)
		// That override Look dumb but needed by Visual Studio WInform Designer
		=> base.ResizeEndCallback(sender, e);

	/// <summary>
	/// Handler for the Resize event.  Used for handling the maximize and minimize functions.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	protected override void ResizeBeginCallback(object? sender, EventArgs e)
		// That override Look dumb but needed by Visual Studio WInform Designer
		=> base.ResizeBeginCallback(sender, e);

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
			if (this.DragInfo.IsActive)
			{
				this.Services.MessageBoxes.Inform(Resources.MainFormHoldingItem, Resources.MainFormHoldingItem2);
				return false;
			}

			// No hot reload may land while files are saved and the form goes away.
			this._fileReloadDebouncer?.Dispose();
			this._fileReloadDebouncer = null;

			// Files, then configuration (added by VillageIdiot), then git backup.
			var request = new SaveRequest(
				Files: true
				, Configuration: this.CurrentSessionConfiguration()
				, GitBackup: true);

			var result = this.saveOrchestrator.Save(request, this.AskSaveDecision);
			this.ShowSavedNotification(result);
			if (result.ConfigChanged is bool configChanged)
				this.configChanged = configChanged;

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

		await this.LoadResourcesAsync();
	}

	/// <summary>
	/// Handler for key presses on the main form
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">KeyPressEventArgs data</param>
	private void MainFormKeyPress(object? sender, KeyPressEventArgs e)
	{
		// Don't handle this here since we handle key presses within each component.
		////if (e.KeyChar != (char)27)
		////e.Handled = true;
	}

	/// <summary>
	/// Handler for showing the main form.
	/// Used to switch focus to the search text box.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void MainFormShown(object? sender, EventArgs e)
	{
		this.scalingTextBoxHighlight.Dock = DockStyle.Fill;
		this.scalingTextBoxHighlight.Focus();
	}

	/// <summary>
	/// Handler for moving the mouse wheel.
	/// Used to scroll through the vault list.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">MouseEventArgs data</param>
	private void MainFormMouseWheel(object? sender, MouseEventArgs e)
	{
		// Force a single line regardless of the delta value.
		int numberOfTextLinesToMove = ((e.Delta > 0) ? 1 : 0) - ((e.Delta < 0) ? 1 : 0);
		if (numberOfTextLinesToMove != 0)
		{
			int vaultSelection = this.vaultListComboBox.SelectedIndex;
			vaultSelection -= numberOfTextLinesToMove;
			if (vaultSelection < 1)
				vaultSelection = 1;

			if (vaultSelection >= this.vaultListComboBox.Items.Count)
				vaultSelection = this.vaultListComboBox.Items.Count - 1;

			this.vaultListComboBox.SelectedIndex = vaultSelection;
		}
	}

	/// <summary>
	/// Key Handler for the main form.  Most keystrokes should be handled by the individual panels or the search text box.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">KeyEventArgs data</param>
	private void MainFormKeyDown(object? sender, KeyEventArgs e)
	{
		if (e.KeyData == (Keys.Control | Keys.C))
		{
			HandleClipboardExport();
			return;
		}

		if (e.KeyData == (Keys.Control | Keys.V))
		{
			_ = HandleClipboardImportAsync();
			return;
		}

		if (e.KeyData == (Keys.Control | Keys.F))
			this.ActivateSearchCallback(this, new SackPanelEventArgs(null, null));

		if (e.KeyData == (Keys.Control | Keys.Add) || e.KeyData == (Keys.Control | Keys.Oemplus))
			this.ResizeFormCallback(this, new ResizeEventArgs(0.1F));

		if (e.KeyData == (Keys.Control | Keys.Subtract) || e.KeyData == (Keys.Control | Keys.OemMinus))
			this.ResizeFormCallback(this, new ResizeEventArgs(-0.1F));

		if (e.KeyData == (Keys.Control | Keys.Home))
			this.ResizeFormCallback(this, new ResizeEventArgs(1.0F));
	}


	/// <summary>
	/// Handles Timer ticks for fading in the main form.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void FadeInTimerTick(object? sender, EventArgs e)
	{
		if (this.Opacity < 1)
			this.Opacity = Math.Min(1.0F, this.Opacity + this.fadeInterval);
		else
			this.fadeInTimer.Stop();
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
		// Windows Forms scaled the dialog to the DPI: only the zoom is left.
		using (ZoomScope.Enter())
			dlg.Scale(new SizeF(UIService.Zoom, UIService.Zoom));
		dlg.ShowDialog();
	}

	#endregion

	#region Scaling

	/// <summary>
	/// The main window moved to a monitor of another DPI. Before Windows Forms scales the layout, UIService takes the new
	/// DPI with the same zoom, so the panels and drawing built in code follow; the other windows open on this monitor.
	/// </summary>
	protected override void OnDpiChanged(DpiChangedEventArgs e)
	{
		var zoom = UIService.Zoom;
		UIService.DpiScale = e.DeviceDpiNew / UIService.DESIGNDPI;
		UIService.Scale = zoom * UIService.DpiScale;
		this.Log.LogDebug("DPI {Old} -> {New}: scale {Scale}", e.DeviceDpiOld, e.DeviceDpiNew, UIService.Scale);

		Tooltips.ItemTooltip.HideTooltip();
		base.OnDpiChanged(e);
		this.Invalidate(true);
	}

	/// <summary>
	/// Scales the main form according to the scale factor.
	/// </summary>
	/// <param name="scaleFactor">Float which signifies the scale factor of the form.  This is an absolute from the original size unless useRelativeScaling is set to true.</param>
	/// <param name="useRelativeScaling">Indicates whether the scale factor is relative.  Used to support a resize operation.</param>
	protected override void ScaleForm(float scaleFactor, bool useRelativeScaling)
	{
		base.ScaleForm(scaleFactor, useRelativeScaling);

		this.NotificationText.Text = string.Empty;

		this.Invalidate();
	}

	/// <summary>
	/// Sets the size of the main form along with scaling the internal controls for startup.
	/// </summary>
	private void SetupFormSize()
	{
		this.DrawCustomBorder = true;
		this.ResizeCustomAllowed = true;
		this.fadeInterval = USettings.FadeInInterval;

		Rectangle workingArea = Screen.FromControl(this).WorkingArea;

		// Windows Forms already scaled the designer layout to the screen DPI (AutoScaleMode.Dpi); the code-built panels and
		// the drawing follow UIService.Scale, which gets the same factor on top of the user's zoom.
		UIService.DpiScale = this.CurrentAutoScaleDimensions.Width / UIService.DESIGNDPI;

		this.ScaleOnResize = false;

		this.ClientSize = InitialScaling(workingArea);

		this.ScaleOnResize = true;

		UIService.Scale = USettings.Scale * UIService.DpiScale;
		this.Log.LogDebug("Zoom {Zoom} at {Dpi} times the design DPI: scale {Scale}", UIService.Zoom, UIService.DpiScale, UIService.Scale);

		// The caption buttons were placed for the previous scale.
		this.PlaceButtons();

		// Save the height / width ratio for resizing.
		this.FormDesignRatio = (float)this.Height / (float)this.Width;
		this.FormMaximumSize = new Size(this.Width * 2, this.Height * 2);
		this.FormMinimumSize = new Size(
			Convert.ToInt32((float)this.Width * 0.4F),
			Convert.ToInt32((float)this.Height * 0.4F));

		this.OriginalFormSize = this.Size;
		this.OriginalFormScale = USettings.Scale;

		this.LastFormSize = this.Size;

		// A maximize keeps the aspect ratio on the monitor the window is on then (VaultForm, WM_GETMINMAXINFO).
		this.Location = new Point(workingArea.Left + Convert.ToInt16((workingArea.Width - this.ClientSize.Width) / 2), workingArea.Top + Convert.ToInt16((workingArea.Height - this.ClientSize.Height) / 2));
	}


	#endregion
}
