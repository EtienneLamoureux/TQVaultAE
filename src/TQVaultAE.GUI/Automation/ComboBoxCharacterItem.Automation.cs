namespace TQVaultAE.GUI.Components.Character;

/// <summary>
/// Debug-only access to an entry of the character list for the automation endpoint.
/// </summary>
public partial class ComboBoxCharacterItem
{
	/// <summary>Whether the entry shows its "Read-only" badge (read while the list is open).</summary>
	internal bool AutomationShowsReadOnlyBadge => this.labelReadOnly.Visible;
}
