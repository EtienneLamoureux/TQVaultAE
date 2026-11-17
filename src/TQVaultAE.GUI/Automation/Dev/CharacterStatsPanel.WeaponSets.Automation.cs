namespace TQVaultAE.GUI.Components.Character;

/// <summary>Access to the weapon set gems of tab I for the automation endpoint.</summary>
internal sealed partial class CharacterStatsPanel
{
	/// <summary>The gems as drawn (counted, bounds in client pixels, tool tip lines) and the tab I values they change.</summary>
	internal object AutomationWeaponSetState()
	{
		if (this.View?.Invoke() is not { } view)
			return new { };

		var stats = view.Stats;
		return new
		{
			tab = this.Tab + 1,
			counted = stats.WeaponSets.FirstOrDefault(s => s.Counted)?.Name,
			sets = stats.WeaponSets.Select((s, i) => new
			{
				name = s.Name,
				counted = s.Counted,
				hovered = i == this._hoveredWeaponSet,
				bounds = this.WeaponSetBounds(i),
				toolTip = ToolTipLines(s.ToolTip).Select(l => l.Text).ToList(),
			}).ToList(),
			armor = stats.Armor.Value,
			offense = stats.Offense.Select(r => new { label = r.Label, value = r.Value }).ToList(),
			attributes = view.Attributes.Select(a => new { label = a.Label, value = a.Value }).ToList(),
		};
	}

	/// <summary>Clicks a weapon set gem (0 for I, 1 for II) like the mouse: tab I shown, then the panel's hit test at its centre.</summary>
	/// <returns>false when there is no such gem.</returns>
	internal bool AutomationWeaponSetClick(int set)
	{
		if (set is < 0 or > 1)
			return false;

		if (!this.TabsFixed)
			this.Tab = 0;
		var bounds = this.WeaponSetBounds(set);
		var center = new System.Drawing.Point(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2);
		if (this.HitWeaponSet(center) != set)
			return false;

		this.OnMouseUp(new MouseEventArgs(MouseButtons.Left, 1, center.X, center.Y, 0));
		return true;
	}
}
