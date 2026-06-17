//-----------------------------------------------------------------------
// <copyright file="ItemDragInfo.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using TQVaultAE.Application.Contracts.Services;

namespace TQVaultAE.GUI.Models;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using TQVaultAE.GUI.Components;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Tooltip;

/// <summary>
/// Stores information about the item being dragged around by the user
/// </summary>
public class ItemDragInfo
{
	/// <summary>
	/// The sack where the item came from.
	/// </summary>
	private SackCollection srcSack;

	/// <summary>
	/// The item being dragged around.
	/// </summary>
	private Item srcItem;

	/// <summary>
	/// The sackPanel that is displaying the sack.
	/// </summary>
	private SackPanel srcSackPanel;

	/// <summary>
	/// The offset of the pointer withing the item bitmap
	/// </summary>
	private Point mouseOffset;

	/// <summary>
	/// if we have performed an action on this drag, we can use this to access the original item information.
	/// </summary>
	private ItemDragInfo original;


	/// <summary>
	/// List holding all of the valid AutoMoveLocations
	/// </summary>
	private List<AutoMoveLocation> autoMoveLocationsList;
	private readonly IUIService UIService;

	/// <summary>
	/// Initializes a new instance of the ItemDragInfo class.
	/// </summary>
	public ItemDragInfo(IUIService uiService)
	{
		this.AutoMoveDestination = AutoMoveLocation.NotSet;
		this.DestIndex = -1;
		this.autoMoveLocationsList = new List<AutoMoveLocation>();
		this.UIService = uiService;
	}

	/// <summary>
	/// Gets a value indicating whether an item is currently being dragged around.
	/// </summary>
	public bool IsActive => this.srcItem != null;

	/// <summary>
	/// Gets the item being dragged around
	/// </summary>
	public Item Item => this.srcItem;

	/// <summary>
	/// Gets the sack which the item was taken
	/// </summary>
	public SackCollection SrcSack => this.srcSack;

	/// <summary>
	/// Gets the sack panel which the item was taken
	/// </summary>
	public SackPanel SrcSackPanel => this.srcSackPanel;

	/// <summary>
	/// Gets the offset from the top-left corner of the bitmap to where the mouse pointer is located
	/// </summary>
	public Point MouseOffset => this.mouseOffset;

	/// <summary>
	/// Gets a value indicating whether the item has been modified
	/// </summary>
	public bool IsModifiedItem => this.original != null;

	/// <summary>
	/// Gets the original draginfo data
	/// </summary>
	public ItemDragInfo Original => this.original;

	/// <summary>
	/// Gets a value indicating whether a drag can be canceled
	/// Items that have no valid location cannot be canceled.
	/// Items that have been modified cannot be cancelled
	/// </summary>
	public bool CanBeCanceled
		=> !this.IsModifiedItem && (this.srcItem.PositionX >= 0 || this.srcItem.PositionX == -3) && this.srcItem.PositionY >= 0;

	/// <summary>
	/// Gets a value indicating whether an item can be modified
	/// Items that have no valid location cannot be modified
	/// </summary>
	public bool CanBeModified
		=> this.IsModifiedItem || (this.srcItem.PositionX >= 0 && this.srcItem.PositionY >= 0);

	/// <summary>
	/// Gets or sets the auto move destination
	/// </summary>
	public AutoMoveLocation AutoMoveDestination { get; set; }

	/// <summary>
	/// Gets or sets the bag index for vault destinations.
	/// Used when AutoMoveDestination is Vault or SecondaryVault.
	/// -1 means not set (auto-find first available bag).
	/// </summary>
	public int DestIndex { get; set; }

	/// <summary>
	/// Gets a value indicating whether an auto move is taking place.
	/// </summary>
	public bool IsAutoMoveActive => this.AutoMoveDestination != AutoMoveLocation.NotSet;

	/// <summary>
	/// Gets or sets a value indicating whether the drag is being canceled.
	/// </summary>
	public bool IsBeingCancelled { get; set; }

	/// <summary>
	/// Gets the list of all automovelocations
	/// </summary>
	public ReadOnlyCollection<AutoMoveLocation> AllAutoMoveLocations
		=> new ReadOnlyCollection<AutoMoveLocation>(this.autoMoveLocationsList);

	/// <summary>
	/// Adds an automovelocation to the list
	/// </summary>
	/// <param name="autoMoveLocation">AutoMoveLocation to add to the list.</param>
	public void AddAutoMoveLocationToList(AutoMoveLocation autoMoveLocation) 
		=> this.autoMoveLocationsList.Add(autoMoveLocation);

	/// <summary>
	/// Marks the item as placed removing it from the original container.
	/// </summary>
	/// <param name="slot">slot if an equipment placement.</param>
	public void MarkPlaced()
	{
		int slot = -1;

		// modified items do not have a source sack.
		// so no one needs to be notified of the placement.
		if (!this.IsModifiedItem)
		{       
			// Check to see if the dragged item is from one of the equipped slots.
			if (this.srcSack.SackType == SackType.Equipment)
				slot = EquipmentPanel.FindEquipmentSlot(this.srcSack, this.srcItem);

			if (slot != -1)
			{
				// Remove the item out of the equipment slot
				this.srcSack.RemoveAtItem(slot);

				// Put a dummy item in it's place
				Item newItem = this.srcItem.MakeEmptyItem();
				newItem.PositionX = SackCollection.GetEquipmentLocationOffset(slot).X;
				newItem.PositionY = SackCollection.GetEquipmentLocationOffset(slot).Y;
				this.srcSack.InsertItem(slot, newItem);
			}
			else
				// Remove the item from the old sack
				this.srcSack.RemoveItem(this.srcItem);

			BagButtonTooltip.InvalidateCache(this.SrcSack, this.Original?.SrcSack);
		}

		// finally clear things out
		this.srcItem = null;
		this.srcSack = null;
		this.srcSackPanel = null;
		this.original = null;
		this.AutoMoveDestination = AutoMoveLocation.NotSet;
		this.DestIndex = -1;
		this.IsBeingCancelled = false;
	}

	/// <summary>
	/// Cancels a drag
	/// </summary>
	public void Cancel()
	{
		this.IsBeingCancelled = true;

		// let the owner know.
		this.srcSackPanel.CancelDrag(this);

		// now clear things out
		this.srcItem = null;

		// If the item came from the equipment panel,
		// recalcuate the gear stats before clearing out the sackPanel.
		// The item needs to be cleared at this point to get the correct calculation.
		EquipmentPanel equipmentPanel = srcSackPanel as EquipmentPanel;
		equipmentPanel?.GetGearStatBonus();

		this.srcSack = null;
		this.srcSackPanel = null;
		this.AutoMoveDestination = AutoMoveLocation.NotSet;
		this.DestIndex = -1;
		this.IsBeingCancelled = false;
	}

	/// <summary>
	/// Sets a drag.  Initializes the ItemDragInfo
	/// </summary>
	/// <param name="srcSackPanel">sack panel which contains the item</param>
	/// <param name="srcSack">sack which contains the item</param>
	/// <param name="srcItem">the item being dragged</param>
	/// <param name="mouseOffset">offset of the mouse pointer to the top left corner of the item bitmap</param>
	public void Set(SackPanel srcSackPanel, SackCollection srcSack, Item srcItem, Point mouseOffset)
	{
		this.srcItem = srcItem;
		this.srcSack = srcSack;
		this.srcSackPanel = srcSackPanel;
		this.mouseOffset = mouseOffset;
		this.original = null;
		this.AutoMoveDestination = AutoMoveLocation.NotSet;
		this.DestIndex = -1;
		this.IsBeingCancelled = false;
	}

	/// <summary>
	/// Marks an item as modified.  Used for splitting stacks and removing charms and relics.
	/// </summary>
	/// <param name="newItem">New item to be created after the split.</param>
	public void MarkModified(Item newItem)
	{
		if (this.IsModifiedItem)
		{
			// The item is already modified.  If it has been modified again, then we should tell the sackPanel to redraw it again
			this.original.srcSackPanel.CancelDrag(this.original);
			this.srcItem = newItem;
		}
		else
		{
			// Tell the sackPanel the drag was cancelled so that it will redraw its now modified item
			this.srcSackPanel.CancelDrag(this);

			// Tell the sack that it has been modified
			this.srcSack.IsModified = true;

			// Now store our info inside original
			this.original = (ItemDragInfo)this.MemberwiseClone();

			// Now set us up to the new item
			this.srcItem = newItem;
			this.srcSack = null; // it does not belong to a sack
			this.srcSackPanel = null; // nor a sack panel

			// reposition the mouse at the center of the new item if it is a different size than the old item
			if (newItem.Width != this.original.srcItem.Width 
			    || newItem.Height != this.original.srcItem.Height
			   )
			{
				var ibmp = this.UIService.GetBitmap(newItem);
				this.mouseOffset.X = ibmp.Width / 2;
				this.mouseOffset.Y = ibmp.Height / 2;
			}
		}
	}
}