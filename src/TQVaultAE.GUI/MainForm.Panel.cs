using TQVaultAE.Application;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Components;
using TQVaultAE.GUI.Models;
using TQVaultAE.GUI.Tooltip;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI;

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

		// Added support for custom map character list
		this.customMapText.Visible = false;
		if (GamePathResolver.IsCustom)
		{
			this.customMapText.Visible = true;
			this.customMapText.Text = string.Format(
				GamePathResolver.MapName.ToUpper().Contains(@"\CUSTOMMAPS\")
					? "Legacy : " + Resources.MainFormCustomMapLabel
					: "Steam : " + Resources.MainFormCustomMapLabel
				, this.PathIO.GetFileName(GamePathResolver.MapName)
			);
		}

		this.CreateVaultPanel(12); // # of bags in a vault.  This number is also buried in the CreateVault() function
		this.CreateSecondaryVaultPanel(12); // # of bags in a vault.  This number is also buried in the CreateVault() function
		this.secondaryVaultPanel.Enabled = false;
		this.secondaryVaultPanel.Visible = false;
		this.lastBag = -1;

		this.itemTextPanel.Size = new Size(this.vaultPanel.Width, Convert.ToInt32(22.0F * UIService.Scale));
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
			if (this.lastStash != null)
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
	private void NewItemHighlightedCallback(object sender, SackPanelEventArgs e)
	{
		Item item = e.Item;
		SackCollection sack = e.Sack;
		SackPanel sackPanel = (SackPanel)sender;
		if (item == null)
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
			var itt = ItemTooltip.ShowTooltip(this.ServiceProvider, item, sackPanel);

			this.NotificationText.ForeColor = itt.Data.Item.ExtractTextColorOrItemColor(itt.Data.BaseItemInfoDescription);
			this.NotificationText.Text = itt.Data.FullNameBagTooltipClean;
		}

		this.lastSackHighlighted = sack;
		this.lastSackPanelHighlighted = sackPanel;
	}

	/// <summary>
	/// Used to clear out selections on other panels if the user tries to select across multiple panels.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">SackPanelEventArgs data</param>
	private void ItemSelectedCallback(object sender, SackPanelEventArgs e)
	{
		SackPanel sackPanel = (SackPanel)sender;

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
	private void ClearAllItemsSelectedCallback(object sender, SackPanelEventArgs e)
	{
		this.playerPanel.SackPanel.ClearSelectedItems();
		this.playerPanel.BagSackPanel.ClearSelectedItems();
		this.vaultPanel.SackPanel.ClearSelectedItems();
		this.secondaryVaultPanel.SackPanel.ClearSelectedItems();
		this.stashPanel.SackPanel.ClearSelectedItems();
	}

	/// <summary>
	/// Callback for activating the search text box.
	/// Used when a hot key is pressed.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">SackPanelEventArgs data</param>
	private void ActivateSearchCallback(object sender, SackPanelEventArgs e)
		=> this.OpenSearchDialog();

	/// <summary>
	/// Used for sending items between sacks or panels.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">SackPanelEventArgs data</param>
	private void AutoMoveItemCallback(object sender, SackPanelEventArgs e)
	{
		SackPanel sackPanel = (SackPanel)sender;

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
	private (VaultPanel Panel, SackPanel SackPanel, int SackNumber) GetAutoMoveDestination(SackPanel sourcePanel)
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
	private void AutoMoveItemToStash()
	{
		if (!this.ValidateStashAvailability())
			return;

		Point location = this.stashPanel.SackPanel.FindOpenCells(this.DragInfo.Item.Width, this.DragInfo.Item.Height);
		if (location.X == -1 || !this.stashPanel.SackPanel.IsItemValidForPlacement(this.DragInfo.Item))
		{
			this.DragInfo.Cancel();
			return;
		}

		Item dragItem = this.DragInfo.Item;
		this.DragInfo.MarkPlaced();
		dragItem.PositionX = location.X;
		dragItem.PositionY = location.Y;
		this.stashPanel.SackPanel.Sack.AddItem(dragItem);
		this.UpdateItemLocation(dragItem, this.stashPanel.Player, this.stashPanel.SackPanel.Sack.SackType, this.stashPanel.SackPanel.CurrentSack, this.stashPanel.SackPanel.Sack.StashType);
		this.lastSackPanelHighlighted.Invalidate();
		this.stashPanel.Refresh();
		BagButtonTooltip.InvalidateCache(this.stashPanel.SackPanel.Sack);
	}

	/// <summary>
	/// Validates that the target stash is available for item placement.
	/// </summary>
	private bool ValidateStashAvailability()
	{
		if (this.stashPanel.CurrentBag == BagIdConstants.BAGID_PLAYERSTASH && this.stashPanel.Player == null)
		{
			this.DragInfo.Cancel();
			return false;
		}

		if (this.stashPanel.CurrentBag == BagIdConstants.BAGID_EQUIPMENTPANEL)
			this.stashPanel.CurrentBag = BagIdConstants.BAGID_TRANSFERSTASH;

		if (this.stashPanel.TransferStash == null && this.stashPanel.CurrentBag == BagIdConstants.BAGID_TRANSFERSTASH)
		{
			this.DragInfo.Cancel();
			return false;
		}

		if (this.stashPanel.RelicVaultStash == null && this.stashPanel.CurrentBag == BagIdConstants.BAGID_RELICVAULTSTASH)
		{
			this.DragInfo.Cancel();
			return false;
		}

		return true;
	}

	/// <summary>
	/// Handles auto-moving an item to a non-stash panel.
	/// </summary>
	private void AutoMoveItemToPanel(SackPanel sourcePanel)
	{
		var (destinationPanel, destinationSackPanel, destSackNumber) = this.GetAutoMoveDestination(sourcePanel);
		var destinationSack = destinationPanel.Player.GetSack(destSackNumber);

		if (destinationPanel?.Player == null || destinationSack == null)
		{
			this.DragInfo.Cancel();
			return;
		}

		SackCollection oldSack = destinationSackPanel.Sack;
		destinationSackPanel.Sack = destinationSack;

		Point location = destinationSackPanel.FindOpenCells(this.DragInfo.Item.Width, this.DragInfo.Item.Height);
		int destination = this.GetDestinationIndex(destinationSackPanel, destinationPanel.CurrentBag, destSackNumber);

		if (location.X == -1)
		{
			destinationSackPanel.Sack = oldSack;
			this.DragInfo.Cancel();
			return;
		}

		Item dragItem = this.DragInfo.Item;
		this.DragInfo.MarkPlaced();
		dragItem.PositionX = location.X;
		dragItem.PositionY = location.Y;
		destinationSackPanel.Sack.AddItem(dragItem);
		this.UpdateItemLocation(dragItem, destinationPanel.Player, destinationSackPanel.SackType, destination, destinationSackPanel.Sack.StashType);
		destinationSackPanel.Sack = oldSack;
		sourcePanel.Invalidate();
		destinationPanel.Refresh();
		BagButtonTooltip.InvalidateCache(destinationSackPanel.Sack, oldSack);
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
	private void PanelSelectButtonClick(object sender, EventArgs e)
	{
		this.showSecondaryVault = !this.showSecondaryVault;
		this.UpdateTopPanel();
	}

	/// <summary>
	/// Updates the item's location properties after it has been moved to a new container.
	/// This ensures search results always reflect the current location.
	/// </summary>
	/// <param name="item">The item that was moved</param>
	/// <param name="destinationPlayer">The destination PlayerCollection (vault/player/stash)</param>
	/// <param name="destinationSackType"></param>
	/// <param name="destinationSackNumber"></param>
	private void UpdateItemLocation(Item item, PlayerCollection destinationPlayer, SackType destinationSackType, int destinationSackNumber, StashType? destinationStashType)
	{
		if (item == null || destinationPlayer == null)
			return;

		// Update the item's location properties to reflect its new home
		item.Place.Path = destinationPlayer.PlayerFile;
		item.Place.Name = this.GamePathResolver.GetNameFromFile(destinationPlayer.PlayerFile)
			?? GamePathResolver.GetVaultNameFromPath(destinationPlayer.PlayerFile);

		// Calculate sack number based on sack type
		switch (destinationSackType)
		{
			case SackType.Equipment:
				item.Place.SackNumber = BagIdConstants.BAGID_EQUIPMENTPANEL;
				break;
			case SackType.Stash:
				item.Place.SackNumber = destinationStashType is not null ? (int)destinationStashType : destinationSackNumber;
				break;
			case SackType.Vault:
			case SackType.Player:
			default:
				// For vaults and player sacks, use the current sack index
				item.Place.SackNumber = destinationSackNumber;
				break;
		}

		item.Place.SackType = destinationSackType;
		item.Place.StashType = destinationStashType;
	}
}
