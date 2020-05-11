//-----------------------------------------------------------------------
// <copyright file="ItemDragInfo.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI.Models
{
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Drawing;
	using TQVaultAE.GUI.Components;
	using TQVaultAE.Domain.Entities;
	using TQVaultAE.GUI.Tooltip;
	using TQVaultAE.Domain.Contracts.Services;

	/// <summary>
	/// Stores information about the item being dragged around by the user
	/// </summary>
	public class ItemDragInfo
	{
		/// <summary>
		/// The sack where the item came from.
		/// </summary>
		private SackCollection sack;

		/// <summary>
		/// The item being dragged around.
		/// </summary>
		private Item item;

		/// <summary>
		/// The sackPanel that is displaying the sack.
		/// </summary>
		private SackPanel sackPanel;

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
			this.AutoMove = AutoMoveLocation.NotSet;
			this.autoMoveLocationsList = new List<AutoMoveLocation>();
			this.UIService = uiService;
		}

		/// <summary>
		/// Gets a value indicating whether an item is currently being dragged around.
		/// </summary>
		public bool IsActive => this.item != null;

		/// <summary>
		/// Gets the item being dragged around
		/// </summary>
		public Item Item => this.item;

		/// <summary>
		/// Gets the sack which the item was taken
		/// </summary>
		public SackCollection Sack => this.sack;

		/// <summary>
		/// Gets the sack panel which the item was taken
		/// </summary>
		public SackPanel SackPanel => this.sackPanel;

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
			=> !this.IsModifiedItem && (this.item.PositionX >= 0 || this.item.PositionX == -3) && this.item.PositionY >= 0;

		/// <summary>
		/// Gets a value indicating whether an item can be modified
		/// Items that have no valid location cannot be modified
		/// </summary>
		public bool CanBeModified
			=> this.IsModifiedItem || (this.item.PositionX >= 0 && this.item.PositionY >= 0);

		/// <summary>
		/// Gets or sets the auto move destination
		/// </summary>
		public AutoMoveLocation AutoMove { get; set; }

		/// <summary>
		/// Gets a value indicating whether an auto move is taking place.
		/// </summary>
		public bool IsAutoMoveActive => this.AutoMove != AutoMoveLocation.NotSet;

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
				if (this.sack.SackType == SackType.Equipment)
					slot = EquipmentPanel.FindEquipmentSlot(this.sack, this.item);

				if (slot != -1)
				{
					// Remove the item out of the equipment slot
					this.sack.RemoveAtItem(slot);

					// Put a dummy item in it's place
					Item newItem = this.item.MakeEmptyItem();
					newItem.PositionX = SackCollection.GetEquipmentLocationOffset(slot).X;
					newItem.PositionY = SackCollection.GetEquipmentLocationOffset(slot).Y;
					this.sack.InsertItem(slot, newItem);
				}
				else
					// Remove the item from the old sack
					this.sack.RemoveItem(this.item);

				BagButtonTooltip.InvalidateCache(this.Sack, this.Original?.Sack);
			}

			// finally clear things out
			this.item = null;
			this.sack = null;
			this.sackPanel = null;
			this.original = null;
			this.AutoMove = AutoMoveLocation.NotSet;
			this.IsBeingCancelled = false;
		}

		/// <summary>
		/// Cancels a drag
		/// </summary>
		public void Cancel()
		{
			this.IsBeingCancelled = true;

			// let the owner know.
			this.sackPanel.CancelDrag(this);

			// now clear things out
			this.item = null;
			this.sack = null;
			this.sackPanel = null;
			this.AutoMove = AutoMoveLocation.NotSet;
			this.IsBeingCancelled = false;
		}

		/// <summary>
		/// Sets a drag.  Initializes the ItemDragInfo
		/// </summary>
		/// <param name="sackPanel">sack panel which contains the item</param>
		/// <param name="sack">sack which contains the item</param>
		/// <param name="item">the item being dragged</param>
		/// <param name="mouseOffset">offset of the mouse pointer to the top left corner of the item bitmap</param>
		public void Set(SackPanel sackPanel, SackCollection sack, Item item, Point mouseOffset)
		{
			this.item = item;
			this.sack = sack;
			this.sackPanel = sackPanel;
			this.mouseOffset = mouseOffset;
			this.original = null;
			this.AutoMove = AutoMoveLocation.NotSet;
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
				this.original.sackPanel.CancelDrag(this.original);
				this.item = newItem;
			}
			else
			{
				// Tell the sackPanel the drag was cancelled so that it will redraw its now modified item
				this.sackPanel.CancelDrag(this);

				// Tell the sack that it has been modified
				this.sack.IsModified = true;

				// Now store our info inside original
				this.original = (ItemDragInfo)this.MemberwiseClone();

				// Now set us up to the new item
				this.item = newItem;
				this.sack = null; // it does not belong to a sack
				this.sackPanel = null; // nor a sack panel

				// reposition the mouse at the center of the new item if it is a different size than the old item
				if (newItem.Width != this.original.item.Width 
					|| newItem.Height != this.original.item.Height
				)
				{
					var ibmp = this.UIService.GetBitmap(newItem);
					this.mouseOffset.X = ibmp.Width / 2;
					this.mouseOffset.Y = ibmp.Height / 2;
				}
			}
		}
	}
}