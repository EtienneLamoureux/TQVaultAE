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
using TQVaultAE.Application.KeyBinds;
using TQVaultAE.Config;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Components.Rendering;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Forms;
using TQVaultAE.GUI.Infrastructure;
using TQVaultAE.GUI.Models;
using TQVaultAE.GUI.Tooltips;
using TQVaultAE.Presentation;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.GUI.Components.Panels;

public partial class SackPanel
{
	/// <summary>The keys of the panel, in the order they are tried (<see cref="KeyDownCallback"/>).</summary>
	private static readonly KeyBindAction[] PanelKeyActions =
	[
		KeyBindAction.CancelDrag,
		KeyBindAction.ItemCopy,
		KeyBindAction.ItemDuplicate,
		KeyBindAction.ItemDelete,
		KeyBindAction.SelectAll,
		KeyBindAction.SelectHighlighted,
		KeyBindAction.ClearSelection,
		KeyBindAction.MoveSelectionUp,
		KeyBindAction.MoveSelectionLeft,
		KeyBindAction.MoveSelectionDown,
		KeyBindAction.MoveSelectionRight,
		KeyBindAction.MoveToPlayer,
		KeyBindAction.MoveToVault,
		KeyBindAction.MoveToStash,
		KeyBindAction.RefreshPanel,
	];

#if DEBUG
	/// <summary>The modifiers a Debug automation tool holds for its click, in place of the keyboard's.</summary>
	internal Keys? AutomationModifiers;

	/// <summary>The modifier keys held now.</summary>
	private Keys CurrentModifiers => this.AutomationModifiers ?? ModifierKeys;
#else
	/// <summary>The modifier keys held now.</summary>
	private static Keys CurrentModifiers => ModifierKeys;
#endif

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

		// The copy and duplicate clicks act like their keys, by the context menu's rules: a copy changes nothing in a
		// read-only sack.
		if (e.Button == MouseButtons.Left && !this.DragInfo.IsActive && this.RunItemClickShortcut())
			return;

		// A read-only character's sack: nothing is picked up and the user is told; a drop there is refused by the drop
		// rules (TryToPutdownItem), which tell why as well.
		if (e.Button == MouseButtons.Left && !this.DragInfo.IsActive && this.ItemsReadOnly)
			this.NotifyIfReadOnlyCharacter(this.Sack);
		else if (e.Button == MouseButtons.Left && (!this.ItemsReadOnly || this.Sack.IsReadOnly))
		{
			if (!this.DragInfo.IsActive)
			{
				var click = KeyBindInput.Click(CurrentModifiers);
				if (this.Services.KeyBinds.Matches(KeyBindAction.SelectItemClick, click))
					// Add the focused item to the selected items list.
					this.AddFocusedItemToSelectedItems(sender, e);
				// Detect a mouse drag for a multiselect.
				else if (this.Services.KeyBinds.Matches(KeyBindAction.SelectRectangle, click))
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
	/// Copy or Duplicate of the clicked item by its click shortcut (<see cref="KeyBindAction.ItemCopyClick"/>,
	/// <see cref="KeyBindAction.ItemDuplicateClick"/>): the copy is picked up, as with the keys.
	/// </summary>
	/// <returns>true when the click ran one; false lets the click do its usual work.</returns>
	private bool RunItemClickShortcut()
	{
		var requested = this.Services.KeyBinds.Find(KeyBindInput.Click(CurrentModifiers), KeyBindAction.ItemCopyClick, KeyBindAction.ItemDuplicateClick) switch
		{
			KeyBindAction.ItemCopyClick => ItemActions.Copy,
			KeyBindAction.ItemDuplicateClick => ItemActions.Duplicate,
			_ => ItemActions.None,
		};

		return requested != ItemActions.None && this.RunItemShortcut(requested);
	}

	/// <summary>
	/// Runs Copy, Duplicate or Delete on the item under the cursor, by the context menu's rules (nothing is deleted from
	/// the equipment of a read-only player). Not while an item is dragged: a copy would replace the dragged item.
	/// </summary>
	/// <returns>true when the action ran.</returns>
	private bool RunItemShortcut(ItemActions requested)
	{
		if (this.Sack is null || this.DragInfo.IsActive)
			return false;

		Item? focusedItem = this.FindItem(this.LastCellWithFocus);
		var action = ItemActionRules.KeyAction(requested, this.ActionContext(focusedItem, 0, false));
		if (action == ItemActions.None)
			return false;

		this.RunFocusedItemAction(action, focusedItem!);
		this.HideTooltipsAfterKey();
		return true;
	}

	/// <summary>
	/// Callback for key down: runs the action of the panel bound to the key (<see cref="PanelKeyActions"/>). The keys of
	/// the window (search, zoom, clipboard, forge) are read by the main window first.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">KeyEventArgs data</param>
	private void KeyDownCallback(object? sender, KeyEventArgs e)
	{
		if (this.Services.KeyBinds.Find(KeyBindInput.FromKeys(e.KeyData), PanelKeyActions) is not { } action)
			return;

		bool handled = true;
		switch (action)
		{
			case KeyBindAction.CancelDrag:
				handled = this.CancelDragOrCompare();
				break;
			case KeyBindAction.ItemCopy:
				handled = this.RunItemShortcut(ItemActions.Copy);
				break;
			case KeyBindAction.ItemDuplicate:
				handled = this.RunItemShortcut(ItemActions.Duplicate);
				break;
			case KeyBindAction.ItemDelete:
				handled = this.RunItemShortcut(ItemActions.Delete);
				break;
			case KeyBindAction.SelectAll:
				this.SelectAllItems();
				break;
			case KeyBindAction.SelectHighlighted:
				this.SelectAllHighlightedItems();
				break;
			case KeyBindAction.ClearSelection:
				this.OnClearAllItemsSelected?.Invoke(this, new SackPanelEventArgs(null, null));
				break;
			case KeyBindAction.MoveSelectionUp:
				this.MoveSelectedItemsInSack("up");
				break;
			case KeyBindAction.MoveSelectionLeft:
				this.MoveSelectedItemsInSack("left");
				break;
			case KeyBindAction.MoveSelectionDown:
				this.MoveSelectedItemsInSack("down");
				break;
			case KeyBindAction.MoveSelectionRight:
				this.MoveSelectedItemsInSack("right");
				break;
			case KeyBindAction.MoveToPlayer:
				QuickMovePanel(AutoMoveLocation.Player);
				break;
			case KeyBindAction.MoveToVault:
				QuickMovePanel(AutoMoveLocation.Vault);
				break;
			case KeyBindAction.MoveToStash:
				QuickMovePanel(AutoMoveLocation.Stash);
				break;
			case KeyBindAction.RefreshPanel:
				this.Refresh();
				break;
		}

		if (handled)
		{
			e.Handled = true;
			e.SuppressKeyPress = true;
		}
	}

	/// <summary>Cancels the compare selection, else the drag of an item that can go back.</summary>
	/// <returns>true when something was cancelled.</returns>
	private bool CancelDragOrCompare()
	{
		if (this.Sack is null)
			return false;

		bool cancelled = false;
		if (!this.DragInfo.IsActive && this.DragInfo.Compare.Cancel())
			cancelled = true;
		else if (this.DragInfo.IsActive && this.DragInfo.CanBeCanceled)
		{
			this.DragInfo.Cancel();

			// Now redraw this sack
			this.Refresh();
			cancelled = true;
		}

		this.HideTooltipsAfterKey();
		return cancelled;
	}

	/// <summary>
	/// Callback for key press: a character typed over the panel hides the item tooltip. The keys bound to an action are
	/// run at key down.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">KeyPressEventArgs data</param>
	private void KeyPressCallback(object? sender, KeyPressEventArgs e) => this.HideTooltipsAfterKey();

	/// <summary>Hides the item tooltip and drops the bag tooltip of the sack, which a key may have changed.</summary>
	private void HideTooltipsAfterKey()
	{
		if (this.Sack is null)
			return;

		ItemTooltip.HideTooltip();
		BagButtonTooltip.InvalidateCache(this.Sack);
	}

	private void MouseEnterCallback(object? sender, EventArgs e) => this.Select();

	private void MouseLeaveCallback(object? sender, EventArgs e)
	{
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
