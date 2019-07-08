using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using TQVaultAE.Data;
using TQVaultAE.Entities;
using TQVaultAE.GUI.Components;
using TQVaultAE.GUI.Models;
using TQVaultAE.Presentation;
using TQVaultAE.Presentation.Html;

namespace TQVaultAE.GUI
{
	internal partial class MainForm
	{

		/// <summary>
		/// Creates the form's internal panels
		/// </summary>
		private void CreatePanels()
		{
			this.CreatePlayerPanel();

			// Put the secondary vault list on top of the player list drop down
			// since only one can be shown at a time.
			this.secondaryVaultListComboBox.Location = this.characterComboBox.Location;
			this.secondaryVaultListComboBox.Enabled = false;
			this.secondaryVaultListComboBox.Visible = false;

			this.GetPlayerList();

			// Added support for custom map character list
			if (TQData.IsCustom)
			{
				this.customMapText.Visible = true;
				this.customMapText.Text = string.Format(CultureInfo.CurrentCulture, Resources.MainFormCustomMapLabel, Path.GetFileName(TQData.MapName));
			}
			else
			{
				this.customMapText.Visible = false;
			}

			this.CreateVaultPanel(12); // # of bags in a vault.  This number is also buried in the CreateVault() function
			this.CreateSecondaryVaultPanel(12); // # of bags in a vault.  This number is also buried in the CreateVault() function
			this.secondaryVaultPanel.Enabled = false;
			this.secondaryVaultPanel.Visible = false;
			this.lastBag = -1;

			int textPanelOffset = Convert.ToInt32(18.0F * UIService.UI.Scale);
			this.itemTextPanel.Size = new Size(this.vaultPanel.Width, Convert.ToInt32(22.0F * UIService.UI.Scale));
			this.itemTextPanel.Location = new Point(this.vaultPanel.Location.X, this.ClientSize.Height - (this.itemTextPanel.Size.Height + textPanelOffset));
			this.itemText.Width = this.itemTextPanel.Width - Convert.ToInt32(4.0F * UIService.UI.Scale);
			this.GetVaultList(false);

			// Now we always create the stash panel since everyone can have equipment
			this.CreateStashPanel();
			this.stashPanel.CurrentBag = 0; // set to default to the equipment panel
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
				this.panelSelectButton.Text = Resources.MainFormBtnShowPlayer;
				this.characterComboBox.Enabled = false;
				this.characterComboBox.Visible = false;
				this.secondaryVaultListComboBox.Enabled = true;
				this.secondaryVaultListComboBox.Visible = true;
				this.characterLabel.Text = Resources.MainForm2ndVault;
				this.lastStash = this.stashPanel.Stash;
				this.lastBag = this.stashPanel.CurrentBag;
				this.stashPanel.Player = null;
				this.stashPanel.Stash = null;
				if (this.stashPanel.CurrentBag != 1)
				{
					this.stashPanel.SackPanel.ClearSelectedItems();
					this.stashPanel.CurrentBag = 1;
				}

				this.vaultPanel.SackPanel.SecondaryVaultShown = true;
				this.stashPanel.SackPanel.SecondaryVaultShown = true;

				this.secondaryVaultPanel.SackPanel.IsSecondaryVault = true;
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
				this.panelSelectButton.Text = Resources.MainFormBtnPanelSelect;
				this.characterComboBox.Enabled = true;
				this.characterComboBox.Visible = true;
				this.secondaryVaultListComboBox.Enabled = false;
				this.secondaryVaultListComboBox.Visible = false;
				this.characterLabel.Text = Resources.MainFormLabel1;
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
					this.itemText.Text = string.Empty;

					// hide the tooltip
					this.tooltipText = null;
					this.tooltip.ChangeText(this.tooltipText);
				}
			}
			else
			{
				var result = ItemHtmlHelper.NewItemHighlightedTooltip(item);
				this.itemText.ForeColor = result.ForeColor;
				this.itemText.Text = result.FriendlyName;
				this.tooltipText = result.TooltipText;
				this.tooltip.ChangeText(this.tooltipText);
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
		{
			this.OpenSearchDialog();
		}

		/// <summary>
		/// Used for sending items between sacks or panels.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">SackPanelEventArgs data</param>
		private void AutoMoveItemCallback(object sender, SackPanelEventArgs e)
		{
			SackPanel sackPanel = (SackPanel)sender;

			// Make sure we have to move something.
			if (this.dragInfo.IsAutoMoveActive)
			{
				SackCollection oldSack = null;
				VaultPanel destinationPlayerPanel = null;
				int sackNumber = 0;

				SackPanel destinationSackPanel = null;
				if (this.dragInfo.AutoMove < AutoMoveLocation.Vault)
				{
					// This is a sack to sack move on the same panel.
					destinationSackPanel = sackPanel;
					switch (sackPanel.SackType)
					{
						case SackType.Vault:
							{
								if (sackPanel.IsSecondaryVault)
								{
									destinationPlayerPanel = this.secondaryVaultPanel;
								}
								else
								{
									destinationPlayerPanel = this.vaultPanel;
								}

								break;
							}

						default:
							{
								destinationPlayerPanel = this.playerPanel;
								break;
							}
					}

					sackNumber = (int)this.dragInfo.AutoMove;
				}
				else if (this.dragInfo.AutoMove == AutoMoveLocation.Vault)
				{
					// Vault
					destinationPlayerPanel = this.vaultPanel;
					destinationSackPanel = destinationPlayerPanel.SackPanel;
					sackNumber = destinationPlayerPanel.CurrentBag;
				}
				else if (this.dragInfo.AutoMove == AutoMoveLocation.Player)
				{
					// Player
					destinationPlayerPanel = this.playerPanel;
					destinationSackPanel = ((PlayerPanel)destinationPlayerPanel).SackPanel;

					// Main Player panel
					sackNumber = 0;
				}
				else if (this.dragInfo.AutoMove == AutoMoveLocation.SecondaryVault)
				{
					// Secondary Vault
					destinationPlayerPanel = this.secondaryVaultPanel;
					destinationSackPanel = destinationPlayerPanel.SackPanel;
					sackNumber = destinationPlayerPanel.CurrentBag;
				}

				// Special Case for moving to stash.
				if (this.dragInfo.AutoMove == AutoMoveLocation.Stash)
				{
					// Check if we are moving to the player's stash
					if (this.stashPanel.CurrentBag == 2 && this.stashPanel.Player == null)
					{
						// We have nowhere to send the item so cancel the move.
						this.dragInfo.Cancel();
						return;
					}

					// Check for the equipment panel
					if (this.stashPanel.CurrentBag == 0)
					{
						// Equipment Panel is active so switch to the transfer stash.
						this.stashPanel.CurrentBag = 1;
					}

					// Check the transfer stash
					if (this.stashPanel.TransferStash == null && this.stashPanel.CurrentBag == 1)
					{
						// We have nowhere to send the item so cancel the move.
						this.dragInfo.Cancel();
						return;
					}

					// Check the relic vault stash
					if (this.stashPanel.RelicVaultStash == null && this.stashPanel.CurrentBag == 3)
					{
						// We have nowhere to send the item so cancel the move.
						this.dragInfo.Cancel();
						return;
					}

					// See if we have an open space to put the item.
					Point location = this.stashPanel.SackPanel.FindOpenCells(this.dragInfo.Item.Width, this.dragInfo.Item.Height);

					// We have no space in the sack so we cancel.
					if (location.X == -1)
					{
						this.dragInfo.Cancel();
					}
					else
					{
						Item dragItem = this.dragInfo.Item;

						if (!this.stashPanel.SackPanel.IsItemValidForPlacement(dragItem))
						{
							this.dragInfo.Cancel();
							return;
						}

						// Use the same method as if we used to mouse to pickup and place the item.
						this.dragInfo.MarkPlaced(-1);
						dragItem.PositionX = location.X;
						dragItem.PositionY = location.Y;
						this.stashPanel.SackPanel.Sack.AddItem(dragItem);

						this.lastSackPanelHighlighted.Invalidate();
						this.stashPanel.Refresh();
					}
				}
				else
				{
					// The stash is not involved.
					if (destinationPlayerPanel.Player == null)
					{
						// We have nowhere to send the item so cancel the move.
						this.dragInfo.Cancel();
						return;
					}

					// Save the current sack.
					oldSack = destinationSackPanel.Sack;

					// Find the destination sack.
					destinationSackPanel.Sack = destinationPlayerPanel.Player.GetSack(sackNumber);

					// See if we have an open space to put the item.
					Point location = destinationSackPanel.FindOpenCells(this.dragInfo.Item.Width, this.dragInfo.Item.Height);

					// CurrentBag only returns the values for the bag panels and is zero based.  Main sack is not included.
					int destination = destinationPlayerPanel.CurrentBag;

					// We need to accout for the player panel offsets.
					if (sackPanel.SackType == SackType.Sack)
					{
						destination++;
					}
					else if (sackPanel.SackType == SackType.Player)
					{
						destination = 0;
					}

					// We either have no space or are sending the item to the same sack so we cancel.
					if (location.X == -1 || (int)this.dragInfo.AutoMove == destination)
					{
						destinationSackPanel.Sack = oldSack;
						this.dragInfo.Cancel();
					}
					else
					{
						Item dragItem = this.dragInfo.Item;

						// Use the same method as if we used to mouse to pickup and place the item.
						this.dragInfo.MarkPlaced(-1);
						dragItem.PositionX = location.X;
						dragItem.PositionY = location.Y;
						destinationSackPanel.Sack.AddItem(dragItem);

						// Set it back to the original sack so the display does not change.
						destinationSackPanel.Sack = oldSack;
						sackPanel.Invalidate();
						destinationPlayerPanel.Refresh();
					}
				}
			}
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
	}
}
