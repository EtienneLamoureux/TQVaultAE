using Microsoft.Extensions.Logging;
using System.Globalization;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Data;

public partial class ItemProvider
{
	/// <summary>
	/// Gets the global chance string
	/// </summary>
	/// <param name="attributeList">List of the attribute variables of the group</param>
	/// <param name="varNum">offset number of the variable value that we are using</param>
	/// <param name="v">variable structure</param>
	/// <param name="font">font string</param>
	/// <returns>formatted global chance string</returns>
	private string GetGlobalChance(List<Variable?> attributeList, int varNum, Variable v, ref TQColor? font)
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
				// The chance stays a placeholder: formatting the tag into "{0:f1}% ?{0}?" lost the value.
				formatSpec = string.Concat("{0:f1}% ?", tag, "?");
				font = ItemStyle.Legendary.TQColor();
			}
			else
			{
				if (USettings.ItemDebugLevel > 2)
					Log.LogDebug("Item.formatspec (chance of one) = {FormatSpec}", formatSpec);

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
	private string? GetRacialBonus(DBRecordCollection record, Item itm, List<string> results, int varNum, bool isGlobal, string globalIndent, Variable v, ItemAttributesData d, string? line, ref TQColor? color)
	{
		// Added by VillageIdiot
		// Updated to accept multiple racial bonuses in record
		string[]? races = record.GetAllStrings("racialBonusRace");
		if (races is not null)
		{
			for (int j = 0; j < races.Length; ++j)
			{

				if (!TranslationService.TryTranslateXTag($"racialBonusRace{races[j]}", out var finalRace))
				{
					finalRace = races[j];

					if (USettings.DebugEnabled)
						Log.LogDebug("missing racialBonusRace={FinalRace}", finalRace);
				}

				// Optimized: use span-based ToFirstCharUpperCase
				string formatTag = d.FullAttribute.AsSpan().ToFirstCharUpperCase();

				if (!TranslationService.TryTranslateXTag(formatTag, out var formatSpec))
					formatSpec = string.Concat(formatTag, " {0} {1}");
				else
				{
					if (USettings.ItemDebugLevel > 2)
						Log.LogDebug("Item.formatspec (race bonus) = {FormatSpec}", formatSpec);

					formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
				}


				if (line is not null)
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
					itm.CurrentFriendlyNameResult?.TmpAttrib.Add(line);
				}

				line = Format(formatSpec, v[Math.Min(v.NumberOfValues - 1, varNum)], finalRace);
				color = ItemStyle.Epic.TQColor();
			}
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
	private string? GetFormulae(List<string> results, Variable variable, ItemAttributesData attributeData, string? line, ref TQColor? font)
	{
		// Special case for formulae reagents
		if (attributeData.FullAttribute.StartsWith("reagent", noCase))
		{
			var reagentId = variable.GetString(0);
			DBRecordCollection? reagentRecord = Database.GetRecordFromFile(reagentId);
			if (reagentRecord is not null)
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
		else if (attributeData.FullAttribute.Equals("artifactCreationCost", StringComparison.Ordinal))
		{
			if (!TranslationService.TryTranslateXTag("xtagArtifactCost", out var formatSpec))
				formatSpec = "Gold Cost: {0}";
			else
				formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);

			if (USettings.ItemDebugLevel > 2)
				Log.LogDebug("Item.formatspec (Artifact cost) = {FormatSpec}", formatSpec);

			font = ItemStyle.Rare.TQColor();
			results.Add(string.Empty);
			line = Format(formatSpec, string.Format(CultureInfo.CurrentCulture, "{0:N0}", variable[0]));
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
		string? line = null;

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
		if (label is not null && label.Contains('{'))
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
}
