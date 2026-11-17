using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using TQVaultAE.GUI.Components.Character;
using static TQVaultAE.Automation.ControlTree;

namespace TQVaultAE.GUI.Automation;

internal static partial class AutomationTools
{
	internal static partial class DevTools
	{
		#region Buffs tab of the statistics panels

		[McpServerTool(Name = "character_buffs", ReadOnly = true)]
		[Description("Shows the Buffs tab (tab IV) of a statistics panel, the character editor's, the main window's read-only one or a floating character window's, and reads it: the scroll choice (title, selected, drop-down), each section (Equipment, Passive, Active, Pets, Curses, Shrines, Tartarus, Scroll) with its sources: key, name, icon, on, bounds in client pixels when in sight, tool tip lines. 'scroll' scrolls the list first by game pixels (as the wheel). With capture=true the panel is also returned as a PNG (saved under %TEMP%).")]
		public static IEnumerable<ContentBlock> CharacterBuffs(
			[Description("'editor' (the open character editor), 'main' (the main window's panel, Equipment tab) or 'window:<name>' (a floating character window's Buffs tab).")] string panel = "editor",
			[Description("Game pixels to scroll the list by first: positive down, negative up.")] int scroll = 0,
			[Description("true to return a PNG of the panel too.")] bool capture = false)
		{
			var (state, png) = AutomationHost.OnUi(_ =>
			{
				var stats = FindStatsPanel(panel);
				stats.AutomationScrollBuffs(scroll);
				var state = Serialize(stats.AutomationBuffState());
				if (!capture)
					return (state, (byte[]?)null);

				using var bitmap = AutomationImages.Render(stats);
				return (state, AutomationImages.Png(bitmap));
			});

			return AutomationImages.Result(state, png, "characterbuffs");
		}

		[McpServerTool(Name = "character_buff_toggle")]
		[Description("In the Buffs tab of a statistics panel (the character editor's, the main window's or a floating character window's), clicks a source like the mouse (scrolled into sight, then the panel's hit test): it turns on or off and the statistics follow. Toggles live in memory only and are never saved, on any character. Or picks a scroll in the drop-down with scrollIndex (0 = none; the names are listed when the index is out of range). Returns the Buffs tab state.")]
		public static string CharacterBuffToggle(
			[Description("The source's key, from character_buffs ('/' accepted for a backslash); empty to only pick a scroll.")] string id = "",
			[Description("'editor', 'main' or 'window:<name>'.")] string panel = "editor",
			[Description("The scroll to pick in the drop-down (0 for none); omit to leave it.")] int? scrollIndex = null)
			=> AutomationHost.OnUi(_ =>
			{
				var stats = FindStatsPanel(panel);
				if (scrollIndex is { } index && !stats.AutomationPickScroll(index))
					throw new McpException($"No scroll {index}: {string.Join(" | ", stats.AutomationScrollNames().Select((n, i) => $"{i} {n}"))}.");

				// Record paths in keys may be written with '/' (JSON without escapes).
				if (!string.IsNullOrEmpty(id) && !stats.AutomationBuffClick(id.Replace('/', '\\')))
					throw new McpException($"No source '{id}' in the Buffs tab: use character_buffs for the keys.");

				return Serialize(stats.AutomationBuffState());
			});

		/// <summary>
		/// The statistics panel of the open character editor, of the main window (its Equipment tab), or a tab of a
		/// floating character window (<c>window:&lt;name&gt;</c>), its Buffs tab unless <paramref name="windowTab"/> says.
		/// </summary>
		private static CharacterStatsPanel FindStatsPanel(string panel, int windowTab = CharacterStatsPanel.BuffsTab)
			=> panel.StartsWith(AutomationPanels.CharacterWindowPrefix, StringComparison.OrdinalIgnoreCase)
				? FindCharacterWindow(AutomationHost.MainForm ?? throw new McpException("The main window is not ready."), panel[AutomationPanels.CharacterWindowPrefix.Length..])
					.StatsPanels[windowTab]
			: string.Equals(panel, "main", StringComparison.OrdinalIgnoreCase)
				? AllControls(AutomationHost.MainForm ?? throw new McpException("The main window is not ready."))
					.OfType<CharacterStatsPanel>().FirstOrDefault(p => p.Name == "PlayerStatsPanel" && p.Visible)
					?? throw new McpException("No statistics panel shown in the main window: select a character and show its Equipment tab.")
				: ShowCharacterPanel();

		#endregion
	}
}
