using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using TQVaultAE.GUI.Interop;

namespace TQVaultAE.GUI.Automation;

internal static partial class AutomationTools
{
	internal static partial class DevTools
	{
		#region Main window statistics panel rollovers

		private const int WM_MOUSEMOVE = 0x0200;
		private const int WM_MOUSELEAVE = 0x02A3;

		[McpServerTool(Name = "player_stats_hover", ReadOnly = true)]
		[Description("Rests the virtual cursor on a rollover of the statistics panel next to the equipment (main window, stash bag 0) and lets the panel's own mouse move show its game tool tip, placed from that cursor as a user's hover places it. Returns the tool tip lines, its window rectangle on screen (from the window itself), the hovered area, the equipment panel and the statistics panel on screen, the working area of the screen, whether the tool tip is inside it and whether it covers the hovered area, and a PNG of the main window with the tool tip drawn at its place (the tool tip window cannot be captured headless). Without index, lists the tab's rollovers by number with their first line. hide=true ends the hover (the tool tip closes).")]
		public static IEnumerable<ContentBlock> PlayerStatsHover(
			[Description("Tab of the panel: 1 (I), 2 (II), 3 (III) or 4 (Buffs).")] int tab = 1,
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
				User32.SendMessage(panel.Handle, WM_MOUSELEAVE, IntPtr.Zero, IntPtr.Zero);
				VirtualCursor.Release();
				if (hide)
					return (Serialize(new { hidden = true }), (byte[]?)null);

				panel.Tab = Math.Clamp(tab, 1, 4) - 1;
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
					User32.SendMessage(panel.Handle, WM_MOUSEMOVE, IntPtr.Zero, User32.MakeLParam(center));
				}
				finally
				{
					VirtualCursor.Release();
				}

				var tooltip = panel.Tooltip ?? throw new McpException("The panel has no game tool tip.");
				var window = NativeWindows.GetProcessTopLevelWindows()
					.Where(w => w.ClassName.Contains("tooltips_class32", StringComparison.OrdinalIgnoreCase))
					.Select(w => NativeWindows.WindowRect(w.Handle))
					.FirstOrDefault(r => !r.IsEmpty);

				var equipment = ResolveShownPanel(form, "equipment").Panel;
				var equipmentScreen = equipment.RectangleToScreen(equipment.ClientRectangle);
				var panelScreen = panel.RectangleToScreen(panel.ClientRectangle);
				var areaScreen = panel.RectangleToScreen(area);
				var formScreen = form.RectangleToScreen(form.ClientRectangle);
				var working = Screen.FromControl(form).WorkingArea;

				// The main window with the statistics panel over the stash, and the tool tip at its place.
				using var rendered = tooltip.Render(tips[index.Value]);
				using var stashPicture = RenderStashWithPlayerInfo(form);
				var stashScreen = stash.RectangleToScreen(stash.ClientRectangle).Location;
				using var bitmap = window.IsEmpty
					? AutomationImages.RenderWithOverlays(form, (stashPicture, stashScreen))
					: AutomationImages.RenderWithOverlays(form, (stashPicture, stashScreen), (rendered, window.Location));
				return (Serialize(new
				{
					tab = panel.Tab + 1,
					index,
					shown = tooltip.Visible,
					lines = tooltip.Lines.Select(l => l.Text).ToList(),
					tooltipWindow = window.IsEmpty ? (Rectangle?)null : window,
					renderedSize = rendered.Size,
					hoveredArea = areaScreen,
					statsPanel = panelScreen,
					equipmentPanel = equipmentScreen,
					mainWindow = formScreen,
					workingArea = working,
					insideWorkingArea = !window.IsEmpty && working.Contains(window),
					coversHoveredArea = !window.IsEmpty && window.IntersectsWith(areaScreen),
					overlapsEquipmentPanel = !window.IsEmpty && window.IntersectsWith(equipmentScreen),
				}), AutomationImages.Png(bitmap));
			});

			return AutomationImages.Result(text, png, "playerstatshover");
		}

		#endregion
	}
}
