using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Dialogs;

namespace TQVaultAE.GUI.Automation;

internal static partial class AutomationTools
{
	internal static partial class DevTools
	{
		#region Results window: Move To and Undo

		[McpServerTool(Name = "results_move", Destructive = true)]
		[Description("Drives Move To and Undo of the open results window (Advanced Search or the item menu's Search, see find_copies) through its own handlers. action 'state' reads it: per row its item, container, container type, bag and whether it is selected, the report line, the moves Undo can still put back, the enabled buttons and the entries of its right-click menu (built as it opens). action 'select' selects rows like Ctrl clicks ('rows': 'all' like the Select All button, 'none', or row indexes '0,2,5'). action 'menu' clicks an entry of the right-click menu along 'path' (' > ' between levels, e.g. 'Move To > Vaults > vault-test1 > Bag #2', 'Move To > Transfer Stash', 'Undo'); with 'menu'='move' the path is in the Move To button's menu instead ('Relic Vault'). Test data only: every selected item must be in a test character or vault ('player-test', 'player-mod', 'vault-'), and a vault target a test vault; a move to a shared stash changes the real transfer stash or relic vault once saved: undo it, never save it. Returns the state after the click.")]
		public static string ResultsMove(
			[Description("state, select or menu.")] string action = "state",
			[Description("For select: 'all', 'none' or row indexes separated by commas.")] string? rows = null,
			[Description("For menu: the entries to click, separated by ' > '.")] string? path = null,
			[Description("For menu: 'results' (right-click menu, default) or 'move' (the Move To button's menu).")] string menu = "results")
		{
			switch (action.ToLowerInvariant())
			{
				case "state":
					return AutomationHost.OnUi(_ => Serialize(ResultsMoveState(OpenResultsDialog())));

				case "select":
					return AutomationHost.OnUi(_ =>
					{
						var dialog = OpenResultsDialog();
						SelectResultRows(dialog, rows ?? throw new McpException("'rows' is required: 'all', 'none' or indexes."));
						return Serialize(ResultsMoveState(dialog));
					});

				case "menu":
					var steps = MenuPath(path);
					if (steps.Length == 0)
						throw new McpException("'path' is required, e.g. 'Move To > Transfer Stash'.");

					AutomationHost.OnUi(_ =>
					{
						var dialog = OpenResultsDialog();
						if (string.Equals(menu, "move", StringComparison.OrdinalIgnoreCase) || steps.Any(s => string.Equals(s, Presentation.Resources.ResultsMenuMoveTo, StringComparison.OrdinalIgnoreCase)))
							CheckResultsMoveTestData(dialog, steps);
						var strip = ResultsMenuFilled(dialog, menu);
						return AutomationMenus.ClickLater(dialog, AutomationMenus.Resolve(strip, steps));
					});
					// The click runs after the call above returned: this call queues behind it.
					return AutomationHost.OnUi(_ => Serialize(ResultsMoveState(OpenResultsDialog())));

				default:
					throw new McpException($"Unknown action '{action}': state, select or menu.");
			}
		}

		[McpServerTool(Name = "capture_results_menu", ReadOnly = true)]
		[Description("Renders a menu of the open results window to PNG as it opens, with the sub menus opened along 'open' (' > ' between levels, e.g. 'Move To > Vaults'), then closes it without choosing anything: 'results' for the right-click menu of the list, 'move' for the menu of the Move To button.")]
		public static IEnumerable<ContentBlock> CaptureResultsMenu(
			[Description("'results' or 'move'.")] string menu = "results",
			[Description("Sub menus to open, texts separated by ' > '; empty for the menu alone.")] string? open = null)
		{
			var steps = MenuPath(open);
			var png = AutomationHost.OnUi(_ =>
			{
				var dialog = OpenResultsDialog();
				var strip = string.Equals(menu, "move", StringComparison.OrdinalIgnoreCase) ? dialog.MoveMenu : dialog.ResultsMenu;
				var anchor = string.Equals(menu, "move", StringComparison.OrdinalIgnoreCase)
					? (Control)dialog.Controls.Find("moveButton", true).Single()
					: dialog.ResultsGrid;
				// Shown like the user opens it: the menu fills itself when it opens.
				strip.Show(anchor, new Point(0, anchor.Height / 2));
				using var bitmap = AutomationMenus.Render(strip, steps);
				return AutomationImages.Png(bitmap);
			});

			return AutomationImages.Result(null, png, $"results-menu-{menu}");
		}

		private static ResultsDialog OpenResultsDialog()
			=> OpenResultsDialogs().LastOrDefault() ?? throw new McpException("No results window is open: use find_copies or the Advanced Search first.");

		/// <summary>A menu of the results window, filled as when it opens.</summary>
		private static ContextMenuStrip ResultsMenuFilled(ResultsDialog dialog, string menu)
		{
			bool move = string.Equals(menu, "move", StringComparison.OrdinalIgnoreCase);
			var strip = move ? dialog.MoveMenu : dialog.ResultsMenu;
			dialog.FillMenu(strip, full: !move);
			return strip;
		}

		private static void SelectResultRows(ResultsDialog dialog, string rows)
		{
			var grid = dialog.ResultsGrid;
			if (string.Equals(rows, "all", StringComparison.OrdinalIgnoreCase))
			{
				dialog.SelectAllRows();
				return;
			}

			grid.ClearSelection();
			if (string.Equals(rows, "none", StringComparison.OrdinalIgnoreCase))
				return;

			foreach (var part in rows.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
			{
				if (!int.TryParse(part, out int index) || index < 0 || index >= grid.Rows.Count)
					throw new McpException($"No row '{part}': {grid.Rows.Count} rows.");
				grid.Rows[index].Selected = true;
			}
		}

		/// <summary>
		/// Refuses a move of items out of a container that is not test data, or into a vault that is not: nothing blocks
		/// the save of what automation changed.
		/// </summary>
		private static void CheckResultsMoveTestData(ResultsDialog dialog, string[] steps)
		{
			foreach (var result in dialog.SelectedResults())
			{
				var place = result.Item.Place;
				bool shared = place.StashType is StashType.TransferStash or StashType.RelicVaultStash;
				if (shared || !AutomationTestNames.IsTestFile(place.Name))
					throw new McpException($"'{result.ItemName}' is in '{place.Name}', not test data ({AutomationTestNames.Accepted}): select test items only.");
			}

			int vaults = Array.FindIndex(steps, s => string.Equals(s, Presentation.Resources.ResultsMenuVaults, StringComparison.OrdinalIgnoreCase));
			if (vaults >= 0 && vaults + 1 < steps.Length && !AutomationTestNames.IsTestFile(steps[vaults + 1]))
				throw new McpException($"'{steps[vaults + 1]}' is not a test vault ({AutomationTestNames.Accepted}).");
		}

		/// <summary>
		/// The entries of the right-click menu and of its Move To sub menu, with the vault names only: opening a vault's entry
		/// reads it to list its bags.
		/// </summary>
		private static IEnumerable<object> DescribeResultsMenu(ToolStripItemCollection items)
			=> items.Cast<ToolStripItem>().Select(item => new
			{
				text = item is ToolStripSeparator ? "-" : item.Text,
				enabled = item.Enabled,
				entries = item is ToolStripMenuItem { HasDropDownItems: true } sub
					? sub.Name == "moveVaults"
						? sub.DropDownItems.Cast<ToolStripItem>().Select(v => (object)new { text = v.Text, enabled = v.Enabled })
						: DescribeResultsMenu(sub.DropDownItems)
					: null,
			});

		private static object ResultsMoveState(ResultsDialog dialog)
		{
			var grid = dialog.ResultsGrid;
			var strip = ResultsMenuFilled(dialog, "results");
			return new
			{
				title = dialog.Text,
				rows = grid.Rows.Cast<DataGridViewRow>().Select(row => new
				{
					row = row.Index,
					item = Convert.ToString(row.Cells[0].FormattedValue),
					container = Convert.ToString(row.Cells[2].FormattedValue),
					type = Convert.ToString(row.Cells[3].FormattedValue),
					bag = Convert.ToString(row.Cells[4].FormattedValue),
					selected = row.Selected,
				}),
				report = dialog.MoveReport,
				undo = dialog.UndoCount,
				buttons = new[] { "selectAllButton", "moveButton", "undoButton" }
					.ToDictionary(name => name, name => dialog.Controls.Find(name, true).Single().Enabled),
				menu = DescribeResultsMenu(strip.Items),
			};
		}

		#endregion
	}
}
