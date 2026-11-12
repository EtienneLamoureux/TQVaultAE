using TQVaultAE.Application.KeyBinds;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Automation;
using TQVaultAE.GUI.Infrastructure;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Dialogs;

/// <summary>A row of the key binds dialog as shown.</summary>
internal sealed record AutomationKeyBindRow(string Action, string Name, string Kind, IReadOnlyList<string> Cells, IReadOnlyList<bool> Conflicts
	, bool Greyed, string? ToolTip);

/// <summary>
/// Debug-only access to the key binds dialog for the automation endpoint: its rows as shown, and a rebind through the
/// dialog's own capture handlers (a click on the cell, then the key or the click with modifiers).
/// </summary>
internal sealed partial class KeyBindsDialog
{
	/// <summary>The rows as shown, the text under the list and whether OK is enabled.</summary>
	internal (IReadOnlyList<AutomationKeyBindRow> Rows, string Status, bool OkEnabled, string? Armed) AutomationState()
		=> (KeyBindRegistry.All
				.Select(d => new AutomationKeyBindRow(
					d.Action.ToString()
					, KeyBindText.Action(d.Action)
					, d.Kind.ToString()
					, [.. Enumerable.Range(0, KeyBindMap.MaxGestures).Select(slot => this.cells[(d.Action, slot)].Text)]
					, [.. Enumerable.Range(0, KeyBindMap.MaxGestures).Select(slot => this.map.HasConflict(d.Action, slot))]
					, this.names[d.Action].ForeColor != TQColor.White.Color()
					, this.toolTip.GetToolTip(this.names[d.Action])))
				.ToList()
			, this.labelStatus.Text
			, this.buttonOk.Enabled
			, this.armed is { } a ? $"{a.Action}{a.Slot}" : null);

	/// <summary>
	/// Rebinds a slot like the user, with real mouse and key messages (<see cref="AutomationInput"/>): a left click on
	/// its cell arms it, then the key goes to the focused window through the message loop's steps (the dialog's
	/// ProcessCmdKey takes it), or, for a click action, the cell is clicked again with the modifiers held; an empty
	/// gesture presses Clear.
	/// </summary>
	internal void AutomationRebind(KeyBindAction action, int slot, KeyGesture gesture)
	{
		if (!this.cells.ContainsKey((action, slot)))
			throw new InvalidOperationException($"No slot {slot} for {action}.");

		if (this.armed != (action, slot))
			this.AutomationClickCell(action, slot, Keys.None);

		// A button's mouse click checks that the window under the screen point is the button, which a hidden desktop
		// does not answer: Clear is clicked like a keyboard press of the button.
		if (gesture.IsNone)
			this.buttonClear.PerformClick();
		else if (gesture.IsClick)
			this.AutomationClickCell(action, slot, KeyBindInput.ToKeys(new KeyGesture("A", gesture.Modifiers)) & Keys.Modifiers);
		else
		{
			var keys = KeyBindInput.ToKeys(gesture);
			if (keys == Keys.None)
				throw new InvalidOperationException($"'{gesture}' is not a key.");
			AutomationInput.SendKey(this, keys);
		}
	}

	/// <summary>A click on a shortcut cell with <paramref name="modifiers"/> held, through its window procedure.</summary>
	internal void AutomationClickCell(KeyBindAction action, int slot, Keys modifiers, MouseButtons button = MouseButtons.Left)
	{
		if (!this.cells.TryGetValue((action, slot), out var cell))
			throw new InvalidOperationException($"No slot {slot} for {action}.");

		AutomationInput.SendClick(cell, AutomationInput.Center(cell), button, modifiers);
	}

	/// <summary>Presses a key in the dialog like the keyboard, to its focused window.</summary>
	/// <returns>The control the key was sent to.</returns>
	internal Control AutomationPressKey(Keys keys) => AutomationInput.SendKey(this, keys);

	/// <summary>Whether the Clear button can be clicked.</summary>
	internal bool AutomationClearEnabled => this.buttonClear.Enabled;

	/// <summary>Presses the Defaults button.</summary>
	internal void AutomationDefaults() => this.buttonDefaults.PerformClick();
}
