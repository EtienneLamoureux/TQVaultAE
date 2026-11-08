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
/// <summary>A context menu entry read by <see cref="SackPanel.AutomationContextMenu"/>.</summary>
internal sealed record AutomationMenuEntry(string Text, IReadOnlyList<AutomationMenuEntry>? Items, bool? Enabled = null, bool? Bold = null);

/// <summary>Reads and clicks the context menus built by the panels.</summary>
internal static class AutomationMenus
{
	/// <summary>The entries, <c>-</c> for separators, with the entries of their sub menus.</summary>
	internal static IReadOnlyList<AutomationMenuEntry> Describe(ToolStripItemCollection items)
		=> items.Cast<ToolStripItem>()
			.Select(i => new AutomationMenuEntry(
				i is ToolStripSeparator ? "-" : i.Text ?? string.Empty
				, i is ToolStripMenuItem { HasDropDownItems: true } menu ? Describe(menu.DropDownItems) : null
				, i.Enabled ? null : false
				, i is ToolStripMenuItem { Font.Bold: true } ? true : null))
			.ToList();

	/// <summary>
	/// Finds the entry of <paramref name="menu"/> by its path of texts (exact text first, else the first containing it)
	/// and clicks it once the calling tool has returned, through <paramref name="owner"/>'s message loop.
	/// </summary>
	/// <returns>The text of the entry to be clicked.</returns>
	internal static string ClickLater(Control owner, ToolStrip menu, IReadOnlyList<string> path)
	{
		ToolStripItemCollection items = menu.Items;
		ToolStripItem? entry = null;
		foreach (var text in path)
		{
			entry = items.Cast<ToolStripItem>().FirstOrDefault(i => string.Equals(i.Text, text, StringComparison.OrdinalIgnoreCase))
				?? items.Cast<ToolStripItem>().FirstOrDefault(i => i.Text?.Contains(text, StringComparison.OrdinalIgnoreCase) == true)
				?? throw new InvalidOperationException($"No entry '{text}'. Entries: {string.Join(" | ", items.Cast<ToolStripItem>().Select(i => i.Text))}");
			items = (entry as ToolStripMenuItem)?.DropDownItems ?? new ToolStripItemCollection(menu, []);
		}

		if (entry is null)
			throw new InvalidOperationException("Empty menu path.");
		if (!entry.Enabled)
			throw new InvalidOperationException($"'{entry.Text}' is disabled: a user could not click it.");
		if (entry is ToolStripMenuItem { HasDropDownItems: true } parent)
			throw new InvalidOperationException($"'{entry.Text}' opens a sub menu: add one of its entries to the path ({string.Join(" | ", parent.DropDownItems.Cast<ToolStripItem>().Select(i => i.Text))}).");

		var clicked = entry;
		owner.BeginInvoke(clicked.PerformClick);
		return clicked.Text ?? string.Empty;
	}
}

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
	/// Ctrl-clicks a cell like a user holding Ctrl, which adds the item there to the selection (or removes it).
	/// </summary>
	internal void AutomationCtrlClick(Point cell)
	{
		this.controlKeyDown = true;
		try
		{
			this.AutomationMouseDown(this.AutomationCellCenter(cell), MouseButtons.Left);
		}
		finally
		{
			this.controlKeyDown = false;
		}
	}

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
			return;
		}

		VirtualCursor.Set(this, clientLocation);
	}
}
