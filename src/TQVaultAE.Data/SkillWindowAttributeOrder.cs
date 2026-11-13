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
/// (charge levels, damage absorption, protections, summon limit, "+1 to all skills"). Then the stores, in the order the
/// game's skill window writes a skill's attributes (unlike an item tool tip, which follows the item class): offense
/// (% bonuses and damage over time before flat damage and debuffs, as in item tool tips), character, the energy
/// reserved, skill, retaliation and defense. Inside a store, the attributes come in the order the game loads them.
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

	/// <summary>
	/// The store outputs after the skill's own lines, in the order the game's skill window writes them: offense,
	/// character, the energy reserved, skill, retaliation, defense.
	/// </summary>
	private static readonly (GameAttributeStore Store, int Output)[] Stores =
	[
		(GameAttributeStore.Offense, 1), (GameAttributeStore.Offense, 2), (GameAttributeStore.Offense, 3),
		(GameAttributeStore.Character, 1), (GameAttributeStore.Character, 2), (GameAttributeStore.Character, EnergyReservedOutput),
		(GameAttributeStore.Skill, 1),
		(GameAttributeStore.Retaliation, 1), (GameAttributeStore.Retaliation, 2), (GameAttributeStore.Retaliation, 3),
		(GameAttributeStore.Defense, 1), (GameAttributeStore.Defense, 2),
	];

	/// <summary>
	/// A skill keeps the energy it reserves (<c>characterManaLimitReserve</c> and its modifier) in a store of its own,
	/// written right after the character attributes: "+25 Defensive Ability", then "75 Energy Reserved".
	/// </summary>
	private const int EnergyReservedOutput = 3;

	private static readonly HashSet<string> EnergyReserved = new(StringComparer.OrdinalIgnoreCase)
	{
		"characterManaLimitReserve", "characterManaLimitReserveModifier",
	};

	/// <summary>
	/// The character attributes of a skill, in the order the game loads and writes them; it differs from an item's
	/// (<see cref="GameAttributeOrder"/>): the movement speed comes before the attack and casting speeds. Attributes not
	/// listed follow, in the item order.
	/// </summary>
	private static readonly string[] CharacterSequence =
	[
		"characterStrength", "characterStrengthModifier", "characterIntelligence", "characterIntelligenceModifier",
		"characterDexterity", "characterDexterityModifier", "characterLife", "characterLifeModifier",
		"characterLifeRegen", "characterLifeRegenModifier", "characterMana", "characterManaModifier",
		"characterManaRegen", "characterManaRegenModifier",
		"characterManaLimitReserveReduction", "characterManaLimitReserveReductionModifier",
		"characterOffensiveAbility", "characterOffensiveAbilityModifier", "characterDefensiveAbility", "characterDefensiveAbilityModifier",
		"characterTotalSpeedModifier", "characterRunSpeed", "characterRunSpeedModifier",
		"characterAttackSpeed", "characterAttackSpeedModifier", "characterSpellCastSpeed", "characterSpellCastSpeedModifier",
	];

	/// <summary>
	/// The last offensive attributes of a skill, after the impaired aim, in the order the game loads them; it differs from
	/// an item's: skill disruption first, then sleep, stun, trap, freeze, petrify, taunt, convert, fear, confusion and the
	/// slowed speeds (total, attack, casting, movement).
	/// </summary>
	private static readonly string[] OffenseTail =
	[
		"Offense:offensiveDisruption", "Offense:offensiveSleep", "OffenseModifier:offensiveSleepModifier",
		"Offense:offensiveStun", "OffenseModifier:offensiveStunModifier", "Offense:offensiveTrap", "Offense:offensiveFreeze",
		"Offense:offensivePetrify", "Offense:offensiveTaunt", "Offense:offensiveConvert", "Offense:offensiveFear",
		"Offense:offensiveConfusion",
		"OffenseSlow:offensiveSlowTotalSpeed", "OffenseSlowModifier:offensiveSlowTotalSpeed",
		"OffenseSlow:offensiveSlowAttackSpeed", "OffenseSlowModifier:offensiveSlowAttackSpeed",
		"OffenseSlow:offensiveSlowSpellCastSpeed", "OffenseSlowModifier:offensiveSlowSpellCastSpeed",
		"OffenseSlow:offensiveSlowRunSpeed", "OffenseSlowModifier:offensiveSlowRunSpeed",
	];

	/// <summary>
	/// Where <see cref="OffenseTail"/> starts: after every other offensive attribute, all of which come before these in
	/// the item order too.
	/// </summary>
	private const int OffenseTailStart = 10_000;

	private static readonly Dictionary<string, int> OffenseTailRanks = OffenseTail
		.Select((name, index) => (name, index))
		.ToDictionary(p => p.name, p => p.index, StringComparer.OrdinalIgnoreCase);

	private static readonly Dictionary<string, int> CharacterRanks = CharacterSequence
		.Select((name, index) => (name, index))
		.ToDictionary(p => p.name, p => p.index, StringComparer.OrdinalIgnoreCase);

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
		int output = slot.Store == GameAttributeStore.Skill ? 1 : slot.Output;
		int index = slot.Index;
		if (slot.Store == GameAttributeStore.Offense && data is not null && OffenseTailRanks.TryGetValue($"{data.EffectType}:{data.Effect}", out var tail))
			index = OffenseTailStart + tail;
		else if (slot.Store == GameAttributeStore.Character)
		{
			if (EnergyReserved.Contains(variableName))
				output = EnergyReservedOutput;

			index = CharacterRanks.TryGetValue(variableName, out var rank) ? rank : CharacterSequence.Length + slot.Index;
		}

		int store = Array.IndexOf(Stores, (slot.Store, output));
		if (store < 0)
			return Other;

		// A global chance keeps its groups right after its header line, in the order the record gives them.
		return new(store + 1, slot.Global ? -1 : index);
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
