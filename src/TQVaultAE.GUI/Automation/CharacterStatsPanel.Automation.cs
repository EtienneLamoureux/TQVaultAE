namespace TQVaultAE.GUI.Components.Character;

/// <summary>
/// Debug-only access to the statistics panel for the automation endpoint: what it shows, and clicks on the "+" buttons
/// through its own hit test.
/// </summary>
internal sealed partial class CharacterStatsPanel
{
	/// <summary>What the panel draws.</summary>
	/// <summary>The rollovers of the tab shown, in the order of the areas: their lines as the game tool tip shows them.</summary>
	internal IReadOnlyList<IReadOnlyList<GameTooltipLine>> AutomationToolTips()
		=> this.View?.Invoke() is { } view
			? [.. this.ToolTipAreas(view).Where(a => !a.ToolTip.IsEmpty).Select(a => (IReadOnlyList<GameTooltipLine>)ToolTipLines(a.ToolTip))]
			: [];

	/// <summary>The areas of the rollovers of the tab shown, in client pixels, in the order of <see cref="AutomationToolTips"/>.</summary>
	internal IReadOnlyList<System.Drawing.Rectangle> AutomationToolTipBounds()
		=> this.View?.Invoke() is { } view
			? [.. this.ToolTipAreas(view).Where(a => !a.ToolTip.IsEmpty).Select(a => this.ScaledRect(a.Area))]
			: [];

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

	/// <summary>
	/// Clicks the centre of the "+" button of an attribute with <paramref name="modifiers"/> held, through the panel's
	/// mouse handler: its hit test and the key binds read the click as the user's.
	/// </summary>
	/// <returns>false when no button is there (no point left).</returns>
	internal bool AutomationClick(int attribute, int step, Keys modifiers)
	{
		var bounds = this.PlusBounds(attribute);
		var center = new System.Drawing.Point(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2);
		if (this.HitPlus(center) != attribute)
			return false;

		using var held = TQVaultAE.GUI.Automation.AutomationInput.HoldModifiers(modifiers);
		this.OnMouseUp(new MouseEventArgs(step > 0 ? MouseButtons.Left : MouseButtons.Right, 1, center.X, center.Y, 0));
		return true;
	}
}
