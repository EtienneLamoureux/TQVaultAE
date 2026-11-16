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
		/// <param name="shown">The value as shown, from an evaluation.</param>
		/// <param name="penalty">The difficulty penalty the value includes, listed last when not 0.</param>
		public StatToolTip Append(StatToolTip tip, Func<Evaluation, double> shown, IReadOnlyList<string> variables, double penalty = 0)
		{
			var culture = CultureInfo.CurrentCulture;
			var lines = new List<string>();
			double value = shown(this.Total);
			foreach (var share in shares)
			{
				var raw = variables.Where(v => share.Source.Sum[v] != 0).Select(v => Raw(v, share.Source.Sum[v], culture)).ToList();
				double effect = value - shown(share.Without);
				if (raw.Count == 0 && Math.Abs(effect) < 0.005)
					continue;

				lines.Add(string.Format(culture, Resources.BuffsSourceLine, share.Name, string.Join(" ", raw), Signed(effect, culture)).Replace(":  (", ": (", StringComparison.Ordinal));
			}

			if (penalty != 0)
				lines.Add(string.Format(culture, Resources.BuffsDifficultyLine, Signed(penalty, culture)));

			return lines.Count == 0 ? tip : new StatToolTip([.. tip.Blocks, new StatToolTipBlock(Resources.BuffsSourcesTitle, lines)]);
		}

		/// <summary>A variable's value, signed, with % for a modifier.</summary>
		private static string Raw(string variable, double value, CultureInfo culture)
			=> Signed(value, culture) + (variable.EndsWith("Modifier", StringComparison.OrdinalIgnoreCase) ? "%" : string.Empty);

		private static string Signed(double value, CultureInfo culture)
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
			text = skill.Attributes;

		// Plain text: the rollover writes every line in grey. A variable the game has no text for ("?name?: 1.0") is left out.
		lines = [.. text.Select(l => l.RemoveAllTQTags() ?? string.Empty).Where(l => !string.IsNullOrWhiteSpace(l) && !l.StartsWith('?'))];
		this._descriptions[source.Id] = lines;
		return lines;
	}
}
