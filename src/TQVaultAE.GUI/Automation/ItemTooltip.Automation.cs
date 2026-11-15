using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Automation;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Tooltips;

/// <summary>
/// Entry point of the Debug tool <c>tooltip_text_dump</c>: builds an item tooltip without showing it
/// and records the rows its own code creates, panel by panel, before they are drawn.
/// </summary>
public partial class ItemTooltip
{
	/// <summary>Receives the rows of the panels built by <see cref="FillToolTip"/>, when set.</summary>
	internal ItemTooltipRecorder? AutomationRecorder { get; private set; }

	/// <summary>
	/// Builds the tooltip of <paramref name="item"/> with <paramref name="view"/>, like a hover does but bypassing the image cache,
	/// and returns its rows in display order.
	/// </summary>
	internal static IReadOnlyList<TooltipDumpRow> AutomationDumpRows(GuiServices services, Item item, ItemTooltipView view, bool inGameValues)
	{
		// A cached image would skip the build: drop it before, and the dump's own image after.
		InvalidateCache(item);
		var recorder = new ItemTooltipRecorder();
		try
		{
			using var tooltip = new ItemTooltip(services, item)
			{
				View = view,
				ShowInGameValues = inGameValues,
				ItemDatabase = services.ItemDatabaseService,
				AutomationRecorder = recorder,
			};

			// The window handle, as when shown (Load is not raised: nothing is positioned or shown).
			_ = tooltip.Handle;
			tooltip.FillToolTip();
		}
		finally
		{
			InvalidateCache(item);
		}

		return recorder.Rows;
	}
}
