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

	[McpServerTool(Name = "raw_tooltip_lines", ReadOnly = true)]
	[Description("Scans every owned item (search database) and reports the tooltip lines still shown raw: an attribute without game text ('?name?'), '<unknown>', or a record file name ('burst_stormorb'). Covers the item attributes and the granted skills (description and effects). Grouped by raw name, with the number of lines, the skills or items where it appears and one sample line.")]
	public static string RawTooltipLines(
		[Description("Also scan the item attributes, not only the granted skills (default false).")] bool items = false,
		[Description("Maximum number of examples per raw name (default 5).")] int examples = 5)
		=> AutomationHost.OnUi(_ =>
		{
			var services = Program.ServiceProvider;
			var provider = services.GetRequiredService<IItemTextFormatter>();
			var owned = services.GetRequiredService<IItemDatabaseService>().ItemDatabase;
			var raw = new System.Text.RegularExpressions.Regex(@"\?[A-Za-z][\w ]*\?|<unknown>|\b[a-z0-9]+_[a-z0-9_]+\b");
			var groups = new Dictionary<string, (int Count, HashSet<string> Where, string Sample)>(StringComparer.OrdinalIgnoreCase);

			void Scan(string line, string where)
			{
				var text = line.RemoveAllTQTags() ?? line;
				foreach (System.Text.RegularExpressions.Match m in raw.Matches(text))
				{
					var g = groups.TryGetValue(m.Value, out var v) ? v : (0, [], text.Trim());
					if (g.Item2.Count < examples)
						g.Item2.Add(where);
					groups[m.Value] = (g.Item1 + 1, g.Item2, g.Item3);
				}
			}

			int scanned = 0;
			foreach (var result in owned)
			{
				var names = provider.GetFriendlyNames(result.Item, FriendlyNamesExtraScopes.GrantedSkills);
				scanned++;
				foreach (var skill in names.GrantedSkills)
				{
					// A name without translation falls back to the record file name; a real name can be the same word
					// ("Thunderclap"), so only the ones that look like a file name count: underscores or all lowercase.
					if (skill.Name.Equals(System.IO.Path.GetFileNameWithoutExtension(skill.SkillId.Raw), StringComparison.OrdinalIgnoreCase)
						&& (skill.Name.Contains('_') || !skill.Name.Any(char.IsUpper)))
						Scan($"?file name as skill name {skill.Name}?", $"skill {skill.SkillId.Raw}");

					foreach (var line in skill.Description.Concat(skill.Attributes))
						Scan(line, $"skill {skill.Name} ({skill.SkillId.Raw})");
				}

				if (items)
					foreach (var line in names.AttributesAll)
						Scan(line, $"item {names.FullNameClean} ({RecordId.RawOf(result.Item.BaseItemId)})");
			}

			return Serialize(new
			{
				scanned,
				raw = groups.OrderByDescending(g => g.Value.Count).Select(g => new { name = g.Key, lines = g.Value.Count, where = g.Value.Where, sample = g.Value.Sample }),
			});
		});
}
