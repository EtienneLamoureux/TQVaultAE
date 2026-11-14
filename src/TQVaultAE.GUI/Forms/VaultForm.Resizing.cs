//-----------------------------------------------------------------------
// <copyright file="VaultForm.Resizing.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
/// <summary>
/// Abstract class used for constructing TQVault themed forms.
/// </summary>

using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.KeyBinds;
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
	protected virtual void ScaleForm(int zoomStep)
	{
		float previous = UIService.Scale;
		this.ZoomStep = Math.Clamp(zoomStep, 0, ZoomSteps.Count - 1);
		UIService.Scale = ZoomSteps.Scale(this.ZoomStep, UIService.DpiScale);

		if (this.ZoomStep == ZoomSteps.Reset)
		{
			// Reset the border graphics to the originals.
			this.topBorder = Images.BorderTop;
			this.bottomBorder = Images.BorderBottom;
			this.sideBorder = Images.BorderSide;
			this.bottomRightCorner = Images.BorderBottomRightCorner;
			this.bottomLeftCorner = Images.BorderBottomLeftCorner;
		}

		// Maximized (or minimized), the window cannot be given its size at zoom 1, and the size Windows restores it to
		// follows the scaling of its size: it is scaled from the zoom before, as the content until it is restored; the
		// zoom of the restore gives the content its layout from zoom 1 back.
		if (!this.LayOutFromSnapshot(previous))
		{
			float factor = UIService.Scale / previous;
			using (ZoomScope.Enter())
				this.Scale(new SizeF(factor, factor));
		}

		USettings.Scale = this.NominalZoom;
		USettings.Save();

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

			this.ScaleForm(step);
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
		if (this.fitContent is { } content)
			this.ClientSize = new Size(content.Right + this.Padding.Right, content.Bottom + this.Padding.Bottom);
	}

	/// <summary>
	/// The window moved to a monitor of another DPI and Windows Forms scaled it: the sizes kept for the user's resizing
	/// follow, else the end of the move would read the new size as a zoom.
	/// </summary>
	protected override void OnDpiChanged(DpiChangedEventArgs e)
	{
		float factor = (float)e.DeviceDpiNew / e.DeviceDpiOld;
		base.OnDpiChanged(e);
		if (e.Cancel || factor == 1.0F)
			return;

		// Windows gives the window its suggested size after the layout was scaled, over the fit made while the content
		// changed: fit again once it is applied.
		this.FitToContent();

		this.OriginalFormSize = TQVaultAE.Application.WindowLayout.Scaled(this.OriginalFormSize, factor);
		this.FormMaximumSize = TQVaultAE.Application.WindowLayout.Scaled(this.FormMaximumSize, factor);
		this.FormMinimumSize = TQVaultAE.Application.WindowLayout.Scaled(this.FormMinimumSize, factor);
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
			this.FollowDpi();
			this.LastFormSize = this.Size;
		}

		base.OnResizeEnd(e);
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
				// Maximized: the largest step whose window fits, the margin left around it. The step before is given
				// back by the restore.
				this.stepBeforeMaximize = this.ZoomStep;
				step = ZoomSteps.LargestFitting(new Size(NORMAL_FORMWIDTH, NORMAL_FORMHEIGHT), UIService.DpiScale, this.ClientSize);
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
			this.ScaleForm(step);
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
