using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Data;

public partial class ItemProvider
{
	private const string RacialBonusRace = "racialBonusRace";

	/// <inheritdoc/>
	public AttributeSum GetAttributeSums(Item item)
	{
		var sum = new AttributeSum();

		// The item and its affixes, then each relic or charm at its number of pieces, and its completion bonus.
		(RecordId? Id, int Level)[] sources =
		[
			(item.BaseItemId, 1),
			(item.prefixID, 1),
			(item.suffixID, 1),
			(item.relicID, item.Var1),
			(item.RelicBonusId, 1),
			(item.relic2ID, item.Var2),
			(item.RelicBonus2Id, 1),
		];

		foreach (var (id, level) in sources)
		{
			if (!RecordId.IsNullOrEmpty(id))
				AddAttributesFromRecord(sum, Database.GetRecordFromFile(id), level);
		}

		return sum;
	}

	/// <inheritdoc/>
	public AttributeSum GetSetBonuses(IEnumerable<Item> equipped)
	{
		var sum = new AttributeSum();

		// The game counts the pieces of each set the character wears (the game) and
		// reads the set record at that number of pieces.
		var pieces = equipped
			.Where(item => item.baseItemInfo is not null)
			.Select(item => (Set: item.baseItemInfo!.GetString("itemSetName"), Item: item.BaseItemId))
			.Where(p => !string.IsNullOrWhiteSpace(p.Set))
			.GroupBy(p => p.Set, StringComparer.OrdinalIgnoreCase);

		foreach (var set in pieces)
		{
			int worn = set.Select(p => p.Item).Distinct().Count();
			AddAttributesFromRecord(sum, Database.GetRecordFromFile(set.Key.ToRecordId()), worn);
		}

		return sum;
	}

	/// <inheritdoc/>
	public AttributeSum GetRecordAttributes(RecordId? recordId, int level)
	{
		var sum = new AttributeSum();
		if (!RecordId.IsNullOrEmpty(recordId))
			AddAttributesFromRecord(sum, Database.GetRecordFromFile(recordId), level);

		return sum;
	}

	/// <summary>
	/// Adds the numeric variables of a record at a level to a sum, and its racial bonuses to their races. Arrays are read
	/// at <paramref name="level"/> - 1, the last value past their end (as <c>GetProtection</c> in the game does): a skill
	/// at level 12 with a single value keeps that value, a relic with 3 pieces reads the third one.
	/// </summary>
	/// <param name="level">The skill level, the number of relic pieces, the number of set pieces; 1 for an item.</param>
	internal static void AddAttributesFromRecord(AttributeSum sum, DBRecordCollection? record, int level)
	{
		if (record is null)
			return;

		foreach (var variable in record)
		{
			if (variable.NumberOfValues == 0 || variable.DataType is not (VariableDataType.Integer or VariableDataType.Float))
				continue;

			sum.Add(variable.Name, ValueAt(variable, level));
		}

		if (record.GetAllStrings(RacialBonusRace) is { Length: > 0 } races)
		{
			var bonus = new RacialBonus(
				ValueAt(record["racialBonusAbsoluteDamage"], level),
				ValueAt(record["racialBonusPercentDamage"], level),
				ValueAt(record["racialBonusPercentDefense"], level));
			foreach (var race in races.Where(r => !string.IsNullOrWhiteSpace(r)).Distinct(StringComparer.OrdinalIgnoreCase))
				sum.AddRace(race, bonus);
		}
	}

	private static double ValueAt(Variable? variable, int level)
	{
		if (variable is null || variable.NumberOfValues == 0)
			return 0;

		int index = Math.Clamp(level, 1, variable.NumberOfValues) - 1;
		return variable.DataType == VariableDataType.Float ? variable.GetSingle(index) : variable.GetInt32(index);
	}
}
