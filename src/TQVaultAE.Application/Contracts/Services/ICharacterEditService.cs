using TQVaultAE.Application.DTOs;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Results;

namespace TQVaultAE.Application.Contracts.Services;

/// <summary>
/// Applies the character edit dialog to a character: level, experience, difficulty, attributes and masteries.
/// </summary>
public interface ICharacterEditService
{
	/// <summary>
	/// Builds the edited player info of <paramref name="source"/>, without changing it.
	/// </summary>
	/// <remarks>
	/// <list type="bullet">
	/// <item>The masteries allowed follow the level: none under 2, one under 8, else two.</item>
	/// <item>Experience text that is not a number keeps the experience of <paramref name="source"/>.</item>
	/// <item>A reset of the attributes puts them back to their minimum and adds the points invested to the pool of
	/// <paramref name="source"/>.</item>
	/// <item>A reset of the masteries removes their skills and gives back their skill points.</item>
	/// <item>An edited skill list replaces the skills of <paramref name="source"/>; other masteries clear the class.</item>
	/// <item>Unlocking another difficulty raises the money to 5M for Epic, 7.5M for Legendary.</item>
	/// </list>
	/// </remarks>
	PlayerInfo BuildPlayerInfo(PlayerInfo source, CharacterEditRequest request);

	/// <summary>
	/// Applies <paramref name="request"/> to the character of <paramref name="player"/> and writes it to its raw data.
	/// </summary>
	/// <returns>false, changing nothing, when the character has no player info or is read-only.</returns>
	bool Apply(PlayerCollection player, CharacterEditRequest request);

	/// <summary>
	/// The masteries of a character, as the skill window of the game database in use lays them out (a mod's own
	/// masteries with a mod's database), in the order they were chosen: the masteries of
	/// <see cref="CreateSkillEditor"/>.
	/// </summary>
	IReadOnlyList<MasteryTree> GetMasteries(PlayerInfo info);

	/// <summary>
	/// Starts editing the masteries and skills of a character, with the skills of each mastery from the game database.
	/// </summary>
	CharacterSkillEditor CreateSkillEditor(PlayerInfo info);

	/// <summary>
	/// Gets the skill bonuses of the items the character wears, except the weapons of the second weapon set.
	/// </summary>
	IReadOnlyList<SkillAugment> GetGearSkillAugments(PlayerCollection player);

	/// <summary>
	/// Gets the skill bonuses of equipped items, except the weapons of the set not in hand.
	/// </summary>
	/// <param name="useAlternate">True when the second weapon set is in hand (<see cref="PlayerCollection.UseAlternate"/>).</param>
	IReadOnlyList<SkillAugment> GetGearSkillAugments(IEnumerable<Item> equipment, bool useAlternate);

	/// <summary>
	/// A skill at a level as the game's skill window describes it: name, description and its effects at that level.
	/// </summary>
	/// <param name="context">What the character adds to the skill's own values (modifiers, recharge reduction...).</param>
	/// <returns><see langword="null"/> when the game has no such skill.</returns>
	GrantedSkill? DescribeSkill(RecordId skillId, int level, SkillWindowContext? context = null);
}
