//-----------------------------------------------------------------------
// <copyright file="Item.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.RegularExpressions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Data;

public partial class ItemProvider
{

	/// <summary>
	/// Gets the item's attributes from the database record.
	/// </summary>
	/// <remarks>
	/// Changed by VillageIdiot
	/// Added option to NOT translate the attributes to strings
	/// </remarks>
	/// <param name="record">DBRecord for the database record</param>
	/// <param name="filtering">whether or not we are filtering strings</param>
	/// <param name="recordId">string containing the database record id</param>
	/// <param name="results">List for the results</param>
	/// <param name="convertStrings">flag on whether we convert attributes to strings.</param>
	private void GetAttributesFromRecord(Item itm, DBRecordCollection record, bool filtering, RecordId recordId, List<string> results, bool convertStrings = true)
	{
		if (USettings.ItemDebugLevel > 0)
		{
			Log.LogDebug("Item.GetAttributesFromRecord({0}, {1}, {2}, {3}, {4})"
				, record, filtering, recordId, results, convertStrings
			);
		}

		// First get a list of attributes, grouped by effect.
		Dictionary<string, List<Variable>> attrByEffect = new();
		if (record == null)
		{
			if (USettings.ItemDebugLevel > 0)
				Log.LogDebug("Error - record was null.");

			results.Add("<unknown>");
			return;
		}

		if (USettings.ItemDebugLevel > 1)
			Log.LogDebug(record.Id);

		// Added by Village Idiot
		// To keep track of groups so they are not counted twice
		List<string> countedGroups = new();

		foreach (Variable variable in record)
		{
			if (USettings.ItemDebugLevel > 2)
				Log.LogDebug(variable.Name);


			if (this.FilterValue(variable, !filtering))
				continue;

			if (filtering && this.FilterKey(variable.Name))
				continue;

			if (filtering && this.FilterRequirements(variable.Name))
				continue;

			ItemAttributesData data = ItemAttributeProvider.GetAttributeData(variable.Name);
			if (data == null)
			{
				// unknown attribute
				if (USettings.ItemDebugLevel > 2)
					Log.LogDebug("Unknown Attribute");

				data = new ItemAttributesData(ItemAttributesEffectType.Other, variable.Name, variable.Name, string.Empty, 0);
			}

			string effectGroup;

			// Changed by VillageIdiot to group DamageQualifiers together.
			if (data.EffectType == ItemAttributesEffectType.DamageQualifierEffect)
				effectGroup = string.Concat(data.EffectType.ToString(), ":", "DamageQualifier");
			else
				effectGroup = string.Concat(data.EffectType.ToString(), ":", data.Effect);

			// Find or create the attrList for itm effect
			List<Variable> attrList;
			if (!attrByEffect.TryGetValue(effectGroup, out attrList))
			{
				attrList = new List<Variable>();
				attrByEffect[effectGroup] = attrList;
			}

			// Add itm guy to the attrList
			attrList.Add(variable);

			// Added by VillageIdiot
			// Set number of attributes parameter for level calculation
			// Filter relics and relic bonuses
			if (recordId != itm.relic2ID && recordId != itm.RelicBonus2Id && recordId != itm.relicID && recordId != itm.RelicBonusId && !itm.isAttributeCounted)
			{
				// Added test to see if itm has already been done
				if (!countedGroups.Contains(effectGroup))
				{
					string normalizedVariableName = variable.Name.ToUpperInvariant();
					if (!normalizedVariableName.Contains("CHANCE") && !normalizedVariableName.Contains("DURATION"))
					{
						// Filter Attribute chance and duration tags
						// Filter base attributes
						if (normalizedVariableName != "CHARACTERBASEATTACKSPEEDTAG" &&
							normalizedVariableName != "OFFENSIVEPHYSICALMIN" &&
							normalizedVariableName != "OFFENSIVEPHYSICALMAX" &&
							normalizedVariableName != "DEFENSIVEPROTECTION" &&
							normalizedVariableName != "DEFENSIVEBLOCK" &&
							normalizedVariableName != "BLOCKRECOVERYTIME" &&
							normalizedVariableName != "OFFENSIVEGLOBALCHANCE" &&
							normalizedVariableName != "RETALIATIONGLOBALCHANCE" &&
							normalizedVariableName != "OFFENSIVEPIERCERATIOMIN")
						{
							// Chance of effects are still messed up.
							if (normalizedVariableName.StartsWith("AUGMENTSKILLLEVEL", noCase))
							{
								// Add value of augment skill level to count instead of incrementing
								itm.attributeCount += variable.GetInt32(0);
								countedGroups.Add(effectGroup);
							}
							else
							{
								++itm.attributeCount;
								countedGroups.Add(effectGroup);
							}
						}
					}
				}
			}
		}

		// Added by VillageIdiot
		// Some attributes have been counted so set the flag so we do not count them again
		if (itm.attributeCount != 0)
			itm.isAttributeCounted = true;

		// Now we have all our attributes grouped by effect.  Now lets sort them
		List<Variable>[] attrArray = new List<Variable>[attrByEffect.Count];
		attrByEffect.Values.CopyTo(attrArray, 0);
		Array.Sort(attrArray, new ItemAttributeListCompare(itm.IsArmor || itm.IsShield, this.ItemAttributeProvider));

		// Now for the global params, we need to check to see if they are XOR or all.
		// We do itm by checking the effect just after the global param.
		for (int i = 0; i < attrArray.Length; ++i)
		{
			List<Variable> attrList = attrArray[i];

			if (ItemAttributeProvider.AttributeGroupIs(new Collection<Variable>(attrList), "offensiveGlobalChance") ||
				ItemAttributeProvider.AttributeGroupIs(new Collection<Variable>(attrList), "retaliationGlobalChance"))
			{
				// check the next effect group
				int j = i + 1;
				if (j < attrArray.Length)
				{
					List<Variable> next = attrArray[j];
					if (!ItemAttributeProvider.AttributeGroupHas(new Collection<Variable>(next), "Global"))
					{
						// itm is a spurious globalChance entry.  Let's add 2 null entries to signal it should be ignored
						attrList.Add(null);
						attrList.Add(null);
					}
					else if (ItemAttributeProvider.AttributeGroupHas(new Collection<Variable>(next), "XOR"))
					{
						// Yes it is global and is also XOR
						// flag our current attribute as XOR
						// We do itm by adding a second NULL entry to the list.  Its a hack but it works
						attrList.Add(null);
					}
				}
				else
				{
					// itm is a spurious globalChance entry.  Let's add 2 null entries to signal it should be ignored
					attrList.Add(null);
					attrList.Add(null);
				}
			}
		}

		foreach (List<Variable> attrList in attrArray)
		{
			// Used to sort out the Damage Qualifier effects.
			if (ItemAttributeProvider.AttributeGroupIs(new Collection<Variable>(attrList), ItemAttributesEffectType.DamageQualifierEffect))
				attrList.Sort(new ItemAttributeSubListCompare(this.ItemAttributeProvider));

			if (!convertStrings)
				ConvertBareAttributeListToString(attrList, results);
			else
				ConvertAttributeListToString(itm, record, attrList, recordId, results);
		}

		if (USettings.ItemDebugLevel > 0)
			Log.LogDebug("Exiting Item.GetAttributesFromRecord()");
	}

	/// <summary>
	/// Converts the item's attribute list to a string
	/// </summary>
	/// <param name="record">DBRecord for the item</param>
	/// <param name="attributeList">ArrayList containing the attributes list</param>
	/// <param name="recordId">string containing the record id</param>
	/// <param name="results">List containing the results</param>
	private void ConvertAttributeListToString(Item itm, DBRecordCollection record, List<Variable> attributeList, RecordId recordId, List<string> results)
	{
		if (USettings.ItemDebugLevel > 0)
		{
			Log.LogDebug("Item.ConvertAttrListToString ({0}, {1}, {2}, {3})"
				, record, attributeList, recordId, results
			);
		}

		// see what kind of effects are in this list
		Variable variable = (Variable)attributeList[0];
		ItemAttributesData data = ItemAttributeProvider.GetAttributeData(variable.Name);
		if (data == null)
		{
			// unknown attribute
			if (USettings.ItemDebugLevel > 0)
				Log.LogDebug("Error - Unknown Attribute.");

			data = new ItemAttributesData(ItemAttributesEffectType.Other, variable.Name, variable.Name, string.Empty, 0);
		}

		if (USettings.ItemDebugLevel > 0)
			Log.LogDebug("Exiting Item.ConvertAttrListToString ()");

		ConvertOffenseAttributesToString(itm, record, attributeList, data, recordId, results);
		return;
	}

	/// <summary>
	/// Checks if the Effect type does not has value magnitude dependent on Duration.
	/// </summary>
	/// <param name="effectName">ItemAttributesData.Effect value</param>
	/// <returns>True if the effect magnitude does depend on the duration</returns>
	private bool IsDurationReliantValue(string effectName)
	{
		return Array.IndexOf(durationIndependentEffects, effectName.ToUpperInvariant()) == -1;
	}

	/// <summary>
	/// Gets a formatted range amount
	/// </summary>
	/// <param name="data">ItemAttributesData data</param>
	/// <param name="varNum">variable number to look up</param>
	/// <param name="minVar">minVar variable</param>
	/// <param name="maxVar">maxVar variable</param>
	/// <param name="label">label string</param>
	/// <param name="labelColor">label color</param>
	/// <returns>formatted range string</returns>
	private string GetAmountRange(Item itm, ItemAttributesData data, int varNum, Variable minVar, Variable maxVar, ref string label, TQColor? labelColor, Variable minDurVar = null)
	{
		// Added by VillageIdiot : check to see if min and max are the same
		TQColor? color = null;
		string amount = null;

		Variable min = null;
		Variable max = null;

		if (minVar != null)
			min = minVar.Clone();

		if (maxVar != null)
			max = maxVar.Clone();

		// sweet we have a range
		string tag = "DamageRangeFormat";
		if (data.Effect.EndsWith("Stun", noCase)
			|| data.Effect.EndsWith("Freeze", noCase)
			|| data.Effect.EndsWith("Petrify", noCase)
			|| data.Effect.EndsWith("Trap", noCase)
			|| data.Effect.EndsWith("Convert", noCase)
			|| data.Effect.EndsWith("Fear", noCase)
			|| data.Effect.EndsWith("Confusion", noCase)
			|| data.Effect.EndsWith("Disruption", noCase)
		)
		{
			tag = "DamageInfluenceRangeFormat";
		}
		else if (data.Effect.Equals("defensiveBlock"))
			tag = "DefenseBlock";

		if (!TranslationService.TryTranslateXTag(tag, out var formatSpec))
		{
			formatSpec = "{0}..{1}";
			color = ItemStyle.Legendary.TQColor();
		}
		else
		{
			if (USettings.ItemDebugLevel > 2)
				Log.LogDebug("Item.formatspec (range) = " + formatSpec);

			formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
		}

		if (label.IndexOf('{') >= 0)
		{
			// the label has formatting codes.  Use it to format the amount
			formatSpec = label;
			label = null;
			color = labelColor;
		}

		// Added by VillageIdiot
		// Adjust for itemScalePercent
		// AMS: Added If the value is reliant on the duration.
		if ((minDurVar != null) && IsDurationReliantValue(data.Effect))
		{
			min[Math.Min(min.NumberOfValues - 1, varNum)] = (float)min[Math.Min(min.NumberOfValues - 1, varNum)] * (float)minDurVar[minDurVar.NumberOfValues - 1] * itm.itemScalePercent;
			max[Math.Min(max.NumberOfValues - 1, varNum)] = (float)max[Math.Min(max.NumberOfValues - 1, varNum)] * (float)minDurVar[minDurVar.NumberOfValues - 1] * itm.itemScalePercent;
		}
		else
		{
			min[Math.Min(min.NumberOfValues - 1, varNum)] = (float)min[Math.Min(min.NumberOfValues - 1, varNum)] * itm.itemScalePercent;
			max[Math.Min(max.NumberOfValues - 1, varNum)] = (float)max[Math.Min(max.NumberOfValues - 1, varNum)] * itm.itemScalePercent;
		}

		amount = this.Format(formatSpec, min[Math.Min(min.NumberOfValues - 1, varNum)], max[Math.Min(max.NumberOfValues - 1, varNum)]);
		return color.HasValue ? $"{color?.ColorTag()}{amount}" : amount;
	}

	[GeneratedRegex(@"(?<Prefix>\{(\d):)(?<Sign>[+-])(?<Suffix>#([\d\.]+)})")]
	private static partial Regex GetAmountSingleRegEx();
	/// <summary>
	/// Gets a formatted single amount
	/// </summary>
	/// <param name="data">ItemAttributesData data</param>
	/// <param name="varNum">variable number to look up</param>
	/// <param name="minVar">minVar variable</param>
	/// <param name="maxVar">maxVar variable</param>
	/// <param name="label">label string</param>
	/// <param name="labelColor">label color</param>
	/// <param name="minDurVar">Duration of Damage</param>
	/// <returns>formatted single amount string</returns>
	private string GetAmountSingle(Item itm, ItemAttributesData data, int varNum, Variable minVar, Variable maxVar, ref string label, TQColor? labelColor, Variable minDurVar = null)
	{
		TQColor? color = null;
		string amount = null;

		string tag = "DamageSingleFormat";
		if (data.Effect.EndsWith("Stun", noCase)
			|| data.Effect.EndsWith("Freeze", noCase)
			|| data.Effect.EndsWith("Petrify", noCase)
			|| data.Effect.EndsWith("Trap", noCase)
			|| data.Effect.EndsWith("Convert", noCase)
			|| data.Effect.EndsWith("Fear", noCase)
			|| data.Effect.EndsWith("Confusion", noCase)
			|| data.Effect.EndsWith("Disruption", noCase)
		)
		{
			tag = "DamageInfluenceSingleFormat";
		}

		if (!TranslationService.TryTranslateXTag(tag, out var formatSpec))
		{
			formatSpec = "{0}";
			color = ItemStyle.Legendary.TQColor();
		}
		else
		{
			if (USettings.ItemDebugLevel > 2)
				Log.LogDebug("Item.formatspec (single) = " + formatSpec);

			formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
		}

		if (label.IndexOf('{') >= 0)
		{
			// the label has formatting codes.  Use it to format the amount
			formatSpec = label;
			label = null;
			color = labelColor;
		}

		Variable currentVariable = null;

		if (minVar != null)
			currentVariable = minVar.Clone();
		else if (maxVar != null)
			currentVariable = maxVar.Clone();

		if (currentVariable != null)
		{
			// Adjust for itemScalePercent
			// only for floats
			var curvar = currentVariable[Math.Min(currentVariable.NumberOfValues - 1, varNum)];
			if (currentVariable.DataType == VariableDataType.Float)
			{
				if ((minDurVar != null) && IsDurationReliantValue(data.Effect))
				{
					curvar = (float)curvar * (float)minDurVar[minDurVar.NumberOfValues - 1] * itm.itemScalePercent;
				}
				else
				{
					curvar = (float)curvar * itm.itemScalePercent;
				}
				currentVariable[Math.Min(currentVariable.NumberOfValues - 1, varNum)] = curvar;

				// Fix#246, double signed result on negative value Ex : string.Format("{0:+#0} d'intelligence", -10) by removing format sign.
				// Fix "Dotted decimal mask" matching Ex : {0:#0.0} Health Regeneration per second
				formatSpec = GetAmountSingleRegEx().Replace(formatSpec
					, (Match m) =>
					{
						var Prefix = m.Groups["Prefix"].Value;
						var Sign = m.Groups["Sign"].Value;
						var Suffix = m.Groups["Suffix"].Value;
						var val = (float)curvar;

						if ((Sign == "+" && val < 0) || (Sign == "-" && val >= 0))
							return $"{Prefix}{Suffix}";

						return m.Value;
					}
				);
			}

			amount = this.Format(formatSpec, curvar);
		}

		return color.HasValue ? $"{color?.ColorTag()}{amount}" : amount;
	}

	/// <summary>
	/// Gets the formatted duration range values
	/// </summary>
	/// <param name="varNum">offset number of the variable value that we are using</param>
	/// <param name="minDurVar">minimum duration variable</param>
	/// <param name="maxDurVar">maximum duration variable</param>
	/// <returns>formatted duration string</returns>
	private string GetDurationRange(int varNum, Variable minDurVar, Variable maxDurVar)
	{
		string duration = null;
		TQColor? color = null;

		if (!TranslationService.TryTranslateXTag("DamageRangeFormatTime", out var formatSpec))
		{
			formatSpec = "for {0}..{1} seconds";
			color = ItemStyle.Legendary.TQColor();
		}
		else
		{
			if (USettings.ItemDebugLevel > 2)
				Log.LogDebug("Item.formatspec (time range) = " + formatSpec);

			formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
		}

		duration = this.Format(formatSpec, minDurVar[Math.Min(minDurVar.NumberOfValues - 1, varNum)], maxDurVar[Math.Min(maxDurVar.NumberOfValues - 1, varNum)]);

		return color.HasValue ? $"{color?.ColorTag()}{duration}" : duration;
	}

	/// <summary>
	/// Gets the formatted duration single value
	/// </summary>
	/// <param name="varNum">offset number of the variable value that we are using</param>
	/// <param name="minDurVar">minimum duration variable</param>
	/// <param name="maxDurVar">maximum duration variable</param>
	/// <returns>formatted duration string</returns>
	private string GetDurationSingle(int varNum, Variable minDurVar, Variable maxDurVar)
	{
		string duration = null;
		TQColor? color = null;

		if (!TranslationService.TryTranslateXTag("DamageSingleFormatTime", out var formatSpec))
		{
			formatSpec = "{0}";
			color = ItemStyle.Legendary.TQColor();
		}
		else
		{
			if (USettings.ItemDebugLevel > 2)
				Log.LogDebug("Item.formatspec (time single) = " + formatSpec);

			formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
		}

		Variable durationVariable = minDurVar;
		if (durationVariable == null)
			durationVariable = maxDurVar;

		if (durationVariable != null)
		{
			duration = this.Format(formatSpec, durationVariable[Math.Min(durationVariable.NumberOfValues - 1, varNum)]);
			duration = $"{color?.ColorTag()}{duration}";
		}

		return duration;
	}

	/// <summary>
	/// Gets the formatted damage ratio string
	/// </summary>
	/// <param name="varNum">offset number of the variable value that we are using</param>
	/// <param name="damageRatioData">ItemAttributesData for the damage ratio</param>
	/// <param name="damageRatioVar">Damage Ratio variable</param>
	/// <returns>formatted damage ratio string</returns>
	private string GetDamageRatio(int varNum, ItemAttributesData damageRatioData, Variable damageRatioVar)
	{
		string damageRatio = null;
		TQColor? color = null;
		string formatSpec = null;

		// Original: concatenating with substring (already optimized by compiler)
		string tag = string.Concat("Damage", damageRatioData.FullAttribute.Substring(9, damageRatioData.FullAttribute.Length - 20), "Ratio");

		if (!TranslationService.TryTranslateXTag(tag, out formatSpec))
		{
			formatSpec = string.Concat("{0:f1}% ?", damageRatioData.FullAttribute, "?");
			color = ItemStyle.Legendary.TQColor();
		}
		else
		{
			if (USettings.ItemDebugLevel > 2)
				Log.LogDebug("Item.formatspec (percent) = " + formatSpec);

			formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
		}

		damageRatio = this.Format(formatSpec, damageRatioVar[Math.Min(damageRatioVar.NumberOfValues - 1, varNum)]);

		return $"{color?.ColorTag()}{damageRatio}";
	}

	/// <summary>
	/// Gets formatted chance string
	/// </summary>
	/// <param name="varNum">offset number of the variable value that we are using</param>
	/// <param name="chanceVar">chance variable</param>
	/// <returns>formatted chance string.</returns>
	private string GetChance(int varNum, Variable chanceVar)
	{
		string chance = null;
		TQColor? color = null;

		if (!TranslationService.TryTranslateXTag("ChanceOfTag", out var formatSpec))
		{
			formatSpec = "?{%.1f0}% Chance of?";
			color = ItemStyle.Legendary.TQColor();
		}

		if (USettings.ItemDebugLevel > 2)
			Log.LogDebug("Item.formatspec (chance) = " + formatSpec);

		formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
		if (chanceVar != null)
		{
			chance = this.Format(formatSpec, chanceVar[Math.Min(chanceVar.NumberOfValues - 1, varNum)]);
			chance = $"{color?.ColorTag()}{chance}";
		}

		return chance;
	}


	/// <summary>
	/// Gets formatted modifier string
	/// </summary>
	/// <param name="data">ItemAttributesData for the attribute</param>
	/// <param name="varNum">offset number of the variable value that we are using</param>
	/// <param name="modifierData">ItemAttributesData for the modifier</param>
	/// <param name="modifierVar">modifier variable</param>
	/// <returns>formatted modifier string</returns>
	private string GetModifier(ItemAttributesData data, int varNum, ItemAttributesData modifierData, Variable modifierVar)
	{
		string modifier = null;
		TQColor? color = null;
		string formatSpec = null;

		string tag = ItemAttributeProvider.GetAttributeTextTag(data);
		if (string.IsNullOrEmpty(tag))
		{
			formatSpec = string.Concat("{0:f1}% ?", modifierData.FullAttribute, "?");
			color = ItemStyle.Legendary.TQColor();
		}
		else
		{
			if (!TranslationService.TryTranslateXTag(tag, out formatSpec))
			{
				formatSpec = string.Concat("{0:f1}% ?", modifierData.FullAttribute, "?");
				color = ItemStyle.Legendary.TQColor();
			}
			else
			{
				if (USettings.ItemDebugLevel > 2)
					Log.LogDebug("Item.formatspec (percent) = " + formatSpec);

				formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
			}

		}

		modifier = Format(formatSpec, modifierVar[Math.Min(modifierVar.NumberOfValues - 1, varNum)]);

		return $"{color?.ColorTag()}{modifier}";
	}

	/// <summary>
	/// Gets formatted duration modifier string
	/// </summary>
	/// <param name="varNum">offset number of the variable value that we are using</param>
	/// <param name="durationModifierVar">duration modifier variable</param>
	/// <returns>formatted duration modifier string</returns>
	private string GetDurationModifier(int varNum, Variable durationModifierVar)
	{
		string durationModifier = null;
		TQColor? color = null;

		if (!TranslationService.TryTranslateXTag("ImprovedTimeFormat", out var formatSpec))
		{
			formatSpec = "?with {0:f0}% Improved Duration?";
			color = ItemStyle.Legendary.TQColor();
		}
		else
		{
			if (USettings.ItemDebugLevel > 2)
				Log.LogDebug("Item.formatspec (improved time) = " + formatSpec);

			formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
		}

		durationModifier = Format(formatSpec, durationModifierVar[Math.Min(durationModifierVar.NumberOfValues - 1, varNum)]);

		return $"{color?.ColorTag()}{durationModifier}";
	}

	/// <summary>
	/// Gets a formatted chance modifier string
	/// </summary>
	/// <param name="varNum">offset number of the variable value that we are using</param>
	/// <param name="modifierChanceVar">Chance modifier variable</param>
	/// <returns>formatted chance modifier string</returns>
	private string GetChanceModifier(int varNum, Variable modifierChanceVar)
	{
		string modifierChance = null;
		TQColor? color = null;

		if (!TranslationService.TryTranslateXTag("ChanceOfTag", out var formatSpec))
		{
			formatSpec = "?{%.1f0}% Chance of?";
			color = ItemStyle.Legendary.TQColor();
		}
		else
		{
			if (USettings.ItemDebugLevel > 2)
				Log.LogDebug("Item.formatspec (chance) = " + formatSpec);

			formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
		}

		modifierChance = this.Format(formatSpec, modifierChanceVar[Math.Min(modifierChanceVar.NumberOfValues - 1, varNum)]);

		return $"{color?.ColorTag()}{modifierChance}";
	}

	/// <summary>
	/// Gets the global chance string
	/// </summary>
	/// <param name="attributeList">Arraylist containing the attributes</param>
	/// <param name="varNum">offset number of the variable value that we are using</param>
	/// <param name="v">variable structure</param>
	/// <param name="font">font string</param>
	/// <returns>formatted global chance string</returns>
	private string GetGlobalChance(List<Variable> attributeList, int varNum, Variable v, ref TQColor? font)
	{
		string line;
		string tag = "GlobalPercentChanceOfAllTag";

		// use our hack to determine if it was XOR or not.
		if (attributeList.Count > 2)
		{
			// Spurious global chance indicator.  Do not use
			line = string.Empty;
		}
		else
		{
			if (attributeList.Count > 1)
				tag = "GlobalPercentChanceOfOneTag";

			if (!TranslationService.TryTranslateXTag(tag, out var formatSpec))
			{
				formatSpec = string.Format(CultureInfo.CurrentCulture, "{0:f1}% ?{0}?", tag);
				font = ItemStyle.Legendary.TQColor();
			}
			else
			{
				if (USettings.ItemDebugLevel > 2)
					Log.LogDebug("Item.formatspec (chance of one) = " + formatSpec);

				formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
				font = ItemStyle.Epic.TQColor();
			}

			line = Format(formatSpec, v[System.Math.Min(v.NumberOfValues - 1, varNum)]);
		}

		return line;
	}


	/// <summary>
	/// Gets the formatted racial bonus string(s)
	/// </summary>
	/// <param name="record">DBRecord of the databse record</param>
	/// <param name="results">List containing the results</param>
	/// <param name="varNum">offset number of the variable value that we are using</param>
	/// <param name="isGlobal">Flag to signal global parameters</param>
	/// <param name="globalIndent">global indent string</param>
	/// <param name="v">variable structure</param>
	/// <param name="d">ItemAttributesData structure</param>
	/// <param name="line">line string</param>
	/// <param name="color">display font string</param>
	/// <returns>formatted string of racial bonus(es)  adds to the results if there are multiple.</returns>
	private string GetRacialBonus(DBRecordCollection record, Item itm, List<string> results, int varNum, bool isGlobal, string globalIndent, Variable v, ItemAttributesData d, string line, ref TQColor? color)
	{
		// Added by VillageIdiot
		// Updated to accept multiple racial bonuses in record
		string[] races = record.GetAllStrings("racialBonusRace");
		if (races != null)
		{
			for (int j = 0; j < races.Length; ++j)
			{

				if (!TranslationService.TryTranslateXTag($"racialBonusRace{races[j]}", out var finalRace))
				{
					finalRace = races[j];

					if (USettings.DebugEnabled)
						Log.LogDebug("missing racialBonusRace={0}", finalRace);
				}

				// Optimized: use span-based ToFirstCharUpperCase
				string formatTag = d.FullAttribute.AsSpan().ToFirstCharUpperCase();

				if (!TranslationService.TryTranslateXTag(formatTag, out var formatSpec))
					formatSpec = string.Concat(formatTag, " {0} {1}");
				else
				{
					if (USettings.ItemDebugLevel > 2)
						Log.LogDebug("Item.formatspec (race bonus) = " + formatSpec);

					formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
				}


				if (line != null)
				{
					if (d.Variable.Length > 0)
					{
						string s = $"{ItemStyle.Legendary.TQColor().ColorTag()}{d.Variable}";
						line = string.Concat(line, s);
					}
					else
					{
						// There are multiple lines to the attribute so the color tag needs to be added.
						string s = $"{ItemStyle.Epic.TQColor().ColorTag()}";
						line = string.Concat(s, line);
					}

					if (isGlobal)
						line = string.Concat(globalIndent, line);

					results.Add(line);
					itm.CurrentFriendlyNameResult.TmpAttrib.Add(line);
				}

				line = Format(formatSpec, v[Math.Min(v.NumberOfValues - 1, varNum)], finalRace);
				color = ItemStyle.Epic.TQColor();
			}
		}

		return line;
	}



	/// <summary>
	/// Gets the + to all skills string
	/// </summary>
	/// <param name="variableNumber">offset number of the variable value that we are using</param>
	/// <param name="variable">variable structure</param>
	/// <param name="color">display font string</param>
	/// <returns>formatted string for + to all skills</returns>
	private string GetAugmentAllLevel(int variableNumber, Variable variable, ref TQColor? color)
	{
		string tag = "ItemAllSkillIncrement";

		if (!TranslationService.TryTranslateXTag(tag, out var formatSpec))
		{
			formatSpec = "?+{0} to all skills?";
			color = ItemStyle.Legendary.TQColor();
		}
		else
		{
			if (USettings.ItemDebugLevel > 2)
				Log.LogDebug("Item.formatspec (augment level) = " + formatSpec);

			formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
			color = ItemStyle.Epic.TQColor();
		}

		return Format(formatSpec, variable[System.Math.Min(variable.NumberOfValues - 1, variableNumber)]);
	}

	/// <summary>
	/// Get + to a Mastery string
	/// </summary>
	/// <param name="record">DBRecord database record</param>
	/// <param name="variable">variable structure</param>
	/// <param name="attributeData">ItemAttributesData structure</param>
	/// <param name="font">display font string</param>
	/// <returns>formatted string with the + to mastery</returns>
	private string GetAugmentMasteryLevel(DBRecordCollection record, Variable variable, ItemAttributesData attributeData, ref TQColor? font)
	{
		// Optimized: use span to get single char
		string augmentNumber = attributeData.FullAttribute.AsSpan().Slice(19, 1).ToString();
		string augmentMasteryName = string.Concat("augmentMasteryName", augmentNumber);
		string augmentMasteryValue = record.GetString(augmentMasteryName, 0);

		if (string.IsNullOrEmpty(augmentMasteryValue))
			augmentMasteryValue = augmentMasteryName;

		string skillName = null;
		DBRecordCollection skillRecord = Database.GetRecordFromFile(augmentMasteryValue);

		if (skillRecord != null)
		{
			string nameTag = skillRecord.GetString("skillDisplayName", 0);

			if (!string.IsNullOrEmpty(nameTag))
				TranslationService.TryTranslateXTag(nameTag, out skillName);
		}

		if (string.IsNullOrEmpty(skillName))
		{
			skillName = PathIO.GetFileNameWithoutExtension(augmentMasteryValue);
			font = ItemStyle.Legendary.TQColor();
		}

		// now get the formatSpec
		if (!TranslationService.TryTranslateXTag("ItemMasteryIncrement", out var formatSpec))
		{
			formatSpec = "?+{0} to skills in {1}?";
			if (font == null)
				font = ItemStyle.Legendary.TQColor();
		}
		else
		{
			if (USettings.ItemDebugLevel > 2)
				Log.LogDebug("Item.formatspec (augment mastery) = " + formatSpec);

			formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
			if (!font.HasValue)
				font = ItemStyle.Epic.TQColor();
		}

		return Format(formatSpec, variable[0], skillName);
	}


	/// <summary>
	/// Gets a formatted + to skill string
	/// </summary>
	/// <param name="record">DBRecord database record</param>
	/// <param name="variable">variable structure</param>
	/// <param name="attributeData">ItemAttributesData structure</param>
	/// <param name="line">line of text</param>
	/// <param name="font">display font string</param>
	/// <returns>formatted string containing + to skill</returns>
	private string GetAugmentSkillLevel(DBRecordCollection record, Variable variable, ItemAttributesData attributeData, string line, ref TQColor? font)
	{
		// Optimized: use span to get single char
		string augmentSkillNumber = attributeData.FullAttribute.AsSpan().Slice(17, 1).ToString();
		string skillRecordKey = string.Concat("augmentSkillName", augmentSkillNumber);
		string skillRecordID = record.GetString(skillRecordKey, 0);

		if (!string.IsNullOrEmpty(skillRecordID))
		{
			string skillName = null;
			string nameTag = null;
			DBRecordCollection skillRecord = Database.GetRecordFromFile(skillRecordID);
			if (skillRecord != null)
			{
				// Changed by VillageIdiot
				// for augmenting buff skills
				string buffSkillName = skillRecord.GetString("buffSkillName", 0);
				if (string.IsNullOrEmpty(buffSkillName))
				{
					// Not a buff so look up the name
					nameTag = skillRecord.GetString("skillDisplayName", 0);
					if (!string.IsNullOrEmpty(nameTag))
						TranslationService.TryTranslateXTag(nameTag, out skillName);
					else
					{
						// Added by VillageIdiot
						// Check to see if this is a pet skill
						nameTag = skillRecord.GetString("Class", 0);
						if (nameTag.Contains("PetModifier"))
						{
							string petSkillID = skillRecord.GetString("petSkillName", 0);
							DBRecordCollection petSkillRecord = Database.GetRecordFromFile(petSkillID);
							if (petSkillRecord != null)
							{
								// Try to get display name
								string petNameTag = petSkillRecord.GetString("skillDisplayName", 0);
								if (!string.IsNullOrEmpty(petNameTag))
									TranslationService.TryTranslateXTag(petNameTag, out skillName);
								else
								{ // It may fail because there is another level of redirection (records\xpack\skills\dream\nightmare_petmodifier_mastermind.dbr)
									petNameTag = petSkillRecord.GetString("buffSkillName", 0);
									if (!string.IsNullOrWhiteSpace(petNameTag))
									{
										var petSkillRecordLvl2 = Database.GetRecordFromFile(petNameTag);
										if (petSkillRecordLvl2 is not null)
										{
											petNameTag = petSkillRecordLvl2.GetString("skillDisplayName", 0);
											if (!string.IsNullOrEmpty(petNameTag))
												TranslationService.TryTranslateXTag(petNameTag, out skillName);

										}
									}
								}

							}
						}
					}
				}
				else
				{
					// This is a buff skill
					DBRecordCollection buffSkillRecord = Database.GetRecordFromFile(buffSkillName);
					if (buffSkillRecord != null)
					{
						nameTag = buffSkillRecord.GetString("skillDisplayName", 0);
						if (!string.IsNullOrEmpty(nameTag))
							TranslationService.TryTranslateXTag(nameTag, out skillName);
					}
				}
			}

			if (string.IsNullOrEmpty(skillName))
				skillName = PathIO.GetFileNameWithoutExtension(skillRecordID);

			// now get the formatSpec
			if (!TranslationService.TryTranslateXTag("ItemSkillIncrement", out var formatSpec))
			{
				formatSpec = "?+{0} to skill {1}?";
				font = ItemStyle.Legendary.TQColor();
			}
			else
			{
				if (USettings.ItemDebugLevel > 2)
					Log.LogDebug("Item.formatspec (item skill) = " + formatSpec);

				formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
				font = ItemStyle.Epic.TQColor();
			}

			line = this.Format(formatSpec, variable[0], skillName);
		}

		return line;
	}

	/// <summary>
	/// Gets the formatted formulae string(s)
	/// </summary>
	/// <param name="results">results list</param>
	/// <param name="variable">variable structure</param>
	/// <param name="attributeData">ItemAttributesData structure</param>
	/// <param name="line">line of text</param>
	/// <param name="font">display font string</param>
	/// <returns>formatted formulae string</returns>
	private string GetFormulae(List<string> results, Variable variable, ItemAttributesData attributeData, string line, ref TQColor? font)
	{
		// Special case for formulae reagents
		if (attributeData.FullAttribute.StartsWith("reagent", noCase))
		{
			var reagentId = variable.GetString(0);
			DBRecordCollection reagentRecord = Database.GetRecordFromFile(reagentId);
			if (reagentRecord != null)
			{
				string nameTag = reagentRecord.GetString("description", 0);
				if (!string.IsNullOrEmpty(nameTag))
				{
					string reagentName = TranslationService.TranslateXTag(nameTag);
					string formatSpec = "{0}";
					font = ItemStyle.Common.TQColor();
					line = Format(formatSpec, reagentName);
				}
			}
		}
		else if (attributeData.FullAttribute.Equals("artifactCreationCost"))
		{
			if (!TranslationService.TryTranslateXTag("xtagArtifactCost", out var formatSpec))
				formatSpec = "Gold Cost: {0}";
			else
				formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);

			if (USettings.ItemDebugLevel > 2)
				Log.LogDebug("Item.formatspec (Artifact cost) = " + formatSpec);

			font = ItemStyle.Rare.TQColor();
			results.Add(string.Empty);
			line = Format(formatSpec, string.Format(CultureInfo.CurrentCulture, "{0:N0}", variable[0]));
		}

		return line;
	}

	/// <summary>
	/// Gets a formatted string of the granted skill
	/// </summary>
	/// <param name="record">DBRecord database record</param>
	/// <param name="results">results list</param>
	/// <param name="variable">variable structure</param>
	/// <param name="line">line of text</param>
	/// <param name="font">display font string</param>
	/// <returns>formatted granted skill string.</returns>
	private string GetGrantedSkill(DBRecordCollection record, Item itm, List<string> results, Variable variable, string line, ref TQColor? font)
	{
		// Added by VillageIdiot
		// Special case for granted skills
		var grantedSkillId = variable.GetString(0);
		DBRecordCollection skillRecord = Database.GetRecordFromFile(grantedSkillId);
		if (skillRecord != null)
		{
			// Add a blank line and then the Grants Skill text
			results.Add(string.Empty);
			font = ItemStyle.Mundane.TQColor();

			if (!TranslationService.TryTranslateXTag("tagItemGrantSkill", out var skillTag))
				skillTag = "Grants Skill :";

			var value = $"{font?.ColorTag()}{skillTag}";
			results.Add(value);
			itm.CurrentFriendlyNameResult.TmpAttrib.Add(value);

			string skillName = null;
			string nameTag = null;

			// Changed by VillageIdiot
			// Let's actually test if there is a buff skill
			string buffSkillName = skillRecord.GetString("buffSkillName", 0);
			if (string.IsNullOrEmpty(buffSkillName))
			{
				nameTag = skillRecord.GetString("skillDisplayName", 0);
				if (!string.IsNullOrEmpty(nameTag))
				{
					if (!TranslationService.TryTranslateXTag(nameTag, out skillName))
						skillName = PathIO.GetFileNameWithoutExtension(variable.GetString(0));
				}
			}
			else
			{
				// This is a buff skill
				DBRecordCollection buffSkillRecord = Database.GetRecordFromFile(buffSkillName);
				if (buffSkillRecord != null)
				{
					nameTag = buffSkillRecord.GetString("skillDisplayName", 0);
					if (!string.IsNullOrEmpty(nameTag))
					{
						if (!TranslationService.TryTranslateXTag(nameTag, out skillName))
							skillName = PathIO.GetFileNameWithoutExtension(variable.GetString(0));
					}
				}
			}

			// Added by VillageIdiot to support skill activation text
			string triggerType = null;
			string activationTag = null;
			string activationText = null;
			string autoController = record.GetString("itemSkillAutoController", 0);
			if (!string.IsNullOrEmpty(autoController))
			{
				DBRecordCollection autoControllerRecord = Database.GetRecordFromFile(autoController);
				if (autoControllerRecord != null)
					triggerType = autoControllerRecord.GetString("triggerType", 0);
			}

			// Convert TriggerType into text tag
			if (!string.IsNullOrEmpty(triggerType))
			{
				switch (triggerType.ToUpperInvariant())
				{
					case "LOWHEALTH":
						// Activated on low health
						activationTag = "xtagAutoSkillCondition01";
						break;

					case "LOWMANA":
						// Activated on low energy
						activationTag = "xtagAutoSkillCondition02";
						break;

					case "HITBYENEMY":
						// Activated upon taking damage
						activationTag = "xtagAutoSkillCondition03";
						break;

					case "HITBYMELEE":
						// Activated upon taking melee damage
						activationTag = "xtagAutoSkillCondition04";
						break;

					case "HITBYPROJECTILE":
						// Activated upon taking ranged damage
						activationTag = "xtagAutoSkillCondition05";
						break;

					case "CASTBUFF":
						// Activated upon casting a buff
						activationTag = "xtagAutoSkillCondition06";
						break;

					case "ATTACKENEMY":
						// Activated on attack
						activationTag = "xtagAutoSkillCondition07";
						break;

					case "ONEQUIP":
						// Activated when equipped
						activationTag = "xtagAutoSkillCondition08";
						break;

					default:
						activationTag = string.Empty;
						break;
				}
			}

			if (!string.IsNullOrEmpty(activationTag))
				TranslationService.TryTranslateXTag(activationTag, out activationText);
			else
				activationText = string.Empty;

			if (string.IsNullOrEmpty(activationText))
				font = ItemStyle.Epic.TQColor();
			else
				font = ItemStyle.Mundane.TQColor();

			line = Format("{0} {1}", skillName, activationText);
		}

		return line;
	}


	/// <summary>
	/// Gets the pet bonus string
	/// </summary>
	/// <param name="color">display font string</param>
	/// <returns>formatted pet bonus name</returns>
	private string GetPetBonusName(ref TQColor? color)
	{
		string tag = "xtagPetBonusNameAllPets";
		if (!TranslationService.TryTranslateXTag(tag, out var formatSpec))
		{
			formatSpec = "?Bonus to All Pets:?";
			color = ItemStyle.Legendary.TQColor();
		}
		else
		{
			if (USettings.ItemDebugLevel > 2)
				Log.LogDebug("Item.formatspec (pet bonus) = " + formatSpec);

			formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
			color = ItemStyle.Relic.TQColor();
		}

		return formatSpec;
	}


	/// <summary>
	/// Gets the skill effects string
	/// </summary>
	/// <param name="baseAttributeData">ItemAttributesData structure of the base attribute</param>
	/// <param name="variableNumber">offset number of the variable value that we are using</param>
	/// <param name="variable">variable structure</param>
	/// <param name="currentAttributeData">ItemAttributesData structure of the current attribute</param>
	/// <param name="line">line of text</param>
	/// <param name="color">display font string</param>
	/// <returns>formatted skill effect string</returns>
	private string GetSkillEffect(ItemAttributesData baseAttributeData, int variableNumber, Variable variable, ItemAttributesData currentAttributeData, string line, ref TQColor? color)
	{
		string labelTag = ItemAttributeProvider.GetAttributeTextTag(baseAttributeData);
		if (string.IsNullOrEmpty(labelTag))
		{
			labelTag = string.Concat("?", baseAttributeData.FullAttribute, "?");
			color = ItemStyle.Legendary.TQColor();
		}

		if (!TranslationService.TryTranslateXTag(labelTag, out var label))
		{
			label = string.Concat("?", labelTag, "?");
			color = ItemStyle.Legendary.TQColor();
		}

		if (USettings.ItemDebugLevel > 2)
			Log.LogDebug("Item.label (scroll) = " + label);

		label = ItemAttributeProvider.ConvertFormat(label);

		// Find the extra format tag for those that take 2 parameters.
		string formatSpecTag = null;
		string formatSpec = null;
		if (currentAttributeData.FullAttribute.EndsWith("Cost", noCase))
			formatSpecTag = "SkillIntFormat";
		else if (currentAttributeData.FullAttribute.EndsWith("Level", noCase))
			formatSpecTag = "SkillIntFormat";
		else if (currentAttributeData.FullAttribute.EndsWith("Duration", noCase))
			formatSpecTag = "SkillSecondFormat";
		else if (currentAttributeData.FullAttribute.EndsWith("Radius", noCase))
			formatSpecTag = "SkillDistanceFormat";

		if (!string.IsNullOrEmpty(formatSpecTag))
		{
			if (!TranslationService.TryTranslateXTag(formatSpecTag, out formatSpec))
			{
				formatSpec = "?{0} {1}?";
				color = ItemStyle.Legendary.TQColor();
			}
			else
			{
				if (USettings.ItemDebugLevel > 2)
					Log.LogDebug("Item.formatspec (2 parameter) = " + formatSpec);

				formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
				color = ItemStyle.Epic.TQColor();
			}
		}

		if (string.IsNullOrEmpty(formatSpecTag))
		{
			color = ItemStyle.Epic.TQColor();
			line = Format(label, variable[Math.Min(variable.NumberOfValues - 1, variableNumber)]);
		}
		else
		{
			line = Format(formatSpec, variable[Math.Min(variable.NumberOfValues - 1, variableNumber)], label);
		}

		return line;
	}

	/// <summary>
	/// Gets a raw attribute string
	/// </summary>
	/// <param name="attributeData">ItemAttributesData structure</param>
	/// <param name="variableNumber">offset number of the variable value that we are using</param>
	/// <param name="variable">variable structure</param>
	/// <param name="color">display font string</param>
	/// <returns>formatted raw attribute string</returns>
	private string GetRawAttribute(ItemAttributesData attributeData, int variableNumber, Variable variable, ref TQColor? color)
	{
		string line = null;

		string labelTag = ItemAttributeProvider.GetAttributeTextTag(attributeData);
		if (string.IsNullOrWhiteSpace(labelTag))
		{
			labelTag = string.Concat("?", attributeData.FullAttribute, "?");
			color = ItemStyle.Legendary.TQColor();
		}

		if (!TranslationService.TryTranslateXTag(labelTag, out var label))
		{
			label = string.Concat("?", labelTag, "?");
			color = ItemStyle.Legendary.TQColor();
		}

		label = ItemAttributeProvider.ConvertFormat(label);
		if (label.IndexOf('{') >= 0)
		{
			// we have a format string.  try using it.
			line = Format(label, variable[Math.Min(variable.NumberOfValues - 1, variableNumber)]);
			if (!color.HasValue)
				color = ItemStyle.Epic.TQColor();
		}
		else
		{
			// no format string.
			line = Database.VariableToStringNice(variable);
		}

		if (!color.HasValue)
			color = ItemStyle.Legendary.TQColor(); // make these unknowns stand out

		return line;
	}

	/// <summary>
	/// Converts the item's offensive attributes to a string
	/// </summary>
	/// <param name="record">DBRecord of the database record</param>
	/// <param name="attributeList">ArrayList containing the attribute list</param>
	/// <param name="data">ItemAttributesData for the item</param>
	/// <param name="recordId">string containing the record id</param>
	/// <param name="results">List containing the results</param>
	private void ConvertOffenseAttributesToString(Item itm, DBRecordCollection record, List<Variable> attributeList, ItemAttributesData data, RecordId recordId, List<string> results)
	{
		if (USettings.ItemDebugLevel > 0)
		{
			Log.LogDebug("Item.ConvertOffenseAttrToString({0}, {1}, {2}, {3}, {4})"
				, record, attributeList, data, recordId, results
			);
		}

		// If we are a relic, then sometimes there are multiple values per variable depending on how many pieces we have.
		// Let's determine which variable we want in these cases.
		int variableNumber = 0;
		if (itm.IsRelicOrCharm && recordId == itm.BaseItemId)
			variableNumber = itm.Number - 1;
		else if (itm.HasRelicOrCharmSlot1 && recordId == itm.relicID)
			variableNumber = Math.Max(itm.Var1, 1) - 1;
		else if (itm.HasRelicOrCharmSlot2 && recordId == itm.relic2ID)
			variableNumber = Math.Max(itm.Var2, 1) - 1;

		// Pet skills can also have multiple values so we attempt to decode it here
		if (itm.IsScroll || itm.IsRelicOrCharm)
			variableNumber = GetPetSkillLevel(itm, record, recordId, variableNumber);

		// Triggered skills can have also multiple values so we need to decode it here
		if (record.GetString("Class", 0).StartsWith("SKILL", noCase))
			variableNumber = GetTriggeredSkillLevel(itm, record, recordId, variableNumber);

		// See what variables we have
		ItemAttributesData minData = null;
		ItemAttributesData maxData = null;
		ItemAttributesData minDurData = null;
		ItemAttributesData maxDurData = null;
		ItemAttributesData chanceData = null;
		ItemAttributesData modifierData = null;
		ItemAttributesData durationModifierData = null;
		ItemAttributesData modifierChanceData = null;
		ItemAttributesData damageRatioData = null;  // Added by VillageIdiot

		Variable minVar = null;
		Variable maxVar = null;
		Variable minDurVar = null;
		Variable maxDurVar = null;
		Variable chanceVar = null;
		Variable modifierVar = null;
		Variable durationModifierVar = null;
		Variable modifierChanceVar = null;
		Variable damageRatioVar = null;  // Added by VillageIdiot

		bool isGlobal = ItemAttributeProvider.AttributeGroupHas(new Collection<Variable>(attributeList), "Global");
		string globalIndent = new string(' ', 4);

		foreach (Variable variable in attributeList)
		{
			if (variable == null)
				continue;

			ItemAttributesData attributeData = ItemAttributeProvider.GetAttributeData(variable.Name);
			if (attributeData == null)
			{
				// unknown attribute
				attributeData = new ItemAttributesData(ItemAttributesEffectType.Other, variable.Name, variable.Name, string.Empty, 0);
			}

			switch (attributeData.Variable.ToUpperInvariant())
			{
				case "MIN":
					minData = attributeData;
					minVar = variable;
					break;


				case "MAX":
					maxData = attributeData;
					maxVar = variable;
					break;


				case "DURATIONMIN":
					minDurData = attributeData;
					minDurVar = variable;
					break;


				case "DURATIONMAX":
					maxDurData = attributeData;
					maxDurVar = variable;
					break;


				case "CHANCE":
					chanceData = attributeData;
					chanceVar = variable;
					break;


				case "MODIFIER":
					modifierData = attributeData;
					modifierVar = variable;
					break;


				case "MODIFIERCHANCE":
					modifierChanceData = attributeData;
					modifierChanceVar = variable;
					break;


				case "DURATIONMODIFIER":
					durationModifierData = attributeData;
					durationModifierVar = variable;
					break;


				case "DRAINMIN":
					// Added by VillageIdiot
					minData = attributeData;
					minVar = variable;
					break;


				case "DRAINMAX":
					// Added by VillageIdiot
					maxData = attributeData;
					maxVar = variable;
					break;


				case "DAMAGERATIO":
					// Added by VillageIdiot
					damageRatioData = attributeData;
					damageRatioVar = variable;
					break;

			}
		}

		// Figure out the label string
		string labelTag = null;
		TQColor? labelColor = null;
		string label = GetLabelAndColorFromTag(itm, data, recordId, ref labelTag, ref labelColor);

		if (USettings.ItemDebugLevel > 1)
		{
			Log.LogDebug("Full attribute = " + data.FullAttribute);
			Log.LogDebug("Item.label = " + label);
		}

		label = ItemAttributeProvider.ConvertFormat(label);

		// Figure out the Amount string
		string amount = null;
		if (minData != null
			&& maxData != null
			&& minVar.GetSingle(Math.Min(minVar.NumberOfValues - 1, variableNumber)) != maxVar.GetSingle(Math.Min(maxVar.NumberOfValues - 1, variableNumber))
		)
		{
			if (minDurVar != null)
				amount = GetAmountRange(itm, data, variableNumber, minVar, maxVar, ref label, labelColor, minDurVar);
			else
				amount = GetAmountRange(itm, data, variableNumber, minVar, maxVar, ref label, labelColor);
		}
		else
		{
			if (minDurVar != null)
				amount = GetAmountSingle(itm, data, variableNumber, minVar, maxVar, ref label, labelColor, minDurVar);
			else
				amount = GetAmountSingle(itm, data, variableNumber, minVar, maxVar, ref label, labelColor);
		}

		// Figure out the duration string
		string duration = null;
		//If we have both minDurData and maxDurData we also need to check if the actual Values of minDurVar and maxDurVar are actually different
		float minDurVarValue = -1;
		float maxDurVarValue = -1;

		if (minDurData != null)
			minDurVarValue = (float)minDurVar[minDurVar.NumberOfValues - 1];

		if (maxDurData != null)
			maxDurVarValue = (float)maxDurVar[maxDurVar.NumberOfValues - 1];
		if (minDurData != null && maxDurData != null && minDurVarValue != maxDurVarValue)
			duration = GetDurationRange(variableNumber, minDurVar, maxDurVar);
		else
			duration = GetDurationSingle(variableNumber, minDurVar, maxDurVar);

		// Figure out the Damage Ratio string
		string damageRatio = null;
		if (damageRatioData != null)
			damageRatio = GetDamageRatio(variableNumber, damageRatioData, damageRatioVar);

		// Figure out the chance string
		string chance = null;
		if (chanceData != null)
			chance = GetChance(variableNumber, chanceVar);

		// Display the chance + label + Amount + Duration + DamageRatio
		string[] strarray = new string[5];
		int numberOfStrings = 0;
		if (!string.IsNullOrEmpty(label))
		{
			if (!label.HasColorPrefix())
				label = $"{labelColor?.ColorTag()}{label}";
		}

		if (!string.IsNullOrEmpty(chance))
			strarray[numberOfStrings++] = chance;

		if (!string.IsNullOrEmpty(amount))
			strarray[numberOfStrings++] = amount;

		if (!string.IsNullOrEmpty(label))
			strarray[numberOfStrings++] = label;

		if (!string.IsNullOrEmpty(duration))
			strarray[numberOfStrings++] = duration;

		if (!string.IsNullOrEmpty(damageRatio))
			// Added by VillageIdiot
			strarray[numberOfStrings++] = damageRatio;

		if (!string.IsNullOrEmpty(amount) || !string.IsNullOrEmpty(duration))
		{
			string amountOrDurationText = string.Join(" ", strarray, 0, numberOfStrings);

			// Figure out what color to use
			TQColor? fontColor = null;
			if (!isGlobal
				&& (string.IsNullOrEmpty(chance) || data.Effect.Equals("defensiveBlock"))
				&& recordId == itm.BaseItemId
				&& string.IsNullOrEmpty(duration)
				&& !string.IsNullOrEmpty(amount)
			)
			{
				if (itm.IsWeapon)
				{
					if (data.Effect.Equals("offensivePierceRatio")
						|| data.Effect.Equals("offensivePhysical")
						|| data.Effect.Equals("offensiveBaseFire")
						|| data.Effect.Equals("offensiveBaseCold")
						|| data.Effect.Equals("offensiveBaseLightning")
						|| data.Effect.Equals("offensiveBaseLife")
					)
					{
						// mundane effect
						fontColor = ItemStyle.Mundane.TQColor();
					}
				}

				if (itm.IsShield)
				{
					if (data.Effect.Equals("defensiveBlock")
						|| data.Effect.Equals("blockRecoveryTime")
						|| data.Effect.Equals("offensivePhysical")
					)
					{
						fontColor = ItemStyle.Mundane.TQColor();
					}
				}
			}

			// magical effect
			if (!fontColor.HasValue)
				fontColor = ItemStyle.Epic.TQColor();

			if (!amountOrDurationText.HasColorPrefix())
				amountOrDurationText = $"{fontColor?.ColorTag()}{amountOrDurationText}";

			if (isGlobal)
				amountOrDurationText = amountOrDurationText.InsertAfterColorPrefix(globalIndent);

			results.Add(amountOrDurationText);
			itm.CurrentFriendlyNameResult.TmpAttrib.Add(amountOrDurationText);
		}
		else
		{
			// null these out to indicate they did not get used
			amount = null;
			duration = null;
			chance = null;
		}

		// now see if we have a modifier
		string modifier = null;
		if (modifierData != null)
			modifier = GetModifier(data, variableNumber, modifierData, modifierVar);

		string durationModifier = null;
		if (durationModifierData != null)
			durationModifier = GetDurationModifier(variableNumber, durationModifierVar);

		string modifierChance = null;
		if (modifierChanceData != null)
			modifierChance = GetChanceModifier(variableNumber, modifierChanceVar);

		numberOfStrings = 0;
		if (!string.IsNullOrEmpty(modifierChance))
			strarray[numberOfStrings++] = modifierChance;

		if (!string.IsNullOrEmpty(modifier))
			strarray[numberOfStrings++] = modifier;

		if (!string.IsNullOrEmpty(durationModifier))
			strarray[numberOfStrings++] = durationModifier;

		if (!string.IsNullOrEmpty(modifier))
		{
			string modifierText = string.Join(" ", strarray, 0, numberOfStrings);

			if (isGlobal)
				modifierText = string.Concat(globalIndent, modifierText);

			if (!modifierText.HasColorPrefix())
				modifierText = $"{ItemStyle.Epic.TQColor().ColorTag()}{modifierText}";

			results.Add(modifierText);
			itm.CurrentFriendlyNameResult.TmpAttrib.Add(modifierText);
		}
		else
		{
			modifier = null;
			modifierChance = null;
			durationModifier = null;
		}

		// Added so we only show the title once for the group.
		bool displayDamageQualifierTitle = true;

		// Now display any other variables we did not see here.
		foreach (Variable variable in attributeList)
		{
			if (variable == null)
				continue;

			ItemAttributesData attributeData = ItemAttributeProvider.GetAttributeData(variable.Name);

			if (attributeData == null)
				// unknown attribute
				attributeData = new ItemAttributesData(ItemAttributesEffectType.Other, variable.Name, variable.Name, string.Empty, 0);

			string normalizedAttributeVariable = attributeData.Variable.ToUpperInvariant();
			if (
				!(
					amount != null
					&& (normalizedAttributeVariable == "MIN"
						|| normalizedAttributeVariable == "MAX"
						|| normalizedAttributeVariable == "DRAINMIN"
						|| normalizedAttributeVariable == "DRAINMAX"
					)
				)
				&& !(duration != null && (normalizedAttributeVariable == "DURATIONMIN" || normalizedAttributeVariable == "DURATIONMAX"))
				&& !(chance != null && normalizedAttributeVariable == "CHANCE")
				&& !(modifier != null && normalizedAttributeVariable == "MODIFIER")
				&& !(durationModifier != null && normalizedAttributeVariable == "DURATIONMODIFIER")
				&& !(modifierChance != null && normalizedAttributeVariable == "MODIFIERCHANCE")
				&& !(damageRatio != null && normalizedAttributeVariable == "DAMAGERATIO")
				&& normalizedAttributeVariable != "GLOBAL"
				&& !(normalizedAttributeVariable == "XOR" && isGlobal)
			)
			{
				string line = null;
				TQColor? color = null;
				string normalizedFullAttribute = attributeData.FullAttribute.ToUpperInvariant();
				if (normalizedFullAttribute == "CHARACTERBASEATTACKSPEEDTAG")
				{
					// only display itm tag if we are a basic weapon
					if (itm.IsWeapon && recordId == itm.BaseItemId)
					{
						color = ItemStyle.Mundane.TQColor();
						line = TranslationService.TranslateXTag(variable.GetString(0));
					}
					else
						line = string.Empty;
				}
				else if (normalizedFullAttribute.EndsWith("GLOBALCHANCE", noCase))
					line = GetGlobalChance(attributeList, variableNumber, variable, ref color);
				else if (normalizedFullAttribute.StartsWith("RACIALBONUS", noCase))
					line = GetRacialBonus(record, itm, results, variableNumber, isGlobal, globalIndent, variable, attributeData, line, ref color);
				else if (normalizedFullAttribute == "AUGMENTALLLEVEL")
					line = GetAugmentAllLevel(variableNumber, variable, ref color);
				else if (normalizedFullAttribute.StartsWith("AUGMENTMASTERYLEVEL", noCase))
					line = GetAugmentMasteryLevel(record, variable, attributeData, ref color);
				else if (normalizedFullAttribute.StartsWith("AUGMENTSKILLLEVEL", noCase))
					line = GetAugmentSkillLevel(record, variable, attributeData, line, ref color);
				else if (itm.IsFormulae && recordId == itm.BaseItemId)
					line = GetFormulae(results, variable, attributeData, line, ref color);
				else if (normalizedFullAttribute == "ITEMSKILLNAME")
					line = GetGrantedSkill(record, itm, results, variable, line, ref color);

				// Added by VillageIdiot
				// Shows the header text for the pet bonus
				if (normalizedFullAttribute == "PETBONUSNAME")
					line = StringHelper.TQNewLineTag + GetPetBonusName(ref color);

				// Added by VillageIdiot
				// Set the scale percent here
				if (recordId == itm.BaseItemId && normalizedFullAttribute == "ATTRIBUTESCALEPERCENT" && itm.itemScalePercent == 1.00)
				{
					itm.itemScalePercent += variable.GetSingle(0) / 100;

					// Set line to nothing so we do not see the tag text.
					line = string.Empty;
				}
				else if (normalizedFullAttribute == "SKILLNAME")
				{
					// Added by VillageIdiot
					// itm is for Scroll effects which get decoded in the skill code below
					// Set line to nothing so we do not see the tag text.
					line = string.Empty;
				}
				else if (attributeData.EffectType == ItemAttributesEffectType.SkillEffect)
				{
					line = GetSkillEffect(data, variableNumber, variable, attributeData, line, ref color);
				}
				else if (normalizedFullAttribute.EndsWith("DAMAGEQUALIFIER", noCase))
				{
					// Added by VillageIdiot
					// for Damage Absorption

					// Get the qualifier title
					if (!TranslationService.TryTranslateXTag("tagDamageAbsorptionTitle", out var title))
						title = "Protects Against :";

					// We really only want to show the title once for the group.
					if (displayDamageQualifierTitle)
					{
						results.Add(title);
						itm.CurrentFriendlyNameResult.TmpAttrib.Add(title);
						displayDamageQualifierTitle = false;
					}

					// Show the damage type - Optimized using span-based RemoveSuffix and ToFirstCharUpperCase
					string damageTag = attributeData.FullAttribute.AsSpan().RemoveSuffix(15);
					damageTag = damageTag.AsSpan().ToFirstCharUpperCase();
					TranslationService.TryTranslateXTag(string.Concat("tagQualifyingDamage", damageTag), out var damageType);

					if (!TranslationService.TryTranslateXTag("formatQualifyingDamage", out var formatSpec))
						formatSpec = "{0}";
					else
					{
						if (USettings.ItemDebugLevel > 2)
							Log.LogDebug("Item.formatspec (Damage type) = " + formatSpec);

						formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
					}

					color = ItemStyle.Mundane.TQColor();
					line = Format(formatSpec, damageType);
				}

				// We have no line so just show the raw attribute
				if (line == null)
					line = GetRawAttribute(data, variableNumber, variable, ref color);

				// Start finalizing the line of text
				string itemSkillAutoController = null;
				if (line.Length > 0)
				{
					if (attributeData.Variable.Length > 0)
						line = string.Concat(
							line
							, ' '
							, $"{ItemStyle.Legendary.TQColor().ColorTag()}{attributeData.Variable}"
						);

					// Add another special case for skill name formatting
					// if it's an activated skill
					if (normalizedFullAttribute == "ITEMSKILLNAME")
					{
						itemSkillAutoController = record.GetString("itemSkillAutoController", 0);
						if (!string.IsNullOrEmpty(itemSkillAutoController))
						{
							// Granted Skill name BOLD. TODO must implement extended ColorTag {^(?<Color>.)(?<Style>.)} to support this.
							//line = string.Concat("<b>", line, "</b>");
						}
					}

					line = $"{color?.ColorTag()}{line}";

					if (isGlobal)
						line = line.InsertAfterColorPrefix(globalIndent);

					// Indent formulae reagents
					if (itm.IsFormulae && normalizedFullAttribute.StartsWith("REAGENT", noCase))
						line = line.InsertAfterColorPrefix(globalIndent);

					results.Add(line);
					itm.CurrentFriendlyNameResult.TmpAttrib.Add(line);
				}

				// Added by VillageIdiot
				// itm a special case for pet bonuses
				if (normalizedFullAttribute == "PETBONUSNAME")
				{
					string petBonusID = record.GetString("petBonusName", 0);
					DBRecordCollection petBonusRecord = Database.GetRecordFromFile(petBonusID);
					if (petBonusRecord != null)
					{
						var tmp = new List<string>();
						GetAttributesFromRecord(itm, petBonusRecord, true, petBonusID, tmp);
						results.AddRange(tmp.Select(s => s.InsertAfterColorPrefix(globalIndent)));
						results.Add(string.Empty);
					}
				}

				// Added by VillageIdiot
				// Another special case for skill description and effects of activated skills
				if (normalizedFullAttribute == "ITEMSKILLNAME" || ((itm.IsScroll || itm.IsPotion) && normalizedFullAttribute == "SKILLNAME"))
					GetSkillDescriptionAndEffects(itm, record, results, variable, line);
			}
		}

		if (USettings.ItemDebugLevel > 0)
			Log.LogDebug("Exiting Item.ConvertOffenseAttrToString()");
	}

	/// <summary>
	/// Adds the formatted skill description and effects for granted skills to the results list
	/// </summary>
	/// <param name="record">DBRecord databse record</param>
	/// <param name="results">results list</param>
	/// <param name="variable">variable structure</param>
	/// <param name="line">line of text</param>
	private void GetSkillDescriptionAndEffects(Item itm, DBRecordCollection record, List<string> results, Variable variable, string line)
	{
		string autoController = record.GetString("itemSkillAutoController", 0);
		string SkillDescriptionAndEffectsVar = variable.GetString(0);

		if (!string.IsNullOrEmpty(autoController) || itm.IsScroll || itm.IsPotion)
		{
			DBRecordCollection skillRecord = Database.GetRecordFromFile(SkillDescriptionAndEffectsVar);

			// Changed by VillageIdiot
			// Get title from the last line
			// Remove the HTML formatting and use for word wrapping
			string lastline = string.Empty;
			if (!itm.IsScroll)
				lastline = autoController ?? string.Empty;

			// Set the minimum column width to 30
			// Also takes care of scrolls
			int lineLength = lastline.Length;
			if (lineLength < 30)
				lineLength = 30;

			// Show the description text first
			if (skillRecord != null)
			{
				string buffSkillName = skillRecord.GetString("buffSkillName", 0);
				var buffSkillNameId = buffSkillName.ToRecordId();

				if (!itm.IsScroll)
				{
					// Skip scrolls since they are handled elsewhere with the flavor text
					string descriptionTag, skillDescription;
					Collection<string> skillDescriptionList;

					// Changed by VillageIdiot
					// Let's actually test if it's a buff skill
					if (string.IsNullOrEmpty(buffSkillName))
					{
						descriptionTag = skillRecord.GetString("skillBaseDescription", 0);
						if (descriptionTag.Length != 0)
						{
							if (TranslationService.TryTranslateXTag(descriptionTag, out skillDescription))
							{
								skillDescriptionList = StringHelper.WrapWords(skillDescription, lineLength);

								foreach (string skillDescriptionFromList in skillDescriptionList)
								{
									var value = $"{ItemStyle.Mundane.TQColor().ColorTag()}    {skillDescriptionFromList}";
									results.Add(value);
									itm.CurrentFriendlyNameResult.TmpAttrib.Add(value);
								}

								// Show granted skill level
								if (this.USettings.ShowSkillLevel)
								{
									if (!TranslationService.TryTranslateXTag("MenuLevel", out var formatSpec))
										formatSpec = "Level:   {0}";
									else
										formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);

									int skillLevel = record.GetInt32("itemSkillLevel", 0);
									if (skillLevel > 0)
									{
										line = Format(formatSpec, skillLevel);
										var value = $"{ItemStyle.Mundane.TQColor().ColorTag()}{line}";
										results.Add(value);
										itm.CurrentFriendlyNameResult.TmpAttrib.Add(value);
									}
								}
							}
						}
					}
					else
					{
						// itm skill is a buff
						DBRecordCollection buffSkillRecord = Database.GetRecordFromFile(buffSkillNameId);
						if (buffSkillRecord != null)
						{
							descriptionTag = buffSkillRecord.GetString("skillBaseDescription", 0);
							if (!string.IsNullOrEmpty(descriptionTag))
							{
								skillDescription = TranslationService.TranslateXTag(descriptionTag);
								skillDescriptionList = StringHelper.WrapWords(skillDescription, lineLength);
								foreach (string skillDescriptionFromList in skillDescriptionList)
								{
									var value = $"{ItemStyle.Mundane.TQColor().ColorTag()}    {skillDescriptionFromList}";
									results.Add(value);
									itm.CurrentFriendlyNameResult.TmpAttrib.Add(value);
								}

								// Show granted skill level
								if (this.USettings.ShowSkillLevel)
								{
									if (!TranslationService.TryTranslateXTag("MenuLevel", out var formatSpec))
										formatSpec = "Level:   {0}";
									else
										formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);

									int skillLevel = record.GetInt32("itemSkillLevel", 0);
									if (skillLevel > 0)
									{
										line = Format(formatSpec, skillLevel);
										var value = $"{ItemStyle.Mundane.TQColor().ColorTag()}    {line}";
										results.Add(value);
										itm.CurrentFriendlyNameResult.TmpAttrib.Add(value);
									}
								}
							}
						}
					}
				}

				// Clear out effects for unnamed skills, unless it's a buff or a scroll.
				if (skillRecord.GetString("skillDisplayName", 0).Length == 0 && string.IsNullOrEmpty(buffSkillName) && !itm.IsScroll)
					skillRecord = null;

				// Added by VillageIdiot
				// Adjust for the flavor text of scrolls
				if (skillRecord != null && !itm.IsScroll && !itm.IsPotion)
					results.Add(string.Empty);

				// Added by VillageIdiot
				// Add the skill effects
				if (skillRecord != null)
				{
					// itm is a summon
					if (skillRecord.GetString("Class", 0).Equals("SKILL_SPAWNPET", noCase))
						ConvertPetStats(itm, skillRecord, results);
					else
					{
						// Skill Effects
						if (!string.IsNullOrEmpty(buffSkillName))
							GetAttributesFromRecord(itm, Database.GetRecordFromFile(buffSkillNameId), true, buffSkillNameId, results);
						else
							GetAttributesFromRecord(itm, skillRecord, true, SkillDescriptionAndEffectsVar, results);
					}
				}
			}
		}
	}


	/// <summary>
	/// Used for showing the pet statistics
	/// </summary>
	/// <param name="skillRecord">DBRecord of the skill</param>
	/// <param name="results">List containing the results</param>
	private void ConvertPetStats(Item itm, DBRecordCollection skillRecord, List<string> results)
	{
		string formatSpec, petLine;
		int summonLimit = skillRecord.GetInt32("petLimit", 0);
		if (summonLimit > 1)
		{
			if (!TranslationService.TryTranslateXTag("SkillPetLimit", out formatSpec))
				formatSpec = "{0} Summon Limit";
			else
				formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);

			petLine = string.Format(CultureInfo.CurrentCulture, formatSpec, summonLimit.ToString(CultureInfo.CurrentCulture));
			var value = $"{ItemStyle.Mundane.TQColor().ColorTag()}{petLine}";
			results.Add(value);
			itm.CurrentFriendlyNameResult.TmpAttrib.Add(value);
		}
		var skillRecordspawnObjects = skillRecord.GetString("spawnObjects", 0);
		DBRecordCollection petRecord = Database.GetRecordFromFile(skillRecordspawnObjects);
		if (petRecord != null)
		{
			// Print out Pet attributes
			if (!TranslationService.TryTranslateXTag("SkillPetDescriptionHeading", out formatSpec))
				formatSpec = "{0} Attributes:";
			else
				formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);

			string petNameTag = petRecord.GetString("description", 0);
			string petName = TranslationService.TranslateXTag(petNameTag);
			float value = 0.0F;

			petLine = string.Format(CultureInfo.CurrentCulture, formatSpec, petName);
			var valueStr = $"{ItemStyle.Mundane.TQColor().ColorTag()}{petLine}";
			results.Add(valueStr);
			itm.CurrentFriendlyNameResult.TmpAttrib.Add(valueStr);

			// Time to live
			if (!TranslationService.TryTranslateXTag("tagSkillPetTimeToLive", out formatSpec))
				formatSpec = "Life Time {0} Seconds";
			else
				formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);

			petLine = string.Format(CultureInfo.CurrentCulture, formatSpec, skillRecord.GetSingle("spawnObjectsTimeToLive", 0));
			valueStr = $"{ItemStyle.Mundane.TQColor().ColorTag()}{petLine}";
			results.Add(valueStr);
			itm.CurrentFriendlyNameResult.TmpAttrib.Add(valueStr);

			// Health
			value = petRecord.GetSingle("characterLife", 0);
			if (value != 0.0F)
			{
				if (!TranslationService.TryTranslateXTag("SkillPetDescriptionHealth", out formatSpec))
					formatSpec = "{0}  Health";
				else
					formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);

				petLine = string.Format(CultureInfo.CurrentCulture, formatSpec, value);
				valueStr = $"{ItemStyle.Mundane.TQColor().ColorTag()}{petLine}";
				results.Add(valueStr);
				itm.CurrentFriendlyNameResult.TmpAttrib.Add(valueStr);
			}

			// Energy
			value = petRecord.GetSingle("characterMana", 0);
			if (value != 0.0F)
			{
				if (!TranslationService.TryTranslateXTag("SkillPetDescriptionMana", out formatSpec))
					formatSpec = "{0}  Energy";
				else
					formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);

				petLine = string.Format(CultureInfo.CurrentCulture, formatSpec, value);
				valueStr = $"{ItemStyle.Mundane.TQColor().ColorTag()}{petLine}";
				results.Add(valueStr);
				itm.CurrentFriendlyNameResult.TmpAttrib.Add(valueStr);
			}

			// Add abilities text
			results.Add(string.Empty);
			if (!TranslationService.TryTranslateXTag("tagSkillPetAbilities", out formatSpec))
				formatSpec = "{0} Abilities:";
			else
				formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);

			petLine = string.Format(CultureInfo.CurrentCulture, formatSpec, petName);
			valueStr = $"{ItemStyle.Mundane.TQColor().ColorTag()}{petLine}";
			results.Add(valueStr);
			itm.CurrentFriendlyNameResult.TmpAttrib.Add(valueStr);

			// Show Physical Damage
			value = petRecord.GetSingle("handHitDamageMin", 0);
			float value2 = petRecord.GetSingle("handHitDamageMax", 0);

			if (value > 1.0F || value2 > 2.0F)
			{
				if (value2 == 0.0F || value == value2)
				{
					if (!TranslationService.TryTranslateXTag("SkillPetDescriptionDamageMinOnly", out formatSpec))
						formatSpec = "{0}  Damage";
					else
						formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);

					petLine = string.Format(CultureInfo.CurrentCulture, formatSpec, value);
					valueStr = $"{ItemStyle.Mundane.TQColor().ColorTag()}{petLine}";
					results.Add(valueStr);
					itm.CurrentFriendlyNameResult.TmpAttrib.Add(valueStr);
				}
				else
				{
					if (!TranslationService.TryTranslateXTag("SkillPetDescriptionDamageMinMax", out formatSpec))
						formatSpec = "{0} - {1}  Damage";
					else
						formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);

					petLine = string.Format(CultureInfo.CurrentCulture, formatSpec, value, value2);
					valueStr = $"{ItemStyle.Mundane.TQColor().ColorTag()}{petLine}";
					results.Add(valueStr);
					itm.CurrentFriendlyNameResult.TmpAttrib.Add(valueStr);
				}
			}

			// Show the pet's skills
			string[] skills = new string[17];  // Upto 17 skills in the tree
			int[] skillLevels = new int[17];
			int numSkills = 0;
			string petskillName;
			int petskillLevel;

			for (int i = 0; i < skills.Length; i++)
			{
				petskillName = petRecord.GetString(string.Concat("skillName", i), 0);
				if (string.IsNullOrEmpty(petskillName))
					continue;

				skills[numSkills] = petskillName;
				petskillLevel = petRecord.GetInt32(string.Concat("skillLevel", i), 0);
				if (petskillLevel < 1)
					petskillLevel = 1;

				skillLevels[numSkills] = petskillLevel;
				numSkills++;
			}

			for (int i = 0; i < numSkills; i++)
			{
				string recordID = skills[i];
				if (recordID != null && !recordID.ToLower().StartsWith("records"))
					continue;

				DBRecordCollection skillRecord1 = Database.GetRecordFromFile(recordID);
				DBRecordCollection record = null;
				string skillClass = skillRecord1.GetString("Class", 0);

				// Skip passive skills
				if (skillClass.Equals("SKILL_PASSIVE", noCase))
					continue;

				string skillNameTag = null;
				string skillName = null;
				string buffSkillName = skillRecord1.GetString("buffSkillName", 0);

				if (string.IsNullOrEmpty(buffSkillName))
				{
					record = skillRecord1;
					skillNameTag = skillRecord.GetString("skillDisplayName", 0);
					if (!string.IsNullOrWhiteSpace(skillNameTag))
						TranslationService.TryTranslateXTag(skillNameTag, out skillName);
				}
				else
				{
					// This is a buff skill
					DBRecordCollection buffSkillRecord = Database.GetRecordFromFile(buffSkillName);
					if (buffSkillRecord != null)
					{
						record = buffSkillRecord;
						recordID = buffSkillName;
						skillNameTag = buffSkillRecord.GetString("skillDisplayName", 0);
						if (!string.IsNullOrWhiteSpace(skillNameTag))
							TranslationService.TryTranslateXTag(skillNameTag, out skillName);
					}
				}

				if (string.IsNullOrWhiteSpace(skillName))
					valueStr = $"{ItemStyle.Legendary.TQColor().ColorTag()}{skillNameTag}";
				else
					valueStr = $"{ItemStyle.Mundane.TQColor().ColorTag()}{skillName}";

				results.Add(valueStr);
				itm.CurrentFriendlyNameResult.TmpAttrib.Add(valueStr);

				GetAttributesFromRecord(itm, record, true, recordID, results);
				results.Add(string.Empty);
			}
		}
	}


	/// <summary>
	/// Gets the item label from the tag
	/// </summary>
	/// <param name="data">ItemAttributesData structure for the attribute</param>
	/// <param name="recordId">string containing the database record id</param>
	/// <param name="labelTag">the label tag</param>
	/// <param name="labelColor">the label color which gets modified here</param>
	/// <returns>string containing the label.</returns>
	private string GetLabelAndColorFromTag(Item itm, ItemAttributesData data, RecordId recordId, ref string labelTag, ref TQColor? labelColor)
	{
		labelTag = ItemAttributeProvider.GetAttributeTextTag(data);
		string label, TrailingNL = string.Empty;

		if (string.IsNullOrEmpty(labelTag))
		{
			labelTag = string.Concat("?", data.FullAttribute, "?");
			labelColor = ItemStyle.Legendary.TQColor();
		}

		// if itm is an Armor effect and we are not armor, then change it to bonus
		if (labelTag.Equals("DEFENSEABSORPTIONPROTECTION", noCase))
		{
			if (!itm.IsArmor || recordId != itm.BaseItemId)
			{
				labelTag = "DefenseAbsorptionProtectionBonus";
				labelColor = ItemStyle.Epic.TQColor();
			}
			else
			{
				// regular armor attribute is not magical
				labelColor = ItemStyle.Mundane.TQColor();
				// Add trailing '\n' + space for regular armor pieces to force empty row in tooltip (only if is first attribute)
				if (!itm.CurrentFriendlyNameResult.TmpAttrib.Any())
					TrailingNL = StringHelper.TQNewLineTag + ' ';
			}
		}

		if (!TranslationService.TryTranslateXTag(labelTag, out label))
		{
			label = string.Concat("?", labelTag, "?");
			labelColor = ItemStyle.Legendary.TQColor();
		}

		return $"{label}{TrailingNL}";
	}


	/// <summary>
	/// For displaying raw attribute data
	/// </summary>
	/// <param name="attributeList">ArrayList containing the arributes</param>
	/// <param name="results">List containing the attribute strings.</param>
	private void ConvertBareAttributeListToString(List<Variable> attributeList, List<string> results)
	{
		foreach (Variable variable in attributeList)
		{
			if (variable != null)
				results.Add(variable.ToString());
		}
	}
}
