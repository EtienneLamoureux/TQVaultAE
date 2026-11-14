//-----------------------------------------------------------------------
// <copyright file="EquipmentPanel.Rendering.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
/// <summary>
/// Class for holding all of the UI functions of the sack panel in the stash panel.
/// </summary>

using TQVaultAE.Config;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Models;
using TQVaultAE.GUI.Tooltips;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Components.Panels;

public partial class EquipmentPanel
{
	/// <summary>
	/// Dims the second picture of a two-handed weapon. Built once, as it never changes; only the UI thread draws with it.
	/// </summary>
	private static readonly System.Drawing.Imaging.ImageAttributes DimmedImage = CreateDimmedImage();

	private static System.Drawing.Imaging.ImageAttributes CreateDimmedImage()
	{
		var attributes = new System.Drawing.Imaging.ImageAttributes();
		attributes.SetColorMatrix(new System.Drawing.Imaging.ColorMatrix()
		{
			Matrix00 = 0.80f, // Red
			Matrix11 = 0.80f, // Green
			Matrix22 = 0.80f, // Blue
			Matrix33 = 0.60f, // alpha
			Matrix44 = 1.00f  // w
		});
		return attributes;
	}

	/// <summary>
	/// Draws the background of the items in the panel during a Paint call.
	/// </summary>
	/// <param name="e">PaintEventArgs data</param>
	protected override void PaintAreaUnderItem(PaintEventArgs e)
	{
		Item? focusedItem = this.FindItem(this.LastCellWithFocus);
		foreach (Item item in this.ShownSack)
		{
			// Do not draw the item being dragged.
			if (item == this.DragInfo.Item)
				continue;

			// Figure out the background brush to use.
			bool showAccent = true;
			int alpha = this.UserAlpha;
			Color backgroundColor = this.GetItemBackgroundColor(item);

			// Check if the item is selected and use a different background
			if (this.IsItemSelected(item))
				alpha = AdjustAlpha(alpha);

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
					(this.USettings.EnableItemRequirementRestriction && !this.PlayerMeetRequierements(item))
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

			// See if this item is under the drag item
			if (this.DragInfo.IsActive && this.ItemsUnderDragItem.Contains(item))
			{
				// Use highlight color if it is the only item under the drag point, else use invalid
				alpha = AdjustAlpha(alpha);
				backgroundColor = this.HighlightInvalidItemColor;

				if (this.ItemsUnderDragItem.Count == 1)
				{
					int slot = EquipmentRules.FindSlot(this.ShownSack, this.ItemsUnderDragItem[0]);
					if (slot != -1 && EquipmentRules.CanEquip(this.ShownSack, this.DragInfo.Item, slot))
					{
						backgroundColor = this.HighlightValidItemColor;
						if (
							(this.USettings.EnableItemRequirementRestriction && !this.PlayerMeetRequierements(DragInfo.Item))
							|| !IsSuitableForCurrentPlayer(DragInfo.Item)
						) backgroundColor = this.HighlightInvalidItemColor;
					}
				}
			}

			if (!this.DragInfo.IsActive && item == focusedItem)
				alpha = AdjustAlpha(alpha);

			// Now do the shading
			if (!RecordId.IsNullOrEmpty(item.BaseItemId))
				this.ShadeAreaUnderItem(e.Graphics, item, backgroundColor, alpha, highlight);

			// Adjust the alpha and draw the accent.
			if (showAccent & HasItemBackgroundColor(item))
				this.DrawItemAccent(e.Graphics, item, backgroundColor, AdjustAlpha(alpha));

			// Check to see if we need to shade a 2H weapon slot
			if (item.IsInWeaponSlot && item.Is2HWeapon)
			{
				Item? item2H = this.GetItemFromShadowSlot(item.PositionY);
				if (item2H is not null && RecordId.IsNullOrEmpty(item2H.BaseItemId))
				{
					Rectangle rect = this.FindSlotRect(item2H.PositionX, item2H.PositionY);
					this.ShadeAreaUnderItem(e.Graphics, rect, backgroundColor, alpha, highlight);

					// Adjust the alpha and draw the accent.
					if (showAccent & HasItemBackgroundColor(item))
						this.DrawItemAccent(e.Graphics, item2H, backgroundColor, AdjustAlpha(alpha));
				}
			}

		}
	}

	/// <summary>
	/// Draws the background under a dragged item during a Paint call.
	/// </summary>
	/// <param name="e">PaintEventArgs data</param>
	protected override void PaintAreaUnderDragItem(PaintEventArgs e)
	{
		if (!this.DragInfo.IsActive || this.CellsUnderDragItem.Size.IsEmpty)
			return;

		if (this.ItemsUnderDragItem.Count <= 1)
		{
			Point tl = Point.Empty;
			Point br = Point.Empty;
			Color backgroundColor;
			int slot = -1;

			if (this.ItemsUnderDragItem.Count == 0)
			{
				// Find the slot which is under the drag item
				slot = this.FindEquipmentSlot(this.CellsUnderDragItem);
				Rectangle rect = this.FindSlotRect(slot);
				if (rect != Rectangle.Empty)
				{
					tl = new Point(rect.X, rect.Y);
					br = new Point(rect.X + rect.Width - 1, rect.Y + rect.Height - 1);
				}
				else
					slot = -1;

				// Check to see if the item is correct for the slot
				if (slot != -1)
				{
					backgroundColor = this.HighlightValidItemColor;

					if (!EquipmentRules.CanEquip(this.ShownSack, this.DragInfo.Item, slot)
						|| (
							this.USettings.EnableItemRequirementRestriction
							&& !this.PlayerMeetRequierements(DragInfo.Item)
						)
						|| !IsSuitableForCurrentPlayer(DragInfo.Item)
					)
					{
						backgroundColor = this.HighlightInvalidItemColor;
					}

					using (Brush brush = new SolidBrush(backgroundColor))
					{
						e.Graphics.FillRectangle(brush, tl.X, tl.Y, br.X - tl.X + 1, br.Y - tl.Y + 1);

						if (SackCollection.IsWeaponSlot(slot))
						{
							// Draw additional weapon box area
							e.Graphics.FillRectangle(
								brush,
								tl.X + UIService.HalfUnitSize,
								tl.Y - UIService.HalfUnitSize,
								UIService.ItemUnitSize,
								5 * UIService.ItemUnitSize);
						}
					}
				}
			}
		}
	}

	/// <summary>
	/// Draws the items in the panel during a Paint call.
	/// </summary>
	/// <param name="e">PaintEventArgs data</param>
	protected override void PaintItems(PaintEventArgs e)
	{
		// Now draw all the sack items
		foreach (Item item in this.ShownSack)
		{
			if (item != this.DragInfo.Item)
			{
				// do not draw the item being dragged.
				if (item.IsInWeaponSlot)
				{
					// Weapon slots only
					if (RecordId.IsNullOrEmpty(item.BaseItemId))
						// If the item is null try to draw it grayed out
						this.DrawItemShaded(e.Graphics, item);
					else
						// Otherwise draw it normally
						this.DrawItem(e.Graphics, item);
				}
				else if (!RecordId.IsNullOrEmpty(item.BaseItemId))
					this.DrawItem(e.Graphics, item);
			}
		}

		this.PaintLoadoutMarks(e.Graphics);
	}

	/// <summary>
	/// Draws an accent on the item graphic
	/// </summary>
	/// <param name="graphics">graphics instance</param>
	/// <param name="item">item we are adding the accent to</param>
	/// <param name="accentColor">Color that the accent will be painted</param>
	/// <param name="alpha">alpha value for the color</param>
	protected override void DrawItemAccent(Graphics graphics, Item item, Color accentColor, int alpha)
	{
		if (item is null)
			return;

		Rectangle rect = this.FindSlotRect(item.PositionX, item.PositionY);

		if (item.IsInWeaponSlot)
			// Adjust the accent to appear in the center 2x4 area of the weapon slot.
			rect = new Rectangle(rect.X, rect.Y + UIService.HalfUnitSize, rect.Width, rect.Height - UIService.ItemUnitSize);

		base.DrawItemAccent(graphics, rect, accentColor, alpha);
	}

	/// <summary>
	/// Shades the background of an item.
	/// </summary>
	/// <param name="graphics">graphics instance</param>
	/// <param name="item">Item to be shaded</param>
	/// <param name="backgroundColor">Color that we are using to paint the background.</param>
	/// <param name="alpha">Int containing the alpha value</param>
	protected override void ShadeAreaUnderItem(Graphics graphics, Item item, Color backgroundColor, int alpha, bool highlight)
		=> this.ShadeAreaUnderItem(graphics, this.FindSlotRect(item.PositionX, item.PositionY), backgroundColor, alpha, highlight);

	/// <summary>
	/// Shades the background of an item.
	/// </summary>
	/// <param name="graphics">graphics instance</param>
	/// <param name="backgroundRectangle">cell rectangle that needs to be drawn</param>
	/// <param name="backgroundColor">Color that we are using to paint the background.</param>
	/// <param name="alpha">Int containing the alpha value</param>
	protected override void ShadeAreaUnderItem(Graphics graphics, Rectangle backgroundRectangle, Color backgroundColor, int alpha, bool highlight)
	{
		// Fill the weapon box shape.
		int slot = this.FindEquipmentSlot(this.FindCell(backgroundRectangle.Location));
		if (SackCollection.IsWeaponSlot(slot))
		{
			// Since the weapon box is not a single rectangle, it needs to be drawn using CompositingMode.SourceCopy
			// so that the alpha values remain consistent for the combined image.
			using (Bitmap weaponBmp = new Bitmap(2 * UIService.ItemUnitSize, 5 * UIService.ItemUnitSize))
			using (Brush brush = new SolidBrush(Color.FromArgb(alpha, backgroundColor)))
			using (Graphics weaponBmpGraphics = Graphics.FromImage(weaponBmp))
			{
				weaponBmpGraphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;

				weaponBmpGraphics.FillRectangle(brush, UIService.HalfUnitSize, 0, UIService.ItemUnitSize, UIService.ItemUnitSize * 5);
				if (highlight)
				{
					// Add Highlight border
					weaponBmpGraphics.DrawRectangle(this.HighlightSearchItemBorder
						, UIService.HalfUnitSize
						, 0
						, UIService.ItemUnitSize, UIService.ItemUnitSize * 5);
				}

				weaponBmpGraphics.FillRectangle(brush, 0, UIService.HalfUnitSize, UIService.ItemUnitSize * 2, UIService.ItemUnitSize * 4);
				if (highlight)
				{
					// Add Highlight border
					weaponBmpGraphics.DrawRectangle(this.HighlightSearchItemBorder
						, 0
						, UIService.HalfUnitSize
						, UIService.ItemUnitSize * 2, UIService.ItemUnitSize * 4);

					// Cover inner borders
					weaponBmpGraphics.FillRectangle(brush
						, UIService.HalfUnitSize + this.HighlightSearchItemBorder.Width
						, this.HighlightSearchItemBorder.Width
						, UIService.ItemUnitSize - (this.HighlightSearchItemBorder.Width * 2)
						, (UIService.ItemUnitSize * 5) - (this.HighlightSearchItemBorder.Width * 2)
					);
				}

				graphics.DrawImage(weaponBmp, backgroundRectangle.X, backgroundRectangle.Y);
			}
		}
		else
		{
			if (slot == 1)
				// Adjust the rectangle to fit the amulet box.
				backgroundRectangle = this.FindSlotRect(1);

			// Do the normal shading.
			base.ShadeAreaUnderItem(graphics, backgroundRectangle, backgroundColor, alpha, highlight);
		}
	}

	/// <summary>
	/// Draws an item
	/// Changed by VillageIdiot
	/// A null background is now used to signal that there is a background bitmap that needs to be redrawn
	/// Use the transparent color to draw without a background
	/// </summary>
	/// <param name="graphics">graphics instance</param>
	/// <param name="item">item we are drawing</param>
	/// <param name="backgroundBrush">brush used to draw the background</param>
	protected override void DrawItem(Graphics graphics, Item item)
	{
		Point screenLocation;
		if (item.IsInWeaponSlot)
		{
			// Adjust for weapon slots
			screenLocation = WeaponTopLeft(
				SackCollection.GetWeaponLocationOffset(item.PositionY).X,
				SackCollection.GetWeaponLocationOffset(item.PositionY).Y,
				item.Width,
				item.Height);
		}
		else
			screenLocation = this.CellTopLeft(item.Location);

		if (item.IsAmulet)
			// Adjust the amulet to center it in the equipment box.
			screenLocation.X += (SackCollection.GetEquipmentLocationSize(1).Width - item.Width) * UIService.HalfUnitSize;

		this.DrawItem(graphics, item, screenLocation);
	}

	/// <summary>
	/// Draw the item grayed out.  Used for 2 handed items in weapon slots
	/// </summary>
	/// <param name="g">graphics instance</param>
	/// <param name="item">item we are drawing</param>
	/// <param name="backgroundBrush">brush we are using for the background</param>
	private void DrawItemShaded(Graphics g, Item item)
	{
		if (!RecordId.IsNullOrEmpty(item.BaseItemId))
		{
			// The item is not null
			// So we draw it normally and exit
			this.DrawItem(g, item);
			return;
		}

		Item? item2H = this.GetItemFromShadowSlot(item.PositionY);

		// TODO: Update logic to draw the shadow slot if the item was cancelled.
		// otherwise it is skipped because this.m_dragInfo.Item == item2H will be true.
		if (item2H is null || !item2H.Is2HWeapon || (this.DragInfo.Item == item2H && !this.DragInfo.IsBeingCancelled) || RecordId.IsNullOrEmpty(item2H.BaseItemId))
		{
			// Skip if the item is being dragged.
			// Also, only 2 Handed weapons get drawn this way
			return;
		}

		if (this.UIService.GetBitmap(item2H) is null)
			return;

		Point screenLocation;
		if (item.IsInWeaponSlot)
		{
			screenLocation = WeaponTopLeft(
				SackCollection.GetWeaponLocationOffset(item.PositionY).X,
				SackCollection.GetWeaponLocationOffset(item.PositionY).Y,
				item2H.Width,
				item2H.Height);
		}
		else
			screenLocation = this.CellTopLeft(item.Location);

		this.DrawItem(g, item2H, screenLocation, DimmedImage);
	}
}
