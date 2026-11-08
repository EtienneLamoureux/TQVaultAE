using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
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
	[Description("Lists the entries of the search database (what the search finds) for one base item record id: the number of entries and, for each, its place (container name and file, sack type and number, stash type), cell and stack size. Use it to check that a moved or reloaded item has exactly one entry, at the right place.")]
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
				})
				.ToList();

			return Serialize(new { count = entries.Count, entries });
		});
}
