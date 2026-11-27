//-----------------------------------------------------------------------
// <copyright file="EquipmentRollTable.Data.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace TQVaultAE.Domain.Jitter;

// Roll order and rolled fields of every equipment attribute, as the game rolls an item's stats from its seed.
// Rolled fields per kind of attribute, in the game's order:
//   Character and skill attributes: the single value (affixes always roll, the base value at its store's percent).
//   Absolute and duration damage: per level (min, max - min); durations never roll.
//   Damage modifiers: the modifier. Duration damage modifiers: the modifier, then the duration modifier.
//   Base physical damage: (min, whole (max - min)) pairs, then offensivePierceRatio.
//   Defenses: the value only (the *Chance variable never rolls).

public static partial class EquipmentRollTable
{
	private static partial RollAttribute[] CreateAttributes() => [
		// Base attack speed never rolls, neither the base value nor an affix's. Only loaded when the item is NOT armor.
		new(RollStore.Character, 1, 0f, RollScope.None, [new("characterBaseAttackSpeed")], Items: RollItemFilter.NonArmorOnly, Scales: false),
		new(RollStore.Character, 2, 20f, RollScope.All, [new("characterStrength")]),
		new(RollStore.Character, 3, 20f, RollScope.All, [new("characterStrengthModifier")]),
		new(RollStore.Character, 4, 20f, RollScope.All, [new("characterIntelligence")]),
		new(RollStore.Character, 5, 20f, RollScope.All, [new("characterIntelligenceModifier")]),
		new(RollStore.Character, 6, 20f, RollScope.All, [new("characterDexterity")]),
		new(RollStore.Character, 7, 20f, RollScope.All, [new("characterDexterityModifier")]),
		new(RollStore.Character, 8, 0f, RollScope.All, [new("characterLife")]),
		new(RollStore.Character, 9, 20f, RollScope.All, [new("characterLifeModifier")]),
		new(RollStore.Character, 10, 0f, RollScope.All, [new("characterLifeRegen")]),
		new(RollStore.Character, 11, 20f, RollScope.All, [new("characterLifeRegenModifier")]),
		new(RollStore.Character, 12, 20f, RollScope.All, [new("characterMana")]),
		new(RollStore.Character, 13, 20f, RollScope.All, [new("characterManaModifier")]),
		new(RollStore.Character, 14, 0f, RollScope.All, [new("characterManaRegen")]),
		new(RollStore.Character, 15, 20f, RollScope.All, [new("characterManaRegenModifier")]),
		new(RollStore.Character, 16, 20f, RollScope.All, [new("characterManaLimitReserve")]),
		new(RollStore.Character, 17, 20f, RollScope.All, [new("characterManaLimitReserveModifier")]),
		new(RollStore.Character, 18, 20f, RollScope.All, [new("characterManaLimitReserveReduction")]),
		new(RollStore.Character, 19, 20f, RollScope.All, [new("characterManaLimitReserveReductionModifier")]),
		new(RollStore.Character, 20, 20f, RollScope.All, [new("characterOffensiveAbility")]),
		new(RollStore.Character, 21, 20f, RollScope.All, [new("characterOffensiveAbilityModifier")]),
		new(RollStore.Character, 22, 20f, RollScope.All, [new("characterDefensiveAbility")]),
		new(RollStore.Character, 23, 20f, RollScope.All, [new("characterDefensiveAbilityModifier")]),
		new(RollStore.Character, 24, 20f, RollScope.All, [new("characterTotalSpeedModifier")]),
		// The base value never rolls, but affix values roll like any character attribute's; base percent is 0 anyway.
		new(RollStore.Character, 25, 0f, RollScope.All, [new("characterAttackSpeed")], Scales: false),
		new(RollStore.Character, 26, 20f, RollScope.All, [new("characterAttackSpeedModifier")]),
		// The base value never rolls, but affix values roll like any character attribute's; base percent is 0 anyway.
		new(RollStore.Character, 27, 0f, RollScope.All, [new("characterSpellCastSpeed")], Scales: false),
		new(RollStore.Character, 28, 20f, RollScope.All, [new("characterSpellCastSpeedModifier")]),
		// The base value never rolls, but affix values roll like any character attribute's; base percent is 0 anyway.
		new(RollStore.Character, 29, 0f, RollScope.All, [new("characterRunSpeed")], Scales: false),
		new(RollStore.Character, 30, 20f, RollScope.All, [new("characterRunSpeedModifier")]),
		new(RollStore.Character, 31, 20f, RollScope.All, [new("characterDefensiveBlockRecoveryReduction")]),
		new(RollStore.Character, 32, 20f, RollScope.All, [new("characterEnergyAbsorptionPercent")]),
		new(RollStore.Character, 33, 20f, RollScope.All, [new("characterDodgePercent")]),
		new(RollStore.Character, 34, 20f, RollScope.All, [new("characterDeflectProjectile")]),
		new(RollStore.Character, 35, 0f, RollScope.All, [new("characterGlobalReqReduction")]),
		new(RollStore.Character, 36, 0f, RollScope.All, [new("characterWeaponStrengthReqReduction")]),
		new(RollStore.Character, 37, 0f, RollScope.All, [new("characterWeaponDexterityReqReduction")]),
		new(RollStore.Character, 38, 0f, RollScope.All, [new("characterWeaponIntelligenceReqReduction")]),
		new(RollStore.Character, 39, 0f, RollScope.All, [new("characterMeleeStrengthReqReduction")]),
		new(RollStore.Character, 40, 0f, RollScope.All, [new("characterMeleeDexterityReqReduction")]),
		new(RollStore.Character, 41, 0f, RollScope.All, [new("characterMeleeIntelligenceReqReduction")]),
		new(RollStore.Character, 42, 0f, RollScope.All, [new("characterArmorStrengthReqReduction")]),
		new(RollStore.Character, 43, 0f, RollScope.All, [new("characterArmorDexterityReqReduction")]),
		new(RollStore.Character, 44, 0f, RollScope.All, [new("characterArmorIntelligenceReqReduction")]),
		new(RollStore.Character, 45, 0f, RollScope.All, [new("characterJewelryStrengthReqReduction")]),
		new(RollStore.Character, 46, 0f, RollScope.All, [new("characterJewelryDexterityReqReduction")]),
		new(RollStore.Character, 47, 0f, RollScope.All, [new("characterJewelryIntelligenceReqReduction")]),
		new(RollStore.Character, 48, 0f, RollScope.All, [new("characterShieldStrengthReqReduction")]),
		new(RollStore.Character, 49, 0f, RollScope.All, [new("characterShieldDexterityReqReduction")]),
		new(RollStore.Character, 50, 0f, RollScope.All, [new("characterShieldIntelligenceReqReduction")]),
		new(RollStore.Character, 51, 0f, RollScope.All, [new("characterHuntingStrengthReqReduction")]),
		new(RollStore.Character, 52, 0f, RollScope.All, [new("characterHuntingDexterityReqReduction")]),
		new(RollStore.Character, 53, 0f, RollScope.All, [new("characterHuntingIntelligenceReqReduction")]),
		new(RollStore.Character, 54, 0f, RollScope.All, [new("characterStaffStrengthReqReduction")]),
		new(RollStore.Character, 55, 0f, RollScope.All, [new("characterStaffDexterityReqReduction")]),
		new(RollStore.Character, 56, 0f, RollScope.All, [new("characterStaffIntelligenceReqReduction")]),
		// The base value never rolls, but affix values roll like any character attribute's; base percent is 0 anyway.
		new(RollStore.Character, 57, 0f, RollScope.All, [new("characterLevelReqReduction")], Scales: false),
		new(RollStore.Character, 58, 0f, RollScope.All, [new("characterIncreasedExperience")]),
		new(RollStore.Character, 59, 0f, RollScope.All, [new("characterPhysToElementalRatio")]),
		new(RollStore.Skill, 1, 0f, RollScope.All, [new("skillCooldownReduction")]),
		new(RollStore.Skill, 2, 0f, RollScope.All, [new("skillManaCostReduction")]),
		new(RollStore.Skill, 3, 0f, RollScope.All, [new("skillProjectileSpeedModifier")]),
		new(RollStore.Skill, 4, 0f, RollScope.All, [new("skillDurationModifier")]),
		new(RollStore.Skill, 5, 0f, RollScope.All, [new("skillCooldownReductionModifier")]),
		new(RollStore.Skill, 6, 0f, RollScope.All, [new("skillManaCostReductionModifier")]),
		// only loaded when the item IS armor; second roll is (offensivePhysicalMax - offensivePhysicalMin) truncated to uint, jittered, truncated again; no instance when offensivePhysicalMin == 0.
		new(RollStore.Damage, 2, 20f, RollScope.All, [new("offensivePhysicalMin"), new("offensivePhysicalMax", RoundToInt: true, RangeOf: "offensivePhysicalMin"), new("offensivePierceRatio", Scales: false)], Requires: ["offensivePhysicalMin"], Items: RollItemFilter.ArmorOnly),
		// only loaded when the item is NOT armor; nothing rolls.
		new(RollStore.Damage, 3, 20f, RollScope.None, [new("offensivePhysicalMin"), new("offensivePhysicalMax", RangeOf: "offensivePhysicalMin")], Items: RollItemFilter.NonArmorOnly),
		// only loaded when the item is NOT armor.
		new(RollStore.Damage, 4, 20f, RollScope.None, [new("offensiveBaseFireMin"), new("offensiveBaseFireMax", RangeOf: "offensiveBaseFireMin")], Items: RollItemFilter.NonArmorOnly),
		// only loaded when the item is NOT armor.
		new(RollStore.Damage, 5, 20f, RollScope.None, [new("offensiveBaseColdMin"), new("offensiveBaseColdMax", RangeOf: "offensiveBaseColdMin")], Items: RollItemFilter.NonArmorOnly),
		// only loaded when the item is NOT armor.
		new(RollStore.Damage, 6, 20f, RollScope.None, [new("offensiveBaseLightningMin"), new("offensiveBaseLightningMax", RangeOf: "offensiveBaseLightningMin")], Items: RollItemFilter.NonArmorOnly),
		// only loaded when the item is NOT armor.
		new(RollStore.Damage, 7, 20f, RollScope.None, [new("offensiveBasePoisonMin"), new("offensiveBasePoisonMax", RangeOf: "offensiveBasePoisonMin")], Items: RollItemFilter.NonArmorOnly),
		// only loaded when the item is NOT armor.
		new(RollStore.Damage, 8, 20f, RollScope.None, [new("offensiveBaseLifeMin"), new("offensiveBaseLifeMax", RangeOf: "offensiveBaseLifeMin")], Items: RollItemFilter.NonArmorOnly),
		// second roll is (offensiveBonusPhysicalMax - offensiveBonusPhysicalMin) clamped >= 0, not offensiveBonusPhysicalMax; no draw when max <= min or absent; no instance when offensiveBonusPhysicalMin == 0.
		// only loaded when the item is NOT armor.
		new(RollStore.Damage, 9, 20f, RollScope.All, [new("offensiveBonusPhysicalMin"), new("offensiveBonusPhysicalMax", RangeOf: "offensiveBonusPhysicalMin")], Requires: ["offensiveBonusPhysicalMin"], Items: RollItemFilter.NonArmorOnly),
		new(RollStore.Damage, 10, 20f, RollScope.All, [new("offensivePhysicalModifier")]),
		new(RollStore.Damage, 11, 20f, RollScope.All, [new("offensiveSlowPhysicalModifier"), new("offensiveSlowPhysicalDurationModifier")]),
		new(RollStore.Damage, 12, 20f, RollScope.All, [new("offensivePierceRatioModifier")]),
		// second roll is (offensivePierceMax - offensivePierceMin) clamped >= 0, not offensivePierceMax; no draw when max <= min or absent; no instance when offensivePierceMin == 0.
		new(RollStore.Damage, 13, 20f, RollScope.All, [new("offensivePierceMin"), new("offensivePierceMax", RangeOf: "offensivePierceMin")], Requires: ["offensivePierceMin"]),
		new(RollStore.Damage, 14, 20f, RollScope.All, [new("offensivePierceModifier")]),
		// second roll is (offensiveSlowBleedingMax - offensiveSlowBleedingMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when offensiveSlowBleedingDurationMin == 0.
		new(RollStore.Damage, 15, 20f, RollScope.All, [new("offensiveSlowBleedingMin"), new("offensiveSlowBleedingMax", RangeOf: "offensiveSlowBleedingMin")], Requires: ["offensiveSlowBleedingMin", "offensiveSlowBleedingDurationMin"]),
		new(RollStore.Damage, 16, 20f, RollScope.All, [new("offensiveBleedingModifier")]),
		new(RollStore.Damage, 17, 20f, RollScope.All, [new("offensiveSlowBleedingModifier"), new("offensiveSlowBleedingDurationModifier")]),
		// second roll is (offensiveElementalMax - offensiveElementalMin) clamped >= 0, not offensiveElementalMax; no draw when max <= min or absent; no instance when offensiveElementalMin == 0.
		new(RollStore.Damage, 18, 20f, RollScope.All, [new("offensiveElementalMin"), new("offensiveElementalMax", RangeOf: "offensiveElementalMin")], Requires: ["offensiveElementalMin"]),
		new(RollStore.Damage, 19, 20f, RollScope.All, [new("offensiveElementalModifier")]),
		new(RollStore.Damage, 20, 20f, RollScope.All, [new("offensiveTotalDamageModifier")]),
		// second roll is (offensiveFireMax - offensiveFireMin) clamped >= 0, not offensiveFireMax; no draw when max <= min or absent; no instance when offensiveFireMin == 0.
		new(RollStore.Damage, 21, 20f, RollScope.All, [new("offensiveFireMin"), new("offensiveFireMax", RangeOf: "offensiveFireMin")], Requires: ["offensiveFireMin"]),
		// second roll is (offensiveSlowFireMax - offensiveSlowFireMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when offensiveSlowFireDurationMin == 0.
		new(RollStore.Damage, 22, 20f, RollScope.All, [new("offensiveSlowFireMin"), new("offensiveSlowFireMax", RangeOf: "offensiveSlowFireMin")], Requires: ["offensiveSlowFireMin", "offensiveSlowFireDurationMin"]),
		new(RollStore.Damage, 23, 20f, RollScope.All, [new("offensiveFireModifier")]),
		new(RollStore.Damage, 24, 20f, RollScope.All, [new("offensiveSlowFireModifier"), new("offensiveSlowFireDurationModifier")]),
		// second roll is (offensiveColdMax - offensiveColdMin) clamped >= 0, not offensiveColdMax; no draw when max <= min or absent; no instance when offensiveColdMin == 0.
		new(RollStore.Damage, 25, 20f, RollScope.All, [new("offensiveColdMin"), new("offensiveColdMax", RangeOf: "offensiveColdMin")], Requires: ["offensiveColdMin"]),
		// second roll is (offensiveSlowColdMax - offensiveSlowColdMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when offensiveSlowColdDurationMin == 0.
		new(RollStore.Damage, 26, 20f, RollScope.All, [new("offensiveSlowColdMin"), new("offensiveSlowColdMax", RangeOf: "offensiveSlowColdMin")], Requires: ["offensiveSlowColdMin", "offensiveSlowColdDurationMin"]),
		new(RollStore.Damage, 27, 20f, RollScope.All, [new("offensiveColdModifier")]),
		new(RollStore.Damage, 28, 20f, RollScope.All, [new("offensiveSlowColdModifier"), new("offensiveSlowColdDurationModifier")]),
		// second roll is (offensiveLightningMax - offensiveLightningMin) clamped >= 0, not offensiveLightningMax; no draw when max <= min or absent; no instance when offensiveLightningMin == 0.
		new(RollStore.Damage, 29, 20f, RollScope.All, [new("offensiveLightningMin"), new("offensiveLightningMax", RangeOf: "offensiveLightningMin")], Requires: ["offensiveLightningMin"]),
		// second roll is (offensiveSlowLightningMax - offensiveSlowLightningMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when offensiveSlowLightningDurationMin == 0.
		new(RollStore.Damage, 30, 20f, RollScope.All, [new("offensiveSlowLightningMin"), new("offensiveSlowLightningMax", RangeOf: "offensiveSlowLightningMin")], Requires: ["offensiveSlowLightningMin", "offensiveSlowLightningDurationMin"]),
		new(RollStore.Damage, 31, 20f, RollScope.All, [new("offensiveLightningModifier")]),
		new(RollStore.Damage, 32, 20f, RollScope.All, [new("offensiveSlowLightningModifier"), new("offensiveSlowLightningDurationModifier")]),
		// second roll is (offensivePoisonMax - offensivePoisonMin) clamped >= 0, not offensivePoisonMax; no draw when max <= min or absent; no instance when offensivePoisonMin == 0.
		new(RollStore.Damage, 33, 20f, RollScope.All, [new("offensivePoisonMin"), new("offensivePoisonMax", RangeOf: "offensivePoisonMin")], Requires: ["offensivePoisonMin"]),
		// second roll is (offensiveSlowPoisonMax - offensiveSlowPoisonMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when offensiveSlowPoisonDurationMin == 0.
		new(RollStore.Damage, 34, 20f, RollScope.All, [new("offensiveSlowPoisonMin"), new("offensiveSlowPoisonMax", RangeOf: "offensiveSlowPoisonMin")], Requires: ["offensiveSlowPoisonMin", "offensiveSlowPoisonDurationMin"]),
		new(RollStore.Damage, 35, 20f, RollScope.All, [new("offensivePoisonModifier")]),
		new(RollStore.Damage, 36, 20f, RollScope.All, [new("offensiveSlowPoisonModifier"), new("offensiveSlowPoisonDurationModifier")]),
		// second roll is (offensivePercentCurrentLifeMax - offensivePercentCurrentLifeMin) clamped >= 0, not offensivePercentCurrentLifeMax; no draw when max <= min or absent; no instance when offensivePercentCurrentLifeMin == 0.
		new(RollStore.Damage, 37, 20f, RollScope.All, [new("offensivePercentCurrentLifeMin"), new("offensivePercentCurrentLifeMax", RangeOf: "offensivePercentCurrentLifeMin")], Requires: ["offensivePercentCurrentLifeMin"]),
		// second roll is (offensiveLifeMax - offensiveLifeMin) clamped >= 0, not offensiveLifeMax; no draw when max <= min or absent; no instance when offensiveLifeMin == 0.
		new(RollStore.Damage, 38, 20f, RollScope.All, [new("offensiveLifeMin"), new("offensiveLifeMax", RangeOf: "offensiveLifeMin")], Requires: ["offensiveLifeMin"]),
		// second roll is (offensiveSlowLifeMax - offensiveSlowLifeMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when offensiveSlowLifeDurationMin == 0.
		new(RollStore.Damage, 39, 20f, RollScope.All, [new("offensiveSlowLifeMin"), new("offensiveSlowLifeMax", RangeOf: "offensiveSlowLifeMin")], Requires: ["offensiveSlowLifeMin", "offensiveSlowLifeDurationMin"]),
		new(RollStore.Damage, 40, 20f, RollScope.All, [new("offensiveLifeModifier")]),
		new(RollStore.Damage, 41, 20f, RollScope.All, [new("offensiveSlowLifeModifier"), new("offensiveSlowLifeDurationModifier")]),
		// second roll is (offensiveLifeLeechMax - offensiveLifeLeechMin) clamped >= 0, not offensiveLifeLeechMax; no draw when max <= min or absent; no instance when offensiveLifeLeechMin == 0.
		new(RollStore.Damage, 42, 20f, RollScope.All, [new("offensiveLifeLeechMin"), new("offensiveLifeLeechMax", RangeOf: "offensiveLifeLeechMin")], Requires: ["offensiveLifeLeechMin"]),
		// second roll is (offensiveSlowLifeLeachMax - offensiveSlowLifeLeachMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when offensiveSlowLifeLeachDurationMin == 0.
		new(RollStore.Damage, 43, 20f, RollScope.All, [new("offensiveSlowLifeLeachMin"), new("offensiveSlowLifeLeachMax", RangeOf: "offensiveSlowLifeLeachMin")], Requires: ["offensiveSlowLifeLeachMin", "offensiveSlowLifeLeachDurationMin"]),
		new(RollStore.Damage, 44, 20f, RollScope.All, [new("offensiveSlowLifeLeachModifier"), new("offensiveSlowLifeLeachDurationModifier")]),
		// second roll is (offensiveManaBurnDrainMax - offensiveManaBurnDrainMin) clamped >= 0, not offensiveManaBurnDrainMax; no draw when max <= min or absent; no instance when offensiveManaBurnDrainMin == 0.
		new(RollStore.Damage, 45, 20f, RollScope.All, [new("offensiveManaBurnDrainMin"), new("offensiveManaBurnDrainMax", RangeOf: "offensiveManaBurnDrainMin")], Requires: ["offensiveManaBurnDrainMin"]),
		new(RollStore.Damage, 46, 20f, RollScope.All, [new("offensiveManaBurnRatioAdder")]),
		// second roll is (offensiveSlowManaLeachMax - offensiveSlowManaLeachMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when offensiveSlowManaLeachDurationMin == 0.
		new(RollStore.Damage, 47, 20f, RollScope.All, [new("offensiveSlowManaLeachMin"), new("offensiveSlowManaLeachMax", RangeOf: "offensiveSlowManaLeachMin")], Requires: ["offensiveSlowManaLeachMin", "offensiveSlowManaLeachDurationMin"]),
		new(RollStore.Damage, 48, 20f, RollScope.All, [new("offensiveSlowManaLeachModifier"), new("offensiveSlowManaLeachDurationModifier")]),
		// second roll is (offensiveSlowOffensiveAbilityMax - offensiveSlowOffensiveAbilityMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when offensiveSlowOffensiveAbilityDurationMin == 0.
		new(RollStore.Damage, 49, 20f, RollScope.All, [new("offensiveSlowOffensiveAbilityMin"), new("offensiveSlowOffensiveAbilityMax", RangeOf: "offensiveSlowOffensiveAbilityMin")], Requires: ["offensiveSlowOffensiveAbilityMin", "offensiveSlowOffensiveAbilityDurationMin"]),
		new(RollStore.Damage, 50, 20f, RollScope.All, [new("offensiveSlowOffensiveAbilityModifier"), new("offensiveSlowOffensiveAbilityDurationModifier")]),
		// second roll is (offensiveSlowDefensiveAbilityMax - offensiveSlowDefensiveAbilityMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when offensiveSlowDefensiveAbilityDurationMin == 0.
		new(RollStore.Damage, 51, 20f, RollScope.All, [new("offensiveSlowDefensiveAbilityMin"), new("offensiveSlowDefensiveAbilityMax", RangeOf: "offensiveSlowDefensiveAbilityMin")], Requires: ["offensiveSlowDefensiveAbilityMin", "offensiveSlowDefensiveAbilityDurationMin"]),
		new(RollStore.Damage, 52, 20f, RollScope.All, [new("offensiveSlowDefensiveAbilityModifier"), new("offensiveSlowDefensiveAbilityDurationModifier")]),
		// second roll is (offensiveSlowOffensiveReductionMax - offensiveSlowOffensiveReductionMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when offensiveSlowOffensiveReductionDurationMin == 0.
		new(RollStore.Damage, 53, 20f, RollScope.All, [new("offensiveSlowOffensiveReductionMin"), new("offensiveSlowOffensiveReductionMax", RangeOf: "offensiveSlowOffensiveReductionMin")], Requires: ["offensiveSlowOffensiveReductionMin", "offensiveSlowOffensiveReductionDurationMin"]),
		new(RollStore.Damage, 54, 20f, RollScope.All, [new("offensiveSlowOffensiveReductionModifier"), new("offensiveSlowOffensiveReductionDurationModifier")]),
		// second roll is (offensiveSlowDefensiveReductionMax - offensiveSlowDefensiveReductionMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when offensiveSlowDefensiveReductionDurationMin == 0.
		new(RollStore.Damage, 55, 20f, RollScope.All, [new("offensiveSlowDefensiveReductionMin"), new("offensiveSlowDefensiveReductionMax", RangeOf: "offensiveSlowDefensiveReductionMin")], Requires: ["offensiveSlowDefensiveReductionMin", "offensiveSlowDefensiveReductionDurationMin"]),
		// second roll is (offensiveTotalDamageReductionPercentMax - offensiveTotalDamageReductionPercentMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when offensiveTotalDamageReductionPercentDurationMin == 0.
		new(RollStore.Damage, 56, 20f, RollScope.All, [new("offensiveTotalDamageReductionPercentMin"), new("offensiveTotalDamageReductionPercentMax", RangeOf: "offensiveTotalDamageReductionPercentMin")], Requires: ["offensiveTotalDamageReductionPercentMin", "offensiveTotalDamageReductionPercentDurationMin"]),
		// second roll is (offensiveTotalDamageReductionAbsoluteMax - offensiveTotalDamageReductionAbsoluteMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when offensiveTotalDamageReductionAbsoluteDurationMin == 0.
		new(RollStore.Damage, 57, 20f, RollScope.All, [new("offensiveTotalDamageReductionAbsoluteMin"), new("offensiveTotalDamageReductionAbsoluteMax", RangeOf: "offensiveTotalDamageReductionAbsoluteMin")], Requires: ["offensiveTotalDamageReductionAbsoluteMin", "offensiveTotalDamageReductionAbsoluteDurationMin"]),
		// second roll is (offensiveTotalResistanceReductionPercentMax - offensiveTotalResistanceReductionPercentMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when offensiveTotalResistanceReductionPercentDurationMin == 0.
		new(RollStore.Damage, 58, 20f, RollScope.All, [new("offensiveTotalResistanceReductionPercentMin"), new("offensiveTotalResistanceReductionPercentMax", RangeOf: "offensiveTotalResistanceReductionPercentMin")], Requires: ["offensiveTotalResistanceReductionPercentMin", "offensiveTotalResistanceReductionPercentDurationMin"]),
		// second roll is (offensiveTotalResistanceReductionAbsoluteMax - offensiveTotalResistanceReductionAbsoluteMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when offensiveTotalResistanceReductionAbsoluteDurationMin == 0.
		new(RollStore.Damage, 59, 20f, RollScope.All, [new("offensiveTotalResistanceReductionAbsoluteMin"), new("offensiveTotalResistanceReductionAbsoluteMax", RangeOf: "offensiveTotalResistanceReductionAbsoluteMin")], Requires: ["offensiveTotalResistanceReductionAbsoluteMin", "offensiveTotalResistanceReductionAbsoluteDurationMin"]),
		// second roll is (offensiveFumbleMax - offensiveFumbleMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when offensiveFumbleDurationMin == 0.
		new(RollStore.Damage, 60, 20f, RollScope.All, [new("offensiveFumbleMin"), new("offensiveFumbleMax", RangeOf: "offensiveFumbleMin")], Requires: ["offensiveFumbleMin", "offensiveFumbleDurationMin"]),
		// second roll is (offensiveProjectileFumbleMax - offensiveProjectileFumbleMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when offensiveProjectileFumbleDurationMin == 0.
		new(RollStore.Damage, 61, 20f, RollScope.All, [new("offensiveProjectileFumbleMin"), new("offensiveProjectileFumbleMax", RangeOf: "offensiveProjectileFumbleMin")], Requires: ["offensiveProjectileFumbleMin", "offensiveProjectileFumbleDurationMin"]),
		// second roll is (offensiveSleepMax - offensiveSleepMin) clamped >= 0, not offensiveSleepMax; no draw when max <= min or absent; no instance when offensiveSleepMin == 0.
		new(RollStore.Damage, 62, 20f, RollScope.All, [new("offensiveSleepMin"), new("offensiveSleepMax", RangeOf: "offensiveSleepMin")], Requires: ["offensiveSleepMin"]),
		new(RollStore.Damage, 63, 20f, RollScope.All, [new("offensiveSleepModifier")]),
		// second roll is (offensiveStunMax - offensiveStunMin) clamped >= 0, not offensiveStunMax; no draw when max <= min or absent; no instance when offensiveStunMin == 0.
		new(RollStore.Damage, 64, 20f, RollScope.All, [new("offensiveStunMin"), new("offensiveStunMax", RangeOf: "offensiveStunMin")], Requires: ["offensiveStunMin"]),
		new(RollStore.Damage, 65, 20f, RollScope.All, [new("offensiveStunModifier")]),
		// second roll is (offensiveFreezeMax - offensiveFreezeMin) clamped >= 0, not offensiveFreezeMax; no draw when max <= min or absent; no instance when offensiveFreezeMin == 0.
		new(RollStore.Damage, 66, 20f, RollScope.All, [new("offensiveFreezeMin"), new("offensiveFreezeMax", RangeOf: "offensiveFreezeMin")], Requires: ["offensiveFreezeMin"]),
		// second roll is (offensivePetrifyMax - offensivePetrifyMin) clamped >= 0, not offensivePetrifyMax; no draw when max <= min or absent; no instance when offensivePetrifyMin == 0.
		new(RollStore.Damage, 67, 20f, RollScope.All, [new("offensivePetrifyMin"), new("offensivePetrifyMax", RangeOf: "offensivePetrifyMin")], Requires: ["offensivePetrifyMin"]),
		// second roll is (offensiveConvertMax - offensiveConvertMin) clamped >= 0, not offensiveConvertMax; no draw when max <= min or absent; no instance when offensiveConvertMin == 0.
		new(RollStore.Damage, 68, 20f, RollScope.All, [new("offensiveConvertMin"), new("offensiveConvertMax", RangeOf: "offensiveConvertMin")], Requires: ["offensiveConvertMin"]),
		// second roll is (offensiveConfusionMax - offensiveConfusionMin) clamped >= 0, not offensiveConfusionMax; no draw when max <= min or absent; no instance when offensiveConfusionMin == 0.
		new(RollStore.Damage, 69, 20f, RollScope.All, [new("offensiveConfusionMin"), new("offensiveConfusionMax", RangeOf: "offensiveConfusionMin")], Requires: ["offensiveConfusionMin"]),
		// second roll is (offensiveSlowTotalSpeedMax - offensiveSlowTotalSpeedMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when offensiveSlowTotalSpeedDurationMin == 0.
		new(RollStore.Damage, 70, 20f, RollScope.All, [new("offensiveSlowTotalSpeedMin"), new("offensiveSlowTotalSpeedMax", RangeOf: "offensiveSlowTotalSpeedMin")], Requires: ["offensiveSlowTotalSpeedMin", "offensiveSlowTotalSpeedDurationMin"]),
		// second roll is (offensiveSlowAttackSpeedMax - offensiveSlowAttackSpeedMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when offensiveSlowAttackSpeedDurationMin == 0.
		new(RollStore.Damage, 71, 20f, RollScope.All, [new("offensiveSlowAttackSpeedMin"), new("offensiveSlowAttackSpeedMax", RangeOf: "offensiveSlowAttackSpeedMin")], Requires: ["offensiveSlowAttackSpeedMin", "offensiveSlowAttackSpeedDurationMin"]),
		new(RollStore.Damage, 72, 20f, RollScope.All, [new("offensiveSlowAttackSpeedModifier"), new("offensiveSlowAttackSpeedDurationModifier")]),
		// second roll is (offensiveSlowSpellCastSpeedMax - offensiveSlowSpellCastSpeedMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when offensiveSlowSpellCastSpeedDurationMin == 0.
		new(RollStore.Damage, 73, 20f, RollScope.All, [new("offensiveSlowSpellCastSpeedMin"), new("offensiveSlowSpellCastSpeedMax", RangeOf: "offensiveSlowSpellCastSpeedMin")], Requires: ["offensiveSlowSpellCastSpeedMin", "offensiveSlowSpellCastSpeedDurationMin"]),
		new(RollStore.Damage, 74, 20f, RollScope.All, [new("offensiveSlowSpellCastSpeedModifier"), new("offensiveSlowSpellCastSpeedDurationModifier")]),
		// second roll is (offensiveSlowRunSpeedMax - offensiveSlowRunSpeedMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when offensiveSlowRunSpeedDurationMin == 0.
		new(RollStore.Damage, 75, 20f, RollScope.All, [new("offensiveSlowRunSpeedMin"), new("offensiveSlowRunSpeedMax", RangeOf: "offensiveSlowRunSpeedMin")], Requires: ["offensiveSlowRunSpeedMin", "offensiveSlowRunSpeedDurationMin"]),
		new(RollStore.Damage, 76, 20f, RollScope.All, [new("offensiveSlowRunSpeedModifier"), new("offensiveSlowRunSpeedDurationModifier")]),
		// second roll is (offensiveDisruptionMax - offensiveDisruptionMin) clamped >= 0, not offensiveDisruptionMax; no draw when max <= min or absent; no instance when offensiveDisruptionMin == 0.
		new(RollStore.Damage, 77, 20f, RollScope.All, [new("offensiveDisruptionMin"), new("offensiveDisruptionMax", RangeOf: "offensiveDisruptionMin")], Requires: ["offensiveDisruptionMin"]),
		// second roll is (retaliationPhysicalMax - retaliationPhysicalMin) clamped >= 0, not retaliationPhysicalMax; no draw when max <= min or absent; no instance when retaliationPhysicalMin == 0.
		new(RollStore.Retaliation, 2, 20f, RollScope.All, [new("retaliationPhysicalMin"), new("retaliationPhysicalMax", RangeOf: "retaliationPhysicalMin")], Requires: ["retaliationPhysicalMin"]),
		// second roll is (retaliationSlowPhysicalMax - retaliationSlowPhysicalMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when retaliationSlowPhysicalDurationMin == 0.
		new(RollStore.Retaliation, 3, 20f, RollScope.All, [new("retaliationSlowPhysicalMin"), new("retaliationSlowPhysicalMax", RangeOf: "retaliationSlowPhysicalMin")], Requires: ["retaliationSlowPhysicalMin", "retaliationSlowPhysicalDurationMin"]),
		new(RollStore.Retaliation, 4, 20f, RollScope.All, [new("retaliationPhysicalModifier")]),
		new(RollStore.Retaliation, 5, 20f, RollScope.All, [new("retaliationSlowPhysicalModifier"), new("retaliationSlowPhysicalDurationModifier")]),
		new(RollStore.Retaliation, 6, 20f, RollScope.All, [new("retaliationPierceRatioModifier")]),
		// second roll is (retaliationPierceMax - retaliationPierceMin) clamped >= 0, not retaliationPierceMax; no draw when max <= min or absent; no instance when retaliationPierceMin == 0.
		new(RollStore.Retaliation, 7, 20f, RollScope.All, [new("retaliationPierceMin"), new("retaliationPierceMax", RangeOf: "retaliationPierceMin")], Requires: ["retaliationPierceMin"]),
		new(RollStore.Retaliation, 8, 20f, RollScope.All, [new("retaliationPierceModifier")]),
		// second roll is (retaliationSlowBleedingMax - retaliationSlowBleedingMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when retaliationSlowBleedingDurationMin == 0.
		new(RollStore.Retaliation, 9, 20f, RollScope.All, [new("retaliationSlowBleedingMin"), new("retaliationSlowBleedingMax", RangeOf: "retaliationSlowBleedingMin")], Requires: ["retaliationSlowBleedingMin", "retaliationSlowBleedingDurationMin"]),
		new(RollStore.Retaliation, 10, 20f, RollScope.All, [new("retaliationSlowBleedingModifier"), new("retaliationSlowBleedingDurationModifier")]),
		// second roll is (retaliationElementalMax - retaliationElementalMin) clamped >= 0, not retaliationElementalMax; no draw when max <= min or absent; no instance when retaliationElementalMin == 0.
		new(RollStore.Retaliation, 11, 20f, RollScope.All, [new("retaliationElementalMin"), new("retaliationElementalMax", RangeOf: "retaliationElementalMin")], Requires: ["retaliationElementalMin"]),
		// second roll is (retaliationColdMax - retaliationColdMin) clamped >= 0, not retaliationColdMax; no draw when max <= min or absent; no instance when retaliationColdMin == 0.
		new(RollStore.Retaliation, 12, 20f, RollScope.All, [new("retaliationColdMin"), new("retaliationColdMax", RangeOf: "retaliationColdMin")], Requires: ["retaliationColdMin"]),
		// second roll is (retaliationSlowColdMax - retaliationSlowColdMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when retaliationSlowColdDurationMin == 0.
		new(RollStore.Retaliation, 13, 20f, RollScope.All, [new("retaliationSlowColdMin"), new("retaliationSlowColdMax", RangeOf: "retaliationSlowColdMin")], Requires: ["retaliationSlowColdMin", "retaliationSlowColdDurationMin"]),
		new(RollStore.Retaliation, 14, 20f, RollScope.All, [new("retaliationColdModifier")]),
		new(RollStore.Retaliation, 15, 20f, RollScope.All, [new("retaliationSlowColdModifier"), new("retaliationSlowColdDurationModifier")]),
		// second roll is (retaliationPoisonMax - retaliationPoisonMin) clamped >= 0, not retaliationPoisonMax; no draw when max <= min or absent; no instance when retaliationPoisonMin == 0.
		new(RollStore.Retaliation, 16, 20f, RollScope.All, [new("retaliationPoisonMin"), new("retaliationPoisonMax", RangeOf: "retaliationPoisonMin")], Requires: ["retaliationPoisonMin"]),
		// second roll is (retaliationSlowPoisonMax - retaliationSlowPoisonMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when retaliationSlowPoisonDurationMin == 0.
		new(RollStore.Retaliation, 17, 20f, RollScope.All, [new("retaliationSlowPoisonMin"), new("retaliationSlowPoisonMax", RangeOf: "retaliationSlowPoisonMin")], Requires: ["retaliationSlowPoisonMin", "retaliationSlowPoisonDurationMin"]),
		new(RollStore.Retaliation, 18, 20f, RollScope.All, [new("retaliationPoisonModifier")]),
		new(RollStore.Retaliation, 19, 20f, RollScope.All, [new("retaliationSlowPoisonModifier"), new("retaliationSlowPoisonDurationModifier")]),
		// second roll is (retaliationLightningMax - retaliationLightningMin) clamped >= 0, not retaliationLightningMax; no draw when max <= min or absent; no instance when retaliationLightningMin == 0.
		new(RollStore.Retaliation, 20, 20f, RollScope.All, [new("retaliationLightningMin"), new("retaliationLightningMax", RangeOf: "retaliationLightningMin")], Requires: ["retaliationLightningMin"]),
		// second roll is (retaliationSlowLightningMax - retaliationSlowLightningMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when retaliationSlowLightningDurationMin == 0.
		new(RollStore.Retaliation, 21, 20f, RollScope.All, [new("retaliationSlowLightningMin"), new("retaliationSlowLightningMax", RangeOf: "retaliationSlowLightningMin")], Requires: ["retaliationSlowLightningMin", "retaliationSlowLightningDurationMin"]),
		new(RollStore.Retaliation, 22, 20f, RollScope.All, [new("retaliationLightningModifier")]),
		new(RollStore.Retaliation, 23, 20f, RollScope.All, [new("retaliationSlowLightningModifier"), new("retaliationSlowLightningDurationModifier")]),
		// second roll is (retaliationFireMax - retaliationFireMin) clamped >= 0, not retaliationFireMax; no draw when max <= min or absent; no instance when retaliationFireMin == 0.
		new(RollStore.Retaliation, 24, 20f, RollScope.All, [new("retaliationFireMin"), new("retaliationFireMax", RangeOf: "retaliationFireMin")], Requires: ["retaliationFireMin"]),
		// second roll is (retaliationSlowFireMax - retaliationSlowFireMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when retaliationSlowFireDurationMin == 0.
		new(RollStore.Retaliation, 25, 20f, RollScope.All, [new("retaliationSlowFireMin"), new("retaliationSlowFireMax", RangeOf: "retaliationSlowFireMin")], Requires: ["retaliationSlowFireMin", "retaliationSlowFireDurationMin"]),
		new(RollStore.Retaliation, 26, 20f, RollScope.All, [new("retaliationFireModifier")]),
		new(RollStore.Retaliation, 27, 20f, RollScope.All, [new("retaliationSlowFireModifier"), new("retaliationSlowFireDurationModifier")]),
		// second roll is (retaliationPercentCurrentLifeMax - retaliationPercentCurrentLifeMin) clamped >= 0, not retaliationPercentCurrentLifeMax; no draw when max <= min or absent; no instance when retaliationPercentCurrentLifeMin == 0.
		new(RollStore.Retaliation, 28, 20f, RollScope.All, [new("retaliationPercentCurrentLifeMin"), new("retaliationPercentCurrentLifeMax", RangeOf: "retaliationPercentCurrentLifeMin")], Requires: ["retaliationPercentCurrentLifeMin"]),
		// second roll is (retaliationLifeMax - retaliationLifeMin) clamped >= 0, not retaliationLifeMax; no draw when max <= min or absent; no instance when retaliationLifeMin == 0.
		new(RollStore.Retaliation, 29, 20f, RollScope.All, [new("retaliationLifeMin"), new("retaliationLifeMax", RangeOf: "retaliationLifeMin")], Requires: ["retaliationLifeMin"]),
		// second roll is (retaliationSlowLifeMax - retaliationSlowLifeMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when retaliationSlowLifeDurationMin == 0.
		new(RollStore.Retaliation, 30, 20f, RollScope.All, [new("retaliationSlowLifeMin"), new("retaliationSlowLifeMax", RangeOf: "retaliationSlowLifeMin")], Requires: ["retaliationSlowLifeMin", "retaliationSlowLifeDurationMin"]),
		new(RollStore.Retaliation, 31, 20f, RollScope.All, [new("retaliationLifeModifier")]),
		new(RollStore.Retaliation, 32, 20f, RollScope.All, [new("retaliationSlowLifeModifier"), new("retaliationSlowLifeDurationModifier")]),
		// second roll is (retaliationSlowLifeLeachMax - retaliationSlowLifeLeachMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when retaliationSlowLifeLeachDurationMin == 0.
		new(RollStore.Retaliation, 33, 20f, RollScope.All, [new("retaliationSlowLifeLeachMin"), new("retaliationSlowLifeLeachMax", RangeOf: "retaliationSlowLifeLeachMin")], Requires: ["retaliationSlowLifeLeachMin", "retaliationSlowLifeLeachDurationMin"]),
		new(RollStore.Retaliation, 34, 20f, RollScope.All, [new("retaliationSlowLifeLeachModifier"), new("retaliationSlowLifeLeachDurationModifier")]),
		// second roll is (retaliationSlowManaLeachMax - retaliationSlowManaLeachMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when retaliationSlowManaLeachDurationMin == 0.
		new(RollStore.Retaliation, 35, 20f, RollScope.All, [new("retaliationSlowManaLeachMin"), new("retaliationSlowManaLeachMax", RangeOf: "retaliationSlowManaLeachMin")], Requires: ["retaliationSlowManaLeachMin", "retaliationSlowManaLeachDurationMin"]),
		new(RollStore.Retaliation, 36, 20f, RollScope.All, [new("retaliationSlowManaLeachModifier"), new("retaliationSlowManaLeachDurationModifier")]),
		// second roll is (retaliationSlowOffensiveAbilityMax - retaliationSlowOffensiveAbilityMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when retaliationSlowOffensiveAbilityDurationMin == 0.
		new(RollStore.Retaliation, 37, 20f, RollScope.All, [new("retaliationSlowOffensiveAbilityMin"), new("retaliationSlowOffensiveAbilityMax", RangeOf: "retaliationSlowOffensiveAbilityMin")], Requires: ["retaliationSlowOffensiveAbilityMin", "retaliationSlowOffensiveAbilityDurationMin"]),
		new(RollStore.Retaliation, 38, 20f, RollScope.All, [new("retaliationSlowOffensiveAbilityModifier"), new("retaliationSlowOffensiveAbilityDurationModifier")]),
		// second roll is (retaliationSlowDefensiveAbilityMax - retaliationSlowDefensiveAbilityMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when retaliationSlowDefensiveAbilityDurationMin == 0.
		new(RollStore.Retaliation, 39, 20f, RollScope.All, [new("retaliationSlowDefensiveAbilityMin"), new("retaliationSlowDefensiveAbilityMax", RangeOf: "retaliationSlowDefensiveAbilityMin")], Requires: ["retaliationSlowDefensiveAbilityMin", "retaliationSlowDefensiveAbilityDurationMin"]),
		new(RollStore.Retaliation, 40, 20f, RollScope.All, [new("retaliationSlowDefensiveAbilityModifier"), new("retaliationSlowDefensiveAbilityDurationModifier")]),
		// second roll is (retaliationSlowOffensiveReductionMax - retaliationSlowOffensiveReductionMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when retaliationSlowOffensiveReductionDurationMin == 0.
		new(RollStore.Retaliation, 41, 20f, RollScope.All, [new("retaliationSlowOffensiveReductionMin"), new("retaliationSlowOffensiveReductionMax", RangeOf: "retaliationSlowOffensiveReductionMin")], Requires: ["retaliationSlowOffensiveReductionMin", "retaliationSlowOffensiveReductionDurationMin"]),
		new(RollStore.Retaliation, 42, 20f, RollScope.All, [new("retaliationSlowOffensiveReductionModifier"), new("retaliationSlowOffensiveReductionDurationModifier")]),
		// second roll is (retaliationStunMax - retaliationStunMin) clamped >= 0, not retaliationStunMax; no draw when max <= min or absent; no instance when retaliationStunMin == 0.
		new(RollStore.Retaliation, 43, 20f, RollScope.All, [new("retaliationStunMin"), new("retaliationStunMax", RangeOf: "retaliationStunMin")], Requires: ["retaliationStunMin"]),
		new(RollStore.Retaliation, 44, 20f, RollScope.All, [new("retaliationStunModifier")]),
		// second roll is (retaliationSlowAttackSpeedMax - retaliationSlowAttackSpeedMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when retaliationSlowAttackSpeedDurationMin == 0.
		new(RollStore.Retaliation, 45, 20f, RollScope.All, [new("retaliationSlowAttackSpeedMin"), new("retaliationSlowAttackSpeedMax", RangeOf: "retaliationSlowAttackSpeedMin")], Requires: ["retaliationSlowAttackSpeedMin", "retaliationSlowAttackSpeedDurationMin"]),
		new(RollStore.Retaliation, 46, 20f, RollScope.All, [new("retaliationSlowAttackSpeedModifier"), new("retaliationSlowAttackSpeedDurationModifier")]),
		// second roll is (retaliationSlowSpellCastSpeedMax - retaliationSlowSpellCastSpeedMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when retaliationSlowSpellCastSpeedDurationMin == 0.
		new(RollStore.Retaliation, 47, 20f, RollScope.All, [new("retaliationSlowSpellCastSpeedMin"), new("retaliationSlowSpellCastSpeedMax", RangeOf: "retaliationSlowSpellCastSpeedMin")], Requires: ["retaliationSlowSpellCastSpeedMin", "retaliationSlowSpellCastSpeedDurationMin"]),
		new(RollStore.Retaliation, 48, 20f, RollScope.All, [new("retaliationSlowSpellCastSpeedModifier"), new("retaliationSlowSpellCastSpeedDurationModifier")]),
		// second roll is (retaliationSlowRunSpeedMax - retaliationSlowRunSpeedMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when retaliationSlowRunSpeedDurationMin == 0.
		new(RollStore.Retaliation, 49, 20f, RollScope.All, [new("retaliationSlowRunSpeedMin"), new("retaliationSlowRunSpeedMax", RangeOf: "retaliationSlowRunSpeedMin")], Requires: ["retaliationSlowRunSpeedMin", "retaliationSlowRunSpeedDurationMin"]),
		new(RollStore.Retaliation, 50, 20f, RollScope.All, [new("retaliationSlowRunSpeedModifier"), new("retaliationSlowRunSpeedDurationModifier")]),
		new(RollStore.Defense, 1, 20f, RollScope.AffixOnlyArmor, [new("defensiveProtection")]),
		new(RollStore.Defense, 2, 20f, RollScope.All, [new("defensiveProtectionModifier")]),
		new(RollStore.Defense, 3, 20f, RollScope.All, [new("defensiveAbsorptionModifier")]),
		new(RollStore.Defense, 4, 20f, RollScope.All, [new("defensivePhysical")]),
		new(RollStore.Defense, 5, 20f, RollScope.All, [new("defensivePhysicalModifier")]),
		new(RollStore.Defense, 6, 20f, RollScope.All, [new("defensivePhysicalDuration")]),
		new(RollStore.Defense, 7, 20f, RollScope.All, [new("defensivePhysicalDurationModifier")]),
		new(RollStore.Defense, 8, 20f, RollScope.All, [new("defensivePierce")]),
		new(RollStore.Defense, 9, 20f, RollScope.All, [new("defensivePierceModifier")]),
		new(RollStore.Defense, 10, 20f, RollScope.All, [new("defensiveBleeding")]),
		new(RollStore.Defense, 11, 20f, RollScope.All, [new("defensiveBleedingDuration")]),
		new(RollStore.Defense, 12, 20f, RollScope.All, [new("defensiveBleedingModifier")]),
		new(RollStore.Defense, 13, 20f, RollScope.All, [new("defensiveBleedingDurationModifier")]),
		new(RollStore.Defense, 14, 20f, RollScope.All, [new("defensiveElementalResistance")]),
		new(RollStore.Defense, 15, 20f, RollScope.All, [new("defensiveFire")]),
		new(RollStore.Defense, 16, 20f, RollScope.All, [new("defensiveFireDuration")]),
		new(RollStore.Defense, 17, 20f, RollScope.All, [new("defensiveFireModifier")]),
		new(RollStore.Defense, 18, 20f, RollScope.All, [new("defensiveFireDurationModifier")]),
		new(RollStore.Defense, 19, 20f, RollScope.All, [new("defensiveCold")]),
		new(RollStore.Defense, 20, 20f, RollScope.All, [new("defensiveColdDuration")]),
		new(RollStore.Defense, 21, 20f, RollScope.All, [new("defensiveColdModifier")]),
		new(RollStore.Defense, 22, 20f, RollScope.All, [new("defensiveColdDurationModifier")]),
		new(RollStore.Defense, 23, 20f, RollScope.All, [new("defensiveLightning")]),
		new(RollStore.Defense, 24, 20f, RollScope.All, [new("defensiveLightningDuration")]),
		new(RollStore.Defense, 25, 20f, RollScope.All, [new("defensiveLightningModifier")]),
		new(RollStore.Defense, 26, 20f, RollScope.All, [new("defensiveLightningDurationModifier")]),
		new(RollStore.Defense, 27, 20f, RollScope.All, [new("defensivePoison")]),
		new(RollStore.Defense, 28, 20f, RollScope.All, [new("defensivePoisonDuration")]),
		new(RollStore.Defense, 29, 20f, RollScope.All, [new("defensivePoisonModifier")]),
		new(RollStore.Defense, 30, 20f, RollScope.All, [new("defensivePoisonDurationModifier")]),
		new(RollStore.Defense, 31, 20f, RollScope.All, [new("defensiveLife")]),
		new(RollStore.Defense, 32, 20f, RollScope.All, [new("defensiveLifeDuration")]),
		new(RollStore.Defense, 33, 20f, RollScope.All, [new("defensiveLifeModifier")]),
		new(RollStore.Defense, 34, 20f, RollScope.All, [new("defensiveLifeDurationModifier")]),
		new(RollStore.Defense, 35, 20f, RollScope.All, [new("defensiveSlowLifeLeach")]),
		new(RollStore.Defense, 36, 20f, RollScope.All, [new("defensiveSlowLifeLeachDuration")]),
		new(RollStore.Defense, 37, 20f, RollScope.All, [new("defensiveSlowLifeLeachModifier")]),
		new(RollStore.Defense, 38, 20f, RollScope.All, [new("defensiveSlowLifeLeachDurationModifier")]),
		new(RollStore.Defense, 39, 20f, RollScope.All, [new("defensiveSlowManaLeach")]),
		new(RollStore.Defense, 40, 20f, RollScope.All, [new("defensiveSlowManaLeachDuration")]),
		new(RollStore.Defense, 41, 20f, RollScope.All, [new("defensiveSlowManaLeachModifier")]),
		new(RollStore.Defense, 42, 20f, RollScope.All, [new("defensiveSlowManaLeachDurationModifier")]),
		new(RollStore.Defense, 43, 20f, RollScope.All, [new("defensiveSleep")]),
		new(RollStore.Defense, 44, 20f, RollScope.All, [new("defensiveSleepModifier")]),
		new(RollStore.Defense, 45, 20f, RollScope.All, [new("defensiveStun")]),
		new(RollStore.Defense, 46, 20f, RollScope.All, [new("defensiveStunModifier")]),
		new(RollStore.Defense, 47, 20f, RollScope.All, [new("defensiveTrap")]),
		new(RollStore.Defense, 48, 20f, RollScope.All, [new("defensiveFreeze")]),
		new(RollStore.Defense, 49, 20f, RollScope.All, [new("defensivePetrify")]),
		new(RollStore.Defense, 50, 20f, RollScope.All, [new("defensiveDisruption")]),
		new(RollStore.Defense, 51, 20f, RollScope.All, [new("defensiveTaunt")]),
		new(RollStore.Defense, 52, 20f, RollScope.All, [new("defensiveFear")]),
		new(RollStore.Defense, 53, 20f, RollScope.All, [new("defensiveConfusion")]),
		new(RollStore.Defense, 54, 20f, RollScope.All, [new("defensiveBlockModifier")]),
		new(RollStore.Defense, 55, 20f, RollScope.All, [new("defensiveReflect")]),
		new(RollStore.Defense, 56, 20f, RollScope.All, [new("defensiveTotalSpeedResistance")]),
	];
}
