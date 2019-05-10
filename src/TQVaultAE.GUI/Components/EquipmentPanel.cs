//-----------------------------------------------------------------------
// <copyright file="EquipmentPanel.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace TQVaultAE.GUI.Components
{
	using Properties;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Windows.Forms;
	using TQVaultAE.GUI.Models;
	using TQVaultData;

	/// <summary>
	/// Class for holding all of the UI functions of the sack panel in the stash panel.
	/// </summary>
	public class EquipmentPanel : SackPanel
	{
		/// <summary>
		/// Initializes a new instance of the EquipmentPanel class.
		/// </summary>
		/// <param name="sackWidth">Width of the sack panel in cells</param>
		/// <param name="sackHeight">Height of the sack panel in cells</param>
		/// <param name="dragInfo">ItemDragInfo instance.</param>
		/// <param name="autoMoveLocation">AutoMoveLocation for this panel.</param>
		public EquipmentPanel(int sackWidth, int sackHeight, ItemDragInfo dragInfo, AutoMoveLocation autoMoveLocation)
			: base(sackWidth, sackHeight, dragInfo, autoMoveLocation)
		{
			this.SackType = SackType.Equipment;
			this.BackColor = Color.Transparent;
			this.DisableGrid = true;
			this.DisableBorder = true;
			this.DisableMultipleSelection = true;

			// Change the highlight color to green.
			this.HighlightUnselectedItemBrush = new SolidBrush(Color.FromArgb(23, 149, 15));
		}

		#region EquipmentPanel Public Methods

		/// <summary>
		/// Cancels an item drag.  Takes into account the shadow slot for the weapon panel.
		/// </summary>
		/// <param name="dragInfo">ItemDragInfo instance</param>
		public override void CancelDrag(ItemDragInfo dragInfo)
		{
			base.CancelDrag(dragInfo);

			// Check to see if we need to restore the shadow slot.
			if (this.Sack == dragInfo.Sack && dragInfo.Item.Is2HWeapon)
			{
				Graphics graphics = this.CreateGraphics();
				try
				{
					Brush backgroundBrush = this.CellHasItemBrush;

					if (this.IsItemSelected(dragInfo.Item))
					{
						backgroundBrush = this.HighlightSelectedItemBrush;
					}

					Item new2HItem = this.GetItemFromShadowSlot(dragInfo.Item.PositionY);

					if (new2HItem.BaseItemId.Length == 0)
					{
						this.DrawItemShaded(graphics, new2HItem, backgroundBrush);
					}
				}
				catch (NullReferenceException exception)
				{
					MessageBox.Show(exception.ToString(), Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, RightToLeftOptions);
				}
			}
		}

		#endregion EquipmentPanel Public Methods

		#region EquipmentPanel Protected Methods

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
		/// Gets an item at a cell location
		/// </summary>
		/// <param name="cellLocation">Point of the cell location we are interested in</param>
		/// <returns>Item at that cell location</returns>
		protected override Item FindItem(Point cellLocation)
		{
			if (this.Sack == null)
			{
				return null;
			}

			// Find the item for this point
			int itemSlot = -1;
			foreach (Item item in this.Sack)
			{
				++itemSlot;
				if (item == this.DragInfo.Item)
				{
					// hide the item being dragged
					continue;
				}

				// store the x and y values
				int x = item.PositionX;
				int y = item.PositionY;

				if (item.IsInWeaponSlot)
				{
					// Check if it's an item in the weapon box
					if (IsInsideWeaponBox(cellLocation, y))
					{
						// Verify that we are over the weapon box
						// If we are over an empty weapon box we need to check if there is a 2 Handed weapon
						// in the corresponding weapon box and if there is we return that instead.
						// Otherwise we just skip the empty item.
						if (item.BaseItemId.Length == 0)
						{
							Item item2H = this.GetItemFromShadowSlot(itemSlot - 7);
							/*if (itemSlot == 7 || itemSlot == 9)
                            {
                                item2H = this.Sack.GetItem(itemSlot + 1);
                            }
                            else if (itemSlot == 8 || itemSlot == 10)
                            {
                                item2H = this.Sack.GetItem(itemSlot - 1);
                            }*/

							if (item2H.BaseItemId.Length != 0 && item2H.Is2HWeapon)
							{
								// Make sure we got something and it's a 2 Handed weapon
								return item2H;
							}
							else
							{
								// Otherwise just skip it
								continue;
							}
						}
						else
						{
							// There is really an item here so we just return it.
							return item;
						}
					}
					else
					{
						// Just skip the item if we are not over a weapon box.
						continue;
					}
				}
				else if (string.IsNullOrEmpty(item.BaseItemId))
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
		/// Gets the bottom cell of an item.  Not scaled to screen coordinates.
		/// </summary>
		/// <param name="item">Item which we are checking</param>
		/// <returns>Y value of the bottom cell of the item.</returns>
		protected override int CellBottom(Item item)
		{
			if (item.PositionX == Item.WeaponSlotIndicator)
			{
				// If we are over an equipment panel then we skip to the next column
				// since the x, y coordinates embedded in the item are a special case
				return SackCollection.WeaponLocationSize.Height + SackCollection.GetWeaponLocationOffset(item.PositionY).Y - 1;
			}

			return item.Height + item.PositionY - 1;
		}

		/// <summary>
		/// Find all items that are at least partially within the cell rectangle
		/// </summary>
		/// <param name="cellRectangle">cell rectangle we are looking at</param>
		/// <returns>Array of items within the rectangle</returns>
		/*protected override Item[] FindAllItems(Rectangle cellRectangle)
        {
            List<Item> items = new List<Item>();
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

                        if (item.PositionX == Item.WeaponSlotIndicator)
                        {
                            // If we are over an equipment panel then we skip to the next column
                            // since the x, y coordinates embedded in the item are a special case
                            y = SackCollection.WeaponLocationSize.Height + SackCollection.GetWeaponLocationOffset(item.PositionY).Y - 1;
                        }
                        else
                        {
                            // Speed things up by skipping past the remaining height of the item
                            y = y + item.Height - 1 - (y - item.PositionY);
                        }
                    }
                }
            }

            return items.ToArray<Item>();
        }*/

		/// <summary>
		/// Calculates the mouse offset in screen coordinates within an item bitmap.
		/// </summary>
		/// <param name="location">Point containing the screen coordinates of the mouse.</param>
		/// <param name="item">Item that we are over</param>
		/// <returns>Point containing the mouse offset.</returns>
		protected override Point GetMouseOffset(Point location, Item item)
		{
			if (item == null)
			{
				return Point.Empty;
			}

			Point topLeft = this.CellTopLeft(item.Location);

			// Adjust for weapon panel items
			if (item.IsInWeaponSlot)
			{
				int itemOffset = item.PositionY;
				if (item.Is2HWeapon)
				{
					// We need to adjust, just in case we clicked on the shadow item of a 2H item.
					int focusSlot = FindEquipmentSlot(this.FindCell(location));
					int itemSlot = FindEquipmentSlot(this.Sack, item);
					if (focusSlot != itemSlot)
					{
						itemOffset = SackCollection.GetEquipmentLocationOffset(focusSlot).Y;
					}
				}

				topLeft = WeaponTopLeft(
					SackCollection.GetWeaponLocationOffset(itemOffset).X,
					SackCollection.GetWeaponLocationOffset(itemOffset).Y,
					item.Width,
					item.Height);
			}

			return new Point(location.X - topLeft.X, location.Y - topLeft.Y);
		}

		/// <summary>
		/// Handler for putting an item down with the mouse
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">MouseEventArgs data</param>
		protected override void TryToPutdownItem(object sender, MouseEventArgs e)
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

				// Check that the item is being dropped into the proper slot.
				int oldSlot = -1;
				int slot = FindEquipmentSlot(this.CellsUnderDragItem);
				if (slot == -1 || !this.CheckItemType(dragItem, slot))
				{
					return;
				}

				if (this.DragInfo.Sack != null)
				{
					oldSlot = FindEquipmentSlot(this.DragInfo.Sack, dragItem);
				}

				// Yes we can drop it here!
				// First take the item that is under us
				Item itemUnderUs = null;
				if (this.ItemsUnderDragItem != null && this.ItemsUnderDragItem.Count == 1)
				{
					itemUnderUs = this.ItemsUnderDragItem[0];
				}

				// Maybe we are putting the item back or dropping on an equipment shadow slot.
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
				this.DragInfo.MarkPlaced(oldSlot);

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
					int adjustedNumber;

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
					adjustedNumber = itemUnderUs.Number - originalNumber;
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
					if (slot != -1)
					{
						int slotRH = ((slot + 1) / 2) * 2;
						if (dragItem.Is2HWeapon)
						{
							// The check function in the beginning ensures that 1 slot is open for 2H weapons.
							// Find the RH weapon slot.
							if (slot != slotRH)
							{
								// If I am not dropping on the RH slot then we may need to pick up the item in the LH slot.
								if (itemUnderUs != null && itemUnderUs.BaseItemId.Length != 0)
								{
									if (itemUnderUs.Is2HWeapon)
									{
										// For 2H weapons itemUnderUs points to the real item in the RH slot.
										this.Sack.AddItem(this.Sack.GetItem(slotRH).Duplicate(true));
									}
									else
									{
										this.Sack.AddItem(this.Sack.GetItem(slot).Duplicate(true));
									}

									itemUnderUs = this.Sack.GetItem(this.Sack.Count - 1);

									// Clear the item in the LH slot.
									// Put a dummy item in it's place
									Item newItem = dragItem.MakeEmptyItem();
									newItem.Location = SackCollection.GetEquipmentLocationOffset(slot);
									this.Sack.RemoveAtItem(slot);
									this.Sack.InsertItem(slot, newItem);
								}
								else if (!string.IsNullOrEmpty(this.Sack.GetItem(slotRH).BaseItemId))
								{
									// There might be something in the other slot so we pick it up.
									// Move the original item to the end of the sack temporarily
									this.Sack.AddItem(this.Sack.GetItem(slotRH).Duplicate(true));
									itemUnderUs = this.Sack.GetItem(this.Sack.Count - 1);

									// Clear the item in the RH slot and put a dummy item in it's place
									Item newItem = dragItem.MakeEmptyItem();
									newItem.Location = SackCollection.GetEquipmentLocationOffset(slotRH);
									this.Sack.RemoveAtItem(slotRH);
									this.Sack.InsertItem(slotRH, newItem);
								}

								slot = slotRH;
							}
							else
							{
								// We are over the RH slot, but there may be something in the LH slot.
								int slotLH = slotRH - 1;
								if (itemUnderUs != null && !string.IsNullOrEmpty(itemUnderUs.BaseItemId))
								{
									this.Sack.AddItem(this.Sack.GetItem(slot).Duplicate(true));
									itemUnderUs = this.Sack.GetItem(this.Sack.Count - 1);

									// Clear the item in the RH slot.
									// Put a dummy item in it's place
									Item newItem = dragItem.MakeEmptyItem();
									newItem.Location = SackCollection.GetEquipmentLocationOffset(slot);
									this.Sack.RemoveAtItem(slot);
									this.Sack.InsertItem(slot, newItem);
								}
								else if (!string.IsNullOrEmpty(this.Sack.GetItem(slotLH).BaseItemId))
								{
									// There might be something in the other slot so we pick it up.
									// Move the original item to the end of the sack temporarily
									this.Sack.AddItem(this.Sack.GetItem(slotLH).Duplicate(true));
									itemUnderUs = this.Sack.GetItem(this.Sack.Count - 1);

									// Clear the item in the LH slot and put a dummy item in it's place
									Item newItem = dragItem.MakeEmptyItem();
									newItem.Location = SackCollection.GetEquipmentLocationOffset(slotLH);
									this.Sack.RemoveAtItem(slotLH);
									this.Sack.InsertItem(slotLH, newItem);
								}
							}
						}

						// Check to see if we dropped a 1H weapon onto a 2H weapon shadow slot
						// Assume that the real 2H weapon is in the right hand slot.
						if (IsWeaponSlot(slot) && slot != slotRH)
						{
							if (this.Sack.GetItem(slotRH).Is2HWeapon)
							{
								this.Sack.AddItem(this.Sack.GetItem(slotRH).Duplicate(true));
								itemUnderUs = this.Sack.GetItem(this.Sack.Count - 1);

								// Clear the item in the LH slot and put a dummy item in it's place
								Item newItem = dragItem.MakeEmptyItem();
								newItem.Location = SackCollection.GetEquipmentLocationOffset(slotRH);
								this.Sack.RemoveAtItem(slotRH);
								this.Sack.InsertItem(slotRH, newItem);
							}
						}

						dragItem.Location = SackCollection.GetEquipmentLocationOffset(slot);
						this.Sack.RemoveAtItem(slot);
						this.Sack.InsertItem(slot, dragItem);
					}
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
		/// Changes the background of the item under the mouse so that it appears highlighted
		/// </summary>
		/// <param name="cell">Point containing the cell that the mouse is over</param>
		protected override void HighlightItemUnderMouse(Point cell)
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
					if (lastItem != null && lastItem.Is2HWeapon)
					{
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

						Item last2HItem = this.GetItemFromShadowSlot(lastItem.PositionY);
						if (last2HItem.BaseItemId.Length == 0)
						{
							this.DrawItemShaded(g, last2HItem, backgroundBrush);
						}
					}

					if (newItem != lastItem)
					{
						if (newItem != null && newItem.Is2HWeapon)
						{
							// Now we need to highlight the current item
							// Check if the item is selected and use a different background
							if (this.IsItemSelected(newItem))
							{
								backgroundBrush = this.HighlightSelectedItemBrush;
							}
							else
							{
								backgroundBrush = this.HighlightUnselectedItemBrush;
							}

							Item new2HItem = this.GetItemFromShadowSlot(newItem.PositionY);
							if (string.IsNullOrEmpty(new2HItem.BaseItemId))
							{
								this.DrawItemShaded(g, new2HItem, backgroundBrush);
							}
						}

						// Call the base method to update the last cell and to draw the items.
						base.HighlightItemUnderMouse(cell);
					}
				}
			}
		}

		/// <summary>
		/// Find the rectanglular area of cell coords affected by these screen coords
		/// </summary>
		/// <param name="screenRectangle">Rectangle with the screen coordinates which will be converted to cell coordinates.</param>
		/// <returns>Rectangle containing the cells within the screen area</returns>
		protected override Rectangle FindAllCells(Rectangle screenRectangle)
		{
			Rectangle cellRect = base.FindAllCells(screenRectangle);

			// Check to see if the weapon boxes need to be included
			for (int i = 0; i < SackCollection.NumberOfWeaponSlots; ++i)
			{
				Rectangle weaponRect = new Rectangle(SackCollection.GetWeaponLocationOffset(i), SackCollection.WeaponLocationSize);
				if (weaponRect.IntersectsWith(cellRect))
				{
					cellRect = Rectangle.Union(cellRect, weaponRect);
					break;
				}
				else
				{
					continue;
				}
			}

			return cellRect;
		}

		/// <summary>
		/// Gets a rectangle of the screen coordinates for all of the items that need to be redrawn from an item drag.
		/// </summary>
		/// <returns>Rectangle of the redraw area.</returns>
		protected override Rectangle GetRepaintDragRect()
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

				// Find the slot which is under the drag item
				Rectangle rect = this.FindAllEquipmentCells(cellRectangle);
				if (rect != null)
				{
					Point rectUL = this.CellTopLeft(rect.Location);
					Point rectBR = this.CellBottomRight(new Point(rect.Right - 1, rect.Bottom - 1));

					// Extend our clipping rectangle to include all these cells
					int oldX1 = x;
					int oldY1 = y;
					x = Math.Min(x, rectUL.X);
					y = Math.Min(y, rectUL.Y);
					width = Math.Max(oldX1 + width - 1, rectBR.X) - x + 1;
					height = Math.Max(oldY1 + height - 1, rectBR.Y) - y + 1;
				}
			}

			// We also need to wipe out any items that were under or are now under the dragItem so they can be redrawn
			if (this.ItemsUnderOldDragLocation != null)
			{
				foreach (Item item in this.ItemsUnderOldDragLocation)
				{
					// well we do not actually need to clear the item and redraw it since that will happen automatically
					// in the PaintCallback().  We just need to include this item in the clip region
					Point tl = this.CellTopLeft(item.Location);
					Point br = this.CellBottomRight(new Point(item.Right - 1, item.Bottom - 1));

					if (item.IsInWeaponSlot)
					{
						int itemOffsetR, itemOffsetL;
						if (item.Is2HWeapon)
						{
							// We need to redraw both items
							itemOffsetL = (item.PositionY / 2) * 2;
							itemOffsetR = ((item.PositionY / 2) * 2) + 1;
						}
						else
						{
							// Only include the item under the box
							itemOffsetL = item.PositionY;
							itemOffsetR = item.PositionY;
						}

						// Get the left most item
						Point itemL = SackCollection.GetWeaponLocationOffset(itemOffsetL);

						// Now get the right most item and create a rectangle to cover both
						Point itemR = SackCollection.GetWeaponLocationOffset(itemOffsetR);

						// we do not need the weapon box drawing offsets here e.g. WeaponTopLeft()
						tl = this.CellTopLeft(itemL);
						br = this.CellBottomRight(new Point(itemR.X + SackCollection.WeaponLocationSize.Width - 1, itemR.Y + SackCollection.WeaponLocationSize.Height - 1));
					}

					// Extend our clipping rectangle to include all these cells
					int oldX = x;
					int oldY = y;
					x = Math.Min(x, tl.X);
					y = Math.Min(y, tl.Y);
					width = Math.Max(oldX + width - 1, br.X) - x + 1;
					height = Math.Max(oldY + height - 1, br.Y) - y + 1;
				}
			}

			if (this.ItemsUnderDragItem != null)
			{
				foreach (Item item in this.ItemsUnderDragItem)
				{
					// well we do not actually need to clear the item and redraw it since that will happen automatically
					// in the PaintCallback().  We just need to include this item in the clip region
					Point tl = this.CellTopLeft(item.Location);
					Point br = this.CellBottomRight(new Point(item.Right - 1, item.Bottom - 1));

					if (item.IsInWeaponSlot)
					{
						int itemOffsetR, itemOffsetL;
						if (item.Is2HWeapon)
						{
							// We need to redraw both items
							itemOffsetL = (item.PositionY / 2) * 2;
							itemOffsetR = ((item.PositionY / 2) * 2) + 1;
						}
						else
						{
							// Only include the item under the box
							itemOffsetL = item.PositionY;
							itemOffsetR = item.PositionY;
						}

						// Get the left most item
						Point itemL = SackCollection.GetWeaponLocationOffset(itemOffsetL);

						// Now get the right most item and create a rectangle to cover both
						Point itemR = SackCollection.GetWeaponLocationOffset(itemOffsetR);

						// we do not need the weapon box drawing offsets here e.g. WeaponTopLeft()
						tl = this.CellTopLeft(itemL);
						br = this.CellBottomRight(new Point(itemR.X + SackCollection.WeaponLocationSize.Width - 1, itemR.Y + SackCollection.WeaponLocationSize.Height - 1));
					}

					// Extend our clipping rectangle to include all these cells
					int oldX = x;
					int oldY = y;
					x = Math.Min(x, tl.X);
					y = Math.Min(y, tl.Y);
					width = Math.Max(oldX + width - 1, br.X) - x + 1;
					height = Math.Max(oldY + height - 1, br.Y) - y + 1;
				}
			}

			return new Rectangle(x, y, width, height);
		}

		/// <summary>
		/// Draws the background of the items in the panel during a Paint call.
		/// </summary>
		/// <param name="e">PaintEventArgs data</param>
		protected override void PaintAreaUnderItem(PaintEventArgs e)
		{
			Item lastItem = this.FindItem(this.LastCellWithFocus);
			foreach (Item item in this.Sack)
			{
				if (item != this.DragInfo.Item)
				{
					// do not draw the item being dragged.
					// Figure out the background brush to use.
					Brush backgroundBrush = null;

					// Check if the item is selected and use a different background
					if (this.IsItemSelected(item))
					{
						backgroundBrush = this.HighlightSelectedItemBrush;
					}
					else
					{
						backgroundBrush = this.CellHasItemBrush;
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
								int slot = FindEquipmentSlot(this.Sack, this.ItemsUnderDragItem[0]);
								if (slot != -1)
								{
									if (this.CheckItemType(this.DragInfo.Item, slot))
									{
										backgroundBrush = this.HighlightValidItemBrush;
									}
									else
									{
										backgroundBrush = this.HighlightInvalidItemBrush;
									}
								}
								else
								{
									backgroundBrush = this.HighlightInvalidItemBrush;
								}
							}
						}
						else
						{
							backgroundBrush = this.CellHasItemBrush;
						}
					}
					else if (item == lastItem)
					{
						backgroundBrush = this.HighlightValidItemBrush;
					}

					// Now do the shading
					this.ShadeAreaUnderItem(e.Graphics, item, backgroundBrush);

					// Check to see if we need to shade a 2H weapon slot
					if (item.IsInWeaponSlot && item.Is2HWeapon)
					{
						Item item2H = this.GetItemFromShadowSlot(item.PositionY);
						if (string.IsNullOrEmpty(item2H.BaseItemId))
						{
							Rectangle rect = this.FindSlotRect(item2H.PositionX, item2H.PositionY, backgroundBrush == null);
							this.ShadeAreaUnderItem(e.Graphics, rect, backgroundBrush);
						}
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
			if (this.DragInfo.IsActive && !this.CellsUnderDragItem.Size.IsEmpty && (this.ItemsUnderDragItem == null ||
				this.ItemsUnderDragItem.Count <= 1))
			{
				Point tl = Point.Empty;
				Point br = Point.Empty;
				Brush backgroundBrush = null;
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
					{
						slot = -1;
					}

					// Check to see if the item is correct for the slot
					if (slot != -1)
					{
						if (this.CheckItemType(this.DragInfo.Item, slot))
						{
							backgroundBrush = this.HighlightValidItemBrush;
						}
						else
						{
							backgroundBrush = this.HighlightInvalidItemBrush;
						}

						e.Graphics.FillRectangle(backgroundBrush, tl.X, tl.Y, br.X - tl.X + 1, br.Y - tl.Y + 1);

						if (IsWeaponSlot(slot))
						{
							// Draw additional weapon box area
							e.Graphics.FillRectangle(
								backgroundBrush,
								tl.X + Database.DB.HalfUnitSize,
								tl.Y - Database.DB.HalfUnitSize,
								Database.DB.ItemUnitSize,
								5 * Database.DB.ItemUnitSize);
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
			foreach (Item item in this.Sack)
			{
				if (item != this.DragInfo.Item)
				{
					// do not draw the item being dragged.
					if (item.IsInWeaponSlot)
					{
						// Weapon slots only
						if (item.BaseItemId.Length == 0)
						{
							// If the item is null try to draw it grayed out
							this.DrawItemShaded(e.Graphics, item, new SolidBrush(Color.Transparent));
						}
						else
						{
							// Otherwise draw it normally
							this.DrawItem(e.Graphics, item, new SolidBrush(Color.Transparent));
						}
					}
					else
					if (item.BaseItemId.Length != 0)
					{
						this.DrawItem(e.Graphics, item, new SolidBrush(Color.Transparent));
					}
				}
				else
				{
					// item is being dragged
				}
			}
		}

		/// <summary>
		/// Shades the background of an item.
		/// </summary>
		/// <param name="graphics">graphics instance</param>
		/// <param name="item">Item to be shaded</param>
		/// <param name="backgroundBrush">brush we are using to paint the background.</param>
		protected override void ShadeAreaUnderItem(Graphics graphics, Item item, Brush backgroundBrush)
		{
			Rectangle rect = this.FindSlotRect(item.PositionX, item.PositionY, backgroundBrush == null);
			this.ShadeAreaUnderItem(graphics, rect, backgroundBrush);
		}

		/// <summary>
		/// Shades the background of an item.
		/// </summary>
		/// <param name="graphics">graphics instance</param>
		/// <param name="backgroundRectangle">cell rectangle that needs to be drawn</param>
		/// <param name="backgroundBrush">brush we are using to paint the background.</param>
		protected override void ShadeAreaUnderItem(Graphics graphics, Rectangle backgroundRectangle, Brush backgroundBrush)
		{
			// Do the normal shading.
			base.ShadeAreaUnderItem(graphics, backgroundRectangle, backgroundBrush);

			// Fill the center 1x5 strip in the weapon boxes.
			if (IsWeaponSlot(FindEquipmentSlot(FindCell(backgroundRectangle.Location))) && backgroundBrush != null)
			{
				graphics.FillRectangle(
					backgroundBrush,
					backgroundRectangle.X + Database.DB.HalfUnitSize,
					backgroundRectangle.Y - Database.DB.HalfUnitSize,
					Database.DB.ItemUnitSize,
					Database.DB.ItemUnitSize * 5);
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
		protected override void DrawItem(Graphics graphics, Item item, Brush backgroundBrush)
		{
			// First draw the background for all the cells this item occupies
			this.ShadeAreaUnderItem(graphics, item, backgroundBrush);

			// Draw the item
			if (item.ItemBitmap != null)
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
				{
					screenLocation = this.CellTopLeft(item.Location);
				}

				this.DrawItem(graphics, item, screenLocation);
			}
		}

		#endregion EquipmentPanel Protected Methods

		#region EquipmentPanel Private Methods

		/// <summary>
		/// Returns whether the equipment slot is a weapon slot
		/// </summary>
		/// <param name="slot">equipment slot we are checking</param>
		/// <returns>True if it is an equipment slot.</returns>
		private static bool IsWeaponSlot(int slot)
		{
			return slot > 6 && slot < 11;
		}

		/// <summary>
		/// Finds the equipment slot
		/// </summary>
		/// <param name="sack">current sack instance</param>
		/// <param name="item">item we are checking</param>
		/// <returns>slot number if the item is in an equipment slot otherwise -1</returns>
		private static int FindEquipmentSlot(SackCollection sack, Item item)
		{
			if (sack == null || sack.SackType != SackType.Equipment || item == null)
			{
				// Make sure we are on the equipment panel
				return -1;
			}

			// Iterate through the equipment slots
			for (int slot = 0; slot < sack.NumberOfSlots; ++slot)
			{
				int itemX;
				int itemY;

				if (item.IsInWeaponSlot)
				{
					// adjust for weapon panel
					itemX = SackCollection.GetWeaponLocationOffset(item.PositionY).X;
					itemY = SackCollection.GetWeaponLocationOffset(item.PositionY).Y;
				}
				else
				{
					itemX = item.PositionX;
					itemY = item.PositionY;
				}

				Point upperLeft = SackCollection.GetEquipmentLocationOffset(slot);
				Size size = SackCollection.GetEquipmentLocationSize(slot);
				if (SackCollection.IsWeaponSlot(slot))
				{
					// Adjust for the equipment slots
					size = SackCollection.WeaponLocationSize;
					upperLeft = SackCollection.GetWeaponLocationOffset(upperLeft.Y);
				}

				if (itemX < upperLeft.X || itemX > (upperLeft.X + size.Width - 1) || itemY < upperLeft.Y || itemY > (upperLeft.Y + size.Height - 1))
				{
					continue;
				}
				else
				{
					// We found it
					return slot;
				}
			}

			return -1;
		}

		/// <summary>
		/// Returns whether the cell is within a specific weapon box.
		/// </summary>
		/// <param name="cellLocation">location we are checking</param>
		/// <param name="weaponSlot">weapon slot we are checking</param>
		/// <returns>true if the cells are within the specified weapon slot</returns>
		private static bool IsInsideWeaponBox(Point cellLocation, int weaponSlot)
		{
			// adjust it to use the real position
			Point slotUpperLeft = SackCollection.GetWeaponLocationOffset(weaponSlot);

			if ((slotUpperLeft.X <= cellLocation.X) && ((slotUpperLeft.X + SackCollection.WeaponLocationSize.Width - 1) >= cellLocation.X) &&
				(slotUpperLeft.Y <= cellLocation.Y) && ((slotUpperLeft.Y + SackCollection.WeaponLocationSize.Height - 1) >= cellLocation.Y))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Finds the top left corner of a weapon withn a weapon slot
		/// Used to center the weapon inside the weapon panel
		/// </summary>
		/// <param name="x">x cell coordinate of the weapon</param>
		/// <param name="y">y cell coordinate of the weapon</param>
		/// <param name="width">width of the weapon</param>
		/// <param name="height">height of the weapon</param>
		/// <returns>Point containing the top left corner of the weapon</returns>
		private static Point WeaponTopLeft(int x, int y, int width, int height)
		{
			Point val = new Point();
			int offsetX = ((SackCollection.WeaponLocationSize.Width - width) * Database.DB.ItemUnitSize) / 2;
			int offsetY = ((SackCollection.WeaponLocationSize.Height - height) * Database.DB.ItemUnitSize) / 2;
			val.X = Convert.ToInt32(BorderWidth) + (x * Database.DB.ItemUnitSize) + offsetX;
			val.Y = Convert.ToInt32(BorderWidth) + (y * Database.DB.ItemUnitSize) + offsetY;

			return val;
		}

		/// <summary>
		/// Finds the bottom right corner of a weapon within the weapon slot
		/// Used for drawing text in weapon panel
		/// </summary>
		/// <param name="x">x cell coordinate of the weapon</param>
		/// <param name="y">y cell coordinate of the weapon</param>
		/// <param name="width">width of the weapon</param>
		/// <param name="height">height of the weapon</param>
		/// <returns>Point containing the bottom right corner of the weapon</returns>
		private static Point WeaponBottomLeft(int x, int y, int width, int height)
		{
			Point val = new Point();
			int offsetX = ((SackCollection.WeaponLocationSize.Width - width) * Database.DB.ItemUnitSize) / 2;
			int offsetY = ((SackCollection.WeaponLocationSize.Height - height) * Database.DB.ItemUnitSize) / 2;
			val.X = Convert.ToInt32(BorderWidth) + (x * Database.DB.ItemUnitSize) + offsetX;
			val.Y = Convert.ToInt32(BorderWidth) + ((y + 1) * Database.DB.ItemUnitSize) + offsetY - 1;

			return val;
		}

		/// <summary>
		/// Used find the screen coordinates of an equipment slot
		/// </summary>
		/// <param name="slot">equipment or weapon slot we are interested in</param>
		/// <returns>Rectangle containing the coordinates of the equipment slot</returns>
		private Rectangle FindSlotRect(int slot)
		{
			if (slot == -1 || slot > this.Sack.NumberOfSlots)
			{
				return Rectangle.Empty;
			}

			bool weaponSlot = false;
			int offset = 0;

			Point slotUL = SackCollection.GetEquipmentLocationOffset(slot);
			Size slotSize = SackCollection.GetEquipmentLocationSize(slot);

			// Check for weapon slot
			if (SackCollection.IsWeaponSlot(slot))
			{
				int slotOffset = slotUL.Y;
				slotSize = SackCollection.WeaponLocationSize;
				slotUL = SackCollection.GetWeaponLocationOffset(slotOffset);
				weaponSlot = true;
			}

			Point ul = this.CellTopLeft(slotUL);

			// Adjust the weapon slot for graphic
			if (weaponSlot)
			{
				ul.Y += Database.DB.HalfUnitSize;
				offset = 1;
			}

			return new Rectangle(
				ul.X,
				ul.Y,
				slotSize.Width * Database.DB.ItemUnitSize,
				(slotSize.Height - offset) * Database.DB.ItemUnitSize);
		}

		/// <summary>
		/// Gets the item instance for a specified weapon shadow slot.
		/// </summary>
		/// <param name="weaponSlot">weapon slot that we are checking</param>
		/// <returns>Base item instance</returns>
		private Item GetItemFromShadowSlot(int weaponSlot)
		{
			// Convert from weapon slot (0-3) to equipment slot (7-10).
			int offset = weaponSlot + 7;

			// Make sure it is valid.
			if (!IsWeaponSlot(offset))
			{
				return null;
			}

			// Find the corresponding slot in the pair.  Should be either 7/8 or 9/10.
			if ((offset % 2) == 1)
			{
				offset++;
			}
			else
			{
				offset--;
			}

			return this.Sack.GetItem(offset);
		}

		/// <summary>
		/// Gets the bounding rectangle for given cell.
		/// </summary>
		/// <param name="x">X coordinate of the cell to be checked.</param>
		/// <param name="y">Y coordinate of the cell to be checked.</param>
		/// <param name="useImage">Specifies whether we are using an image or a colored fill for the background.</param>
		/// <returns>Scaled rectangle for the weapon slot.</returns>
		private Rectangle FindSlotRect(int x, int y, bool useImage)
		{
			int width;
			int height;
			Point screenLocation;

			if (x == Item.WeaponSlotIndicator)
			{
				// See if it is a weapon
				// Adjust for special placement of weapons in the equipment panel
				width = SackCollection.GetEquipmentLocationSize(y + 7).Width;
				height = SackCollection.GetEquipmentLocationSize(y + 7).Height;
				if (!useImage)
				{
					// Make the bounding rectagle a little smaller
					// because of the additional drawing for the 1x5 center piece.
					// We use the larger size to restore the background image.
					height--;
				}

				int itemX = SackCollection.GetWeaponLocationOffset(y).X;
				int itemY = SackCollection.GetWeaponLocationOffset(y).Y;

				screenLocation = WeaponTopLeft(itemX, itemY, width, height);
			}
			else
			{
				Point cellLocation = new Point(x, y);
				screenLocation = this.CellTopLeft(cellLocation);
				int slot = FindEquipmentSlot(cellLocation);
				width = SackCollection.GetEquipmentLocationSize(slot).Width;
				height = SackCollection.GetEquipmentLocationSize(slot).Height;
			}

			return new Rectangle(screenLocation.X, screenLocation.Y, width * Database.DB.ItemUnitSize, height * Database.DB.ItemUnitSize);
		}

		/// <summary>
		/// Draw the item grayed out.  Used for 2 handed items in weapon slots
		/// </summary>
		/// <param name="g">graphics instance</param>
		/// <param name="item">item we are drawing</param>
		/// <param name="backgroundBrush">brush we are using for the background</param>
		private void DrawItemShaded(Graphics g, Item item, Brush backgroundBrush)
		{
			if (item.BaseItemId.Length != 0)
			{
				// The item is not null
				// So we draw it normally and exit
				this.DrawItem(g, item, backgroundBrush);
				return;
			}

			Item item2H = this.GetItemFromShadowSlot(item.PositionY);

			// TODO: Update logic to draw the shadow slot if the item was cancelled.
			// otherwise it is skipped because this.m_dragInfo.Item == item2H will be true.
			if (!item2H.Is2HWeapon || (this.DragInfo.Item == item2H && !this.DragInfo.IsBeingCanceled) || item2H.BaseItemId.Length == 0)
			{
				// Skip if the item is being dragged.
				// Also, only 2 Handed weapons get drawn this way
				return;
			}

			// First draw the background for all the cells this item occupies
			Rectangle rect = this.FindSlotRect(item.PositionX, item.PositionY, backgroundBrush == null);
			this.ShadeAreaUnderItem(g, rect, backgroundBrush);

			// Set the color matrix so that the item is dimmed
			System.Drawing.Imaging.ColorMatrix colorMatrix = new System.Drawing.Imaging.ColorMatrix();
			colorMatrix.Matrix00 = 0.80f; // Red
			colorMatrix.Matrix11 = 0.80f; // Green
			colorMatrix.Matrix22 = 0.80f; // Blue
			colorMatrix.Matrix33 = 0.60f; // alpha
			colorMatrix.Matrix44 = 1.00f; // w

			System.Drawing.Imaging.ImageAttributes imgAttr = new System.Drawing.Imaging.ImageAttributes();
			imgAttr.SetColorMatrix(colorMatrix);

			// Draw the item
			if (item2H.ItemBitmap != null)
			{
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
				{
					screenLocation = this.CellTopLeft(item.Location);
				}

				this.DrawItem(g, item2H, screenLocation, imgAttr);
			}
		}

		/// <summary>
		/// Used to find the equipment slot under the mouse based on cell location
		/// </summary>
		/// <param name="cell">cell we are searching</param>
		/// <returns>-1 if nothing found, otherwise the slot location in the equiment sack is returned</returns>
		private int FindEquipmentSlot(Point cell)
		{
			if (cell == null)
			{
				// Make sure we are on the equipment panel
				return -1;
			}

			// Iterate through the equipment slots
			for (int slot = 0; slot < this.Sack.NumberOfSlots; ++slot)
			{
				Size size = SackCollection.GetEquipmentLocationSize(slot);
				Point ul = SackCollection.GetEquipmentLocationOffset(slot);

				if (SackCollection.IsWeaponSlot(slot))
				{
					// Adjust for the equipment slots
					size = SackCollection.WeaponLocationSize;
					ul = SackCollection.GetWeaponLocationOffset(ul.Y);
				}

				if (cell.X < ul.X || cell.X > (ul.X + size.Width - 1) || cell.Y < ul.Y || cell.Y > (ul.Y + size.Height - 1))
				{
					continue;
				}
				else
				{
					// We found it
					return slot;
				}
			}

			return -1;
		}

		/// <summary>
		/// Used to find the equipment slot under the mouse based on the drag rectangle
		/// </summary>
		/// <param name="cellsUnderMouse">Rectangle containing all the cells under the mouse pointer</param>
		/// <returns>-1 if nothing found, otherwise the slot location in the equiment sack is returned</returns>
		private int FindEquipmentSlot(Rectangle cellsUnderMouse)
		{
			if (cellsUnderMouse == null)
			{
				// Make sure we are on the equipment panel
				return -1;
			}

			// Iterate through the equipment slots
			Point slotUL;
			Size slotSize;
			Rectangle slotRect;
			for (int slot = 0; slot < this.Sack.NumberOfSlots; ++slot)
			{
				slotUL = SackCollection.GetEquipmentLocationOffset(slot);
				if (SackCollection.IsWeaponSlot(slot))
				{
					slotUL = SackCollection.GetWeaponLocationOffset(slotUL.Y);
					slotSize = SackCollection.WeaponLocationSize;
				}
				else
				{
					slotSize = SackCollection.GetEquipmentLocationSize(slot);
				}

				slotRect = new Rectangle(slotUL, slotSize);
				if (cellsUnderMouse.IntersectsWith(slotRect))
				{
					// Found it, so we return the slot number.
					return slot;
				}
			}

			// We didn't find it so we return -1
			return -1;
		}

		/// <summary>
		/// Used to determine all slots under the current mouse cells and then creates a rectangle to encompass all of those cells.
		/// </summary>
		/// <param name="cellsUnderMouse">Rectangle containing all of the cells under the mouse</param>
		/// <returns>Rectangle of cells including all of the equipment slots under the mouse</returns>
		private Rectangle FindAllEquipmentCells(Rectangle cellsUnderMouse)
		{
			if (cellsUnderMouse == null)
			{
				// Make sure we are on the equipment panel
				return Rectangle.Empty;
			}

			// Iterate through the equipment slots
			Point slotUL, slotBR;
			Size slotSize;
			Rectangle slotRect;

			// Start with the cells Under the mouse
			int x = cellsUnderMouse.X;
			int y = cellsUnderMouse.Y;
			int width = cellsUnderMouse.Width;
			int height = cellsUnderMouse.Height;
			bool foundCells = false;

			for (int slot = 0; slot < this.Sack.NumberOfSlots; ++slot)
			{
				slotUL = SackCollection.GetEquipmentLocationOffset(slot);
				if (SackCollection.IsWeaponSlot(slot))
				{
					slotUL = SackCollection.GetWeaponLocationOffset(slotUL.Y);
					slotSize = SackCollection.WeaponLocationSize;
				}
				else
				{
					slotSize = SackCollection.GetEquipmentLocationSize(slot);
				}

				slotBR = new Point(slotUL.X + slotSize.Width - 1, slotUL.Y + slotSize.Height - 1);
				slotRect = new Rectangle(slotUL, slotSize);

				if (cellsUnderMouse.IntersectsWith(slotRect))
				{
					// Check to see if it's a 2H weapon over a weapon slot.
					if (this.DragInfo.Item.Is2HWeapon && (slot > 6 && slot < 11))
					{
						// We need to redraw both items
						int itemOffsetL = (((slot - 1) / 2) * 2) - 6;
						int itemOffsetR = (((slot - 1) / 2) * 2) - 5;
						slotUL = SackCollection.GetWeaponLocationOffset(itemOffsetL);
						slotBR = new Point(
							SackCollection.GetWeaponLocationOffset(itemOffsetR).X + SackCollection.WeaponLocationSize.Width - 1,
							SackCollection.GetWeaponLocationOffset(itemOffsetR).Y + SackCollection.WeaponLocationSize.Height - 1);
					}

					// Found it, so we expand our rectangle.
					foundCells = true;
					int oldX = x;
					int oldY = y;
					x = Math.Min(x, slotUL.X);
					y = Math.Min(y, slotUL.Y);
					width = Math.Max(oldX + width - 1, slotBR.X - slotUL.X + 1) - x + 1;
					height = Math.Max(oldY + height - 1, slotBR.Y - slotUL.Y + 1) - y + 1;
				}
			}

			// We didn't find it so we return a null
			if (foundCells)
			{
				return new Rectangle(x, y, width, height);
			}
			else
			{
				return Rectangle.Empty;
			}
		}

		/// <summary>
		/// Used to check that the item is appropriate for the weapon slot
		/// </summary>
		/// <param name="item">Item we are checking</param>
		/// <param name="slot">slot that we are trying to put the item into</param>
		/// <returns>true if the item is appropriate for the slot</returns>
		private bool CheckItemType(Item item, int slot)
		{
			if (item == null || slot < 0 || slot > this.Sack.NumberOfSlots)
			{
				return false;
			}

			switch (slot)
			{
				case 0: // Head
					return item.IsHelm;

				case 1: // Neck
					return item.IsAmulet;

				case 2: // Body
					return item.IsTorsoArmor;

				case 3: // Legs
					return item.IsGreave;

				case 4: // Arms
					return item.IsBracer;

				case 5: // Ring1
				case 6: // Ring2
					return item.IsRing;

				case 7: // Weapon1
				case 8: // Shield1
				case 9: // Weapon2
				case 10: // Shield2
					if (item.IsWeaponShield)
					{
						if (item.Is2HWeapon)
						{
							// For 2H Weapons we need to check both weapon slots to make sure at least one is free
							int itemOffset = slot;
							if (((slot / 2) * 2) == slot)
							{
								itemOffset--;
							}
							else
							{
								itemOffset++;
							}

							return this.Sack.GetItem(itemOffset).BaseItemId.Length == 0 || this.Sack.GetItem(slot).BaseItemId.Length == 0;
						}
						else
						{
							return true;
						}
					}

					break;

				case 11: // Artifact
					return item.IsArtifact;

				default:
					return false;
			}

			return false;
		}

		#endregion EquipmentPanel Private Methods
	}
}