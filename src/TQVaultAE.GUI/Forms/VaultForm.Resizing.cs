//-----------------------------------------------------------------------
// <copyright file="VaultForm.Resizing.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
/// <summary>
/// Abstract class used for constructing TQVault themed forms.
/// </summary>

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
	/// Gives the window <paramref name="zoom"/> (the DPI is applied on top of it: UIService.Scale = zoom * DpiScale) and
	/// saves it. The layout kept at zoom 1 is given back, then scaled once by the zoom: the layout is the same whatever
	/// the zooms before, and a reset gives back the layout at zoom 1.
	/// </summary>
	/// <param name="zoom">The zoom, an absolute value (1 for the design size).</param>
	protected virtual void ScaleForm(float zoom)
	{
		float previous = UIService.Scale;
		UIService.Scale = zoom * UIService.DpiScale;

		if (zoom == 1.0F)
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
		if (this.zoomSnapshot is { } snapshot && this.WindowState == FormWindowState.Normal)
		{
			snapshot.Zoom(previous, UIService.Scale, factor =>
			{
				// The window at zoom 1 too: the content is scaled as the window holding it; the size before the next
				// resize is not a resize by the user to keep in the design ratio.
				if (this.ClientSize != snapshot.ClientSize)
				{
					this.LastFormSize = this.SizeFromClientSize(snapshot.ClientSize);
					this.ClientSize = snapshot.ClientSize;
				}

				using (ZoomScope.Enter())
					this.Scale(new SizeF(factor, factor));
			});
		}
		else
		{
			float factor = UIService.Scale / previous;
			using (ZoomScope.Enter())
				this.Scale(new SizeF(factor, factor));
		}

		USettings.Scale = UIService.Zoom;
		USettings.Save();

		RefreshNormalizeBox();

		this.Log.LogDebug("Config.Settings.Default.Scale changed to {Scale} !", USettings.Scale);
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
			float scale;
			if (e.ResizeDelta == 1.0F)
				scale = e.ResizeDelta;
			else
				// Without the float noise of the steps added (1.0000001 after zooms out and back in): the layout depends on
				// the zoom alone, and 2.5000002 pixels rounded to 3 where the zoom itself gives 2.
				scale = MathF.Round(UIService.Zoom + e.ResizeDelta, 2);

			if (scale < 0.39F || scale > 2.01F)
				return;

			// A zoom in stops where the window would be larger than Windows lets it be: Windows would cut it behind the
			// layout's back, and the sizes the next zooms start from would be wrong.
			if (e.ResizeDelta > 0.0F && e.ResizeDelta != 1.0F && scale > this.LargestZoom() + ZoomTolerance)
				return;

			this.ScaleForm(scale);
		}

		this.LastFormSize = this.Size;
	}

	/// <summary>
	/// The zoom saved from the last session, lowered (and saved) when the window would not fit the working area at it.
	/// </summary>
	protected float InitialZoom(Rectangle workingArea)
	{
		// The working area is in screen pixels, the normal size in design pixels.
		float dpi = UIService.DpiScale;

		// If screen is smaller than NORMAL_FORM sizes, adjust scaling (mostly at first run)
		if (workingArea.Width < NORMAL_FORMWIDTH * dpi || workingArea.Height < NORMAL_FORMHEIGHT * dpi)
		{
			var initialScale = Math.Min(
				Convert.ToSingle(workingArea.Width) / (NORMAL_FORMWIDTH * dpi)
				, Convert.ToSingle(workingArea.Height) / (NORMAL_FORMHEIGHT * dpi)
			);

			if (USettings.Scale > initialScale)
			{
				USettings.Scale = initialScale;
				USettings.Save();
			}
		}

		// Nor larger than Windows lets a window be (a zoom saved on a larger monitor, or by a version without the limit).
		float largest = this.LargestZoom();
		if (USettings.Scale > largest + ZoomTolerance)
		{
			USettings.Scale = largest;
			USettings.Save();
		}

		return USettings.Scale;
	}

	/// <summary>The float noise a zoom step leaves (1.3000001): not a reason to refuse it.</summary>
	private const float ZoomTolerance = 0.0001F;

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
	/// The largest zoom at which the window at its design size fits <see cref="MaxClientSize"/>.
	/// </summary>
	protected float LargestZoom()
		=> TQVaultAE.Application.WindowLayout.LargestZoom(
			new Size(NORMAL_FORMWIDTH, NORMAL_FORMHEIGHT), UIService.DpiScale, this.MaxClientSize);

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
		this.LastFormSize = this.Size;
		this.PlaceButtons();
	}

	protected virtual void DoResizeEndCallback(object? sender, EventArgs e, bool fromMinimizedMaximized = false)
	{
		// Dragging the form will trigger this event.
		// If the size did not change or if sizing is disabled we just skip it.
		if (this.Size == this.LastFormSize || !this.ResizeCustomAllowed)
			return;

		if (this.ScaleOnResize)
		{
			Size resizeTo = this.LastFormSize;
			// Scale the internal controls to the new size, from the width as a zoom does: the main window may be taller
			// than its design ratio, grown to fit its content.
			float scalingFactor = (float)this.Size.Width / (float)resizeTo.Width;

			// Reset scaling to 1.0 if resize from minimize/maximize to normal with its modifier held (Shift).
			if (fromMinimizedMaximized)
			{
				var held = this.Services.KeyBinds.Get(KeyBindAction.ResetZoomOnRestore, 0);
				if (!held.IsNone && KeyBindInput.ToModifiers(Control.ModifierKeys).HasFlag(held.Modifiers))
				{
					resizeTo = new Size(NORMAL_FORMWIDTH, NORMAL_FORMHEIGHT);
					scalingFactor = 1.0F;
				}
			}

			// Set it back to the original size since the Scale() call in ScaleForm() will also resize the form.
			this.Size = resizeTo;

			// The zoom the new width gives (a factor of 1 resets the zoom); out of the zoom's range, the zoom stays.
			float zoom = scalingFactor == 1.0F ? 1.0F : UIService.Zoom * scalingFactor;
			if (zoom > Presentation.Win32.UIService.MaxZoom || zoom < Presentation.Win32.UIService.MinZoom)
				zoom = UIService.Zoom;

			this.ScaleForm(zoom);
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
