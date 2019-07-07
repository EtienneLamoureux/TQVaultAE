using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TQVaultAE.Entities
{

	public static class TQColorHelper
	{
		/// <summary>
		/// Regex Match color tag 4 chars & 2 chars
		/// </summary>
		public const string RegExColorTag = @"(?<ColorTag>\{\^(?<ColorId>\w)}|\^(?<ColorId>\w))";
		/// <summary>
		/// Regex Match starting color tag 4 chars & 2 chars or empty
		/// </summary>
		public const string RegExStartingColorTagOrEmpty = @"^" + RegExColorTag + @"?";

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


	}
}
