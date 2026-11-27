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
/// Access to the key binds dialog for the automation endpoint: its rows as shown, and a rebind through the
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
}
