using TQVaultAE.GUI.Tooltips;

namespace TQVaultAE.GUI.Components.Character;

/// <summary>Access to the statistics panel's Buffs tab for the automation endpoint: its list, clicks, the scroll choice.</summary>
internal sealed partial class CharacterStatsPanel
{
	/// <summary>The Buffs tab as drawn: sections, sources (on, place in client pixels when in sight, tool tip lines), the scroll choice.</summary>
	internal object AutomationBuffState()
	{
		var view = this.View?.Invoke();
		if (view is null)
			return new { };

		var shown = this.VisibleBuffs(view.Buffs).ToDictionary(p => p.Entry.Id, p => this.ScaledRect(p.Area), StringComparer.OrdinalIgnoreCase);
		return new
		{
			tab = this.Tab + 1,
			scroll = this._buffScroll,
			maxScroll = MaxBuffScroll(view.Buffs),
			scrollChoice = new
			{
				title = view.Buffs.ScrollTitle,
				selected = view.Buffs.SelectedScroll,
				name = view.Buffs.Scrolls.ElementAtOrDefault(view.Buffs.SelectedScroll)?.Name,
				count = view.Buffs.Scrolls.Count,
				box = this._scrollBox is { } box ? new { box.Visible, box.Bounds, box.Text } : null,
			},
			sections = view.Buffs.Sections.Select(s => new
			{
				group = s.Group.ToString(),
				title = s.Title,
				entries = s.Entries.Select(e => new
				{
					id = e.Id,
					name = e.Name,
					icon = e.Icon,
					on = e.On,
					bounds = shown.TryGetValue(e.Id, out var bounds) ? bounds : (System.Drawing.Rectangle?)null,
					toolTip = ToolTipLines(e.ToolTip).Select(l => l.Text).ToList(),
				}).ToList(),
			}).ToList(),
		};
	}

	/// <summary>The scroll choices of the Buffs tab, in their order.</summary>
	internal IReadOnlyList<string> AutomationScrollNames()
		=> this.View?.Invoke()?.Buffs.Scrolls.Select(s => s.Name).ToList() ?? [];

	/// <summary>
	/// Clicks a source of the Buffs tab like the mouse: scrolls it into sight, then the panel's mouse handler and hit test
	/// take the click at its centre.
	/// </summary>
	/// <returns>false when the tab has no such source.</returns>
	internal bool AutomationBuffClick(string id)
	{
		this.Tab = BuffsTab;
		if (this.View?.Invoke() is not { } view
			|| BuffLayout(view.Buffs).FirstOrDefault(p => p.Entry?.Id.Equals(id, StringComparison.OrdinalIgnoreCase) == true) is not { Entry: not null } place)
			return false;

		// The source fully in sight: scrolled so that it is the first row when it is below or above the list.
		var top = place.Area.Y - this._buffScroll;
		if (top < 0 || top + place.Area.Height > BuffBottom - BuffTop)
			this.ScrollBuffs(place.Area.Y - this._buffScroll);

		var bounds = this.ScaledRect(this.OnPanel(place.Area) ?? place.Area);
		var center = new System.Drawing.Point(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2);
		if (this.HitBuff(center)?.Id.Equals(id, StringComparison.OrdinalIgnoreCase) != true)
			return false;

		this.OnMouseUp(new MouseEventArgs(MouseButtons.Left, 1, center.X, center.Y, 0));
		return true;
	}

	/// <summary>Picks a scroll in the Buffs tab's drop-down, as the user does (its selection event).</summary>
	/// <returns>false when there is no such choice.</returns>
	internal bool AutomationPickScroll(int index)
	{
		this.Tab = BuffsTab;
		this.SyncScrollBox(this.View?.Invoke());
		if (this._scrollBox is not { } box || index < 0 || index >= box.Items.Count)
			return false;

		box.SelectedIndex = index;
		return true;
	}

	/// <summary>Scrolls the Buffs tab's list by game pixels, as the wheel does.</summary>
	internal void AutomationScrollBuffs(int delta)
	{
		this.Tab = BuffsTab;
		this.ScrollBuffs(delta);
	}

	/// <summary>The lines of a source's tool tip as the game-style tool tip shows them.</summary>
	internal IReadOnlyList<GameTooltipLine>? AutomationBuffToolTip(string id)
		=> this.View?.Invoke()?.Buffs.Sections.SelectMany(s => s.Entries).FirstOrDefault(e => e.Id.Equals(id, StringComparison.OrdinalIgnoreCase)) is { } entry
			? ToolTipLines(entry.ToolTip)
			: null;
}
