//-----------------------------------------------------------------------
// <copyright file="SackPanel.Input.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Extensions.Logging;
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
using TQVaultAE.Presentation;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.GUI.Components.Panels;

public partial class SackPanel
{
	protected virtual void MouseUpCallback(object? sender, MouseEventArgs e)
	{
		this.mouseDraw = false;
		Invalidate();
	}

	/// <summary>
	/// Handler for mouse button down click
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">MouseEventArgs data</param>
	protected virtual void MouseDownCallback(object? sender, MouseEventArgs e)
	{
		if (this.Sack is null)
			return;

		// The compare selection takes the click, read-only equipment included: comparing only reads the items.
		if (this.CompareSelectionMouseDown(e))
			return;

		if (e.Button == MouseButtons.Left && !this.ItemsReadOnly)
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
					this.OnClearAllItemsSelected?.Invoke(this, new SackPanelEventArgs(null, null));
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
			if (this.DragInfo.IsActive && this.DragInfo.CanBeCanceled && !this.ItemsReadOnly)
			{
				this.DragInfo.Cancel();

				// Now redraw this sack
				this.Refresh();
			}
			else
			{
				Item? focusedItem = this.FindItem(this.LastCellWithFocus);

				// The affixes and bonuses offered for an item linked to a mod come from that mod's data.
				using var modScope = this.Services.ItemModService.Scope(focusedItem);
				int selectionCount = this.selectedItems?.Count ?? 0;
				bool singleSelectionFocused = selectionCount == 1 && focusedItem == this.selectedItems![0];

				var actions = ItemActionRules.Available(this.ActionContext(focusedItem, selectionCount, singleSelectionFocused));

				this.CustomContextMenu.Items.Clear();

				if (focusedItem is not null)
					this.contextMenuCellWithFocus = this.LastCellWithFocus;

				if (actions.HasFlag(ItemActions.Delete))
				{
					this.AddActionMenuItem(Resources.SackPanelMenuDelete, ItemActions.Delete);
					this.CustomContextMenu.Items.Add("-");
				}

				if (actions.HasFlag(ItemActions.RemoveRelic))
					this.AddActionMenuItem(Resources.SackPanelMenuRemoveRelic, ItemActions.RemoveRelic);

				if (actions.HasFlag(ItemActions.RemoveRelic2))
					this.AddActionMenuItem(Resources.SackPanelMenuRemoveRelic2, ItemActions.RemoveRelic2);

				if (actions.HasFlag(ItemActions.Split))
					this.AddActionMenuItem(Resources.SackPanelMenuSplit, ItemActions.Split);

				if (actions.HasFlag(ItemActions.Copy))
					this.AddActionMenuItem(Resources.SackPanelMenuCopy, ItemActions.Copy);

				if (actions.HasFlag(ItemActions.Duplicate))
					this.AddActionMenuItem(Resources.SackPanelMenuDuplicate, ItemActions.Duplicate);

				AddExportMenuItems(focusedItem, singleSelectionFocused);

				if (actions.HasFlag(ItemActions.MoveTo))
					AddAutoMoveMenuItems(focusedItem);

				if (actions.HasFlag(ItemActions.Edit))
				{
					// Edit implies a single focused item.
					AddRegularItemEditMenuItems(focusedItem!);
					AddPrefixSuffixMenuItems(focusedItem!);
					AddSocketedItemCompletionBonusMenuItems(focusedItem!);
					AddRelicOrArticaftCompletionBonusMenuItems(focusedItem!);
					AddItemSetMenuItems(focusedItem!);
				}

				if (actions.HasFlag(ItemActions.ChangeMod))
					this.AddModMenuItems(focusedItem!);

				if (actions.HasFlag(ItemActions.Properties))
				{
					this.CustomContextMenu.Items.Add("-");
					this.AddActionMenuItem(Resources.SackPanelMenuProperties, ItemActions.Properties);
				}

				if (actions.HasFlag(ItemActions.Compare))
				{
					this.CustomContextMenu.Items.Add("-");
					this.AddActionMenuItem(Resources.SackPanelMenuCompare, ItemActions.Compare);
				}

				if (actions.HasFlag(ItemActions.ClearSelection))
					this.AddActionMenuItem(Resources.SackPanelMenuClear, ItemActions.ClearSelection);

				if ((focusedItem is not null || selectionCount > 0) && this.CustomContextMenu.Items.Count > 0)
					this.CustomContextMenu.Show(this, new Point(e.X, e.Y));
			}
		}
	}

	/// <summary>
	/// What the item actions of this sack may do, for the context menu and the keys: the equipment of a read-only
	/// player cannot change.
	/// </summary>
	private ItemActionContext ActionContext(Item? focusedItem, int selectionCount, bool singleSelectionFocused)
		=> new(
			focusedItem
			, selectionCount
			, singleSelectionFocused
			, ReadOnly: this.ItemsReadOnly
			, USettings.AllowItemEdit
			, USettings.AllowItemCopy);

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
			Item? lastItem = this.FindItem(this.LastCellWithFocus);
			Item? newItem = this.FindItem(cell);

			if (newItem != lastItem || redrawSelection)
			{
				if (lastItem is not null)
					this.Invalidate(GetItemScreenRectangle(lastItem));

				if (cell != this.LastCellWithFocus)
					this.LastCellWithFocus = cell;

				if (newItem != lastItem)
				{
					if (newItem is not null)
						this.Invalidate(GetItemScreenRectangle(newItem));

					this.OnNewItemHighlighted?.Invoke(this, new SackPanelEventArgs(this.Sack, newItem));
				}
			}
		}
	}

	/// <summary>
	/// Callback for moving the mouse
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">MouseEventArgs data</param>
	protected void MouseMoveCallback(object? sender, MouseEventArgs e)
	{
		if (this.Sack is null)
			return;

		if (!this.DragInfo.IsActive)
		{
			if (this.mouseDraw)
			{
				this.OnClearAllItemsSelected?.Invoke(this, new SackPanelEventArgs(null, null));
				currentPosition = e.Location;
				this.SelectItemsInMouseDraw(sender, e);
				this.Invalidate();
			}
			else
				this.HighlightItemUnderMouse(this.FindCell(e.Location));
		}
		else if (this.RepaintsDragAreaOnly)
		{
			// Only the area of the drag, not the whole sack at each mouse move: where the item, the cells and the items
			// under it were, and where they are now. The target is updated first, so the paint shades the current cells.
			Rectangle before = this.GetRepaintDragRect();
			this.UpdateDragTarget(new Point(e.Location.X - this.DragInfo.MouseOffset.X, e.Location.Y - this.DragInfo.MouseOffset.Y));
			this.Invalidate(Rectangle.Union(before, this.GetRepaintDragRect()));
		}
		else
			this.Invalidate();
	}

	/// <summary>
	/// Whether a drag over this panel repaints only the area of the dragged item (<see cref="GetRepaintDragRect"/>);
	/// otherwise the whole panel.
	/// </summary>
	protected virtual bool RepaintsDragAreaOnly => true;

	/// <summary>
	/// Callback for key down
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">KeyEventArgs data</param>
	private void KeyDownCallback(object? sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.ControlKey)
			this.controlKeyDown = true;

		if (e.KeyCode == Keys.ShiftKey)
			this.shiftKeyDown = true;

		if (e.KeyData == (Keys.Control | Keys.F))
			this.OnActivateSearch?.Invoke(this, new SackPanelEventArgs(null, null));

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
			this.OnClearAllItemsSelected?.Invoke(this, new SackPanelEventArgs(null, null));

		if (e.KeyData == (Keys.Control | Keys.Add) || e.KeyData == (Keys.Control | Keys.Oemplus))
			this.OnResizeForm?.Invoke(this, new ResizeEventArgs(0.1F));

		if (e.KeyData == (Keys.Control | Keys.Subtract) || e.KeyData == (Keys.Control | Keys.OemMinus))
			this.OnResizeForm?.Invoke(this, new ResizeEventArgs(-0.1F));

		if (e.KeyData == (Keys.Control | Keys.Home))
			this.OnResizeForm?.Invoke(this, new ResizeEventArgs(1.0F));

		if (e.KeyData == Keys.F5)
			this.Refresh();

		if (e.KeyData == Keys.Right)
			QuickMovePanel(AutoMoveLocation.Player);

		if (e.KeyData == Keys.Left)
			QuickMovePanel(AutoMoveLocation.Vault);

		if (e.KeyData == Keys.Down)
			QuickMovePanel(AutoMoveLocation.Stash);
	}

	/// <summary>
	/// Callback for key up
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">KeyEventArgs data</param>
	private void KeyUpCallback(object? sender, KeyEventArgs e)
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
	private void KeyPressCallback(object? sender, KeyPressEventArgs e)
	{
		if (this.Sack is not null)
		{
			if (e.KeyChar == (char)27 && !this.DragInfo.IsActive && this.DragInfo.Compare.Cancel())
				e.Handled = true;
			else if (e.KeyChar == (char)27 && this.DragInfo.IsActive && this.DragInfo.CanBeCanceled)
			{
				this.DragInfo.Cancel();

				// Now redraw this sack
				this.Refresh();
				e.Handled = true;
			}
			else if (!this.DragInfo.IsActive)
			{
				// c copies, d duplicates, Backspace deletes the item under the cursor, by the context menu's rules
				// (nothing is deleted from the equipment of a read-only player). Not while an item is dragged: it
				// would replace the dragged item.
				Item? focusedItem = this.FindItem(this.LastCellWithFocus);
				var action = ItemActionRules.KeyAction(e.KeyChar, this.ActionContext(focusedItem, 0, false));
				if (action != ItemActions.None)
				{
					this.RunFocusedItemAction(action, focusedItem!);
					e.Handled = true;
				}
			}

			ItemTooltip.HideTooltip();
			BagButtonTooltip.InvalidateCache(this.Sack);
		}
	}

	private void MouseEnterCallback(object? sender, EventArgs e) => this.Select();

	private void MouseLeaveCallback(object? sender, EventArgs e)
	{
		this.controlKeyDown = false;
		if (this.Sack is null)
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
				Log.LogError(exception, "Failed to reset the drag target when the mouse left sack panel {Panel}", this.Name);
				this.Services.MessageBoxes.ShowException(exception);
			}
		}
	}
}
