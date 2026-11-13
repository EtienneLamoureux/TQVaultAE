using System.ComponentModel;
using System.Drawing.Imaging;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using TQVaultAE.GUI.Components.Character;

namespace TQVaultAE.GUI.Automation;

internal static partial class AutomationTools
{
	#region Character panel

	[McpServerTool(Name = "character_panel", ReadOnly = true)]
	[Description("In the open character editor, shows the Character page and reads its statistics panel (drawn like the game's character window) on tab 1, 2 or 3: name, class, level and XP lines, points line, attribute rows (label, value, tool tip), whether the + buttons show, leveling rows and values, tab III statistics, gold and Electrum, the values computed like the game (armor, resistances, tab II rows, tab III damage and race bonuses with their tool tips), and the side panel beside it (leveling rows, masteries, the statistics options Difficulty and Include buffs, and their editors: visible, enabled, text). With capture=true the panel is also returned as a PNG (saved under %TEMP%).")]
	public static IEnumerable<ContentBlock> CharacterPanel(
		[Description("Tab of the panel: 1 (I), 2 (II) or 3 (III).")] int tab = 1,
		[Description("true to return a PNG of the panel too.")] bool capture = false)
	{
		var (state, png) = AutomationHost.OnUi(_ =>
		{
			var panel = ShowCharacterPanel();
			panel.Tab = Math.Clamp(tab, 1, 3) - 1;
			var side = AllControls(FindCharacterEditor()).OfType<CharacterSidePanel>().FirstOrDefault();
			var state = Serialize(new { stats = panel.AutomationState(), side = side?.AutomationState() });
			if (!capture)
				return (state, (byte[]?)null);

			using var bitmap = new Bitmap(panel.Width, panel.Height);
			panel.DrawToBitmap(bitmap, new Rectangle(Point.Empty, panel.Size));
			using var stream = new MemoryStream();
			bitmap.Save(stream, ImageFormat.Png);
			return (state, stream.ToArray());
		});

		var blocks = new List<ContentBlock> { new TextContentBlock { Text = state } };
		if (png is not null)
		{
			var path = SaveCapture($"characterpanel{tab}", png);
			blocks.Add(ImageContentBlock.FromBytes(png, "image/png"));
			blocks.Add(new TextContentBlock { Text = $"saved to {path}" });
		}

		return blocks;
	}

	[McpServerTool(Name = "character_tooltip", ReadOnly = true)]
	[Description("In the open character editor, renders a rollover of the statistics panel's tab 1, 2 or 3 as the game-style tool tip shows it on hover: its lines (text, colour, title) and a PNG (saved under %TEMP%). The rollovers are numbered in the order of character_panel's areas with a tool tip on that tab (attribute rows, armor, icons, rows); without index, the list of their first lines.")]
	public static IEnumerable<ContentBlock> CharacterTooltip(
		[Description("Tab of the panel: 1 (I), 2 (II) or 3 (III).")] int tab,
		[Description("Rollover number from 0; omit for the list.")] int? index = null)
	{
		var (text, png) = AutomationHost.OnUi(_ =>
		{
			var panel = ShowCharacterPanel();
			panel.Tab = Math.Clamp(tab, 1, 3) - 1;
			var tips = panel.AutomationToolTips();
			if (index is null)
				return (Serialize(tips.Select((t, i) => new { index = i, first = t.Count > 0 ? t[0].Text : null }).ToList()), (byte[]?)null);

			if (index < 0 || index >= tips.Count)
				throw new McpException($"No rollover {index} on tab {tab}: 0 to {tips.Count - 1}.");

			var lines = tips[index.Value];
			var tooltip = panel.Tooltip ?? throw new McpException("The panel has no game tool tip.");
			using var bitmap = tooltip.Render(lines);
			using var stream = new MemoryStream();
			bitmap.Save(stream, ImageFormat.Png);
			return (Serialize(lines.Select(l => new { text = l.Text, color = l.Color.IsEmpty ? null : $"#{l.Color.R:X2}{l.Color.G:X2}{l.Color.B:X2}", title = l.Title }).ToList()), stream.ToArray());
		});

		var blocks = new List<ContentBlock> { new TextContentBlock { Text = text } };
		if (png is not null)
		{
			var path = SaveCapture("charactertooltip", png);
			blocks.Add(ImageContentBlock.FromBytes(png, "image/png"));
			blocks.Add(new TextContentBlock { Text = $"saved to {path}" });
		}

		return blocks;
	}

	[McpServerTool(Name = "character_attribute_click", Destructive = true)]
	[Description("In the open character editor, clicks the + button of an attribute on tab I of the statistics panel like the mouse, through its hit test: 'left' spends an attribute point, 'right' takes one back, shift=true all the points left or back to the value at the start. Attributes: health, energy, strength, intelligence, dexterity (or 0-4); 'undo' clicks the Undo button that replaces the Attributes title once an attribute changed (all back to the start). Refused when the button does not show (no + button while no point is left). Returns the panel state. Work only on claude-* test characters.")]
	public static string CharacterAttributeClick(
		[Description("health, energy, strength, intelligence, dexterity, 0-4, or undo.")] string attribute,
		[Description("'left' or 'right'.")] string button = "left",
		[Description("true for a Shift click.")] bool shift = false,
		[Description("Modifiers held for the click, like 'ctrl' or 'shift' (the key binds decide what they do); with shift=true Shift is added.")] string modifiers = "")
		=> AutomationHost.OnUi(_ =>
		{
			var panel = ShowCharacterPanel();
			panel.Tab = 0;
			if (string.Equals(attribute, "undo", StringComparison.OrdinalIgnoreCase))
			{
				if (!panel.AutomationUndo())
					throw new McpException("No Undo button: it shows only once an attribute changed.");

				return Serialize(panel.AutomationState());
			}

			string[] names = ["health", "energy", "strength", "intelligence", "dexterity"];
			var index = int.TryParse(attribute, out var n) ? n : Array.IndexOf(names, attribute.ToLowerInvariant());
			if (index is < 0 or > 4)
				throw new McpException($"Unknown attribute '{attribute}': {string.Join(", ", names)}.");

			var step = button.ToLowerInvariant() switch
			{
				"left" => 1,
				"right" => -1,
				_ => throw new McpException($"Unknown button '{button}': 'left' or 'right'."),
			};
			if (!panel.AutomationClick(index, step, ClickModifiers(modifiers, shift)))
				throw new McpException("No + button there: the panel shows none while no attribute point is left.");

			return Serialize(panel.AutomationState());
		});

	[McpServerTool(Name = "character_stat_sources", ReadOnly = true)]
	[Description("In the open character editor, returns the sums behind the statistics the Character page computes like the game: the variables of the equipped items and item sets (gear), of the skills that count (skills: passives, masteries, modifiers of counted skills, buffs when 'Include buffs' is ticked), both at their level, and the racial bonuses; plus the difficulty and buffs options. 'filter' keeps the variables whose name contains it (e.g. 'defensive').")]
	public static string CharacterStatSources(
		[Description("Part of the variable names to keep; empty for all.")] string filter = "")
		=> AutomationHost.OnUi(_ =>
		{
			return Serialize(FindCharacterEditor().AutomationStatSources(filter));
		});

	/// <summary>The character editor shown in the main window.</summary>
	private static Views.CharacterEditorUserControl FindCharacterEditor()
		=> AutomationHost.MainForm?.AutomationShownView(nameof(Views.CharacterEditorUserControl)) as Views.CharacterEditorUserControl
			?? throw new McpException($"The character editor is not shown (view shown: {AutomationHost.MainForm?.AutomationShownViewName}). Use open_character_editor.");

	/// <summary>Shows the Character page of the open character editor and returns its statistics panel.</summary>
	private static CharacterStatsPanel ShowCharacterPanel()
	{
		var form = FindCharacterEditor();
		var panel = AllControls(form).OfType<CharacterStatsPanel>().FirstOrDefault()
			?? throw new McpException("No statistics panel in the character editor.");
		if (AllControls(form).OfType<GameTabControl>().FirstOrDefault() is { } frame)
			frame.SelectedIndex = 0;

		return panel;
	}

	#endregion
}
