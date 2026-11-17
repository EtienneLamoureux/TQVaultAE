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

#if DEBUG
	/// <summary>When set, what the loadout actions take for "the game is running" instead of the real processes (Debug).</summary>
	internal static bool? GameRunningOverride { get; private set; }

	/// <summary>The description of the Debug-only parameters of <c>loadouts</c>.</summary>
	private const string LoadoutsDebugNote = " Debug builds: gameRunning=true/false makes the actions take the game as running or not; null uses the real processes.";
#else
	private const string LoadoutsDebugNote = "";
#endif

	[McpServerTool(Name = "loadouts", Destructive = true)]
	[Description("The loadouts (saved equipment sets) of the character shown in the stash panel: the loadout bar under the statistics panel (visible, bounds and its parts, names, selected one, whether they can change), the loadouts file, and each loadout's pieces by slot (record, seed, and whether the piece worn in that slot is identical). With menu='list' or 'actions', the entries of the bar's drop-down or right-click menu (text, enabled, checked, tip, and a submenu's items with their tip). The marks beside the equipment slots (slot, match: Same, Different or OneEmpty, canCompare, bounds in the equipment panel's pixels), which compare the worn pieces with the selected loadout. With action, first does it like the user, through the bar: select (name) picks it in the name box's list; add clicks the [+] button; update, rename, copy, delete and swap (Gear swap: the loadout's pieces and the worn ones change places) click the entry of the bar's right-click menu, and empty (to: inventory, stash or vault) the entry of its Empty loadout submenu (the pieces move to the main inventory bag, the stash an auto-move finds (the transfer stash while the equipment shows) or the bag of the vault shown; a piece without room stays in the loadout) — on the selected loadout unless name is given (then picked in the list first). A greyed action is refused, as the bar refuses it (swap and empty to inventory when PlayerReadonly is set: equipmentLocked; update and copy, which write copies, when Allow Item Copying is off: copyLocked). add (name), rename (newName) and copy (newName) type the name into the name dialog, queued first as its answer: its own checks run (a used or empty name shows an error box, then the dialog again: queue_dialog_answers). compare (slot) clicks the mark of a slot, which opens the Item Comparison of the worn piece and the loadout's in a new modeless window (after the call returns; comparison_windows lists and closes them). hover (slot) rests the pointer on the mark of a slot through the panel's mouse handling and returns its tool tip (markTooltip: the state against the loadout, then 'Click to compare' when a click compares); hover without slot leaves the panel. Choosing a loadout (select) opens the loadout panel in place of the statistics panel (panel: visible, bounds in the stash panel's pixels, the loadout shown, cellScaleFactor and unitSize of its cells, the bounds of its Back, previous, next, Gear swap (swap) and Empty loadout (empty) buttons in the view's pixels, canTurn (false with a single loadout), canSwap, swapLocked (why Gear swap is greyed: its tool tip), emptyLocked (why Empty loadout is greyed), modified when its pieces changed since the file was written, and its slots with their pieces); its items are driven as panel 'loadout' with pick_item, drop_item, cancel_drag, get_state and the other panel tools. back clicks the panel's Back arrow (any character), which shows the statistics again; previous and next click its arrows, which show the previous or next loadout of the list (the last followed by the first); swapButton clicks its Gear swap button and emptyButton (to) its Empty loadout button then the destination in the menu it opens (test characters). Actions other than select and compare run after the call returns (queue the answers of their boxes first: update and delete confirm, swap and update ask when the game runs) (Debug builds: only on test characters, names starting with 'player-test' or 'player-mod')." + LoadoutsDebugNote)]
	public static string Loadouts(
		[Description("select, add, update, rename, copy, delete, swap, empty, swapButton, emptyButton, compare, hover, back, previous or next; none to only read.")] string? action = null,
		[Description("The loadout (select, and the others instead of the selected one); the new loadout's name for add.")] string? name = null,
		[Description("The new name for rename and copy.")] string? newName = null,
		[Description("'list' or 'actions' to read that menu of the bar.")] string? menu = null,
		[Description("The target of empty and emptyButton: inventory, stash or vault.")] string? to = null,
#if DEBUG
		[Description("true or false to take the game as running or not for the next actions; omit to keep the current choice.")] bool? gameRunning = null,
		[Description("true to go back to the real game processes.")] bool realGame = false,
#endif
		[Description("The equipment slot whose mark compare clicks or hover rests on (0 head .. 11 artifact).")] int? slot = null)
		=> AutomationHost.OnUi(form =>
		{
#if DEBUG
			if (gameRunning is not null)
				GameRunningOverride = gameRunning;
			if (realGame)
				GameRunningOverride = null;
#endif

			var stash = form.AutomationStashPanel ?? throw new McpException("No stash panel.");
			var player = stash.Player;
			var bar = stash.LoadoutBar;
			var equipment = stash.EquipmentPanel;

			// Hovering a mark only shows its tool tip: any character.
			object? markTooltip = null;
			if (string.Equals(action?.Trim(), "hover", StringComparison.OrdinalIgnoreCase))
			{
				Domain.Actions.LoadoutSlotMark? hovered = slot is { } hoverSlot
					? equipment.LoadoutMarks.Where(m => m.Slot == hoverSlot).Select(m => (Domain.Actions.LoadoutSlotMark?)m).FirstOrDefault()
						?? throw new McpException("No mark at that slot (no loadout selected, or the slot is empty on both sides).")
					: null;
				markTooltip = equipment.AutomationHoverMark(hovered).Select(line => new { text = line.Text, color = AutomationJson.HexOrNull(line.Color) }).ToList();
				action = null;
			}

			// The buttons of the loadout panel only show the statistics or another loadout: any character.
			if (action?.Trim().ToLowerInvariant() is "back" or "previous" or "next" && stash.LoadoutView is var view0)
			{
				if (view0 is not { Visible: true } shown)
					throw new McpException("The loadout panel is not shown: choose a loadout first (action select).");
				var button = action.Trim().ToLowerInvariant() switch
				{
					"back" => shown.BackButton,
					"previous" => shown.PreviousButton,
					_ => shown.NextButton,
				};
				if (button != shown.BackButton && !shown.CanTurn)
					throw new McpException("The button is greyed: the character has a single loadout.");
				AutomationInput.SendClick(button, new Point(button.Width / 2, button.Height / 2), MouseButtons.Left, Keys.None);
				action = null;
			}

			// Comparing only reads the pieces: any character.
			if (string.Equals(action?.Trim(), "compare", StringComparison.OrdinalIgnoreCase))
			{
				// A user cannot click the panel while a modal dialog is open (a message box); the comparisons are modeless.
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
					if (string.Equals(action.Trim(), "emptyButton", StringComparison.OrdinalIgnoreCase))
				{
					// The Empty loadout button of the loadout panel opens the menu of destinations; the entry is clicked
					// after the call returns.
					if (stash.LoadoutView is not { Visible: true } view2)
						throw new McpException("The loadout panel is not shown: choose a loadout first (action select).");
					if (view2.EmptyLocked is { } emptyLocked)
						throw new McpException($"Empty loadout is greyed: {emptyLocked}");
					var target = EmptyTarget(to);
					var emptyButton = view2.EmptyButton;
					AutomationInput.SendClick(emptyButton, new Point(emptyButton.Width / 2, emptyButton.Height / 2), MouseButtons.Left, Keys.None);
					var entry = bar.EmptyMenu.Items.OfType<ToolStripItem>().First(i => i.Tag is LoadoutBarAction tagged && tagged == target);
					AutomationMenus.ClickLater(form, AutomationMenus.Clickable([entry]));
				}
				else if (string.Equals(action.Trim(), "swapButton", StringComparison.OrdinalIgnoreCase))
				{
					// The Gear swap button of the loadout panel, clicked after the call returns (it may ask when the game runs).
					if (stash.LoadoutView is not { Visible: true } view1)
						throw new McpException("The loadout panel is not shown: choose a loadout first (action select).");
					var swapButton = view1.SwapButton;
					if (view1.SwapLocked is { } locked)
						throw new McpException($"Gear swap is greyed: {locked}");
					form.BeginInvoke(() => AutomationInput.SendClick(swapButton, new Point(swapButton.Width / 2, swapButton.Height / 2), MouseButtons.Left, Keys.None));
				}
				else
				{
					var barAction = string.Equals(action.Trim(), "empty", StringComparison.OrdinalIgnoreCase)
						? EmptyTarget(to)
						: ParseEnum<LoadoutBarAction>(action, "action");

					// Another loadout than the selected one: picked in the bar's list first, as the user does.
					if (name is not null && (barAction == LoadoutBarAction.Select || (barAction != LoadoutBarAction.Add && name != bar.Selected)))
						PickInBar(bar, name);
					if (barAction != LoadoutBarAction.Select)
						ListBarAct(form, bar, barAction, barAction == LoadoutBarAction.Add ? name : newName);
				}
			}

			object? menuEntries = null;
			if (bar is not null && menu is not null)
			{
				menuEntries = BarMenuEntries(bar, menu);
			}

			var set = player is null ? null : form.Services.Loadouts.Find(Path.GetDirectoryName(player.PlayerFile) ?? string.Empty);
			var worn = player?.EquipmentSack?.ToList();
			return Serialize(new
			{
				character = player?.PlayerName,
				useAlternate = player?.UseAlternate,
#if DEBUG
				gameRunningOverride = GameRunningOverride,
#endif
				bar = bar is null ? null : new
				{
					visible = bar.Visible,
					bounds = stash.AutomationBounds(bar),
					field = bar.FieldBounds,
					add = bar.AddBounds,
					names = bar.Names,
					selected = bar.Selected,
					canChange = bar.CanChange,
					equipmentLocked = bar.EquipmentLocked,
					copyLocked = bar.CopyLocked?.Invoke(),
					font = bar.TextFont?.ToString(),
				},
				// Where the bar sits among its neighbours, in the stash panel's pixels: the statistics panel above, the
				// Edit button, and the panel's own size (nothing may go past its bottom).
				layout = bar is null ? null : new
				{
					statsPanel = stash.AutomationPlayerStatsPanel is { } stats ? stash.AutomationBounds(stats) : (Rectangle?)null,
					editButton = stash.AutomationEditButton is { } edit ? stash.AutomationBounds(edit) : (Rectangle?)null,
					editButtonVisible = stash.AutomationEditButton?.Visible,
					editButtonFont = stash.AutomationEditButton?.Font.ToString(),
					stashClient = stash.ClientRectangle,
				},
				menu = menuEntries,
				equipmentPanel = equipment.Bounds,
				marks = equipment.LoadoutMarks.Select(m => new
				{
					slot = m.Slot,
					match = m.Match.ToString(),
					canCompare = m.CanCompare,
					bounds = equipment.MarkScreenBounds(m),
				}),
				markTooltip,
				panel = stash.LoadoutView is not { } view ? null : new
				{
					visible = AutomationPanels.IsShown(view) && view.Visible,
					bounds = stash.AutomationBounds(view),
					loadout = view.Panel.LoadoutName,
					panelBounds = view.Panel.Bounds,
					cellScaleFactor = view.Panel.CellScaleFactor,
					unitSize = view.Panel.UnitSize,
					back = view.BackButton.Bounds,
					previous = view.PreviousButton.Bounds,
					next = view.NextButton.Bounds,
					canTurn = view.CanTurn,
					swap = view.SwapButton.Bounds,
					canSwap = view.CanSwap,
					swapLocked = view.SwapLocked,
					empty = view.EmptyButton.Bounds,
					emptyLocked = view.EmptyLocked,
					statsPanelVisible = stash.AutomationPlayerStatsPanel?.Visible,
					slots = view.Panel.Sack?.Take(view.Panel.Sack.NumberOfSlots).Select((item, index) => ItemKey.IsEmptyItem(item) ? null : new
					{
						slot = index,
						record = item.BaseItemId.Raw,
						seed = item.Seed,
						search = item.Place.SackType.ToString(),
						size = item.Size,
					}).Where(s => s is not null),
				},
				file = set?.FilePath,
				fileExists = set is not null && File.Exists(set.FilePath),
				readFailed = set?.ReadFailed,
				modified = set?.IsModified,
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


	/// <summary>The Empty loadout entry of a destination: inventory, stash or vault.</summary>
	private static LoadoutBarAction EmptyTarget(string? to) => to?.Trim().ToLowerInvariant() switch
	{
		"inventory" => LoadoutBarAction.EmptyToInventory,
		"stash" => LoadoutBarAction.EmptyToStash,
		"vault" => LoadoutBarAction.EmptyToVault,
		_ => throw new McpException("to must be inventory, stash or vault."),
	};

	/// <summary>
	/// The entries of a list bar's drop-down (<paramref name="menu"/> 'list') or right-click menu ('actions'), opened
	/// then closed: text, enabled, checked, tip, separator, and a submenu's items with their tip.
	/// </summary>
	internal static object BarMenuEntries<TAction>(GameListBar<TAction> bar, string menu)
		where TAction : struct, Enum
	{
		var strip = menu.Equals("list", StringComparison.OrdinalIgnoreCase) ? bar.ListMenu : bar.ActionMenu;
		strip.Show(bar, new Point(0, bar.Height));
		var entries = strip.Items.OfType<ToolStripItem>()
			.Select(i => new
			{
				text = i.Text,
				enabled = i.Enabled,
				@checked = (i as ToolStripMenuItem)?.Checked ?? false,
				tip = i.ToolTipText,
				separator = i is ToolStripSeparator,
				items = i is ToolStripMenuItem { HasDropDownItems: true } sub
					? sub.DropDownItems.OfType<ToolStripItem>().Select(e => new { text = e.Text, enabled = e.Enabled, tip = e.ToolTipText }).ToList()
					: null,
			})
			.ToList();
		strip.Close();
		return entries;
	}

	/// <summary>Selects an entry of a list bar (loadout, class set) like the user: a click on the bar's name box opens its list, then a click on the name.</summary>
	internal static void PickInBar<TAction>(GameListBar<TAction> bar, string name)
		where TAction : struct, Enum
	{
		var field = bar.FieldBounds;
		AutomationInput.SendClick(bar, new Point(field.X + (field.Width / 2), field.Y + (field.Height / 2)), MouseButtons.Left, Keys.None);
		try
		{
			var entry = bar.ListMenu.Items.OfType<ToolStripMenuItem>().FirstOrDefault(i => string.Equals(i.Tag as string, name, StringComparison.OrdinalIgnoreCase))
				?? throw new McpException($"No entry '{name}'. Entries: {string.Join(", ", bar.Names)}");
			entry.PerformClick();
		}
		finally
		{
			bar.ListMenu.Close();
		}
	}

	/// <summary>
	/// Does an action of a list bar (loadout, class set) like the user, after the calling tool has returned (it may open
	/// the name dialog or a confirmation): Add by a click on its button, the others by their entry of the bar's
	/// right-click menu, or of a submenu (<paramref name="target"/>: the entry of a submenu naming another set, as the
	/// class set bar's Update). <paramref name="typed"/> is queued first as the name dialog's answer.
	/// </summary>
	internal static void ListBarAct<TAction>(Forms.MainForm form, GameListBar<TAction> bar, TAction action, string? typed, string? target = null)
		where TAction : struct, Enum
	{
		if (!bar.CanAct(action))
			throw new McpException(bar.LockedReason(action) is { } locked && bar.CanChange
				? $"{action} is greyed: {locked}"
				: $"{action} is greyed: a user could not do it (nothing selected, or the character cannot change).");
		if (typed is not null)
			AutomationHost.QueueDialogAnswerFirst(new QueuedAnswer(DialogResult.OK, typed));

		if (EqualityComparer<TAction>.Default.Equals(action, bar.AddAction))
		{
			var button = bar.AddBounds;
			var center = new Point(button.X + (button.Width / 2), button.Y + (button.Height / 2));
			form.BeginInvoke(() => AutomationInput.SendClick(bar, center, MouseButtons.Left, Keys.None));
			return;
		}

		AutomationInput.SendContextClick(bar, new Point(bar.FieldBounds.Width / 2, bar.Height / 2));
		// An entry of the menu, or of a submenu (its parent first in the path).
		bool Carries(ToolStripItem item) => target is null
			? item.Tag is TAction tagged && EqualityComparer<TAction>.Default.Equals(tagged, action)
			: item.Tag is ListBarTarget<TAction> aimed && EqualityComparer<TAction>.Default.Equals(aimed.Action, action) && string.Equals(aimed.Name, target, StringComparison.OrdinalIgnoreCase);
		var top = bar.ActionMenu.Items.OfType<ToolStripMenuItem>().ToList();
		IReadOnlyList<ToolStripItem> chain = top.FirstOrDefault(Carries) is { } entry
			? [entry]
			: top.Select(parent => (parent, sub: parent.DropDownItems.OfType<ToolStripItem>().FirstOrDefault(Carries)))
				.Where(p => p.sub is not null).Select(p => (IReadOnlyList<ToolStripItem>)[p.parent, p.sub!]).FirstOrDefault()
				?? throw new McpException($"No menu entry for {action}{(target is null ? string.Empty : $" '{target}'")}.");
		AutomationMenus.ClickLater(form, AutomationMenus.Clickable(chain));
	}

	#endregion
}
