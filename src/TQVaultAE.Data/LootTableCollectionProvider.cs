﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
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
	private const StringComparison noCase = StringComparison.OrdinalIgnoreCase;

	private readonly ILogger<LootTableCollectionProvider> Log;
	private readonly IDatabase Database;
	private readonly ITranslationService TranslationService;

	private Dictionary<RecordId, LootTableCollection> LootTableCache = new();

	ReadOnlyDictionary<RecordId, LootTableCollection> _AllLootRandomizerTable;
	public ReadOnlyDictionary<RecordId, LootTableCollection> AllLootRandomizerTable
	{
		get
		{
			if (_AllLootRandomizerTable is null)
			{
				foreach (var tableId in Database.AllLootRandomizerTable.Keys)
					LoadTable(tableId);

				_AllLootRandomizerTable = new ReadOnlyDictionary<RecordId, LootTableCollection>(LootTableCache);
			}

			return _AllLootRandomizerTable;
		}
	}

	private ReadOnlyCollection<LootRandomizerItem> _AllLootRandomizerTranslated;
	/// <summary>
	/// Return all loot randomizer (Affix effect infos)
	/// </summary>
	/// <returns></returns>
	public ReadOnlyCollection<LootRandomizerItem> AllLootRandomizerTranslated
	{
		get
		{
			if (_AllLootRandomizerTranslated is null)
			{
				_AllLootRandomizerTranslated = Database.AllLootRandomizer.Select(r =>
				{
					var translation = TranslationService.TranslateXTag(r.Tag).TQCleanup();// Get translation

					if (string.IsNullOrWhiteSpace(translation))
						translation = r.FileDescription.TQCleanup();

					if (string.IsNullOrWhiteSpace(translation))
						translation = r.Id.PrettyFileName;

					return r with
					{
						Translation = translation,
					};
				}).ToList().AsReadOnly();
			}

			return _AllLootRandomizerTranslated;
		}
	}

	public LootTableCollectionProvider(ILogger<LootTableCollectionProvider> log, IDatabase database, ITranslationService translationService)
	{
		this.Log = log;
		this.Database = database;
		this.TranslationService = translationService;
	}

	private Dictionary<RecordId, (float Weight, LootRandomizerItem LootRandomizer)> MakeTable(RecordId tableId, DBRecordCollection records)
	{
		var Data = new Dictionary<RecordId, (float Weight, LootRandomizerItem LootRandomizer)>();

		#region Build Table

		Dictionary<string, (RecordId AffixId, float Weight)> dico = new();
		string number, randomizerName = "randomizerName", randomizerWeight = "randomizerWeight", affixId;
		foreach (var variable in records)
		{

			switch (variable.Name)
			{
				case var name when name.StartsWith(randomizerName, noCase):
					number = name.Substring(randomizerName.Length);
					affixId = variable.GetString(0);
					if (!string.IsNullOrWhiteSpace(affixId))
					{
						var recid = affixId.ToRecordId();
						if (dico.TryGetValue(number, out var val))
						{
							val.AffixId = recid;
							dico[number] = val;
						}
						else dico.Add(number, (recid, 0F));
					}
					break;
				case var name when name.StartsWith(randomizerWeight, noCase):
					number = name.Substring(randomizerWeight.Length);

					// Make sure the value is an integer or float
					float value = -1.0F;

					if (variable.DataType == VariableDataType.Integer)
						value = (float)variable.GetInt32(0);
					else if (variable.DataType == VariableDataType.Float)
						value = variable.GetSingle(0);

					if (dico.TryGetValue(number, out var valBis))
					{
						valBis.Weight = value;
						dico[number] = valBis;
					}
					else dico.Add(number, (RecordId.Empty, value));

					break;
			}
		}

		// Iterate the query to build the new unweighted table.
		foreach (var kvp in dico.Where(k => k.Value.Weight > 0 && !k.Value.AffixId.IsEmpty))
		{
			var affix = kvp.Value.AffixId;

			// Check for a double entry in the table.
			if (Data.TryGetValue(affix, out var val))// DISTINCT
			{
				// for a double entry just add the chance.
				val.Weight += kvp.Value.Weight;
				Data[affix] = val;
				continue;
			}

			// get affix translations
			var lootrandom = this.AllLootRandomizerTranslated.SingleOrDefault(lr => lr.Id == affix);

			if (lootrandom is null)
			{
				Log.LogError(@"Unknown affix record ""{RecordId}"" from table ""{TableId}"""
					, kvp.Key, tableId);

				lootrandom = LootRandomizerItem.Default(affix);
			}

			Data.Add(affix, (kvp.Value.Weight, lootrandom));
		}

		#endregion

		return Data;
	}

	/// <summary>
	/// Builds the table from the database using the passed table Id.
	/// </summary>
	public LootTableCollection LoadTable(RecordId tableId)
	{
		if (RecordId.IsNullOrEmpty(tableId))
			return null;

		if (LootTableCache.TryGetValue(tableId, out var value))
			return value;

		// Get the data
		if (Database.AllLootRandomizerTable.TryGetValue(tableId, out var records))
		{
			var Data = MakeTable(tableId, records);
			value = new LootTableCollection(tableId, Data);
			LootTableCache.Add(tableId, value);
		}

		return value;
	}
}
