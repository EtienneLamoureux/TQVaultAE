using System.Diagnostics.CodeAnalysis;

namespace TQVaultAE.Application.Results;

/// <summary>
/// Outcome of a character duplication.
/// </summary>
public sealed class CharacterDuplicateResult
{
	private CharacterDuplicateResult(string? newFolder, Exception? error)
	{
		this.NewFolder = newFolder;
		this.Error = error;
	}

	/// <summary>
	/// The save folder of the new character; null when the duplication failed.
	/// </summary>
	public string? NewFolder { get; }

	/// <summary>
	/// The failure; null on success. The partial copy has been removed.
	/// </summary>
	public Exception? Error { get; }

	/// <summary>
	/// True when the new character's files are in place.
	/// </summary>
	[MemberNotNullWhen(true, nameof(NewFolder))]
	[MemberNotNullWhen(false, nameof(Error))]
	public bool IsSuccess => this.Error is null;

	/// <summary>
	/// Creates a successful result.
	/// </summary>
	public static CharacterDuplicateResult Succeeded(string newFolder)
		=> new(newFolder, null);

	/// <summary>
	/// Creates a failed result.
	/// </summary>
	public static CharacterDuplicateResult Failed(Exception error)
		=> new(null, error);
}
