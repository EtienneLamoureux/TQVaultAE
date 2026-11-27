using ModelContextProtocol;
using static TQVaultAE.Automation.ControlTree;

namespace TQVaultAE.GUI.Automation;

/// <summary>The grids of the open forms, read by the user tools (find_copies) and the Debug grid tools.</summary>
internal static partial class AutomationTools
{
	/// <summary>The grid of <paramref name="form"/> named <paramref name="grid"/>; with no name, its only grid.</summary>
	private static DataGridView FindGrid(Control form, string? grid)
	{
		var grids = AllControls(form).OfType<DataGridView>().ToList();
		if (string.IsNullOrEmpty(grid))
			return grids.Count == 1 ? grids[0]
				: grids.Count == 0 ? throw new McpException($"No grid in '{form.Text}'.")
				: throw new McpException($"{grids.Count} grids in '{form.Text}': name one of {string.Join(", ", grids.Select(g => g.Name))}.");

		return grids.FirstOrDefault(g => string.Equals(g.Name, grid, StringComparison.OrdinalIgnoreCase))
			?? throw new McpException($"No grid '{grid}' in '{form.Text}'. Grids: {string.Join(", ", grids.Select(g => g.Name))}");
	}

	private static object DescribeGrid(Control form, DataGridView grid) => new
	{
		form = form.Text,
		grid = grid.Name,
		enabled = grid.Enabled,
		columns = grid.Columns.Cast<DataGridViewColumn>().Select(c => c.HeaderText).ToList(),
		rows = grid.Rows.Cast<DataGridViewRow>().Select(row => new
		{
			cells = row.Cells.Cast<DataGridViewCell>().Select(c => c.FormattedValue is System.Drawing.Image ? "image" : Convert.ToString(c.FormattedValue)).ToList(),
			foreColor = AutomationJson.HexOrNull(row.DefaultCellStyle.ForeColor),
			toolTip = row.Cells.Cast<DataGridViewCell>().Select(c => c.ToolTipText).FirstOrDefault(t => !string.IsNullOrEmpty(t)),
		}).ToList(),
	};
}
