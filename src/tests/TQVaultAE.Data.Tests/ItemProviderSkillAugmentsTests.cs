using AwesomeAssertions;
using TQVaultAE.Data;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Tests.Data;

public class ItemProviderSkillAugmentsTests
{
	private const string IceShard = """Records\Skills\Storm\IceShard.dbr""";
	private const string StormMastery = """Records\Skills\Storm\StormMastery.dbr""";

	private static DBRecordCollection Record(params (string Name, object[] Values)[] vars)
	{
		var record = new DBRecordCollection("""records\item\test.dbr""".ToRecordId(), "test");
		foreach (var (name, values) in vars)
		{
			var variable = new Variable(name, values[0] is string ? VariableDataType.StringVar : VariableDataType.Integer, values.Length);
			for (int i = 0; i < values.Length; i++)
				variable[i] = values[i];
			record.Set(variable);
		}

		return record;
	}

	private static List<SkillAugment> Augments(DBRecordCollection record, int level = 1)
	{
		var augments = new List<SkillAugment>();
		ItemProvider.AddSkillAugments(augments, record, level);
		return augments;
	}

	[Fact]
	public void AddSkillAugments_SkillMasteryAndAllBonuses_AreReadWithTheirTargets()
	{
		var record = Record(
			("augmentSkillName1", [IceShard]), ("augmentSkillLevel1", [2]),
			("augmentMasteryName2", [StormMastery]), ("augmentMasteryLevel2", [1]),
			("augmentAllLevel", [3]),
			("characterStrength", [20]));

		Augments(record).Should().BeEquivalentTo(new[]
		{
			new SkillAugment(SkillAugmentKind.Skill, IceShard.ToRecordId(), 2),
			new SkillAugment(SkillAugmentKind.Mastery, StormMastery.ToRecordId(), 1),
			new SkillAugment(SkillAugmentKind.All, null, 3),
		});
	}

	[Fact]
	public void AddSkillAugments_BonusWithoutTargetOrZero_IsDropped()
	{
		// The game drops a mastery bonus whose name is empty, like the Clashing Suit.
		var record = Record(
			("augmentMasteryName1", [""]), ("augmentMasteryLevel1", [1]),
			("augmentSkillName2", [IceShard]), ("augmentSkillLevel2", [0]));

		Augments(record).Should().BeEmpty();
	}

	[Theory]
	[InlineData(0, 1)]
	[InlineData(2, 2)]
	[InlineData(3, 3)]
	[InlineData(9, 3)]
	public void AddSkillAugments_RelicLevels_TakeTheValueOfItsNumberOfPieces(int pieces, int expected)
	{
		var relic = Record(("augmentAllLevel", [1, 2, 3]));

		Augments(relic, pieces).Should().ContainSingle().Which.Levels.Should().Be(expected);
	}
}
