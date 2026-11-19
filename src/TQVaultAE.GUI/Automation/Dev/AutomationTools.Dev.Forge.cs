using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using TQVaultAE.Domain.Forge;
using TQVaultAE.GUI.Components.Panels;
using TQVaultAE.GUI.Components;
using TQVaultAE.GUI.Forms;

namespace TQVaultAE.GUI.Automation;

internal static partial class AutomationTools
{
	internal static partial class DevTools
	{
		[McpServerTool(Name = "forge_drag_cursor")]
		[Description("Runs the forge's drag cursor update, like a mouse move over the forge while an item is dragged (pick_item first). Returns whether a cursor was built from the item's bitmap, its size and hot spot (the bitmap centre).")]
		public static string ForgeDragCursor()
			=> AutomationHost.OnUi(form =>
			{
				var cursor = RequireForge(form).AutomationUpdateDragCursor();
				return Serialize(new
				{
					built = cursor is not null,
					width = cursor?.Size.Width,
					height = cursor?.Size.Height,
					hotSpot = cursor?.HotSpot,
				});
			});

		[McpServerTool(Name = "forge_hover", Destructive = true)]
		[Description("Rests the mouse on a forge slot through the slot's own MouseEnter: the item tooltip shows, for the Base slot the preview of the forged item. Returns the tooltips shown, the tooltip and forge rectangles on screen, and a PNG of the main window with the tooltip drawn at its place. hide=true ends the hover (MouseLeave), which closes the tooltip.")]
		public static IEnumerable<ContentBlock> ForgeHover(
			[Description("Base, Prefix, Suffix, Relic1 or Relic2.")] string slot,
			[Description("true to end the hover instead.")] bool hide = false)
		{
			var (json, png) = AutomationHost.OnUi(form =>
			{
				var forge = RequireForge(form);
				var value = ParseEnum<ForgeSlot>(slot, "slot");
				if (hide)
				{
					forge.AutomationHoverEnd(value);
					return (Serialize(new { slot = value.ToString(), tooltips = DescribeTooltips() }), (byte[]?)null);
				}

				if (form.AutomationDragInfo.IsActive)
					throw new McpException("An item is being dragged: the forge shows no tooltip then. Drop it or cancel_drag first.");

				forge.AutomationHover(value);
				var tooltip = OpenTooltips().LastOrDefault();
				var formScreen = form.RectangleToScreen(form.ClientRectangle);
				var tooltipScreen = tooltip?.Bounds ?? Rectangle.Empty;
				byte[]? image = null;
				if (tooltip is { Width: > 0, Height: > 0 })
				{
					using var tip = AutomationImages.Render(tooltip);
					using var bitmap = AutomationImages.RenderWithOverlays(form, (tip, tooltipScreen.Location));
					image = AutomationImages.Png(bitmap);
				}

				return (Serialize(new
				{
					slot = value.ToString(),
					tooltips = DescribeTooltips(),
					tooltipWindow = tooltip is null ? (Rectangle?)null : tooltipScreen,
					forgePanel = forge.RectangleToScreen(forge.ClientRectangle),
					mainWindow = formScreen,
				}), image);
			});

			return AutomationImages.Result(json, png);
		}

		[McpServerTool(Name = "recent_sounds", ReadOnly = true)]
		[Description("Returns the last sounds TQVault played, oldest first. For a named sound, 'loaded' and 'wavBytes' tell whether its data could be loaded (an MP3 is converted to WAV by NAudio first).")]
		public static string RecentSounds([Description("Number of sounds (default 20).")] int n = 20)
			=> Serialize(new
			{
				soundsEnabled = Services.SoundService is RecordingSoundService recorder
					? recorder.SoundsEnabled
					: (bool?)null,
				sounds = RecordingSoundService.Last(Math.Clamp(n, 1, 200)).Select(DescribeSound),
			});

		internal static object DescribeSound(RecordingSoundService.PlayedSound sound) => new
		{
			time = sound.Time.ToString("HH:mm:ss.fff"),
			call = sound.Call,
			resource = sound.Resource,
			loaded = sound.Loaded,
			wavBytes = sound.WavBytes,
			error = sound.Error,
		};
	}
}
