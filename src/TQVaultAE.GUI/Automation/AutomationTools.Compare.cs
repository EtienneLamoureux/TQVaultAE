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
	[Description("Opens the item comparison dialog (the context menu entry 'Compare' of a selection of two items) on two items, which may be on different panels (equipment vs vault), and returns its four columns as rendered (firstDifferences, first, second, secondDifferences; rows as tooltip_text_dump: kind, text, color, bold) with a PNG of the whole content, scroll included. The dialog is shown modeless and closed at the end unless keepOpen (then capture it by its title and close it with close_dialog). Nothing is changed.")]
	public static IEnumerable<ContentBlock> CompareItems(
		[Description("Panel id of the first item.")] string panel,
		[Description("Cell column of the first item.")] int x,
		[Description("Cell row of the first item.")] int y,
		[Description("Cell column of the second item.")] int x2,
		[Description("Cell row of the second item.")] int y2,
		[Description("Panel id of the second item; empty for the first item's panel.")] string? panel2 = null,
		[Description("PNG file the capture is also written to; empty for a temporary file.")] string? path = null,
		[Description("Leave the dialog open.")] bool keepOpen = false)
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
				var table = dialog.Controls.Find("comparisonTable", true).Single();
				var columns = ItemComparisonDialog.ColumnNames.ToDictionary(n => n, n =>
				{
					var recorder = new ItemTooltipRecorder();
					recorder.Panel(table.Controls[n]!);
					return recorder.Rows;
				});

				// The whole table at its size, beyond what the dialog shows without scrolling.
				using var bitmap = new Bitmap(table.Width, table.Height);
				table.DrawToBitmap(bitmap, new Rectangle(Point.Empty, table.Size));
				using var stream = new MemoryStream();
				bitmap.Save(stream, ImageFormat.Png);

				var result = Serialize(new
				{
					first = DescribeItem(item1, null),
					second = DescribeItem(item2, null),
					dialogSize = new { dialog.Width, dialog.Height },
					contentSize = new { table.Width, table.Height },
					columns,
				});
				return (result, stream.ToArray(), table.Size);
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
}
