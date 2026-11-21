using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Session;

/// <summary>
/// The game's rule for the requirements of an item worn or put in a slot: each requirement is lowered by the character's
/// requirement reductions in % (<c>character*ReqReduction</c>: the item type's, the weapons', the global one; the
/// level's for the level), rounded to the nearest, then compared with the character's level and attribute totals. A worn
/// piece whose requirements fail stays in its slot but gives nothing (<see cref="EquipmentState"/>).
/// </summary>
public static class EquipmentRequirements
{
	/// <summary>The reduction of the level requirement.</summary>
	public const string LevelReduction = "characterLevelReqReduction";

	/// <summary>The reduction of the strength, dexterity and intelligence requirements of every item.</summary>
	public const string GlobalReduction = "characterGlobalReqReduction";

	/// <summary>The attribute requirements, as named in the requirement variables and in the reduction fields.</summary>
	private static readonly string[] Attributes = [Variable.KEY_STRENGTH, Variable.KEY_DEXTERITY, Variable.KEY_INTELLIGENCE];

	/// <summary>
	/// The type an item's requirement reductions are named after (<c>characterArmorStrengthReqReduction</c>), null for
	/// none (an artifact, a relic: level only).
	/// </summary>
	public static string? TypeOf(string? itemClass) => itemClass switch
	{
		null => null,
		Item.ICLASS_SHIELD => "Shield",
		Item.ICLASS_STAFF => "Staff",
		_ when itemClass.StartsWith(Item.ICLASS_PREFIX_ARMORPROTECTIVE, StringComparison.OrdinalIgnoreCase)
			|| itemClass.StartsWith(Item.ICLASS_PREFIX_ARMORMISC,StringComparison.OrdinalIgnoreCase) => "Armor",
		_ when itemClass.StartsWith(Item.ICLASS_PREFIX_ARMORJEWELRY, StringComparison.OrdinalIgnoreCase) => "Jewelry",
		_ when itemClass.StartsWith(Item.ICLASS_PREFIX_MELEE, StringComparison.OrdinalIgnoreCase) => "Melee",
		_ when itemClass.StartsWith(Item.ICLASS_PREFIX_HUNTING,StringComparison.OrdinalIgnoreCase) => "Hunting",
		_ => null,
	};

	/// <summary>
	/// The reduction in % of the <paramref name="attribute"/> requirement (Strength, Dexterity or Intelligence) of an item
	/// of <paramref name="itemClass"/>: its type's (none below 0), the weapons' for a weapon (a shield and a staff
	/// included), and the global one (when above 0).
	/// </summary>
	public static double Reduction(string? itemClass, string attribute, Func<string, double> bonus)
	{
		double reduction = TypeOf(itemClass) is { } type ? Math.Max(0, bonus($"character{type}{attribute}ReqReduction")) : 0;
		if (itemClass?.StartsWith(Item.ICLASS_PREFIX_WEAPON, StringComparison.OrdinalIgnoreCase) == true)
			reduction += bonus($"characterWeapon{attribute}ReqReduction");

		return reduction + Math.Max(0, bonus(GlobalReduction));
	}

	/// <summary>A requirement lowered by a reduction in %, rounded to the nearest as the game does; no requirement stays none.</summary>
	public static int Reduced(int requirement, double reductionPercent)
		=> requirement <= 0 ? requirement : (int)(requirement - (reductionPercent / 100 * requirement) + 0.5);

	/// <summary>Whether <paramref name="have"/> meets the requirements of an item of <paramref name="itemClass"/>, once reduced.</summary>
	/// <param name="requirements">The item's requirement variables (level, strength, dexterity, intelligence).</param>
	public static bool Meets(IReadOnlyDictionary<string, Variable> requirements, string? itemClass, RequirementAttributes have)
		=> ReducedValues(requirements, itemClass, have).Values.All(r => r.Met);

	/// <summary>
	/// The level and attribute requirements of an item of <paramref name="itemClass"/> once lowered by the reductions of
	/// <paramref name="have"/>, by requirement variable name; the requirements the item does not have are left out.
	/// </summary>
	public static IReadOnlyDictionary<string, ReducedRequirement> ReducedValues(IReadOnlyDictionary<string, Variable> requirements, string? itemClass, RequirementAttributes have)
	{
		var reduced = new Dictionary<string, ReducedRequirement>(StringComparer.Ordinal);
		if (requirements.TryGetValue(Variable.KEY_LEVELREQ, out var level))
			reduced[Variable.KEY_LEVELREQ] = Of(level.GetInt32(), Math.Max(0, have.Bonus(LevelReduction)), have.Level);

		foreach (var attribute in Attributes)
		{
			if (requirements.TryGetValue(attribute, out var need))
				reduced[attribute] = Of(need.GetInt32(), Reduction(itemClass, attribute, have.Bonus), Have(have, attribute));
		}

		return reduced;
	}

	private static ReducedRequirement Of(int requirement, double reductionPercent, double have)
		=> new(requirement, Reduced(requirement, reductionPercent), reductionPercent, have);

	private static double Have(RequirementAttributes have, string attribute) => attribute switch
	{
		Variable.KEY_STRENGTH => have.Strength,
		Variable.KEY_DEXTERITY => have.Dexterity,
		_ => have.Intelligence,
	};
}

/// <summary>A requirement of an item for a character: its base value, once reduced by <paramref name="Percent"/>, and what
/// the character has.</summary>
public readonly record struct ReducedRequirement(int Base, int Value, double Percent, double Have)
{
	/// <summary>The character meets the requirement once reduced.</summary>
	public bool Met => this.Value <= this.Have;

	/// <summary>
	/// The game's tool tip shows the reduction ("[ base - x% Reduction]") only when the base is not met and a reduction
	/// applies; a base met is shown alone.
	/// </summary>
	public bool ShowsReduction => this.Have < this.Base && this.Percent > 0;
}
