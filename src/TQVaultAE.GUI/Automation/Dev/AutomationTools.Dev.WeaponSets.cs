using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace TQVaultAE.GUI.Automation;

internal static partial class AutomationTools
{
	internal static partial class DevTools
	{
		[McpServerTool(Name = "character_weapon_set")]
		[Description("Reads the weapon set gems I and II of a statistics panel's tab I (the character editor's, the main window's or a floating character window's): the set counted, the set in hand in the save, each gem's bounds and tool tip, tab I's armor and offense rows. With set=1 or 2, first clicks that gem like the mouse (shows tab I): the statistics count that set's weapons, the save does not change.")]
		public static string CharacterWeaponSet(
			[Description("'editor', 'main' or 'window:<name>'.")] string panel = "editor",
			[Description("1 or 2 to click that set's gem first; omit to only read.")] int? set = null)
			=> AutomationHost.OnUi(_ =>
			{
				var stats = FindStatsPanel(panel, windowTab: 0);
				if (set is { } chosen && !stats.AutomationWeaponSetClick(chosen - 1))
					throw new McpException($"No weapon set {chosen}: 1 or 2.");

				return Serialize(stats.AutomationWeaponSetState());
			});
	}
}
