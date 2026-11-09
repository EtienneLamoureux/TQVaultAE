namespace TQVaultAE.GUI.Components.Character;

/// <summary>Debug-only access to the side panel of the Character page for the automation endpoint.</summary>
internal sealed partial class CharacterSidePanel
{
	/// <summary>What the panel draws, and its editors (leveling, statistics options).</summary>
	internal object AutomationState() => new
	{
		panel = this.Name,
		scale = this.PanelScale,
		bounds = new { this.Left, this.Top, this.Width, this.Height },
		view = this.View?.Invoke(),
		editors = this.Editors.Select(c => new { c.Name, c.Visible, c.Enabled, c.Text }).ToList(),
	};
}
