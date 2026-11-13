using TQVaultAE.Application.Results;

namespace TQVaultAE.Application.Contracts.Services;

/// <summary>
/// The loadouts of the characters (saved equipment sets): a file in each character's save folder holding copies of the
/// pieces the character wore when the loadout was recorded.
/// </summary>
public interface ILoadoutService
{
	/// <summary>
	/// The loadouts file, in the character's save folder next to its player file: it follows the folder when the
	/// character is archived, copied by the game or backed up with git.
	/// </summary>
	const string FileName = "TQVaultAE.Loadouts.json";

	/// <summary>The loadouts file of a character.</summary>
	/// <param name="saveFolder">The character's save folder.</param>
	string FileOf(string saveFolder);

	/// <summary>
	/// Reads the loadouts of a character (none without a file), with the pieces read in the character's map, and keeps
	/// them for the session.
	/// </summary>
	/// <param name="saveFolder">The character's save folder.</param>
	/// <param name="characterName">The character's name.</param>
	/// <param name="characterMod">The character's map, null for the base game.</param>
	LoadoutSet Read(string saveFolder, string characterName, string? characterMod);

	/// <summary>The loadouts read for a character in this session, or null.</summary>
	/// <param name="saveFolder">The character's save folder.</param>
	LoadoutSet? Find(string saveFolder);

	/// <summary>Checks a loadout name.</summary>
	/// <param name="loadouts">The character's loadouts.</param>
	/// <param name="name">The name asked for.</param>
	/// <param name="current">The loadout being renamed, which may keep its name with another case.</param>
	LoadoutNameError ValidateName(LoadoutSet loadouts, string? name, string? current = null);

	/// <summary>Records the worn equipment as a new loadout and writes the file.</summary>
	/// <param name="loadouts">The character's loadouts.</param>
	/// <param name="name">The new loadout's name.</param>
	/// <param name="player">The character, whose equipment and weapon set in hand are recorded.</param>
	LoadoutChange Add(LoadoutSet loadouts, string name, PlayerCollection player);

	/// <summary>Records the worn equipment over a loadout and writes the file.</summary>
	/// <param name="loadouts">The character's loadouts.</param>
	/// <param name="name">The loadout.</param>
	/// <param name="player">The character.</param>
	LoadoutChange Update(LoadoutSet loadouts, string name, PlayerCollection player);

	/// <summary>Renames a loadout and writes the file.</summary>
	/// <param name="loadouts">The character's loadouts.</param>
	/// <param name="name">The loadout.</param>
	/// <param name="newName">Its new name.</param>
	LoadoutChange Rename(LoadoutSet loadouts, string name, string newName);

	/// <summary>Copies a loadout under a new name and writes the file.</summary>
	/// <param name="loadouts">The character's loadouts.</param>
	/// <param name="name">The loadout.</param>
	/// <param name="newName">The copy's name.</param>
	LoadoutChange Copy(LoadoutSet loadouts, string name, string newName);

	/// <summary>Deletes a loadout and writes the file (deleted when no loadout is left).</summary>
	/// <param name="loadouts">The character's loadouts.</param>
	/// <param name="name">The loadout.</param>
	LoadoutChange Delete(LoadoutSet loadouts, string name);
}
