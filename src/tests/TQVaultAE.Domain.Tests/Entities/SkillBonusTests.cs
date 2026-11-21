using AwesomeAssertions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Tests.Entities;

public class SkillBonusTests
{
	private static readonly RecordId StormMastery = """Records\Skills\Storm\StormMastery.dbr""".ToRecordId();
	private static readonly RecordId EarthMastery = """Records\Skills\Earth\EarthMastery.dbr""".ToRecordId();
	private static readonly RecordId IceShard = """Records\Skills\Storm\IceShard.dbr""".ToRecordId();
	private static readonly RecordId Squall = """Records\Skills\Storm\Squall.dbr""".ToRecordId();

	private static readonly MasteryTree Storm = new(Masteries.Storm, StormMastery, "", "", "", 40,
	[
		new MasterySkill(IceShard, "", "", "", 12, 1, null),
		new MasterySkill(Squall, "", "", "", 12, 2, null),
	]);

	/// <summary>A helm with +2 Ice Shard and +1 Storm, a ring with +1 to all skills and +3 Squall, an amulet with +2 Earth.</summary>
	private static readonly SkillAugment[] Gear =
	[
		new(SkillAugmentKind.Skill, IceShard, 2),
		new(SkillAugmentKind.Mastery, StormMastery, 1),
		new(SkillAugmentKind.All, null, 1),
		new(SkillAugmentKind.Skill, Squall, 3),
		new(SkillAugmentKind.Mastery, EarthMastery, 2),
		new(SkillAugmentKind.Skill, IceShard, 1),
	];

	[Fact]
	public void For_SkillOfTheMastery_SumsItsOwnTheMasteryAndTheAllSkillsBonuses()
	{
		var bonus = SkillBonus.For(Gear, Storm, IceShard);

		bonus.Should().Be(new SkillBonus(Skill: 3, Mastery: 1, All: 1));
		bonus.Total.Should().Be(5);
	}

	[Fact]
	public void For_OtherSkill_IgnoresTheBonusesOfIceShardAndOfOtherMasteries()
		=> SkillBonus.For(Gear, Storm, Squall).Should().Be(new SkillBonus(Skill: 3, Mastery: 1, All: 1));

	[Fact]
	public void For_TheMasteryItself_GetsNoBonus()
		=> SkillBonus.For(Gear, Storm, StormMastery).Total.Should().Be(0);

	[Fact]
	public void For_NoGear_IsZero()
		=> SkillBonus.For([], Storm, IceShard).Total.Should().Be(0);
}
