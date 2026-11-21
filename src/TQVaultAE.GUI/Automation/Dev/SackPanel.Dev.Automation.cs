using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Automation;
using TQVaultAE.GUI.Models;

namespace TQVaultAE.GUI.Components.Panels;

/// <summary>Debug access to the sack panels: drag selection and keys.</summary>
public partial class SackPanel
{
	/// <summary>
	/// Presses the left button on a cell with <paramref name="modifiers"/> held, moves the mouse to another cell and
	/// releases it there, through the regular mouse handlers: the rectangle selection (Shift by default).
	/// </summary>
	internal void AutomationDragSelect(Point from, Point to, Keys modifiers)
	{
		using var held = AutomationInput.HoldModifiers(modifiers);
		var start = this.AutomationCellCenter(from);
		var end = this.AutomationCellCenter(to);
		this.AutomationMouseMove(start);
		this.MouseDownCallback(this, new MouseEventArgs(MouseButtons.Left, 1, start.X, start.Y, 0));
		VirtualCursor.Set(this, end);
		this.MouseMoveCallback(this, new MouseEventArgs(MouseButtons.Left, 0, end.X, end.Y, 0));
		this.MouseUpCallback(this, new MouseEventArgs(MouseButtons.Left, 1, end.X, end.Y, 0));
		this.AutomationPaintNow();
	}

	/// <summary>
	/// Rests the virtual cursor on a cell, then presses a key like the keyboard (<see cref="AutomationInput.PostKey"/>):
	/// to the focused window, handled after the calling tool has returned (Backspace asks for a confirmation).
	/// </summary>
	/// <returns>The control the key was sent to.</returns>
	internal Control AutomationKeyDown(Point cell, Keys keyData, Forms.MainForm form)
	{
		this.AutomationMouseMove(this.AutomationCellCenter(cell));
		return AutomationInput.PostKey(form, keyData);
	}
}
