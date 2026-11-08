//-----------------------------------------------------------------------
// <copyright file="SackPanel.DragDrop.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Extensions.Logging;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Results;
using TQVaultAE.Config;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Components.Rendering;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Forms;
using TQVaultAE.GUI.Models;
using TQVaultAE.GUI.Tooltips;
using TQVaultAE.Presentation;
using TQVaultAE.Presentation.Win32;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.GUI.Components.Panels;

public partial class SackPanel
{
	/// <summary>
	/// Cancels an item drag
	/// </summary>
	/// <param name="dragInfo">ItemDragInfo instance</param>
	public virtual void CancelDrag(ItemDragInfo dragInfo)
	{
		// if the drag sack is not visible then we really do not need to do anything
		if (this.Sack == dragInfo.SrcSack)
			this.Invalidate(this.GetItemScreenRectangle(dragInfo.Item));
	}

	/// <summary>
	/// Handler for picking up an item with the mouse
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">MouseEventArgs data</param>
	protected virtual void PickupItem(object? sender, MouseEventArgs e)
	{
		// process the mouse moving to this location...just in case
		this.MouseMoveCallback(sender, e);

		Item? focusedItem = this.FindItem(this.LastCellWithFocus);
		if (focusedItem is not null)
		{
			// We picked up an item so clear the selection
			if (this.selectedItems is not null)
			{
				// We might be over the only selected item so no need to redraw the whole sack.
				if (this.selectedItems.Count != 1 || focusedItem != this.selectedItems[0])
					this.Invalidate();

				this.ClearSelection();
			}

			// Send a message to clear selections in the other panels.
			this.OnClearAllItemsSelected?.Invoke(this, new SackPanelEventArgs(null, null));

			// pick up item.
			this.DragInfo.Set(this, this.ShownSack, focusedItem, this.GetMouseOffset(e.Location, focusedItem));

			// process mouse move again to apply the graphical effects of dragging an item.
			this.MouseMoveCallback(sender, e);

			ItemTooltip.HideTooltip();
			BagButtonTooltip.InvalidateCache(this.Sack);
		}
	}

	/// <summary>
	/// Calculates the mouse offset in screen coordinates within an item bitmap.
	/// </summary>
	/// <param name="location">Point containing the screen coordinates of the mouse.</param>
	/// <param name="item">Item that we are over</param>
	/// <returns>Point containing the mouse offset.</returns>
	protected virtual Point GetMouseOffset(Point location, Item item)
	{
		if (item is null)
			return Point.Empty;

		Point topLeft = this.CellTopLeft(item.Location);
		return new Point(location.X - topLeft.X, location.Y - topLeft.Y);
	}

	/// <summary>
	/// Handler for putting an item down with the mouse
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">MouseEventArgs data</param>
	protected virtual void TryToPutdownItem(object? sender, MouseEventArgs e)
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

			// The only item under us, if any
			Item? itemUnderUs = null;
			if (this.ItemsUnderDragItem.Count == 1)
				itemUnderUs = this.ItemsUnderDragItem[0];

			var decision = this.Services.ItemDropService.Decide(
				dragItem, itemUnderUs, this.SackType, this.sack?.StashType, this.DragInfo.IsActive && this.DragInfo.CanBeCanceled);

			if (decision == DropDecision.Refused)
				return;

			// We are putting the item back: we just cancel.
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

			// Stack on the item under us when both are the same stackable, otherwise place the item here.
			var drop = this.Services.ItemDropService.Drop(
				dragItem, itemUnderUs, this.ShownSack, this.CellsUnderDragItem.Location, this.PlaceHere(), USettings.DisableAutoStacking);

			if (drop.Stacked)
				this.InvalidateItemCacheAll(itemUnderUs, dragItem);

			dragItem = drop.DroppedItem;
			itemUnderUs = drop.ItemUnderUs;

			// clear the "last drag" variables
			this.LastDragLocation = InvalidDragLocation;
			this.CellsUnderDragItem = InvalidDragRectangle;
			this.ItemsUnderOldDragLocation.Clear();
			this.ItemsUnderDragItem.Clear();

			// Now mark itemUnderUs as picked up.
			if (itemUnderUs is not null && !RecordId.IsNullOrEmpty(itemUnderUs.BaseItemId))
			{
				// set our mouse offset to be the center of the item.
				Point mouseOffset = new Point(
					itemUnderUs.Width * UIService.HalfUnitSize,
					itemUnderUs.Height * UIService.HalfUnitSize);
				this.DragInfo.Set(this, this.ShownSack, itemUnderUs, mouseOffset);

				// since we have dropped something in this location, we can no longer put this item here.
				// mark its location as invalid to prevent that from happening.
				itemUnderUs.Location = InvalidDragLocation;

				// set the info to this item
				this.OnNewItemHighlighted?.Invoke(this, new SackPanelEventArgs(this.Sack, itemUnderUs));
			}

			// clear the "last focus variables"
			this.LastCellWithFocus = InvalidDragLocation;

			// Clear any selections.
			this.OnClearAllItemsSelected?.Invoke(this, new SackPanelEventArgs(null, null));

			this.InvalidateItemCacheAll(itemUnderUs, dragItem);

			// Repaint everything to clear up any graphical issues
			this.Refresh();

			// and now do a MouseMove() to properly draw the new drag item and/or focus
			this.MouseMoveCallback(sender, e);

			ItemTooltip.HideTooltip();
			BagButtonTooltip.InvalidateCache(this.Sack);

		}
	}

	/// <summary>
	/// Stacks the dropped item on the item under it when both are the same potion, scroll, or incomplete relic or charm.
	/// </summary>
	/// <param name="dragItem">The dropped item; becomes the completed relic left in the sack when shards are to spare.</param>
	/// <param name="itemUnderUs">The item under the drop; becomes the item to pick up next, or <see langword="null"/>.</param>
	/// <returns><see langword="true"/> when stacked: the caller does not place <paramref name="dragItem"/>.</returns>
	protected bool TryStackOnDrop(ref Item dragItem, ref Item? itemUnderUs)
	{
		var stack = this.Services.ItemStackingService.TryStack(
			dragItem, itemUnderUs, this.ShownSack, this.CellsUnderDragItem.Location, USettings.DisableAutoStacking);
		if (!stack.Stacked)
			return false;

		this.InvalidateItemCacheAll(itemUnderUs, dragItem);

		dragItem = stack.DroppedItem;
		itemUnderUs = stack.PickedUpItem;
		return true;
	}

	/// <summary>
	/// Gets a rectangle of the screen coordinates for all of the items that need to be redrawn from an item drag.
	/// </summary>
	/// <returns>Rectangle of the redraw area.</returns>
	protected virtual Rectangle GetRepaintDragRect()
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
		}

		// We also need to wipe out any items that were under or are now under the dragItem so they can be redrawn
		if (this.ItemsUnderOldDragLocation is not null && this.ItemsUnderOldDragLocation.Count != 0)
		{
			foreach (Item item in this.ItemsUnderOldDragLocation)
			{
				// well we do not actually need to clear the item and redraw it since that will happen automatically
				// in the PaintCallback().  We just need to include this item in the clip region
				Point topLeft = this.CellTopLeft(item.Location);
				Point bottomRight = this.CellBottomRight(new Point(item.Right - 1, item.Bottom - 1));

				// Extend our clipping rectangle to include all these cells
				int oldX = x;
				int oldY = y;
				x = Math.Min(x, topLeft.X);
				y = Math.Min(y, topLeft.Y);
				width = Math.Max(oldX + width - 1, bottomRight.X) - x + 1;
				height = Math.Max(oldY + height - 1, bottomRight.Y) - y + 1;
			}
		}

		if (this.ItemsUnderDragItem is not null && this.ItemsUnderDragItem.Count != 0)
		{
			foreach (Item item in this.ItemsUnderDragItem)
			{
				// well we do not actually need to clear the item and redraw it since that will happen automatically
				// in the PaintCallback().  We just need to include this item in the clip region
				Point topLeft = this.CellTopLeft(item.Location);
				Point bottomRight = this.CellBottomRight(new Point(item.Right - 1, item.Bottom - 1));

				// Extend our clipping rectangle to include all these cells
				int oldX = x;
				int oldY = y;
				x = Math.Min(x, topLeft.X);
				y = Math.Min(y, topLeft.Y);
				width = Math.Max(oldX + width - 1, bottomRight.X) - x + 1;
				height = Math.Max(oldY + height - 1, bottomRight.Y) - y + 1;
			}
		}

		return new Rectangle(x, y, width, height);
	}

	/// <summary>
	/// Finds the rectangle under the mouse drag
	/// </summary>
	/// <returns>Rectangle representing the mouse drag area</returns>
	protected Rectangle GetMouseDragRectangle()
		=> new Rectangle(Math.Min(startPosition.X, currentPosition.X), Math.Min(startPosition.Y, currentPosition.Y), Math.Abs(startPosition.X - currentPosition.X), Math.Abs(startPosition.Y - currentPosition.Y));

	/// <summary>
	/// Updates the item's location properties to reflect its current position in this panel.
	/// This ensures search results always show the correct location.
	/// Also adds the item to the search database if it's not already present.
	/// </summary>
	/// <param name="item">The item to update</param>
	protected void UpdateItemLocation(Item item)
	{
		if (item is null)
			return;

		item.Place.CopyFrom(this.PlaceHere());
	}

	/// <summary>
	/// Gets the place of the shown sack: container, sack number and type.
	/// </summary>
	protected ContainerPlace PlaceHere() => new()
	{
		SackType = this.SackType,
		Path = this.PlayerCollection?.PlayerFile ?? string.Empty,
		Name = this.PlayerCollection?.PlayerName ?? string.Empty,
		StashType = this.ShownSack.StashType,
		SackNumber = (this.ShownSack.SackType, this.ShownSack.StashType) switch
		{
			(SackType.Stash, _) => (int)this.ShownSack.StashType!,
			(SackType.Equipment, _) => BagIdConstants.BAGID_EQUIPMENTPANEL,
			_ => this.CurrentSack,
		},
	};
}
