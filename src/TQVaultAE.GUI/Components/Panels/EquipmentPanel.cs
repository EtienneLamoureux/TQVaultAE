//-----------------------------------------------------------------------
// <copyright file="EquipmentPanel.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Models;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Tooltips;
using Microsoft.Extensions.Logging;
using TQVaultAE.Config;

namespace TQVaultAE.GUI.Components.Panels;

/// <summary>
/// Class for holding all of the UI functions of the sack panel in the stash panel.
/// </summary>
public partial class EquipmentPanel : SackPanel, IScalingControl
{
	private readonly ILogger Log;
	private readonly UserSettings USettings;

	/// <summary>An equipped item lies in a slot, not in cells: whatever its size, it stays in its slot.</summary>
	protected override bool HasRoomOnceReadAgain(Item item) => true;

	/// <summary>
	/// Initializes a new instance of the EquipmentPanel class.
	/// </summary>
	/// <param name="sackWidth">Width of the sack panel in cells</param>
	/// <param name="sackHeight">Height of the sack panel in cells</param>
	/// <param name="dragInfo">ItemDragInfo instance.</param>
	/// <param name="autoMoveLocation">AutoMoveLocation for this panel.</param>
	public EquipmentPanel(int sackWidth, int sackHeight, ItemDragInfo dragInfo, AutoMoveLocation autoMoveLocation, GuiServices services)
		: base(sackWidth, sackHeight, dragInfo, autoMoveLocation, services)
	{
		this.Log = services.Logger<EquipmentPanel>();
		this.USettings = services.Settings;

		this.SackType = SackType.Equipment;
		this.BackColor = Color.Transparent;
		this.DisableGrid = true;
		this.DisableBorder = true;
		this.DisableMultipleSelection = true;
		this.DefaultImage = null;
		this.MouseMove += this.LoadoutMarkMouseMove;
	}

	/// <summary>A click on a loadout mark opens its comparison; any other click goes on as in every sack.</summary>
	protected override void MouseDownCallback(object? sender, MouseEventArgs e)
	{
		if (this.Sack is not null && this.LoadoutMarkMouseDown(e))
			return;

		base.MouseDownCallback(sender, e);
	}

	#region EquipmentPanel Properties

	/// <summary>
	/// Gets or sets the equipment changed event handler
	/// </summary>
	public EventHandler<SackPanelEventArgs>? OnEquipmentChanged { get; set; }

	#endregion EquipmentPanel Properties

	#region EquipmentPanel Public Methods

	/// <summary>
	/// The items worn: the equipment without the item being dragged. The item requirements and the statistics panel
	/// both count these.
	/// </summary>
	public IReadOnlyList<Item> WornItems
		=> this.Sack is null ? [] : this.Services.PlayerService.GetWornEquipment(this.Sack, this.DragInfo.Item);

	/// <summary>
	/// Calculates the attribute bonuses of the equipped gear and the skills and updates the player, then tells the
	/// statistics panel to show them.
	/// </summary>
	public void GetGearStatBonus()
	{
		if (Sack is null)
			return;

		var currentPlayer = userContext.CurrentPlayer;
		if (currentPlayer is null)
			return;

		this.Services.PlayerService.UpdateAttributeBonuses(currentPlayer, this.WornItems);

		// Let the container panel know that the calculation has changed.
		OnEquipmentChanged?.Invoke(this, new SackPanelEventArgs(null, null));
	}

	#endregion EquipmentPanel Public Methods

	/// <summary>An equipped item deleted or changed in place: the bonuses are summed again.</summary>
	protected override void OnShownItemsChanged() => this.GetGearStatBonus();

	#region EquipmentPanel Protected Methods

	/// <summary>The equipment slots and their frames are drawn apart from the cell grid: a drag repaints the whole panel.</summary>
	protected override bool RepaintsDragAreaOnly => false;

	/// <summary>
	/// Changes the background of the item under the mouse so that it appears highlighted
	/// </summary>
	/// <param name="cell">Point containing the cell that the mouse is over</param>
	protected override void HighlightItemUnderMouse(Point cell)
	{
		// Check to see if the item needs to be redrawn if the selected items list got changed.
		bool redrawSelection = this.IsSelectedItemsChanged();

		if (cell != this.LastCellWithFocus || redrawSelection)
		{
			// We have moved to a different cell
			Item? lastItem = this.FindItem(LastCellWithFocus);
			Item? newItem = this.FindItem(cell);

			if (newItem != lastItem || redrawSelection)
			{
				// We have moved to a different item
				if (lastItem is not null && lastItem.Is2HWeapon)
				{
					if (this.GetItemFromShadowSlot(lastItem.PositionY) is { } last2HItem)
						this.Invalidate(this.FindSlotRect(last2HItem.Location.X, last2HItem.Location.Y));
				}

				if (newItem != lastItem)
				{
					if (newItem is not null && newItem.Is2HWeapon)
					{
						// Now we need to highlight the current item
						// Check if the item is selected and use a different background
						if (this.GetItemFromShadowSlot(newItem.PositionY) is { } new2HItem)
							this.Invalidate(this.FindSlotRect(new2HItem.Location.X, new2HItem.Location.Y));
					}
				}
			}
		}

		// Call the base method to update the last cell and to draw the items.
		base.HighlightItemUnderMouse(cell);
	}

	#endregion EquipmentPanel Protected Methods
}