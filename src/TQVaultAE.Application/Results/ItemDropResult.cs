using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Results;

/// <summary>
/// What a drop on a sack does, decided before the dragged item leaves its source sack.
/// </summary>
public enum DropDecision
{
	/// <summary>The item may not go there: nothing happens.</summary>
	Refused,

	/// <summary>
	/// The sack belongs to a read-only character: nothing happens and the user is told why.
	/// </summary>
	ReadOnly,

	/// <summary>The item is put back where it was picked up: the drag is cancelled.</summary>
	PutBack,

	/// <summary>The item is dropped: stacked on the item under it, or placed.</summary>
	Drop,
}

/// <summary>
/// Result of a drop on a sack.
/// </summary>
/// <param name="Stacked">The item was stacked on the item under it instead of being placed.</param>
/// <param name="DroppedItem">The item that ends in the sack (or is thrown away once merged).</param>
/// <param name="ItemUnderUs">The item under the drop after it, possibly an empty item, or <see langword="null"/>.</param>
public sealed record ItemDropResult(bool Stacked, Item DroppedItem, Item? ItemUnderUs);
