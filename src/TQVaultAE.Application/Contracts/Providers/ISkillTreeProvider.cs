using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Contracts.Providers;

/// <summary>
/// Reads the masteries and their skills from the game database, as the game's skill window lays them out.
/// </summary>
public interface ISkillTreeProvider
{
	/// <summary>
	/// Gets a mastery and the skills of its pane.
	/// </summary>
	/// <returns><see langword="null"/> when the game database has no pane for the mastery, like Neidan without Eternal Embers.</returns>
	MasteryTree? GetMasteryTree(Masteries mastery);
}
