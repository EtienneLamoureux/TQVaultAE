using System;
using System.Collections.Generic;
using System.Linq;
using TQVaultAE.Domain.Contracts.Providers;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Data
{
	public class LootTableCollectionProvider : ILootTableCollectionProvider
	{
		private readonly IDatabase Database;

		public LootTableCollectionProvider(IDatabase database)
		{
			this.Database = database;
		}

		/// <summary>
		/// Builds the table from the database using the passed table Id.
		/// </summary>
		public LootTableCollection LoadTable(string tableId)
		{
			var ntab = new LootTableCollection(tableId);
			// Get the data
			DBRecordCollection record = Database.GetRecordFromFile(ntab.tableId);
			if (record == null)
				return ntab;

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
							value = (float)variable.GetInt32(0);
						else if (variable.DataType == VariableDataType.Float)
							value = variable.GetSingle(0);

						if (value > 0)
							weights.Add(num, value);
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
							names.Add(num, value);
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
				if (ntab.data.ContainsKey(kvp.Key))
					// for a double entry just add the chance.
					ntab.data[kvp.Key] += kvp.Value;
				else
					ntab.data.Add(kvp.Key, kvp.Value);
			}

			// Calculate the total weight.
			ntab.totalWeight = buildTableQuery.Sum(kvp => kvp.Value);

			return ntab;
		}
	}
}
