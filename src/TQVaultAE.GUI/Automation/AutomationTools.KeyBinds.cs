using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using TQVaultAE.Application.KeyBinds;
using TQVaultAE.GUI.Dialogs;
using TQVaultAE.GUI.Infrastructure;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Automation;

/// <summary>
/// MCP tools of the key binds: the bindings in use and saved, and the key binds dialog.
/// </summary>
internal static partial class AutomationTools
{
	/// <summary>
	/// Reads a key for <c>press_key</c> and <c>click_cell</c>: one character (<c>c</c>, <c>5</c>), a name
	/// (<c>backspace</c>, <c>esc</c>) or a gesture with modifiers (<c>ctrl+alt+d</c>, <c>alt</c>).
	/// </summary>
	private static Keys ParseKey(string text)
	{
		var trimmed = text.Trim();
		var normalized = trimmed.ToUpperInvariant() switch
		{
			"BACKSPACE" => "Back",
			"ESC" => "Escape",
			"DEL" => "Delete",
			var t when t.Length == 1 && char.IsAsciiDigit(t[0]) => "D" + t,
			_ => trimmed,
		};

		if (!KeyGesture.TryParse(normalized, out var gesture))
			throw new McpException($"Unknown key '{text}'. Use a character, a key name or a gesture like 'ctrl+alt+d'.");

		if (gesture.IsModifiersOnly)
			return KeyBindInput.ToKeys(new KeyGesture("A", gesture.Modifiers)) & Keys.Modifiers;

		var keys = KeyBindInput.ToKeys(gesture);
		return keys == Keys.None ? throw new McpException($"'{text}' is not a key Windows Forms knows.") : keys;
	}

	[McpServerTool(Name = "key_binds", ReadOnly = true)]
	[Description("Returns the key and mouse bindings in use (every action with its gestures, the fixed ones marked), the changes saved in UserConfig.xml (<KeyBinds .../>) and the conflicts.")]
	public static string KeyBinds()
		=> AutomationHost.OnUi(form =>
		{
			var map = form.Services.KeyBinds;
			return Serialize(new
			{
				actions = KeyBindRegistry.All.Select(d => new
				{
					action = d.Action.ToString(),
					kind = d.Kind.ToString(),
					scope = d.Scope.ToString(),
					gestures = map.Get(d.Action).Select(g => g.ToString()).ToList(),
					shown = KeyBindText.Gestures(map.Get(d.Action)),
					isDefault = map.IsDefault(d.Action),
				}).ToList(),
				saved = form.AutomationSettings.KeyBinds.Overrides,
				conflicts = map.Conflicts().Select(KeyBindText.Conflict).ToList(),
			});
		});

	[McpServerTool(Name = "key_binds_dialog", Destructive = true)]
	[Description("Reads the open key binds dialog (open it from the settings dialog: click_control(\"Settings\", \"keyBindsButton\")): every row as shown, the conflicts or hint under the list, whether OK is enabled. With an action, rebinds one slot like the user, through the dialog's own handlers: a click on the cell, then the key ('ctrl+g', 'x') or, for a click action, the click with its modifiers ('ctrl+shift+click'); an empty gesture presses Clear. defaults=true presses Defaults. Close it with close_dialog(\"Key binds\", \"OK\").")]
	public static string KeyBindsDialog(
		[Description("Action to rebind, as listed by key_binds (e.g. ItemDuplicateClick); empty to only read.")] string action = "",
		[Description("Slot: 0 shortcut, 1 alternate.")] int slot = 0,
		[Description("The new gesture, like 'ctrl+g' or 'ctrl+shift+click'; empty clears the slot.")] string gesture = "",
		[Description("true presses Defaults first.")] bool defaults = false)
		=> AutomationHost.OnUi(form =>
		{
			var dialog = System.Windows.Forms.Application.OpenForms.OfType<KeyBindsDialog>().FirstOrDefault(f => f.Visible)
				?? throw new McpException("The key binds dialog is not open.");

			if (defaults)
				dialog.AutomationDefaults();

			if (!string.IsNullOrWhiteSpace(action))
			{
				if (!Enum.TryParse<KeyBindAction>(action, ignoreCase: true, out var value))
					throw new McpException($"Unknown action '{action}'. Actions: {string.Join(", ", Enum.GetNames<KeyBindAction>())}");

				var parsed = KeyGesture.None;
				if (!string.IsNullOrWhiteSpace(gesture) && !KeyGesture.TryParse(gesture, out parsed))
					throw new McpException($"Unreadable gesture '{gesture}'.");

				try
				{
					dialog.AutomationRebind(value, slot, parsed);
				}
				catch (InvalidOperationException ex)
				{
					throw new McpException(ex.Message);
				}
			}

			var (rows, status, okEnabled, armed) = dialog.AutomationState();
			return Serialize(new { rows, status, okEnabled, armed });
		});
}
