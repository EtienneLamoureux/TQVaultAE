using System.ComponentModel;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace TQVaultAE.GUI.Automation;

internal static partial class AutomationTools
{
	#region Main window statistics panel rollovers

	private const int WM_MOUSEMOVE = 0x0200;
	private const int WM_MOUSELEAVE = 0x02A3;

	[McpServerTool(Name = "player_stats_hover", ReadOnly = true)]
	[Description("Rests the virtual cursor on a rollover of the statistics panel next to the equipment (main window, stash bag 0) and lets the panel's own mouse move show its game tool tip, placed from that cursor as a user's hover places it. Returns the tool tip lines, its window rectangle on screen (from the window itself), the hovered area, the equipment panel and the statistics panel on screen, the working area of the screen, whether the tool tip is inside it and whether it covers the hovered area, and a PNG of the main window with the tool tip drawn at its place (the tool tip window cannot be captured headless). Without index, lists the tab's rollovers by number with their first line. hide=true ends the hover (the tool tip closes).")]
	public static IEnumerable<ContentBlock> PlayerStatsHover(
		[Description("Tab of the panel: 1 (I), 2 (II) or 3 (III).")] int tab = 1,
		[Description("Rollover number from 0; omit for the list.")] int? index = null,
		[Description("End the hover instead.")] bool hide = false)
	{
		var (text, png) = AutomationHost.OnUi(form =>
		{
			var stash = form.AutomationStashPanel ?? throw new McpException("No stash panel.");
			var panel = stash.AutomationPlayerStatsPanel;
			if (panel is not { Visible: true })
				throw new McpException("The statistics panel is not shown: select a character and stash bag 0 (equipment).");

			// A leave first, as the pointer coming from elsewhere: the panel forgets the rollover it showed.
			NativeWindows.Send(panel.Handle, WM_MOUSELEAVE, IntPtr.Zero, IntPtr.Zero);
			VirtualCursor.Release();
			if (hide)
				return (Serialize(new { hidden = true }), (byte[]?)null);

			panel.Tab = Math.Clamp(tab, 1, 3) - 1;
			var tips = panel.AutomationToolTips();
			if (index is null)
				return (Serialize(tips.Select((t, i) => new { index = i, first = t.Count > 0 ? t[0].Text : null }).ToList()), (byte[]?)null);
			if (index < 0 || index >= tips.Count)
				throw new McpException($"No rollover {index} on tab {tab}: 0 to {tips.Count - 1}.");

			var area = panel.AutomationToolTipBounds()[index.Value];
			var center = new Point(area.X + area.Width / 2, area.Y + area.Height / 2);
			VirtualCursor.Set(panel, center);
			try
			{
				NativeWindows.Send(panel.Handle, WM_MOUSEMOVE, IntPtr.Zero, (IntPtr)((center.Y << 16) | (center.X & 0xFFFF)));
			}
			finally
			{
				VirtualCursor.Release();
			}

			var tooltip = panel.Tooltip ?? throw new McpException("The panel has no game tool tip.");
			var window = NativeWindows.GetProcessTopLevelWindows()
				.Where(w => w.ClassName.Contains("tooltips_class32", StringComparison.OrdinalIgnoreCase))
				.Select(w => GetWindowRect(w.Handle, out var r) ? Rectangle.FromLTRB(r.Left, r.Top, r.Right, r.Bottom) : Rectangle.Empty)
				.FirstOrDefault(r => !r.IsEmpty);

			var equipment = ResolveShownPanel(form, "equipment").Panel;
			var equipmentScreen = equipment.RectangleToScreen(equipment.ClientRectangle);
			var panelScreen = panel.RectangleToScreen(panel.ClientRectangle);
			var areaScreen = panel.RectangleToScreen(area);
			var formScreen = form.RectangleToScreen(form.ClientRectangle);
			var working = Screen.FromControl(form).WorkingArea;

			// The main window with the statistics panel over the stash, and the tool tip at its place.
			using var rendered = tooltip.Render(tips[index.Value]);
			var placed = window.IsEmpty ? Rectangle.Empty : new Rectangle(window.Location, rendered.Size);
			var canvas = placed.IsEmpty ? formScreen : Rectangle.Union(formScreen, placed);
			using var bitmap = new Bitmap(canvas.Width, canvas.Height);
			using (var g = Graphics.FromImage(bitmap))
			{
				g.Clear(Color.Magenta);
				using (var main = new Bitmap(form.ClientSize.Width, form.ClientSize.Height))
				{
					form.DrawToBitmap(main, new Rectangle(Point.Empty, form.ClientSize));
					g.DrawImage(main, formScreen.X - canvas.X, formScreen.Y - canvas.Y);
				}

				using (var stashPicture = RenderStashWithPlayerInfo(form))
				{
					var stashScreen = stash.RectangleToScreen(stash.ClientRectangle);
					g.DrawImage(stashPicture, stashScreen.X - canvas.X, stashScreen.Y - canvas.Y);
				}

				if (!placed.IsEmpty)
					g.DrawImage(rendered, placed.X - canvas.X, placed.Y - canvas.Y);
			}

			using var stream = new MemoryStream();
			bitmap.Save(stream, ImageFormat.Png);
			static object Box(Rectangle r) => new { r.X, r.Y, r.Width, r.Height };
			return (Serialize(new
			{
				tab = panel.Tab + 1,
				index,
				shown = tooltip.Visible,
				lines = tooltip.Lines.Select(l => l.Text).ToList(),
				tooltipWindow = window.IsEmpty ? null : Box(window),
				renderedSize = new { rendered.Width, rendered.Height },
				hoveredArea = Box(areaScreen),
				statsPanel = Box(panelScreen),
				equipmentPanel = Box(equipmentScreen),
				mainWindow = Box(formScreen),
				workingArea = Box(working),
				insideWorkingArea = !window.IsEmpty && working.Contains(window),
				coversHoveredArea = !window.IsEmpty && window.IntersectsWith(areaScreen),
				overlapsEquipmentPanel = !window.IsEmpty && window.IntersectsWith(equipmentScreen),
			}), stream.ToArray());
		});

		var blocks = new List<ContentBlock> { new TextContentBlock { Text = text } };
		if (png is not null)
		{
			var path = SaveCapture("playerstatshover", png);
			blocks.Add(ImageContentBlock.FromBytes(png, "image/png"));
			blocks.Add(new TextContentBlock { Text = $"saved to {path}" });
		}

		return blocks;
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct NativeRect
	{
		public int Left;
		public int Top;
		public int Right;
		public int Bottom;
	}

	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool GetWindowRect(IntPtr hWnd, out NativeRect rect);

	#endregion
}
