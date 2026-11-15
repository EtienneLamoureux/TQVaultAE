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

	/// <summary>
	/// The check box lists of the panel (types, rarities, games, installed mods), by short name: the control's name
	/// without its <c>scalingCheckedListBox</c> prefix (<c>Types</c>, <c>Rarity</c>, <c>Origin</c>, <c>Mods</c>).
	/// </summary>
	internal IReadOnlyDictionary<string, CheckedListBox> AutomationLists
		=> ControlTree.AllControls(this).OfType<CheckedListBox>()
			.ToDictionary(l => l.Name.Replace("scalingCheckedListBox", string.Empty, StringComparison.Ordinal), l => l, StringComparer.OrdinalIgnoreCase);

	/// <summary>Applies the filters like the Apply button.</summary>
	internal void AutomationApply() => this.buttonSave_Click(this, EventArgs.Empty);
}
