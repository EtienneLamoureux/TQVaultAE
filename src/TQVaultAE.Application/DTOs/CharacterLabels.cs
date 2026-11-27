using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;

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

	/// <summary>
	/// The masteries of a character for the character drop down: their names in the game's text joined by " + ", in
	/// the order of the skill window's panes (<see cref="Masteries"/>).
	/// </summary>
	/// <param name="masteries">The character's masteries (<see cref="ICharacterEditService.GetMasteries"/>).</param>
	/// <param name="translations">Translates the name tags.</param>
	/// <returns>An empty text when the character has no mastery.</returns>
	public static string JoinMasteryNames(IEnumerable<MasteryTree> masteries, ITranslationService translations)
		=> string.Join(" + ", masteries
			.OrderBy(tree => tree.Mastery)
			.Select(tree => translations.TranslateXTag(tree.NameTag)));
}
