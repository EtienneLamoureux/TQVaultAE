﻿using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.GUI.Components.Buttons;
using TQVaultAE.GUI.Components;
using TQVaultAE.GUI.Forms;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.GUI.Tooltips;

public partial class BagButtonLabelTooltip : BaseTooltip
{
	private static object syncObj = new object();

	private static BagButtonLabelTooltip? _Current = null;
	private string _Label = string.Empty;
	private Control? _AnchorControl;
	private readonly Bitmap? bgimg;

	internal Control AnchorControl
	{
		set
		{
			_AnchorControl = value;

			// Move it on top of anchor
			var loc = value.PointToScreen(Point.Empty);

			loc.Y -= this.Size.Height + 5;

			var halfbutton = value.Size.Width / 2;

			loc.X += halfbutton;
			loc.X -= this.Size.Width / 2;

			this.Location = loc;
		}
	}

	/// <summary>Size of the frame at zoom 1 and 96 dpi, when the text fits in it.</summary>
	private static readonly Size DesignSize = new(187, 40);

	/// <summary>Room around the text when it is wider or taller than the frame, at zoom 1 and 96 dpi.</summary>
	private static readonly Size DesignTextMargin = new(25, 20);

	/// <summary>Text size in points at zoom 1.</summary>
	private const float DesignFontSize = 15F;

	internal string Label
	{
		set
		{
			_Label = value;

			// The tool tip is kept between shows: its font and size are set again from the current zoom and DPI, never
			// from the previous show's, else every change of scale since then would add up.
			// Rounded: the zoom carries float noise after maximize/restore cycles (0.9999998), which would make a new
			// font of almost the same size.
			var font = this.FontService.GetFont(MathF.Round(DesignFontSize * this.UIService.FontZoom, 1));
			this.scalingLabel.Font = font;
			this.scalingLabel.Text = _Label;

			var frame = Scaled(DesignSize, this.UIService.Scale);
			var margin = Scaled(DesignTextMargin, this.UIService.Scale);
			var text = TextRenderer.MeasureText(this._Label, font, Size.Empty, TextFormatFlags.SingleLine);
			this.ClientSize = new Size(
				Math.Max(frame.Width, text.Width + margin.Width),
				Math.Max(frame.Height, text.Height + margin.Height));
			this.scalingLabel.Bounds = this.ClientRectangle;

			this.scalingLabel.Visible = true;
		}
	}


#if DEBUG
	// For Design Mode
#pragma warning disable CS8618 // Designer only: the runtime constructor sets the services and state.
	public BagButtonLabelTooltip() => InitializeComponent();
#pragma warning restore CS8618
#endif

	private BagButtonLabelTooltip(
		MainForm instance
		, IItemProvider itemProvider
		, IFontService fontService
		, IUIService uiService
		, ITranslationService translationService
	) : base(itemProvider, fontService, uiService, translationService)
	{
		this.Owner = instance;

		InitializeComponent();

		SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

		// Font and size are set at each show (Label).
		this.bgimg = this.UIService.LoadBitmap(@"INGAMEUI\HEALTHMANAOVERLAY01_NEW.TEX");
		this.BackgroundImage = bgimg;
	}

	// to avoid Mainform lost focus with this.TopMost = false
	protected override bool ShowWithoutActivation => true;

	public static void HideTooltip()
	{
		lock (syncObj)
		{
			if (_Current is not null)
			{
				_Current.Hide();
				_Current.scalingLabel.Visible = false;
			}
		}
	}

	#region Factory

	public static BagButtonLabelTooltip? ShowTooltip(GuiServices services, BagButtonBase anchorControl)
	{
		var label = anchorControl.Sack?.BagButtonIconInfo?.Label;
		if (anchorControl is null) return null;
		if (string.IsNullOrWhiteSpace(label)) return null;

		lock (syncObj)
		{
			HideTooltip();
			if (_Current is null)
				_Current = new BagButtonLabelTooltip(
					services.Forms.MainForm
					, services.ItemProvider
					, services.FontService
					, services.UIService
					, services.TranslationService
				);

			_Current.Label = label;
			_Current.AnchorControl = anchorControl;
			_Current.Show();
		}
		return _Current;
	}

	#endregion


}