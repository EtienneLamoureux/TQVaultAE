using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Data;

/// <summary>
/// Where the game's skill window lists an effect of a skill: first the skill's own lines (recharge and chances, then
/// its parameters, then its own effects), then the attribute stores in a fixed order.
/// </summary>
/// <param name="Section">The part of the tool tip: 0 for the skill's own lines, then one per store output.</param>
/// <param name="Index">The rank inside the part.</param>
public readonly record struct SkillWindowRank(int Section, int Index) : IComparable<SkillWindowRank>
{
	/// <inheritdoc/>
	public int CompareTo(SkillWindowRank other)
		=> this.Section != other.Section ? this.Section.CompareTo(other.Section) : this.Index.CompareTo(other.Index);

	public static bool operator <(SkillWindowRank left, SkillWindowRank right) => left.CompareTo(right) < 0;

	public static bool operator <=(SkillWindowRank left, SkillWindowRank right) => left.CompareTo(right) <= 0;

	public static bool operator >(SkillWindowRank left, SkillWindowRank right) => left.CompareTo(right) > 0;

	public static bool operator >=(SkillWindowRank left, SkillWindowRank right) => left.CompareTo(right) >= 0;
}

/// <summary>
/// The order in which the game's skill window lists the effects of a skill (and of a summon's skills).
/// </summary>
/// <remarks>
/// The skill's own lines come first: the recharge and the chances to be used or to activate, then the parameters
/// (energy and life costs, duration, area, targets, projectiles, the summon's life time), then the skill's own effects
/// (charge levels, damage absorption, protections, summon limit, "+1 to all skills"). Then the stores: offense
/// (% bonuses and damage over time before flat damage and debuffs, as in item tool tips), character, skill,
/// retaliation and defense.
/// </remarks>
public static class SkillWindowAttributeOrder
{
	/// <summary>The skill's own lines, in the game's order.</summary>
	private static readonly string[] OwnSequence =
	[
		// Recharge and chances.
		"skillCooldownTime", "skillChanceWeight", "lifeMonitorPercent", "onHitActivationChance",

		// Parameters.
		"skillManaCost", "skillActiveLifeCost", "skillActiveManaCost", "skillActiveDuration", "skillTargetRadius",
		"skillTargetAngle", "skillTargetNumber", "skillChargeDuration", "projectileLaunchNumber", "projectilePiercing",
		"piercingProjectile", "explosionRadius", "projectileExplosionRadius", "spawnObjectsTimeToLive",

		// The skill's own effects.
		"skillChargeLevel", "skillLifeBonus", "skillManaBonus", "damageAbsorption", "damageAbsorptionPercent",
		DamageQualifiers, "petLimit", "allSkillEnhancement",
	];

	/// <summary>Stands for the group of damage qualifiers ("Protects Against:") in <see cref="OwnSequence"/>.</summary>
	private const string DamageQualifiers = "*DamageQualifier";

	private static readonly Dictionary<string, int> OwnRanks = OwnSequence
		.Select((name, index) => (name, index))
		.ToDictionary(p => p.name, p => p.index, StringComparer.OrdinalIgnoreCase);

	/// <summary>The store outputs after the skill's own lines, in the game's order.</summary>
	private static readonly (GameAttributeStore Store, int Output)[] Stores =
	[
		(GameAttributeStore.Offense, 1), (GameAttributeStore.Offense, 2), (GameAttributeStore.Offense, 3),
		(GameAttributeStore.Character, 1), (GameAttributeStore.Character, 2),
		(GameAttributeStore.Skill, 1),
		(GameAttributeStore.Retaliation, 1), (GameAttributeStore.Retaliation, 2), (GameAttributeStore.Retaliation, 3),
		(GameAttributeStore.Defense, 1), (GameAttributeStore.Defense, 2),
	];

	/// <summary>An effect the order does not know: after the others.</summary>
	public static SkillWindowRank Other { get; } = new(Stores.Length + 1, 0);

	/// <summary>
	/// Gets where the skill window lists an effect group.
	/// </summary>
	/// <param name="variableName">The name of the first variable of the group.</param>
	/// <param name="data">The attribute data of that variable, or null when unknown.</param>
	/// <param name="isGlobal">Whether the group belongs to a global chance.</param>
	public static SkillWindowRank RankOf(string variableName, ItemAttributesData? data, bool isGlobal = false)
	{
		if (data?.EffectType == ItemAttributesEffectType.DamageQualifierEffect)
			return new(0, OwnRanks[DamageQualifiers]);

		if (OwnRanks.TryGetValue(variableName, out var own) || (data is not null && OwnRanks.TryGetValue(data.Effect, out own)))
			return new(0, own);

		var slot = GameAttributeOrder.SlotOf(variableName, data, isGlobal, physicalIsBase: false);
		int store = Array.IndexOf(Stores, (slot.Store, slot.Store == GameAttributeStore.Skill ? 1 : slot.Output));
		if (store < 0)
			return Other;

		// A global chance keeps its groups right after its header line, in the order the record gives them.
		return new(store + 1, slot.Global ? -1 : slot.Index);
	}
}

/// <summary>
/// The lines the game's skill window writes itself for a record (recharge, energy cost, chance of activating, the
/// summon's life time...), each with the variable that places it in <see cref="SkillWindowAttributeOrder"/>.
/// </summary>
/// <param name="Lines">The variable and the text of each line.</param>
public sealed record SkillWindowLines(IReadOnlyList<(string Variable, string Text)> Lines)
{
	/// <summary>No line of its own: only the order.</summary>
	public static SkillWindowLines None { get; } = new([]);
}
