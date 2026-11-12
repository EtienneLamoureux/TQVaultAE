//-----------------------------------------------------------------------
// <copyright file="AutomationTools.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.GUI.Components.Panels;
using TQVaultAE.GUI.Components;
using TQVaultAE.GUI.Forms;

namespace TQVaultAE.GUI.Automation;

/// <summary>
/// MCP tools that inspect and drive the running TQVault UI.
/// Every UI access is marshalled to the UI thread through <see cref="AutomationHost.OnUi{T}"/>.
/// </summary>
[McpServerToolType]
internal static partial class AutomationTools
{
	private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

	private static readonly MethodInfo OnMouseDownMethod
		= typeof(Control).GetMethod("OnMouseDown", BindingFlags.Instance | BindingFlags.NonPublic)!;

	#region Windows

	[McpServerTool(Name = "list_windows", ReadOnly = true)]
	[Description("Lists TQVault's visible top-level windows (forms and native message boxes) with title, class, modal state and buttons.")]
	public static string ListWindows()
	{
		var forms = new Dictionary<IntPtr, (string Type, bool Modal)>();
		if (AutomationHost.MainForm is { IsHandleCreated: true })
		{
			AutomationHost.OnUi(_ =>
			{
				foreach (Form form in System.Windows.Forms.Application.OpenForms)
				{
					if (form.IsHandleCreated)
						forms[form.Handle] = (form.GetType().Name, form.Modal);
				}
				return 0;
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
			};
		});

		return Serialize(new { endpoint = AutomationHost.Address, headless = Headless.DesktopName, windows });
	}

	[McpServerTool(Name = "close_dialog", Destructive = true)]
	[Description("Answers a modal dialog by pressing one of its buttons, e.g. title 'Default Vault Path', button 'OK'. Works for message boxes and WinForms dialogs, even when they are not in the foreground.")]
	public static string CloseDialog(
		[Description("Part of the dialog title (case-insensitive). Empty picks the only dialog that is open.")] string title,
		[Description("Button text, e.g. OK, Cancel, Yes, No.")] string button)
	{
		var candidates = NativeWindows.GetProcessTopLevelWindows()
			.Where(w => w.Enabled && (string.IsNullOrEmpty(title) || w.Title.Contains(title, StringComparison.OrdinalIgnoreCase)))
			.Where(w => w.IsDialogClass || NativeWindows.GetButtons(w.Handle).Count > 0)
			.ToList();

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
						mouseOffset = new { x = drag.MouseOffset.X, y = drag.MouseOffset.Y },
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
					gridSize = new { width = p.Panel.SackSize.Width, height = p.Panel.SackSize.Height },
					itemCount = p.Panel.Sack?.Count(i => !RecordId.IsNullOrEmpty(i.BaseItemId)),
					isModified = p.Panel.Sack?.IsModified,
					// The sack belongs to a read-only character.
					isReadOnly = p.Panel.Sack?.IsReadOnly,
				}),
				items,
				equipmentSlots,
			});
		});

	[McpServerTool(Name = "recent_log", ReadOnly = true)]
	[Description("Returns the last log lines written by TQVault, oldest first.")]
	public static string RecentLog([Description("Number of lines (default 50).")] int n = 50)
		=> string.Join(Environment.NewLine, AutomationHost.RecentLog.GetLast(Math.Clamp(n, 1, 2000)));

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
	[Description("Imports an exported JSON file like the vault menu's 'Import from file' without its file dialog: items go into the current tab; a whole vault asks to replace the vault shown or import it as a new vault (answer with queue_dialog_answers: Yes replaces, No creates a new vault, Cancel). The vault shown must be a test vault ('claude-...'), since it may be replaced. Queue the answers first: the import waits for its boxes.")]
	public static string ImportFile([Description("Path of the exported JSON file.")] string path)
		=> AutomationHost.OnUi(form =>
		{
			var shown = form.AutomationSelectedVault ?? string.Empty;
			RequireTestName(shown);
			form.AutomationImportFile(path);
			return Serialize(new { vault = form.AutomationSelectedVault, vaults = form.AutomationVaults });
		});

	[McpServerTool(Name = "import_text", Destructive = true)]
	[Description("Pastes text like Ctrl+V with the clipboard holding it, without touching the clipboard: exported JSON (an item, items, a tab or a vault, as copied by 'Share to Clipboard') or a PasteBin link. Items go into the current tab of the vault shown; a whole vault asks to replace the vault shown or import it as a new vault (queue_dialog_answers first: Yes replaces, No creates a new vault, Cancel). Give the text, or the path of a file holding it. The vault shown must be a test vault ('claude-...').")]
	public static string ImportText(
		[Description("Text to paste; empty to read it from path.")] string? text = null,
		[Description("File holding the text to paste, when text is empty.")] string? path = null)
		=> AutomationHost.OnUi(form =>
		{
			RequireTestName(form.AutomationSelectedVault ?? string.Empty);
			string content = !string.IsNullOrEmpty(text) ? text
				: !string.IsNullOrEmpty(path) ? File.ReadAllText(path)
				: throw new McpException("Give the text to paste, or the path of a file holding it.");
			string? error = form.AutomationImportText(content);
			return Serialize(new { vault = form.AutomationSelectedVault, vaultMod = form.AutomationSelectedVaultMod, error });
		});

	/// <summary>Automation's own test tags (see Safety in AUTOMATION.md).</summary>
	private const string TestFilePrefix = "claude-";

	/// <summary>Automation creates and changes only test characters and vaults (see <see cref="AutomationTestNames"/>).</summary>
	private static void RequireTestName(string name)
	{
		if (!AutomationTestNames.IsTestFile(name))
			throw new McpException($"Test files only: the name must start with {AutomationTestNames.Accepted}, e.g. 'claude-vault1'.");
	}

	[McpServerTool(Name = "create_vault", Destructive = true)]
	[Description("Creates a test vault like the New action of 'Maintain Vault Files...', then selects and loads it. The name must start with 'claude-' (e.g. claude-vault1): automation works only in its own test files. The file is written when the vault is saved.")]
	public static string CreateVault([Description("New vault name, starting with 'claude-'.")] string name)
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
	[Description("Duplicates the selected character like the 'Duplicate Character' menu entry (a full save set: player file, stash and all), with the given name instead of the name dialog. The name must start with 'claude-' (e.g. claude-character1). On success TQVault restarts, as it does for a user: wait until get_state answers with loadingComplete, then select the copy.")]
	public static string DuplicateCharacter([Description("Name of the copy, starting with 'claude-'.")] string name)
	{
		RequireTestName(name);
		return AutomationHost.OnUi(form =>
		{
			var error = form.AutomationDuplicateCharacter(name);
			if (error is not null)
				throw new McpException(error);
			return Serialize(new { duplicated = form.AutomationSelectedCharacter, newName = name, restarting = true });
		});
	}


	[McpServerTool(Name = "save_files", Destructive = true)]
	[Description("Saves the modified characters, vaults and stashes like the Save button. A file error shows the Abort/Retry/Ignore box: this call then waits until the box is answered, so answer it from another call with close_dialog (button Abort, Retry or Ignore). Work only in claude-* test files. Returns whether characters were saved and the panels still modified; see recent_log for the save's log lines.")]
	public static string SaveFiles()
		=> AutomationHost.OnUi(form =>
		{
			var playersSaved = form.AutomationSaveFiles();
			return Serialize(new
			{
				playersSaved,
				stillModified = AutomationPanels.List(form).Where(p => p.Panel.Sack?.IsModified == true).Select(p => p.Id).ToList(),
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
			var container = AutomationPanels.Resolve(form, panel).Container
				?? throw new McpException($"Panel '{panel}' has no bag buttons.");

			if (bag < 0 || bag >= container.BagButtons.Count)
				throw new McpException($"Bag {bag} is out of range 0..{container.BagButtons.Count - 1}.");

			var button = container.BagButtons[bag];
			OnMouseDownMethod.Invoke(button, [new MouseEventArgs(MouseButtons.Left, 1, button.Width / 2, button.Height / 2, 0)]);
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

			EnsureWritable(form, named.Panel, "pick up");

			var cell = new Point(x, y);
			var item = named.Panel.AutomationFindItem(cell)
				?? throw new McpException($"No item at cell ({x},{y}) of '{panel}'.");

			named.Panel.AutomationMouseDown(named.Panel.AutomationCellCenter(cell), MouseButtons.Left);

			if (!drag.IsActive)
				throw new McpException($"TQVault did not pick up '{ItemName(item)}'. Notification: '{form.AutomationNotificationText}'. See recent_log.");

			return DragStatus(form);
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
			return DragStatus(form);
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
			EnsureWritable(form, named.Panel, "drop into");

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
				targetCells = target.Size.IsEmpty ? null : new { x = target.X, y = target.Y, width = target.Width, height = target.Height },
				droppedItem = DescribeItem(dropped, null),
				status = JsonDocument.Parse(DragStatus(form)).RootElement,
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
			if (form.AutomationDragInfo.IsActive)
				throw new McpException("An item is being dragged: use move_drag to hover with it, or cancel_drag first.");

			var cell = new Point(x, y);
			var item = named.Panel.AutomationFindItem(cell);
			named.Panel.AutomationMouseMove(named.Panel.AutomationCellCenter(cell));

			var tooltip = OpenTooltips().LastOrDefault();
			byte[]? image = null;
			if (tooltip is { Width: > 0, Height: > 0 })
			{
				using var bitmap = new Bitmap(tooltip.Width, tooltip.Height);
				tooltip.DrawToBitmap(bitmap, new Rectangle(Point.Empty, tooltip.Size));
				using var stream = new MemoryStream();
				bitmap.Save(stream, ImageFormat.Png);
				image = stream.ToArray();
			}

			return (Serialize(new
			{
				item = item is null ? null : DescribeItem(item, null),
				notification = form.AutomationNotificationText,
				tooltips = DescribeTooltips(),
			}), image);
		});

		var blocks = new List<ContentBlock> { new TextContentBlock { Text = json } };
		if (png is not null)
			blocks.Add(ImageContentBlock.FromBytes(png, "image/png"));
		return blocks;
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
			if (form.AutomationDragInfo.IsActive)
				throw new McpException("An item is being dragged: a right click would cancel it. Use cancel_drag first.");

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
			if (form.AutomationDragInfo.IsActive)
				throw new McpException("An item is being dragged: a right click would cancel it. Use cancel_drag first.");

			var cell = new Point(x, y);
			var item = named.Panel.AutomationFindItem(cell);
			string clicked;
			try
			{
				clicked = named.Panel.AutomationClickContextMenu(cell, path.Split('>', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
			}
			catch (InvalidOperationException ex)
			{
				throw new McpException(ex.Message);
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
			if (form.AutomationDragInfo.IsActive)
				throw new McpException("An item is being dragged: a right click would cancel it. Use cancel_drag first.");

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

	[McpServerTool(Name = "press_key", Destructive = true)]
	[Description("Rests the cursor on a cell of a panel and presses a key there like the user: 'c' copies, 'd' duplicates, 'backspace' deletes the item under the cursor (with its confirmation: queue the answer first), 'escape' cancels a drag. Runs after the call returns; follow with get_state or dialog_answers.")]
	public static string PressKey(
		[Description("Panel id.")] string panel,
		[Description("Cell column.")] int x,
		[Description("Cell row.")] int y,
		[Description("A character, or 'backspace' / 'escape'.")] string key)
		=> AutomationHost.OnUi(form =>
		{
			var named = ResolveShownPanel(form, panel);
			char c = key.ToLowerInvariant() switch
			{
				"backspace" => '\b',
				"escape" => (char)27,
				_ when key.Length == 1 => key[0],
				_ => throw new McpException($"Unknown key '{key}'. Use one character, 'backspace' or 'escape'."),
			};
			var item = named.Panel.AutomationFindItem(new Point(x, y));
			named.Panel.AutomationKeyPress(new Point(x, y), c);
			return Serialize(new { panel = named.Id, item = DescribeItem(item, null), key });
		});

	[McpServerTool(Name = "select_items", Destructive = true)]
	[Description("Ctrl-clicks cells of a panel like a user holding Ctrl, which adds each item to the selection (a selected item is removed). Returns the selected items. A plain pick_item or the 'Clear Selection' entry clears it.")]
	public static string SelectItems(
		[Description("Panel id.")] string panel,
		[Description("Cells as 'x,y;x,y'.")] string cells)
		=> AutomationHost.OnUi(form =>
		{
			var named = ResolveShownPanel(form, panel);
			if (form.AutomationDragInfo.IsActive)
				throw new McpException("An item is being dragged. Use cancel_drag first.");

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
	[Description("Shows the tooltip of a bag button (the summary of the bag's items), like resting the pointer on it; capture it with capture('tooltip'). hide=true closes it.")]
	public static string HoverBag(
		[Description("Panel id: vault, secondaryVault, playerBag, stash.")] string panel,
		[Description("Zero-based bag button index.")] int bag,
		[Description("True to close the bag tooltip instead.")] bool hide = false)
		=> AutomationHost.OnUi(form =>
		{
			var container = AutomationPanels.Resolve(form, panel).Container
				?? throw new McpException($"Panel '{panel}' has no bag buttons.");

			if (bag < 0 || bag >= container.BagButtons.Count)
				throw new McpException($"Bag {bag} is out of range 0..{container.BagButtons.Count - 1}.");

			Tooltips.BagButtonTooltip.HideTooltip();
			if (!hide)
				Tooltips.BagButtonTooltip.ShowTooltip(Program.ServiceProvider.GetRequiredService<GuiServices>(), container.BagButtons[bag]);

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
			return DragStatus(form);
		});

	#endregion

	#region Capture

	[McpServerTool(Name = "capture", ReadOnly = true)]
	[Description("Renders a control to PNG with Control.DrawToBitmap, so it works when the window is hidden behind others. Target: a panel id (vault, player, equipment...), '<panel>.container' for the panel with its bag buttons, 'main' for the main window, 'playerInfo' for the stash panel with the character statistics panel and its Edit button (which DrawToBitmap leaves out) drawn over it, or part of a form title.")]
	public static IEnumerable<ContentBlock> Capture(
		[Description("What to capture.")] string control = "main")
	{
		var (png, size) = AutomationHost.OnUi(form =>
		{
			if (string.Equals(control, "playerInfo", StringComparison.OrdinalIgnoreCase))
			{
				using var withPlayerInfo = RenderStashWithPlayerInfo(form);
				using var pngStream = new MemoryStream();
				withPlayerInfo.Save(pngStream, ImageFormat.Png);
				return (pngStream.ToArray(), withPlayerInfo.Size);
			}

			var target = ResolveCaptureTarget(form, control);
			if (target.Width <= 0 || target.Height <= 0)
				throw new McpException($"'{control}' has no size (is it shown?).");

			using var bitmap = new Bitmap(target.Width, target.Height);
			target.DrawToBitmap(bitmap, new Rectangle(Point.Empty, target.Size));
			using var stream = new MemoryStream();
			bitmap.Save(stream, ImageFormat.Png);
			return (stream.ToArray(), target.Size);
		});

		var path = Path.Combine(Path.GetTempPath(), $"tqvault-capture-{control.Replace('.', '-')}-{DateTime.Now:yyyyMMdd-HHmmss-fff}.png");
		File.WriteAllBytes(path, png);

		return
		[
			ImageContentBlock.FromBytes(png, "image/png"),
			new TextContentBlock { Text = $"{control}: {size.Width}x{size.Height} px, saved to {path}" },
		];
	}

	[McpServerTool(Name = "capture_menu", ReadOnly = true)]
	[Description("Opens a menu like the user and renders it to PNG with the sub menus opened along 'open' (' > ' between levels, e.g. 'Modded'), drawn to the right level with their entry, then closes it without choosing anything. Target: 'vaultTools' for the vault tools button next to the vault list, 'characterTools' for the tools button next to the character list (as it opens for the selected character), or a panel id with x and y for the right-click menu of that cell.")]
	public static IEnumerable<ContentBlock> CaptureMenu(
		[Description("'vaultTools', 'characterTools', or a panel id.")] string target,
		[Description("Cell column, for a panel.")] int x = 0,
		[Description("Cell row, for a panel.")] int y = 0,
		[Description("Sub menus to open, texts separated by ' > '; empty for the menu alone.")] string? open = null)
	{
		var path = open?.Split('>', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) ?? [];
		var (png, size) = AutomationHost.OnUi(form =>
		{
			try
			{
				using var bitmap = string.Equals(target, "vaultTools", StringComparison.OrdinalIgnoreCase)
					? form.AutomationRenderVaultMenu(path)
					: string.Equals(target, "characterTools", StringComparison.OrdinalIgnoreCase)
					? form.AutomationCharacterCombo.AutomationRenderMenu(path)
					: AutomationPanels.Resolve(form, target).Panel.AutomationRenderContextMenu(new Point(x, y), path);
				using var stream = new MemoryStream();
				bitmap.Save(stream, ImageFormat.Png);
				return (stream.ToArray(), bitmap.Size);
			}
			catch (InvalidOperationException ex)
			{
				throw new McpException(ex.Message);
			}
		});

		var file = Path.Combine(Path.GetTempPath(), $"tqvault-menu-{target}-{DateTime.Now:yyyyMMdd-HHmmss-fff}.png");
		File.WriteAllBytes(file, png);
		return
		[
			ImageContentBlock.FromBytes(png, "image/png"),
			new TextContentBlock { Text = $"menu {target}: {size.Width}x{size.Height} px, saved to {file}" },
		];
	}

	#endregion

	#region Helpers

	private static string Serialize(object value) => JsonSerializer.Serialize(value, JsonOptions);

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
	/// Respects the PlayerReadonly setting, which the equipment panel enforces for real mouse clicks too. A read-only
	/// character's sack is left to the panel's own rules, which refuse and tell the user (the notification).
	/// </summary>
	private static void EnsureWritable(MainForm form, SackPanel panel, string action)
	{
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
		=> OpenTooltips().Select(t => new { type = t.GetType().Name, width = t.Width, height = t.Height }).ToList();

	private static void ReleaseCursor(SackPanel? panel)
	{
		VirtualCursor.Release();
		panel?.AutomationMouseLeave();
	}

	private static string DragStatus(MainForm form)
	{
		var drag = form.AutomationDragInfo;
		var hovered = VirtualCursor.Target as SackPanel;
		var cells = hovered?.AutomationCellsUnderDragItem ?? Rectangle.Empty;
		return Serialize(new
		{
			dragging = drag.IsActive,
			item = drag.IsActive ? DescribeItem(drag.Item, null) : null,
			sourcePanel = drag.IsActive ? AutomationPanels.IdOf(form, drag.SrcSackPanel) : null,
			hoveredPanel = AutomationPanels.IdOf(form, hovered),
			cellsUnderItem = cells.Size.IsEmpty ? null : new { x = cells.X, y = cells.Y, width = cells.Width, height = cells.Height },
		});
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

		var panel = AutomationPanels.List(form).FirstOrDefault(p => string.Equals(p.Id, control, StringComparison.OrdinalIgnoreCase));
		if (panel is not null)
			return panel.Panel;

		foreach (Form open in System.Windows.Forms.Application.OpenForms)
		{
			if (open.Text.Contains(control, StringComparison.OrdinalIgnoreCase))
				return open;
		}

		throw new McpException($"Unknown capture target '{control}'. Use 'main', 'tooltip', 'characterList[:name]', a panel id ({string.Join(", ", AutomationPanels.List(form).Select(p => p.Id))}), '<panel>.container' or a form title.");
	}

	private static string ItemName(Item item)
	{
		try
		{
			var provider = Program.ServiceProvider.GetRequiredService<IItemProvider>();
			return provider.GetFriendlyNames(item).FullNameClean;
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
			var names = Program.ServiceProvider.GetRequiredService<IItemProvider>().GetFriendlyNames(item);
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
			// Records the loaded game data lacks (a mod not loaded): the tooltip shows the item incomplete.
			// The item's map (null: the base game) and where it comes from: Own, Inherited, or None before a container.
			mod = item.Mod,
			modOrigin = item.ModOrigin.ToString(),
			missingRecords = item.IsIncomplete ? item.MissingRecords.Select(m => new { role = m.Role.ToString(), id = m.Id.Raw }) : null,
			x = item.PositionX,
			y = item.PositionY,
			width = item.Width,
			height = item.Height,
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
			return Program.ServiceProvider.GetRequiredService<IItemProvider>().GetRequirementInfo(item).Lvl;
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
