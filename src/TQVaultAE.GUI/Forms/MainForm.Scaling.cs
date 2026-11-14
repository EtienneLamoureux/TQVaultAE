using Microsoft.Extensions.Logging;
using TQVaultAE.Application;
using TQVaultAE.GUI.Infrastructure;
using TQVaultAE.GUI.Components.Scaling;

namespace TQVaultAE.GUI.Forms;

/// <summary>
/// The size and zoom of the main window: built at zoom 1, given the user's zoom, the monitor it opens on, DPI changes,
/// and the layout of the main content inside the border.
/// </summary>
public partial class MainForm
{
	/// <summary>
	/// The main window moved to a monitor of another DPI. Before Windows Forms scales the layout, UIService takes the new
	/// DPI with the same zoom, so the panels and drawing built in code follow; the other windows open on this monitor.
	/// </summary>
	protected override void OnDpiChanged(DpiChangedEventArgs e)
	{
		UIService.DpiScale = e.DeviceDpiNew / UIService.DESIGNDPI;
		UIService.Scale = ZoomSteps.Scale(this.ZoomStep, UIService.DpiScale);
		this.Log.LogDebug("DPI {Old} -> {New}: scale {Scale}", e.DeviceDpiOld, e.DeviceDpiNew, UIService.Scale);

		Tooltips.ItemTooltip.HideTooltip();
		var watch = System.Diagnostics.Stopwatch.StartNew();
		base.OnDpiChanged(e);
		this.Invalidate(true);
		this.Log.LogDebug("DPI {New}: window scaled in {Elapsed} ms ({View} shown)", e.DeviceDpiNew, watch.ElapsedMilliseconds, this.shownView?.Name ?? "main content");
	}

	/// <summary>
	/// Windows Forms scaled the layout from the DPI before, rounded again at each change (a round trip 144 -> 96 -> 120 ->
	/// 144 left 88 controls off): the layout is made again from the one kept at zoom 1, scaled once to the new DPI and the
	/// same zoom step, as a zoom does, and the window takes the step's size there.
	/// </summary>
	protected override void FollowDpi()
	{
		if (this.LayOutFromSnapshot(UIService.Scale))
		{
			this.mainView.ApplyHighlightFonts();
			this.SizeToZoom();
			this.Invalidate(true);
		}
	}

	/// <summary>
	/// Gives the main form the zoom of <paramref name="zoomStep"/>, then the fonts set in code and the window's size at that
	/// zoom.
	/// </summary>
	/// <param name="zoomStep">The zoom step (<see cref="ZoomSteps.Reset"/> for the design size).</param>
	protected override void ScaleForm(int zoomStep)
	{
		base.ScaleForm(zoomStep);

		// The highlight link and the filters panel's non-zooming controls do not scale their fonts by themselves.
		this.mainView.ApplyHighlightFonts();

		this.SizeToZoom();

		this.mainView.ShowNotification(string.Empty);

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
			var client = TQVaultAE.Application.WindowLayout.Scaled(DesignClientSize, UIService.Scale);
			client = TQVaultAE.Application.WindowLayout.GrowToFit(
				client, this.ContentAreaOf(client), this.mainView.Location, this.mainView.PanelsPreferredSize);

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
		this.startupStep = this.InitialStep(workingArea);

		this.ScaleOnResize = false;

		this.ClientSize = TQVaultAE.Application.WindowLayout.Scaled(DesignClientSize, UIService.DpiScale);

		this.ScaleOnResize = true;

		UIService.Scale = ZoomSteps.Scale(ZoomSteps.Reset, UIService.DpiScale);

		// The height / width ratio a resize or a maximize keeps.
		this.FormDesignRatio = (float)this.Height / (float)this.Width;
	}

	/// <summary>
	/// The zoom the window takes once built at zoom 1 (<see cref="SetupFormSize"/>).
	/// </summary>
	private int startupStep = ZoomSteps.Reset;

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

		if (this.startupStep != ZoomSteps.Reset)
			this.ScaleForm(this.startupStep);
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
}
