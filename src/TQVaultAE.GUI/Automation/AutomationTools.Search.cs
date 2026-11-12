using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.GUI.Automation;

/// <summary>
/// Tool that reads the search database.
/// </summary>
internal static partial class AutomationTools
{
	[McpServerTool(Name = "search_entries", ReadOnly = true)]
	[Description("Lists the entries of the search database (what the search finds) for one base item record id: the number of entries and, for each, its place (container name and file, sack type and number, stash type), cell and stack size, the item's map (mod, modOrigin), the name the search shows (read as the search reads it, whatever character is selected) and the records that name could not resolve. Use it to check that a moved or reloaded item has exactly one entry, at the right place, and that its names come from its own map.")]
	public static string SearchEntries(
		[Description("Base item record id, as get_state reports it (case does not matter).")] string baseItemId)
		=> AutomationHost.OnUi(_ =>
		{
			var wanted = RecordId.Create(baseItemId);
			var entries = Program.ServiceProvider.GetRequiredService<IItemDatabaseService>().ItemDatabase
				.Where(r => r.Item.BaseItemId == wanted)
				.Select(r => (object)new
				{
					containerName = r.ContainerName,
					container = r.Container,
					sackType = r.SackType.ToString(),
					sackNumber = r.SackNumber,
					stashType = r.StashType?.ToString(),
					x = r.Item.PositionX,
					y = r.Item.PositionY,
					stackSize = r.Item.StackSize,
					mod = r.Item.Mod,
					modOrigin = r.Item.ModOrigin.ToString(),
					name = SearchName(r),
					missingRecords = r.Item.IsIncomplete ? r.Item.MissingRecords.Select(m => m.Id.Raw) : null,
				})
				.ToList();

			return Serialize(new { count = entries.Count, entries });
		});

	[McpServerTool(Name = "search_containers", ReadOnly = true)]
	[Description("Lists the containers of the search database (files and the names the search shows for them) with their number of entries and the base records of their items, optionally only those whose file or name contains a text. Use it to check that a file read again (hot reload, a shared stash of another map) replaced its items in the search.")]
	public static string SearchContainers(
		[Description("Optional text the container's file or name must contain (case does not matter).")] string? filter = null)
		=> AutomationHost.OnUi(_ =>
		{
			var containers = Program.ServiceProvider.GetRequiredService<IItemDatabaseService>().ItemDatabase
				.Where(r => string.IsNullOrEmpty(filter)
					|| r.Container.Contains(filter, StringComparison.OrdinalIgnoreCase)
					|| r.ContainerName.Contains(filter, StringComparison.OrdinalIgnoreCase))
				.GroupBy(r => (r.Container, r.ContainerName))
				.Select(g => (object)new
				{
					container = g.Key.Container,
					containerName = g.Key.ContainerName,
					count = g.Count(),
					items = g.Select(r => r.Item.BaseItemId.Raw).Order(StringComparer.OrdinalIgnoreCase).ToList(),
				})
				.ToList();

			return Serialize(new { count = containers.Count, containers });
		});

	[McpServerTool(Name = "select_search_result", Destructive = true)]
	[Description("Shows a search result like a click on its row in the results dialog, through the main window's own handler: the first entry of the search database whose container file or name contains 'container' (and whose item is at cell x, y when given). The handler selects the vault or the character (which loads it, as the results dialog does), the bag, and the item. Returns the entry, then the character and vault shown, the notification, the active map, and per panel the bag shown and the items selected (base record and cell).")]
	public static string SelectSearchResult(
		[Description("Text the container's file or name must contain (case does not matter), e.g. 'Diablo 2 Lilith' or 'winsys.dxb'.")] string container,
		[Description("Optional cell column of the item.")] int? x = null,
		[Description("Optional cell row of the item.")] int? y = null)
	{
		var picked = AutomationHost.OnUi(form =>
		{
			var entry = Program.ServiceProvider.GetRequiredService<IItemDatabaseService>().ItemDatabase
				.FirstOrDefault(r => (r.Container.Contains(container, StringComparison.OrdinalIgnoreCase)
						|| r.ContainerName.Contains(container, StringComparison.OrdinalIgnoreCase))
					&& (x is null || r.Item.PositionX == x) && (y is null || r.Item.PositionY == y))
				?? throw new McpException($"No search entry in a container matching '{container}'.");
			form.AutomationSelectResult(entry);
			return new
			{
				containerName = entry.ContainerName,
				container = entry.Container,
				item = entry.Item.BaseItemId.Raw,
				x = entry.Item.PositionX,
				y = entry.Item.PositionY,
				stashType = entry.StashType?.ToString(),
			};
		});

		return AutomationHost.OnUi(form => Serialize(new
		{
			result = picked,
			character = form.AutomationSelectedSave?.Name,
			vault = form.AutomationSelectedVault,
			notification = form.AutomationNotificationText,
			activeMap = form.AutomationGameMapService.ActiveMap,
			panels = AutomationPanels.List(form).Select(p => new
			{
				id = p.Id,
				bag = p.Container?.ShownBag(),
				selected = p.Panel.AutomationSelectedItems.Select(i => new { item = i.BaseItemId.Raw, x = i.PositionX, y = i.PositionY }),
			}),
		}));
	}

	/// <summary>The name the search shows for an entry, loaded as the search loads it.</summary>
	private static string SearchName(TQVaultAE.Application.Results.SearchResult entry)
	{
		entry.LazyLoad();
		return entry.ItemName;
	}
}
