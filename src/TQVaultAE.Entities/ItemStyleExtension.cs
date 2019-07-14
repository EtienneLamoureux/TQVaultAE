using System;
using System.Collections.Generic;
using System.Text;

namespace TQVaultAE.Entities
{
	public static class ItemStyleExtension
	{
		/// <summary>
		/// Gets the color for a particular item style
		/// </summary>
		/// <param name="style">ItemStyle enumeration</param>
		/// <returns>System.Drawing.Color for the particular itemstyle</returns>
		public static TQColor TQColor(this ItemStyle style)
		{
			switch (style)
			{
				case ItemStyle.Broken:
					return Entities.TQColor.DarkGray;

				case ItemStyle.Common:
					return Entities.TQColor.Yellow;

				case ItemStyle.Epic:
					return Entities.TQColor.Blue;

				case ItemStyle.Legendary:
					return Entities.TQColor.Purple;

				case ItemStyle.Potion:
					return Entities.TQColor.Red;

				case ItemStyle.Quest:
					return Entities.TQColor.Purple;

				case ItemStyle.Rare:
					return Entities.TQColor.Green;

				case ItemStyle.Relic:
					return Entities.TQColor.Orange;

				case ItemStyle.Parchment:
					return Entities.TQColor.Blue;

				case ItemStyle.Scroll:
					return Entities.TQColor.YellowGreen;

				case ItemStyle.Formulae:
					return Entities.TQColor.Turquoise;

				case ItemStyle.Artifact:
					return Entities.TQColor.Turquoise;

				case ItemStyle.Mundane:
				default:
					return Entities.TQColor.White;
			}
		}
	}
}
