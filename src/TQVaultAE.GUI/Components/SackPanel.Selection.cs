//-----------------------------------------------------------------------
// <copyright file="SackPanel.Selection.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.ObjectModel;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.GUI.Components;

/// <summary>
/// Class for holding all of the UI functions of the sack panel.
/// </summary>
public partial class SackPanel
{
	/// <summary>
	/// Selects and highlights an item.
	/// </summary>
	/// <param name="location">location coordinates of the item we are highlighting</param>
	public void SelectItem(Point location)
	{
		Item item = this.FindItem(location);

		if (item != null)
		{
			this.SelectItem(item);
			this.Invalidate();
		}
	}

	/// <summary>
	/// Clears all selected items.
	/// </summary>
	public void ClearSelectedItems()
	{
		if (this.selectedItems != null)
		{
			this.ClearSelection();
			this.Invalidate();
		}
	}

	/// <summary>
	/// Selects all items in the sack
	/// </summary>
	public void SelectAllItems()
	{
		if (this.Sack != null && !this.Sack.IsEmpty)
		{
			if (this.selectedItems != null)
				this.ClearSelection();

			foreach (Item item in this.Sack)
				this.SelectItem(item);

			this.Invalidate();
		}
	}

	/// <summary>
	/// Selects all highlighted items in the sack
	/// </summary>
	public void SelectAllHighlightedItems()
	{
		if (this.Sack != null && !this.Sack.IsEmpty)
		{
			if (this.selectedItems != null)
				this.ClearSelection();

			foreach (Item item in this.HighlightService.HighlightedItems)
			{
				if (this.Sack.Contains(item))
					this.SelectItem(item);
			}
			this.Invalidate();
		}
	}

	/// <summary>
	/// Moves selected items by one space in a direction, does not move any items if one of them can't move
	/// </summary>
	public void MoveSelectedItemsInSack(string direction)
	{
		// Set offsets for the movement direction
		int offsetX = 0, offsetY = 0;
		switch (direction)
		{
			case "up": offsetY = -1; break;
			case "down": offsetY = 1; break;
			case "left": offsetX = -1; break;
			case "right": offsetX = 1; break;
		}

		// Disable feature in the equipment panel
		if (this.SackType == SackType.Equipment)
			return;

		// Make sure we are not holding an item.
		if (this.DragInfo.IsActive)
			return;

		if (this.Sack == null || this.Sack.IsEmpty)
			return;

		// Make sure there we have items selected
		if (this.selectedItems == null)
			return;

		Rectangle movedItemArea = new();

		// Look for other items in the way
		foreach (Item item in this.selectedItems)
		{
			movedItemArea = new Rectangle(new Point(item.PositionX + offsetX, item.PositionY + offsetY), item.Size);

			// Find items in the new location and add them to the list, if they aren't one of the selected items
			foreach (Item blockingItem in this.FindAllItems(movedItemArea))
			{
				if (!this.selectedItems.Contains(blockingItem))
					return;
			}

			// Check if we are still inside the area of the sack
			if (movedItemArea.Top < 0 | movedItemArea.Left < 0 | movedItemArea.Bottom - 1 >= this.SackSize.Height | movedItemArea.Right - 1 >= this.SackSize.Width)
				return;
		}

		// Move the items once we know nothing is in the way
		foreach (Item item in this.selectedItems)
		{
			item.PositionX += offsetX;
			item.PositionY += offsetY;
		}

		// Redraw Sack
		this.Refresh();
	}
}
