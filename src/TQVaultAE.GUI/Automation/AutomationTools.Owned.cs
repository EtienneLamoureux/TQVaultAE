using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.GUI.Automation;

internal static partial class AutomationTools
{
	[McpServerTool(Name = "owned_skill_items", ReadOnly = true)]
	[Description("Lists the owned items that grant a skill (by their base, affixes, relics or relic bonuses), from the search database: vaults, characters and their stashes when 'Load all files' is on. Each entry has the record ids, seed, vars, stack size, size in cells, location and granted skills (record id and level), enough to write a vault file.")]
	public static string OwnedSkillItems()
		=> AutomationHost.OnUi(_ =>
		{
			var services = Program.ServiceProvider;
			var database = services.GetRequiredService<IDatabase>();
			var uiService = services.GetRequiredService<IUIService>();
			var owned = services.GetRequiredService<IItemDatabaseService>().ItemDatabase;

			var entries = new List<object>();
			foreach (var result in owned)
			{
				var item = result.Item;
				var skills = new List<object>();
				RecordId[] sources = [item.BaseItemId, item.prefixID, item.suffixID, item.relicID, item.RelicBonusId, item.relic2ID, item.RelicBonus2Id];
				foreach (var source in sources.Where(s => !RecordId.IsNullOrEmpty(s)).Distinct())
				{
					if (database.GetRecordFromFile(source) is not { } record)
						continue;

					var skill = record.GetString("itemSkillName", 0);
					if (!string.IsNullOrEmpty(skill))
						skills.Add(new { id = skill, level = Math.Max(record.GetInt32("itemSkillLevel", 0), 1), source = source.Raw });
				}

				if (skills.Count == 0)
					continue;

				// The size in cells comes from the item bitmap, at the current zoom.
				uiService.GetBitmap(item);
				entries.Add(new
				{
					baseName = RecordId.RawOf(item.BaseItemId),
					prefixName = RecordId.RawOf(item.prefixID),
					suffixName = RecordId.RawOf(item.suffixID),
					relicName = RecordId.RawOf(item.relicID),
					relicBonus = RecordId.RawOf(item.RelicBonusId),
					var1 = item.Var1,
					relicName2 = RecordId.RawOf(item.relic2ID),
					relicBonus2 = RecordId.RawOf(item.RelicBonus2Id),
					var2 = item.Var2,
					stackSize = item.StackSize,
					seed = item.Seed,
					width = item.Width,
					height = item.Height,
					modified = item.IsModified,
					location = new { result.ContainerName, sackType = result.SackType.ToString(), result.SackNumber },
					skills,
				});
			}

			return Serialize(new { count = entries.Count, scanned = owned.Count, items = entries });
		});

}
