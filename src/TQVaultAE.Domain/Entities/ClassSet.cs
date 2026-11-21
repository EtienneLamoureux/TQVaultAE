namespace TQVaultAE.Domain.Entities;

/// <summary>
/// A saved build of a character: its masteries with their skill points, and how its attribute points are spent. The
/// character editor applies it to the character it edits.
/// </summary>
/// <param name="Name">The name the user gave it, unique among the character's class sets (case ignored).</param>
/// <param name="Masteries">The masteries in slot order, none for an empty set.</param>
/// <param name="Attributes">The attribute points spent in each attribute.</param>
public sealed record ClassSet(string Name, IReadOnlyList<ClassSetMastery> Masteries, ClassSetAttributes Attributes)
{
	/// <summary>A set without masteries nor attribute points: applied, every point goes back to the pools.</summary>
	public static ClassSet Empty(string name) => new(name, [], default);

	/// <summary>
	/// True when both sets hold the same build (the name aside): same masteries in any slot order, same skill levels, same
	/// attribute points.
	/// </summary>
	public bool SameBuild(ClassSet other)
		=> this.Attributes == other.Attributes
			&& this.Masteries.Count == other.Masteries.Count
			&& this.Masteries.All(m => other.Masteries.Any(m.SameAs));

	/// <summary>True when both sets have the same masteries, in any order, whatever their points: the class.</summary>
	public bool SameMasteries(ClassSet other)
		=> this.Masteries.Select(m => m.Mastery).ToHashSet().SetEquals(other.Masteries.Select(m => m.Mastery));

	/// <summary>
	/// What this build has more (or less, negative) than <paramref name="reference"/>: the points of each mastery (its level
	/// and its skills) that differ, and the attribute points.
	/// </summary>
	public ClassSetDifference DifferenceFrom(ClassSet reference)
	{
		static int Points(ClassSet set, Masteries mastery)
			=> set.Masteries.Where(m => m.Mastery == mastery).Sum(m => m.Level + m.Skills.Sum(s => s.Level));

		var skills = this.Masteries.Select(m => m.Mastery)
			.Concat(reference.Masteries.Select(m => m.Mastery))
			.Distinct()
			.Select(mastery => (Mastery: mastery, Points: Points(this, mastery) - Points(reference, mastery)))
			.ToList();
		var mine = this.Attributes.ToArray();
		var theirs = reference.Attributes.ToArray();
		return new ClassSetDifference(skills, ClassSetAttributes.FromArray([.. mine.Zip(theirs, (a, b) => a - b)]));
	}
}

/// <summary>How a build differs from a class set (<see cref="ClassSet.DifferenceFrom"/>).</summary>
/// <param name="SkillPoints">Per mastery of either side, the points it has more (negative: less); 0 when only skills moved inside it.</param>
/// <param name="Attributes">The attribute points more (negative: less), per attribute.</param>
public sealed record ClassSetDifference(IReadOnlyList<(Masteries Mastery, int Points)> SkillPoints, ClassSetAttributes Attributes);

/// <summary>A mastery of a class set: its level and the levels of its skills.</summary>
/// <param name="Mastery">The mastery.</param>
/// <param name="Level">The mastery level.</param>
/// <param name="Skills">The skills with points (record id and level).</param>
public sealed record ClassSetMastery(Masteries Mastery, int Level, IReadOnlyList<ClassSetSkill> Skills)
{
	/// <summary>True for the same mastery, level and skill levels, in any order.</summary>
	public bool SameAs(ClassSetMastery other)
		=> this.Mastery == other.Mastery
			&& this.Level == other.Level
			&& this.Skills.Count == other.Skills.Count
			&& this.Skills.All(skill => other.Skills.Any(o => o.Level == skill.Level && string.Equals(o.Skill, skill.Skill, StringComparison.OrdinalIgnoreCase)));
}

/// <summary>A skill of a class set mastery.</summary>
/// <param name="Skill">The skill's record id.</param>
/// <param name="Level">Its points.</param>
public sealed record ClassSetSkill(string Skill, int Level);

/// <summary>The attribute points spent in each attribute (points, not values: 1 point is 4 strength or 40 health).</summary>
public readonly record struct ClassSetAttributes(int Health, int Energy, int Strength, int Intelligence, int Dexterity)
{
	/// <summary>The points in the game's order: Health, Energy, Strength, Intelligence, Dexterity.</summary>
	public int[] ToArray() => [this.Health, this.Energy, this.Strength, this.Intelligence, this.Dexterity];

	/// <summary>The points from an array in the game's order.</summary>
	public static ClassSetAttributes FromArray(IReadOnlyList<int> points) => new(points[0], points[1], points[2], points[3], points[4]);
}
