using TQVaultAE.Domain.Contracts.Services;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Presentation
{
	public class ItemStyleService : IItemStyleService
	{
		/// <summary>
		/// Gets the string name of a particular item style
		/// </summary>
		/// <param name="itemStyle">ItemStyle enumeration</param>
		/// <returns>Localized string of the item style</returns>
		public string Translate(ItemStyle itemStyle)
		{
			return Resources.ResourceManager.GetString($"ItemStyle{itemStyle}");
		}
	}
}
