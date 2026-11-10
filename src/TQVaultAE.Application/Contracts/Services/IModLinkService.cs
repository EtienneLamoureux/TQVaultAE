using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Results;

namespace TQVaultAE.Application.Contracts.Services;

/// <summary>
/// Links custom map characters to installed mods: the save files do not say which mod they belong to, so the choice is
/// kept in a small JSON file (<see cref="LinkFileName"/>) in the character's save folder.
/// </summary>
public interface IModLinkService
{
	/// <summary>
	/// Name of the link file in a character's save folder. The game never writes a file with this name.
	/// </summary>
	public const string LinkFileName = "TQVaultAE.ModLink.json";

	/// <summary>
	/// The mods installed on this computer (legacy custom maps and Steam Workshop items), read once.
	/// </summary>
	/// <param name="refresh">Reads the folders again.</param>
	IReadOnlyList<InstalledMod> GetInstalledMods(bool refresh = false);

	/// <summary>
	/// Reads the link file of a save folder.
	/// </summary>
	/// <returns>The link; null when there is none or it cannot be read.</returns>
	ModLinkFile? ReadLink(string saveFolder);

	/// <summary>
	/// Resolves the mod of a character: none for a main campaign character, else its link and the installed mod it
	/// names (by folder first, then by name).
	/// </summary>
	/// <param name="saveFolder">The character's save folder.</param>
	/// <param name="isCustomCharacter">True for a custom map character (<c>SaveData\User</c>).</param>
	CharacterMod Resolve(string saveFolder, bool isCustomCharacter);

	/// <summary>
	/// Links a character to a mod, replacing any previous link.
	/// </summary>
	ModLinkResult Link(string saveFolder, InstalledMod installedMod);

	/// <summary>
	/// Removes the link of a character. Succeeds when there was none.
	/// </summary>
	ModLinkResult Unlink(string saveFolder);

	/// <summary>
	/// Suggests the mod of a character from the records its save files use that the base game does not have.
	/// Never links anything.
	/// </summary>
	/// <param name="saveFolder">The character's save folder.</param>
	ModSuggestion Suggest(string saveFolder);

	/// <summary>
	/// Suggests the mod of some records (an item's, a vault's): the installed mods having those the base game does not
	/// have. What each record was found in is kept for the session, so asking again is instant.
	/// </summary>
	/// <param name="records">Record ids, as saved (<c>records\...\x.dbr</c>).</param>
	ModSuggestion SuggestFor(IEnumerable<string> records);
}
