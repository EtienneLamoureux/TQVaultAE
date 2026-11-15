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

	[McpServerTool(Name = "close_form", Destructive = true)]
	[Description("Closes an open modeless form that has no button to answer it (close_dialog needs one), like its close box: for example the search results window. The main window is refused. Returns the title of the form closed.")]
	public static string CloseForm(
		[Description("Part of the form title or its type name (case-insensitive); the last one opened when several match.")] string form)
		=> AutomationHost.OnUi(main =>
		{
			var target = FindOpenForm(form) as Form
				?? throw new McpException($"'{form}' is a view of the main window, not a form.");
			if (ReferenceEquals(target, main))
				throw new McpException("The main window is not closed by close_form: use click_control(\"\", \"exitButton\").");

			string title = target.Text;
			target.Close();
			return Serialize(new { closed = title });
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
			var (entries, clicked) = container.AutomationBagMenu(bag, MenuPath(path));
			return Serialize(new { panel, bag, currentBag = container.ShownBag(), entries, clicked, icons = container.AutomationBagIcons });
		});

	[McpServerTool(Name = "sort_menu", Destructive = true)]
	[Description("Right-clicks the sort button of a panel like the user and returns its menu (one entry per sort order, the order a left click on this button uses checked), then closes it. With path (e.g. 'Sort by required level'), clicks that entry after this call returns: it becomes the order of this button's left click only (saved in UserConfig.xml as the button's attribute, <Sort Vault=... />) and sorts the panel's bag at once. Also returns this button's sort choices as they were before the click.")]
	public static string SortMenu(
		[Description("Panel id with a sort button: vault, secondaryVault, player (main inventory), playerBag or stash (the shown transfer, storage or relic tab; refused on the equipment tab, where the button is hidden).")] string panel,
		[Description("Entry to click; empty to only read the menu.")] string? path = null)
		=> AutomationHost.OnUi(form =>
		{
			var named = AutomationPanels.Resolve(form, panel);
			var container = named.Container ?? throw new McpException($"Panel '{panel}' has no sort button.");
			var (entries, clicked) = container.AutomationSortMenu(named.Panel, MenuPath(path));
			return Serialize(new { panel, entries, clicked, sort = SortChoices(form, container, named.Panel) });
		});

	[McpServerTool(Name = "sort_click", Destructive = true)]
	[Description("Left-clicks the sort button of a panel like the user: sorts the shown bag with the order picked in this button's sort_menu (each click alternates rows and columns), merging relic and charm shards first when the 'Merge relic shards when sorting' setting is on. The click runs after this call returns (a sort that does not fit shows a message, see dialog_answers): read the result with get_state(panel). Returns the order used.")]
	public static string SortClick(
		[Description("Panel id with a sort button: vault, secondaryVault, player (main inventory), playerBag or stash (the shown transfer, storage or relic tab; refused on the equipment tab, where the button is hidden).")] string panel)
		=> AutomationHost.OnUi(form =>
		{
			var named = AutomationPanels.Resolve(form, panel);
			var container = named.Container ?? throw new McpException($"Panel '{panel}' has no sort button.");
			var sort = SortChoices(form, container, named.Panel);
			container.AutomationSortClickLater(named.Panel);
			return Serialize(new { panel, queued = true, sort });
		});

	/// <summary>
	/// The sort choices of the sort button of <paramref name="target"/>: its key, the order of its left click, whether that
	/// order is its own (else the default <c>Algorithm</c>), the default, and the shard merge.
	/// </summary>
	private static object SortChoices(Forms.MainForm form, Components.Panels.VaultPanel container, Components.Panels.SackPanel target)
	{
		var (key, algorithm) = container.AutomationSortOrder(target);
		var sort = form.AutomationSettings.Sort;
		return new
		{
			key = key.ToString(),
			algorithm = algorithm.ToString(),
			own = sort?.HasOwnAlgorithm(key) ?? false,
			defaultAlgorithm = sort?.Algorithm.ToString(),
			mergeRelics = sort?.MergeRelics,
		};
	}

	[McpServerTool(Name = "vault_menu", Destructive = true)]
	[Description("Returns the menu of the vault tools button next to the vault list (maintain vault files, Modded vault, share, import) as it opens for the shown vault. With path (' > ' between levels, e.g. 'Modded vault > shadowchampions'), clicks that entry after this call returns, since it may open a dialog.")]
	public static string VaultMenu(
		[Description("Entry to click, texts separated by ' > '; empty to only read the menu.")] string? path = null)
		=> AutomationHost.OnUi(form =>
		{
			var (entries, clicked) = form.AutomationVaultMenu(MenuPath(path));
			return Serialize(new { vault = form.AutomationSelectedVault, vaultMod = form.AutomationSelectedVaultMod, entries, clicked });
		});

	[McpServerTool(Name = "bag_settings_pick_icon", Destructive = true)]
	[Description("In the open bag button settings dialog, right-clicks an icon of a tab's grid like the user, which picks its pictures (predefined selection) and switches to Custom icon. Then read the dialog with control_values and apply it with click_control('Bag Button Settings', 'applyButton').")]
	public static string BagSettingsPickIcon(
		[Description("Tab: Skills, Relics, Artifacts, Jewellery, Scrolls, Potions, Buttons, Misc, Helmets, Shields, Armbands, Greaves.")] string tab,
		[Description("Zero-based icon index in that grid.")] int index = 0)
		=> AutomationHost.OnUi(_ =>
		{
			var dialog = OpenDialog<Dialogs.BagButtonSettings>("bag button settings");
			return Serialize(new { picked = dialog.AutomationPickIcon(tab, index) });
		});

	[McpServerTool(Name = "open_bag_settings", Destructive = true)]
	[Description("Opens the bag button settings (icon and label picker) of a vault bag like the user: right-clicks its bag button (which shows that bag) and clicks 'Change icon' (bag_menu(panel, bag, 'Change icon')), after this call returns; OK applies the choice and redraws the button, as for the user. Only vault bags have the entry: the character's bags and the stash tabs are refused. The dialog is modal: capture it with capture('Bag Button Settings') and close it with close_dialog('Bag Button Settings', 'Cancel'). Waits for the game icons, which load in the background after startup ('iconsLoaded').")]
	public static async Task<string> OpenBagSettings(
		[Description("Panel id of a vault: vault or secondaryVault.")] string panel = "vault",
		[Description("Zero-based bag button index; omit for the bag shown.")] int? bag = null)
	{
		AutomationHost.OnUi(form =>
		{
			var container = AutomationPanels.Resolve(form, panel).Container
				?? throw new McpException($"Panel '{panel}' has no bag buttons.");
			return container.AutomationChangeIcon(bag ?? container.ShownBag());
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
