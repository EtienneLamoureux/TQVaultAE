using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Results;

namespace TQVaultAE.Application.Contracts.Providers;

public interface IItemProvider
{
	/// <summary>
	/// Gets s string containing the prefix of the item class for use in the requirements equation.
	/// </summary>
	/// <param name="itemClass">string containing the item class</param>
	/// <returns>string containing the prefix of the item class for use in the requirements equation</returns>
	string GetRequirementEquationPrefix(string itemClass);

	/// <summary>
	/// Find all affixes available for this item <paramref name="type"/>
	/// </summary>
	/// <param name="itemId"></param>
	/// <returns><c>null</c> if <paramref name="type"/> is not wearable (can't have an affix)</returns>
	ItemAffixes? GetAllAvailableAffixes(GearType type);
	/// <summary>
	/// Gets the socketed charm/relic bonus loot table
	/// </summary>
	/// <param name="Item"></param>
	/// <param name="RelicTable1"></param>
	/// <param name="RelicTable2"></param>
	/// <returns>Returns <c>false</c> if the item does not contain a charm/relic</returns>
	bool BonusTableSocketedRelic(Item Item, out LootTableCollection? RelicTable1, out LootTableCollection? RelicTable2);
	/// <summary>
	/// Return all affixes for <paramref name="itemId"/>
	/// </summary>
	/// <param name="itemId"></param>
	/// <returns></returns>
	ItemAffixes? GetItemAffixes(RecordId itemId);
	/// <summary>
	/// Gets the artifact/charm/relic bonus loot table
	/// </summary>
	/// <param name="itm"></param>
	/// <returns>returns null if the item is not an artifact/charm/relic</returns>
	LootTableCollection? BonusTableRelicOrArtifact(Item itm);
	/// <summary>
	/// Create an artifact from its formulae
	/// </summary>
	/// <returns>A new artifact</returns>
	Item? CraftArtifact(Item itm);
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
	string Format(string? formatSpec, object? parameter1, object? parameter2 = null, object? parameter3 = null);
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
	SetItemInfo? GetItemSetTranslations(Item itm);
	/// <summary>
	/// Pet skills can also have multiple values so we attempt to decode it here
	/// unfortunately the level is not in the skill record so we need to look up
	/// the level from the pet record.
	/// </summary>
	/// <param name="record">DBRecord of the skill</param>
	/// <param name="recordId">string of the record id</param>
	/// <param name="varNum">Which variable we are using since there can be multiple values.</param>
	/// <returns>int containing the pet skill level</returns>
	int GetPetSkillLevel(Item itm, DBRecordCollection record, RecordId recordId, int varNum);
	/// <summary>
	/// Gets the item's requirements
	/// </summary>
	/// <returns>A string containing the items requirements</returns>
	(string[] Requirements, SortedList<string, Variable> RequirementVariables) GetRequirements(Item itm);
	SortedList<string, Variable> GetRequirementVariables(Item itm);

	/// <summary>
	/// Extract numerical requirements
	/// </summary>
	/// <param name="item"></param>
	/// <returns></returns>
	RequirementInfo GetRequirementInfo(Item item);

	/// <summary>
	/// Extract numerical requirements
	/// </summary>
	/// <param name="item"></param>
	/// <param name="requirementVariables"></param>
	/// <returns></returns>
	RequirementInfo GetRequirementInfo(Item item, SortedList<string, Variable> requirementVariables);

	SortedList<string, int> GetStatBonuses(Item item);

	/// <summary>
	/// Gets the skill levels an item adds (to a skill, to the skills of a mastery, to all skills): from its base
	/// record, its affixes, its relics at their number of pieces and their completion bonuses.
	/// </summary>
	IReadOnlyList<SkillAugment> GetSkillAugments(Item item);
	void GetStatBonusesFromRecord(SortedList<string, int> statBonuses, DBRecordCollection? record, int statLevel = 0);
	/// <summary>
	/// Sums every numeric variable of an item, as the game collects it for the character window: its base record, its
	/// affixes (with the values the game rolls from the item seed), its relics or charms at their number of pieces and
	/// their completion bonuses.
	/// </summary>
	AttributeSum GetAttributeSums(Item item);

	/// <summary>
	/// Sums the bonuses of the item sets worn: each set record read at the number of its pieces among the items.
	/// </summary>
	AttributeSum GetSetBonuses(IEnumerable<Item> equipped);

	/// <summary>Sums the numeric variables of one record at a level (arrays read at level - 1, the last value past their end).</summary>
	AttributeSum GetRecordAttributes(RecordId? recordId, int level);
	/// <summary>
	/// Gets the itemID's of all the items in the set.
	/// </summary>
	/// <returns>Returns <see cref="SetItemInfo"/> containing the remaining set items or null if the item is not part of a set.</returns>
	SetItemInfo? GetSetItems(Item itm);
	/// <summary>
	/// Tells whether the item grants a skill, by its base record, its affixes, its relics or their completion bonuses.
	/// Only skills with a name count, as in the tooltip: the game's hidden helper skills do not.
	/// </summary>
	bool HasGrantedSkill(Item itm);
	/// <summary>
	/// The translated names of the skills the item grants (see <see cref="HasGrantedSkill"/>), each once.
	/// </summary>
	IReadOnlyList<string> GetGrantedSkillNames(Item itm);
	/// <summary>
	/// Describes a skill at a level as the item tooltips describe a granted skill: name, icon, description and its effects
	/// at that level (a buff skill takes them from its buff record, a pet skill from its pet).
	/// </summary>
	/// <param name="context">What the character adds to the skill's own values (modifiers, recharge reduction...).</param>
	/// <returns><see langword="null"/> when the record is missing or has no name.</returns>
	GrantedSkill? GetSkillAtLevel(RecordId skillId, int level, SkillWindowContext? context = null);
	/// <summary>
	/// Gets the level of a triggered skill
	/// </summary>
	/// <param name="record">DBRecord for the triggered skill</param>
	/// <param name="recordId">string containing the record id</param>
	/// <param name="varNum">variable number which we are looking up since there can be multiple values</param>
	/// <returns>int containing the skill level</returns>
	int GetTriggeredSkillLevel(Item itm, DBRecordCollection record, RecordId recordId, int varNum);
	/// <summary>
	/// Parses an item from the save file format
	/// </summary>
	/// <param name="reader">BinaryReader instance</param>
	void Parse(Item itm, BinaryReader reader);

	/// <summary>
	/// Removes the first relic from this item
	/// </summary>
	/// <returns>Returns the removed relic as a new Item</returns>
	Item? RemoveRelic1(Item itm);

	/// <summary>
	/// Removes the second relic from this item
	/// </summary>
	/// <returns>Returns the removed relic as a new Item</returns>
	Item? RemoveRelic2(Item itm);

	/// <summary>
	/// Invalidate item data cache
	/// </summary>
	/// <returns>true if cache have been changed</returns>
	bool InvalidateFriendlyNamesCache(params IEnumerable<Item> items);
}