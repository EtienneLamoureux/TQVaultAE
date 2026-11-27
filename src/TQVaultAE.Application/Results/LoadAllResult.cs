namespace TQVaultAE.Application.Results;

/// <summary>
/// A file that could not be read while loading all the files.
/// </summary>
/// <param name="File">The file, when known.</param>
/// <param name="Error">The read error.</param>
public sealed record LoadAllError(string? File, Exception Error);

/// <summary>
/// Outcome of loading every character, character stash and vault.
/// </summary>
public sealed class LoadAllResult
{
	/// <summary>
	/// Gets whether there was something to load.
	/// </summary>
	public bool Ran { get; init; }

	/// <summary>
	/// Gets the files that could not be read.
	/// </summary>
	public IReadOnlyList<LoadAllError> Errors { get; init; } = [];

	/// <summary>
	/// Gets how long the loading took.
	/// </summary>
	public TimeSpan Elapsed { get; init; }

	/// <summary>
	/// Nothing was loaded.
	/// </summary>
	public static LoadAllResult Nothing { get; } = new();
}
