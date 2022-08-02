using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TQVaultAE.Domain.Contracts.Providers;
using TQVaultAE.Domain.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Data;

public class LootTableCollectionProvider : ILootTableCollectionProvider
{
	private readonly ILogger<LootTableCollectionProvider> log;
	private readonly IDatabase Database;
	private readonly ITranslationService TranslationService;
	private readonly LazyConcurrentDictionary<string, LootTableCollection> LootTableCache = new LazyConcurrentDictionary<string, LootTableCollection>();

	private ReadOnlyCollection<LootRandomizerItem> _LootRandomizerList;
	/// <summary>
	/// Return all loot randomizer (Affix effect infos)
	/// </summary>
	/// <returns></returns>
	public ReadOnlyCollection<LootRandomizerItem> LootRandomizerList
	{
		get
		{
			if (_LootRandomizerList is null)
			{
				var records = Database.ReadLootRandomizerList();

				_LootRandomizerList = records.Select(r =>
				{
					var translation = TranslationService.TranslateXTag(r.Tag).TQCleanup();// Get translation
					translation = string.IsNullOrWhiteSpace(translation)
						? string.IsNullOrWhiteSpace(r.FileDescription) // Or FileDesc
							? r.PrettyFileName // Or Pretty
							: r.FileDescription.TQCleanup()
						: translation;
					return r with
					{
						Translation = translation,
					};
				}).ToList().AsReadOnly();
			}

			return _LootRandomizerList;
		}
	}

	public LootTableCollectionProvider(ILogger<LootTableCollectionProvider> log, IDatabase database, ITranslationService translationService)
	{
		this.log = log;
		this.Database = database;
		this.TranslationService = translationService;
	}

	/// <summary>
	/// Builds the table from the database using the passed table Id.
	/// </summary>
	public LootTableCollection LoadTable(string tableId)
	{
		return LootTableCache.GetOrAddAtomic(tableId, k =>
		{
			var Data = new Dictionary<string, (float Weight, LootRandomizerItem LootRandomizer)>();

			#region Build Table

			// Get the data
			DBRecordCollection record = Database.GetRecordFromFile(k);
			if (record == null)
				return null;

			// Go through and get all of the randomizerWeight randomizerName pairs that have a weight > 0.
			var weights = new Dictionary<int, float>();
			var names = new Dictionary<int, string>();

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
			IEnumerable<KeyValuePair<string, float>> buildTableQuery =
				from weight in weights
				join name in names on weight.Key equals name.Key
				select new KeyValuePair<string, float>(name.Value, weight.Value);

			#endregion

			// Iterate the query to build the new unweighted table.
			foreach (KeyValuePair<string, float> kvp in buildTableQuery)
			{
				// Check for a double entry in the table.
				if (Data.ContainsKey(kvp.Key))
				{
					// for a double entry just add the chance.
					var val = Data[kvp.Key];
					val.Weight += kvp.Value;
					Data[kvp.Key] = val;
					continue;
				}

				// get affix translations
				var affixRec = this.LootRandomizerList.SingleOrDefault(lr =>
					lr.Id == kvp.Key.NormalizeRecordPath()
				);
				if (affixRec is null)
				{
					log.LogError(@"Unknown affix record ""{RecordId}"" in table ""{TableId}"""
						, kvp.Key, tableId);
					Data.Add(kvp.Key, (kvp.Value, LootRandomizerItem.Empty));
					continue;
				}

				Data.Add(kvp.Key, (kvp.Value, affixRec));
			}

			return new LootTableCollection(k, Data);
		});
	}
}
