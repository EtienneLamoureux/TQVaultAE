using System.Drawing;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Contracts.Services;

/// <summary>
/// Rules of an item dropped with the mouse on a sack of a vault, a player or a stash.
/// </summary>
/// <remarks>
/// A drop runs in two steps: <see cref="Decide"/>, then, for <see cref="DropDecision.Drop"/>, the caller takes the
/// dragged item out of its source sack and calls <see cref="Drop"/>.
/// </remarks>
public interface IItemDropService
{
	/// <summary>
	/// Decides what a drop does.
	/// </summary>
	/// <remarks>
	/// <see cref="DropDecision.ReadOnly"/> on a sack of a read-only character. Refused when the sack cannot change
	/// otherwise (the equipment with the <c>PlayerReadonly</c> setting on, see <c>ItemActionRules.IsReadOnly</c>), when
	/// the stash does not take this kind of item (the relic vault), or when a player or equipment sack gets an item the
	/// current character cannot use. Put back when dropped on itself and the drag can be cancelled.
	/// </remarks>
	/// <param name="dragItem">The dragged item.</param>
	/// <param name="itemUnderUs">The only item under the drop, or <see langword="null"/>.</param>
	/// <param name="sack">The sack of the drop (its stash type and read-only state are read).</param>
	/// <param name="panelSackType">The sack type of the panel of the drop.</param>
	/// <param name="canCancelDrag">The drag can be cancelled (not a new or modified item).</param>
	DropDecision Decide(Item dragItem, Item? itemUnderUs, SackCollection sack, SackType panelSackType, bool canCancelDrag);

	/// <summary>
	/// Drops an item on a sack: stacks it on the item under it, or places it at <paramref name="cell"/>, moves its
	/// place to <paramref name="place"/> and registers it in the search database.
	/// </summary>
	/// <param name="dragItem">The dropped item, already taken out of its source sack.</param>
	/// <param name="itemUnderUs">The only item under the drop, or <see langword="null"/>.</param>
	/// <param name="sack">The sack of the drop.</param>
	/// <param name="cell">The top left cell of the drop.</param>
	/// <param name="place">Where the sack is: container, sack number and type.</param>
	/// <param name="disableAutoStacking">The user turned stacking off.</param>
	/// <returns>What ends in the sack and what to pick up next.</returns>
	ItemDropResult Drop(Item dragItem, Item? itemUnderUs, SackCollection sack, Point cell, ContainerPlace place, bool disableAutoStacking);

	/// <summary>
	/// Drops an item on a slot of the equipment: stacks it on the item under it like <see cref="Drop"/>, or equips it
	/// with the slot rules of <c>EquipmentRules.Equip</c> (two-handed weapon swaps, shadow slots), moves its place to
	/// <paramref name="place"/> and registers it in the search database.
	/// </summary>
	/// <param name="dragItem">The dropped item, already taken out of its source sack.</param>
	/// <param name="itemUnderUs">The only item under the drop (for an empty shadow slot, its two-handed weapon), or
	/// <see langword="null"/>.</param>
	/// <param name="sack">The equipment sack.</param>
	/// <param name="slot">The slot of the drop, accepted by <c>EquipmentRules.CanEquip</c>.</param>
	/// <param name="cell">The top left cell of the drop, for the stacking.</param>
	/// <param name="place">Where the equipment is: the character, its equipment sack.</param>
	/// <param name="disableAutoStacking">The user turned stacking off.</param>
	/// <returns>What ends in the slot and what to pick up next (possibly an empty item).</returns>
	ItemDropResult Equip(Item dragItem, Item? itemUnderUs, SackCollection sack, int slot, Point cell, ContainerPlace place, bool disableAutoStacking);
}
