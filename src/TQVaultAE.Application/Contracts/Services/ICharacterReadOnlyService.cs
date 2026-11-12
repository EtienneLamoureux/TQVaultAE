using TQVaultAE.Application.Results;

namespace TQVaultAE.Application.Contracts.Services;

/// <summary>
/// The read-only state of a character, kept by the file system: a character is read-only when one of the files TQVault
/// writes for it in its save folder (<see cref="OwnedFiles"/>: the character file, its own stash and the mod link file)
/// carries the read-only attribute. A read-only character cannot change in any way from TQVault (see
/// <c>ItemActionRules.IsReadOnly</c>) and the save skips it. The transfer stash and the relic vault belong to every
/// character, so they are never part of it.
/// </summary>
public interface ICharacterReadOnlyService
{
	/// <summary>
	/// The files TQVault writes for the character of <paramref name="saveFolder"/>, existing or not: the character file
	/// (<c>Player.chr</c>), its stash (<c>winsys.dxb</c> and <c>winsys.dxg</c>) and the mod link file.
	/// </summary>
	IReadOnlyList<string> OwnedFiles(string saveFolder);

	/// <summary>
	/// Whether the character of <paramref name="saveFolder"/> is read-only: one of its <see cref="OwnedFiles"/> that
	/// exists carries the read-only attribute. Read from the disk at each call.
	/// </summary>
	bool IsReadOnly(string saveFolder);

	/// <summary>
	/// Makes the character read-only, or writable again: sets or clears the read-only attribute of every owned file
	/// that exists, then marks the character and its stash already read (<see cref="PlayerCollection.IsReadOnly"/>,
	/// <c>Stash.IsReadOnly</c>) and <see cref="PlayerSave.IsReadOnly"/> with the state read again from the files.
	/// Making it read-only is refused while the character or its stash has unsaved changes.
	/// </summary>
	ReadOnlyChangeResult SetReadOnly(PlayerSave save, bool makeReadOnly);

	/// <summary>
	/// Clears the read-only attribute of the owned files of <paramref name="saveFolder"/>, without anything else: for
	/// a copy of a read-only character, which starts writable.
	/// </summary>
	void ClearReadOnly(string saveFolder);
}
