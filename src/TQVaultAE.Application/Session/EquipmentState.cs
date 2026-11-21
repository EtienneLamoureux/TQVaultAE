using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Session;

/// <summary>
/// The build the worn pieces are checked against: the level, the base attributes and the skills (the save's, or a class
/// set shown).
/// </summary>
public sealed record RequirementBuild(int Level, int Strength, int Dexterity, int Intelligence, IReadOnlyList<SkillRecord> Skills, IReadOnlyList<MasteryTree> Trees)
{
	/// <summary>The save's build.</summary>
	public static RequirementBuild Of(PlayerInfo info, IReadOnlyList<MasteryTree> trees)
		=> new(info.CurrentLevel, info.BaseStrength, info.BaseDexterity, info.BaseIntelligence, info.SkillRecordList, trees);
}

/// <summary>
/// The worn pieces as the game attaches them (<see cref="Contracts.Services.IPlayerService.ResolveEquipment"/>): a piece
/// whose requirements fail, its own bonuses counted, stays in its slot but is detached and gives nothing (stats, item
/// set, granted skills, "+skill" levels); it is attached again once they pass without its bonuses. The weapon set not in
/// hand gives nothing either.
/// </summary>
/// <param name="Active">The worn pieces without the detached ones (the weapon set not in hand included: the sums leave it out).</param>
/// <param name="Detached">The pieces of the set in hand and of the other slots whose requirements fail.</param>
/// <param name="Totals">The level and the attribute totals with the attached pieces and the buffs running, which an item out of reach is told by.</param>
/// <param name="Gear">The bonuses of the attached pieces and their sets.</param>
/// <param name="Skills">The bonuses of the learned skills (with the attached pieces' "+skill" levels) and of the granted skills, no buff.</param>
public sealed record EquipmentState(IReadOnlyList<Item> Active, IReadOnlyList<Item> Detached, RequirementAttributes Totals, AttributeSum Gear, AttributeSum Skills);
