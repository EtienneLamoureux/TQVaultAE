//-----------------------------------------------------------------------
// <copyright file="EquipmentPanel.DragDrop.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
/// <summary>
/// Class for holding all of the UI functions of the sack panel in the stash panel.
/// </summary>

using Microsoft.Extensions.Logging;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Results;
using TQVaultAE.Config;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Models;
using TQVaultAE.GUI.Tooltips;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Components.Panels;

public partial class EquipmentPanel
{
	/// <summary>
	/// Cancels an item drag.  Takes into account the shadow slot for the weapon panel.
	/// </summary>
	/// <param name="dragInfo">ItemDragInfo instance</param>
	public override void CancelDrag(ItemDragInfo dragInfo)
	{
		base.CancelDrag(dragInfo);

		// Check to see if we need to restore the shadow slot.
		if (this.Sack == dragInfo.SrcSack && dragInfo.Item is { Is2HWeapon: true } dragged)
		{
			try
			{
				if (this.GetItemFromShadowSlot(dragged.PositionY) is { } new2HItem)
					this.Invalidate(this.FindSlotRect(new2HItem.Location.X, new2HItem.Location.Y));
			}
			catch (NullReferenceException exception)
			{
				Log.LogError(exception, "Failed to restore the shadow slot of the two-handed weapon {Item}", dragged.BaseItemId);
				this.Services.MessageBoxes.ShowException(exception);
			}
		}
	}

	/// <summary>
	/// Calculates the mouse offset in screen coordinates within an item bitmap.
	/// </summary>
	/// <param name="location">Point containing the screen coordinates of the mouse.</param>
	/// <param name="item">Item that we are over</param>
	/// <returns>Point containing the mouse offset.</returns>
	protected override Point GetMouseOffset(Point location, Item item)
	{
		if (item is null)
			return Point.Empty;

		Point topLeft = this.CellTopLeft(item.Location);

		// Adjust for weapon panel items
		if (item.IsInWeaponSlot)
		{
			int itemOffset = item.PositionY;
			if (item.Is2HWeapon)
			{
				// We need to adjust, just in case we clicked on the shadow item of a 2H item.
				int focusSlot = FindEquipmentSlot(this.FindCell(location));
				int itemSlot = EquipmentRules.FindSlot(this.ShownSack, item);
				if (focusSlot != itemSlot)
					itemOffset = SackCollection.GetEquipmentLocationOffset(focusSlot).Y;
			}

			topLeft = WeaponTopLeft(
				SackCollection.GetWeaponLocationOffset(itemOffset).X,
				SackCollection.GetWeaponLocationOffset(itemOffset).Y,
				item.Width,
				item.Height);
		}

		return new Point(location.X - topLeft.X, location.Y - topLeft.Y);
	}

	/// <summary>
	/// Handler for picking up an item with the mouse
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">MouseEventArgs data</param>
	protected override void PickupItem(object? sender, MouseEventArgs e)
	{
		base.PickupItem(sender, e);

		// Recalculate the character stat bonuses.
		GetGearStatBonus();
	}

	/// <summary>
	/// Handler for putting an item down with the mouse
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">MouseEventArgs data</param>
	protected override void TryToPutdownItem(object? sender, MouseEventArgs e)
	{
		// process mouse moving to this location
		this.MouseMoveCallback(sender, e);

		// Make sure we have room to place the item here
		// we do this by making sure that:
		// (a) there are cells under us
		// (b) there is <= 1 items under us
		if (!this.CellsUnderDragItem.Size.IsEmpty && this.ItemsUnderDragItem.Count <= 1)
		{
			if (this.DragInfo.Item is not { } dragItem)
				return;

			// Check that the item is being dropped into the proper slot.
			int slot = FindEquipmentSlot(this.CellsUnderDragItem);
			if (slot == -1 || !EquipmentRules.CanEquip(this.ShownSack, dragItem, slot))
				return;

			// The only item under us, if any
			Item? itemUnderUs = null;
			if (this.ItemsUnderDragItem.Count == 1)
				itemUnderUs = this.ItemsUnderDragItem[0];

			// Refused when the character cannot use the item.
			var decision = this.Services.ItemDropService.Decide(
				dragItem, itemUnderUs, this.SackType, this.Sack?.StashType, this.DragInfo.IsActive && this.DragInfo.CanBeCanceled);
			if (decision == DropDecision.Refused)
				return;

			// If the requirement setting is enabled check to see if the item can be equipped.
			if (this.USettings.EnableItemRequirementRestriction && !this.PlayerMeetRequierements(dragItem))
				return;

			// Maybe we are putting the item back or dropping on an equipment shadow slot.
			// Then we just cancel.
			if (decision == DropDecision.PutBack)
			{
				this.DragInfo.Cancel();

				// Now redraw this sack
				this.Refresh();
				this.MouseMoveCallback(sender, e);
				return;
			}

			// Notify that the item has been placed.  This will remove it from the old sack				
			this.DragInfo.MarkPlaced();

			// Stack on the item under us when both are the same stackable, otherwise equip the item: the weapon swaps
			// may pick up another item.
			var drop = this.Services.ItemDropService.Equip(
				dragItem, itemUnderUs, this.ShownSack, slot, this.CellsUnderDragItem.Location, this.PlaceHere(), USettings.DisableAutoStacking);

			if (drop.Stacked)
				this.InvalidateItemCacheAll(itemUnderUs, dragItem);

			dragItem = drop.DroppedItem;
			itemUnderUs = drop.ItemUnderUs;

			// Equipped, an item without an own map inherits the character's; a slot holds any size.
			if (!drop.Stacked
				&& this.Services.ItemEditService.SettleIn(dragItem, false, this.Services.GameMapService.ActiveCharacterMod, _ => true) != SettleOutcome.Unchanged)
				this.InvalidateItemCacheAll(dragItem);

			this.PickUpAfterDrop(itemUnderUs);

			// Repaint everything to clear up any graphical issues
			this.Refresh();

			// and now do a MouseMove() to properly draw the new drag item and/or focus
			this.MouseMoveCallback(sender, e);

			// Recalculate the character stat bonuses.
			GetGearStatBonus();

			ItemTooltip.HideTooltip();
			BagButtonTooltip.InvalidateCache(this.Sack);
		}
	}

	/// <summary>
	/// Gets a rectangle of the screen coordinates for all of the items that need to be redrawn from an item drag.
	/// </summary>
	/// <returns>Rectangle of the redraw area.</returns>
	protected override Rectangle GetRepaintDragRect()
	{
		// Figure out the rectangle that needs to be redrawn
		int x = this.LastDragLocation.X;
		int y = this.LastDragLocation.Y;
		if (this.DragInfo.Item is not { } dragged || this.UIService.GetBitmap(dragged) is not { } ibmp)
			return Rectangle.Empty;

		int width = Convert.ToInt32(ibmp.Width * UIService.Scale);
		int height = Convert.ToInt32(ibmp.Height * UIService.Scale);

		// We also know we need to wipe out any cells under the old drag point
		// This is used to restore the background after highlighting the cells underneath
		Rectangle cellRectangle = this.FindAllCells(new Rectangle(x, y, width, height));
		if (!cellRectangle.Size.IsEmpty)
		{
			Point tl = this.CellTopLeft(cellRectangle.Location);
			Point br = this.CellBottomRight(new Point(cellRectangle.Right - 1, cellRectangle.Bottom - 1));

			// Extend our clipping rectangle to include all these cells
			int oldX = x;
			int oldY = y;
			x = Math.Min(x, tl.X);
			y = Math.Min(y, tl.Y);
			width = Math.Max(oldX + width - 1, br.X) - x + 1;
			height = Math.Max(oldY + height - 1, br.Y) - y + 1;

			// Find the slot which is under the drag item
			Rectangle rect = this.FindAllEquipmentCells(cellRectangle);
			if (!rect.IsEmpty)
			{
				Point rectUL = this.CellTopLeft(rect.Location);
				Point rectBR = this.CellBottomRight(new Point(rect.Right - 1, rect.Bottom - 1));

				// Extend our clipping rectangle to include all these cells
				int oldX1 = x;
				int oldY1 = y;
				x = Math.Min(x, rectUL.X);
				y = Math.Min(y, rectUL.Y);
				width = Math.Max(oldX1 + width - 1, rectBR.X) - x + 1;
				height = Math.Max(oldY1 + height - 1, rectBR.Y) - y + 1;
			}
		}

		// We also need to wipe out any items that were under or are now under the dragItem so they can be redrawn
		if (this.ItemsUnderOldDragLocation is not null)
		{
			foreach (Item item in this.ItemsUnderOldDragLocation)
			{
				// well we do not actually need to clear the item and redraw it since that will happen automatically
				// in the PaintCallback().  We just need to include this item in the clip region
				Point tl = this.CellTopLeft(item.Location);
				Point br = this.CellBottomRight(new Point(item.Right - 1, item.Bottom - 1));

				if (item.IsInWeaponSlot)
				{
					int itemOffsetR, itemOffsetL;
					if (item.Is2HWeapon)
					{
						// We need to redraw both items
						itemOffsetL = (item.PositionY / 2) * 2;
						itemOffsetR = ((item.PositionY / 2) * 2) + 1;
					}
					else
					{
						// Only include the item under the box
						itemOffsetL = item.PositionY;
						itemOffsetR = item.PositionY;
					}

					// Get the left most item
					Point itemL = SackCollection.GetWeaponLocationOffset(itemOffsetL);

					// Now get the right most item and create a rectangle to cover both
					Point itemR = SackCollection.GetWeaponLocationOffset(itemOffsetR);

					// we do not need the weapon box drawing offsets here e.g. WeaponTopLeft()
					tl = this.CellTopLeft(itemL);
					br = this.CellBottomRight(new Point(itemR.X + SackCollection.WeaponLocationSize.Width - 1, itemR.Y + SackCollection.WeaponLocationSize.Height - 1));
				}

				// Extend our clipping rectangle to include all these cells
				int oldX = x;
				int oldY = y;
				x = Math.Min(x, tl.X);
				y = Math.Min(y, tl.Y);
				width = Math.Max(oldX + width - 1, br.X) - x + 1;
				height = Math.Max(oldY + height - 1, br.Y) - y + 1;
			}
		}

		if (this.ItemsUnderDragItem is not null)
		{
			foreach (Item item in this.ItemsUnderDragItem)
			{
				// well we do not actually need to clear the item and redraw it since that will happen automatically
				// in the PaintCallback().  We just need to include this item in the clip region
				Point tl = this.CellTopLeft(item.Location);
				Point br = this.CellBottomRight(new Point(item.Right - 1, item.Bottom - 1));

				if (item.IsInWeaponSlot)
				{
					int itemOffsetR, itemOffsetL;
					if (item.Is2HWeapon)
					{
						// We need to redraw both items
						itemOffsetL = (item.PositionY / 2) * 2;
						itemOffsetR = ((item.PositionY / 2) * 2) + 1;
					}
					else
					{
						// Only include the item under the box
						itemOffsetL = item.PositionY;
						itemOffsetR = item.PositionY;
					}

					// Get the left most item
					Point itemL = SackCollection.GetWeaponLocationOffset(itemOffsetL);

					// Now get the right most item and create a rectangle to cover both
					Point itemR = SackCollection.GetWeaponLocationOffset(itemOffsetR);

					// we do not need the weapon box drawing offsets here e.g. WeaponTopLeft()
					tl = this.CellTopLeft(itemL);
					br = this.CellBottomRight(new Point(itemR.X + SackCollection.WeaponLocationSize.Width - 1, itemR.Y + SackCollection.WeaponLocationSize.Height - 1));
				}

				// Extend our clipping rectangle to include all these cells
				int oldX = x;
				int oldY = y;
				x = Math.Min(x, tl.X);
				y = Math.Min(y, tl.Y);
				width = Math.Max(oldX + width - 1, br.X) - x + 1;
				height = Math.Max(oldY + height - 1, br.Y) - y + 1;
			}
		}

		return new Rectangle(x, y, width, height);
	}
}
