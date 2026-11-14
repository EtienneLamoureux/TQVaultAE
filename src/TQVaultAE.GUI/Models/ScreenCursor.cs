//-----------------------------------------------------------------------
// <copyright file="ScreenCursor.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI.Models;

/// <summary>
/// The mouse cursor's position on the screen, for the code that hit-tests it outside a mouse event (a click on any
/// control of a panel, the end of a drag captured by another control).
/// </summary>
/// <remarks>In Debug builds, the position automation rests its virtual cursor on while it drives the app.</remarks>
internal static class ScreenCursor
{
	/// <summary>Gets the cursor position in screen coordinates.</summary>
	public static Point Position
	{
		get
		{
#if DEBUG
			if (Automation.VirtualCursor.IsActive)
				return Automation.VirtualCursor.ScreenPosition;
#endif
			return Cursor.Position;
		}
	}
}
