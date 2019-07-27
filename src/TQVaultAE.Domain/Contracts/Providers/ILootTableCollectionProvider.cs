using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Domain.Contracts.Providers
{
	public interface ILootTableCollectionProvider
	{
		/// <summary>
		/// Builds the table from the database using the passed table Id.
		/// </summary>
		LootTableCollection LoadTable(string tableId);
	}
}