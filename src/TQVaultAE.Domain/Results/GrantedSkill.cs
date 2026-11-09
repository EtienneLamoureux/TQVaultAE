using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Domain.Results;

/// <summary>
/// A skill granted by an item, an affix or a relic, ready for display.
/// </summary>
/// <param name="SkillId">The granted skill record (<c>itemSkillName</c>).</param>
/// <param name="Name">The translated skill name.</param>
/// <param name="Level">The level granted (<c>itemSkillLevel</c>), at least 1.</param>
/// <param name="LevelText">The level, formatted like the game.</param>
/// <param name="Icon">The skill icon (<c>skillUpBitmapName</c>), or <see langword="null"/> when the skill has none.</param>
/// <param name="Activation">When the skill triggers by itself (<c>itemSkillAutoController</c>), or <see langword="null"/>.</param>
/// <param name="Description">The skill description, word wrapped.</param>
/// <param name="Attributes">The skill effects at <paramref name="Level"/>, color tagged like the item attributes.</param>
public sealed record GrantedSkill(
	RecordId SkillId,
	string Name,
	int Level,
	string LevelText,
	RecordId? Icon,
	string? Activation,
	string[] Description,
	string[] Attributes
)
{
	/// <summary>
	/// Indexes in <see cref="Attributes"/> of the lines the character's modifiers changed (the skill window's energy cost
	/// raised by Flush Out), which the skill window repeats at the next level even when they do not change there.
	/// </summary>
	public IReadOnlyList<int> ModifiedLines { get; init; } = [];
}
