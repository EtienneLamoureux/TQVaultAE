//-----------------------------------------------------------------------
// <copyright file="LootTableCollection.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TQVaultAE.Domain.Entities;

/// <summary>
/// Builds a loot table from a particular loot table ID.
/// </summary>
/// <remarks>
/// Used for showing possible completion bonuses.
/// </remarks>
public class LootTableCollection : IEnumerable<KeyValuePair<string, LootTableValue>>
{
	/// <summary>
	/// String containing the database Id for the table
	/// </summary>
	public readonly string TableId;

	/// <summary>
	/// Loot table data
	/// </summary>
	public readonly ReadOnlyDictionary<string, LootTableValue> Data;

	/// <summary>
	/// Holds the total of all of the weight values in the table.  Used to calculate the percentage in the interator block.
	/// </summary>
	public readonly float TotalWeight;

	/// <summary>
	/// Initializes a new instance of the LootTableCollection class.
	/// </summary>
	/// <param name="tableId">String containing the table Id we are looking up.</param>
	public LootTableCollection(string tableId, Dictionary<string, (float Weight, LootRandomizerItem LootRandomizer)> data)
	{
		TableId = tableId;
		// Calculate the total weight.
		TotalWeight = data.Sum(kvp => kvp.Value.Weight);
		// Make sure we have something to divide by.
		if (TotalWeight == 0.0F)
			TotalWeight = 1.0F;

		// Compute and make immutable
		Data = new ReadOnlyDictionary<string, LootTableValue>(
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

	/// <summary>
	/// Gets the number of items in the loot table.
	/// </summary>
	public int Length => this.Data.Count;


	/// <summary>
	/// Generic Iterator Block which returns the individual table values with the weighting as percent of total weight.
	/// </summary>
	/// <returns>KeyValuePair for each value in the table.</returns>
	public IEnumerator<KeyValuePair<string, LootTableValue>> GetEnumerator()
	{
		foreach (var kvp in this.Data) yield return kvp;
	}

	/// <summary>
	/// Non Generic enumerator interface.
	/// </summary>
	/// <returns>Generic interface implementation.</returns>
	IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
}