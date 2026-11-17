using System.Net;
using System.Reflection;
using AwesomeAssertions;
using ModelContextProtocol.Server;

namespace TQVaultAE.Tests.Win32.Gui;

/// <summary>
/// The two levels of the MCP automation endpoint (documentation/AUTOMATION.md): the user tools, shipped in Release builds,
/// are the methods of <c>AutomationTools</c>; the developer tools, the methods of the nested <c>AutomationTools.DevTools</c>,
/// whose files Release builds leave out. Run with <c>-c Release</c>, these tests check the Release tool list.
/// </summary>
public sealed class AutomationToolLevelsTests
{
	private static readonly Assembly Gui = typeof(TQVaultAE.GUI.Program).Assembly;

	private static Type AutomationTools => Gui.GetType("TQVaultAE.GUI.Automation.AutomationTools", throwOnError: true)!;

	private static Type AutomationHost => Gui.GetType("TQVaultAE.GUI.Automation.AutomationHost", throwOnError: true)!;

	private static Type? DevTools => AutomationTools.GetNestedType("DevTools", BindingFlags.NonPublic | BindingFlags.Public);

	/// <summary>The user tools: reading the state, the user's actions and the captures (documentation/mcp.md).</summary>
	private static readonly string[] UserTools =
	[
		"archive_character", "bag_menu", "bag_settings_pick_icon", "cancel_drag", "capture", "capture_menu",
		"character_attribute_click", "character_list", "character_panel", "character_read_only", "character_stat_sources",
		"character_tooltip", "click_cell", "close_dialog", "close_form", "compare_click", "compare_items", "comparison_windows", "context_menu",
		"context_menu_click", "create_vault", "dialog_answers", "drop_item", "duplicate_character", "export_csv",
		"export_items", "export_tab", "export_vault", "find_copies", "forge_cancel", "forge_drop", "forge_forge",
		"forge_mode_card", "forge_reset", "forge_select_part", "forge_set_hardcore", "forge_set_mode", "forge_show",
		"forge_state", "game_text", "get_state", "highlight_filters", "hover_bag", "hover_end", "hover_item", "import_file",
		"import_text", "key_binds", "list_tags", "list_windows", "loadouts", "menu_click", "mod_state", "mod_suggest",
		"move_drag", "open_bag_settings", "open_character_editor", "open_item_dialog", "open_mod_link", "owned_skill_items",
		"pick_item", "player_info", "queue_dialog_answers", "save_files", "search_containers", "search_entries",
		"select_bag", "select_character", "select_items", "select_search_result", "select_tab", "select_vault",
		"skill_panel", "skill_panel_click", "skill_tooltip", "sort_click", "sort_menu", "tag_archive_all", "tag_box",
		"vault_menu",
	];

	/// <summary>The developer tools: UI internals, raw input, diagnostics, fault injection.</summary>
	private static readonly string[] DeveloperTools =
	[
		"bag_settings_drag_icon", "bag_settings_hover_icon", "capture_results_menu", "character_buff_toggle", "character_buffs", "character_weapon_set", "character_window", "character_window_close", "character_window_list", "character_window_loadout", "character_window_open", "check_list_items", "click_control", "control_tree",
		"control_values", "create_item", "drag_select", "forge_drag_cursor", "forge_hover", "grid_click", "grid_values",
		"item_properties_dump", "key_binds_dialog", "list_forms", "list_items", "list_keys", "list_state", "load_resource",
		"mouse_click", "mouse_wheel", "move_window", "open_form", "open_input_dialog", "picture_fault", "player_stats_fonts",
		"player_stats_hover", "press_key", "raw_tooltip_lines", "recent_log", "results_move", "recent_sounds", "search_dialog_scroll",
		"search_dialog_state", "search_tooltip", "set_control", "set_grid_cell", "socket_click", "stash_background_switch", "text_fit",
		"tooltip_text_dump", "window_bounds", "window_state", "zoom",
	];

	private static IEnumerable<string> ToolNames(Type type)
		=> type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
			.Select(m => m.GetCustomAttribute<McpServerToolAttribute>()?.Name)
			.OfType<string>();

	[Fact]
	public void UserTools_AreTheToolsOfAutomationTools()
		=> ToolNames(AutomationTools).Should().BeEquivalentTo(UserTools,
			"a tool added to AutomationTools ships in Release builds: classify it in AUTOMATION.md and mcp.md, or put it in Dev/");

	[Fact]
	public void DeveloperTools_ExistInDebugBuildsOnly()
	{
#if DEBUG
		DevTools.Should().NotBeNull();
		ToolNames(DevTools!).Should().BeEquivalentTo(DeveloperTools);
#else
		DevTools.Should().BeNull("Release builds must not contain the developer tools");
#endif
	}

	[Fact]
	public void ServedToolTypes_AddTheDeveloperToolsInDebugBuildsOnly()
	{
		var served = (Type[])AutomationHost.GetProperty("ToolTypes", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;

#if DEBUG
		served.Should().Equal(AutomationTools, DevTools!);
#else
		served.Should().Equal(AutomationTools);
#endif
	}

	[Fact]
	public void UserAndDeveloperTools_DoNotShareAName()
		=> UserTools.Intersect(DeveloperTools).Should().BeEmpty();

	[Theory]
	[InlineData(47017)]
	[InlineData(47075)]
	public void ListenEndPoint_IsTheLoopbackInterfaceOnly(int port)
	{
		var endPoint = (IPEndPoint)AutomationHost.GetMethod("ListenEndPoint", BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, [port])!;

		endPoint.Address.Should().Be(IPAddress.Loopback, "the endpoint has no password: it must never listen on the network");
		endPoint.Port.Should().Be(port);
	}
}
