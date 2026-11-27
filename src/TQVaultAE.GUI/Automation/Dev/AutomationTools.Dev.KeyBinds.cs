using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using TQVaultAE.Application.KeyBinds;
using TQVaultAE.GUI.Dialogs;
using TQVaultAE.GUI.Infrastructure;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Automation;

internal static partial class AutomationTools
{
	internal static partial class DevTools
	{
		[McpServerTool(Name = "key_binds_dialog", Destructive = true)]
		[Description("Reads the open key binds dialog (open it from the settings: click_control(\"\", \"configureButton\"), then click_control(\"Settings\", \"keyBindsButton\")): every row as shown, the conflicts or hint under the list, whether OK and Clear are enabled, the armed slot. With an action, rebinds one slot like the user, with real mouse and key messages: a click on the cell, then the key ('ctrl+g', 'x') sent to the focused window through the message loop (the dialog's ProcessCmdKey) or, for a click action, a click on the cell with its modifiers ('ctrl+shift+click'); an empty gesture clicks Clear. click=true only clicks the cell (with the modifiers of 'gesture', right=true for the right button) without rebinding. key presses a key in the dialog as is, last (e.g. 'escape'). defaults=true clicks Defaults first. Close it with close_dialog(\"Key binds\", \"OK\").")]
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
				var dialog = OpenDialog<KeyBindsDialog>("key binds dialog");

				if (defaults)
					dialog.AutomationDefaults();

				string? keyTarget = null;
				if (!string.IsNullOrWhiteSpace(action))
				{
					var value = ParseEnum<KeyBindAction>(action, "action");

					var parsed = KeyGesture.None;
					if (!string.IsNullOrWhiteSpace(gesture) && !KeyGesture.TryParse(gesture, out parsed))
						throw new McpException($"Unreadable gesture '{gesture}'.");

					if (click)
						dialog.AutomationClickCell(value, slot, AutomationInput.ModifierKeys(parsed.Modifiers), right ? MouseButtons.Right : MouseButtons.Left);
					else
						dialog.AutomationRebind(value, slot, parsed);
				}

				if (!string.IsNullOrWhiteSpace(key))
				{
					var target = dialog.AutomationPressKey(ParseKey(key));
					keyTarget = $"{target.GetType().Name} {target.Name}".TrimEnd();
				}

				var (rows, status, okEnabled, armed) = dialog.AutomationState();
				return Serialize(new { rows, status, okEnabled, clearEnabled = dialog.AutomationClearEnabled, armed, visible = dialog.Visible, keyTarget });
			});
	}
}
