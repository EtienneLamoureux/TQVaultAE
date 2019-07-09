using System.Drawing;
using System.Text.RegularExpressions;
using TQVaultAE.Entities;

namespace TQVaultAE.Presentation
{
	public static class ItemGfxHelper
	{

		/// <summary>
		/// Gets the color for a particular item style
		/// </summary>
		/// <param name="style">ItemStyle enumeration</param>
		/// <returns>System.Drawing.Color for the particular itemstyle</returns>
		public static Color Color(ItemStyle style)
		{
			return Color(style.TQColor());
		}


		/// <summary>
		/// Gets the Color for a particular TQ defined color
		/// </summary>
		/// <param name="color">TQ color enumeration</param>
		/// <returns>System.Drawing.Color for the particular TQ color</returns>
		public static Color Color(this TQColor color)
		{
			switch (color)
			{
				case TQColor.Aqua:
					return System.Drawing.Color.FromArgb(0, 255, 255);

				case TQColor.Blue:
					return System.Drawing.Color.FromArgb(0, 163, 255);

				case TQColor.DarkGray:
					return System.Drawing.Color.FromArgb(153, 153, 153);

				case TQColor.Fuschia:
					return System.Drawing.Color.FromArgb(255, 0, 255);

				case TQColor.Green:
					return System.Drawing.Color.FromArgb(64, 255, 64);

				case TQColor.Indigo:
					return System.Drawing.Color.FromArgb(75, 0, 130);

				case TQColor.Khaki:
					return System.Drawing.Color.FromArgb(195, 176, 145);

				case TQColor.LightCyan:
					return System.Drawing.Color.FromArgb(224, 255, 255);

				case TQColor.Maroon:
					return System.Drawing.Color.FromArgb(128, 0, 0);

				case TQColor.Orange:
					return System.Drawing.Color.FromArgb(255, 173, 0);

				case TQColor.Purple:
					return System.Drawing.Color.FromArgb(217, 5, 255);

				case TQColor.Red:
					return System.Drawing.Color.FromArgb(255, 0, 0);

				case TQColor.Silver:
					return System.Drawing.Color.FromArgb(224, 224, 224);

				case TQColor.Turquoise:
					return System.Drawing.Color.FromArgb(0, 255, 209);

				case TQColor.White:
					return System.Drawing.Color.FromArgb(255, 255, 255);

				case TQColor.Yellow:
					return System.Drawing.Color.FromArgb(255, 245, 43);

				case TQColor.YellowGreen:
					return System.Drawing.Color.FromArgb(145, 203, 0);

				default:
					return System.Drawing.Color.White;
			}
		}

		/// <summary>
		/// Gets a color tag for a line of text
		/// </summary>
		/// <param name="text">text containing the color tag</param>
		/// <returns>System.Drawing.Color of the embedded color code</returns>
		public static Color GetColor(this Item itm, string text)
		{
			if (string.IsNullOrEmpty(text))
				// Use the standard color code for the item
				return Color(itm.ItemStyle);

			// Look for a formatting tag in the beginning of the string
			TQColor? colorCode = TQColorHelper.GetColorFromTaggedString(text);

			// We didn't find a code so use the standard color code for the item
			if (colorCode is null)
				return Color(itm.ItemStyle);

			// We found something so lets try to find the code
			return Color(colorCode.Value);
		}

		/// <summary>
		/// Gets the item's bitmap
		/// </summary>
		public static Bitmap ItemBitmap(this Item itm)
		{
			return UIService.UI.GetBitmap(itm);
		}
	}

}
