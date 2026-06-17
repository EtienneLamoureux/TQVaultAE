//-----------------------------------------------------------------------
// <copyright file="SackPanel.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Globalization;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Config;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.GUI.Models;
using TQVaultAE.GUI.Tooltip;
using TQVaultAE.Logs;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Components;


/// <summary>
/// Class for holding all of the UI functions of the sack panel.
/// </summary>
public partial class SackPanel : Panel, IScalingControl
{
	private readonly ILogger Log = null;
	private readonly ITranslationService TranslationService;
	private readonly UserSettings USettings;
	private VaultForm _VaultForm;

	protected readonly IFontService FontService;
	protected readonly IUIService UIService;
	protected readonly IDatabase Database;
	protected readonly IItemProvider ItemProvider;
	protected readonly ITQDataService TQData;
	protected readonly IServiceProvider ServiceProvider;
	protected readonly IHighlightService HighlightService;
	protected readonly IItemDatabaseService ItemDatabaseService;
	protected readonly IItemExchangeService ExchangeService;
	private readonly Bitmap CustomContextMenuAffixUnknown;
	private readonly Bitmap CustomContextMenuAffixUntranslated;
	ItemStyle[] ItemStyleBackGroundColorEnable = new[] {
			ItemStyle.Epic,
			ItemStyle.Legendary,
			ItemStyle.Rare,
			ItemStyle.Common,
			ItemStyle.Relic,
			ItemStyle.Artifact,
			ItemStyle.Quest,
			ItemStyle.Scroll,
			ItemStyle.Formulae,
			ItemStyle.Parchment };

	#region SackPanel Fields

	/// <summary>
	/// User current data context
	/// </summary>
	protected SessionContext userContext;

	/// <summary>
	/// The currently selected/displayed sack
	/// </summary>
	private SackCollection sack;

	/// <summary>
	/// List holding all of the items that have been multi-selected.
	/// </summary>
	private List<Item> selectedItems;

	/// <summary>
	/// Indicates that the control key is being held down.
	/// Used to multi-select items.
	/// </summary>
	private bool controlKeyDown;

	/// <summary>
	/// Idicates that the shift key is being held down.
	/// Used to start a mouse drag rectangle.
	/// </summary>
	private bool shiftKeyDown;

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

	public SackPanel()
	{
		InitializeComponent();
	}

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
		this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.KeyUpCallback);
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
	public SackPanel(int sackWidth, int sackHeight, ItemDragInfo dragInfo, AutoMoveLocation autoMoveLocation, IServiceProvider serviceProvider)
	{
		InitializeComponent();

		this.ServiceProvider = serviceProvider;
		this.FontService = this.ServiceProvider.GetService<IFontService>();
		this.UIService = this.ServiceProvider.GetService<IUIService>();
		this.Database = this.ServiceProvider.GetService<IDatabase>();
		this.ItemProvider = this.ServiceProvider.GetService<IItemProvider>();
		this.TQData = this.ServiceProvider.GetService<ITQDataService>();
		this.TranslationService = this.ServiceProvider.GetService<ITranslationService>();
		this.userContext = this.ServiceProvider.GetService<SessionContext>();
		this.USettings = this.ServiceProvider.GetService<UserSettings>();
		this.HighlightService = this.ServiceProvider.GetService<IHighlightService>();
		this.ItemDatabaseService = this.ServiceProvider.GetService<IItemDatabaseService>();
		this.ExchangeService = this.ServiceProvider.GetService<IItemExchangeService>();

		this.Log = this.ServiceProvider.GetService<ILogger<SackPanel>>();

		this.DragInfo = dragInfo;
		this.AutoMoveLocation = autoMoveLocation;
		this.DragInfo.AddAutoMoveLocationToList(autoMoveLocation);
		this.LastCellWithFocus = InvalidDragLocation;
		this.contextMenuCellWithFocus = InvalidDragLocation;

		this.SackSize = new Size(sackWidth, sackHeight);
		this.SackType = SackType.Player;

		this.itemsUnderDragItem = new Collection<Item>();
		this.itemsUnderOldDragLocation = new Collection<Item>();

		this.borderPen = new Pen(Color.FromArgb(223, 188, 97));
		this.borderPen.Width = BorderWidth;

		this.DefaultImage = Resources.tqmedallion;
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
		this.BackColor = Color.FromArgb(46, 41, 31);

		this.CustomContextMenu.Renderer = new CustomProfessionalRenderer();
		this.CustomContextMenu.Font = FontService.GetFont(9.0F * UIService.Scale);

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
	public EventHandler<SackPanelEventArgs> OnClearAllItemsSelected { get; set; }

	/// <summary>
	/// Gets or sets the item selected event handler
	/// </summary>
	public EventHandler<SackPanelEventArgs> OnItemSelected { get; set; }

	/// <summary>
	/// Gets or sets the activate search event handler
	/// </summary>
	public EventHandler<SackPanelEventArgs> OnActivateSearch { get; set; }

	/// <summary>
	/// Gets or sets the resize form Event Handler
	/// </summary>
	public EventHandler<ResizeEventArgs> OnResizeForm { get; set; }

	/// <summary>
	/// Gets or sets the automove item event handler
	/// </summary>
	public EventHandler<SackPanelEventArgs> OnAutoMoveItem { get; set; }

	/// <summary>
	/// Gets or sets the new item highlighted event handler
	/// </summary>
	public EventHandler<SackPanelEventArgs> OnNewItemHighlighted { get; set; }

	/// <summary>
	/// Gets or sets the sack size
	/// </summary>
	public Size SackSize { get; protected set; }

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
	public bool SelectionActive => this.selectedItems != null && this.selectedItems.Count > 0;

	/// <summary>
	/// Gets the list of selected items.
	/// </summary>
	public IReadOnlyList<Item> GetSelectedItems()
		=> this.selectedItems?.AsReadOnly() ?? Array.Empty<Item>().AsReadOnly();

	/// <summary>
	/// Gets or sets the sack instance
	/// </summary>
	public SackCollection Sack
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
	public PlayerCollection PlayerCollection { get; set; }

	/// <summary>
	/// Gets MessageBoxOptions for right to left reading.
	/// </summary>
	protected static MessageBoxOptions RightToLeftOptions
	{
		get
		{
			// Set options for Right to Left reading.
			if (CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft)
				return MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading;

			return (MessageBoxOptions)0;
		}
	}

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
	protected Bitmap DefaultImage { get; set; }

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
	/// Cancels an item drag
	/// </summary>
	/// <param name="dragInfo">ItemDragInfo instance</param>
	public virtual void CancelDrag(ItemDragInfo dragInfo)
	{
		// if the drag sack is not visible then we really do not need to do anything
		if (this.Sack == dragInfo.SrcSack)
			this.Invalidate(this.GetItemScreenRectangle(dragInfo.Item));
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
	/// Override of ScaleControl which supports scaling of the fonts and internal items.
	/// </summary>
	/// <param name="factor">SizeF for the scale factor</param>
	/// <param name="specified">BoundsSpecified value.</param>
	protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
	{
		this.borderPen.Width = Math.Max((4.0F * UIService.Scale), 1.0F);
		this.numberFont = new Font(this.numberFont.Name, 10.0F * UIService.Scale, GraphicsUnit.Pixel);
		this.CustomContextMenu.Font = new Font(this.CustomContextMenu.Font.Name, 9.0F * UIService.Scale);

		this.Size = new Size(
			(Convert.ToInt32(this.borderPen.Width) * 2) + (UIService.ItemUnitSize * this.SackSize.Width),
			(Convert.ToInt32(this.borderPen.Width) * 2) + (UIService.ItemUnitSize * this.SackSize.Height));

		this.Location = new Point(
			Convert.ToInt32((float)this.originalLocation.X * UIService.Scale),
			Convert.ToInt32((float)this.originalLocation.Y * UIService.Scale));
	}

	/// <summary>
	/// Converts the screen coords into cell coords
	/// </summary>
	/// <param name="location">Point containing the screen coordinates</param>
	/// <returns>Point of corresponding cell coordinate</returns>
	protected Point FindCell(Point location)
	{
		int x;
		if (location.X < this.borderPen.Width)
			x = -1;
		else
			x = (location.X - (int)this.borderPen.Width) / UIService.ItemUnitSize;

		if (x >= this.SackSize.Width)
			x = -2;

		int y;
		if (location.Y < this.borderPen.Width)
			y = -1;
		else
			y = (location.Y - (int)this.borderPen.Width) / UIService.ItemUnitSize;

		if (y >= this.SackSize.Height)
			y = -2;

		return new Point(x, y);
	}

	/// <summary>
	/// Gets an item at a cell location
	/// </summary>
	/// <param name="cellLocation">Point of the cell location we are interested in</param>
	/// <returns>Item at that cell location</returns>
	protected virtual Item FindItem(Point cellLocation)
	{
		if (this.Sack == null)
			return null;

		// Find the item for this point
		foreach (Item item in this.Sack)
		{
			if (item == this.DragInfo.Item)
				// hide the item being dragged
				continue;

			// store the x and y values
			int x = item.PositionX;
			int y = item.PositionY;

			if (RecordId.IsNullOrEmpty(item.BaseItemId))
				// Skip over empty items
				continue;

			// see if this item overlaps the cell
			if ((x <= cellLocation.X) && ((x + item.Width - 1) >= cellLocation.X) &&
				(y <= cellLocation.Y) && ((y + item.Height - 1) >= cellLocation.Y))
			{
				return item;
			}
		}

		return null;
	}

	/// <summary>
	/// Find all items that are at least partially within the cell rectangle
	/// </summary>
	/// <param name="cellRectangle">cell rectangle we are looking at</param>
	/// <returns>Array of items within the rectangle</returns>
	protected virtual Collection<Item> FindAllItems(Rectangle cellRectangle)
	{
		Collection<Item> items = new Collection<Item>();
		int minX = cellRectangle.X;
		int maxX = cellRectangle.X + cellRectangle.Width;
		int minY = cellRectangle.Y;
		int maxY = cellRectangle.Y + cellRectangle.Height;

		for (int x = minX; x < maxX; ++x)
		{
			for (int y = minY; y < maxY; ++y)
			{
				Item item = this.FindItem(new Point(x, y));

				if (item != null)
				{
					// Add this item to the array if it is not already in there
					if (!items.Contains(item))
					{
						items.Add(item);
					}

					y = this.CellBottom(item);
				}
			}
		}

		return items;
	}

	/// <summary>
	/// Given the cell X and Y coordinates, get the screen coordinates for the top left corner of the cell
	/// </summary>
	/// <param name="location">cell coordinates</param>
	/// <returns>Point for the corresponding screen position of the top left corner of the cell.</returns>
	protected Point CellTopLeft(Point location)
	{
		return new Point(
			Convert.ToInt32(this.borderPen.Width) + (location.X * UIService.ItemUnitSize),
			Convert.ToInt32(this.borderPen.Width) + (location.Y * UIService.ItemUnitSize));
	}

	/// <summary>
	/// Given the cell X and Y coordinates, get the screen coordinates for the top right corner of the cell
	/// </summary>
	/// <param name="location">cell coordinates</param>
	/// <returns>Point for the corresponding screen position of the top right corner of the cell.</returns>
	protected Point CellTopRight(Point location)
	{
		return new Point(
			Convert.ToInt32(this.borderPen.Width) + ((location.X + 1) * UIService.ItemUnitSize) - 1,
			Convert.ToInt32(this.borderPen.Width) + (location.Y * UIService.ItemUnitSize));
	}

	/// <summary>
	/// Given the cell X and Y coordinates, get the screen coordinates for the bottom left corner of the cell
	/// </summary>
	/// <param name="location">cell coordinates</param>
	/// <returns>Point for the corresponding screen position of the bottom left corner of the cell.</returns>
	protected Point CellBottomLeft(Point location)
	{
		return new Point(
			Convert.ToInt32(this.borderPen.Width) + (location.X * UIService.ItemUnitSize),
			Convert.ToInt32(this.borderPen.Width) + ((location.Y + 1) * UIService.ItemUnitSize) - 1);
	}

	/// <summary>
	/// Gets the bottom cell of an item.  Not scaled to screen coordinates.
	/// </summary>
	/// <param name="item">Item which we are checking</param>
	/// <returns>Y value of the bottom cell of the item.</returns>
	protected virtual int CellBottom(Item item) => item.Height + item.Location.Y - 1;

	/// <summary>
	/// Given the cell X and Y coordinates, get the screen coordinates for the bottom right corner of the cell
	/// </summary>
	/// <param name="location">cell coordinates</param>
	/// <returns>Point for the corresponding screen position of the bottom right corner of the cell.</returns>
	protected Point CellBottomRight(Point location)
	{
		return new Point(
			Convert.ToInt32(this.borderPen.Width) + ((location.X + 1) * UIService.ItemUnitSize) - 1,
			Convert.ToInt32(this.borderPen.Width) + ((location.Y + 1) * UIService.ItemUnitSize) - 1);
	}

	/// <summary>
	/// Gets whether a particular item has been multi-selected
	/// </summary>
	/// <param name="item">Item we are testing</param>
	/// <returns>true if the item has been multi-selected</returns>
	protected bool IsItemSelected(Item item)
	{
		// Make sure we have something to check
		if (this.selectedItems == null || this.selectedItems.Count == 0)
			return false;

		// Iterate through the list of selected items
		foreach (Item selectedItem in this.selectedItems)
		{
			if (item == selectedItem)
				// We have a match
				return true;
		}

		return false;
	}

	/// <summary>
	/// Handler for picking up an item with the mouse
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">MouseEventArgs data</param>
	protected virtual void PickupItem(object sender, MouseEventArgs e)
	{
		// process the mouse moving to this location...just in case
		this.MouseMoveCallback(sender, e);

		Item focusedItem = this.FindItem(this.LastCellWithFocus);
		if (focusedItem != null)
		{
			// We picked up an item so clear the selection
			if (this.selectedItems != null)
			{
				// We might be over the only selected item so no need to redraw the whole sack.
				if (this.selectedItems.Count != 1 || focusedItem != this.selectedItems[0])
					this.Invalidate();

				this.ClearSelection();
			}

			// Send a message to clear selections in the other panels.
			this.OnClearAllItemsSelected(this, new SackPanelEventArgs(null, null));

			// pick up item.
			this.DragInfo.Set(this, this.Sack, focusedItem, this.GetMouseOffset(e.Location, focusedItem));

			// process mouse move again to apply the graphical effects of dragging an item.
			this.MouseMoveCallback(sender, e);

			ItemTooltip.HideTooltip();
			BagButtonTooltip.InvalidateCache(this.Sack);
		}
	}

	/// <summary>
	/// Calculates the mouse offset in screen coordinates within an item bitmap.
	/// </summary>
	/// <param name="location">Point containing the screen coordinates of the mouse.</param>
	/// <param name="item">Item that we are over</param>
	/// <returns>Point containing the mouse offset.</returns>
	protected virtual Point GetMouseOffset(Point location, Item item)
	{
		if (item == null)
			return Point.Empty;

		Point topLeft = this.CellTopLeft(item.Location);
		return new Point(location.X - topLeft.X, location.Y - topLeft.Y);
	}

	/// <summary>
	/// Adds an item to the selected items list and if it's already there it gets removed.
	/// </summary>
	/// <param name="item">Item we are adding to the selection list.</param>
	protected void SelectItem(Item item)
	{
		if (item == null)
			return;

		// Allocate the List if needed.
		if (this.selectedItems == null)
			this.selectedItems = new List<Item>();

		// Check to see if the item is already selected
		if (!this.IsItemSelected(item))
		{
			// Need to add the item to the list
			this.selectedItems.Add(item);
			this.selectedItemsChanged = true;
		}
		else
			// It's already there so we need to remove it.
			this.RemoveSelectedItem(item);

		this.OnItemSelected(this, new SackPanelEventArgs(null, null));
	}

	/// <summary>
	/// Removes an item from the selected items list.
	/// </summary>
	/// <param name="item">Item that we are removing</param>
	protected void RemoveSelectedItem(Item item)
	{
		if (item == null || this.selectedItems == null)
			return;

		if (this.IsItemSelected(item))
		{
			// Check to see if the item is already selected
			// If the item is in the list then remove it.
			this.selectedItems.Remove(item);
			this.selectedItemsChanged = true;

			if (this.selectedItems.Count == 0)
				this.ClearSelection();
		}
	}

	/// <summary>
	/// Indicates whether the item selection has changed.
	/// </summary>
	/// <returns>True if the item selection has changed and resets back to false afterward.</returns>
	protected bool IsSelectedItemsChanged()
	{
		// Check if the flag is set
		if (this.selectedItemsChanged)
		{
			// Reset the flag and return that the flag WAS set.
			this.selectedItemsChanged = false;
			return true;
		}
		else
			return false;
	}

	/// <summary>
	/// Clears the item selection array
	/// </summary>
	protected void ClearSelection()
	{
		if (this.selectedItems != null)
			this.selectedItems = null;
	}

	/// <summary>
	/// Handler for putting an item down with the mouse
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">MouseEventArgs data</param>
	protected virtual void TryToPutdownItem(object sender, MouseEventArgs e)
	{
		// process mouse moving to this location
		this.MouseMoveCallback(sender, e);

		// Make sure we have room to place the item here
		// we do this by making sure that:
		// (a) there are cells under us
		// (b) there is <= 1 items under us
		if (!this.CellsUnderDragItem.Size.IsEmpty && (this.ItemsUnderDragItem == null || this.ItemsUnderDragItem.Count <= 1))
		{
			Item dragItem = this.DragInfo.Item;

			/*
						if ((
							this.SackType == SackType.Player
							|| this.SackType == SackType.Equipment
							|| this.SackType == SackType.Player
							) && IsCurrentPlayerReadOnly()
						) return;
			 */

			if (!this.IsItemValidForPlacement(dragItem))
				return;

			if ((
				this.SackType == SackType.Player
				|| this.SackType == SackType.Equipment
				|| this.SackType == SackType.Player
				) && !IsSuitableForCurrentPlayer(dragItem)
			) return;

			// Yes we can drop it here!
			// First take the item that is under us
			Item itemUnderUs = null;
			if (this.ItemsUnderDragItem != null && this.ItemsUnderDragItem.Count == 1)
				itemUnderUs = this.ItemsUnderDragItem[0];

			// Maybe we are putting the item back.
			// Then we just cancel.
			if (itemUnderUs == this.DragInfo.Item && this.DragInfo.IsActive && this.DragInfo.CanBeCanceled)
			{
				this.DragInfo.Cancel();

				// Now redraw this sack
				this.Refresh();
				this.MouseMoveCallback(sender, e);
				return;
			}

			// Notify that the item has been placed.  This will remove it from the old sack
			this.DragInfo.MarkPlaced();

			// If we are a stackable and we have a stackable under us and we are the same type of stackable
			// then just add to the stack instead of picking up the other stack

			if (!(doStackPotions(dragItem, ref itemUnderUs) || doStackRelics(ref dragItem, ref itemUnderUs)))
			{
				// If not stackable
				// Drop the dragItem here
				dragItem.Location = this.CellsUnderDragItem.Location;

				// Now add the item to our sack
				this.Sack.AddItem(dragItem);

				// Update item location properties to reflect new position
				this.UpdateItemLocation(dragItem);

				// Register new items in the database (existing items are already tracked)
				this.ItemDatabaseService.TryAddItemToDatabase(dragItem);
			}

			// clear the "last drag" variables
			this.LastDragLocation = InvalidDragLocation;
			this.CellsUnderDragItem = InvalidDragRectangle;
			this.ItemsUnderOldDragLocation.Clear();
			this.ItemsUnderDragItem.Clear();

			// Now mark itemUnderUs as picked up.
			if (itemUnderUs != null && !RecordId.IsNullOrEmpty(itemUnderUs.BaseItemId))
			{
				// set our mouse offset to be the center of the item.
				Point mouseOffset = new Point(
					itemUnderUs.Width * UIService.HalfUnitSize,
					itemUnderUs.Height * UIService.HalfUnitSize);
				this.DragInfo.Set(this, this.Sack, itemUnderUs, mouseOffset);

				// since we have dropped something in this location, we can no longer put this item here.
				// mark its location as invalid to prevent that from happening.
				itemUnderUs.Location = InvalidDragLocation;

				// set the info to this item
				this.OnNewItemHighlighted(this, new SackPanelEventArgs(this.Sack, itemUnderUs));
			}

			// clear the "last focus variables"
			this.LastCellWithFocus = InvalidDragLocation;

			// Clear any selections.
			this.OnClearAllItemsSelected(this, new SackPanelEventArgs(null, null));

			this.InvalidateItemCacheAll(itemUnderUs, dragItem);

			// Repaint everything to clear up any graphical issues
			this.Refresh();

			// and now do a MouseMove() to properly draw the new drag item and/or focus
			this.MouseMoveCallback(sender, e);

			ItemTooltip.HideTooltip();
			BagButtonTooltip.InvalidateCache(this.Sack);

		}
	}

	protected bool doStackRelics(ref Item dragItem, ref Item itemUnderUs)
	{
		bool doStackRelics = dragItem.IsRelicOrCharm
			&& itemUnderUs != null && itemUnderUs.IsRelicOrCharm
			&& !itemUnderUs.IsRelicComplete && !dragItem.IsRelicComplete
			&& dragItem.BaseItemId.Equals(itemUnderUs.BaseItemId)
			&& !USettings.DisableAutoStacking;
		if (doStackRelics)
		{
			// Stack relics
			// Save the original Relic number
			int originalNumber = itemUnderUs.Number;

			// Adjust the item in the sack
			// This is limited to the completion level of the Relic
			itemUnderUs.Number += dragItem.Number;

			// Check if we completed the item
			if (itemUnderUs.IsRelicComplete)
			{
				float randPercent = (float)Item.GenerateSeed() / 0x7fff;
				LootTableCollection table = ItemProvider.BonusTableRelicOrArtifact(itemUnderUs);

				if (table != null && table.Length > 0)
				{
					int i = table.Length;
					foreach (var e1 in table)
					{
						i--;
						if (randPercent <= e1.Value.WeightPercent || i == 0)
						{
							itemUnderUs.RelicBonusId = e1.Key;
							break;
						}
						else
							randPercent -= e1.Value.WeightPercent;
					}
				}

				ItemProvider.GetDBData(itemUnderUs);
			}

			itemUnderUs.IsModified = true;

			this.InvalidateItemCacheAll(itemUnderUs, dragItem);

			// Just in case we have more relics than what we need to complete
			// We then adjust the one we are holding
			int adjustedNumber = itemUnderUs.Number - originalNumber;
			if (adjustedNumber != dragItem.Number)
			{
				dragItem.Number -= adjustedNumber;
				dragItem.IsModified = true;

				// Swap the items so the completed item stays in the
				// sack and the remaining items are still being dragged
				Item temp = itemUnderUs;
				itemUnderUs = dragItem;
				dragItem = temp;

				// Drop the dragItem here
				dragItem.Location = this.CellsUnderDragItem.Location;

				// Now add the item to our sack
				this.Sack.AddItem(dragItem);
			}
			else
			{
				this.Sack.IsModified = true;

				// Get rid of ref to itemUnderUs so code below wont do anything with it.
				itemUnderUs = null;
				// we will just throw away the dragItem now.
			}
		}
		return doStackRelics;
	}

	protected bool doStackPotions(Item dragItem, ref Item itemUnderUs)
	{
		bool doStackpotions = dragItem.DoesStack
			&& itemUnderUs != null && itemUnderUs.DoesStack
			&& dragItem.BaseItemId.Equals(itemUnderUs.BaseItemId)
			&& !USettings.DisableAutoStacking;
		if (doStackpotions)
		{
			itemUnderUs.StackSize += dragItem.StackSize;

			this.InvalidateItemCacheAll(itemUnderUs, dragItem);

			// Added this so the tooltip would update with the correct number
			itemUnderUs.IsModified = true;
			this.Sack.IsModified = true;

			// Get rid of ref to itemUnderUs so code below wont do anything with it.
			itemUnderUs = null;

			// we will just throw away the dragItem now.
		}
		return doStackpotions;
	}

	protected virtual void MouseUpCallback(object sender, MouseEventArgs e)
	{
		this.mouseDraw = false;
		Invalidate();
	}

	/// <summary>
	/// Handler for mouse button down click
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">MouseEventArgs data</param>
	protected virtual void MouseDownCallback(object sender, MouseEventArgs e)
	{
		if (this.Sack == null)
			return;

		var isEquipmentReadOnly = (USettings.PlayerReadonly == true && this.SackType == SackType.Equipment);

		if (e.Button == MouseButtons.Left && !isEquipmentReadOnly)
		{
			if (!this.DragInfo.IsActive)
			{
				// Detect a CTRL-Click.
				if (this.controlKeyDown)
					// Add the focused item to the selected items list.
					this.AddFocusedItemToSelectedItems(sender, e);
				// Detect a mouse drag for a multiselect.
				else if (this.shiftKeyDown)
				{
					this.mouseDraw = true;
					this.startPosition = this.currentPosition = e.Location;
					this.OnClearAllItemsSelected(this, new SackPanelEventArgs(null, null));
				}
				else
					this.PickupItem(sender, e);
			}
			else
			{
				// We are holding an item already so put it down.
				this.TryToPutdownItem(sender, e);
			}
		}
		else if (e.Button == MouseButtons.Right)
		{
			if (this.DragInfo.IsActive && this.DragInfo.CanBeCanceled && !isEquipmentReadOnly)
			{
				this.DragInfo.Cancel();

				// Now redraw this sack
				this.Refresh();
			}
			else
			{
				Item focusedItem = this.FindItem(this.LastCellWithFocus);
				bool singleSelectionFocused = false;
				if (this.selectedItems != null)
					singleSelectionFocused = focusedItem == (Item)this.selectedItems[0] && this.selectedItems.Count == 1;

				this.CustomContextMenu.Items.Clear();

				if (focusedItem != null)
					this.contextMenuCellWithFocus = this.LastCellWithFocus;

				if ((focusedItem != null || this.selectedItems != null) && !isEquipmentReadOnly)
				{
					this.CustomContextMenu.Items.Add(Resources.SackPanelMenuDelete);
					this.CustomContextMenu.Items.Add("-");
				}

				if (focusedItem != null && (this.selectedItems == null || singleSelectionFocused) && !isEquipmentReadOnly)
				{
					if (USettings.AllowItemEdit)
					{
						if (focusedItem.HasRelicOrCharmSlot1)
							this.CustomContextMenu.Items.Add(Resources.SackPanelMenuRemoveRelic);

						if (focusedItem.HasRelicOrCharmSlot2)
							this.CustomContextMenu.Items.Add(Resources.SackPanelMenuRemoveRelic2);
					}

					if (focusedItem.DoesStack && focusedItem.Number > 1)
						this.CustomContextMenu.Items.Add(Resources.SackPanelMenuSplit);
				}

				if (focusedItem != null && (this.selectedItems == null || singleSelectionFocused))
				{
					if (USettings.AllowItemCopy)
					{
						this.CustomContextMenu.Items.Add(Resources.SackPanelMenuCopy);
						this.CustomContextMenu.Items.Add(Resources.SackPanelMenuDuplicate);
					}
				}

				AddExportMenuItems(focusedItem, singleSelectionFocused);

				if ((focusedItem != null || this.selectedItems != null) && !isEquipmentReadOnly)
				{
					AddAutoMoveMenuItems(focusedItem);
				}

				if ((focusedItem != null && (this.selectedItems == null || singleSelectionFocused)) && !isEquipmentReadOnly)
				{
					// Item Editing options
					if (USettings.AllowItemEdit)
					{
						AddRegularItemEditMenuItems(focusedItem);

						AddPrefixSuffixMenuItems(focusedItem);

						AddSocketedItemCompletionBonusMenuItems(focusedItem);

						AddRelicOrArticaftCompletionBonusMenuItems(focusedItem);

						AddItemSetMenuItems(focusedItem);
					}
				}

				if (focusedItem != null && (this.selectedItems == null || singleSelectionFocused))
				{
					this.CustomContextMenu.Items.Add("-");
					this.CustomContextMenu.Items.Add(Resources.SackPanelMenuProperties);
				}

				if (this.selectedItems != null && !isEquipmentReadOnly)
					this.CustomContextMenu.Items.Add(Resources.SackPanelMenuClear);

				if ((focusedItem != null || this.selectedItems != null) && this.CustomContextMenu.Items.Count > 0)
					this.CustomContextMenu.Show(this, new Point(e.X, e.Y));
			}
		}
	}

	/// <summary>
	/// Changes the background of the item under the mouse so that it appears highlighted
	/// </summary>
	/// <param name="cell">Point containing the cell that the mouse is over</param>
	protected virtual void HighlightItemUnderMouse(Point cell)
	{
		// Check to see if the item needs to be redrawn if the selected items list got changed.
		bool redrawSelection = this.IsSelectedItemsChanged();

		if (cell != this.LastCellWithFocus || redrawSelection)
		{
			// We have moved to a different cell
			Item lastItem = this.FindItem(this.LastCellWithFocus);
			Item newItem = this.FindItem(cell);

			if (newItem != lastItem || redrawSelection)
			{
				if (lastItem != null)
					this.Invalidate(GetItemScreenRectangle(lastItem));

				if (cell != this.LastCellWithFocus)
					this.LastCellWithFocus = cell;

				if (newItem != lastItem)
				{
					if (newItem != null)
						this.Invalidate(GetItemScreenRectangle(newItem));

					this.OnNewItemHighlighted(this, new SackPanelEventArgs(this.Sack, newItem));
				}
			}
		}
	}

	/// <summary>
	/// Find the rectanglular area of cell coords affected by these screen coords
	/// </summary>
	/// <param name="screenRectangle">Rectangle with the screen coordinates which will be converted to cell coordinates.</param>
	/// <returns>Rectangle containing the cells within the screen area</returns>
	protected virtual Rectangle FindAllCells(Rectangle screenRectangle)
	{
		int endX = screenRectangle.Right - 1;
		int endY = screenRectangle.Bottom - 1;

		Point screenTL = this.CellTopLeft(Point.Empty);
		Point screenBR = this.CellBottomRight(new Point(this.SackSize.Width - 1, this.SackSize.Height - 1));
		Point topLeft = new Point(-1, -1);
		Point bottomRight = new Point(-1, -1);

		// First trim the rectangle to our panel cell region
		// See if any of the X's are in our area
		if (endX >= screenTL.X && screenRectangle.X <= screenBR.X)
		{
			topLeft.X = Math.Max(screenRectangle.X, screenTL.X);
			bottomRight.X = Math.Min(endX, screenBR.X);
		}

		// See if any of the Y's are in our area
		if (endY >= screenTL.Y && screenRectangle.Y <= screenBR.Y)
		{
			topLeft.Y = Math.Max(screenRectangle.Y, screenTL.Y);
			bottomRight.Y = Math.Min(endY, screenBR.Y);
		}

		// Okay now convert topLeft and bottomRight to Cell coords and return it.
		topLeft = this.FindCell(topLeft);
		bottomRight = this.FindCell(bottomRight);
		return new Rectangle(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X + 1, bottomRight.Y - topLeft.Y + 1);
	}

	/// <summary>
	/// Gets the item Rectangle converted to screen coordinates.
	/// </summary>
	/// <param name="item">Item that needs screen coordinates</param>
	/// <returns>Rectangle containing the screen coordinates occupied by the item.</returns>
	protected virtual Rectangle GetItemScreenRectangle(Item item)
	{
		if (item == null)
			return Rectangle.Empty;

		Point screenLocation = this.CellTopLeft(item.Location);
		return new Rectangle(screenLocation.X, screenLocation.Y, item.Size.Width * UIService.ItemUnitSize, item.Size.Height * UIService.ItemUnitSize);
	}

	/// <summary>
	/// Gets a rectangle of the screen coordinates for all of the items that need to be redrawn from an item drag.
	/// </summary>
	/// <returns>Rectangle of the redraw area.</returns>
	protected virtual Rectangle GetRepaintDragRect()
	{
		// Figure out the rectangle that needs to be redrawn
		int x = this.LastDragLocation.X;
		int y = this.LastDragLocation.Y;
		var ibmp = this.UIService.GetBitmap(this.DragInfo.Item);
		int width = Convert.ToInt32(ibmp.Width * UIService.Scale);
		int height = Convert.ToInt32(ibmp.Height * UIService.Scale);

		// We also know we need to wipe out any cells under the old drag point
		// This is used to restore the background after highlighting the cells underneath
		Rectangle cellRectangle = this.FindAllCells(new Rectangle(x, y, width, height));
		if (!cellRectangle.Size.IsEmpty)
		{
			Point tl = this.CellTopLeft(cellRectangle.Location);
			Point br = this.CellBottomRight(new Point(cellRectangle.Right - 1, cellRectangle.Bottom - 1));

			// Extend our clipping rectangle to include all these cells
			int oldX = x;
			int oldY = y;
			x = Math.Min(x, tl.X);
			y = Math.Min(y, tl.Y);
			width = Math.Max(oldX + width - 1, br.X) - x + 1;
			height = Math.Max(oldY + height - 1, br.Y) - y + 1;
		}

		// We also need to wipe out any items that were under or are now under the dragItem so they can be redrawn
		if (this.ItemsUnderOldDragLocation != null && this.ItemsUnderOldDragLocation.Count != 0)
		{
			foreach (Item item in this.ItemsUnderOldDragLocation)
			{
				// well we do not actually need to clear the item and redraw it since that will happen automatically
				// in the PaintCallback().  We just need to include this item in the clip region
				Point topLeft = this.CellTopLeft(item.Location);
				Point bottomRight = this.CellBottomRight(new Point(item.Right - 1, item.Bottom - 1));

				// Extend our clipping rectangle to include all these cells
				int oldX = x;
				int oldY = y;
				x = Math.Min(x, topLeft.X);
				y = Math.Min(y, topLeft.Y);
				width = Math.Max(oldX + width - 1, bottomRight.X) - x + 1;
				height = Math.Max(oldY + height - 1, bottomRight.Y) - y + 1;
			}
		}

		if (this.ItemsUnderDragItem != null && this.ItemsUnderDragItem.Count != 0)
		{
			foreach (Item item in this.ItemsUnderDragItem)
			{
				// well we do not actually need to clear the item and redraw it since that will happen automatically
				// in the PaintCallback().  We just need to include this item in the clip region
				Point topLeft = this.CellTopLeft(item.Location);
				Point bottomRight = this.CellBottomRight(new Point(item.Right - 1, item.Bottom - 1));

				// Extend our clipping rectangle to include all these cells
				int oldX = x;
				int oldY = y;
				x = Math.Min(x, topLeft.X);
				y = Math.Min(y, topLeft.Y);
				width = Math.Max(oldX + width - 1, bottomRight.X) - x + 1;
				height = Math.Max(oldY + height - 1, bottomRight.Y) - y + 1;
			}
		}

		return new Rectangle(x, y, width, height);
	}

	/// <summary>
	/// Callback for moving the mouse
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">MouseEventArgs data</param>
	protected void MouseMoveCallback(object sender, MouseEventArgs e)
	{
		if (this.Sack == null)
			return;

		if (!this.DragInfo.IsActive)
		{
			if (this.mouseDraw)
			{
				this.OnClearAllItemsSelected(this, new SackPanelEventArgs(null, null));
				currentPosition = e.Location;
				this.SelectItemsInMouseDraw(sender, e);
				this.Invalidate();
			}
			else
				this.HighlightItemUnderMouse(this.FindCell(e.Location));
		}
		else
			this.Invalidate();
	}

	/// <summary>
	/// Finds the rectangle under the mouse drag
	/// </summary>
	/// <returns>Rectangle representing the mouse drag area</returns>
	protected Rectangle GetMouseDragRectangle()
		=> new Rectangle(Math.Min(startPosition.X, currentPosition.X), Math.Min(startPosition.Y, currentPosition.Y), Math.Abs(startPosition.X - currentPosition.X), Math.Abs(startPosition.Y - currentPosition.Y));

	/// <summary>
	/// Indicates whether the passed item meets the item requirementt for equipping
	/// </summary>
	/// <param name="item">Item to check</param>
	/// <returns>True if item is able to be equipped</returns>
	protected virtual bool PlayerMeetRequierements(Item item)
	{
		var reqs = this.ItemProvider.GetFriendlyNames(item, FriendlyNamesExtraScopes.Requirements).RequirementVariables;
		var currPlayer = this.userContext.CurrentPlayer;
		if (currPlayer != null && reqs != null && reqs.Any() && !currPlayer.IsPlayerMeetRequierements(reqs))
			return false;

		return true;
	}

	/// <summary>
	/// Indicates whether the current player file can be edited.
	/// </summary>
	/// <returns></returns>
	protected virtual bool IsCurrentPlayerReadOnly()
	{
		var currPlayer = this.userContext.CurrentPlayer;
		if (!(currPlayer?.IsImmortalThrone ?? false) // TODO for now TQ Original Player is read only but could be issue #268
		) return true;

		return false;
	}
	/// <summary>
	/// Indicates whether the passed item is suitable for equipping.
	/// e.g. an Immortal throne or greater item on a Titan Quest Original player.
	/// </summary>
	/// <param name="item"></param>
	/// <returns></returns>
	protected virtual bool IsSuitableForCurrentPlayer(Item item)
	{
		var currPlayer = this.userContext.CurrentPlayer;

		if (currPlayer is not null && !currPlayer.IsImmortalThrone // Player is TQ Original
			&& item.GameDlc != GameDlc.TitanQuest // Non base game item
		) return false;

		return true;
	}

	#endregion SackPanel Protected Methods

	private void InvalidateItemCacheAll(params Item[] items)
	{
		ItemTooltip.InvalidateCache(items);
		BagButtonTooltip.InvalidateCache(items);
		this.ItemProvider.InvalidateFriendlyNamesCache(items);
	}

	private void InvalidateItemCacheItemTooltip(params Item[] items)
	{
		ItemTooltip.InvalidateCache(items);
		this.ItemProvider.InvalidateFriendlyNamesCache(items);
	}

	/// <summary>
	/// Callback for key down
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">KeyEventArgs data</param>
	private void KeyDownCallback(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.ControlKey)
			this.controlKeyDown = true;

		if (e.KeyCode == Keys.ShiftKey)
			this.shiftKeyDown = true;

		if (e.KeyData == (Keys.Control | Keys.F))
			this.OnActivateSearch(this, new SackPanelEventArgs(null, null));

		if (e.KeyData == (Keys.Control | Keys.A))
			this.SelectAllItems();

		if (e.KeyData == (Keys.Control | Keys.S))
			this.SelectAllHighlightedItems();

		if (e.KeyData == (Keys.Alt | Keys.W))
			this.MoveSelectedItemsInSack("up");

		if (e.KeyData == (Keys.Alt | Keys.A))
			this.MoveSelectedItemsInSack("left");

		if (e.KeyData == (Keys.Alt | Keys.S))
			this.MoveSelectedItemsInSack("down");

		if (e.KeyData == (Keys.Alt | Keys.D))
			this.MoveSelectedItemsInSack("right");

		if (e.KeyData == (Keys.Control | Keys.D))
			this.OnClearAllItemsSelected(this, new SackPanelEventArgs(null, null));

		if (e.KeyData == (Keys.Control | Keys.Add) || e.KeyData == (Keys.Control | Keys.Oemplus))
			this.OnResizeForm(this, new ResizeEventArgs(0.1F));

		if (e.KeyData == (Keys.Control | Keys.Subtract) || e.KeyData == (Keys.Control | Keys.OemMinus))
			this.OnResizeForm(this, new ResizeEventArgs(-0.1F));

		if (e.KeyData == (Keys.Control | Keys.Home))
			this.OnResizeForm(this, new ResizeEventArgs(1.0F));

		if (e.KeyData == Keys.F5)
			this.Refresh();

		if (e.KeyData == Keys.Right)
			QuickMovePanel(AutoMoveLocation.Player);

		if (e.KeyData == Keys.Left)
			QuickMovePanel(AutoMoveLocation.Vault);

		if (e.KeyData == Keys.Down)
			QuickMovePanel(AutoMoveLocation.Stash);

		if (char.IsDigit((char)e.KeyData) || e.KeyData == Keys.OemMinus || e.KeyData == Keys.Oemplus)
		{
			int keyOffset = 49;

			switch (e.KeyData)
			{
				case Keys.Oemplus:
					{
						keyOffset = 176;
						break;
					}
				case Keys.OemMinus:
					{
						keyOffset = 179;
						break;
					}
				case Keys.D0:
					{
						keyOffset = 39;
						break;
					}
				default:
					break;
			}

			QuickMoveSack(e.KeyValue - keyOffset);
		}
	}

	/// <summary>
	/// Callback for key up
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">KeyEventArgs data</param>
	private void KeyUpCallback(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.ControlKey)
			this.controlKeyDown = false;

		if (e.KeyCode == Keys.ShiftKey)
			this.shiftKeyDown = false;
	}

	/// <summary>
	/// Callback for key press
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">KeyPressEventArgs data</param>
	private void KeyPressCallback(object sender, KeyPressEventArgs e)
	{
		if (this.Sack != null)
		{
			if (e.KeyChar == (char)27 && this.DragInfo.IsActive && this.DragInfo.CanBeCanceled)
			{
				this.DragInfo.Cancel();

				// Now redraw this sack
				this.Refresh();
				e.Handled = true;
			}
			else if (e.KeyChar == 'c' && USettings.AllowItemCopy == true)
			{
				// Copy
				Item focusedItem = this.FindItem(this.LastCellWithFocus);

				if (focusedItem != null)
				{
					// Set DragInfo to focused item.
					this.DragInfo.Set(this, this.Sack, focusedItem, new Point(1, 1));

					// copy the item
					Item newItem = focusedItem.Duplicate(false);

					// now drag it
					this.DragInfo.MarkModified(newItem);
					this.Refresh();
					e.Handled = true;
				}
			}
			else if (e.KeyChar == (char)8)
			{
				// Delete
				Item focusedItem = this.FindItem(this.LastCellWithFocus);

				if (focusedItem != null)
				{
					this.DeleteItem(focusedItem, false);
					this.Refresh();
					e.Handled = true;
				}
			}
			else if (e.KeyChar == 'd' && USettings.AllowItemCopy == true)
			{
				// Duplicate
				Item focusedItem = this.FindItem(this.LastCellWithFocus);

				if (focusedItem != null)
				{
					// Set DragInfo to focused item.
					this.DragInfo.Set(this, this.Sack, focusedItem, new Point(1, 1));

					// duplicate the item
					Item newItem = focusedItem.Duplicate(true);

					// now drag it
					this.DragInfo.MarkModified(newItem);
					this.Refresh();
					e.Handled = true;
				}
			}

			ItemTooltip.HideTooltip();
			BagButtonTooltip.InvalidateCache(this.Sack);
		}
	}

	private void MouseEnterCallback(object sender, EventArgs e) => this.Select();

	private void MouseLeaveCallback(object sender, EventArgs e)
	{
		this.controlKeyDown = false;
		if (this.Sack == null)
			return;

		if (!this.DragInfo.IsActive)
			this.HighlightItemUnderMouse(new Point(-1, -1));
		else
		{
			try
			{
				this.CellsUnderDragItem = InvalidDragRectangle;
				this.ItemsUnderDragItem.Clear();
				this.LastDragLocation = InvalidDragLocation;
				this.ItemsUnderOldDragLocation.Clear();
				this.Invalidate();
			}
			catch (NullReferenceException exception)
			{
				Log.ErrorException(exception);
				MessageBox.Show(Log.FormatException(exception), Resources.GlobalError
					, MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, RightToLeftOptions);
			}
		}
	}

	/// <summary>
	/// Updates the item's location properties to reflect its current position in this panel.
	/// This ensures search results always show the correct location.
	/// Also adds the item to the search database if it's not already present.
	/// </summary>
	/// <param name="item">The item to update</param>
	protected void UpdateItemLocation(Item item)
	{
		if (item == null)
			return;

		item.Place.SackType = this.SackType;
		item.Place.Path = this.PlayerCollection?.PlayerFile ?? string.Empty;
		item.Place.Name = this.PlayerCollection?.PlayerName ?? string.Empty;
		item.Place.StashType = this.Sack.StashType;
		item.Place.SackNumber = (this.Sack.SackType, this.Sack.StashType) switch
		{
			(SackType.Stash, _) => (int)this.Sack.StashType!,
			(SackType.Equipment, _) => BagIdConstants.BAGID_EQUIPMENTPANEL,
			_ => this.CurrentSack,
		};
	}
	
}
