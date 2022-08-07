using System.Collections.ObjectModel;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Domain.Contracts.Providers
{
	public interface ILootTableCollectionProvider
	{

		/// <summary>
		/// Return all loot randomizer (Affix effect infos)
		/// </summary>
		/// <returns></returns>

		ReadOnlyCollection<LootRandomizerItem> LootRandomizerList { get; }

		/// <summary>
		/// Builds the table from the database using the passed table Id.
		/// </summary>
		LootTableCollection LoadTable(string tableId);
	}
}