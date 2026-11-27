using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Config;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Data;

public class LootTableCollectionProvider(ILogger<LootTableCollectionProvider> log, IDatabase database, ITranslationService translationService, UserSettings uSettings) : ILootTableCollectionProvider, IGameDataCache
{
	private const StringComparison noCase = StringComparison.OrdinalIgnoreCase;

	private readonly ILogger<LootTableCollectionProvider> Log = log;
	private readonly IDatabase Database = database;
	private readonly ITranslationService TranslationService = translationService;
	private readonly UserSettings USettings = uSettings;

	/// <summary>The tables read so far, per data scope: a mod may override a table of the base game.</summary>
	private Dictionary<string, Dictionary<RecordId, LootTableCollection>> _lootTableCaches = new(StringComparer.OrdinalIgnoreCase);
	private Dictionary<string, ReadOnlyDictionary<RecordId, LootTableCollection>> _allLootRandomizerTables = new(StringComparer.OrdinalIgnoreCase);

	/// <summary>The tables read so far for the data the calling thread reads now.</summary>
	private Dictionary<RecordId, LootTableCollection> LootTableCache
	{
		get
		{
			var scope = this.Database.DataScope ?? string.Empty;
			if (!this._lootTableCaches.TryGetValue(scope, out var cache))
				this._lootTableCaches[scope] = cache = [];
			return cache;
		}
	}

	/// <inheritdoc/>
	public void ClearGameDataCache()
	{
		this._lootTableCaches = new(StringComparer.OrdinalIgnoreCase);
		this._allLootRandomizerTables = new(StringComparer.OrdinalIgnoreCase);
		this._allLootRandomizerTranslated = new(StringComparer.OrdinalIgnoreCase);
	}

	public ReadOnlyDictionary<RecordId, LootTableCollection> AllLootRandomizerTable
	{
		get
		{
			var scope = this.Database.DataScope ?? string.Empty;
			if (!this._allLootRandomizerTables.TryGetValue(scope, out var all))
			{
				foreach (var tableId in Database.AllLootRandomizerTable.Keys)
					LoadTable(tableId);

				this._allLootRandomizerTables[scope] = all = new ReadOnlyDictionary<RecordId, LootTableCollection>(LootTableCache);
			}

			return all;
		}
	}

	/// <summary>The translated affix records, per data scope: a mod adds affixes and may override those of the base game.</summary>
	private Dictionary<string, ReadOnlyDictionary<RecordId, LootRandomizerItem>> _allLootRandomizerTranslated = new(StringComparer.OrdinalIgnoreCase);

	/// <summary>
	/// Return all loot randomizer (Affix effect infos) of the data the calling thread reads now.
	/// </summary>
	public ReadOnlyDictionary<RecordId, LootRandomizerItem> AllLootRandomizerTranslated
	{
		get
		{
			var scope = this.Database.DataScope ?? string.Empty;
			if (!this._allLootRandomizerTranslated.TryGetValue(scope, out var translated))
			{
				var dico = Database.AllLootRandomizer.Select(r =>
				{
					string? translation = null;

					if (r.Value.TranslationTagIsEmpty)
					{
						if (USettings.LootTableDebugEnabled)
							Log.LogWarning(@"{RCLASS_LOOTRANDOMIZER} record ""{RecordId}"" dont have translation tag!", Data.Database.RCLASS_LOOTRANDOMIZER, r.Key);
					}
					else translation = TranslationService.TranslateXTag(r.Value.Tag).TQCleanup();// Get translation

					if (string.IsNullOrWhiteSpace(translation))
						translation = r.Value.FileDescription.TQCleanup();

					if (string.IsNullOrWhiteSpace(translation))
						translation = r.Key.PrettyFileName;

					return r.Value with
					{
						Translation = translation,
					};
				}).ToDictionary(v => v.Id);

				this._allLootRandomizerTranslated[scope] = translated = new ReadOnlyDictionary<RecordId, LootRandomizerItem>(dico);
			}

			return translated;
		}
	}

	private Dictionary<RecordId, (float Weight, LootRandomizerItem LootRandomizer)> MakeTable(RecordId tableId, DBRecordCollection records)
	{
		var Data = new Dictionary<RecordId, (float Weight, LootRandomizerItem LootRandomizer)>();

		#region Build Table

		const string randomizerName = "randomizerName", randomizerWeight = "randomizerWeight";
		var entries = records.GroupNumberedVariables(
			variable => !variable.Name.StartsWith(randomizerName, noCase) || !string.IsNullOrWhiteSpace(variable.GetString(0))
			, randomizerName, randomizerWeight);

		// Iterate the query to build the new unweighted table.
		foreach (var entry in entries.Values)
		{
			var affix = entry[0] is { } name ? name.GetString(0).ToRecordId() : RecordId.Empty;
			float weight = entry[1] is { } weightVariable ? WeightOf(weightVariable) : 0F;
			if (weight <= 0 || affix.IsEmpty)
				continue;

			// Check for a double entry in the table.
			if (Data.TryGetValue(affix, out var val))// DISTINCT
			{
				// for a double entry just add the chance.
				val.Weight += weight;
				Data[affix] = val;
				continue;
			}

			// get affix translations
			if (!this.AllLootRandomizerTranslated.TryGetValue(affix, out var lootrandom))
			{
				if (USettings.LootTableDebugEnabled)
					Log.LogError(@"Unknown affix record ""{RecordId}"" from table ""{TableId}"""
						, affix, tableId);

				lootrandom = LootRandomizerItem.Default(affix) with { Unknown = true };
			}

			Data.Add(affix, (weight, lootrandom));
		}

		#endregion

		return Data;
	}

	/// <summary>The weight of a table entry: its integer or float value, -1 for any other type.</summary>
	private static float WeightOf(Variable variable) => variable.DataType switch
	{
		VariableDataType.Integer => variable.GetInt32(0),
		VariableDataType.Float => variable.GetSingle(0),
		_ => -1.0F,
	};

	/// <summary>
	/// Builds the table from the database using the passed table Id.
	/// </summary>
	public LootTableCollection? LoadTable(RecordId tableId)
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
		else if (USettings.LootTableDebugEnabled)
		{
			Log.LogError(@"Unknown {RCLASS_LOOTRANDOMIZERTABLE} record ""{TableId}"""
				, Data.Database.RCLASS_LOOTRANDOMIZERTABLE, tableId);
		}

		return value;
	}
}
