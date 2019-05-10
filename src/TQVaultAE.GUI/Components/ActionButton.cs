//-----------------------------------------------------------------------
// <copyright file="ActionButton.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI.Components
{
	using Properties;
	using System;
	using System.Drawing;
	using System.Globalization;
	using System.Timers;
	using System.Windows.Forms;
	using TQVaultAE.GUI.Models;
	using TQVaultData;

	/// <summary>
	/// Class for displaying the action panel which has the animation of
	/// removing a relic from an item or splitting a stack.
	/// </summary>
	public class ActionButton : Panel
	{
		/// <summary>
		/// Bitmap for left door
		/// </summary>
		private Bitmap leftDoor;

		/// <summary>
		/// Bitmap for right door
		/// </summary>
		private Bitmap rightDoor;

		/// <summary>
		/// DragInfo for current dragged item
		/// </summary>
		private ItemDragInfo dragInfo;

		/// <summary>
		/// Brush for the normal background
		/// </summary>
		private Brush normalBackground;

		/// <summary>
		/// Brush for the active background
		/// </summary>
		private Brush activeBackground;

		/// <summary>
		/// Brush for the background flash
		/// </summary>
		private Brush flashBackground;

		/// <summary>
		/// Font for displaying quantity
		/// </summary>
		private Font numberFont;

		/// <summary>
		/// Brush for numbers
		/// </summary>
		private Brush numberBrush;

		/// <summary>
		/// Format for numbers
		/// </summary>
		private StringFormat numberFormat;

		/// <summary>
		/// Location of the dragged item
		/// </summary>
		private Point dragLocation;

		/// <summary>
		/// Bool for determining if the animation is active.
		/// </summary>
		private bool isActive;

		/// <summary>
		/// Flag for paint completion.
		/// </summary>
		private bool havePaintedOnce;

		/// <summary>
		/// Animation timer
		/// </summary>
		private System.Timers.Timer timer;

		/// <summary>
		/// The current tick
		/// </summary>
		private int tick;

		/// <summary>
		/// Door animation maximum position
		/// </summary>
		private int maxDoorPositions;

		/// <summary>
		/// Door animation current position.
		/// 0 == closed, maxDoorPositions-1 == open
		/// </summary>
		private int doorPosition;

		/// <summary>
		/// stack animation - number of ticks to move the stack to the center
		/// </summary>
		private int stackTicksToCenter;

		/// <summary>
		/// stack animation - number of ticks to flash
		/// </summary>
		private int stackFlashTicks;

		/// <summary>
		/// stack animation - number of ticks to move the 2 items out
		/// </summary>
		private int stackSplitTicks;

		/// <summary>
		/// stack animation - number of ticks to pause before repeat.
		/// </summary>
		private int stackPauseTicks;

		/// <summary>
		/// Initializes a new instance of the ActionButton class.
		/// </summary>
		/// <param name="width">Width of the panel</param>
		/// <param name="height">Height of the panel</param>
		/// <param name="dragInfo">Info for dragged item</param>
		public ActionButton(int width, int height, ItemDragInfo dragInfo)
		{
			this.dragInfo = dragInfo;
			this.dragLocation.X = -1;
			this.dragLocation.Y = -1;
			Width = width;
			Height = height;

			this.CreateDoors();
			this.CreateBrushes();
			this.InitializeAnimationParams();
			this.InitializeTimer();

			BackColor = ((SolidBrush)this.normalBackground).Color;

			// set up event handlers
			this.Paint += new PaintEventHandler(this.PaintCallback);
			this.MouseMove += new MouseEventHandler(this.MouseMoveCallback);
			this.MouseLeave += new EventHandler(this.MouseLeaveCallback);
			this.MouseEnter += new EventHandler(this.MouseEnterCallback);
			this.MouseDown += new MouseEventHandler(this.MouseDownCallback);

			// Da_FileServer: Some small paint optimizations.
			this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
		}

		/// <summary>
		/// Gets a value indicating whether the doors are closed.
		/// </summary>
		public bool AreDoorsClosed
		{
			get
			{
				return this.doorPosition == 0;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the doors are fully open
		/// </summary>
		public bool AreDoorsFullyOpen
		{
			get
			{
				return this.doorPosition == (this.maxDoorPositions - 1);
			}
		}

		/// <summary>
		/// Gets a value indicating whether a stack is being dragged
		/// </summary>
		public bool IsStackBeingDragged
		{
			get
			{
				return this.dragInfo.IsActive && this.dragInfo.CanBeModified && this.dragInfo.Item.DoesStack && this.dragInfo.Item.Number > 1;
			}
		}

		/// <summary>
		/// Gets a value indicating whether an item with a relic is being dragged
		/// </summary>
		public bool IsItemWithRelicBeingDragged
		{
			get
			{
				return this.dragInfo.IsActive && this.dragInfo.CanBeModified && this.dragInfo.Item.HasRelic;
			}
		}

		/// <summary>
		/// Gets the total stack animation ticks
		/// </summary>
		public int StackTotalAnimationTicks
		{
			get
			{
				return this.stackTicksToCenter + this.stackFlashTicks + this.stackSplitTicks + this.stackPauseTicks;
			}
		}

		/// <summary>
		/// Handler for mouse button clicks
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">MouseEventArgs data</param>
		private void MouseDownCallback(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				if (this.IsStackBeingDragged)
				{
					this.SplitStack();
				}
				else if (this.IsItemWithRelicBeingDragged)
				{
					this.SplitItemAndRelic();
				}
			}
			else if (e.Button == MouseButtons.Right)
			{
				if (this.dragInfo.IsActive && this.dragInfo.CanBeCanceled)
				{
					this.dragInfo.Cancel();

					// Now redraw ourselves
					Refresh();
				}
			}
		}

		/// <summary>
		/// Splits a stack
		/// </summary>
		private void SplitStack()
		{
			if (this.dragInfo.IsModifiedItem)
			{
				// we have already been splitting this stack.
				// Here is what we do:
				// just increment the original stackSize and decrement the dragItem stackSize
				this.dragInfo.Original.Item.StackSize++;
				this.dragInfo.Item.StackSize--;
				this.dragInfo.MarkModified(this.dragInfo.Item);
			}
			else
			{
				// We need to pop all but one item off the stack as a new item.
				Item newStack = this.dragInfo.Item.PopAllButOneItem();
				this.dragInfo.MarkModified(newStack);
			}

			Refresh();
		}

		/// <summary>
		/// Removes a relic from the item
		/// </summary>
		private void SplitItemAndRelic()
		{
			// pull out the relic
			Item relic = this.dragInfo.Item.RemoveRelic();
			this.dragInfo.MarkModified(relic);
			Refresh();
		}

		/// <summary>
		/// Handler for mouse pointer leaving the action button
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void MouseLeaveCallback(object sender, EventArgs e)
		{
			this.isActive = false;
			BackColor = ((SolidBrush)this.normalBackground).Color;
			this.dragLocation.X = -1;
			this.dragLocation.Y = -1;
			if (this.dragInfo.IsActive)
			{
				Refresh();
			}
		}

		/// <summary>
		/// Handler for mouse entering the action button.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void MouseEnterCallback(object sender, EventArgs e)
		{
			this.isActive = true;
			BackColor = ((SolidBrush)this.activeBackground).Color;
			if (this.dragInfo.IsActive)
			{
				Refresh();
			}
		}

		/// <summary>
		/// Handler for mouse moving within the action button.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">MouseEventArgs data</param>
		private void MouseMoveCallback(object sender, MouseEventArgs e)
		{
			this.dragLocation = new Point(e.X, e.Y);
			if (this.dragInfo.IsActive)
			{
				Refresh();
			}
		}

		/// <summary>
		/// Creates the brushes for drawing the background and numbers
		/// </summary>
		private void CreateBrushes()
		{
			this.normalBackground = new SolidBrush(Color.Black);
			this.activeBackground = new SolidBrush(Color.FromArgb(23, 149, 15));
			this.flashBackground = new SolidBrush(Color.White);
			this.numberFont = new Font("Arial", 10F * Database.DB.Scale, GraphicsUnit.Pixel);
			this.numberBrush = new SolidBrush(Color.White);
			this.numberFormat = new StringFormat();
			this.numberFormat.Alignment = StringAlignment.Far; // right-justify

			this.isActive = false;
		}

		/// <summary>
		/// Initializes the animation timer.
		/// </summary>
		private void InitializeTimer()
		{
			this.timer = new System.Timers.Timer();
			((System.ComponentModel.ISupportInitialize)this.timer).BeginInit();
			this.timer.Interval = 100; // 10x per second
			this.timer.Enabled = true;
			this.timer.SynchronizingObject = this;
			this.timer.Elapsed += new ElapsedEventHandler(this.AnimationTick);
			((System.ComponentModel.ISupportInitialize)this.timer).EndInit();
		}

		/// <summary>
		/// Initalizes the animation parameters.
		/// </summary>
		private void InitializeAnimationParams()
		{
			this.maxDoorPositions = 10;
			this.doorPosition = 0;
			this.havePaintedOnce = false;
			this.tick = 0;

			this.stackTicksToCenter = 10; // 1 second to get to center
			this.stackFlashTicks = 1; // 1/10ths of a second
			this.stackSplitTicks = 10; // move 2 stacks out for 1s
			this.stackPauseTicks = 5;
		}

		/// <summary>
		/// Creates the doors
		/// Loads the door bitmaps from the resources.
		/// </summary>
		private void CreateDoors()
		{
			// Take the medallion bitmap and split it in twain
			Bitmap medallion = Resources.tqmedalliondoor;

			RectangleF rect = new RectangleF(0.0F, 0.0F, (float)medallion.Width / 2.0F, (float)medallion.Height);

			this.leftDoor = medallion.Clone(rect, medallion.PixelFormat);
			rect.X = rect.Width;
			this.rightDoor = medallion.Clone(rect, medallion.PixelFormat);
		}

		/// <summary>
		/// Animation tick handler.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">ElapsedEventArgs data</param>
		private void AnimationTick(object sender, ElapsedEventArgs e)
		{
			++this.tick;
			if (!this.havePaintedOnce)
			{
				return;
			}

			// see if something is being dragged
			if ((!this.IsStackBeingDragged && !this.IsItemWithRelicBeingDragged) || !this.dragInfo.CanBeModified)
			{
				// just close the doors if necessary
				if (!this.AreDoorsClosed)
				{
					this.doorPosition--;
					if (this.isActive)
					{
						this.isActive = false; // we cannot be active if we have no good drag item
						BackColor = ((SolidBrush)this.normalBackground).Color;
					}

					Refresh();
				}
				else if (this.isActive)
				{
					this.isActive = false;
					BackColor = ((SolidBrush)this.normalBackground).Color;
					Refresh();
				}

				return; // nothing else to do.
			}

			// okay something is being dragged.  open the doors
			if (!this.AreDoorsFullyOpen)
			{
				this.doorPosition++;
			}

			// now redraw
			Refresh();
		}

		/// <summary>
		/// Draws the dragged item
		/// </summary>
		/// <param name="g">graphics instance</param>
		/// <param name="item">item bitmap</param>
		/// <param name="x">x location</param>
		/// <param name="y">y location</param>
		/// <param name="scale">item scale</param>
		/// <param name="quantity">item quantity for stacks</param>
		/// <param name="drawRelic">if item has a relic</param>
		private void DrawItem(Graphics g, Bitmap item, float x, float y, float scale, int quantity, bool drawRelic)
		{
			// first draw the base item
			g.DrawImage(item, x, y, item.Width * scale, item.Height * scale);

			// Add the relic overlay if this item has a relic in it.
			if (drawRelic)
			{
				Bitmap overlay = Database.DB.LoadRelicOverlayBitmap();
				if (overlay != null)
				{
					// draw it in the bottom-right most cell of this item
					float rx = x + ((item.Width - Database.DB.ItemUnitSize) * scale);
					float ry = y + ((item.Height - Database.DB.ItemUnitSize) * scale);

					g.DrawImage(overlay, rx, ry, overlay.Width * scale, overlay.Height * scale);
				}
			}

			// Add any number we need to add.
			if (quantity > 0)
			{
				string numberString = quantity.ToString(CultureInfo.CurrentCulture);

				// Draw the number along the bottom of the item
				float nx = x;
				float ny = y + (Database.DB.ItemUnitSize * scale);

				float height = (float)this.numberFont.Height;
				float width = (float)Database.DB.ItemUnitSize * scale;
				float yy = (float)(ny - (0.75 * this.numberFont.Height) - 1);
				float xx = (float)nx;

				RectangleF rect = new RectangleF(xx, yy, width, height);
				g.DrawString(numberString, this.numberFont, this.numberBrush, rect, this.numberFormat);
			}
		}

		/// <summary>
		/// Draws the floating stack animation
		/// </summary>
		/// <param name="g">graphics instance</param>
		private void DrawStackTick(Graphics g)
		{
			int tickNum = this.tick % this.StackTotalAnimationTicks;

			int prevTick = 0;
			int tickTotal = this.stackTicksToCenter;
			if (tickNum < tickTotal)
			{
				Item item = this.dragInfo.Item;

				// draw the single item moving towards the center
				float scale = Database.DB.Scale;

				// Figure out its offset
				float pctComplete = (float)(tickNum - prevTick) / (float)(tickTotal - prevTick - 1);

				// linear interpolate between off-screen and center
				float xloc = ((1 - pctComplete) * (-1 * item.ItemBitmap.Width * scale)) + (pctComplete * ((Width / 2.0F) - (scale * item.ItemBitmap.Width / 2.0F)));
				float yloc = ((1 - pctComplete) * (Height / 4.0F)) + (pctComplete * (Height / 2.0F));
				yloc -= scale * item.ItemBitmap.Height / 2.0F;

				// now draw it
				this.DrawItem(g, item.ItemBitmap, xloc, yloc, scale, item.Number, false);

				return;
			}

			prevTick = tickTotal;
			tickTotal += this.stackFlashTicks;
			if (tickNum < tickTotal)
			{
				// draw the flash
				g.FillRectangle(this.flashBackground, 0, 0, Width, Height);
				return;
			}

			prevTick = tickTotal;
			tickTotal += this.stackSplitTicks;
			if (tickNum < tickTotal)
			{
				// draw the split stack -- all but 1 going up and 1 going down
				Item item = this.dragInfo.Item;

				float scale = Database.DB.Scale;

				// Figure out its offset
				float pctComplete = (float)(tickNum - prevTick) / (float)(tickTotal - prevTick - 1);

				float offset = pctComplete * item.ItemBitmap.Height * scale;
				float x = (Width / 2.0F) - (scale * item.ItemBitmap.Width / 2.0F);

				// Draw the stack
				this.DrawItem(g, item.ItemBitmap, x, (Height / 2.0F) - offset, scale, item.Number - 1, false);

				// Draw the single
				this.DrawItem(g, item.ItemBitmap, x, (Height / 2.0F) + offset, scale, 1, false);
				return;
			}

			prevTick = tickTotal;
			tickTotal += this.stackPauseTicks;
			{
				// draw the pause
				Item item = this.dragInfo.Item;

				float scale = Database.DB.Scale;

				// Just draw the 2 stack at a fixed offset
				float offset = item.ItemBitmap.Height * scale;
				float x = (Width / 2.0F) - (scale * item.ItemBitmap.Width / 2.0F);

				// Draw the stack
				this.DrawItem(g, item.ItemBitmap, x, (Height / 2.0F) - offset, scale, item.Number - 1, false);

				// Draw the single
				this.DrawItem(g, item.ItemBitmap, x, (Height / 2.0F) + offset, scale, 1, false);
			}
		}

		/// <summary>
		/// Draws the floating relic animation
		/// </summary>
		/// <param name="g">graphics instance</param>
		private void DrawRelicTick(Graphics g)
		{
			// Just use the stack tick animation data!
			int tickNum = this.tick % this.StackTotalAnimationTicks;

			Item item = this.dragInfo.Item;

			// We need to set the scale such that the item is no more than x% of the height or width
			float maxPct = 0.50F;
			float scale = Database.DB.Scale;
			if (item.ItemBitmap.Width > maxPct * Width)
			{
				scale = (maxPct * Width * Database.DB.Scale) / item.ItemBitmap.Width;
			}

			if (item.ItemBitmap.Height > maxPct * Height)
			{
				float vscale = (maxPct * Height * Database.DB.Scale) / item.ItemBitmap.Height;
				if (vscale < scale)
				{
					scale = vscale;
				}
			}

			int prevTick = 0;
			int tickTotal = this.stackTicksToCenter;
			if (tickNum < tickTotal)
			{
				// draw the item moving towards the center

				// Figure out its offset
				float pctComplete = (float)(tickNum - prevTick) / (float)(tickTotal - prevTick - 1);

				// linear interpolate between off-screen and center
				float xloc = ((1 - pctComplete) * (-1 * item.ItemBitmap.Width * scale)) + (pctComplete * ((Width / 2.0F) - (scale * item.ItemBitmap.Width / 2.0F)));
				float yloc = ((1 - pctComplete) * (Height / 4.0F)) + (pctComplete * (Height / 2.0F));
				yloc -= scale * item.ItemBitmap.Height / 2.0F;

				// now draw it
				this.DrawItem(g, item.ItemBitmap, xloc, yloc, scale, 0, true);

				return;
			}

			prevTick = tickTotal;
			tickTotal += this.stackFlashTicks;
			if (tickNum < tickTotal)
			{
				// draw the flash
				g.FillRectangle(this.flashBackground, 0, 0, Width, Height);
				return;
			}

			prevTick = tickTotal;
			tickTotal += this.stackSplitTicks;
			if (tickNum < tickTotal)
			{
				// draw the item going up and the relic going down
				// Figure out the item offset
				float pctComplete = (float)(tickNum - prevTick) / (float)(tickTotal - prevTick - 1);

				float offset = pctComplete * item.ItemBitmap.Height * scale;
				if (offset >= .75 * Height / 2.0F)
				{
					offset = .75F * Height / 2.0F;
				}

				float x = (Width / 2.0F) - (scale * item.ItemBitmap.Width / 2.0F);

				// Draw the item
				this.DrawItem(g, item.ItemBitmap, x, (Height / 2.0F) - offset, scale, 0, false);

				// Draw the relic
				// We need to figure out the bitmap to use
				Bitmap relicBitmap = Database.DB.LoadRelicOverlayBitmap();
				if (item.RelicInfo != null)
				{
					if (item.Var1 >= item.RelicInfo.CompletedRelicLevel)
					{
						relicBitmap = Database.DB.LoadBitmap(item.RelicInfo.Bitmap);
					}
					else
					{
						relicBitmap = Database.DB.LoadBitmap(item.RelicInfo.ShardBitmap);
					}
				}

				// Draw the relic FULL SIZE
				int num = item.Var1;
				if (num == 0)
				{
					num = 1;
				}
				else if (item.RelicInfo != null && num >= item.RelicInfo.CompletedRelicLevel)
				{
					num = 0;
				}

				this.DrawItem(g, relicBitmap, x, (Height / 2.0F) + (offset * scale), Database.DB.Scale, num, false);
				return;
			}

			prevTick = tickTotal;
			tickTotal += this.stackPauseTicks;
			{
				// draw the pause
				// Just draw the item and relic at a fixed offset
				float offset = item.ItemBitmap.Height * scale;
				if (offset >= .75 * Height / 2.0F)
				{
					offset = .75F * Height / 2.0F;
				}

				float x = (Width / 2.0F) - (scale * item.ItemBitmap.Width / 2.0F);

				// Draw the item
				this.DrawItem(g, item.ItemBitmap, x, (Height / 2.0F) - offset, scale, 0, false);

				// Draw the relic
				// We need to figure out the bitmap to use
				Bitmap relicBitmap = Database.DB.LoadRelicOverlayBitmap();
				if (item.RelicInfo != null)
				{
					if (item.Var1 >= item.RelicInfo.CompletedRelicLevel)
					{
						relicBitmap = Database.DB.LoadBitmap(item.RelicInfo.Bitmap);
					}
					else
					{
						relicBitmap = Database.DB.LoadBitmap(item.RelicInfo.ShardBitmap);
					}
				}

				// Draw the relic FULL SIZE
				int num = item.Var1;
				if (num == 0)
				{
					num = 1;
				}
				else if (item.RelicInfo != null && num >= item.RelicInfo.CompletedRelicLevel)
				{
					num = 0;
				}

				this.DrawItem(g, relicBitmap, x, (Height / 2.0F) + (offset * scale), Database.DB.Scale, num, false);
			}
		}

		/// <summary>
		/// Paint callback handler
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">PaintEventArgs data</param>
		private void PaintCallback(object sender, PaintEventArgs e)
		{
			this.havePaintedOnce = true;

			// First draw the active background if we are active
			if (this.isActive)
			{
				e.Graphics.FillRectangle(this.activeBackground, 0, 0, Width, Height);
			}

			// Now draw the animation
			if (this.IsStackBeingDragged)
			{
				this.DrawStackTick(e.Graphics);
			}
			else if (this.IsItemWithRelicBeingDragged)
			{
				this.DrawRelicTick(e.Graphics);
			}

			// draw the doors
			this.DrawDoors(e.Graphics);

			// Last step is to draw the drag item
			if (this.dragInfo.IsActive && (this.dragLocation.X >= 0))
			{
				this.DrawItem(
					e.Graphics,
					this.dragInfo.Item.ItemBitmap,
					(float)this.dragLocation.X - this.dragInfo.MouseOffset.X,
					(float)this.dragLocation.Y - this.dragInfo.MouseOffset.Y,
					Database.DB.Scale,
					this.dragInfo.Item.Number,
					this.dragInfo.Item.HasRelic);
			}
		}

		/// <summary>
		/// Draws the doors
		/// </summary>
		/// <param name="g">graphics instance</param>
		private void DrawDoors(Graphics g)
		{
			if (this.doorPosition == this.maxDoorPositions - 1)
			{
				// doors are completely open
				return;
			}

			// figure out the offset to draw each door
			float offset = (this.doorPosition / (float)(this.maxDoorPositions - 1)) * Width / 2;

			// Now draw each door
			g.DrawImage(this.leftDoor, 0.0F - offset, 0.0F, (float)Width / 2.0F, (float)Height);
			g.DrawImage(this.rightDoor, (float)(Width / 2.0F) + offset, 0.0F, (float)Width / 2.0F, (float)Height);
		}
	}
}