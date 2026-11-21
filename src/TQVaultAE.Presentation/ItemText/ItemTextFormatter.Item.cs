//-----------------------------------------------------------------------
// <copyright file="ItemTextFormatter.Item.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Globalization;
using Microsoft.Extensions.Logging;
using TQVaultAE.Application.Session;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Presentation.ItemText;

public partial class ItemTextFormatter
{
	/// <summary>
	/// Get the translations for this set item
	/// </summary>-
	/// <returns>string containing the set items</returns>
	public SetItemInfo? GetItemSetTranslations(Item itm)
	{
		using var modScope = this.ModScope(itm);
		var setMembers = this.ItemProvider.GetSetItems(itm);
		if (setMembers is not null)
		{
			var name = TranslationService.TranslateXTag(setMembers.setName);
			setMembers.Translations.Add(setMembers.setName, $"{ItemStyle.Rare.TQColor().ColorTag()}{name}");

			foreach (var memb in setMembers.setMembers)
			{
				name = "?? Missing database info ??";

				if (memb.Value is not null)
					name = TranslationService.TranslateXTag(memb.Value.DescriptionTag);

				setMembers.Translations.Add(memb.Key, $"{ItemStyle.Common.TQColor().ColorTag()}    {name}");
			}
		}
		return setMembers;
	}

	/// <summary>
	/// Gets the item's requirements
	/// </summary>
	/// <returns>A string containing the items requirements</returns>
	public (string[] Requirements, SortedList<string, Variable> RequirementVariables) GetRequirements(Item itm)
	{
		using var modScope = this.ModScope(itm);
		SortedList<string, Variable> requirementVariables = this.ItemProvider.GetRequirementVariables(itm);
		return (this.FormatRequirements(requirementVariables), requirementVariables);
	}

	/// <summary>The order of the requirement lines in the game's tool tip; other requirements follow, by name.</summary>
	private static readonly string[] RequirementOrder = [Variable.KEY_LEVELREQ, Variable.KEY_STRENGTH, Variable.KEY_DEXTERITY, Variable.KEY_INTELLIGENCE];

	/// <inheritdoc/>
	public string[] FormatRequirements(SortedList<string, Variable> variables, IReadOnlyDictionary<string, ReducedRequirement>? reduced = null)
	{
		// The game's formats: met or failed, with the reduction "[ base - x% Reduction]" when there is one.
		string meets = this.GameFormat("MeetsRequirement", "?Required? {0}: {1:f0}");
		string? meetsReduced = null, fails = null, failsReduced = null;

		List<string> lines = [];
		foreach (var (key, variable) in variables.OrderBy(kvp => Array.IndexOf(RequirementOrder, kvp.Key) is var i and >= 0 ? i : RequirementOrder.Length))
		{
			// A requirement should have one number; anything else is shown raw.
			if (variable.NumberOfValues > 1 || variable.DataType == VariableDataType.StringVar)
			{
				lines.Add(string.Concat(key, ": ", variable.ToStringValue()));
				continue;
			}

			if (!this.TranslationService.TryTranslateXTag(key, out var name))
				name = string.Concat("?", key, "?");

			if (reduced is null || !reduced.TryGetValue(key, out var need))
			{
				lines.Add(Format(meets, name, variable[0]));
				continue;
			}

			// As the game: a base met is shown alone; else the reduced value, met or not, with the reduction if any.
			if (need.Have >= need.Base)
			{
				lines.Add(Format(meets, name, need.Base));
				continue;
			}

			string format = (need.Met, need.ShowsReduction) switch
			{
				(true, false) => meets,
				(true, true) => meetsReduced ??= this.GameFormat("MeetsRequirementWithReduction", "Required {0}: {1:#0} [ {{^r}}{2:#0} {{^y}}- {3:#0}% Reduction]"),
				(false, false) => fails ??= this.GameFormat("FailsRequirement", "{{^r}}Required {0}: {1:#0}"),
				(false, true) => failsReduced ??= this.GameFormat("FailsRequirementWithReduction", "{{^r}}Required {0}: {1:#0} [ {2:#0} {{^y}}- {3:#0}% Reduction]"),
			};
			lines.Add(FormatReduced(format, name, need) ?? Format(meets, name, need.Value));
		}

		return [.. lines];
	}

	/// <summary>A requirement line: name, reduced value, base value, reduction %; null for a malformed format (logged).</summary>
	private string? FormatReduced(string format, string name, ReducedRequirement need)
	{
		try
		{
			return string.Format(CultureInfo.CurrentCulture, format, name, need.Value, need.Base, need.Percent);
		}
		catch (FormatException ex)
		{
			this.Log.LogWarning(ex, "Failed to format the requirement line {Format}", format);
			return null;
		}
	}
}
