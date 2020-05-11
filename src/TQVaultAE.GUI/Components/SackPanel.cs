//-----------------------------------------------------------------------
// <copyright file="SackPanel.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI.Components
{
	using Microsoft.Extensions.DependencyInjection;
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Drawing;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Windows.Forms;
	using TQVaultAE.GUI.Models;
	using TQVaultAE.Logs;
	using TQVaultAE.Domain.Entities;
	using TQVaultAE.Presentation;
	using TQVaultAE.GUI.Tooltip;
	using TQVaultAE.Domain.Contracts.Services;
	using TQVaultAE.Domain.Contracts.Providers;
	using TQVaultAE.Domain.Helpers;
	using Microsoft.Extensions.Logging;
	using System.Net.Http.Headers;

	/// <summary>
	/// Class for holding all of the UI functions of the sack panel.
	/// </summary>
	public class SackPanel : Panel, IScalingControl
	{
		private readonly ILogger Log = null;
		protected readonly IFontService FontService;
		protected readonly IUIService UIService;
		protected readonly IDatabase Database;
		protected readonly IItemProvider ItemProvider;
		protected readonly ITQDataService TQData;
		private readonly ITranslationService TranslationService;
		protected readonly IServiceProvider ServiceProvider;
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
		private SessionContext userContext;

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
		private ContextMenuStrip contextMenu;

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

		#endregion SackPanel Fields

		public SackPanel()
		{
			InitializeComponent();
		}

		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.SuspendLayout();
            // 
            // contextMenu
            // 
            this.contextMenu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(41)))), ((int)(((byte)(31)))));
            this.contextMenu.Font = new System.Drawing.Font("Albertus MT", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.contextMenu.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.contextMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenu.Name = "contextMenu";
            this.contextMenu.Opacity = 0.8D;
            this.contextMenu.ShowImageMargin = false;
            this.contextMenu.Size = new System.Drawing.Size(36, 4);
            this.contextMenu.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.ContextMenuItemClicked);
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

			this.Log = this.ServiceProvider.GetService<ILogger<SackPanel>>();

			this.DragInfo = dragInfo;
			this.AutoMoveLocation = autoMoveLocation;
			this.DragInfo.AddAutoMoveLocationToList(autoMoveLocation);
			this.LastCellWithFocus = InvalidDragLocation;
			this.contextMenuCellWithFocus = InvalidDragLocation;

			this.SackSize = new Size(sackWidth, sackHeight);
			this.SackType = SackType.Sack;

			this.itemsUnderDragItem = new Collection<Item>();
			this.itemsUnderOldDragLocation = new Collection<Item>();

			this.borderPen = new Pen(Color.FromArgb(223, 188, 97));
			this.borderPen.Width = BorderWidth;

			this.DefaultItemBackgroundColor = Color.FromArgb(220, 220, 220);  // White
			this.HighlightValidItemColor = Color.FromArgb(23, 149, 15);       // Green
			this.HighlightInvalidItemColor = Color.FromArgb(153, 28, 28);     // Red

			this.gridPen = new Pen(Color.FromArgb(142, 140, 129));

			this.numberFont = new Font("Arial", 10.0F * UIService.Scale, GraphicsUnit.Pixel);
			this.numberBrush = new SolidBrush(Color.White);
			this.numberFormat = new StringFormat();
			this.numberFormat.Alignment = StringAlignment.Far; // right-justify

			this.Size = new Size(
				(Convert.ToInt32(this.borderPen.Width) * 2) + (UIService.ItemUnitSize * sackWidth),
				(Convert.ToInt32(this.borderPen.Width) * 2) + (UIService.ItemUnitSize * sackHeight));
			this.BackColor = Color.FromArgb(46, 41, 31);

			this.contextMenu.Renderer = new CustomProfessionalRenderer();
			this.contextMenu.Font = FontService.GetFont(9.0F * UIService.Scale);

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
		/// Gets a value indicating whether items have been selected
		/// </summary>
		public bool SelectionActive => this.selectedItems != null && this.selectedItems.Count > 0;

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
		protected  int UserAlpha => Config.Settings.Default.ItemBGColorOpacity > 127 ? 127 : Config.Settings.Default.ItemBGColorOpacity;
		
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
		/// Sorts the items in the sack by size
		/// </summary>
		public void Autosort()
		{
			// Check to see if something is picked up.
			if (this.DragInfo.IsActive)
			{
				MessageBox.Show(
					Resources.SackPanelAutoSortMsg,
					Resources.SackPanelAutoSort,
					MessageBoxButtons.OK,
					MessageBoxIcon.None,
					MessageBoxDefaultButton.Button1,
					RightToLeftOptions);

				return;
			}

			// Sort the items and put them in a temporary sack.
			SackCollection tempSack = new SackCollection();
			var autoSortQuery = from Item item in this.Sack
								orderby (((item.Height * 3) + item.Width) * 100) descending, item.ItemGroup descending, item.BaseItemId, item.IsRelicComplete descending
								select item;

			foreach (Item item in autoSortQuery)
				tempSack.AddItem(item);

			if (tempSack == null || tempSack.IsEmpty)
				return;

			SackCollection backUpSack = this.Sack.Duplicate();

			this.Sack.EmptySack();
			// Toggle sorting direction.
			this.sortVertical = !this.sortVertical;

			int param1 = this.SackSize.Height;
			int param2 = this.SackSize.Width;
			if (this.sortVertical)
			{
				param1 = this.SackSize.Width;
				param2 = this.SackSize.Height;
			}

			foreach (Item tempItem in tempSack)
			{
				bool itemPlaced = false;
				int offset;

				if (this.sortVertical)
					offset = tempItem.Width - 1;
				else
					offset = tempItem.Height - 1;

				for (int j = 0; j < param1 - offset; ++j)
				{
					for (int k = 0; k < param2; ++k)
					{
						Collection<Item> foundItems;
						if (this.sortVertical)
						{
							foundItems = this.FindAllItems(new Rectangle(j, k, tempItem.Width, tempItem.Height));
							if ((foundItems == null || foundItems.Count == 0) && tempItem.Width + j <= this.SackSize.Width)
							{
								// The slot is free so try to place the item.
								if (k + tempItem.Height <= this.SackSize.Height)
								{
									// Make sure we do not extend beyond the sack borders.
									tempItem.PositionX = j;
									tempItem.PositionY = k;
									this.Sack.AddItem(tempItem);
									itemPlaced = true;
								}

								break;
							}
							else
								// Skip over the item
								k += foundItems[0].Height - 1;
						}
						else
						{
							foundItems = this.FindAllItems(new Rectangle(k, j, tempItem.Width, tempItem.Height));
							if ((foundItems == null || foundItems.Count == 0) && tempItem.Height + j <= this.SackSize.Height)
							{
								// The slot is free so try to place the item.
								if (k + tempItem.Width <= this.SackSize.Width)
								{
									// Make sure we do not extend beyond the sack borders.
									tempItem.PositionX = k;
									tempItem.PositionY = j;
									this.Sack.AddItem(tempItem);
									itemPlaced = true;
								}

								break;
							}
							else
								// Skip over the item
								k += foundItems[0].Width - 1;
						}
					}

					// Check to see if the item was placed and
					// move on to the next item.
					if (itemPlaced)
						break;
				}

				// We could not find a place for the item,
				// so we have a problem since they all should fit.
				if (!itemPlaced)
				{
					this.sortVertical = !this.sortVertical;
					this.Sack.EmptySack();

					foreach (Item item in backUpSack)
						this.Sack.AddItem(item.Clone());

					this.Sack.IsModified = false;
					BagButtonTooltip.InvalidateCache(this.Sack);
					return;
				}
			}

			// Redraw the sack.
			this.Invalidate();
			BagButtonTooltip.InvalidateCache(this.Sack);
		}

		/// <summary>
		/// Finds the coordinates in the panel to place an item of a given size.
		/// Searches vertically for open space and returns (-1, -1) on failure.
		/// </summary>
		/// <param name="width">width of the item we are placing</param>
		/// <param name="height">height of the item we are placing</param>
		/// <returns>Point with the coordinates for the upper left cell in the block of cell which will fit the item.</returns>
		public Point FindOpenCells(int width, int height)
		{
			Collection<Item> foundItems;
			for (int j = 0; j < this.SackSize.Width - width + 1; ++j)
			{
				for (int k = 0; k < this.SackSize.Height; ++k)
				{
					foundItems = this.FindAllItems(new Rectangle(j, k, width, height));
					if ((foundItems == null || foundItems.Count == 0) && width + j <= this.SackSize.Width)
					{
						if (k + height <= this.SackSize.Height)
							// The slot is free.
							return new Point(j, k);
					}
					else
						k += foundItems[0].Height - 1;
				}
			}

			// We could not find a place for the item.
			return new Point(-1, -1);
		}

		/// <summary>
		/// Selects and highlights an item.
		/// </summary>
		/// <param name="location">location coordinates of the item we are highlighting</param>
		public void SelectItem(Point location)
		{
			Item item = this.FindItem(location);

			if (item != null)
			{
				this.SelectItem(item);
				this.Invalidate();
			}
		}

		/// <summary>
		/// Clears all selected items.
		/// </summary>
		public void ClearSelectedItems()
		{
			if (this.selectedItems != null)
			{
				this.ClearSelection();
				this.Invalidate();
			}
		}

		/// <summary>
		/// Selects all items in the sack
		/// </summary>
		public void SelectAllItems()
		{
			if (this.Sack != null && !this.Sack.IsEmpty)
			{
				if (this.selectedItems != null)
					this.ClearSelection();

				foreach (Item item in this.Sack)
					this.SelectItem(item);

				this.Invalidate();
			}
		}

		/// <summary>
		/// Merges the items from two sacks into one sack
		/// </summary>
		/// <param name="destination">sack number where we are storing the combined sack</param>
		/// <returns>true if successful</returns>
		public bool MergeSack(int destination)
		{
			// Do a little bit of error handling
			if (destination < 0 || destination > this.MaxSacks - 1 || destination == this.CurrentSack)
				return false;

			if (!this.DragInfo.IsActive)
			{
				// Make sure we are not holding an item.
				if (this.Sack != null && !this.Sack.IsEmpty)
				{
					var mergeQuery = from Item item in this.Sack
									 where item != null
									 orderby (((item.Height * 3) + item.Width) * 100) + item.ItemGroup descending
									 select item;

					foreach (Item item in mergeQuery)
					{
						if (!this.DragInfo.IsActive)
						{
							// Check to make sure the last item got placed.
							this.DragInfo.Set(this, this.Sack, item, new Point(1, 1));
							this.DragInfo.AutoMove = (AutoMoveLocation)destination;
							this.OnAutoMoveItem(this, new SackPanelEventArgs(null, null));
						}
					}

					// Just in case clear any selected items.
					this.ClearSelectedItems();

					ItemTooltip.HideTooltip();
					BagButtonTooltip.InvalidateCache(this.Sack);

					// We moved something so return a true.
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Checks if item is eligible for placement in this stack
		/// </summary>
		/// <param name="itemToCheck">Item the method is going to validate</param>
		/// <returns></returns>
		public bool IsItemValidForPlacement(Item itemToCheck)
			=> this.sack.StashType != SackType.RelicVaultStash || itemToCheck.IsArtifact || itemToCheck.IsRelic || itemToCheck.IsFormulae || itemToCheck.IsCharm;

		/// <summary>
		/// Cancels an item drag
		/// </summary>
		/// <param name="dragInfo">ItemDragInfo instance</param>
		public virtual void CancelDrag(ItemDragInfo dragInfo)
		{
			// if the drag sack is not visible then we really do not need to do anything
			if (this.Sack == dragInfo.Sack)			
				this.Invalidate(this.GetItemScreenRectangle(dragInfo.Item));							
		}

		#endregion SackPanel Public Methods

		#region SackPanel Protected Methods

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
			this.contextMenu.Font = new Font(this.contextMenu.Font.Name, 9.0F * UIService.Scale);

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

				if (string.IsNullOrEmpty(item.BaseItemId))
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

				if (!this.IsItemValidForPlacement(dragItem))
					return;

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
				if (dragItem.DoesStack
					&& itemUnderUs != null
					&& itemUnderUs.DoesStack
					&& dragItem.BaseItemId.Equals(itemUnderUs.BaseItemId)
				)
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
				else if (dragItem.IsRelic
					&& itemUnderUs != null
					&& itemUnderUs.IsRelic
					&& !itemUnderUs.IsRelicComplete
					&& !dragItem.IsRelicComplete
					&& dragItem.BaseItemId.Equals(itemUnderUs.BaseItemId)
				)
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
						LootTableCollection table = ItemProvider.BonusTable(itemUnderUs);

						if (table != null && table.Length > 0)
						{
							int i = table.Length;
							foreach (KeyValuePair<string, float> e1 in table)
							{
								i--;
								if (randPercent <= e1.Value || i == 0)
								{
									itemUnderUs.RelicBonusId = e1.Key;
									break;
								}
								else
									randPercent -= e1.Value;
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
				else
				{
					// Drop the dragItem here
					dragItem.Location = this.CellsUnderDragItem.Location;

					// Now add the item to our sack
					this.Sack.AddItem(dragItem);
				}

				// clear the "last drag" variables
				this.LastDragLocation = InvalidDragLocation;
				this.CellsUnderDragItem = InvalidDragRectangle;
				this.ItemsUnderOldDragLocation.Clear();
				this.ItemsUnderDragItem.Clear();

				// Now mark itemUnderUs as picked up.
				if (itemUnderUs != null && !string.IsNullOrEmpty(itemUnderUs.BaseItemId))
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

			var isEquipmentReadOnly = (Config.Settings.Default.PlayerReadonly == true && this.SackType == SackType.Equipment);

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

					this.contextMenu.Items.Clear();

					if (focusedItem != null)
						this.contextMenuCellWithFocus = this.LastCellWithFocus;

					if ((focusedItem != null || this.selectedItems != null) && !isEquipmentReadOnly)
					{
						this.contextMenu.Items.Add(Resources.SackPanelMenuDelete);
						this.contextMenu.Items.Add("-");
					}

					if (focusedItem != null && (this.selectedItems == null || singleSelectionFocused) && !isEquipmentReadOnly)
					{
						if (focusedItem.HasRelicSlot1 && Config.Settings.Default.AllowItemEdit)
							this.contextMenu.Items.Add(Resources.SackPanelMenuRemoveRelic);

						if (focusedItem.DoesStack && focusedItem.Number > 1)
							this.contextMenu.Items.Add(Resources.SackPanelMenuSplit);
					}

					if (focusedItem != null && (this.selectedItems == null || singleSelectionFocused))
					{
						if (Config.Settings.Default.AllowItemCopy)
						{
							this.contextMenu.Items.Add(Resources.SackPanelMenuCopy);
							this.contextMenu.Items.Add(Resources.SackPanelMenuDuplicate);
						}
					}

					if ((focusedItem != null || this.selectedItems != null) && !isEquipmentReadOnly)
					{
						List<string> choices = new List<string>();

						if (this.MaxSacks > 1)
						{
							// Calculate offsets for the Player's sack panels.
							int offset = 1; // This is for the numerical display in the menu.
							int offset2 = 0; // This is for comparison of the current sack.

							if (this.SackType == SackType.Player || this.SackType == SackType.Sack)
							{
								// Since the player panel bag's are already starting with 1.
								offset = 0;

								// But internally to the sack panel they are still zero based
								// so we need to account for that.
								if (this.SackType == SackType.Sack)
									offset2 = 1;
							}

							for (int i = 0; i < this.MaxSacks; ++i)
							{
								// The sacks do not need to list sack#0 since it is the Main player panel
								// and it will be accounted for later.
								if (this.SackType == SackType.Sack && i == 0)
									continue;

								if (i != this.CurrentSack + offset2)
								{
									int val = i + offset;
									choices.Add(string.Format(CultureInfo.CurrentCulture, Resources.GlobalMenuBag, val));
								}
							}
						}

						var autoMoveChoices = (
							from location in this.DragInfo.AllAutoMoveLocations
							where location != this.AutoMoveLocation
							select location)
							.Distinct();

						foreach (var choice in autoMoveChoices)
						{
							string location = this.GetStringFromAutoMove(choice);
							if (!string.IsNullOrEmpty(location))
								choices.Add(location);
						}

						ToolStripItem[] moveChoices = new ToolStripItem[choices.Count];
						EventHandler moveCallback = new EventHandler(this.MoveItemClicked);

						for (int j = 0; j < choices.Count; ++j)
						{
							moveChoices[j] = new ToolStripMenuItem(choices[j], null, moveCallback, choices[j]);
							moveChoices[j].BackColor = this.contextMenu.BackColor;
							moveChoices[j].Font = this.contextMenu.Font;
							moveChoices[j].ForeColor = this.contextMenu.ForeColor;
						}

						ToolStripMenuItem moveSubMenu = new ToolStripMenuItem(Resources.SackPanelMenuMoveTo, null, moveChoices);
						moveSubMenu.BackColor = this.contextMenu.BackColor;
						moveSubMenu.Font = this.contextMenu.Font;
						moveSubMenu.ForeColor = this.contextMenu.ForeColor;
						moveSubMenu.DisplayStyle = ToolStripItemDisplayStyle.Text;

						this.contextMenu.Items.Add(moveSubMenu);
					}

					if ((focusedItem != null && (this.selectedItems == null || singleSelectionFocused)) && !isEquipmentReadOnly)
					{
						// Item Editing options
						if (Config.Settings.Default.AllowItemEdit)
						{
							this.contextMenu.Items.Add(Resources.SackPanelMenuSeed);

							// Add option to complete a charm or relic if
							// not already completed.
							if (focusedItem.IsRelic && !focusedItem.IsRelicComplete)
							{
								if (focusedItem.IsCharm)
									this.contextMenu.Items.Add(Resources.SackPanelMenuCharm);
								else
									this.contextMenu.Items.Add(Resources.SackPanelMenuRelic);
							}

							// Add option to craft an artifact from formulae.
							if (focusedItem.IsFormulae)
								this.contextMenu.Items.Add(Resources.SackPanelMenuFormulae);

							// If the item is a completed relic/charm/artifact or contains such then
							// add a menu of possible completion bonuses to choose from.
							if ((focusedItem.HasRelicSlot1 && focusedItem.RelicBonusInfo != null) ||
								(focusedItem.IsRelic && focusedItem.IsRelicComplete) ||
								(focusedItem.IsArtifact))
							{
								LootTableCollection table = ItemProvider.BonusTable(focusedItem);
								if (table != null && table.Length > 0)
								{
									int numItems = table.Length;
									ToolStripItem[] choices = new ToolStripItem[numItems];
									EventHandler callback = new EventHandler(this.ChangeBonusItemClicked);
									int i = 0;
									foreach (KeyValuePair<string, float> e1 in table)
									{
										string fileBase = Path.GetFileNameWithoutExtension(e1.Key);
										float weight = e1.Value;
										string txt = string.Format(CultureInfo.CurrentCulture, "{0} ({1:p2})", fileBase, weight);
										choices[i] = new ToolStripMenuItem(txt, null, callback, e1.Key);
										choices[i].BackColor = this.contextMenu.BackColor;
										choices[i].Font = this.contextMenu.Font;
										choices[i].ForeColor = this.contextMenu.ForeColor;
										choices[i].ToolTipText = e1.Key;

										// make the currently selected bonus bold
										if (TQData.NormalizeRecordPath(e1.Key).Equals(TQData.NormalizeRecordPath(focusedItem.RelicBonusId)))
											choices[i].Font = new Font(choices[i].Font, FontStyle.Bold);

										++i;
									}

									// sort the array of choices alphabetically
									Array.Sort(choices, CompareMenuChoices);

									ToolStripMenuItem subMenu = new ToolStripMenuItem(Resources.SackPanelMenuBonus, null, choices);
									subMenu.BackColor = this.contextMenu.BackColor;
									subMenu.Font = this.contextMenu.Font;
									subMenu.ForeColor = this.contextMenu.ForeColor;
									subMenu.DisplayStyle = ToolStripItemDisplayStyle.Text;

									this.contextMenu.Items.Add(subMenu);
								}
							}

							// If the item is a set item, then add a menu to create the rest of the set
							string[] setItems = ItemProvider.GetSetItems(focusedItem, false);
							if (setItems != null && setItems.Length > 1)
							{
								ToolStripItem[] choices = new ToolStripItem[setItems.Length - 1];
								EventHandler callback = new EventHandler(this.NewSetItemClicked);
								int i = 0;
								foreach (string s in setItems)
								{
									// do not put the current item in the menu
									if (TQData.NormalizeRecordPath(focusedItem.BaseItemId).Equals(TQData.NormalizeRecordPath(s)))
										continue;

									// Get the name of the item
									Info info = Database.GetInfo(s);
									string name = Path.GetFileNameWithoutExtension(s);
									if (info == null)
										continue;
									
									name = this.TranslationService.TranslateXTag(info.DescriptionTag);								
									choices[i] = new ToolStripMenuItem(name, null, callback, s);
									choices[i].BackColor = this.contextMenu.BackColor;
									choices[i].Font = this.contextMenu.Font;
									choices[i].ForeColor = this.contextMenu.ForeColor;
									choices[i].ToolTipText = s;

									++i;
								}

								// sort the array of choices alphabetically
								Array.Sort(choices, CompareMenuChoices);

								ToolStripMenuItem subMenu = new ToolStripMenuItem(Resources.SackPanelMenuSet, null, choices);
								subMenu.BackColor = this.contextMenu.BackColor;
								subMenu.Font = this.contextMenu.Font;
								subMenu.ForeColor = this.contextMenu.ForeColor;
								subMenu.DisplayStyle = ToolStripItemDisplayStyle.Text;

								this.contextMenu.Items.Add(subMenu);
							}
						}
					}

					if (focusedItem != null && (this.selectedItems == null || singleSelectionFocused))
					{
						this.contextMenu.Items.Add("-");
						this.contextMenu.Items.Add(Resources.SackPanelMenuProperties);
					}

					if (this.selectedItems != null && !isEquipmentReadOnly)
						this.contextMenu.Items.Add(Resources.SackPanelMenuClear);

					if ((focusedItem != null || this.selectedItems != null) && this.contextMenu.Items.Count > 0)
						this.contextMenu.Show(this, new Point(e.X, e.Y));
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
			return new Rectangle(screenLocation.X, screenLocation.Y, item.Size.Width* UIService.ItemUnitSize, item.Size.Height * UIService.ItemUnitSize);
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
			=>new Rectangle(Math.Min(startPosition.X, currentPosition.X), Math.Min(startPosition.Y, currentPosition.Y), Math.Abs(startPosition.X - currentPosition.X), Math.Abs(startPosition.Y - currentPosition.Y));
				
		/// <summary>
		/// Paint callback
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">PaintEventArgs data</param>
		protected void PaintCallback(object sender, PaintEventArgs e)
		{
			// Assign the clipping to the rectangle
			Region oldClip = e.Graphics.Clip;
			e.Graphics.Clip = new Region(e.ClipRectangle);

			try
			{
				if (this.Sack == null)
					// Draw the medallion if the sack is not created
					e.Graphics.DrawImage(Resources.tqmedallion, 0, 0, this.Width, this.Height);
				else
				{
					// Draw the border.
					this.DrawBorder(e.Graphics);

					// shade the area under all the items
					this.PaintAreaUnderItem(e);

					// Shade the area under the drag item
					this.PaintAreaUnderDragItem(e);

					// Draw the grid.
					this.DrawGrid(e.Graphics);

					// Draw the items.
					this.PaintItems(e);

					Point cursorPosition = this.PointToClient(Cursor.Position);
					if (this.DragInfo.IsActive && this.ClientRectangle.Contains(cursorPosition))
						this.RedrawDragItem(e.Graphics, new Point(cursorPosition.X - this.DragInfo.MouseOffset.X, cursorPosition.Y - this.DragInfo.MouseOffset.Y));

					if (mouseDraw)
						e.Graphics.DrawRectangle(Pens.White, GetMouseDragRectangle());
				}
			}
			finally
			{
				e.Graphics.Clip = oldClip;
			}
		}

		/// <summary>
		/// Draws the background under a dragged item during a Paint call.
		/// </summary>
		/// <param name="e">PaintEventArgs data</param>
		protected virtual void PaintAreaUnderDragItem(PaintEventArgs e)
		{
			if (!this.DragInfo.IsActive || this.CellsUnderDragItem.Size.IsEmpty)
				return;

			Color highlightColor =  this.HighlightInvalidItemColor;

			if ((this.ItemsUnderDragItem == null || this.ItemsUnderDragItem.Count <= 1) && this.IsItemValidForPlacement(this.DragInfo.Item))
				highlightColor = this.HighlightValidItemColor;

			Point topLeft = this.CellTopLeft(this.CellsUnderDragItem.Location);
			Point bottomRight = this.CellBottomRight(new Point(this.CellsUnderDragItem.Right - 1, this.CellsUnderDragItem.Bottom - 1));

			using (SolidBrush brush = new SolidBrush(highlightColor))
			{
				// Draw the area
				e.Graphics.FillRectangle(brush, topLeft.X, topLeft.Y, bottomRight.X - topLeft.X + 1, bottomRight.Y - topLeft.Y + 1);
			}

		}

		/// <summary>
		/// Draws the items in the panel during a Paint call.
		/// </summary>
		/// <param name="e">PaintEventArgs data</param>
		protected virtual void PaintItems(PaintEventArgs e)
		{
			// Now draw all the sack items
			foreach (Item item in this.Sack)
			{
				// Skip over empty and dragged items.
				if (item != this.DragInfo.Item && !string.IsNullOrEmpty(item.BaseItemId))
					this.DrawItem(e.Graphics, item);
			}
		}

		/// <summary>
		/// Gets the item background shading color based on the item quality.
		/// </summary>
		/// <param name="item">Item that needs the color applied.</param>
		/// <returns>Color for the item.  Returns base color if specific color is not found.</returns>
		protected virtual Color GetItemBackgroundColor(Item item)
			=> this.HasItemBackgroundColor(item) ? item.ItemStyle.Color() : this.DefaultItemBackgroundColor;
		

		/// <summary>
		/// Indicates whether the passed item meets the item requirementt for equipping
		/// </summary>
		/// <param name="item">Item to check</param>
		/// <returns>True if item is able to be equipped</returns>
		protected virtual bool CanBeEquipped(Item item)
		{
			var reqs = this.ItemProvider.GetFriendlyNames(item, FriendlyNamesExtraScopes.Requirements).RequirementVariables;
			var currPlayer = this.userContext.CurrentPlayer;
			if (currPlayer != null && reqs != null && reqs.Any() && !currPlayer.IsPlayerMeetRequierements(reqs))
				return false;

			return true;
		}

		/// <summary>
		/// Indicates whether the item has a specific background color based on the ItemStyle.
		/// </summary>
		/// <param name="item">Item that needs needs a background color</param>
		/// <returns>True if the Item has a specific background color</returns>
		protected virtual bool HasItemBackgroundColor(Item item)
			=> this.ItemStyleBackGroundColorEnable.Contains(item.ItemStyle);

		/// <summary>
		/// Draws the background of the items in the panel during a Paint call.
		/// </summary>
		/// <param name="e">PaintEventArgs data</param>
		protected virtual void PaintAreaUnderItem(PaintEventArgs e)
		{
			Item focusedItem = this.FindItem(this.LastCellWithFocus);
			foreach (Item item in this.Sack)
			{
				// Do not draw the item being dragged.
				if (item == this.DragInfo.Item)
					continue;

				// Figure out the background brush to use.
				bool showAccent = true;
				int alpha = this.UserAlpha;
				Color backgroundColor = this.GetItemBackgroundColor(item);

				// If we are showing the cannot equip background then 
				// change to invalid color and adjust the alpha.
				if (Config.Settings.Default.EnableCharacterRequierementBGColor && !this.CanBeEquipped(item))
				{
					backgroundColor = this.HighlightInvalidItemColor;

					// Un-equippable items do not show the accent.
					showAccent = false;

					// Make the background stand out since we are not showing the accent.
					alpha = AdjustAlpha(alpha);
				}

				// Check if the item is selected and use adjust the alpha
				if (this.IsItemSelected(item))
					alpha = AdjustAlpha(alpha);

				// See if this item is under the drag item
				if (this.DragInfo.IsActive && this.ItemsUnderDragItem != null && this.ItemsUnderDragItem.Contains(item))
				{
					// Use highlight color if it is the only item under the drag point, else use invalid
					backgroundColor = this.ItemsUnderDragItem.Count > 1 ? this.HighlightInvalidItemColor : this.HighlightValidItemColor;
					alpha = AdjustAlpha(alpha);
				}

				// See if this is the focused item and adjust the alpha again.
				if (!this.DragInfo.IsActive && item == focusedItem)
					alpha = AdjustAlpha(alpha);

				// Now do the shading
				this.ShadeAreaUnderItem(e.Graphics, item, backgroundColor, alpha);

				// Adjust the alpha and draw the accent.
				if (showAccent && HasItemBackgroundColor(item))
					this.DrawItemAccent(e.Graphics, item, backgroundColor, AdjustAlpha(alpha));
			}
		}

		/// <summary>
		/// Redraws the grid for a specified area.
		/// Used to refresh the screen display
		/// </summary>
		/// <param name="graphics">graphics instance</param>
		/// <param name="redrawRectangle">Rectangle containing the screen coordinated which will be redrawn</param>
		protected void RedrawGrid(Graphics graphics, Rectangle redrawRectangle)
		{
			if (this.DisableGrid || graphics == null)
				return;

			for (int x = redrawRectangle.X; x < redrawRectangle.Right; x += UIService.ItemUnitSize)
				graphics.DrawLine(this.gridPen, new Point(x, redrawRectangle.Y), new Point(x, redrawRectangle.Bottom - 1));

			for (int y = redrawRectangle.Y; y < redrawRectangle.Bottom; y += UIService.ItemUnitSize)
				graphics.DrawLine(this.gridPen, new Point(redrawRectangle.X, y), new Point(redrawRectangle.Right - 1, y));
		}

		/// <summary>
		/// Draws an item on the screen at the specified coordinates without background shading.
		/// </summary>
		/// <param name="graphics">graphics instance</param>
		/// <param name="item">item we are drawing</param>
		protected virtual void DrawItem(Graphics graphics, Item item)
			=> this.DrawItem(graphics, item, this.CellTopLeft(item.Location));

		/// <summary>
		/// Draws an item on the screen at the specified coordinates without background shading.
		/// </summary>
		/// <param name="graphics">graphics instance</param>
		/// <param name="item">item we are drawing</param>
		/// <param name="screenLocation">Point containing the screen coordinates where the item will be drawn</param>
		protected void DrawItem(Graphics graphics, Item item, Point screenLocation)
		{
			// Color matrix for drawing the image as-is
			System.Drawing.Imaging.ColorMatrix colorMatrix = new System.Drawing.Imaging.ColorMatrix()
			{
				Matrix00 = 1.00f, // Red
				Matrix11 = 1.00f, // Green
				Matrix22 = 1.00f, // Blue
				Matrix33 = 1.00f, // alpha
				Matrix44 = 1.00f  // w
			};
			
			System.Drawing.Imaging.ImageAttributes imgAttr = new System.Drawing.Imaging.ImageAttributes();
			imgAttr.SetColorMatrix(colorMatrix);

			this.DrawItem(graphics, item, screenLocation, imgAttr);
		}

		/// <summary>
		/// Draws an item on the screen at the specified coordinates without background shading but with specific ImageAttributes.
		/// </summary>
		/// <param name="graphics">graphics instance</param>
		/// <param name="item">item we are drawing</param>
		/// <param name="screenLocation">Point containing the screen coordinates where the item will be drawn</param>
		/// <param name="imageAttributes">ImageAttributes used for drawing the image.</param>
		protected void DrawItem(Graphics graphics, Item item, Point screenLocation, System.Drawing.Imaging.ImageAttributes imageAttributes)
		{
			if (item == null || graphics == null || this.UIService.GetBitmap(item) == null)
				return;

			var ibmp = this.UIService.GetBitmap(item);

			Rectangle itemRect = new Rectangle(
				screenLocation.X
				, screenLocation.Y
				, Convert.ToInt32(ibmp.Width * UIService.Scale)
				, Convert.ToInt32(ibmp.Height * UIService.Scale)
			);

			graphics.DrawImage(ibmp, itemRect, 0, 0, ibmp.Width, ibmp.Height, GraphicsUnit.Pixel, imageAttributes);

			// Add the relic overlay if this item has a relic in it.
			if (item.HasRelicSlot1)
			{
				Bitmap relicOverlay = UIService.LoadRelicOverlayBitmap();
				if (relicOverlay != null)
				{
					// draw it in the bottom-right most cell of this item
					int x2 = screenLocation.X + ((item.Width - 1) * UIService.ItemUnitSize);
					int y2 = screenLocation.Y + ((item.Height - 1) * UIService.ItemUnitSize);

					Rectangle overlayRect = new Rectangle(x2, y2
						, Convert.ToInt32(relicOverlay.Width * UIService.Scale)
						, Convert.ToInt32(relicOverlay.Height * UIService.Scale)
					);

					graphics.DrawImage(relicOverlay, overlayRect, 0, 0, relicOverlay.Width, relicOverlay.Height, GraphicsUnit.Pixel, imageAttributes);
				}
			}

			// Add any number we need to add.
			// Only show the number when there is more than 1 in the stack
			// Relics and charms still show the number.
			if (item.HasNumber && !(item.DoesStack && item.Number == 1))
			{
				string numberString = item.Number.ToString(CultureInfo.CurrentCulture);

				// Draw the number along the bottom of the cell
				Point loc = new Point(screenLocation.X, screenLocation.Y + (item.Height * UIService.ItemUnitSize) - 1);
				float height = (float)this.numberFont.Height * UIService.Scale;
				float width = (float)item.Width * UIService.ItemUnitSize;
				float fy = (float)(loc.Y - (0.75F * this.numberFont.Height) - 1.0F);
				float fx = (float)loc.X;

				RectangleF rect = new RectangleF(fx, fy, width, height);
				graphics.DrawString(numberString, this.numberFont, this.numberBrush, rect, this.numberFormat);
			}
		}

		/// <summary>
		/// Draws an accent on the item graphic
		/// </summary>
		/// <param name="graphics">graphics instance</param>
		/// <param name="item">item we are adding the accent to</param>
		/// <param name="accentColor">Color that the accent will be painted</param>
		/// <param name="alpha">alpha value for the color</param>
		protected virtual void DrawItemAccent(Graphics graphics, Item item, Color accentColor, int alpha)
		{
			if (item == null)
				return;

			Point screenLocation = this.CellTopLeft(item.Location);
			this.DrawItemAccent(graphics, new Rectangle(screenLocation, new Size(item.Width * UIService.ItemUnitSize, item.Height * UIService.ItemUnitSize)), accentColor, alpha);
		}

		/// <summary>
		/// Draws an accent on the item graphic
		/// </summary>
		/// <param name="graphics">graphics instance</param>
		/// <param name="itemRectangle">cell rectangle for the item</param>
		/// <param name="accentColor">Color that the accent will be painted</param>
		/// <param name="alpha">alpha value for the color</param>
		protected virtual void DrawItemAccent(Graphics graphics, Rectangle itemRectangle, Color accentColor, int alpha)
		{
			if (graphics == null)
				return;

			System.Drawing.Imaging.ColorMatrix colorMatrix = new System.Drawing.Imaging.ColorMatrix()
			{
				Matrix00 = accentColor.R / 255.0f, // Red
				Matrix11 = accentColor.G / 255.0f, // Green
				Matrix22 = accentColor.B / 255.0f, // Blue
				Matrix33 = alpha / 255.0f,         // alpha
				Matrix44 = 1.00f                   // w
			};

			System.Drawing.Imaging.ImageAttributes imgAttr = new System.Drawing.Imaging.ImageAttributes();
			imgAttr.SetColorMatrix(colorMatrix);

			graphics.DrawImage(Resources.ItemAccent, itemRectangle, 0, 0, Resources.ItemAccent.Width, Resources.ItemAccent.Height, GraphicsUnit.Pixel, imgAttr);
		}

		/// <summary>
		/// Shades the background of an item with alpha blending.
		/// </summary>
		/// <param name="graphics">graphics instance</param>
		/// <param name="item">item we are shading</param>
		/// <param name="backgroundColor">Color that the background will be painted</param>
		/// <param name="alpha">alpha value for the color</param>
		protected virtual void ShadeAreaUnderItem(Graphics graphics, Item item, Color backgroundColor, int alpha)
		{
			if (item == null)
				return;

			Point screenLocation = this.CellTopLeft(item.Location);
			this.ShadeAreaUnderItem(graphics, new Rectangle(screenLocation, new Size(item.Width * UIService.ItemUnitSize, item.Height * UIService.ItemUnitSize)), backgroundColor, alpha);
		}

		/// <summary>
		/// Shades the background of an item with alpha blending.
		/// </summary>
		/// <param name="graphics">graphics instance</param>
		/// <param name="backgroundRectangle">cell rectangle which needs to be drawn</param>
		/// <param name="backgroundColor">Color that the background will be painted</param>
		/// <param name="alpha">alpha value for the color</param>
		protected virtual void ShadeAreaUnderItem(Graphics graphics, Rectangle backgroundRectangle, Color backgroundColor, int alpha)
		{
			if (graphics == null)
				return;

			using (SolidBrush brush = new SolidBrush(Color.FromArgb(alpha, backgroundColor)))
			{
				graphics.FillRectangle(brush, backgroundRectangle);
			}
		}

		#endregion SackPanel Protected Methods

		#region SackPanel Private Methods

		/// <summary>
		/// Compares context menu choices.  Used for sorting the list.
		/// </summary>
		/// <param name="item1">First item to compare</param>
		/// <param name="item2">Second item to compare</param>
		/// <returns>-1 0 1 for item1 less than item2, equal, item1 greather than item2 respectively</returns>
		private static int CompareMenuChoices(ToolStripItem item1, ToolStripItem item2)
		{
			int ans = string.Compare(item1.Text, item2.Text, StringComparison.OrdinalIgnoreCase);
			if (ans == 0)
				ans = string.Compare(item1.Name, item2.Name, StringComparison.OrdinalIgnoreCase);

			return ans;
		}

		/// <summary>
		/// Deletes the highlighted item
		/// </summary>
		/// <param name="focusedItem">Item containing the focus (which will get deleted)</param>
		/// <param name="suppressMessage">Determines whether we will show a delete confirmation message.</param>
		private void DeleteItem(Item focusedItem, bool suppressMessage)
		{
			if (focusedItem != null)
			{
				if (suppressMessage || Config.Settings.Default.SuppressWarnings || MessageBox.Show(
					Resources.SackPanelDeleteMsg,
					Resources.SackPanelDelete,
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Warning,
					MessageBoxDefaultButton.Button1,
					RightToLeftOptions) == DialogResult.Yes)
				{
					// remove item
					this.Sack.RemoveItem(focusedItem);
					this.Refresh();
					BagButtonTooltip.InvalidateCache(this.Sack);
				}
			}
		}

		/// <summary>
		/// Adds the highlighted item to the selected items list.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">MouseEventArgs data</param>
		private void AddFocusedItemToSelectedItems(object sender, MouseEventArgs e)
		{
			if (this.DisableMultipleSelection)
				return;

			this.MouseMoveCallback(sender, e); // process the mouse moving to this location...just in case

			Item focusedItem = this.FindItem(this.LastCellWithFocus);
			if (focusedItem != null)
			{
				// Allocate the List if not already done.
				if (this.selectedItems == null)
					this.selectedItems = new List<Item>();

				if (!this.IsItemSelected(focusedItem))
				{
					// Check to see if the item is already selected
					this.selectedItems.Add(focusedItem);
					this.selectedItemsChanged = true;
				}
				else
					this.RemoveSelectedItem(focusedItem);

				this.Invalidate(this.GetItemScreenRectangle(focusedItem));
			}

			this.OnItemSelected(this, new SackPanelEventArgs(null, null));
			this.MouseMoveCallback(sender, e); // process mouse move again to apply the graphical effects of dragging an item.
		}

		/// <summary>
		/// Adds the items inside of the mouse drag to the selected items list.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">MouseEventArgs data</param>
		private void SelectItemsInMouseDraw(object sender, MouseEventArgs e)
		{
			if (!this.mouseDraw)
				return;

			// Allocate the List if not already done.
			if (this.selectedItems == null)
				this.selectedItems = new List<Item>();

			Collection<Item> itemsUnderDraw = FindAllItems(FindAllCells(GetMouseDragRectangle()));

			if (itemsUnderDraw != null)
			{
				foreach (Item item in itemsUnderDraw)
				{
					if (!selectedItems.Contains(item))
					{
						this.selectedItems.Add(item);
					}
				}

				this.OnItemSelected(this, new SackPanelEventArgs(null, null));
			}
		}

		/// <summary>
		/// Handler for the mouse pointer entering the sack panel.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void MouseEnterCallback(object sender, EventArgs e)
			=> this.Select();

		/// <summary>
		/// Handler for the mouse pointer leaving the sack panel.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
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
					MessageBox.Show(Log.FormatException(exception), Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, RightToLeftOptions);
				}
			}
		}

		/// <summary>
		/// Gets the localized string for an AutoMoveLocation.
		/// </summary>
		/// <param name="autoMoveLocation">AutoMoveLocation that is to be translated</param>
		/// <returns>Localized String name of the AutoMoveLocation</returns>
		private string GetStringFromAutoMove(AutoMoveLocation autoMoveLocation)
		{
			if (autoMoveLocation == AutoMoveLocation.SecondaryVault && this.SecondaryVaultShown)
				return Resources.SackPanelMenuVault2;

			if (autoMoveLocation == AutoMoveLocation.Player && !this.SecondaryVaultShown)
				return Resources.SackPanelMenuPlayer;

			if (autoMoveLocation == AutoMoveLocation.Stash && !this.SecondaryVaultShown)
				return Resources.SackPanelMenuStash;

			if (autoMoveLocation == AutoMoveLocation.Vault)
				return Resources.SackPanelMenuVault;

			if (autoMoveLocation == AutoMoveLocation.Trash)
				return Resources.SackPanelMenuTrash;

			return string.Empty;
		}

		/// <summary>
		/// Handler for selecting Move Item from the context menu
		/// Auto moves an item to another sack or panel.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void MoveItemClicked(object sender, EventArgs e)
		{
			if (!this.DragInfo.IsActive)
			{
				Item focusedItem;
				AutoMoveLocation automoveDestination = AutoMoveLocation.NotSet;

				// Find out what was selected
				ToolStripMenuItem toolStripItem = (ToolStripMenuItem)sender;
				if (toolStripItem != null)
				{
					// Now that the bag can have a different name
					// we cannot hard code it.
					int hashSign = Resources.GlobalMenuBag.IndexOf(Resources.GlobalMenuBagDelimiter, StringComparison.OrdinalIgnoreCase) + 1;
					if (hashSign == -1)
						return;

					if (toolStripItem.Name.StartsWith(Resources.GlobalMenuBag.Substring(0, hashSign), StringComparison.OrdinalIgnoreCase))
					{
						int offset = 1;

						// For the player panel we do not need an offset since the bags are already offset by 1.
						if (this.SackType == SackType.Player || this.SackType == SackType.Sack)
							offset = 0;

						automoveDestination = (AutoMoveLocation)(Convert.ToInt32(toolStripItem.Name.Substring(hashSign), CultureInfo.InvariantCulture) - offset);
					}
					else if (toolStripItem.Name == Resources.SackPanelMenuVault)
						automoveDestination = AutoMoveLocation.Vault;
					else if (toolStripItem.Name == Resources.SackPanelMenuPlayer)
						automoveDestination = AutoMoveLocation.Player;
					else if (toolStripItem.Name == Resources.SackPanelMenuTrash)
						automoveDestination = AutoMoveLocation.Trash;
					else if (toolStripItem.Name == Resources.SackPanelMenuVault2)
						automoveDestination = AutoMoveLocation.SecondaryVault;
					else if (toolStripItem.Name == Resources.SackPanelMenuStash)
						automoveDestination = AutoMoveLocation.Stash;
				}

				if (this.selectedItems != null)
				{
					// Moving selected items.
					var autoMoveQuery = from Item item in this.selectedItems
										where item != null
										orderby (((item.Height * 3) + item.Width) * 100) + item.ItemGroup descending
										select item;

					foreach (Item item in autoMoveQuery)
					{
						if (!this.DragInfo.IsActive)
						{
							// Check to make sure the last item got placed.
							this.DragInfo.Set(this, this.Sack, item, new Point(1, 1));
							this.DragInfo.AutoMove = automoveDestination;
							this.OnAutoMoveItem(this, new SackPanelEventArgs(null, null));
						}
					}

					this.ClearSelectedItems();
				}
				else
				{
					// Single item highlighted
					focusedItem = this.FindItem(this.contextMenuCellWithFocus);
					if (focusedItem != null)
					{
						this.DragInfo.Set(this, this.Sack, focusedItem, new Point(1, 1));
						this.DragInfo.AutoMove = automoveDestination;
						this.OnAutoMoveItem(this, new SackPanelEventArgs(null, null));
					}
				}

				ItemTooltip.HideTooltip();
				BagButtonTooltip.InvalidateCache(this.Sack);
			}
		}

		/// <summary>
		/// Handler for selecting New Set Item from the context menu
		/// Creates the selected item from the set and sets it as the drag item.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void NewSetItemClicked(object sender, EventArgs e)
		{
			Item focusedItem = this.FindItem(this.contextMenuCellWithFocus);
			if (focusedItem != null)
			{
				ToolStripMenuItem item = (ToolStripMenuItem)sender;
				if (item != null)
				{
					// Create the item
					Item newItem = focusedItem.MakeEmptyCopy(item.Name);
					ItemProvider.GetDBData(newItem);

					// Set DragInfo to focused item.
					this.DragInfo.Set(this, this.Sack, focusedItem, new Point(1, 1));

					// now drag the new item
					this.DragInfo.MarkModified(newItem);
					Refresh();

					ItemTooltip.HideTooltip();
					BagButtonTooltip.InvalidateCache(this.Sack);
				}
			}
		}

		/// <summary>
		/// Handler for selecting Change Bonus from the context menu
		/// Changes an item's completion bonus.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void ChangeBonusItemClicked(object sender, EventArgs e)
		{
			Item focusedItem = this.FindItem(this.contextMenuCellWithFocus);
			if (focusedItem != null)
			{
				ToolStripMenuItem item = (ToolStripMenuItem)sender;
				if (item != null)
				{
					string newBonus = item.Name;

					// See if the bonus is different
					if (!TQData.NormalizeRecordPath(newBonus).Equals(TQData.NormalizeRecordPath(focusedItem.RelicBonusId)))
					{
						// change the item
						focusedItem.RelicBonusId = newBonus;
						focusedItem.RelicBonusInfo = Database.GetInfo(newBonus);
						focusedItem.IsModified = true;

						// mark the sack as modified also
						this.Sack.IsModified = true;
						this.InvalidateItemCacheAll(focusedItem);
					}
				}
			}
		}

		/// <summary>
		/// Handler for clicking an item on the context menu
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">ToolStripItemClickedEventArgs data</param>
		private void ContextMenuItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			Item focusedItem = this.FindItem(this.contextMenuCellWithFocus);
			if (focusedItem != null || this.selectedItems != null)
			{
				string selectedMenuItem = e.ClickedItem.Text;
				if (selectedMenuItem == Resources.SackPanelMenuDelete)
				{
					if (this.selectedItems != null)
					{
						if (Config.Settings.Default.SuppressWarnings || MessageBox.Show(
							Resources.SackPanelDeleteMultiMsg,
							Resources.SackPanelDeleteMulti,
							MessageBoxButtons.YesNo,
							MessageBoxIcon.Warning,
							MessageBoxDefaultButton.Button1,
							RightToLeftOptions) == DialogResult.Yes)
						{
							foreach (Item sackSelectedItem in this.selectedItems)
								this.DeleteItem(sackSelectedItem, true);

							this.ClearSelection();
						}
					}
					else
					{
						this.DeleteItem(focusedItem, false);
					}
				}
				else if (selectedMenuItem == Resources.SackPanelMenuRemoveRelic)
				{
					if (Config.Settings.Default.SuppressWarnings || MessageBox.Show(
						Resources.SackPanelRemoveRelicMsg,
						Resources.SackPanelMenuRemoveRelic,
						MessageBoxButtons.YesNo,
						MessageBoxIcon.Question,
						MessageBoxDefaultButton.Button1,
						RightToLeftOptions) == DialogResult.Yes)
					{
						// Set DragInfo to focused item.
						this.DragInfo.Set(this, this.Sack, focusedItem, new Point(1, 1));

						// pull out the relic
						Item relic = ItemProvider.RemoveRelic(focusedItem);

						// Put relic in DragInfo
						this.DragInfo.MarkModified(relic);
						Refresh();

						this.InvalidateItemCacheItemTooltip(focusedItem);
					}
				}
				else if (selectedMenuItem == Resources.SackPanelMenuCopy)
				{
					// Set DragInfo to focused item.
					this.DragInfo.Set(this, this.Sack, focusedItem, new Point(1, 1));

					// copy the item
					Item newItem = focusedItem.Duplicate(false);

					// now drag it
					this.DragInfo.MarkModified(newItem);
					Refresh();
				}
				else if (selectedMenuItem == Resources.SackPanelMenuDuplicate)
				{
					// Set DragInfo to focused item.
					this.DragInfo.Set(this, this.Sack, focusedItem, new Point(1, 1));

					// copy the item
					Item newItem = focusedItem.Duplicate(true);

					// now drag it
					this.DragInfo.MarkModified(newItem);
					Refresh();
				}
				else if (selectedMenuItem == Resources.SackPanelMenuProperties)
				{
					var dlg = this.ServiceProvider.GetService<ItemProperties>();
					dlg.Item = focusedItem;
					dlg.ShowDialog();
				}
				else if (selectedMenuItem == Resources.SackPanelMenuSeed)
				{
					var dlg = this.ServiceProvider.GetService<ItemSeedDialog>();
					dlg.SelectedItem = focusedItem;
					int origSeed = focusedItem.Seed;
					dlg.ShowDialog();

					// See if the seed was changed
					if (focusedItem.Seed != origSeed)
					{
						// Tell the sack that it has been modified
						this.Sack.IsModified = true;
						this.InvalidateItemCacheItemTooltip(focusedItem);
					}
				}
				else if (selectedMenuItem == Resources.SackPanelMenuCharm || selectedMenuItem == Resources.SackPanelMenuRelic)
				{
					focusedItem.Number = 10;

					float randPercent = (float)Item.GenerateSeed() / 0x7fff;
					LootTableCollection table = ItemProvider.BonusTable(focusedItem);

					if (table != null && table.Length > 0)
					{
						int i = table.Length;
						foreach (KeyValuePair<string, float> e1 in table)
						{
							i--;
							if (randPercent <= e1.Value || i == 0)
							{
								focusedItem.RelicBonusId = e1.Key;
								break;
							}
							else
								randPercent -= e1.Value;
						}
					}

					ItemProvider.GetDBData(focusedItem);

					focusedItem.IsModified = true;
					this.Sack.IsModified = true;
					InvalidateItemCacheItemTooltip(focusedItem);
					Refresh();
				}
				else if (selectedMenuItem == Resources.SackPanelMenuFormulae)
				{
					// Set DragInfo to focused item.
					this.DragInfo.Set(this, this.Sack, focusedItem, new Point(1, 1));

					// create artifact
					Item artifact = ItemProvider.CraftArtifact(focusedItem);

					// generate bonus
					float randPercent = (float)Item.GenerateSeed() / 0x7fff;
					LootTableCollection table = ItemProvider.BonusTable(artifact);

					if (table != null && table.Length > 0)
					{
						int i = table.Length;
						foreach (KeyValuePair<string, float> e1 in table)
						{
							i--;
							if (randPercent <= e1.Value || i == 0)
							{
								artifact.RelicBonusId = e1.Key;
								artifact.RelicBonusInfo = Database.GetInfo(e1.Key);
								artifact.IsModified = true;
								break;
							}
							else
								randPercent -= e1.Value;
						}
					}

					// Put artifact in DragInfo
					this.DragInfo.MarkModified(artifact);

					InvalidateItemCacheItemTooltip(focusedItem);

					Refresh();
				}
				else if (selectedMenuItem == Resources.SackPanelMenuSplit)
				{
					// Set DragInfo to focused item.
					this.DragInfo.Set(this, this.Sack, focusedItem, new Point(1, 1));

					// pull out all but one item
					Item newItem = focusedItem.PopAllButOneItem();

					// Put the item in DragInfo
					this.DragInfo.MarkModified(newItem);

					InvalidateItemCacheItemTooltip(focusedItem);

					Refresh();
				}
				else if (selectedMenuItem == Resources.SackPanelMenuClear)
				{
					this.ClearSelection();
					Refresh();
				}

				ItemTooltip.HideTooltip();
				BagButtonTooltip.InvalidateCache(this.Sack);

			}
		}

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
		/// Redraws the drag item.
		/// </summary>
		/// <param name="graphics">graphics instance</param>
		/// <param name="location">screen coordinates where we will be drawing</param>
		private void RedrawDragItem(Graphics graphics, Point location)
		{
			try
			{
				// Identify all the cells and items under the drag item.  We want the actual cells that we will
				// drop into so we add 1/2 a cell to the drag location so that we pick the cells closest
				// to the center of the item.
				Point topLeftCell = this.FindCell(new Point(location.X + UIService.HalfUnitSize, location.Y + UIService.HalfUnitSize));
				if (topLeftCell.X < 0 || topLeftCell.Y < 0)
				{
					// out of area
					this.CellsUnderDragItem = InvalidDragRectangle;
				}
				else
				{
					this.CellsUnderDragItem = new Rectangle(topLeftCell, this.DragInfo.Item.Size);

					// See if the area is big enough to hold the item (ie the item does not go off the panel)
					int right = this.CellsUnderDragItem.Right - 1;
					int bottom = this.CellsUnderDragItem.Bottom - 1;
					if (right >= this.SackSize.Width || bottom >= this.SackSize.Height)
					{
						// uh oh we do not fit!
						// reset the cells to "out of area"
						this.CellsUnderDragItem = InvalidDragRectangle;
					}
				}

				this.ItemsUnderDragItem.Clear();
				foreach (Item item in this.FindAllItems(this.CellsUnderDragItem))
					this.ItemsUnderDragItem.Add(item);

				this.LastDragLocation = location;
				this.ItemsUnderOldDragLocation.Clear();
				foreach (Item item in this.ItemsUnderDragItem)
					this.ItemsUnderOldDragLocation.Add(item);

				this.DrawItem(graphics, this.DragInfo.Item, this.LastDragLocation);
			}
			catch (NullReferenceException exception)
			{
				Log.ErrorException(exception);
				MessageBox.Show(Log.FormatException(exception), Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, RightToLeftOptions);
			}
		}

		/// <summary>
		/// Moves an item from one sack to another without the context menu.
		/// </summary>
		/// <param name="desintationSack">int containing the number of the destination sack.</param>
		private void QuickMoveSack(int sackNumber)
		{
			if (Sack == null)
				return;

			var isEquipmentReadOnly = (Config.Settings.Default.PlayerReadonly == true && SackType == SackType.Equipment);
			Item focusedItem = FindItem(LastCellWithFocus);

			if ((focusedItem == null && selectedItems == null) || isEquipmentReadOnly)
				return;

			if (MaxSacks > 1 && sackNumber < MaxSacks)
			{
				// Calculate offsets for the Player's sack panels.
				int offset = 1; // This is for the numerical display in the menu.
				int offset2 = 0; // This is for comparison of the current sack.

				if (SackType == SackType.Player || SackType == SackType.Sack)
				{
					// Since the player panel bag's are already starting with 1.
					offset = 0;

					// But internally to the sack panel they are still zero based
					// so we need to account for that.
					if (SackType == SackType.Sack)
						offset2 = 1;
				}

				if (sackNumber != CurrentSack + offset2)
				{
					QuickMovePanel((AutoMoveLocation)sackNumber);// + offset2);					
				}
			}
		}

		/// <summary>
		/// Moves an item from one panel to another without the context menu.
		/// </summary>
		/// <param name="location">Destination AutoMoveLocation</param>
		private void QuickMovePanel(AutoMoveLocation location)
		{
			if (Sack == null)
				return;

			var isEquipmentReadOnly = (Config.Settings.Default.PlayerReadonly == true && SackType == SackType.Equipment);
			Item focusedItem = FindItem(LastCellWithFocus);
			AutoMoveLocation destination = AutoMoveLocation.NotSet;

			if ((focusedItem == null && selectedItems == null) || isEquipmentReadOnly)
				return;

			if (location != AutoMoveLocation)
			{
				destination = location;

				if (SecondaryVaultShown && location == AutoMoveLocation.Player)
					destination = AutoMoveLocation.SecondaryVault;
			}
			 			
			if ((SecondaryVaultShown && (destination == AutoMoveLocation.Stash || destination == AutoMoveLocation.Player)) || destination == AutoMoveLocation.NotSet)
				return;

			if (this.selectedItems != null)
			{
				// Moving selected items.
				var autoMoveQuery = from Item item in selectedItems
									where item != null
									orderby (((item.Height * 3) + item.Width) * 100) + item.ItemGroup descending
									select item;

				foreach (Item item in autoMoveQuery)
				{
					if (!DragInfo.IsActive)
					{
						// Check to make sure the last item got placed.
						DragInfo.Set(this, Sack, item, new Point(1, 1));
						DragInfo.AutoMove = destination;
						OnAutoMoveItem(this, new SackPanelEventArgs(null, null));
					}
				}

				ClearSelectedItems();
			}
			else if (focusedItem != null)
			{
				// Single item highlighted
				DragInfo.Set(this, Sack, focusedItem, new Point(1, 1));
				DragInfo.AutoMove = destination;
				OnAutoMoveItem(this, new SackPanelEventArgs(null, null));
			}

			ItemTooltip.HideTooltip();
			BagButtonTooltip.InvalidateCache(Sack);
		}

		/// <summary>
		/// Draw the sack panel border
		/// </summary>
		/// <param name="graphics">graphics instance</param>
		private void DrawBorder(Graphics graphics)
		{
			if (this.DisableBorder || graphics == null)
				return;

			graphics.DrawRectangle(this.borderPen, 0, 0, this.Size.Width, this.Size.Height);
		}

		/// <summary>
		/// Draws the grid within the sack panel
		/// </summary>
		/// <param name="graphics">graphics instance</param>
		private void DrawGrid(Graphics graphics)
		{
			if (this.DisableGrid || graphics == null)
				return;

			for (int x = 1; x < this.SackSize.Width; ++x)
				graphics.DrawLine(this.gridPen, this.CellTopLeft(new Point(x, 0)), this.CellBottomLeft(new Point(x, this.SackSize.Height - 1)));

			for (int y = 1; y < this.SackSize.Height; ++y)
				graphics.DrawLine(this.gridPen, this.CellTopLeft(new Point(0, y)), this.CellTopRight(new Point(this.SackSize.Width - 1, y)));
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
				else if (e.KeyChar == 'c' && Config.Settings.Default.AllowItemCopy == true)
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
				else if (e.KeyChar == 'd' && Config.Settings.Default.AllowItemCopy == true)
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

		#endregion SackPanel Private Methods

		/// <summary>
		/// Class for rendering the context menu strip.
		/// </summary>
		protected class CustomProfessionalRenderer : ToolStripProfessionalRenderer
		{
			/// <summary>
			/// Handler for rendering the contect meny strip.
			/// </summary>
			/// <param name="e">ToolStripItemTextRenderEventArgs data</param>
			protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
			{
				if (e.Item.Selected)
					e.TextColor = Color.Black;
				else
					e.TextColor = Color.FromArgb(200, 200, 200);

				base.OnRenderItemText(e);
			}
		}
	}
}
