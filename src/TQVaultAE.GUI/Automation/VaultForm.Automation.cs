//-----------------------------------------------------------------------
// <copyright file="VaultForm.Automation.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI.Forms;

using System.Drawing;
using System.Windows.Forms;
using TQVaultAE.GUI.Interop;

/// <summary>
/// Access to the custom border of a TQVault form for the automation endpoint.
/// </summary>
public partial class VaultForm
{
	/// <summary>
	/// Right-clicks the middle of the title bar with the message Windows sends (<c>WM_NCRBUTTONUP</c> on the caption),
	/// which opens the window menu through the form's own handler.
	/// </summary>
	/// <returns>The window menu, open; null without the custom border (Windows' own menu then).</returns>
	internal ContextMenuStrip? AutomationOpenSystemMenu()
	{
		if (!this.DrawCustomBorder)
			return null;

		var screen = this.PointToScreen(new Point(this.topBorderRect.Left + (this.topBorderRect.Width / 2), this.topBorderRect.Top + (this.topBorderRect.Height / 2)));
		User32.SendMessage(this.Handle, (int)User32.NonClientMouseMessage.RightButtonUp, (int)User32.NonClientHitTestResult.Caption, User32.MakeLParam(screen));
		return this.systemMenu;
	}
}
