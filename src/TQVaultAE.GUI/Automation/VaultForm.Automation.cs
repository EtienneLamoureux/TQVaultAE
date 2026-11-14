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

	/// <summary>
	/// Right-clicks the middle of the title bar with the message Windows sends (<c>WM_NCRBUTTONUP</c> on the caption),
	/// which opens the window menu through the form's own handler.
	/// </summary>
	/// <returns>The window menu, open; null without the custom border (Windows' own menu then).</returns>
	internal ContextMenuStrip? AutomationOpenSystemMenu()
	{
		if (!this.DrawCustomBorder)
			return null;

		const int WM_NCRBUTTONUP = 0x00A5, HTCAPTION = 2;
		var screen = this.PointToScreen(new Point(this.topBorderRect.Left + (this.topBorderRect.Width / 2), this.topBorderRect.Top + (this.topBorderRect.Height / 2)));
		TQVaultAE.GUI.Automation.NativeWindows.Send(this.Handle, WM_NCRBUTTONUP, HTCAPTION, (screen.Y << 16) | (screen.X & 0xFFFF));
		return this.systemMenu;
	}
}
