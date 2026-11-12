//-----------------------------------------------------------------------
// <copyright file="SackPanel.SackManagement.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Globalization;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Config;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Models;
using TQVaultAE.GUI.Tooltips;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Components.Panels;

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
			this.Services.MessageBoxes.Inform(Resources.SackPanelAutoSortMsg, Resources.SackPanelAutoSort);

			return;
		}

		if (this.ShownSack.IsEmpty)
			return;

		// Toggle sorting direction.
		this.sortVertical = !this.sortVertical;

		// The order picked in the sort button's menu.
		var algorithm = this.USettings.Sort?.Algorithm ?? SackSortAlgorithm.Normal;
		if (!this.Services.ItemSortService.Sort(this.ShownSack, this.SackSize, this.sortVertical, algorithm))
		{
			// An item did not fit: the sack is back as it was.
			this.sortVertical = !this.sortVertical;
			BagButtonTooltip.InvalidateCache(this.Sack);
			this.Services.MessageBoxes.Inform(Resources.SackPanelSortNoRoom, Resources.SackPanelAutoSortError);
			return;
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
			if (this.Sack is not null && !this.Sack.IsEmpty)
			{
				var plan = new AutoMovePlan(AutoMovePlanner.Order(this.Sack), this.AutoMoveLocation, destination);
				this.RaiseAutoMove(this.Sack, plan);

				this.ClearSelectedItems();

				ItemTooltip.HideTooltip();
				BagButtonTooltip.InvalidateCache(this.Sack);

				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Moves an item from one panel to another without context menu.
	/// </summary>
	/// <param name="location">Destination AutoMoveLocation</param>
	private void QuickMovePanel(AutoMoveLocation location)
	{
		if (Sack is null)
			return;

		Item? focusedItem = FindItem(LastCellWithFocus);

		if ((focusedItem is null && selectedItems is null) || this.ItemsReadOnly)
			return;

		var destination = AutoMovePlanner.QuickMoveDestination(location, this.AutoMoveLocation, this.SecondaryVaultShown);
		if (destination == AutoMoveLocation.NotSet)
			return;

		this.RaiseAutoMove(this.Sack, AutoMovePlanner.Plan(this.selectedItems, focusedItem, destination));

		if (this.selectedItems is not null)
			this.ClearSelectedItems();

		ItemTooltip.HideTooltip();
		BagButtonTooltip.InvalidateCache(Sack);
	}
}
