//-----------------------------------------------------------------------
// <copyright file="SackPanel.Rendering.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Globalization;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Logs;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Components;

/// <summary>
/// Class for holding all of the UI functions of the sack panel.
/// </summary>
public partial class SackPanel : Panel, IScalingControl
{
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
				if (DefaultImage != null)
					// Draw the medallion if the sack is not created
					e.Graphics.DrawImage(DefaultImage, 0, 0, this.Width, this.Height);
			}
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

		Color highlightColor = this.HighlightInvalidItemColor;

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
			if (item != this.DragInfo.Item && !RecordId.IsNullOrEmpty(item.BaseItemId))
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

			var highlight = false;
			// Highlight search
			if (this.HighlightService.HighlightedItems.Count > 0 && this.HighlightService.HighlightedItems.Contains(item))
			{
				highlight = true;
				backgroundColor = this.HighlightService.HighlightSearchItemColor;
				alpha = AdjustAlpha(alpha);
			}
			// If we are showing the cannot equip background then 
			// change to invalid color and adjust the alpha.
			else if (
				!this.SecondaryVaultShown
				&& (
					(USettings.EnableItemRequirementRestriction && !this.PlayerMeetRequierements(item))
					|| !IsSuitableForCurrentPlayer(item)
				)
			)
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
			this.ShadeAreaUnderItem(e.Graphics, item, backgroundColor, alpha, highlight);

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
		if (item.HasRelicOrCharmSlot1 || item.HasRelicOrCharmSlot2)
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
	protected virtual void ShadeAreaUnderItem(Graphics graphics, Item item, Color backgroundColor, int alpha, bool highlight)
	{
		if (item == null)
			return;

		Point screenLocation = this.CellTopLeft(item.Location);
		this.ShadeAreaUnderItem(graphics, new Rectangle(screenLocation, new Size(item.Width * UIService.ItemUnitSize, item.Height * UIService.ItemUnitSize)), backgroundColor, alpha, highlight);
	}

	/// <summary>
	/// Shades the background of an item with alpha blending.
	/// </summary>
	/// <param name="graphics">graphics instance</param>
	/// <param name="backgroundRectangle">cell rectangle which needs to be drawn</param>
	/// <param name="backgroundColor">Color that the background will be painted</param>
	/// <param name="alpha">alpha value for the color</param>
	protected virtual void ShadeAreaUnderItem(Graphics graphics, Rectangle backgroundRectangle, Color backgroundColor, int alpha, bool highlight)
	{
		if (graphics == null)
			return;

		using (SolidBrush brush = new SolidBrush(Color.FromArgb(alpha, backgroundColor)))
		{
			graphics.FillRectangle(brush, backgroundRectangle);

			// Add highlight borders
			if (highlight)
				graphics.DrawRectangle(this.HighlightSearchItemBorder, backgroundRectangle);

		}
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
}
