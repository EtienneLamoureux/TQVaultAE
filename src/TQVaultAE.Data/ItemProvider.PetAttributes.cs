using Microsoft.Extensions.Logging;
using System.Globalization;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Data;

public partial class ItemProvider
{
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
				Log.LogDebug("Item.formatspec (pet bonus) = {FormatSpec}", formatSpec);

			formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
			color = ItemStyle.Relic.TQColor();
		}

		return formatSpec;
	}

	/// <summary>
	/// The format of a pet statistic line, the game's text as it is ("{%.0f0}  Health"), or <paramref name="fallback"/>.
	/// </summary>
	private string PetStatFormat(string tag, string fallback)
		=> TranslationService.TryTranslateXTag(tag, out var text) ? ItemAttributeProvider.ConvertFormat(text) : fallback;

	/// <summary>
	/// Used for showing the pet statistics
	/// </summary>
	/// <param name="skillRecord">DBRecord of the skill</param>
	/// <param name="results">List containing the results</param>
	/// <param name="level">The skill level: the pet record (<c>spawnObjects</c>) and its life time are given per level.</param>
	/// <param name="hiddenVariables">
	/// Variables of the pet's skills that are not shown; given for the skill window, which also leaves out the pet skills
	/// at level 0 (those that a pet upgrade gives, such as the Battle Standard's "Triumph").
	/// </param>
	/// <param name="petSkillBonus">Levels added to the pet's skills (the equipment's bonus to the summoning skill).</param>
	private void ConvertPetStats(Item itm, DBRecordCollection skillRecord, List<string> results, int level = 1
		, IReadOnlySet<string>? hiddenVariables = null, int petSkillBonus = 0)
	{
		int levelIndex(string name) => skillRecord[name] is { NumberOfValues: > 0 } v ? Math.Clamp(level - 1, 0, v.NumberOfValues - 1) : 0;

		string formatSpec, petLine;
		int summonLimit = skillRecord.GetInt32("petLimit", levelIndex("petLimit"));
		if (summonLimit > 1)
		{
			if (!TranslationService.TryTranslateXTag("SkillPetLimit", out formatSpec))
				formatSpec = "{0} Summon Limit";
			else
				formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);

			petLine = string.Format(CultureInfo.CurrentCulture, formatSpec, summonLimit.ToString(CultureInfo.CurrentCulture));
			var value = $"{ItemStyle.Mundane.TQColor().ColorTag()}{petLine}";
			results.Add(value);
			itm.CurrentFriendlyNameResult?.TmpAttrib.Add(value);
		}
		var skillRecordspawnObjects = skillRecord.GetString("spawnObjects", levelIndex("spawnObjects"));
		DBRecordCollection? petRecord = Database.GetRecordFromFile(skillRecordspawnObjects);
		if (petRecord is not null)
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
			AddTextLine(itm, results, valueStr);

			// Time to live: 0 is a permanent pet, which has no life time line.
			float timeToLive = skillRecord.GetSingle("spawnObjectsTimeToLive", levelIndex("spawnObjectsTimeToLive"));
			if (timeToLive > 0)
			{
				if (!TranslationService.TryTranslateXTag("tagSkillPetTimeToLive", out formatSpec))
					formatSpec = "Life Time {0} Seconds";
				else
					formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);

				petLine = string.Format(CultureInfo.CurrentCulture, formatSpec, timeToLive);
				valueStr = $"{ItemStyle.Mundane.TQColor().ColorTag()}{petLine}";
				results.Add(valueStr);
				itm.CurrentFriendlyNameResult?.TmpAttrib.Add(valueStr);
			}

			// Health
			value = petRecord.GetSingle("characterLife", 0);
			if (value != 0.0F)
			{
				formatSpec = this.PetStatFormat("SkillPetDescriptionHealth", "{0}  Health");

				petLine = string.Format(CultureInfo.CurrentCulture, formatSpec, value);
				valueStr = $"{ItemStyle.Mundane.TQColor().ColorTag()}{petLine}";
				results.Add(valueStr);
				itm.CurrentFriendlyNameResult?.TmpAttrib.Add(valueStr);
			}

			// Energy
			value = petRecord.GetSingle("characterMana", 0);
			if (value != 0.0F)
			{
				formatSpec = this.PetStatFormat("SkillPetDescriptionMana", "{0}  Energy");

				petLine = string.Format(CultureInfo.CurrentCulture, formatSpec, value);
				valueStr = $"{ItemStyle.Mundane.TQColor().ColorTag()}{petLine}";
				results.Add(valueStr);
				itm.CurrentFriendlyNameResult?.TmpAttrib.Add(valueStr);
			}

			// Add abilities text
			results.Add(string.Empty);
			if (!TranslationService.TryTranslateXTag("tagSkillPetAbilities", out formatSpec))
				formatSpec = "{0} Abilities:";
			else
				formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);

			petLine = string.Format(CultureInfo.CurrentCulture, formatSpec, petName);
			valueStr = $"{ItemStyle.Mundane.TQColor().ColorTag()}{petLine}";
			AddTextLine(itm, results, valueStr);

			// Show Physical Damage; the skill window leaves it out (its pets show their "Basic Attack" skill instead).
			value = petRecord.GetSingle("handHitDamageMin", 0);
			float value2 = petRecord.GetSingle("handHitDamageMax", 0);

			if ((value > 1.0F || value2 > 2.0F) && hiddenVariables is null)
			{
				if (value2 == 0.0F || value == value2)
				{
					formatSpec = this.PetStatFormat("SkillPetDescriptionDamageMinOnly", "{0}  Damage");

					petLine = string.Format(CultureInfo.CurrentCulture, formatSpec, value);
					valueStr = $"{ItemStyle.Mundane.TQColor().ColorTag()}{petLine}";
					results.Add(valueStr);
					itm.CurrentFriendlyNameResult?.TmpAttrib.Add(valueStr);
				}
				else
				{
					formatSpec = this.PetStatFormat("SkillPetDescriptionDamageMinMax", "{0} - {1}  Damage");

					petLine = string.Format(CultureInfo.CurrentCulture, formatSpec, value, value2);
					valueStr = $"{ItemStyle.Mundane.TQColor().ColorTag()}{petLine}";
					results.Add(valueStr);
					itm.CurrentFriendlyNameResult?.TmpAttrib.Add(valueStr);
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
				if (petskillLevel < 1 && hiddenVariables is not null)
					continue;

				if (petskillLevel < 1)
					petskillLevel = 1;
				else
					petskillLevel += petSkillBonus;

				skillLevels[numSkills] = petskillLevel;
				numSkills++;
			}

			for (int i = 0; i < numSkills; i++)
			{
				string recordID = skills[i];
				if (recordID is not null && !recordID.StartsWith("records", StringComparison.OrdinalIgnoreCase))
					continue;

				DBRecordCollection? skillRecord1 = Database.GetRecordFromFile(recordID);
				DBRecordCollection? record = null;
				string skillClass = skillRecord1?.GetString("Class", 0) ?? string.Empty;

				// Skip passive skills, which the skill window shows (the wolves' "Dodge Attacks").
				if (skillClass.Equals("SKILL_PASSIVE", noCase) && hiddenVariables is null)
					continue;

				string? skillNameTag = null;
				string? skillName = null;
				bool skillWindow = hiddenVariables is not null;
				string buffSkillName = skillRecord1?.GetString("buffSkillName", 0) ?? string.Empty;

				if (string.IsNullOrEmpty(buffSkillName))
				{
					record = skillRecord1;
					skillNameTag = skillRecord1?.GetString("skillDisplayName", 0);
					if (!string.IsNullOrWhiteSpace(skillNameTag))
						TranslationService.TryTranslateXTag(skillNameTag, out skillName);
				}
				else
				{
					// This is a buff skill
					DBRecordCollection? buffSkillRecord = Database.GetRecordFromFile(buffSkillName);
					if (buffSkillRecord is not null)
					{
						record = buffSkillRecord;
						recordID = buffSkillName;
						skillNameTag = buffSkillRecord.GetString("skillDisplayName", 0);
						if (!string.IsNullOrWhiteSpace(skillNameTag))
							TranslationService.TryTranslateXTag(skillNameTag, out skillName);
					}
				}

				// The skill window shows only the pet skills marked for it (isPetDisplayable, on the skill or its buff), not
				// its innate armor and resistances or its death skill.
				if (skillWindow && (string.IsNullOrWhiteSpace(skillName)
					|| (skillRecord1?.GetInt32("isPetDisplayable", 0) != 1 && record?.GetInt32("isPetDisplayable", 0) != 1)))
					continue;

				if (string.IsNullOrWhiteSpace(skillName))
					valueStr = $"{ItemStyle.Legendary.TQColor().ColorTag()}{skillNameTag}";
				else
					valueStr = $"{ItemStyle.Mundane.TQColor().ColorTag()}{skillName}";

				results.Add(valueStr);
				itm.CurrentFriendlyNameResult?.TmpAttrib.Add(valueStr);

				// "+1 to all Skills" of a pet's buff (allSkillEnhancement), which item tool tips leave out.
				var ownLines = new List<(string Variable, string Text)>();
				if (hiddenVariables is not null && record?["allSkillEnhancement"] is { NumberOfValues: > 0 } allSkills
					&& allSkills.GetInt32(Math.Clamp(skillLevels[i] - 1, 0, allSkills.NumberOfValues - 1)) is var all and > 0
					&& TranslationService.TryTranslateXTag("ItemAllSkillIncrement", out var allSkillsText))
				{
					ownLines.Add(("allSkillEnhancement", Format(ItemAttributeProvider.ConvertFormat(allSkillsText), all)));
				}

				// The pet's skill at the level the pet record gives it, in the skill window's order there.
				GetAttributesFromRecord(itm, record, true, recordID, results, skillLevelIndex: skillLevels[i] - 1, hiddenVariables: hiddenVariables
					, skillWindow: hiddenVariables is null ? null : new SkillWindowLines(ownLines));
				results.Add(string.Empty);
			}
		}
	}
}
