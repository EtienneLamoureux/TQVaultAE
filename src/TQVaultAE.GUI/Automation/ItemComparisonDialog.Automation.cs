//-----------------------------------------------------------------------
// <copyright file="ItemComparisonDialog.Automation.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI.Dialogs;

/// <summary>
/// Access to the options of the item comparison dialog for the automation endpoint.
/// </summary>
internal sealed partial class ItemComparisonDialog
{
	/// <summary>Sets the option check boxes of the toolbar (null leaves one as it is), as their Checked property does for a click.</summary>
	internal void AutomationSetOptions(bool? alignRows, bool? hideUnchanged, bool? showSources)
	{
		if (alignRows is { } align)
			this._alignRows.Checked = align;
		if (hideUnchanged is { } hide)
			this._hideUnchanged.Checked = hide;
		if (showSources is { } sources)
			this._showSources.Checked = sources;
	}
}
