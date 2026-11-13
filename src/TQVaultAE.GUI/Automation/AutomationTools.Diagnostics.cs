using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using TQVaultAE.Presentation.Win32.Contracts;
using TQVaultAE.GUI.Components.Panels;

namespace TQVaultAE.GUI.Automation;

/// <summary>
/// Tools that check resources the screen does not show: pictures disposed when replaced, fonts in use.
/// </summary>
internal static partial class AutomationTools
{
	[McpServerTool(Name = "stash_background_switch", Destructive = true)]
	[Description("Clicks a bag button of the stash panel like select_bag and reports what happened to the panel background: the picture drawn before the click, whether it was a source picture (kept) or a scaled copy (made at a zoom other than 1), and whether that copy is disposed after the click. Use it to check that tab switches do not leave undisposed copies.")]
	public static string StashBackgroundSwitch([Description("Zero-based stash bag button index (0 is the equipment).")] int bag)
		=> AutomationHost.OnUi(form =>
		{
			if (AutomationPanels.Resolve(form, "stash").Container is not StashPanel stash)
				throw new McpException("No stash panel.");

			if (bag < 0 || bag >= stash.BagButtons.Count)
				throw new McpException($"Bag {bag} is out of range 0..{stash.BagButtons.Count - 1}.");

			var before = stash.AutomationBackground;
			var beforeIsSource = before is not null && stash.AutomationIsSourceBackground(before);
			var button = stash.BagButtons[bag];
			OnMouseDownMethod.Invoke(button, [new MouseEventArgs(MouseButtons.Left, 1, button.Width / 2, button.Height / 2, 0)]);
			stash.Refresh();
			var after = stash.AutomationBackground;

			return Serialize(new
			{
				currentBag = stash.ShownBag(),
				zoom = Program.ServiceProvider.GetRequiredService<IUIService>().Scale,
				before = before is null ? null : new { source = beforeIsSource, replaced = !ReferenceEquals(before, after), disposed = IsDisposed(before) },
				after = after is null ? null : new { source = stash.AutomationIsSourceBackground(after), size = IsDisposed(after) ? null : (object)new { after.Width, after.Height } },
			});
		});

	[McpServerTool(Name = "player_stats_fonts", ReadOnly = true)]
	[Description("Lists the fonts of the statistics panel shown next to the equipment (name, class, text, title, number, tab and resistance fonts: family, size, unit, style) and the application base font, to check that the panel follows a live font change (settings).")]
	public static string PlayerStatsFonts()
		=> AutomationHost.OnUi(form =>
		{
			var panel = (AutomationPanels.Resolve(form, "stash").Container as StashPanel)?.AutomationPlayerStatsPanel
				?? throw new McpException("No statistics panel yet: select a character first.");

			static object? Describe(Font? font) => font is null ? null : new { family = font.FontFamily.Name, font.Size, unit = font.Unit.ToString(), style = font.Style.ToString() };

			return Serialize(new
			{
				baseFont = Describe(Program.ServiceProvider.GetRequiredService<IFontService>().GetFontLight(11F)),
				nameFont = Describe(panel.NameFont),
				classFont = Describe(panel.ClassFont),
				textFont = Describe(panel.TextFont),
				titleFont = Describe(panel.TitleFont),
				numberFont = Describe(panel.NumberFont),
				tabFont = Describe(panel.TabFont),
				resistanceFont = Describe(panel.ResistanceFont),
			});
		});

	/// <summary>A disposed GDI+ bitmap throws on any property read.</summary>
	private static bool IsDisposed(Bitmap bitmap)
	{
		try
		{
			_ = bitmap.Width;
			return false;
		}
		catch (ArgumentException)
		{
			return true;
		}
	}
}
