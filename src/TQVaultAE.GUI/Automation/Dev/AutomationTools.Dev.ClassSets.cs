using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Components.Character;
using TQVaultAE.GUI.Interop;

namespace TQVaultAE.GUI.Automation;

internal static partial class AutomationTools
{
	internal static partial class DevTools
	{
		[McpServerTool(Name = "class_sets", Destructive = true)]
		[Description("The class sets (saved builds: masteries with their skill points, attribute points) of the character in the open character editor: the bar under the mastery pages (bounds in the editor, field, mark and [+] parts, names, selected one, canChange, markToolTip: why the build shown differs from the selected set, null when it holds it), the file (path, exists, readFailed, lastApplied: the set selected at the last OK), the sets as edited (changed: something for OK to write) with each set's masteries, skills and attribute points and whether it holds the build shown (matches), the build shown now, the skill and attribute points left. menu='list' or 'actions' reads that menu of the bar. hover (mark, add or field) sends a mouse move to the centre of that part, then shows the tool tip it set as a hover does and returns it (hoverToolTip: part, text, the tool tip window's screen bounds), then hides it. With action, first does it like the user, through the bar: select (name) picks the set in the list, which applies it to the editor (a partly applied set shows a box); saveAs clicks [+] and types name into the name dialog (a new set holding the build shown, selected); save, rename (newName) and delete click their menu entry on the selected set (name picks it first, which applies it). All but select run after the call returns (save and delete confirm: queue_dialog_answers). OK (close_dialog) writes the file and applies the build to the character; Cancel drops both. Test characters only ('player-test', 'player-mod').")]
		public static string ClassSets(
			[Description("select, saveAs, save, rename or delete; none to only read.")] string? action = null,
			[Description("The set to select (select, and save / rename / delete instead of the selected one); the new set's name for saveAs.")] string? name = null,
			[Description("The new name for rename.")] string? newName = null,
			[Description("'list' or 'actions' to read that menu of the bar.")] string? menu = null,
			[Description("mark, add or field: rests the pointer on that part and returns its tool tip.")] string? hover = null)
			=> AutomationHost.OnUi(form =>
			{
				var editor = FindCharacterEditor();
				var bar = editor.ClassSetBar;
				if (!string.IsNullOrWhiteSpace(action))
				{
					if (!AutomationTestNames.IsTestFile(editor.PlayerCollection.PlayerName))
						throw new McpException($"Class set actions only on test characters ({AutomationTestNames.Accepted}).");

					var barAction = ParseEnum<ClassSetBarAction>(action, "action");
					if (name is not null && (barAction == ClassSetBarAction.Select || (barAction is not ClassSetBarAction.SaveAs && name != bar.Selected)))
						PickInBar(bar, name);
					if (barAction != ClassSetBarAction.Select)
						ListBarAct(form, bar, barAction, barAction == ClassSetBarAction.SaveAs ? name : newName);
				}

				object? menuEntries = menu is null ? null : BarMenuEntries(bar, menu);
				object? hoverToolTip = hover is null ? null : HoverBar(bar, hover);
				var current = editor.CaptureClassSet(string.Empty);
				object Describe(ClassSet set) => new
				{
					name = set.Name,
					matches = set.SameBuild(current),
					masteries = set.Masteries.Select(m => new
					{
						mastery = m.Mastery.ToString(),
						level = m.Level,
						skillPoints = m.Skills.Sum(s => s.Level),
						skills = m.Skills.Select(s => new { skill = s.Skill, level = s.Level }),
					}),
					attributes = set.Attributes,
				};
				var book = editor.ClassSetBook;
				var draft = editor.ClassSets;
				return Serialize(new
				{
					character = editor.PlayerCollection.PlayerName,
					bar = new
					{
						visible = bar.Visible,
						bounds = editor.RectangleToClient(bar.RectangleToScreen(bar.ClientRectangle)),
						field = bar.FieldBounds,
						add = bar.AddBounds,
						mark = bar.MarkBounds,
						markToolTip = bar.MarkToolTip,
						names = bar.Names,
						selected = bar.Selected,
						canChange = bar.CanChange,
						font = bar.TextFont?.ToString(),
					},
					menu = menuEntries,
					hoverToolTip,
					file = book.FilePath,
					fileExists = File.Exists(book.FilePath),
					readFailed = book.ReadFailed,
					lastApplied = book.LastApplied,
					changed = draft.Changed,
					sets = draft.Sets.Select(Describe),
					current = Describe(current),
					skillPointsLeft = editor.AutomationSkillPointsLeft,
					attributePointsLeft = editor.AutomationAttributePointsLeft,
				});
			});

		/// <summary>A mouse move to the centre of a part of the bar, then its tool tip shown as a hover shows it.</summary>
		private static object HoverBar(ClassSetBar bar, string part)
		{
			var bounds = part.Trim().ToLowerInvariant() switch
			{
				"mark" => bar.MarkBounds,
				"add" => bar.AddBounds,
				"field" => bar.FieldBounds,
				_ => throw new McpException($"Unknown part '{part}': mark, add or field."),
			};
			if (bounds.IsEmpty)
				throw new McpException($"The bar shows no {part} now.");

			const int WM_MOUSEMOVE = 0x0200;
			var center = AutomationInput.Center(bounds);
			User32.SendMessage(bar.Handle, WM_MOUSEMOVE, IntPtr.Zero, User32.MakeLParam(center));
			var text = bar.AutomationToolTip;
			if (string.IsNullOrEmpty(text))
				return new { part, text };

			bar.AutomationShowToolTip(center);
			var window = NativeWindows.GetProcessTopLevelWindows()
				.Where(w => w.ClassName.Contains("tooltips_class32", StringComparison.OrdinalIgnoreCase))
				.Select(w => NativeWindows.WindowRect(w.Handle))
				.FirstOrDefault(r => !r.IsEmpty);
			bar.AutomationHideToolTip();
			return new { part, text, window = window.IsEmpty ? (Rectangle?)null : window };
		}
	}
}
