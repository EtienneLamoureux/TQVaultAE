using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Results;

namespace TQVaultAE.Domain.Contracts.Services
{
	public interface IItemService
	{
		/// <summary>
		/// Gets the item name and attributes.
		/// </summary>
		/// <param name="itm"></param>
		/// <param name="scopes">Extra data scopes as a bitmask</param>
		/// <param name="filterExtra">filter extra properties</param>
		/// <returns>An object containing the item name and attributes</returns>
		ToFriendlyNameResult GetFriendlyNames(Item itm, FriendlyNamesExtraScopes? scopes = null, bool filterExtra = true);
	}
}