using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Domain.Helpers
{

	public static class TQColorHelper
	{
		/// <summary>
		/// Regex Match color tag 4 chars & 2 chars
		/// </summary>
		public const string RegExTQTag = @"(?<ColorTag>\{\^(?<ColorId>\w)}|\^(?<ColorId>\w))";
		/// <summary>
		/// Regex Match starting color tag 4 chars & 2 chars or empty
		/// </summary>
		public const string RegExStartingColorTagOrEmpty = @"^" + RegExTQTag + @"?";

		static readonly (TQColor ColorEnum, char ColorChar, Color ColorSys)[] ColorMap = new[] {
			(TQColor.Aqua, 'A', System.Drawing.Color.FromArgb(0, 255, 255))
			, (TQColor.Blue, 'B', System.Drawing.Color.FromArgb(0, 163, 255))
			, (TQColor.LightCyan, 'C', System.Drawing.Color.FromArgb(224, 255, 255))
			, (TQColor.DarkGray, 'D', System.Drawing.Color.FromArgb(153, 153, 153))
			, (TQColor.Fuschia, 'F', System.Drawing.Color.FromArgb(255, 0, 255))
			, (TQColor.Green, 'G', System.Drawing.Color.FromArgb(64, 255, 64))
			, (TQColor.Indigo, 'I', System.Drawing.Color.FromArgb(75, 0, 130))
			, (TQColor.Khaki, 'K', System.Drawing.Color.FromArgb(195, 176, 145))
			, (TQColor.YellowGreen, 'L', System.Drawing.Color.FromArgb(145, 203, 0))
			, (TQColor.Maroon, 'M', System.Drawing.Color.FromArgb(128, 0, 0))
			, (TQColor.Orange, 'O', System.Drawing.Color.FromArgb(255, 173, 0))
			, (TQColor.Purple, 'P', System.Drawing.Color.FromArgb(217, 5, 255))
			, (TQColor.Red, 'R', System.Drawing.Color.FromArgb(255, 0, 0))
			, (TQColor.Silver, 'S', System.Drawing.Color.FromArgb(224, 224, 224))
			, (TQColor.Turquoise, 'T', System.Drawing.Color.FromArgb(0, 255, 209))
			, (TQColor.Yellow, 'Y', System.Drawing.Color.FromArgb(255, 245, 43))
			, (TQColor.White, 'W', System.Drawing.Color.White)
		};

		/// <summary>
		/// Return color from color tag identifier
		/// </summary>
		/// <param name="identifier"></param>
		/// <returns></returns>
		public static TQColor GetColorFromTagIdentifier(char identifier)
		{
			var map = ColorMap.Where(c => c.ColorChar == identifier).Select(c => c.ColorEnum);
			return map.Any() ? map.First() : TQColor.White;
		}

		/// <summary>
		/// Return color tag identifier from color 
		/// </summary>
		/// <param name="color"></param>
		/// <returns></returns>
		public static char TagIdentifier(this TQColor color)
		{
			var map = ColorMap.Where(c => c.ColorEnum == color).Select(c => c.ColorChar);
			return map.Any() ? map.First() : 'W';
		}

		/// <summary>
		/// Return the TQColor corresponding to color tag prefix
		/// </summary>
		/// <param name="text"></param>
		/// <returns>null if no color prefix</returns>
		public static TQColor? GetColorFromTaggedString(this string text)
		{
			if (string.IsNullOrWhiteSpace(text)) return null;
			TQColor? res = null;
			string ColorId = Regex.Replace(text, $@"{RegExStartingColorTagOrEmpty}.*", @"${ColorId}").ToUpperInvariant();
			if (ColorId.Any())
				res = GetColorFromTagIdentifier(ColorId.First());
			return res;
		}

		/// <summary>
		/// Get color tag from <see cref="TQColor"/>.
		/// </summary>
		/// <param name="color"></param>
		/// <param name="fourCharFormat"></param>
		/// <returns></returns>
		public static string ColorTag(this TQColor color, bool fourCharFormat = true)
			=> fourCharFormat ? $"{{^{color.TagIdentifier()}}}" : $"^{color.TagIdentifier()}";

		/// <summary>
		/// Remove leading color tag from <paramref name="TQText"/>
		/// </summary>
		/// <param name="TQText"></param>
		/// <returns></returns>
		public static string RemoveLeadingColorTag(this string TQText)
		{
			if (string.IsNullOrWhiteSpace(TQText)) return TQText ?? string.Empty;
			return Regex.Replace(TQText, RegExStartingColorTagOrEmpty, string.Empty);
		}


		/// <summary>
		/// Gets the Color for a particular TQ defined color
		/// </summary>
		/// <param name="color">TQ color enumeration</param>
		/// <returns>System.Drawing.Color for the particular TQ color</returns>
		public static Color Color(this TQColor color)
		{
			var map = ColorMap.Where(c => c.ColorEnum == color).Select(c => c.ColorSys);
			return map.Any() ? map.First() : System.Drawing.Color.White;
		}
	}
}
