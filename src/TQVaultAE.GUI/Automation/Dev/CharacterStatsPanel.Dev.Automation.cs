using TQVaultAE.GUI.Tooltips;

namespace TQVaultAE.GUI.Components.Character;

/// <summary>Debug access to the statistics panel's tool tip.</summary>
internal sealed partial class CharacterStatsPanel
{
	/// <summary>The areas of the rollovers of the tab shown, in client pixels, in the order of <see cref="AutomationToolTips"/>.</summary>
	internal IReadOnlyList<System.Drawing.Rectangle> AutomationToolTipBounds()
		=> this.View?.Invoke() is { } view
			? [.. this.ToolTipAreas(view).Where(a => !a.ToolTip.IsEmpty).Select(a => this.ScaledRect(a.Area))]
			: [];
}
