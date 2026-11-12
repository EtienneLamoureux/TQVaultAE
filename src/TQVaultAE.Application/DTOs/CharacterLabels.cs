namespace TQVaultAE.Application.DTOs;

/// <summary>
/// The texts naming a character's class and masteries, read with the character's own map (its linked mod's data when
/// that mod is installed, else the base game's), whatever map is active.
/// </summary>
/// <param name="ClassName">The class name; empty when the save has no class or is not loaded.</param>
/// <param name="MasteryNames">The mastery names joined by " + "; empty when the character has none or is not loaded.</param>
public sealed record CharacterLabels(string ClassName, string MasteryNames)
{
	/// <summary>No class, no mastery: a character not loaded.</summary>
	public static CharacterLabels Empty { get; } = new(string.Empty, string.Empty);
}
