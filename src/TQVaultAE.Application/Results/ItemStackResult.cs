using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Results;

/// <summary>
/// Result of dropping an item on another one: stacked, or left for a regular drop.
/// </summary>
/// <param name="Stacked">The items were stacked; the caller does not place the dropped item.</param>
/// <param name="DroppedItem">The item that ends in the sack (or is thrown away once merged).</param>
/// <param name="PickedUpItem">The item the caller picks up after the drop, or <see langword="null"/>.</param>
public sealed record ItemStackResult(bool Stacked, Item DroppedItem, Item? PickedUpItem)
{
	/// <summary>No stacking: <paramref name="dragItem"/> is dropped and <paramref name="itemUnderUs"/> picked up as usual.</summary>
	public static ItemStackResult NotStacked(Item dragItem, Item? itemUnderUs)
		=> new(false, dragItem, itemUnderUs);
}
