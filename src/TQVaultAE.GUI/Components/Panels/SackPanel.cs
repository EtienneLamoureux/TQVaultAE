//-----------------------------------------------------------------------
// <copyright file="SackPanel.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Globalization;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Config;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Components.Rendering;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Forms;
using TQVaultAE.GUI.Models;
using TQVaultAE.GUI.Tooltips;
using TQVaultAE.Presentation.Win32.Contracts;
using TQVaultAE.Presentation.Win32;

namespace TQVaultAE.GUI.Components.Panels;

/// <summary>
/// Class for holding all of the UI functions of the sack panel.
/// </summary>
public partial class SackPanel : Panel, IScalingControl, IFontRebasing
{
	private readonly ILogger Log;
	private readonly ITranslationService TranslationService;
	private readonly UserSettings USettings;
	private VaultForm? _VaultForm;

	protected readonly IFontService FontService;
	protected readonly IUIService UIService;
	protected readonly IItemProvider ItemProvider;
	protected readonly GuiServices Services;
	protected readonly IHighlightService HighlightService;
	protected readonly IItemDatabaseService ItemDatabaseService;
	protected readonly IItemExchangeService ExchangeService;
	private readonly Bitmap CustomContextMenuAffixUnknown;
	private readonly Bitmap CustomContextMenuAffixUntranslated;
	ItemStyle[] ItemStyleBackGroundColorEnable = [
			ItemStyle.Epic,
			ItemStyle.Legendary,
			ItemStyle.Rare,
			ItemStyle.Common,
			ItemStyle.Relic,
			ItemStyle.Artifact,
			ItemStyle.Quest,
			ItemStyle.Scroll,
			ItemStyle.Formulae,
			ItemStyle.Parchment ];

	#region SackPanel Fields

	/// <summary>
	/// User current data context
	/// </summary>
	protected SessionContext userContext;

	/// <summary>
	/// The currently selected/displayed sack
	/// </summary>
	private SackCollection? sack;

	/// <summary>
	/// List holding all of the items that have been multi-selected.
	/// </summary>
	private List<Item>? selectedItems;

	/// <summary>
	/// Indicates that the mouse currently has a drag rectangle.
	/// </summary>
	private bool mouseDraw;

	/// <summary>
	/// The start corner of the mouse drag rectangle.
	/// </summary>
	private Point startPosition;

	/// <summary>
	/// The current corner of the mouse drag rectangle.
	/// </summary>
	private Point currentPosition;

	/// <summary>
	/// Collection of items under the drag item.
	/// </summary>
	private Collection<Item> itemsUnderDragItem;

	/// <summary>
	/// Collection of items under the previous drag location.
	/// </summary>
	private Collection<Item> itemsUnderOldDragLocation;

	/// <summary>
	/// Indicates that the multi-selection has changed
	/// </summary>
	private bool selectedItemsChanged;

	/// <summary>
	/// Used for drawing the panel border
	/// </summary>
	private Pen borderPen;

	/// <summary>
	/// Used for drawing the panel grid
	/// </summary>
	private Pen gridPen;

	/// <summary>
	/// Font used to display numbers of charms, relics or stackable items
	/// </summary>
	private Font numberFont;

	/// <summary>
	/// Used for drawing the numbers of charms, relics, or stackable items over the item bitmap.
	/// </summary>
	private Brush numberBrush;

	/// <summary>
	/// Used for format the numbers overlay for charms, relics, or stackable items which displys over the item bitmap.
	/// </summary>
	private StringFormat numberFormat;

	/// <summary>
	/// Context menu strip
	/// </summary>
	private ContextMenuStrip CustomContextMenu;

	/// <summary>
	/// Coordinates of the cell which the context menu applies
	/// </summary>
	private Point contextMenuCellWithFocus;

	/// <summary>
	/// Indicates whether the current sort should be vertical.
	/// </summary>
	private bool sortVertical;

	private System.ComponentModel.IContainer components;

	/// <summary>
	/// Holds the original location of the panel.
	/// Used for scaling.
	/// </summary>
	private Point originalLocation;

	#endregion

#pragma warning disable CS8618 // Designer only: the runtime constructor sets the services and state.
	public SackPanel()
	{
		InitializeComponent();
	}
#pragma warning restore CS8618

	[MemberNotNull(nameof(components), nameof(CustomContextMenu))]
	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		this.CustomContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
		this.SuspendLayout();
		// 
		// CustomContextMenu
		// 
		this.CustomContextMenu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(41)))), ((int)(((byte)(31)))));
		this.CustomContextMenu.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
		this.CustomContextMenu.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
		this.CustomContextMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
		this.CustomContextMenu.Name = "CustomContextMenu";
		this.CustomContextMenu.Opacity = 0.8D;
		this.CustomContextMenu.ShowImageMargin = false;
		this.CustomContextMenu.Size = new System.Drawing.Size(36, 4);
		this.CustomContextMenu.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.ContextMenuItemClicked);
		// 
		// SackPanel
		// 
		this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.KeyDownCallback);
		this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.KeyPressCallback);
		this.Paint += new System.Windows.Forms.PaintEventHandler(this.PaintCallback);
		this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MouseDownCallback);
		this.MouseEnter += new System.EventHandler(this.MouseEnterCallback);
		this.MouseLeave += new System.EventHandler(this.MouseLeaveCallback);
		this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MouseMoveCallback);
		this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MouseUpCallback);
		this.ResumeLayout(false);
	}

	/// <summary>
	/// Initializes a new instance of the SackPanel class.
	/// </summary>
	/// <param name="sackWidth">Width of the sack panel in cells</param>
	/// <param name="sackHeight">Height of the sack panel in cells</param>
	/// <param name="dragInfo">ItemDragInfo instance.</param>
	/// <param name="autoMoveLocation">AutoMoveLocation for this sack</param>
	public SackPanel(int sackWidth, int sackHeight, ItemDragInfo dragInfo, AutoMoveLocation autoMoveLocation, GuiServices services)
	{
		InitializeComponent();

		this.Services = services;
		this.FontService = this.Services.FontService;
		this.UIService = this.Services.UIService;
		this.ItemProvider = this.Services.ItemProvider;
		this.TranslationService = this.Services.TranslationService;
		this.userContext = this.Services.Session;
		this.USettings = this.Services.Settings;
		this.HighlightService = this.Services.HighlightService;
		this.ItemDatabaseService = this.Services.ItemDatabaseService;
		this.ExchangeService = this.Services.ItemExchangeService;

		this.Log = this.Services.Logger<SackPanel>();

		this.DragInfo = dragInfo;
		this.AutoMoveLocation = autoMoveLocation;
		this.DragInfo.AddAutoMoveLocationToList(autoMoveLocation);
		this.SubscribeCompareSelection();
		this.LastCellWithFocus = InvalidDragLocation;
		this.contextMenuCellWithFocus = InvalidDragLocation;

		this.SackSize = new Size(sackWidth, sackHeight);
		this.SackType = SackType.Player;

		this.itemsUnderDragItem = [];
		this.itemsUnderOldDragLocation = [];

		this.borderPen = new Pen(Color.FromArgb(223, 188, 97));
		this.borderPen.Width = BorderWidth;

		this.DefaultImage = Images.tqmedallion;
		this.DefaultItemBackgroundColor = Color.FromArgb(220, 220, 220);  // White
		this.HighlightValidItemColor = Color.FromArgb(23, 149, 15);       // Green
		this.HighlightInvalidItemColor = Color.FromArgb(153, 28, 28);     // Red

		this.HighlightSearchItemBorder = new Pen(this.HighlightService.HighlightSearchItemBorderColor)
		{
			Width = 4,
		};

		this.gridPen = new Pen(Color.FromArgb(142, 140, 129));

		this.numberFont = new Font("Arial", 10.0F * UIService.Scale, GraphicsUnit.Pixel);
		this.numberBrush = new SolidBrush(Color.White);
		this.numberFormat = new StringFormat();
		this.numberFormat.Alignment = StringAlignment.Far; // right-justify

		this.Size = new Size(
			(Convert.ToInt32(this.borderPen.Width) * 2) + (UIService.ItemUnitSize * sackWidth),
			(Convert.ToInt32(this.borderPen.Width) * 2) + (UIService.ItemUnitSize * sackHeight));
		this.BackColor = Theme.Background;

		CustomProfessionalRenderer.Apply(this.CustomContextMenu);
		this.CustomContextMenu.Font = this.MenuFont();

		this.CustomContextMenuAffixUnknown = this.UIService.LoadBitmap(@"INGAMEUI\MAP\ICONS\ICONSMALLQUEST01.TEX");
		this.CustomContextMenuAffixUntranslated = this.UIService.LoadBitmap(@"INGAMEUI\MAP\ICONS\ICONSMALLAREAOFINTEREST01.TEX");

		// Da_FileServer: Enable double buffering to remove flickering.
		this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
	}

	#region SackPanel Properties

	/// <summary>
	/// Gets the width of the border pen.  Scaled by the DB scale.
	/// </summary>
	public float BorderWidth => Math.Max((4.0F * UIService.Scale), 1.0F);

	/// <summary>
	/// Gets or sets the Clear selected items event handler
	/// </summary>
	public EventHandler<SackPanelEventArgs>? OnClearAllItemsSelected { get; set; }

	/// <summary>
	/// Gets or sets the item selected event handler
	/// </summary>
	public EventHandler<SackPanelEventArgs>? OnItemSelected { get; set; }

	/// <summary>
	/// Gets or sets the automove item event handler
	/// </summary>
	public EventHandler<SackPanelEventArgs>? OnAutoMoveItem { get; set; }

	/// <summary>
	/// Gets or sets the new item highlighted event handler
	/// </summary>
	public EventHandler<SackPanelEventArgs>? OnNewItemHighlighted { get; set; }

	/// <summary>
	/// Gets or sets the sack size
	/// </summary>
	public Size SackSize { get; protected set; }

	/// <summary>
	/// The mod of the vault shown, when the panel shows a vault's bag: items put here without a link of their own
	/// follow it. Null for other panels.
	/// </summary>
	public Func<string?>? ContainerMod { get; set; }

	/// <summary>
	/// Whether the panel shows a vault's bag: an item put here without any link is read with the base game data.
	/// Null (not a vault) for the panels of a character.
	/// </summary>
	public Func<bool>? HoldsVault { get; set; }

	/// <summary>
	/// Whether an item of the panel, read again with other data (it may take another size), still has room: where it
	/// lies, or at the free spot it is then moved to.
	/// </summary>
	protected virtual bool HasRoomOnceReadAgain(Item item) => SackLayout.FitsOrMoves(this.ShownSack, this.SackSize, item);

	/// <summary>
	/// Whether the dragged item, dropped on the cells under it, has room in the shown bag once it takes its map here
	/// (MODLINK.md): read with a new map it may take another size. Room is where it is dropped, else a free spot of
	/// the bag; never another tab.
	/// </summary>
	/// <param name="dragItem">The dragged item.</param>
	/// <param name="itemUnderUs">The item a swap drop picks up, if any.</param>
	protected bool HasRoomForDrop(Item dragItem, Item? itemUnderUs)
	{
		// The same item under it may stack: the dragged one does not stay as it is.
		if (itemUnderUs is not null && dragItem.BaseItemId.Equals(itemUnderUs.BaseItemId))
			return true;

		var cell = this.CellsUnderDragItem.Location;
		return this.Services.ItemEditService.HasRoomOnceSettled(dragItem, this.HoldsVault?.Invoke() ?? false, this.ContainerMap
			, item => SackLayout.HasRoomAt(this.ShownSack, this.SackSize, item, cell, itemUnderUs));
	}

	/// <summary>
	/// Tells the user that an item has no room in the shown tab once read with its map's data, and to use another tab.
	/// </summary>
	/// <param name="item">The item, read with the data it keeps.</param>
	/// <param name="message">The message, with the item's name as <c>{0}</c>.</param>
	internal void NotifyNoRoomInTab(Item item, string message)
		=> this.Services.UIService.NotifyUser(string.Format(CultureInfo.CurrentCulture, message
			, this.Services.ItemProvider.GetFriendlyNames(item).FullNameClean), TQColor.Red);

	/// <summary>
	/// The map the items of the panel's container inherit: the vault's mod, or else the shown character's map (its
	/// bags, equipment and stashes, and the shared stashes shown with it).
	/// </summary>
	internal string? ContainerMap
		=> this.HoldsVault?.Invoke() ?? false
			? this.ContainerMod?.Invoke()
			: this.Services.GameMapService.ActiveCharacterMod;

	/// <summary>
	/// Tells the user which items kept their previous map for want of room once read with the new one.
	/// </summary>
	internal void NotifyKeptMap(IReadOnlyCollection<Item> kept)
	{
		if (kept.Count == 0)
			return;

		var names = string.Join(", ", kept.Select(i => this.Services.ItemProvider.GetFriendlyNames(i).FullNameClean));
		this.Services.UIService.NotifyUser(string.Format(System.Globalization.CultureInfo.CurrentCulture, TQVaultAE.Presentation.Resources.ModMapKeptNoRoom, names));
	}

	/// <summary>
	/// Gets or sets the AutoMoveLocation for this sack.
	/// </summary>
	public AutoMoveLocation AutoMoveLocation { get; protected set; }

	/// <summary>
	/// Gets or sets the current sack
	/// </summary>
	public int CurrentSack { get; set; }

	/// <summary>
	/// Gets or sets the total sacks
	/// </summary>
	public int MaxSacks { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the secondary vault is being shown
	/// </summary>
	public bool SecondaryVaultShown { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether this is the secondary vault panel
	/// </summary>
	public bool IsSecondaryVault { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether this is the main player panel
	/// </summary>
	public bool IsMainPlayerPanel { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether this is the player's bag panel
	/// </summary>
	public bool IsPlayerBagPanel { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the background grid will be drawn.
	/// </summary>
	public bool DisableGrid { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the border around the sack should be drawn.
	/// </summary>
	public bool DisableBorder { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether this panel allows selection of multiple items.
	/// </summary>
	public bool DisableMultipleSelection { get; set; }

	/// <summary>
	/// Gets or sets the default background Color for cells which contain an item.
	/// </summary>
	public Color DefaultItemBackgroundColor { get; protected set; }

	/// <summary>
	/// Gets or sets the background Color for cells which are valid for an item drop.
	/// </summary>
	public Color HighlightValidItemColor { get; protected set; }

	/// <summary>
	/// Gets or sets the background Color for cells which are NOT valid for an item drop.
	/// </summary>
	public Color HighlightInvalidItemColor { get; protected set; }

	/// <summary>
	/// Gets or sets the border for item highlight.
	/// </summary>
	public Pen HighlightSearchItemBorder { get; protected set; }

	/// <summary>
	/// Gets a value indicating whether items have been selected
	/// </summary>
	public bool SelectionActive => this.selectedItems is not null && this.selectedItems.Count > 0;

	/// <summary>
	/// Gets the list of selected items.
	/// </summary>
	public IReadOnlyList<Item> GetSelectedItems()
		=> this.selectedItems?.AsReadOnly() ?? Array.Empty<Item>().AsReadOnly();

	/// <summary>
	/// Gets or sets the sack instance
	/// </summary>
	public SackCollection? Sack
	{
		get => this.sack;

		set
		{
			this.sack = value;
			this.Refresh();
		}
	}

	/// <summary>
	/// Gets or sets the type of container this is.
	/// </summary>
	public SackType SackType { get; set; }

	/// <summary>
	/// Gets or sets the container (vault/player/stash) that owns this sack.
	/// Used to track item locations for search functionality.
	/// </summary>
	public PlayerCollection? PlayerCollection { get; set; }

	/// <summary>
	/// Gets the stash of the shown sack, for the sack panel of the stash panel: its items belong to the stash file.
	/// </summary>
	public Func<Stash?>? ShownStash { get; set; }

	/// <summary>
	/// Sack shown by the panel, for the actions that need one (context menu, drag and drop).
	/// </summary>
	internal SackCollection ShownSack => this.Sack
		?? throw new InvalidOperationException("The panel shows no sack.");

	/// <summary>
	/// Gets the Point (-1, -1) which is used to indicate an invalid location when dragging.
	/// </summary>
	protected static Point InvalidDragLocation => new Point(-1, -1);

	/// <summary>
	/// Gets the Rectangle (-1, -1, 0, 0) which is used to indicate an invalid area when dragging.
	/// </summary>
	protected static Rectangle InvalidDragRectangle => new Rectangle(InvalidDragLocation, Size.Empty);

	/// <summary>
	/// Gets or sets the dragInfo instance of any items being dragged.
	/// </summary>
	protected ItemDragInfo DragInfo { get; set; }

	/// <summary>
	/// Gets or sets the cell coordinates of the dragged item's location prior to being picked up
	/// </summary>
	protected Point LastDragLocation { get; set; }

	/// <summary>
	/// Gets or sets the rectangle of cells under the mouse position of the drag item
	/// </summary>
	/// <remarks>
	/// Takes into account the size of the drag item and the mouse position within the bitmap.
	/// </remarks>
	protected Rectangle CellsUnderDragItem { get; set; }

	/// <summary>
	/// Gets the collection of items under the drag item.
	/// </summary>
	/// <remarks>
	/// Uses items that fall either partly or completely within the CellsUnderDragItem property.
	/// Takes into account the the size of the drag item and the mouse position within the bitmap.
	/// </remarks>
	protected Collection<Item> ItemsUnderDragItem => this.itemsUnderDragItem;

	/// <summary>
	/// Gets the collection of items under the previous drag location.
	/// </summary>
	/// <remarks>
	/// Used to redraw the areas underneath the drag item as the item moves with the mouse.
	/// </remarks>
	protected Collection<Item> ItemsUnderOldDragLocation => this.itemsUnderOldDragLocation;

	/// <summary>
	/// Gets or sets the coordinates of the last cell which had focus.
	/// </summary>
	protected Point LastCellWithFocus { get; set; }

	/// <summary>
	/// Gets the alpha value from the user settings and applies any necessary clamping of the value.
	/// </summary>
	protected int UserAlpha => USettings.ItemBGColorOpacity > 127 ? 127 : USettings.ItemBGColorOpacity;

	/// <summary>
	/// Gets or sets the background image that is shown when there are no sacks to display.
	/// </summary>
	protected Bitmap? DefaultImage { get; set; }

	#endregion SackPanel Properties

	#region SackPanel Public Methods

	/// <summary>
	/// Resizes the sack panel.
	/// Used for the stash since the different sacks have different sizes
	/// </summary>
	/// <param name="sackWidth">Width of the new sack</param>
	/// <param name="sackHeight">Height of the new sack</param>
	public void ResizeSackPanel(int sackWidth, int sackHeight)
	{
		this.SackSize = new Size(sackWidth, sackHeight);
		this.Size = new Size(
			(Convert.ToInt32(this.borderPen.Width) * 2) + (UIService.ItemUnitSize * sackWidth),
			(Convert.ToInt32(this.borderPen.Width) * 2) + (UIService.ItemUnitSize * sackHeight));
		this.Invalidate();
	}

	/// <summary>
	/// Sets the original location of the panel.
	/// Used to handle moving the panel on scaling.
	/// </summary>
	/// <param name="location">Point holding the location of the panel.</param>
	public void SetLocation(Point location)
	{
		this.Location = location;

		// Set the unscaled origin.
		this.originalLocation = new Point(
			Convert.ToInt32((float)location.X / UIService.Scale),
			Convert.ToInt32((float)location.Y / UIService.Scale));
	}

	/// <summary>
	/// Adjusts the alpha value to provide contrast for highlighting the background of an item.
	/// </summary>
	/// <param name="alpha">Int with the adjusted alpha value.</param>
	/// <returns></returns>
	protected static int AdjustAlpha(int alpha)
	{
		alpha += 64;
		if (alpha < 127) alpha = 127;
		if (alpha > 255) alpha = 255;

		return alpha;
	}

	/// <summary>
	/// The base font changed: the menu it shows itself, outside the controls, takes it.
	/// </summary>
	void IFontRebasing.RebaseFonts(IFontService fontService)
		=> FontRebaser.RebaseMenu(this.CustomContextMenu, fontService);

	/// <summary>
	/// The font of the context menu, at the zoom: the game font of the font service. The zoom made it again from its
	/// name, which gave the game font's light face: the whole menu looked thinner after a zoom.
	/// </summary>
	private Font MenuFont() => this.FontService.GetFont(9.0F * UIService.FontZoom);

	/// <summary>
	/// Override of ScaleControl which supports scaling of the fonts and internal items.
	/// </summary>
	/// <param name="factor">SizeF for the scale factor</param>
	/// <param name="specified">BoundsSpecified value.</param>
	protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
	{
		this.borderPen.Width = Math.Max((4.0F * UIService.Scale), 1.0F);
		this.numberFont = new Font(this.numberFont.Name, 10.0F * UIService.Scale, GraphicsUnit.Pixel);
		this.CustomContextMenu.Font = this.MenuFont();

		this.Size = new Size(
			(Convert.ToInt32(this.borderPen.Width) * 2) + (UIService.ItemUnitSize * this.SackSize.Width),
			(Convert.ToInt32(this.borderPen.Width) * 2) + (UIService.ItemUnitSize * this.SackSize.Height));

		this.Location = new Point(
			Convert.ToInt32((float)this.originalLocation.X * UIService.Scale),
			Convert.ToInt32((float)this.originalLocation.Y * UIService.Scale));

		// The cells are whole pixels at every zoom while the panel holding the sack is sized in proportion to the zoom:
		// the panel grows to keep the sack whole.
		if (ZoomScope.IsZooming)
			(this.Parent as VaultPanel)?.Contain(this);
	}

	/// <summary>The sack's size in design pixels.</summary>
	private DesignSize designSize;

	/// <summary>
	/// How much larger the sack is than its size in proportion to the scale: its cells are whole pixels.
	/// </summary>
	internal Size ZoomExcess
		=> this.designSize.IsEmpty ? Size.Empty : this.Size - this.designSize.At(UIService.Scale);

	/// <inheritdoc/>
	protected override void OnSizeChanged(EventArgs e)
	{
		base.OnSizeChanged(e);

		// The panel's construction sizes it before the services are set.
		if (this.UIService is not null)
			this.designSize.Keep(this.Size, UIService.Scale);
	}

	/// <summary>
	/// Indicates whether the passed item meets the item requirementt for equipping
	/// </summary>
	/// <param name="item">Item to check</param>
	/// <returns>True if item is able to be equipped</returns>
	protected virtual bool PlayerMeetRequierements(Item item)
		=> this.Services.PlayerService.CurrentPlayerMeetsRequirements(item);

	/// <summary>
	/// Indicates whether the items of the shown sack cannot change: a sack of a read-only character, or the equipment
	/// with the <c>PlayerReadonly</c> setting on (<see cref="ItemActionRules.IsReadOnly"/>).
	/// </summary>
	public bool ItemsReadOnly => ItemActionRules.IsReadOnly(this.SackType, this.USettings.PlayerReadonly, this.Sack?.IsReadOnly ?? false);

	/// <summary>
	/// Tells the user that <paramref name="sack"/> belongs to a read-only character, when it does.
	/// </summary>
	/// <returns>Whether the sack is a read-only character's (and the user was told).</returns>
	internal bool NotifyIfReadOnlyCharacter(SackCollection? sack)
	{
		if (sack is not { IsReadOnly: true })
			return false;

		// Only the character shown has its sacks on screen.
		this.Services.UIService.NotifyUser(string.Format(CultureInfo.CurrentCulture, TQVaultAE.Presentation.Resources.CharacterReadOnlyRefused
			, this.userContext.CurrentPlayer?.PlayerName ?? this.PlayerCollection?.PlayerName ?? string.Empty), TQColor.Red);
		return true;
	}

	/// <summary>
	/// Indicates whether the passed item is suitable for equipping.
	/// e.g. an Immortal throne or greater item on a Titan Quest Original player.
	/// </summary>
	/// <param name="item"></param>
	/// <returns></returns>
	protected virtual bool IsSuitableForCurrentPlayer(Item item)
		=> this.Services.PlayerService.IsSuitableForCurrentPlayer(item);

	/// <summary>
	/// Called after an item action changed the items of the shown sack in place (delete, relic removal, affix or bonus
	/// change, seed...). Drags tell their own panels through <see cref="ItemDragInfo"/>.
	/// </summary>
	protected virtual void OnShownItemsChanged()
	{
	}

	#endregion SackPanel Protected Methods

	protected void InvalidateItemCacheAll(params Item?[] items)
	{
		ItemTooltip.InvalidateCache(items);
		BagButtonTooltip.InvalidateCache(items);
		this.ItemProvider.InvalidateFriendlyNamesCache(items.OfType<Item>().ToArray());
	}

	private void InvalidateItemCacheItemTooltip(params Item[] items)
	{
		ItemTooltip.InvalidateCache(items);
		this.ItemProvider.InvalidateFriendlyNamesCache(items);
	}
}
