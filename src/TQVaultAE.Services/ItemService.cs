using TQVaultAE.Domain.Contracts.Providers;
using TQVaultAE.Domain.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Results;

namespace TQVaultAE.Services
{
	public class ItemService : IItemService
	{
		private readonly IItemProvider ItemProvider;

		public ItemService(IItemProvider itemProvider)
		{ this.ItemProvider = itemProvider; }

		/// <summary>
		/// Gets the item name and attributes.
		/// </summary>
		/// <param name="itm"></param>
		/// <param name="scopes">Extra data scopes as a bitmask</param>
		/// <param name="filterExtra">filter extra properties</param>
		/// <returns>An object containing the item name and attributes</returns>
		public ToFriendlyNameResult GetFriendlyNames(Item itm, FriendlyNamesExtraScopes? scopes = null, bool filterExtra = true)
		{
			var result = ItemProvider.GetFriendlyNames(itm, scopes, filterExtra);
			return result;
		}
	}
}
