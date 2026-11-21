//-----------------------------------------------------------------------
// <copyright file="SackPanel.Geometry.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.ObjectModel;
using TQVaultAE.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Globalization;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Config;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.GUI.Components.Rendering;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Forms;
using TQVaultAE.GUI.Models;
using TQVaultAE.GUI.Tooltips;
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
	/// Checks if the item can be placed here
	/// </summary>
	/// <param name="itemToCheck">Item instance</param>
	/// <returns>true if valid</returns>
	public bool IsItemValidForPlacement(Item itemToCheck)
		=> this.Services.ItemMovementService.IsItemValidForStashType(itemToCheck, this.sack?.StashType);

	/// <summary>
	/// Converts the screen coords into cell coords
	/// </summary>
	/// <param name="location">Point containing the screen coordinates</param>
	/// <returns>Point of corresponding cell coordinate</returns>
	protected Point FindCell(Point location)
	{
		int x;
		if (location.X < this.borderPen.Width)
			x = -1;
		else
			x = (location.X - (int)this.borderPen.Width) / this.UnitSize;

		if (x >= this.SackSize.Width)
			x = -2;

		int y;
		if (location.Y < this.borderPen.Width)
			y = -1;
		else
			y = (location.Y - (int)this.borderPen.Width) / this.UnitSize;

		if (y >= this.SackSize.Height)
			y = -2;

		return new Point(x, y);
	}

	/// <summary>
	/// Gets an item at a cell location
	/// </summary>
	/// <param name="cellLocation">Point of the cell location we are interested in</param>
	/// <returns>Item at that cell location</returns>
	protected virtual Item? FindItem(Point cellLocation)
	{
		if (this.Sack is null)
			return null;

		// hide the item being dragged
		return this.Services.ItemMovementService.FindItem(this.Sack, cellLocation, this.DragInfo.Item);
	}

	/// <summary>
	/// Find all items that are at least partially within the cell rectangle
	/// </summary>
	/// <param name="cellRectangle">cell rectangle we are looking at</param>
	/// <returns>Array of items within the rectangle</returns>
	protected virtual Collection<Item> FindAllItems(Rectangle cellRectangle)
	{
		Collection<Item> items = [];
		int minX = cellRectangle.X;
		int maxX = cellRectangle.X + cellRectangle.Width;
		int minY = cellRectangle.Y;
		int maxY = cellRectangle.Y + cellRectangle.Height;

		for (int x = minX; x < maxX; ++x)
		{
			for (int y = minY; y < maxY; ++y)
			{
				Item? item = this.FindItem(new Point(x, y));

				if (item is not null)
				{
					// Add this item to the array if it is not already in there
					if (!items.Contains(item))
					{
						items.Add(item);
					}

					y = this.CellBottom(item);
				}
			}
		}

		return items;
	}

	/// <summary>
	/// Given the cell X and Y coordinates, get the screen coordinates for the top left corner of the cell
	/// </summary>
	/// <param name="location">cell coordinates</param>
	/// <returns>Point for the corresponding screen position of the top left corner of the cell.</returns>
	protected Point CellTopLeft(Point location)
	{
		return new Point(
			Convert.ToInt32(this.borderPen.Width) + (location.X * this.UnitSize),
			Convert.ToInt32(this.borderPen.Width) + (location.Y * this.UnitSize));
	}

	/// <summary>
	/// Given the cell X and Y coordinates, get the screen coordinates for the top right corner of the cell
	/// </summary>
	/// <param name="location">cell coordinates</param>
	/// <returns>Point for the corresponding screen position of the top right corner of the cell.</returns>
	protected Point CellTopRight(Point location)
	{
		return new Point(
			Convert.ToInt32(this.borderPen.Width) + ((location.X + 1) * this.UnitSize) - 1,
			Convert.ToInt32(this.borderPen.Width) + (location.Y * this.UnitSize));
	}

	/// <summary>
	/// Given the cell X and Y coordinates, get the screen coordinates for the bottom left corner of the cell
	/// </summary>
	/// <param name="location">cell coordinates</param>
	/// <returns>Point for the corresponding screen position of the bottom left corner of the cell.</returns>
	protected Point CellBottomLeft(Point location)
	{
		return new Point(
			Convert.ToInt32(this.borderPen.Width) + (location.X * this.UnitSize),
			Convert.ToInt32(this.borderPen.Width) + ((location.Y + 1) * this.UnitSize) - 1);
	}

	/// <summary>
	/// Gets the bottom cell of an item.  Not scaled to screen coordinates.
	/// </summary>
	/// <param name="item">Item which we are checking</param>
	/// <returns>Y value of the bottom cell of the item.</returns>
	protected virtual int CellBottom(Item item) => item.Height + item.Location.Y - 1;

	/// <summary>
	/// Given the cell X and Y coordinates, get the screen coordinates for the bottom right corner of the cell
	/// </summary>
	/// <param name="location">cell coordinates</param>
	/// <returns>Point for the corresponding screen position of the bottom right corner of the cell.</returns>
	protected Point CellBottomRight(Point location)
	{
		return new Point(
			Convert.ToInt32(this.borderPen.Width) + ((location.X + 1) * this.UnitSize) - 1,
			Convert.ToInt32(this.borderPen.Width) + ((location.Y + 1) * this.UnitSize) - 1);
	}

	/// <summary>
	/// Find the rectanglular area of cell coords affected by these screen coords
	/// </summary>
	/// <param name="screenRectangle">Rectangle with the screen coordinates which will be converted to cell coordinates.</param>
	/// <returns>Rectangle containing the cells within the screen area</returns>
	protected virtual Rectangle FindAllCells(Rectangle screenRectangle)
	{
		int endX = screenRectangle.Right - 1;
		int endY = screenRectangle.Bottom - 1;

		Point screenTL = this.CellTopLeft(Point.Empty);
		Point screenBR = this.CellBottomRight(new Point(this.SackSize.Width - 1, this.SackSize.Height - 1));
		Point topLeft = new Point(-1, -1);
		Point bottomRight = new Point(-1, -1);

		// First trim the rectangle to our panel cell region
		// See if any of the X's are in our area
		if (endX >= screenTL.X && screenRectangle.X <= screenBR.X)
		{
			topLeft.X = Math.Max(screenRectangle.X, screenTL.X);
			bottomRight.X = Math.Min(endX, screenBR.X);
		}

		// See if any of the Y's are in our area
		if (endY >= screenTL.Y && screenRectangle.Y <= screenBR.Y)
		{
			topLeft.Y = Math.Max(screenRectangle.Y, screenTL.Y);
			bottomRight.Y = Math.Min(endY, screenBR.Y);
		}

		// Okay now convert topLeft and bottomRight to Cell coords and return it.
		topLeft = this.FindCell(topLeft);
		bottomRight = this.FindCell(bottomRight);
		return new Rectangle(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X + 1, bottomRight.Y - topLeft.Y + 1);
	}

	/// <summary>
	/// Gets the item Rectangle converted to screen coordinates.
	/// </summary>
	/// <param name="item">Item that needs screen coordinates</param>
	/// <returns>Rectangle containing the screen coordinates occupied by the item.</returns>
	protected virtual Rectangle GetItemScreenRectangle(Item? item)
	{
		if (item is null)
			return Rectangle.Empty;

		Point screenLocation = this.CellTopLeft(item.Location);
		return new Rectangle(screenLocation.X, screenLocation.Y, item.Size.Width * this.UnitSize, item.Size.Height * this.UnitSize);
	}
}
