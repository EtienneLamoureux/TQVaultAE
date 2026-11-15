using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Jitter;
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
	/// Pulls data out of the TQ item database for this item.
	/// </summary>
	void GetDBData(Item itm);
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

	/// <summary>
	/// Gets the skill levels an item adds (to a skill, to the skills of a mastery, to all skills): from its base
	/// record, its affixes, its relics at their number of pieces and their completion bonuses.
	/// </summary>
	IReadOnlyList<SkillAugment> GetSkillAugments(Item item);
	/// <summary>
	/// Sums every numeric variable of an item, as the game collects it for the character window: its base record, its
	/// affixes (with the values the game rolls from the item seed), its relics or charms at their number of pieces and
	/// their completion bonuses.
	/// </summary>
	AttributeSum GetAttributeSums(Item item);
	/// <summary>
	/// Replays the game's stat jitter for equipment (armor, jewellery, weapons, shields): the values the game rolled from
	/// the item seed for its base record and its affixes, and their possible ranges.
	/// </summary>
	/// <param name="baseRecord">The item's base record, already read.</param>
	/// <returns><see langword="null"/> when the item is not equipment or has no seed.</returns>
	ItemRollResult? RollInGameValues(Item itm, DBRecordCollection? baseRecord);

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
	/// The skills the item grants with their level (<c>itemSkillName</c> at <c>itemSkillLevel</c>, at least 1), from its
	/// base record, its affixes, its relics and their completion bonuses, hidden helper skills included: what the game
	/// gives the character who wears it.
	/// </summary>
	IReadOnlyList<(RecordId SkillId, int Level)> GetGrantedSkillLevels(Item itm);
	/// <summary>
	/// The records of an item that grant a skill (<c>itemSkillName</c>): its base, its affixes, its relics and their
	/// completion bonuses, each once, with the skill and the level they grant it at (<c>itemSkillLevel</c>, at least 1).
	/// </summary>
	IEnumerable<(DBRecordCollection Source, RecordId SkillId, int Level)> GetGrantedSkillSources(Item itm);
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
}