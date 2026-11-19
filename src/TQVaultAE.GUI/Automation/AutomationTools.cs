//-----------------------------------------------------------------------
// <copyright file="AutomationTools.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.GUI.Components.Panels;
using TQVaultAE.GUI.Components;
using TQVaultAE.GUI.Forms;
using TQVaultAE.Automation;
using static TQVaultAE.Automation.ControlTree;

namespace TQVaultAE.GUI.Automation;

/// <summary>
/// MCP tools that inspect and drive the running TQVault UI: the user tools, shipped in Release builds (reading the state,
/// the user's actions, captures; documentation/mcp.md). The Debug-only developer tools are in the nested
/// <see cref="DevTools"/> class, whose files (<c>Automation/Dev</c>) Release builds leave out.
/// Every UI access is marshalled to the UI thread through <see cref="AutomationHost.OnUi{T}(Func{MainForm, T})"/>; the tools
/// that read only thread-safe state (the log, the dialog answers, the recorded sounds, the registered forms, the game
/// files) and the native window calls run on the request thread.
/// </summary>
[McpServerToolType]
internal static partial class AutomationTools
{
	/// <summary>The serializer options of the tools: the one JSON format of <see cref="AutomationJson"/>.</summary>
	private static JsonSerializerOptions JsonOptions => AutomationJson.Options;

	private static readonly MethodInfo OnMouseDownMethod
		= typeof(Control).GetMethod("OnMouseDown", BindingFlags.Instance | BindingFlags.NonPublic)!;

	#region Windows

	[McpServerTool(Name = "list_windows", ReadOnly = true)]
	[Description("Lists TQVault's visible top-level windows (forms and native message boxes) with title, class, modal state and buttons; for TQVault's forms also their window: screen bounds, DPI, owner and its bounds, start position, taskbar button and icon.")]
	public static string ListWindows()
	{
		var forms = new Dictionary<IntPtr, (string Type, bool Modal, object Window)>();
		if (AutomationHost.MainForm is { IsHandleCreated: true })
		{
			AutomationHost.OnUi(_ =>
			{
				foreach (Form form in System.Windows.Forms.Application.OpenForms)
				{
					if (form.IsHandleCreated)
					{
						var window = new
						{
							bounds = form.Bounds,
							deviceDpi = form.DeviceDpi,
							owner = form.Owner?.Text,
							ownerBounds = form.Owner?.Bounds,
							startPosition = form.StartPosition.ToString(),
							showInTaskbar = form.ShowInTaskbar,
							showIcon = form.ShowIcon,
						};
						forms[form.Handle] = (form.GetType().Name, form.Modal, window);
					}
				}
			});
		}

		var windows = NativeWindows.GetProcessTopLevelWindows().Select(w =>
		{
			var isForm = forms.TryGetValue(w.Handle, out var info);
			return new
			{
				handle = $"0x{w.Handle.ToInt64():X}",
				title = w.Title,
				className = w.ClassName,
				formType = isForm ? info.Type : null,
				// A native dialog (#32770) shown by MessageBox/TaskDialog is always modal.
				modal = isForm ? info.Modal : w.IsDialogClass,
				enabled = w.Enabled,
				text = w.IsDialogClass ? NativeWindows.GetStaticTexts(w.Handle) : null,
				buttons = NativeWindows.GetButtons(w.Handle).Select(b => b.Text).Where(t => t.Length > 0).ToList(),
				window = isForm ? info.Window : null,
			};
		});

		return Serialize(new { endpoint = AutomationHost.Address, headless = AutomationHost.HeadlessDesktopName, windows });
	}

	[McpServerTool(Name = "close_dialog", Destructive = true)]
	[Description("Answers a modal dialog by pressing one of its buttons, e.g. title 'Default Vault Path', button 'OK'. Works for message boxes and WinForms dialogs, even when they are not in the foreground, and for the views shown in the main window: the settings (title 'Settings': OK or Cancel, pressed after the call returns, since OK may ask to restart) and the character editor (title 'Character Editor').")]
	public static string CloseDialog(
		[Description("Part of the dialog title (case-insensitive). Empty picks the only dialog that is open.")] string title,
		[Description("Button text, e.g. OK, Cancel, Yes, No.")] string button)
	{
		var candidates = NativeWindows.GetProcessTopLevelWindows()
			.Where(w => w.Enabled && (string.IsNullOrEmpty(title) || w.Title.Contains(title, StringComparison.OrdinalIgnoreCase)))
			.Where(w => w.IsDialogClass || NativeWindows.GetButtons(w.Handle).Count > 0)
			.ToList();

		// The settings are a view of the main window, not a dialog: their buttons are pressed like a click. A message
		// box they opened (same title, "Configure Settings") is answered first, as the user can only answer it then.
		if (candidates.Count == 0 && !string.IsNullOrEmpty(title)
			&& AutomationHost.OnUi(main => main.AutomationShownView(title) is { } view ? PressViewButton(main, view, button) : null) is { } viewResult)
			return viewResult;

		if (candidates.Count == 0)
			throw new McpException($"No enabled dialog with title containing '{title}'. Call list_windows to see what is open.");

		if (candidates.Count > 1 && string.IsNullOrEmpty(title))
			throw new McpException($"Several dialogs are open: {string.Join(", ", candidates.Select(c => $"'{c.Title}'"))}. Pass a title.");

		foreach (var dialog in candidates)
		{
			var pressed = NativeWindows.PressButton(dialog.Handle, button);
			if (pressed is not null)
				return Serialize(new { dialog = dialog.Title, pressed });
		}

		var available = candidates.SelectMany(c => NativeWindows.GetButtons(c.Handle)).Select(b => b.Text);
		throw new McpException($"No button '{button}' found. Buttons: {string.Join(", ", available)}");
	}

	/// <summary>
	/// Presses the button of a view of the main window whose text is <paramref name="button"/> (ignoring case and the
	/// mnemonic '&amp;'), after the call returns, as <c>click_control</c> does.
	/// </summary>
	private static string PressViewButton(Forms.MainForm main, Control view, string button)
	{
		static string Plain(string text) => text.Replace("&", string.Empty, StringComparison.Ordinal);
		var target = AllControls(view).OfType<Button>()
			.FirstOrDefault(b => b.Visible && b.Enabled && string.Equals(Plain(b.Text), Plain(button), StringComparison.OrdinalIgnoreCase))
			?? throw new McpException($"No button '{button}' in '{view.Text}'. Buttons: {string.Join(", ", AllControls(view).OfType<ButtonBase>().Where(b => b.Visible).Select(b => b.Text))}");
		main.BeginInvoke(() => ((IButtonControl)target).PerformClick());
		return Serialize(new { dialog = view.Text, pressed = target.Text });
	}

	#endregion

	#region State

	[McpServerTool(Name = "get_state", ReadOnly = true)]
	[Description("Returns the selected vault and character, the item panels shown, the drag state and, when 'panel' is given, the items in that panel's current sack (name, base name, cell location and size). Panels: vault, secondaryVault, player, playerBag, stash, equipment.")]
	public static string GetState(
		[Description("Panel whose items to list. Empty lists no items.")] string? panel = null)
		=> AutomationHost.OnUi(form =>
		{
			var drag = form.AutomationDragInfo;
			var panels = AutomationPanels.List(form);
			object? items = null;
			object? equipmentSlots = null;

			if (!string.IsNullOrEmpty(panel))
			{
				var named = AutomationPanels.Resolve(form, panel);
				items = named.Panel.Sack?.Select(i => DescribeItem(i, named.Panel.Sack)).Where(i => i is not null).ToList();
				if (named.Panel is EquipmentPanel)
					equipmentSlots = DescribeEquipmentSlots(named.Panel.Sack);
			}

			return Serialize(new
			{
				endpoint = AutomationHost.Address,
				loadingComplete = form.AutomationLoadingComplete,
				view = form.AutomationShownViewName,
				// The last message shown to the user in the main window (NotifyUser): an import's end message, for example.
				notification = form.AutomationNotificationText,
				vault = form.AutomationSelectedVault,
				vaultMod = form.AutomationSelectedVaultMod,
				secondaryVault = form.AutomationSelectedSecondaryVault,
				character = form.AutomationSelectedCharacter,
				characterFile = form.AutomationCurrentPlayerFile,
				characters = form.AutomationCharacters,
				vaults = form.AutomationVaults,
				playerReadonly = form.AutomationSettings.PlayerReadonly,
				drag = drag.IsActive
					? new
					{
						item = DescribeItem(drag.Item, null),
						sourcePanel = AutomationPanels.IdOf(form, drag.SrcSackPanel),
						canBeCanceled = drag.CanBeCanceled,
						isModified = drag.IsModifiedItem,
						mouseOffset = drag.MouseOffset,
					}
					: null,
				virtualCursor = VirtualCursor.Target is SackPanel cursorPanel
					? new { panel = AutomationPanels.IdOf(form, cursorPanel), x = VirtualCursor.ClientLocation.X, y = VirtualCursor.ClientLocation.Y }
					: null,
				panels = panels.Select(p => new
				{
					id = p.Id,
					shown = AutomationPanels.IsShown(p.Panel),
					type = p.Panel.GetType().Name,
					sackType = p.Panel.SackType.ToString(),
					hasSack = p.Panel.Sack is not null,
					currentBag = p.Container?.ShownBag(),
					bagCount = p.Container?.BagButtons?.Count,
					gridSize = p.Panel.SackSize,
					itemCount = p.Panel.Sack?.Count(i => !RecordId.IsNullOrEmpty(i.BaseItemId)),
					isModified = AutomationPanels.IsModified(p.Panel),
					// The sack belongs to a read-only character.
					isReadOnly = p.Panel.Sack?.IsReadOnly,
				}),
				items,
				equipmentSlots,
			});
		});

	#endregion

	#region Selection

	[McpServerTool(Name = "select_character", Destructive = true)]
	[Description("Selects a character in the character drop down, which loads the player, its equipment and its stashes. Names are listed by get_state.")]
	public static string SelectCharacter([Description("Character entry as listed in get_state 'characters'.")] string name)
		=> AutomationHost.OnUi(form =>
		{
			if (!form.AutomationSelectCharacter(name))
				throw new McpException($"Unknown character '{name}'. Characters: {string.Join(", ", form.AutomationCharacters)}");
			return Serialize(new { character = form.AutomationSelectedCharacter, characterFile = form.AutomationCurrentPlayerFile });
		});

	[McpServerTool(Name = "select_vault", Destructive = true)]
	[Description("Selects a vault in the main vault drop down, which loads it. Names are listed by get_state. The first entry ('Maintain Vault Files...') opens the modal maintenance dialog after this call returns: drive it with control_values, set_control and click_control (form 'VaultMaintenanceDialog').")]
	public static string SelectVault([Description("Vault name as listed in get_state 'vaults'.")] string name)
		=> AutomationHost.OnUi(form =>
		{
			if (!form.AutomationSelectVault(name))
				throw new McpException($"Unknown vault '{name}'. Vaults: {string.Join(", ", form.AutomationVaults)}");
			return Serialize(new { vault = form.AutomationSelectedVault });
		});

	[McpServerTool(Name = "export_vault")]
	[Description("Exports the vault shown to a JSON file, like the vault menu's 'Export to file' without its file dialog.")]
	public static string ExportVault([Description("Path of the JSON file to write.")] string path)
		=> AutomationHost.OnUi(form =>
		{
			var vault = form.AutomationExportVault(path) ?? throw new McpException("No vault is shown.");
			return Serialize(new { vault, path });
		});

	[McpServerTool(Name = "export_csv")]
	[Description("Writes the CSV export of the vault shown, or of one of its bags, to a file, like the vault context menu's 'Export CSV' entries without the clipboard. Uses the CSV delimiter of the settings.")]
	public static string ExportCsv(
		[Description("Path of the CSV file to write.")] string path,
		[Description("Zero-based index of the bag to export; omit for the whole vault.")] int? bag = null)
		=> AutomationHost.OnUi(form =>
		{
			var vault = form.AutomationExportCsv(path, bag) ?? throw new McpException("No vault is shown.");
			return Serialize(new { vault, bag, path });
		});

	[McpServerTool(Name = "import_file", Destructive = true)]
	[Description("Imports an exported JSON file like the vault menu's 'Import from file' without its file dialog: items go into the current tab; a whole vault asks to replace the vault shown or import it as a new vault (answer with queue_dialog_answers: Yes replaces, No creates a new vault, Cancel). The vault shown may be replaced (Debug builds: it must be a test vault, 'vault-...'). Queue the answers first: the import waits for its boxes.")]
	public static string ImportFile([Description("Path of the exported JSON file.")] string path)
		=> AutomationHost.OnUi(form =>
		{
			var shown = form.AutomationSelectedVault ?? string.Empty;
			RequireTestName(shown);
			form.AutomationImportFile(path);
			return Serialize(new { vault = form.AutomationSelectedVault, vaults = form.AutomationVaults });
		});

	[McpServerTool(Name = "import_text", Destructive = true)]
	[Description("Pastes text like Ctrl+V with the clipboard holding it, without touching the clipboard: exported JSON (an item, items, a tab or a vault, as copied by 'Share to Clipboard') or a PasteBin link. Items go into the current tab of the vault shown; a whole vault asks to replace the vault shown or import it as a new vault (queue_dialog_answers first: Yes replaces, No creates a new vault, Cancel). Give the text, or the path of a file holding it. The vault shown must be a test vault ('vault-...').")]
	public static async Task<string> ImportText(
		[Description("Text to paste; empty to read it from path.")] string? text = null,
		[Description("File holding the text to paste, when text is empty.")] string? path = null)
	{
		var form = AutomationHost.OnUi(form =>
		{
			RequireTestName(form.AutomationSelectedVault ?? string.Empty);
			return form;
		});
		string content = !string.IsNullOrEmpty(text) ? text
			: !string.IsNullOrEmpty(path) ? await File.ReadAllTextAsync(path).ConfigureAwait(false)
			: throw new McpException("Give the text to paste, or the path of a file holding it.");

		// Parsed off the UI thread (a PasteBin link is downloaded), placed on it.
		var result = await form.AutomationParseImportTextAsync(content).ConfigureAwait(false);
		return AutomationHost.OnUi(form =>
		{
			RequireTestName(form.AutomationSelectedVault ?? string.Empty);
			string? error = form.AutomationHandleImportResult(result);
			return Serialize(new { vault = form.AutomationSelectedVault, vaultMod = form.AutomationSelectedVaultMod, error });
		});
	}

	/// <summary>Debug builds: automation creates and changes only test characters and vaults (see <see cref="AutomationTestNames"/>).</summary>
	private static void RequireTestName(string name)
	{
		if (!AutomationTestNames.IsTestFile(name))
			throw new McpException($"Test files only: the name must start with {AutomationTestNames.Accepted}, e.g. 'vault-test1'.");
	}

	[McpServerTool(Name = "create_vault", Destructive = true)]
	[Description("Creates a vault like the New action of 'Maintain Vault Files...', then selects and loads it. Debug builds: the name must start with 'vault-' (e.g. vault-test1), automation works only in its own test files. The file is written when the vault is saved.")]
	public static string CreateVault([Description("New vault name (Debug builds: starting with 'vault-').")] string name)
	{
		RequireTestName(name);
		return AutomationHost.OnUi(form =>
		{
			var error = form.AutomationCreateVault(name);
			if (error != VaultMaintenanceError.None)
				throw new McpException($"Vault name '{name}' refused: {error}.");
			return Serialize(new { vault = form.AutomationSelectedVault });
		});
	}

	[McpServerTool(Name = "duplicate_character", Destructive = true)]
	[Description("Duplicates the selected character like the user (a full save set: player file, stash and all): clicks 'Duplicate Character' in the menu of the tools button next to the character list (menu_click('characterTools', 'Duplicate')), after this call returns, and types the name into its name dialog (queued first as its answer: the dialog's checks run, a refused name shows an error box, then the dialog again). The entry shows only when 'Allow character edit' is on in the settings. Debug builds: the name must start with 'player-test' or 'player-mod' (e.g. player-test-copy). On success TQVault restarts, as it does for a user: wait until get_state answers with loadingComplete, then select the copy; dialog_answers tells a refusal.")]
	public static string DuplicateCharacter([Description("Name of the copy (Debug builds: starting with 'player-test' or 'player-mod').")] string name)
	{
		RequireTestName(name);
		return AutomationHost.OnUi(form =>
		{
			var entry = form.AutomationCharacterCombo.AutomationDuplicateEntry;
			if (!entry.Available)
				throw new McpException("'Duplicate Character' is hidden: select a character, and turn on 'Allow character edit' in the settings.");
			ClickCharacterTool(form, [entry], new QueuedAnswer(DialogResult.OK, name));
			return Serialize(new { duplicating = form.AutomationSelectedCharacter, newName = name });
		});
	}


	[McpServerTool(Name = "save_files", Destructive = true)]
	[Description("Saves the modified characters, vaults and stashes like the Save button. A file error shows the Abort/Retry/Ignore box: this call then waits until the box is answered, so answer it from another call with close_dialog (button Abort, Retry or Ignore). Debug builds: work only in test files (player-test*, player-mod*, vault-*). Returns whether characters were saved and the panels still modified; see recent_log for the save's log lines.")]
	public static string SaveFiles()
		=> AutomationHost.OnUi(form =>
		{
			var playersSaved = form.AutomationSaveFiles();
			return Serialize(new
			{
				playersSaved,
				stillModified = AutomationPanels.List(form).Where(p => AutomationPanels.IsModified(p.Panel) == true).Select(p => p.Id).ToList(),
			});
		});

	#endregion

	#region Drag & drop

	[McpServerTool(Name = "select_bag", Destructive = true)]
	[Description("Clicks a bag button of a panel, e.g. panel 'stash' bag 0 shows the equipment panel, 'vault' bag 3 shows the 4th vault bag.")]
	public static string SelectBag(
		[Description("Panel id: vault, secondaryVault, playerBag, stash (or equipment).")] string panel,
		[Description("Zero-based bag button index.")] int bag)
		=> AutomationHost.OnUi(form =>
		{
			var container = AutomationPanels.ResolveContainer(form, panel).Container;

			container.RequireBag(bag);

			var button = container.BagButtons[bag];
			OnMouseDownMethod.Invoke(button, [AutomationInput.CenterClick(button, MouseButtons.Left)]);
			container.Refresh();
			return Serialize(new { panel, currentBag = container.ShownBag() });
		});

	[McpServerTool(Name = "pick_item", Destructive = true)]
	[Description("Picks up the item at a cell of a panel, exactly like a left click there. Cells are those reported by get_state.")]
	public static string PickItem(
		[Description("Panel id.")] string panel,
		[Description("Cell column.")] int x,
		[Description("Cell row.")] int y)
		=> AutomationHost.OnUi(form =>
		{
			var named = ResolveShownPanel(form, panel);
			var drag = form.AutomationDragInfo;
			if (drag.IsActive)
				throw new McpException("An item is already being dragged. Use drop_item or cancel_drag first.");

			EnsureWritable(named.Panel, "pick up");

			var cell = new Point(x, y);
			var item = named.Panel.AutomationFindItem(cell)
				?? throw new McpException($"No item at cell ({x},{y}) of '{panel}'.");

			named.Panel.AutomationMouseDown(named.Panel.AutomationCellCenter(cell), MouseButtons.Left);

			if (!drag.IsActive)
				throw new McpException($"TQVault did not pick up '{ItemName(item)}'. Notification: '{form.AutomationNotificationText}'. See recent_log.");

			return Serialize(DragStatus(form));
		});

	[McpServerTool(Name = "move_drag", Destructive = true)]
	[Description("Moves the dragged item over a panel so that its top left corner lands on cell (x, y), like hovering with the mouse. The real cursor does not move.")]
	public static string MoveDrag(
		[Description("Panel id.")] string panel,
		[Description("Target cell column of the item's top left corner.")] int x,
		[Description("Target cell row of the item's top left corner.")] int y)
		=> AutomationHost.OnUi(form =>
		{
			var named = ResolveShownPanel(form, panel);
			RequireDrag(form);
			named.Panel.AutomationMouseMove(DragCursorFor(form, named.Panel, new Point(x, y)));
			return Serialize(DragStatus(form));
		});

	[McpServerTool(Name = "drop_item", Destructive = true)]
	[Description("Drops the dragged item so that its top left corner lands on cell (x, y) of a panel, like a left click there. Dropping on an occupied cell swaps items (the other item is then dragged). Like a real drop, dropping it back on its own cells re-places it and marks the sack modified; use cancel_drag to put it back untouched.")]
	public static string DropItem(
		[Description("Panel id.")] string panel,
		[Description("Target cell column of the item's top left corner.")] int x,
		[Description("Target cell row of the item's top left corner.")] int y)
		=> AutomationHost.OnUi(form =>
		{
			var named = ResolveShownPanel(form, panel);
			var drag = RequireDrag(form);
			EnsureWritable(named.Panel, "drop into");

			var dropped = drag.Item;
			var cursor = DragCursorFor(form, named.Panel, new Point(x, y));
			named.Panel.AutomationMouseMove(cursor);
			var target = named.Panel.AutomationCellsUnderDragItem;
			named.Panel.AutomationMouseDown(cursor, MouseButtons.Left);

			var stillHeld = drag.IsActive && ReferenceEquals(drag.Item, dropped);
			if (!drag.IsActive)
				ReleaseCursor(named.Panel);

			return Serialize(new
			{
				accepted = !stillHeld,
				targetCells = target.Size.IsEmpty ? (Rectangle?)null : target,
				droppedItem = DescribeItem(dropped, null),
				status = DragStatus(form),
				hint = stillHeld ? "The panel refused the drop (cell out of range, occupied by several items, wrong slot, requirements or a read-only character: see notification). The item is still dragged." : null,
				notification = form.AutomationNotificationText,
			});
		});

	[McpServerTool(Name = "hover_item", Destructive = true)]
	[Description("Hovers a cell of a panel without dragging, like resting the mouse there: TQVault shows the item's tooltip and its name in the notification bar. Returns the item, the tooltips shown and a PNG of the tooltip, taken right away since the tooltip may close on its own. End the hover with hover_end.")]
	public static IEnumerable<ContentBlock> HoverItem(
		[Description("Panel id.")] string panel,
		[Description("Cell column.")] int x,
		[Description("Cell row.")] int y)
	{
		var (json, png) = AutomationHost.OnUi(form =>
		{
			var named = ResolveShownPanel(form, panel);
			RequireNoDrag(form, "An item is being dragged: use move_drag to hover with it, or cancel_drag first.");

			var cell = new Point(x, y);
			var item = named.Panel.AutomationFindItem(cell);
			named.Panel.AutomationMouseMove(named.Panel.AutomationCellCenter(cell));

			var tooltip = OpenTooltips().LastOrDefault();
			byte[]? image = null;
			if (tooltip is { Width: > 0, Height: > 0 })
			{
				using var bitmap = AutomationImages.Render(tooltip);
				image = AutomationImages.Png(bitmap);
			}

			return (Serialize(new
			{
				item = item is null ? null : DescribeItem(item, null),
				notification = form.AutomationNotificationText,
				tooltips = DescribeTooltips(),
			}), image);
		});

		return AutomationImages.Result(json, png);
	}

	[McpServerTool(Name = "context_menu", Destructive = true)]
	[Description("Right-clicks a cell of a panel, as the user would, and returns the context menu TQVault builds for it (separators as '-', sub menus with their entries), then closes the menu without choosing anything. Selected items count, as for a real right click.")]
	public static string ContextMenu(
		[Description("Panel id.")] string panel,
		[Description("Cell column.")] int x,
		[Description("Cell row.")] int y)
		=> AutomationHost.OnUi(form =>
		{
			var named = ResolveShownPanel(form, panel);
			RequireNoDrag(form, "An item is being dragged: a right click would cancel it. Use cancel_drag first.");

			var cell = new Point(x, y);
			var item = named.Panel.AutomationFindItem(cell);
			var entries = named.Panel.AutomationContextMenu(cell);
			ReleaseCursor(named.Panel);

			return Serialize(new { panel = named.Id, item = item is null ? null : DescribeItem(item, null), entries });
		});

	[McpServerTool(Name = "context_menu_click", Destructive = true)]
	[Description("Right-clicks a cell like context_menu, then clicks the entry found by its path of texts (' > ' between levels, e.g. 'Move Item To > Bag #2'; an exact text first, else the first entry containing it). The click runs after this call returns, since an entry may open a modal dialog (properties, seed, delete confirmation): follow with list_windows, dialog_answers or get_state.")]
	public static string ContextMenuClick(
		[Description("Panel id.")] string panel,
		[Description("Cell column.")] int x,
		[Description("Cell row.")] int y,
		[Description("Entry texts from the top level down, separated by ' > '.")] string path)
		=> AutomationHost.OnUi(form =>
		{
			var named = ResolveShownPanel(form, panel);
			RequireNoDrag(form, "An item is being dragged: a right click would cancel it. Use cancel_drag first.");

			var cell = new Point(x, y);
			var item = named.Panel.AutomationFindItem(cell);
			string clicked;
			try
			{
				clicked = named.Panel.AutomationClickContextMenu(cell, MenuPath(path));
			}
			finally
			{
				ReleaseCursor(named.Panel);
			}

			return Serialize(new { panel = named.Id, item = DescribeItem(item, null), clicked });
		});

	[McpServerTool(Name = "export_items")]
	[Description("Right-clicks a cell of a panel, then runs the item menu's 'Share Item > Share to File...' without its file dialog: the selection (select_items) when there is one, else the item under the cursor. Returns false in 'exported' when there was nothing to export. Never touches the clipboard or PasteBin.")]
	public static string ExportItems(
		[Description("Panel id.")] string panel,
		[Description("Cell column.")] int x,
		[Description("Cell row.")] int y,
		[Description("Path of the JSON file to write.")] string path)
		=> AutomationHost.OnUi(form =>
		{
			var named = ResolveShownPanel(form, panel);
			RequireNoDrag(form, "An item is being dragged: a right click would cancel it. Use cancel_drag first.");

			bool exported;
			try
			{
				exported = named.Panel.AutomationExportItemsToFile(new Point(x, y), path);
			}
			finally
			{
				ReleaseCursor(named.Panel);
			}

			return Serialize(new { panel = named.Id, exported, path });
		});

	[McpServerTool(Name = "export_tab")]
	[Description("Right-clicks a bag button of a vault panel like the user (which shows that bag), then runs its menu's 'Export tab > File' without the file dialog: the tab is written as exported JSON to path. Only the vault bags holding items have the entry: an empty bag, the character's bags and the stash tabs are refused. Never touches the clipboard or PasteBin.")]
	public static string ExportTab(
		[Description("Path of the JSON file to write.")] string path,
		[Description("Zero-based bag button index.")] int bag,
		[Description("Panel id of a vault: vault or secondaryVault.")] string panel = "vault")
		=> AutomationHost.OnUi(form =>
		{
			var container = AutomationPanels.ResolveContainer(form, panel).Container;
			return Serialize(new { panel, bag, exported = container.AutomationExportTab(bag, path), path });
		});

	[McpServerTool(Name = "click_cell", Destructive = true)]
	[Description("Left-clicks a cell of a panel like a user holding modifier keys, through the panel's mouse handler, so the key binds apply: 'ctrl+alt' duplicates the item into the cursor and 'alt' copies it by default (item copy must be allowed), 'ctrl' adds it to the selection. Returns the dragged item and the selection; put a picked item back with cancel_drag.")]
	public static string ClickCell(
		[Description("Panel id.")] string panel,
		[Description("Cell column.")] int x,
		[Description("Cell row.")] int y,
		[Description("Modifiers held, like 'ctrl+alt', 'alt', 'shift'; empty for none.")] string modifiers)
		=> AutomationHost.OnUi(form =>
		{
			var named = ResolveShownPanel(form, panel);
			RequireNoDrag(form);

			var held = ClickModifiers(modifiers);
			var item = named.Panel.AutomationFindItem(new Point(x, y));
			named.Panel.AutomationModifierClick(new Point(x, y), held);
			var drag = form.AutomationDragInfo;
			return Serialize(new
			{
				panel = named.Id,
				clicked = DescribeItem(item, null),
				modifiers = held.ToString(),
				dragged = drag.IsActive ? DescribeItem(drag.Item, null) : null,
				draggedIsNew = drag.IsActive && drag.Item is not null && item is not null && !ReferenceEquals(drag.Item, item),
				selected = named.Panel.AutomationSelectedItems.Select(i => DescribeItem(i, null)).ToList(),
			});
		});

	[McpServerTool(Name = "select_items", Destructive = true)]
	[Description("Ctrl-clicks cells of a panel like a user holding Ctrl, which adds each item to the selection (a selected item is removed). Returns the selected items. A plain pick_item or the 'Clear Selection' entry clears it.")]
	public static string SelectItems(
		[Description("Panel id.")] string panel,
		[Description("Cells as 'x,y;x,y'.")] string cells)
		=> AutomationHost.OnUi(form =>
		{
			var named = ResolveShownPanel(form, panel);
			RequireNoDrag(form);

			foreach (var pair in cells.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
			{
				var xy = pair.Split(',', StringSplitOptions.TrimEntries);
				if (xy.Length != 2 || !int.TryParse(xy[0], out int cx) || !int.TryParse(xy[1], out int cy))
					throw new McpException($"Bad cell '{pair}', expected 'x,y'.");
				named.Panel.AutomationCtrlClick(new Point(cx, cy));
			}

			ReleaseCursor(named.Panel);
			return Serialize(new { panel = named.Id, selected = named.Panel.AutomationSelectedItems.Select(i => DescribeItem(i, null)).ToList() });
		});

	[McpServerTool(Name = "hover_bag", Destructive = true)]
	[Description("Shows the tooltips of a bag button like resting the pointer on it: the summary of the bag's items and, for a bag with a custom label, the label above the button. Returns each tooltip shown with its size and the font of its text. Capture the summary with capture('tooltip'), the label with capture('tooltip:BagButtonLabelTooltip'). hide=true closes them.")]
	public static string HoverBag(
		[Description("Panel id: vault, secondaryVault, playerBag, stash.")] string panel,
		[Description("Zero-based bag button index.")] int bag,
		[Description("True to close the bag tooltip instead.")] bool hide = false)
		=> AutomationHost.OnUi(form =>
		{
			var container = AutomationPanels.ResolveContainer(form, panel).Container;

			container.RequireBag(bag);

			Tooltips.BagButtonTooltip.HideTooltip();
			Tooltips.BagButtonLabelTooltip.HideTooltip();
			if (!hide)
			{
				// Both tool tips the pointer shows: the bag's custom label above the button, and the summary of its items.
				var services = Services;
				Tooltips.BagButtonLabelTooltip.ShowTooltip(services, container.BagButtons[bag]);
				Tooltips.BagButtonTooltip.ShowTooltip(services, container.BagButtons[bag]);
			}

			return Serialize(new { panel, bag, tooltips = DescribeTooltips() });
		});

	[McpServerTool(Name = "hover_end", Destructive = true)]
	[Description("Ends a hover_item: the virtual cursor leaves the panel and the tooltip closes.")]
	public static string HoverEnd()
		=> AutomationHost.OnUi(form =>
		{
			ReleaseCursor(VirtualCursor.Target as SackPanel);
			return Serialize(new { tooltips = DescribeTooltips() });
		});

	[McpServerTool(Name = "cancel_drag", Destructive = true)]
	[Description("Cancels the current drag and returns the item to where it was picked up, like a right click.")]
	public static string CancelDrag()
		=> AutomationHost.OnUi(form =>
		{
			var drag = RequireDrag(form);
			if (!drag.CanBeCanceled)
				throw new McpException("This drag cannot be canceled (the item was split or modified). Drop it somewhere instead.");

			var source = drag.SrcSackPanel;
			drag.Cancel();
			source?.Refresh();
			ReleaseCursor(VirtualCursor.Target as SackPanel);
			return Serialize(DragStatus(form));
		});

	#endregion

	#region Capture

	[McpServerTool(Name = "capture", ReadOnly = true)]
	[Description("Renders a control to PNG with Control.DrawToBitmap, so it works when the window is hidden behind others. Target: a panel id (vault, player, equipment...), '<panel>.container' for the panel with its bag buttons, 'main' for the main window, 'playerInfo' for the stash panel with the character statistics panel and its Edit button (which DrawToBitmap leaves out) drawn over it, or part of a form title. 'shown=true' copies the pixels the window really shows on its desktop instead (PrintWindow from the desktop composition): use it when DrawToBitmap may differ from the screen (overlaps, transparent children).")]
	public static IEnumerable<ContentBlock> Capture(
		[Description("What to capture.")] string control = "main",
		[Description("Copy the pixels shown on the desktop instead of DrawToBitmap.")] bool shown = false)
	{
		var png = AutomationHost.OnUi(form =>
		{
			if (string.Equals(control, "playerInfo", StringComparison.OrdinalIgnoreCase))
			{
				using var withPlayerInfo = RenderStashWithPlayerInfo(form);
				return AutomationImages.Png(withPlayerInfo);
			}

			var target = ResolveCaptureTarget(form, control);
			if (target.Width <= 0 || target.Height <= 0)
				throw new McpException($"'{control}' has no size (is it shown?).");

			using var bitmap = shown ? NativeWindows.RenderAsShown(target) : AutomationImages.Render(target);
			return AutomationImages.Png(bitmap);
		});

		return AutomationImages.Result(null, png, control);
	}

	[McpServerTool(Name = "capture_menu", ReadOnly = true)]
	[Description("Opens a menu like the user and renders it to PNG with the sub menus opened along 'open' (' > ' between levels, e.g. 'Modded'), drawn to the right level with their entry, then closes it without choosing anything. Target: 'vaultTools' for the vault tools button next to the vault list, 'characterTools' for the tools button next to the character list (as it opens for the selected character), a panel id with x and y for the right-click menu of that cell, or '<panel>.sort' (vault.sort, player.sort, playerBag.sort, stash.sort...) for the right-click menu of that panel's sort button, '<panel>.bag<N>' (vault.bag3, stash.bag1...) for the right-click menu of that bag button (it selects that bag), 'loadouts' for the loadout bar's right-click menu, 'loadouts.list' for its drop-down list.")]
	public static IEnumerable<ContentBlock> CaptureMenu(
		[Description("'vaultTools', 'characterTools', a panel id, '<panel>.sort', '<panel>.bag<N>', 'loadouts' or 'loadouts.list'.")] string target,
		[Description("Cell column, for a panel.")] int x = 0,
		[Description("Cell row, for a panel.")] int y = 0,
		[Description("Sub menus to open, texts separated by ' > '; empty for the menu alone.")] string? open = null)
	{
		var path = MenuPath(open);
		var png = AutomationHost.OnUi(form =>
		{
			using var bitmap = string.Equals(target, "vaultTools", StringComparison.OrdinalIgnoreCase)
				? form.AutomationRenderVaultMenu(path)
				: string.Equals(target, "characterTools", StringComparison.OrdinalIgnoreCase)
				? form.AutomationCharacterCombo.AutomationRenderMenu(path)
				: target.EndsWith(".sort", StringComparison.OrdinalIgnoreCase)
				? RenderSortMenu(form, target[..^".sort".Length])
				: BagMenuTarget(target) is var (bagPanel, bag)
				? RenderBagMenu(form, bagPanel, bag, path)
				: target.StartsWith("loadouts", StringComparison.OrdinalIgnoreCase)
				? RenderLoadoutMenu(form, target)
				: AutomationPanels.Resolve(form, target).Panel.AutomationRenderContextMenu(new Point(x, y), path);
			return AutomationImages.Png(bitmap);
		});

		return AutomationImages.Result(null, png, $"menu-{target}");
	}

	/// <summary>The loadout bar's right-click menu ('loadouts') or drop-down list ('loadouts.list'), drawn as it opens.</summary>
	private static Bitmap RenderLoadoutMenu(MainForm form, string target)
	{
		var bar = form.AutomationStashPanel?.LoadoutBar ?? throw new AutomationRefusedException("The loadout bar is not built yet: show the Equipment tab first.");
		var menu = target.EndsWith(".list", StringComparison.OrdinalIgnoreCase) ? bar.ListMenu : bar.ActionMenu;
		menu.Show(bar, new Point(0, bar.Height));
		return AutomationMenus.Render(menu, []);
	}

	/// <summary>The panel and bag of a '<panel>.bag<N>' target, null for another target.</summary>
	private static (string Panel, int Bag)? BagMenuTarget(string target)
	{
		int dot = target.LastIndexOf(".bag", StringComparison.OrdinalIgnoreCase);
		return dot > 0 && int.TryParse(target.AsSpan(dot + ".bag".Length), out int bag) ? (target[..dot], bag) : null;
	}

	/// <summary>The right-click menu of <paramref name="panel"/>'s bag button <paramref name="bag"/>, drawn as it opens.</summary>
	private static Bitmap RenderBagMenu(MainForm form, string panel, int bag, IReadOnlyList<string> open)
	{
		var container = AutomationPanels.ResolveContainer(form, panel).Container;
		return container.AutomationRenderBagMenu(bag, open);
	}

	/// <summary>The sort menu of <paramref name="panel"/>'s sort button, drawn as it opens.</summary>
	private static Bitmap RenderSortMenu(MainForm form, string panel)
	{
		var (sortPanel, container) = AutomationPanels.ResolveContainer(form, panel, "sort button");
		return container.AutomationRenderSortMenu(sortPanel);
	}

	#endregion

	#region Helpers

	private static string Serialize(object value) => JsonSerializer.Serialize(value, JsonOptions);

	/// <summary>Refuses an action while an item is dragged, with <paramref name="message"/>.</summary>
	private static void RequireNoDrag(MainForm form, string message = "An item is being dragged. Use cancel_drag first.")
	{
		if (form.AutomationDragInfo.IsActive)
			throw new McpException(message);
	}

	/// <summary>The services of the forms and controls (the bundle each form gets).</summary>
	private static GuiServices Services => Service<GuiServices>();

	/// <summary>A service the forms do not get through <see cref="GuiServices"/> (tags, mod links, the databases).</summary>
	private static T Service<T>() where T : notnull => Program.ServiceProvider.GetRequiredService<T>();

	private static AutomationPanels.NamedPanel ResolveShownPanel(MainForm form, string panel)
	{
		var named = AutomationPanels.Resolve(form, panel);
		if (!AutomationPanels.IsShown(named.Panel) || named.Panel.Sack is null)
			throw new McpException($"Panel '{named.Id}' is not shown or has no sack. Use select_bag (stash bag 0 shows the equipment).");
		return named;
	}

	private static Models.ItemDragInfo RequireDrag(MainForm form)
	{
		var drag = form.AutomationDragInfo;
		if (!drag.IsActive)
			throw new McpException("No item is being dragged. Use pick_item first.");
		return drag;
	}

	/// <summary>
	/// Respects the PlayerReadonly setting, which the equipment panel enforces for real mouse clicks too, and the loadout
	/// shown read-only in a floating window. A read-only character's sack is left to the panel's own rules, which refuse
	/// and tell the user (the notification).
	/// </summary>
	private static void EnsureWritable(SackPanel panel, string action)
	{
		if (panel is CharacterWindowEquipmentPanel { ShowsLoadout: true })
			throw new McpException($"Cannot {action} the floating window's equipment: it shows a loadout, read-only.");
		if (panel.ItemsReadOnly && panel.Sack is not { IsReadOnly: true })
			throw new McpException($"Cannot {action} the equipment panel: the PlayerReadonly setting is on.");
	}

	/// <summary>
	/// Client location of the cursor that puts the dragged item's top left corner on <paramref name="cell"/>.
	/// </summary>
	private static Point DragCursorFor(MainForm form, SackPanel panel, Point cell)
	{
		var topLeft = panel.AutomationCellTopLeft(cell);
		var offset = form.AutomationDragInfo.MouseOffset;
		return new Point(topLeft.X + offset.X, topLeft.Y + offset.Y);
	}

	/// <summary>The tooltip windows shown, oldest first.</summary>
	private static List<Tooltips.BaseTooltip> OpenTooltips()
		=> System.Windows.Forms.Application.OpenForms.OfType<Tooltips.BaseTooltip>().Where(t => t.Visible).ToList();

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1859", Justification = "Anonymous JSON payload.")]
	private static object DescribeTooltips()
		=> OpenTooltips().Select(t => new
		{
			type = t.GetType().Name,
			x = t.Left,
			y = t.Top,
			width = t.Width,
			height = t.Height,
			// The working area of the tool tip's monitor (virtual screen pixels): a placement check per monitor.
			workingArea = Screen.FromControl(t).WorkingArea,
			// The font of the tool tip's first text control: shows a tool tip whose text grows or shrinks over time.
			font = t.Controls.Cast<Control>().FirstOrDefault(c => c is Label) is { } label
				? FontText(label.Font) : null,
		}).ToList();

	/// <summary>A font as the tools report it, "Name 9.75pt": with a dot whatever the UI language (OnUi sets its culture).</summary>
	private static string FontText(Font font) => FormattableString.Invariant($"{font.Name} {font.SizeInPoints:0.##}pt");

	private static void ReleaseCursor(SackPanel? panel)
	{
		VirtualCursor.Release();
		panel?.AutomationMouseLeave();
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1859", Justification = "Anonymous JSON payload.")]
	private static object DragStatus(MainForm form)
	{
		var drag = form.AutomationDragInfo;
		var hovered = VirtualCursor.Target as SackPanel;
		var cells = hovered?.AutomationCellsUnderDragItem ?? Rectangle.Empty;
		return new
		{
			dragging = drag.IsActive,
			item = drag.IsActive ? DescribeItem(drag.Item, null) : null,
			sourcePanel = drag.IsActive ? AutomationPanels.IdOf(form, drag.SrcSackPanel) : null,
			hoveredPanel = AutomationPanels.IdOf(form, hovered),
			cellsUnderItem = cells.Size.IsEmpty ? (Rectangle?)null : cells,
		};
	}

	private static Control ResolveCaptureTarget(MainForm form, string control)
	{
		if (string.Equals(control, "main", StringComparison.OrdinalIgnoreCase))
			return form;

		const string containerSuffix = ".container";
		if (control.EndsWith(containerSuffix, StringComparison.OrdinalIgnoreCase))
		{
			var id = control[..^containerSuffix.Length];
			return (Control?)AutomationPanels.Resolve(form, id).Container
				?? throw new McpException($"Panel '{id}' has no container.");
		}

		const string characterList = "characterList";
		if (control.StartsWith(characterList, StringComparison.OrdinalIgnoreCase))
			return form.AutomationCharacterCombo.AutomationOpenDropDown(control[characterList.Length..].TrimStart(':'));

		if (string.Equals(control, "tooltip", StringComparison.OrdinalIgnoreCase))
			return OpenTooltips().LastOrDefault()
				?? throw new McpException("No tooltip is shown. Use hover_item first.");

		// 'tooltip:<type>' picks one of several tool tips shown at once, e.g. 'tooltip:BagButtonLabelTooltip'.
		const string tooltipPrefix = "tooltip:";
		if (control.StartsWith(tooltipPrefix, StringComparison.OrdinalIgnoreCase))
		{
			var type = control[tooltipPrefix.Length..];
			return OpenTooltips().LastOrDefault(t => string.Equals(t.GetType().Name, type, StringComparison.OrdinalIgnoreCase))
				?? throw new McpException($"No tooltip of type '{type}' is shown.");
		}

		var panel = AutomationPanels.List(form).FirstOrDefault(p => string.Equals(p.Id, control, StringComparison.OrdinalIgnoreCase));
		if (panel is not null)
			return panel.Panel;

		foreach (Form open in System.Windows.Forms.Application.OpenForms)
		{
			if (open.Visible && open.Text.Contains(control, StringComparison.OrdinalIgnoreCase))
				return open;
		}

		// The settings or the character editor shown in the main window, by title or type name.
		if (form.AutomationShownView(control) is { } view)
			return view;

		throw new McpException($"Unknown capture target '{control}'. Use 'main', 'tooltip', 'characterList[:name]', a panel id ({string.Join(", ", AutomationPanels.List(form).Select(p => p.Id))}), '<panel>.container', a form title or the title of the view shown in the main window ({form.AutomationShownViewName}).");
	}

	private static string ItemName(Item item)
	{
		try
		{
			var names = Services.ItemText;
			return names.GetFriendlyNames(item).FullNameClean;
		}
		catch (Exception)
		{
			// Unknown records (mods, corrupted items) must not break the state dump.
			return item.BaseItemId.ToString();
		}
	}

	private static object? DescribeItem(Item? item, SackCollection? sack)
	{
		if (item is null || RecordId.IsNullOrEmpty(item.BaseItemId))
			return null;

		string name = item.BaseItemId.ToString();
		string? baseName = null;
		try
		{
			var names = Services.ItemText.GetFriendlyNames(item);
			name = names.FullNameClean;
			baseName = names.BaseItemInfoDescription?.RemoveAllTQTags();
		}
		catch (Exception)
		{
			// Unknown records (mods, corrupted items) must not break the state dump.
		}

		return new
		{
			name,
			baseName,
			baseItemId = item.BaseItemId.ToString(),
			// The mod its records were read from (null: the base game), the map it is read with (its container's; null:
			// the base game), and the records the loaded game data lacks (a mod not loaded): the tooltip shows the item
			// incomplete.
			mod = item.Mod,
			containerMod = item.ContainerMod,
			missingRecords = item.IsIncomplete ? item.MissingRecords.Select(m => new { role = m.Role.ToString(), id = m.Id.Raw }) : null,
			x = item.PositionX,
			y = item.PositionY,
			width = item.Width,
			height = item.Height,
			// Shown 1 x 1 for want of room for its real size where it lies.
			lacksRoom = item.LacksRoom ? true : (bool?)null,
			realWidth = item.LacksRoom ? item.RealWidth : (int?)null,
			realHeight = item.LacksRoom ? item.RealHeight : (int?)null,
			quantity = item.DoesStack ? item.Number : (int?)null,
			// What the sort orders read, and the shards a relic or charm holds (its completion bonus once complete).
			itemClass = item.ItemClass,
			rarity = item.Rarity.ToString(),
			requiredLevel = RequiredLevel(item),
			shards = item.IsRelicOrCharm ? item.Number : (int?)null,
			relicBonus = item.IsRelicOrCharm && !RecordId.IsNullOrEmpty(item.RelicBonusId) ? item.RelicBonusId.Raw : null,
			seed = item.Seed,
			equipmentSlot = sack?.SackType == SackType.Equipment ? EquipmentRules.FindSlot(sack, item) : (int?)null,
			// Where the search shows the item (its search result reads this place).
			place = new
			{
				containerName = item.Place.Name,
				container = item.Place.Path,
				sackType = item.Place.SackType.ToString(),
				sackNumber = item.Place.SackNumber,
				stashType = item.Place.StashType?.ToString(),
			},
		};
	}

	/// <summary>The level requirement of an item as the sort by level reads it, null when the records do not give one.</summary>
	private static int? RequiredLevel(Item item)
	{
		try
		{
			return Services.ItemProvider.GetRequirementInfo(item).Lvl;
		}
		catch (Exception)
		{
			// Unknown records must not break the state dump.
			return null;
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1859", Justification = "Anonymous JSON payload.")]
	private static object? DescribeEquipmentSlots(SackCollection? sack)
	{
		if (sack is null)
			return null;

		return Enumerable.Range(0, sack.NumberOfSlots).Select(slot =>
		{
			var upperLeft = SackCollection.GetEquipmentLocationOffset(slot);
			var size = SackCollection.GetEquipmentLocationSize(slot);
			if (SackCollection.IsWeaponSlot(slot))
			{
				size = SackCollection.WeaponLocationSize;
				upperLeft = SackCollection.GetWeaponLocationOffset(upperLeft.Y);
			}

			return new
			{
				slot,
				x = upperLeft.X,
				y = upperLeft.Y,
				width = size.Width,
				height = size.Height,
				weapon = SackCollection.IsWeaponSlot(slot),
			};
		}).ToList();
	}

	#endregion
}
