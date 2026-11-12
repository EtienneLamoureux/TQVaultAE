﻿﻿﻿using TQVaultAE.Application;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Components.Panels;
using TQVaultAE.GUI.Components;
using TQVaultAE.GUI.Models;
using TQVaultAE.GUI.Tooltips;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Forms;

public partial class MainForm
{

	/// <summary>
	/// Creates the form's internal panels
	/// </summary>
	private void CreatePanels()
	{
		this.CreatePlayerPanel();

		// Put the secondary vault list on top of the player list drop down
		// since only one can be shown at a time.
		this.bufferedFlowLayoutPanelsecondaryVaultList.Visible = false;

		this.GetPlayerList();

		this.CreateVaultPanel(12); // # of bags in a vault.  This number is also buried in the CreateVault() function
		this.CreateSecondaryVaultPanel(12); // # of bags in a vault.  This number is also buried in the CreateVault() function
		this.secondaryVaultPanel.Enabled = false;
		this.secondaryVaultPanel.Visible = false;
		this.lastBag = -1;

		this.itemTextPanel.Size = new Size(this.vaultPanel.Width, Convert.ToInt32(22.0F * UIService.Scale));

		// The highlight row is as wide as the vault, and scales with it. Docked to fill its cell, it would set the width
		// of that auto-sized column itself: a column it had widened (a maximize) would never narrow again.
		this.tableLayoutPanelHighlight.Width = this.vaultPanel.Width;
		this.GetVaultList(false);

		// Now we always create the stash panel since everyone can have equipment
		this.CreateStashPanel();
		this.stashPanel.CurrentBag = 0; // set to default to the equipment panel

		// Add the Forge
		this.CreateForgePanel();
	}



	/// <summary>
	/// Used to toggle the upper display between the player panel or another vault.
	/// </summary>
	private void UpdateTopPanel()
	{
		if (this.showSecondaryVault)
		{
			this.playerPanel.Enabled = false;
			this.playerPanel.Visible = false;
			this.playerPanel.SackPanel.ClearSelectedItems();
			this.playerPanel.BagSackPanel.ClearSelectedItems();
			this.stashPanel.Visible = false;
			this.stashPanel.Enabled = false;
			this.secondaryVaultPanel.Enabled = true;
			this.secondaryVaultPanel.Visible = true;
			this.showVaulButton.Text = Resources.MainFormBtnShowPlayer;
			this.comboBoxCharacter.Enabled = false;
			this.comboBoxCharacter.Visible = false;

			this.bufferedFlowLayoutPanelsecondaryVaultList.Visible = true;


			this.lastStash = this.stashPanel.Stash;
			this.lastBag = this.stashPanel.CurrentBag;
			this.stashPanel.Player = null;
			this.stashPanel.Stash = null;
			if (this.stashPanel.CurrentBag != BagIdConstants.BAGID_TRANSFERSTASH)
			{
				this.stashPanel.SackPanel.ClearSelectedItems();
				this.stashPanel.CurrentBag = BagIdConstants.BAGID_TRANSFERSTASH;
			}

			this.vaultPanel.SackPanel.SecondaryVaultShown = true;
			this.stashPanel.SackPanel.SecondaryVaultShown = true;

			this.secondaryVaultPanel.SackPanel.IsSecondaryVault = true;
			this.secondaryVaultPanel.SackPanel.SecondaryVaultShown = true;
			this.GetSecondaryVaultList();
		}
		else
		{
			this.stashPanel.Visible = true;
			this.stashPanel.Enabled = true;
			this.secondaryVaultPanel.Enabled = false;
			this.secondaryVaultPanel.Visible = false;
			this.secondaryVaultPanel.SackPanel.ClearSelectedItems();
			this.playerPanel.Enabled = true;
			this.playerPanel.Visible = true;
			this.showVaulButton.Text = Resources.MainFormBtnPanelSelect;
			this.comboBoxCharacter.Enabled = true;
			this.comboBoxCharacter.Visible = true;

			this.bufferedFlowLayoutPanelsecondaryVaultList.Visible = false;

			this.stashPanel.Player = this.playerPanel.Player;
			if (this.lastStash is not null)
			{
				this.stashPanel.Stash = this.lastStash;
				if (this.lastBag != -1 && this.lastBag != this.stashPanel.CurrentBag)
				{
					this.stashPanel.CurrentBag = this.lastBag;
					this.stashPanel.SackPanel.ClearSelectedItems();
				}
			}

			this.vaultPanel.SackPanel.SecondaryVaultShown = false;
			this.stashPanel.SackPanel.SecondaryVaultShown = false;
			this.secondaryVaultPanel.SackPanel.SecondaryVaultShown = false;
		}
	}


	/// <summary>
	/// Callback for highlighting a new item.
	/// Updates the text box on the main form.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">SackPanelEventArgs data</param>
	private void NewItemHighlightedCallback(object? sender, SackPanelEventArgs e)
	{
		Item? item = e.Item;
		SackCollection? sack = e.Sack;
		var sackPanel = (SackPanel)sender!;
		if (item is null)
		{
			// Only do something if this sack is the "owner" of the current item highlighted.
			if (sack == this.lastSackHighlighted)
			{
				this.NotificationText.Text = string.Empty;

				// hide the tooltip
				ItemTooltip.HideTooltip();
			}
		}
		else
		{
			var itt = ItemTooltip.ShowTooltip(this.Services, item, sackPanel);

			if (itt.Data is { } data)
			{
				this.NotificationText.ForeColor = data.Item.ExtractTextColorOrItemColor(data.BaseItemInfoDescription);
				this.NotificationText.Text = data.FullNameBagTooltipClean;
			}
		}

		this.lastSackHighlighted = sack;
		this.lastSackPanelHighlighted = sackPanel;
	}

	/// <summary>
	/// Used to clear out selections on other panels if the user tries to select across multiple panels.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">SackPanelEventArgs data</param>
	private void ItemSelectedCallback(object? sender, SackPanelEventArgs e)
	{
		SackPanel? sackPanel = (SackPanel)sender!;

		if (this.playerPanel.SackPanel == sackPanel)
		{
			this.playerPanel.BagSackPanel.ClearSelectedItems();
			this.vaultPanel.SackPanel.ClearSelectedItems();
			this.secondaryVaultPanel.SackPanel.ClearSelectedItems();
			this.stashPanel.SackPanel.ClearSelectedItems();
		}
		else if (this.playerPanel.BagSackPanel == sackPanel)
		{
			this.playerPanel.SackPanel.ClearSelectedItems();
			this.vaultPanel.SackPanel.ClearSelectedItems();
			this.secondaryVaultPanel.SackPanel.ClearSelectedItems();
			this.stashPanel.SackPanel.ClearSelectedItems();
		}
		else if (this.vaultPanel.SackPanel == sackPanel)
		{
			this.playerPanel.SackPanel.ClearSelectedItems();
			this.playerPanel.BagSackPanel.ClearSelectedItems();
			this.secondaryVaultPanel.SackPanel.ClearSelectedItems();
			this.stashPanel.SackPanel.ClearSelectedItems();
		}
		else if (this.secondaryVaultPanel.SackPanel == sackPanel)
		{
			this.playerPanel.SackPanel.ClearSelectedItems();
			this.playerPanel.BagSackPanel.ClearSelectedItems();
			this.vaultPanel.SackPanel.ClearSelectedItems();
			this.stashPanel.SackPanel.ClearSelectedItems();
		}
		else if (this.stashPanel.SackPanel == sackPanel)
		{
			this.playerPanel.SackPanel.ClearSelectedItems();
			this.playerPanel.BagSackPanel.ClearSelectedItems();
			this.vaultPanel.SackPanel.ClearSelectedItems();
			this.secondaryVaultPanel.SackPanel.ClearSelectedItems();
		}
	}

	/// <summary>
	/// Used to clear the selection when a bag button is clicked.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">SackPanelEventArgs data</param>
	private void ClearAllItemsSelectedCallback(object? sender, SackPanelEventArgs e)
	{
		this.playerPanel.SackPanel.ClearSelectedItems();
		this.playerPanel.BagSackPanel.ClearSelectedItems();
		this.vaultPanel.SackPanel.ClearSelectedItems();
		this.secondaryVaultPanel.SackPanel.ClearSelectedItems();
		this.stashPanel.SackPanel.ClearSelectedItems();
	}

	/// <summary>
	/// Used for sending items between sacks or panels.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">SackPanelEventArgs data</param>
	private void AutoMoveItemCallback(object? sender, SackPanelEventArgs e)
	{
		SackPanel? sackPanel = (SackPanel)sender!;

		if (!this.DragInfo.IsAutoMoveActive)
			return;

		if (this.DragInfo.AutoMoveDestination == AutoMoveLocation.Stash)
		{
			this.AutoMoveItemToStash();
			return;
		}

		this.AutoMoveItemToPanel(sackPanel);
	}

	/// <summary>
	/// Resolves the destination panel and sack for auto-move operations.
	/// </summary>
	// This is a sack to sack move on the same panel.
	private (VaultPanel? Panel, SackPanel? SackPanel, int SackNumber) GetAutoMoveDestination(SackPanel sourcePanel)
	{
		var sacNumber = this.DragInfo.DestIndex;
		if (sacNumber > -1)
		{
			VaultPanel panel = sourcePanel.SackType == SackType.Vault && sourcePanel.IsSecondaryVault
				? this.secondaryVaultPanel
				: sourcePanel.SackType == SackType.Vault
					? this.vaultPanel
					: this.playerPanel;

			return (panel, sourcePanel, sacNumber);
		}

		return this.DragInfo.AutoMoveDestination switch
		{
			AutoMoveLocation.Vault => (this.vaultPanel, this.vaultPanel.SackPanel, this.vaultPanel.CurrentBag),
			AutoMoveLocation.Player => (this.playerPanel, ((PlayerPanel)this.playerPanel).SackPanel, 0),
			AutoMoveLocation.SecondaryVault => (this.secondaryVaultPanel, this.secondaryVaultPanel.SackPanel, this.secondaryVaultPanel.CurrentBag),
			_ => (null, null, 0)
		};
	}

	/// <summary>
	/// Handles auto-moving an item to the stash panel.
	/// </summary>
	// Special Case for moving to stash.
	private void AutoMoveItemToStash()
	{
		var target = AutoMovePlanner.StashTarget(
			this.stashPanel.CurrentBag
			, this.stashPanel.Player is not null
			, this.stashPanel.TransferStash is not null
			, this.stashPanel.RelicVaultStash is not null);

		// The equipment is shown: show the transfer stash instead.
		if (target.Bag != this.stashPanel.CurrentBag)
			this.stashPanel.CurrentBag = target.Bag;

		if (!target.IsAvailable)
		{
			// We have nowhere to send the item so cancel the move.
			this.DragInfo.Cancel();
			return;
		}

		if (this.DragInfo.Item is not { } dragItem)
			return;

		var stashSackPanel = this.stashPanel.SackPanel;
		var stashSack = stashSackPanel.ShownSack;
		// The same place as a drop on the stash: the stash file.
		var place = stashSackPanel.PlaceHere();

		// No room, an item the relic vault does not take, or a read-only character's stash: the item goes back.
		if (!this.Services.ItemMovementService.TryPlace(dragItem, stashSack, stashSackPanel.SackSize, place, this.DragInfo.MarkPlaced))
		{
			this.DragInfo.Cancel();
			stashSackPanel.NotifyIfReadOnlyCharacter(stashSack);
			return;
		}

		// Out of a vault, an item without a link follows the character's map.
		this.stashPanel.AdoptVaultMod(stashSack, [dragItem], stashSackPanel.SackSize);

		this.lastSackPanelHighlighted?.Invalidate();
		this.stashPanel.Refresh();
		BagButtonTooltip.InvalidateCache(stashSack);
	}

	/// <summary>
	/// Handles auto-moving an item to a non-stash panel.
	/// </summary>
	private void AutoMoveItemToPanel(SackPanel sourcePanel)
	{
		var (destinationPanel, destinationSackPanel, destSackNumber) = this.GetAutoMoveDestination(sourcePanel);
		var destinationSack = destinationPanel?.Player?.GetSack(destSackNumber);

		if (destinationPanel?.Player is null || destinationSackPanel is null || destinationSack is null)
		{
			// We have nowhere to send the item so cancel the move.
			this.DragInfo.Cancel();
			return;
		}

		if (this.DragInfo.Item is not { } dragItem)
			return;

		int destination = this.GetDestinationIndex(destinationSackPanel, destinationPanel.CurrentBag, destSackNumber);
		var place = this.Services.ItemMovementService.PlaceIn(destinationPanel.Player, destinationSackPanel.SackType, destination, destinationSack.StashType);

		// We have no space in the sack, or it is a read-only character's, so we cancel.
		if (!this.Services.ItemMovementService.TryPlace(dragItem, destinationSack, destinationSackPanel.SackSize, place, this.DragInfo.MarkPlaced))
		{
			this.DragInfo.Cancel();
			destinationSackPanel.NotifyIfReadOnlyCharacter(destinationSack);
			return;
		}

		// Like a dropped item, it takes the data of its new place (the vault's mod or the base game, or the character's map).
		destinationPanel.AdoptVaultMod(destinationSack, [dragItem], destinationSackPanel.SackSize);

		sourcePanel.Invalidate();
		destinationPanel.Refresh();
		BagButtonTooltip.InvalidateCache(destinationSack, destinationSackPanel.Sack);
	}

	/// <summary>
	/// Calculates the destination index accounting for panel type offsets.
	/// </summary>
	private int GetDestinationIndex(SackPanel destinationSackPanel, int currentBag, int sackNumber)
	{
		if (destinationSackPanel.SackType is SackType.Vault or SackType.Player)
			return sackNumber;

		return currentBag;
	}

	/// <summary>
	/// Handler for clicking the panel selection button.
	/// Switches between the player panel and seconday vault panel.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs strucure</param>
	private void PanelSelectButtonClick(object? sender, EventArgs e)
	{
		this.showSecondaryVault = !this.showSecondaryVault;
		this.UpdateTopPanel();
	}
}
