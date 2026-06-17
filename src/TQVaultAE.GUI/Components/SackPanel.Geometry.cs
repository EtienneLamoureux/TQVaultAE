//-----------------------------------------------------------------------
// <copyright file="SackPanel.Geometry.cs" company="None">
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
	/// Finds the coordinates in the panel to place an item of a given size.
	/// Searches vertically for open space and returns (-1, -1) on failure.
	/// </summary>
	/// <param name="width">width of the item we are placing</param>
	/// <param name="height">height of the item we are placing</param>
	/// <returns>Point with the coordinates for the upper left cell in the block of cell which will fit the item.</returns>
	public Point FindOpenCells(int width, int height)
	{
		Collection<Item> foundItems;
		for (int j = 0; j < this.SackSize.Width - width + 1; ++j)
		{
			for (int k = 0; k < this.SackSize.Height; ++k)
			{
				foundItems = this.FindAllItems(new Rectangle(j, k, width, height));
				if ((foundItems == null || foundItems.Count == 0) && width + j <= this.SackSize.Width)
				{
					if (k + height <= this.SackSize.Height)
						return new Point(j, k);
				}
				else
					k += foundItems[0].Height - 1;
			}
		}

		return new Point(-1, -1);
	}

	/// <summary>
	/// Checks if a cell is available for placement
	/// </summary>
	/// <param name="x">X coordinate</param>
	/// <param name="y">Y coordinate</param>
	/// <param name="width">width of the item</param>
	/// <param name="height">height of the item</param>
	/// <returns>true if available</returns>
	public bool IsCellAvailable(int x, int y, int width, int height)
	{
		if (x < 0 || y < 0 || width < 1 || height < 1)
			return false;

		if (x + width > this.SackSize.Width || y + height > this.SackSize.Height)
			return false;

		var items = this.FindAllItems(new Rectangle(x, y, width, height));
		return items == null || items.Count == 0;
	}

	/// <summary>
	/// Checks if the item can be placed here
	/// </summary>
	/// <param name="itemToCheck">Item instance</param>
	/// <returns>true if valid</returns>
	public bool IsItemValidForPlacement(Item itemToCheck)
		=> this.sack.StashType != Domain.Entities.StashType.RelicVaultStash || itemToCheck.IsArtifact || itemToCheck.IsRelicOrCharm || itemToCheck.IsFormulae;
}
