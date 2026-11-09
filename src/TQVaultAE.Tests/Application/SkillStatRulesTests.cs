using AwesomeAssertions;
using TQVaultAE.Application;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Tests.Application;

public class SkillStatRulesTests
{
	private static readonly RecordId StormMastery = """Records\Skills\Storm\StormMastery.dbr""".ToRecordId();
	private static readonly RecordId IceShard = """Records\Skills\Storm\IceShard.dbr""".ToRecordId();
	private static readonly RecordId QuestSkill = """Records\Quests\Rewards\QuestRewardResistances.dbr""".ToRecordId();

	private static readonly MasteryTree Storm = new(Masteries.Storm, StormMastery, "tagStorm", "", "", 40,
	[
		new MasterySkill(IceShard, "tagIceShard", "", "", 12, 1, null),
	]);

	[Theory]
	[InlineData("Skill_Passive", SkillStatRole.Passive)]
	[InlineData("Skill_Mastery", SkillStatRole.Passive)]
	[InlineData("Skill_Modifier", SkillStatRole.Modifier)]
	[InlineData("Skill_BuffRadiusToggled", SkillStatRole.Buff)]
	[InlineData("Skill_BuffSelfDuration", SkillStatRole.Buff)]
	[InlineData("Skill_PassiveOnHitBuffSelf", SkillStatRole.Buff)]
	[InlineData("Skill_PassiveOnTargetKilled", SkillStatRole.Buff)]
	[InlineData("SkillBuff_Debuf", SkillStatRole.None)]
	[InlineData("Skill_AttackProjectile", SkillStatRole.None)]
	[InlineData("", SkillStatRole.None)]
	public void RoleOf_RecordClass_TellsWhenTheSkillCounts(string recordClass, SkillStatRole expected)
		=> SkillStatRules.RoleOf(recordClass).Should().Be(expected);

	[Theory]
	[InlineData(SkillStatRole.Passive, SkillStatRole.Passive)]
	[InlineData(SkillStatRole.Buff, SkillStatRole.Buff)]
	[InlineData(SkillStatRole.None, SkillStatRole.None)]
	[InlineData(null, SkillStatRole.None)]
	public void Resolve_Modifier_TakesTheRoleOfItsParent(SkillStatRole? parent, SkillStatRole expected)
		=> SkillStatRules.Resolve(SkillStatRole.Modifier, parent).Should().Be(expected);

	[Theory]
	[InlineData(SkillStatRole.Passive, false, true)]
	[InlineData(SkillStatRole.Buff, false, false)]
	[InlineData(SkillStatRole.Buff, true, true)]
	[InlineData(SkillStatRole.None, true, false)]
	public void Counts_BuffsOnlyWhenAsked(SkillStatRole role, bool includeBuffs, bool expected)
		=> SkillStatRules.Counts(role, includeBuffs).Should().Be(expected);

	[Fact]
	public void EffectiveLevel_SkillOfAMastery_AddsSkillMasteryAndAllBonuses()
	{
		SkillAugment[] augments =
		[
			new(SkillAugmentKind.Skill, IceShard, 2),
			new(SkillAugmentKind.Mastery, StormMastery, 1),
			new(SkillAugmentKind.All, null, 3),
		];

		SkillStatRules.EffectiveLevel(5, IceShard, Storm, augments).Should().Be(11);
		SkillStatRules.EffectiveLevel(32, StormMastery, Storm, augments).Should().Be(32, "the mastery itself gets no item level");
		SkillStatRules.EffectiveLevel(0, IceShard, Storm, augments).Should().Be(0, "item levels need a point spent");
	}

	[Fact]
	public void EffectiveLevel_SkillOutsideTheMasteries_GetsItsOwnAndAllBonuses()
	{
		SkillAugment[] augments = [new(SkillAugmentKind.Mastery, StormMastery, 1), new(SkillAugmentKind.All, null, 2)];

		SkillStatRules.EffectiveLevel(1, QuestSkill, null, augments).Should().Be(3);
	}
}
