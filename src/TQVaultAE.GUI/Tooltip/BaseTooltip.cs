using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TQVaultAE.Domain.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Tooltip
{
	public class BaseTooltip : Form
	{
		protected IItemService ItemService;
		protected IFontService FontService;
		protected IUIService UIService;

		internal const string TOOLTIPDELIM = @"TOOLTIPDELIM";
		internal const string TOOLTIPSPACER = @"TOOLTIPSPACER";

#if DEBUG
		// For Design Mode
		public BaseTooltip() { }
#endif

		public BaseTooltip(IItemService itemService, IFontService fontService, IUIService uiService)
		{
			this.ItemService = itemService;
			this.FontService = fontService;
			this.UIService = uiService;
		}

		internal static Control MakeRow(IUIService uiService, IFontService fontService, string friendlyName, Color? FGColor = null, float fontSize = 10F, FontStyle style = FontStyle.Regular, Color? BGColor = null)
		{
			friendlyName = friendlyName ?? string.Empty;
			Control row = null;
			if (friendlyName == TOOLTIPSPACER)
			{
				row = new Label()
				{
					Text = " ",
					Font = fontService.GetFontAlbertusMTLight(fontSize, style, uiService.Scale),
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
				// If there is a color tag in the middle
				if (friendlyName.LastIndexOf('{') > 0)
				{
					var multiColors = friendlyName.Split('{').Where(t => !string.IsNullOrEmpty(t)).ToArray();
					if (multiColors.Count() > 1)
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
							BackColor = BGColor.Value,
						};
						row.SuspendLayout();
						foreach (var coloredSegment in multiColors)
						{
							// IsColorTagged
							if (coloredSegment.First() == '^')
							{
								var segTxt = '{' + coloredSegment;
								row.Controls.Add(MakeSingleColorLabel(uiService, fontService, segTxt, FGColor, fontSize, style, BGColor));
							}
							else
								row.Controls.Add(MakeSingleColorLabel(uiService, fontService, coloredSegment, FGColor, fontSize, style, BGColor));
						}
						row.ResumeLayout();
					}
				}
				else
					row = MakeSingleColorLabel(uiService, fontService, friendlyName, FGColor, fontSize, style, BGColor);
			}

			return row;
		}

		internal static Label MakeSingleColorLabel(IUIService uiService, IFontService fontService, string friendlyName, Color? FGColor, float fontSize, FontStyle style, Color? BGColor = null)
		{
			// Single Color
			FGColor = TQColorHelper.GetColorFromTaggedString(friendlyName)?.Color() ?? FGColor ?? TQColor.White.Color();// Color Tag take précédence
			var txt = TQColorHelper.RemoveLeadingColorTag(friendlyName);
			var row = new Label()
			{
				Text = txt,
				ForeColor = FGColor.Value,
				Font = fontService.GetFontAlbertusMTLight(fontSize, style, uiService.Scale),
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
}
