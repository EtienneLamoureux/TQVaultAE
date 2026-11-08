using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Data;

public partial class ItemProvider
{
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
	private string GetAmountRange(Item itm, ItemAttributesData data, int varNum, Variable minVar, Variable maxVar, ref string? label, TQColor? labelColor, Variable? minDurVar = null)
	{
		// Added by VillageIdiot : check to see if min and max are the same
		TQColor? color = null;
		string? amount = null;

		Variable min = minVar.Clone();
		Variable max = maxVar.Clone();

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
		else if (data.Effect.Equals("defensiveBlock", StringComparison.Ordinal))
			tag = "DefenseBlock";

		if (!TranslationService.TryTranslateXTag(tag, out var formatSpec))
		{
			formatSpec = "{0}..{1}";
			color = ItemStyle.Legendary.TQColor();
		}
		else
		{
			if (USettings.ItemDebugLevel > 2)
				Log.LogDebug("Item.formatspec (range) = {FormatSpec}", formatSpec);

			formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
		}

		if (label is not null && label.Contains('{'))
		{
			// the label has formatting codes.  Use it to format the amount
			formatSpec = label;
			label = null;
			color = labelColor;
		}

		// Added by VillageIdiot
		// Adjust for itemScalePercent
		// AMS: Added If the value is reliant on the duration.
		if ((minDurVar is not null) && IsDurationReliantValue(data.Effect))
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
	private string? GetAmountSingle(Item itm, ItemAttributesData data, int varNum, Variable? minVar, Variable? maxVar, ref string? label, TQColor? labelColor, Variable? minDurVar = null)
	{
		TQColor? color = null;
		string? amount = null;

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
				Log.LogDebug("Item.formatspec (single) = {FormatSpec}", formatSpec);

			formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
		}

		if (label is not null && label.Contains('{'))
		{
			// the label has formatting codes.  Use it to format the amount
			formatSpec = label;
			label = null;
			color = labelColor;
		}

		Variable? currentVariable = null;

		if (minVar is not null)
			currentVariable = minVar.Clone();
		else if (maxVar is not null)
			currentVariable = maxVar.Clone();

		if (currentVariable is not null)
		{
			// Adjust for itemScalePercent
			// only for floats
			var curvar = currentVariable[Math.Min(currentVariable.NumberOfValues - 1, varNum)];
			if (currentVariable.DataType == VariableDataType.Float)
			{
				if ((minDurVar is not null) && IsDurationReliantValue(data.Effect))
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
		string? duration = null;
		TQColor? color = null;

		if (!TranslationService.TryTranslateXTag("DamageRangeFormatTime", out var formatSpec))
		{
			formatSpec = "for {0}..{1} seconds";
			color = ItemStyle.Legendary.TQColor();
		}
		else
		{
			if (USettings.ItemDebugLevel > 2)
				Log.LogDebug("Item.formatspec (time range) = {FormatSpec}", formatSpec);

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
	private string? GetDurationSingle(int varNum, Variable? minDurVar, Variable? maxDurVar)
	{
		string? duration = null;
		TQColor? color = null;

		if (!TranslationService.TryTranslateXTag("DamageSingleFormatTime", out var formatSpec))
		{
			formatSpec = "{0}";
			color = ItemStyle.Legendary.TQColor();
		}
		else
		{
			if (USettings.ItemDebugLevel > 2)
				Log.LogDebug("Item.formatspec (time single) = {FormatSpec}", formatSpec);

			formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
		}

		Variable? durationVariable = minDurVar ?? maxDurVar;

		if (durationVariable is not null)
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
		string? damageRatio = null;
		TQColor? color = null;
		string? formatSpec = null;

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
				Log.LogDebug("Item.formatspec (percent) = {FormatSpec}", formatSpec);

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
	private string? GetChance(int varNum, Variable chanceVar)
	{
		string? chance = null;
		TQColor? color = null;

		if (!TranslationService.TryTranslateXTag("ChanceOfTag", out var formatSpec))
		{
			formatSpec = "?{%.1f0}% Chance of?";
			color = ItemStyle.Legendary.TQColor();
		}

		if (USettings.ItemDebugLevel > 2)
			Log.LogDebug("Item.formatspec (chance) = {FormatSpec}", formatSpec);

		formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
		if (chanceVar is not null)
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
		string? modifier = null;
		TQColor? color = null;
		string? formatSpec = null;

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
					Log.LogDebug("Item.formatspec (percent) = {FormatSpec}", formatSpec);

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
		string? durationModifier = null;
		TQColor? color = null;

		if (!TranslationService.TryTranslateXTag("ImprovedTimeFormat", out var formatSpec))
		{
			formatSpec = "?with {0:f0}% Improved Duration?";
			color = ItemStyle.Legendary.TQColor();
		}
		else
		{
			if (USettings.ItemDebugLevel > 2)
				Log.LogDebug("Item.formatspec (improved time) = {FormatSpec}", formatSpec);

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
		string? modifierChance = null;
		TQColor? color = null;

		if (!TranslationService.TryTranslateXTag("ChanceOfTag", out var formatSpec))
		{
			formatSpec = "?{%.1f0}% Chance of?";
			color = ItemStyle.Legendary.TQColor();
		}

		if (USettings.ItemDebugLevel > 2)
			Log.LogDebug("Item.formatspec (chance) = {FormatSpec}", formatSpec);

		// The fallback is in the game's format too: string.Format would throw on "{%.1f0}".
		formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);

		modifierChance =this.Format(formatSpec, modifierChanceVar[Math.Min(modifierChanceVar.NumberOfValues - 1, varNum)]);

		return $"{color?.ColorTag()}{modifierChance}";
	}
}
