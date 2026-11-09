using AwesomeAssertions;
using TQVaultAE.Data;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Tests.Data;

public class ItemProviderAttributeSumsTests
{
	private static DBRecordCollection Record(params (string Name, object[] Values)[] vars)
	{
		var record = new DBRecordCollection("""records\item\test.dbr""".ToRecordId(), "test");
		foreach (var (name, values) in vars)
		{
			var type = values[0] switch
			{
				string => VariableDataType.StringVar,
				float => VariableDataType.Float,
				bool => VariableDataType.Boolean,
				_ => VariableDataType.Integer,
			};
			var variable = new Variable(name, type, values.Length);
			for (int i = 0; i < values.Length; i++)
				variable[i] = values[i] is bool b ? (b ? 1 : 0) : values[i];
			record.Set(variable);
		}

		return record;
	}

	private static AttributeSum Sum(DBRecordCollection record, int level = 1)
	{
		var sum = new AttributeSum();
		ItemProvider.AddAttributesFromRecord(sum, record, level);
		return sum;
	}

	[Fact]
	public void AddAttributesFromRecord_SumsIntegersAndFloats_IgnoresStringsAndBooleans()
	{
		var record = Record(
			("defensiveFire", [20]), ("defensiveCold", [12.5F]),
			("itemSetName", ["""records\item\sets\set001.dbr"""]), ("offensiveFireGlobal", [true]));

		var sum = Sum(record);

		sum["DEFENSIVEFIRE"].Should().Be(20);
		sum["defensiveCold"].Should().Be(12.5);
		sum.Values.Should().HaveCount(2);
	}

	[Theory]
	[InlineData(1, 0)]
	[InlineData(3, 20)]
	[InlineData(6, 30)]
	[InlineData(9, 30)]
	public void AddAttributesFromRecord_Arrays_ReadTheLevelThenTheLastValue(int level, int expected)
	{
		// A set record: nothing with one piece, the bonus rises with the pieces worn, the last value past the end.
		var record = Record(("characterStrength", [0, 10, 20, 30, 30, 30]));

		Sum(record, level)["characterStrength"].Should().Be(expected);
	}

	[Fact]
	public void AddAttributesFromRecord_SingleValueOfASkill_KeepsItsValueAtAnyLevel()
	{
		// The 5-attribute path skipped a single value past level 1; the game reads it at any level.
		Sum(Record(("defensivePoison", [15])), 7)["defensivePoison"].Should().Be(15);
	}

	[Fact]
	public void AddAttributesFromRecord_ChanceBonuses_CountValueTimesChance()
	{
		// The window counts a chance-based bonus at its expected value; durations are not scaled.
		var record = Record(
			("defensiveFire", [40]), ("defensiveFireChance", [25F]),
			("defensiveColdModifier", [20]), ("defensiveColdModifierChance", [50F]),
			("offensiveBonusPhysicalMin", [100]), ("offensiveBonusPhysicalMax", [300]), ("offensiveBonusPhysicalChance", [23F]),
			("offensiveSlowFireMin", [10]), ("offensiveSlowFireDurationMin", [3]), ("offensiveSlowFireChance", [50F]));

		var sum = Sum(record);

		sum["defensiveFire"].Should().Be(10);
		sum["defensiveColdModifier"].Should().Be(10);
		sum["offensiveBonusPhysicalMin"].Should().BeApproximately(23, 1e-9);
		sum["offensiveBonusPhysicalMax"].Should().BeApproximately(69, 1e-9);
		sum["offensiveSlowFireMin"].Should().Be(5);
		sum["offensiveSlowFireDurationMin"].Should().Be(3);
	}

	[Fact]
	public void AddAttributesFromRecord_Ranges_AddTheirMiddlePerRecord()
	{
		// A missing or lower max is the min; the chance scales the middle too.
		var sum = Sum(Record(("offensiveFireMin", [10F]), ("offensiveFireMax", [20F])));
		ItemProvider.AddAttributesFromRecord(sum, Record(("offensiveFireMin", [5F])), 1);
		ItemProvider.AddAttributesFromRecord(sum, Record(("offensiveFireMin", [8F]), ("offensiveFireMax", [2F]), ("offensiveFireChance", [50F])), 1);

		sum["offensiveFireAverage"].Should().Be(15 + 5 + 4);
	}

	[Theory]
	[InlineData(0F, 30)]
	[InlineData(100F, 30)]
	[InlineData(150F, 30)]
	public void AddAttributesFromRecord_ChanceZeroOrFull_CountsTheWholeValue(float chance, double expected)
		=> Sum(Record(("defensivePoison", [30]), ("defensivePoisonChance", [chance])))["defensivePoison"].Should().Be(expected);

	[Fact]
	public void AddAttributesFromRecord_ChanceArray_IsReadAtTheLevel()
		=> Sum(Record(("defensiveStun", [20, 40]), ("defensiveStunChance", [10F, 50F])), 2)["defensiveStun"].Should().Be(20);

	[Fact]
	public void AddAttributesFromRecord_RacialBonus_GoesToEachRaceOfTheRecord()
	{
		var record = Record(
			("racialBonusRace", ["Demon", "Magical"]),
			("racialBonusPercentDefense", [24]), ("racialBonusAbsoluteDamage", [51]));
		var other = Record(("racialBonusRace", ["Demon"]), ("racialBonusPercentDamage", [10, 16, 22]));

		var sum = Sum(record);
		ItemProvider.AddAttributesFromRecord(sum, other, 2);

		sum.Races["demon"].Should().Be(new RacialBonus(51, 16, 24));
		sum.Races["Magical"].Should().Be(new RacialBonus(51, 0, 24));
	}
}
