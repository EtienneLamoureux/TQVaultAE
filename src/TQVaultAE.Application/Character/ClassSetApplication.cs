using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Character;

/// <summary>
/// A class set applied to a build: its masteries and skills, then its attributes, within the points the character has.
/// The character editor applies it to its editors, the read-only panels to the save, both through <see cref="Apply"/>.
/// </summary>
public static class ClassSetApplication
{
	/// <summary>
	/// Applies <paramref name="set"/>: its masteries to <paramref name="skills"/> (<see cref="CharacterSkillEditor.ApplyClassSet"/>),
	/// then its attribute points to <paramref name="attributes"/> (<see cref="ClassSetAttributeRules.Targets"/>). No point is
	/// lost: those the set does not spend stay in the points left.
	/// </summary>
	/// <param name="skills">The skills of the build, changed in place.</param>
	/// <param name="attributes">The base attributes: Health, Energy, Strength, Intelligence, Dexterity.</param>
	/// <param name="attributePoints">The attribute points left.</param>
	/// <param name="set">The class set.</param>
	public static ClassSetApplied Apply(CharacterSkillEditor skills, IReadOnlyList<int> attributes, int attributePoints, ClassSet set)
	{
		bool masteriesComplete = skills.ApplyClassSet(set.Masteries);
		var (targets, attributesComplete) = ClassSetAttributeRules.Targets(attributes, set.Attributes, attributePoints);

		// The points left: those before, plus the whole points given back, less those given.
		int left = attributePoints + ClassSetAttributeRules.PointsOf(attributes).ToArray().Sum() - ClassSetAttributeRules.PointsOf(targets).ToArray().Sum();
		return new ClassSetApplied(targets, left, masteriesComplete && attributesComplete);
	}
}

/// <summary>A build a class set gave: its attributes; its skills are those of the skill editor it was applied to.</summary>
/// <param name="Attributes">The base attributes: Health, Energy, Strength, Intelligence, Dexterity.</param>
/// <param name="AttributePoints">The attribute points left.</param>
/// <param name="Complete">False when the set did not fit (too few points or mastery slots): applied in part.</param>
public sealed record ClassSetApplied(IReadOnlyList<int> Attributes, int AttributePoints, bool Complete);
