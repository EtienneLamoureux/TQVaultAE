//-----------------------------------------------------------------------
// <copyright file="VaultForm.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.GUI.Components.Menus;
using TQVaultAE.GUI.Components.Rendering;
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
	protected readonly IFontService FontService;
	protected readonly IUIService UIService;
	protected readonly IItemTextFormatter ItemText;
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
		this.ItemText = this.Services.ItemText;
		this.GamePathResolver = this.Services.GamePathService;
		this.GameFileService = this.Services.GameFileService;
		this.SoundService = this.Services.SoundService;
		this.titleFont = this.TitleFontAt(1.0F);
		this.Log = this.Services.Logger<VaultForm>();
		this.TranslationService = this.Services.TranslationService;
		this.PathIO = this.Services.PathIO;
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

		// The tool tips rendered in the old font are made again (each window asks it: the caches clear once per change seen).
		TooltipImageCache.InvalidateAll();
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

		this.topBorder = UIPictures.BorderTop;
		this.bottomBorder = UIPictures.BorderBottom;
		this.sideBorder = UIPictures.BorderSide;
		this.bottomRightCorner = UIPictures.BorderBottomRightCorner;
		this.bottomLeftCorner = UIPictures.BorderBottomLeftCorner;

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
	/// Gets or sets a value indicating whether the form resizing will be constrained to the original form's width and height ratio.
	/// </summary>
	public bool ConstrainToDesignRatio { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the form will be scaled when it is resized.
	/// </summary>
	public bool ScaleOnResize { get; set; }

	/// <summary>
	/// Gets or sets the the ratio of height / width before any scaling.
	/// </summary>
	public float FormDesignRatio { get; set; }

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
	internal void RaiseGlobalMouseButtonLeft(Point pt) => GlobalMouseButtonLeft?.Invoke(this, pt);

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
	/// A window with a marble or a stretched picture paints its background from a brush or a picture made once
	/// (<see cref="MarbleBackground.TryPaint"/>).
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
		// The user's zoom of a dialog grows its font: the controls without a font of their own follow it.
		if (this.applyingUserZoom)
			ZoomScope.Grow(this, factor.Height);

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
	/// disposes it, and the service shares its fonts. No taller than the top border picture it is drawn on, which keeps its
	/// own height at every DPI and zoom: at 144 dpi, or a zoom of 1.2, 9.5 pt grew past it and the caption's top was cut.
	/// </summary>
	private Font TitleFontAt(float zoom)
	{
		// Fonts in points are drawn for the system DPI: an em of size * DPI / 72 pixels.
		float fitting = CustomBorderThickness.Top * 72F / (this.UIService.DESIGNDPI * this.UIService.SystemDpiScale);
		return (Font)this.FontService.GetFontLight(Math.Min(9.5F * zoom, fitting)).Clone();
	}

	internal const int NORMAL_FORMWIDTH = 1350;
	internal const int NORMAL_FORMHEIGHT = 910;

	/// <summary>
	/// Handler for the ResizeEnd event.  Used to scale the internal controls after the window has been resized.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	protected virtual void ResizeEndCallback(object? sender, EventArgs e) => DoResizeEndCallback(sender, e);

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