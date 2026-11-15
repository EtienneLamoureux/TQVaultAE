//-----------------------------------------------------------------------
// <copyright file="HighlightFilters.Automation.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using TQVaultAE.Automation;

namespace TQVaultAE.GUI.Components;

/// <summary>
/// Entry points used by the automation endpoint.
/// </summary>
public partial class HighlightFilters
{
	/// <summary>The check boxes of the panel, in layout order.</summary>
	internal IReadOnlyList<CheckBox> AutomationCheckBoxes => ControlTree.AllControls(this).OfType<CheckBox>().ToList();

	/// <summary>Applies the filters like the Apply button.</summary>
	internal void AutomationApply() => this.buttonSave_Click(this, EventArgs.Empty);
}
