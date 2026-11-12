using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Data;

public partial class ItemProvider
{
	private const string RacialBonusRace = "racialBonusRace";
	private const string PetBonusName = "petBonusName";

	/// <summary>The suffix of the middle of a Min/Max range: <c>offensiveFireMin/Max</c> give <c>offensiveFireAverage</c>.</summary>
	public const string AverageSuffix = "Average";

	/// <inheritdoc/>
	public AttributeSum GetAttributeSums(Item item)
	{
		// Read in the item's own map, whatever the active one is (an item linked to another mod than its character's).
		using var modScope = this.ModScope(item);
		var sum = new AttributeSum();

		// The item and its affixes with the values the game rolled from the item seed (issue #552), then each relic or
		// charm at its number of pieces, and its completion bonus (relics never roll).
		DBRecordCollection? Read(RecordId? id) => RecordId.IsNullOrEmpty(id) ? null : Database.GetRecordFromFile(id);
		var baseRecord = Read(item.BaseItemId);
		var rolled = this.RollInGameValues(item, baseRecord);
		(DBRecordCollection? Record, int Level)[] sources =
		[
			(rolled?.Base ?? baseRecord, 1),
			(rolled is null ? Read(item.prefixID) : rolled.Prefix, 1),
			(rolled is null ? Read(item.suffixID) : rolled.Suffix, 1),
			(Read(item.relicID), item.Var1),
			(Read(item.RelicBonusId), 1),
			(Read(item.relic2ID), item.Var2),
			(Read(item.RelicBonus2Id), 1),
		];

		foreach (var (record, level) in sources)
			AddAttributesFromRecord(sum, record, level, this.PetBonusRecord);

		return sum;
	}

	/// <inheritdoc/>
	public AttributeSum GetSetBonuses(IEnumerable<Item> equipped)
	{
		var sum = new AttributeSum();

		// The game counts the pieces of each set the character wears and
		// reads the set record at that number of pieces.
		var pieces = equipped
			.Where(item => item.baseItemInfo is not null)
			.Select(item => (Set: item.baseItemInfo!.GetString("itemSetName"), Item: item))
			.Where(p => !string.IsNullOrWhiteSpace(p.Set))
			.GroupBy(p => p.Set, StringComparer.OrdinalIgnoreCase);

		foreach (var set in pieces)
		{
			int worn = set.Select(p => p.Item.BaseItemId).Distinct().Count();

			// The set record is read in the map of its pieces (the first one's when they are of several maps).
			using var modScope = this.ModScope(set.First().Item);
			AddAttributesFromRecord(sum, Database.GetRecordFromFile(set.Key.ToRecordId()), worn, this.PetBonusRecord);
		}

		return sum;
	}

	/// <inheritdoc/>
	public AttributeSum GetRecordAttributes(RecordId? recordId, int level)
	{
		var sum = new AttributeSum();
		if (!RecordId.IsNullOrEmpty(recordId))
			AddAttributesFromRecord(sum, Database.GetRecordFromFile(recordId), level, this.PetBonusRecord);

		return sum;
	}

	/// <summary>
	/// Adds the numeric variables of a record at a level to a sum, and its racial bonuses to their races. Arrays are read
	/// at <paramref name="level"/> - 1, the last value past their end (as the game does): a skill
	/// at level 12 with a single value keeps that value, a relic with 3 pieces reads the third one.
	/// </summary>
	/// <param name="level">The skill level, the number of relic pieces, the number of set pieces; 1 for an item.</param>
	/// <param name="petBonus">
	/// Reads the pet bonus record a record names (<c>petBonusName</c>): its variables are added at the same level, under
	/// <see cref="AttributeSum.PetBonusPrefix"/>, as the game's Pet Bonuses rollover sums them (items, relics at
	/// their completion, active skills at their level). Null to leave pet bonuses out.
	/// </param>
	internal static void AddAttributesFromRecord(AttributeSum sum, DBRecordCollection? record, int level, Func<string, DBRecordCollection?>? petBonus = null)
	{
		if (record is null)
			return;

		foreach (var variable in record)
		{
			if (variable.NumberOfValues == 0 || variable.DataType is not (VariableDataType.Integer or VariableDataType.Float))
				continue;

			double chance = ChanceMultiplier(record, variable.Name, level);
			sum.Add(variable.Name, ValueAt(variable, level) * chance);

			// A range counts at its middle in the window (the display accumulator's random generator): the max is
			// min + max(0, max - min), so a missing or lower max is the min. Kept per record as <base>Average.
			if (variable.Name.EndsWith("Min", StringComparison.OrdinalIgnoreCase))
			{
				var baseName = variable.Name[..^3];
				double min = ValueAt(variable, level);
				double max = Math.Max(min, ValueAt(record[baseName + "Max"], level));
				sum.Add(baseName + AverageSuffix, (min + max) / 2 * chance);
			}
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

		if (petBonus is not null && record.GetString(PetBonusName, 0) is { Length: > 0 } petBonusId)
		{
			var pets = new AttributeSum();
			AddAttributesFromRecord(pets, petBonus(petBonusId), level);
			foreach (var (name, value) in pets.Values)
				sum.Add(AttributeSum.PetBonusPrefix + name, value);
		}
	}

	private DBRecordCollection? PetBonusRecord(string id) => Database.GetRecordFromFile(id.ToRecordId());

	/// <summary>
	/// What a chance-based bonus counts in the character window: the game's display accumulator always lets the chance
	/// pass and scales the value by chance / 100, 1 when the
	/// chance is 0 or 100 and more. <c>defensiveFire</c> goes with <c>defensiveFireChance</c>,
	/// <c>defensiveFireModifier</c> with <c>defensiveFireModifierChance</c>, <c>offensiveBonusPhysicalMin/Max</c> with
	/// <c>offensiveBonusPhysicalChance</c>; durations are not scaled.
	/// </summary>
	internal static double ChanceMultiplier(DBRecordCollection record, string name, int level)
	{
		if (name.EndsWith("Chance", StringComparison.OrdinalIgnoreCase)
			|| name.EndsWith("DurationMin", StringComparison.OrdinalIgnoreCase)
			|| name.EndsWith("DurationMax", StringComparison.OrdinalIgnoreCase))
			return 1;

		var chance = record[name + "Chance"];
		if (chance is null && (name.EndsWith("Min", StringComparison.OrdinalIgnoreCase) || name.EndsWith("Max", StringComparison.OrdinalIgnoreCase)))
			chance = record[string.Concat(name.AsSpan(0, name.Length - 3), "Chance")];

		double percent = ValueAt(chance, level);
		return percent is > 0 and < 100 ? percent / 100 : 1;
	}

	private static double ValueAt(Variable? variable, int level)
	{
		if (variable is null || variable.NumberOfValues == 0)
			return 0;

		int index = Math.Clamp(level, 1, variable.NumberOfValues) - 1;
		return variable.DataType == VariableDataType.Float ? variable.GetSingle(index) : variable.GetInt32(index);
	}
}
