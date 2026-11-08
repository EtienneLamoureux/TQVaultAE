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
	/// Gets the table of the player information panel: the Edit button, then one label pair (name, value) per row.
	/// </summary>
	internal TableLayoutPanel AutomationPlayerInfoPanel => this.PlayerPanel;
}
