//-----------------------------------------------------------------------
// <copyright file="EquipmentPanel.Geometry.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
/// <summary>
/// Class for holding all of the UI functions of the sack panel in the stash panel.
/// </summary>

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
	/// Gets an item at a cell location
	/// </summary>
	/// <param name="cellLocation">Point of the cell location we are interested in</param>
	/// <returns>Item at that cell location</returns>
	protected override Item? FindItem(Point cellLocation)
	{
		if (this.Sack is null)
			return null;

		// Find the item for this point
		int itemSlot = -1;
		foreach (Item item in this.Sack)
		{
			++itemSlot;
			if (item == this.DragInfo.Item)
				// hide the item being dragged
				continue;

			// store the x and y values
			int x = item.PositionX;
			int y = item.PositionY;

			if (item.IsInWeaponSlot)
			{
				// Check if it's an item in the weapon box
				if (IsInsideWeaponBox(cellLocation, y))
				{
					// Verify that we are over the weapon box
					// If we are over an empty weapon box we need to check if there is a 2 Handed weapon
					// in the corresponding weapon box and if there is we return that instead.
					// Otherwise we just skip the empty item.
					if (RecordId.IsNullOrEmpty(item.BaseItemId))
					{
						Item? item2H = this.GetItemFromShadowSlot(itemSlot - 7);

						if (item2H is not null && !RecordId.IsNullOrEmpty(item2H.BaseItemId) && item2H.Is2HWeapon)
							// Make sure we got something and it's a 2 Handed weapon
							return item2H;
						else
							// Otherwise just skip it
							continue;
					}
					else
						// There is really an item here so we just return it.
						return item;
				}
				else
					// Just skip the item if we are not over a weapon box.
					continue;
			}
			else if (item.IsAmulet && IsInsideAmuletBox(cellLocation))
				// Check the amulet box since the item may not fill the entire box.
				return item;

			else if (RecordId.IsNullOrEmpty(item.BaseItemId))
				// Skip over empty items
				continue;

			// see if this item overlaps the cell
			if ((x <= cellLocation.X)
				&& ((x + item.Width - 1) >= cellLocation.X)
				&& (y <= cellLocation.Y) && ((y + item.Height - 1) >= cellLocation.Y))
				return item;
		}

		return null;
	}

	/// <summary>
	/// Gets the bottom cell of an item.  Not scaled to screen coordinates.
	/// </summary>
	/// <param name="item">Item which we are checking</param>
	/// <returns>Y value of the bottom cell of the item.</returns>
	protected override int CellBottom(Item item)
	{
		if (item.PositionX == Item.WeaponSlotIndicator)
		{
			// If we are over an equipment panel then we skip to the next column
			// since the x, y coordinates embedded in the item are a special case
			return SackCollection.WeaponLocationSize.Height + SackCollection.GetWeaponLocationOffset(item.PositionY).Y - 1;
		}

		return item.Height + item.PositionY - 1;
	}

	/// <summary>
	/// Find the rectanglular area of cell coords affected by these screen coords
	/// </summary>
	/// <param name="screenRectangle">Rectangle with the screen coordinates which will be converted to cell coordinates.</param>
	/// <returns>Rectangle containing the cells within the screen area</returns>
	protected override Rectangle FindAllCells(Rectangle screenRectangle)
	{
		Rectangle cellRect = base.FindAllCells(screenRectangle);

		// Check to see if the weapon boxes need to be included
		for (int i = 0; i < SackCollection.NumberOfWeaponSlots; ++i)
		{
			Rectangle weaponRect = new Rectangle(SackCollection.GetWeaponLocationOffset(i), SackCollection.WeaponLocationSize);
			if (weaponRect.IntersectsWith(cellRect))
			{
				cellRect = Rectangle.Union(cellRect, weaponRect);
				break;
			}
			else
				continue;
		}

		return cellRect;
	}

	/// <summary>
	/// Gets the item Rectangle converted to screen coordinates.
	/// </summary>
	/// <param name="item">Item that needs screen coordinates</param>
	/// <returns>Rectangle containing the screen coordinates occupied by the item.</returns>
	protected override Rectangle GetItemScreenRectangle(Item? item)
	{
		if (item is null)
			return Rectangle.Empty;

		return this.FindSlotRect(item.Location.X, item.Location.Y);
	}

	/// <summary>
	/// Returns whether the cell is within a specific weapon box.
	/// </summary>
	/// <param name="cellLocation">location we are checking</param>
	/// <param name="weaponSlot">weapon slot we are checking</param>
	/// <returns>true if the cells are within the specified weapon slot</returns>
	private static bool IsInsideWeaponBox(Point cellLocation, int weaponSlot)
	{
		// adjust it to use the real position
		Point slotUpperLeft = SackCollection.GetWeaponLocationOffset(weaponSlot);

		return slotUpperLeft.X <= cellLocation.X
			&& (slotUpperLeft.X + SackCollection.WeaponLocationSize.Width - 1) >= cellLocation.X
			&& slotUpperLeft.Y <= cellLocation.Y
			&& (slotUpperLeft.Y + SackCollection.WeaponLocationSize.Height - 1) >= cellLocation.Y;
	}

	/// <summary>
	/// Checks whether the cell is within the amulet box.
	/// </summary>
	/// <param name="cellLocation">cell location Point that is being checked</param>
	/// <returns>true if the cell location is within the amulet slot</returns>
	private static bool IsInsideAmuletBox(Point cellLocation)
	{
		Point slotUpperLeft = SackCollection.GetEquipmentLocationOffset(1);

		bool ans = slotUpperLeft.X <= cellLocation.X
			&& (slotUpperLeft.X + SackCollection.GetEquipmentLocationSize(1).Width - 1) >= cellLocation.X
			&& slotUpperLeft.Y <= cellLocation.Y
			&& (slotUpperLeft.Y + SackCollection.GetEquipmentLocationSize(1).Height - 1) >= cellLocation.Y;

		return ans;
	}

	/// <summary>
	/// Finds the top left corner of a weapon withn a weapon slot
	/// Used to center the weapon inside the weapon panel
	/// </summary>
	/// <param name="x">x cell coordinate of the weapon</param>
	/// <param name="y">y cell coordinate of the weapon</param>
	/// <param name="width">width of the weapon</param>
	/// <param name="height">height of the weapon</param>
	/// <returns>Point containing the top left corner of the weapon</returns>
	private Point WeaponTopLeft(int x, int y, int width, int height)
	{
		Point val = new Point();
		int offsetX = ((SackCollection.WeaponLocationSize.Width - width) * UIService.ItemUnitSize) / 2;
		int offsetY = ((SackCollection.WeaponLocationSize.Height - height) * UIService.ItemUnitSize) / 2;
		val.X = Convert.ToInt32(BorderWidth) + (x * UIService.ItemUnitSize) + offsetX;
		val.Y = Convert.ToInt32(BorderWidth) + (y * UIService.ItemUnitSize) + offsetY;

		return val;
	}

	/// <summary>
	/// Used find the screen coordinates of an equipment slot
	/// </summary>
	/// <param name="slot">equipment or weapon slot we are interested in</param>
	/// <returns>Rectangle containing the coordinates of the equipment slot</returns>
	private Rectangle FindSlotRect(int slot)
	{
		if (slot == -1 || slot > this.ShownSack.NumberOfSlots)
			return Rectangle.Empty;

		bool weaponSlot = false;
		int offset = 0;

		Point slotUL = SackCollection.GetEquipmentLocationOffset(slot);
		Size slotSize = SackCollection.GetEquipmentLocationSize(slot);

		// Check for weapon slot
		if (SackCollection.IsWeaponSlot(slot))
		{
			int slotOffset = slotUL.Y;
			slotSize = SackCollection.WeaponLocationSize;
			slotUL = SackCollection.GetWeaponLocationOffset(slotOffset);
			weaponSlot = true;
		}

		Point ul = this.CellTopLeft(slotUL);

		// Adjust the weapon slot for graphic
		if (weaponSlot)
		{
			ul.Y += UIService.HalfUnitSize;
			offset = 1;
		}

		return new Rectangle(
			ul.X,
			ul.Y,
			slotSize.Width * UIService.ItemUnitSize,
			(slotSize.Height - offset) * UIService.ItemUnitSize);
	}

	/// <summary>
	/// Gets the item instance for a specified weapon shadow slot.
	/// </summary>
	/// <param name="weaponSlot">weapon slot that we are checking</param>
	/// <returns>Base item instance</returns>
	private Item? GetItemFromShadowSlot(int weaponSlot)
		=> EquipmentRules.GetShadowSlotItem(this.ShownSack, weaponSlot);

	/// <summary>
	/// Gets the bounding rectangle for given cell.
	/// </summary>
	/// <param name="x">X coordinate of the cell to be checked.</param>
	/// <param name="y">Y coordinate of the cell to be checked.</param>
	/// <param name="useImage">Specifies whether we are using an image or a colored fill for the background.</param>
	/// <returns>Scaled rectangle for the weapon slot.</returns>
	private Rectangle FindSlotRect(int x, int y)
	{
		int width;
		int height;
		Point screenLocation;

		if (x == Item.WeaponSlotIndicator)
		{
			// See if it is a weapon
			// Adjust for special placement of weapons in the equipment panel
			width = SackCollection.GetEquipmentLocationSize(y + 7).Width;
			height = SackCollection.GetEquipmentLocationSize(y + 7).Height;
			int itemX = SackCollection.GetWeaponLocationOffset(y).X;
			int itemY = SackCollection.GetWeaponLocationOffset(y).Y;

			screenLocation = WeaponTopLeft(itemX, itemY, width, height);
		}
		else
		{
			Point cellLocation = new Point(x, y);
			screenLocation = this.CellTopLeft(cellLocation);
			int slot = FindEquipmentSlot(cellLocation);
			width = SackCollection.GetEquipmentLocationSize(slot).Width;
			height = SackCollection.GetEquipmentLocationSize(slot).Height;
		}

		return new Rectangle(screenLocation.X, screenLocation.Y, width * UIService.ItemUnitSize, height * UIService.ItemUnitSize);
	}

	/// <summary>
	/// Used to find the equipment slot under the mouse based on cell location
	/// </summary>
	/// <param name="cell">cell we are searching</param>
	/// <returns>-1 if nothing found, otherwise the slot location in the equiment sack is returned</returns>
	private int FindEquipmentSlot(Point cell)
		=> EquipmentRules.SlotAt(cell, this.ShownSack.NumberOfSlots);

	/// <summary>
	/// Used to find the equipment slot under the mouse based on the drag rectangle
	/// </summary>
	/// <param name="cellsUnderMouse">Rectangle containing all the cells under the mouse pointer</param>
	/// <returns>-1 if nothing found, otherwise the slot location in the equiment sack is returned</returns>
	private int FindEquipmentSlot(Rectangle cellsUnderMouse)
		=> EquipmentRules.SlotUnder(cellsUnderMouse, this.ShownSack.NumberOfSlots);

	/// <summary>
	/// Used to determine all slots under the current mouse cells and then creates a rectangle to encompass all of those cells.
	/// </summary>
	/// <param name="cellsUnderMouse">Rectangle containing all of the cells under the mouse</param>
	/// <returns>Rectangle of cells including all of the equipment slots under the mouse</returns>
	private Rectangle FindAllEquipmentCells(Rectangle cellsUnderMouse)
	{
		// Iterate through the equipment slots
		Point slotUL, slotBR;
		Size slotSize;
		Rectangle slotRect;

		// Start with the cells Under the mouse
		int x = cellsUnderMouse.X;
		int y = cellsUnderMouse.Y;
		int width = cellsUnderMouse.Width;
		int height = cellsUnderMouse.Height;
		bool foundCells = false;

		for (int slot = 0; slot < this.ShownSack.NumberOfSlots; ++slot)
		{
			slotUL = SackCollection.GetEquipmentLocationOffset(slot);
			if (SackCollection.IsWeaponSlot(slot))
			{
				slotUL = SackCollection.GetWeaponLocationOffset(slotUL.Y);
				slotSize = SackCollection.WeaponLocationSize;
			}
			else
				slotSize = SackCollection.GetEquipmentLocationSize(slot);

			slotBR = new Point(slotUL.X + slotSize.Width - 1, slotUL.Y + slotSize.Height - 1);
			slotRect = new Rectangle(slotUL, slotSize);

			if (cellsUnderMouse.IntersectsWith(slotRect))
			{
				// Check to see if it's a 2H weapon over a weapon slot.
				if ((this.DragInfo.Item?.Is2HWeapon ?? false) && (slot > 6 && slot < 11))
				{
					// We need to redraw both items
					int itemOffsetL = (((slot - 1) / 2) * 2) - 6;
					int itemOffsetR = (((slot - 1) / 2) * 2) - 5;
					slotUL = SackCollection.GetWeaponLocationOffset(itemOffsetL);
					slotBR = new Point(
						SackCollection.GetWeaponLocationOffset(itemOffsetR).X + SackCollection.WeaponLocationSize.Width - 1,
						SackCollection.GetWeaponLocationOffset(itemOffsetR).Y + SackCollection.WeaponLocationSize.Height - 1);
				}

				// Found it, so we expand our rectangle.
				foundCells = true;
				int oldX = x;
				int oldY = y;
				x = Math.Min(x, slotUL.X);
				y = Math.Min(y, slotUL.Y);
				width = Math.Max(oldX + width - 1, slotBR.X - slotUL.X + 1) - x + 1;
				height = Math.Max(oldY + height - 1, slotBR.Y - slotUL.Y + 1) - y + 1;
			}
		}

		// We didn't find it so we return an empty rectangle
		if (foundCells)
			return new Rectangle(x, y, width, height);
		else
			return Rectangle.Empty;
	}
}
