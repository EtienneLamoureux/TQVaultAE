namespace TQVaultAE.Application.Results;

/// <summary>
/// A step of the save of the modified files, in the order they run.
/// </summary>
public enum SaveStep
{
	Players,
	Vaults,
	Stashes,
}

/// <summary>
/// What to do after a file of a save step could not be written.
/// </summary>
public enum SaveDecision
{
	/// <summary>Stops the whole save: the file error is rethrown.</summary>
	Abort,

	/// <summary>Writes the failed file again; the files already saved are not written again.</summary>
	Retry,

	/// <summary>Gives up the failed file, which stays unsaved, and goes on with the next files.</summary>
	Ignore,
}

/// <summary>
/// A file that could not be written during a save.
/// </summary>
/// <param name="Step">The step that failed.</param>
/// <param name="Name">The name of the character, vault or stash being saved.</param>
/// <param name="Exception">The file error: an <see cref="IOException"/> (file locked, disk full, path not found)
/// or an <see cref="UnauthorizedAccessException"/> (read-only file, no permission, protected folder).</param>
public sealed record SaveError(SaveStep Step, string? Name, Exception Exception);

/// <summary>
/// The state of the session written to the user settings after the files.
/// </summary>
/// <param name="VaultName">The vault shown in the main panel, or null.</param>
/// <param name="CharacterName">The selected character, an empty string when none is selected, or null.</param>
/// <param name="Zoom">The current UI zoom.</param>
public sealed record SessionConfiguration(string? VaultName, string? CharacterName, float Zoom);

/// <summary>
/// What a save writes.
/// </summary>
/// <param name="Files">Saves the modified characters, vaults and stashes.</param>
/// <param name="Configuration">Saves this session state to the user settings when not null.</param>
/// <param name="GitBackup">Commits and pushes the git backup, when it is enabled in the settings.</param>
public sealed record SaveRequest(bool Files, SessionConfiguration? Configuration = null, bool GitBackup = false)
{
	/// <summary>
	/// Saves the modified characters, vaults and stashes only.
	/// </summary>
	public static SaveRequest FilesOnly { get; } = new(true);
}

/// <summary>
/// Outcome of a save.
/// </summary>
public sealed class SaveResult
{
	/// <summary>
	/// Gets whether characters were saved. False when every modified character failed and was ignored.
	/// </summary>
	public bool PlayersSaved { get; init; }

	/// <summary>
	/// Gets the number of vaults saved; the ignored ones are not counted.
	/// </summary>
	public int VaultsSaved { get; init; }

	/// <summary>
	/// Gets the number of stashes saved; the ignored ones are not counted.
	/// </summary>
	public int StashesSaved { get; init; }

	/// <summary>
	/// Gets whether a character, a vault or a stash was saved.
	/// </summary>
	public bool AnythingSaved => this.PlayersSaved || this.VaultsSaved > 0 || this.StashesSaved > 0;

	/// <summary>
	/// Gets the file errors, one per failed attempt, in the order they happened.
	/// </summary>
	public IReadOnlyList<SaveError> Errors { get; init; } = [];

	/// <summary>
	/// Gets whether the configuration save wrote the settings, or null when the configuration was not saved.
	/// </summary>
	public bool? ConfigChanged { get; init; }

	/// <summary>
	/// Gets whether the git backup was committed and pushed.
	/// </summary>
	public bool GitPushed { get; init; }
}
