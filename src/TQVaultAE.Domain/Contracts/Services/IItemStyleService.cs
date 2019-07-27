using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Domain.Contracts.Services
{
	public interface IItemStyleService
	{

		/// <summary>
		/// Gets the string name of a particular item style
		/// </summary>
		/// <param name="itemStyle">ItemStyle enumeration</param>
		/// <returns>Localized string of the item style</returns>
		string Translate(ItemStyle itemStyle);
	}
}