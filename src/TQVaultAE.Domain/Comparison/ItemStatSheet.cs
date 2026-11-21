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
		=> Of(kind, key, title, lines.Select(text => (text, StatSource.Base)));

	/// <summary>
	/// Builds a group from displayed lines, each with the source it comes from: as
	/// <see cref="Of(StatGroupKind, string, string?, IEnumerable{string?})"/>, each summed stat keeping what each source
	/// gives in <see cref="StatLine.Sources"/>.
	/// </summary>
	public static StatGroup Of(StatGroupKind kind, string key, string? title, IEnumerable<(string? Text, StatSource Source)> lines)
	{
		var stats = new List<StatLine>();
		var byKey = new Dictionary<string, int>(StringComparer.Ordinal);
		string? parent = null;
		foreach (var (text, source) in lines)
		{
			var parsed = StatLine.Parse(text, parent);
			if (parsed is null)
				continue;

			var line = parsed with { Sources = [new StatPart(source, parsed)] };

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

	/// <summary>Two lines of the same stat added number by number, and source by source.</summary>
	private static StatLine Sum(StatLine a, StatLine b)
		=> a with
		{
			Values = [.. a.Values.Zip(b.Values, (x, y) => x + y)],
			Decimals = [.. a.Decimals.Zip(b.Decimals, Math.Max)],
			DecimalSeparator = a.Decimals.Any(d => d > 0) ? a.DecimalSeparator : b.DecimalSeparator,
			Sources = SumSources(a.Sources, b.Sources),
		};

	/// <summary>The parts of two lines of the same stat: the parts of the same source summed, in source order.</summary>
	private static StatPart[] SumSources(IReadOnlyList<StatPart> a, IReadOnlyList<StatPart> b)
		=> [.. a.Concat(b)
			.GroupBy(p => p.Source)
			.OrderBy(g => g.Key)
			.Select(g => g.Aggregate((x, y) => x with { Line = Sum(x.Line, y.Line) }))];
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
		var attributes = WithoutSkillNames(OwnAttributes(data), data.GrantedSkills);

		// The socketed relics and charms with their completion bonuses; the completion bonus of an artifact, a relic or a charm.
		IEnumerable<(string?, StatSource)> socketed =
		[
			.. Tagged(data.Relic1Attributes, StatSource.Relic1),
			.. Tagged(data.RelicBonus1Attributes, StatSource.RelicBonus1),
			.. Tagged(data.Relic2Attributes, StatSource.Relic2),
			.. Tagged(data.RelicBonus2Attributes, StatSource.RelicBonus2),
		];

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
	private static List<(string? Text, StatSource Source)> WithoutSkillNames(IEnumerable<(string? Text, StatSource Source)> attributes, GrantedSkill[] skills)
	{
		var names = skills.Select(s => StatLine.Parse(s.Name)?.Template).OfType<string>().ToList();
		var kept = new List<(string? Text, StatSource Source)>();
		foreach (var attribute in attributes)
		{
			var line = StatLine.Parse(attribute.Text);
			if (line is not null && names.Exists(n => line.Template.StartsWith(n, StringComparison.Ordinal)))
			{
				// The title above the name, a text without numbers.
				if (kept.Count != 0 && StatLine.Parse(kept[^1].Text) is { Values.Length: 0 })
					kept.RemoveAt(kept.Count - 1);
				continue;
			}

			kept.Add(attribute);
		}

		return kept;
	}

	/// <summary>
	/// The item's own attribute lines with their source: the classic lines (base, prefix and suffix merged in the game's
	/// order) with <see cref="ToFriendlyNameResult.ClassicAttributeSources"/>, else the base, prefix and suffix lines.
	/// </summary>
	private static IEnumerable<(string? Text, StatSource Source)> OwnAttributes(ToFriendlyNameResult data)
	{
		if (data.ClassicAttributes is not { } classic)
			return [.. Tagged(data.BaseAttributes, StatSource.Base), .. Tagged(data.PrefixAttributes, StatSource.Prefix), .. Tagged(data.SuffixAttributes, StatSource.Suffix)];

		// Without a source per line (texts built elsewhere), every line counts as the base item's.
		var sources = data.ClassicAttributeSources;
		return classic.Select((text, i) => ((string?)text, sources.Length == classic.Length ? sources[i] : StatSource.Base));
	}

	private static IEnumerable<(string? Text, StatSource Source)> Tagged(IEnumerable<string> lines, StatSource source)
		=> lines.Select(text => ((string?)text, source));
}
