//-----------------------------------------------------------------------
// <copyright file="SackPanel.Selection.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using TQVaultAE.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Globalization;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Config;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Forms;
using TQVaultAE.GUI.Models;
using TQVaultAE.Presentation.Win32.Contracts;
using TQVaultAE.Presentation.Win32;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Components.Panels;

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
		Item? item = this.FindItem(location);

		if (item is not null)
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
		if (this.selectedItems is not null)
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
		if (this.Sack is not null && !this.Sack.IsEmpty)
		{
			if (this.selectedItems is not null)
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
		if (this.Sack is not null && !this.Sack.IsEmpty)
		{
			if (this.selectedItems is not null)
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

		if (this.Sack is null || this.Sack.IsEmpty)
			return;

		// Make sure there we have items selected
		if (this.selectedItems is null)
			return;

		// Nothing moves when an item is in the way or would leave the sack.
		if (!this.Services.ItemMovementService.MoveItems(this.Sack, this.selectedItems, this.SackSize, new Point(offsetX, offsetY)))
			return;

		// Redraw Sack
		this.Refresh();
	}

	/// <summary>
	/// Gets whether a particular item has been multi-selected
	/// </summary>
	/// <param name="item">Item we are testing</param>
	/// <returns>true if the item has been multi-selected</returns>
	protected bool IsItemSelected(Item item)
	{
		// Make sure we have something to check
		if (this.selectedItems is null || this.selectedItems.Count == 0)
			return false;

		// Iterate through the list of selected items
		foreach (Item selectedItem in this.selectedItems)
		{
			if (item == selectedItem)
				// We have a match
				return true;
		}

		return false;
	}

	/// <summary>
	/// Adds an item to the selected items list and if it's already there it gets removed.
	/// </summary>
	/// <param name="item">Item we are adding to the selection list.</param>
	protected void SelectItem(Item item)
	{
		if (item is null)
			return;

		// Allocate the List if needed.
		if (this.selectedItems is null)
			this.selectedItems = [];

		// Check to see if the item is already selected
		if (!this.IsItemSelected(item))
		{
			// Need to add the item to the list
			this.selectedItems.Add(item);
			this.selectedItemsChanged = true;
		}
		else
			// It's already there so we need to remove it.
			this.RemoveSelectedItem(item);

		this.OnItemSelected?.Invoke(this, new SackPanelEventArgs(null, null));
	}

	/// <summary>
	/// Removes an item from the selected items list.
	/// </summary>
	/// <param name="item">Item that we are removing</param>
	protected void RemoveSelectedItem(Item item)
	{
		if (item is null || this.selectedItems is null)
			return;

		if (this.IsItemSelected(item))
		{
			// Check to see if the item is already selected
			// If the item is in the list then remove it.
			this.selectedItems.Remove(item);
			this.selectedItemsChanged = true;

			if (this.selectedItems.Count == 0)
				this.ClearSelection();
		}
	}

	/// <summary>
	/// Indicates whether the item selection has changed.
	/// </summary>
	/// <returns>True if the item selection has changed and resets back to false afterward.</returns>
	protected bool IsSelectedItemsChanged()
	{
		// Check if the flag is set
		if (this.selectedItemsChanged)
		{
			// Reset the flag and return that the flag WAS set.
			this.selectedItemsChanged = false;
			return true;
		}
		else
			return false;
	}

	/// <summary>
	/// Clears the item selection array
	/// </summary>
	protected void ClearSelection()
	{
		if (this.selectedItems is not null)
			this.selectedItems = null;
	}
}
