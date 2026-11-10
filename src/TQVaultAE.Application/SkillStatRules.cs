using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application;

/// <summary>How a skill adds its bonuses to the character window.</summary>
public enum SkillStatRole
{
	/// <summary>Never: attacks, pets, spells; their bonuses go to what they cast.</summary>
	None,

	/// <summary>Always, from its first point: passive skills and the masteries.</summary>
	Passive,

	/// <summary>Only while it runs: toggled auras, timed buffs, buffs on hit or on a kill.</summary>
	Buff,

	/// <summary>Like the skill it modifies (<c>the game</c>).</summary>
	Modifier,
}

/// <summary>
/// Which skills the game counts in the character window, and at which level (the game
/// <c>the game</c>, <c>the game</c>,
/// <c>the game</c>).
/// </summary>
public static class SkillStatRules
{
	/// <summary>The role of a skill from the class of its record.</summary>
	public static SkillStatRole RoleOf(string? recordClass)
	{
		if (string.IsNullOrEmpty(recordClass))
			return SkillStatRole.None;

		if (recordClass.Equals("Skill_Passive", StringComparison.OrdinalIgnoreCase)
			|| recordClass.Equals("Skill_Mastery", StringComparison.OrdinalIgnoreCase))
			return SkillStatRole.Passive;

		if (recordClass.Equals("Skill_Modifier", StringComparison.OrdinalIgnoreCase))
			return SkillStatRole.Modifier;

		// Skill_BuffSelfToggled, Skill_BuffRadiusToggled, Skill_BuffSelfDuration, Skill_PassiveOnHitBuffSelf...; not the
		// debuffs, which go on monsters.
		if (recordClass.StartsWith("Skill_", StringComparison.OrdinalIgnoreCase)
			&& recordClass.Contains("Buff", StringComparison.OrdinalIgnoreCase)
			&& !recordClass.Contains("Debuf", StringComparison.OrdinalIgnoreCase))
			return SkillStatRole.Buff;

		if (recordClass.StartsWith("Skill_PassiveOn", StringComparison.OrdinalIgnoreCase))
			return SkillStatRole.Buff;

		return SkillStatRole.None;
	}

	/// <summary>
	/// The role a modifier takes from the skill it modifies: the parent's, or none when the parent is unknown or does
	/// not count.
	/// </summary>
	public static SkillStatRole Resolve(SkillStatRole role, SkillStatRole? parentRole)
		=> role != SkillStatRole.Modifier ? role
			: parentRole is SkillStatRole.Passive or SkillStatRole.Buff ? parentRole.Value
			: SkillStatRole.None;

	/// <summary>Tells whether a skill of that role counts, buffs only when they are asked for.</summary>
	public static bool Counts(SkillStatRole role, bool includeBuffs)
		=> role == SkillStatRole.Passive || (role == SkillStatRole.Buff && includeBuffs);

	/// <summary>
	/// The level the game uses: the points spent plus the levels the equipped items add (to the skill, to its mastery,
	/// to all skills), none without a point spent. The mastery itself gets no item level. Arrays past the skill's
	/// highest level read their last value.
	/// </summary>
	/// <param name="tree">The mastery of the skill, or <see langword="null"/> for a skill outside the masteries (quest rewards).</param>
	public static int EffectiveLevel(int savedLevel, RecordId skillId, MasteryTree? tree, IEnumerable<SkillAugment> augments)
	{
		if (savedLevel <= 0)
			return 0;

		if (tree is not null)
			return savedLevel + SkillBonus.For(augments, tree, skillId).Total;

		return savedLevel + augments
			.Where(a => a.Kind == SkillAugmentKind.All || (a.Kind == SkillAugmentKind.Skill && a.Target == skillId))
			.Sum(a => a.Levels);
	}
}
