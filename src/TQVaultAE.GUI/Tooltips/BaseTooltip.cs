﻿using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Presentation;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.GUI.Tooltips;

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

	/// <summary>Copy of a cached image shown by this tooltip; owned by the tooltip.</summary>
	private Bitmap? _shownImage;

	/// <summary>
	/// Replaces the tooltip content with <paramref name="image"/>, a copy taken from a <c>TooltipImageCache</c>.
	/// The tooltip owns the copy and disposes it with itself.
	/// </summary>
	protected void ShowCachedImage(Bitmap image)
	{
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

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		if (disposing)
		{
			this._shownImage?.Dispose();
			this._shownImage = null;
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