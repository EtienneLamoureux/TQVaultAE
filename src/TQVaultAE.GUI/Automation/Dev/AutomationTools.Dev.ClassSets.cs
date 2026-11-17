using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Components.Character;

namespace TQVaultAE.GUI.Automation;

internal static partial class AutomationTools
{
	internal static partial class DevTools
	{
		[McpServerTool(Name = "class_sets", Destructive = true)]
		[Description("The class sets (saved builds: masteries with their skill points, attribute points) of the character in the open character editor: the bar under the mastery pages (bounds in the editor, field and [+] parts, names, selected one, canChange, updateTargets), the file (path, exists, readFailed), the sets as edited (changed: written on OK) with each set's masteries, skills and attribute points and whether it holds the build shown (matches), the build shown now, the skill and attribute points left. menu='list' or 'actions' reads that menu of the bar. With action, first does it like the user, through the bar: select (name) picks the set in the list, which applies it to the editor (a partly applied set shows a box); add clicks [+] and types name into the name dialog; rename (newName) and delete click their menu entry on the selected set (name picks it first, which applies it); update (target) clicks the entry of the Update submenu naming that set (never the selected one). add, rename, delete and update run after the call returns (delete and update confirm: queue_dialog_answers). OK (close_dialog) writes the file and applies the build to the character; Cancel drops both. Test characters only ('player-test', 'player-mod').")]
		public static string ClassSets(
			[Description("select, add, rename, delete or update; none to only read.")] string? action = null,
			[Description("The set to select (select, and rename / delete instead of the selected one); the new set's name for add.")] string? name = null,
			[Description("The new name for rename.")] string? newName = null,
			[Description("The set update writes the build shown into.")] string? target = null,
			[Description("'list' or 'actions' to read that menu of the bar.")] string? menu = null)
			=> AutomationHost.OnUi(form =>
			{
				var editor = FindCharacterEditor();
				var bar = editor.ClassSetBar;
				if (!string.IsNullOrWhiteSpace(action))
				{
					if (!AutomationTestNames.IsTestFile(editor.PlayerCollection.PlayerName))
						throw new McpException($"Class set actions only on test characters ({AutomationTestNames.Accepted}).");

					var barAction = ParseEnum<ClassSetBarAction>(action, "action");
					if (name is not null && (barAction == ClassSetBarAction.Select || (barAction is ClassSetBarAction.Rename or ClassSetBarAction.Delete && name != bar.Selected)))
						PickInBar(bar, name);
					if (barAction != ClassSetBarAction.Select)
						ListBarAct(form, bar, barAction, barAction == ClassSetBarAction.Add ? name : newName, barAction == ClassSetBarAction.Update
							? target ?? throw new McpException("update needs target: the set to write the build into.")
							: null);
				}

				object? menuEntries = menu is null ? null : BarMenuEntries(bar, menu);
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
						names = bar.Names,
						selected = bar.Selected,
						canChange = bar.CanChange,
						updateTargets = bar.UpdateTargets,
						font = bar.TextFont?.ToString(),
					},
					menu = menuEntries,
					file = book.FilePath,
					fileExists = File.Exists(book.FilePath),
					readFailed = book.ReadFailed,
					changed = draft.Changed,
					sets = draft.Sets.Select(Describe),
					current = Describe(current),
					skillPointsLeft = editor.AutomationSkillPointsLeft,
					attributePointsLeft = editor.AutomationAttributePointsLeft,
				});
			});
	}
}
