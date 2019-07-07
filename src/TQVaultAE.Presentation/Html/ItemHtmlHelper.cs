using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using TQVaultAE.Data;
using TQVaultAE.Entities;
using TQVaultAE.Entities.Results;
using TQVaultAE.Logs;

namespace TQVaultAE.Presentation.Html
{
	public static class ItemHtmlHelper
	{
		private static readonly log4net.ILog Log = Logger.Get(typeof(ItemHtmlHelper));

		/// <summary>
		/// Gets the HTML formatted color of a specified color
		/// </summary>
		/// <param name="color">Color to be HTMLized</param>
		/// <returns>string of HTML formatted color</returns>
		public static string HtmlColor(this Color color)
		{
			return string.Format(CultureInfo.InvariantCulture, "#{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B);
		}

		public static string HtmlColor(this TQColor? tqcolor)
		{
			if (tqcolor is null) return string.Empty;
			var color = tqcolor.Value.Color();
			return string.Format(CultureInfo.InvariantCulture, "#{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B);
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
		/// <summary>
		/// Gets the name of the item
		/// </summary>
		/// <param name="item">item being displayed</param>
		/// <returns>string with the item name</returns>
		public static string GetName(Item item)
		{
			string itemName = ItemHtmlHelper.MakeSafeForHtml(ItemProvider.ToFriendlyName(item, true, false));
			string bgcolor = "#2e1f15";

			Color color = ItemGfxHelper.GetColorTag(item, itemName);
			itemName = Item.ClipColorTag(itemName);
			itemName = string.Format(CultureInfo.InvariantCulture, "<font size=+1 color={0}><b>{1}</b></font>", HtmlHelper.HtmlColor(color), itemName);
			return string.Format(CultureInfo.InvariantCulture, @"<body bgcolor={0} text=white><font face=""{2}"" size=2>{1}", bgcolor, itemName, FontHelper.FONT_ALBERTUSMT.Name);
		}


		/// <summary>
		/// Loads the item properties
		/// </summary>
		public static LoadPropertiesResult LoadProperties(Item item, bool filterExtra)
		{
			var result = new LoadPropertiesResult();

			var bareAttr = ItemProvider.GetFriendlyNames(item, FriendlyNamesExtraScopes.ItemFullDisplay, filterExtra);

			var pattern = @"<body bgcolor={0} text=white><font face=""{2}"" size=1>{1}";

			// Base Item Attributes
			if (bareAttr[0].Any()) result.BaseItemAttributes = string.Format(CultureInfo.InvariantCulture, pattern, result.BgColor, bareAttr[0], FontHelper.FONT_ALBERTUSMT.Name);

			// Prefix Attributes
			if (bareAttr[2].Any()) result.PrefixAttributes = string.Format(CultureInfo.InvariantCulture, pattern, result.BgColor, bareAttr[2], FontHelper.FONT_ALBERTUSMT.Name);


			// Suffix Attributes
			if (bareAttr[3].Any()) result.SuffixAttributes = string.Format(CultureInfo.InvariantCulture, pattern, result.BgColor, bareAttr[3], FontHelper.FONT_ALBERTUSMT.Name);


			return result;
		}

	}
}
