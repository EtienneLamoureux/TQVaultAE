using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Session;
using TQVaultAE.GUI.Components.Character;
using TQVaultAE.Automation;
using static TQVaultAE.Automation.ControlTree;

namespace TQVaultAE.GUI.Automation;

internal static partial class AutomationTools
{
	#region Form controls



	[McpServerTool(Name = "queue_dialog_answers", Destructive = true)]
	[Description("Queues answers for the next message boxes (comma separated: OK, Cancel, Yes, No, Abort, Retry, Ignore), so they are answered at once without being shown and nothing waits for a click. A box takes the next answer only when it is one of its buttons; an OK-only box takes only an OK answer, so an unexpected message keeps the answer meant for a question. 'text:<name>' answers the next text input dialog (the name asked by Duplicate Character and the loadout actions): the dialog opens, the text is typed and its OK button clicked, so its own checks run (Cancel cancels it; no comma in the text). 'clear' empties the queue and the record. Covers every message box, the startup notices included, and the vault import dialog (Yes replaces the vault, No imports it as a new vault, Cancel cancels). In headless mode a box without a queued answer is not shown either: it takes Cancel, No, Ignore or OK and is listed by dialog_answers; a text input dialog without one stays open (close_dialog, or set_control and click_control).")]
	public static string QueueDialogAnswers([Description("Answers in order, e.g. 'Cancel,No' or 'text:claude-set1,OK', or 'clear'.")] string answers)
	{
		if (string.Equals(answers.Trim(), "clear", StringComparison.OrdinalIgnoreCase))
			AutomationHost.ClearDialogAnswers();
		else
			AutomationHost.QueueDialogAnswers(answers.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(QueuedAnswer.Parse).ToList());

		return DialogAnswers();
	}

	[McpServerTool(Name = "dialog_answers", ReadOnly = true)]
	[Description("Lists the message boxes answered from the queue (caption, message, answer), the answers not used yet and, in headless mode, the boxes that found no queued answer (with the answer they took) and the native dialogs (file, folder, color, task dialogs) cancelled because nobody could see them.")]
	public static string DialogAnswers()
		=> Serialize(new
		{
			headless = AutomationHost.HeadlessDesktopName is not null,
			answered = AutomationHost.AnsweredDialogs,
			unanswered = AutomationHost.UnansweredDialogs,
			cancelledNativeDialogs = AutomationHost.CancelledNativeDialogs,
			pending = AutomationHost.PendingDialogAnswers.Select(a => a.ToString()).ToList(),
		});

	/// <summary>
	/// The shown form whose title has <paramref name="form"/> or whose type is named so, else the view shown in the main
	/// window in place of its main content (the settings), found the same way: the tools drive it as they drove the
	/// settings dialog.
	/// </summary>
	private static Control FindOpenForm(string form)
		=> System.Windows.Forms.Application.OpenForms.Cast<Form>().LastOrDefault(f => f.Visible
			&& (f.Text.Contains(form, StringComparison.OrdinalIgnoreCase) || string.Equals(f.GetType().Name, form, StringComparison.OrdinalIgnoreCase)))
		?? AutomationHost.MainForm?.AutomationShownView(form)
		?? throw new McpException($"No open form '{form}'. Open forms: {string.Join(", ", System.Windows.Forms.Application.OpenForms.Cast<Form>().Where(f => f.Visible).Select(f => $"'{f.Text}' ({f.GetType().Name})"))}; view shown in the main window: {AutomationHost.MainForm?.AutomationShownViewName}");

	/// <summary>The main window when <paramref name="form"/> is empty, else <see cref="FindOpenForm"/>.</summary>
	private static Control FormOrMain(Forms.MainForm main, string? form)
		=> string.IsNullOrEmpty(form) ? main : FindOpenForm(form);

	/// <summary>The control of <paramref name="target"/> named <paramref name="name"/> (ignoring case), at any depth.</summary>
	/// <param name="hint">Added to the error message.</param>
	private static Control FindControl(Control target, string name, string? hint = null)
		=> AllControls(target).FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase))
			?? throw new McpException($"No control '{name}' in '{target.Text}'.{hint}");

	/// <summary>The shown dialog of type <typeparamref name="T"/>; <paramref name="what"/> names it in the error message.</summary>
	private static T OpenDialog<T>(string what)
		where T : Form
		=> System.Windows.Forms.Application.OpenForms.OfType<T>().FirstOrDefault(f => f.Visible)
			?? throw new McpException($"The {what} is not open.");


	/// <summary>Raises a protected event method of a control (OnScroll, OnLeave...), as its own input handling does.</summary>
	private static void RaiseProtected(Control c, string method, EventArgs? args = null)
		=> c.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic, [args?.GetType() ?? typeof(EventArgs)])!.Invoke(c, [args ?? EventArgs.Empty]);

	private static bool IsInputControl(Control c)
		=> c is NumericUpDown or TrackBar or ComboBox or ListBox or TextBoxBase or CheckBox or RadioButton
		|| c.GetType().GetProperty("Checked", typeof(bool)) is not null;

	private static object? ValueOf(Control c) => c switch
	{
		NumericUpDown number => number.Value,
		TrackBar track => track.Value,
		ComboBox combo => combo.SelectedItem is not null ? combo.GetItemText(combo.SelectedItem) : combo.DropDownStyle != ComboBoxStyle.DropDownList && combo.Text.Length > 0 ? combo.Text : null,
		ListBox list => list.SelectedItem is not null ? list.GetItemText(list.SelectedItem) : null,
		TextBoxBase text => text.Text,
		Label label => label.Text,
		_ => c.GetType().GetProperty("Checked", typeof(bool))?.GetValue(c) ?? c.Text,
	};

	private static object DescribeValue(Control c) => new
	{
		name = c.Name,
		type = c.GetType().Name,
		enabled = c.Enabled,
		visible = c.Visible,
		value = ValueOf(c),
	};

	#endregion

	#region Character editor and tags

	[McpServerTool(Name = "archive_character", Destructive = true)]
	[Description("Archives (archived=true) or unarchives the selected character like the user: opens the menu of the tools button next to the character list and clicks 'Archived' when its tick differs (menu_click('characterTools', 'Archived')), before returning; TQVault moves its save folder to or from the archive folder. Debug builds: only test characters (names starting with 'claude-', 'player-test' or 'player-mod'); the 'Archive all' entries are not driven, since they move every character.")]
	public static string ArchiveCharacter([Description("true to archive, false to unarchive.")] bool archived)
		=> AutomationHost.OnUi(form =>
		{
			var combo = form.AutomationCharacterCombo;
			if (combo.SelectedItem is not PlayerSave ps)
				throw new McpException("No character is selected.");

			var entry = combo.AutomationArchiveEntry;
			var clicked = RunMenu(form, "characterTools", null, _ => entry.Checked == archived ? [] : [entry], now: true).Clicked;
			return Serialize(new { name = ps.Name, clicked = clicked is not null, isArchived = ps.IsArchived, folder = ps.Folder, selected = form.AutomationSelectedCharacter, characterFile = form.AutomationCurrentPlayerFile });
		});

	[McpServerTool(Name = "character_read_only", Destructive = true)]
	[Description("Reads the read-only state of the selected character: the 'Read-only' tick of the character menu, the badge of its entry in the character list, the read-only state of its sacks and the read-only attribute of each file TQVault writes for it. With toggle=true, first opens the character menu and clicks 'Read-only' like a user (menu_click('characterTools', 'Read-only')), which sets or clears that attribute (Debug builds: test characters only, names starting with 'claude-', 'player-test' or 'player-mod'). Returns the notification shown after the click (a refusal, an error).")]
	public static string CharacterReadOnly([Description("true to click 'Read-only' first; false to only read.")] bool toggle = false)
		=> AutomationHost.OnUi(form =>
		{
			var combo = form.AutomationCharacterCombo;
			bool? checkedBefore = null;
			if (toggle)
			{
				checkedBefore = combo.AutomationReadOnlyChecked;
				RunMenu(form, "characterTools", null, _ => [combo.AutomationReadOnlyEntry], now: true);
			}

			if (combo.SelectedItem is not PlayerSave ps)
				throw new McpException("No character is selected.");

			var service = form.Services.CharacterReadOnlyService;
			combo.AutomationOpenDropDown(ps.Name);
			bool badge = combo.AutomationShowsReadOnlyBadge(ps);
			var player = form.AutomationCurrentPlayer;
			return Serialize(new
			{
				name = ps.Name,
				checkedBefore,
				menuChecked = combo.AutomationReadOnlyChecked,
				saveIsReadOnly = ps.IsReadOnly,
				badge,
				playerIsReadOnly = player?.IsReadOnly,
				// The distinct states of its bags and equipment: one value when they all agree.
				sacksReadOnly = player?.Append(player.EquipmentSack!).OfType<TQVaultAE.Domain.Entities.SackCollection>().Select(s => s.IsReadOnly).Distinct().ToList(),
				filesReadOnly = service.IsReadOnly(ps.Folder),
				files = service.OwnedFiles(ps.Folder)
					.Where(form.Services.FileIO.Exists)
					.Select(f => new { file = Path.GetFileName(f), readOnly = form.Services.FileIO.IsReadOnly(f) }),
				notification = form.AutomationNotificationText,
			});
		});

	[McpServerTool(Name = "character_list", Destructive = true)]
	[Description("Opens the character list and reads it as drawn: 'header', the text of the selected character's box, and per entry its name, class, level, masteries, mod and the badges shown (Archived, Read-only, Modded and the link state). A linked character's class and masteries are its mod's names whatever character is selected. With refresh=true the entries are described again first, as after a tag change, with whatever map is active. With pick (a character name), first clicks its entry like the user, with mouse messages: the list scrolls to it, the click selects the character (loading it) and closes the list; picking the selected one leaves the list open, as for the user. Otherwise the list is left open.")]
	public static string CharacterList(
		[Description("true to describe the entries again before reading them.")] bool refresh = false,
		[Description("The name of a character to click in the list (as in get_state 'characters', before the comma); empty to only read.")] string? pick = null)
		=> AutomationHost.OnUi(form =>
		{
			if (refresh)
				form.AutomationCharacterCombo.RefreshItems();
			if (!string.IsNullOrWhiteSpace(pick))
			{
				form.AutomationCharacterCombo.AutomationPick(pick.Trim());
			}

			var combo = form.AutomationCharacterCombo;
			bool open = combo.AutomationDropDownOpen;
			var (header, entries) = combo.AutomationEntries();
			return Serialize(new { header, selected = form.AutomationSelectedCharacter, characterFile = form.AutomationCurrentPlayerFile, listOpen = open, entries });
		});

	[McpServerTool(Name = "tag_archive_all", Destructive = true)]
	[Description("Clicks 'Archive all' (archived=true) or 'Unarchive all' in the menu of a tag, like a user: TQVault moves the save folders of the characters having that tag. Debug builds: only test tags starting with 'claude-' whose characters are all claude-* test characters. Returns those characters with their state and folder.")]
	public static string TagArchiveAll(
		[Description("The test tag.")] string tag,
		[Description("true to archive, false to unarchive.")] bool archived)
		=> AutomationHost.OnUi(form =>
		{
			var characters = form.AutomationCharacterCombo.AutomationSetArchivedByTag(tag, archived)
				.Select(c => new { name = c.Name, isArchived = c.IsArchived, folder = c.Folder });
			return Serialize(new { tag, characters });
		});

	[McpServerTool(Name = "open_character_editor", Destructive = true)]
	[Description("Shows the character editor of the selected character in place of the main content, like the Edit button of the stash panel, before returning. Read and change it with control_values and set_control (form 'Character Editor'), then leave it with close_dialog('Character Editor', OK or Cancel) (OK applies the changes in memory; save_files writes them). Debug builds: work only on claude-* test characters.")]
	public static string OpenCharacterEditor()
		=> AutomationHost.OnUi(form =>
		{
			if (form.AutomationCurrentPlayerFile is null)
				throw new McpException("No character is selected. Use select_character first.");
			form.AutomationOpenCharacterEditor();
			return Serialize(new { view = form.AutomationShownViewName, character = form.AutomationSelectedCharacter });
		});

	[McpServerTool(Name = "tag_box", Destructive = true)]
	[Description("Types into a tag box of the character drop down's context menu and presses Enter, like a user: box 'add' (new tag, assigned to the selected character) or 'rename' (renames the tag 'tag'). A refused name shows an error box (answer it with close_dialog). Box 'delete' clicks Tags > 'text' > Delete in the menu of the tools button (menu_click), after this call returns, and answers its confirmation with OK; 'assign' and 'unassign' tick or untick the tag 'text' for the selected test character. Debug builds: names must start with 'claude-' and only test characters. Read the tags with list_tags.")]
	public static string TagBox(
		[Description("add, rename, delete, assign or unassign.")] string box,
		[Description("The text typed (add, rename) or the tag to delete, assign or unassign.")] string text,
		[Description("For rename: the tag being renamed.")] string? tag = null)
		=> AutomationHost.OnUi(form =>
		{
			var combo = form.AutomationCharacterCombo;

			// Test tags only; an empty or invalid name is allowed through to test the refusals.

			combo.AutomationRefreshTags();

			switch (box.ToLowerInvariant())
			{
				case "add":
				{
					if (!string.IsNullOrWhiteSpace(text) && !AutomationTestNames.IsTestTag(text))
						throw new McpException($"Test tags only: the name must start with '{AutomationTestNames.TagPrefix}'.");
					combo.AutomationTypeNewTag(text);
					break;
				}
				case "rename":
				{
					if (!AutomationTestNames.IsTestTag(tag))
						throw new McpException($"Test tags only: 'tag' must start with '{AutomationTestNames.TagPrefix}'.");
					combo.AutomationTypeTagRename(tag!, text);
					break;
				}
				case "delete":
				{
					if (!AutomationTestNames.IsTestTag(text))
						throw new McpException($"Test tags only: the name must start with '{AutomationTestNames.TagPrefix}'.");
					ClickCharacterTool(form, combo.AutomationTagDeletePath(text), new QueuedAnswer(DialogResult.OK));
					break;
				}
				case "assign":
				case "unassign":
				{
					if (!AutomationTestNames.IsTestTag(text))
						throw new McpException($"Test tags only: the name must start with '{AutomationTestNames.TagPrefix}'.");
					if (combo.SelectedItem is not PlayerSave ps || !AutomationTestNames.IsTestFile(ps.Name))
						throw new McpException($"Select a test character (name starting with {AutomationTestNames.Accepted}) first.");
					var tagItem = combo.AutomationTagEntry(text);
					// Like a click on the tag entry: its CheckedChanged handler assigns or unassigns the tag.
					// The menu was just rebuilt unticked, so tick it first to be able to untick it.
					tagItem.Checked = true;
					if (box.Equals("unassign", StringComparison.OrdinalIgnoreCase))
						tagItem.Checked = false;
					break;
				}
				default:
					throw new McpException("box must be add, rename, delete, assign or unassign.");
			}

			return Serialize(new { box, text, tag, sent = true });
		});

	[McpServerTool(Name = "list_tags", ReadOnly = true)]
	[Description("Lists the character tags (name and color), and the tags of the selected character.")]
	public static string ListTags()
		=> AutomationHost.OnUi(form =>
		{
			var tags = Program.ServiceProvider.GetRequiredService<ITagService>().Tags;
			var selected = form.AutomationCharacterCombo.SelectedItem as PlayerSave;
			return Serialize(new
			{
				tags = tags.Select(t => new { name = t.Key, color = AutomationJson.Hex(t.Value) }).ToList(),
				selectedCharacterTags = selected?.Tags.Keys.ToList(),
			});
		});

	/// <summary>
	/// Types <paramref name="text"/> in a menu text box and raises its Enter key handler after this call returns,
	/// since a refusal opens a modal error box.
	/// </summary>
	#endregion

	#region Player information panel

	[McpServerTool(Name = "player_info", ReadOnly = true)]
	[Description("Reads the statistics panel shown next to the equipment (stash bag 0): the game's character window, read only, as character_panel reads the editor's (name, class, level, attributes, the computed values of tabs I to III, gold), and requirementAttributes: the attributes the item requirements are checked against (strength, dexterity, intelligence, health, energy with the gear and skill bonuses). tab (1 to 3) shows that tab first. Not visible when no character is selected or another stash tab is shown.")]
	public static string PlayerInfo([Description("Tab to show first: 1, 2 or 3; 0 keeps the one shown.")] int tab = 0)
		=> AutomationHost.OnUi(form =>
		{
			var stash = form.AutomationStashPanel ?? throw new McpException("No stash panel.");
			var panel = stash.AutomationPlayerStatsPanel;
			if (panel is null)
				return Serialize(new { visible = false });

			if (tab is >= 1 and <= 3)
				panel.Tab = tab - 1;

			var info = stash.AutomationPlayerInfo;
			var requirementAttributes = info is null ? null : new
			{
				strength = info.CalculatedStrength,
				dexterity = info.CalculatedDexterity,
				intelligence = info.CalculatedIntelligence,
				health = info.CalculatedHealth,
				energy = info.CalculatedMana,
			};
			return Serialize(new { visible = panel.Visible, bounds = panel.Bounds, stats = panel.AutomationState(), requirementAttributes });
		});

	/// <summary>
	/// Renders the stash panel with the statistics panel drawn over it: <see cref="Control.DrawToBitmap"/> of the panel
	/// may leave a transparent child out.
	/// </summary>
	private static Bitmap RenderStashWithPlayerInfo(Forms.MainForm form)
	{
		var stash = form.AutomationStashPanel ?? throw new McpException("No stash panel.");
		var panel = stash.AutomationPlayerStatsPanel;
		if (panel is not { Visible: true })
			throw new McpException("The statistics panel is not shown: select a character and stash bag 0 (equipment).");

		var bitmap = AutomationImages.Render(stash);
		panel.DrawToBitmap(bitmap, panel.Bounds);
		if (stash.AutomationEditButton is { Visible: true } edit)
			edit.DrawToBitmap(bitmap, edit.Bounds);
		return bitmap;
	}

	#endregion
}
