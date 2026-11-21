using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Globalization;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Presentation.ItemText;

public partial class ItemTextFormatter
{
	/// <summary>
	/// Adds a line that is text, not an attribute: a line of a granted skill's description, or a header that only
	/// introduces the lines below it. The tool tip shows it; the search does not list it as a filter
	/// (<see cref="Domain.Results.ToFriendlyNameResult.IsTextLine"/>).
	/// </summary>
	private static void AddTextLine(Item itm, List<string> results, string line)
	{
		results.Add(line);
		itm.CurrentFriendlyNameResult?.MarkTextLine(line);
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
		string? formatSpec = this.GameFormat("ItemAllSkillIncrement");
		color = formatSpec is null ? ItemStyle.Legendary.TQColor() : ItemStyle.Epic.TQColor();
		return Format(formatSpec ?? "?+{0} to all skills?", ShownValue(variable, variableNumber));
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

		// The game drops a mastery level whose augmentMasteryNameN is empty:
		// the bonus is never applied nor shown. 7 items do so, like the Clashing Suit (xpack3 u_n_set11).
		if (string.IsNullOrEmpty(augmentMasteryValue))
			return string.Empty;

		string? skillName = this.TranslatedSkillName(augmentMasteryValue);
		if (string.IsNullOrEmpty(skillName))
		{
			skillName = PathIO.GetFileNameWithoutExtension(augmentMasteryValue);
			font = ItemStyle.Legendary.TQColor();
		}

		string? formatSpec = this.GameFormat("ItemMasteryIncrement");
		font ??= formatSpec is null ? ItemStyle.Legendary.TQColor() : ItemStyle.Epic.TQColor();
		return Format(formatSpec ?? "?+{0} to skills in {1}?", variable[0], skillName);
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
	private string? GetAugmentSkillLevel(DBRecordCollection record, Variable variable, ItemAttributesData attributeData, string? line, ref TQColor? font)
	{
		// Optimized: use span to get single char
		string augmentSkillNumber = attributeData.FullAttribute.AsSpan().Slice(17, 1).ToString();
		string skillRecordKey = string.Concat("augmentSkillName", augmentSkillNumber);
		string skillRecordID = record.GetString(skillRecordKey, 0);

		if (!string.IsNullOrEmpty(skillRecordID))
		{
			// A buff skill is named by its buff; a pet upgrade without a name of its own by the skill it gives the pet
			// (petSkillName), itself maybe named by its buff (records\xpack\skills\dream\nightmare_petmodifier_mastermind.dbr).
			string? skillName = this.TranslatedSkillName(skillRecordID);
			if (skillName is null && Database.GetRecordFromFile(skillRecordID) is { } skillRecord
				&& skillRecord.GetString("Class", 0).Contains("PetModifier"))
				skillName = this.TranslatedSkillName(skillRecord.GetString("petSkillName", 0));

			if (string.IsNullOrEmpty(skillName))
				skillName = PathIO.GetFileNameWithoutExtension(skillRecordID);

			string? formatSpec = this.GameFormat("ItemSkillIncrement");
			font = formatSpec is null ? ItemStyle.Legendary.TQColor() : ItemStyle.Epic.TQColor();
			line = this.Format(formatSpec ?? "?+{0} to skill {1}?", variable[0], skillName);
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
	private string? GetGrantedSkill(DBRecordCollection record, Item itm, List<string> results, Variable variable, string? line, ref TQColor? font)
	{
		// Added by VillageIdiot
		// Special case for granted skills
		var grantedSkillId = variable.GetString(0);
		DBRecordCollection? skillRecord = Database.GetRecordFromFile(grantedSkillId);
		if (skillRecord is not null)
		{
			// Add a blank line and then the Grants Skill text
			results.Add(string.Empty);
			font = ItemStyle.Mundane.TQColor();

			if (!TranslationService.TryTranslateXTag("tagItemGrantSkill", out var skillTag))
				skillTag = "Grants Skill :";

			AddTextLine(itm, results, $"{font?.ColorTag()}{skillTag}");

			// A buff skill is named by its buff; an unnamed skill writes only its activation.
			string? skillName = this.DisplayedSkillRecord(grantedSkillId) is { } shown ? this.SkillDisplayName(shown, grantedSkillId) : null;

			string activationText = this.GetSkillActivationText(record.GetString("itemSkillAutoController", 0));

			if (string.IsNullOrEmpty(activationText))
				font = ItemStyle.Epic.TQColor();
			else
				font = ItemStyle.Mundane.TQColor();

			// Name and activation text, without a dangling space when one of them is empty.
			line = string.Join(' ', new[] { skillName, activationText }.Where(s => !string.IsNullOrEmpty(s)));
		}

		return line;
	}

	/// <summary>
	/// Translates when an auto controlled granted skill triggers and how likely, e.g.
	/// "(Activated upon taking damage)" then, on its own line, "4% Chance of Activating" (<c>triggerType</c> and
	/// <c>chanceToRun</c>).
	/// </summary>
	/// <remarks>
	/// The one text of the activation, shared by the Classic line, the granted skill panel, Item Properties and the
	/// comparison. A chance of 100 % (or a missing one) is not written: the skill always triggers.
	/// </remarks>
	/// <param name="autoController">The <c>itemSkillAutoController</c> record, if any.</param>
	/// <returns>The activation text, or an empty string when the skill is not auto controlled.</returns>
	private string GetSkillActivationText(string? autoController)
	{
		// Added by VillageIdiot to support skill activation text
		string? triggerType = null;
		string? activationTag = null;
		string? activationText;
		float chance = 0;
		if (!string.IsNullOrEmpty(autoController))
		{
			DBRecordCollection? autoControllerRecord = Database.GetRecordFromFile(autoController);
			if (autoControllerRecord is not null)
			{
				triggerType = autoControllerRecord.GetString("triggerType", 0);
				chance = autoControllerRecord.GetSingle("chanceToRun", 0);
			}
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
			return string.Empty;

		return chance is > 0 and < 100 ? $"{activationText}\n{this.ActivationChanceText(chance)}" : activationText;
	}

	/// <summary>
	/// The game's "4% Chance of Activating" (<c>SkillActivationChance</c>), for a skill triggered by an item's controller
	/// or by hits (<c>onHitActivationChance</c> in the skill window).
	/// </summary>
	private string ActivationChanceText(float chance)
	{
		if (!TranslationService.TryTranslateXTag("SkillActivationChance", out var label))
			label = "Chance of Activating";

		return $"{chance.ToString("0.#", CultureInfo.CurrentCulture)}% {label}";
	}

	/// <summary>
	/// The game's text for a skill cost ("{%.0f0 %s1}"): energy and life costs are written rounded to a whole number,
	/// half away from zero (0.5 energy per second reads "1").
	/// </summary>
	private const string SkillCostFormatTag = "SkillCostFormat";

	/// <summary>
	/// A skill cost line as the game writes it, e.g. "1 Active Energy Cost per Second" for 0.5: the value rounded to a
	/// whole number with <c>SkillCostFormat</c>, then the label.
	/// </summary>
	/// <returns><see langword="null"/> when the game text has no cost format.</returns>
	private string? FormatSkillCost(float value, string label)
		=> this.GameFormat(SkillCostFormatTag) is { } format ? Format(format, value, label) : null;

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
	private string? GetSkillEffect(ItemAttributesData baseAttributeData, int variableNumber, Variable variable, ItemAttributesData currentAttributeData, string? line, ref TQColor? color)
	{
		string label = this.TranslateLabel(this.AttributeLabelTag(baseAttributeData, ref color), ref color);

		if (USettings.ItemDebugLevel > 2)
			Log.LogDebug("Item.label (scroll) = {Label}", label);

		label = ItemAttributeProvider.ConvertFormat(label);

		// Find the extra format tag for those that take 2 parameters.
		string? formatSpecTag = null;
		string? formatSpec = null;
		if (currentAttributeData.FullAttribute.EndsWith("Cost", noCase))
			formatSpecTag = SkillCostFormatTag;
		else if (currentAttributeData.FullAttribute.EndsWith("Level", noCase))
			formatSpecTag = "SkillIntFormat";
		else if (currentAttributeData.FullAttribute.EndsWith("Duration", noCase))
			formatSpecTag = "SkillSecondFormat";
		else if (currentAttributeData.FullAttribute.EndsWith("Radius", noCase))
			formatSpecTag = "SkillDistanceFormat";

		if (string.IsNullOrEmpty(formatSpecTag))
		{
			color = ItemStyle.Epic.TQColor();
			line = Format(label, ShownValue(variable, variableNumber));
		}
		else
		{
			formatSpec = this.GameFormat(formatSpecTag);
			color = formatSpec is null ? ItemStyle.Legendary.TQColor() : ItemStyle.Epic.TQColor();
			line = Format(formatSpec ?? "?{0} {1}?", ShownValue(variable, variableNumber), label);
		}

		return line;
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
			DBRecordCollection? skillRecord = Database.GetRecordFromFile(SkillDescriptionAndEffectsVar);

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
			if (skillRecord is not null)
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
									AddTextLine(itm, results, $"{ItemStyle.Mundane.TQColor().ColorTag()}    {skillDescriptionFromList}");

								// Show granted skill level
								if (this.USettings.ShowSkillLevel)
								{
									string formatSpec = this.GameFormat("MenuLevel", "Level:   {0}");
									int skillLevel = record.GetInt32("itemSkillLevel", 0);
									if (skillLevel > 0)
									{
										line = Format(formatSpec, skillLevel);
										var value = $"{ItemStyle.Mundane.TQColor().ColorTag()}{line}";
										results.Add(value);
									}
								}
							}
						}
					}
					else
					{
						// itm skill is a buff
						DBRecordCollection? buffSkillRecord = Database.GetRecordFromFile(buffSkillNameId);
						if (buffSkillRecord is not null)
						{
							descriptionTag = buffSkillRecord.GetString("skillBaseDescription", 0);
							if (!string.IsNullOrEmpty(descriptionTag))
							{
								skillDescription = TranslationService.TranslateXTag(descriptionTag);
								skillDescriptionList = StringHelper.WrapWords(skillDescription, lineLength);
								foreach (string skillDescriptionFromList in skillDescriptionList)
									AddTextLine(itm, results, $"{ItemStyle.Mundane.TQColor().ColorTag()}    {skillDescriptionFromList}");

								// Show granted skill level
								if (this.USettings.ShowSkillLevel)
								{
									string formatSpec = this.GameFormat("MenuLevel", "Level:   {0}");
									int skillLevel = record.GetInt32("itemSkillLevel", 0);
									if (skillLevel > 0)
									{
										line = Format(formatSpec, skillLevel);
										var value = $"{ItemStyle.Mundane.TQColor().ColorTag()}    {line}";
										results.Add(value);
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
				if (skillRecord is not null && !itm.IsScroll && !itm.IsPotion)
					results.Add(string.Empty);

				// Added by VillageIdiot
				// Add the skill effects
				if (skillRecord is not null)
				{
					// Equipment: as the game's item tool tip, the skill window's lines and order (D24, D25).
					if (!itm.IsScroll && !itm.IsPotion
						&& this.GetItemGrantedSkill(itm, SkillDescriptionAndEffectsVar.ToRecordId(), record.GetInt32("itemSkillLevel", 0), autoController) is { } granted)
						results.AddRange(granted.Attributes);
					// itm is a summon
					else if (skillRecord.GetString("Class", 0).Equals("SKILL_SPAWNPET", noCase))
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
}
