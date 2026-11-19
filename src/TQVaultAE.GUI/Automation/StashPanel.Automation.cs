//-----------------------------------------------------------------------
// <copyright file="StashPanel.Automation.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI.Components.Panels;

/// <summary>
/// Access to the player information panel (the stats shown next to the equipment) for the automation endpoint.
/// </summary>
public partial class StashPanel
{
	/// <summary>
	/// Gets the statistics panel shown next to the equipment (the game's character window, read only); null before the
	/// first character is shown.
	/// </summary>
	internal TQVaultAE.GUI.Components.Character.CharacterStatsPanel? AutomationPlayerStatsPanel => this._statsPanel;

	/// <summary>Gets the information of the character shown, whose attributes the item requirements are checked against.</summary>
	internal TQVaultAE.Application.Session.PlayerInfo? AutomationPlayerInfo => this.Player?.PlayerInfo;

	/// <summary>Gets the Edit button under the statistics panel; null before the first character is shown.</summary>
	internal Button? AutomationEditButton => this._editButton;

	/// <summary>A control of the stash panel or of its room (statistics, loadout panel, loadout bar), in the stash panel's pixels.</summary>
	/// <param name="control">The control.</param>
	internal Rectangle AutomationBounds(Control control)
	{
		var bounds = control.Bounds;
		for (var parent = control.Parent; parent is not null && parent != this; parent = parent.Parent)
			bounds.Offset(parent.Location);
		return bounds;
	}

	/// <summary>Runs the Edit button's click handler, which shows the character editor in place of the main content.</summary>
	internal void AutomationClickEdit() => this.EditButton_Click(this, EventArgs.Empty);
}
