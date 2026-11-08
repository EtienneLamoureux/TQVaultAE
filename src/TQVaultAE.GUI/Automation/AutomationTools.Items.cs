using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.GUI.Automation;

internal static partial class AutomationTools
{
	[McpServerTool(Name = "create_item", Destructive = true)]
	[Description("Creates an item in memory from database records (base item, affixes, relics) and drags it over a panel, like the Duplicate menu entry: place it with drop_item. It is saved with its vault or character like any item: use claude-* test files. Useful to test combinations no vault holds, e.g. an affix that grants a skill.")]
	public static string CreateItem(
		[Description("Panel the drag starts from, e.g. vault.")] string panel,
		[Description("""Base item record, e.g. records\item\equipmentamulet\u_n_moonclaw.dbr.""")] string baseItem,
		[Description("Prefix record (LootRandomizer), or empty.")] string? prefix = null,
		[Description("Suffix record (LootRandomizer), or empty.")] string? suffix = null,
		[Description("Relic or charm socketed in the first slot, or empty.")] string? relic = null,
		[Description("Completion bonus of the first relic, or empty.")] string? relicBonus = null,
		[Description("Relic or charm socketed in the second slot, or empty.")] string? relic2 = null,
		[Description("Completion bonus of the second relic, or empty.")] string? relicBonus2 = null,
		[Description("Item seed; 0 picks a random one.")] int seed = 0)
		=> AutomationHost.OnUi(form =>
		{
			var named = ResolveShownPanel(form, panel);
			var drag = form.AutomationDragInfo;
			if (drag.IsActive)
				throw new McpException("An item is already being dragged. Use drop_item or cancel_drag first.");

			var provider = Program.ServiceProvider.GetRequiredService<IItemProvider>();
			var sack = named.Panel.Sack!;

			// Copy the save blocks of an existing item, as the app does for the items it creates.
			var item = sack.FirstOrDefault()?.MakeEmptyItem() ?? new Item { Seed = Item.GenerateSeed() };
			item.BaseItemId = baseItem;
			item.prefixID = prefix ?? string.Empty;
			item.suffixID = suffix ?? string.Empty;
			item.relicID = relic ?? string.Empty;
			item.RelicBonusId = relicBonus ?? string.Empty;
			item.Var1 = RecordId.IsNullOrEmpty(item.relicID) ? 0 : 1;
			item.relic2ID = relic2 ?? string.Empty;
			item.RelicBonus2Id = relicBonus2 ?? string.Empty;
			if (!RecordId.IsNullOrEmpty(item.relic2ID))
				item.Var2 = 1;
			if (seed != 0)
				item.Seed = seed;

			provider.GetDBData(item);
			if (item.baseItemInfo is null)
				throw new McpException($"No item record '{baseItem}' in the database.");

			string Missing(string? id, object? info, string what) => !string.IsNullOrEmpty(id) && info is null ? $"{what} '{id}' " : string.Empty;
			var missing = Missing(prefix, item.prefixInfo, "prefix") + Missing(suffix, item.suffixInfo, "suffix") + Missing(relic, item.RelicInfo, "relic") + Missing(relic2, item.Relic2Info, "relic2");
			if (missing.Length != 0)
				throw new McpException($"Unknown records: {missing.Trim()}.");

			// The item size in cells comes from its bitmap.
			Program.ServiceProvider.GetRequiredService<IUIService>().GetBitmap(item);


			// A modified drag has no source cell: like Duplicate, it can only be dropped.
			drag.Set(named.Panel, sack, item, new Point(1, 1));
			drag.MarkModified(item);

			return DragStatus(form);
		});
}
