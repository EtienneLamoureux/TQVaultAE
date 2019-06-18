using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
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
		public static Color GetColor(ItemStyle style)
		{
			switch (style)
			{
				case ItemStyle.Broken:
					return GetColor(TQColor.DarkGray);

				case ItemStyle.Common:
					return GetColor(TQColor.Yellow);

				case ItemStyle.Epic:
					return GetColor(TQColor.Blue);

				case ItemStyle.Legendary:
					return GetColor(TQColor.Purple);

				case ItemStyle.Mundane:
					return GetColor(TQColor.White);

				case ItemStyle.Potion:
					return GetColor(TQColor.Red);

				case ItemStyle.Quest:
					return GetColor(TQColor.Purple);

				case ItemStyle.Rare:
					return GetColor(TQColor.Green);

				case ItemStyle.Relic:
					return GetColor(TQColor.Orange);

				case ItemStyle.Parchment:
					return GetColor(TQColor.Blue);

				case ItemStyle.Scroll:
					return GetColor(TQColor.YellowGreen);

				case ItemStyle.Formulae:
					return GetColor(TQColor.Turquoise);

				case ItemStyle.Artifact:
					return GetColor(TQColor.Turquoise);

				default:
					return Color.White;
			}
		}

		/// <summary>
		/// Gets the Color for a particular TQ defined color
		/// </summary>
		/// <param name="color">TQ color enumeration</param>
		/// <returns>System.Drawing.Color for the particular TQ color</returns>
		public static Color GetColor(TQColor color)
		{
			switch (color)
			{
				case TQColor.Aqua:
					return Color.FromArgb(0, 255, 255);

				case TQColor.Blue:
					return Color.FromArgb(0, 163, 255);

				case TQColor.DarkGray:
					return Color.FromArgb(153, 153, 153);

				case TQColor.Fuschia:
					return Color.FromArgb(255, 0, 255);

				case TQColor.Green:
					return Color.FromArgb(64, 255, 64);

				case TQColor.Indigo:
					return Color.FromArgb(75, 0, 130);

				case TQColor.Khaki:
					return Color.FromArgb(195, 176, 145);

				case TQColor.LightCyan:
					return Color.FromArgb(224, 255, 255);

				case TQColor.Maroon:
					return Color.FromArgb(128, 0, 0);

				case TQColor.Orange:
					return Color.FromArgb(255, 173, 0);

				case TQColor.Purple:
					return Color.FromArgb(217, 5, 255);

				case TQColor.Red:
					return Color.FromArgb(255, 0, 0);

				case TQColor.Silver:
					return Color.FromArgb(224, 224, 224);

				case TQColor.Turquoise:
					return Color.FromArgb(0, 255, 209);

				case TQColor.White:
					return Color.FromArgb(255, 255, 255);

				case TQColor.Yellow:
					return Color.FromArgb(255, 245, 43);

				case TQColor.YellowGreen:
					return Color.FromArgb(145, 203, 0);

				default:
					return Color.White;
			}
		}

		/// <summary>
		/// Gets a color tag for a line of text
		/// </summary>
		/// <param name="text">text containing the color tag</param>
		/// <returns>System.Drawing.Color of the embedded color code</returns>
		public static Color GetColorTag(Item itm, string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				// Use the standard color code for the item
				return GetColor(itm.ItemStyle);
			}

			// Look for a formatting tag in the beginning of the string
			string colorCode = null;
			if (text.Contains("{^"))
			{
				int i = text.IndexOf("{^");
				if (i == -1)
				{
					colorCode = null;
				}
				else
				{
					colorCode = text.Substring(i + 2, 1).ToUpperInvariant();
				}
			}
			else if (text.StartsWith("^"))
			{
				// If there are not braces assume a 2 character code.
				colorCode = text.Substring(1, 1).ToUpperInvariant();
			}


			// We didn't find a code so use the standard color code for the item
			if (string.IsNullOrEmpty(colorCode))
			{
				return GetColor(itm.ItemStyle);
			}

			// We found something so lets try to find the code
			switch (colorCode)
			{
				case "A":
					return GetColor(TQColor.Aqua);

				case "B":
					return GetColor(TQColor.Blue);

				case "C":
					return GetColor(TQColor.LightCyan);

				case "D":
					return GetColor(TQColor.DarkGray);

				case "F":
					return GetColor(TQColor.Fuschia);

				case "G":
					return GetColor(TQColor.Green);

				case "I":
					return GetColor(TQColor.Indigo);

				case "K":
					return GetColor(TQColor.Khaki);

				case "L":
					return GetColor(TQColor.YellowGreen);

				case "M":
					return GetColor(TQColor.Maroon);

				case "O":
					return GetColor(TQColor.Orange);

				case "P":
					return GetColor(TQColor.Purple);

				case "R":
					return GetColor(TQColor.Red);

				case "S":
					return GetColor(TQColor.Silver);

				case "T":
					return GetColor(TQColor.Turquoise);

				case "W":
					return GetColor(TQColor.White);

				case "Y":
					return GetColor(TQColor.Yellow);

				default:
					return GetColor(itm.ItemStyle);
			}
		}

		/// <summary>
		/// Gets the item's bitmap
		/// </summary>
		public static Bitmap ItemBitmap(this Item itm) {
			return UIService.UI.GetBitmap(itm);
		}
	}

}
