//-----------------------------------------------------------------------
// <copyright file="Item.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Data;

public partial class ItemProvider
{

	public SortedList<string, Variable> GetRequirementVariables(Item itm)
	{
		var RequirementVariables = new SortedList<string, Variable>();
		if (itm.baseItemInfo != null)
		{
			GetRequirementsFromRecord(RequirementVariables, Database.GetRecordFromFile(itm.BaseItemId));
			GetDynamicRequirementsFromRecord(itm, RequirementVariables, itm.baseItemInfo);
		}

		if (itm.prefixInfo != null)
			GetRequirementsFromRecord(RequirementVariables, Database.GetRecordFromFile(itm.prefixID));

		if (itm.suffixInfo != null)
			GetRequirementsFromRecord(RequirementVariables, Database.GetRecordFromFile(itm.suffixID));

		if (itm.RelicInfo != null)
			GetRequirementsFromRecord(RequirementVariables, Database.GetRecordFromFile(itm.relicID));

		if (itm.Relic2Info != null)
			GetRequirementsFromRecord(RequirementVariables, Database.GetRecordFromFile(itm.relic2ID));

		// Add Artifact level requirement to formula
		if (itm.IsFormulae && itm.baseItemInfo != null)
		{
			string artifactName = itm.baseItemInfo.GetString("artifactName");
			GetRequirementsFromRecord(RequirementVariables, Database.GetRecordFromFile(artifactName));
		}

		return RequirementVariables;
	}

	/// <summary>
	/// Gets the character stat bonuses for an item
	/// </summary>
	/// <param name="item">Item that needs stat bonuses looked up</param>
	/// <returns>Sorted List containing all of the stat bonuses</returns>
	public SortedList<string, int> GetStatBonuses(Item item)
	{
		var statBonuses = new SortedList<string, int>();

		if (item.baseItemInfo != null)
			GetStatBonusesFromRecord(statBonuses, Database.GetRecordFromFile(item.BaseItemId));

		if (item.prefixInfo != null)
			GetStatBonusesFromRecord(statBonuses, Database.GetRecordFromFile(item.prefixID));

		if (item.suffixInfo != null)
			GetStatBonusesFromRecord(statBonuses, Database.GetRecordFromFile(item.suffixID));

		if (item.RelicInfo != null)
			GetStatBonusesFromRecord(statBonuses, Database.GetRecordFromFile(item.relicID), item.RelicInfo.CompletedRelicLevel);

		if (item.Relic2Info != null)
			GetStatBonusesFromRecord(statBonuses, Database.GetRecordFromFile(item.relic2ID), item.Relic2Info.CompletedRelicLevel);

		return statBonuses;
	}

	/// <summary>
	/// Gets the itemID's of all the items in the set.
	/// </summary>
	/// <returns>Returns <see cref="SetItemInfo"/> containing the remaining set items or null if the item is not part of a set.</returns>
	public SetItemInfo GetSetItems(Item itm)
	{
		if (itm.baseItemInfo == null)
			return null;

		string itemSetName = itm.baseItemInfo.GetString("itemSetName");
		if (string.IsNullOrWhiteSpace(itemSetName))
			return null;

		// Get the set record
		DBRecordCollection setRecords = Database.GetRecordFromFile(itemSetName);
		if (setRecords == null)
			return null;

		string[] setMembers = setRecords.GetAllStrings("setMembers");
		if (setMembers == null || setMembers.Length == 0)
			return null;

		var setName = setRecords.GetString("setName", 0);

		var dico = new Dictionary<string, Info>();

		foreach (var item in setMembers)
			dico.Add(item, Database.GetInfo(item));

		return new SetItemInfo(itemSetName, setName, dico, setRecords);
	}

	/// <summary>
	/// Pulls data out of the TQ item database for this item.
	/// </summary>
	public void GetDBData(Item itm)
	{
		if (USettings.ItemDebugLevel > 0)
			Log.LogDebug("Item.GetDBData ()   baseItemID = {0}", itm.BaseItemId);

		itm.BaseItemId = CheckExtension(itm.BaseItemId);
		itm.baseItemInfo = Database.GetInfo(itm.BaseItemId);

		itm.prefixID = CheckExtension(itm.prefixID);
		itm.suffixID = CheckExtension(itm.suffixID);

		if (USettings.ItemDebugLevel > 1)
		{
			Log.LogDebug("prefixID = {0}", itm.prefixID);
			Log.LogDebug("suffixID = {0}", itm.suffixID);
		}

		itm.prefixInfo = Database.GetInfo(itm.prefixID);
		itm.suffixInfo = Database.GetInfo(itm.suffixID);
		itm.relicID = CheckExtension(itm.relicID);
		itm.RelicBonusId = CheckExtension(itm.RelicBonusId);

		if (USettings.ItemDebugLevel > 1)
		{
			Log.LogDebug("relicID = {0}", itm.relicID);
			Log.LogDebug("relicBonusID = {0}", itm.RelicBonusId);
		}

		itm.RelicInfo = Database.GetInfo(itm.relicID);
		itm.RelicBonusInfo = Database.GetInfo(itm.RelicBonusId);

		itm.Relic2Info = Database.GetInfo(itm.relic2ID);
		itm.RelicBonus2Info = Database.GetInfo(itm.RelicBonus2Id);

		if (USettings.ItemDebugLevel > 1)
			Log.LogDebug("'{0}' baseItemInfo is {1} null"
				, GetFriendlyNames(itm).FullNameBagTooltip
				, (itm.baseItemInfo == null) ? string.Empty : "NOT"
			);

		// Extract image raw data
		if (itm.baseItemInfo != null)
		{
			if (itm.IsRelicOrCharm && !itm.IsRelicComplete)
			{
				itm.TexImageResourceId = itm.baseItemInfo.ShardBitmap;
				itm.TexImage = Database.LoadResource(itm.TexImageResourceId);
				if (USettings.ItemDebugLevel > 1)
					Log.LogDebug("Loaded shardbitmap ({0})", itm.baseItemInfo.ShardBitmap);
			}
			else
			{
				itm.TexImageResourceId = itm.baseItemInfo.Bitmap;
				itm.TexImage = Database.LoadResource(itm.TexImageResourceId);
				if (USettings.ItemDebugLevel > 1)
					Log.LogDebug("Loaded regular bitmap ({0})", itm.baseItemInfo.Bitmap);
			}
		}
		else
		{
			// Added by VillageIdiot
			// Try showing something so unknown items are not invisible.
			itm.TexImageResourceId = "DefaultBitmap";
			itm.TexImage = Database.LoadResource(itm.TexImageResourceId);
			if (USettings.ItemDebugLevel > 1)
				Log.LogDebug("Try loading (DefaultBitmap)");
		}

		if (USettings.ItemDebugLevel > 0)
			Log.LogDebug("Exiting Item.GetDBData ()");
	}
	/// <summary>
	/// Gets the level of a triggered skill
	/// </summary>
	/// <param name="record">DBRecord for the triggered skill</param>
	/// <param name="recordId">string containing the record id</param>
	/// <param name="varNum">variable number which we are looking up since there can be multiple values</param>
	/// <returns>int containing the skill level</returns>
	public int GetTriggeredSkillLevel(Item itm, DBRecordCollection record, RecordId recordId, int varNum)
	{
		DBRecordCollection baseItem = Database.GetRecordFromFile(itm.baseItemInfo.ItemId);

		// Check to see if it's a Buff Skill
		if (baseItem.GetString("itemSkillAutoController", 0) != null)
		{
			int level = baseItem.GetInt32("itemSkillLevel", 0);
			string itemSkillName = baseItem.GetString("itemSkillName", 0);

			if (record.GetString("Class", 0).StartsWith("SKILLBUFF", noCase))
			{
				itemSkillName = itm.baseItemInfo.GetString("itemSkillName");
				DBRecordCollection skill = Database.GetRecordFromFile(itemSkillName);

				if (skill != null)
				{
					var buffSkillName = skill.GetString("buffSkillName", 0);

					if (buffSkillName == recordId)
						// Use the level from the Base Item.
						varNum = Math.Max(level, 1) - 1;
				}
			}
			else if (itemSkillName == recordId)
				varNum = Math.Max(level, 1) - 1;
		}

		return varNum;
	}

	/// <summary>
	/// Pet skills can also have multiple values so we attempt to decode it here
	/// unfortunately the level is not in the skill record so we need to look up
	/// the level from the pet record.
	/// </summary>
	/// <param name="record">DBRecord of the skill</param>
	/// <param name="recordId">string of the record id</param>
	/// <param name="varNum">Which variable we are using since there can be multiple values.</param>
	/// <returns>int containing the pet skill level</returns>
	public int GetPetSkillLevel(Item itm, DBRecordCollection record, RecordId recordId, int varNum)
	{
		// Check to see if itm really is a skill
		if (record.GetString("Class", 0).StartsWith("SKILL_ATTACK", noCase))
		{
			// Check to see if itm item creates a pet
			var skillName = itm.baseItemInfo.GetString("skillName");
			var petSkill = Database.GetRecordFromFile(skillName);

			string petID = petSkill.GetString("spawnObjects", 0);
			if (!string.IsNullOrWhiteSpace(petID))
			{
				DBRecordCollection petRecord = Database.GetRecordFromFile(petID);
				int foundSkillOffset = 0;
				for (int skillOffset = 0; skillOffset < 17; skillOffset++)
				{
					// There are upto 17 skills
					// Find the skill in the skill tree so that we can get the level
					var skillNameOffset = petRecord.GetString(string.Concat("skillName", skillOffset), 0);
					if (skillNameOffset == recordId)
						break;

					foundSkillOffset++;
				}

				int level = petRecord.GetInt32(string.Concat("skillLevel", foundSkillOffset), 0);
				varNum = Math.Max(level, 1) - 1;
			}
		}

		return varNum;
	}

	/// <summary>
	/// Extract numerical requirements
	/// </summary>
	/// <param name="item"></param>
	/// <returns></returns>
	public RequirementInfo GetRequirementInfo(Item item)
	{
		SortedList<string, Variable> requirementVariables = GetRequirementVariables(item);
		return GetRequirementInfo(item, requirementVariables);
	}

	/// <summary>
	/// Extract numerical requirements
	/// </summary>
	/// <param name="item"></param>
	/// <param name="requirementVariables"></param>
	/// <returns></returns>
	public RequirementInfo GetRequirementInfo(Item item, SortedList<string, Variable> requirementVariables)
	{
		var info = new RequirementInfo();
		info.Item = item;

		if (requirementVariables.TryGetValue("LevelRequirement", out var varLvl))
			info.Lvl = varLvl.GetInt32(0);

		if (requirementVariables.TryGetValue("Strength", out var varStr))
			info.Str = varStr.GetInt32(0);

		if (requirementVariables.TryGetValue("Dexterity", out var varDex))
			info.Dex = varDex.GetInt32(0);

		if (requirementVariables.TryGetValue("Intelligence", out var varIntel))
			info.Int = varIntel.GetInt32(0);

		return info;
	}

	/// <summary>
	/// Get the translations for this set item
	/// </summary>-
	/// <returns>string containing the set items</returns>
	public SetItemInfo GetItemSetTranslations(Item itm)
	{
		var setMembers = this.GetSetItems(itm);
		if (setMembers != null)
		{
			var name = TranslationService.TranslateXTag(setMembers.setName);
			setMembers.Translations.Add(setMembers.setName, $"{ItemStyle.Rare.TQColor().ColorTag()}{name}");

			foreach (var memb in setMembers.setMembers)
			{
				name = "?? Missing database info ??";

				if (memb.Value != null)
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
		SortedList<string, Variable> requirementVariables = GetRequirementVariables(itm);

		// Get the format string to use to list a requirement
		if (!TranslationService.TryTranslateXTag("MeetsRequirement", out var requirementFormat))
			requirementFormat = "?Required? {0}: {1:f0}";
		else
			requirementFormat = ItemAttributeProvider.ConvertFormat(requirementFormat);

		// Now combine it all with spaces between
		List<string> requirements = new List<string>();
		foreach (KeyValuePair<string, Variable> kvp in requirementVariables)
		{
			if (USettings.ItemDebugLevel > 1)
				Log.LogDebug("Retrieving requirement {0}={1} (type={2})", kvp.Key, kvp.Value, kvp.Value.GetType().ToString());

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
