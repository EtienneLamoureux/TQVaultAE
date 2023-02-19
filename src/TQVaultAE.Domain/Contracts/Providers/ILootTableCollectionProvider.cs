using System.Collections.ObjectModel;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Domain.Contracts.Providers;

public interface ILootTableCollectionProvider
{
	/// <summary>
	/// Return all loot randomizer table (Affix effect infos)
	/// </summary>
	ReadOnlyDictionary<RecordId, LootTableCollection> AllLootRandomizerTable { get; }

	/// <summary>
	/// Return all loot randomizer (Affix effect infos)
	/// </summary>
	/// <returns></returns>
	ReadOnlyDictionary<RecordId, LootRandomizerItem> AllLootRandomizerTranslated { get; }

	/// <summary>
	/// Builds the table from the database using the passed table Id.
	/// </summary>
	LootTableCollection LoadTable(RecordId tableId);
}