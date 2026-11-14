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
		// The vault list is hidden with the main content.
		if (this.shownView is not null)
			return;

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
		if (action == KeyBindAction.Forge && (!this.forgePanel.Visible || this.FocusedControl() is TextBoxBase))
			action = null;

		if (action is not null)
			Log.LogDebug("Key {Gesture}: {Action}", gesture, action);

		switch (action)
		{
			// The clipboard keys also reach the focused control: a text box still copies and pastes its text.
			case KeyBindAction.CopyToClipboard:
				HandleClipboardExport();
				return;
			case KeyBindAction.PasteFromClipboard:
				_ = HandleClipboardImportAsync();
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
				this.forgePanel.ForgeFromShortcut();
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
		var watch = System.Diagnostics.Stopwatch.StartNew();
		base.OnDpiChanged(e);
		this.Invalidate(true);
		this.Log.LogDebug("DPI {New}: window scaled in {Elapsed} ms ({View} shown)", e.DeviceDpiNew, watch.ElapsedMilliseconds, this.shownView?.Name ?? "main content");
	}

	/// <summary>
	/// Gives the main form <paramref name="zoom"/>, then the fonts set in code and the window's size at that zoom.
	/// </summary>
	/// <param name="zoom">The zoom, an absolute value (1 for the design size).</param>
	protected override void ScaleForm(float zoom)
	{
		base.ScaleForm(zoom);

		// A link label does not scale its font with the zoom by itself.
		this.ApplyHighlightLabelFont();

		this.SizeToZoom();

		this.NotificationText.Text = string.Empty;

		this.Invalidate();
	}

	/// <summary>The client size of the window at zoom 1 and 96 dpi.</summary>
	private static readonly Size DesignClientSize = new(NORMAL_FORMWIDTH, NORMAL_FORMHEIGHT);

	/// <summary>
	/// Gives the window in its normal state its size at the current zoom: the design size at the current scale, grown
	/// where the panels need more room. They are not exactly proportional to the zoom (whole-pixel item cells, margins
	/// and fonts rounded), and nothing in them may be cut by the border.
	/// </summary>
	private void SizeToZoom()
	{
		if (this.WindowState == FormWindowState.Normal)
		{
			// A maximize and restore gives back the zoom within float noise (0.99997): not a pixel less for it.
			var zoom = MathF.Round(UIService.Zoom, 3);
			var client = TQVaultAE.Application.WindowLayout.Scaled(DesignClientSize, zoom * UIService.DpiScale);
			client = TQVaultAE.Application.WindowLayout.GrowToFit(
				client, this.ContentAreaOf(client), this.mainView.Location, this.tableLayoutPanelMain.PreferredSize);

			// Never larger than Windows lets the window be (the zoom stops before, LargestZoom): Windows would cut it
			// itself, the resize would then be read as the user's and constrained to the design ratio, down to a 4 x 4
			// window before the startup set the resizing bounds, and the next zooms would start from the wrong width.
			client = TQVaultAE.Application.WindowLayout.Within(client, this.MaxClientSize);
			if (client != this.ClientSize)
			{
				// The size before the next resize: the change is not a resize by the user to keep in the design ratio.
				this.LastFormSize = this.SizeFromClientSize(client);
				this.ClientSize = client;
			}
		}

		// The caption buttons last, on the final size: placed before it (ScaleForm places them), the resize moved
		// them and a corner of the close button kept what the moved controls had left there.
		this.PlaceButtons();
	}

	/// <summary>The highlight label's font at the current zoom.</summary>
	private void ApplyHighlightLabelFont()
		=> this.scalingLabelHighlight.Font = FontService.GetFontLight(10F, UIService.FontZoom);

	/// <summary>
	/// The main content, and its panels' table, fill the window inside its border at each layout (startup, zoom,
	/// maximize, restore, DPI change): the table's last row and column take what the panels leave, so it never reaches
	/// past the border nor over the resize grips, and never keeps a size the window had before.
	/// </summary>
	protected override void OnLayout(LayoutEventArgs levent)
	{
		base.OnLayout(levent);

		// The views are set before InitializeComponent, which lays out before the border is set up.
		if (this.mainView is null)
			return;

		var size = TQVaultAE.Application.WindowLayout.FillTo(this.ContentArea, this.mainView.Location);
		if (this.mainView.Size != size)
			this.mainView.Size = size;

		// The other views take the place of the main content.
		if (this.shownView is { } view && view.Bounds != this.mainView.Bounds)
			view.Bounds = this.mainView.Bounds;
	}

	/// <summary>
	/// Sets the size of the main form at zoom 1 for startup; <see cref="ApplyStartupZoom"/> gives it the user's zoom once
	/// the panels are built.
	/// </summary>
	private void SetupFormSize()
	{
		this.DrawCustomBorder = true;
		this.ResizeCustomAllowed = true;
		this.fadeInterval = USettings.FadeInInterval;

		this.openingScreen = this.PlaceForCreation();

		// Creates the window.
		Rectangle workingArea = Screen.FromControl(this).WorkingArea;

		// The zoom fits the monitor the window opens on, measured at the DPI it is built at.
		if (this.openingScreen is { } opening && MonitorDpi.Of(opening) is int openingDpi)
			workingArea = new Rectangle(
				opening.WorkingArea.Location,
				TQVaultAE.Application.WindowLayout.Scaled(opening.WorkingArea.Size, (float)this.DeviceDpi / openingDpi));

		// Windows Forms already scaled the designer layout to the screen DPI (AutoScaleMode.Dpi); the code-built panels and
		// the drawing follow UIService.Scale, which gets the same factor on top of the user's zoom.
		UIService.DpiScale = this.CurrentAutoScaleDimensions.Width / UIService.DESIGNDPI;

		// The window and its panels are built at zoom 1, then take the user's zoom as a live zoom gives it
		// (ApplyStartupZoom): every control then follows the zoom the same way, so the content keeps the room the zoom 1
		// layout leaves it inside the window, and a reset gives back the layout of a start at zoom 1.
		this.startupZoom = this.InitialZoom(workingArea);

		this.ScaleOnResize = false;

		this.ClientSize = TQVaultAE.Application.WindowLayout.Scaled(DesignClientSize, UIService.DpiScale);

		this.ScaleOnResize = true;

		UIService.Scale = UIService.DpiScale;

		// The height / width ratio a resize or a maximize keeps.
		this.FormDesignRatio = (float)this.Height / (float)this.Width;
	}

	/// <summary>
	/// The zoom the window takes once built at zoom 1 (<see cref="SetupFormSize"/>).
	/// </summary>
	private float startupZoom = 1.0F;

	/// <summary>
	/// Places the window, before it is created, on a monitor at the DPI Windows Forms laid the designer out at (the
	/// system DPI), when the monitor under the mouse, where it opens (CenterScreen), has another DPI: created there, the
	/// window was scaled while it was created, through DPI changes nested in each other, which left sizes of the other
	/// DPI in the content (panels cut and overlapping, a wider window). Returns that monitor, which the window is moved
	/// to once built (<see cref="MoveToOpeningScreen"/>); null when the window can be created where it opens.
	/// </summary>
	private Screen? PlaceForCreation()
	{
		var opening = Screen.FromPoint(Control.MousePosition);
		if (MonitorDpi.Of(opening) is not int openingDpi || openingDpi == this.DeviceDpi)
			return null;

		var creation = Screen.AllScreens.FirstOrDefault(screen => MonitorDpi.Of(screen) == this.DeviceDpi);
		if (creation is null)
			return null;

		this.StartPosition = FormStartPosition.Manual;
		this.Location = TQVaultAE.Application.WindowLayout.CenteredIn(creation.WorkingArea, this.Size);
		return opening;
	}

	/// <summary>
	/// The monitor the window opens on, when it is created on another one (<see cref="PlaceForCreation"/>).
	/// </summary>
	private Screen? openingScreen;

	/// <summary>
	/// Moves the window, built on the monitor it was created on (<see cref="PlaceForCreation"/>), to the one it opens on,
	/// at the size it takes there: Windows sends one DPI change, which scales it as a move there does, then it is
	/// centered there. Once shown, still transparent before its fade in: a hidden window scaled to another DPI kept
	/// sizes of the DPI before in its auto-sized panels (the vault list row was 5 px higher).
	/// </summary>
	private void MoveToOpeningScreen()
	{
		if (this.openingScreen is not { } opening)
			return;

		this.openingScreen = null;
		var size = MonitorDpi.Of(opening) is int dpi
			? TQVaultAE.Application.WindowLayout.Scaled(this.Size, (float)dpi / this.DeviceDpi)
			: this.Size;
		this.Location = TQVaultAE.Application.WindowLayout.CenteredIn(opening.WorkingArea, size);
		this.Location = TQVaultAE.Application.WindowLayout.CenteredIn(Screen.FromControl(this).WorkingArea, this.Size);
	}

	/// <summary>
	/// Gives the window built at zoom 1 the user's zoom, through the same scaling as a live zoom, then keeps the sizes
	/// the user's resizing and maximizing start from and centers the window on its monitor.
	/// </summary>
	private void ApplyStartupZoom()
	{
		this.KeepZoomSnapshot();

		if (this.startupZoom != 1.0F)
			this.ScaleForm(this.startupZoom);
		else
			this.SizeToZoom();

		this.Log.LogDebug("Zoom {Zoom} at {Dpi} times the design DPI: scale {Scale}", UIService.Zoom, UIService.DpiScale, UIService.Scale);

		// The bounds of the user's resizing: the window at its design size at the largest and the smallest zoom, not from
		// its size at the zoom it opens at (a start at zoom 0.5 allowed a window only as large as zoom 1).
		var designWindow = this.SizeFromClientSize(TQVaultAE.Application.WindowLayout.Scaled(DesignClientSize, UIService.DpiScale));
		this.FormMaximumSize = TQVaultAE.Application.WindowLayout.Scaled(designWindow, Presentation.Win32.UIService.MaxZoom);
		this.FormMinimumSize = TQVaultAE.Application.WindowLayout.Scaled(designWindow, Presentation.Win32.UIService.MinZoom);

		this.OriginalFormSize = this.Size;

		this.LastFormSize = this.Size;

		// A maximize keeps the aspect ratio on the monitor the window is on then (VaultForm, WM_GETMINMAXINFO).
		this.Location = TQVaultAE.Application.WindowLayout.CenteredIn(Screen.FromControl(this).WorkingArea, this.Size);
	}


	#endregion
}
