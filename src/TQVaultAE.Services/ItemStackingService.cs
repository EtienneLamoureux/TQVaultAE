using System.Drawing;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Services;

/// <summary>
/// Stacks an item dropped on another one: potions and scrolls, incomplete relics and charms.
/// </summary>
public class ItemStackingService(IItemProvider itemProvider) : IItemStackingService
{
	private readonly IItemProvider _itemProvider = itemProvider;

	/// <inheritdoc/>
	public ItemStackResult TryStack(Item dragItem, Item? itemUnderUs, SackCollection sack, Point dropLocation, bool disableAutoStacking)
	{
		if (itemUnderUs is null || disableAutoStacking || !dragItem.BaseItemId.Equals(itemUnderUs.BaseItemId))
			return ItemStackResult.NotStacked(dragItem, itemUnderUs);

		if (dragItem.DoesStack && itemUnderUs.DoesStack)
			return StackPotions(dragItem, itemUnderUs, sack);

		if (dragItem.IsRelicOrCharm && itemUnderUs.IsRelicOrCharm
			&& !itemUnderUs.IsRelicComplete && !dragItem.IsRelicComplete)
		{
			return this.StackRelics(dragItem, itemUnderUs, sack, dropLocation);
		}

		return ItemStackResult.NotStacked(dragItem, itemUnderUs);
	}

	private static ItemStackResult StackPotions(Item dragItem, Item itemUnderUs, SackCollection sack)
	{
		itemUnderUs.StackSize += dragItem.StackSize;

		// The tooltip shows the new number
		itemUnderUs.IsModified = true;
		sack.IsModified = true;

		// The dragged stack is thrown away.
		return new ItemStackResult(true, dragItem, null);
	}

	private ItemStackResult StackRelics(Item dragItem, Item itemUnderUs, SackCollection sack, Point dropLocation)
	{
		int originalNumber = itemUnderUs.Number;

		// Limited to the completion level of the relic
		itemUnderUs.Number += dragItem.Number;

		if (itemUnderUs.IsRelicComplete)
		{
			RecordId? bonusId = RelicBonus.Roll(this._itemProvider, itemUnderUs);
			if (bonusId is not null)
				itemUnderUs.RelicBonusId = bonusId;

			this._itemProvider.GetDBData(itemUnderUs);
		}

		itemUnderUs.IsModified = true;

		int addedNumber = itemUnderUs.Number - originalNumber;
		if (addedNumber == dragItem.Number)
		{
			// Every shard joined: the dragged relic is thrown away.
			sack.IsModified = true;
			return new ItemStackResult(true, dragItem, null);
		}

		// More shards than needed: the completed relic stays in the sack and the rest is still dragged.
		dragItem.Number -= addedNumber;
		dragItem.IsModified = true;

		// The relic under the drop is still in the sack: adding it again would save it twice.
		itemUnderUs.Location = dropLocation;
		if (sack.IndexOfItem(itemUnderUs) < 0)
			sack.AddItem(itemUnderUs);
		sack.IsModified = true;

		return new ItemStackResult(true, itemUnderUs, dragItem);
	}
}
