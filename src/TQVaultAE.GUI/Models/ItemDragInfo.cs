//-----------------------------------------------------------------------
// <copyright file="ItemDragInfo.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.GUI.Components.Panels;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.GUI.Models;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using TQVaultAE.GUI.Components;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Tooltips;

/// <summary>
/// Stores information about the item being dragged around by the user
/// </summary>
public class ItemDragInfo
{
	/// <summary>
	/// The sack where the item came from.
	/// </summary>
	private SackCollection? srcSack;

	/// <summary>
	/// The item being dragged around.
	/// </summary>
	private Item? srcItem;

	/// <summary>
	/// The sackPanel that is displaying the sack.
	/// </summary>
	private SackPanel? srcSackPanel;

	/// <summary>
	/// The offset of the pointer withing the item bitmap
	/// </summary>
	private Point mouseOffset;

	/// <summary>
	/// if we have performed an action on this drag, we can use this to access the original item information.
	/// </summary>
	private ItemDragInfo? original;


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
		this.autoMoveLocationsList = [];
		this.UIService = uiService;
	}

	/// <summary>
	/// The compare selection shared by every panel: while it is on, a left click on an item picks the second item to
	/// compare instead of taking the item.
	/// </summary>
	public ItemCompareSelection Compare { get; } = new();

	/// <summary>
	/// Gets a value indicating whether an item is currently being dragged around.
	/// </summary>
	[MemberNotNullWhen(true, nameof(Item), nameof(srcItem))]
	public bool IsActive => this.srcItem is not null;

	/// <summary>
	/// Gets the item being dragged around
	/// </summary>
	public Item? Item => this.srcItem;

	/// <summary>
	/// Gets the sack which the item was taken
	/// </summary>
	public SackCollection? SrcSack => this.srcSack;

	/// <summary>
	/// Gets the sack panel which the item was taken
	/// </summary>
	public SackPanel? SrcSackPanel => this.srcSackPanel;

	/// <summary>
	/// Gets the offset from the top-left corner of the bitmap to where the mouse pointer is located
	/// </summary>
	public Point MouseOffset => this.mouseOffset;

	/// <summary>
	/// Gets a value indicating whether the item has been modified
	/// </summary>
	[MemberNotNullWhen(true, nameof(Original), nameof(original))]
	public bool IsModifiedItem => this.original is not null;

	/// <summary>
	/// Gets the original draginfo data
	/// </summary>
	public ItemDragInfo? Original => this.original;

	/// <summary>
	/// Gets a value indicating whether a drag can be canceled
	/// Items that have no valid location cannot be canceled.
	/// Items that have been modified cannot be cancelled
	/// </summary>
	public bool CanBeCanceled
		=> !this.IsModifiedItem && this.srcItem is { } item && (item.PositionX >= 0 || item.PositionX == -3) && item.PositionY >= 0;

	/// <summary>
	/// Gets a value indicating whether an item can be modified
	/// Items that have no valid location cannot be modified
	/// </summary>
	public bool CanBeModified
		=> this.IsModifiedItem || (this.srcItem is { } item && item.PositionX >= 0 && item.PositionY >= 0);

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
		// modified items do not have a source sack.
		// so no one needs to be notified of the placement.
		if (!this.IsModifiedItem && this.srcSack is not null && this.srcItem is not null)
		{
			// Remove the item from the old sack; an equipped item leaves an empty item in its slot.
			EquipmentRules.TakeOut(this.srcSack, this.srcItem);

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
		this.srcSackPanel?.CancelDrag(this);

		// now clear things out
		this.srcItem = null;

		// If the item came from the equipment panel,
		// recalcuate the gear stats before clearing out the sackPanel.
		// The item needs to be cleared at this point to get the correct calculation.
		EquipmentPanel? equipmentPanel = srcSackPanel as EquipmentPanel;
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
	/// Drags <paramref name="newItem"/> instead of the source item, which stays in its sack: a split stack, a removed
	/// relic, a copy, a new set piece or a crafted artifact.
	/// </summary>
	/// <remarks>
	/// The source sack is not marked as modified here: the action that changed the source item (split, relic removal)
	/// marks it, so a copy alone leaves its vault or character unchanged and unsaved (upstream #267).
	/// </remarks>
	/// <param name="newItem">The item dragged from now on.</param>
	public void MarkModified(Item newItem)
	{
		if (this.IsModifiedItem)
		{
			// The item is already modified.  If it has been modified again, then we should tell the sackPanel to redraw it again
			this.original.srcSackPanel?.CancelDrag(this.original);
			this.srcItem = newItem;
		}
		else
		{
			// Tell the sackPanel the drag was cancelled so that it will redraw its now modified item
			this.srcSackPanel?.CancelDrag(this);

			// Now store our info inside original
			var original = (ItemDragInfo)this.MemberwiseClone();
			this.original = original;

			// Now set us up to the new item
			this.srcItem = newItem;
			this.srcSack = null; // it does not belong to a sack
			this.srcSackPanel = null; // nor a sack panel

			// reposition the mouse at the center of the new item if it is a different size than the old item
			if (original.srcItem is { } oldItem
				&& (newItem.Width != oldItem.Width || newItem.Height != oldItem.Height))
			{
				// An item without picture keeps the current offset.
				if (this.UIService.GetBitmap(newItem) is { } ibmp)
				{
					this.mouseOffset.X = ibmp.Width / 2;
					this.mouseOffset.Y = ibmp.Height / 2;
				}
			}
		}
	}
}