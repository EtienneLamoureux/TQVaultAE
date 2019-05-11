//-----------------------------------------------------------------------
// <copyright file="ItemAttributes.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.DAL
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Globalization;
	using System.Text;

	/// <summary>
	/// Enumeration of the Attribute Effect Types
	/// </summary>
	public enum ItemAttributesEffectType
	{
		/// <summary>
		/// Shield Effects
		/// </summary>
		ShieldEffect = 0,

		/// <summary>
		/// Skill Effects
		/// </summary>
		SkillEffect,

		/// <summary>
		/// Offensive Effects
		/// </summary>
		Offense,

		/// <summary>
		/// Offensive Modifier Effects
		/// </summary>
		OffenseModifier,

		/// <summary>
		/// Offensive Slow Effects
		/// </summary>
		OffenseSlow,

		/// <summary>
		/// Offensive Slow Modifier Effects
		/// </summary>
		OffenseSlowModifier,

		/// <summary>
		/// Retaliation Effects
		/// </summary>
		Retaliation,

		/// <summary>
		/// Retaliation Modifier Effects
		/// </summary>
		RetaliationModifier,

		/// <summary>
		/// Retaliation Slow Effects
		/// </summary>
		RetaliationSlow,

		/// <summary>
		/// Retaliation Slow Modifier Effects
		/// </summary>
		RetaliationSlowModifier,

		/// <summary>
		/// Defensive Effects
		/// </summary>
		Defense,

		/// <summary>
		/// Character Effects
		/// </summary>
		Character,

		/// <summary>
		/// Damage Qualifier Effects
		/// </summary>
		DamageQualifierEffect,

		/// <summary>
		/// Other Effects
		/// </summary>
		Other,

		/// <summary>
		/// Artifact Reagents
		/// </summary>
		Reagent
	}

	/// <summary>
	/// Used to hold the Item Attributes
	/// </summary>
	public static class ItemAttributes
	{
		private static readonly log4net.ILog Log = Logging.Logger.Get(typeof(ItemAttributes));

		#region ItemAttributes Fields

		/// <summary>
		/// Other Effects Tags
		/// </summary>
		private static string[] otherEffects =
			{
				"characterBaseAttackSpeedTag",  // string text = TextTag(value)
                "levelRequirement",             // integer level requirement text = Format(TextTag(MeetsRequirement), TextTag(LevelRequirement), (float) value)
                "offensiveGlobalChance",        // float
                "retaliationGlobalChance",      // float
                "racialBonusPercentDamage",     // Added by VillageIdiot : float
                "racialBonusAbsoluteDefense",   // Added by VillageIdiot : int
                "itemSkillName"                 // Added by VillageIdiot : string text (dbr)
            };

		/// <summary>
		/// Character Effects Tags
		/// No "Chance" variable
		/// Effect name is also the text tag
		/// </summary>
		private static string[] characterEffects =
			{
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
            };

		/// <summary>
		/// Defensive Effect Tags
		/// Change "defensive" to "Defense" to get the text tag
		/// Look for "Chance" variable to get the chance of this effect.
		/// </summary>
		private static string[] defenseEffects =
			{
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
                "damageAbsorptionPercent" // Added by VillageIdiot
            };

		/// <summary>
		/// Offensive Effects Tags
		/// Change "offensive" to "Damage" to get the text tag.
		/// </summary>
		private static string[] offensiveEffects =
			{
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
			};

		/// <summary>
		/// Offensive Effects Variables Tags
		/// </summary>
		private static string[] offensiveEffectVariables =
			{
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
            };

		/// <summary>
		/// Offensive Modifier Effect Tags
		/// Change "offensive" to "DamageModifier" and remove trailing "Modifier" to get the text tag.
		/// Look for "Chance" variable to get the chance of this effect.
		/// </summary>
		private static string[] offensiveModifierEffects =
			{
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
            };

		/// <summary>
		/// Offensive Slow Effects Tags
		/// Change "offensiveSlow" to "DamageDuration" to get the text tag.
		/// </summary>
		private static string[] offensiveSlowEffects =
			{
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
			};

		/// <summary>
		/// Offensive Slow Effect Variables Tags
		/// </summary>
		private static string[] offensiveSlowEffectVariables =
			{
				"Min",
				"Max",
				"DurationMin",
				"DurationMax",
				"Chance",
				"XOR",         // boolean
                "Global"       // boolean
            };

		/// <summary>
		/// Offensive Slow Modifier Effects Tags
		/// Replace offensiveSlow with DamageDurationModifier to get the text tag
		/// </summary>
		private static string[] offensiveSlowModifierEffects =
			{
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
            };

		/// <summary>
		/// Offensive Slow Modifer Effect Variables Tags
		/// </summary>
		private static string[] offensiveSlowModifierEffectVariables =
			{
				"DurationModifier",
				"Modifier",
				"ModifierChance"
			};

		/// <summary>
		/// Retaliation Effects Tags
		/// Replace "retaliation" with "Retaliation" to get text tag
		/// </summary>
		private static string[] retaliationEffects =
			{
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
			};

		/// <summary>
		/// Retaliation Effect Variables Tags
		/// </summary>
		private static string[] retaliationEffectVariables =
			{
				"Chance",
				"Max",
				"Min",
				"Global",   // boolean
                "XOR"       // boolean
            };

		/// <summary>
		/// Retaliation Modifier Effects Tags
		/// Replace "retaliation" with "RetaliationModifier" and remove trailing "Modifier" to get text tag.
		/// Look for "Chance" variable to get the chance of this effect.
		/// </summary>
		private static string[] retaliationModifierEffects =
			{
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
			};

		/// <summary>
		/// Retaliation Slow Effects Tags
		/// Replace "retaliationSlow" with "RetaliationDuration" to get text tag
		/// </summary>
		private static string[] retaliationSlowEffects =
			{
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
			};

		/// <summary>
		/// Retaliation Slow Effect Variables Tags
		/// </summary>
		private static string[] retaliationSlowEffectVariables =
			{
				"Chance",
				"Max",
				"Min",
				"DurationMax",
				"DurationMin",
				"Global",      // boolean
                "XOR"          // boolean
            };

		/// <summary>
		/// Retaliation Slow Modifier Effects Tags
		/// Replace "retaliationSlow" with "RetaliationDurationModifier" to get text tag
		/// </summary>
		private static string[] retaliationSlowModifierEffects =
			{
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
			};

		/// <summary>
		/// Retaliation Slow Modifier Effect Variables Tags
		/// </summary>
		private static string[] retaliationSlowModifierEffectVariables =
			{
				"Modifier",
				"ModifierChance",
				"DurationModifier",
			};

		/// <summary>
		/// Reagents for formulae
		/// </summary>
		private static string[] reagents =
			{
				"reagent1BaseName",
				"reagent2BaseName",
				"reagent3BaseName"
			};

		/// <summary>
		/// For skill parameters (duration, radius, angle, etc.)
		/// </summary>
		private static string[] skillEffects =
			{
				"skillManaCost",
				"skillActiveManaCost",
				"skillChanceWeight",
				"skillActiveDuration",
				"skillTargetRadius",
				"projectileExplosionRadius",
				"skillTargetAngle",
				"skillTargetNumber"
			};

		/// <summary>
		/// For grouping the "Protects Against:" damage qualifiers.
		/// </summary>
		private static string[] damageQualifierEffects =
			{
				"physicalDamageQualifier",
				"pierceDamageQualifier",
				"lightningDamageQualifier",
				"fireDamageQualifier",
				"coldDamageQualifier",
				"poisonDamageQualifier",
				"lifeDamageQualifier",
				"bleedingDamageQualifier",
				"elementalDamageQualifier"
			};

		/// <summary>
		/// For shield parameters (defensiveBlock and blockRecoveryTime)
		/// </summary>
		private static string[] shieldEffects =
			{
				"defensiveBlock",
                ////"blockAbsorption",  // This one is suppressed
                "blockRecoveryTime"
			};

		/// <summary>
		/// Dictionary holding all of the attributes
		/// </summary>
		private static Dictionary<string, ItemAttributesData> attributeDictionary = InitializeAttributeDictionary();

		#endregion ItemAttributes Fields

		#region ItemAttribute Public Methods

		/// <summary>
		/// Indicates whether the name is a reagent
		/// </summary>
		/// <param name="name">name to be tested</param>
		/// <returns>true if a reagent</returns>
		public static bool IsReagent(string name)
		{
			return Array.IndexOf(reagents, name) != -1;
		}

		/// <summary>
		/// Gets data for an attibute string.
		/// </summary>
		/// <param name="attribute">attribute string.  Internally normalized to UpperInvariant.</param>
		/// <returns>ItemAttributesData for the attribute</returns>
		public static ItemAttributesData GetAttributeData(string attribute)
		{
			ItemAttributesData result = null;

			if (String.IsNullOrEmpty(attribute))
			{
				return result;
			}

			try
			{
				return attributeDictionary[attribute.ToUpperInvariant()];
			}
			catch (KeyNotFoundException)
			{
				return result;
			}
		}

		/// <summary>
		/// Converts format string from TQ format to string.format
		/// </summary>
		/// <param name="formatValue">format string to be parsed</param>
		/// <returns>updated format string</returns>
		public static string ConvertFormat(string formatValue)
		{
			if (TQDebug.ItemAttributesDebugLevel > 0)
			{
				Log.DebugFormat(CultureInfo.InvariantCulture, "ItemAttributes.ConvertFormatString({0})", formatValue);
			}

			// Takes a TQ Format string and converts it to a .NET Format string.
			StringBuilder formatStringBuilder = new StringBuilder(formatValue.Length);

			int startPosition = 0;
			while (startPosition < formatValue.Length)
			{
				// Find the next {
				int index = formatValue.IndexOf('{', startPosition);
				if (index == -1)
				{
					// no more {.  Just copy the rest of the string
					formatStringBuilder.Append(formatValue, startPosition, formatValue.Length - startPosition);
					startPosition = formatValue.Length;
				}
				else
				{
					if (TQDebug.ItemAttributesDebugLevel > 2)
					{
						Log.DebugFormat(CultureInfo.InvariantCulture, "Found {{ at {0} (search start was {1})", index, startPosition);
					}

					// Copy everything up to (but not including) the open bracket
					if (index > startPosition)
					{
						formatStringBuilder.Append(formatValue, startPosition, index - startPosition);
					}

					// Now process the brackets
					startPosition = ConvertFormatStringBrackets(formatValue, formatStringBuilder, index + 1);
					if (TQDebug.ItemAttributesDebugLevel > 2)
					{
						Log.DebugFormat(CultureInfo.InvariantCulture, "ConvertFormatStringBrackets() returned new ipos={0}", startPosition);
					}
				}
			}

			string processedFormatString = formatStringBuilder.ToString();
			if (TQDebug.ItemAttributesDebugLevel > 1)
			{
				Log.DebugFormat(CultureInfo.InvariantCulture, "'{0}' . '{1}'", formatValue, processedFormatString);
			}

			if (TQDebug.ItemAttributesDebugLevel > 0)
			{
				Log.Debug("Exiting ItemAttributes.ConvertFormatString()");
			}

			return processedFormatString;
		}

		/// <summary>
		/// Gets the effect tag string
		/// </summary>
		/// <param name="data">attribute data</param>
		/// <returns>string containing the effect tag</returns>
		public static string GetAttributeTextTag(ItemAttributesData data)
		{
			string result = string.Empty;
			if (data == null)
			{
				return result;
			}

			if (string.IsNullOrEmpty(data.Effect))
			{
				return result;
			}

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
		public static string GetAttributeTextTag(string attribute)
		{
			ItemAttributesData data = GetAttributeData(attribute);
			if (data == null)
			{
				return attribute;
			}

			return GetAttributeTextTag(data);
		}

		/// <summary>
		/// Indicates whether an attibute has a particular variable name
		/// </summary>
		/// <param name="variable">attribute variable</param>
		/// <param name="variableName">string for variable name</param>
		/// <returns>true if the variable == variable name</returns>
		[CLSCompliantAttribute(false)]
		public static bool AttributeHas(Variable variable, string variableName)
		{
			if (variable == null)
			{
				return false;
			}

			ItemAttributesData data = ItemAttributes.GetAttributeData(variable.Name);
			if (data == null)
			{
				return false;
			}

			return data.Variable.ToUpperInvariant().Equals(variableName.ToUpperInvariant());
		}

		/// <summary>
		/// Indicates whether an effect is part of a particular attribute group
		/// </summary>
		/// <param name="attributeList">Array of attributes</param>
		/// <param name="effect">effect string to be tested</param>
		/// <returns>true if attribute effect in group == effect</returns>
		[CLSCompliantAttribute(false)]
		public static bool AttributeGroupIs(Collection<Variable> attributeList, string effect)
		{
			Variable variable = (Variable)attributeList[0];
			ItemAttributesData data = ItemAttributes.GetAttributeData(variable.Name);
			if (data == null)
			{
				return false;
			}

			return data.Effect.ToUpperInvariant().Equals(effect.ToUpperInvariant());
		}

		/// <summary>
		/// Indicates whether an effect type is part of a particular attribute group
		/// </summary>
		/// <param name="attributeList">Array of attributes</param>
		/// <param name="type">Effect type enumeration</param>
		/// <returns>true if attribute effect in group == type</returns>
		[CLSCompliantAttribute(false)]
		public static bool AttributeGroupIs(Collection<Variable> attributeList, ItemAttributesEffectType type)
		{
			Variable variable = (Variable)attributeList[0];
			ItemAttributesData data = ItemAttributes.GetAttributeData(variable.Name);
			if (data == null)
			{
				return false;
			}

			return data.EffectType == type;
		}

		/// <summary>
		/// Gets the attribute group type.
		/// </summary>
		/// <param name="attributeList">array of attributes</param>
		/// <returns>Effect type of the attribute list</returns>
		[CLSCompliantAttribute(false)]
		public static ItemAttributesEffectType AttributeGroupType(Collection<Variable> attributeList)
		{
			Variable variable = (Variable)attributeList[0];
			ItemAttributesData data = ItemAttributes.GetAttributeData(variable.Name);
			if (data == null)
			{
				return ItemAttributesEffectType.Other;
			}

			return data.EffectType;
		}

		/// <summary>
		/// Inidicates whether an attribute group has a particular variable name
		/// </summary>
		/// <param name="attributeList">array of attributes</param>
		/// <param name="variableName">name of variable</param>
		/// <returns>true if variable is present in the list</returns>
		[CLSCompliantAttribute(false)]
		public static bool AttributeGroupHas(Collection<Variable> attributeList, string variableName)
		{
			foreach (Variable variable in attributeList)
			{
				if (AttributeHas(variable, variableName))
				{
					return true;
				}
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
		private static string GetCharacterEffectTextTag(string effect)
		{
			if (effect.ToUpperInvariant() == "CHARACTERGLOBALREQREDUCTION")
			{
				// awful mispelling by IL.
				return "CharcterItemGlobalReduction";
			}
			else if (effect.ToUpperInvariant() == "CHARACTERDEFLECTPROJECTILE")
			{
				return "CharacterDeflectProjectiles";
			}

			// Just return as-is since the effect is the text tag with the first letter capitalized.
			return effect;
		}

		/// <summary>
		/// Gets Modifier Shield Effect Text Tag.  Normalized to UpperInvariant.
		/// </summary>
		/// <param name="effect">effect tag to modify</param>
		/// <returns>modified effect text tag</returns>
		private static string GetShieldEffectTextTag(string effect)
		{
			if (effect.ToUpperInvariant() == "BLOCKRECOVERYTIME")
			{
				return "ShieldBlockRecoveryTime";
			}

			// Replace Defensive with Defense.
			return string.Concat("Defense", effect.Substring(9));
		}

		/// <summary>
		/// Gets modified defense effect text tag.  Normalized to UpperInvariant.
		/// </summary>
		/// <param name="effect">effect tag to modify</param>
		/// <returns>modified effect text tag</returns>
		private static string GetDefenseEffectTextTag(string effect)
		{
			// Check for specific strings.
			switch (effect.ToUpperInvariant())
			{
				case "DEFENSIVEPROTECTION":
					return "DefenseAbsorptionProtection";

				case "DEFENSIVESLEEP":
					return string.Concat("xtagDefense", effect.Substring(9));

				case "DEFENSIVETOTALSPEEDRESISTANCE":
					return "xtagTotalSpeedResistance";

				case "DAMAGEABSORPTION":
					return "SkillDamageAbsorption";

				case "DAMAGEABSORPTIONPERCENT":
					return "SkillDamageAbsorptionPercent";
			}

			// Replace DefensiveSlow with Defense
			if (effect.ToUpperInvariant().StartsWith("DEFENSIVESLOW", StringComparison.Ordinal))
			{
				return string.Concat("Defense", effect.Substring(13));
			}

			// Otherwise replace Defensive with Defense.
			return string.Concat("Defense", effect.Substring(9));
		}

		/// <summary>
		/// Gets modified offensive effect text tag.  Normalized to UpperInvariant.
		/// </summary>
		/// <param name="effect">effect tag to modify</param>
		/// <returns>modified effect text tag</returns>
		private static string GetOffensiveEffectTextTag(string effect)
		{
			// Check for a skill and return as-is.
			if (effect.ToUpperInvariant().StartsWith("SKILL", StringComparison.Ordinal))
			{
				return effect;
			}

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
			if (effect.ToUpperInvariant().StartsWith("OFFENSIVEBASE", StringComparison.Ordinal))
			{
				return string.Concat("Damage", effect.Substring(13));
			}

			// Otherwise replace Offensive with Damage.
			return string.Concat("Damage", effect.Substring(9));
		}

		/// <summary>
		/// Gets modified offensive modifier effect text tag.  Normalized to UpperInvariant.
		/// </summary>
		/// <param name="effect">effect tag to modify</param>
		/// <returns>modified effect text tag</returns>
		private static string GetOffensiveModifierEffectTextTag(string effect)
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
			if (effect.ToUpperInvariant().StartsWith("SKILL", StringComparison.Ordinal))
			{
				return effect;
			}

			// Just replace Offensive with DamageModifier and remove trailing modifier.
			return string.Concat("DamageModifier", effect.Substring(9, effect.Length - (9 + 8)));
		}

		/// <summary>
		/// Gets modified offensive slow effect text tag.  Normalized to UpperInvariant.
		/// </summary>
		/// <param name="effect">effect tag to modify</param>
		/// <returns>modified effect text tag</returns>
		private static string GetOffensiveSlowEffectTextTag(string effect)
		{
			// Change offensiveSlow to DamageDuration.
			return string.Concat("DamageDuration", effect.Substring(13));
		}

		/// <summary>
		/// Gets modifier offensive slow modifier effect text tag.  Normalized to UpperInvariant.
		/// </summary>
		/// <param name="effect">effect tag to modify</param>
		/// <returns>modified effect text tag</returns>
		private static string GetOffensiveSlowModifierEffectTextTag(string effect)
		{
			// Change offensiveSlow to DamageDurationModifier.
			return string.Concat("DamageDurationModifier", effect.Substring(13));
		}

		/// <summary>
		/// Gets modifier retaliation effect text tag.  Normalized to UpperInvariant.
		/// </summary>
		/// <param name="effect">effect tag to modify</param>
		/// <returns>modified effect text tag</returns>
		private static string GetRetaliationEffectTextTag(string effect)
		{
			// No need to replace retaliation with Retaliation.
			return effect;
		}

		/// <summary>
		/// Gets modifier retaliation modifier effect text tag.  Normalized to UpperInvariant.
		/// </summary>
		/// <param name="effect">effect tag to modify</param>
		/// <returns>modified effect text tag</returns>
		private static string GetRetaliationModifierEffectTextTag(string effect)
		{
			// Replace retaliation with RetaliationModifier and remove trailing Modifier.
			return string.Concat("RetaliationModifier", effect.Substring(11, effect.Length - (11 + 8)));
		}

		/// <summary>
		/// Gets modified retaliation slow effect text tag.  Normalized to UpperInvariant.
		/// </summary>
		/// <param name="effect">effect tag to modify</param>
		/// <returns>modified effect text tag</returns>
		private static string GetRetaliationSlowEffectTextTag(string effect)
		{
			// Replace retaliationSlow with RetaliationDuration.
			return string.Concat("RetaliationDuration", effect.Substring(15));
		}

		/// <summary>
		/// Gets modified retaliation slow modifier effect text tag.  Normalized to UpperInvariant.
		/// </summary>
		/// <param name="effect">effect tag to modify</param>
		/// <returns>modified effect text tag</returns>
		private static string GetRetaliationSlowModifierEffectTextTag(string effect)
		{
			// Replace retaliationSlow with RetaliationDurationModifier.
			return string.Concat("RetaliationDurationModifier", effect.Substring(15));
		}

		/// <summary>
		/// Gets modifier skill effect text tag.  Normalized to UpperInvariant.
		/// </summary>
		/// <param name="effect">effect tag to modify</param>
		/// <returns>modified effect text tag</returns>
		private static string GetSkillEffectTextTag(string effect)
		{
			// Return SkillChanceWeight as-is.
			if (effect.ToUpperInvariant() == "SKILLCHANCEWEIGHT")
			{
				return effect;
			}

			// Strip off Projectile from the text tag.
			if (effect.ToUpperInvariant().StartsWith("PROJECTILE", StringComparison.Ordinal))
			{
				return effect.Substring(10);
			}

			// Otherwise strip off Skill from the tag.
			return effect.Substring(5);
		}

		/// <summary>
		/// Converts format string brackets % ^ }
		/// </summary>
		/// <param name="formatString">format string to be parsed</param>
		/// <param name="answer">answer string</param>
		/// <param name="startPosition">initial string position</param>
		/// <returns>position of the closing bracket</returns>
		private static int ConvertFormatStringBrackets(string formatString, StringBuilder answer, int startPosition)
		{
			if (TQDebug.ItemAttributesDebugLevel > 0)
			{
				Log.DebugFormat(CultureInfo.InvariantCulture, "ItemAttributes.ConvertFormatStringBrackets({0}, {1}, {2})", formatString, answer, startPosition);
			}

			char[] keyChars =
			{
				'}', // end of format section
                '%', // start of printf() format spec
                '^'  // start of font change spec
            };

			// We need to process until we reach a close }
			while (startPosition < formatString.Length)
			{
				// Scan forward until we hit a key character
				int i = formatString.IndexOfAny(keyChars, startPosition);
				if (i == -1)
				{
					// no special chars.  This should not happen! Indicates a missing closing }.
					// Let's just copy the remainder of the string
					answer.Append(formatString, startPosition, formatString.Length - startPosition);
					if (TQDebug.ItemAttributesDebugLevel > 0)
					{
						Log.Debug("Error - No special characters found.");
						Log.Debug("Exiting ItemAttributes.ConvertFormatStringBrackets()");
					}

					return formatString.Length;
				}
				else
				{
					if (TQDebug.ItemAttributesDebugLevel > 2)
					{
						Log.DebugFormat(CultureInfo.InvariantCulture, "Found special char({0}) at {1} (searchstart={2})", formatString.Substring(i, 1), i, startPosition);
					}

					// We found a special char.  First copy all the crap before the char
					if (i > startPosition)
					{
						answer.Append(formatString, startPosition, i - startPosition);
						startPosition = i;
					}

					// now process the special
					switch (formatString[startPosition++])
					{
						case '}':
							{
								if (TQDebug.ItemAttributesDebugLevel > 0)
								{
									Log.Debug("Exiting ItemAttributes.ConvertFormatStringBrackets()");
								}

								return startPosition; // end of the format section.
							}

						case '%':
							{
								// We now have a scanf() format spec.
								// it should have some number of non-alpha characters followed by an alpha char
								// then followed by a digit which is the variable#
								int precisionStart = startPosition;

								// find the letter
								while ((startPosition < formatString.Length) && !char.IsLetter(formatString, startPosition))
								{
									++startPosition;
								}

								if (startPosition >= formatString.Length)
								{
									// shit we ran out of the string
									answer.Append(formatString, precisionStart - 1, formatString.Length - (precisionStart - 1));

									if (TQDebug.ItemAttributesDebugLevel > 0)
									{
										Log.Debug("Error - Ran out of string to parse.");
										Log.Debug("Exiting ItemAttributes.ConvertFormatStringBrackets()");
									}

									return formatString.Length;
								}

								string precision = formatString.Substring(precisionStart, startPosition - precisionStart);
								string formatAlpha = formatString.Substring(startPosition, 1);
								string formatNum = formatString.Substring(startPosition + 1, 1);

								string newFormatSpec = ConvertScanFormatSpec(precision, formatAlpha, formatNum);
								if (TQDebug.ItemAttributesDebugLevel > 2)
								{
									Log.DebugFormat(CultureInfo.InvariantCulture
										, "<{0}> split into<{1}><{2}><{3}>New ipos={4}"
										, formatString.Substring(precisionStart - 1, startPosition + 1 - precisionStart + 2)
										, precision, formatAlpha, formatNum
										, startPosition + 2
									);
								}

								answer.Append(newFormatSpec);
								startPosition = startPosition + 2;

								break;
							}

						case '^':
							{
								// font change.  We currently ignore this crap so we want to skip over
								// the next char which is the new font indicator
								++startPosition; // skip the following char
								break;
							}
					}
				}
			}

			if (TQDebug.ItemAttributesDebugLevel > 0)
			{
				Log.Debug("Exiting ItemAttributes.ConvertFormatStringBrackets()");
			}

			return startPosition;
		}

		/// <summary>
		/// Converts scan formats
		/// </summary>
		/// <param name="precision">numeric precision</param>
		/// <param name="alpha">determines scan code s d f</param>
		/// <param name="formatNumber">number to be formatted</param>
		/// <returns>formatted string</returns>
		private static string ConvertScanFormatSpec(string precision, string alpha, string formatNumber)
		{
			if (TQDebug.ItemAttributesDebugLevel > 0)
			{
				Log.DebugFormat(
					CultureInfo.InvariantCulture,
					"ItemAttributes.ConvertScanfFormatSpec (precision=<{0}>, alpha=<{1}>, formatNum=<{2}>)",
					precision,
					alpha,
					formatNumber);
			}

			if (alpha.Equals("s"))
			{
				// string format.  Ignore precision
				if (TQDebug.ItemAttributesDebugLevel > 1)
				{
					Log.Debug("String Format Spec");
				}

				if (TQDebug.ItemAttributesDebugLevel > 0)
				{
					Log.Debug("Exiting ItemAttributes.ConvertScanfFormatSpec ()");
				}

				return string.Format(CultureInfo.CurrentCulture, "{{{0}}}", formatNumber);
			}

			if (alpha.Equals("d") || alpha.Equals("f"))
			{
				// a number
				if (precision.Length < 1)
				{
					// simple
					if (TQDebug.ItemAttributesDebugLevel > 1)
					{
						Log.Debug("Simple Numeric Format Spec");
					}

					if (TQDebug.ItemAttributesDebugLevel > 0)
					{
						Log.Debug("Exiting ItemAttributes.ConvertScanfFormatSpec ()");
					}

					return string.Format(CultureInfo.CurrentCulture, "{{{0}}}", formatNumber);
				}

				int ipos = 0;

				// See if they want the plus sign.
				bool showPlus = precision[ipos] == '+';
				if (showPlus)
				{
					++ipos;
				}

				// see if they listed a decimal precision
				bool hasDecimal = precision[ipos] == '.';
				string decimalSpec = string.Empty;
				int numDecimals = 0;
				if (hasDecimal)
				{
					++ipos;
					if (TQDebug.ItemAttributesDebugLevel > 1)
					{
						Log.DebugFormat(CultureInfo.InvariantCulture, "Parsing Decimals ipos={0}, string=<{1}>", ipos, precision.Substring(ipos, 1));
					}

					// Changed by VillageIdiot
					// Sometimes the parsing would cause a format exception with 0
					// Use TryParse to handle the exception.
					if (!Int32.TryParse(precision.Substring(ipos, 1), out numDecimals))
					{
						numDecimals = 0;
					}

					if (TQDebug.ItemAttributesDebugLevel > 1)
					{
						Log.DebugFormat(CultureInfo.InvariantCulture, "numDecimals={0}", numDecimals);
					}

					if (numDecimals > 0)
					{
						decimalSpec = ".";
						decimalSpec = decimalSpec.PadRight(numDecimals + 1, '0');
					}
				}

				if (showPlus)
				{
					// Gotta get fancy
					if (TQDebug.ItemAttributesDebugLevel > 1)
					{
						Log.Debug("Decimal with plus Format Spec");
					}

					if (TQDebug.ItemAttributesDebugLevel > 0)
					{
						Log.Debug("Exiting ItemAttributes.ConvertScanfFormatSpec ()");
					}

					return string.Format(CultureInfo.CurrentCulture, "{{{0}:+#0{1};-#0{1}}}", formatNumber, decimalSpec);
				}
				else
				{
					if (TQDebug.ItemAttributesDebugLevel > 1)
					{
						Log.Debug("Decimal Format Spec");
					}

					if (TQDebug.ItemAttributesDebugLevel > 0)
					{
						Log.Debug("Exiting ItemAttributes.ConvertScanfFormatSpec ()");
					}

					return string.Format(CultureInfo.CurrentCulture, "{{{0}:#0{1}}}", formatNumber, decimalSpec);
				}
			}
			else
			{
				// unknown
				if (TQDebug.ItemAttributesDebugLevel > 1)
				{
					Log.Debug("Error - Unknown Format Spec using default");
				}

				if (TQDebug.ItemAttributesDebugLevel > 0)
				{
					Log.Debug("Exiting ItemAttributes.ConvertScanfFormatSpec ()");
				}

				return string.Format(CultureInfo.CurrentCulture, "{{{0}}}", formatNumber);
			}
		}

		/// <summary>
		/// Initialize all of my static arrays
		/// </summary>
		/// <returns>hash table of attributes</returns>
		private static Dictionary<string, ItemAttributesData> InitializeAttributeDictionary()
		{
			attributeDictionary = new Dictionary<string, ItemAttributesData>();

			// Add all the attributes to the dictionary
			int subOrder = 0;
			foreach (string otherEffect in otherEffects)
			{
				attributeDictionary.Add(otherEffect.ToUpperInvariant(), new ItemAttributesData(ItemAttributesEffectType.Other, otherEffect, otherEffect, string.Empty, subOrder));
				++subOrder;
			}

			// Added by VillageIdiot
			subOrder = 0;
			foreach (string shieldEffect in shieldEffects)
			{
				string normalizedEffect = shieldEffect.ToUpperInvariant();
				attributeDictionary.Add(normalizedEffect, new ItemAttributesData(ItemAttributesEffectType.ShieldEffect, shieldEffect, shieldEffect, "Min", subOrder));
				attributeDictionary.Add(
					string.Concat(normalizedEffect, "CHANCE"),
					new ItemAttributesData(ItemAttributesEffectType.ShieldEffect, shieldEffect, shieldEffect, "Chance", subOrder));
				++subOrder;
			}

			subOrder = 0;
			foreach (string characterEffect in characterEffects)
			{
				string normalizedEffect = characterEffect.ToUpperInvariant();
				attributeDictionary.Add(normalizedEffect, new ItemAttributesData(ItemAttributesEffectType.Character, characterEffect, characterEffect, "Min", subOrder));
				attributeDictionary.Add(
					string.Concat(normalizedEffect, "CHANCE"),
					new ItemAttributesData(ItemAttributesEffectType.Character, characterEffect, characterEffect, "Chance", subOrder));
				++subOrder;
			}

			subOrder = 0;
			foreach (string defenseEffect in defenseEffects)
			{
				string normalizedEffect = defenseEffect.ToUpperInvariant();
				attributeDictionary.Add(normalizedEffect, new ItemAttributesData(ItemAttributesEffectType.Defense, defenseEffect, defenseEffect, "Min", subOrder));
				attributeDictionary.Add(
					string.Concat(normalizedEffect, "CHANCE"),
					new ItemAttributesData(ItemAttributesEffectType.Defense, defenseEffect, defenseEffect, "Chance", subOrder));
				++subOrder;
			}

			subOrder = 0;
			foreach (string offensiveEffect in offensiveEffects)
			{
				foreach (string offensiveEffectVariable in offensiveEffectVariables)
				{
					attributeDictionary.Add(
						string.Concat(offensiveEffect.ToUpperInvariant(), offensiveEffectVariable.ToUpperInvariant()),
						new ItemAttributesData(ItemAttributesEffectType.Offense, string.Concat(offensiveEffect, offensiveEffectVariable), offensiveEffect, offensiveEffectVariable, subOrder));
				}

				++subOrder;
			}

			subOrder = 0;
			foreach (string offensiveModifierEffect in offensiveModifierEffects)
			{
				string normalizedEffect = offensiveModifierEffect.ToUpperInvariant();
				attributeDictionary.Add(
					normalizedEffect,
					new ItemAttributesData(ItemAttributesEffectType.OffenseModifier, offensiveModifierEffect, offensiveModifierEffect, "Min", subOrder));
				attributeDictionary.Add(
					string.Concat(normalizedEffect, "CHANCE"),
					new ItemAttributesData(ItemAttributesEffectType.OffenseModifier, offensiveModifierEffect, offensiveModifierEffect, "Chance", subOrder));
				++subOrder;
			}

			subOrder = 0;
			foreach (string offensiveSlowEffect in offensiveSlowEffects)
			{
				foreach (string offensiveSlowEffectVariable in offensiveSlowEffectVariables)
				{
					attributeDictionary.Add(
						string.Concat(offensiveSlowEffect.ToUpperInvariant(), offensiveSlowEffectVariable.ToUpperInvariant()),
						new ItemAttributesData(ItemAttributesEffectType.OffenseSlow, string.Concat(offensiveSlowEffect, offensiveSlowEffectVariable), offensiveSlowEffect, offensiveSlowEffectVariable, subOrder));
				}

				++subOrder;
			}

			subOrder = 0;
			foreach (string offensiveSlowModifierEffect in offensiveSlowModifierEffects)
			{
				foreach (string offensiveSlowModifierEffectVariable in offensiveSlowModifierEffectVariables)
				{
					attributeDictionary.Add(
						string.Concat(offensiveSlowModifierEffect.ToUpperInvariant(), offensiveSlowModifierEffectVariable.ToUpperInvariant()),
						new ItemAttributesData(ItemAttributesEffectType.OffenseSlowModifier, string.Concat(offensiveSlowModifierEffect, offensiveSlowModifierEffectVariable), offensiveSlowModifierEffect, offensiveSlowModifierEffectVariable, subOrder));
				}

				++subOrder;
			}

			subOrder = 0;
			foreach (string retaliationEffect in retaliationEffects)
			{
				foreach (string retaliationEffectVariable in retaliationEffectVariables)
				{
					attributeDictionary.Add(
						string.Concat(retaliationEffect.ToUpperInvariant(), retaliationEffectVariable.ToUpperInvariant()),
						new ItemAttributesData(ItemAttributesEffectType.Retaliation, string.Concat(retaliationEffect, retaliationEffectVariable), retaliationEffect, retaliationEffectVariable, subOrder));
				}

				++subOrder;
			}

			subOrder = 0;
			foreach (string retaliationModifierEffect in retaliationModifierEffects)
			{
				string normalizedEffect = retaliationModifierEffect.ToUpperInvariant();
				attributeDictionary.Add(
					normalizedEffect,
					new ItemAttributesData(ItemAttributesEffectType.RetaliationModifier, retaliationModifierEffect, retaliationModifierEffect, "Min", subOrder));
				attributeDictionary.Add(
					string.Concat(normalizedEffect, "CHANCE"),
					new ItemAttributesData(ItemAttributesEffectType.RetaliationModifier, retaliationModifierEffect, retaliationModifierEffect, "Chance", subOrder));
				++subOrder;
			}

			subOrder = 0;
			foreach (string retaliationSlowEffect in retaliationSlowEffects)
			{
				foreach (string retaliationSlowEffectVariable in retaliationSlowEffectVariables)
				{
					attributeDictionary.Add(
						string.Concat(retaliationSlowEffect.ToUpperInvariant(), retaliationSlowEffectVariable.ToUpperInvariant()),
						new ItemAttributesData(ItemAttributesEffectType.RetaliationSlow, string.Concat(retaliationSlowEffect, retaliationSlowEffectVariable), retaliationSlowEffect, retaliationSlowEffectVariable, subOrder));
				}

				++subOrder;
			}

			subOrder = 0;
			foreach (string retaliationSlowModifierEffect in retaliationSlowModifierEffects)
			{
				foreach (string retaliationSlowModifierEffectVariable in retaliationSlowModifierEffectVariables)
				{
					attributeDictionary.Add(
						string.Concat(retaliationSlowModifierEffect.ToUpperInvariant(), retaliationSlowModifierEffectVariable.ToUpperInvariant()),
						new ItemAttributesData(ItemAttributesEffectType.RetaliationSlowModifier, string.Concat(retaliationSlowModifierEffect, retaliationSlowModifierEffectVariable), retaliationSlowModifierEffect, retaliationSlowModifierEffectVariable, subOrder));
				}

				++subOrder;
			}

			// Added by VillageIdiot
			subOrder = 0;
			foreach (string damageQualifierEffect in damageQualifierEffects)
			{
				attributeDictionary.Add(
					damageQualifierEffect.ToUpperInvariant(),
					new ItemAttributesData(ItemAttributesEffectType.DamageQualifierEffect, damageQualifierEffect, damageQualifierEffect, string.Empty, subOrder));
				++subOrder;
			}

			// Added by VillageIdiot
			subOrder = 0;
			foreach (string reagent in reagents)
			{
				attributeDictionary.Add(reagent.ToUpperInvariant(), new ItemAttributesData(ItemAttributesEffectType.Reagent, reagent, reagent, string.Empty, subOrder));
				++subOrder;
			}

			// Added by VillageIdiot
			subOrder = 0;
			foreach (string skillEffect in skillEffects)
			{
				attributeDictionary.Add(skillEffect.ToUpperInvariant(), new ItemAttributesData(ItemAttributesEffectType.SkillEffect, skillEffect, skillEffect, string.Empty, subOrder));
				++subOrder;
			}

			return attributeDictionary;
		}

		#endregion ItemAttributes Private Methods
	}
}