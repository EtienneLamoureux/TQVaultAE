using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;

namespace TQVaultAE.GUI.Automation;

/// <summary>
/// MCP tools that read the game files the way the app does: resources (textures, sounds) and text tags.
/// </summary>
internal static partial class AutomationTools
{
	[McpServerTool(Name = "load_resource", ReadOnly = true)]
	[Description("Loads resources through the database as the app does (custom map first, then the game) and tells, for each id, the game archive it maps to, whether that archive exists and whether the data loaded, with its size. Ids are as the database references them, like 'Sounds\\XPack\\Monster\\x.wav' or 'Items\\EquipmentRing\\UIBitmaps\\x.tex'. Also gives the custom map in use.")]
	public static string LoadResource([Description("Resource ids, separated by commas or new lines.")] string resourceIds)
	{
		var database = Program.ServiceProvider.GetRequiredService<IDatabase>();
		var gamePath = Program.ServiceProvider.GetRequiredService<IGamePathService>();
		var ids = resourceIds.Split([',', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		return Serialize(new
		{
			mapName = gamePath.IsCustom ? gamePath.MapName : null,
			resources = ids.Select(id =>
			{
				var (archive, isDLC) = gamePath.ResolveArcFileName(id);
				var data = database.LoadResource(id);
				return new
				{
					id,
					gameArchive = archive,
					gameArchiveExists = File.Exists(archive),
					isDLC,
					loaded = data is not null,
					bytes = data?.Length,
				};
			}),
		});
	}

	[McpServerTool(Name = "picture_fault")]
	[Description("Makes a resource read as missing or damaged from now on, without touching any file, to check how an item whose record is found but whose picture cannot be loaded is shown. 'missing': the resource reads as absent (no data, no texture header: the item is 1x1). 'corrupt': only the first 64 bytes of the real data (the texture header reads, so the item keeps its real size, but the picture cannot be decoded). 'clear': the real data again. The pictures already decoded are forgotten; items already loaded keep the size read at their load: create one with create_item (or reload its file) to see the size rule. Gives the faults in place.")]
	public static string PictureFault(
		[Description("Resource id as an item record references it, like 'XPack2\\items\\equipmentweapon\\bow\\uibitmaps\\mi_skeletonbow.tex'.")] string resourceId,
		[Description("'missing', 'corrupt' or 'clear'.")] string mode)
	{
		var database = Program.ServiceProvider.GetRequiredService<IDatabase>();
		var faults = TQVaultAE.Data.Database.ResourceFaults;
		faults.TryRemove(resourceId, out _);
		switch (mode)
		{
			case "missing":
				faults[resourceId] = null;
				break;
			case "corrupt":
				var data = database.LoadResource(resourceId) ?? throw new McpException($"No resource '{resourceId}' to damage.");
				faults[resourceId] = data[..Math.Min(64, data.Length)];
				break;
			case "clear":
				break;
			default:
				throw new McpException("mode must be 'missing', 'corrupt' or 'clear'.");
		}

		(Program.ServiceProvider.GetRequiredService<TQVaultAE.Presentation.Win32.Contracts.IUIService>() as IGameDataCache)?.ClearGameDataCache();
		return Serialize(faults.Select(f => new { id = f.Key.Raw, bytes = f.Value?.Length }));
	}

	[McpServerTool(Name = "game_text", ReadOnly = true)]
	[Description("Translates text tags with the loaded text database (game, expansions, then the custom map's strings): an empty text means the tag is unknown.")]
	public static string GameText([Description("Text tags, separated by commas or new lines, like 'tagSkillName01'.")] string tags)
	{
		var database = Program.ServiceProvider.GetRequiredService<IDatabase>();
		return Serialize(tags.Split([',', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
			.Select(tag => new { tag, text = database.GetFriendlyName(tag) }));
	}
}
