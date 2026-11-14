﻿//-----------------------------------------------------------------------
// <copyright file="ItemAttributes.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using TQVaultAE.Application.Contracts.Providers;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Data;

/// <summary>
/// Used to hold the Item Attributes
/// </summary>
public partial class ItemAttributeProvider : IItemAttributeProvider
{
	private readonly ILogger Log;

	public ItemAttributeProvider(ILogger<ItemAttributeProvider> log)
	{
		this.Log = log;
		this.attributeDictionary = InitializeAttributeDictionary();
	}

	#region ItemAttributes Fields

	/// <summary>
	/// Other Effects Tags
	/// </summary>
	private string[] otherEffects =
	[
		"characterBaseAttackSpeedTag",  // string text = TextTag(value)
		"levelRequirement",             // integer level requirement text = Format(TextTag(MeetsRequirement), TextTag(LevelRequirement), (float) value)
		"offensiveGlobalChance",        // float
		"retaliationGlobalChance",      // float
		"racialBonusPercentDamage",     // Added by VillageIdiot : float
		"racialBonusAbsoluteDefense",   // Added by VillageIdiot : int
		"itemSkillName",                // Added by VillageIdiot : string text (dbr)
		"skillName"						// Added by hgu : string text (dbr) for OneShot_Scroll_Eternal
	];

	/// <summary>
	/// Character Effects Tags
	/// No "Chance" variable
	/// Effect name is also the text tag
	/// </summary>
	private string[] characterEffects =
	[
		"characterStrength",
		"characterDexterity",
		"characterIntelligence",
		"characterLife",
		"characterMana",
		"characterStrengthModifier",
		"characterDexterityModifier",
		"characterIntelligenceModifier",
		"characterLifeModifier",
		"characterManaModifier",
		"characterIncreasedExperience",
		"characterRunSpeed",
		"characterAttackSpeed",
		"characterSpellCastSpeed",
		"characterRunSpeedModifier",
		"characterAttackSpeedModifier",
		"characterSpellCastSpeedModifier",
		"characterTotalSpeedModifier", // Added by VillageIdiot
		"characterLifeRegen",
		"characterManaRegen",
		"characterLifeRegenModifier",
		"characterManaRegenModifier",
		"characterOffensiveAbility",
		"characterDefensiveAbility",
		"characterOffensiveAbilityModifier",
		"characterDefensiveAbilityModifier",
		"characterDefensiveBlockRecoveryReduction",
		"characterEnergyAbsorptionPercent",
		"characterDodgePercent",
		"characterDeflectProjectile",
		"characterDeflectProjectiles",  // Do we need this??
		"characterManaLimitReserve",
		"characterManaLimitReserveReduction",
		"characterManaLimitReserveModifier",
		"characterManaLimitReserveReductionModifier",
		"characterGlobalReqReduction",
		"characterWeaponStrengthReqReduction",
		"characterWeaponDexterityReqReduction",
		"characterWeaponIntelligenceReqReduction",
		"characterMeleeStrengthReqReduction",
		"characterMeleeDexterityReqReduction",
		"characterMeleeIntelligenceReqReduction",
		"characterHuntingStrengthReqReduction",
		"characterHuntingDexterityReqReduction",
		"characterHuntingIntelligenceReqReduction",
		"characterStaffStrengthReqReduction",
		"characterStaffDexterityReqReduction",
		"characterStaffIntelligenceReqReduction",
		"characterShieldStrengthReqReduction",
		"characterShieldIntelligenceReqReduction",
		"characterShieldDexterityReqReduction",
		"characterArmorStrengthReqReduction",
		"characterArmorDexterityReqReduction",
		"characterArmorIntelligenceReqReduction",
		"characterJewelryStrengthReqReduction",
		"characterJewelryDexterityReqReduction",
		"characterJewelryIntelligenceReqReduction",
		"characterLevelReqReduction", // Added by VillageIdiot
		"skillCooldownReduction",
		"skillCooldownReductionModifier",
		"skillManaCostReduction",
		"skillManaCostReductionModifier",
		"projectileLaunchNumber", // Added by VillageIdiot
		"projectilePiercingChance", // Added by VillageIdiot
		"projectileLaunchRotation", // Added by VillageIdiot
		"skillLifeBonus" // Added by VillageIdiot
	];

	/// <summary>
	/// Defensive Effect Tags
	/// Change "defensive" to "Defense" to get the text tag
	/// Look for "Chance" variable to get the chance of this effect.
	/// </summary>
	private string[] defenseEffects =
	[
		"defensiveProtection",
		"defensiveProtectionModifier",
		"defensiveAbsorption",
		"defensiveAbsorptionModifier",
		"defensivePhysical",
		"defensivePhysicalDuration",
		"defensivePhysicalDurationChanceModifier",
		"defensivePhysicalDurationModifier",
		"defensivePhysicalModifier",
		"defensivePierce",
		"defensivePierceDuration",
		"defensivePierceDurationModifier",
		"defensivePierceModifier",
		"defensiveFire",
		"defensiveFireDuration",
		"defensiveFireDurationModifier",
		"defensiveFireModifier",
		"defensiveCold",
		"defensiveColdDuration",
		"defensiveColdDurationModifier",
		"defensiveColdModifier",
		"defensiveLightning",
		"defensiveLightningDuration",
		"defensiveLightningDurationModifier",
		"defensiveLightningModifier",
		"defensivePoison",
		"defensivePoisonDuration",
		"defensivePoisonDurationModifier",
		"defensivePoisonModifier",
		"defensiveLife",
		"defensiveLifeDuration",
		"defensiveLifeDurationModifier",
		"defensiveLifeModifier",
		"defensiveDisruption", // Added by VillageIdiot
		"defensiveElemental",
		"defensiveElementalModifier",
		"defensiveElementalResistance",
		"defensiveSlowLifeLeach",
		"defensiveSlowLifeLeachDuration",
		"defensiveSlowLifeLeachDurationModifier",
		"defensiveSlowLifeLeachModifier",
		"defensiveSlowManaLeach",
		"defensiveSlowManaLeachDuration",
		"defensiveSlowManaLeachDurationModifier",
		"defensiveSlowManaLeachModifier",
		"defensiveBleeding", // Added by VillageIdiot
		"defensiveBleedingDuration", // Added by VillageIdiot
		"defensiveBleedingDurationModifier", // Added by VillageIdiot
		"defensiveBleedingModifier", // Added by VillageIdiot
		"defensiveBlockModifier",
		"defensiveReflect",
		"defensiveConfusion", // Added by VillageIdiot
		"defensiveTaunt", // Added by VillageIdiot
		"defensiveFear", // Added by VillageIdiot
		"defensiveConvert", // Added by VillageIdiot
		"defensiveTrap", // Added by VillageIdiot
		"defensivePetrify", // Added by VillageIdiot
		"defensiveFreeze", // Added by VillageIdiot
		"defensiveStun",
		"defensiveStunModifier",
		"defensiveSleep", // Added by VillageIdiot
		"defensiveSleepModifier", // Added by VillageIdiot
		"defensiveManaBurnRatio", // Added by VillageIdiot
		"defensivePercentCurrentLife", // Added by VillageIdiot
		"defensiveTotalSpeedResistance", // Added by VillageIdiot
		"damageAbsorption", // Added by VillageIdiot
		"damageAbsorptionPercent",  // Added by VillageIdiot
		"defensiveTotalSpeedChance"
	];

	/// <summary>
	/// Offensive Effects Tags
	/// Change "offensive" to "Damage" to get the text tag.
	/// </summary>
	private string[] offensiveEffects =
	[
		"offensiveBasePhysical",
		"offensiveBaseCold",
		"offensiveBaseFire",
		"offensiveBasePoison", // Added by VillageIdiot
		"offensiveBaseLightning",
		"offensiveBaseLife", // Added by VillageIdiot
		"offensivePhysical",
		"offensivePierceRatio",
		"offensivePierce",
		"offensiveCold",
		"offensiveFire",
		"offensivePoison",
		"offensiveLightning",
		"offensiveLife",
		"offensivePercentCurrentLife",
		"offensiveManaBurn", // Added by VillageIdiot
		"offensiveDisruption", // Added by VillageIdiot
		"offensiveLifeLeech",
		"offensiveElemental",
		"offensiveTotalDamageReductionPercent", // Added by VillageIdiot
		"offensiveTotalDamageReductionAbsolute", // Added by VillageIdiot
		"offensiveTotalResistanceReductionPercent", // Added by VillageIdiot
		"offensiveTotalResistanceReductionAbsolute", // Added by VillageIdiot
		"offensiveFumble", // Added by VillageIdiot
		"offensiveProjectileFumble", // Added by VillageIdiot
		"offensiveConvert",
		"offensiveTaunt",
		"offensiveFear",
		"offensiveConfusion",
		"offensiveTrap", // Added by VillageIdiot
		"offensiveFreeze", // Added by VillageIdiot
		"offensivePetrify", // Added by VillageIdiot
		"offensiveStun",
		"offensiveSleep", // Added by VillageIdiot
		"offensiveBonusPhysical"
	];

	/// <summary>
	/// Offensive Effects Variables Tags
	/// </summary>
	private string[] offensiveEffectVariables =
	[
		"Min",
		"Max",
		"Chance",
		"XOR",         // boolean
		"Global",      // boolean
		"DurationMin", // Added by VillageIdiot
		"DurationMax",
		"DrainMin",    // Added by VillageIdiot
		"DrainMax",    // Added by VillageIdiot
		"DamageRatio"  // Added by VillageIdiot
	];

	/// <summary>
	/// Offensive Modifier Effect Tags
	/// Change "offensive" to "DamageModifier" and remove trailing "Modifier" to get the text tag.
	/// Look for "Chance" variable to get the chance of this effect.
	/// </summary>
	private string[] offensiveModifierEffects =
	[
		"offensivePhysicalModifier",
		"offensivePierceRatioModifier",
		"offensivePierceModifier",
		"offensiveColdModifier",
		"offensiveFireModifier",
		"offensivePoisonModifier",
		"offensiveLightningModifier",
		"offensiveLifeModifier",
		"offensiveManaBurnRatioAdder",   // Added by VillageIdiot
		"offensiveElementalModifier",
		"offensiveTotalDamageModifier",  // Added by VillageIdiot
		"offensiveStunModifier",
		"offensiveSleepModifier",        // Added by VillageIdiot
		"skillProjectileSpeedModifier",  // Added by VillageIdiot
		"sparkMaxNumber"                 // Added by VillageIdiot
	];

	/// <summary>
	/// Offensive Slow Effects Tags
	/// Change "offensiveSlow" to "DamageDuration" to get the text tag.
	/// </summary>
	private string[] offensiveSlowEffects =
	[
		"offensiveSlowPhysical",
		"offensiveSlowBleeding",
		"offensiveSlowCold",
		"offensiveSlowFire",
		"offensiveSlowPoison",
		"offensiveSlowLightning",
		"offensiveSlowLife",
		"offensiveSlowTotalSpeed",
		"offensiveSlowAttackSpeed",
		"offensiveSlowRunSpeed",
		"offensiveSlowLifeLeach",
		"offensiveSlowManaLeach",
		"offensiveSlowOffensiveAbility",
		"offensiveSlowDefensiveAbility",
		"offensiveSlowOffensiveReduction",
		"offensiveSlowDefensiveReduction"
	];

	/// <summary>
	/// Offensive Slow Effect Variables Tags
	/// </summary>
	private string[] offensiveSlowEffectVariables =
	[
		"Min",
		"Max",
		"DurationMin",
		"DurationMax",
		"Chance",
		"XOR",         // boolean
		"Global"       // boolean
	];

	/// <summary>
	/// Offensive Slow Modifier Effects Tags
	/// Replace offensiveSlow with DamageDurationModifier to get the text tag
	/// </summary>
	private string[] offensiveSlowModifierEffects =
	[
		"offensiveSlowPhysical",
		"offensiveSlowBleeding",
		"offensiveSlowCold",
		"offensiveSlowFire",
		"offensiveSlowPoison",
		"offensiveSlowLightning",
		"offensiveSlowLife",
		"offensiveSlowTotalSpeed",
		"offensiveSlowAttackSpeed",
		"offensiveSlowRunSpeed",
		"offensiveSlowLifeLeach",
		"offensiveSlowManaLeach",
		"offensiveSlowOffensiveAbility",   // messes things up
		"offensiveSlowDefensiveAbility",   // messes things up
		"offensiveSlowOffensiveReduction", // messes things up
		"offensiveSlowDefensiveReduction"  // messes things up
	];

	/// <summary>
	/// Offensive Slow Modifer Effect Variables Tags
	/// </summary>
	private string[] offensiveSlowModifierEffectVariables =
	[
		"DurationModifier",
		"Modifier",
		"ModifierChance"
	];

	/// <summary>
	/// Retaliation Effects Tags
	/// Replace "retaliation" with "Retaliation" to get text tag
	/// </summary>
	private string[] retaliationEffects =
	[
		"retaliationPhysical",
		"retaliationPierceRatio",
		"retaliationPierce",
		"retaliationCold",
		"retaliationFire",
		"retaliationPoison",
		"retaliationLightning",
		"retaliationLife",
		"retaliationStun",
		"retaliationPercentCurrentLife",
		"retaliationElemental"
	];

	/// <summary>
	/// Retaliation Effect Variables Tags
	/// </summary>
	private string[] retaliationEffectVariables =
	[
		"Chance",
		"Max",
		"Min",
		"Global",   // boolean
		"XOR"       // boolean
	];

	/// <summary>
	/// Retaliation Modifier Effects Tags
	/// Replace "retaliation" with "RetaliationModifier" and remove trailing "Modifier" to get text tag.
	/// Look for "Chance" variable to get the chance of this effect.
	/// </summary>
	private string[] retaliationModifierEffects =
	[
		"retaliationPhysicalModifier",
		"retaliationPierceRatioModifier",
		"retaliationPierceModifier",
		"retaliationColdModifier",
		"retaliationFireModifier",
		"retaliationPoisonModifier",
		"retaliationLightningModifier",
		"retaliationLifeModifier",
		"retaliationStunModifier",
		"retaliationElementalModifier"
	];

	/// <summary>
	/// Retaliation Slow Effects Tags
	/// Replace "retaliationSlow" with "RetaliationDuration" to get text tag
	/// </summary>
	private string[] retaliationSlowEffects =
	[
		"retaliationSlowPhysical",
		"retaliationSlowBleeding",
		"retaliationSlowCold",
		"retaliationSlowFire",
		"retaliationSlowPoison",
		"retaliationSlowLightning",
		"retaliationSlowLife",
		"retaliationSlowTotalSpeed",
		"retaliationSlowAttackSpeed",
		"retaliationSlowRunSpeed",
		"retaliationSlowLifeLeach",
		"retaliationSlowManaLeach",
		"retaliationSlowOffensiveAbility",
		"retaliationSlowDefensiveAbility",
		"retaliationSlowOffensiveReduction"
	];

	/// <summary>
	/// Retaliation Slow Effect Variables Tags
	/// </summary>
	private string[] retaliationSlowEffectVariables =
	[
		"Chance",
		"Max",
		"Min",
		"DurationMax",
		"DurationMin",
		"Global",      // boolean
		"XOR"          // boolean
	];

	/// <summary>
	/// Retaliation Slow Modifier Effects Tags
	/// Replace "retaliationSlow" with "RetaliationDurationModifier" to get text tag
	/// </summary>
	private string[] retaliationSlowModifierEffects =
	[
		"retaliationSlowPhysical",
		"retaliationSlowBleeding",
		"retaliationSlowCold",
		"retaliationSlowFire",
		"retaliationSlowPoison",
		"retaliationSlowLightning",
		"retaliationSlowLife",
		"retaliationSlowTotalSpeed",
		"retaliationSlowAttackSpeed",
		"retaliationSlowRunSpeed",
		"retaliationSlowLifeLeach",
		"retaliationSlowManaLeach",
		"retaliationSlowOffensiveAbility",
		"retaliationSlowDefensiveAbility",
		"retaliationSlowOffensiveReduction"
	];

	/// <summary>
	/// Retaliation Slow Modifier Effect Variables Tags
	/// </summary>
	private string[] retaliationSlowModifierEffectVariables =
	[
		"Modifier",
		"ModifierChance",
		"DurationModifier",
	];

	/// <summary>
	/// Reagents for formulae
	/// </summary>
	private string[] reagents =
	[
		"reagent1BaseName",
		"reagent2BaseName",
		"reagent3BaseName"
	];

	/// <summary>
	/// For skill parameters (duration, radius, angle, etc.)
	/// </summary>
	private string[] skillEffects =
	[
		"skillManaCost",
		"skillActiveManaCost",
		"skillActiveLifeCost",
		"skillChanceWeight",
		"skillActiveDuration",
		"skillTargetRadius",
		"projectileExplosionRadius",
		"explosionRadius",
		"skillTargetAngle",
		"skillTargetNumber",
		"skillChargeDuration",
		"skillChargeLevel",
		"piercingProjectile",
		"projectilePiercing",
		"headVelocity",
		"maxDistance",
		"tailVelocity",
		"refreshTime",
		"skillCooldownTime"
	];

	/// <summary>
	/// For grouping the "Protects Against:" damage qualifiers.
	/// </summary>
	private string[] damageQualifierEffects =
	[
		"physicalDamageQualifier",
		"pierceDamageQualifier",
		"lightningDamageQualifier",
		"fireDamageQualifier",
		"coldDamageQualifier",
		"poisonDamageQualifier",
		"lifeDamageQualifier",
		"bleedingDamageQualifier",
		"elementalDamageQualifier"
	];

	/// <summary>
	/// For shield parameters (defensiveBlock and blockRecoveryTime)
	/// </summary>
	private string[] shieldEffects =
	[
		"defensiveBlock",
		////"blockAbsorption",  // This one is suppressed
		"blockRecoveryTime"
	];

	/// <summary>
	/// Dictionary holding all of the attributes
	/// </summary>
	private Dictionary<string, ItemAttributesData> attributeDictionary;

	#endregion ItemAttributes Fields

	#region ItemAttribute Public Methods

	/// <summary>
	/// Indicates whether the name is a reagent
	/// </summary>
	/// <param name="name">name to be tested</param>
	/// <returns>true if a reagent</returns>
	public bool IsReagent(string name) => Array.IndexOf(reagents, name) != -1;

	/// <summary>
	/// Gets data for an attibute string.
	/// </summary>
	/// <param name="attribute">attribute string.  Internally normalized to UpperInvariant.</param>
	/// <returns>ItemAttributesData for the attribute</returns>
	public ItemAttributesData? GetAttributeData(string attribute)
	{
		if (String.IsNullOrEmpty(attribute))
			return null;

		return attributeDictionary.TryGetValue(attribute, out var value) ? value : null;
	}

	[GeneratedRegex(@"%(?<precis>(?<sign>[+-])?\.(?<numDecimal>\d)?)?(?<alpha>[sdft])(?<formatNumber>\d)")]
	private static partial Regex ConvertFormatRegEx();
	/// <summary>
	/// Converts format string from TQ format to string.format
	/// </summary>
	/// <param name="formatValue">format string to be parsed</param>
	/// <returns>updated format string</returns>
	public string ConvertFormat(string formatValue)
		=> this._convertedFormats.GetOrAdd(formatValue, ConvertFormatUncached);

	/// <summary>
	/// <see cref="ConvertFormat"/> results by TQ format. The formats come from the game translations, so there are few of
	/// them, while each displayed attribute converted its own with two regular expressions: most of the allocations of the
	/// search warm-up.
	/// </summary>
	private readonly ConcurrentDictionary<string, string> _convertedFormats = new(StringComparer.Ordinal);

	private static string ConvertFormatUncached(string formatValue)
	{
		// Local func
		string replaceMatch(Match m)
		{
			var precis = m.Groups["precis"].Value;
			var sign = m.Groups["sign"].Value;
			var numDecimal = m.Groups["numDecimal"].Value;
			var alpha = m.Groups["alpha"].Value;
			var formatNumber = m.Groups["formatNumber"].Value;

			if (alpha.Equals("d", StringComparison.Ordinal) || alpha.Equals("f", StringComparison.Ordinal))
			{
				// a number
				if (precis.Length == 0)
					// simple
					return $@"[{formatNumber}]";

				// see if they listed a decimal precision
				string decimalSpec = string.Empty;
				if (numDecimal.Length != 0)
				{
					// Sometimes the parsing would cause a format exception with 0
					// Use TryParse to handle the exception.
					if (!int.TryParse(numDecimal, out var numDecimalParsed))
						numDecimalParsed = 0;

					if (numDecimalParsed > 0)
						decimalSpec = ".".PadRight(numDecimalParsed + 1, '0');
				}

				// "%+": the sign of the number, '+' included, as printf writes it. A single section "+#0" would write a
				// negative number "-+5", so negative numbers get their own section. "%-" only aligns left: no sign added.
				return sign == "+"
					? $"[{formatNumber}:+#0{decimalSpec};-#0{decimalSpec}]"
					: $"[{formatNumber}:#0{decimalSpec}]";
			}
			else
				// string
				return $"[{formatNumber}]";
		}

		// Escape TQMarking changing "{^.}" to "[^.]"
		formatValue = TQColorHelper.RegExTQTagInstance().Replace(formatValue
			, @"[^${ColorId}]"
		);

		// Takes a TQ Format string and converts it to a .NET Format string using regex.
		var newformat = ConvertFormatRegEx().Replace(formatValue, replaceMatch);

		// Remove residual irrelevant {} on some format
		newformat = newformat.Split('{', '}').JoinString("");

		// Reactivate string.Format markup
		newformat = newformat.Replace("[", "{").Replace("]", "}");

		// Escape TQTags by doubling {}
		newformat = TQColorHelper.RegExTQTagInstance().Replace(newformat
			, @"{${ColorTag}}"
		);

		return newformat;
	}

	/// <summary>
	/// Gets the effect tag string
	/// </summary>
	/// <param name="data">attribute data</param>
	/// <returns>string containing the effect tag</returns>
	public string GetAttributeTextTag(ItemAttributesData data)
	{
		string result = string.Empty;

		if (data is null)
			return result;

		if (string.IsNullOrEmpty(data.Effect))
			return result;

		switch (data.EffectType)
		{
			case ItemAttributesEffectType.ShieldEffect:
				return GetShieldEffectTextTag(data.Effect);

			case ItemAttributesEffectType.Character:
				return GetCharacterEffectTextTag(data.Effect);

			case ItemAttributesEffectType.Defense:
				return GetDefenseEffectTextTag(data.Effect);

			case ItemAttributesEffectType.Offense:
				return GetOffensiveEffectTextTag(data.Effect);

			case ItemAttributesEffectType.OffenseModifier:
				return GetOffensiveModifierEffectTextTag(data.Effect);

			case ItemAttributesEffectType.OffenseSlow:
				return GetOffensiveSlowEffectTextTag(data.Effect);

			case ItemAttributesEffectType.OffenseSlowModifier:
				return GetOffensiveSlowModifierEffectTextTag(data.Effect);

			case ItemAttributesEffectType.Other:
				return data.Effect;

			case ItemAttributesEffectType.Retaliation:
				return GetRetaliationEffectTextTag(data.Effect);

			case ItemAttributesEffectType.RetaliationModifier:
				return GetRetaliationModifierEffectTextTag(data.Effect);

			case ItemAttributesEffectType.RetaliationSlow:
				return GetRetaliationSlowEffectTextTag(data.Effect);

			case ItemAttributesEffectType.RetaliationSlowModifier:
				return GetRetaliationSlowModifierEffectTextTag(data.Effect);

			case ItemAttributesEffectType.Reagent:
				return data.Effect;

			case ItemAttributesEffectType.SkillEffect:
				return GetSkillEffectTextTag(data.Effect);

			default:
				return data.FullAttribute;
		}
	}

	/// <summary>
	/// Gets the effect tag string
	/// </summary>
	/// <param name="attribute">attribute string</param>
	/// <returns>effect tag string</returns>
	public string GetAttributeTextTag(string attribute)
	{
		ItemAttributesData? data = GetAttributeData(attribute);
		if (data is null)
			return attribute;

		return GetAttributeTextTag(data);
	}

	/// <summary>
	/// Indicates whether an attibute has a particular variable name
	/// </summary>
	/// <param name="variable">attribute variable</param>
	/// <param name="variableName">string for variable name</param>
	/// <returns>true if the variable == variable name</returns>
	public bool AttributeHas(Variable? variable, string variableName)
	{
		if (variable is null)
			return false;

		ItemAttributesData? data = this.GetAttributeData(variable.Name);
		if (data is null)
			return false;

		return string.Equals(data.Variable, variableName, StringComparison.OrdinalIgnoreCase);
	}

	/// <summary>
	/// Indicates whether an effect is part of a particular attribute group
	/// </summary>
	/// <param name="attributeList">Array of attributes</param>
	/// <param name="effect">effect string to be tested</param>
	/// <returns>true if attribute effect in group == effect</returns>
	public bool AttributeGroupIs(IReadOnlyList<Variable?> attributeList, string effect)
	{
		if (attributeList[0] is not { } variable)
			return false;

		ItemAttributesData? data = this.GetAttributeData(variable.Name);
		if (data is null)
			return false;

		return string.Equals(data.Effect, effect, StringComparison.OrdinalIgnoreCase);
	}

	/// <summary>
	/// Indicates whether an effect type is part of a particular attribute group
	/// </summary>
	/// <param name="attributeList">Array of attributes</param>
	/// <param name="type">Effect type enumeration</param>
	/// <returns>true if attribute effect in group == type</returns>
	public bool AttributeGroupIs(IReadOnlyList<Variable?> attributeList, ItemAttributesEffectType type)
	{
		if (attributeList[0] is not { } variable)
			return false;

		ItemAttributesData? data = this.GetAttributeData(variable.Name);
		if (data is null)
			return false;

		return data.EffectType == type;
	}

	/// <summary>
	/// Inidicates whether an attribute group has a particular variable name
	/// </summary>
	/// <param name="attributeList">array of attributes</param>
	/// <param name="variableName">name of variable</param>
	/// <returns>true if variable is present in the list</returns>
	public bool AttributeGroupHas(IReadOnlyList<Variable?> attributeList, string variableName)
	{
		foreach (Variable? variable in attributeList)
		{
			if (AttributeHas(variable, variableName))
				return true;
		}

		return false;
	}

	#endregion ItemAttribute Public Methods

	#region ItemAttributes Private Methods

	/// <summary>
	/// Gets Modified character effect tag.  Normalized to UpperInvariant.
	/// </summary>
	/// <param name="effect">effect to modify</param>
	/// <returns>modified character effect text tag</returns>
	private string GetCharacterEffectTextTag(string effect)
	{
		if (effect.Equals("CHARACTERGLOBALREQREDUCTION", StringComparison.OrdinalIgnoreCase))
			// awful mispelling by IL.
			return "CharcterItemGlobalReduction";

		else if (effect.Equals("CHARACTERDEFLECTPROJECTILE", StringComparison.OrdinalIgnoreCase))
			return "CharacterDeflectProjectiles";

		// Just return as-is since the effect is the text tag with the first letter capitalized.
		return effect;
	}

	/// <summary>
	/// Gets Modifier Shield Effect Text Tag.  Normalized to UpperInvariant.
	/// </summary>
	/// <param name="effect">effect tag to modify</param>
	/// <returns>modified effect text tag</returns>
	private string GetShieldEffectTextTag(string effect)
	{
		if (effect.Equals("BLOCKRECOVERYTIME", StringComparison.OrdinalIgnoreCase))
			return "ShieldBlockRecoveryTime";

		// Replace Defensive with Defense.
		return SpanHelper.ConcatWithSlice("Defense", effect.AsSpan(), 9);
	}

	/// <summary>
	/// Gets modified defense effect text tag.  Normalized to UpperInvariant.
	/// </summary>
	/// <param name="effect">effect tag to modify</param>
	/// <returns>modified effect text tag</returns>
	private string GetDefenseEffectTextTag(string effect)
	{
		// database orphan tag with no substitute => return as is
		if (effect.Equals("defensiveTotalSpeedChance", StringComparison.OrdinalIgnoreCase)
		    || effect.Equals("defensiveAbsorption", StringComparison.OrdinalIgnoreCase))
			return effect;

		// Check for specific strings.
		switch (effect.ToUpperInvariant())
		{
			case "DEFENSIVEPROTECTION":
				return "DefenseAbsorptionProtection";

			case "DEFENSIVESLEEP":
				return SpanHelper.ConcatWithSlice("xtagDefense", effect.AsSpan(), 9);

			case "DEFENSIVETOTALSPEEDRESISTANCE":
				return "xtagTotalSpeedResistance";

			case "DAMAGEABSORPTION":
				return "SkillDamageAbsorption";

			case "DAMAGEABSORPTIONPERCENT":
				return "SkillDamageAbsorptionPercent";
		}

		// Replace DefensiveSlow with Defense
		if (effect.StartsWith("DEFENSIVESLOW", StringComparison.OrdinalIgnoreCase))
			return SpanHelper.ConcatWithSlice("Defense", effect.AsSpan(), 13);

		// Otherwise replace Defensive with Defense.
		return SpanHelper.ConcatWithSlice("Defense", effect.AsSpan(), 9);
	}

	/// <summary>
	/// Gets modified offensive effect text tag.  Normalized to UpperInvariant.
	/// </summary>
	/// <param name="effect">effect tag to modify</param>
	/// <returns>modified effect text tag</returns>
	private string GetOffensiveEffectTextTag(string effect)
	{
		// Check for a skill and return as-is.
		if (effect.StartsWith("SKILL", StringComparison.OrdinalIgnoreCase))
			return effect;

		// Check for specific strings
		switch (effect.ToUpperInvariant())
		{
			case "OFFENSIVEPHYSICAL":
			case "OFFENSIVEBASEPHYSICAL":
				return "DamageBasePhysical";

			case "OFFENSIVEPIERCERATIO":
				return "DamageBasePierceRatio";

			case "OFFENSIVEMANABURN":
				return "DamageManaDrain";

			case "OFFENSIVESLEEP":
				return "xtagDamageSleep";

			case "OFFENSIVEFUMBLE":
				return "DamageDurationFumble";

			case "OFFENSIVEPROJECTILEFUMBLE":
				return "DamageDurationProjectileFumble";
		}

		// Check for OffensiveBase and replace with Damage.
		if (effect.StartsWith("OFFENSIVEBASE", StringComparison.OrdinalIgnoreCase))
			return SpanHelper.ConcatWithSlice("Damage", effect.AsSpan(), 13);

		// Otherwise replace Offensive with Damage.
		return SpanHelper.ConcatWithSlice("Damage", effect.AsSpan(), 9);
	}

	/// <summary>
	/// Gets modified offensive modifier effect text tag.  Normalized to UpperInvariant.
	/// </summary>
	/// <param name="effect">effect tag to modify</param>
	/// <returns>modified effect text tag</returns>
	private string GetOffensiveModifierEffectTextTag(string effect)
	{
		// Check for specific strings.
		switch (effect.ToUpperInvariant())
		{
			case "OFFENSIVEMANABURNRATIOADDER":
				return "DamageModifierManaBurn";

			case "OFFENSIVESLEEPMODIFIER":
				return "xtagDamageModifierSleep";

			case "OFFENSIVETOTALDAMAGEMODIFIER":
				return "xtagDamageModifierTotalDamage";

			case "SPARKMAXNUMBER":
				return "xtagSparkMaxNumber";
		}

		// Check for a skill and return as-is.
		if (effect.StartsWith("SKILL", StringComparison.OrdinalIgnoreCase))
			return effect;

		// Just replace Offensive with DamageModifier and remove trailing modifier.
		// "offensiveXXXModifier" -> "DamageModifierXXX" (skip 9 chars, keep (len-17) chars)
		int suffixLength = effect.Length - 17;
		return SpanHelper.ConcatWithSlice("DamageModifier", effect.AsSpan(), 9, suffixLength);
	}

	/// <summary>
	/// Gets modified offensive slow effect text tag.  Normalized to UpperInvariant.
	/// </summary>
	/// <param name="effect">effect tag to modify</param>
	/// <returns>modified effect text tag</returns>
	private string GetOffensiveSlowEffectTextTag(string effect)
		// Change offensiveSlow to DamageDuration.
		=> SpanHelper.ConcatWithSlice("DamageDuration", effect.AsSpan(), 13);

	/// <summary>
	/// Gets modifier offensive slow modifier effect text tag.  Normalized to UpperInvariant.
	/// </summary>
	/// <param name="effect">effect tag to modify</param>
	/// <returns>modified effect text tag</returns>
	private string GetOffensiveSlowModifierEffectTextTag(string effect)
		// Change offensiveSlow to DamageDurationModifier.
		=> SpanHelper.ConcatWithSlice("DamageDurationModifier", effect.AsSpan(), 13);

	/// <summary>
	/// Gets modifier retaliation effect text tag.  Normalized to UpperInvariant.
	/// </summary>
	/// <param name="effect">effect tag to modify</param>
	/// <returns>modified effect text tag</returns>
	private string GetRetaliationEffectTextTag(string effect)
		// No need to replace retaliation with Retaliation.
		=> effect;

	/// <summary>
	/// Gets modifier retaliation modifier effect text tag.  Normalized to UpperInvariant.
	/// </summary>
	/// <param name="effect">effect tag to modify</param>
	/// <returns>modified effect text tag</returns>
	private string GetRetaliationModifierEffectTextTag(string effect)
		// Replace retaliation with RetaliationModifier and remove trailing Modifier.
		// "retaliationXXXModifier" -> "RetaliationModifierXXX" (skip 11 chars, keep (len-19) chars)
		=> SpanHelper.ConcatWithSlice("RetaliationModifier", effect.AsSpan(), 11, effect.Length - 19);

	/// <summary>
	/// Gets modified retaliation slow effect text tag.  Normalized to UpperInvariant.
	/// </summary>
	/// <param name="effect">effect tag to modify</param>
	/// <returns>modified effect text tag</returns>
	private string GetRetaliationSlowEffectTextTag(string effect)
		// Replace retaliationSlow with RetaliationDuration.
		=> SpanHelper.ConcatWithSlice("RetaliationDuration", effect.AsSpan(), 15);

	/// <summary>
	/// Gets modified retaliation slow modifier effect text tag.  Normalized to UpperInvariant.
	/// </summary>
	/// <param name="effect">effect tag to modify</param>
	/// <returns>modified effect text tag</returns>
	private string GetRetaliationSlowModifierEffectTextTag(string effect)
		// Replace retaliationSlow with RetaliationDurationModifier.
		=> SpanHelper.ConcatWithSlice("RetaliationDurationModifier", effect.AsSpan(), 15);


	string[] _GetSkillEffectTextTagUnchanged = [
		"SkillChanceWeight",
		"headVelocity",
		"maxDistance",
		"tailVelocity",
	];
	/// <summary>
	/// Gets modifier skill effect text tag.  Normalized to UpperInvariant.
	/// </summary>
	/// <param name="effect">effect tag to modify</param>
	/// <returns>modified effect text tag</returns>
	private string GetSkillEffectTextTag(string effect)
	{
		// Return effect as-is.
		if (_GetSkillEffectTextTagUnchanged.Any(s => s.Equals(effect, StringComparison.OrdinalIgnoreCase)))
			return effect;

		// Misspelling
		if (effect.Equals("SKILLCHARGEDURATION", StringComparison.OrdinalIgnoreCase))
			return "SkillChargeDurationMod";
		// The number of charge levels: "7 Charge Levels" (the game's SkillChargeMax).
		if (effect.Equals("SKILLCHARGELEVEL", StringComparison.OrdinalIgnoreCase))
			return "SkillChargeMax";
		// The chance to pass through enemies, under both of its record names.
		if (effect.Equals("PIERCINGPROJECTILE", StringComparison.OrdinalIgnoreCase)
			|| effect.Equals("PROJECTILEPIERCING", StringComparison.OrdinalIgnoreCase))
			return "ProjectilePiercingChance";
		if (effect.Equals("EXPLOSIONRADIUS", StringComparison.OrdinalIgnoreCase))
			return "ExplosionRadius";
		if (effect.Equals("REFRESHTIME", StringComparison.OrdinalIgnoreCase))
			return "tagSkillRefreshTime";

		// Strip off Projectile from the text tag.
		if (effect.StartsWith("PROJECTILE", StringComparison.OrdinalIgnoreCase))
			return SpanHelper.ConcatWithSlice("", effect.AsSpan(), 10);

		// Otherwise strip off Skill from the tag.
		return SpanHelper.ConcatWithSlice("", effect.AsSpan(), 5);
	}


	/// <summary>
	/// Initialize all of my arrays
	/// </summary>
	/// <returns>hash table of attributes</returns>
	private Dictionary<string, ItemAttributesData> InitializeAttributeDictionary()
	{
		// Attribute names are looked up whatever their case.
		attributeDictionary = new(StringComparer.OrdinalIgnoreCase);

		// Add all the attributes to the dictionary
		AddEach(otherEffects, ItemAttributesEffectType.Other);
		// Added by VillageIdiot
		AddMinAndChance(shieldEffects, ItemAttributesEffectType.ShieldEffect);
		AddMinAndChance(characterEffects, ItemAttributesEffectType.Character);
		AddMinAndChance(defenseEffects, ItemAttributesEffectType.Defense);
		AddWithVariables(offensiveEffects, offensiveEffectVariables, ItemAttributesEffectType.Offense);
		AddMinAndChance(offensiveModifierEffects, ItemAttributesEffectType.OffenseModifier);
		AddWithVariables(offensiveSlowEffects, offensiveSlowEffectVariables, ItemAttributesEffectType.OffenseSlow);
		AddWithVariables(offensiveSlowModifierEffects, offensiveSlowModifierEffectVariables, ItemAttributesEffectType.OffenseSlowModifier);
		AddWithVariables(retaliationEffects, retaliationEffectVariables, ItemAttributesEffectType.Retaliation);
		AddMinAndChance(retaliationModifierEffects, ItemAttributesEffectType.RetaliationModifier);
		AddWithVariables(retaliationSlowEffects, retaliationSlowEffectVariables, ItemAttributesEffectType.RetaliationSlow);
		AddWithVariables(retaliationSlowModifierEffects, retaliationSlowModifierEffectVariables, ItemAttributesEffectType.RetaliationSlowModifier);
		// Added by VillageIdiot
		AddEach(damageQualifierEffects, ItemAttributesEffectType.DamageQualifierEffect);
		AddEach(reagents, ItemAttributesEffectType.Reagent);
		AddEach(skillEffects, ItemAttributesEffectType.SkillEffect);

		return attributeDictionary;

		// Each effect under its own name; the sub order is its place in its list.
		void AddEach(IEnumerable<string> effects, ItemAttributesEffectType type)
		{
			int subOrder = 0;
			foreach (string effect in effects)
				attributeDictionary.Add(effect, new ItemAttributesData(type, effect, effect, string.Empty, subOrder++));
		}

		// Each effect as its Min value, and with "Chance" appended as its Chance value.
		void AddMinAndChance(IEnumerable<string> effects, ItemAttributesEffectType type)
		{
			int subOrder = 0;
			foreach (string effect in effects)
			{
				attributeDictionary.Add(effect, new ItemAttributesData(type, effect, effect, "Min", subOrder));
				attributeDictionary.Add(string.Concat(effect, "Chance"), new ItemAttributesData(type, effect, effect, "Chance", subOrder));
				++subOrder;
			}
		}

		// Each effect followed by each of its variables (Min, Max, Chance...), the effect's place as the sub order.
		void AddWithVariables(IEnumerable<string> effects, IEnumerable<string> variables, ItemAttributesEffectType type)
		{
			int subOrder = 0;
			foreach (string effect in effects)
			{
				foreach (string variable in variables)
				{
					attributeDictionary.Add(
						string.Concat(effect, variable)
						, new ItemAttributesData(type, string.Concat(effect, variable), effect, variable, subOrder)
					);
				}

				++subOrder;
			}
		}
	}

	#endregion ItemAttributes Private Methods
}