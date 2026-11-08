//-----------------------------------------------------------------------
// <copyright file="ComboBoxCharacter.Automation.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using TQVaultAE.Application;

namespace TQVaultAE.GUI.Components.Character;

/// <summary>
/// Debug-only access to the archive toggle of the character menu for the automation endpoint.
/// </summary>
public partial class ComboBoxCharacter
{
	/// <summary>
	/// Ticks or unticks "Archived" in the character menu like a click, for the selected character only, which moves
	/// its save folder to or from the archive folder. Only test characters (names starting with <c>claude-</c>).
	/// </summary>
	/// <returns>The character, whether it is archived now, and its save folder.</returns>
	internal (string Name, bool IsArchived, string? PlayerFile) AutomationSetArchived(bool archived)
	{
		if (this.SelectedItem is not PlayerSave ps)
			throw new InvalidOperationException("No character is selected.");
		if (!ps.Name.StartsWith("claude-", StringComparison.OrdinalIgnoreCase))
			throw new InvalidOperationException($"'{ps.Name}' is not a test character: only names starting with 'claude-' may be archived.");

		this.archiveToolStripMenuItem.Checked = archived;
		return (ps.Name, ps.IsArchived, ps.Folder);
	}
}
