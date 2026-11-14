using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Components.Character;
using TQVaultAE.GUI.Components.Panels;

namespace TQVaultAE.GUI.Automation;

internal static partial class AutomationTools
{
	#region Loadouts

	/// <summary>When set, what the loadout actions take for "the game is running" instead of the real processes.</summary>
	internal static bool? GameRunningOverride { get; private set; }

	[McpServerTool(Name = "loadouts", Destructive = true)]
	[Description("The loadouts (saved equipment sets) of the character shown in the stash panel: the loadout bar under the statistics panel (visible, bounds and its parts, names, selected one, whether they can change), the loadouts file, and each loadout's pieces by slot (record, seed, and whether the piece worn in that slot is identical). With menu='list' or 'actions', the entries of the bar's drop-down or right-click menu (text, enabled, checked). The marks beside the equipment slots (slot, match: Same, Different or OneEmpty, canCompare, bounds in the equipment panel's pixels), which compare the worn pieces with the selected loadout. With action, first does it like the user, through the bar: select (name) picks it in the name box's list; add and load click the [+] and Load buttons; update, rename, copy and delete click the entry of the bar's right-click menu — on the selected loadout unless name is given (then picked in the list first). A greyed action is refused, as the bar refuses it (Load when PlayerReadonly is set: loadLocked). add (name), rename (newName) and copy (newName) type the name into the name dialog, queued first as its answer: its own checks run (a used or empty name shows an error box, then the dialog again: queue_dialog_answers). compare (slot) clicks the mark of a slot, which opens the Item Comparison dialog of the worn piece and the loadout's (after the call returns; close it with close_dialog('Item Comparison', 'OK')). Actions other than select and compare run after the call returns (queue the answers of their boxes first: update and delete confirm, load and update ask when the game runs) and only on test characters (names starting with 'claude-', 'player-test' or 'player-mod'). gameRunning=true/false makes the actions take the game as running or not; null uses the real processes.")]
	public static string Loadouts(
		[Description("select, add, update, rename, copy, delete or load; empty to only read.")] string? action = null,
		[Description("The loadout (select, and the others instead of the selected one); the new loadout's name for add.")] string? name = null,
		[Description("The new name for rename and copy.")] string? newName = null,
		[Description("'list' or 'actions' to read that menu of the bar.")] string? menu = null,
		[Description("true or false to take the game as running or not for the next actions; omit to keep the current choice.")] bool? gameRunning = null,
		[Description("true to go back to the real game processes.")] bool realGame = false,
		[Description("The equipment slot whose mark compare clicks (0 head .. 11 artifact).")] int? slot = null)
		=> AutomationHost.OnUi(form =>
		{
			if (gameRunning is not null)
				GameRunningOverride = gameRunning;
			if (realGame)
				GameRunningOverride = null;

			var stash = form.AutomationStashPanel ?? throw new McpException("No stash panel.");
			var player = stash.Player;
			var bar = stash.LoadoutBar;
			var equipment = stash.EquipmentPanel;

			// Comparing only reads the pieces: any character.
			if (string.Equals(action?.Trim(), "compare", StringComparison.OrdinalIgnoreCase))
			{
				// A user cannot click the panel while a modal dialog is open (a comparison already shown, a box).
				if (System.Windows.Forms.Application.OpenForms.OfType<Form>().Any(f => f.Modal))
					throw new McpException("A modal dialog is open: close it first.");
				if (slot is not { } markSlot || equipment.LoadoutMarks.Where(m => m.Slot == markSlot).Select(m => (Domain.Actions.LoadoutSlotMark?)m).FirstOrDefault() is not { } mark)
					throw new McpException("No mark at that slot (no loadout selected, or the slot is empty on both sides).");
				// Through the panel's own click handler, at the middle of the mark.
				var bounds = equipment.MarkScreenBounds(mark);
				var center = new Point(bounds.X + (bounds.Width / 2), bounds.Y + (bounds.Height / 2));
				equipment.AutomationMouseDown(center, MouseButtons.Left);
				action = null;
			}

			if (!string.IsNullOrWhiteSpace(action))
			{
				if (player is null)
					throw new McpException("No character is shown.");
				if (!AutomationTestNames.IsTestFile(player.PlayerName))
					throw new McpException($"Loadout actions only on test characters ({AutomationTestNames.Accepted}).");
				if (bar is null)
					throw new McpException("The loadout bar is not built yet: show the Equipment tab first.");

				if (!bar.Visible)
					throw new McpException("The loadout bar is hidden: show the Equipment tab first.");
				if (!Enum.TryParse<LoadoutBarAction>(action.Trim(), true, out var barAction) || int.TryParse(action, out _))
					throw new McpException($"Unknown action '{action}'.");

				// Another loadout than the selected one: picked in the bar's list first, as the user does.
				if (name is not null && (barAction == LoadoutBarAction.Select || (barAction != LoadoutBarAction.Add && name != bar.Selected)))
					PickLoadout(bar, name);
				if (barAction != LoadoutBarAction.Select)
					LoadoutBarAct(form, bar, barAction, barAction == LoadoutBarAction.Add ? name : newName);
			}

			object? menuEntries = null;
			if (bar is not null && menu is not null)
			{
				var strip = menu.Equals("list", StringComparison.OrdinalIgnoreCase) ? bar.ListMenu : bar.ActionMenu;
				strip.Show(bar, new Point(0, bar.Height));
				menuEntries = strip.Items.OfType<ToolStripItem>()
					.Select(i => new { text = i.Text, enabled = i.Enabled, @checked = (i as ToolStripMenuItem)?.Checked ?? false, separator = i is ToolStripSeparator })
					.ToList();
				strip.Close();
			}

			var set = player is null ? null : form.Services.Loadouts.Find(Path.GetDirectoryName(player.PlayerFile) ?? string.Empty);
			var worn = player?.EquipmentSack?.ToList();
			return Serialize(new
			{
				character = player?.PlayerName,
				useAlternate = player?.UseAlternate,
				gameRunningOverride = GameRunningOverride,
				bar = bar is null ? null : new
				{
					visible = bar.Visible,
					bounds = Box(bar.Bounds),
					field = Box(bar.FieldBounds),
					add = Box(bar.AddBounds),
					load = Box(bar.LoadBounds),
					names = bar.Names,
					selected = bar.Selected,
					canChange = bar.CanChange,
					loadLocked = bar.LoadLocked,
					font = bar.TextFont?.ToString(),
				},
				// Where the bar sits among its neighbours, in the stash panel's pixels: the statistics panel above, the
				// Edit button, and the panel's own size (nothing may go past its bottom).
				layout = bar is null ? null : new
				{
					statsPanel = stash.AutomationPlayerStatsPanel is { } stats ? Box(stats.Bounds) : null,
					editButton = stash.AutomationEditButton is { } edit ? Box(edit.Bounds) : null,
					editButtonVisible = stash.AutomationEditButton?.Visible,
					editButtonFont = stash.AutomationEditButton?.Font.ToString(),
					stashClient = Box(stash.ClientRectangle),
				},
				menu = menuEntries,
				equipmentPanel = Box(equipment.Bounds),
				marks = equipment.LoadoutMarks.Select(m => new
				{
					slot = m.Slot,
					match = m.Match.ToString(),
					canCompare = m.CanCompare,
					bounds = Box(equipment.MarkScreenBounds(m)),
				}),
				file = set?.FilePath,
				fileExists = set is not null && File.Exists(set.FilePath),
				readFailed = set?.ReadFailed,
				loadouts = set?.Loadouts.Select(l => new
				{
					name = l.Name,
					useAlternate = l.UseAlternate,
					pieces = l.Pieces.Select((piece, slot) => piece is null ? null : new
					{
						slot,
						record = piece.BaseItemId.Raw,
						seed = piece.Seed,
						worn = worn is not null && slot < worn.Count && ItemIdentityComparer.Instance.Equals(worn[slot], piece),
						search = piece.Place.SackType.ToString(),
					}).Where(p => p is not null),
				}),
				worn = worn?.Select((item, slot) => ItemKey.IsEmptyItem(item) ? null : new { slot, record = item.BaseItemId.Raw, seed = item.Seed }).Where(w => w is not null),
				notification = form.AutomationNotificationText,
			});
		});

	private static int[] Box(Rectangle r) => [r.X, r.Y, r.Width, r.Height];

	/// <summary>Selects a loadout like the user: a click on the bar's name box opens its list, then a click on the name.</summary>
	private static void PickLoadout(LoadoutBar bar, string name)
	{
		var field = bar.FieldBounds;
		AutomationInput.SendClick(bar, new Point(field.X + (field.Width / 2), field.Y + (field.Height / 2)), MouseButtons.Left, Keys.None);
		try
		{
			var entry = bar.ListMenu.Items.OfType<ToolStripMenuItem>().FirstOrDefault(i => string.Equals(i.Tag as string, name, StringComparison.OrdinalIgnoreCase))
				?? throw new McpException($"No loadout '{name}'. Loadouts: {string.Join(", ", bar.Names)}");
			entry.PerformClick();
		}
		finally
		{
			bar.ListMenu.Close();
		}
	}

	/// <summary>
	/// Does an action on the selected loadout like the user, after the calling tool has returned (it may open the name
	/// dialog or a confirmation): Add and Load by a click on their button, the others by their entry of the bar's
	/// right-click menu. <paramref name="typed"/> is queued first as the name dialog's answer.
	/// </summary>
	private static void LoadoutBarAct(Forms.MainForm form, LoadoutBar bar, LoadoutBarAction action, string? typed)
	{
		if (!bar.CanAct(action))
			throw new McpException(action == LoadoutBarAction.Load && bar.LoadLocked is { } locked
				? $"Load is greyed: {locked}"
				: $"{action} is greyed: a user could not do it (no loadout selected, or the character cannot change).");
		if (typed is not null)
			AutomationHost.QueueDialogAnswerFirst(new QueuedAnswer(DialogResult.OK, typed));

		if (action is LoadoutBarAction.Add or LoadoutBarAction.Load)
		{
			var button = action == LoadoutBarAction.Add ? bar.AddBounds : bar.LoadBounds;
			var center = new Point(button.X + (button.Width / 2), button.Y + (button.Height / 2));
			form.BeginInvoke(() => AutomationInput.SendClick(bar, center, MouseButtons.Left, Keys.None));
			return;
		}

		AutomationInput.SendContextClick(bar, new Point(bar.FieldBounds.Width / 2, bar.Height / 2));
		var entry = bar.ActionMenu.Items.OfType<ToolStripMenuItem>().First(i => i.Tag is LoadoutBarAction tagged && tagged == action);
		AutomationMenus.ClickLater(form, [entry]);
	}

	#endregion
}
