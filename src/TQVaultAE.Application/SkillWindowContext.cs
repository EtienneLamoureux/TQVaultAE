using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application;

/// <summary>A modifier of a skill (a child skill such as Resilience for Adrenaline) at the level it is used.</summary>
/// <param name="Id">The modifier's record.</param>
/// <param name="Level">Its level, with the equipment's levels.</param>
public readonly record struct SkillModifierLevel(RecordId Id, int Level);

/// <summary>
/// What the game's skill window adds to a skill's own values for a character: the equipment's levels to a summon's
/// skills, the modifiers with points (their recharge seconds, recharge reduction and energy cost) and the character's
/// recharge reduction with its cap.
/// </summary>
/// <param name="PetSkillBonus">Levels the equipment adds to the skill, which the game also adds to a summon's skills.</param>
/// <param name="Modifiers">The skill's modifiers that have points.</param>
/// <param name="CooldownReduction">The character's recharge reduction, in percent.</param>
/// <param name="CooldownReductionCap">The highest recharge reduction (<c>playerCooldownReductionCap</c>).</param>
public sealed record SkillWindowContext(
	int PetSkillBonus,
	IReadOnlyList<SkillModifierLevel> Modifiers,
	double CooldownReduction,
	double CooldownReductionCap)
{
	/// <summary>No character: the skill's own values.</summary>
	public static SkillWindowContext None { get; } = new(0, [], 0, 80);
}
