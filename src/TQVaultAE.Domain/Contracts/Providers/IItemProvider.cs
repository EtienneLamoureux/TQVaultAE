using System.Collections.Generic;
using System.IO;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Results;

namespace TQVaultAE.Domain.Contracts.Providers
{
	public interface IItemProvider
	{
		/// <summary>
		/// Gets the artifact/charm/relic bonus loot table
		/// returns null if the item is not an artifact/charm/relic or does not contain a charm/relic
		/// </summary>
		LootTableCollection BonusTable(Item itm);
		/// <summary>
		/// Create an artifact from its formulae
		/// </summary>
		/// <returns>A new artifact</returns>
		Item CraftArtifact(Item itm);
		/// <summary>
		/// Encodes an item into the save file format
		/// </summary>
		/// <param name="writer">BinaryWriter instance</param>
		void Encode(Item itm, BinaryWriter writer);
		/// <summary>
		/// Holds all of the keys which we are filtering
		/// </summary>
		/// <param name="key">key which we are checking whether or not it gets filtered.</param>
		/// <returns>true if key is present in this list</returns>
		bool FilterKey(string key);
		/// <summary>
		/// Holds all of the requirements which we are filtering
		/// </summary>
		/// <param name="key">key which we are checking whether or not it gets filtered.</param>
		/// <returns>true if key is present in this list</returns>
		bool FilterRequirements(string key);
		/// <summary>
		/// Gets whether or not the variable contains a value which we are filtering.
		/// </summary>
		/// <param name="variable">Variable which we are checking.</param>
		/// <param name="allowStrings">Flag indicating whether or not we are allowing strings to show up</param>
		/// <returns>true if the variable contains a value which is filtered.</returns>
		bool FilterValue(Variable variable, bool allowStrings);
		/// <summary>
		/// Formats a string based on the formatspec
		/// </summary>
		/// <param name="formatSpec">format specification</param>
		/// <param name="parameter1">first parameter</param>
		/// <param name="parameter2">second parameter</param>
		/// <param name="parameter3">third parameter</param>
		/// <returns>formatted string.</returns>
		string Format(string formatSpec, object parameter1, object parameter2 = null, object parameter3 = null);
		/// <summary>
		/// Pulls data out of the TQ item database for this item.
		/// </summary>
		void GetDBData(Item itm);
		/// <summary>
		/// Gets the item name and attributes.
		/// </summary>
		/// <param name="itm"></param>
		/// <param name="scopes">Extra data scopes as a bitmask</param>
		/// <param name="filterExtra">filter extra properties</param>
		/// <returns>An object containing the item name and attributes</returns>
		ToFriendlyNameResult GetFriendlyNames(Item itm, FriendlyNamesExtraScopes? scopes = null, bool filterExtra = true);
		/// <summary>
		/// Shows the items in a set for the set items
		/// </summary>
		/// <returns>string containing the set items</returns>
		string[] GetItemSetString(Item itm);
		/// <summary>
		/// Pet skills can also have multiple values so we attempt to decode it here
		/// unfortunately the level is not in the skill record so we need to look up
		/// the level from the pet record.
		/// </summary>
		/// <param name="record">DBRecord of the skill</param>
		/// <param name="recordId">string of the record id</param>
		/// <param name="varNum">Which variable we are using since there can be multiple values.</param>
		/// <returns>int containing the pet skill level</returns>
		int GetPetSkillLevel(Item itm, DBRecordCollection record, string recordId, int varNum);
		/// <summary>
		/// Gets the item's requirements
		/// </summary>
		/// <returns>A string containing the items requirements</returns>
		(string[] Requirements, SortedList<string, Variable> RequirementVariables) GetRequirements(Item itm);
		SortedList<string, Variable> GetRequirementVariables(Item itm);
		/// <summary>
		/// Gets the itemID's of all the items in the set.
		/// </summary>
		/// <param name="includeName">Flag to include the set name in the returned array</param>
		/// <returns>Returns a string array containing the remaining set items or null if the item is not part of a set.</returns>
		string[] GetSetItems(Item itm, bool includeName);
		/// <summary>
		/// Gets the level of a triggered skill
		/// </summary>
		/// <param name="record">DBRecord for the triggered skill</param>
		/// <param name="recordId">string containing the record id</param>
		/// <param name="varNum">variable number which we are looking up since there can be multiple values</param>
		/// <returns>int containing the skill level</returns>
		int GetTriggeredSkillLevel(Item itm, DBRecordCollection record, string recordId, int varNum);
		/// <summary>
		/// Parses an item from the save file format
		/// </summary>
		/// <param name="reader">BinaryReader instance</param>
		void Parse(Item itm, BinaryReader reader);
		/// <summary>
		/// Removes the relic from this item
		/// </summary>
		/// <returns>Returns the removed relic as a new Item, if the item has two relics, 
		/// only the first one is returned and the second one is also removed</returns>
		Item RemoveRelic(Item itm);

		/// <summary>
		/// Invalidate item data cache
		/// </summary>
		/// <param name="itm"></param>
		/// <returns>true if cache have been changed</returns>
		bool InvalidateFriendlyNamesCache(params Item[] items);
	}
}