//-----------------------------------------------------------------------
// <copyright file="Item.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
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

		// Get the format string to use to list a requirement
		string requirementFormat = this.GameFormat("MeetsRequirement", "?Required? {0}: {1:f0}");

		// Now combine it all with spaces between
		List<string> requirements = [];
		foreach (KeyValuePair<string, Variable> kvp in requirementVariables)
		{
			if (USettings.ItemDebugLevel > 1)
				Log.LogDebug("Retrieving requirement {Key}={Value} (type={Type})", kvp.Key, kvp.Value, kvp.Value.GetType().ToString());

			Variable variable = kvp.Value;

			// Format the requirement
			string requirementsText;
			if (variable.NumberOfValues > 1 || variable.DataType == VariableDataType.StringVar)
			{
				// reqs should only have 1 entry and should be a number type.  We must punt on itm one
				requirementsText = string.Concat(kvp.Key, ": ", variable.ToStringValue());
			}
			else
			{
				// get the name of itm requirement
				if (!TranslationService.TryTranslateXTag(kvp.Key, out var reqName))
					reqName = string.Concat("?", kvp.Key, "?");


				// Now apply the format string
				requirementsText = Format(requirementFormat, reqName, variable[0]);
			}

			// Changed by VillageIdiot - Change requirement text to Grey
			requirements.Add(requirementsText);
		}

		return (requirements.ToArray(), requirementVariables);
	}
}
