using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.DTOs;

/// <summary>
/// The values of the character edit dialog, to apply to a character.
/// </summary>
/// <param name="Level">The character level.</param>
/// <param name="XpText">The experience as shown by the dialog; text that is not a number keeps the character's experience.</param>
/// <param name="DifficultyUnlocked">The difficulty unlocked, or <see langword="null"/> to keep the character's one.</param>
/// <param name="SkillPoints">The skill points left to spend.</param>
/// <param name="AttributePoints">The attribute points left to spend.</param>
/// <param name="Strength">The base strength.</param>
/// <param name="Dexterity">The base dexterity.</param>
/// <param name="Intelligence">The base intelligence.</param>
/// <param name="Health">The base health.</param>
/// <param name="Mana">The base mana.</param>
/// <param name="ResetAttributes">Puts the base attributes back to their minimum and gives back the points invested.</param>
/// <param name="ResetMasteries">Removes the masteries and gives back their skill points.</param>
/// <param name="SkillRecords">The edited skill list (<see cref="CharacterSkillEditor.BuildSkillRecords"/>), or <see langword="null"/> to keep the character's one.</param>
/// <param name="MasteriesChanged">True when <paramref name="SkillRecords"/> has other masteries, which clears the class.</param>
public sealed record CharacterEditRequest(
	int Level
	, string? XpText
	, int? DifficultyUnlocked
	, int SkillPoints
	, int AttributePoints
	, int Strength
	, int Dexterity
	, int Intelligence
	, int Health
	, int Mana
	, bool ResetAttributes = false
	, bool ResetMasteries = false
	, IReadOnlyList<SkillRecord>? SkillRecords = null
	, bool MasteriesChanged = false);
