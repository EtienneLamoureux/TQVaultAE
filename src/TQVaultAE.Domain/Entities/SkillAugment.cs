namespace TQVaultAE.Domain.Entities;

/// <summary>What a skill bonus of an item applies to.</summary>
public enum SkillAugmentKind
{
	/// <summary>One skill (<c>augmentSkillNameN</c> / <c>augmentSkillLevelN</c>).</summary>
	Skill,

	/// <summary>Every skill of a mastery (<c>augmentMasteryNameN</c> / <c>augmentMasteryLevelN</c>).</summary>
	Mastery,

	/// <summary>Every skill (<c>augmentAllLevel</c>).</summary>
	All,
}

/// <summary>
/// Skill levels an item adds: to one skill, to the skills of a mastery or to all skills.
/// </summary>
/// <param name="Kind">What the bonus applies to.</param>
/// <param name="Target">The skill or mastery record; <see langword="null"/> for all skills.</param>
/// <param name="Levels">The levels added.</param>
public sealed record SkillAugment(SkillAugmentKind Kind, RecordId? Target, int Levels);

/// <summary>
/// The levels the equipped items add to a skill, by kind of bonus.
/// </summary>
/// <param name="Skill">Levels added to this skill itself.</param>
/// <param name="Mastery">Levels added to every skill of its mastery.</param>
/// <param name="All">Levels added to every skill.</param>
public readonly record struct SkillBonus(int Skill, int Mastery, int All)
{
	/// <summary>All the levels added.</summary>
	public int Total => this.Skill + this.Mastery + this.All;

	/// <summary>
	/// Sums the bonuses of the equipped items that apply to a skill of a mastery: the ones naming the skill, the ones
	/// naming the mastery, and the ones to all skills. The mastery itself gets none of them.
	/// </summary>
	public static SkillBonus For(IEnumerable<SkillAugment> augments, MasteryTree tree, RecordId skillId)
	{
		if (skillId == tree.Id)
			return default;

		int skill = 0, mastery = 0, all = 0;
		foreach (var augment in augments)
		{
			switch (augment.Kind)
			{
				case SkillAugmentKind.Skill when augment.Target == skillId:
					skill += augment.Levels;
					break;
				case SkillAugmentKind.Mastery when augment.Target == tree.Id:
					mastery += augment.Levels;
					break;
				case SkillAugmentKind.All:
					all += augment.Levels;
					break;
			}
		}

		return new SkillBonus(skill, mastery, all);
	}
}
