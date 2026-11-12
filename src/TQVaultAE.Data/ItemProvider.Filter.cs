//-----------------------------------------------------------------------
// <copyright file="Item.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using System.Collections.Frozen;
using System.Globalization;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Data;

public partial class ItemProvider
{
	#region Tag Lists

	// Frozen sets with an ignore-case comparer: lookups need no uppercase copy of the key (the names are ASCII).
	internal static readonly FrozenSet<string> unwantedTags = new[]
	{
			"DEFENSIVEABSORPTION",
			"MAXTRANSPARENCY",
			"SCALE",
			"CASTSSHADOWS",
			"MARKETADJUSTMENTPERCENT",
			"LOOTRANDOMIZERCOST",
			"LOOTRANDOMIZERJITTER",
			"ACTORHEIGHT",
			"ACTORRADIUS",
			"SHADOWBIAS",
			"ITEMLEVEL",
			"ITEMCOST",
			"COMPLETEDRELICLEVEL",
			"CHARACTERBASEATTACKSPEED",
			"HIDESUFFIXNAME",
			"HIDEPREFIXNAME",
			"AMULET",
			"RING",
			"HELMET",
			"GREAVES",
			"ARMBAND",
			"BODYARMOR",
			"BOW",
			"SPEAR",
			"STAFF",
			"MACE",
			"SWORD",
			"RANGEDONEHAND",
			"AXE",
			"SHIELD",
			"BRACELET",
			"AMULET",
			"RING",
			"BLOCKABSORPTION",
			"ITEMCOSTSCALEPERCENT",
			"ITEMSKILLLEVEL",
			"USEDELAYTIME",
			"CAMERASHAKEAMPLITUDE",
			"SKILLMAXLEVEL",
			"FORCEHIDEICONFROMQUICKSLOT",
			"TARGETCASTER",
			// Said by the skill descriptions ("Allows and requires dual wielding.", "A Dual Wield technique...").
			"DUALWIELDONLY",
			// Skill mechanics without game text: projectile drift, spawned wall angle, projectile lifetime.
			"DRIFTMIN",
			"DRIFTMAX",
			"SPAWNOBJECTSANGLETOCASTER",
			"SPAWNOBJECTSUSEANGLETOCASTER",
			"SKILLPROJECTILEHITTIMETOLIVE",
			"SKILLPROJECTILEMISSTIMETOLIVE",
			"EXPANSIONTIME",
			"SKILLTIER",
			"CAMERASHAKEDURATIONSECS",
			"SKILLULTIMATELEVEL",
			"SKILLCONNECTIONSPACING",
			"PETBURSTSPAWN",
			"PETLIMIT",
			"ISPETDISPLAYABLE",
			"SPAWNOBJECTSTIMETOLIVE",
			"SKILLPROJECTILENUMBER",
			"SKILLMASTERYLEVELREQUIRED",
			"EXCLUDERACIALDAMAGE",
			"SKILLWEAPONTINTRED",
			"SKILLWEAPONTINTGREEN",
			"SKILLWEAPONTINTBLUE",
			"DEBUFSKILL",
			"HIDEFROMUI",
			"MELEEONLY", // skill restriction flag, shown by the granted skills panel otherwise
			"INSTANTCAST",
			"WAVEENDWIDTH",
			"WAVEDISTANCE",
			"WAVEDEPTH",
			"WAVESTARTWIDTH",
			"RAGDOLLAMPLIFICATION",
			"WAVETIME",
			"SPARKGAP",
			"SPARKCHANCE",
			"PROJECTILEUSESALLDAMAGE",
			"DROPOFFSET",
			"DROPHEIGHT",
			"NUMPROJECTILES",
			"SWORD",
			"AXE",
			"SPEAR",
			"MACE",
			"QUEST",
			"CANNOTPICKUPMULTIPLE",
			"BONUSLIFEPERCENT",
			"BONUSLIFEPOINTS",
			"BONUSMANAPERCENT",
			"BONUSMANAPOINTS",
			"DISPLAYASQUESTITEM",  // New tags from the latest expansions.
			"ACTORSCALE",
			"ACTORSCALETIME",
			"SPAWNOBJECTSDISTANCEINCREMENT", // AMS: Additional tags to ignore
			"SPAWNOBJECTSDISTANCEINNERCIRCLE",
			"SPAWNOBJECTSNUMBEROFRINGS",
			"SPAWNOBJECTSSPACINGANGLE",
			"CONTAGIONINTERVAL",
			"CONTAGIONLIMIT",
			"CONTAGIONMAXSPREAD",
			"CONTAGIONRADIUS",
			"NOHIGHLIGHTDEFAULTCOLORA", // AMS: New property on most EE items
			"FORCEIGNORERUNSPEEDCAPS", // hguy: New property on EE "Potion of Speed"
			"LOOTRANDOMIZERSCALE",
			"PROJECTILEFRAGMENTSLAUNCHNUMBERMAX",
			"PROJECTILEFRAGMENTSLAUNCHNUMBERMIN",
			"SPAWNOBJECTSRANDOMROTATION",
			"SKILLPROJECTILETARGETGROUNDONLY",
			"OFFENSIVETOTALDAMAGEGLOBAL",
			"OFFENSIVETOTALDAMAGEXOR",
			// hguy : HCDUNGEON ITEMS
			"SKILLALLOWSWARMUP",
			"ONHITACTIVATIONCHANCE",
			"DECREMENTSTATTYPE",
			"ALLSKILLENHANCEMENT",
		}.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

	internal static readonly FrozenSet<string> requirementTags = new[]
	{
			"LEVELREQUIREMENT",
			"INTELLIGENCEREQUIREMENT",
			"DEXTERITYREQUIREMENT",
			"STRENGTHREQUIREMENT",
		}.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

	/// <summary>
	/// The effects the game applies at full strength for a time (slows, reduced abilities, armor and resistances, impaired
	/// aim): their value is not multiplied by the duration, and the duration reads "for 3.0 Seconds" (the game's
	/// <c>DamageFixed...FormatTime</c> texts), where damage over time reads "over 3.0 Seconds".
	/// </summary>
	internal static readonly FrozenSet<string> durationIndependentEffects = new[]
	{
			"OFFENSIVESLOWTOTALSPEED",
			"OFFENSIVESLOWATTACKSPEED",
			"OFFENSIVESLOWRUNSPEED",
			"OFFENSIVESLOWSPELLCASTSPEED",
			"OFFENSIVESLOWOFFENSIVEABILITY",
			"OFFENSIVESLOWDEFENSIVEABILITY",
			"OFFENSIVESLOWOFFENSIVEREDUCTION",
			"OFFENSIVESLOWDEFENSIVEREDUCTION",
			"OFFENSIVETOTALDAMAGEREDUCTIONPERCENT",
			"OFFENSIVETOTALDAMAGEREDUCTIONABSOLUTE",
			"OFFENSIVETOTALRESISTANCEREDUCTIONPERCENT",
			"OFFENSIVETOTALRESISTANCEREDUCTIONABSOLUTE",
			"OFFENSIVEFUMBLE",
			"OFFENSIVEPROJECTILEFUMBLE",
			"RETALIATIONSLOWATTACKSPEED",
			"RETALIATIONSLOWRUNSPEED",
			"RETALIATIONSLOWSPELLCASTSPEED",
			"RETALIATIONSLOWOFFENSIVEABILITY",
			"RETALIATIONSLOWDEFENSIVEABILITY",
			"RETALIATIONSLOWOFFENSIVEREDUCTION",
		}.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

	#endregion
	/// <summary>
	/// Holds all of the keys which we are filtering
	/// </summary>
	/// <param name="key">key which we are checking whether or not it gets filtered.</param>
	/// <returns>true if key is present in this list</returns>
	public bool FilterKey(string key)
		=> unwantedTags.Contains(key)
			|| key.EndsWith("SOUND", noCase)
			|| key.EndsWith("MESH", noCase)
			|| key.StartsWith("BODYMASK", noCase)
			// How a skill casts another one (chance, level, on all damage types, at the target...): shown as one line.
			|| key.StartsWith("SKILLCAST", noCase);
	/// <summary>
	/// Holds all of the requirements which we are filtering
	/// </summary>
	/// <param name="key">key which we are checking whether or not it gets filtered.</param>
	/// <returns>true if key is present in this list</returns>
	public bool FilterRequirements(string key)
		=> requirementTags.Contains(key);
	/// <summary>
	/// Gets s string containing the prefix of the item class for use in the requirements equation.
	/// </summary>
	/// <param name="itemClass">string containing the item class</param>
	/// <returns>string containing the prefix of the item class for use in the requirements equation</returns>
	public string GetRequirementEquationPrefix(string itemClass)
		=> GearTypeExtension.GearTypeMap
			.Where(m => m.Value.ICLASS.Equals(itemClass, noCase))
			.Select(m => m.Value.RequirementEquationPrefix)
			.Where(m => !string.IsNullOrWhiteSpace(m))
			.FirstOrDefault() ?? "none";
	/// <summary>
	/// Gets whether or not the variable contains a value which we are filtering.
	/// </summary>
	/// <param name="variable">Variable which we are checking.</param>
	/// <param name="allowStrings">Flag indicating whether or not we are allowing strings to show up</param>
	/// <returns>true if the variable contains a value which is filtered.</returns>
	public bool FilterValue(Variable variable, bool allowStrings)
	{
		for (int i = 0; i < variable.NumberOfValues; i++)
		{
			switch (variable.DataType)
			{
				case VariableDataType.Integer:
					if (variable.GetInt32(i) != 0)
						return false;
					break;

				case VariableDataType.Float:
					if (variable.GetSingle(i) != 0.0)
						return false;
					break;

				case VariableDataType.StringVar:
					if ((
						allowStrings
						|| variable.Name.Equals("CHARACTERBASEATTACKSPEEDTAG", noCase)
						|| variable.Name.Equals("ITEMSKILLNAME", noCase) // Added by VillageIdiot for Granted skills
						|| variable.Name.Equals("SKILLNAME", noCase) // Added by VillageIdiot for scroll skills
						|| variable.Name.Equals("PETBONUSNAME", noCase) // Added by VillageIdiot for pet bonuses
						|| ItemAttributeProvider.IsReagent(variable.Name)
						) && variable.GetString(i).Length > 0
					)
					{
						return false;
					}
					break;

				case VariableDataType.Boolean:
					if (variable.GetInt32(i) != 0)
						return false;
					break;
			}
		}

		return true;
	}
	/// <summary>
	/// Checks to see if the id ends with .dbr and adds it if not.
	/// Sometimes the .dbr extension is not written into the item
	/// </summary>
	/// <param name="itemId">item id to be checked</param>
	/// <returns>string containing itemId with a .dbr extension.</returns>
	private RecordId CheckExtension(RecordId itemId)
	{
		if (RecordId.IsNullOrEmpty(itemId))
			return RecordId.Empty;

		if (itemId.Normalized.Length < 4)
			return itemId;

		if (PathIO.GetExtension(itemId.Normalized).Equals(".DBR", StringComparison.Ordinal))
			return itemId;
		else
			return string.Concat(itemId.Raw, ".dbr");
	}
	/// <summary>
	/// Gets the item requirements from the database record
	/// </summary>
	/// <param name="requirements">SortedList of requirements</param>
	/// <param name="record">database record</param>
	private void GetRequirementsFromRecord(SortedList<string, Variable> requirements, DBRecordCollection? record)
	{
		if (USettings.ItemDebugLevel > 0)
			Log.LogDebug("Item.GetDynamicRequirementsFromRecord({Requirements}, {Record})", requirements, record);

		if (record is null)
		{
			if (USettings.ItemDebugLevel > 0)
				Log.LogDebug("Error - record was null.");

			return;
		}

		if (USettings.ItemDebugLevel > 1)
			Log.LogDebug("{RecordId}", record.Id.Normalized);

		foreach (Variable variable in record)
		{
			if (USettings.ItemDebugLevel > 2)
				Log.LogDebug("{VariableName}", variable.Name);

			if (FilterValue(variable, false))
				continue;

			if (!FilterRequirements(variable.Name))
				continue;

			string value = variable.ToStringValue();
			string key = variable.Name.Replace("Requirement", string.Empty);

			// Upper-case the first char of key
			key = key.AsSpan().ToFirstCharUpperCase();

			// Level needs to be LevelRequirement bah
			if (key.Equals("Level", StringComparison.Ordinal))
				key = Variable.KEY_LEVELREQ;
			;
			// Keep Max value per Requirement (LevelRequirement, Strength, Dexterity, Intelligence)
			if (requirements.TryGetValue(key, out Variable? value1))
			{
				Variable oldVariable = (Variable)value1;

				// Changed by VillageIdiot
				// Comparison was failing when level difference was too high
				// (single digit vs multi-digit)
				if (value.Contains(',') || oldVariable.Name.Contains(','))
				{
					// Just in case there is something with multiple values
					// Keep the original code
					if (string.Compare(value, oldVariable.ToStringValue(), noCase) <= 0)
						continue;

					requirements.Remove(key);
				}
				else
				{
					if (variable.GetInt32(0) <= oldVariable.GetInt32(0))
						continue;

					requirements.Remove(key);
				}
			}

			if (USettings.ItemDebugLevel > 1)
				Log.LogDebug("Added Requirement {Key}={Value}", key, value);

			requirements.Add(key, variable);
		}

		if (USettings.ItemDebugLevel > 0)
			Log.LogDebug("Exiting Item.GetDynamicRequirementsFromRecord()");
	}
	/// <summary>
	/// Gets the dynamic requirements from a database record.
	/// </summary>
	/// <param name="requirements">SortedList of requirements</param>
	/// <param name="itemInfo">ItemInfo for the item</param>
	private void GetDynamicRequirementsFromRecord(Item itm, SortedList<string, Variable> requirements, Info itemInfo)
	{
		if (USettings.ItemDebugLevel > 0)
			Log.LogDebug("Item.GetDynamicRequirementsFromRecord({Requirements}, {ItemInfo})", requirements, itemInfo);

		DBRecordCollection? record = Database.GetRecordFromFile(itemInfo.ItemId);
		if (record is null)
			return;

		string itemLevelTag = "itemLevel";
		Variable? lvl = record[itemLevelTag];
		if (lvl is null)
			return;

		string itemLevel = lvl.ToStringValue();
		string itemCostName = itemInfo.GetString("itemCostName");

		record = Database.GetRecordFromFile(itemCostName);
		if (record is null)
		{
			record = Database.GetRecordFromFile("records/game/itemcost.dbr");
			if (record is null)
				return;
		}

		if (USettings.ItemDebugLevel > 1)
			Log.LogDebug("{RecordId}", record.Id.Normalized);

		string prefix = GetRequirementEquationPrefix(itemInfo.ItemClass);
		foreach (Variable variable in record)
		{
			if (string.Compare(variable.Name, 0, prefix, 0, prefix.Length, noCase) != 0)
				continue;

			if (USettings.ItemDebugLevel > 2)
				Log.LogDebug("{VariableName}", variable.Name);

			if (FilterValue(variable, true))
				// our equation is a string, so we want also strings
				return;

			string keyRaw = variable.Name;
			string key = variable.Name.Replace(prefix, string.Empty);
			key = key.Replace("Equation", string.Empty);
			key = key.Replace(key[0], char.ToUpperInvariant(key[0]));

			// We need to ignore the cost equations.
			// Shields have costs so they will cause an overflow.
			if (key.Equals("Cost", StringComparison.Ordinal))
				continue;

			var variableKey = key.ToLowerInvariant();
			if (variableKey == "level" || variableKey == "strength" || variableKey == "dexterity" || variableKey == "intelligence")
				variableKey += "Requirement";

			// Level needs to be LevelRequirement bah
			if (key.Equals("Level", StringComparison.Ordinal))
				key = Variable.KEY_LEVELREQ;

			// Skip over any requirements that have been set by the database record. 
			if (requirements.ContainsKey(key))
				continue;

			string valueRaw = variable.ToStringValue();
			string value = variable.ToStringValue().Replace(itemLevelTag, itemLevel);

			// Added by VillageIdiot
			// Changed to reflect Total Attribut count
			value = value.Replace("totalAttCount", Convert.ToString(itm.attributeCount, CultureInfo.InvariantCulture));

			// Changed by Bman to fix random overflow crashes
			Variable ans = new Variable(variableKey, VariableDataType.Integer, 1);

			// Changed by VillageIdiot to fix random overflow crashes.
			double tempVal = 0;
			try
			{
				tempVal = value.Eval<double>();
				tempVal = Math.Ceiling(tempVal);
			}
			catch (System.Data.EvaluateException)
			{
				var mess = $@"Item Property value computation failed!

ItemID : {itemInfo.ItemId}
ItemLevel : {lvl.ToStringValue()}
Variablekey : {key}
Variablekey Raw : {keyRaw}
VariableValue : {value} 
VariableValue Raw : {valueRaw}
""{value}"" can't be evaluated!
";
				if (USettings.ItemDebugLevel > 0)
					throw new System.Data.EvaluateException(mess);
				else Log.LogError("{Message}", mess);
			}

			int intVal = 0;
			try
			{
				intVal = Convert.ToInt32(tempVal, CultureInfo.InvariantCulture);
			}
			catch (OverflowException)
			{
				intVal = 0;
			}

			ans[0] = intVal;

			if (USettings.ItemDebugLevel > 2)
				Log.LogDebug("Added Requirement {Key}={Value}", key, ans.ToStringValue());

			requirements.Add(key, ans);
		}

		if (USettings.ItemDebugLevel > 0)
			Log.LogDebug("Exiting Item.GetDynamicRequirementsFromRecord()");
	}
}
