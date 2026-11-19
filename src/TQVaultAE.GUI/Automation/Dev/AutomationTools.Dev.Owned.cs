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
	internal static partial class DevTools
	{
		[McpServerTool(Name = "raw_tooltip_lines", ReadOnly = true)]
		[Description("Scans every owned item (search database) and reports the tooltip lines still shown raw: an attribute without game text ('?name?'), '<unknown>', or a record file name ('burst_stormorb'). Covers the item attributes and the granted skills (description and effects). Grouped by raw name, with the number of lines, the skills or items where it appears and one sample line.")]
		public static string RawTooltipLines(
			[Description("Also scan the item attributes, not only the granted skills (default false).")] bool items = false,
			[Description("Maximum number of examples per raw name (default 5).")] int examples = 5)
			=> AutomationHost.OnUi(_ =>
			{
				var provider = Services.ItemText;
				var owned = Services.ItemDatabaseService.ItemDatabase;
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
}
