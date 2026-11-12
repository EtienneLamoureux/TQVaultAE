using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Results;

namespace TQVaultAE.Domain.Comparison;

/// <summary>What a group of stats is about.</summary>
public enum StatGroupKind
{
	/// <summary>The item's attributes, its socketed relics or charms and their completion bonuses, summed.</summary>
	Attributes,
	/// <summary>The effects of a skill the item grants.</summary>
	GrantedSkill,
	/// <summary>The requirements to equip the item: lower is better.</summary>
	Requirements,
}

/// <summary>
/// The stats of an item in one group, each stat once: lines of the same stat are summed (an attribute of the item and the
/// same attribute of its relic).
/// </summary>
/// <param name="Kind">What the group is about.</param>
/// <param name="Key">Identifies the group between two items: the kind, and the skill record for a granted skill.</param>
/// <param name="Title">The title shown above the group (the skill name), or null.</param>
/// <param name="Stats">The stats in display order.</param>
public sealed record StatGroup(StatGroupKind Kind, string Key, string? Title, IReadOnlyList<StatLine> Stats)
{
	/// <summary>Lower values are better in this group.</summary>
	public bool LowerIsBetter => this.Kind == StatGroupKind.Requirements;

	/// <summary>
	/// Builds a group from displayed lines: parses them, gives each indented line the stat of the line above, and sums
	/// the lines of the same stat into the first one.
	/// </summary>
	public static StatGroup Of(StatGroupKind kind, string key, string? title, IEnumerable<string?> lines)
	{
		var stats = new List<StatLine>();
		var byKey = new Dictionary<string, int>(StringComparer.Ordinal);
		string? parent = null;
		foreach (var text in lines)
		{
			var line = StatLine.Parse(text, parent);
			if (line is null)
				continue;

			if (!line.Indented)
				parent = line.Template;

			// A line without a number (lore, a title, "Attack Speed: Fast") is not compared; it still heads the indented
			// lines under it.
			if (line.Values.Length == 0)
				continue;

			if (byKey.TryGetValue(line.Key, out var index))
				stats[index] = Sum(stats[index], line);
			else
			{
				byKey.Add(line.Key, stats.Count);
				stats.Add(line);
			}
		}

		return new StatGroup(kind, key, title, stats);
	}

	/// <summary>Two lines of the same stat added number by number.</summary>
	private static StatLine Sum(StatLine a, StatLine b)
		=> a with
		{
			Values = [.. a.Values.Zip(b.Values, (x, y) => x + y)],
			Decimals = [.. a.Decimals.Zip(b.Decimals, Math.Max)],
			DecimalSeparator = a.Decimals.Any(d => d > 0) ? a.DecimalSeparator : b.DecimalSeparator,
		};
}

/// <summary>
/// The comparable stats of an item, read from the lines the tooltip displays (so the values follow the
/// "Show In-Game Values" setting): attributes with relics and bonuses, each granted skill, the requirements.
/// </summary>
/// <param name="Groups">The groups in display order; empty groups are left out.</param>
public sealed record ItemStatSheet(IReadOnlyList<StatGroup> Groups)
{
	/// <summary>The group key of the attributes.</summary>
	public const string AttributesKey = "attributes";

	/// <summary>The group key of the requirements.</summary>
	public const string RequirementsKey = "requirements";

	/// <summary>
	/// Reads the stats of an item from its texts, built with the granted skills apart
	/// (<see cref="FriendlyNamesExtraScopes.GrantedSkills"/>) so that their effects do not mix with the item attributes.
	/// </summary>
	public static ItemStatSheet From(ToFriendlyNameResult data)
	{
		var attributes = WithoutSkillNames(
			data.ClassicAttributes ?? [.. data.BaseAttributes, .. data.PrefixAttributes, .. data.SuffixAttributes]
			, data.GrantedSkills);

		// The socketed relics and charms with their completion bonuses; the completion bonus of an artifact, a relic or a charm.
		IEnumerable<string> socketed = [.. data.Relic1Attributes, .. data.RelicBonus1Attributes, .. data.Relic2Attributes, .. data.RelicBonus2Attributes];

		var groups = new List<StatGroup>
		{
			StatGroup.Of(StatGroupKind.Attributes, AttributesKey, null, attributes.Concat(socketed)),
		};

		// Level, activation ("(Activated on attack) 4% Chance of Activating": the chance compares like a stat), effects.
		foreach (var skill in data.GrantedSkills)
		{
			groups.Add(StatGroup.Of(StatGroupKind.GrantedSkill, $"skill:{RecordId.RawOf(skill.SkillId)}", skill.Name
				, [skill.LevelText, .. skill.Activation is null ? Array.Empty<string>() : [skill.Activation], .. skill.Attributes]));
		}

		groups.Add(StatGroup.Of(StatGroupKind.Requirements, RequirementsKey, null, data.Requirements));

		return new ItemStatSheet([.. groups.Where(g => g.Stats.Count != 0)]);
	}

	/// <summary>
	/// The attributes without the lines naming a granted skill ("Grants Skill :" then the skill name): the skill has a
	/// group of its own.
	/// </summary>
	private static List<string> WithoutSkillNames(IEnumerable<string> attributes, GrantedSkill[] skills)
	{
		var names = skills.Select(s => StatLine.Parse(s.Name)?.Template).OfType<string>().ToList();
		var kept = new List<string>();
		foreach (var text in attributes)
		{
			var line = StatLine.Parse(text);
			if (line is not null && names.Exists(n => line.Template.StartsWith(n, StringComparison.Ordinal)))
			{
				// The title above the name, a text without numbers.
				if (kept.Count != 0 && StatLine.Parse(kept[^1]) is { Values.Length: 0 })
					kept.RemoveAt(kept.Count - 1);
				continue;
			}

			kept.Add(text);
		}

		return kept;
	}
}
