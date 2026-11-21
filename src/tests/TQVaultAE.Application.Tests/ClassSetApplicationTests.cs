using AwesomeAssertions;
using TQVaultAE.Application.Character;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Tests.Application;

/// <summary><see cref="ClassSetApplication.Apply"/>: a class set applied to the masteries and the attributes of a build.</summary>
public class ClassSetApplicationTests
{
	private static readonly RecordId StormMastery = """Records\Skills\Storm\StormMastery.dbr""".ToRecordId();
	private static readonly RecordId IceShard = """Records\Skills\Storm\IceShard.dbr""".ToRecordId();
	private static readonly RecordId EarthMastery = """Records\Skills\Earth\EarthMastery.dbr""".ToRecordId();

	private static readonly MasteryTree Storm = new(Masteries.Storm, StormMastery, "tagStorm", "", "", 40,
		[new MasterySkill(IceShard, "tagIceShard", "", "", 12, 1, null)]);

	private static readonly MasteryTree Earth = new(Masteries.Earth, EarthMastery, "tagEarth", "", "", 40, []);

	private static MasteryTree? Trees(Masteries mastery) => mastery switch
	{
		Masteries.Storm => Storm,
		Masteries.Earth => Earth,
		_ => null,
	};

	/// <summary>Earth 6, 4 skill points left.</summary>
	private static CharacterSkillEditor EarthCharacter()
		=> new([new SkillRecord { skillName = EarthMastery.Raw, skillLevel = 6, skillEnabled = 1 }], 2, 4, Trees);

	/// <summary>Health 300 + 2 points, Energy 300, Strength 50 + 10 points, Intelligence 50, Dexterity 50 + 3 points: 15 points spent.</summary>
	private static readonly int[] Attributes = [380, 300, 90, 50, 62];

	[Fact]
	public void Apply_SetWithinThePoints_GivesItsMasteriesAndAttributesAndKeepsTheRestToSpend()
	{
		var skills = EarthCharacter();
		var set = new ClassSet("Storm", [new ClassSetMastery(Masteries.Storm, 8, [new(IceShard.Raw, 2)])], new ClassSetAttributes(0, 0, 0, 12, 0));

		var applied = ClassSetApplication.Apply(skills, Attributes, attributePoints: 5, set);

		applied.Complete.Should().BeTrue();
		skills.CaptureClassSet().Should().ContainSingle().Which.SameAs(set.Masteries[0]).Should().BeTrue();
		applied.Attributes.Should().Equal(300, 300, 50, 98, 50);
		applied.AttributePoints.Should().Be(8, "20 points (15 spent, 5 left), 12 given to intelligence");
	}

	[Fact]
	public void Apply_MorePointsThanTheCharacterHas_AppliesInPartAndLosesNoPoint()
	{
		var skills = EarthCharacter();
		var set = new ClassSet("Big", [], new ClassSetAttributes(0, 0, 30, 0, 0));

		var applied = ClassSetApplication.Apply(skills, Attributes, attributePoints: 5, set);

		applied.Complete.Should().BeFalse();
		applied.Attributes.Should().Equal([300, 300, 130, 50, 50], "the 20 points go to strength");
		applied.AttributePoints.Should().Be(0);
		skills.AvailablePoints.Should().Be(10, "the empty set gives Earth's 6 points back to the 4 left");
	}
}
