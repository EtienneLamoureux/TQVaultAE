using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using TQVaultAE.GUI.Components.Panels;
using TQVaultAE.GUI.Forms;

namespace TQVaultAE.GUI.Automation;

internal static partial class AutomationTools
{
	#region Menus opened by a click

	[McpServerTool(Name = "menu_click", Destructive = true)]
	[Description("Opens a menu the way the user does and returns its entries (text, sub menus, disabled, checked, hint; hidden entries left out), then closes it; with path (' > ' between levels), clicks that entry after this call returns instead, since it may open a dialog (queue its answers first with queue_dialog_answers). Targets: 'characterTools' (left click on the tools button next to the character list: Duplicate Character, Mod link, Archived, Read-only, Tags), 'loadouts' (right click on the loadout bar: its actions), 'loadouts.list' (left click on the loadout bar's name box: the loadouts), 'stash.bag<N>' (right click on bag button N of the stash panel: its tooltip toggle), 'systemMenu' (right click on the title bar of the main window, or of the form named by form: Restore, Minimize, Maximize, Close...). Clicks that change files only on test data: characterTools entries on a test character (claude-, player-test, player-mod), its tag entries on test tags (claude-), Duplicate Character only with a test name queued as 'text:claude-...', never Archive all / Unarchive all; loadout actions on a test character. The window menu's Move and Size are refused: they wait for the keyboard.")]
	public static string MenuClick(
		[Description("characterTools, loadouts, loadouts.list, stash.bag<N> or systemMenu.")] string target,
		[Description("Entry to click, texts separated by ' > ' (e.g. 'Tags > claude-tag > Delete'); empty to only read the menu.")] string? path = null,
		[Description("For systemMenu: part of the title or the type name of the form; empty for the main window.")] string? form = null)
		=> AutomationHost.OnUi(main =>
		{
			var steps = path?.Split('>', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) ?? [];
			var (entries, clicked) = RunMenu(main, target, form, steps.Length == 0 ? null : menu => AutomationMenus.Resolve(menu, steps));
			return Serialize(new { target, entries, clicked });
		});

	/// <summary>
	/// Opens the menu of <paramref name="target"/> like the user (<see cref="OpenMenu"/>), reads it, and clicks the entry
	/// <paramref name="choose"/> finds, once refused or not by the target's rule: after the calling tool returns, or at
	/// once with <paramref name="now"/> (for clicks that open nothing). The menu is closed when nothing is clicked.
	/// </summary>
	/// <returns>The entries, and the text of the entry clicked.</returns>
	private static (IReadOnlyList<AutomationMenuEntry> Entries, string? Clicked) RunMenu(MainForm main, string target, string? form,
		Func<ToolStripDropDown, IReadOnlyList<ToolStripItem>>? choose, bool now = false)
	{
		var (menu, refuse) = OpenMenu(main, target.Trim(), form);
		if (!menu.Visible)
			throw new McpException($"The menu of '{target}' did not open (its Opening event cancelled it, or nothing was under the click).");

		bool clicking = false;
		try
		{
			var entries = AutomationMenus.Describe(menu.Items);
			if (choose is null)
				return (entries, null);

			var picked = choose(menu);
			if (picked.Count == 0)
				return (entries, null);

			var chain = AutomationMenus.Clickable(picked);
			if (refuse(chain) is { } reason)
				throw new McpException(reason);
			if (now)
			{
				chain[^1].PerformClick();
				return (entries, chain[^1].Text);
			}

			clicking = true;
			return (entries, AutomationMenus.ClickLater(main, chain));
		}
		catch (InvalidOperationException ex)
		{
			throw new McpException(ex.Message);
		}
		finally
		{
			if (!clicking && menu.Visible)
				menu.Close();
		}
	}

	/// <summary>
	/// Clicks an entry of the tools menu next to the character list after queueing <paramref name="answer"/> first for the
	/// dialog it opens; the answer is taken back when the click is refused.
	/// </summary>
	private static string? ClickCharacterTool(MainForm main, IReadOnlyList<ToolStripItem> chain, QueuedAnswer? answer = null)
	{
		if (answer is { } queued)
			AutomationHost.QueueDialogAnswerFirst(queued);
		try
		{
			return RunMenu(main, "characterTools", null, _ => chain).Clicked;
		}
		catch
		{
			if (answer is { } taken)
				AutomationHost.WithdrawDialogAnswer(taken);
			throw;
		}
	}

	/// <summary>
	/// Opens the menu of <paramref name="target"/> through the user's own gesture, and gives the rule refusing the
	/// entries automation may not click.
	/// </summary>
	private static (ToolStripDropDown Menu, Func<IReadOnlyList<ToolStripItem>, string?> Refuse) OpenMenu(MainForm main, string target, string? form)
	{
		static string? None(IReadOnlyList<ToolStripItem> _) => null;

		if (target.Equals("characterTools", StringComparison.OrdinalIgnoreCase))
		{
			var combo = main.AutomationCharacterCombo;
			return (combo.AutomationOpenToolsMenu(), combo.AutomationRefuseMenuPath);
		}

		if (target.StartsWith("loadouts", StringComparison.OrdinalIgnoreCase))
		{
			var stash = main.AutomationStashPanel ?? throw new McpException("No stash panel.");
			var bar = stash.LoadoutBar is { Visible: true } shown ? shown : throw new McpException("The loadout bar is not shown: select a character and the Equipment tab.");
			if (target.Equals("loadouts.list", StringComparison.OrdinalIgnoreCase))
			{
				// Picking a loadout only selects it.
				var field = bar.FieldBounds;
				AutomationInput.SendClick(bar, new Point(field.X + (field.Width / 2), field.Y + (field.Height / 2)), MouseButtons.Left, Keys.None);
				return (bar.ListMenu, None);
			}

			if (!target.Equals("loadouts", StringComparison.OrdinalIgnoreCase))
				throw new McpException($"Unknown target '{target}': loadouts or loadouts.list.");

			AutomationInput.SendContextClick(bar, new Point(bar.FieldBounds.Width / 2, bar.Height / 2));
			string? player = stash.Player?.PlayerName;
			return (bar.ActionMenu, _ => player is not null && AutomationTestNames.IsTestFile(player)
				? null
				: $"Loadout actions only on test characters ({AutomationTestNames.Accepted}).");
		}

		if (target.StartsWith("stash.bag", StringComparison.OrdinalIgnoreCase))
		{
			var stash = main.AutomationStashPanel ?? throw new McpException("No stash panel.");
			if (!int.TryParse(target["stash.bag".Length..], out int bag) || bag < 0 || bag >= stash.BagButtons.Count)
				throw new McpException($"'{target}': give a bag button of the stash panel, stash.bag0 to stash.bag{stash.BagButtons.Count - 1}.");
			var button = stash.BagButtons[bag];
			if (!button.Visible)
				throw new McpException($"Bag button {bag} is hidden: a user could not click it.");
			AutomationInput.SendContextClick(button, AutomationInput.Center(button));
			return (button.ContextMenuStrip ?? throw new McpException($"Bag button {bag} has no menu."), None);
		}

		if (target.Equals("systemMenu", StringComparison.OrdinalIgnoreCase))
		{
			var owner = string.IsNullOrEmpty(form) ? main : FindOpenForm(form) as VaultForm
				?? throw new McpException($"'{form}' is not a TQVault form with its own title bar.");
			var menu = owner.AutomationOpenSystemMenu() ?? throw new McpException($"'{owner.Text}' has Windows' own title bar: its menu is not TQVault's.");
			return (menu, chain => menu.Items.IndexOf(chain[^1]) is 1 or 2
				? "Move and Size wait for the arrow keys (the keyboard move loop): not driven; use move_window."
				: null);
		}

		throw new McpException($"Unknown target '{target}'. Use characterTools, loadouts, loadouts.list, stash.bag<N> or systemMenu.");
	}

	#endregion
}
