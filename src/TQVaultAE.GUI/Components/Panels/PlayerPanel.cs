//-----------------------------------------------------------------------
// <copyright file="PlayerPanel.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using TQVaultAE.Application.Session;
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

	/// <summary>The room of the main inventory, in cells: the game's 12 x 5; a larger grid scrolls in it.</summary>
	private readonly Size mainRoomCells;

	/// <summary>The viewports of the main inventory and of the bags (<see cref="SackViewport"/>).</summary>
	private readonly SackViewport mainViewport;
	private readonly SackViewport bagViewport;

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

		this.TabStop = false;

		this.BagPanelOffset = 1; // bag panel starts with bag #1
		this.mainRoomCells = panel1Size;
		this.mainSackPanel = new SackPanel(panel1Size.Width, panel1Size.Height, this.DragInfo, AutoMoveLocation.Player, services);
		this.ForwardEvents(this.mainSackPanel);

		// Both grids in viewports, placed by LayOut: a character's grids may be larger than the game's (an inventory mod).
		this.mainViewport = new SackViewport(this.mainSackPanel) { Name = "playerMainViewport" };
		this.Controls.Add(this.mainViewport);
		this.Controls.Remove(this.BagSackPanel);
		this.bagViewport = new SackViewport(this.BagSackPanel) { Name = "playerBagViewport" };
		this.Controls.Add(this.bagViewport);
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
	/// The grid an item sent to the character goes to (auto-move, emptied loadout): the main inventory, or the bag shown
	/// when the game data gives the main inventory no cells (a mod moving that room into the bags). Its
	/// <see cref="SackPanel.CurrentSack"/> is the character's sack.
	/// </summary>
	internal SackPanel InventoryTarget => HasCells(this.mainSackPanel) ? this.mainSackPanel : this.BagSackPanel;

	private static bool HasCells(SackPanel sack) => sack.SackSize.Width > 0 && sack.SackSize.Height > 0;

	/// <summary>The item grids drawn in the caravan frame: the main inventory and the bags.</summary>
	protected override IEnumerable<SackPanel> FramedSacks => [this.mainSackPanel, this.BagSackPanel];

	/// <summary>The summary tool tips of the character's bags are always shown.</summary>
	protected override bool ShowsBagSummary(int bag) => true;

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
	/// Sizes the panel from the game's rooms (main inventory 12 x 5, bags 8 x 5, whatever the character's grids) and places
	/// in them the viewports of its two grids side by side, the bag buttons above the bags on the left half of the bags'
	/// width (the sort button keeps the right end), a sort button above the right end of each grid. Without a main
	/// inventory (a mod moving it into the bags), the bags take both rooms.
	/// </summary>
	protected internal override void LayOut() => this.LayOut(toTopLeft: false);

	/// <param name="toTopLeft">Other grid sizes (another character): the grids are scrolled to their top-left corner.</param>
	private void LayOut(bool toTopLeft)
	{
		// Called by the base constructor, before the viewports exist: the constructor lays it out once they do.
		if (this.mainViewport is null || this.bagViewport is null)
			return;

		var main = this.mainSackPanel;
		var bags = this.BagSackPanel;
		main.FollowScale();
		bags.FollowScale();
		this.Size = new Size(
			((this.mainRoomCells.Width + this.PanelCells.Width) * UIService.ItemUnitSize) + Convert.ToInt32(24.0F * UIService.Scale)
				+ main.FrameGrowth + bags.FrameGrowth,
			(Math.Max(this.mainRoomCells.Height, this.PanelCells.Height) * UIService.ItemUnitSize) + Convert.ToInt32(58.0F * UIService.Scale)
				+ Math.Max(main.FrameGrowth, bags.FrameGrowth));

		int borderPad = Convert.ToInt32(2.0F * UIService.Scale);
		var mainSize = main.SizeFor(this.mainRoomCells);
		var mainRoom = new Rectangle(borderPad, this.Height - mainSize.Height - borderPad, mainSize.Width, mainSize.Height);
		var bagSize = bags.SizeFor(this.PanelCells);
		var bagRoom = new Rectangle(mainRoom.Right + borderPad, this.Height - bagSize.Height - borderPad, bagSize.Width, bagSize.Height);

		bool mainShown = HasCells(main);
		this.mainViewport.Visible = mainShown;
		if (mainShown)
			this.mainViewport.Place(mainRoom, new Point(mainRoom.Left, mainRoom.Bottom - main.Height), toTopLeft);
		else
			bagRoom = Rectangle.FromLTRB(mainRoom.Left, Math.Min(mainRoom.Top, bagRoom.Top), bagRoom.Right, bagRoom.Bottom);

		this.bagViewport.Place(bagRoom, new Point(bagRoom.Left, bagRoom.Bottom - bags.Height), toTopLeft);

		this.PlaceBagButtons(bagRoom.Location, bagSize.Width, this.BagButtons.Count * 2);

		foreach (var button in this.AutoSortButtons)
			button.FollowScale();
		this.AutoSortButtons[0].Location = new Point(mainRoom.Left, mainRoom.Top - this.AutoSortButtons[0].Height);
		this.AutoSortButtons[1].Location = new Point(bagRoom.Right - this.AutoSortButtons[1].Width, bagRoom.Top - this.AutoSortButtons[1].Height);
	}

	/// <summary>
	/// Assigns sack to buttons, and gives the grids the character's sizes: its game data's (a mod's), grown to hold every
	/// item its save keeps (<see cref="SackLayout.Covering"/>); the game's without a character.
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

		if (this.mainSackPanel is null)
			return;

		var (mainCells, bagCells) = this.CharacterGridSizes();
		if (mainCells != this.mainSackPanel.SackSize || bagCells != this.BagSackPanel.SackSize)
		{
			this.mainSackPanel.ResizeSackPanel(mainCells.Width, mainCells.Height);
			this.BagSackPanel.ResizeSackPanel(bagCells.Width, bagCells.Height);
		}

		this.LayOut(toTopLeft: true);

		// No sorting of a main inventory the character does not have.
		if (this.AutoSortButtons.Count > 0 && !HasCells(this.mainSackPanel))
			this.AutoSortButtons[0].Visible = false;
	}

	/// <summary>The sizes in cells of the character's main inventory and bags (see <see cref="AssignSacks"/>).</summary>
	private (Size Main, Size Bag) CharacterGridSizes()
	{
		if (this.Player is not { NumberOfSacks: > 0 } player)
			return (this.mainRoomCells, this.PanelCells);

		var balance = this.Services.CharacterStatsService.GetGameBalance();
		var bags = Enumerable.Range(1, player.NumberOfSacks - 1).Select(player.GetSack);
		return (SackLayout.Covering(balance.InventoryMainCells, [player.GetSack(0)]), SackLayout.Covering(balance.InventoryBagCells, bags));
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