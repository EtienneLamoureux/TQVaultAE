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
	[Description("In the open character editor, shows the Character page and reads its statistics panel (drawn like the game's character window) on tab 1, 2 or 3: name, class, level and XP lines, points line, attribute rows (label, value, tool tip), whether the + buttons show, leveling rows and values, masteries, tab III statistics, gold and Electrum, and the leveling editors (visible, enabled, text). With capture=true the panel is also returned as a PNG (saved under %TEMP%).")]
	public static IEnumerable<ContentBlock> CharacterPanel(
		[Description("Tab of the panel: 1 (I), 2 (II) or 3 (III).")] int tab = 1,
		[Description("true to return a PNG of the panel too.")] bool capture = false)
	{
		var (state, png) = AutomationHost.OnUi(_ =>
		{
			var panel = ShowCharacterPanel();
			panel.Tab = Math.Clamp(tab, 1, 3) - 1;
			var state = Serialize(panel.AutomationState());
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
			var path = Path.Combine(Path.GetTempPath(), $"tqvault-capture-characterpanel{tab}-{DateTime.Now:yyyyMMdd-HHmmss-fff}.png");
			File.WriteAllBytes(path, png);
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
		[Description("true for a Shift click.")] bool shift = false)
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
			if (!panel.AutomationClick(index, step, shift))
				throw new McpException("No + button there: the panel shows none while no attribute point is left.");

			return Serialize(panel.AutomationState());
		});

	/// <summary>Shows the Character page of the open character editor and returns its statistics panel.</summary>
	private static CharacterStatsPanel ShowCharacterPanel()
	{
		var form = FindOpenForm("CharacterEditDialog");
		var panel = AllControls(form).OfType<CharacterStatsPanel>().FirstOrDefault()
			?? throw new McpException("No statistics panel in the character editor.");
		if (AllControls(form).OfType<GameTabControl>().FirstOrDefault() is { } frame)
			frame.SelectedIndex = 0;

		return panel;
	}

	#endregion
}
