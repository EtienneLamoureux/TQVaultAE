//-----------------------------------------------------------------------
// <copyright file="VaultForm.Resizing.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.KeyBinds;
using TQVaultAE.Application.Layout;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Infrastructure;
using TQVaultAE.GUI.Interop;
using TQVaultAE.Presentation.Win32;

namespace TQVaultAE.GUI.Forms;

using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using TQVaultAE.GUI.Components;
using TQVaultAE.GUI.Models;

public partial class VaultForm
{
	/// <summary>
	/// Gets or sets a value indicating whether resizing is allowed when Custom Borders are enabled.
	/// </summary>
	public bool ResizeCustomAllowed
	{
		get => this.resizeCustomAllowed;

		set
		{
			if (this.systemMenu is not null)
				this.systemMenu.SizeEnabled = value;

			this.resizeCustomAllowed = value;
		}
	}

	/// <summary>
	/// Whether the dialog is scaled to the user's zoom when <see cref="FormFactory.Create{TForm}"/> makes it
	/// (<see cref="ApplyUserZoom"/>): the dialogs laid out in the designer at zoom 1. A dialog laid out in code is laid out
	/// at the zoom instead (<see cref="UserZoomForLayout"/>); the main window zooms by steps (<see cref="ScaleForm"/>).
	/// </summary>
	protected internal virtual bool FollowsUserZoom => false;

	/// <summary>The user's zoom the dialog was scaled or laid out at; 1 at its design size.</summary>
	public float ScaledZoom { get; private set; } = 1.0F;

	/// <summary>Set while <see cref="ApplyUserZoom"/> scales the dialog: its font grows with it (<see cref="ScaleControl"/>).</summary>
	private bool applyingUserZoom;

	/// <summary>
	/// Scales a dialog designed at zoom 1 to the user's zoom, once, before it is shown; Windows Forms adds the monitor's DPI
	/// when it loads. The dialog's font grows with it, for the controls that have no font of their own; the scaling
	/// controls grow theirs. Called by <see cref="FormFactory.Create{TForm}"/> for the dialogs that
	/// <see cref="FollowsUserZoom"/>, then <see cref="OnUserZoomApplied"/> lets the dialog fit what depends on the new sizes.
	/// </summary>
	public void ApplyUserZoom()
	{
		float zoom = this.UIService.Zoom;
		if (zoom == this.ScaledZoom)
			return;
		if (this.ScaledZoom != 1.0F)
			throw new InvalidOperationException("A dialog is scaled to the user's zoom once, from its design size.");

		this.applyingUserZoom = true;
		try
		{
			using (ZoomScope.Enter())
				this.Scale(new SizeF(zoom, zoom));
		}
		finally
		{
			this.applyingUserZoom = false;
		}

		this.ScaledZoom = zoom;
		this.OnUserZoomApplied();
	}

	/// <summary>The dialog was scaled to the user's zoom (<see cref="ApplyUserZoom"/>): it fits what depends on its new sizes.</summary>
	protected virtual void OnUserZoomApplied()
	{
	}

	/// <summary>
	/// Whether the open dialog follows the user's zoom when it changes (<see cref="FollowUserZoom"/>), called by the main
	/// window after each zoom on the dialogs it owns. Off for the dialogs that keep the zoom they opened at.
	/// </summary>
	protected internal virtual bool FollowsUserZoomChanges => false;

	/// <summary>
	/// The user's zoom changed while the dialog is open: scales it from <see cref="ScaledZoom"/> to the new zoom, as a zoom
	/// of the main window scales it (bounds, and the fonts of the scaling controls and of the title), in one rescale, then
	/// <see cref="OnUserZoomFollowed"/> lets the dialog lay out again what it makes in code. Nothing at the same zoom.
	/// </summary>
	public void FollowUserZoom()
	{
		float zoom = this.UIService.Zoom;
		if (zoom == this.ScaledZoom || this.IsDisposed)
			return;

		float factor = zoom / this.ScaledZoom;
		using (this.BeginRescale($"user zoom {this.ScaledZoom} -> {zoom}"))
		{
			using (ZoomScope.Enter())
				this.Scale(new SizeF(factor, factor));
			this.ScaledZoom = zoom;
			this.RescalePhase("scale");
			this.OnUserZoomFollowed();
		}
	}

	/// <summary>
	/// The open dialog was scaled to the user's new zoom (<see cref="FollowUserZoom"/>), its drawing frozen: it lays out
	/// again at <see cref="ScaledZoom"/> what it makes in code.
	/// </summary>
	protected virtual void OnUserZoomFollowed()
	{
	}

	/// <summary>
	/// The user's zoom a dialog laid out in code builds its controls at (fonts, sizes, gaps); recorded as the dialog's
	/// <see cref="ScaledZoom"/>, as if <see cref="ApplyUserZoom"/> had scaled it.
	/// </summary>
	protected float UserZoomForLayout()
		=> this.ScaledZoom = this.UIService.Zoom;

	/// <summary>
	/// The window's layout at zoom 1, kept by <see cref="KeepZoomSnapshot"/>; null for a window that does not zoom.
	/// </summary>
	private ZoomSnapshot? zoomSnapshot;

	/// <summary>
	/// Keeps the window's layout at zoom 1, once built: each zoom (<see cref="ScaleForm"/>) is then scaled from it.
	/// </summary>
	protected void KeepZoomSnapshot()
		=> this.zoomSnapshot = ZoomSnapshot.Take(this, UIService.Scale);

	/// <summary>
	/// Gets the window's zoom step (<see cref="ZoomSteps"/>): its scale is the step's whole cell size over the design cell.
	/// </summary>
	protected int ZoomStep { get; private set; } = ZoomSteps.Reset;

	/// <summary>Gets the nominal zoom of <see cref="ZoomStep"/> (0.4 to 2 by 0.1): the zoom saved.</summary>
	internal float NominalZoom => ZoomSteps.Nominal(this.ZoomStep);

	/// <summary>
	/// Gives the window the zoom of <paramref name="zoomStep"/> and saves it. Its scale is the step's cell size over the
	/// design cell, on top of the DPI. The layout kept at zoom 1 is given back, then scaled once by the new scale: the
	/// layout is the same whatever the zooms before, and a reset gives back the layout at zoom 1.
	/// </summary>
	/// <param name="zoomStep">The zoom step (<see cref="ZoomSteps.Reset"/> for the design size).</param>
	protected void ZoomTo(int zoomStep)
	{
		using (this.BeginRescale($"zoom step {this.ZoomStep} -> {zoomStep}"))
			this.ScaleForm(zoomStep);
	}

	/// <summary>The rescale running (<see cref="BeginRescale"/>); null between rescales.</summary>
	private RescaleScope? rescale;

	/// <summary>
	/// Starts one rescale of the window (a DPI change, a zoom step): its drawing frozen until it ends, then drawn once
	/// (<see cref="RescaleScope"/>), timed by phase (<see cref="RescalePhase"/>) and logged when the window is drawn
	/// again; null, and nothing started, inside a rescale already running.
	/// </summary>
	/// <param name="what">What changes, for the log line (formatted in the invariant culture).</param>
	protected IDisposable? BeginRescale(FormattableString what)
	{
		if (this.rescale is not null)
			return null;

		return this.rescale = new RescaleScope(this.Log, $"{this.Name}: {FormattableString.Invariant(what)}", this, () => this.rescale = null);
	}

	/// <summary>Ends the phase <paramref name="name"/> of the rescale running, if any (<see cref="BeginRescale"/>).</summary>
	protected void RescalePhase(string name)
		=> this.rescale?.Phase(name);

	/// <summary>
	/// Gives the window the zoom of <paramref name="zoomStep"/>, as <see cref="ZoomTo"/> does; called through it, which
	/// makes it one rescale.
	/// </summary>
	/// <param name="zoomStep">The zoom step (<see cref="ZoomSteps.Reset"/> for the design size).</param>
	protected virtual void ScaleForm(int zoomStep)
	{
		float previous = UIService.Scale;
		this.ZoomStep = Math.Clamp(zoomStep, 0, ZoomSteps.Count - 1);

		// A maximize scales freely to fill the window (MaximizedScale); the step kept is the one of before, saved and
		// given back by the restore.
		UIService.Scale = this.freeScale ?? ZoomSteps.Scale(this.ZoomStep, UIService.DpiScale);
		this.freeScale = null;

		// Maximized (or minimized), the window cannot be given its size at zoom 1, and the size Windows restores it to
		// follows the scaling of its size: it is scaled from the zoom before, as the content until it is restored; the
		// zoom of the restore gives the content its layout from zoom 1 back.
		if (!this.LayOutFromSnapshot(previous))
		{
			float factor = UIService.Scale / previous;
			using (ZoomScope.Enter())
				this.Scale(new SizeF(factor, factor));
		}

		this.RescalePhase("layout");

		USettings.Scale = this.NominalZoom;
		USettings.Save();
		this.RescalePhase("save");

		RefreshNormalizeBox();

		this.Log.LogDebug("Config.Settings.Default.Scale changed to {Scale} !", USettings.Scale);
	}

	/// <summary>
	/// Gives the window, in its normal state, the layout kept at zoom 1 scaled once to <see cref="UIService"/>'s scale
	/// (the zoom step's on the current DPI); false, and nothing done, without a snapshot or when the window is maximized
	/// or minimized.
	/// </summary>
	/// <param name="previous">The scale the controls made after the snapshot were made or last scaled at.</param>
	protected bool LayOutFromSnapshot(float previous)
	{
		if (this.zoomSnapshot is not { } snapshot || this.WindowState != FormWindowState.Normal)
			return false;

		snapshot.Zoom(previous, UIService.Scale, factor =>
		{
			// The window at zoom 1 too: the content is scaled as the window holding it; the size before the next resize
			// is not a resize by the user to keep in the design ratio.
			if (this.ClientSize != snapshot.ClientSize)
			{
				this.LastFormSize = this.SizeFromClientSize(snapshot.ClientSize);
				this.ClientSize = snapshot.ClientSize;
			}

			using (ZoomScope.Enter())
				this.Scale(new SizeF(factor, factor));
		});
		return true;
	}

	/// <summary>
	/// Callback for resizing of the main form.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">ResizeEventArgs data</param>
	protected void ResizeFormCallback(object? sender, ResizeEventArgs e)
	{
		if (e.ResizeDelta == 0.0F)
			// Nothing to do so we return.
			return;

		if (this.ScaleOnResize)
		{
			int step = e.ResizeDelta == 1.0F ? ZoomSteps.Reset : this.ZoomStep + Math.Sign(e.ResizeDelta);
			if (step < 0 || step >= ZoomSteps.Count)
				return;

			// A zoom in stops where the window would be larger than Windows lets it be: Windows would cut it behind the
			// layout's back, and the sizes the next zooms start from would be wrong.
			if (e.ResizeDelta > 0.0F && e.ResizeDelta != 1.0F && step > this.LargestStep())
				return;

			this.ZoomTo(step);
		}

		this.LastFormSize = this.Size;
	}

	/// <summary>
	/// The zoom step of the zoom saved from the last session (the nearest step: a drag of an older version left any
	/// zoom), lowered when the window would not fit the working area or what Windows lets a window be; saved when it
	/// changed.
	/// </summary>
	protected int InitialStep(Rectangle workingArea)
	{
		// The working area is in screen pixels, the normal size in design pixels.
		var design = new Size(NORMAL_FORMWIDTH, NORMAL_FORMHEIGHT);
		float dpi = UIService.DpiScale;
		int step = ZoomSteps.Nearest(USettings.Scale);

		// If screen is smaller than NORMAL_FORM sizes, adjust scaling (mostly at first run)
		if (workingArea.Width < NORMAL_FORMWIDTH * dpi || workingArea.Height < NORMAL_FORMHEIGHT * dpi)
			step = Math.Min(step, ZoomSteps.LargestFitting(design, dpi, workingArea.Size));

		// Nor larger than Windows lets a window be (a zoom saved on a larger monitor, or by a version without the limit).
		step = Math.Min(step, this.LargestStep());

		if (ZoomSteps.Nominal(step) != USettings.Scale)
		{
			USettings.Scale = ZoomSteps.Nominal(step);
			USettings.Save();
		}

		return step;
	}

	/// <summary>
	/// The largest client size Windows lets the window take: it cuts a larger size it is given, whatever the layout
	/// asked for.
	/// </summary>
	protected Size MaxClientSize
	{
		get
		{
			Size frame = this.SizeFromClientSize(Size.Empty);
			Size track = SystemInformation.MaxWindowTrackSize;
			return new Size(track.Width - frame.Width, track.Height - frame.Height);
		}
	}

	/// <summary>
	/// The largest zoom step at which the window at its design size fits <see cref="MaxClientSize"/>.
	/// </summary>
	protected int LargestStep()
		=> ZoomSteps.LargestFitting(new Size(NORMAL_FORMWIDTH, NORMAL_FORMHEIGHT), UIService.DpiScale, this.MaxClientSize);

	/// <summary>
	/// Handler for the ResizeBegin event.  Used for handling the maximize and minimize functions.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	protected virtual void ResizeBeginCallback(object? sender, EventArgs e)
	{
		if (this.WindowState != this.lastState)
		{
			// If we are coming out of a minimized or maximized state, we need to restore
			// the form the size prior to minimizing or maximizing.
			bool fromMinimizedMaximized = false;
			if (this.lastState == FormWindowState.Minimized || this.lastState == FormWindowState.Maximized)
			{
				this.LastFormSize = this.RestoreBounds.Size;
				fromMinimizedMaximized = true;
			}

			this.lastState = this.WindowState;

			this.DoResizeEndCallback(this, e, fromMinimizedMaximized);
		}
		else if (this.Size != this.LastFormSize && this.FormDesignRatio != 0.0F && this.ConstrainToDesignRatio)
		{
			Size tempSize = this.Size;

			// Keep the resizing within the form's design aspect ratio and bounds.
			// Find which was resized.
			if (tempSize.Height != this.LastFormSize.Height)
				tempSize.Width = Convert.ToInt32((float)tempSize.Height / this.FormDesignRatio);
			else
				tempSize.Height = Convert.ToInt32((float)tempSize.Width * this.FormDesignRatio);

			// Make sure we do not exceed the maximum values.
			if (tempSize.Height >= this.FormMaximumSize.Height || tempSize.Width >= this.FormMaximumSize.Width)
				tempSize = this.FormMaximumSize;

			if (tempSize.Height <= this.FormMinimumSize.Height || tempSize.Width <= this.FormMinimumSize.Width)
				tempSize = this.FormMinimumSize;

			this.Size = tempSize;
		}
	}

	/// <summary>
	/// The control the window is sized around, set by <see cref="FitAround"/>.
	/// </summary>
	private Control? fitContent;

	/// <summary>
	/// Sizes the window around <paramref name="content"/> (its inner table, at its place), plus the padding on the right
	/// and bottom, now and each time the content changes size (a text, a font, a DPI change), after a DPI change and on
	/// load. Not Form.AutoSize: it measures the close button of the custom border too, which sits on the right edge, so
	/// each layout pass widened the window by the button's margins and it never shrank back.
	/// </summary>
	protected void FitAround(Control content)
	{
		this.fitContent = content;
		content.SizeChanged += (_, _) => this.FitToContent();
		this.FitToContent();
	}

	/// <summary>
	/// Sizes the window around the content given to <see cref="FitAround"/>; nothing without one.
	/// </summary>
	private void FitToContent()
	{
		// Not while Windows Forms scales the window for a new DPI (OnDpiChanged fits it after): resized then, a window not
		// yet on the monitor of that DPI got a DPI change back inside the first one, half applied. A modal dialog given
		// its owner on a 96 dpi monitor (120 dpi system) came out scaled twice, at 0.64.
		if (this.scalingForDpi)
			return;

		if (this.fitContent is { } content)
			this.ClientSize = new Size(content.Right + this.Padding.Right, content.Bottom + this.Padding.Bottom);
	}

	/// <summary>Set while Windows Forms scales the window for a DPI change (<see cref="OnDpiChanged"/>).</summary>
	private bool scalingForDpi;

	/// <summary>
	/// The window moved to a monitor of another DPI and Windows Forms scaled it: the sizes kept for the user's resizing
	/// follow, else the end of the move would read the new size as a zoom.
	/// </summary>
	protected override void OnDpiChanged(DpiChangedEventArgs e)
	{
		float factor = (float)e.DeviceDpiNew / e.DeviceDpiOld;
		this.scalingForDpi = true;
		try
		{
			base.OnDpiChanged(e);
		}
		finally
		{
			this.scalingForDpi = false;
		}

		this.RescalePhase("Windows Forms scale");
		if (e.Cancel || factor == 1.0F)
			return;

		// Windows gives the window its suggested size after the layout was scaled, over the fit made while the content
		// changed: fit again once it is applied.
		this.FitToContent();

		this.FormMaximumSize = TQVaultAE.Application.Layout.WindowLayout.Scaled(this.FormMaximumSize, factor);
		this.FormMinimumSize = TQVaultAE.Application.Layout.WindowLayout.Scaled(this.FormMinimumSize, factor);
		// The layout made again for the new DPI; during a drag by the title bar, once the drag ends: Windows gives the
		// window its suggested size during its move loop, over a size set here, and the end of the drag then read the
		// difference as a resize by the user.
		if (this.inMoveLoop)
			this.followDpiPending = true;
		else
			this.FollowDpi();

		this.LastFormSize = this.Size;
		this.PlaceButtons();
	}

	/// <summary>
	/// The window moved to a monitor of another DPI and Windows Forms scaled it: a window that keeps its layout at zoom 1
	/// makes it again from there. Nothing by default.
	/// </summary>
	protected virtual void FollowDpi()
	{
	}

	/// <summary>Whether Windows' move or size loop is running (a drag by the title bar or the border).</summary>
	private bool inMoveLoop;

	/// <summary>Whether the DPI changed during the move loop: <see cref="FollowDpi"/> runs when it ends.</summary>
	private bool followDpiPending;

	/// <inheritdoc/>
	protected override void OnResizeBegin(EventArgs e)
	{
		this.inMoveLoop = true;
		base.OnResizeBegin(e);
	}

	/// <summary>
	/// The move or size loop ended: a DPI change during it makes the layout again now, before the end is read as a
	/// resize by the user (the window's size then is the one it takes, not a resize).
	/// </summary>
	protected override void OnResizeEnd(EventArgs e)
	{
		this.inMoveLoop = false;
		if (this.followDpiPending)
		{
			this.followDpiPending = false;
			using (this.BeginRescale($"DPI {this.DeviceDpi}, after the drag"))
				this.FollowDpi();
			this.LastFormSize = this.Size;
		}

		base.OnResizeEnd(e);
	}

	/// <summary>The scale the next <see cref="ScaleForm"/> gives instead of its step's: a maximize's.</summary>
	private float? freeScale;

	/// <summary>
	/// The scale a maximize gives, the window already at its maximized size: the window at its design size fills it. Not
	/// a zoom step: the item cells are rounded from it, and the gaps around the bags may differ by a pixel or two.
	/// </summary>
	protected virtual float MaximizedScale()
		=> Math.Min(this.ClientSize.Width / (float)NORMAL_FORMWIDTH, this.ClientSize.Height / (float)NORMAL_FORMHEIGHT);

	/// <summary>Gives the window <paramref name="scale"/>, not a zoom step's, its step kept (<see cref="freeScale"/>).</summary>
	protected void ScaleFreely(float scale)
	{
		this.freeScale = scale;
		this.ZoomTo(this.ZoomStep);
	}

	/// <summary>The zoom step before the window was maximized, given back when it is restored.</summary>
	private int? stepBeforeMaximize;

	protected virtual void DoResizeEndCallback(object? sender, EventArgs e, bool fromMinimizedMaximized = false)
	{
		// Dragging the form will trigger this event.
		// If the size did not change or if sizing is disabled we just skip it.
		if (this.Size == this.LastFormSize || !this.ResizeCustomAllowed)
			return;

		if (this.ScaleOnResize)
		{
			Size resizeTo = this.LastFormSize;
			int step;
			if (this.WindowState == FormWindowState.Maximized)
			{
				// Maximized: a free scale that fills the window (MaximizedScale), the step kept; the restore gives back
				// the step's scale.
				this.stepBeforeMaximize = this.ZoomStep;
				step = this.ZoomStep;
				this.freeScale = this.MaximizedScale();
			}
			else if (this.WindowState == FormWindowState.Minimized)
				step = this.ZoomStep;
			else if (fromMinimizedMaximized)
			{
				// Restored: the step it had before the maximize; zoom 1 with the reset modifier held (Shift).
				var held = this.Services.KeyBinds.Get(KeyBindAction.ResetZoomOnRestore, 0);
				bool reset = !held.IsNone && KeyBindInput.ToModifiers(Control.ModifierKeys).HasFlag(held.Modifiers);
				step = reset ? ZoomSteps.Reset : this.stepBeforeMaximize ?? this.ZoomStep;
				this.stepBeforeMaximize = null;
			}
			else
			{
				// A resize by the user: the step nearest to the zoom of the new width, from the width as a zoom does (the
				// main window may be taller than its design ratio, grown to fit its content); the window then takes the
				// step's size.
				float zoom = UIService.Zoom * this.Size.Width / resizeTo.Width;
				step = Math.Min(ZoomSteps.Nearest(zoom), this.LargestStep());
			}

			// Set it back to the original size since the Scale() call in ScaleForm() will also resize the form.
			this.Size = resizeTo;
			this.ZoomTo(step);
		}

		this.LastFormSize = this.Size;
	}

	/// <summary>
	/// Handler for the Resize event.  Used to update the borders after the window has been resized.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	protected virtual void ResizeCallback(object? sender, EventArgs e)
	{
		if (!this.ScaleOnResize)
		{
			this.PlaceButtons();
			this.CreateBorderRects();
			this.Invalidate();
		}
	}
}
