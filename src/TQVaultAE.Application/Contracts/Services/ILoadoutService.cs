using TQVaultAE.Application.Results;
using TQVaultAE.Application.Session;

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

	/// <summary>Adds an empty loadout and writes the file.</summary>
	/// <param name="loadouts">The character's loadouts.</param>
	/// <param name="name">The new loadout's name.</param>
	LoadoutChange Add(LoadoutSet loadouts, string name);

	/// <summary>
	/// Merges the worn equipment into a loadout (copies of the worn pieces replace the loadout's, slots empty on the
	/// character keep the loadout's piece; see <c>EquipmentRules.MergeWorn</c>), records the weapon set in hand and
	/// writes the file.
	/// </summary>
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

	/// <summary>
	/// Gear swap: exchanges the loadout's pieces and the worn ones, slot by slot, and the weapon set in hand. Every item
	/// stays one item, never copied: the loadout's pieces go on the character, the worn pieces take their place in the
	/// loadout; a slot empty on one side becomes empty on the other. Swapping again swaps back. The character's equipment
	/// changes (saved with it); the loadouts are marked modified and written with the other files. The
	/// pieces keep the character's map, a slot holds any size. The search follows both sides.
	/// </summary>
	/// <param name="loadouts">The character's loadouts.</param>
	/// <param name="name">The loadout.</param>
	/// <param name="player">The character.</param>
	LoadoutSwapResult Swap(LoadoutSet loadouts, string name, PlayerCollection player);

	/// <summary>
	/// Moves the pieces of a loadout to a bag, one by one in slot order at the first free spot as an auto-move places
	/// them; the loadout keeps its name. The pieces themselves move, none is copied. A piece without room (or that the bag
	/// does not take) stays in the loadout. The bag is modified (saved with its file) and the loadouts are marked modified
	/// (written with the other files). The search follows the pieces.
	/// </summary>
	/// <param name="loadouts">The character's loadouts.</param>
	/// <param name="name">The loadout.</param>
	/// <param name="bag">The bag receiving the pieces: inventory, stash or vault bag.</param>
	/// <param name="bagSize">Its size in cells.</param>
	/// <param name="bagPlace">Its place, given to the pieces (the search follows them).</param>
	LoadoutEmptyResult Empty(LoadoutSet loadouts, string name, Domain.Entities.SackCollection bag, System.Drawing.Size bagSize, Domain.Entities.ContainerPlace bagPlace);

	/// <summary>
	/// Takes pieces out of their loadouts (their slots become empty) and writes the file: the results window moved them
	/// elsewhere. A loadout keeps its name and its other pieces, even when none is left.
	/// </summary>
	/// <param name="loadouts">The character's loadouts.</param>
	/// <param name="pieces">The pieces, found by reference.</param>
	LoadoutChange TakePieces(LoadoutSet loadouts, IReadOnlyCollection<Domain.Entities.Item> pieces);

	/// <summary>
	/// Puts pieces back in loadout slots and writes the file: the results window undoes their move. A piece whose loadout
	/// is gone or whose slot is taken is left out.
	/// </summary>
	/// <param name="loadouts">The character's loadouts.</param>
	/// <param name="pieces">The pieces and their slots.</param>
	/// <returns>The change, and the pieces put back.</returns>
	(LoadoutChange Change, IReadOnlyList<Domain.Entities.Item> PutBack) PutBackPieces(LoadoutSet loadouts, IReadOnlyList<LoadoutPiecePlacement> pieces);

	/// <summary>
	/// Gives a loadout the pieces composed in the loadout panel, in memory: the loadouts are marked modified and written
	/// with the other modified files (<see cref="SaveModified"/>), or by the next change written at once (add, update,
	/// rename, copy, delete). The search follows the pieces at once.
	/// </summary>
	/// <param name="loadouts">The character's loadouts.</param>
	/// <param name="name">The loadout.</param>
	/// <param name="pieces">One entry per equipment slot, <see langword="null"/> for an empty slot; the items themselves are
	/// kept (moved, not copied).</param>
	/// <returns>True when a slot changed; false when the pieces are the same items as before, or the loadout is missing.</returns>
	bool Compose(LoadoutSet loadouts, string name, IReadOnlyList<Domain.Entities.Item?> pieces);

	/// <summary>The loadouts read in this session whose pieces changed and are not written yet.</summary>
	IReadOnlyList<LoadoutSet> GetModified();

	/// <summary>
	/// Writes modified loadouts to their file (deleted when no loadout is left), as a save writes a character.
	/// </summary>
	/// <param name="loadouts">The character's loadouts.</param>
	/// <exception cref="IOException">The file could not be written.</exception>
	/// <exception cref="UnauthorizedAccessException">The file or its folder is read-only or protected.</exception>
	void SaveModified(LoadoutSet loadouts);
}
