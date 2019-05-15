//-----------------------------------------------------------------------
// <copyright file="SackPanel.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI
{
	using Properties;
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Drawing;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Windows.Forms;
	using TQVaultData;

	/// <summary>
	/// Class for holding all of the UI functions of the sack panel.
	/// </summary>
	public class SackPanel : Panel
	{
		#region SackPanel Fields

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

		/// <summary>
		/// Holds the original location of the panel.
		/// Used for scaling.
		/// </summary>
		private Point originalLocation;

		#endregion SackPanel Fields

		/// <summary>
		/// Initializes a new instance of the SackPanel class.
		/// </summary>
		/// <param name="sackWidth">Width of the sack panel in cells</param>
		/// <param name="sackHeight">Height of the sack panel in cells</param>
		/// <param name="dragInfo">ItemDragInfo instance.</param>
		/// <param name="autoMoveLocation">AutoMoveLocation for this sack</param>
		public SackPanel(int sackWidth, int sackHeight, ItemDragInfo dragInfo, AutoMoveLocation autoMoveLocation)
		{
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

			this.CellHasItemBrush = new SolidBrush(Color.FromArgb(64, 60, 60));
			this.EmptyCellBrush = new SolidBrush(Color.FromArgb(46, 41, 31));
			this.HighlightUnselectedItemBrush = new SolidBrush(Color.FromArgb(26, 29, 157));  // Light Blue
			this.HighlightValidItemBrush = new SolidBrush(Color.FromArgb(23, 149, 15));       // Green
			this.HighlightInvalidItemBrush = new SolidBrush(Color.FromArgb(153, 28, 28));     // Red
			this.HighlightSelectedItemBrush = new SolidBrush(Color.FromArgb(50, 60, 229));    // Dark Blue

			this.gridPen = new Pen(Color.FromArgb(142, 140, 129));

			this.numberFont = new Font("Arial", 10.0F * Database.DB.Scale, GraphicsUnit.Pixel);
			this.numberBrush = new SolidBrush(Color.White);
			this.numberFormat = new StringFormat();
			this.numberFormat.Alignment = StringAlignment.Far; // right-justify

			this.Size = new Size(
				(Convert.ToInt32(this.borderPen.Width) * 2) + (Database.DB.ItemUnitSize * sackWidth),
				(Convert.ToInt32(this.borderPen.Width) * 2) + (Database.DB.ItemUnitSize * sackHeight));
			this.BackColor = ((SolidBrush)this.EmptyCellBrush).Color;
			this.Paint += new PaintEventHandler(this.PaintCallback);
			this.MouseMove += new MouseEventHandler(this.MouseMoveCallback);
			this.MouseLeave += new EventHandler(this.MouseLeaveCallback);
			this.MouseEnter += new EventHandler(this.MouseEnterCallback);
			this.MouseDown += new MouseEventHandler(this.MouseDownCallback);
			this.KeyDown += new KeyEventHandler(this.KeyDownCallback);
			this.KeyUp += new KeyEventHandler(this.KeyUpCallback);
			this.KeyPress += new KeyPressEventHandler(this.KeyPressCallback);

			// Context menu
			this.contextMenu = new ContextMenuStrip();
			this.contextMenu.BackColor = Color.FromArgb(46, 41, 31);
			this.contextMenu.DropShadowEnabled = true;
			this.contextMenu.Font = Program.GetFontAlbertusMT(9.0F * Database.DB.Scale);
			this.contextMenu.ForeColor = Color.FromArgb(200, 200, 200);
			this.contextMenu.Opacity = 0.80;
			this.contextMenu.ShowImageMargin = false;
			this.contextMenu.ItemClicked += new ToolStripItemClickedEventHandler(this.ContextMenuItemClicked);

			this.contextMenu.Renderer = new CustomProfessionalRenderer();

			// Da_FileServer: Enable double buffering to remove flickering.
			this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
		}

		#region SackPanel Properties

		/// <summary>
		/// Gets the width of the border pen.  Scaled by the DB scale.
		/// </summary>
		public static float BorderWidth
		{
			get
			{
				return Math.Max((4.0F * Database.DB.Scale), 1.0F);
			}
		}

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
		/// Gets or sets the background Brush for cells which contain an item.
		/// </summary>
		public Brush CellHasItemBrush { get; protected set; }

		/// <summary>
		/// Gets or sets the background Brush for cells which do not contain an item.
		/// </summary>
		public Brush EmptyCellBrush { get; protected set; }

		/// <summary>
		/// Gets or sets the background Brush for cells which are valid for an item drop.
		/// </summary>
		public Brush HighlightValidItemBrush { get; protected set; }

		/// <summary>
		/// Gets or sets the background Brush for cells which are NOT valid for an item drop.
		/// </summary>
		public Brush HighlightInvalidItemBrush { get; protected set; }

		/// <summary>
		/// Gets or sets the background Brush for items as you mouse over them.
		/// Is not used if the item have been multi-selected.
		/// </summary>
		public Brush HighlightUnselectedItemBrush { get; protected set; }

		/// <summary>
		/// Gets or sets the background Brush for items that have been multi-selected.
		/// </summary>
		public Brush HighlightSelectedItemBrush { get; protected set; }

		/// <summary>
		/// Gets a value indicating whether items have been selected
		/// </summary>
		public bool SelectionActive
		{
			get
			{
				return this.selectedItems != null && this.selectedItems.Count > 0;
			}
		}

		/// <summary>
		/// Gets or sets the sack instance
		/// </summary>
		public SackCollection Sack
		{
			get
			{
				return this.sack;
			}

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
				{
					return MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading;
				}

				return (MessageBoxOptions)0;
			}
		}

		/// <summary>
		/// Gets the Point (-1, -1) which is used to indicate an invalid location when dragging.
		/// </summary>
		protected static Point InvalidDragLocation
		{
			get
			{
				return new Point(-1, -1);
			}
		}

		/// <summary>
		/// Gets the Rectangle (-1, -1, 0, 0) which is used to indicate an invalid area when dragging.
		/// </summary>
		protected static Rectangle InvalidDragRectangle
		{
			get
			{
				return new Rectangle(InvalidDragLocation, Size.Empty);
			}
		}

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
		protected Collection<Item> ItemsUnderDragItem
		{
			get
			{
				return this.itemsUnderDragItem;
			}
		}

		/// <summary>
		/// Gets the collection of items under the previous drag location.
		/// </summary>
		/// <remarks>
		/// Used to redraw the areas underneath the drag item as the item moves with the mouse.
		/// </remarks>
		protected Collection<Item> ItemsUnderOldDragLocation
		{
			get
			{
				return this.itemsUnderOldDragLocation;
			}
		}

		/// <summary>
		/// Gets or sets the coordinates of the last cell which had focus.
		/// </summary>
		protected Point LastCellWithFocus { get; set; }

		#endregion SackPanel Properties

		#region SackPanel Public Methods

		/// <summary>
		/// Tooltip callback
		/// </summary>
		/// <param name="windowHandle">window handle of the caller</param>
		/// <returns>Tooltip string</returns>
		public string ToolTipCallback(int windowHandle)
		{
			// see if this is us
			if (this.Handle.ToInt32() == windowHandle)
			{
				// yep.
				/*if (this.m_getToolTip != null)
				{
					return this.m_getToolTip(this);
				}*/
			}

			return null;
		}

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
				(Convert.ToInt32(this.borderPen.Width) * 2) + (Database.DB.ItemUnitSize * sackWidth),
				(Convert.ToInt32(this.borderPen.Width) * 2) + (Database.DB.ItemUnitSize * sackHeight));
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
				Convert.ToInt32((float)location.X / Database.DB.Scale),
				Convert.ToInt32((float)location.Y / Database.DB.Scale));
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
								orderby (((item.Height * 3) + item.Width) * 100)  descending, item.ItemGroup descending, item.BaseItemId, item.IsRelicComplete descending

								select item;

			foreach (Item item in autoSortQuery)
			{
				tempSack.AddItem(item);
			}

			if (tempSack == null || tempSack.IsEmpty)
			{
				return;
			}

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
				{
					offset = tempItem.Width - 1;
				}
				else
				{
					offset = tempItem.Height - 1;
				}

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
							{
								// Skip over the item
								k += foundItems[0].Height - 1;
							}
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
							{
								// Skip over the item
								k += foundItems[0].Width - 1;
							}
						}
					}

					// Check to see if the item was placed and
					// move on to the next item.
					if (itemPlaced)
					{
						break;
					}
				}

				// We could not find a place for the item,
				// so we have a problem since they all should fit.
				if (!itemPlaced)
				{
					this.sortVertical = !this.sortVertical;
					this.Sack.EmptySack();
					foreach (Item item in backUpSack)
					{
						this.Sack.AddItem(item.Clone());
					}
					this.Sack.IsModified = false;
					return;
				}
			}

			// Redraw the sack.
			this.Invalidate();
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
						{
							// The slot is free.
							return new Point(j, k);
						}
					}
					else
					{
						k += foundItems[0].Height - 1;
					}
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
				{
					this.ClearSelection();
				}

				foreach (Item item in this.Sack)
				{
					this.SelectItem(item);
				}

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
			{
				return false;
			}

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
		{
			return this.sack.StashType != SackType.RelicVaultStash || itemToCheck.IsArtifact || itemToCheck.IsRelic || itemToCheck.IsFormulae || itemToCheck.IsCharm;
		}

		/// <summary>
		/// Cancels an item drag
		/// </summary>
		/// <param name="dragInfo">ItemDragInfo instance</param>
		public virtual void CancelDrag(ItemDragInfo dragInfo)
		{
			// if the drag sack is not visible then we really do not need to do anything
			if (this.Sack == dragInfo.Sack)
			{
				// All we need to do is redraw the item to restore it
				Graphics graphics = this.CreateGraphics();
				try
				{
					Brush backgroundBrush = this.CellHasItemBrush;

					if (this.IsItemSelected(dragInfo.Item))
					{
						backgroundBrush = this.HighlightSelectedItemBrush;
					}

					this.DrawItem(graphics, dragInfo.Item, backgroundBrush);
				}
				catch (NullReferenceException exception)
				{
					MessageBox.Show(
						exception.ToString(),
						Resources.GlobalError,
						MessageBoxButtons.OK,
						MessageBoxIcon.Warning,
						MessageBoxDefaultButton.Button1,
						RightToLeftOptions);
				}
			}
		}

		#endregion SackPanel Public Methods

		#region SackPanel Protected Methods

		/// <summary>
		/// Override of ScaleControl which supports scaling of the fonts and internal items.
		/// </summary>
		/// <param name="factor">SizeF for the scale factor</param>
		/// <param name="specified">BoundsSpecified value.</param>
		protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
		{
			this.borderPen.Width = Math.Max((4.0F * Database.DB.Scale), 1.0F);
			this.numberFont = new Font(this.numberFont.Name, 10.0F * Database.DB.Scale, GraphicsUnit.Pixel);
			this.contextMenu.Font = new Font(this.contextMenu.Font.Name, 9.0F * Database.DB.Scale);

			this.Size = new Size(
				(Convert.ToInt32(this.borderPen.Width) * 2) + (Database.DB.ItemUnitSize * this.SackSize.Width),
				(Convert.ToInt32(this.borderPen.Width) * 2) + (Database.DB.ItemUnitSize * this.SackSize.Height));

			this.Location = new Point(
				Convert.ToInt32((float)this.originalLocation.X * Database.DB.Scale),
				Convert.ToInt32((float)this.originalLocation.Y * Database.DB.Scale));
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
			{
				x = -1;
			}
			else
			{
				x = (location.X - (int)this.borderPen.Width) / Database.DB.ItemUnitSize;
			}

			if (x >= this.SackSize.Width)
			{
				x = -2;
			}

			int y;
			if (location.Y < this.borderPen.Width)
			{
				y = -1;
			}
			else
			{
				y = (location.Y - (int)this.borderPen.Width) / Database.DB.ItemUnitSize;
			}

			if (y >= this.SackSize.Height)
			{
				y = -2;
			}

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
			{
				return null;
			}

			// Find the item for this point
			foreach (Item item in this.Sack)
			{
				if (item == this.DragInfo.Item)
				{
					// hide the item being dragged
					continue;
				}

				// store the x and y values
				int x = item.PositionX;
				int y = item.PositionY;

				if (string.IsNullOrEmpty(item.BaseItemId))
				{
					// Skip over empty items
					continue;
				}

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
				Convert.ToInt32(this.borderPen.Width) + (location.X * Database.DB.ItemUnitSize),
				Convert.ToInt32(this.borderPen.Width) + (location.Y * Database.DB.ItemUnitSize));
		}

		/// <summary>
		/// Given the cell X and Y coordinates, get the screen coordinates for the top right corner of the cell
		/// </summary>
		/// <param name="location">cell coordinates</param>
		/// <returns>Point for the corresponding screen position of the top right corner of the cell.</returns>
		protected Point CellTopRight(Point location)
		{
			return new Point(
				Convert.ToInt32(this.borderPen.Width) + ((location.X + 1) * Database.DB.ItemUnitSize) - 1,
				Convert.ToInt32(this.borderPen.Width) + (location.Y * Database.DB.ItemUnitSize));
		}

		/// <summary>
		/// Given the cell X and Y coordinates, get the screen coordinates for the bottom left corner of the cell
		/// </summary>
		/// <param name="location">cell coordinates</param>
		/// <returns>Point for the corresponding screen position of the bottom left corner of the cell.</returns>
		protected Point CellBottomLeft(Point location)
		{
			return new Point(
				Convert.ToInt32(this.borderPen.Width) + (location.X * Database.DB.ItemUnitSize),
				Convert.ToInt32(this.borderPen.Width) + ((location.Y + 1) * Database.DB.ItemUnitSize) - 1);
		}

		/// <summary>
		/// Gets the bottom cell of an item.  Not scaled to screen coordinates.
		/// </summary>
		/// <param name="item">Item which we are checking</param>
		/// <returns>Y value of the bottom cell of the item.</returns>
		protected virtual int CellBottom(Item item)
		{
			return item.Height + item.Location.Y - 1;
		}

		/// <summary>
		/// Given the cell X and Y coordinates, get the screen coordinates for the bottom right corner of the cell
		/// </summary>
		/// <param name="location">cell coordinates</param>
		/// <returns>Point for the corresponding screen position of the bottom right corner of the cell.</returns>
		protected Point CellBottomRight(Point location)
		{
			return new Point(
				Convert.ToInt32(this.borderPen.Width) + ((location.X + 1) * Database.DB.ItemUnitSize) - 1,
				Convert.ToInt32(this.borderPen.Width) + ((location.Y + 1) * Database.DB.ItemUnitSize) - 1);
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
			{
				return false;
			}

			// Iterate through the list of selected items
			foreach (Item selectedItem in this.selectedItems)
			{
				if (item == selectedItem)
				{
					// We have a match
					return true;
				}
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
					{
						this.Invalidate();
					}

					this.ClearSelection();
				}

				// Send a message to clear selections in the other panels.
				this.OnClearAllItemsSelected(this, new SackPanelEventArgs(null, null));

				// pick up item.
				this.DragInfo.Set(this, this.Sack, focusedItem, this.GetMouseOffset(e.Location, focusedItem));

				// process mouse move again to apply the graphical effects of dragging an item.
				this.MouseMoveCallback(sender, e);
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
			{
				return Point.Empty;
			}

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
			{
				return;
			}

			// Allocate the List if needed.
			if (this.selectedItems == null)
			{
				this.selectedItems = new List<Item>();
			}

			// Check to see if the item is already selected
			if (!this.IsItemSelected(item))
			{
				// Need to add the item to the list
				this.selectedItems.Add(item);
				this.selectedItemsChanged = true;
			}
			else
			{
				// It's already there so we need to remove it.
				this.RemoveSelectedItem(item);
			}

			this.OnItemSelected(this, new SackPanelEventArgs(null, null));
		}

		/// <summary>
		/// Removes an item from the selected items list.
		/// </summary>
		/// <param name="item">Item that we are removing</param>
		protected void RemoveSelectedItem(Item item)
		{
			if (item == null || this.selectedItems == null)
			{
				return;
			}

			if (this.IsItemSelected(item))
			{
				// Check to see if the item is already selected
				// If the item is in the list then remove it.
				this.selectedItems.Remove(item);
				this.selectedItemsChanged = true;

				if (this.selectedItems.Count == 0)
				{
					this.ClearSelection();
				}
			}
		}

		/// <summary>
		/// Indicates whether the item selection has changed.
		/// </summary>
		/// <returns>true if the item selection has changed</returns>
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
			{
				return false;
			}
		}

		/// <summary>
		/// Clears the item selection array
		/// </summary>
		protected void ClearSelection()
		{
			if (this.selectedItems != null)
			{
				this.selectedItems = null;
			}
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
				{
					return;
				}

				// Yes we can drop it here!
				// First take the item that is under us
				Item itemUnderUs = null;
				if (this.ItemsUnderDragItem != null && this.ItemsUnderDragItem.Count == 1)
				{
					itemUnderUs = this.ItemsUnderDragItem[0];
				}

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
				this.DragInfo.MarkPlaced(-1);

				// If we are a stackable and we have a stackable under us and we are the same type of stackable
				// then just add to the stack instead of picking up the other stack
				if (dragItem.DoesStack && itemUnderUs != null && itemUnderUs.DoesStack && dragItem.BaseItemId.Equals(itemUnderUs.BaseItemId))
				{
					itemUnderUs.StackSize += dragItem.StackSize;

					// Added this so the tooltip would update with the correct number
					itemUnderUs.MarkModified();
					this.Sack.IsModified = true;

					// Get rid of ref to itemUnderUs so code below wont do anything with it.
					itemUnderUs = null;

					// we will just throw away the dragItem now.
				}
				else if (dragItem.IsRelic && itemUnderUs != null && itemUnderUs.IsRelic &&
					!itemUnderUs.IsRelicComplete && !dragItem.IsRelicComplete &&
					dragItem.BaseItemId.Equals(itemUnderUs.BaseItemId))
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
						LootTableCollection table = itemUnderUs.BonusTable;

						if (table != null && table.Length > 0)
						{
							int i = table.Length;
							foreach (KeyValuePair<string, float> e1 in table)
							{
								i--;
								if (randPercent <= e1.Value || i == 0)
								{
									itemUnderUs.RelicBonusId = TQData.NormalizeRecordPath(e1.Key);
									break;
								}
								else
								{
									randPercent -= e1.Value;
								}
							}
						}

						itemUnderUs.GetDBData();
					}

					itemUnderUs.MarkModified();

					// Just in case we have more relics than what we need to complete
					// We then adjust the one we are holding
					int adjustedNumber = itemUnderUs.Number - originalNumber;
					if (adjustedNumber != dragItem.Number)
					{
						dragItem.Number -= adjustedNumber;
						dragItem.MarkModified();

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
						itemUnderUs.Width * Database.DB.HalfUnitSize,
						itemUnderUs.Height * Database.DB.HalfUnitSize);
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

				// Repaint everything to clear up any graphical issues
				this.Refresh();

				// and now do a MouseMove() to properly draw the new drag item and/or focus
				this.MouseMoveCallback(sender, e);
			}
		}

		/// <summary>
		/// Handler for mouse button down click
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">MouseEventArgs data</param>
		protected virtual void MouseDownCallback(object sender, MouseEventArgs e)
		{
			if (this.Sack == null || (Settings.Default.PlayerReadonly == true && this.SackType == SackType.Equipment))
			{
				return;
			}

			if (e.Button == MouseButtons.Left)
			{
				if (!this.DragInfo.IsActive)
				{
					// Detect a CTRL-Click.
					if (this.controlKeyDown)
					{
						// Add the focused item to the selected items list.
						this.AddFocusedItemToSelectedItems(sender, e);
					}
					else
					{
						this.PickupItem(sender, e);
					}
				}
				else
				{
					// We are holding an item already so put it down.
					this.TryToPutdownItem(sender, e);
				}
			}
			else if (e.Button == MouseButtons.Right)
			{
				if (this.DragInfo.IsActive && this.DragInfo.CanBeCanceled)
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
					{
						singleSelectionFocused = focusedItem == (Item)this.selectedItems[0] && this.selectedItems.Count == 1;
					}

					if (focusedItem != null || this.selectedItems != null)
					{
						if (focusedItem != null)
						{
							this.contextMenuCellWithFocus = this.LastCellWithFocus;
						}

						this.contextMenu.Items.Clear();
						this.contextMenu.Items.Add(Resources.SackPanelMenuDelete);
						this.contextMenu.Items.Add("-");
					}

					if (focusedItem != null && (this.selectedItems == null || singleSelectionFocused))
					{
						if (focusedItem.HasRelic && Settings.Default.AllowItemEdit)
						{
							this.contextMenu.Items.Add(Resources.SackPanelMenuRemoveRelic);
						}

						if (focusedItem.DoesStack && focusedItem.Number > 1)
						{
							this.contextMenu.Items.Add(Resources.SackPanelMenuSplit);
						}

						if (Settings.Default.AllowItemCopy)
						{
							this.contextMenu.Items.Add(Resources.SackPanelMenuCopy);
							this.contextMenu.Items.Add(Resources.SackPanelMenuDuplicate);
						}
					}

					if (focusedItem != null || this.selectedItems != null)
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
								{
									offset2 = 1;
								}
							}

							for (int i = 0; i < this.MaxSacks; ++i)
							{
								// The sacks do not need to list sack#0 since it is the Main player panel
								// and it will be accounted for later.
								if (this.SackType == SackType.Sack && i == 0)
								{
									continue;
								}

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
							{
								choices.Add(location);
							}
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

					if (focusedItem != null && (this.selectedItems == null || singleSelectionFocused))
					{
						// Item Editing options
						if (Settings.Default.AllowItemEdit)
						{
							this.contextMenu.Items.Add(Resources.SackPanelMenuSeed);

							// Add option to complete a charm or relic if
							// not already completed.
							if (focusedItem.IsRelic && !focusedItem.IsRelicComplete)
							{
								if (focusedItem.IsCharm)
								{
									this.contextMenu.Items.Add(Resources.SackPanelMenuCharm);
								}
								else
								{
									this.contextMenu.Items.Add(Resources.SackPanelMenuRelic);
								}
							}

							// Add option to craft an artifact from formulae.
							if (focusedItem.IsFormulae)
							{
								this.contextMenu.Items.Add(Resources.SackPanelMenuFormulae);
							}

							// If the item is a completed relic/charm/artifact or contains such then
							// add a menu of possible completion bonuses to choose from.
							if ((focusedItem.HasRelic && focusedItem.RelicBonusInfo != null) ||
								(focusedItem.IsRelic && focusedItem.IsRelicComplete) ||
								(focusedItem.IsArtifact))
							{
								LootTableCollection table = focusedItem.BonusTable;
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
										{
											choices[i].Font = new Font(choices[i].Font, FontStyle.Bold);
										}

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
							string[] setItems = focusedItem.GetSetItems(false);
							if (setItems != null && setItems.Length > 1)
							{
								ToolStripItem[] choices = new ToolStripItem[setItems.Length - 1];
								EventHandler callback = new EventHandler(this.NewSetItemClicked);
								int i = 0;
								foreach (string s in setItems)
								{
									// do not put the current item in the menu
									if (TQData.NormalizeRecordPath(focusedItem.BaseItemId).Equals(TQData.NormalizeRecordPath(s)))
									{
										continue;
									}

									// Get the name of the item
									Info info = Database.DB.GetInfo(s);
									string name = Path.GetFileNameWithoutExtension(s);
									if (info != null)
									{
										string nameTag = info.DescriptionTag;
										name = Database.DB.GetFriendlyName(nameTag);
									}

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

						this.contextMenu.Items.Add("-");
						this.contextMenu.Items.Add(Resources.SackPanelMenuProperties);
					}

					if (this.selectedItems != null)
					{
						this.contextMenu.Items.Add(Resources.SackPanelMenuClear);
					}

					if (focusedItem != null || this.selectedItems != null)
					{
						this.contextMenu.Show(this, new Point(e.X, e.Y));
					}
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
					// We have moved to a different item
					Graphics g = this.CreateGraphics();
					Brush backgroundBrush;
					if (lastItem != null)
					{
						// We need to restore the highlighted item's background
						if (this.IsItemSelected(lastItem))
						{
							backgroundBrush = this.HighlightSelectedItemBrush;
						}
						else if (redrawSelection)
						{
							backgroundBrush = this.HighlightUnselectedItemBrush;
						}
						else
						{
							backgroundBrush = this.CellHasItemBrush;
						}

						this.DrawItem(g, lastItem, backgroundBrush);
					}

					if (cell != this.LastCellWithFocus)
					{
						this.LastCellWithFocus = cell;
					}

					if (newItem != lastItem)
					{
						if (newItem != null)
						{
							// Check if the item is selected and use a different background
							if (this.IsItemSelected(newItem))
							{
								backgroundBrush = this.HighlightSelectedItemBrush;
							}
							else
							{
								backgroundBrush = this.HighlightUnselectedItemBrush;
							}

							this.DrawItem(g, newItem, backgroundBrush);
						}

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
		/// Redraws what was under the last drag location.
		/// Used to restore the screen when the item is dragged around.
		/// </summary>
		/// <param name="graphics">graphics instance</param>
		protected virtual void RepaintLastDragLocation(Graphics graphics)
		{
			if (graphics == null)
			{
				return;
			}

			Rectangle dragRectangle = this.GetRepaintDragRect();

			// we know we need to wipe out the area under the old drag point
			this.ClearArea(graphics, dragRectangle);

			// Now just call the Paint method to redraw the grids and items.
			this.PaintCallback(this, new PaintEventArgs(graphics, dragRectangle));
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
			int width = Convert.ToInt32(this.DragInfo.Item.ItemBitmap.Width * Database.DB.Scale);
			int height = Convert.ToInt32(this.DragInfo.Item.ItemBitmap.Height * Database.DB.Scale);

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
			{
				return;
			}

			Point cell = this.FindCell(e.Location);

			if (!this.DragInfo.IsActive)
			{
				this.HighlightItemUnderMouse(cell);
			}
			else
			{
				this.Invalidate();
			}
		}

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
				{
					// Otherwise draw the medallion
					e.Graphics.DrawImage(Resources.tqmedallion, 0, 0, this.Width, this.Height);
				}
				else
				{
					// Draw the border.
					this.DrawBorder(e.Graphics);

					// shade the area under all the items
					this.PaintAreaUnderItem(e);

					// Shade the area under the drag item if there is <= 1 items under the drag item
					this.PaintAreaUnderDragItem(e);

					// Draw the grid.
					this.DrawGrid(e.Graphics);

					// Draw the items.
					this.PaintItems(e);

					Point cursorPosition = this.PointToClient(Cursor.Position);
					if (this.DragInfo.IsActive && this.ClientRectangle.Contains(cursorPosition))
					{
						this.RedrawDragItem(e.Graphics, new Point(cursorPosition.X - this.DragInfo.MouseOffset.X, cursorPosition.Y - this.DragInfo.MouseOffset.Y));
					}
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
			if (this.DragInfo.IsActive && !this.CellsUnderDragItem.Size.IsEmpty && (this.ItemsUnderDragItem == null ||
				this.ItemsUnderDragItem.Count <= 1))
			{
				Brush highlightBrush = this.IsItemValidForPlacement(this.DragInfo.Item) ? this.HighlightValidItemBrush : this.HighlightInvalidItemBrush;

				Point topLeft = this.CellTopLeft(this.CellsUnderDragItem.Location);
				Point bottomRight = this.CellBottomRight(new Point(this.CellsUnderDragItem.Right - 1, this.CellsUnderDragItem.Bottom - 1));

				// Draw the area
				e.Graphics.FillRectangle(highlightBrush, topLeft.X, topLeft.Y, bottomRight.X - topLeft.X + 1, bottomRight.Y - topLeft.Y + 1);
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
				{
					this.DrawItem(e.Graphics, item, new SolidBrush(Color.Transparent));
				}
			}
		}

		/// <summary>
		/// Draws the background of the items in the panel during a Paint call.
		/// </summary>
		/// <param name="e">PaintEventArgs data</param>
		protected virtual void PaintAreaUnderItem(PaintEventArgs e)
		{
			Item lastItem = this.FindItem(this.LastCellWithFocus);
			foreach (Item item in this.Sack)
			{
				// Do not draw the item being dragged.
				if (item != this.DragInfo.Item)
				{
					// Figure out the background brush to use.
					Brush backgroundBrush = this.CellHasItemBrush;

					// Check if the item is selected and use a different background
					if (this.IsItemSelected(item))
					{
						backgroundBrush = this.HighlightSelectedItemBrush;
					}

					if (this.DragInfo.IsActive)
					{
						// See if this item is under the drag item
						if (this.ItemsUnderDragItem != null && this.ItemsUnderDragItem.Contains(item))
						{
							// Use highlight color if it is the only item under the drag point, else use invalid
							if (this.ItemsUnderDragItem.Count > 1)
							{
								backgroundBrush = this.HighlightInvalidItemBrush;
							}
							else
							{
								backgroundBrush = this.HighlightUnselectedItemBrush;
							}
						}
						else
						{
							backgroundBrush = this.CellHasItemBrush;
						}
					}
					else if (item == lastItem)
					{
						backgroundBrush = this.HighlightUnselectedItemBrush;
					}

					// Now do the shading
					this.ShadeAreaUnderItem(e.Graphics, item, backgroundBrush);
				}
				else
				{
					// item is being dragged
				}
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
			{
				return;
			}

			for (int x = redrawRectangle.X; x < redrawRectangle.Right; x += Database.DB.ItemUnitSize)
			{
				graphics.DrawLine(this.gridPen, new Point(x, redrawRectangle.Y), new Point(x, redrawRectangle.Bottom - 1));
			}

			for (int y = redrawRectangle.Y; y < redrawRectangle.Bottom; y += Database.DB.ItemUnitSize)
			{
				graphics.DrawLine(this.gridPen, new Point(redrawRectangle.X, y), new Point(redrawRectangle.Right - 1, y));
			}
		}

		/// <summary>
		/// Draws an item on the screen at the specified coordinates without background shading.
		/// </summary>
		/// <param name="graphics">graphics instance</param>
		/// <param name="item">item we are drawing</param>
		/// <param name="screenLocation">Point containing the screen coordinates where the item will be drawn</param>
		protected void DrawItem(Graphics graphics, Item item, Point screenLocation)
		{
			// Set the color matrix so that the item is dimmed
			System.Drawing.Imaging.ColorMatrix colorMatrix = new System.Drawing.Imaging.ColorMatrix();
			colorMatrix.Matrix00 = 1.00f; // Red
			colorMatrix.Matrix11 = 1.00f; // Green
			colorMatrix.Matrix22 = 1.00f; // Blue
			colorMatrix.Matrix33 = 1.00f; // alpha
			colorMatrix.Matrix44 = 1.00f; // w

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
			if (item == null || graphics == null || item.ItemBitmap == null)
			{
				return;
			}

			Rectangle itemRect = new Rectangle(
				screenLocation.X,
				screenLocation.Y,
				Convert.ToInt32(item.ItemBitmap.Width * Database.DB.Scale),
				Convert.ToInt32(item.ItemBitmap.Height * Database.DB.Scale));

			graphics.DrawImage(item.ItemBitmap, itemRect, 0, 0, item.ItemBitmap.Width, item.ItemBitmap.Height, GraphicsUnit.Pixel, imageAttributes);

			// Add the relic overlay if this item has a relic in it.
			if (item.HasRelic)
			{
				Bitmap relicOverlay = Database.DB.LoadRelicOverlayBitmap();
				if (relicOverlay != null)
				{
					// draw it in the bottom-right most cell of this item
					int x2 = screenLocation.X + ((item.Width - 1) * Database.DB.ItemUnitSize);
					int y2 = screenLocation.Y + ((item.Height - 1) * Database.DB.ItemUnitSize);

					Rectangle overlayRect = new Rectangle(
						x2,
						y2,
						Convert.ToInt32(relicOverlay.Width * Database.DB.Scale),
						Convert.ToInt32(relicOverlay.Height * Database.DB.Scale));

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
				Point loc = new Point(screenLocation.X, screenLocation.Y + (item.Height * Database.DB.ItemUnitSize) - 1);
				float height = (float)this.numberFont.Height * Database.DB.Scale;
				float width = (float)item.Width * Database.DB.ItemUnitSize;
				float fy = (float)(loc.Y - (0.75F * this.numberFont.Height) - 1.0F);
				float fx = (float)loc.X;

				RectangleF rect = new RectangleF(fx, fy, width, height);
				graphics.DrawString(numberString, this.numberFont, this.numberBrush, rect, this.numberFormat);
			}
		}

		/// <summary>
		/// Shades the background of an item.
		/// </summary>
		/// <param name="graphics">graphics instance</param>
		/// <param name="item">item we are shading</param>
		/// <param name="backgroundBrush">brush we are using for painting the background</param>
		protected virtual void ShadeAreaUnderItem(Graphics graphics, Item item, Brush backgroundBrush)
		{
			if (item == null)
			{
				return;
			}

			Point screenLocation = this.CellTopLeft(item.Location);
			this.ShadeAreaUnderItem(graphics, new Rectangle(screenLocation, new Size(item.Width * Database.DB.ItemUnitSize, item.Height * Database.DB.ItemUnitSize)), backgroundBrush);
		}

		/// <summary>
		/// Shades the background of an item.
		/// </summary>
		/// <param name="graphics">graphics instance</param>
		/// <param name="backgroundRectangle">cell rectangle which needs to be drawn</param>
		/// <param name="backgroundBrush">brush we are using to paint the background</param>
		protected virtual void ShadeAreaUnderItem(Graphics graphics, Rectangle backgroundRectangle, Brush backgroundBrush)
		{
			if (graphics == null)
			{
				return;
			}

			if (backgroundBrush != null)
			{
				graphics.FillRectangle(backgroundBrush, backgroundRectangle);
				this.RedrawGrid(graphics, backgroundRectangle);
			}
		}

		/// <summary>
		/// Draws an item
		/// A null background is now used to signal that there is a background bitmap that needs to be redrawn
		/// Use the transparent color to draw without a background
		/// </summary>
		/// <param name="graphics">graphics instance</param>
		/// <param name="item">item we are drawing</param>
		/// <param name="backgroundBrush">brush used to draw the background</param>
		protected virtual void DrawItem(Graphics graphics, Item item, Brush backgroundBrush)
		{
			if (item == null)
			{
				return;
			}

			Point itemScreenLocation = this.CellTopLeft(item.Location);

			// First draw the background for all the cells this item occupies
			this.ShadeAreaUnderItem(graphics, new Rectangle(itemScreenLocation, new Size(item.Width * Database.DB.ItemUnitSize, item.Height * Database.DB.ItemUnitSize)), backgroundBrush);

			// Draw the item
			this.DrawItem(graphics, item, itemScreenLocation);
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
			{
				ans = string.Compare(item1.Name, item2.Name, StringComparison.OrdinalIgnoreCase);
			}

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
				if (suppressMessage || Settings.Default.SuppressWarnings || MessageBox.Show(
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
			{
				return;
			}

			this.MouseMoveCallback(sender, e); // process the mouse moving to this location...just in case

			Item focusedItem = this.FindItem(this.LastCellWithFocus);
			if (focusedItem != null)
			{
				// Allocate the List if not already done.
				if (this.selectedItems == null)
				{
					this.selectedItems = new List<Item>();
				}

				if (!this.IsItemSelected(focusedItem))
				{
					// Check to see if the item is already selected
					this.selectedItems.Add(focusedItem);
					this.selectedItemsChanged = true;
				}
				else
				{
					this.RemoveSelectedItem(focusedItem);
				}
			}

			this.OnItemSelected(this, new SackPanelEventArgs(null, null));
			this.MouseMoveCallback(sender, e); // process mouse move again to apply the graphical effects of dragging an item.
		}

		/// <summary>
		/// Handler for the mouse pointer entering the sack panel.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void MouseEnterCallback(object sender, EventArgs e)
		{
			////this.Focus();
			this.Select();
		}

		/// <summary>
		/// Handler for the mouse pointer leaving the sack panel.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void MouseLeaveCallback(object sender, EventArgs e)
		{
			this.controlKeyDown = false;
			if (this.Sack == null)
			{
				return;
			}

			if (!this.DragInfo.IsActive)
			{
				this.HighlightItemUnderMouse(new Point(-1, -1));
			}
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
					MessageBox.Show(exception.ToString(), Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, RightToLeftOptions);
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
			{
				return Resources.SackPanelMenuVault2;
			}

			if (autoMoveLocation == AutoMoveLocation.Player && !this.SecondaryVaultShown)
			{
				return Resources.SackPanelMenuPlayer;
			}

			if (autoMoveLocation == AutoMoveLocation.Stash)
			{
				return Resources.SackPanelMenuStash;
			}

			if (autoMoveLocation == AutoMoveLocation.Vault)
			{
				return Resources.SackPanelMenuVault;
			}

			if (autoMoveLocation == AutoMoveLocation.Trash)
			{
				return Resources.SackPanelMenuTrash;
			}

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
					{
						return;
					}

					if (toolStripItem.Name.StartsWith(Resources.GlobalMenuBag.Substring(0, hashSign), StringComparison.OrdinalIgnoreCase))
					{
						int offset = 1;

						// For the player panel we do not need an offset since the bags are already offset by 1.
						if (this.SackType == SackType.Player || this.SackType == SackType.Sack)
						{
							offset = 0;
						}

						automoveDestination = (AutoMoveLocation)(Convert.ToInt32(toolStripItem.Name.Substring(hashSign), CultureInfo.InvariantCulture) - offset);
					}
					else if (toolStripItem.Name == Resources.SackPanelMenuVault)
					{
						automoveDestination = AutoMoveLocation.Vault;
					}
					else if (toolStripItem.Name == Resources.SackPanelMenuPlayer)
					{
						automoveDestination = AutoMoveLocation.Player;
					}
					else if (toolStripItem.Name == Resources.SackPanelMenuTrash)
					{
						automoveDestination = AutoMoveLocation.Trash;
					}
					else if (toolStripItem.Name == Resources.SackPanelMenuVault2)
					{
						automoveDestination = AutoMoveLocation.SecondaryVault;
					}
					else if (toolStripItem.Name == Resources.SackPanelMenuStash)
					{
						automoveDestination = AutoMoveLocation.Stash;
					}
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
					newItem.GetDBData();

					// Set DragInfo to focused item.
					this.DragInfo.Set(this, this.Sack, focusedItem, new Point(1, 1));

					// now drag the new item
					this.DragInfo.MarkModified(newItem);
					Refresh();
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
						focusedItem.RelicBonusInfo = Database.DB.GetInfo(newBonus);
						focusedItem.MarkModified();

						// mark the sack as modified also
						this.Sack.IsModified = true;
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
				string selectedItem = e.ClickedItem.Text;
				if (selectedItem == Resources.SackPanelMenuDelete)
				{
					if (this.selectedItems != null)
					{
						if (Settings.Default.SuppressWarnings || MessageBox.Show(
							Resources.SackPanelDeleteMultiMsg,
							Resources.SackPanelDeleteMulti,
							MessageBoxButtons.YesNo,
							MessageBoxIcon.Warning,
							MessageBoxDefaultButton.Button1,
							RightToLeftOptions) == DialogResult.Yes)
						{
							foreach (Item sackSelectedItem in this.selectedItems)
							{
								this.DeleteItem(sackSelectedItem, true);
							}

							this.ClearSelection();
						}
					}
					else
					{
						this.DeleteItem(focusedItem, false);
					}
				}
				else if (selectedItem == Resources.SackPanelMenuRemoveRelic)
				{
					if (Settings.Default.SuppressWarnings || MessageBox.Show(
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
						Item relic = focusedItem.RemoveRelic();

						// Put relic in DragInfo
						this.DragInfo.MarkModified(relic);
						Refresh();
					}
				}
				else if (selectedItem == Resources.SackPanelMenuCopy)
				{
					// Set DragInfo to focused item.
					this.DragInfo.Set(this, this.Sack, focusedItem, new Point(1, 1));

					// copy the item
					Item newItem = focusedItem.Duplicate(false);

					// now drag it
					this.DragInfo.MarkModified(newItem);
					Refresh();
				}
				else if (selectedItem == Resources.SackPanelMenuDuplicate)
				{
					// Set DragInfo to focused item.
					this.DragInfo.Set(this, this.Sack, focusedItem, new Point(1, 1));

					// copy the item
					Item newItem = focusedItem.Duplicate(true);

					// now drag it
					this.DragInfo.MarkModified(newItem);
					Refresh();
				}
				else if (selectedItem == Resources.SackPanelMenuProperties)
				{
					ItemProperties dlg = new ItemProperties();
					dlg.Item = focusedItem;
					dlg.ShowDialog();
				}
				else if (selectedItem == Resources.SackPanelMenuSeed)
				{
					ItemSeedDialog dlg = new ItemSeedDialog();
					dlg.SelectedItem = focusedItem;
					int origSeed = focusedItem.Seed;
					dlg.ShowDialog();

					// See if the seed was changed
					if (focusedItem.Seed != origSeed)
					{
						// Tell the sack that it has been modified
						this.Sack.IsModified = true;
					}
				}
				else if (selectedItem == Resources.SackPanelMenuCharm || selectedItem == Resources.SackPanelMenuRelic)
				{
					focusedItem.Number = 10;

					float randPercent = (float)Item.GenerateSeed() / 0x7fff;
					LootTableCollection table = focusedItem.BonusTable;

					if (table != null && table.Length > 0)
					{
						int i = table.Length;
						foreach (KeyValuePair<string, float> e1 in table)
						{
							i--;
							if (randPercent <= e1.Value || i == 0)
							{
								focusedItem.RelicBonusId = TQData.NormalizeRecordPath(e1.Key);
								break;
							}
							else
							{
								randPercent -= e1.Value;
							}
						}
					}

					focusedItem.GetDBData();
					focusedItem.MarkModified();
					this.Sack.IsModified = true;
					Refresh();
				}
				else if (selectedItem == Resources.SackPanelMenuFormulae)
				{
					// Set DragInfo to focused item.
					this.DragInfo.Set(this, this.Sack, focusedItem, new Point(1, 1));

					// create artifact
					Item artifact = focusedItem.CraftArtifact();

					// generate bonus
					float randPercent = (float)Item.GenerateSeed() / 0x7fff;
					LootTableCollection table = artifact.BonusTable;

					if (table != null && table.Length > 0)
					{
						int i = table.Length;
						foreach (KeyValuePair<string, float> e1 in table)
						{
							i--;
							if (randPercent <= e1.Value || i == 0)
							{
								artifact.RelicBonusId = TQData.NormalizeRecordPath(e1.Key);
								artifact.RelicBonusInfo = Database.DB.GetInfo(e1.Key);
								artifact.MarkModified();
								break;
							}
							else
							{
								randPercent -= e1.Value;
							}
						}
					}

					// Put artifact in DragInfo
					this.DragInfo.MarkModified(artifact);
					Refresh();
				}
				else if (selectedItem == Resources.SackPanelMenuSplit)
				{
					// Set DragInfo to focused item.
					this.DragInfo.Set(this, this.Sack, focusedItem, new Point(1, 1));

					// pull out all but one item
					Item newItem = focusedItem.PopAllButOneItem();

					// Put the item in DragInfo
					this.DragInfo.MarkModified(newItem);
					Refresh();
				}
				else if (selectedItem == Resources.SackPanelMenuClear)
				{
					this.ClearSelection();
					Refresh();
				}
			}
		}

		/// <summary>
		/// Draws an empty background in the specified area
		/// </summary>
		/// <param name="graphics">graphics instance</param>
		/// <param name="x">x cell coordinate</param>
		/// <param name="y">y cell coordinate</param>
		/// <param name="width">width of the fill</param>
		/// <param name="height">height of the fill</param>
		private void ClearArea(Graphics graphics, int x, int y, int width, int height)
		{
			graphics.FillRectangle(this.EmptyCellBrush, x, y, width, height);
		}

		/// <summary>
		/// Draws an empty background in the specified rectangle.
		/// </summary>
		/// <param name="graphics">graphics instance</param>
		/// <param name="clearRect">Rectangle to be cleared.</param>
		private void ClearArea(Graphics graphics, Rectangle clearRect)
		{
			this.ClearArea(graphics, clearRect.X, clearRect.Y, clearRect.Width, clearRect.Height);
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
				Point topLeftCell = this.FindCell(new Point(location.X + Database.DB.HalfUnitSize, location.Y + Database.DB.HalfUnitSize));
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
				{
					this.ItemsUnderDragItem.Add(item);
				}

				this.LastDragLocation = location;
				this.ItemsUnderOldDragLocation.Clear();
				foreach (Item item in this.ItemsUnderDragItem)
				{
					this.ItemsUnderOldDragLocation.Add(item);
				}

				this.DrawItem(graphics, this.DragInfo.Item, this.LastDragLocation);
			}
			catch (NullReferenceException exception)
			{
				MessageBox.Show(exception.ToString(), Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, RightToLeftOptions);
			}
		}

		/// <summary>
		/// Draw the sack panel border
		/// </summary>
		/// <param name="graphics">graphics instance</param>
		private void DrawBorder(Graphics graphics)
		{
			if (this.DisableBorder || graphics == null)
			{
				return;
			}

			graphics.DrawRectangle(this.borderPen, 0, 0, this.Size.Width, this.Size.Height);
		}

		/// <summary>
		/// Draws the grid within the sack panel
		/// </summary>
		/// <param name="graphics">graphics instance</param>
		private void DrawGrid(Graphics graphics)
		{
			if (this.DisableGrid || graphics == null)
			{
				return;
			}

			for (int x = 1; x < this.SackSize.Width; ++x)
			{
				graphics.DrawLine(this.gridPen, this.CellTopLeft(new Point(x, 0)), this.CellBottomLeft(new Point(x, this.SackSize.Height - 1)));
			}

			for (int y = 1; y < this.SackSize.Height; ++y)
			{
				graphics.DrawLine(this.gridPen, this.CellTopLeft(new Point(0, y)), this.CellTopRight(new Point(this.SackSize.Width - 1, y)));
			}
		}

		/// <summary>
		/// Callback for key down
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">KeyEventArgs data</param>
		private void KeyDownCallback(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.ControlKey)
			{
				this.controlKeyDown = true;
			}

			if (e.KeyData == (Keys.Control | Keys.F))
			{
				this.OnActivateSearch(this, new SackPanelEventArgs(null, null));
			}

			if (e.KeyData == (Keys.Control | Keys.A))
			{
				this.SelectAllItems();
			}

			if (e.KeyData == (Keys.Control | Keys.D))
			{
				this.OnClearAllItemsSelected(this, new SackPanelEventArgs(null, null));
			}

			if (e.KeyData == (Keys.Control | Keys.Add) || e.KeyData == (Keys.Control | Keys.Oemplus))
			{
				this.OnResizeForm(this, new ResizeEventArgs(0.1F));
			}

			if (e.KeyData == (Keys.Control | Keys.Subtract) || e.KeyData == (Keys.Control | Keys.OemMinus))
			{
				this.OnResizeForm(this, new ResizeEventArgs(-0.1F));
			}

			if (e.KeyData == (Keys.Control | Keys.Home))
			{
				this.OnResizeForm(this, new ResizeEventArgs(1.0F));
			}

			if (e.KeyData == Keys.F5)
			{
				this.Refresh();
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
			{
				this.controlKeyDown = false;
			}
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
				else if (e.KeyChar == 'c')
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
				else if (e.KeyChar == 'd')
				{
					// Drop (move to trash)
					if (!this.DragInfo.IsActive)
					{
						Item focusedItem = this.FindItem(this.LastCellWithFocus);
						if (focusedItem != null)
						{
							this.DragInfo.Set(this, this.Sack, focusedItem, new Point(1, 1));
							this.DragInfo.AutoMove = AutoMoveLocation.Trash;
							this.OnAutoMoveItem(this, new SackPanelEventArgs(null, null));
						}
					}
				}
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
				{
					e.TextColor = Color.Black;
				}
				else
				{
					e.TextColor = Color.FromArgb(200, 200, 200);
				}

				base.OnRenderItemText(e);
			}
		}
	}
}