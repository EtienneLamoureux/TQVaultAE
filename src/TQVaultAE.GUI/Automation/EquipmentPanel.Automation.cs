using TQVaultAE.Domain.Actions;
using TQVaultAE.GUI.Automation;

namespace TQVaultAE.GUI.Components.Panels;

/// <summary>Access to the loadout marks' tool tip for the automation endpoint.</summary>
public partial class EquipmentPanel
{
	/// <summary>
	/// Rests the pointer on <paramref name="mark"/> (null: leaves the panel) through the panel's own mouse move and leave
	/// handling, the virtual cursor placing the tool tip, and returns the tool tip's lines.
	/// </summary>
	internal IReadOnlyList<(string Text, Color Color)> AutomationHoverMark(LoadoutSlotMark? mark)
	{
		if (mark is not { } shown)
		{
			this.HideMarkTip();
			return [];
		}

		var bounds = this.MarkScreenBounds(shown);
		var center = new Point(bounds.X + (bounds.Width / 2), bounds.Y + (bounds.Height / 2));
		VirtualCursor.Set(this, center);
		try
		{
			this.LoadoutMarkMouseMove(this, new MouseEventArgs(MouseButtons.None, 0, center.X, center.Y, 0));
		}
		finally
		{
			VirtualCursor.Release();
		}

		return this.MarkTooltip is { Visible: true } tooltip
			? tooltip.Lines.Select(line => (line.Text, line.Color)).ToList()
			: [];
	}
}
