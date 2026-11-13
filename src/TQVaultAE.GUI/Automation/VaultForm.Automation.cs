//-----------------------------------------------------------------------
// <copyright file="VaultForm.Automation.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI.Forms;

using System.Drawing;
using System.Windows.Forms;

/// <summary>
/// Debug-only access to the custom border of a TQVault form for the automation endpoint.
/// </summary>
public partial class VaultForm
{
	/// <summary>
	/// Gets the space the border pictures drawn now take on each side (they follow the zoom); empty without the custom
	/// border.
	/// </summary>
	internal Padding AutomationBorder => this.DrawCustomBorder
		? new Padding(this.sideBorder.Width, this.topBorder.Height, this.sideBorder.Width, this.bottomBorder.Height)
		: Padding.Empty;
}
