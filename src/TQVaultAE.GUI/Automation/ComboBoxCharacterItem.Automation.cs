namespace TQVaultAE.GUI.Components.Character;

/// <summary>
/// Debug-only access to an entry of the character list for the automation endpoint.
/// </summary>
public partial class ComboBoxCharacterItem
{
	/// <summary>Whether the entry shows its "Read-only" badge (read while the list is open).</summary>
	internal bool AutomationShowsReadOnlyBadge => this.labelReadOnly.Visible;

	/// <summary>
	/// The texts of the entry as drawn: name, class, level, masteries, mod and the badges shown (read while the list is
	/// open, since a label of a hidden panel reads as not visible).
	/// </summary>
	internal object AutomationTexts => new
	{
		name = this.scalingLabelCharName.Text,
		className = this.scalingLabelCLassName.Text,
		level = this.scalingLabelLevel.Text,
		masteries = this.scalingLabelMasteries.Visible ? this.scalingLabelMasteries.Text : null,
		mod = this.scalingLabelModName.Visible ? this.scalingLabelModName.Text : null,
		badges = new[] { this.scalingLabelArchived, this.labelReadOnly, this.scalingLabelModded, this.scalingLabelModState }
			.Where(label => label.Visible)
			.Select(label => label.Text)
			.ToArray(),
	};
}
