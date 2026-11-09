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
