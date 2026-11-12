using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Results;

/// <summary>
/// Result of the sort button of a bag.
/// </summary>
/// <param name="Sorted">The items were placed in the new order; <see langword="false"/> when one did not fit and the sack was left as it was.</param>
/// <param name="Changed">The relics and charms the merge gave more shards (their tooltip and picture change).</param>
/// <param name="Removed">The shards the merge emptied, out of the sack and of the search.</param>
public sealed record SackSortResult(bool Sorted, IReadOnlyList<Item> Changed, IReadOnlyList<Item> Removed)
{
	/// <summary>Sorted without any merge.</summary>
	public static SackSortResult SortedOnly { get; } = new(true, [], []);

	/// <summary>An item did not fit: nothing changed.</summary>
	public static SackSortResult NotSorted { get; } = new(false, [], []);
}
