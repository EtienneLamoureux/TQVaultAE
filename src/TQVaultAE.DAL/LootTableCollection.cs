//-----------------------------------------------------------------------
// <copyright file="LootTableCollection.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.DAL
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;

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
		private string tableId;

		/// <summary>
		/// Loot table data
		/// </summary>
		private Dictionary<string, float> data;

		/// <summary>
		/// Holds the total of all of the weight values in the table.  Used to calculate the percentage in the interator block.
		/// </summary>
		private float totalWeight;

		/// <summary>
		/// Initializes a new instance of the LootTableCollection class.
		/// </summary>
		/// <param name="tableId">String containing the table Id we are looking up.</param>
		public LootTableCollection(string tableId)
		{
			this.tableId = tableId;
			this.data = new Dictionary<string, float>();
			this.LoadTable();
		}

		/// <summary>
		/// Gets the number of items in the loot table.
		/// </summary>
		public int Length
		{
			get
			{
				if (this.data == null)
				{
					return 0;
				}

				return this.data.Count;
			}
		}

		/// <summary>
		/// Generic Iterator Block which returns the individual table values with the weighting as percent of total weight.
		/// </summary>
		/// <returns>KeyValuePair for each value in the table.</returns>
		public IEnumerator<KeyValuePair<string, float>> GetEnumerator()
		{
			float divisor = this.totalWeight;

			// Make sure we have something to divide by.
			if (divisor == 0.0F)
			{
				divisor = 1.0F;
			}

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
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		/// <summary>
		/// Builds the table from the database using the passed table Id.
		/// </summary>
		private void LoadTable()
		{
			// Get the data
			DBRecordCollection record = Database.DB.GetRecordFromFile(this.tableId);
			if (record == null)
			{
				return;
			}

			// Go through and get all of the randomizerWeight randomizerName pairs that have a weight > 0.
			Dictionary<int, float> weights = new Dictionary<int, float>();
			Dictionary<int, string> names = new Dictionary<int, string>();

			foreach (Variable variable in record)
			{
				string upperCase = variable.Name.ToUpperInvariant();
				if (upperCase.StartsWith("RANDOMIZERWEIGHT", StringComparison.Ordinal))
				{
					string numPart = upperCase.Substring(16);
					int num;
					if (int.TryParse(numPart, out num))
					{
						// Make sure the value is an integer or float
						float value = -1.0F;

						if (variable.DataType == VariableDataType.Integer)
						{
							value = (float)variable.GetInt32(0);
						}
						else if (variable.DataType == VariableDataType.Float)
						{
							value = variable.GetSingle(0);
						}

						if (value > 0)
						{
							weights.Add(num, value);
						}
					}
				}
				else if (upperCase.StartsWith("RANDOMIZERNAME", StringComparison.Ordinal))
				{
					string numPart = upperCase.Substring(14);
					int num;
					if (int.TryParse(numPart, out num))
					{
						// now get the value
						string value = variable.GetString(0);
						if (!string.IsNullOrEmpty(value))
						{
							names.Add(num, value);
						}
					}
				}
			}

			// Now do an INNER JOIN on the 2 dictionaries to find valid pairs.
			IEnumerable<KeyValuePair<string, float>> buildTableQuery = from weight in weights
																	   join name in names on weight.Key equals name.Key
																	   select new KeyValuePair<string, float>(name.Value, weight.Value);

			// Iterate the query to build the new unweighted table.
			foreach (KeyValuePair<string, float> kvp in buildTableQuery)
			{
				// Check for a double entry in the table.
				if (this.data.ContainsKey(kvp.Key))
				{
					// for a double entry just add the chance.
					this.data[kvp.Key] += kvp.Value;
				}
				else
				{
					this.data.Add(kvp.Key, kvp.Value);
				}
			}

			// Calculate the total weight.
			this.totalWeight = buildTableQuery.Sum(kvp => kvp.Value);
		}
	}
}