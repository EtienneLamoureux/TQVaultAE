//-----------------------------------------------------------------------
// <copyright file="LootTable.cs" company="bman654">
//     Copyright (c) Brandon Wallace. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVault
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using TQVaultData;
    
    /// <summary>
    /// Builds a loot table from a particular loot table ID.
    /// </summary>
    /// <remarks>
    /// Used for showing possible completion bonuses.
    /// </remarks>
    public class LootTable
    {
        /// <summary>
        /// String containing the database Id for the table
        /// </summary>
        private string tableId;

        /// <summary>
        /// Loot table data
        /// </summary>
        private KeyValuePair<string, float>[] data;

        /// <summary>
        /// Initializes a new instance of the LootTable class.
        /// </summary>
        /// <param name="tableId">String containing the table Id we are looking up.</param>
        public LootTable(string tableId)
        {
            this.tableId = tableId;
            this.LoadTable();
        }

        /// <summary>
        /// Gets the loot table that we just built from the table Id.
        /// </summary>
        /// <returns>Array of the loot table KeyValuePairs.</returns>
        public KeyValuePair<string, float>[] GetData()
        {
            return (KeyValuePair<string, float>[])this.data.Clone();
        }

        /// <summary>
        /// Builds the table from the database using the passed table Id.
        /// </summary>
        private void LoadTable()
        {
            // Get the data
            DBRecord record = Database.DB.GetRecordFromFile(this.tableId);
            if (record == null)
            {
                this.data = new KeyValuePair<string, float>[0];
                return;
            }

            // Go through and get all of the randomizerWeight randomizerName pairs that have a weight > 0.
            Dictionary<int, float> weights = new Dictionary<int, float>();
            Dictionary<int, string> names = new Dictionary<int, string>();

            foreach (Variable variable in record.VariableCollection)
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
                        if (value.Length != 0)
                        {
                            names.Add(num, value);
                        }
                    }
                }
            }

            // Now do an INNER JOIN on the 2 dictionaries to find valid pairs
            int numEntries = 0;
            float totalWeight = 0;
            foreach (KeyValuePair<int, float> kvp in weights)
            {
                // Make sure we also have a name
                if (names.ContainsKey(kvp.Key))
                {
                    ++numEntries;
                    totalWeight += kvp.Value;
                }
            }

            // Now create our results array.
            this.data = new KeyValuePair<string, float>[numEntries];

            int i = 0;
            foreach (KeyValuePair<int, float> kvp in weights)
            {
                this.data[i] = new KeyValuePair<string, float>(names[kvp.Key], kvp.Value / totalWeight);
                ++i;
            }
        }
    }
}
