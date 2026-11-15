//-----------------------------------------------------------------------
// <copyright file="LootTableCollection.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.Collections;
using System.Collections.ObjectModel;

namespace TQVaultAE.Domain.Entities;

/// <summary>
/// Builds a loot table from a particular loot table ID.
/// </summary>
/// <remarks>
/// Used for showing possible completion bonuses.
/// </remarks>
public class LootTableCollection : IEnumerable<KeyValuePair<RecordId, LootTableValue>>
{
	/// <summary>
	/// String containing the database Id for the table
	/// </summary>
	public readonly RecordId TableId;

	/// <summary>
	/// Loot table data
	/// </summary>
	public readonly ReadOnlyDictionary<RecordId, LootTableValue> Data;

	/// <summary>
	/// Holds the total of all of the weight values in the table.  Used to calculate the percentage in the interator block.
	/// </summary>
	public readonly float TotalWeight;

	/// <summary>
	/// Initializes a new instance of the LootTableCollection class.
	/// </summary>
	/// <param name="tableId">String containing the table Id we are looking up.</param>
	public LootTableCollection(RecordId tableId, Dictionary<RecordId, (float Weight, LootRandomizerItem LootRandomizer)> data)
	{
		TableId = tableId;
		// Calculate the total weight.
		TotalWeight = data.Sum(kvp => kvp.Value.Weight);
		// Make sure we have something to divide by.
		if (TotalWeight == 0.0F)
			TotalWeight = 1.0F;

		// Compute and make immutable
		Data = new ReadOnlyDictionary<RecordId, LootTableValue>(
			data.ToDictionary(kv =>
				kv.Key
				, kv => new LootTableValue(
					kv.Value.Weight
					, kv.Value.Weight / TotalWeight
					, kv.Value.LootRandomizer
				)
			)
		);
	}

	/// <summary>A table whose shares are already computed.</summary>
	private LootTableCollection(RecordId tableId, ReadOnlyDictionary<RecordId, LootTableValue> data)
	{
		TableId = tableId;
		TotalWeight = 1.0F;
		Data = data;
	}

	/// <summary>
	/// The distribution of affixes of several weighted tables, as the game picks them: first a table by weight, then an
	/// entry of that table by weight. An entry's share is the sum, over the tables listing it, of the table's share times
	/// the entry's share in that table.
	/// </summary>
	/// <remarks>
	/// A table that cannot be loaded (<see langword="null"/>) still takes its share: the game picks it and gets no affix.
	/// The shares then add up to less than 1. Each value's <see cref="LootTableValue.Weight"/> is its share.
	/// </remarks>
	/// <param name="tableId">The id of the mix, usually the loot item table that weighs the tables.</param>
	/// <param name="tables">The tables and their weights; tables without weight are left out.</param>
	public static LootTableCollection Mix(RecordId tableId, IEnumerable<(float Weight, LootTableCollection? Table)> tables)
	{
		var weighted = tables.Where(t => t.Weight > 0).ToList();
		float total = weighted.Sum(t => t.Weight);

		var shares = new Dictionary<RecordId, (float Share, LootRandomizerItem LootRandomizer)>();
		foreach (var (weight, table) in weighted)
		{
			if (table is null)
				continue;

			float tableShare = weight / total;
			foreach (var entry in table.Data)
			{
				float share = tableShare * entry.Value.WeightPercent;
				shares[entry.Key] = shares.TryGetValue(entry.Key, out var known)
					? (known.Share + share, known.LootRandomizer)
					: (share, entry.Value.LootRandomizer);
			}
		}

		return new LootTableCollection(tableId, new ReadOnlyDictionary<RecordId, LootTableValue>(
			shares.ToDictionary(kv => kv.Key, kv => new LootTableValue(kv.Value.Share, kv.Value.Share, kv.Value.LootRandomizer))));
	}

	/// <summary>
	/// Gets the number of items in the loot table.
	/// </summary>
	public int Length => this.Data.Count;

	/// <summary>
	/// Picks an entry of the table by weight.
	/// </summary>
	/// <remarks>
	/// The entries are walked in table order and the roll is reduced by each weight until it falls within one.
	/// The last entry wins when the roll exceeds the sum of the weights.
	/// </remarks>
	/// <param name="roll">Roll between 0 and 1, compared to <see cref="LootTableValue.WeightPercent"/>.</param>
	/// <returns>The key of the picked entry, or <see langword="null"/> when the table is empty.</returns>
	public RecordId? PickWeighted(float roll)
	{
		int remaining = this.Data.Count;
		foreach (var entry in this.Data)
		{
			remaining--;
			if (roll <= entry.Value.WeightPercent || remaining == 0)
				return entry.Key;

			roll -= entry.Value.WeightPercent;
		}

		return null;
	}


	/// <summary>
	/// Generic Iterator Block which returns the individual table values with the weighting as percent of total weight.
	/// </summary>
	/// <returns>KeyValuePair for each value in the table.</returns>
	public IEnumerator<KeyValuePair<RecordId, LootTableValue>> GetEnumerator()
	{
		foreach (var kvp in this.Data) yield return kvp;
	}

	/// <summary>
	/// Non Generic enumerator interface.
	/// </summary>
	/// <returns>Generic interface implementation.</returns>
	IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
}