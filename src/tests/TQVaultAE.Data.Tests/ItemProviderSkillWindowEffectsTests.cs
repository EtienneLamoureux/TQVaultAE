using AwesomeAssertions;
using TQVaultAE.Application;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Data;

/// <summary>
/// Tests the effects of a skill at a level, as the game's skill window writes them: what the game does not load or
/// show, the values at the level shown, and the lines the skill window writes itself.
/// </summary>
public class ItemProviderSkillWindowEffectsTests : ItemProviderTextTestBase
{
	private const string SkillId = """records\test\skill.dbr""";

	public ItemProviderSkillWindowEffectsTests()
	{
		this.Translations["tagSkillTest"] = "Test Skill";
		this.Translations["DamageTotalResistanceReductionAbsolute"] = "Reduced Resistances";
		this.Translations["DamageLife"] = "Vitality Damage";
		this.Translations["DamageFear"] = "second(s) of Fear";
		this.Translations["characterRunSpeedModifier"] = "{%+.0f0}% Movement Speed";
		this.Translations["characterAttackSpeedModifier"] = "{%+.0f0}% Attack Speed";
		this.Translations["characterDefensiveAbility"] = "{%+.0f0} Defensive Ability";
		this.Translations["characterManaLimitReserve"] = "{%.0f0} Energy Reserved";
		this.Translations["SpawnObjectsTimeToLive"] = "Extra Life Time {%.0f} Seconds";
		this.Translations["SkillChanceWeightFloat"] = "{%.1f0}% Chance to be Used";
		this.Translations["GlobalChanceOfOneTag"] = "Chance for one of the following:";
		this.Translations["DamageCold"] = "Cold Damage";
		this.Translations["DamageLightning"] = "Lightning Damage";
	}

	private string[] Effects(int level, params Variable[] variables)
	{
		this.CreateRecord(SkillId, "Skill_Modifier", [Str("skillDisplayName", "tagSkillTest"), .. variables]);
		return [.. this.Provider.GetSkillAtLevel(SkillId, level, SkillWindowContext.None)!.Attributes.Select(a => a.RemoveLeadingColorTag())];
	}

	[Fact]
	public void ResistanceReduction_WithoutDuration_IsNotShown()
		// Doom Horn: the game loads an effect over time only with its duration; its "...AbsoluteMax 8" is no duration.
		=> this.Effects(1, Floats("offensiveTotalResistanceReductionAbsoluteMin", 25f, 29f), Float("offensiveTotalResistanceReductionAbsoluteMax", 8f))
			.Should().BeEmpty();

	[Fact]
	public void Max_BelowTheMin_IsASingleValue()
		// The game keeps max(0, max - min) above the min: "25 ~ 8" reads "25".
		=> this.Effects(1, Floats("offensiveTotalResistanceReductionAbsoluteMin", 25f, 29f), Float("offensiveTotalResistanceReductionAbsoluteMax", 8f),
				Float("offensiveTotalResistanceReductionAbsoluteDurationMin", 5f))
			.Should().Equal("25 Reduced Resistances for 5.0 Seconds");

	[Fact]
	public void Chance_WithoutItsDamage_IsNotShown()
		// Frightening Power wrote "Vitality Damage Chance: 30.000000, 46.000000, ..." for an offensiveLifeChance alone.
		=> this.Effects(1, Floats("offensiveLifeChance", 30f, 46f)).Should().BeEmpty();

	[Theory]
	[InlineData(1, new string[0])]
	[InlineData(2, new[] { "10.0% Chance of 1.0 second(s) of Fear", "+10% Movement Speed" })]
	public void Effect_ZeroAtTheLevel_IsNotShown(int level, string[] shown)
		// Active Block level 1 showed "+0% Movement Speed"; Poison Gas Bomb's Spread "0.0% Chance of 0.0 second(s) of Fear".
		=> this.Effects(level, Floats("characterRunSpeedModifier", 0f, 10f), Floats("offensiveFearMin", 0f, 1f), Floats("offensiveFearChance", 0f, 10f))
			.Should().Equal(shown);

	[Theory]
	[InlineData(1, "20 Burn Damage over 2.0 Seconds")]
	[InlineData(2, "30 Burn Damage over 3.0 Seconds")]
	public void DamageOverTime_TakesTheDurationOfTheLevelShown(int level, string shown)
		// The total was multiplied by the last level's duration (3 s) at every level.
		=> this.Effects(level, Float("offensiveSlowFireMin", 10f), Floats("offensiveSlowFireDurationMin", 2f, 3f))
			.Should().Equal(shown);

	[Fact]
	public void Modifier_WritesTheLifeTimeItAdds()
		// Lasting Legacy, a modifier of Ancestral Horn, has only spawnObjectsTimeToLive: its blocks were empty.
		=> this.Effects(2, Floats("spawnObjectsTimeToLive", 4f, 7f)).Should().Equal("Extra Life Time 7 Seconds");

	[Fact]
	public void CharacterAttributes_FollowTheSkillOrder_ThenTheEnergyReserved()
		=> this.Effects(1, Int("characterManaLimitReserve", 75), Float("characterAttackSpeedModifier", 5f), Float("characterRunSpeedModifier", 6f),
				Float("characterDefensiveAbility", 25f))
			.Should().Equal("+25 Defensive Ability", "+6% Movement Speed", "+5% Attack Speed", "75 Energy Reserved");

	[Theory]
	[InlineData(1, "40 Fire Damage", "41 Cold Damage", "42 Lightning Damage")]
	[InlineData(2, "54 Fire Damage", "55 Cold Damage", "56 Lightning Damage")]
	public void SkillCastOnDamageType_ShowsTheChance_AndTheDamageOfEachCastSkill(int level, string fire, string cold, string lightning)
	{
		// Echoes of the Ancestors casts a fire, cold or lightning blast on a kill: its blocks were empty.
		this.CreateRecord("""records\test\fire.dbr""", "Skill_AttackRadius", Floats("offensiveFireMin", 40f, 54.6f));
		this.CreateRecord("""records\test\cold.dbr""", "Skill_AttackRadius", Floats("offensiveColdMin", 41f, 55f));
		this.CreateRecord("""records\test\lightning.dbr""", "Skill_AttackRadius", Floats("offensiveLightningMin", 42f, 56f));
		this.CreateRecord(SkillId, "Skill_PassiveOnTargetKilled", Str("skillDisplayName", "tagSkillTest"), Floats("skillCastChance", 33f, 33f),
			Str("skillNameCastOnDamage_Fire", """records\test\fire.dbr"""), Str("skillNameCastOnDamage_Cold", """records\test\cold.dbr"""),
			Str("skillNameCastOnDamage_Lightning", """records\test\lightning.dbr"""));

		var skill = this.Provider.GetSkillAtLevel(SkillId, level, SkillWindowContext.None);

		skill!.Attributes.Select(a => a.RemoveLeadingColorTag()).Should().Equal(
			"33.0% Chance to be Used", string.Empty, "Chance for one of the following:", fire, cold, lightning);
	}

	[Fact]
	public void SkillCastOnDamageType_TriggeredByASkill_NamesItAfterTheChance()
	{
		// Echoes of the Ancestors triggers on Shen Pao's kills: the game writes "33.0% Chance to be Used : Shen Pao".
		this.Translations["tagShenPao"] = "Shen Pao";
		this.CreateRecord("""records\test\shenpao.dbr""", "Skill_AttackProjectile", Str("skillDisplayName", "tagShenPao"));
		this.CreateRecord("""records\test\fire.dbr""", "Skill_AttackRadius", Floats("offensiveFireMin", 40f, 54.6f));
		this.CreateRecord(SkillId, "Skill_PassiveOnTargetKilled", Str("skillDisplayName", "tagSkillTest"), Floats("skillCastChance", 33f, 33f),
			Str("skillNameCastOnDamage_Fire", """records\test\fire.dbr"""), Str("triggeringSkill", """records\test\shenpao.dbr"""));

		var skill = this.Provider.GetSkillAtLevel(SkillId, 2, SkillWindowContext.None);

		skill!.Attributes.Select(a => a.RemoveLeadingColorTag()).Should().Equal(
			"33.0% Chance to be Used : Shen Pao", string.Empty, "Chance for one of the following:", "54 Fire Damage");
	}

	[Fact]
	public void SkillCastOnDamageType_TriggeringSkillMissing_WritesTheChanceAlone()
	{
		this.CreateRecord("""records\test\fire.dbr""", "Skill_AttackRadius", Floats("offensiveFireMin", 40f, 54.6f));
		this.CreateRecord(SkillId, "Skill_PassiveOnTargetKilled", Str("skillDisplayName", "tagSkillTest"), Floats("skillCastChance", 33f, 33f),
			Str("skillNameCastOnDamage_Fire", """records\test\fire.dbr"""), Str("triggeringSkill", """records\test\missing.dbr"""));

		var skill = this.Provider.GetSkillAtLevel(SkillId, 1, SkillWindowContext.None);

		skill!.Attributes.Select(a => a.RemoveLeadingColorTag()).First().Should().Be("33.0% Chance to be Used");
	}
}
