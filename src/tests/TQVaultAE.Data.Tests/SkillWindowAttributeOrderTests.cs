using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using TQVaultAE.Data;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Data;

public class SkillWindowAttributeOrderTests
{
	private static readonly ItemAttributeProvider Attributes = new(NullLogger<ItemAttributeProvider>.Instance);

	private static SkillWindowRank Rank(string variable)
		=> SkillWindowAttributeOrder.RankOf(variable, Attributes.GetAttributeData(variable));

	private static string[] Ordered(params string[] variables)
		=> [.. variables.OrderBy(Rank)];

	[Fact]
	public void BattleStandard_OwnEffects_ThenOffense_ThenCharacter_ThenSkill()
		// The game's Battle Standard buff: duration, radius, damage absorption, +1 to all skills, damage, Offensive
		// Ability, energy cost reduction.
		=> Ordered("skillManaCostReduction", "characterOffensiveAbility", "offensivePhysicalMin", "allSkillEnhancement",
				"damageAbsorptionPercent", "skillTargetRadius", "skillActiveDuration")
			.Should().Equal("skillActiveDuration", "skillTargetRadius", "damageAbsorptionPercent", "allSkillEnhancement",
				"offensivePhysicalMin", "characterOffensiveAbility", "skillManaCostReduction");

	[Fact]
	public void PercentBonuses_ComeBeforeFlatDamage()
		// Earth Enchantment: "+44% Fire Damage", "+44% Burn Damage", "5 ~ 8 Fire Damage"; Psionic Touch: "+108% Total
		// Damage", "112 Vitality Damage".
		=> Ordered("offensiveFireMin", "offensiveSlowFireModifier", "offensiveFireModifier", "offensiveLifeMin", "offensiveTotalDamageModifier")
			.Should().Equal("offensiveTotalDamageModifier", "offensiveFireModifier", "offensiveSlowFireModifier", "offensiveFireMin", "offensiveLifeMin");

	[Fact]
	public void Debuffs_FollowTheGameTypeOrder()
		// Hamstring: reduced Defensive Ability, reduced armor, then slower movement.
		=> Ordered("offensiveSlowRunSpeedMin", "offensiveSlowDefensiveReductionMin", "offensiveSlowDefensiveAbilityMin")
			.Should().Equal("offensiveSlowDefensiveAbilityMin", "offensiveSlowDefensiveReductionMin", "offensiveSlowRunSpeedMin");

	[Fact]
	public void Recharge_ThenChances_ThenParameters_ThenSummon()
		=> Ordered("petLimit", "spawnObjectsTimeToLive", "skillTargetRadius", "skillManaCost", "onHitActivationChance", "skillCooldownTime")
			.Should().Equal("skillCooldownTime", "onHitActivationChance", "skillManaCost", "skillTargetRadius", "spawnObjectsTimeToLive", "petLimit");

	[Fact]
	public void Stores_OffenseCharacterSkillRetaliationDefense()
		=> Ordered("defensiveFire", "retaliationPhysicalMin", "skillCooldownReduction", "characterStrength", "offensiveColdMin")
			.Should().Equal("offensiveColdMin", "characterStrength", "skillCooldownReduction", "retaliationPhysicalMin", "defensiveFire");

	[Fact]
	public void UnknownVariable_ComesLast()
		=> Rank("someUnknownVariable").Should().Be(SkillWindowAttributeOrder.Other);
}
