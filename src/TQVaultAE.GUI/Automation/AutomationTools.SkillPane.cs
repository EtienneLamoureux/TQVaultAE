using System.ComponentModel;
using System.Drawing.Imaging;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Components.Character;

namespace TQVaultAE.GUI.Automation;

internal static partial class AutomationTools
{
	#region Skill pane

	[McpServerTool(Name = "skill_panel", ReadOnly = true)]
	[Description("In the open character editor, shows the page of a mastery slot and reads its skill pane as drawn: mastery, mastery level, bar fill, points left, and per skill its id, name, tier, button position, round or square border, level, item bonus, level text, state border (CanRaise green, NoPointLeft gold, Maxed blue, TierLocked gray, ParentLocked red), icon and number of connector pieces. With capture=true the pane is also returned as a PNG (saved under %TEMP%).")]
	public static IEnumerable<ContentBlock> SkillPanel(
		[Description("Mastery slot, from 1.")] int slot = 1,
		[Description("true to return a PNG of the pane too.")] bool capture = false)
	{
		var (state, png) = AutomationHost.OnUi(_ =>
		{
			var pane = ShowSkillPane(slot);
			var state = Serialize(pane.AutomationState());
			if (!capture)
				return (state, (byte[]?)null);

			using var bitmap = new Bitmap(pane.Width, pane.Height);
			pane.DrawToBitmap(bitmap, new Rectangle(Point.Empty, pane.Size));
			using var stream = new MemoryStream();
			bitmap.Save(stream, ImageFormat.Png);
			return (state, stream.ToArray());
		});

		var blocks = new List<ContentBlock> { new TextContentBlock { Text = state } };
		if (png is not null)
		{
			var path = Path.Combine(Path.GetTempPath(), $"tqvault-capture-skillpane{slot}-{DateTime.Now:yyyyMMdd-HHmmss-fff}.png");
			File.WriteAllBytes(path, png);
			blocks.Add(ImageContentBlock.FromBytes(png, "image/png"));
			blocks.Add(new TextContentBlock { Text = $"saved to {path}" });
		}

		return blocks;
	}

	[McpServerTool(Name = "skill_panel_click", Destructive = true)]
	[Description("In the open character editor, clicks a button of a mastery slot's skill pane like the mouse, through its hit test: left raises the skill (or the mastery) by one point, right lowers it, shift=true goes to the highest level the points allow or down to the lowest. The editor's rules apply (tier, parent skill, points left). Returns the pane state afterwards. Work only on claude-* test characters.")]
	public static string SkillPanelClick(
		[Description("Mastery slot, from 1.")] int slot,
		[Description("The skill: its record id, part of its name, or 'mastery' for the mastery button.")] string skill,
		[Description("'left' or 'right'.")] string button = "left",
		[Description("true for a Shift click.")] bool shift = false)
		=> AutomationHost.OnUi(_ =>
		{
			var pane = ShowSkillPane(slot);
			var step = button.ToLowerInvariant() switch
			{
				"left" => 1,
				"right" => -1,
				_ => throw new McpException($"Unknown button '{button}': 'left' or 'right'."),
			};

			MasterySkill? target = null;
			if (!string.Equals(skill, "mastery", StringComparison.OrdinalIgnoreCase))
			{
				var skills = pane.Tree?.Skills ?? throw new McpException($"Slot {slot} has no mastery.");
				target = skills.FirstOrDefault(s => string.Equals(s.Id.Raw, skill, StringComparison.OrdinalIgnoreCase))
					?? skills.FirstOrDefault(s => pane.Translate(s.NameTag).Contains(skill, StringComparison.OrdinalIgnoreCase))
					?? throw new McpException($"No skill '{skill}' in slot {slot}.");
			}

			var hit = pane.AutomationClick(target, step, shift)
				?? throw new McpException("The click hit no button: the hit test missed it.");
			return Serialize(new { clicked = hit is MasterySkill s ? s.Id.Raw : "mastery", state = pane.AutomationState() });
		});

	[McpServerTool(Name = "select_tab")]
	[Description("Selects a page of a tab control of an open form, like a click on its tab (for example the Character page or a mastery page of the character editor), so that capture shows it. Returns the pages of the tab controls of the form.")]
	public static string SelectTab(
		[Description("Part of the form title or its type name (case-insensitive).")] string form,
		[Description("The page: its index, name or part of its text.")] string tab)
		=> AutomationHost.OnUi(_ =>
		{
			var target = FindOpenForm(form);
			var controls = AllControls(target).OfType<TabControl>().ToList();
			var pages = controls.SelectMany(t => t.TabPages.Cast<TabPage>().Select((p, i) => (Tabs: t, Page: p, Index: i))).ToList();
			var found = (int.TryParse(tab, out var index) ? pages.FirstOrDefault(p => p.Index == index) : default);
			if (found.Page is null)
				found = pages.FirstOrDefault(p => string.Equals(p.Page.Name, tab, StringComparison.OrdinalIgnoreCase)
					|| p.Page.Text.Contains(tab, StringComparison.OrdinalIgnoreCase));
			if (found.Page is null)
				throw new McpException($"No page '{tab}' in '{target.Text}'. Pages: {string.Join(", ", pages.Select(p => $"'{p.Page.Text}'"))}");

			found.Tabs.SelectedTab = found.Page;
			return Serialize(controls.Select(t => new
			{
				tabControl = t.Name,
				selected = t.SelectedTab?.Text,
				pages = t.TabPages.Cast<TabPage>().Select(p => new { p.Name, p.Text }).ToList(),
			}).ToList());
		});

	/// <summary>Selects the page of a mastery slot in the open character editor and returns its pane.</summary>
	private static SkillPaneControl ShowSkillPane(int slot)
	{
		var form = FindOpenForm("CharacterEditDialog");
		var name = $"SkillsMastery{slot}Pane";
		var pane = AllControls(form).OfType<SkillPaneControl>().FirstOrDefault(p => p.Name == name)
			?? throw new McpException($"No skill pane for slot {slot}.");
		if (AllControls(form).OfType<TabControl>().FirstOrDefault(t => t.TabPages.Cast<TabPage>().Any(p => p.Contains(pane))) is { } tabs)
			tabs.SelectedTab = tabs.TabPages.Cast<TabPage>().First(p => p.Contains(pane));

		return pane;
	}

	#endregion
}
