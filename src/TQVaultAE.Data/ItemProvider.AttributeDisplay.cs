//-----------------------------------------------------------------------
// <copyright file="Item.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Globalization;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Domain.Jitter;

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
	/// <param name="lineSlots">Receives, for each line added to <paramref name="results"/>, where the game lists its attribute group.</param>
	private void GetAttributesFromRecord(Item itm, DBRecordCollection? record, bool filtering, RecordId recordId, List<string> results, bool convertStrings = true
		, IReadOnlyDictionary<string, ValueRange>? valueRanges = null, List<string?>? lineRanges = null, int? skillLevelIndex = null
		, List<GameAttributeSlot>? lineSlots = null, IReadOnlySet<string>? hiddenVariables = null)
	{
		if (USettings.ItemDebugLevel > 0)
		{
			Log.LogDebug("Item.GetAttributesFromRecord({Record}, {Filtering}, {RecordId}, {Results}, {ConvertStrings})"
				, record, filtering, recordId, results, convertStrings
			);
		}

		// First get a list of attributes, grouped by effect.
		Dictionary<string, List<Variable?>> attrByEffect = [];
		if (record is null)
		{
			if (USettings.ItemDebugLevel > 0)
				Log.LogDebug("Error - record was null.");

			results.Add("<unknown>");
			while (lineSlots is not null && lineSlots.Count < results.Count)
				lineSlots.Add(GameAttributeSlot.Other);
			return;
		}

		if (USettings.ItemDebugLevel > 1)
			Log.LogDebug("{RecordId}", record.Id);

		// Added by Village Idiot
		// To keep track of groups so they are not counted twice
		List<string> countedGroups = [];

		foreach (Variable variable in record)
		{
			if (USettings.ItemDebugLevel > 2)
				Log.LogDebug("{VariableName}", variable.Name);


			if (this.FilterValue(variable, !filtering))
				continue;

			if (hiddenVariables?.Contains(variable.Name) == true)
				continue;

			if (filtering && this.FilterKey(variable.Name))
				continue;

			if (filtering && this.FilterRequirements(variable.Name))
				continue;

			ItemAttributesData? data = ItemAttributeProvider.GetAttributeData(variable.Name);
			if (data is null)
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
			List<Variable?>? attrList;
			if (!attrByEffect.TryGetValue(effectGroup, out attrList))
			{
				attrList = [];
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
		List<Variable?>[] attrArray = new List<Variable?>[attrByEffect.Count];
		attrByEffect.Values.CopyTo(attrArray, 0);
		Array.Sort(attrArray, itm.IsArmor || itm.IsShield ? this.SortArmorAttributeGroups : this.SortOtherAttributeGroups);

		// Now for the global params, we need to check to see if they are XOR or all.
		// We do itm by checking the effect just after the global param.
		for (int i = 0; i < attrArray.Length; ++i)
		{
			List<Variable?> attrList = attrArray[i];

			if (ItemAttributeProvider.AttributeGroupIs(new Collection<Variable?>(attrList), "offensiveGlobalChance") ||
				ItemAttributeProvider.AttributeGroupIs(new Collection<Variable?>(attrList), "retaliationGlobalChance"))
			{
				// check the next effect group
				int j = i + 1;
				if (j < attrArray.Length)
				{
					List<Variable?> next = attrArray[j];
					if (!ItemAttributeProvider.AttributeGroupHas(new Collection<Variable?>(next), "Global"))
					{
						// itm is a spurious globalChance entry.  Let's add 2 null entries to signal it should be ignored
						attrList.Add(null);
						attrList.Add(null);
					}
					else if (ItemAttributeProvider.AttributeGroupHas(new Collection<Variable?>(next), "XOR"))
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

		// Keep lineRanges and lineSlots aligned with results (one entry per line).
		while (lineRanges is not null && lineRanges.Count < results.Count)
			lineRanges.Add(null);
		while (lineSlots is not null && lineSlots.Count < results.Count)
			lineSlots.Add(GameAttributeSlot.Other);

		foreach (List<Variable?> attrList in attrArray)
		{
			// Used to sort out the Damage Qualifier effects.
			if (ItemAttributeProvider.AttributeGroupIs(new Collection<Variable?>(attrList), ItemAttributesEffectType.DamageQualifierEffect))
				attrList.Sort(this.SortDamageQualifiers);

			if (!convertStrings)
				ConvertBareAttributeListToString(attrList, results);
			else
				ConvertAttributeListToString(itm, record, attrList, recordId, results, skillLevelIndex);

			if (lineSlots is not null)
			{
				var slot = this.GameSlotOf(itm, attrList);
				while (lineSlots.Count < results.Count)
					lineSlots.Add(slot);
			}

			// The group's possible in-game values go on its last line.
			if (lineRanges is not null)
			{
				var rangeText = valueRanges is null ? null : FormatValueRanges(attrList, valueRanges);
				while (lineRanges.Count < results.Count)
					lineRanges.Add(lineRanges.Count == results.Count - 1 ? rangeText : null);
			}
		}

		if (USettings.ItemDebugLevel > 0)
			Log.LogDebug("Exiting Item.GetAttributesFromRecord()");
	}

	/// <summary>
	/// Gets where the game lists an attribute group, from its first variable.
	/// </summary>
	private GameAttributeSlot GameSlotOf(Item itm, List<Variable?> attributeList)
	{
		if (attributeList.FirstOrDefault(v => v is not null) is not { } first)
			return GameAttributeSlot.Other;

		bool isGlobal = ItemAttributeProvider.AttributeGroupHas(new Collection<Variable?>(attributeList), "Global");
		return GameAttributeOrder.SlotOf(first.Name, ItemAttributeProvider.GetAttributeData(first.Name), isGlobal, physicalIsBase: itm.IsWeapon || itm.IsShield);
	}

	/// <summary>
	/// Formats the possible in-game values of the randomized variables of an attribute group,
	/// e.g. "(90 - 126)", or "(9 - 12 / 26 - 36)" for a damage min/max pair.
	/// </summary>
	/// <returns><see langword="null"/> when no variable of the group is randomized.</returns>
	private string? FormatValueRanges(List<Variable?> attributeList, IReadOnlyDictionary<string, ValueRange> valueRanges)
	{
		// Same multiplier as GetAmountSingle/GetAmountRange: damage over time is shown as value x duration.
		float durationFactor = 1f;
		var first = attributeList.FirstOrDefault(v => v is not null);
		var effect = first is null ? null : ItemAttributeProvider.GetAttributeData(first.Name)?.Effect;
		var durationMin = attributeList.FirstOrDefault(v => v is not null
			&& string.Equals(ItemAttributeProvider.GetAttributeData(v.Name)?.Variable, "DURATIONMIN", StringComparison.OrdinalIgnoreCase));
		if (durationMin is not null && effect is not null && IsDurationReliantValue(effect))
			durationFactor = durationMin.GetSingle(durationMin.NumberOfValues - 1);

		// Min before max, like the "min ~ max" damage text (groups may list the max variable first).
		var parts = new List<(int Order, string Text)>();
		foreach (var variable in attributeList)
		{
			if (variable is null || !valueRanges.TryGetValue(variable.Name, out var range) || range.Min == range.Max)
				continue;

			var kind = ItemAttributeProvider.GetAttributeData(variable.Name)?.Variable;
			int order = string.Equals(kind, "MIN", StringComparison.OrdinalIgnoreCase) ? 0
				: string.Equals(kind, "MAX", StringComparison.OrdinalIgnoreCase) ? 1
				: 2;

			float factor = variable.DataType == VariableDataType.Float ? durationFactor : 1f;
			parts.Add((order, string.Format(CultureInfo.CurrentCulture, "{0:0.#} - {1:0.#}", range.Min * factor, range.Max * factor)));
		}

		if (parts.Count == 0)
			return null;

		// A min/max pair reads "(minLow - minHigh ~ maxLow - maxHigh)"; other groups separate values with " / ".
		var ordered = parts.OrderBy(p => p.Order).ToList();
		var separator = ordered.Count == 2 && ordered[0].Order == 0 && ordered[1].Order == 1 ? " ~ " : " / ";
		return $"({string.Join(separator, ordered.Select(p => p.Text))})";
	}

	/// <summary>
	/// Converts the item's attribute list to a string
	/// </summary>
	/// <param name="record">DBRecord for the item</param>
	/// <param name="attributeList">List of the attribute variables of the group</param>
	/// <param name="recordId">string containing the record id</param>
	/// <param name="results">List containing the results</param>
	/// <param name="skillLevelIndex">value index of a skill record shown at a known level (granted skills); null to deduce it</param>
	private void ConvertAttributeListToString(Item itm, DBRecordCollection record, List<Variable?> attributeList, RecordId recordId, List<string> results, int? skillLevelIndex = null)
	{
		if (USettings.ItemDebugLevel > 0)
		{
			Log.LogDebug("Item.ConvertAttrListToString ({Record}, {AttributeList}, {RecordId}, {Results})"
				, record, attributeList, recordId, results
			);
		}

		// see what kind of effects are in this list
		if (attributeList[0] is not { } variable)
			return;

		ItemAttributesData? data = ItemAttributeProvider.GetAttributeData(variable.Name);
		if (data is null)
		{
			// unknown attribute
			if (USettings.ItemDebugLevel > 0)
				Log.LogDebug("Error - Unknown Attribute.");

			data = new ItemAttributesData(ItemAttributesEffectType.Other, variable.Name, variable.Name, string.Empty, 0);
		}

		if (USettings.ItemDebugLevel > 0)
			Log.LogDebug("Exiting Item.ConvertAttrListToString ()");

		ConvertOffenseAttributesToString(itm, record, attributeList, data, recordId, results, skillLevelIndex);
		return;
	}

	/// <summary>
	/// Converts the item's offensive attributes to a string
	/// </summary>
	/// <param name="record">DBRecord of the database record</param>
	/// <param name="attributeList">List of the attribute variables of the group</param>
	/// <param name="data">ItemAttributesData for the item</param>
	/// <param name="recordId">string containing the record id</param>
	/// <param name="results">List containing the results</param>
	private void ConvertOffenseAttributesToString(Item itm, DBRecordCollection record, List<Variable?> attributeList, ItemAttributesData data, RecordId recordId, List<string> results, int? skillLevelIndex = null)
	{
		if (USettings.ItemDebugLevel > 0)
		{
			Log.LogDebug("Item.ConvertOffenseAttrToString({Record}, {AttributeList}, {Data}, {RecordId}, {Results})"
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

		if (skillLevelIndex is int levelIndex)
			variableNumber = levelIndex;

		// See what variables we have
		ItemAttributesData? minData = null;
		ItemAttributesData? maxData = null;
		ItemAttributesData? minDurData = null;
		ItemAttributesData? maxDurData = null;
		ItemAttributesData? chanceData = null;
		ItemAttributesData? modifierData = null;
		ItemAttributesData? durationModifierData = null;
		ItemAttributesData? modifierChanceData = null;
		ItemAttributesData? damageRatioData = null;  // Added by VillageIdiot

		Variable? minVar = null;
		Variable? maxVar = null;
		Variable? minDurVar = null;
		Variable? maxDurVar = null;
		Variable? chanceVar = null;
		Variable? modifierVar = null;
		Variable? durationModifierVar = null;
		Variable? modifierChanceVar = null;
		Variable? damageRatioVar = null;  // Added by VillageIdiot

		bool isGlobal = ItemAttributeProvider.AttributeGroupHas(new Collection<Variable?>(attributeList), "Global");
		string globalIndent = new string(' ', 4);

		foreach (Variable? variable in attributeList)
		{
			if (variable is null)
				continue;

			ItemAttributesData? attributeData = ItemAttributeProvider.GetAttributeData(variable.Name);
			if (attributeData is null)
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
		string? labelTag = null;
		TQColor? labelColor = null;
		string? label = GetLabelAndColorFromTag(itm, data, recordId, ref labelTag, ref labelColor);

		if (USettings.ItemDebugLevel > 1)
		{
			Log.LogDebug("Full attribute = {FullAttribute}", data.FullAttribute);
			Log.LogDebug("Item.label = {Label}", label);
		}

		if (label is not null)
			label = ItemAttributeProvider.ConvertFormat(label);

		// Figure out the Amount string
		string? amount = null;
		if (minVar is not null
			&& maxVar is not null
			&& minVar.GetSingle(Math.Min(minVar.NumberOfValues - 1, variableNumber)) != maxVar.GetSingle(Math.Min(maxVar.NumberOfValues - 1, variableNumber))
		)
		{
			if (minDurVar is not null)
				amount = GetAmountRange(itm, data, variableNumber, minVar, maxVar, ref label, labelColor, minDurVar);
			else
				amount = GetAmountRange(itm, data, variableNumber, minVar, maxVar, ref label, labelColor);
		}
		else
		{
			if (minDurVar is not null)
				amount = GetAmountSingle(itm, data, variableNumber, minVar, maxVar, ref label, labelColor, minDurVar);
			else
				amount = GetAmountSingle(itm, data, variableNumber, minVar, maxVar, ref label, labelColor);
		}

		// Figure out the duration string
		string? duration = null;
		//If we have both minDurData and maxDurData we also need to check if the actual Values of minDurVar and maxDurVar are actually different
		float minDurVarValue = -1;
		float maxDurVarValue = -1;

		if (minDurVar is not null)
			minDurVarValue = (float)minDurVar[minDurVar.NumberOfValues - 1];

		if (maxDurVar is not null)
			maxDurVarValue = (float)maxDurVar[maxDurVar.NumberOfValues - 1];
		bool fixedDuration = !IsDurationReliantValue(data.Effect);
		if (minDurVar is not null && maxDurVar is not null && minDurVarValue != maxDurVarValue)
			duration = GetDurationRange(variableNumber, minDurVar, maxDurVar, fixedDuration);
		else
			duration = GetDurationSingle(variableNumber, minDurVar, maxDurVar, fixedDuration);

		// Figure out the Damage Ratio string
		string? damageRatio = null;
		if (damageRatioData is not null && damageRatioVar is not null)
			damageRatio = GetDamageRatio(variableNumber, damageRatioData, damageRatioVar);

		// Figure out the chance string
		string? chance = null;
		if (chanceVar is not null)
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
		{
			// A label starting with "%" ("% Slowed", "% Energy Drain"...) completes the amount: the game shows "30% Slowed".
			if (!string.IsNullOrEmpty(amount) && label.RemoveLeadingColorTag().StartsWith('%'))
				strarray[numberOfStrings - 1] = string.Concat(amount, label);
			else
				strarray[numberOfStrings++] = label;
		}

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
				&& (string.IsNullOrEmpty(chance) || data.Effect.Equals("defensiveBlock", StringComparison.Ordinal))
				&& recordId == itm.BaseItemId
				&& string.IsNullOrEmpty(duration)
				&& !string.IsNullOrEmpty(amount)
			)
			{
				if (itm.IsWeapon)
				{
					if (data.Effect.Equals("offensivePierceRatio", StringComparison.Ordinal)
						|| data.Effect.Equals("offensivePhysical", StringComparison.Ordinal)
						|| data.Effect.Equals("offensiveBaseFire", StringComparison.Ordinal)
						|| data.Effect.Equals("offensiveBaseCold", StringComparison.Ordinal)
						|| data.Effect.Equals("offensiveBaseLightning", StringComparison.Ordinal)
						|| data.Effect.Equals("offensiveBaseLife", StringComparison.Ordinal)
					)
					{
						// mundane effect
						fontColor = ItemStyle.Mundane.TQColor();
					}
				}

				if (itm.IsShield)
				{
					if (data.Effect.Equals("defensiveBlock", StringComparison.Ordinal)
						|| data.Effect.Equals("blockRecoveryTime", StringComparison.Ordinal)
						|| data.Effect.Equals("offensivePhysical", StringComparison.Ordinal)
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
			itm.CurrentFriendlyNameResult?.TmpAttrib.Add(amountOrDurationText);
		}
		else
		{
			// null these out to indicate they did not get used
			amount = null;
			duration = null;
			chance = null;
		}

		// now see if we have a modifier
		string? modifier = null;
		if (modifierData is not null && modifierVar is not null)
			modifier = GetModifier(data, variableNumber, modifierData, modifierVar);

		string? durationModifier = null;
		if (durationModifierVar is not null)
			durationModifier = GetDurationModifier(variableNumber, durationModifierVar);

		string? modifierChance = null;
		if (modifierChanceVar is not null)
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
			itm.CurrentFriendlyNameResult?.TmpAttrib.Add(modifierText);
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
		foreach (Variable? variable in attributeList)
		{
			if (variable is null)
				continue;

			ItemAttributesData? attributeData = ItemAttributeProvider.GetAttributeData(variable.Name);

			if (attributeData is null)
				// unknown attribute
				attributeData = new ItemAttributesData(ItemAttributesEffectType.Other, variable.Name, variable.Name, string.Empty, 0);

			string normalizedAttributeVariable = attributeData.Variable.ToUpperInvariant();
			if (
				!(
					amount is not null
					&& (normalizedAttributeVariable == "MIN"
						|| normalizedAttributeVariable == "MAX"
						|| normalizedAttributeVariable == "DRAINMIN"
						|| normalizedAttributeVariable == "DRAINMAX"
					)
				)
				&& !(duration is not null && (normalizedAttributeVariable == "DURATIONMIN" || normalizedAttributeVariable == "DURATIONMAX"))
				&& !(chance is not null && normalizedAttributeVariable == "CHANCE")
				&& !(modifier is not null && normalizedAttributeVariable == "MODIFIER")
				&& !(durationModifier is not null && normalizedAttributeVariable == "DURATIONMODIFIER")
				&& !(modifierChance is not null && normalizedAttributeVariable == "MODIFIERCHANCE")
				&& !(damageRatio is not null && normalizedAttributeVariable == "DAMAGERATIO")
				&& normalizedAttributeVariable != "GLOBAL"
				// XOR is a flag, not an effect: in a global group it makes "Chance for one of the following:", elsewhere
				// (e.g. the durations of Rune Storm) it has nothing to show.
				&& normalizedAttributeVariable != "XOR"
			)
			{
				string? line = null;
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
					// With in-game values the roll simulator already applies the scale, and only to the item's
					// own stores, as the game does: socketed relics/charms and their bonuses are not scaled.
					if (!this.USettings.ShowInGameValues)
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
						itm.CurrentFriendlyNameResult?.TmpAttrib.Add(title);
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
							Log.LogDebug("Item.formatspec (Damage type) = {FormatSpec}", formatSpec);

						formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
					}

					color = ItemStyle.Mundane.TQColor();
					line = Format(formatSpec, damageType);
				}

				// We have no line so just show the raw attribute
				if (line is null)
					line = GetRawAttribute(data, variableNumber, variable, ref color);

				// Start finalizing the line of text
				string? itemSkillAutoController = null;
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
					itm.CurrentFriendlyNameResult?.TmpAttrib.Add(line);
				}

				// Added by VillageIdiot
				// itm a special case for pet bonuses
				if (normalizedFullAttribute == "PETBONUSNAME")
				{
					string petBonusID = record.GetString("petBonusName", 0);
					DBRecordCollection? petBonusRecord = Database.GetRecordFromFile(petBonusID);
					if (petBonusRecord is not null)
					{
						var tmp = new List<string>();
						GetAttributesFromRecord(itm, petBonusRecord, true, petBonusID, tmp);
						results.AddRange(tmp.Select(s => s.InsertAfterColorPrefix(globalIndent)));
						results.Add(string.Empty);
					}
				}

				// Added by VillageIdiot
				// Another special case for skill description and effects of activated skills
				// A display that shows the granted skills apart only names them here.
				bool grantedSkillApart = normalizedFullAttribute == "ITEMSKILLNAME" && (itm.CurrentFriendlyNameResult?.GrantedSkillsApart ?? false);
				if (!grantedSkillApart && (normalizedFullAttribute == "ITEMSKILLNAME" || ((itm.IsScroll || itm.IsPotion) && normalizedFullAttribute == "SKILLNAME")))
					GetSkillDescriptionAndEffects(itm, record, results, variable, line);
			}
		}

		if (USettings.ItemDebugLevel > 0)
			Log.LogDebug("Exiting Item.ConvertOffenseAttrToString()");
	}

	/// <summary>
	/// Gets the item label from the tag
	/// </summary>
	/// <param name="data">ItemAttributesData structure for the attribute</param>
	/// <param name="recordId">string containing the database record id</param>
	/// <param name="labelTag">the label tag</param>
	/// <param name="labelColor">the label color which gets modified here</param>
	/// <returns>string containing the label.</returns>
	private string? GetLabelAndColorFromTag(Item itm, ItemAttributesData data, RecordId recordId, ref string? labelTag, ref TQColor? labelColor)
	{
		labelTag = ItemAttributeProvider.GetAttributeTextTag(data);
		string label;

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
				// regular armor attribute is not magical; the Classic order puts a blank line after the base stats
				labelColor = ItemStyle.Mundane.TQColor();
			}
		}

		if (!TranslationService.TryTranslateXTag(labelTag, out label))
		{
			label = string.Concat("?", labelTag, "?");
			labelColor = ItemStyle.Legendary.TQColor();
		}

		return label;
	}

	/// <summary>
	/// For displaying raw attribute data
	/// </summary>
	/// <param name="attributeList">List of the attribute variables of the group</param>
	/// <param name="results">List containing the attribute strings.</param>
	private void ConvertBareAttributeListToString(List<Variable?> attributeList, List<string> results)
	{
		foreach (Variable? variable in attributeList)
		{
			if (variable is not null)
				results.Add(variable.ToString());
		}
	}
}
