using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace TQVaultAE.GUI.Automation;

/// <summary>
/// MCP tools that create the app's forms and dialogs directly, to check them without finding the click that opens them.
/// </summary>
internal static partial class AutomationTools
{
	[McpServerTool(Name = "zoom", Destructive = true)]
	[Description("Zooms the main window like its keyboard shortcuts: 'in' (Ctrl +, +10 %), 'out' (Ctrl -, -10 %) or 'reset' (Ctrl Home, 100 %). Returns the zoom, the DPI factor, the resulting scale and the client size. The zoom is saved to UserConfig.xml like a user zoom: end with 'reset'.")]
	public static string Zoom([Description("in, out or reset.")] string step)
		=> AutomationHost.OnUi(main =>
		{
			float delta = step.ToLowerInvariant() switch
			{
				"in" => 0.1F,
				"out" => -0.1F,
				"reset" => 1.0F,
				_ => throw new McpException("step must be in, out or reset."),
			};

			main.AutomationZoom(delta);
			var ui = main.Services.UIService;
			return Serialize(new { zoom = ui.Zoom, dpiScale = ui.DpiScale, scale = ui.Scale, clientSize = new { main.ClientSize.Width, main.ClientSize.Height } });
		});

	[McpServerTool(Name = "control_tree", ReadOnly = true)]
	[Description("Lists the controls of an open form with their type, bounds (client coordinates of their parent, in pixels), font, visibility and enabled state, to check layout and scaling. Also reports the form's DPI and TQVault's scale, zoom and DPI factor.")]
	public static string ControlTree(
		[Description("Part of the form title (case-insensitive); empty for the main window.")] string form = "",
		[Description("Maximum depth below the form (default 3).")] int depth = 3)
		=> AutomationHost.OnUi(main =>
		{
			Form target = string.IsNullOrEmpty(form)
				? main
				: System.Windows.Forms.Application.OpenForms.Cast<Form>().FirstOrDefault(f => f.Text.Contains(form, StringComparison.OrdinalIgnoreCase))
					?? throw new McpException($"No open form has '{form}' in its title.");

			object Describe(Control c, int level) => new
			{
				name = c.Name,
				type = c.GetType().Name,
				bounds = new { c.Left, c.Top, c.Width, c.Height },
				font = $"{c.Font.Name} {c.Font.SizeInPoints:0.##}pt",
				visible = c.Visible,
				enabled = c.Enabled,
				controls = level < depth && c.Controls.Count > 0
					? c.Controls.Cast<Control>().Select(child => Describe(child, level + 1)).ToList()
					: null,
			};

			var ui = main.Services.UIService;
			return Serialize(new
			{
				form = target.Text,
				deviceDpi = target.DeviceDpi,
				clientSize = new { target.ClientSize.Width, target.ClientSize.Height },
				scale = ui.Scale,
				zoom = ui.Zoom,
				dpiScale = ui.DpiScale,
				controls = target.Controls.Cast<Control>().Select(c => Describe(c, 1)).ToList(),
			});
		});

	[McpServerTool(Name = "list_forms", ReadOnly = true)]
	[Description("Lists the form and dialog types registered in the container, which open_form can create.")]
	public static string ListForms()
		=> Serialize(FormTypes().Select(t => new { name = t.Name, type = t.FullName }));

	[McpServerTool(Name = "open_form", Destructive = true)]
	[Description("Creates a form or dialog from the container, as the app does (FormFactory), so its InitializeComponent and resources run. With show=false (the default) it reports the created form and disposes it; with show=true it opens it modeless over the main window, to capture or close afterwards with capture and close_dialog. Singletons (MainForm, BagButtonSettings) are only reported. Dialogs that need data first (ResultsDialog, ItemProperties...) may fail when shown: see recent_log.")]
	public static string OpenForm(
		[Description("Type name, as listed by list_forms, e.g. AboutBox or SettingsDialog.")] string name,
		[Description("true to show the form modeless instead of disposing it.")] bool show = false)
		=> AutomationHost.OnUi(main =>
		{
			var type = FormTypes().FirstOrDefault(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase))
				?? throw new McpException($"Unknown form '{name}'. Forms: {string.Join(", ", FormTypes().Select(t => t.Name))}");

			var form = (Form)Program.ServiceProvider.GetRequiredService(type);

			// A transient registration returns a new instance on each call.
			var other = (Form)Program.ServiceProvider.GetRequiredService(type);
			var singleton = ReferenceEquals(form, other);
			if (!singleton)
				other.Dispose();

			var report = new
			{
				type = type.FullName,
				singleton,
				title = form.Text,
				size = new { width = form.Width, height = form.Height },
				controls = CountControls(form),
				backgroundImage = form.BackgroundImage is not null,
				icon = form.Icon is not null,
				shown = show && !singleton,
			};

			if (singleton)
				return Serialize(report);

			if (show)
			{
				// Shown after this call returns, so a Load handler opening a message box cannot block the endpoint.
				main.BeginInvoke(() =>
				{
					try
					{
						form.Show(main);
					}
					catch (Exception ex)
					{
						AutomationHost.Log?.LogError(ex, "open_form: {Form} failed to show", type.Name);
						form.Dispose();
					}
				});
			}
			else
			{
				form.Dispose();
			}

			return Serialize(report);
		});

	[McpServerTool(Name = "open_item_dialog", Destructive = true)]
	[Description("Opens the Properties or Seed dialog of an item, like its context menu entries, but modeless over the main window, to check its layout with control_tree, text_fit and capture. Close it with close_dialog and its Cancel/OK button: the Seed dialog's OK only validates the typed seed (the context menu entry then applies it), so it leaves the item unchanged here.")]
	public static string OpenItemDialog(
		[Description("Panel id.")] string panel,
		[Description("Cell column of the item.")] int x,
		[Description("Cell row of the item.")] int y,
		[Description("properties or seed.")] string dialog = "properties")
		=> AutomationHost.OnUi(main =>
		{
			var named = ResolveShownPanel(main, panel);
			var item = named.Panel.AutomationFindItem(new Point(x, y))
				?? throw new McpException($"No item at {x},{y} in '{named.Id}'.");

			Form form = dialog.ToLowerInvariant() switch
			{
				"properties" => main.Services.Forms.Create<Dialogs.ItemProperties>().With(d => d.Item = item),
				"seed" => main.Services.Forms.Create<Dialogs.ItemSeedDialog>().With(d => d.SelectedItem = item),
				_ => throw new McpException("dialog must be properties or seed."),
			};

			// Shown after this call returns, so a Load handler opening a message box cannot block the endpoint.
			main.BeginInvoke(() => form.Show(main));
			return Serialize(new { dialog = form.GetType().Name, item = item.ToString() });
		});

	private static T With<T>(this T form, Action<T> set)
		where T : Form
	{
		set(form);
		return form;
	}

	[McpServerTool(Name = "open_input_dialog", Destructive = true)]
	[Description("Opens the text input dialog (InputDialog, used to name a duplicated character) modeless over the main window, to check its layout with capture and close it with close_dialog (button Cancel or OK).")]
	public static string OpenInputDialog(
		[Description("Prompt above the text box.")] string prompt = "New name of the character:",
		[Description("Dialog title.")] string title = "Input",
		[Description("Text box content.")] string value = "NewName")
		=> AutomationHost.OnUi(main =>
		{
			var dialog = new Dialogs.InputDialog(prompt, title, value);
			dialog.Show(main);
			return Serialize(new { title = dialog.Text, size = new { dialog.Width, dialog.Height }, deviceDpi = dialog.DeviceDpi });
		});

	[McpServerTool(Name = "bag_menu", Destructive = true)]
	[Description("Right-clicks a bag button of a panel like the user (it selects that bag) and returns the bag context menu (move, copy, merge, empty, tooltips, CSV and JSON export, change, copy and paste icon, import), then closes it. With path (' > ' between levels, e.g. 'Copy Bag > Bag 3' or 'Change Icon'), clicks that entry after this call returns, since it may open a dialog or a confirmation. Only vault bags have this menu. Also returns the icon settings of every bag.")]
	public static string BagMenu(
		[Description("Panel id with bag buttons: vault, secondaryVault, playerBag or stash.")] string panel,
		[Description("Zero-based bag button index.")] int bag,
		[Description("Entry to click, texts separated by ' > '; empty to only read the menu.")] string? path = null)
		=> AutomationHost.OnUi(form =>
		{
			var container = AutomationPanels.Resolve(form, panel).Container
				?? throw new McpException($"Panel '{panel}' has no bag buttons.");
			try
			{
				var (entries, clicked) = container.AutomationBagMenu(bag, path?.Split('>', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
				return Serialize(new { panel, bag, currentBag = container.ShownBag(), entries, clicked, icons = container.AutomationBagIcons });
			}
			catch (InvalidOperationException ex)
			{
				throw new McpException(ex.Message);
			}
		});

	[McpServerTool(Name = "sort_menu", Destructive = true)]
	[Description("Right-clicks the sort button of a panel like the user and returns its menu (one entry per sort order, the order a left click uses checked), then closes it. With path (e.g. 'Sort by required level'), clicks that entry after this call returns: it becomes the order of the left click (saved in UserConfig.xml, <Sort Algorithm=... />) and sorts the panel's bag at once. Also returns the saved sort choices.")]
	public static string SortMenu(
		[Description("Panel id with a sort button: vault, secondaryVault, player (main inventory), playerBag or stash (the shown transfer, storage or relic tab; refused on the equipment tab, where the button is hidden).")] string panel,
		[Description("Entry to click; empty to only read the menu.")] string? path = null)
		=> AutomationHost.OnUi(form =>
		{
			var named = AutomationPanels.Resolve(form, panel);
			var container = named.Container ?? throw new McpException($"Panel '{panel}' has no sort button.");
			try
			{
				var (entries, clicked) = container.AutomationSortMenu(named.Panel, path?.Split('>', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
				return Serialize(new { panel, entries, clicked, sort = SortChoices(form) });
			}
			catch (InvalidOperationException ex)
			{
				throw new McpException(ex.Message);
			}
		});

	[McpServerTool(Name = "sort_click", Destructive = true)]
	[Description("Left-clicks the sort button of a panel like the user: sorts the shown bag with the order picked in sort_menu (each click alternates rows and columns), merging relic and charm shards first when the 'Merge relic shards when sorting' setting is on. The click runs after this call returns (a sort that does not fit shows a message, see dialog_answers): read the result with list_items.")]
	public static string SortClick(
		[Description("Panel id with a sort button: vault, secondaryVault, player (main inventory), playerBag or stash (the shown transfer, storage or relic tab; refused on the equipment tab, where the button is hidden).")] string panel)
		=> AutomationHost.OnUi(form =>
		{
			var named = AutomationPanels.Resolve(form, panel);
			var container = named.Container ?? throw new McpException($"Panel '{panel}' has no sort button.");
			try
			{
				container.AutomationSortClickLater(named.Panel);
				return Serialize(new { panel, queued = true, sort = SortChoices(form) });
			}
			catch (InvalidOperationException ex)
			{
				throw new McpException(ex.Message);
			}
		});

	/// <summary>The saved sort choices: the order of the left click and the shard merge.</summary>
	private static object SortChoices(Forms.MainForm form)
		=> new { algorithm = form.AutomationSettings.Sort?.Algorithm.ToString(), mergeRelics = form.AutomationSettings.Sort?.MergeRelics };

	[McpServerTool(Name = "vault_menu", Destructive = true)]
	[Description("Returns the menu of the vault tools button next to the vault list (maintain vault files, Modded vault, share, import) as it opens for the shown vault. With path (' > ' between levels, e.g. 'Modded vault > shadowchampions'), clicks that entry after this call returns, since it may open a dialog.")]
	public static string VaultMenu(
		[Description("Entry to click, texts separated by ' > '; empty to only read the menu.")] string? path = null)
		=> AutomationHost.OnUi(form =>
		{
			try
			{
				var (entries, clicked) = form.AutomationVaultMenu(path?.Split('>', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
				return Serialize(new { vault = form.AutomationSelectedVault, vaultMod = form.AutomationSelectedVaultMod, entries, clicked });
			}
			catch (InvalidOperationException ex)
			{
				throw new McpException(ex.Message);
			}
		});

	[McpServerTool(Name = "bag_settings_pick_icon", Destructive = true)]
	[Description("In the open bag button settings dialog, right-clicks an icon of a tab's grid like the user, which picks its pictures (predefined selection) and switches to Custom icon. Then read the dialog with control_values and apply it with click_control('Bag Button Settings', 'applyButton').")]
	public static string BagSettingsPickIcon(
		[Description("Tab: Skills, Relics, Artifacts, Jewellery, Scrolls, Potions, Buttons, Misc, Helmets, Shields, Armbands, Greaves.")] string tab,
		[Description("Zero-based icon index in that grid.")] int index = 0)
		=> AutomationHost.OnUi(_ =>
		{
			var dialog = System.Windows.Forms.Application.OpenForms.OfType<Dialogs.BagButtonSettings>().FirstOrDefault(f => f.Visible)
				?? throw new McpException("The bag button settings are not open.");
			try
			{
				return Serialize(new { picked = dialog.AutomationPickIcon(tab, index) });
			}
			catch (InvalidOperationException ex)
			{
				throw new McpException(ex.Message);
			}
		});

	[McpServerTool(Name = "open_bag_settings", Destructive = true)]
	[Description("Opens the bag button settings (icon and label picker) for the current bag of a panel, like its 'Change icon' context menu entry. The dialog is modal: capture it with capture('Bag Button Settings') and close it with close_dialog('Bag Button Settings', 'Cancel'). The game icons load in the background after startup, so 'iconsLoaded' may be false right after launch.")]
	public static async Task<string> OpenBagSettings(
		[Description("Panel id with bag buttons: vault, secondaryVault, playerBag or stash.")] string panel = "vault")
	{
		AutomationHost.OnUi(form =>
		{
			var container = AutomationPanels.Resolve(form, panel).Container
				?? throw new McpException($"Panel '{panel}' has no bag buttons.");

			var dialog = form.Services.Forms.Create<Dialogs.BagButtonSettings>();
			dialog.CurrentBagButton = container.BagButtons[container.ShownBag()];

			// ShowDialog runs its own message loop: start it after this call returns.
			form.BeginInvoke(() => dialog.ShowDialog(form));
			return 0;
		});

		// Let the dialog load (its icons come from the icon service, already loading in the background).
		var iconService = Program.ServiceProvider.GetRequiredService<Presentation.Win32.Contracts.IIconService>();
		var icons = await iconService.GetIconDatabaseAsync().WaitAsync(TimeSpan.FromSeconds(30));
		await Task.Delay(500);

		return AutomationHost.OnUi(_ => Serialize(new
		{
			shown = System.Windows.Forms.Application.OpenForms.OfType<Dialogs.BagButtonSettings>().Any(f => f.Visible),
			iconsLoaded = icons.Count,
			iconsWithBitmap = icons.Count(i => !i.IsEmpty),
		}));
	}

	[McpServerTool(Name = "highlight_filters", Destructive = true)]
	[Description("Opens the highlight filters panel of the main window (the 'Highlight items having' link), ticks or unticks its check boxes by text or name, and applies the filters when asked. Returns the check boxes and how many items are highlighted. See it with capture('main'); 'Reset' in the panel, or highlight_filters(uncheck=...) then apply, clears the highlight.")]
	public static string HighlightFilters(
		[Description("Comma separated check box texts or names to tick, e.g. 'Having Skill,Set Item'.")] string? check = null,
		[Description("Comma separated check box texts or names to untick.")] string? uncheck = null,
		[Description("true to apply the filters like the Apply button.")] bool apply = false)
		=> AutomationHost.OnUi(main =>
		{
			var panel = main.AutomationShowHighlightFilters();
			var boxes = panel.AutomationCheckBoxes;

			void Set(string? list, bool value)
			{
				foreach (var wanted in (list ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
				{
					var box = boxes.FirstOrDefault(b => string.Equals(b.Text, wanted, StringComparison.OrdinalIgnoreCase) || string.Equals(b.Name, wanted, StringComparison.OrdinalIgnoreCase))
						?? throw new McpException($"No check box '{wanted}' in the highlight filters.");
					box.Checked = value;
				}
			}

			Set(check, true);
			Set(uncheck, false);
			if (apply)
				panel.AutomationApply();

			var highlighted = Program.ServiceProvider.GetRequiredService<Application.Contracts.Services.IHighlightService>().HighlightedItems;
			return Serialize(new
			{
				shown = panel.Visible,
				checkBoxes = boxes.Select(b => new { b.Name, b.Text, b.Checked }),
				highlighted = highlighted.Count,
			});
		});

	[McpServerTool(Name = "window_bounds", ReadOnly = true)]
	[Description("The main window's bounds (virtual screen pixels), its state (Normal, Maximized, Minimized), its DPI, zoom and scale, the right and bottom edges of its content (its visible child controls), and the working area of the monitor it is on: checks a maximize on each monitor, and that the content still fits the window after a maximize and a restore.")]
	public static string WindowBounds()
		=> AutomationHost.OnUi(main =>
		{
			var screen = Screen.FromHandle(main.Handle);
			var children = main.Controls.Cast<Control>().Where(c => c.Visible).ToList();
			var ui = main.Services.UIService;
			return Serialize(new
			{
				bounds = new { main.Bounds.X, main.Bounds.Y, main.Bounds.Width, main.Bounds.Height },
				clientSize = new { main.ClientSize.Width, main.ClientSize.Height },
				contentRight = children.Count == 0 ? 0 : children.Max(c => c.Right),
				contentBottom = children.Count == 0 ? 0 : children.Max(c => c.Bottom),
				state = main.WindowState.ToString(),
				zoom = ui.Zoom,
				scale = ui.Scale,
				deviceDpi = main.DeviceDpi,
				monitor = screen.DeviceName,
				workingArea = new { screen.WorkingArea.X, screen.WorkingArea.Y, screen.WorkingArea.Width, screen.WorkingArea.Height },
				insideWorkingArea = screen.WorkingArea.Contains(main.Bounds),
			});
		});

	[McpServerTool(Name = "move_window", Destructive = true)]
	[Description("Moves the main window, or another open form, to a screen position (virtual screen pixels, so negative on a monitor left of the primary one), like dragging it there (the size is left to Windows): on a monitor of another DPI, Windows sends the DPI change. Returns the monitors (bounds and DPI) and the window's DPI and scales after the move. Move it back to its monitor when done.")]
	public static async Task<string> MoveWindow(
		[Description("Left of the window in virtual screen pixels.")] int x,
		[Description("Top of the window in virtual screen pixels.")] int y,
		[Description("Part of the form title (case-insensitive); empty for the main window.")] string form = "")
	{
		Form Target(Form main) => string.IsNullOrEmpty(form)
			? main
			: System.Windows.Forms.Application.OpenForms.Cast<Form>().FirstOrDefault(f => f.Text.Contains(form, StringComparison.OrdinalIgnoreCase))
				?? throw new McpException($"No open form has '{form}' in its title.");

		AutomationHost.OnUi(main =>
		{
			// Without the size: Location would send the old size too, over the one the DPI change gives.
			NativeWindows.MoveTo(Target(main).Handle, x, y);
			return 0;
		});

		// The DPI change comes as a posted window message.
		await Task.Delay(500);

		return AutomationHost.OnUi(main =>
		{
			var ui = main.Services.UIService;
			var target = Target(main);
			return Serialize(new
			{
				location = new { target.Location.X, target.Location.Y },
				size = new { target.Width, target.Height },
				clientSize = new { target.ClientSize.Width, target.ClientSize.Height },
				deviceDpi = target.DeviceDpi,
				zoom = ui.Zoom,
				dpiScale = ui.DpiScale,
				systemDpiScale = ui.SystemDpiScale,
				scale = ui.Scale,
				fontZoom = ui.FontZoom,
				monitors = Screen.AllScreens.Select(s => new
				{
					s.DeviceName,
					bounds = new { s.Bounds.X, s.Bounds.Y, s.Bounds.Width, s.Bounds.Height },
					s.Primary,
				}),
			});
		});
	}

	private static IEnumerable<Type> FormTypes()
	{
		var check = Program.ServiceProvider.GetRequiredService<IServiceProviderIsService>();
		return typeof(Program).Assembly.GetTypes()
			.Where(t => typeof(Form).IsAssignableFrom(t) && !t.IsAbstract && check.IsService(t))
			.OrderBy(t => t.Name);
	}

	private static int CountControls(Control control)
		=> control.Controls.Cast<Control>().Sum(c => 1 + CountControls(c));
}
