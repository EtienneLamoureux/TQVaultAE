//-----------------------------------------------------------------------
// <copyright file="VirtualCursor.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI.Automation;

/// <summary>
/// Debug-only replacement for <see cref="Cursor.Position"/> while automation drives a drag.
/// The real cursor is never moved: the panels read the cursor position from here when an override is active.
/// </summary>
internal static class VirtualCursor
{
	/// <summary>
	/// Point far away from any control, used so that panels other than the hovered one do not draw the dragged item.
	/// </summary>
	private static readonly Point Nowhere = new(-100000, -100000);

	private static Control? _target;
	private static Point _clientLocation;

	/// <summary>
	/// Gets a value indicating whether automation currently owns the cursor position.
	/// </summary>
	public static bool IsActive => _target is not null;

	/// <summary>
	/// Gets the control the virtual cursor is over.
	/// </summary>
	public static Control? Target => _target;

	/// <summary>
	/// Gets the virtual cursor location in <see cref="Target"/> client coordinates.
	/// </summary>
	public static Point ClientLocation => _clientLocation;

	/// <summary>
	/// Gets the virtual cursor location on the screen; the real cursor's when automation does not hold it.
	/// </summary>
	public static Point ScreenPosition => _target is { IsDisposed: false } target ? target.PointToScreen(_clientLocation) : Cursor.Position;

	/// <summary>
	/// Places the virtual cursor over <paramref name="target"/> at a client location.
	/// </summary>
	public static void Set(Control target, Point clientLocation)
	{
		_target = target;
		_clientLocation = clientLocation;
	}

	/// <summary>
	/// Hands the cursor back to the real mouse.
	/// </summary>
	public static void Release() => _target = null;

	/// <summary>
	/// Gets the cursor position in <paramref name="control"/> client coordinates, honoring the automation override.
	/// </summary>
	public static Point GetClientPosition(Control control)
	{
		if (_target is null)
			return control.PointToClient(Cursor.Position);

		return ReferenceEquals(_target, control) ? _clientLocation : Nowhere;
	}
}
