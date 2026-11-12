using TQVaultAE.Application.KeyBinds;
using TQVaultAE.GUI.Infrastructure;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Dialogs;

/// <summary>A row of the key binds dialog as shown.</summary>
internal sealed record AutomationKeyBindRow(string Action, string Name, string Kind, IReadOnlyList<string> Cells, IReadOnlyList<bool> Conflicts);

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
					, [.. Enumerable.Range(0, KeyBindMap.MaxGestures).Select(slot => this.map.HasConflict(d.Action, slot))]))
				.ToList()
			, this.labelStatus.Text
			, this.buttonOk.Enabled
			, this.armed is { } a ? $"{a.Action}{a.Slot}" : null);

	/// <summary>
	/// Rebinds a slot like the user: a left click on its cell arms it, then the gesture goes through the same handler
	/// as a real key (<see cref="CaptureKey"/>) or click with modifiers (<see cref="CellMouseDown"/>); an empty gesture
	/// presses Clear.
	/// </summary>
	internal void AutomationRebind(KeyBindAction action, int slot, KeyGesture gesture)
	{
		if (!this.cells.ContainsKey((action, slot)))
			throw new InvalidOperationException($"No slot {slot} for {action}.");

		if (this.armed != (action, slot))
			this.CellMouseDown((action, slot), MouseButtons.Left, Keys.None);

		if (gesture.IsNone)
			this.buttonClear.PerformClick();
		else if (gesture.IsClick)
			this.CellMouseDown((action, slot), MouseButtons.Left, KeyBindInput.ToKeys(new KeyGesture("A", gesture.Modifiers)) & Keys.Modifiers);
		else
		{
			var keys = KeyBindInput.ToKeys(gesture);
			if (keys == Keys.None)
				throw new InvalidOperationException($"'{gesture}' is not a key.");
			this.CaptureKey(keys);
		}
	}

	/// <summary>Presses the Defaults button.</summary>
	internal void AutomationDefaults() => this.buttonDefaults.PerformClick();
}
