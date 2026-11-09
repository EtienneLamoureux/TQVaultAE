namespace TQVaultAE.GUI.Components.Character;

/// <summary>
/// Debug-only access to the statistics panel for the automation endpoint: what it shows, and clicks on the "+" buttons
/// through its own hit test.
/// </summary>
internal sealed partial class CharacterStatsPanel
{
	/// <summary>What the panel draws.</summary>
	internal object AutomationState()
	{
		var view = this.View?.Invoke();
		return new
		{
			panel = this.Name,
			tab = this.Tab + 1,
			scale = this.PanelScale,
			size = new { this.Width, this.Height },
			view,
			editors = this.Tab1Controls.Select(c => new { c.Name, c.Visible, c.Enabled, c.Text }).ToList(),
		};
	}

	/// <summary>Clicks the centre of the Undo button, through the hit test.</summary>
	/// <returns>false when the button does not show (no attribute changed).</returns>
	internal bool AutomationUndo()
	{
		var bounds = this.UndoBounds;
		if (!this.HitUndo(new System.Drawing.Point(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2)))
			return false;

		this.RaiseUndo();
		return true;
	}

	/// <summary>Clicks the centre of the "+" button of an attribute, through the hit test.</summary>
	/// <returns>false when no button is there (no point left).</returns>
	internal bool AutomationClick(int attribute, int step, bool toEnd)
	{
		var bounds = this.PlusBounds(attribute);
		var center = new System.Drawing.Point(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2);
		if (this.HitPlus(center) != attribute)
			return false;

		this.RaiseAttributeClick(attribute, step, toEnd);
		return true;
	}
}
