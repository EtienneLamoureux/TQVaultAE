using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;
using static TQVaultAE.Automation.ControlTree;

namespace TQVaultAE.GUI.Automation;

internal static partial class AutomationTools
{
	internal static partial class DevTools
	{
		[McpServerTool(Name = "search_dialog_state", ReadOnly = true)]
		[Description("Reads the user's state of the open Advanced Search dialog from its controls: zoom and size, search text (typed and applied), category filter, saved query box, operator, reduce, values per category, quick filters, requirements, selected filters, category order and hidden categories, per category list its checked entries, collapsed state, focus entry and first column, the categories area scroll, and the results (count, a hash of the sorted results, the first ones). Use it to check that the dialog made again after a zoom change keeps the state.")]
		public static string SearchDialogState()
			=> AutomationHost.OnUi(_ => Serialize(OpenSearchDialog().AutomationState()));

		[McpServerTool(Name = "search_tooltip", Destructive = true)]
		[Description("Shows a tool tip of the open Advanced Search dialog as resting the pointer on its label does, through the label's own handler: 'filters' for the selected filters (label scalingLabelFiltersSelected; select some first with check_list_items), 'results' for the results found (label scalingLabelProgress), 'none' hides them. Capture it with capture('tooltip').")]
		public static string SearchTooltip([Description("filters, results or none.")] string kind)
			=> AutomationHost.OnUi(_ => Serialize(new { kind, shown = OpenSearchDialog().AutomationShowTooltip(kind) }));

		[McpServerTool(Name = "search_dialog_scroll", Destructive = true)]
		[Description("Scrolls the open Advanced Search dialog like its scroll bars: the categories area to x, y (pixels from the top left), and/or a category list to its first shown column. Returns the dialog state as search_dialog_state does.")]
		public static string SearchDialogScroll(
			[Description("Horizontal scroll of the categories area, in pixels; omit to keep it.")] int? x = null,
			[Description("Vertical scroll of the categories area, in pixels; omit to keep it.")] int? y = null,
			[Description("Category list name (as list_items lists them) whose columns to scroll.")] string? list = null,
			[Description("First column the list shows.")] int firstColumn = 0)
			=> AutomationHost.OnUi(_ =>
			{
				var dialog = OpenSearchDialog();
				var area = AllControls(dialog).OfType<FlowLayoutPanel>().First(c => c.Name == "flowLayoutPanelMainAutoScroll");
				if (x is not null || y is not null)
				{
					var current = area.AutoScrollPosition;
					area.AutoScrollPosition = new Point(x ?? -current.X, y ?? -current.Y);
				}
				if (!string.IsNullOrEmpty(list))
				{
					FindVirtualList(dialog, list).FirstColumn = firstColumn;
				}
				return Serialize(dialog.AutomationState());
			});
	}
}
