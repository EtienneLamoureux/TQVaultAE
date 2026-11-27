using TQVaultAE.Application.Character;
using TQVaultAE.Application.Session;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Results;

namespace TQVaultAE.Application.Contracts.Services;

/// <summary>
/// Builds the texts of an item for its tool tips, its properties and the search: names, attribute lines with their colour
/// tags, requirements, set, granted skills.
/// </summary>
public interface IItemTextFormatter
{
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
	/// Gets the item's requirements
	/// </summary>
	/// <returns>A string containing the items requirements</returns>
	(string[] Requirements, SortedList<string, Variable> RequirementVariables) GetRequirements(Item itm);
	/// <summary>
	/// The requirement lines as the game's tool tip writes them: level, strength, dexterity, intelligence; with
	/// <paramref name="reduced"/> (a character's), the reduced value, "[ base - x% Reduction]" and red when not met.
	/// </summary>
	string[] FormatRequirements(SortedList<string, Variable> variables, IReadOnlyDictionary<string, ReducedRequirement>? reduced = null);
	/// <summary>
	/// The bonus lines of the item's set for <paramref name="worn"/> pieces worn, as the game lists them under the set
	/// pieces; empty without set or bonus at that count.
	/// </summary>
	IReadOnlyList<string> FormatSetBonus(Item itm, int worn);
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
	/// Invalidate item data cache
	/// </summary>
	/// <returns>true if cache have been changed</returns>
	bool InvalidateFriendlyNamesCache(params IEnumerable<Item> items);
}
