//-----------------------------------------------------------------------
// <copyright file="VaultForm.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Interop;
using TQVaultAE.Presentation.Win32.Contracts;
using TQVaultAE.Presentation.Win32;

namespace TQVaultAE.GUI.Forms;

using Microsoft.Extensions.Logging;
using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using TQVaultAE.Config;
using TQVaultAE.GUI.Components;
using TQVaultAE.GUI.Infrastructure;
using TQVaultAE.GUI.Models;

/// <summary>
/// Abstract class used for constructing TQVault themed forms.
/// </summary>
public partial class VaultForm : Form, IScalingControl
{
	private readonly ILogger Log;
	/// <summary>
	/// Holds the last state of the form.  Used to support maximizing.
	/// </summary>
	private FormWindowState lastState;

	/// <summary>
	/// Bitmap graphic for the top border
	/// </summary>
	private Bitmap topBorder;

	/// <summary>
	/// Bitmap graphic for the bottom border
	/// </summary>
	private Bitmap bottomBorder;

	/// <summary>
	/// Bitmap graphic for the side borders
	/// </summary>
	private Bitmap sideBorder;

	/// <summary>
	/// Bitmap graphic for the bottom right corner
	/// </summary>
	private Bitmap bottomRightCorner;

	/// <summary>
	/// Bitmap graphic for the bottom left
	/// </summary>
	private Bitmap bottomLeftCorner;

	/// <summary>
	/// Flag to indicate whether custom border will be drawn
	/// </summary>
	private bool drawCustomBorder;

	/// <summary>
	/// Bounding Rectangle for the upper left corner of the form.
	/// </summary>
	private Rectangle upperLeftCorner;

	/// <summary>
	/// Bounding Rectangle for the upper right corner of the form
	/// </summary>
	private Rectangle upperRightCorner;

	/// <summary>
	/// Bounding Rectangle for the lower left corner of the form.
	/// </summary>
	private Rectangle lowerLeftCorner;

	/// <summary>
	/// Bounding Rectangle for the lower right corner of the form.
	/// </summary>
	private Rectangle lowerRightCorner;

	/// <summary>
	/// Bounding Rectangle for the top border of the form.
	/// </summary>
	private Rectangle topBorderRect;

	/// <summary>
	/// Bounding Rectangle for the bottom border of the form.
	/// </summary>
	private Rectangle bottomBorderRect;

	/// <summary>
	/// Bounding Rectangle for the right border of the form.
	/// </summary>
	private Rectangle rightBorderRect;

	/// <summary>
	/// Bounding Rectangle for the left border of the form.
	/// </summary>
	private Rectangle leftBorderRect;

	/// <summary>
	/// Bounding Rectangle for the top of the form used for resizing.
	/// </summary>
	private Rectangle topResizeRect;

	/// <summary>
	/// Font used to draw the title.
	/// </summary>
	private Font titleFont;

	/// <summary>
	/// WindowMenu used to display the system menu.
	/// </summary>
	private WindowMenu systemMenu;

	/// <summary>
	/// Indicates whether resizing is allowed when custom borders are enabled.
	/// </summary>
	private bool resizeCustomAllowed;

	protected readonly UserSettings USettings;
	protected readonly ITranslationService TranslationService;
	protected readonly IPathIO PathIO;
	protected readonly IFileIO FileIO;
	protected readonly IDirectoryIO DirectoryIO;
	protected readonly IFontService FontService;
	protected readonly IUIService UIService;
	protected readonly IItemProvider ItemProvider;
	protected readonly IGamePathService GamePathResolver;
	protected readonly IGameFileService GameFileService;
	protected readonly ISoundService SoundService;
	internal readonly GuiServices Services;

#if DEBUG
	// For Design Mode
#pragma warning disable CS8618 // Designer only: the runtime constructor sets the services and state.
	public VaultForm() => InitForm();
#pragma warning restore CS8618
#endif
	/// <summary>
	/// Initializes a new instance of the VaultForm class.
	/// </summary>
	public VaultForm(GuiServices services)
	{
		// Runtime only: the designer uses the parameterless constructor.
		this.Services = services;
		this.FontService = this.Services.FontService;
		this.UIService = this.Services.UIService;
		this.ItemProvider = this.Services.ItemProvider;
		this.GamePathResolver = this.Services.GamePathService;
		this.GameFileService = this.Services.GameFileService;
		this.SoundService = this.Services.SoundService;
		this.titleFont = this.TitleFontAt(1.0F);
		this.Log = this.Services.Logger<VaultForm>();
		this.TranslationService = this.Services.TranslationService;
		this.PathIO = this.Services.PathIO;
		this.FileIO = this.Services.FileIO;
		this.DirectoryIO = this.Services.DirectoryIO;
		this.USettings = this.Services.Settings;

		InitForm();

		this.FontService.FontChanged += this.FontService_FontChanged;
	}

	/// <summary>
	/// The base font changed in the settings: the window and its controls take it, keeping their sizes, so no restart
	/// is needed.
	/// </summary>
	private void FontService_FontChanged(object? sender, EventArgs e)
	{
		if (this.IsDisposed || this.Disposing)
			return;

		if (this.InvokeRequired)
		{
			this.BeginInvoke(() => this.FontService_FontChanged(sender, e));
			return;
		}

		this.OnBaseFontChanged();
	}

	/// <summary>
	/// Moves the window's fonts to the base font in use (<see cref="IFontService.Rebase"/>), then lays it out and paints it
	/// again. A window keeping fonts outside its controls overrides it and calls the base.
	/// </summary>
	protected virtual void OnBaseFontChanged()
	{
		if (this.titleFont is not null)
		{
			// Its own copy, like the one the constructor makes: the service's fonts are shared.
			var rebased = this.FontService.Rebase(this.titleFont);
			if (!ReferenceEquals(rebased, this.titleFont))
			{
				var previous = this.titleFont;
				this.titleFont = (Font)rebased.Clone();
				previous.Dispose();
			}
		}

		FontRebaser.Apply(this, this.FontService);
		this.Invalidate(true);
	}

	[MemberNotNull(nameof(bottomBorder), nameof(bottomLeftCorner), nameof(bottomRightCorner), nameof(sideBorder), nameof(systemMenu), nameof(topBorder))]
	private void InitForm()
	{
		this.InitializeComponent();

		this.topBorder = Images.BorderTop;
		this.bottomBorder = Images.BorderBottom;
		this.sideBorder = Images.BorderSide;
		this.bottomRightCorner = Images.BorderBottomRightCorner;
		this.bottomLeftCorner = Images.BorderBottomLeftCorner;
		this.ShowResizeBorders = false;

		this.TitleTextColor = SystemColors.ControlText;

		#region Apply custom font & scaling

		// PlaceButtons sizes and positions the caption buttons from the scale.
		this.PlaceButtons();

		// Caption buttons are pictures: their text only names them (it used to be hidden by shrinking the font to fit).
		this.buttonMaximize.ImageOnly = this.ButtonScaleTo1.ImageOnly = this.buttonMinimize.ImageOnly = this.buttonClose.ImageOnly = true;

		#endregion

		this.CreateBorderRects();

		// to avoid flickering use double buffer and to force control to use OnPaint
		this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);

		this.ResizeBegin += new System.EventHandler(this.ResizeBeginCallback);
		this.ResizeEnd += new System.EventHandler(this.ResizeEndCallback);
		this.Paint += new System.Windows.Forms.PaintEventHandler(this.PaintCallback);
		this.Resize += new System.EventHandler(this.ResizeCallback);

		this.systemMenu = new WindowMenu(this);
		this.systemMenu.SystemEvent += new EventHandler<WindowMenuEventArgs>(this.SystemMenuSystemEvent);
		this.systemMenu.MaximizeEnabled = this.MaximizeBox;
		this.systemMenu.NormalizeEnabled = this.NormalizeBox;
		this.systemMenu.MinimizeEnabled = this.MinimizeBox;
	}

	/// <summary>
	/// Applies the user's zoom to a designer control, which Windows Forms already scaled to the DPI; a control built in
	/// code, at design size, also needs the DPI (<paramref name="builtInCode"/>).
	/// </summary>
	public static void ScaleControl(IUIService uiService, System.Windows.Forms.Control ctrl, bool isAbsolutePositioning = true, bool builtInCode = false)
	{
		float factor = uiService is null ? 1.0F : builtInCode ? uiService.Scale : uiService.Zoom;

		if (isAbsolutePositioning)
			ctrl.Location = new Point((int)System.Math.Round(ctrl.Location.X * factor), (int)System.Math.Round(ctrl.Location.Y * factor));

		ctrl.Size = new Size((int)System.Math.Round(ctrl.Size.Width * factor), (int)System.Math.Round(ctrl.Size.Height * factor));
	}

	/// <summary>
	/// Gets or sets a value indicating whether or not the resize border boxes are shown.
	/// </summary>
	public bool ShowResizeBorders { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the form resizing will be constrained to the original form's width and height ratio.
	/// </summary>
	public bool ConstrainToDesignRatio { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the form will be scaled when it is resized.
	/// </summary>
	public bool ScaleOnResize { get; set; }

	/// <summary>
	/// Gets or sets the original scale for resetting the form scaling.
	/// </summary>
	public float OriginalFormScale { get; set; }

	/// <summary>
	/// Gets or sets the the ratio of height / width before any scaling.
	/// </summary>
	public float FormDesignRatio { get; set; }

	/// <summary>
	/// Gets or sets the size of the form after the initial scaling.
	/// </summary>
	public Size OriginalFormSize { get; set; }

	/// <summary>
	/// Gets or sets the size of the form prior to the current scaling operation.
	/// </summary>
	public Size LastFormSize { get; set; }

	/// <summary>
	/// Gets or sets the upper bounds for the form size.
	/// </summary>
	public Size FormMaximumSize { get; set; }

	/// <summary>
	/// Gets or sets the lower bounds for the form size
	/// </summary>
	public Size FormMinimumSize { get; set; }

	/// <summary>
	/// Gets or sets the color for the text in the title bar
	/// </summary>
	public Color TitleTextColor { get; set; }

	bool _NormalizeBox = true;

	#region Capture Mouse Wheel Event Globally

	protected internal event EventHandler? GlobalMouseWheelUp;
	protected internal event EventHandler? GlobalMouseWheelDown;
	internal void RaiseGlobalMouseWheelDown() => GlobalMouseWheelDown?.Invoke(this, EventArgs.Empty);
	internal void RaiseGlobalMouseWheelUp() => GlobalMouseWheelUp?.Invoke(this, EventArgs.Empty);

	#endregion

	#region Capture Mouse Button Event Globally

	protected internal event EventHandler<Point>? GlobalMouseButtonLeft;
	protected internal event EventHandler<Point>? GlobalMouseButtonRight;
	internal void RaiseGlobalMouseButtonLeft(Point pt) => GlobalMouseButtonLeft?.Invoke(this, pt);
	internal void RaiseGlobalMouseButtonRight(Point pt) => GlobalMouseButtonRight?.Invoke(this, pt);

	#endregion

	/// <summary>
	/// Processes a system event on the system menu.
	/// </summary>
	/// <param name="sender">Sender object</param>
	/// <param name="e">Window menu event args</param>
	protected void SystemMenuSystemEvent(object? sender, WindowMenuEventArgs e)
		=> this.SendNCWinMessage(User32.WindowMessage.SysCommand, (IntPtr)e.SystemCommand, IntPtr.Zero);

	/// <summary>
	/// A window opened from a topmost one (Settings, Advanced Search, the item dialogs) is topmost too: otherwise it
	/// opens behind its owner and a modal one leaves the application blocked with nothing visible. A window sized around
	/// its content (<see cref="FitAround"/>) fits it once more, laid out at its final DPI.
	/// </summary>
	protected override void OnLoad(EventArgs e)
	{
		if (this.Owner is { TopMost: true })
			this.TopMost = true;

		base.OnLoad(e);
		this.FitToContent();
	}

	/// <summary>
	/// A marble window paints its background from a brush made once (<see cref="MarbleBackground.TryPaint"/>).
	/// </summary>
	protected override void OnPaintBackground(PaintEventArgs e)
	{
		if (!MarbleBackground.TryPaint(this, e))
			base.OnPaintBackground(e);
	}

	/// <summary>
	/// Scales the form.
	/// </summary>
	/// <param name="factor">factor which the form will be scaled.</param>
	/// <param name="specified">BoundsSpecified enum.</param>
	protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
	{
		if (this.DrawCustomBorder)
		{
			// From the zoom, not from the font of the zoom before (12 -> 11.98 pt after a few zooms).
			if (this.titleFont is not null && ZoomScope.IsZooming)
			{
				var previous = this.titleFont;
				this.titleFont = this.TitleFontAt(UIService.Zoom);
				previous.Dispose();
			}
		}

		base.ScaleControl(factor, specified);

		if (this.DrawCustomBorder)
			this.CreateBorderRects();
	}

	/// <summary>
	/// The caption's font at <paramref name="zoom"/> (points follow the DPI by themselves), the form's own copy: the form
	/// disposes it, and the service shares its fonts.
	/// </summary>
	private Font TitleFontAt(float zoom) => (Font)this.FontService.GetFontLight(9.5F, zoom).Clone();

	internal const int NORMAL_FORMWIDTH = 1350;
	internal const int NORMAL_FORMHEIGHT = 910;

	/// <summary>
	/// Handler for the ResizeEnd event.  Used to scale the internal controls after the window has been resized.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	protected virtual void ResizeEndCallback(object? sender, EventArgs e) => DoResizeEndCallback(sender, e);

	/// <summary>
	/// The top border picture at its design size, read once: the paint handler measures the title's background on it, and
	/// a transparent control has its window painted behind it, so it was read from the resources several times per control.
	/// </summary>
	private static Bitmap DesignTopBorder => designTopBorder ??= Images.BorderTop;

	private static Bitmap? designTopBorder;

	/// <summary>
	/// Handler for the form Paint callback
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">PaintEventArgs data</param>
	protected virtual void PaintCallback(object? sender, PaintEventArgs e)
	{
		if (this.IsDisposed || this.Disposing)
			return;

		if (this.DrawCustomBorder)
		{
			{
				// Draw the Top (the border bitmaps are set by InitForm)
				Rectangle topBorderRect = new Rectangle(ClientRectangle.Left, ClientRectangle.Top, this.ClientRectangle.Width, this.topBorder.Height);
				e.Graphics.DrawImage(this.topBorder, topBorderRect);

				// Show the title text if we are drawing the top border.
				if (!string.IsNullOrEmpty(this.Text) && this.titleFont is not null)
				{
					Size textSize = TextRenderer.MeasureText(this.Text, this.titleFont);
					Rectangle textRect = new Rectangle(new Point((this.ClientSize.Width - textSize.Width) / 2, (this.topBorder.Height - textSize.Height) / 2), textSize);

					// Check if the background for the title text needs to be resized.
					float factor = (float)topBorderRect.Width / (float)DesignTopBorder.Width;
					int textBackroundWidth = Convert.ToInt32(114.0F * factor);
					if (textRect.Width > textBackroundWidth)
					{
						// Copy the center 126 pixels from the top border which make up the background to the title text.
						Rectangle srcRect = new Rectangle((DesignTopBorder.Width / 2) - 61, 0, 126, DesignTopBorder.Height);
						Rectangle destRect = new Rectangle(
							textRect.Left - Convert.ToInt32(6.0F * factor)
							, 0
							, textRect.Width + Convert.ToInt32(12.0F * factor)
							, topBorderRect.Height
						);
						e.Graphics.DrawImage(DesignTopBorder, destRect, srcRect, GraphicsUnit.Pixel);
					}

					TextRenderer.DrawText(
						e.Graphics
						, this.Text
						, this.titleFont
						, textRect
						, this.TitleTextColor
					);
				}
			}

			int topBorderOffset = Convert.ToInt32(3.0F * ((float)this.topBorder.Height) / (float)DesignTopBorder.Height);

			// Draw the Left side
			e.Graphics.DrawImage(
				this.sideBorder,
				new Rectangle(ClientRectangle.Left, ClientRectangle.Top + topBorderOffset, this.sideBorder.Width, this.ClientRectangle.Height - topBorderOffset));

			// Draw the Right side
			e.Graphics.DrawImage(
				this.sideBorder,
				new Rectangle(ClientRectangle.Right - this.sideBorder.Width, ClientRectangle.Top + topBorderOffset, this.sideBorder.Width, this.ClientRectangle.Height - topBorderOffset));

			// Draw the Bottom
			e.Graphics.DrawImage(
				this.bottomBorder,
				new Rectangle(ClientRectangle.Left, ClientRectangle.Bottom - this.bottomBorder.Height, this.ClientRectangle.Width, this.bottomBorder.Height));

			// Draw the Bottom Right Corner Graphics
			e.Graphics.DrawImage(
				this.bottomRightCorner,
				new Rectangle(ClientRectangle.Right - this.bottomRightCorner.Width, ClientRectangle.Bottom - this.bottomRightCorner.Height, this.bottomRightCorner.Width, this.bottomRightCorner.Height));

			// Draw the Bottom Left Corner Graphics
			e.Graphics.DrawImage(
				this.bottomLeftCorner,
				new Rectangle(ClientRectangle.Left, ClientRectangle.Bottom - this.bottomLeftCorner.Height, this.bottomLeftCorner.Width, this.bottomLeftCorner.Height));

			// Show the resize bounding boxes for debugging.
			if (this.ShowResizeBorders)
			{
				// Show Corners
				e.Graphics.FillRectangle(Brushes.Red, this.upperLeftCorner);
				e.Graphics.FillRectangle(Brushes.Red, this.upperRightCorner);
				e.Graphics.FillRectangle(Brushes.Red, this.lowerLeftCorner);
				e.Graphics.FillRectangle(Brushes.Red, this.lowerRightCorner);

				// Show Sides
				e.Graphics.FillRectangle(Brushes.Yellow, this.topBorderRect);
				e.Graphics.FillRectangle(Brushes.Green, this.topResizeRect);
				e.Graphics.FillRectangle(Brushes.Green, this.bottomBorderRect);
				e.Graphics.FillRectangle(Brushes.Green, this.leftBorderRect);
				e.Graphics.FillRectangle(Brushes.Green, this.rightBorderRect);
			}
		}
	}

	/// <summary>
	/// Processes a Client Right Button Up Event.
	/// </summary>
	/// <param name="parameter">Windows parameters</param>
	private void OnNonClientRButtonUp(IntPtr parameter)
	{
		if (this.topBorderRect.Contains(this.PointToClient(new Point(parameter.ToInt32()))))
			this.SendNCWinMessage(User32.WindowMessage.GetSysMenu, IntPtr.Zero, parameter);
	}

	/// <summary>
	/// Handler for clicking the Close button.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void CloseButtonClick(object? sender, EventArgs e) => this.Close();

	private void ButtonScaleTo1_Click(object? sender, EventArgs e)
	{
		if (!this.NormalizeBox)
			return;

		this.WindowState = FormWindowState.Normal;
		// As the keyboard reset does: the size kept for the next resize or maximize follows the new scale, else the next
		// maximize would read the size before this reset and zoom too far.
		this.ResizeFormCallback(this, new ResizeEventArgs(1.0F));
	}
}