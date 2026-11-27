namespace TQVaultAE.Application.Results;

/// <summary>
/// How a change of a character's read-only state ended.
/// </summary>
public enum ReadOnlyChange
{
	/// <summary>The files carry the asked state (they may have carried it already).</summary>
	Changed,

	/// <summary>
	/// Refused: the character or its stash has changes not saved yet, which a read-only character could never save.
	/// </summary>
	UnsavedChanges,

	/// <summary>An attribute could not be changed (access denied, file locked): see <see cref="ReadOnlyChangeResult.Error"/>.</summary>
	Failed,
}

/// <summary>
/// Outcome of <c>ICharacterReadOnlyService.SetReadOnly</c>.
/// </summary>
/// <param name="Change">How the change ended.</param>
/// <param name="IsReadOnly">The read-only state of the character afterwards, read again from its files.</param>
/// <param name="Error">The failure of <see cref="ReadOnlyChange.Failed"/>; null otherwise.</param>
public sealed record ReadOnlyChangeResult(ReadOnlyChange Change, bool IsReadOnly, Exception? Error = null);
