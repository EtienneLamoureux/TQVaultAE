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

	/// <summary>The modifiers of a click tool: <paramref name="modifiers"/> ('ctrl+alt', empty for none), Shift added by <paramref name="shift"/>.</summary>
	private static Keys ClickModifiers(string modifiers, bool shift)
		=> (string.IsNullOrWhiteSpace(modifiers) ? Keys.None : ParseKey(modifiers) & Keys.Modifiers) | (shift ? Keys.Shift : Keys.None);

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
	[Description("Reads the open key binds dialog (open it from the settings dialog: click_control(\"Settings\", \"keyBindsButton\")): every row as shown, the conflicts or hint under the list, whether OK and Clear are enabled, the armed slot. With an action, rebinds one slot like the user, with real mouse and key messages: a click on the cell, then the key ('ctrl+g', 'x') sent to the focused window through the message loop (the dialog's ProcessCmdKey) or, for a click action, a click on the cell with its modifiers ('ctrl+shift+click'); an empty gesture clicks Clear. click=true only clicks the cell (with the modifiers of 'gesture', right=true for the right button) without rebinding. key presses a key in the dialog as is, last (e.g. 'escape'). defaults=true clicks Defaults first. Close it with close_dialog(\"Key binds\", \"OK\").")]
	public static string KeyBindsDialog(
		[Description("Action to rebind, as listed by key_binds (e.g. ItemDuplicateClick); empty to only read.")] string action = "",
		[Description("Slot: 0 shortcut, 1 alternate.")] int slot = 0,
		[Description("The new gesture, like 'ctrl+g' or 'ctrl+shift+click'; empty clears the slot.")] string gesture = "",
		[Description("true presses Defaults first.")] bool defaults = false,
		[Description("true only clicks the cell of action/slot, with the modifiers of gesture.")] bool click = false,
		[Description("With click: the right button.")] bool right = false,
		[Description("A key pressed in the dialog after the rest, like 'escape' or 'ctrl+alt+c'; empty for none.")] string key = "")
		=> AutomationHost.OnUi(form =>
		{
			var dialog = System.Windows.Forms.Application.OpenForms.OfType<KeyBindsDialog>().FirstOrDefault(f => f.Visible)
				?? throw new McpException("The key binds dialog is not open.");

			if (defaults)
				dialog.AutomationDefaults();

			string? keyTarget = null;
			try
			{
				if (!string.IsNullOrWhiteSpace(action))
				{
					if (!Enum.TryParse<KeyBindAction>(action, ignoreCase: true, out var value))
						throw new McpException($"Unknown action '{action}'. Actions: {string.Join(", ", Enum.GetNames<KeyBindAction>())}");

					var parsed = KeyGesture.None;
					if (!string.IsNullOrWhiteSpace(gesture) && !KeyGesture.TryParse(gesture, out parsed))
						throw new McpException($"Unreadable gesture '{gesture}'.");

					if (click)
						dialog.AutomationClickCell(value, slot, KeyBindInput.ToKeys(new KeyGesture("A", parsed.Modifiers)) & Keys.Modifiers, right ? MouseButtons.Right : MouseButtons.Left);
					else
						dialog.AutomationRebind(value, slot, parsed);
				}

				if (!string.IsNullOrWhiteSpace(key))
				{
					var target = dialog.AutomationPressKey(ParseKey(key));
					keyTarget = $"{target.GetType().Name} {target.Name}".TrimEnd();
				}
			}
			catch (InvalidOperationException ex)
			{
				throw new McpException(ex.Message);
			}

			var (rows, status, okEnabled, armed) = dialog.AutomationState();
			return Serialize(new { rows, status, okEnabled, clearEnabled = dialog.AutomationClearEnabled, armed, visible = dialog.Visible, keyTarget });
		});
}
