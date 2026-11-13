//-----------------------------------------------------------------------
// <copyright file="VaultPanel.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Config;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Components.Buttons;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Forms;
using TQVaultAE.GUI.Models;
using TQVaultAE.Presentation.Win32.Contracts;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Components.Panels;

/// <summary>
/// Represents a TQ Vault control that displays a frame around a group of TQ Vault panels with an optional caption.
/// </summary>
public partial class VaultPanel : Panel, INotifyPropertyChanged, IScalingControl, IFontRebasing
{
	protected readonly IFontService FontService;
	protected readonly IUIService UIService;
	protected readonly SessionContext userContext;
	protected readonly GuiServices Services;
	protected readonly UserSettings USettings;
	protected readonly IHighlightService HighlightService;
	protected readonly IItemExchangeService ExchangeService;

	/// <summary>
	/// Gets the SackPanel instance
	/// </summary>
	public SackPanel SackPanel => this.BagSackPanel;

	/// <summary>
	/// player instance
	/// </summary>
	private PlayerCollection? player;

	/// <summary>
	/// Array of Bag buttons for the panel
	/// </summary>
	private Collection<BagButtonBase> bagButtons;

	/// <summary>
	/// Array of autosort buttons
	/// </summary>
	private Collection<AutoSortButton> autoSortButtons = [];

	/// <summary>
	/// Holds the currently selected bag.
	/// </summary>
	private int currentBag;

	private IContainer components;

	/// <summary>
	/// Context menu instance
	/// </summary>
	private ContextMenuStrip contextMenu;

#pragma warning disable CS8618 // Designer only: the runtime constructor sets the services and state.
	public VaultPanel()
	{
		InitializeComponent();
	}
#pragma warning restore CS8618

	[MemberNotNull(nameof(components), nameof(contextMenu))]
	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
		this.SuspendLayout();
		// 
		// contextMenu
		// 
		this.contextMenu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(41)))), ((int)(((byte)(31)))));
		this.contextMenu.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
		this.contextMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
		this.contextMenu.Name = "contextMenu";
		this.contextMenu.Opacity = 0.8D;
		this.contextMenu.ShowImageMargin = false;
		this.contextMenu.Size = new System.Drawing.Size(36, 4);
		this.contextMenu.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.ContextMenuItemClicked);
		// 
		// VaultPanel
		// 
		this.BackColor = System.Drawing.Color.Transparent;
		this.Paint += new System.Windows.Forms.PaintEventHandler(this.PaintCallback);
		this.ResumeLayout(false);
	}

	/// <summary>
	/// Initializes a new instance of the VaultPanel class.
	/// </summary>
	/// <param name="dragInfo">Instance of the ItemDragInfo</param>
	/// <param name="numberOfBags">Number of bags in this panel</param>
	/// <param name="panelSize">Main panel size</param>
	/// <param name="numberOfAutosortButtons">The number of AutoSort buttons associated with this panel.,</param>
	/// <param name="autoMoveLocation">The automovelocation for this panel.</param>
	public VaultPanel(ItemDragInfo dragInfo, int numberOfBags, Size panelSize, int numberOfAutosortButtons, AutoMoveLocation autoMoveLocation, GuiServices services)
	{
		InitializeComponent();

		this.Services = services;
		this.FontService = this.Services.FontService;
		this.UIService = this.Services.UIService;
		this.userContext = this.Services.Session;
		this.USettings = this.Services.Settings;
		this.HighlightService = this.Services.HighlightService;
		this.ExchangeService = this.Services.ItemExchangeService;

		this.DragInfo = dragInfo;
		this.AutoMoveLocation = autoMoveLocation;
		this.Text = Resources.PlayerPanelNoVault;
		this.NoPlayerString = Resources.PlayerPanelNoVault;
		this.DrawAsGroupBox = false;

		// Setup the offset to make room for the autosort button
		int autosortOffset = 0;
		if (numberOfAutosortButtons > 0)
		{
			autosortOffset = Convert.ToInt32(27.0F * UIService.Scale);
		}

		this.Size = new Size(
			(panelSize.Width * UIService.ItemUnitSize) + Convert.ToInt32(10.0F * UIService.Scale) + autosortOffset + BorderPad,
			(panelSize.Height * UIService.ItemUnitSize) + Convert.ToInt32(56.0F * UIService.Scale) + BorderPad);

		this.Font = new Font(this.Font.FontFamily, this.Font.SizeInPoints * UIService.FontZoom, this.Font.Style);

		this.BagPanelOffset = 0; // bag panel starts with bag #0
		this.BagSackPanel = new SackPanel(panelSize.Width, panelSize.Height, this.DragInfo, autoMoveLocation, this.Services);
		this.BagSackPanel.ContainerMod = () => this.Vault?.Mod;
		this.BagSackPanel.HoldsVault = () => this.Vault is not null;
		this.BagSackPanel.SetLocation(new Point(autosortOffset + BorderPad, this.Size.Height - (this.BagSackPanel.Size.Height + BorderPad)));
		this.BagSackPanel.OnNewItemHighlighted += new EventHandler<SackPanelEventArgs>(this.NewItemHighlightedCallback);
		this.BagSackPanel.OnAutoMoveItem += new EventHandler<SackPanelEventArgs>(this.AutoMoveItemCallback);
		this.BagSackPanel.OnItemSelected += new EventHandler<SackPanelEventArgs>(this.ItemSelectedCallback);
		this.BagSackPanel.OnClearAllItemsSelected += new EventHandler<SackPanelEventArgs>(this.ClearAllItemsSelectedCallback);
		this.Controls.Add(this.BagSackPanel);
		this.BagSackPanel.IsPlayerBagPanel = false;
		this.BagSackPanel.MaxSacks = numberOfBags;
		// Create the buttons
		this.bagButtons = [];
		this.CreateBagButtons(numberOfBags);

		// Assume it's the trash panel if we are not autosorting it.
		if (numberOfAutosortButtons == 0)
		{
			this.BagSackPanel.SackType = SackType.Trash;
		}
		else
		{
			this.autoSortButtons = [];

			for (int i = 0; i < numberOfAutosortButtons; ++i)
			{
				this.autoSortButtons.Insert(i, this.CreateAutoSortButton(i));
				this.Controls.Add(this.autoSortButtons[i]);
			}

			this.BagSackPanel.SackType = SackType.Vault;
		}

		this.contextMenu.Font = FontService.GetFont(9.0F * UIService.FontZoom);

		this.PropertyChanged += new PropertyChangedEventHandler(this.PropertyChangedCallback);

		// to avoid flickering use double buffer and to force control to use OnPaint
		this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
	}

	/// <summary>
	/// Event for signaling that a property has changed.
	/// </summary>
	public event PropertyChangedEventHandler? PropertyChanged;

	#region VaultPanel Properties

	/// <summary>
	/// Gets the scaled border pad.
	/// </summary>
	public int BorderPad => Convert.ToInt32(2.0F * UIService.Scale);

	/// <summary>
	/// After a zoom, grows the panel by what <paramref name="sack"/> came out larger than its size at the zoom, so the
	/// sack stays whole inside it.
	/// </summary>
	internal void Contain(SackPanel sack)
	{
		if (this.designSize.IsEmpty)
			return;

		var scaled = this.designSize.At(UIService.Scale);
		var excess = sack.ZoomExcess;
		var size = new Size(
			Math.Max(this.Width, scaled.Width + Math.Max(0, excess.Width)),
			Math.Max(this.Height, scaled.Height + Math.Max(0, excess.Height)));
		if (size != this.Size)
			this.Size = size;
	}

	/// <summary>
	/// Gets the bagbuttons array
	/// </summary>
	public Collection<BagButtonBase> BagButtons => this.bagButtons;

	/// <summary>
	/// Gets the autosort buttons array
	/// </summary>
	public Collection<AutoSortButton> AutoSortButtons => this.autoSortButtons;

	/// <summary>
	/// Gets or sets the Clear All Items Selected Event Handler
	/// </summary>
	public EventHandler<SackPanelEventArgs>? OnClearAllItemsSelected { get; set; }

	/// <summary>
	/// Gets or sets the item selected Event Handler
	/// </summary>
	public EventHandler<SackPanelEventArgs>? OnItemSelected { get; set; }

	/// <summary>
	/// Gets or sets the Automove Item Event Handler
	/// </summary>
	public EventHandler<SackPanelEventArgs>? OnAutoMoveItem { get; set; }

	/// <summary>
	/// Gets or sets the New Item Highlighted Event Handler
	/// </summary>
	public EventHandler<SackPanelEventArgs>? OnNewItemHighlighted { get; set; }

	/// <summary>
	/// Gets or sets the string used when no player is set.
	/// </summary>
	public string NoPlayerString { get; set; }

	/// <summary>
	/// Gets or sets the AutoMoveLocation for this panel.
	/// </summary>
	public AutoMoveLocation AutoMoveLocation { get; protected set; }

	/// <summary>
	/// Gets this panel's ItemDragInfo instance
	/// </summary>
	public ItemDragInfo DragInfo { get; private set; }

	/// <summary>
	/// Gets or sets the bag sack panel SackPanel instance
	/// </summary>
	public SackPanel BagSackPanel { get; set; }

	/// <summary>
	/// Gets or sets the bag offset.  Vaults are 1 based, player is 0 based.
	/// </summary>
	public int BagPanelOffset { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether this form is rendered as a groupbox or panel.
	/// </summary>
	public bool DrawAsGroupBox { get; set; }

	/// <summary>
	/// return the vault instance if it's a vault
	/// </summary>
	public PlayerCollection? Vault
		=> (this.player?.IsVault ?? false) ? this.player : null;

	/// <summary>
	/// Gets or sets the player instance
	/// </summary>
	/// <summary>
	/// Player or vault shown by the panel, for the actions that need one.
	/// </summary>
	internal PlayerCollection ShownPlayer => this.Player
		?? throw new InvalidOperationException("The panel shows no player or vault.");

	/// <summary>
	/// Vault shown by the panel, for the vault actions (export, tooltips).
	/// </summary>
	protected PlayerCollection ShownVault => this.Vault
		?? throw new InvalidOperationException("The panel shows no vault.");

	public PlayerCollection? Player
	{
		get => this.player;

		set
		{
			if ((!this.player?.IsVault ?? false) || (!value?.IsVault ?? false))
			{
				// Not Vault
				this.userContext.CurrentPlayer = value;
			}

			this.player = value;

			// Set container reference for the sack panel to track item locations
			this.BagSackPanel.PlayerCollection = value;

			this.UpdateText();

			this.ApplyCustomButtonIcons();

			this.OnPropertyChanged(nameof(Player));
			this.Refresh();
		}
	}

	/// <summary>
	/// Gets or sets the currently selected bag
	/// </summary>
	public int CurrentBag
	{
		get => this.currentBag;

		set
		{
			if (value != this.currentBag)
			{
				// turn off the current bag and turn on the new bag
				this.BagButtons[this.currentBag].IsOn = false;
				this.BagButtons[value].IsOn = true;
				this.currentBag = value;
				this.BagSackPanel.Sack = this.ShownPlayer.GetSack(this.currentBag + this.BagPanelOffset);
				this.BagSackPanel.CurrentSack = this.currentBag;

				if (this.Vault is not null && this.IsSecondaryVaultPanel is bool secondary)
					this.Services.VaultService.RememberSelectedBag(this.Vault, secondary, this.currentBag);
			}
		}
	}

	/// <summary>
	/// Gets which vault panel this is, for the bag each vault remembers per panel:
	/// false for the main vault panel, true for the secondary one, null for the other panels.
	/// </summary>
	private bool? IsSecondaryVaultPanel
		=> this.AutoMoveLocation switch
		{
			AutoMoveLocation.Vault => false,
			AutoMoveLocation.SecondaryVault => true,
			_ => null,
		};

	#endregion VaultPanel Properties

	#region VaultPanel Public Methods

	/// <summary>
	/// Callback for the property changed event
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">PropertyChangedEventArgs data</param>
	public void PropertyChangedCallback(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(Player))
		{
			this.AssignSacks();
			this.HighlightService.FindHighlight();
		}
	}

	/// <summary>
	/// Callback for the Paint event
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">PaintEventArgs data</param>
	public void PaintCallback(object? sender, PaintEventArgs e)
	{
		if (this.DrawAsGroupBox)
		{
			GroupBoxRenderer.DrawGroupBox(e.Graphics, ClientRectangle, this.Text, this.Font, System.Windows.Forms.VisualStyles.GroupBoxState.Normal);
		}
	}

	/// <summary>
	/// Tooltip callback for displaying the bag summary
	/// </summary>
	/// <param name="windowHandle">Window handle</param>
	/// <returns>String containing the tooltip text</returns>
	public virtual void ToolTipCallback(int windowHandle)
	{
		// check the bag buttons
		if (this.BagButtons is not null && this.BagButtons.Count > 0)
		{
			foreach (BagButtonBase button in this.BagButtons)
			{
				if (button is not null)
				{
					button.ToolTipCallback(windowHandle);
				}
			}
		}
	}

	#endregion VaultPanel Public Methods

	#region VaultPanel Protected Methods

	/// <summary>
	/// Assigns sacks to buttons.
	/// </summary>
	protected virtual void AssignSacks()
	{
		if ((this.Player is null) || (this.Player.NumberOfSacks < 1))
		{
			foreach (BagButtonBase button in this.BagButtons)
			{
				button.Visible = false;
			}

			this.BagSackPanel.Sack = null;

			this.HideAutoSortButtons();
		}
		else
		{
			this.UnhideAutoSortButtons();

			int numberOfBags = this.Player.NumberOfSacks - this.BagPanelOffset;

			// The bag saved in the vault for this panel, else the current one.
			var secondary = this.IsSecondaryVaultPanel;
			this.CurrentBag = this.Services.VaultService.ResolveInitialBag(
				secondary is null ? null : this.Vault, secondary ?? false, this.CurrentBag, numberOfBags);

			// hide/show bag buttons and assign initial bitmaps
			int index = 0;
			foreach (BagButtonBase button in this.BagButtons)
			{
				button.Visible = index < numberOfBags;
				button.IsOn = index == this.CurrentBag;

				button.Sack = this.Player.GetSack(index + this.BagPanelOffset);

				++index;
			}

			if (numberOfBags > 0)
			{
				this.BagSackPanel.Sack = this.Player.GetSack(this.CurrentBag + this.BagPanelOffset);
			}
			else
			{
				this.BagSackPanel.Sack = null;
			}
		}
	}

	/// <summary>
	/// Updates the text for the group box when the player is changed.
	/// </summary>
	protected virtual void UpdateText()
	{
		this.Text = string.Empty;
		if (this.DrawAsGroupBox)
		{
			this.Text = this.NoPlayerString;
			if (this.player is not null)
			{
				this.Text = this.player.PlayerName;
			}
		}
	}

	private void ImportFromFileClicked(object? sender, EventArgs e)
	{
		var mainForm = this.FindForm() as MainForm;
		mainForm?.ImportFromFile();
	}

	/// <summary>
	/// The base font changed: the menu it shows itself, outside the controls, takes it.
	/// </summary>
	void IFontRebasing.RebaseFonts(IFontService fontService)
	{
		FontRebaser.RebaseMenu(this.contextMenu, fontService);
		this.OnFontsRebased();
	}

	/// <summary>The base font changed: fonts a derived panel made itself are made again.</summary>
	protected virtual void OnFontsRebased()
	{
	}

	/// <summary>
	/// Override of ScaleControl which supports font scaling.
	/// </summary>
	/// <param name="factor">SizeF for the scale factor</param>
	/// <param name="specified">BoundsSpecified value.</param>
	protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
	{
		this.contextMenu.Font = new Font(this.contextMenu.Font.FontFamily, 9.0F * UIService.FontZoom);
		this.Font = ZoomScope.ScaleFont(this.Font, factor.Height);

		base.ScaleControl(factor, specified);

		// From the scale, not from the size at the zoom before. The sacks, scaled next, make it grow where their
		// whole-pixel cells need it (Contain).
		if (ZoomScope.IsZooming && !this.designSize.IsEmpty)
			this.Size = this.designSize.At(UIService.Scale);
	}

	/// <summary>The panel's size in design pixels.</summary>
	private DesignSize designSize;

	/// <inheritdoc/>
	protected override void OnSizeChanged(EventArgs e)
	{
		base.OnSizeChanged(e);

		// The base panel's construction sizes it before the services are set.
		if (this.UIService is not null)
			this.designSize.Keep(this.Size, UIService.Scale);
	}

	/// <summary>
	/// New Item Highlighted Callback
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">SackPanelEventArgs data</param>
	protected void NewItemHighlightedCallback(object? sender, SackPanelEventArgs e)
		=> this.OnNewItemHighlighted?.Invoke(sender, e);

	/// <summary>
	/// Automove Item Callback
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">SackPanelEventArgs data</param>
	protected void AutoMoveItemCallback(object? sender, SackPanelEventArgs e)
		=> this.OnAutoMoveItem?.Invoke(sender, e);

	/// <summary>
	/// Item Selected Callback
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">SackPanelEventArgs data</param>
	protected void ItemSelectedCallback(object? sender, SackPanelEventArgs e)
		=> this.OnItemSelected?.Invoke(sender, e);

	/// <summary>
	/// Clear all selected items Callback
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">SackPanelEventArgs data</param>
	protected void ClearAllItemsSelectedCallback(object? sender, SackPanelEventArgs e)
		=> this.OnClearAllItemsSelected?.Invoke(sender, e);

	#endregion VaultPanel Protected Methods

	#region VaultPanel Private Methods

	/// <summary>
	/// Property Changed event trigger
	/// </summary>
	/// <param name="name">Name of the property that has changed</param>
	private void OnPropertyChanged(string name)
	{
		PropertyChangedEventHandler? handler = this.PropertyChanged;

		if (handler is not null)
		{
			handler(this, new PropertyChangedEventArgs(name));
		}
	}

	#endregion VaultPanel Private Methods
}
