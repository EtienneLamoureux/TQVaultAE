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

	/// <summary>Records the worn equipment as a new loadout and writes the file.</summary>
	/// <param name="loadouts">The character's loadouts.</param>
	/// <param name="name">The new loadout's name.</param>
	/// <param name="player">The character, whose equipment and weapon set in hand are recorded.</param>
	LoadoutChange Add(LoadoutSet loadouts, string name, PlayerCollection player);

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
	/// Wears a loadout: a worn piece identical to a loadout piece stays worn (moved to its loadout slot when worn in
	/// another), the other loadout pieces are copied in, and the worn pieces the loadout does not use go to the bag. The
	/// loadout's weapon set is put in hand. Without room in the bag for every piece taken off, nothing changes. No
	/// requirement is checked: the game keeps a piece out of reach in its slot and ignores its bonuses.
	/// </summary>
	/// <param name="loadouts">The character's loadouts.</param>
	/// <param name="name">The loadout.</param>
	/// <param name="player">The character.</param>
	/// <param name="bag">The vault bag receiving the pieces taken off.</param>
	/// <param name="bagSize">Its size in cells.</param>
	/// <param name="bagPlace">Its place, given to the pieces stored (the search follows them).</param>
	LoadoutLoadResult Load(LoadoutSet loadouts, string name, PlayerCollection player, Domain.Entities.SackCollection bag, System.Drawing.Size bagSize, Domain.Entities.ContainerPlace bagPlace);

	/// <summary>
	/// Wears a loadout as <see cref="Load"/> does, but the worn pieces the loadout does not use are deleted instead of
	/// stored (a loadout keeps copies: this saves piling up duplicates). They leave the search too.
	/// </summary>
	/// <param name="loadouts">The character's loadouts.</param>
	/// <param name="name">The loadout.</param>
	/// <param name="player">The character.</param>
	LoadoutLoadResult LoadReplacing(LoadoutSet loadouts, string name, PlayerCollection player);

	/// <summary>
	/// Gear swap: exchanges the loadout's pieces and the worn ones, slot by slot, and the weapon set in hand. Every item
	/// stays one item, never copied: the loadout's pieces go on the character, the worn pieces take their place in the
	/// loadout; a slot empty on one side becomes empty on the other. Swapping again swaps back. The character's equipment
	/// changes as for a load (saved with it); the loadouts are marked modified and written with the other files. The
	/// pieces keep the character's map, a slot holds any size. The search follows both sides.
	/// </summary>
	/// <param name="loadouts">The character's loadouts.</param>
	/// <param name="name">The loadout.</param>
	/// <param name="player">The character.</param>
	LoadoutSwapResult Swap(LoadoutSet loadouts, string name, PlayerCollection player);

	/// <summary>
	/// The worn pieces a load of the loadout would take off, in slot order (none when the loadout or the equipment is
	/// missing): what <see cref="LoadReplacing"/> deletes, to show before it.
	/// </summary>
	/// <param name="loadouts">The character's loadouts.</param>
	/// <param name="name">The loadout.</param>
	/// <param name="player">The character.</param>
	IReadOnlyList<Domain.Entities.Item> TakenOff(LoadoutSet loadouts, string name, PlayerCollection player);

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
