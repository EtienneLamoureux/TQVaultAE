using System;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;

namespace TQVaultAE.Presentation.Html
{
	public static class HtmlHelper
	{

		/// <summary>
		/// Gets the HTML formatted color of a specified color
		/// </summary>
		/// <param name="color">Color to be HTMLized</param>
		/// <returns>string of HTML formatted color</returns>
		public static string HtmlColor(Color color)
		{
			return string.Format(CultureInfo.InvariantCulture, "#{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B);
		}

		/// <summary>
		/// Gets the Tooltip body tag.
		/// </summary>
		public static string TooltipBodyTag(float Scale)
		{
			return string.Format(CultureInfo.CurrentCulture, @"<body bgcolor=#2e291f text=white><font face=""{1}"" size={0}>", Convert.ToInt32(9.0F * Scale), FontHelper.FONT_ALBERTUSMT.Name);
		}

		/// <summary>
		/// Gets the Tooltip Title tag.
		/// </summary>
		public static string TooltipTitleTag(float Scale)
		{
			return string.Format(CultureInfo.CurrentCulture, @"<body bgcolor=#2e291f text=white><font face=""{1}"" size={0}>", Convert.ToInt32(10.0F * Scale), FontHelper.FONT_ALBERTUSMT.Name);
		}

		/// <summary>
		/// Takes plain text and replaces any characters that do not belong in html with the appropriate stuff.
		/// i.e. replaces > with &gt; etc
		/// </summary>
		/// <param name="text">Text to be formatted</param>
		/// <returns>Formatted text string</returns>
		public static string MakeSafeForHtml(string text)
		{
			text = Regex.Replace(text, "&", "&amp;");
			text = Regex.Replace(text, "<", "&lt;");
			text = Regex.Replace(text, ">", "&gt;");
			return text;
		}
	}
}
