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

		this.TabStop = false;
		this.Font = new Font(this.Font.FontFamily, this.Font.SizeInPoints * UIService.FontZoom, this.Font.Style);

		this.BagPanelOffset = 1; // bag panel starts with bag #1
		this.mainSackPanel = new SackPanel(panel1Size.Width, panel1Size.Height, this.DragInfo, AutoMoveLocation.Player, services);
		this.mainSackPanel.OnNewItemHighlighted += new EventHandler<SackPanelEventArgs>(this.NewItemHighlightedCallback);
		this.mainSackPanel.OnAutoMoveItem += new EventHandler<SackPanelEventArgs>(this.AutoMoveItemCallback);
		this.mainSackPanel.OnClearAllItemsSelected += new EventHandler<SackPanelEventArgs>(this.ClearAllItemsSelectedCallback);
		this.mainSackPanel.OnItemSelected += new EventHandler<SackPanelEventArgs>(this.ItemSelectedCallback);
		this.Controls.Add(this.mainSackPanel);
		this.mainSackPanel.MaxSacks = 1;
		this.mainSackPanel.SackType = SackType.Player;
		this.mainSackPanel.IsMainPlayerPanel = true;

		this.BagSackPanel.IsPlayerBagPanel = true;
		this.LayOut();

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
		return this.NewSortButton(buttonNumber, false);
	}

	/// <summary>
	/// Sizes the panel and places its two sacks side by side, the bag buttons above the bags on the left half of the bags'
	/// width (the sort button keeps the right end), a sort button above the right end of each sack.
	/// </summary>
	protected internal override void LayOut()
	{
		// Called by the base constructor, before the main sack exists: the constructor lays it out once it does.
		if (this.mainSackPanel is null)
			return;

		var main = this.mainSackPanel;
		var bags = this.BagSackPanel;
		main.FollowScale();
		bags.FollowScale();
		this.Size = new Size(
			((main.SackSize.Width + this.PanelCells.Width) * UIService.ItemUnitSize) + Convert.ToInt32(24.0F * UIService.Scale),
			(Math.Max(main.SackSize.Height, this.PanelCells.Height) * UIService.ItemUnitSize) + Convert.ToInt32(58.0F * UIService.Scale));

		int borderPad = Convert.ToInt32(2.0F * UIService.Scale);
		main.Location = new Point(borderPad, this.Height - main.Height - borderPad);
		bags.Location = new Point(main.Right + borderPad, this.Height - bags.Height - borderPad);

		this.PlaceBagButtons(bags.Location, bags.Width, this.BagButtons.Count * 2);

		foreach (var button in this.AutoSortButtons)
			button.FollowScale();
		this.AutoSortButtons[0].Location = new Point(main.Left, main.Top - this.AutoSortButtons[0].Height);
		this.AutoSortButtons[1].Location = new Point(bags.Right - this.AutoSortButtons[1].Width, bags.Top - this.AutoSortButtons[1].Height);
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