using System.ComponentModel;
using System.Drawing.Imaging;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using TQVaultAE.GUI.Dialogs;

namespace TQVaultAE.GUI.Automation;

/// <summary>
/// Tool that opens the item comparison dialog on two items and returns what it shows.
/// </summary>
internal static partial class AutomationTools
{
	[McpServerTool(Name = "compare_items", ReadOnly = true)]
	[Description("Opens the item comparison dialog directly on two items, which may be on different panels (equipment vs vault), with its toolbar choices (alignRows, hideUnchanged, showSources; empty keeps the last ones) and per-stat source clicks (toggle), and returns its four columns as rendered (firstDifferences, first, second, secondDifferences; rows as tooltip_text_dump: kind, text, color, bold) with a PNG of the whole content, scroll included. The dialog is shown modeless and closed at the end unless keepOpen (then capture it by its title and close it with close_dialog). Nothing is changed.")]
	public static IEnumerable<ContentBlock> CompareItems(
		[Description("Panel id of the first item.")] string panel,
		[Description("Cell column of the first item.")] int x,
		[Description("Cell row of the first item.")] int y,
		[Description("Cell column of the second item.")] int x2,
		[Description("Cell row of the second item.")] int y2,
		[Description("Panel id of the second item; empty for the first item's panel.")] string? panel2 = null,
		[Description("PNG file the capture is also written to; empty for a temporary file.")] string? path = null,
		[Description("Leave the dialog open.")] bool keepOpen = false,
		[Description("The 'Align rows' check box; empty keeps the last choice.")] bool? alignRows = null,
		[Description("The 'Hide unchanged' check box; empty keeps the last choice.")] bool? hideUnchanged = null,
		[Description("The 'Show sources' check box; empty keeps the last choice.")] bool? showSources = null,
		[Description("Rows to click, like the user, to show or hide one stat's sources: '|'-separated texts, each the start of a clickable row's text (difference or item line of a stat with sources, white space trimmed), clicked in order after the check boxes.")] string? toggle = null)
	{
		var (json, png, size) = AutomationHost.OnUi(main =>
		{
			var first = ResolveShownPanel(main, panel);
			var second = string.IsNullOrWhiteSpace(panel2) ? first : ResolveShownPanel(main, panel2);
			var item1 = first.Panel.AutomationFindItem(new Point(x, y))
				?? throw new McpException($"No item at {x},{y} in '{first.Id}'.");
			var item2 = second.Panel.AutomationFindItem(new Point(x2, y2))
				?? throw new McpException($"No item at {x2},{y2} in '{second.Id}'.");

			var dialog = main.Services.Forms.Create<ItemComparisonDialog>();
			dialog.First = item1;
			dialog.Second = item2;
			dialog.Show(main);

			try
			{
				if (alignRows is { } align)
					((CheckBox)dialog.Controls.Find("alignRowsCheckBox", true).Single()).Checked = align;
				if (hideUnchanged is { } hide)
					((CheckBox)dialog.Controls.Find("hideUnchangedCheckBox", true).Single()).Checked = hide;
				if (showSources is { } sources)
					((CheckBox)dialog.Controls.Find("showSourcesCheckBox", true).Single()).Checked = sources;

				var table = dialog.Controls.Find("comparisonTable", true).Single();
				var toggled = new List<string>();
				foreach (var text in (toggle ?? string.Empty).Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
				{
					// A row the dialog made clickable: its key in Tag and the hand cursor, as the mouse finds it.
					var row = ItemComparisonDialog.ColumnNames.SelectMany(n => table.Controls[n]!.Controls.Cast<Control>())
						.FirstOrDefault(c => c.Tag is string && c.Cursor == Cursors.Hand && RowText(c).TrimStart().StartsWith(text, StringComparison.Ordinal))
						?? throw new McpException($"No clickable row starts with '{text}'.");
					// The rows are made again: read the clicked one first. The mouse lands on a label: the row's first
					// colored piece for a row in pieces. Its click handler shows or hides the sources once the click
					// is over (posted to the message loop): let the loop run it.
					toggled.Add(RowText(row).Trim());
					var target = row is Label || row.Controls.Count == 0 ? row : row.Controls[0];
					ClickMethod.Invoke(target, [EventArgs.Empty]);
					System.Windows.Forms.Application.DoEvents();
				}

				var columns = ItemComparisonDialog.ColumnNames.ToDictionary(n => n, n =>
				{
					var recorder = new ItemTooltipRecorder();
					recorder.Panel(table.Controls[n]!);
					return recorder.Rows;
				});

				// The whole dialog with its border when it shows all the content; else the whole table at its size,
				// beyond what the dialog shows without scrolling.
				Control captured = table.Parent is ScrollableControl scroll
					&& table.Width <= scroll.ClientSize.Width && table.Height <= scroll.ClientSize.Height ? dialog : table;
				using var bitmap = new Bitmap(captured.Width, captured.Height);
				captured.DrawToBitmap(bitmap, new Rectangle(Point.Empty, captured.Size));
				using var stream = new MemoryStream();
				bitmap.Save(stream, ImageFormat.Png);

				var result = Serialize(new
				{
					first = DescribeItem(item1, null),
					second = DescribeItem(item2, null),
					options = dialog.Options,
					toggled,
					dialogSize = new { dialog.Width, dialog.Height },
					contentSize = new { table.Width, table.Height },
					columns,
				});
				return (result, stream.ToArray(), captured.Size);
			}
			finally
			{
				if (!keepOpen)
				{
					dialog.Close();
					dialog.Dispose();
				}
			}
		});

		var file = string.IsNullOrWhiteSpace(path)
			? Path.Combine(Path.GetTempPath(), $"tqvault-compare-{DateTime.Now:yyyyMMdd-HHmmss-fff}.png")
			: Path.GetFullPath(path);
		File.WriteAllBytes(file, png);

		return
		[
			new TextContentBlock { Text = json },
			ImageContentBlock.FromBytes(png, "image/png"),
			new TextContentBlock { Text = $"capture: {size.Width}x{size.Height} px, saved to {file}" },
		];
	}

	/// <summary><c>Control.OnClick</c>, which raises the control's Click event as a mouse click does.</summary>
	private static readonly System.Reflection.MethodInfo ClickMethod = typeof(Control).GetMethod("OnClick",
		System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, [typeof(EventArgs)])!;

	/// <summary>The text of a comparison row: a label's, or its colored pieces' put together.</summary>
	private static string RowText(Control row)
		=> row is Label ? row.Text : string.Concat(row.Controls.Cast<Control>().Select(c => c.Text));

	[McpServerTool(Name = "compare_click", Destructive = true)]
	[Description("Clicks a cell of a panel like the user while the compare selection may be on (started by the item menu's 'Compare' entry: context_menu_click(panel, x, y, 'Compare')). Left on an item: picks the second item and opens the comparison dialog (modal, after the call returns: drive it with set_control / control_values / capture('Item Comparison') and close it with close_dialog('Item Comparison', 'OK')); left on the first item ends the selection; left on an empty cell keeps it; right cancels it. Without the selection on, a left click picks the item up like pick_item. Returns the compare selection before and after.")]
	public static string CompareClick(
		[Description("Panel id.")] string panel,
		[Description("Cell column.")] int x,
		[Description("Cell row.")] int y,
		[Description("'left' or 'right'.")] string button = "left")
		=> AutomationHost.OnUi(main =>
		{
			var named = ResolveShownPanel(main, panel);
			var selection = main.AutomationDragInfo.Compare;
			var before = new { active = selection.IsActive, first = selection.First is { } f ? DescribeItem(f, null) : null };
			var item = named.Panel.AutomationFindItem(new Point(x, y));
			var mouse = button.ToLowerInvariant() switch
			{
				"left" => MouseButtons.Left,
				"right" => MouseButtons.Right,
				_ => throw new McpException($"Unknown button '{button}'. Use 'left' or 'right'."),
			};

			named.Panel.AutomationMouseDown(named.Panel.AutomationCellCenter(new Point(x, y)), mouse);
			named.Panel.AutomationCloseContextMenu();

			return Serialize(new
			{
				panel = named.Id,
				clicked = item is null ? null : DescribeItem(item, null),
				before,
				after = new { active = selection.IsActive, first = selection.First is { } f2 ? DescribeItem(f2, null) : null },
				cursor = named.Panel.Cursor == Cursors.Hand ? "hand" : "default",
			});
		});
}
