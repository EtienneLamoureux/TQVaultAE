using System.ComponentModel;
using System.Reflection;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using TQVaultAE.GUI.Components.Layout;

namespace TQVaultAE.GUI.Automation;

internal static partial class AutomationTools
{
	#region Lists and grids

	[McpServerTool(Name = "list_items", ReadOnly = true)]
	[Description("Reads the entries of the list controls of an open form: check box lists (the search dialog's categories), list boxes and data grids, with the checked or selected state. Without control, every list of the form with its entry count and checked entries; with control, its entries (from skip, at most take).")]
	public static string ListItems(
		[Description("Part of the form title or its type name (case-insensitive).")] string form,
		[Description("List control name; empty for a summary of every list.")] string? control = null,
		[Description("Entries to skip.")] int skip = 0,
		[Description("Maximum entries returned.")] int take = 200)
		=> AutomationHost.OnUi(_ =>
		{
			var target = FindOpenForm(form);
			var lists = AllControls(target).Where(c => !string.IsNullOrEmpty(c.Name) && c is VirtualCheckedListBox or ListBox or DataGridView).ToList();
			if (string.IsNullOrEmpty(control))
			{
				return Serialize(new
				{
					form = target.Text,
					lists = lists.Select(c => new
					{
						name = c.Name,
						type = c.GetType().Name,
						visible = c.Visible,
						enabled = c.Enabled,
						count = Entries(c).Count,
						@checked = Entries(c).Where(e => e.on).Select(e => e.text).Take(20).ToList(),
					}).ToList(),
				});
			}

			var list = lists.FirstOrDefault(c => string.Equals(c.Name, control, StringComparison.OrdinalIgnoreCase))
				?? throw new McpException($"No list '{control}' in '{target.Text}'. Lists: {string.Join(", ", lists.Select(c => c.Name))}");
			var entries = Entries(list);
			return Serialize(new
			{
				name = list.Name,
				count = entries.Count,
				entries = entries.Skip(skip).Take(take).Select((e, i) => new { index = skip + i, e.text, @checked = e.on }).ToList(),
			});
		});

	[McpServerTool(Name = "check_list_items", Destructive = true)]
	[Description("Ticks or unticks entries of a check box list of an open form like a user click, so its change event runs (the search dialog then updates its result count). Entries by exact text, else the first containing it, or by index ('#3'), separated by '|'. Returns the checked entries afterwards.")]
	public static string CheckListItems(
		[Description("Part of the form title or its type name (case-insensitive).")] string form,
		[Description("Check box list name, as listed by list_items.")] string control,
		[Description("Entries separated by '|', by text or '#index'.")] string items,
		[Description("true to tick, false to untick.")] bool check = true)
		=> AutomationHost.OnUi(_ =>
		{
			var target = FindOpenForm(form);
			var c = AllControls(target).FirstOrDefault(x => string.Equals(x.Name, control, StringComparison.OrdinalIgnoreCase))
				?? throw new McpException($"No control '{control}' in '{target.Text}'.");
			if (!c.Enabled || !c.Visible)
				throw new McpException($"'{c.Name}' is disabled or hidden: a user could not click it.");

			foreach (var wanted in items.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
			{
				var entries = Entries(c);
				int index = wanted.StartsWith('#') && int.TryParse(wanted[1..], out int n) ? n
					: entries.FindIndex(e => string.Equals(e.text, wanted, StringComparison.OrdinalIgnoreCase)) is int exact and >= 0 ? exact
					: entries.FindIndex(e => e.text.Contains(wanted, StringComparison.OrdinalIgnoreCase));
				if (index < 0 || index >= entries.Count)
					throw new McpException($"No entry '{wanted}' in '{c.Name}'.");

				switch (c)
				{
					case VirtualCheckedListBox virtualList:
						if (virtualList.GetItemChecked(index) != check)
						{
							virtualList.SetItemChecked(index, check);
							RaiseEvent(virtualList, nameof(VirtualCheckedListBox.SelectedValueChanged));
						}
						break;
					case CheckedListBox checkedList:
						checkedList.SetItemChecked(index, check);
						break;
					case ListBox listBox:
						listBox.SetSelected(index, check);
						break;
					default:
						throw new McpException($"'{c.Name}' is a {c.GetType().Name}, not a check box list.");
				}
			}

			return Serialize(new { name = c.Name, @checked = Entries(c).Where(e => e.on).Select(e => e.text).ToList() });
		});

	[McpServerTool(Name = "grid_click", Destructive = true)]
	[Description("Clicks a row of a data grid of an open form like a user: the row becomes current (RowEnter) and gets a cell click; double=true adds the double click (the results dialog then jumps to the item, and a double click closes it). The click runs after this call returns. Rows as listed by list_items.")]
	public static string GridClick(
		[Description("Part of the form title or its type name (case-insensitive).")] string form,
		[Description("Row index.")] int row,
		[Description("Grid name; empty for the form's only grid.")] string? control = null,
		[Description("Column index.")] int column = 0,
		[Description("true for a double click.")] bool @double = false)
		=> AutomationHost.OnUi(main =>
		{
			var target = FindOpenForm(form);
			var grids = AllControls(target).OfType<DataGridView>().Where(g => string.IsNullOrEmpty(control) || string.Equals(g.Name, control, StringComparison.OrdinalIgnoreCase)).ToList();
			if (grids.Count != 1)
				throw new McpException($"{grids.Count} grids match in '{target.Text}': {string.Join(", ", AllControls(target).OfType<DataGridView>().Select(g => g.Name))}");
			var grid = grids[0];
			if (row < 0 || row >= grid.Rows.Count || column < 0 || column >= grid.Columns.Count)
				throw new McpException($"Row {row} / column {column} out of range ({grid.Rows.Count} rows, {grid.Columns.Count} columns).");

			main.BeginInvoke(() =>
			{
				grid.CurrentCell = grid.Rows[row].Cells[column];
				var args = new DataGridViewCellEventArgs(column, row);
				Invoke(grid, "OnCellClick", args);
				if (@double)
					Invoke(grid, "OnCellDoubleClick", args);
			});
			return Serialize(new { form = target.Text, grid = grid.Name, row, values = grid.Rows[row].Cells.Cast<DataGridViewCell>().Select(c => c.FormattedValue?.ToString()).ToList() });
		});

	private static List<(string text, bool on)> Entries(Control c) => c switch
	{
		VirtualCheckedListBox v => Enumerable.Range(0, v.Items.Count).Select(i => (v.Items[i]?.ToString() ?? string.Empty, v.GetItemChecked(i))).ToList(),
		CheckedListBox cl => Enumerable.Range(0, cl.Items.Count).Select(i => (cl.GetItemText(cl.Items[i]) ?? string.Empty, cl.GetItemChecked(i))).ToList(),
		ListBox l => Enumerable.Range(0, l.Items.Count).Select(i => (l.GetItemText(l.Items[i]) ?? string.Empty, l.GetSelected(i))).ToList(),
		DataGridView g => g.Rows.Cast<DataGridViewRow>()
			.Select(r => (string.Join(" | ", r.Cells.Cast<DataGridViewCell>().Select(cell => cell.FormattedValue?.ToString())), r.Selected))
			.ToList(),
		_ => [],
	};

	private static void RaiseEvent(object target, string eventName)
	{
		var field = target.GetType().GetField(eventName, BindingFlags.Instance | BindingFlags.NonPublic);
		(field?.GetValue(target) as EventHandler)?.Invoke(target, EventArgs.Empty);
	}

	private static void Invoke(Control target, string method, EventArgs args)
		=> typeof(DataGridView).GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic, [args.GetType()])!.Invoke(target, [args]);

	#endregion
}
