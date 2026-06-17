//-----------------------------------------------------------------------
// <copyright file="Item.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.ObjectModel;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Data;

public partial class ItemProvider
{
	public ItemAffixes GetAllAvailableAffixes(GearType type)
	{
		if ((type & GearType.AllWearable) == GearType.Undefined) return null;

		var affixmap = this.LootTableCollectionProvider.AllLootRandomizerTable
			.Where(t =>
				(t.Key.LootTableGearType & type) != GearType.Undefined
				|| (t.Key.LootTableGearType & GearType.MonsterInfrequent) != GearType.Undefined // Always get Monster infrequent affixes
				|| t.Key.IsTablesUnique // Always get loot table for uniques
			);

		Dictionary<GameDlc, ReadOnlyCollection<LootTableCollection>> Broken, Prefix, Suffix;

		List<(GameDlc Dlc, LootTableCollection Table)> LBroken = new(), LPrefix = new(), LSuffix = new();

		LootTableCollection table;
		GameDlc dlc;
		foreach (var map in affixmap)
		{
			if (!map.Key.IsEmpty && map.Key.IsBroken)
			{
				dlc = map.Key.Dlc;
				table = map.Value;
				if (table is not null) LBroken.Add((dlc, table));
			}

			if (!map.Key.IsEmpty && map.Key.IsPrefix)
			{
				dlc = map.Key.Dlc;
				table = map.Value;
				if (table is not null) LPrefix.Add((dlc, table));
			}
			if (!map.Key.IsEmpty && map.Key.IsSuffix)
			{
				dlc = map.Key.Dlc;
				table = map.Value;
				if (table is not null) LSuffix.Add((dlc, table));
			}
		}

		Broken = LBroken.GroupBy(i => i.Dlc).OrderBy(i => i.Key)
			.ToDictionary(i => i.Key, j => j.Select(k => k.Table).ToList().AsReadOnly());
		Prefix = LPrefix.GroupBy(i => i.Dlc).OrderBy(i => i.Key)
			.ToDictionary(i => i.Key, j => j.Select(k => k.Table).ToList().AsReadOnly());
		Suffix = LSuffix.GroupBy(i => i.Dlc).OrderBy(i => i.Key)
			.ToDictionary(i => i.Key, j => j.Select(k => k.Table).ToList().AsReadOnly());

		return new ItemAffixes(
			new ReadOnlyDictionary<GameDlc, ReadOnlyCollection<LootTableCollection>>(Broken)
			, new ReadOnlyDictionary<GameDlc, ReadOnlyCollection<LootTableCollection>>(Prefix)
			, new ReadOnlyDictionary<GameDlc, ReadOnlyCollection<LootTableCollection>>(Suffix)
		);
	}

	public ItemAffixes GetItemAffixes(RecordId itemId)
	{
		return ItemAffixesCache.GetOrAddAtomic(itemId, k =>
	{
		var affixmap = this.Database.GetItemAffixTableMap(k);
		if (affixmap is null || affixmap.All(a => a.IsEmpty)) return null;

		Dictionary<GameDlc, ReadOnlyCollection<LootTableCollection>> Broken, Prefix, Suffix;

		List<(GameDlc Dlc, LootTableCollection Table)> LBroken = new(), LPrefix = new(), LSuffix = new();

		LootTableCollection table;
		GameDlc dlc;
		foreach (var map in affixmap)
		{
			if (!map.BrokenTable.IsEmpty)
			{
				dlc = map.BrokenTable.Dlc;
				table = this.LootTableCollectionProvider.LoadTable(map.BrokenTable);
				if (table is not null) LBroken.Add((dlc, table));
			}

			if (!map.PrefixTable.IsEmpty)
			{
				dlc = map.PrefixTable.Dlc;
				table = this.LootTableCollectionProvider.LoadTable(map.PrefixTable);
				if (table is not null) LPrefix.Add((dlc, table));
			}
			if (!map.SuffixTable.IsEmpty)
			{
				dlc = map.SuffixTable.Dlc;
				table = this.LootTableCollectionProvider.LoadTable(map.SuffixTable);
				if (table is not null) LSuffix.Add((dlc, table));
			}
		}

		Broken = LBroken.GroupBy(i => i.Dlc).OrderBy(i => i.Key)
			.ToDictionary(i => i.Key, j => j.Select(k => k.Table).ToList().AsReadOnly());
		Prefix = LPrefix.GroupBy(i => i.Dlc).OrderBy(i => i.Key)
			.ToDictionary(i => i.Key, j => j.Select(k => k.Table).ToList().AsReadOnly());
		Suffix = LSuffix.GroupBy(i => i.Dlc).OrderBy(i => i.Key)
			.ToDictionary(i => i.Key, j => j.Select(k => k.Table).ToList().AsReadOnly());

		return new ItemAffixes(
			new ReadOnlyDictionary<GameDlc, ReadOnlyCollection<LootTableCollection>>(Broken)
			, new ReadOnlyDictionary<GameDlc, ReadOnlyCollection<LootTableCollection>>(Prefix)
			, new ReadOnlyDictionary<GameDlc, ReadOnlyCollection<LootTableCollection>>(Suffix)
		);
	});
	}
	#region Must be a flat prop

	/// <summary>
	/// Gets the socketed charm/relic bonus loot table
	/// </summary>
	/// <param name="Item"></param>
	/// <param name="RelicTable1"></param>
	/// <param name="RelicTable2"></param>
	/// <returns>Returns <c>false</c> if the item does not contain a charm/relic</returns>
	public bool BonusTableSocketedRelic(Item Item, out LootTableCollection RelicTable1, out LootTableCollection RelicTable2)
	{
		RelicTable1 = RelicTable2 = null;
		var hasCompleteRelic1 = Item.HasRelicOrCharmSlot1 && Item.RelicInfo is not null && Item.IsRelicBonus1Complete;
		var hasCompleteRelic2 = Item.HasRelicOrCharmSlot2 && Item.Relic2Info is not null && Item.IsRelicBonus2Complete;

		if (Item.baseItemInfo is null
			|| (!hasCompleteRelic1 && !hasCompleteRelic2)
		) return false;

		string tableId = null;
		RecordId tableIdRec = null;

		if (hasCompleteRelic1)
		{
			tableId = Item.RelicInfo.GetString("bonusTableName");
			tableIdRec = tableId;
			RelicTable1 = LootTableCollectionProvider.LoadTable(tableIdRec);
		}

		if (hasCompleteRelic2)
		{
			tableId = Item.Relic2Info.GetString("bonusTableName");
			tableIdRec = tableId;
			RelicTable2 = LootTableCollectionProvider.LoadTable(tableIdRec);
		}

		return RelicTable1 is not null || RelicTable2 is not null;
	}

	/// <summary>
	/// Gets the artifact/charm/relic bonus loot table
	/// returns null if the item is not an artifact/charm/relic or does not contain a charm/relic
	/// </summary>
	/// <param name="itm"></param>
	/// <returns>returns null if the item is not an artifact/charm/relic</returns>
	public LootTableCollection BonusTableRelicOrArtifact(Item itm)
	{
		if (itm.baseItemInfo == null)
			return null;

		string lootTableID = null;
		if (itm.IsRelicOrCharm)
			lootTableID = itm.baseItemInfo.GetString("bonusTableName");
		else if (itm.IsArtifact)
		{
			// for artifacts we need to find the formulae that was used to create the artifact.  sucks to be us
			// The formulas seem to always be in the arcaneformulae subfolder with a _formula on the end
			// of the filename
			string folder = PathIO.GetDirectoryName(itm.BaseItemId.Raw);
			folder = PathIO.Combine(folder, "arcaneformulae");
			string file = PathIO.GetFileNameWithoutExtension(itm.BaseItemId.Raw);

			// Damn it, IL did not keep the filename consistent on Kingslayer (Sands of Kronos)
			if (file.Equals("E_GA_SANDOFKRONOS", noCase))
				file = file.Insert(9, "s");

			file = string.Concat(file, "_formula");
			file = PathIO.Combine(folder, file);
			file = PathIO.ChangeExtension(file, PathIO.GetExtension(itm.BaseItemId.Raw));

			// Now lookup itm record.
			DBRecordCollection record = Database.GetRecordFromFile(file);
			if (record is not null)
				lootTableID = record.GetString("artifactBonusTableName", 0);
		}

		if (!string.IsNullOrWhiteSpace(lootTableID))
			return LootTableCollectionProvider.LoadTable(lootTableID);

		return null;
	}
	#endregion


	/// <summary>
	/// Removes the first relic from this item
	/// </summary>
	/// <returns>Returns the removed relic as a new Item</returns>
	public Item RemoveRelic1(Item itm)
	{
		if (!itm.HasRelicOrCharmSlot1)
			return null;

		Item newRelic = itm.MakeEmptyCopy(itm.relicID);
		newRelic.RelicBonusId = itm.RelicBonusId;
		newRelic.RelicBonusInfo = itm.RelicBonusInfo;
		newRelic.Var1 = itm.Var1;
		GetDBData(newRelic);

		// Now clear out our relic data
		itm.relicID = RecordId.Empty;
		itm.RelicBonusId = RecordId.Empty;
		itm.Var1 = 0;
		itm.RelicInfo = null;
		itm.RelicBonusInfo = null;

		itm.IsModified = true;

		return newRelic;
	}

	/// <summary>
	/// Removes the second relic from this item
	/// </summary>
	/// <returns>Returns the removed relic as a new Item</returns>
	public Item RemoveRelic2(Item itm)
	{
		if (!itm.HasRelicOrCharmSlot2)
			return null;

		Item newRelic = itm.MakeEmptyCopy(itm.relic2ID);
		newRelic.RelicBonusId = itm.RelicBonus2Id;
		newRelic.RelicBonusInfo = itm.RelicBonus2Info;
		newRelic.Var1 = itm.Var2;
		GetDBData(newRelic);

		// Now clear out our relic data
		itm.relic2ID = RecordId.Empty;
		itm.RelicBonus2Id = RecordId.Empty;
		itm.Var2 = Item.var2Default;
		itm.Relic2Info = null;
		itm.RelicBonus2Info = null;

		itm.IsModified = true;

		return newRelic;
	}

	/// <summary>
	/// Create an artifact from its formulae
	/// </summary>
	/// <returns>A new artifact</returns>
	public Item CraftArtifact(Item itm)
	{
		if (itm.IsFormulae && itm.baseItemInfo != null)
		{
			string artifactname = itm.baseItemInfo.GetString("artifactName");
			Item newArtifact = itm.MakeEmptyCopy(artifactname);
			GetDBData(newArtifact);

			itm.IsModified = true;

			return newArtifact;
		}
		return null;
	}
}
