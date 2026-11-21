using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Forge;

namespace TQVaultAE.Application.Results;

/// <summary>
/// Outcome of a forge: the refusal, or the forged item and the material items consumed.
/// </summary>
/// <param name="Problem">Why the selection cannot be forged, or <see langword="null"/> when it was.</param>
/// <param name="Forged">The base item, now forged; <see langword="null"/> when refused.</param>
/// <param name="Consumed">The material items removed from their sacks (hardcore mode).</param>
public sealed record ForgeResult(ForgeMessage? Problem, Item? Forged, IReadOnlyList<Item> Consumed)
{
	/// <summary><see langword="true"/> when the item was forged.</summary>
	public bool IsSuccess => this.Problem is null;

	/// <summary>The selection cannot be forged: nothing changed.</summary>
	public static ForgeResult Refused(ForgeMessage problem) => new(problem, null, []);

	/// <summary>The item was forged.</summary>
	public static ForgeResult Succeeded(Item forged, IReadOnlyList<Item> consumed) => new(null, forged, consumed);
}
