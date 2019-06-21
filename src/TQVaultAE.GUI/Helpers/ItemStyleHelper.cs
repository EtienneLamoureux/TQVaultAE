using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TQVaultAE.Entities;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Helpers
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
