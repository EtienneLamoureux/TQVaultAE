using System.Globalization;
using TQVaultAE.Application.Character;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Presentation;

/// <summary>The Buffs tab (tab IV) and the sources listed in the statistics' rollovers.</summary>
public sealed partial class CharacterPanelViewBuilder
{
	/// <summary>The names of the sources, read once per list of sources (an item's name is costly).</summary>
	private readonly Dictionary<string, string> _names = new(StringComparer.OrdinalIgnoreCase);

	/// <summary>The effect lines of the sources' tool tips, read once per list of sources.</summary>
	private readonly Dictionary<string, IReadOnlyList<string>> _descriptions = new(StringComparer.OrdinalIgnoreCase);

	private IReadOnlyList<BuffScrollChoice>? _scrollChoices;

	/// <summary>True when a source adds nothing: the tab does not list it.</summary>
	private static bool IsEmpty(BuffSource source) => source.Sum.Values.Count == 0 && source.Sum.Races.Count == 0;

	/// <summary>
	/// The statistics with every source turned on, and without each of them in turn: what a rollover lists as a source's
	/// share of a value.
	/// </summary>
	private Composition Compose(CharacterPanelInput input)
	{
		var sources = this.Sources(input);
		var total = this.Evaluate(input, sources);
		var others = sources
			.Where(s => input.Buffs.IsOn(s) && !IsEmpty(s))
			.Select(s => new SourceShare(this.SourceName(s), s, this.Evaluate(input, sources, s)))
			.ToList();
		return new Composition(total, others);
	}

	private sealed record SourceShare(string Name, BuffSource Source, Evaluation Without);

	/// <summary>
	/// The sources of the values: per source turned on, its own values of the variables a statistic reads ("raw"), then
	/// in parentheses what it changes in the value shown, the value with every source minus the value without it.
	/// </summary>
	private sealed class Composition(Evaluation total, IReadOnlyList<SourceShare> shares)
	{
		public Evaluation Total { get; } = total;

		/// <summary>
		/// A rollover with a block listing the sources of a value; a source that has none of <paramref name="variables"/>
		/// shows when it changes the value (a strength bonus changes the damage).
		/// </summary>
		/// <param name="shown">The value as shown, from an evaluation; null for a plain sum, listed without effect.</param>
		/// <param name="penalty">The difficulty penalty the value includes, listed last when not 0.</param>
		/// <param name="title">The block's title; "Sources" by default.</param>
		public StatToolTip Append(StatToolTip tip, Func<Evaluation, double>? shown, IReadOnlyList<string> variables, double penalty = 0, string? title = null)
		{
			var culture = CultureInfo.CurrentCulture;
			IReadOnlyList<string> last = penalty == 0 ? [] : [string.Format(culture, Resources.BuffsDifficultyLine, Signed(penalty, culture))];
			return this.Append(tip, shown, sum => variables.Where(v => sum[v] != 0).Select(v => Raw(v, sum[v], culture)), title, last: last);
		}

		/// <summary>A rollover with a block listing the sources of a value, each source's raw part given by <paramref name="raw"/>.</summary>
		/// <param name="first">Lines put before the sources (a total).</param>
		/// <param name="last">Lines put after them (the difficulty penalty).</param>
		public StatToolTip Append(StatToolTip tip, Func<Evaluation, double>? shown, Func<AttributeSum, IEnumerable<string>> raw, string? title = null,
			IReadOnlyList<string>? first = null, IReadOnlyList<string>? last = null)
		{
			var culture = CultureInfo.CurrentCulture;
			var lines = new List<string>(first ?? []);
			double value = shown?.Invoke(this.Total) ?? 0;
			foreach (var share in shares)
			{
				var parts = raw(share.Source.Sum).ToList();
				if (shown is null)
				{
					if (parts.Count != 0)
						lines.Add(string.Format(culture, Resources.BuffsSourceRawLine, share.Name, string.Join(" ", parts)));
					continue;
				}

				double effect = value - shown(share.Without);
				if (parts.Count == 0 && Math.Abs(effect) < 0.005)
					continue;

				lines.Add(string.Format(culture, Resources.BuffsSourceLine, share.Name, string.Join(" ", parts), Signed(effect, culture))
					.Replace(":  (", ": (", StringComparison.Ordinal));
			}

			lines.AddRange(last ?? []);
			return lines.Count == (first?.Count ?? 0) && (last?.Count ?? 0) == 0 ? tip
				: new StatToolTip([.. tip.Blocks, new StatToolTipBlock(title ?? Resources.BuffsSourcesTitle, lines)]);
		}

		/// <summary>A variable's value, signed, with % for a modifier.</summary>
		public static string Raw(string variable, double value, CultureInfo culture)
			=> Signed(value, culture) + (variable.EndsWith("Modifier", StringComparison.OrdinalIgnoreCase) ? "%" : string.Empty);

		public static string Signed(double value, CultureInfo culture)
			=> (value > 0 ? "+" : string.Empty) + Math.Round(value, 2).ToString("0.##", culture);
	}

	/// <summary>The name of a source as the Buffs tab writes it.</summary>
	public string NameOf(BuffSource source)
	{
		ArgumentNullException.ThrowIfNull(source);
		return this.SourceName(source);
	}

	/// <summary>The name of a source: an item's full name, else its tag translated.</summary>
	private string SourceName(BuffSource source)
	{
		if (this._names.TryGetValue(source.Id, out var name))
			return name;

		if (source.Item is { } item)
			name = this._itemText?.GetFriendlyNames(item).FullNameClean is { Length: > 0 } full ? full : item.BaseItemId?.PrettyFileName ?? source.Id;
		else
			name = (this._translation.TranslateOr(source.NameTag, source.NameTag) ?? source.NameTag).RemoveAllTQTags() ?? source.NameTag;

		this._names[source.Id] = name;
		return name;
	}

	/// <summary>The title of a section of the Buffs tab.</summary>
	private static string GroupTitle(BuffGroup group) => group switch
	{
		BuffGroup.Gear => Resources.BuffsGear,
		BuffGroup.Passive => Resources.BuffsPassive,
		BuffGroup.Active => Resources.BuffsActive,
		BuffGroup.Pet => Resources.BuffsPets,
		BuffGroup.Curse => Resources.BuffsCurses,
		BuffGroup.Shrine => Resources.BuffsShrines,
		BuffGroup.Tartarus => Resources.BuffsTartarus,
		_ => Resources.BuffsScroll,
	};

	/// <summary>What the Buffs tab shows: a section per kind of source the character has, and the scroll choice.</summary>
	private BuffTabView BuffView(CharacterPanelInput input)
	{
		var sources = this.Sources(input);
		var sections = Enum.GetValues<BuffGroup>()
			.Select(group => new BuffSectionView(group, GroupTitle(group), [.. sources
				.Where(s => s.Group == group && !IsEmpty(s))
				.Select(s =>
				{
					var on = input.Buffs.IsOn(s);
					return new BuffEntryView(s.Id, this.SourceName(s), s.Icon, on, this.BuffToolTip(s, on));
				})]))
			.Where(section => section.Entries.Count != 0)
			.ToList();

		this._scrollChoices ??= [new BuffScrollChoice(null, Resources.BuffsNoScroll), .. this._service.GetBuffScrolls()
			.Select(s => new BuffScrollChoice(s.Id, (this._translation.TranslateOr(s.NameTag, s.Id.PrettyFileName) ?? s.Id.PrettyFileName).RemoveAllTQTags() ?? s.Id.PrettyFileName))
			.OrderBy(s => s.Name, StringComparer.CurrentCultureIgnoreCase)];
		var scrolls = this._scrollChoices;
		int selected = input.Buffs.Scroll is { } scroll ? Math.Max(0, scrolls.ToList().FindIndex(s => Equals(s.Id, scroll))) : 0;

		return new BuffTabView
		{
			Sections = sections,
			ScrollTitle = Resources.BuffsScroll,
			Scrolls = scrolls,
			SelectedScroll = selected,
		};
	}

	/// <summary>
	/// A source's tool tip: its name, whether it counts and how to change it, its level, its effects as the game describes
	/// them (an item's attributes, a skill's or buff's effects at its level), and what grants it.
	/// </summary>
	private StatToolTip BuffToolTip(BuffSource source, bool on)
	{
		var culture = CultureInfo.CurrentCulture;
		var head = new List<string> { on ? Resources.BuffsOn : Resources.BuffsOff };
		if (source.Group is BuffGroup.Passive or BuffGroup.Active or BuffGroup.Pet && source.Item is null)
			head.Add(string.Format(culture, Resources.BuffsLevel, source.Level));
		if (source.OnEquip)
			head.Add(Resources.BuffsOnEquip);
		if (source.PotionTrigger is var (chance, duration))
			head.Add(chance >= 100
				? string.Format(culture, Resources.BuffsPotionTrigger, duration.ToString("0.##", culture))
				: string.Format(culture, Resources.BuffsPotionTriggerChance, chance.ToString("0.##", culture), duration.ToString("0.##", culture)));

		List<StatToolTipBlock> blocks = [new StatToolTipBlock(this.SourceName(source), head)];
		if (this.Description(source) is { Count: > 0 } description)
			blocks.Add(new StatToolTipBlock(null, description));

		if (source.Origins.Count != 0)
		{
			var names = source.Origins.Select(i => this._itemText?.GetFriendlyNames(i).FullNameClean ?? i.BaseItemId?.PrettyFileName ?? string.Empty)
				.Where(n => n.Length != 0).Distinct();
			var format = source.Group == BuffGroup.Gear ? Resources.BuffsSetPieces : Resources.BuffsGrantedBy;
			blocks.Add(new StatToolTipBlock(null, [string.Format(culture, format, string.Join(", ", names))]));
		}

		return new StatToolTip(blocks);
	}

	/// <summary>The effects of a source as the game writes them, without their leading colour.</summary>
	private IReadOnlyList<string> Description(BuffSource source)
	{
		if (this._descriptions.TryGetValue(source.Id, out var lines))
			return lines;

		IEnumerable<string> text = [];
		if (source.Item is { } item)
		{
			if (this._itemText?.GetFriendlyNames(item) is { } names)
				text = names.BaseAttributes.Concat(names.PrefixAttributes).Concat(names.SuffixAttributes);
		}
		else if (source.Group != BuffGroup.Gear && source.RecordId is { } id && this._editService?.DescribeSkill(id, Math.Max(1, source.Level)) is { } skill)
		{
			// A skill that casts another (Blessing of Jin Chan on a kill): the skill window's "Level:   2" of the skill cast
			// repeats the source's level, and "100.0% Chance of ..." says nothing; a lower chance stays, it tells how
			// often. A passive whose buff a potion triggers shows that buff as a source of its own: its block goes.
			var (chanceLine, levelLine, lastLine, chance) = skill.CastLines;
			int first = chanceLine >= 0 ? chanceLine : levelLine;
			text = source.Group == BuffGroup.Passive && levelLine >= 0
				? skill.Attributes.Where((_, i) => i < first || i > lastLine)
				: skill.Attributes.Where((_, i) => i != levelLine && (i != chanceLine || chance < 100));
		}

		// Plain text: the rollover writes every line in grey. Only what is not zero: a line whose numbers are all 0 says
		// nothing. A variable the game has no text for gets ours when it tells the player something, else it is left out;
		// the energy reserve, which has none, gets its own line.
		var culture = CultureInfo.CurrentCulture;
		var reserve = source.Sum[EnergyReserve];

		// "+1 to all skills" of a buff (Battle Standard, Stone of Mastery): the game writes it in the skill window, the item
		// texts the description comes from leave it out.
		var allSkills = source.Group == BuffGroup.Gear ? 0 : (long)Math.Round(source.Sum[AllSkills]);
		IEnumerable<string> allSkillsLine = allSkills <= 0 ? [] : [Application.Layout.CharacterPanelLayout.FormatGameText(
			this._translation.TranslateOr("ItemAllSkillIncrement", "+{%d0} to all skills") ?? "+{%d0} to all skills", allSkills)];
		lines = [.. text
			.Select(l => l.RemoveAllTQTags() ?? string.Empty)
			.Where(l => !string.IsNullOrWhiteSpace(l) && !l.Contains(EnergyReserve, StringComparison.OrdinalIgnoreCase))
			.Select(UnknownVariableLine)
			.OfType<string>()
			.Where(HasNonZeroNumberOrNone),
			.. allSkillsLine,
			.. reserve == 0 ? [] : new[] { string.Format(culture, Resources.BuffsEnergyReserve, Composition.Signed(reserve, culture).TrimStart('+')) }];
		this._descriptions[source.Id] = lines;
		return lines;
	}

	/// <summary>The variable the game reserves a part of the energy with (curses, Tartarus).</summary>
	private const string EnergyReserve = "characterManaLimitReservePercentage";

	/// <summary>The variable of the levels a buff adds to every skill.</summary>
	private const string AllSkills = "allSkillEnhancement";

	/// <summary>
	/// A variable the game has no text for ("?potionCooldownIncrease?: 5.000000, 10.000000"): our own text when it tells the
	/// player something, else null (left out: enemyScaleModifier and the like). Any other line is kept as it is.
	/// </summary>
	private static string? UnknownVariableLine(string line)
	{
		var match = UnknownVariableRegex().Match(line);
		if (!match.Success)
			return line;

		var values = match.Groups["values"].Value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
			.Select(v => double.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) ? d : 0).ToList();
		if (values.Count == 0 || values.All(v => v == 0))
			return null;

		var culture = CultureInfo.CurrentCulture;
		string Number(double value) => value.ToString("0.##", culture);
		return match.Groups["name"].Value switch
		{
			// Seconds added to the potions' recharge, drawn between the two values (Tartarus).
			"potionCooldownIncrease" => values.Min() == values.Max()
				? string.Format(culture, Resources.BuffsPotionRecharge, Number(values[0]))
				: string.Format(culture, Resources.BuffsPotionRechargeRange, Number(values.Min()), Number(values.Max())),
			// % added to the duration of skills (a Neidan passive).
			"skillDurationModifier" => string.Format(culture, Resources.BuffsSkillDuration, Composition.Signed(values[0], culture)),
			_ => null,
		};
	}

	[System.Text.RegularExpressions.GeneratedRegex(@"^\?(?<name>[^?]+)\?:\s*(?<values>.*)$")]
	private static partial System.Text.RegularExpressions.Regex UnknownVariableRegex();

	[System.Text.RegularExpressions.GeneratedRegex(@"\d+(?:[.,]\d+)?")]
	private static partial System.Text.RegularExpressions.Regex NumberRegex();

	/// <summary>False for a line whose numbers are all 0 ("+0% Total Damage"); true for one without number.</summary>
	private static bool HasNonZeroNumberOrNone(string line)
	{
		var numbers = NumberRegex().Matches(line);
		return numbers.Count == 0 || numbers.Any(n => n.Value.Any(c => c is >= '1' and <= '9'));
	}
}
