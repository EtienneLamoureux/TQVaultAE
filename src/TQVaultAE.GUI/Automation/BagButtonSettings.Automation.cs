//-----------------------------------------------------------------------
// <copyright file="BagButtonSettings.Automation.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.Reflection;
using TQVaultAE.GUI.Components.Icons;

namespace TQVaultAE.GUI.Dialogs;

/// <summary>
/// Debug-only access to the icon grids of the bag button settings for the automation endpoint.
/// </summary>
public partial class BagButtonSettings
{
	private static readonly FieldInfo ShownCells
		= typeof(IconGrid).GetField("_shown", BindingFlags.Instance | BindingFlags.NonPublic)!;

	/// <summary>
	/// Right-clicks the icon at <paramref name="index"/> of a tab's grid through the grid's own handler, which picks
	/// its On, Off and Over pictures (the predefined selection).
	/// </summary>
	/// <param name="category">Grid category, as its tab (Skills, Relics, Artifacts...).</param>
	/// <returns>The record id of the icon's Off picture.</returns>
	internal string AutomationPickIcon(string category, int index)
	{
		var (grid, cell) = this.AutomationCell(category, index);
		var bounds = grid.BoundsOf(cell);
		this.Grid_MouseDown(grid, new MouseEventArgs(MouseButtons.Right, 1, bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2, 0));
		return cell.Info.Off?.Raw ?? cell.Id.Raw;
	}

	/// <summary>
	/// Rests the pointer on the icon at <paramref name="index"/> of a tab's grid through the grid's hover handler, which
	/// shows the magnifier below it; a null index leaves the grid, which hides it.
	/// </summary>
	/// <returns>The record id of the hovered icon, or null.</returns>
	internal string? AutomationHoverIcon(string category, int? index)
	{
		if (index is null)
		{
			this.Grid_HoverChanged(this.AutomationGrid(category).Grid, null);
			return null;
		}

		var (grid, cell) = this.AutomationCell(category, index.Value);
		this.Grid_HoverChanged(grid, cell);
		return cell.Id.Raw;
	}

	private (string Key, IconGrid Grid) AutomationGrid(string category)
	{
		var (key, grid) = this.GridByCategory.FirstOrDefault(kv => kv.Key.ToString().StartsWith(category, StringComparison.OrdinalIgnoreCase));
		if (grid is null)
			throw new InvalidOperationException($"No grid '{category}'. Grids: {string.Join(", ", this.GridByCategory.Keys)}");
		return (key.ToString(), grid);
	}

	private (IconGrid Grid, IconGrid.Cell Cell) AutomationCell(string category, int index)
	{
		var (key, grid) = this.AutomationGrid(category);
		var cells = (List<IconGrid.Cell>)ShownCells.GetValue(grid)!;
		if (index < 0 || index >= cells.Count)
			throw new InvalidOperationException($"Icon {index} out of range 0..{cells.Count - 1} in {key}.");
		return (grid, cells[index]);
	}
}
