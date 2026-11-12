using System.Drawing;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Results;
using TQVaultAE.Config;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Services;

/// <summary>
/// Rules of an item dropped with the mouse on a sack of a vault, a player or a stash.
/// </summary>
public class ItemDropService(
	IItemMovementService itemMovementService
	, IItemStackingService itemStackingService
	, IItemDatabaseService itemDatabaseService
	, IPlayerService playerService
	, UserSettings? userSettings = null
) : IItemDropService
{
	private readonly IItemMovementService _itemMovementService = itemMovementService;
	private readonly IItemStackingService _itemStackingService = itemStackingService;
	private readonly IItemDatabaseService _itemDatabaseService = itemDatabaseService;
	private readonly IPlayerService _playerService = playerService;
	private readonly UserSettings? _userSettings = userSettings;

	/// <inheritdoc/>
	public DropDecision Decide(Item dragItem, Item? itemUnderUs, SackCollection sack, SackType panelSackType, bool canCancelDrag)
	{
		if (sack.IsReadOnly)
			return DropDecision.ReadOnly;

		if (ItemActionRules.IsReadOnly(panelSackType, this._userSettings?.PlayerReadonly ?? false))
			return DropDecision.Refused;

		if (!this._itemMovementService.IsItemValidForStashType(dragItem, sack.StashType))
			return DropDecision.Refused;

		if ((panelSackType == SackType.Player || panelSackType == SackType.Equipment)
			&& !this._playerService.IsSuitableForCurrentPlayer(dragItem))
		{
			return DropDecision.Refused;
		}

		if (itemUnderUs == dragItem && canCancelDrag)
			return DropDecision.PutBack;

		return DropDecision.Drop;
	}

	/// <inheritdoc/>
	public ItemDropResult Drop(Item dragItem, Item? itemUnderUs, SackCollection sack, Point cell, ContainerPlace place, bool disableAutoStacking)
	{
		var stack = this._itemStackingService.TryStack(dragItem, itemUnderUs, sack, cell, disableAutoStacking);
		if (stack.Stacked)
			return new ItemDropResult(true, stack.DroppedItem, stack.PickedUpItem);

		dragItem.Location = cell;
		sack.AddItem(dragItem);

		dragItem.Place.CopyFrom(place);

		// Items already tracked are skipped.
		this._itemDatabaseService.TryAddItemToDatabase(dragItem);

		return new ItemDropResult(false, dragItem, itemUnderUs);
	}

	/// <inheritdoc/>
	public ItemDropResult Equip(Item dragItem, Item? itemUnderUs, SackCollection sack, int slot, Point cell, ContainerPlace place, bool disableAutoStacking)
	{
		var stack = this._itemStackingService.TryStack(dragItem, itemUnderUs, sack, cell, disableAutoStacking);
		if (stack.Stacked)
			return new ItemDropResult(true, stack.DroppedItem, stack.PickedUpItem);

		// The weapon swaps may pick up another item than the one under the drop.
		var pickedUp = EquipmentRules.Equip(sack, dragItem, slot, itemUnderUs);

		dragItem.Place.CopyFrom(place);

		// Items already tracked are skipped.
		this._itemDatabaseService.TryAddItemToDatabase(dragItem);

		return new ItemDropResult(false, dragItem, pickedUp);
	}
}
