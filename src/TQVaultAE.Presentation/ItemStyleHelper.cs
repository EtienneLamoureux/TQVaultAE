using TQVaultAE.Entities;

namespace TQVaultAE.Presentation
{
	public static class ItemStyleHelper
	{
		/// <summary>
		/// Gets the string name of a particular item style
		/// </summary>
		/// <param name="itemStyle">ItemStyle enumeration</param>
		/// <returns>Localized string of the item style</returns>
		public static string Translate(ItemStyle itemStyle)
		{
			return Resources.ResourceManager.GetString($"ItemStyle{itemStyle}");
		}
	}
}
