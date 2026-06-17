//-----------------------------------------------------------------------
// <copyright file="SackPanel.SackManagement.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Globalization;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Config;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.GUI.Models;
using TQVaultAE.GUI.Tooltip;
using TQVaultAE.Logs;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Components;

/// <summary>
/// Class for holding all of the UI functions of the sack panel.
/// </summary>
public partial class SackPanel : Panel, IScalingControl
{
	/// <summary>
	/// Sorts the items in the sack by size
	/// </summary>
	public void Autosort()
	{
		// Check to see if something is picked up.
		if (this.DragInfo.IsActive)
		{
			MessageBox.Show(
				Resources.SackPanelAutoSortMsg,
				Resources.SackPanelAutoSort,
				MessageBoxButtons.OK,
				MessageBoxIcon.None,
				MessageBoxDefaultButton.Button1,
				RightToLeftOptions);

			return;
		}

		// Sort the items and put them in a temporary sack.
		SackCollection tempSack = new SackCollection();
		var autoSortQuery = from Item item in this.Sack
							orderby (((item.Height * 3) + item.Width) * 100) descending, item.ItemGroup descending, item.BaseItemId, item.IsRelicComplete descending
							select item;

		foreach (Item item in autoSortQuery)
			tempSack.AddItem(item);

		if (tempSack == null || tempSack.IsEmpty)
			return;

		SackCollection backUpSack = this.Sack.Duplicate();

		this.Sack.EmptySack();
		// Toggle sorting direction.
		this.sortVertical = !this.sortVertical;

		int param1 = this.SackSize.Height;
		int param2 = this.SackSize.Width;
		if (this.sortVertical)
		{
			param1 = this.SackSize.Width;
			param2 = this.SackSize.Height;
		}

		foreach (Item tempItem in tempSack)
		{
			bool itemPlaced = false;
			int offset;

			if (this.sortVertical)
				offset = tempItem.Width - 1;
			else
				offset = tempItem.Height - 1;

			for (int j = 0; j < param1 - offset; ++j)
			{
				for (int k = 0; k < param2; ++k)
				{
					Collection<Item> foundItems;
					if (this.sortVertical)
					{
						foundItems = this.FindAllItems(new Rectangle(j, k, tempItem.Width, tempItem.Height));
						if ((foundItems == null || foundItems.Count == 0) && tempItem.Width + j <= this.SackSize.Width)
						{
							if (k + tempItem.Height <= this.SackSize.Height)
							{
								tempItem.PositionX = j;
								tempItem.PositionY = k;
								this.Sack.AddItem(tempItem);
								itemPlaced = true;
							}

							break;
						}
						else
							k += foundItems[0].Height - 1;
					}
					else
					{
						foundItems = this.FindAllItems(new Rectangle(k, j, tempItem.Width, tempItem.Height));
						if ((foundItems == null || foundItems.Count == 0) && tempItem.Height + j <= this.SackSize.Height)
						{
							if (k + tempItem.Width <= this.SackSize.Width)
							{
								tempItem.PositionX = k;
								tempItem.PositionY = j;
								this.Sack.AddItem(tempItem);
								itemPlaced = true;
							}

							break;
						}
						else
							k += foundItems[0].Width - 1;
					}
				}

				if (itemPlaced)
					break;
			}

			if (!itemPlaced)
			{
				this.sortVertical = !this.sortVertical;
				this.Sack.EmptySack();

				foreach (Item item in backUpSack)
					this.Sack.AddItem(item.Clone());

				this.Sack.IsModified = false;
				BagButtonTooltip.InvalidateCache(this.Sack);
				return;
			}
		}

		this.Invalidate();
		BagButtonTooltip.InvalidateCache(this.Sack);
	}

	/// <summary>
	/// Merges the items from two sacks into one sack
	/// </summary>
	/// <param name="destination">sack number where we are storing the combined sack</param>
	/// <returns>true if successful</returns>
	public bool MergeSack(int destination)
	{
		if (destination < 0 || destination > this.MaxSacks - 1 || destination == this.CurrentSack)
			return false;

		if (!this.DragInfo.IsActive)
		{
			if (this.Sack != null && !this.Sack.IsEmpty)
			{
				var mergeQuery = from Item item in this.Sack
								 where item != null
								 orderby (((item.Height * 3) + item.Width) * 100) + item.ItemGroup descending
								 select item;

				foreach (Item item in mergeQuery)
				{
					if (!this.DragInfo.IsActive)
					{
						this.DragInfo.Set(this, this.Sack, item, new Point(1, 1));
						this.DragInfo.AutoMoveDestination = this.AutoMoveLocation;
						this.DragInfo.DestIndex = destination;
						this.OnAutoMoveItem(this, new SackPanelEventArgs(null, null));
					}
				}

				this.ClearSelectedItems();

				ItemTooltip.HideTooltip();
				BagButtonTooltip.InvalidateCache(this.Sack);

				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Moves an item from one sack to another without the context menu.
	/// </summary>
	/// <param name="desintationSack">int containing the number of the destination sack.</param>
	private void QuickMoveSack(int sackNumber)
	{
		if (Sack == null)
			return;

		var isEquipmentReadOnly = (USettings.PlayerReadonly == true && SackType == SackType.Equipment);
		Item focusedItem = FindItem(LastCellWithFocus);

		if ((focusedItem == null && selectedItems == null) || isEquipmentReadOnly)
			return;

		if (MaxSacks > 1 && sackNumber < MaxSacks)
		{
			int offset2 = 0;

			if (SackType == SackType.Player || SackType == SackType.Player)
			{
				if (SackType == SackType.Player)
					offset2 = 1;
			}

			if (sackNumber != CurrentSack + offset2)
			{
				QuickMovePanel(this.AutoMoveLocation, sackNumber);
			}
		}
	}

	/// <summary>
	/// Moves an item from one panel to another without context menu.
	/// </summary>
	/// <param name="location">Destination AutoMoveLocation</param>
	/// <param name="destIndex">Bag index within the destination (or -1 for auto-find)</param>
	private void QuickMovePanel(AutoMoveLocation location, int destIndex = -1)
	{
		if (Sack == null)
			return;

		var isEquipmentReadOnly = (USettings.PlayerReadonly == true && SackType == SackType.Equipment);
		Item focusedItem = FindItem(LastCellWithFocus);
		AutoMoveLocation destination = AutoMoveLocation.NotSet;

		if ((focusedItem == null && selectedItems == null) || isEquipmentReadOnly)
			return;

		if (location != AutoMoveLocation)
		{
			destination = location;

			if (SecondaryVaultShown && location == AutoMoveLocation.Player)
				destination = AutoMoveLocation.SecondaryVault;
		}

		if ((SecondaryVaultShown && (destination == AutoMoveLocation.Stash || destination == AutoMoveLocation.Player)) || destination == AutoMoveLocation.NotSet)
			return;

		if (this.selectedItems != null)
		{
			var autoMoveQuery = from Item item in selectedItems
								where item != null
								orderby (((item.Height * 3) + item.Width) * 100) + item.ItemGroup descending
								select item;

			foreach (Item item in autoMoveQuery)
			{
				if (!DragInfo.IsActive)
				{
					DragInfo.Set(this, Sack, item, new Point(1, 1));
					DragInfo.AutoMoveDestination = destination;
					DragInfo.DestIndex = destIndex;
					OnAutoMoveItem(this, new SackPanelEventArgs(null, null));
				}
			}

			ClearSelectedItems();
		}
		else if (focusedItem != null)
		{
			DragInfo.Set(this, Sack, focusedItem, new Point(1, 1));
			DragInfo.AutoMoveDestination = destination;
			DragInfo.DestIndex = destIndex;
			OnAutoMoveItem(this, new SackPanelEventArgs(null, null));
		}

		ItemTooltip.HideTooltip();
		BagButtonTooltip.InvalidateCache(Sack);
	}
}
