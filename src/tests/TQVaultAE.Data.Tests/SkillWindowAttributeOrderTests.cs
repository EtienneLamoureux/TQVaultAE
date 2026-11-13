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
	public void CharacterAttributes_FollowTheSkillLoadOrder_MovementBeforeAttackAndCastingSpeed()
		// A skill loads its character attributes in its own order: Offensive Ability before the speeds (Weapon Training),
		// and the movement speed before the attack and casting speeds (Onslaught's Ardor), unlike an item.
		=> Ordered("characterSpellCastSpeedModifier", "characterAttackSpeedModifier", "characterRunSpeedModifier", "characterOffensiveAbility")
			.Should().Equal("characterOffensiveAbility", "characterRunSpeedModifier", "characterAttackSpeedModifier", "characterSpellCastSpeedModifier");

	[Fact]
	public void EnergyReserved_HasItsOwnStore_AfterTheCharacterAttributes_BeforeTheSkillAttributes()
		// Battle Awareness: "+25 Defensive Ability", "75 Energy Reserved", then its armor (defense store).
		=> Ordered("defensiveProtection", "skillCooldownReduction", "characterManaLimitReserve", "characterDefensiveAbility", "characterTotalSpeedModifier")
			.Should().Equal("characterDefensiveAbility", "characterTotalSpeedModifier", "characterManaLimitReserve", "skillCooldownReduction", "defensiveProtection");

	[Fact]
	public void LastOffensiveAttributes_FollowTheSkillLoadOrder()
		// Club Slam: impaired aim, then skill disruption before the stun; then trap, freeze, petrify, taunt, convert, fear,
		// confusion and the slowed speeds, attack before movement.
		=> Ordered("offensiveSlowRunSpeedMin", "offensiveSlowAttackSpeedMin", "offensiveConfusionMin", "offensiveFearMin", "offensiveConvertMin",
				"offensiveTauntMin", "offensivePetrifyMin", "offensiveFreezeMin", "offensiveTrapMin", "offensiveStunMin", "offensiveDisruptionMin",
				"offensiveProjectileFumbleMin")
			.Should().Equal("offensiveProjectileFumbleMin", "offensiveDisruptionMin", "offensiveStunMin", "offensiveTrapMin", "offensiveFreezeMin",
				"offensivePetrifyMin", "offensiveTauntMin", "offensiveConvertMin", "offensiveFearMin", "offensiveConfusionMin",
				"offensiveSlowAttackSpeedMin", "offensiveSlowRunSpeedMin");

	[Fact]
	public void UnknownVariable_ComesLast()
		=> Rank("someUnknownVariable").Should().Be(SkillWindowAttributeOrder.Other);
}
