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

	/// <summary>Like the skill it modifies.</summary>
	Modifier,
}

/// <summary>
/// Which skills the game counts in the character window, and at which level.
/// </summary>
public static class SkillStatRules
{
	/// <summary>
	/// The role of a skill from the class of its record and, for a skill that casts a buff (<c>buffSkillName</c>), the
	/// class of that buff's record. What goes on monsters never counts: an attack's buff (Freezing Blast casts on what it
	/// hits a buff of -100% physical resistance), and a buff whose record is a debuff (an aura that weakens the monsters
	/// around, a contagion).
	/// </summary>
	/// <param name="recordClass">The class of the skill's record.</param>
	/// <param name="buffRecordClass">The class of the record of the buff it casts, or <see langword="null"/> when it casts none.</param>
	public static SkillStatRole RoleOf(string? recordClass, string? buffRecordClass = null)
	{
		if (string.IsNullOrEmpty(recordClass))
			return SkillStatRole.None;

		// Skill_AttackBuff, Skill_AttackBuffRadius...: the buff goes on the targets of the attack.
		if (recordClass.StartsWith("Skill_Attack", StringComparison.OrdinalIgnoreCase))
			return SkillStatRole.None;

		if (IsMonsterBuff(buffRecordClass))
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
	/// Tells whether a buff record goes on monsters: the debuffs (<c>SkillBuff_Debuf</c>, <c>SkillBuff_DebufFreeze</c>,
	/// <c>SkillBuff_DebufTrap</c>...) and the contagions (<c>SkillBuff_Contageous</c>, spread from monster to monster).
	/// </summary>
	public static bool IsMonsterBuff(string? buffRecordClass)
		=> !string.IsNullOrEmpty(buffRecordClass)
			&& (buffRecordClass.StartsWith("SkillBuff_Debuf", StringComparison.OrdinalIgnoreCase)
				|| buffRecordClass.StartsWith("SkillBuff_Contageous", StringComparison.OrdinalIgnoreCase));

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
	/// The skills the equipped items grant, as the game gives them to the character: each skill once, whatever the
	/// number of items granting it, at the level an item grants (the higher one when several do; the game keeps the
	/// level of the item put on last). They count by their role like the character's own skills (passives always,
	/// buffs and auras only while they run), at that level: no point is needed and the items' "+skill" levels are not
	/// added.
	/// </summary>
	public static IEnumerable<(RecordId SkillId, int Level)> ItemSkills(IEnumerable<(RecordId SkillId, int Level)> granted)
		=> granted
			.Where(s => !RecordId.IsNullOrEmpty(s.SkillId) && s.Level > 0)
			.GroupBy(s => s.SkillId)
			.Select(g => (g.Key, g.Max(s => s.Level)));

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
