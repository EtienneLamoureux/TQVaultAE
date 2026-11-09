using System.ComponentModel;
using System.Reflection;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace TQVaultAE.GUI.Automation;

internal static partial class AutomationTools
{
	#region Grids

	[McpServerTool(Name = "grid_values", ReadOnly = true)]
	[Description("Reads a DataGridView of an open form (for example the search results): its column headers and, per row, the text of each cell (images as 'image'), the row text color and the tool tip of the cells. Visible or not: the grids of the other tab pages are read too.")]
	public static string GridValues(
		[Description("Part of the form title or its type name (case-insensitive).")] string form,
		[Description("Grid name, as listed by control_tree.")] string grid)
		=> AutomationHost.OnUi(_ =>
		{
			var target = FindOpenForm(form);
			return Serialize(DescribeGrid(target, FindGrid(target, grid)));
		});

	[McpServerTool(Name = "set_grid_cell", Destructive = true)]
	[Description("Changes a cell of a DataGridView of an open form like a user: a button cell is clicked (CellContentClick), a text cell is edited then left (CellEndEdit). The row is its index or the text of one of its cells (for example a skill name); the column is its index or header text. Read-only cells are refused. Read the grid again with grid_values: a dialog may refresh it after the edit ends.")]
	public static string SetGridCell(
		[Description("Part of the form title or its type name (case-insensitive).")] string form,
		[Description("Grid name, as listed by control_tree.")] string grid,
		[Description("Row index, or the text of one of its cells.")] string row,
		[Description("Column index, or its header text.")] string column,
		[Description("New text of a text cell; ignored for a button cell.")] string value = "")
		=> AutomationHost.OnUi(_ =>
		{
			var target = FindOpenForm(form);
			var view = FindGrid(target, grid);
			if (!view.Enabled)
				throw new McpException($"'{view.Name}' is disabled: a user could not change it.");

			var rowIndex = int.TryParse(row, out var r)
				? r
				: view.Rows.Cast<DataGridViewRow>().FirstOrDefault(x => x.Cells.Cast<DataGridViewCell>().Any(c => string.Equals(Convert.ToString(c.FormattedValue), row, StringComparison.OrdinalIgnoreCase)))?.Index
					?? throw new McpException($"No row with a cell '{row}' in '{view.Name}'.");
			var columnIndex = int.TryParse(column, out var c)
				? c
				: view.Columns.Cast<DataGridViewColumn>().FirstOrDefault(x => string.Equals(x.HeaderText, column, StringComparison.OrdinalIgnoreCase))?.Index
					?? throw new McpException($"No column '{column}' in '{view.Name}'.");
			if (rowIndex < 0 || rowIndex >= view.Rows.Count || columnIndex < 0 || columnIndex >= view.Columns.Count)
				throw new McpException($"Cell ({rowIndex}, {columnIndex}) is outside '{view.Name}'.");

			var cell = view.Rows[rowIndex].Cells[columnIndex];
			if (cell is DataGridViewButtonCell)
			{
				var onClick = typeof(DataGridView).GetMethod("OnCellContentClick", BindingFlags.Instance | BindingFlags.NonPublic)!;
				onClick.Invoke(view, [new DataGridViewCellEventArgs(columnIndex, rowIndex)]);
				return Serialize(new { grid = view.Name, clicked = new { row = rowIndex, column = columnIndex } });
			}

			if (cell.ReadOnly)
				throw new McpException($"Cell ({rowIndex}, {columnIndex}) of '{view.Name}' is read-only.");

			view.CurrentCell = cell;
			view.BeginEdit(true);
			if (view.EditingControl is { } editor)
				editor.Text = value;
			else
				cell.Value = value;
			view.EndEdit();
			return Serialize(new { grid = view.Name, edited = new { row = rowIndex, column = columnIndex, value } });
		});

	private static DataGridView FindGrid(Form form, string grid)
		=> AllControls(form).OfType<DataGridView>().FirstOrDefault(g => string.Equals(g.Name, grid, StringComparison.OrdinalIgnoreCase))
			?? throw new McpException($"No grid '{grid}' in '{form.Text}'. Grids: {string.Join(", ", AllControls(form).OfType<DataGridView>().Select(g => g.Name))}");

	private static object DescribeGrid(Form form, DataGridView grid) => new
	{
		form = form.Text,
		grid = grid.Name,
		enabled = grid.Enabled,
		columns = grid.Columns.Cast<DataGridViewColumn>().Select(c => c.HeaderText).ToList(),
		rows = grid.Rows.Cast<DataGridViewRow>().Select(row => new
		{
			cells = row.Cells.Cast<DataGridViewCell>().Select(c => c.FormattedValue is System.Drawing.Image ? "image" : Convert.ToString(c.FormattedValue)).ToList(),
			foreColor = row.DefaultCellStyle.ForeColor.IsEmpty ? null : row.DefaultCellStyle.ForeColor.Name,
			toolTip = row.Cells.Cast<DataGridViewCell>().Select(c => c.ToolTipText).FirstOrDefault(t => !string.IsNullOrEmpty(t)),
		}).ToList(),
	};

	#endregion
}
