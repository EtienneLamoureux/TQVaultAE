using System.Drawing;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Contracts.Services;

/// <summary>
/// Stacks an item dropped on another one: potions and scrolls, incomplete relics and charms.
/// </summary>
public interface IItemStackingService
{
	/// <summary>
	/// Stacks <paramref name="dragItem"/> on <paramref name="itemUnderUs"/> when both are the same stackable item.
	/// </summary>
	/// <remarks>
	/// <list type="bullet">
	/// <item>Potions and scrolls: the whole stack joins the item in the sack.</item>
	/// <item>
	/// Incomplete relics and charms: the shards join the item in the sack up to its completion level; a completed relic
	/// gets a rolled bonus. The extra shards stay in the drag: the completed relic is then put at
	/// <paramref name="dropLocation"/> in the sack and returned as <see cref="ItemStackResult.DroppedItem"/>, and the
	/// rest as <see cref="ItemStackResult.PickedUpItem"/>.
	/// </item>
	/// </list>
	/// </remarks>
	/// <param name="dragItem">The dropped item, already taken out of its source sack.</param>
	/// <param name="itemUnderUs">The item of the sack under the drop, or <see langword="null"/>.</param>
	/// <param name="sack">The sack of the drop.</param>
	/// <param name="dropLocation">The top left cell of the drop.</param>
	/// <param name="disableAutoStacking">The user turned stacking off.</param>
	/// <returns>What the caller places and picks up.</returns>
	ItemStackResult TryStack(Item dragItem, Item? itemUnderUs, SackCollection sack, Point dropLocation, bool disableAutoStacking);
}
