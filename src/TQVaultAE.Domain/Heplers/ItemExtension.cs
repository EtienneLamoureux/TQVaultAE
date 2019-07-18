using System.Drawing;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Domain.Helpers
{
	public static class ItemExtension
	{
		/// <summary>
		/// Gets a color tag for a line of text
		/// </summary>
		/// <param name="text">text containing the color tag</param>
		/// <returns>System.Drawing.Color of the embedded color code</returns>
		public static Color GetColor(this Item itm, string text)
		{
			if (string.IsNullOrEmpty(text))
				// Use the standard color code for the item
				return itm.ItemStyle.Color();

			// Look for a formatting tag in the beginning of the string
			TQColor? colorCode = TQColorHelper.GetColorFromTaggedString(text);

			// We didn't find a code so use the standard color code for the item
			if (colorCode is null)
				return itm.ItemStyle.Color();

			// We found something so lets try to find the code
			return colorCode.Value.Color();
		}
	}
}
