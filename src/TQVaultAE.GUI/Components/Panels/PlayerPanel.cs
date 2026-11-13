//-----------------------------------------------------------------------
// <copyright file="PlayerPanel.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using TQVaultAE.Application;
using TQVaultAE.Config;
using TQVaultAE.GUI.Components.Buttons;
using TQVaultAE.Presentation.Win32;

namespace TQVaultAE.GUI.Components.Panels;

using System;
using System.Drawing;
using System.Windows.Forms;
using TQVaultAE.GUI.Models;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Presentation;

/// <summary>
/// Class for holding the UI functions on the player panel.
/// </summary>
public class PlayerPanel : VaultPanel
{
	/// <summary>
	/// Main sack panel instance
	/// </summary>
	private SackPanel mainSackPanel;

	/// <summary>
	/// Initializes a new instance of the PlayerPanel class.
	/// </summary>
	/// <param name="dragInfo">Instance of the ItemDragInfo</param>
	/// <param name="numberOfBags">Number of bags in this panel</param>
	/// <param name="panel1Size">Main panel size</param>
	/// <param name="panel2Size">Secondary panel size for players who have the additional game sacks</param>
	public PlayerPanel(ItemDragInfo dragInfo, int numberOfBags, Size panel1Size, Size panel2Size, GuiServices services)
		: base(dragInfo, numberOfBags, panel2Size, 2, AutoMoveLocation.Player, services)
	{
		this.Text = Resources.PlayerPanelNoPlayer;
		this.NoPlayerString = Resources.PlayerPanelNoPlayer;

		this.Size = new Size(
			((panel1Size.Width + panel2Size.Width) * UIService.ItemUnitSize) + Convert.ToInt32(24.0F * UIService.Scale),
			(Math.Max(panel1Size.Height, panel2Size.Height) * UIService.ItemUnitSize) + Convert.ToInt32(58.0F * UIService.Scale));
		this.TabStop = false;
		this.Font = new Font(this.Font.FontFamily, this.Font.SizeInPoints * UIService.FontZoom, this.Font.Style);

		int borderPad = Convert.ToInt32(2.0F * UIService.Scale);

		this.BagPanelOffset = 1; // bag panel starts with bag #1
		this.mainSackPanel = new SackPanel(panel1Size.Width, panel1Size.Height, this.DragInfo, AutoMoveLocation.Player, services);
		this.mainSackPanel.SetLocation(new Point(borderPad, this.Size.Height - this.mainSackPanel.Size.Height - borderPad));
		this.mainSackPanel.OnNewItemHighlighted += new EventHandler<SackPanelEventArgs>(this.NewItemHighlightedCallback);
		this.mainSackPanel.OnAutoMoveItem += new EventHandler<SackPanelEventArgs>(this.AutoMoveItemCallback);
		this.mainSackPanel.OnClearAllItemsSelected += new EventHandler<SackPanelEventArgs>(this.ClearAllItemsSelectedCallback);
		this.mainSackPanel.OnItemSelected += new EventHandler<SackPanelEventArgs>(this.ItemSelectedCallback);
		this.Controls.Add(this.mainSackPanel);
		this.mainSackPanel.MaxSacks = 1;
		this.mainSackPanel.SackType = SackType.Player;
		this.mainSackPanel.IsMainPlayerPanel = true;

		this.BagSackPanel.SetLocation(new Point(this.mainSackPanel.Right + borderPad, this.Size.Height - this.BagSackPanel.Size.Height - borderPad));
		this.BagSackPanel.IsPlayerBagPanel = true;

		// Recalculate the button sizing and placement since we moved the BagSackPanel.
		if (this.BagButtons is not null && this.BagButtons.Count > 0)
		{
			float buttonWidth = (float)Images.inventorybagup01.Width;
			float buttonHeight = (float)Images.inventorybagup01.Height;
			float pad = 2.0F;
			float slotWidth = buttonWidth + (2.0F * pad);

			// we need to scale down the bag size depending on the # we have
			// but keep room for the autosort button so the buttons only use half of the panel size.
			float scale = this.GetBagButtonScale(slotWidth, (this.BagButtons.Count * 2));
			float bagSlotWidth = scale * slotWidth;

			int index = 0;
			foreach (BagButtonBase button in this.BagButtons)
			{
				button.Size = new Size((int)Math.Round(scale * buttonWidth), (int)Math.Round(scale * buttonHeight));
				float offset = (bagSlotWidth * index) + ((bagSlotWidth - button.Width) / 2.0F);

				button.Location = new Point(this.BagSackPanel.Location.X + (int)Math.Round(offset), this.BagSackPanel.Location.Y - button.Height);
				index++;
			}
		}

		// Move the autosort buttons to their place since the panels got moved.
		AutoSortButtons[0].Location = new Point(this.mainSackPanel.Location.X, this.mainSackPanel.Location.Y - AutoSortButtons[0].Height);
		AutoSortButtons[1].Location = new Point(
			this.BagSackPanel.Location.X + this.BagSackPanel.Width - AutoSortButtons[1].Width,
			this.BagSackPanel.Location.Y - AutoSortButtons[1].Height);

		this.BagSackPanel.SackType = SackType.Player;
	}

	/// <summary>
	/// Gets or sets the player instance
	/// </summary>
	public new PlayerCollection? Player
	{
		get => base.Player;
		set
		{
			base.Player = value;
			this.mainSackPanel.PlayerCollection = value;
		}
	}

	/// <summary>
	/// Gets the SackPanel instance
	/// </summary>
	public new SackPanel SackPanel
	{
		get => (this.mainSackPanel is not null) ? this.mainSackPanel : this.BagSackPanel;
	}

	/// <summary>
	/// Tooltip callback for displaying the bag summary
	/// </summary>
	/// <param name="windowHandle">Window handle</param>
	/// <returns>String containing the tooltip text</returns>
	public override void ToolTipCallback(int windowHandle)
	{
		base.ToolTipCallback(windowHandle);
	}

	/// <summary>
	/// Creates a new Autosort button
	/// </summary>
	/// <param name="buttonNumber">Number of the autosort button we are creating</param>
	/// <returns>A new AutoSortButton instance</returns>
	protected override AutoSortButton CreateAutoSortButton(int buttonNumber)
	{
		AutoSortButton button = this.NewSortButton(buttonNumber, false);

		// Temporary location since we will be moving the panel later.
		// This is called from the base constructor.
		button.Location = new Point(
			this.BagSackPanel.Location.X + this.BagSackPanel.Width - button.Width,
			this.BagSackPanel.Location.Y - button.Height);

		return button;
	}

	/// <summary>
	/// Assigns sack to buttons
	/// </summary>
	protected override void AssignSacks()
	{
		if (this.mainSackPanel is not null)
		{
			if ((this.Player is null) || (this.Player.NumberOfSacks < 1))
			{
				this.mainSackPanel.Sack = null;
			}
			else
			{
				this.mainSackPanel.Sack = this.Player.GetSack(0);
				this.mainSackPanel.MaxSacks = this.Player.NumberOfSacks;
			}
		}

		base.AssignSacks();
	}

	/// <summary>
	/// The sack a sort button sorts: button 0 the main inventory, button 1 the bags.
	/// </summary>
	protected override SackPanel SortTarget(AutoSortButton button)
		=> button.ButtonNumber == 0 ? this.mainSackPanel : this.BagSackPanel;

	/// <summary>Each sort button keeps its own order: button 0 the main inventory's, button 1 the bags'.</summary>
	protected override SortButtonKey SortKey(AutoSortButton button)
		=> button.ButtonNumber == 0 ? SortButtonKey.Player : SortButtonKey.PlayerBag;
}