//-----------------------------------------------------------------------
// <copyright file="StashPanel.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Results;
using TQVaultAE.GUI.Components.Buttons;
using TQVaultAE.GUI.Components.Layout;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Dialogs;
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
/// Class for handling the stash panel ui functions
/// </summary>
public partial class StashPanel : VaultPanel, IScalingControl
{
	#region StashPanel Fields

	/// <summary>
	/// Localized tab names
	/// </summary>
	private static string[] buttonNames =
	[
		Resources.StashPanelBtnEquipment,
		Resources.StashPanelBtnTransferArea,
		Resources.StashPanelBtnStorageArea,
		Resources.GlobalRelicVaultStash
	];

	/// <summary>
	/// Stash file instance
	/// </summary>
	private Stash? stash;

	/// <summary>
	/// Transfer stash file instance
	/// </summary>
	private Stash? transferStash;

	/// <summary>
	/// Relic Vault stash file instance
	/// </summary>
	private Stash? relicVaultStash;

	/// <summary>
	/// background bitmap
	/// </summary>
	private Bitmap? background;

	/// <summary>
	/// Currently selected bag
	/// </summary>
	private int currentBag;

	/// <summary>
	/// Maximum size of the panel.  Uses the initial constructor size.
	/// </summary>
	private Size maxPanelSize;

	/// <summary>
	/// Equipment Panel instance
	/// </summary>
	private EquipmentPanel equipmentPanel;

	/// <summary>
	/// Gets the equipment panel instance
	/// </summary>
	internal EquipmentPanel EquipmentPanel => equipmentPanel;

	/// <summary>
	/// Background image for the equipment panel
	/// </summary>
	private Bitmap? equipmentBackground;

	/// <summary>
	/// Background image for the stash vault panels
	/// </summary>
	private Bitmap? stashBackground;

	private BufferedTableLayoutPanel PlayerPanel;

	string? DisplayPlayerInfoLastName;
	private const int NORMAL_PLAYERINFO_HEIGHT = 480;//BGImage = 490
	private const int NORMAL_PLAYERINFO_WIDTH = 186;//BGImage = 186
	private const int NORMAL_PLAYERINFO_TOPRIGHT_ORIGIN_WIDTH = 17;
	private const int NORMAL_PLAYERINFO_TOPRIGHT_ORIGIN_HEIGHT = 25;

	int PLAYERINFO_TOPRIGHT => this.ClientRectangle.Right - Convert.ToInt32(NORMAL_PLAYERINFO_TOPRIGHT_ORIGIN_WIDTH * this.UIService.Scale);
	int PLAYERINFO_TOPHEIGHT =>
		this.BagSackPanel.ClientRectangle.Top + this.BagButtons[0].Size.Height + UIService.HalfUnitSize
		+ Convert.ToInt32(NORMAL_PLAYERINFO_TOPRIGHT_ORIGIN_HEIGHT * this.UIService.Scale);
	int PLAYERINFO_WIDTH => Convert.ToInt32(NORMAL_PLAYERINFO_WIDTH * this.UIService.Scale);
	int PLAYERINFO_HEIGHT => Convert.ToInt32(NORMAL_PLAYERINFO_HEIGHT * this.UIService.Scale);

	private readonly ITranslationService TranslationService;

	private System.ComponentModel.IContainer components;
	private ContextMenuStrip buttonContextMenuStrip;
	private ToolStripMenuItem toolStripMenuItemEnableTooltip;
	private ToolStripMenuItem toolStripMenuItemDisableTooltip;

	#endregion StashPanel Fields

#if DEBUG
#pragma warning disable CS8618 // Designer only: the runtime constructor sets the services and state.
	public StashPanel() => InitializeComponent();
#pragma warning restore CS8618
#endif

	/// <summary>
	/// Initializes a new instance of the StashPanel class.  Hard coded to 4 stash buttons and no autosort buttons.
	/// </summary>
	/// <param name="dragInfo">ItemDragInfo instance</param>
	/// <param name="panelSize">Size of the panel in cells</param>
	/// <param name="tooltip">ToolTip instance</param>
	public StashPanel(ItemDragInfo dragInfo, Size panelSize, GuiServices services)
		: base(dragInfo, 4, panelSize, 0, AutoMoveLocation.Stash, services)
	{
		InitializeComponent();

		this.TranslationService = this.Services.TranslationService;

		SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

		this.equipmentPanel = new EquipmentPanel(16, 14, dragInfo, AutoMoveLocation.Stash, services);
		this.Controls.Add(this.equipmentPanel);
		this.equipmentPanel.OnNewItemHighlighted += new EventHandler<SackPanelEventArgs>(this.NewItemHighlightedCallback);
		this.equipmentPanel.OnAutoMoveItem += new EventHandler<SackPanelEventArgs>(this.AutoMoveItemCallback);
		this.equipmentPanel.OnActivateSearch += new EventHandler<SackPanelEventArgs>(this.ActivateSearchCallback);
		this.equipmentPanel.OnItemSelected += new EventHandler<SackPanelEventArgs>(this.ItemSelectedCallback);
		this.equipmentPanel.OnClearAllItemsSelected += new EventHandler<SackPanelEventArgs>(this.ClearAllItemsSelectedCallback);
		this.equipmentPanel.OnResizeForm += new EventHandler<ResizeEventArgs>(this.ResizeFormCallback);
		this.equipmentPanel.OnEquipmentChanged += new EventHandler<SackPanelEventArgs>(this.EquipmentChangedCallback);
		this.BagSackPanel.ShownStash = () => this.ShownStash;

		this.Text = Resources.StashPanelText;
		this.NoPlayerString = Resources.StashPanelText;
		this.Size = new Size(
			(panelSize.Width * UIService.ItemUnitSize) + Convert.ToInt32(10.0F * UIService.Scale) + BorderPad,
			(panelSize.Height * UIService.ItemUnitSize) + Convert.ToInt32(60.0F * UIService.Scale) + BorderPad);
		this.Paint += new PaintEventHandler(this.PaintCallback);

		this.BagSackPanel.SetLocation(new Point(BorderPad, this.Size.Height - (this.BagSackPanel.Size.Height + BorderPad)));
		this.BagSackPanel.MaxSacks = 1;
		this.BagSackPanel.Anchor = AnchorStyles.None;

		//this.EquipmentBackground = Images.Equipment_bg_new;
		this.background = this.EquipmentBackground = Images.equipment_bg_and_char;
		this.StashBackground = Images.caravan_bg;

		// Set up the inital font size
		if (UIService.FontZoom != 1.0F)
			this.Font = new Font(this.Font.FontFamily, this.Font.SizeInPoints * UIService.FontZoom, this.Font.Style);

		// Now that the buttons are set we can move the panel
		this.BagSackPanel.SetLocation(new Point(
			BorderPad,
			this.BagButtons[0].Location.Y + this.BagButtons[0].Size.Height + UIService.HalfUnitSize));

		int offsetX = (panelSize.Width - this.equipmentPanel.SackSize.Width) * UIService.HalfUnitSize;
		if (offsetX < 0)
			offsetX = 0;

		int offsetY = (panelSize.Height - this.equipmentPanel.SackSize.Height) * UIService.HalfUnitSize;
		if (offsetY < 0)
			offsetY = 0;

		this.equipmentPanel.SetLocation(new Point(
			offsetX,
			this.BagButtons[0].Location.Y + this.BagButtons[0].Size.Height + offsetY));

		this.maxPanelSize = panelSize;

		this.BagSackPanel.Visible = false;
		this.BagSackPanel.Enabled = false;

		#region Init Player Panel

		// Based on testing at scale 1
		var table = new BufferedTableLayoutPanel()
		{
			Location = new Point(
				PLAYERINFO_TOPRIGHT - PLAYERINFO_WIDTH
				, PLAYERINFO_TOPHEIGHT
			),
			Size = new Size(PLAYERINFO_WIDTH, PLAYERINFO_HEIGHT),
			//CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
			Margin = new Padding(0),
			Padding = new Padding(1),
			BackColor = Color.Transparent,
		};
		table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
		table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));

		this.Controls.Add(table);
		this.PlayerPanel = table;

		table.BringToFront();

		this.equipmentPanel.VisibleChanged += EquipmentPanel_VisibleChanged;

		#endregion

		this.buttonContextMenuStrip.Font = FontService.GetFont(9.0F * UIService.FontZoom);

		this.toolStripMenuItemEnableTooltip.Text = Resources.PlayerPanelMenuEnableTooltip;
		this.toolStripMenuItemDisableTooltip.Text = Resources.PlayerPanelMenuDisableTooltip;

		foreach (var bag in this.BagButtons)
			bag.ContextMenuStrip = this.buttonContextMenuStrip;
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
			this.equipmentPanel.PlayerCollection = value;
		}
	}

	/// <summary>
	/// Forces an update to the player panel.
	/// </summary>
	public void UpdatePlayerInfo()
	{
		// Force an update of the Player Info Panel.
		DisplayPlayerInfoLastName = null;
		PlayerPanel.Invalidate();
	}

	private void EquipmentPanel_VisibleChanged(object? sender, EventArgs e)
		=> DisplayPlayerInfo();

	/// <summary>
	/// Used to signal that the gear stats bonuses have been re-calculated.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">SackPanelEventArgs object</param>
	private void EquipmentChangedCallback(object? sender, SackPanelEventArgs e)
	{
		// Force an update of the Player Info Panel.
		DisplayPlayerInfoLastName = null;
		DisplayPlayerInfo();
	}

	private void EditButton_Click(object? sender, EventArgs e)
	{
		var dlg = this.Services.Forms.Create<CharacterEditDialog>();
		dlg.PlayerCollection = this.ShownPlayer;
		dlg.ShowDialog();
		this.GetSkillStatBonus();
		this.DisplayPlayerInfoLastName = null;
		this.DisplayPlayerInfo();
	}

	#region StashPanel Properties

	/// <summary>
	/// Gets or sets the stash instance
	/// </summary>
	public Stash? Stash
	{
		get => this.stash;

		set
		{
			this.stash = value;
			this.Text = Resources.StashPanelText;
			this.AssignSacks();
		}
	}

	/// <summary>
	/// Gets the stash of the selected bag, null for the equipment.
	/// </summary>
	internal Stash? ShownStash => this.currentBag switch
	{
		BagIdConstants.BAGID_PLAYERSTASH => this.stash,
		BagIdConstants.BAGID_TRANSFERSTASH => this.transferStash,
		BagIdConstants.BAGID_RELICVAULTSTASH => this.relicVaultStash,
		_ => null,
	};

	/// <summary>
	/// Gets or sets the transfer stash instance
	/// </summary>
	public Stash? TransferStash
	{
		get => this.transferStash;

		set
		{
			this.transferStash = value;
			this.Text = Resources.StashPanelText;
			this.AssignSacks();
		}
	}

	/// <summary>
	/// Gets or sets the relic vault stash instance
	/// </summary>
	public Stash? RelicVaultStash
	{
		get => this.relicVaultStash;

		set
		{
			this.relicVaultStash = value;
			this.Text = Resources.StashPanelText;
			this.AssignSacks();
		}
	}

	/// <summary>
	/// Gets the SackPanel instance
	/// </summary>
	public new SackPanel SackPanel
	{
		get
		{
			if (this.CurrentBag == 0 && this.equipmentPanel is not null)
				return this.equipmentPanel;

			return this.BagSackPanel;
		}
	}

	/// <summary>
	/// Gets or sets the background image for the equipment panel scaled by the panel size
	/// </summary>
	public Bitmap? EquipmentBackground
	{
		get
		{
			if (UIService.Scale != 1.0F && this.equipmentBackground is not null)
			{
				// We need to scale this since we use it to redraw the background.
				return new Bitmap(
					this.equipmentBackground,
					(this.maxPanelSize.Width * UIService.ItemUnitSize) + (Convert.ToInt32(SackPanel.BorderWidth) * 2),
					(this.maxPanelSize.Height * UIService.ItemUnitSize) + (Convert.ToInt32(SackPanel.BorderWidth) * 2));
			}

			return this.equipmentBackground;
		}

		set => this.equipmentBackground = value;
	}

	/// <summary>
	/// Gets or sets the bitmap for the stash panel background scaled by the panel size.
	/// </summary>
	public Bitmap? StashBackground
	{
		get
		{
			if (UIService.Scale != 1.0F && this.stashBackground is not null)
			{
				// We need to scale this since we use it to redraw the background.
				return new Bitmap(
					this.stashBackground,
					(this.maxPanelSize.Width * UIService.ItemUnitSize) + (Convert.ToInt32(SackPanel.BorderWidth) * 2),
					(this.maxPanelSize.Height * UIService.ItemUnitSize) + (Convert.ToInt32(SackPanel.BorderWidth) * 2));
			}

			return this.stashBackground;
		}

		set => this.stashBackground = value;
	}

	/// <summary>
	/// Gets or sets the currently selected bag id
	/// </summary>
	public new int CurrentBag
	{
		get => this.currentBag;

		set
		{
			// figure out the current bag to use
			int bagID = this.Services.StashService.ClampBagId(value);

			if (bagID != this.currentBag)
			{
				// turn off the current bag and turn on the new bag
				this.BagButtons[this.currentBag].IsOn = false;
				this.BagButtons[bagID].IsOn = true;
				this.currentBag = bagID;
				BagButtonBase button = this.BagButtons[this.currentBag];
				int buttonOffsetY = button.Location.Y + button.Size.Height;

				StashBag? bag = this.Services.StashService.GetBag(this.currentBag, this.Player, this.transferStash, this.stash, this.relicVaultStash);
				if (bag is { IsEquipment: true })
				{
					this.equipmentPanel.Sack = bag.Sack;
					this.background = this.EquipmentBackground;
					this.equipmentPanel.Visible = true;
					this.equipmentPanel.Enabled = true;
					this.BagSackPanel.Visible = false;
					this.BagSackPanel.Enabled = false;
				}
				else if (bag is not null)
				{
					this.background = this.StashBackground;
					this.BagSackPanel.Sack = bag.Sack;
					this.BagSackPanel.SackType = SackType.Stash;
					this.BagSackPanel.ResizeSackPanel(bag.Width, bag.Height);

					// Center the sack in the panel. SetLocation, so a zoom keeps the centering (ScaleControl places the sack from that origin).
					var (marginX, marginY) = bag.CenteringMargin(this.maxPanelSize.Width, this.maxPanelSize.Height);
					this.BagSackPanel.SetLocation(new Point(
						BorderPad + (marginX * UIService.HalfUnitSize),
						buttonOffsetY + (marginY * UIService.HalfUnitSize)));
					this.equipmentPanel.Visible = false;
					this.equipmentPanel.Enabled = false;
					this.BagSackPanel.Visible = true;
					this.BagSackPanel.Enabled = true;
				}

				this.Refresh();
			}
		}
	}

	#endregion StashPanel Properties

	#region StashPanel Protected Methods

	/// <summary>
	/// Override of ScaleControl which supports scaling of the fonts and internal items.
	/// </summary>
	/// <param name="factor">SizeF for the scale factor</param>
	/// <param name="specified">BoundsSpecified value.</param>
	protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
	{
		base.ScaleControl(factor, specified);
	}

	/// <summary>
	/// Updates the text for the group box.  Override the base method since we do not need to change it based on the player name.
	/// </summary>
	protected override void UpdateText()
	{
		this.Text = string.Empty;
		if (this.DrawAsGroupBox)
			this.Text = Resources.StashPanelText;

		// Player was changed, force an update to the player info panel.
		this.DisplayPlayerInfoLastName = null;
	}

	/// <summary>
	/// Assigns sacks to bag buttons
	/// </summary>
	protected override void AssignSacks()
	{
		// figure out the current bag to use
		this.currentBag = this.Services.StashService.ClampBagId(this.currentBag);

		#region hide/show bag buttons and assign initial bitmaps

		int buttonOffset = 0;
		foreach (StashButton button in this.BagButtons)
		{
			button.Visible = buttonOffset < 4;
			button.IsOn = buttonOffset == this.currentBag;

			++buttonOffset;
		}

		if ((this.relicVaultStash is null) || (this.relicVaultStash.NumberOfSacks < 1))
			this.BagButtons[3].Visible = false;

		if ((this.stash is null) || (this.stash.NumberOfSacks < 1))
			this.BagButtons[2].Visible = false;

		if ((this.transferStash is null) || (this.transferStash.NumberOfSacks < 1))
			this.BagButtons[1].Visible = false;

		#endregion

		this.SackPanel.Sack = this.GetBagSack(this.currentBag);
		if (this.currentBag == BagIdConstants.BAGID_EQUIPMENTPANEL && this.Player is not null)
		{
			this.equipmentPanel.GetGearStatBonus();
			GetSkillStatBonus();
		}

		// Assign sacks to button
		foreach (var button in this.BagButtons.Take(4))
			button.Sack = this.GetBagSack(button.ButtonNumber);
	}

	/// <summary>
	/// Gets the sack shown by a bag of this panel.
	/// </summary>
	/// <param name="bagId">The bag id (see <see cref="BagIdConstants"/>).</param>
	/// <returns>The sack, null when the bag has nothing to show.</returns>
	private SackCollection? GetBagSack(int bagId)
		=> this.Services.StashService.GetBagSack(bagId, this.Player, this.transferStash, this.stash, this.relicVaultStash);

	/// <summary>
	/// Paint callback
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">PaintEventArgs data</param>
	protected new void PaintCallback(object? sender, PaintEventArgs e)
	{
		var rect = this.GetBackgroundRect();
		if (this.background is not null)
			e.Graphics.DrawImage(this.background, rect);
		base.PaintCallback(sender, e);

		// Update the player info panel
		DisplayPlayerInfo();
	}

	#endregion StashPanel Protected Methods

	#region StashPanel Private Methods

	/// <summary>
	/// Gets a rectangle scaled by the size of the child vault panels.
	/// </summary>
	/// <returns>Rectangle containg the size and position of the child vault panel.</returns>
	private Rectangle GetBackgroundRect()
	{
		if (this.BagButtons is null || this.BagButtons.Count == 0)
			return Rectangle.Empty;

		return new Rectangle(
			BorderPad
			, this.BagButtons[0].Location.Y + this.BagButtons[0].Size.Height
			, this.Width - (BorderPad * 2)
			, this.Height - (this.BagButtons[0].Location.Y + this.BagButtons[0].Size.Height + BorderPad)
		);
	}

	#endregion StashPanel Private Methods

	[MemberNotNull(nameof(components), nameof(buttonContextMenuStrip), nameof(toolStripMenuItemEnableTooltip), nameof(toolStripMenuItemDisableTooltip))]
	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		this.buttonContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
		this.toolStripMenuItemEnableTooltip = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItemDisableTooltip = new System.Windows.Forms.ToolStripMenuItem();
		this.buttonContextMenuStrip.SuspendLayout();
		this.SuspendLayout();
		// 
		// buttonContextMenuStrip
		// 
		this.buttonContextMenuStrip.Items.AddRange([
			this.toolStripMenuItemEnableTooltip,
			this.toolStripMenuItemDisableTooltip]);
		this.buttonContextMenuStrip.Name = "contextMenuStrip";
		this.buttonContextMenuStrip.Size = new System.Drawing.Size(151, 48);
		this.buttonContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.buttonContextMenuStrip_Opening);
		this.buttonContextMenuStrip.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.contextMenuStrip_ItemClicked);
		this.buttonContextMenuStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(41)))), ((int)(((byte)(31)))));
		this.buttonContextMenuStrip.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
		this.buttonContextMenuStrip.Opacity = 0.8D;
		this.buttonContextMenuStrip.ShowImageMargin = false;
		// 
		// toolStripMenuItemEnableTooltip
		// 
		this.toolStripMenuItemEnableTooltip.Name = "toolStripMenuItemEnableTooltip";
		this.toolStripMenuItemEnableTooltip.Size = new System.Drawing.Size(150, 22);
		this.toolStripMenuItemEnableTooltip.Text = "Enable tooltip";
		this.toolStripMenuItemEnableTooltip.Visible = true;
		// 
		// toolStripMenuItemDisableTooltip
		// 
		this.toolStripMenuItemDisableTooltip.Name = "toolStripMenuItemDisableTooltip";
		this.toolStripMenuItemDisableTooltip.Size = new System.Drawing.Size(150, 22);
		this.toolStripMenuItemDisableTooltip.Text = "Disable tooltip";
		this.toolStripMenuItemDisableTooltip.Visible = true;
		this.buttonContextMenuStrip.ResumeLayout(false);
		this.ResumeLayout(false);
	}

}