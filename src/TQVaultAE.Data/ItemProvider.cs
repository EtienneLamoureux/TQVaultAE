//-----------------------------------------------------------------------
// <copyright file="Item.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using Microsoft.Extensions.Logging;
using TQVaultAE.Config;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Domain.Jitter;

namespace TQVaultAE.Data;

/// <summary>
/// Reads and writes items: their records and affixes, their save format, their requirements, the skills and attribute
/// sums they give. Their texts are built by <see cref="ItemTextFormatter"/>.
/// </summary>
public partial class ItemProvider : IItemProvider, IGameDataCache
{
	private const StringComparison noCase = StringComparison.OrdinalIgnoreCase;

	private readonly ILogger Log;
	private readonly IDatabase Database;
	private readonly ILootTableCollectionProvider LootTableCollectionProvider;
	private readonly IItemAttributeProvider ItemAttributeProvider;
	private readonly ITQDataService TQData;
	private readonly IPathIO PathIO;
	private readonly UserSettings USettings;

	/// <summary>
	/// Gives an item linked to a mod its mod's database; null in tests that do not need it.
	/// </summary>
	private readonly IItemModService? ItemMods;

	/// <summary>
	/// The database of the item's mod for the current thread, until disposed.
	/// </summary>
	private IDisposable ModScope(Item? itm) => this.ItemMods.ScopeOrNone(itm);
	private readonly LazyConcurrentDictionary<(string Scope, RecordId Item, bool Unreachable), ItemAffixes?> ItemAffixesCache = new();

	/// <inheritdoc/>
	public void ClearGameDataCache() => this.ItemAffixesCache.Clear();

	public ItemProvider(
			ILogger<ItemProvider> log
			, IDatabase database
			, ILootTableCollectionProvider lootTableCollectionProvider
			, IItemAttributeProvider itemAttributeProvider
			, ITQDataService tQData
			, IPathIO pathIO
			, UserSettings uSettings
			, IItemModService? itemMods = null
		)
	{
		this.ItemMods = itemMods;
		this.Log = log;
		this.Database = database;
		this.LootTableCollectionProvider = lootTableCollectionProvider;
		this.ItemAttributeProvider = itemAttributeProvider;
		this.TQData = tQData;
		PathIO = pathIO;
		USettings = uSettings;
	}

	/// <inheritdoc/>
	public ItemRollResult? RollInGameValues(Item itm, DBRecordCollection? baseRecord)
	{
		var itemClass = itm.baseItemInfo?.ItemClass;
		if (baseRecord is null || itm.Seed == 0 || itemClass is null || itm.IsRelicOrCharm
			|| !(itemClass.StartsWith(Item.ICLASS_PREFIX_ARMORPROTECTIVE, StringComparison.OrdinalIgnoreCase)
				|| itemClass.StartsWith(Item.ICLASS_PREFIX_ARMORJEWELRY, StringComparison.OrdinalIgnoreCase)
				|| itemClass.StartsWith(Item.ICLASS_PREFIX_WEAPON, StringComparison.OrdinalIgnoreCase)))
			return null;

		try
		{
			var prefix = RecordId.IsNullOrEmpty(itm.prefixID) ? null : this.Database.GetRecordFromFile(itm.prefixID);
			var suffix = RecordId.IsNullOrEmpty(itm.suffixID) ? null : this.Database.GetRecordFromFile(itm.suffixID);
			return ItemRollSimulator.Roll((uint)itm.Seed, itm.IsArmor, baseRecord, prefix, suffix);
		}
		catch (Exception ex)
		{
			this.Log.LogError(ex, "Failed to compute in-game values for {Item}", itm.BaseItemId);
			return null;
		}
	}
}
