using System.Drawing;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Domain.Helpers
{
	public static class ItemStyleExtension
	{
		/// <summary>
		/// Gets the color for a particular item style
		/// </summary>
		/// <param name="style">ItemStyle enumeration</param>
		/// <returns>System.Drawing.Color for the particular itemstyle</returns>
		public static TQColor TQColor(this ItemStyle style) => style switch
		{
			ItemStyle.Broken => Entities.TQColor.DarkGray,
			ItemStyle.Mundane => Entities.TQColor.White,
			ItemStyle.Common => Entities.TQColor.Yellow,
			ItemStyle.Rare => Entities.TQColor.Green,
			ItemStyle.Epic => Entities.TQColor.Blue,
			ItemStyle.Legendary => Entities.TQColor.Purple,
			ItemStyle.Quest => Entities.TQColor.Purple,
			ItemStyle.Relic => Entities.TQColor.Orange,
			ItemStyle.Potion => Entities.TQColor.Red,
			ItemStyle.Scroll => Entities.TQColor.YellowGreen,
			ItemStyle.Parchment => Entities.TQColor.Blue,
			ItemStyle.Formulae => Entities.TQColor.Turquoise,
			ItemStyle.Artifact => Entities.TQColor.Turquoise,
			_ => Entities.TQColor.White,
		};

		/// <summary>
		/// Gets the color for a particular item style
		/// </summary>
		/// <param name="style">ItemStyle enumeration</param>
		/// <returns>System.Drawing.Color for the particular itemstyle</returns>
		public static Color Color(this ItemStyle style)
			=> style.TQColor().Color();

	}
}
