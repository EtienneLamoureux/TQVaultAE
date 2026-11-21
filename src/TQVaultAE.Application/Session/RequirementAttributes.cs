using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Session;

/// <summary>
/// What the item requirements are checked against: the level and the attributes with their gear and skill bonuses, and
/// those bonus sums for the requirement reductions (<see cref="EquipmentRequirements"/>).
/// </summary>
/// <param name="Gear">The bonuses of the gear counted, for the reductions; null for none.</param>
/// <param name="Skills">The bonuses of the skills counted, for the reductions; null for none.</param>
public sealed record RequirementAttributes(int Level, int Strength, int Dexterity, int Intelligence, AttributeSum? Gear = null, AttributeSum? Skills = null)
{
	/// <summary>The save's: <see cref="PlayerInfo.CalculatedStrength"/> and the others, with its bonuses.</summary>
	public static RequirementAttributes Of(PlayerInfo info)
		=> new(info.CurrentLevel, info.CalculatedStrength, info.CalculatedDexterity, info.CalculatedIntelligence, info.GearBonus, info.SkillBonus);

	/// <summary>A bonus variable summed over the gear and the skills (a requirement reduction).</summary>
	public double Bonus(string field) => (this.Gear?[field] ?? 0) + (this.Skills?[field] ?? 0);
}
