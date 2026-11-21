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

}
