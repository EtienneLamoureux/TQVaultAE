//-----------------------------------------------------------------------
// <copyright file="StashPanel.Automation.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI.Components.Panels;

/// <summary>
/// Debug-only access to the player information panel (the stats shown next to the equipment) for the automation endpoint.
/// </summary>
public partial class StashPanel
{
	/// <summary>
	/// Gets the statistics panel shown next to the equipment (the game's character window, read only); null before the
	/// first character is shown.
	/// </summary>
	internal TQVaultAE.GUI.Components.Character.CharacterStatsPanel? AutomationPlayerStatsPanel => this._statsPanel;

	/// <summary>Gets the Edit button under the statistics panel; null before the first character is shown.</summary>
	internal Button? AutomationEditButton => this._editButton;

	/// <summary>Gets the background drawn now: a source picture at zoom 1, else a scaled copy made at the tab switch.</summary>
	internal System.Drawing.Bitmap? AutomationBackground => this.background;

	/// <summary>Whether <paramref name="bitmap"/> is one of the source pictures (equipment or stash), kept for the session.</summary>
	internal bool AutomationIsSourceBackground(System.Drawing.Bitmap bitmap)
		=> ReferenceEquals(bitmap, this.equipmentBackground) || ReferenceEquals(bitmap, this.stashBackground);
}
