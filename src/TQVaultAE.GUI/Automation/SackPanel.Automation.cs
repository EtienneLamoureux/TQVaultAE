//-----------------------------------------------------------------------
// <copyright file="SackPanel.Automation.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Automation;
using TQVaultAE.GUI.Models;

namespace TQVaultAE.GUI.Components.Panels;

/// <summary>
/// Debug-only entry points used by the automation endpoint.
/// They feed the panel's own mouse handlers with synthetic events and a <see cref="VirtualCursor"/>,
/// so the regular pick / drag / drop logic runs without touching the real mouse.
/// </summary>
public partial class SackPanel
{
	internal ItemDragInfo AutomationDragInfo => this.DragInfo;

	/// <summary>
	/// Gets the cells the dragged item would be dropped into, as computed by the last paint.
	/// </summary>
	internal Rectangle AutomationCellsUnderDragItem => this.CellsUnderDragItem;

	internal Item? AutomationFindItem(Point cell) => this.FindItem(cell);

	/// <summary>
	/// Converts a cell coordinate into the client coordinate of the cell's top left corner.
	/// </summary>
	internal Point AutomationCellTopLeft(Point cell) => this.CellTopLeft(cell);

	/// <summary>
	/// Converts a cell coordinate into the client coordinate of the cell's center.
	/// </summary>
	internal Point AutomationCellCenter(Point cell)
	{
		var topLeft = this.CellTopLeft(cell);
		return new Point(topLeft.X + this.UIService.HalfUnitSize, topLeft.Y + this.UIService.HalfUnitSize);
	}

	/// <summary>
	/// Moves the virtual cursor over this panel and runs the regular mouse move logic,
	/// then paints synchronously so that the drop target cells are up to date.
	/// </summary>
	internal void AutomationMouseMove(Point clientLocation)
	{
		this.AutomationEnter(clientLocation);
		this.MouseMoveCallback(this, new MouseEventArgs(MouseButtons.None, 0, clientLocation.X, clientLocation.Y, 0));
		this.AutomationPaintNow();
	}

	/// <summary>
	/// Simulates a mouse button press at a client location.
	/// </summary>
	internal void AutomationMouseDown(Point clientLocation, MouseButtons button)
	{
		this.AutomationMouseMove(clientLocation);
		var args = new MouseEventArgs(button, 1, clientLocation.X, clientLocation.Y, 0);
		this.MouseDownCallback(this, args);
		this.MouseUpCallback(this, args);
		this.AutomationPaintNow();
	}

	/// <summary>
	/// Right-clicks a cell through the regular mouse handlers and draws the context menu they built, with the sub menus
	/// opened along <paramref name="open"/>; the menu is closed after.
	/// </summary>
	internal Bitmap AutomationRenderContextMenu(Point cell, IReadOnlyList<string> open)
	{
		this.AutomationMouseDown(this.AutomationCellCenter(cell), MouseButtons.Right);
		return AutomationMenus.Render(this.CustomContextMenu, open);
	}

	/// <summary>
	/// Right-clicks a cell through the regular mouse handlers, reads the context menu they built, then closes it.
	/// </summary>
	/// <returns>The menu entries, <c>-</c> for separators, with the entries of their sub menus.</returns>
	internal IReadOnlyList<AutomationMenuEntry> AutomationContextMenu(Point cell)
	{
		this.AutomationMouseDown(this.AutomationCellCenter(cell), MouseButtons.Right);
		try
		{
			return AutomationMenus.Describe(this.CustomContextMenu.Items);
		}
		finally
		{
			this.CustomContextMenu.Close();
		}
	}

	/// <summary>
	/// Right-clicks a cell through the regular mouse handlers, closes the menu they built and then, after the calling
	/// tool has returned (an entry may open a modal dialog), clicks the entry found by <paramref name="path"/>.
	/// </summary>
	/// <param name="path">Entry texts from the top level down, for example <c>["Move To", "Bag 2"]</c>.</param>
	/// <returns>The text of the entry clicked.</returns>
	internal string AutomationClickContextMenu(Point cell, IReadOnlyList<string> path)
	{
		this.AutomationMouseDown(this.AutomationCellCenter(cell), MouseButtons.Right);
		try
		{
			return AutomationMenus.ClickLater(this, this.CustomContextMenu, path);
		}
		finally
		{
			this.CustomContextMenu.Close();
		}
	}

	/// <summary>
	/// Rests the virtual cursor on a cell, then presses a key like the keyboard (<see cref="AutomationInput.PostKey"/>):
	/// to the focused window, handled after the calling tool has returned (Backspace asks for a confirmation).
	/// </summary>
	/// <returns>The control the key was sent to.</returns>
	internal Control AutomationKeyDown(Point cell, Keys keyData, Forms.MainForm form)
	{
		this.AutomationMouseMove(this.AutomationCellCenter(cell));
		return AutomationInput.PostKey(form, keyData);
	}

	/// <summary>
	/// Left-clicks a cell like a user holding <paramref name="modifiers"/> (Ctrl adds the item to the selection,
	/// Ctrl+Alt duplicates it by default), through the regular mouse handler, the modifiers held in the keyboard state.
	/// </summary>
	internal void AutomationModifierClick(Point cell, Keys modifiers)
	{
		using var held = AutomationInput.HoldModifiers(modifiers);
		this.AutomationMouseDown(this.AutomationCellCenter(cell), MouseButtons.Left);
	}

	/// <summary>
	/// Presses the left button on a cell with <paramref name="modifiers"/> held, moves the mouse to another cell and
	/// releases it there, through the regular mouse handlers: the rectangle selection (Shift by default).
	/// </summary>
	internal void AutomationDragSelect(Point from, Point to, Keys modifiers)
	{
		using var held = AutomationInput.HoldModifiers(modifiers);
		var start = this.AutomationCellCenter(from);
		var end = this.AutomationCellCenter(to);
		this.AutomationMouseMove(start);
		this.MouseDownCallback(this, new MouseEventArgs(MouseButtons.Left, 1, start.X, start.Y, 0));
		VirtualCursor.Set(this, end);
		this.MouseMoveCallback(this, new MouseEventArgs(MouseButtons.Left, 0, end.X, end.Y, 0));
		this.MouseUpCallback(this, new MouseEventArgs(MouseButtons.Left, 1, end.X, end.Y, 0));
		this.AutomationPaintNow();
	}

	/// <summary>
	/// Ctrl-clicks a cell like a user holding Ctrl, which adds the item there to the selection (or removes it).
	/// </summary>
	internal void AutomationCtrlClick(Point cell) => this.AutomationModifierClick(cell, Keys.Control);

	/// <summary>Closes the context menu a right click may have opened.</summary>
	internal void AutomationCloseContextMenu() => this.CustomContextMenu.Close();

	/// <summary>The items selected in this panel, in selection order.</summary>
	internal IReadOnlyList<Item> AutomationSelectedItems => this.selectedItems ?? [];

	/// <summary>
	/// Simulates the virtual cursor leaving this panel.
	/// </summary>
	internal void AutomationMouseLeave()
	{
		this.MouseLeaveCallback(this, EventArgs.Empty);
		this.Invalidate();
	}

	/// <summary>
	/// Runs the paint logic now, even when the window is minimized or hidden behind other windows.
	/// </summary>
	internal void AutomationPaintNow()
	{
		var form = this.FindForm();
		if (this.Visible && form is not null && form.WindowState != FormWindowState.Minimized)
		{
			this.Refresh();
			return;
		}

		if (this.Width <= 0 || this.Height <= 0)
			return;

		// WM_PRINT goes through the same paint callback and refreshes the drag state.
		using var scratch = new Bitmap(this.Width, this.Height);
		this.DrawToBitmap(scratch, new Rectangle(Point.Empty, this.Size));
	}

	private void AutomationEnter(Point clientLocation)
	{
		if (VirtualCursor.Target is SackPanel previous && !ReferenceEquals(previous, this))
		{
			VirtualCursor.Set(this, clientLocation);
			previous.AutomationMouseLeave();
			previous.AutomationPaintNow();
			this.OnMouseEnter(EventArgs.Empty);
			return;
		}

		bool entering = !ReferenceEquals(VirtualCursor.Target, this);
		VirtualCursor.Set(this, clientLocation);

		// The mouse entering a panel raises its MouseEnter event, as Windows Forms does at the first mouse move.
		if (entering)
			this.OnMouseEnter(EventArgs.Empty);
	}

	/// <summary>
	/// Right-clicks a cell like the user, closes the menu, then runs the item menu's "Share to File..." without its file dialog:
	/// the selection when there is one, else the item under the cursor.
	/// </summary>
	/// <returns><see langword="false"/> when there was nothing to export.</returns>
	internal bool AutomationExportItemsToFile(Point cell, string path)
	{
		this.AutomationContextMenu(cell);
		return this.ExportItemsToFile(path);
	}
}
