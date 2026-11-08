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
		var (key, grid) = this.GridByCategory.FirstOrDefault(kv => kv.Key.ToString().StartsWith(category, StringComparison.OrdinalIgnoreCase));
		if (grid is null)
			throw new InvalidOperationException($"No grid '{category}'. Grids: {string.Join(", ", this.GridByCategory.Keys)}");

		var cells = (List<IconGrid.Cell>)ShownCells.GetValue(grid)!;
		if (index < 0 || index >= cells.Count)
			throw new InvalidOperationException($"Icon {index} out of range 0..{cells.Count - 1} in {key}.");

		var cell = cells[index];
		this.Grid_MouseDown(grid, MouseAt(grid, cell));
		return cell.Info.Off?.Raw ?? cell.Id.Raw;

		static MouseEventArgs MouseAt(IconGrid grid, IconGrid.Cell cell)
		{
			var bounds = grid.BoundsOf(cell);
			return new MouseEventArgs(MouseButtons.Right, 1, bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2, 0);
		}
	}
}
