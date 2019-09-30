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

		/// <summary>
		/// Return color from color tag identifier
		/// </summary>
		/// <param name="identifier"></param>
		/// <returns></returns>
		public static TQColor GetColorFromTagIdentifier(char identifier)
		{
			switch (identifier)
			{
				case 'A':
					return TQColor.Aqua;

				case 'B':
					return TQColor.Blue;

				case 'C':
					return TQColor.LightCyan;

				case 'D':
					return TQColor.DarkGray;

				case 'F':
					return TQColor.Fuschia;

				case 'G':
					return TQColor.Green;

				case 'I':
					return TQColor.Indigo;

				case 'K':
					return TQColor.Khaki;

				case 'L':
					return TQColor.YellowGreen;

				case 'M':
					return TQColor.Maroon;

				case 'O':
					return TQColor.Orange;

				case 'P':
					return TQColor.Purple;

				case 'R':
					return TQColor.Red;

				case 'S':
					return TQColor.Silver;

				case 'T':
					return TQColor.Turquoise;

				case 'Y':
					return TQColor.Yellow;

				case 'W':
				default:
					return TQColor.White;
			}
		}

		/// <summary>
		/// Return color tag identifier from color 
		/// </summary>
		/// <param name="color"></param>
		/// <returns></returns>
		public static char TagIdentifier(this TQColor color)
		{
			switch (color)
			{
				case TQColor.Aqua:
					return 'A';

				case TQColor.Blue:
					return 'B';

				case TQColor.LightCyan:
					return 'C';

				case TQColor.DarkGray:
					return 'D';

				case TQColor.Fuschia:
					return 'F';

				case TQColor.Green:
					return 'G';

				case TQColor.Indigo:
					return 'I';

				case TQColor.Khaki:
					return 'K';

				case TQColor.YellowGreen:
					return 'L';

				case TQColor.Maroon:
					return 'M';

				case TQColor.Orange:
					return 'O';

				case TQColor.Purple:
					return 'P';

				case TQColor.Red:
					return 'R';

				case TQColor.Silver:
					return 'S';

				case TQColor.Turquoise:
					return 'T';

				case TQColor.Yellow:
					return 'Y';

				case TQColor.White:
				default:
					return 'W';
			}
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


	}
}
