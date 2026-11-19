using System.Reflection;
using TQVaultAE.GUI.Automation;
using TQVaultAE.GUI.Components.Icons;

namespace TQVaultAE.GUI.Dialogs;

/// <summary>Debug access to the bag icon picker: icon drag and magnifier.</summary>
public partial class BagButtonSettings
{
	/// <summary>
	/// Drags the icon at <paramref name="index"/> of a tab's grid onto a picture box like the user: left button down on
	/// the icon (mouse messages to the grid), the virtual cursor moved onto the target's middle, button up sent to the
	/// grid, which has the mouse capture; the grid's handler finds the target under the cursor.
	/// </summary>
	/// <param name="category">Grid category, as its tab (Skills, Relics, Artifacts...).</param>
	/// <param name="target">on, off, over (detailed pictures) or simple (simple picture).</param>
	/// <returns>The record id of the icon dragged.</returns>
	internal string AutomationDragIcon(string category, int index, string target)
	{
		var box = target.Trim().ToLowerInvariant() switch
		{
			"on" => this.groupBoxOnBitmap,
			"off" => this.groupBoxOffBitmap,
			"over" => this.groupBoxOverBitmap,
			"simple" => this.groupBoxSimpleImage,
			_ => throw new AutomationRefusedException("target must be on, off, over or simple."),
		};
		if (!box.Visible)
			throw new AutomationRefusedException($"The '{target}' picture box is hidden in this mode (Simple or Detailed): a user could not drop on it.");

		var (grid, cell) = this.AutomationCell(category, index);
		var bounds = grid.BoundsOf(cell);
		var start = AutomationInput.Center(bounds);
		var end = grid.PointToClient(box.PointToScreen(Automation.AutomationInput.Center(box)));
		Automation.AutomationInput.SendButton(grid, start, MouseButtons.Left, down: true);
		Automation.VirtualCursor.Set(box, Automation.AutomationInput.Center(box));
		try
		{
			Automation.AutomationInput.SendButton(grid, end, MouseButtons.Left, down: false);
		}
		finally
		{
			Automation.VirtualCursor.Release();
		}

		return cell.Id.Raw;
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
}
