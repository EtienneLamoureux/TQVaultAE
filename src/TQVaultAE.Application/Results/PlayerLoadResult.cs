using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Results;

public class PlayerLoadResult
{
	/// <summary>
	/// Path to the player file.
	/// </summary>
	public string PlayerFile { get; set; }

	/// <summary>
	/// The loaded player collection.
	/// </summary>
	public PlayerCollection Player { get; set; }

	/// <summary>
	/// The player's stash.
	/// </summary>
	public Stash PlayerStash { get; set; }

	/// <summary>
	/// The player's stash file path.
	/// </summary>
	public string PlayerStashFile { get; set; }

	/// <summary>
	/// The transfer stash (Immortal Throne).
	/// </summary>
	public Stash TransferStash { get; set; }

	/// <summary>
	/// The relic vault stash.
	/// </summary>
	public Stash RelicVaultStash { get; set; }

	/// <summary>
	/// Indicates whether the player was loaded successfully.
	/// </summary>
	public bool IsSuccess => Player != null;

	/// <summary>
	/// Any exception that occurred during loading.
	/// </summary>
	public Exception Error { get; set; }

	/// <summary>
	/// Creates a failed result with the given exception.
	/// </summary>
	public static PlayerLoadResult Failed(Exception ex)
		=> new() { Error = ex };
}
