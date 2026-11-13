using System.Diagnostics.CodeAnalysis;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Results;

public class PlayerLoadResult
{
	/// <summary>
	/// Path to the player file.
	/// </summary>
	public string? PlayerFile { get; set; }

	/// <summary>
	/// The loaded player collection.
	/// </summary>
	public PlayerCollection? Player { get; set; }

	/// <summary>
	/// Indicates whether the player was loaded successfully.
	/// </summary>
	[MemberNotNullWhen(true, nameof(Player))]
	public bool IsSuccess => Player is not null;

	/// <summary>
	/// Any exception that occurred during loading.
	/// </summary>
	public Exception? Error { get; set; }

	/// <summary>
	/// Creates a failed result with the given exception.
	/// </summary>
	public static PlayerLoadResult Failed(Exception ex)
		=> new() { Error = ex };
}
