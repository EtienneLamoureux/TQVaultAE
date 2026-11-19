using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using TQVaultAE.Application.Search;
using TQVaultAE.GUI.Dialogs;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Automation;

/// <summary>
/// Tool that runs the item menu's Search (an item's copies) and reads the results window it opens.
/// </summary>
internal static partial class AutomationTools
{
	[McpServerTool(Name = "find_copies", Destructive = true)]
	[Description("Right-clicks a cell of a panel and clicks the item menu's 'Search' entry for 'kind' like the user (SameSeed: copies with the same seed; AllCopies: same base, prefix and suffix, any seed; SameBase; SamePrefix; SameSuffix), then reads the results window it opens: its title and its rows (grid_values' columns: the item searched from comes first, marked '(source)', then the matches of every vault, character, stash and loadout). A disabled entry (a prefix or suffix search of an item without one) is refused. The window stays open: click a row with grid_click, close it with close_form.")]
	public static string FindCopies(
		[Description("Panel id.")] string panel,
		[Description("Cell column.")] int x,
		[Description("Cell row.")] int y,
		[Description("SameSeed, AllCopies, SameBase, SamePrefix or SameSuffix (case does not matter).")] string kind)
	{
		if (!Enum.TryParse<CopySearchKind>(kind, ignoreCase: true, out var search) || !Enum.IsDefined(search))
			throw new McpException($"Unknown kind '{kind}': {string.Join(", ", ItemCopySearch.Kinds)}.");

		var before = AutomationHost.OnUi(form =>
		{
			var named = ResolveShownPanel(form, panel);
			RequireNoDrag(form, "An item is being dragged: a right click would cancel it. Use cancel_drag first.");

			var opened = OpenResultsDialogs();
			try
			{
				named.Panel.AutomationClickContextMenu(new Point(x, y), [Resources.SackPanelMenuSearch, ItemCopySearchText.MenuLabel(search)]);
			}
			finally
			{
				ReleaseCursor(named.Panel);
			}

			return opened;
		});

		// The entry is clicked after the call above returned, and the window fills its grid when shown: wait for both.
		for (int attempt = 0; attempt < 100; attempt++)
		{
			var read = AutomationHost.OnUi(_ =>
			{
				var dialog = OpenResultsDialogs().FirstOrDefault(d => !before.Contains(d));
				if (dialog is null)
					return null;

				var grid = FindGrid(dialog, null);
				return grid.Rows.Count == 0 ? null : Serialize(new { title = dialog.Text, grid = DescribeGrid(dialog, grid) });
			});
			if (read is not null)
				return read;

			Thread.Sleep(50);
		}

		throw new McpException("No results window opened: see recent_log.");
	}

	private static List<ResultsDialog> OpenResultsDialogs()
		=> System.Windows.Forms.Application.OpenForms.OfType<ResultsDialog>().Where(d => d.Visible).ToList();
}
