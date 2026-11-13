﻿using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Presentation;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.GUI.Tooltips;

/// <summary>
/// A tool tip window. Its sizes and fonts come from the current scale (<see cref="IUIService.Scale"/>,
/// <see cref="IUIService.FontZoom"/>) each time it is shown, from design values: never from the size or font of a
/// previous show, since the zoom and the DPI may have changed since then and the changes would add up.
/// </summary>
public class BaseTooltip : Form
{
	protected IItemProvider ItemProvider;
	protected IFontService FontService;
	protected IUIService UIService;
	protected ITranslationService TranslationService;
	internal const string TOOLTIPDELIM = @"TOOLTIPDELIM";
	internal const string TOOLTIPSPACER = @"TOOLTIPSPACER";

#if DEBUG
	// For Design Mode
#pragma warning disable CS8618 // Designer only: the runtime constructor sets the services and state.
	public BaseTooltip() { }
#pragma warning restore CS8618
#endif

	public BaseTooltip(IItemProvider itemProvider, IFontService fontService, IUIService uiService, ITranslationService translationService)
	{
		this.ItemProvider = itemProvider;
		this.FontService = fontService;
		this.UIService = uiService;
		this.TranslationService = translationService;
	}

	/// <summary>A size given at zoom 1 and 96 dpi, in pixels at <paramref name="scale"/>.</summary>
	internal static Size Scaled(Size design, float scale)
		=> new((int)Math.Round(design.Width * scale), (int)Math.Round(design.Height * scale));

	/// <summary>Copy of a cached image shown by this tooltip; owned by the tooltip.</summary>
	private Bitmap? _shownImage;

	/// <summary>The controls a cached image took the place of.</summary>
	private readonly List<Control> _replaced = [];

	/// <summary>
	/// Replaces the tooltip content with <paramref name="image"/>, a copy taken from a <c>TooltipImageCache</c>.
	/// The tooltip owns the copy and disposes it with itself.
	/// </summary>
	protected void ShowCachedImage(Bitmap image)
	{
		// The controls the image replaces leave the form, so the form no longer disposes them: kept to be disposed
		// with it (they may still be read until then).
		this._replaced.AddRange(this.Controls.Cast<Control>());
		this.Controls.Clear();
		this.Padding = new Padding(0);
		this._shownImage?.Dispose();
		this._shownImage = image;
		this.Controls.Add(new PictureBox()
		{
			Image = image,
			SizeMode = PictureBoxSizeMode.AutoSize,
			Dock = DockStyle.Fill,
		});
	}

	/// <summary>
	/// A copy of <paramref name="image"/> inside the tooltip border: the game's golden rollover border, as the
	/// character editor's tool tips have, or a 1 pixel gray line when the game files lack it.
	/// </summary>
	protected Bitmap Framed(Bitmap image, Color background)
	{
		var gameFrame = TQVaultAE.GUI.Components.Character.GameTooltipFrame.Load(this.UIService);
		var frame = gameFrame?.Thickness(this.UIService.Scale) ?? new Padding(1);
		var bounds = new Rectangle(0, 0, image.Width + frame.Horizontal, image.Height + frame.Vertical);
		var framed = new Bitmap(bounds.Width, bounds.Height);
		using var g = Graphics.FromImage(framed);
		using (var back = new SolidBrush(gameFrame is null ? TQColor.DarkGray.Color() : background))
			g.FillRectangle(back, bounds);
		g.DrawImageUnscaled(image, frame.Left, frame.Top);
		gameFrame?.Draw(g, bounds, this.UIService.Scale);
		return framed;
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		if (disposing)
		{
			this._shownImage?.Dispose();
			this._shownImage = null;
			foreach (var control in this._replaced)
				control.Dispose();
			this._replaced.Clear();
		}
	}

	internal static Control? MakeRow(IUIService uiService, IFontService fontService, string? friendlyName, Color? FGColor = null, float fontSize = 10F, FontStyle style = FontStyle.Regular, Color? BGColor = null)
	{
		friendlyName ??= string.Empty;
		Control? row = null;
		if (friendlyName == TOOLTIPSPACER)
		{
			row = new Label()
			{
				Text = " ",
				Font = fontService.GetFontLight(fontSize, style, uiService.FontZoom),
				AutoSize = true,
			};
		}
		else if (friendlyName == TOOLTIPDELIM)
		{
			row = new Label()
			{
				Text = string.Empty,
				BackColor = TQColor.DarkGray.Color(),
				BorderStyle = BorderStyle.FixedSingle,
				Height = 3,
				Anchor = AnchorStyles.Left | AnchorStyles.Right,
				Margin = new Padding(2, 3, 2, 3),
			};
		}
		else
		{
			// Replace all TQNewLine to regular '\n'
			friendlyName = friendlyName.Replace(StringHelper.TQNewLineTag, "\n");
			// If there is a color tag in the middle
			if (friendlyName.LastIndexOf('{') > 0)
			{
				var multiColors = friendlyName.SplitColorSegments();
				if (multiColors.Length > 1)
				{
					row = new FlowLayoutPanel()
					{
						AutoSize = true,
						AutoSizeMode = AutoSizeMode.GrowOnly,
						FlowDirection = FlowDirection.LeftToRight,
						Padding = new Padding(0),
						Anchor = AnchorStyles.Left,

						BorderStyle = BorderStyle.None,
						Margin = new Padding(0),
					};

					if (BGColor.HasValue) row.BackColor = BGColor.Value;

					row.SuspendLayout();
					foreach (var coloredSegment in multiColors)
						row.Controls.Add(MakeSingleColorLabel(uiService, fontService, coloredSegment, FGColor, fontSize, style, BGColor));
					row.ResumeLayout();
				}
			}
			else
				row = MakeSingleColorLabel(uiService, fontService, friendlyName, FGColor, fontSize, style, BGColor);
		}

		return row;
	}

	/// <summary>
	/// The row of a tooltip line built in Presentation: text, spacer or delimiter. A range line is drawn as its text followed
	/// by its range; the item tooltip aligns ranges itself.
	/// </summary>
	internal static Control? MakeLineRow(IUIService uiService, IFontService fontService, TooltipLine line, Color? BGColor = null)
		=> line.Kind switch
		{
			TooltipLineKind.Spacer => MakeRow(uiService, fontService, TOOLTIPSPACER, BGColor: BGColor),
			TooltipLineKind.Delimiter => MakeRow(uiService, fontService, TOOLTIPDELIM, BGColor: BGColor),
			TooltipLineKind.Range => MakeRow(uiService, fontService, $"{line.Text}  {line.Range}", line.Color?.Color(), style: line.Bold ? FontStyle.Bold : FontStyle.Regular, BGColor: BGColor),
			_ => MakeRow(uiService, fontService, line.Text, line.Color?.Color(), style: line.Bold ? FontStyle.Bold : FontStyle.Regular, BGColor: BGColor),
		};

	internal static Label MakeSingleColorLabel(IUIService uiService, IFontService fontService, string friendlyName, Color? FGColor, float fontSize, FontStyle style, Color? BGColor = null)
	{
		// Single Color
		var (tagColor, txt) = friendlyName.SplitLeadingColorTag();
		FGColor = tagColor?.Color() ?? FGColor ?? TQColor.White.Color();// Color Tag take précédence
		var row = new Label()
		{
			Text = txt,
			ForeColor = FGColor.Value,
			Font = fontService.GetFontLight(fontSize, style, uiService.FontZoom),
			AutoSize = true,
			Anchor = AnchorStyles.Left,
			BackColor = BGColor ?? Color.Transparent,

			//BackColor = Color.Red,
			BorderStyle = BorderStyle.None,// BorderStyle.FixedSingle
			Margin = new Padding(0),
		};

		if (BGColor.HasValue) row.BackColor = BGColor.Value;

		return row;
	}
}