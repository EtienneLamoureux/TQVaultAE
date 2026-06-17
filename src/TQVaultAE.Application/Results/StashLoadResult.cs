using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Results;

public class StashLoadResult
{
	/// <summary>
	/// The loaded stash.
	/// </summary>
	public Stash Stash { get; set; }

	/// <summary>
	/// Path to the stash file.
	/// </summary>
	public string StashFile { get; set; }

	/// <summary>
	/// Indicates whether the stash was loaded successfully.
	/// </summary>
	public bool IsSuccess => Stash != null;

	/// <summary>
	/// Creates a successful result.
	/// </summary>
	public static StashLoadResult Succeeded(Stash stash, string stashFile)
		=> new() { Stash = stash, StashFile = stashFile };

	/// <summary>
	/// Creates an empty failed result.
	/// </summary>
	public static StashLoadResult Empty()
		=> new();
}
