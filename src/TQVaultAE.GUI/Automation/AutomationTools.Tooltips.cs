using System.ComponentModel;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Tooltips;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Automation;

internal static partial class AutomationTools
{
	/// <summary>The displays <c>tooltip_text_dump</c> can build, by name.</summary>
	private static readonly string[] TooltipDumpViews = ["bare", "classic", "detailed", "full", "settings"];

	[McpServerTool(Name = "tooltip_text_dump", ReadOnly = true)]
	[Description("Builds the item tooltip of every owned item (search database, as raw_tooltip_lines), or of the item on one panel cell, with the tooltip's own code (bypassing its image cache, nothing shown), and returns its rows as rendered, in order: kind (text, spacer, delimiter, range, icon, separator, column), text, color (#RRGGBB), bold, the colored segments of a multi-color line and the value on the right of a range row. Granted skills and the relic column come after a 'column' row. Items are sorted by location, so two dumps diff line by line. With 'path', the full dump is written to that JSON file and the result is a summary with its SHA-256.")]
	public static string TooltipTextDump(
		[Description("JSON file the full dump is written to; empty to return the dump in the result (fine for one item).")] string? path = null,
		[Description("Displays to build, comma separated: bare (the classic view without any extra), classic (the classic view with the extras checked in the settings), detailed (the detailed view with the extras checked in the settings), full (detailed with every extra: in-game rolls, value ranges, granted skills, set piece counts), settings (what a hover shows now). Default classic,full.")] string views = "classic,full",
		[Description("Only the item on this panel cell (with x and y), e.g. vault; empty for every owned item.")] string? panel = null,
		[Description("Cell column, with panel.")] int x = 0,
		[Description("Cell row, with panel.")] int y = 0,
		[Description("Owned items to skip, in the sorted order (to dump in batches).")] int skip = 0,
		[Description("Owned items to dump after skip; 0 for all.")] int take = 0)
		=> AutomationHost.OnUi(form =>
		{
			var services = form.Services;
			var settings = form.AutomationSettings;

			var names = views.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
				.Select(v => v.ToLowerInvariant()).Distinct().ToList();
			var unknown = names.Except(TooltipDumpViews).ToList();
			if (names.Count == 0 || unknown.Count != 0)
				throw new McpException($"Unknown view '{string.Join(",", unknown)}'. Use {string.Join(", ", TooltipDumpViews)}.");

			var viewOf = names.ToDictionary(n => n, n => n switch
			{
				"bare" => ItemTooltipView.Classic,
				"classic" => ItemTooltip.ViewOf(settings, false),
				"detailed" => ItemTooltip.ViewOf(settings, true),
				"full" => ItemTooltipView.Detailed | ItemTooltipView.InGameRolls | ItemTooltipView.ValueRanges
					| ItemTooltipView.GrantedSkills | ItemTooltipView.SetPieceCounts,
				_ => ItemTooltip.ViewOf(form.Services),
			});

			List<(Item Item, object Location)> targets;
			int total;
			if (!string.IsNullOrWhiteSpace(panel))
			{
				var named = ResolveShownPanel(form, panel);
				var item = named.Panel.AutomationFindItem(new Point(x, y))
					?? throw new McpException($"No item at ({x}, {y}) on panel '{named.Id}'.");
				targets = [(item, new { panel = named.Id, x = item.PositionX, y = item.PositionY })];
				total = 1;
			}
			else
			{
				var owned = services.ItemDatabaseService.ItemDatabase
					.OrderBy(r => r.Container, StringComparer.Ordinal)
					.ThenBy(r => r.SackType)
					.ThenBy(r => r.SackNumber)
					.ThenBy(r => r.Item.PositionY)
					.ThenBy(r => r.Item.PositionX)
					.ThenBy(r => RecordId.RawOf(r.Item.BaseItemId), StringComparer.Ordinal)
					.ThenBy(r => r.Item.Seed)
					.ToList();
				total = owned.Count;
				var batch = owned.Skip(Math.Max(skip, 0));
				if (take > 0)
					batch = batch.Take(take);
				targets = batch.Select(r => (r.Item, (object)new
				{
					container = r.ContainerName,
					sackType = r.SackType.ToString(),
					sack = r.SackNumber,
					x = r.Item.PositionX,
					y = r.Item.PositionY,
				})).ToList();
			}

			var items = targets.Select(t => new
			{
				location = t.Location,
				baseName = RecordId.RawOf(t.Item.BaseItemId),
				prefixName = RecordId.RawOf(t.Item.prefixID),
				suffixName = RecordId.RawOf(t.Item.suffixID),
				relicName = RecordId.RawOf(t.Item.relicID),
				relicBonus = RecordId.RawOf(t.Item.RelicBonusId),
				var1 = t.Item.Var1,
				relicName2 = RecordId.RawOf(t.Item.relic2ID),
				relicBonus2 = RecordId.RawOf(t.Item.RelicBonus2Id),
				var2 = t.Item.Var2,
				seed = t.Item.Seed,
				stackSize = t.Item.StackSize,
				views = names.ToDictionary(n => n, n => ItemTooltip.AutomationDumpRows(services, t.Item, viewOf[n], settings.ShowInGameValues)),
			}).ToList();

			var dump = new
			{
				views = names.ToDictionary(n => n, n => viewOf[n].ToString()),
				total,
				skip = string.IsNullOrWhiteSpace(panel) ? Math.Max(skip, 0) : 0,
				count = items.Count,
				items,
			};

			if (string.IsNullOrWhiteSpace(path))
				return Serialize(dump);

			var json = JsonSerializer.SerializeToUtf8Bytes(dump, JsonOptions);
			var full = Path.GetFullPath(path);
			File.WriteAllBytes(full, json);
			return Serialize(new
			{
				path = full,
				dump.views,
				total,
				dump.skip,
				count = items.Count,
				rows = items.Sum(i => i.views.Values.Sum(r => r.Count)),
				unknownRows = items.Sum(i => i.views.Values.Sum(r => r.Count(row => row.Kind.StartsWith("unknown", StringComparison.Ordinal)))),
				bytes = json.Length,
				sha256 = Convert.ToHexString(SHA256.HashData(json)),
			});
		});
}
