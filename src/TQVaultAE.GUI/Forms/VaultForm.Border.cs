//-----------------------------------------------------------------------
// <copyright file="VaultForm.Border.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
/// <summary>
/// Abstract class used for constructing TQVault themed forms.
/// </summary>

using TQVaultAE.Application;
using TQVaultAE.GUI.Components.Rendering;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Interop;
using TQVaultAE.Presentation.Win32;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.GUI.Forms;

using System;
using System.Drawing;
using System.Windows.Forms;
using TQVaultAE.Config;
using TQVaultAE.GUI.Components;
using TQVaultAE.GUI.Models;
using TQVaultAE.Presentation;

public partial class VaultForm
{
	/// <summary>
	/// The space the TQ border (<see cref="DrawCustomBorder"/>) takes on each side of the window: its pictures are drawn
	/// at their own size. A dialog laid out in code keeps its content inside with this padding.
	/// </summary>
	internal static Padding CustomBorderThickness => customBorderThickness.Value;

	private static readonly Lazy<Padding> customBorderThickness = new(() =>
	{
		using var top = Images.BorderTop;
		using var side = Images.BorderSide;
		using var bottom = Images.BorderBottom;
		return new Padding(side.Width, top.Height, side.Width, bottom.Height);
	});

	/// <summary>
	/// Gets or sets a value indicating whether the custom borders will be drawn.
	/// </summary>
	public bool DrawCustomBorder
	{
		get => this.drawCustomBorder;

		set
		{
			if (value)
			{
				this.FormBorderStyle = FormBorderStyle.None;

				this.buttonClose.Visible = true;
				this.buttonClose.Enabled = true;

				this.buttonMaximize.Visible = this.MaximizeBox;
				this.buttonMaximize.Enabled = this.MaximizeBox;

				this.ButtonScaleTo1.Visible = this.NormalizeBox;
				this.ButtonScaleTo1.Enabled = this.NormalizeBox;

				this.buttonMinimize.Visible = this.MinimizeBox;
				this.buttonMinimize.Enabled = this.MinimizeBox;

				this.PlaceButtons();
			}
			else
			{
				this.FormBorderStyle = FormBorderStyle.FixedSingle;
				this.buttonMaximize.Visible = false;
				this.buttonMaximize.Enabled = false;
				this.ButtonScaleTo1.Visible = false;
				this.ButtonScaleTo1.Enabled = false;
				this.buttonMinimize.Visible = false;
				this.buttonMinimize.Enabled = false;
				this.buttonClose.Visible = false;
				this.buttonClose.Enabled = false;
			}

			this.drawCustomBorder = value;
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether the Maximize button is displayed in the caption bar of the form.
	/// </summary>
	public new bool MaximizeBox
	{
		get => base.MaximizeBox;

		set
		{
			if (this.systemMenu is not null)
				this.systemMenu.MaximizeEnabled = value;

			if (this.buttonMaximize is not null)
			{
				this.buttonMaximize.Visible = value;
				this.buttonMaximize.Enabled = value;
			}

			base.MaximizeBox = value;
		}
	}

	/// <summary>
	/// Gets a value indicating whether the Normalize button is displayed in the caption bar of the form.
	/// </summary>
	public virtual bool NormalizeBox
	{
		get => _NormalizeBox && (this.UIService?.Zoom ?? 1.0F) != 1.0F;
		set
		{
			_NormalizeBox = value;
			RefreshNormalizeBox();
		}
	}

	private void RefreshNormalizeBox()
	{
		var val = _NormalizeBox && (this.UIService?.Zoom ?? 1.0F) != 1.0F;
		if (this.systemMenu is not null)
			this.systemMenu.NormalizeEnabled = val;

		if (this.ButtonScaleTo1 is not null)
		{
			this.ButtonScaleTo1.Visible = val;
			this.ButtonScaleTo1.Enabled = val;
			this.PlaceButtons();
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether the Minimize button is displayed in the caption bar of the form.
	/// </summary>
	public new bool MinimizeBox
	{
		get => base.MinimizeBox;

		set
		{
			if (this.systemMenu is not null)
				this.systemMenu.MinimizeEnabled = value;

			if (this.buttonMinimize is not null)
			{
				this.buttonMinimize.Visible = value;
				this.buttonMinimize.Enabled = value;
			}

			base.MinimizeBox = value;
		}
	}

	/// <summary>
	/// Processes a Windows Message
	/// </summary>
	/// <param name="m">Message which needs to be processed</param>
	protected override void WndProc(ref Message m)
	{
		if (this.WindowState == FormWindowState.Maximized)
		{
			if (
				(
					(m.Msg == (int)User32.WindowMessage.SysCommand)
					&& (m.WParam.ToInt32() == (int)User32.SystemMenuCommand.Move)
				)
				||
				(
					(m.Msg == (int)User32.NonClientMouseMessage.LeftButtonDown)
					&& (m.WParam.ToInt32() == (int)User32.NonClientHitTestResult.Caption)
				)
			)
			{
				m.Msg = (int)User32.WindowMessage.Null;
			}
		}

		base.WndProc(ref m);

		if (m.Msg == (int)User32.WindowMessage.GetMinMaxInfo && this.ConstrainToDesignRatio && this.FormDesignRatio != 0.0F)
			this.SetMaximizedBoundsOnItsMonitor(m.LParam);

		if (this.DrawCustomBorder)
		{
			switch (m.Msg)
			{
				case (int)User32.WindowMessage.GetSysMenu:
					this.systemMenu.Show(this, this.PointToClient(new Point(m.LParam.ToInt32())));
					break;
				case (int)User32.WindowMessage.NonClientHitTest:
					m.Result = this.OnNonClientHitTest(m.LParam);
					break;
				case (int)User32.NonClientMouseMessage.RightButtonUp:
					this.OnNonClientRButtonUp(m.LParam);
					break;
				default:
					break;
			}
		}
	}

	/// <summary>The MINMAXINFO structure of WM_GETMINMAXINFO.</summary>
	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
	private struct MinMaxInfo
	{
		public Point Reserved;
		public Point MaxSize;
		public Point MaxPosition;
		public Point MinTrackSize;
		public Point MaxTrackSize;
	}

	/// <summary>
	/// Makes a maximize fill the working area of the monitor the window is on, keeping <see cref="FormDesignRatio"/>.
	/// Asked at each maximize: a size computed once would be the size for the startup monitor, which Windows keeps on a
	/// monitor of another size.
	/// </summary>
	private void SetMaximizedBoundsOnItsMonitor(IntPtr minMaxInfo)
	{
		var screen = Screen.FromHandle(this.Handle);
		var bounds = MaximizedBoundsIn(screen.Bounds, screen.WorkingArea, this.FormDesignRatio);

		var info = System.Runtime.InteropServices.Marshal.PtrToStructure<MinMaxInfo>(minMaxInfo);
		info.MaxPosition = bounds.Location;
		info.MaxSize = new Point(bounds.Width, bounds.Height);
		// The maximized size is also bounded by the maximum tracking size.
		info.MaxTrackSize = new Point(Math.Max(info.MaxTrackSize.X, bounds.Width), Math.Max(info.MaxTrackSize.Y, bounds.Height));
		System.Runtime.InteropServices.Marshal.StructureToPtr(info, minMaxInfo, false);
	}

	/// <summary>
	/// The largest rectangle of height / width <paramref name="ratio"/> centered in <paramref name="workingArea"/>, relative
	/// to the top left of <paramref name="monitor"/> (MINMAXINFO places a maximized window on its monitor).
	/// </summary>
	internal static Rectangle MaximizedBoundsIn(Rectangle monitor, Rectangle workingArea, float ratio)
	{
		int width = workingArea.Width;
		int height = Convert.ToInt32(width * ratio);
		if (height > workingArea.Height)
		{
			height = workingArea.Height;
			width = Convert.ToInt32(height / ratio);
		}

		return new Rectangle(
			workingArea.Left - monitor.Left + (workingArea.Width - width) / 2,
			workingArea.Top - monitor.Top + (workingArea.Height - height) / 2,
			width,
			height);
	}

	/// <summary>
	/// Perfoms a hit test on the non client area
	/// </summary>
	/// <param name="parameter">Windows parameters</param>
	/// <returns>IntPte containing the area which was hit.</returns>
	private IntPtr OnNonClientHitTest(IntPtr parameter)
	{
		User32.NonClientHitTestResult result = User32.NonClientHitTestResult.Client;

		Point point = this.PointToClient(new Point(parameter.ToInt32()));

		if (this.topBorderRect.Contains(point))
			result = User32.NonClientHitTestResult.Caption;

		if (this.WindowState == FormWindowState.Normal && this.ResizeCustomAllowed)
		{
			if (this.upperLeftCorner.Contains(point))
				result = User32.NonClientHitTestResult.TopLeftCorner;
			else if (this.topResizeRect.Contains(point))
				result = User32.NonClientHitTestResult.TopBorder;
			else if (this.upperRightCorner.Contains(point))
				result = User32.NonClientHitTestResult.TopRightCorner;
			else if (this.leftBorderRect.Contains(point))
				result = User32.NonClientHitTestResult.LeftBorder;
			else if (this.rightBorderRect.Contains(point))
				result = User32.NonClientHitTestResult.RightBorder;
			else if (this.lowerLeftCorner.Contains(point))
				result = User32.NonClientHitTestResult.BottomLeftCorner;
			else if (this.bottomBorderRect.Contains(point))
				result = User32.NonClientHitTestResult.BottomBorder;
			else if (this.lowerRightCorner.Contains(point))
				result = User32.NonClientHitTestResult.BottomRightCorner;
		}


		return (IntPtr)result;
	}

	/// <summary>
	/// Sends a Non Client Window Message
	/// </summary>
	/// <param name="msg">Message which is sent</param>
	/// <param name="parameter1">Word Window paramters</param>
	/// <param name="parameter2">Long Window paramets</param>
	private void SendNCWinMessage(User32.WindowMessage msg, IntPtr parameter1, IntPtr parameter2)
	{
		Message message = Message.Create(this.Handle, (int)msg, parameter1, parameter2);
		this.WndProc(ref message);
	}

	/// <summary>
	/// Sets the location for the minimize, maximize and close buttons for the custom border.
	/// </summary>
	/// <summary>Minimize, maximize and normalize button size at 96 DPI, zoom 1.</summary>
	private const float CaptionButtonSize = 15.0F;

	/// <summary>Close button size at 96 DPI, zoom 1.</summary>
	private const float CloseButtonSize = 26.0F;

	/// <summary>Space between the caption buttons at 96 DPI, zoom 1.</summary>
	private const float CaptionButtonGap = 5.0F;

	protected void PlaceButtons()
	{
		// The buttons are created by InitializeComponent; the border can be set while it runs.
		if (this.buttonClose is null || this.buttonMaximize is null || this.ButtonScaleTo1 is null || this.buttonMinimize is null)
			return;

		// Minimized, the window has no client area: placed for it, the buttons would stay off the window after the
		// restore. The restore's client size change places them (OnClientSizeChanged).
		if (this.WindowState == FormWindowState.Minimized || this.ClientSize.Width <= 0)
			return;

		// Sizes from the design (15 and 26 pixels at 96 DPI) times the scale, the same in every form whatever its
		// autoscaling, and without the rounding drift of repeated zooms.
		float scale = this.UIService?.Scale ?? 1.0F;
		var small = new Size((int)Math.Round(CaptionButtonSize * scale), (int)Math.Round(CaptionButtonSize * scale));
		this.buttonClose.Size = new Size((int)Math.Round(CloseButtonSize * scale), (int)Math.Round(CloseButtonSize * scale));
		this.buttonMaximize.Size = this.ButtonScaleTo1.Size = this.buttonMinimize.Size = small;
		this.DrawCaptionButton(this.buttonClose, CaptionButtonArt.Glyph.Close);
		this.DrawCaptionButton(this.buttonMaximize, CaptionButtonArt.Glyph.Window);
		this.DrawCaptionButton(this.ButtonScaleTo1, CaptionButtonArt.Glyph.Window);
		this.DrawCaptionButton(this.buttonMinimize, CaptionButtonArt.Glyph.Minimize);

		// The border pictures start with one black line outside their gold line: the buttons sit on the gold line, the
		// close button in the top right corner.
		int edge = this.topBorder is null ? 0 : Math.Max(1, (int)Math.Round(this.topBorder.Height / (float)BorderTopDesignHeight));
		this.buttonClose.Text = Resources.GlobalClose;
		this.buttonClose.Location = new Point(
			this.ClientRectangle.Right - edge - this.buttonClose.Width
			, this.ClientRectangle.Top + edge);

		this.buttonMaximize.Text = Resources.GlobalMaximize;
		this.ButtonScaleTo1.Text = Resources.GlobalNormalize;
		this.buttonMinimize.Text = Resources.GlobalMinimize;

		// Right to left from the close button, only the buttons shown, so a hidden one leaves no hole. Shown is read from
		// the form's settings: Visible is false until the form shows, and Enabled while it is disabled (during loading).
		int gap = Math.Max(1, (int)Math.Round(CaptionButtonGap * scale));
		int right = this.buttonClose.Left;
		foreach (var (button, shown) in new[] {
			(this.buttonMaximize, this.MaximizeBox),
			(this.ButtonScaleTo1, this.NormalizeBox),
			(this.buttonMinimize, this.MinimizeBox) })
		{
			if (!this.DrawCustomBorder || !shown)
				continue;

			button.Location = new Point(right - gap - button.Width, this.ClientRectangle.Top + edge);
			right = button.Left;
		}

		// A zoom moves the controls under the buttons before the buttons: repaint them whole, or a corner keeps what
		// the moved controls left there.
		this.buttonClose.Invalidate();
		this.buttonMaximize.Invalidate();
		this.ButtonScaleTo1.Invalidate();
		this.buttonMinimize.Invalidate();
	}

	/// <summary>
	/// The caption buttons follow every change of the client area, whatever made it: a zoom, the user's resize, a
	/// maximize, or a restore from the taskbar, which none of the resize handlers sees as a change of size.
	/// </summary>
	protected override void OnClientSizeChanged(EventArgs e)
	{
		base.OnClientSizeChanged(e);
		this.PlaceButtons();
	}

	/// <summary>The height of <see cref="Images.BorderTop"/>, in pixels.</summary>
	private const int BorderTopDesignHeight = 17;

	/// <summary>The size each caption button's pictures were drawn at; a button not drawn yet is missing.</summary>
	private readonly Dictionary<ScalingButton, int> captionButtonArtSizes = [];

	/// <summary>
	/// Draws a caption button's pictures at the size it is shown (<see cref="CaptionButtonArt"/>), so it stays sharp at
	/// any zoom and screen DPI; again only when that size changes.
	/// </summary>
	private void DrawCaptionButton(ScalingButton button, CaptionButtonArt.Glyph glyph)
	{
		int size = button.Width;
		bool drawn = this.captionButtonArtSizes.TryGetValue(button, out int drawnSize);
		if (size <= 0 || (drawn && size == drawnSize))
			return;

		var old = drawn ? new[] { button.UpBitmap, button.OverBitmap, button.DownBitmap } : null;
		button.UpBitmap = CaptionButtonArt.Render(glyph, size, CaptionButtonArt.State.Up);
		button.OverBitmap = CaptionButtonArt.Render(glyph, size, CaptionButtonArt.State.Over);
		button.DownBitmap = CaptionButtonArt.Render(glyph, size, CaptionButtonArt.State.Down);
		var shown = button.Image;
		button.Image = shown == old?[1] ? button.OverBitmap
			: shown == old?[2] ? button.DownBitmap
			: button.UpBitmap;
		this.captionButtonArtSizes[button] = size;

		if (old is not null)
			foreach (var bitmap in old)
				bitmap?.Dispose();
	}

	/// <summary>How far the resize grips reach past the side and bottom border pictures.</summary>
	private const int GripMargin = 2;

	/// <summary>
	/// The part of the client area the content may use: inside the border pictures and clear of the resize grips (the
	/// whole client area without the custom border).
	/// </summary>
	protected Rectangle ContentArea => this.ContentAreaOf(this.ClientSize);

	/// <summary>
	/// The <see cref="ContentArea"/> the window would have with the client size <paramref name="client"/>.
	/// </summary>
	protected Rectangle ContentAreaOf(Size client) => this.DrawCustomBorder
		? WindowLayout.ContentArea(client, this.sideBorder.Width, this.topBorder.Height, this.bottomBorder.Height, GripMargin)
		: new Rectangle(Point.Empty, client);

	/// <summary>
	/// Creates the bounding Rectangles for the border graphics
	/// </summary>
	private void CreateBorderRects()
	{
		// Make it a couple of pixels wider than the side graphic.
		int cornerWidth = 6;
		if (this.sideBorder is not null)
			cornerWidth = this.sideBorder.Width + GripMargin;

		int cornerHeight = 6;
		if (this.bottomBorder is not null)
			cornerHeight = this.bottomBorder.Height + GripMargin;

		Size cornerSize = new Size(cornerWidth, cornerHeight);
		this.upperLeftCorner = new Rectangle(new Point(ClientRectangle.Left, ClientRectangle.Top), cornerSize);
		this.upperRightCorner = new Rectangle(new Point(ClientRectangle.Right - cornerSize.Width, ClientRectangle.Top), cornerSize);
		this.lowerLeftCorner = new Rectangle(new Point(ClientRectangle.Left, ClientRectangle.Bottom - cornerSize.Height), cornerSize);
		this.lowerRightCorner = new Rectangle(new Point(ClientRectangle.Right - cornerSize.Width, ClientRectangle.Bottom - cornerSize.Height), cornerSize);
		this.bottomBorderRect = new Rectangle(
			ClientRectangle.Left + cornerSize.Width,
			ClientRectangle.Bottom - cornerSize.Height,
			ClientRectangle.Width - (2 * cornerSize.Width),
			cornerSize.Height);
		this.rightBorderRect = new Rectangle(
			ClientRectangle.Right - cornerSize.Width,
			ClientRectangle.Top + cornerSize.Height,
			cornerSize.Width,
			ClientRectangle.Height - (2 * cornerSize.Height));
		this.leftBorderRect = new Rectangle(
			ClientRectangle.Left,
			ClientRectangle.Top + cornerSize.Height,
			cornerSize.Width,
			ClientRectangle.Height - (2 * cornerSize.Height));

		int titleBarHeight = this.topBorder.Height + 2;

		this.topBorderRect = new Rectangle(ClientRectangle.Left + cornerSize.Width, ClientRectangle.Top, ClientRectangle.Width - (2 * cornerSize.Width), titleBarHeight);

		// This should be inside of the topBorderRect
		this.topResizeRect = new Rectangle(this.topBorderRect.X, this.topBorderRect.Y, this.topBorderRect.Width, cornerSize.Height);
	}

	/// <summary>
	/// Handler for clicking the Minimize button
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void MaximizeButtonClick(object? sender, EventArgs e)
	{
		if (!this.MaximizeBox)
			return;

		if (this.WindowState == FormWindowState.Maximized)
			this.WindowState = FormWindowState.Normal;
		else
			this.WindowState = FormWindowState.Maximized;
	}

	/// <summary>
	/// Handler for clicking the Maximize button
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void MinimizeButtonClick(object? sender, EventArgs e)
	{
		if (!this.MinimizeBox)
			return;

		if (this.WindowState != FormWindowState.Minimized)
			this.WindowState = FormWindowState.Minimized;
	}
}
