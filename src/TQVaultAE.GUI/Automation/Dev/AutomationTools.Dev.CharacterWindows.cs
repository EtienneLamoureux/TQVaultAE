using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using TQVaultAE.Application.Session;
using TQVaultAE.GUI.Dialogs;

namespace TQVaultAE.GUI.Automation;

internal static partial class AutomationTools
{
	internal static partial class DevTools
	{
		#region Floating character windows

		[McpServerTool(Name = "character_window_open")]
		[Description("Opens the floating character window of a listed character, as the button beside Edit does (or brings its window to the front): its equipment and the four statistics tabs side by side, its own Buffs toggles. Opening writes nothing. Returns the window's state (as character_window).")]
		public static string CharacterWindowOpen(
			[Description("The character: its key (IT/Main/<name>), its name, or its full drop-down label.")] string character,
			[Description("Where to open it (screen pixels), as a window reopened at start; omit to cascade it on the main window.")] int? x = null,
			[Description("Top of the window, with x.")] int? y = null)
			=> AutomationHost.OnUi(form =>
			{
				var save = FindListedCharacter(form, character);
				var window = form.AutomationOpenCharacterWindow(save, x is { } left && y is { } top ? new System.Drawing.Point(left, top) : null)
					?? throw new McpException($"The character '{save.Key}' could not be read.");
				return Serialize(CharacterWindowState(window, full: false));
			});

		[McpServerTool(Name = "character_window", ReadOnly = true)]
		[Description("Reads the floating character windows: per window the character key, bounds, DPI, scale, read-only, the loadout shown in place of the equipment (null: the equipment worn) and the loadouts listed, the loaded save's instance id (changes when the file is read again), the equipment panel's id for the item tools (window:<name>, usable with click_cell, cell_info...), the equipped items, the numbers of tabs I to III (attributes, armor, resistances, rows, offense) and the Buffs toggles that are on. full=true adds each panel's whole view. With character, only that window; capture=true also returns it as a PNG (saved under %TEMP%).")]
		public static IEnumerable<ContentBlock> CharacterWindow(
			[Description("The character's key or name; empty for every window.")] string character = "",
			[Description("true to add each tab's whole view.")] bool full = false,
			[Description("true to return a PNG of the window (needs character).")] bool capture = false)
		{
			var (state, png) = AutomationHost.OnUi(form =>
			{
				var windows = string.IsNullOrEmpty(character) ? form.AutomationCharacterWindows : [FindCharacterWindow(form, character)];
				var state = Serialize(new
				{
					saved = form.AutomationSettings.CharacterWindows.Windows.Select(w => new { w.Character, w.X, w.Y }).ToList(),
					windows = windows.Select(w => CharacterWindowState(w, full)).ToList(),
				});
				if (!capture || windows.Count != 1)
					return (state, (byte[]?)null);

				using var bitmap = AutomationImages.Render(windows[0]);
				return (state, AutomationImages.Png(bitmap));
			});

			return AutomationImages.Result(state, png, "characterwindow");
		}

		[McpServerTool(Name = "character_window_loadout")]
		[Description("Shows a loadout's pieces, read-only, in place of a floating window's equipment, as its list under tab II does (the statistics follow), or the equipment worn; or turns the list like its arrows (the equipment worn, then each loadout, round). Writes nothing. Returns the window's state (as character_window).")]
		public static string CharacterWindowLoadout(
			[Description("The character's key or name.")] string character,
			[Description("The loadout's name; empty for the equipment worn. Ignored with step.")] string loadout = "",
			[Description("-1 or +1 to click the previous or next arrow instead; 0 to select by name.")] int step = 0)
			=> AutomationHost.OnUi(form =>
			{
				var window = FindCharacterWindow(form, character);
				if (step != 0)
					window.TurnLoadout(Math.Sign(step));
				else if (string.IsNullOrEmpty(loadout))
					window.ShowLoadout(null);
				else if (window.LoadoutNames.FirstOrDefault(n => string.Equals(n, loadout, StringComparison.OrdinalIgnoreCase)) is { } name)
					window.ShowLoadout(name);
				else
					throw new McpException($"No loadout '{loadout}' for {window.Save.Key}: {string.Join(" | ", window.LoadoutNames)}.");

				return Serialize(CharacterWindowState(window, full: false));
			});

		[McpServerTool(Name = "character_window_close")]
		[Description("Closes a floating character window as its close button does (the user's close: it is not reopened at the next start). Returns the windows left.")]
		public static string CharacterWindowClose([Description("The character's key or name.")] string character)
			=> AutomationHost.OnUi(form =>
			{
				FindCharacterWindow(form, character).Close();
				return Serialize(form.AutomationCharacterWindows.Select(w => w.Save.Key).ToList());
			});

		/// <summary>A listed character by key, name or drop-down label (case ignored).</summary>
		private static PlayerSave FindListedCharacter(Forms.MainForm form, string character)
		{
			var characters = form.AutomationListedCharacters;
			return characters.FirstOrDefault(c => string.Equals(c.Key, character, StringComparison.OrdinalIgnoreCase))
				?? characters.FirstOrDefault(c => string.Equals(c.Name, character, StringComparison.OrdinalIgnoreCase))
				?? characters.FirstOrDefault(c => string.Equals(c.ToString(), character, StringComparison.OrdinalIgnoreCase))
				?? throw new McpException($"No listed character '{character}': {string.Join(" | ", characters.Select(c => c.Key))}.");
		}

		/// <summary>The floating window of a character by key or name (case ignored).</summary>
		private static CharacterWindow FindCharacterWindow(Forms.MainForm form, string character)
			=> form.AutomationCharacterWindows.FirstOrDefault(w => string.Equals(w.Save.Key, character, StringComparison.OrdinalIgnoreCase)
					|| string.Equals(w.Save.Name, character, StringComparison.OrdinalIgnoreCase))
				?? throw new McpException($"No floating window of '{character}': {string.Join(" | ", form.AutomationCharacterWindows.Select(w => w.Save.Key))}.");

		/// <summary>What a floating window shows.</summary>
		private static object CharacterWindowState(CharacterWindow window, bool full)
		{
			var tabs = window.StatsPanels;
			var view = tabs[0].View?.Invoke();
			var stats = view?.Stats;
			return new
			{
				character = window.Save.Key,
				title = window.Text,
				bounds = window.Bounds,
				dpi = window.DeviceDpi,
				scale = tabs[0].PanelScale,
				cellSize = window.EquipmentPanel.UnitSize,
				readOnly = window.Player.IsReadOnly,
				loadout = window.LoadoutName,
				loadouts = window.LoadoutNames,
				panel = AutomationPanels.CharacterWindowPrefix + window.Save.Name,
				sharedWithMain = ReferenceEquals(window.Player, AutomationHost.MainForm?.AutomationStashPanel?.Player),
				playerInstance = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(window.Player),
				equipment = window.EquipmentPanel.Sack?.Where(i => !i.IsEmpty)
					.Select(i => new { slot = Domain.Actions.EquipmentRules.FindSlot(window.EquipmentPanel.Sack, i), id = i.BaseItemId.ToString() }).ToList(),
				attributes = view?.Attributes.Select(a => new { a.Label, a.Value }).ToList(),
				armor = stats?.Armor.Value,
				resistances = stats?.Resistances.Select(r => r.Value).ToList(),
				offense = stats?.Offense.Select(r => new { r.Label, r.Value }).ToList(),
				tab2 = stats?.Tab2Rows.Select(r => new { r.Label, r.Value }).ToList(),
				tab3 = stats?.Tab3Rows.Select(r => new { r.Label, r.Value }).ToList(),
				buffsOn = view?.Buffs.Sections.SelectMany(s => s.Entries).Where(e => e.On).Select(e => e.Id).ToList(),
				tabs = full ? tabs.Select(t => t.AutomationState()).ToList() : null,
			};
		}

		#endregion
	}
}
