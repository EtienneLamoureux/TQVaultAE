using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Contracts.Services;

/// <summary>
/// Collects what the game adds up for the character window: the bonuses of the equipped items and item sets, of the
/// skills, and the game's caps and equations.
/// </summary>
public interface ICharacterStatsService
{
	/// <summary>
	/// Sums the equipped items (not the weapons of the other weapon set) and the bonuses of the item sets they make.
	/// </summary>
	AttributeSum GetGearAttributes(IEnumerable<Item> equipment);

	/// <summary>
	/// Sums the skills: the passive ones (and masteries) always, the buffs when <paramref name="includeBuffs"/>, the
	/// modifiers like the skill they modify; each at its level plus the levels the equipped items add.
	/// </summary>
	/// <param name="skills">The skills of the character, with their points.</param>
	/// <param name="augments">The skill levels the equipped items add.</param>
	/// <param name="trees">The masteries of the character, for the mastery bonuses and the modifiers' parents.</param>
	AttributeSum GetSkillAttributes(IEnumerable<SkillRecord> skills, IReadOnlyList<SkillAugment> augments, IReadOnlyList<MasteryTree> trees, bool includeBuffs);
}
