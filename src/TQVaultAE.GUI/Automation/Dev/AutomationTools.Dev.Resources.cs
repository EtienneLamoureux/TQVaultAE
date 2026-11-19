using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;

namespace TQVaultAE.GUI.Automation;

internal static partial class AutomationTools
{
	internal static partial class DevTools
	{
		[McpServerTool(Name = "load_resource", ReadOnly = true)]
		[Description("Loads resources through the database as the app does (custom map first, then the game) and tells, for each id, the game archive it maps to, whether that archive exists and whether the data loaded, with its size. Ids are as the database references them, like 'Sounds\\XPack\\Monster\\x.wav' or 'Items\\EquipmentRing\\UIBitmaps\\x.tex'. Also gives the custom map in use.")]
		public static string LoadResource([Description("Resource ids, separated by commas or new lines.")] string resourceIds)
		{
			var database = Service<IDatabase>();
			var gamePath = Services.GamePathService;
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
			var database = Service<IDatabase>();
			var faults = TQVaultAE.Data.Database.ResourceFaults;
			faults.TryRemove(resourceId, out _);
			switch (mode.Trim().ToLowerInvariant())
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

			(Services.UIService as IGameDataCache)?.ClearGameDataCache();
			return Serialize(faults.Select(f => new { id = f.Key.Raw, bytes = f.Value?.Length }));
		}
	}
}
