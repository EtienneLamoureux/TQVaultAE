//-----------------------------------------------------------------------
// <copyright file="LootTableCollection.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.Domain.Entities
{
	using System.Collections;
	using System.Collections.Generic;

	/// <summary>
	/// Builds a loot table from a particular loot table ID.
	/// </summary>
	/// <remarks>
	/// Used for showing possible completion bonuses.
	/// </remarks>
	public class LootTableCollection : IEnumerable<KeyValuePair<string, float>>
	{
		/// <summary>
		/// String containing the database Id for the table
		/// </summary>
		public string tableId;

		/// <summary>
		/// Loot table data
		/// </summary>
		public Dictionary<string, float> data;

		/// <summary>
		/// Holds the total of all of the weight values in the table.  Used to calculate the percentage in the interator block.
		/// </summary>
		public float totalWeight;

		/// <summary>
		/// Initializes a new instance of the LootTableCollection class.
		/// </summary>
		/// <param name="tableId">String containing the table Id we are looking up.</param>
		public LootTableCollection(string tableId)
		{
			this.tableId = tableId;
			this.data = new Dictionary<string, float>();
		}

		/// <summary>
		/// Gets the number of items in the loot table.
		/// </summary>
		public int Length => this.data == null ? 0 : this.data.Count;


		/// <summary>
		/// Generic Iterator Block which returns the individual table values with the weighting as percent of total weight.
		/// </summary>
		/// <returns>KeyValuePair for each value in the table.</returns>
		public IEnumerator<KeyValuePair<string, float>> GetEnumerator()
		{
			float divisor = this.totalWeight;

			// Make sure we have something to divide by.
			if (divisor == 0.0F)
				divisor = 1.0F;

			// Iterate and return the weighted values in the table.
			foreach (KeyValuePair<string, float> kvp in this.data)
			{
				yield return new KeyValuePair<string, float>(kvp.Key, kvp.Value / divisor);
			}
		}

		/// <summary>
		/// Non Generic enumerator interface.
		/// </summary>
		/// <returns>Generic interface implementation.</returns>
		IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

	}
}