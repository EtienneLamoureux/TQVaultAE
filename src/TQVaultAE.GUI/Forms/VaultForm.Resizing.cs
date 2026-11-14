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
	/// Scales the form according to the scale factor.
	/// </summary>
	/// <param name="scaleFactor">Float which signifies the scale factor of the form.  This is an absolute from the original size unless useRelativeScaling is set to true.</param>
	/// <param name="useRelativeScaling">Indicates whether the scale factor is relative.  Used to support a resize operation.</param>
	protected virtual void ScaleForm(float scaleFactor, bool useRelativeScaling)
	{
		// Support relative resizing of the DB value.
		// scaleFactor is a zoom: the DPI is applied on top of it (UIService.Scale = zoom * DpiScale).
		float dpi = UIService.DpiScale;

		if (useRelativeScaling && scaleFactor != 1.0F)
		{
			float newZoom = UIService.Zoom * scaleFactor;
			if (newZoom > Presentation.Win32.UIService.MaxZoom || newZoom < Presentation.Win32.UIService.MinZoom)
				return;

			UIService.Scale = newZoom * dpi;
			using (ZoomScope.Enter())
				this.Scale(new SizeF(scaleFactor, scaleFactor));

			USettings.Scale = UIService.Zoom;
			USettings.Save();
		}
		else if (scaleFactor == 1.0F)
		{
			// Reset the border graphics to the originals.
			this.topBorder = Images.BorderTop;
			this.bottomBorder = Images.BorderBottom;
			this.sideBorder = Images.BorderSide;
			this.bottomRightCorner = Images.BorderBottomRightCorner;
			this.bottomLeftCorner = Images.BorderBottomLeftCorner;

			UIService.Scale = dpi;

			// Use the width since it is usually more drastic of a change.
			// especially when coming from a small size.
			var size = new SizeF(
				(float)NORMAL_FORMWIDTH * dpi / (float)this.Width,
				(float)NORMAL_FORMWIDTH * dpi / (float)this.Width);
			using (ZoomScope.Enter())
				this.Scale(size);

			USettings.Scale = 1.0F;
			USettings.Save();
		}
		else
		{
			// From the width, as a reset does: the main window may be taller than its design ratio, grown to fit its
			// content.
			float scaling = (float)NORMAL_FORMWIDTH * dpi / (float)this.Width * scaleFactor;

			UIService.Scale = scaleFactor * dpi;
			using (ZoomScope.Enter())
				this.Scale(new SizeF(scaling, scaling));

			USettings.Scale = UIService.Zoom;
			USettings.Save();
		}

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
				scale = UIService.Zoom + e.ResizeDelta;

			if (scale < 0.39F || scale > 2.01F)
				return;

			// A zoom in stops where the window would be larger than Windows lets it be: Windows would cut it behind the
			// layout's back, and the sizes the next zooms start from would be wrong.
			if (e.ResizeDelta > 0.0F && e.ResizeDelta != 1.0F && scale > this.LargestZoom() + ZoomTolerance)
				return;

			this.ScaleForm(scale, false);
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
			this.ScaleForm(scalingFactor, true);
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
