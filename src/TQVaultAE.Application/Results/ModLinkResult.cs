using System.Diagnostics.CodeAnalysis;

namespace TQVaultAE.Application.Results;

/// <summary>
/// Outcome of writing or removing a character's mod link file.
/// </summary>
public sealed class ModLinkResult
{
	private ModLinkResult(Exception? error) => this.Error = error;

	/// <summary>
	/// The failure; null on success.
	/// </summary>
	public Exception? Error { get; }

	/// <summary>
	/// True when the link file is as asked.
	/// </summary>
	[MemberNotNullWhen(false, nameof(Error))]
	public bool IsSuccess => this.Error is null;

	public static readonly ModLinkResult Succeeded = new(null);

	public static ModLinkResult Failed(Exception error) => new(error);
}
