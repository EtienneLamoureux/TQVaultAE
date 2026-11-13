using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Config;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Domain.Results;

namespace TQVaultAE.Presentation;

/// <summary>
/// What an item tooltip shows ("Tooltip Enhanced" settings): the classic or the detailed display, plus the extras, which
/// apply to both.
/// </summary>
[Flags]
public enum ItemTooltipView
{
	/// <summary>Classic display, without the extras.</summary>
	Classic = 0,
	/// <summary>Attributes split into prefix, base and suffix.</summary>
	Detailed = 1,
	/// <summary>Each value rolled by the in-game jitter (database -> in-game).</summary>
	InGameRolls = 2,
	/// <summary>The possible min/max of each rolled attribute, on its right.</summary>
	ValueRanges = 4,
	/// <summary>The skills granted by the item in a panel on the right.</summary>
	GrantedSkills = 8,
	/// <summary>How many of each piece of the item's set are loaded, on its right.</summary>
	SetPieceCounts = 16,
}

/// <summary>What a tooltip line is drawn as.</summary>
public enum TooltipLineKind
{
	/// <summary>A line of text; color tags in it are honored.</summary>
	Text,
	/// <summary>A blank line.</summary>
	Spacer,
	/// <summary>The gray line between two sections.</summary>
	Delimiter,
	/// <summary>A line of text with a value right-aligned on the same row (a value range, a set piece count).</summary>
	Range,
	/// <summary>A line of text after a warning icon (an incomplete item).</summary>
	Warning,
}

/// <summary>
/// One line of a tooltip.
/// </summary>
/// <param name="Kind">How the line is drawn.</param>
/// <param name="Text">The text, with its TQ color tags; empty for a spacer or a delimiter.</param>
/// <param name="Color">The color of the text without a leading color tag; null for the default (white).</param>
/// <param name="Bold">Bold font.</param>
/// <param name="Range">The value on the right of a <see cref="TooltipLineKind.Range"/> line, drawn in <see cref="ItemTooltipContentBuilder.RangeColor"/>.</param>
public sealed record TooltipLine(TooltipLineKind Kind, string Text, TQColor? Color = null, bool Bold = false, string? Range = null)
{
	/// <summary>A blank line.</summary>
	public static readonly TooltipLine Spacer = new(TooltipLineKind.Spacer, string.Empty);

	/// <summary>The gray line between two sections.</summary>
	public static readonly TooltipLine Delimiter = new(TooltipLineKind.Delimiter, string.Empty);

	/// <summary>A detail under the line above (a source of a compared stat): drawn in a smaller font.</summary>
	public bool Detail { get; init; }

	/// <summary>
	/// A click on the row shows or hides the details of the stat with this key (the item comparison's sources); null for a
	/// row a click does nothing on.
	/// </summary>
	public string? ToggleKey { get; init; }

	/// <summary>A line of text; a null text is shown as an empty line.</summary>
	public static TooltipLine Of(string? text, TQColor? color = null, bool bold = false)
		=> new(TooltipLineKind.Text, text ?? string.Empty, color, bold);

	/// <summary>A line of text with <paramref name="range"/> right-aligned.</summary>
	public static TooltipLine WithRange(string text, string range)
		=> new(TooltipLineKind.Range, text, Range: range);

	/// <summary>A line of text after a warning icon.</summary>
	public static TooltipLine WarningOf(string text)
		=> new(TooltipLineKind.Warning, text, ItemTooltipContentBuilder.IncompleteColor);
}

/// <summary>
/// The lines of an item tooltip panel.
/// </summary>
/// <param name="Lines">The lines, top to bottom.</param>
/// <param name="RelicStart">Index of the first line about the socketed relics (or the completion bonus), which may go in a column on the right.</param>
/// <param name="RelicEnd">Index after the last of those lines; equal to <paramref name="RelicStart"/> when there are none.</param>
public sealed record ItemTooltipContent(IReadOnlyList<TooltipLine> Lines, int RelicStart, int RelicEnd);

/// <summary>
/// The lines of a granted skill block.
/// </summary>
/// <param name="Name">Skill name, beside the icon.</param>
/// <param name="Level">Level line, under the name.</param>
/// <param name="Body">Activation, description, then the effects.</param>
public sealed record SkillTooltipContent(TooltipLine Name, TooltipLine Level, IReadOnlyList<TooltipLine> Body);

/// <summary>
/// Decides which lines the item tooltip shows, in what order and color; the GUI only draws them.
/// </summary>
public class ItemTooltipContentBuilder
{
	/// <summary>Color of the value on the right of a range line.</summary>
	public static readonly TQColor RangeColor = ItemStyle.Broken.TQColor();

	/// <summary>Color of the warning of an incomplete item.</summary>
	public static readonly TQColor IncompleteColor = TQColor.Orange;

	/// <summary>Color of the line naming the mod an item is linked to.</summary>
	public static readonly TQColor ModColor = TQColor.Turquoise;

	private readonly ITranslationService _translationService;

	public ItemTooltipContentBuilder(ITranslationService translationService)
		=> this._translationService = translationService;

	/// <summary>
	/// The display chosen in the settings: the classic or the detailed one, with the extras checked in "Tooltip Enhanced".
	/// </summary>
	/// <param name="settings">User settings.</param>
	/// <param name="enableDetailedTooltipView">Forces the detailed display on or off, whatever the settings.</param>
	/// <param name="sessionForcesDetailed">The session shows the detailed display for now (the forge is open), without
	/// changing the setting.</param>
	public static ItemTooltipView ViewOf(UserSettings settings, bool? enableDetailedTooltipView = null, bool sessionForcesDetailed = false)
	{
		var view = enableDetailedTooltipView ?? (sessionForcesDetailed || settings.EnableDetailedTooltipView)
			? ItemTooltipView.Detailed
			: ItemTooltipView.Classic;
		if (settings.TooltipShowInGameRolls)
			view |= ItemTooltipView.InGameRolls;
		if (settings.TooltipShowValueRanges)
			view |= ItemTooltipView.ValueRanges;
		if (settings.TooltipShowGrantedSkills)
			view |= ItemTooltipView.GrantedSkills;
		if (settings.TooltipShowSetPieceCounts)
			view |= ItemTooltipView.SetPieceCounts;
		return view;
	}

	/// <summary>
	/// What to ask the item provider for: without the skills panel, the granted skills are described inline.
	/// </summary>
	public static FriendlyNamesExtraScopes ScopesFor(ItemTooltipView view)
		=> view.HasFlag(ItemTooltipView.GrantedSkills)
			? FriendlyNamesExtraScopes.ItemFullDisplay | FriendlyNamesExtraScopes.GrantedSkills
			: FriendlyNamesExtraScopes.ItemFullDisplay;

	/// <summary>
	/// Builds the lines of the item panel.
	/// </summary>
	/// <param name="data">Texts of the item, from <c>IItemProvider.GetFriendlyNames</c> with <see cref="ScopesFor"/>.</param>
	/// <param name="view">The display.</param>
	/// <param name="setCounts">How many of each set piece are loaded, shown on the right of the set pieces; null without them.</param>
	public ItemTooltipContent Build(ToFriendlyNameResult data, ItemTooltipView view, IReadOnlyDictionary<RecordId, int>? setCounts = null)
	{
		var item = data.Item;
		var lines = new List<TooltipLine>();
		var relic = ItemStyle.Relic.TQColor();
		var broken = ItemStyle.Broken.TQColor();
		var nameColor = TextColorOrItemColor(item, data.BaseItemInfoDescription);

		// Fullname
		lines.Add(TooltipLine.Of(data.FullName, nameColor, bold: true));

		// Records the loaded game data lacks (a mod not linked, or not installed)
		AddIncomplete(lines, item);

		// Thrown
		if (item.IsThrownWeapon)
			lines.Add(TooltipLine.Of(data.BaseItemInfoStyle));

		// Artifact Level
		if (item.IsArtifact)
			lines.Add(TooltipLine.Of(data.ArtifactClass));

		// Relic Completion
		if (item.IsRelicOrCharm)
			lines.Add(TooltipLine.Of(data.RelicCompletionFormat, nameColor));

		// Recipe Label
		if (item.IsFormulae)
			lines.Add(TooltipLine.Of(data.ArtifactRecipe));

		// Flavor text
		if (data.FlavorText.Length != 0)
		{
			lines.AddRange(data.FlavorText.Select(s => TooltipLine.Of(s)));
			lines.Add(TooltipLine.Spacer);
		}

		// Required Reagents
		if (item.IsFormulae)
		{
			lines.Add(TooltipLine.Spacer);
			lines.Add(TooltipLine.Of(data.FormulaeFormat));
		}

		// Attributes
		if (view.HasFlag(ItemTooltipView.Detailed))
		{
			var ranges = view.HasFlag(ItemTooltipView.ValueRanges);
			AddSection(lines, data.PrefixAttributes, data.PrefixAttributeRanges, data.PrefixInfoDescription, Resources.ItemPropertiesLabelPrefixProperties, ranges);
			AddSection(lines, data.BaseAttributes, data.BaseAttributeRanges, data.BaseItemInfoDescription, Resources.ItemPropertiesLabelBaseItemProperties, ranges);
			AddSection(lines, data.SuffixAttributes, data.SuffixAttributeRanges, data.SuffixInfoDescription, Resources.ItemPropertiesLabelSuffixProperties, ranges);
		}
		else
		{
			// Classic display: base, prefix and suffix lines in the game's order.
			var attributes = data.ClassicAttributes ?? [.. data.BaseAttributes, .. data.PrefixAttributes, .. data.SuffixAttributes];
			var ranges = !view.HasFlag(ItemTooltipView.ValueRanges) ? []
				: data.ClassicAttributes is not null ? data.ClassicAttributeRanges
				: [.. Padded(data.BaseAttributeRanges, data.BaseAttributes.Length)
					, .. Padded(data.PrefixAttributeRanges, data.PrefixAttributes.Length)
					, .. Padded(data.SuffixAttributeRanges, data.SuffixAttributes.Length)];

			// Rings and amulets have no base stats: a blank line separates their bonuses from the name, as the base
			// stats of the other items are separated from theirs.
			if (item.IsJewellery && attributes.Length != 0 && attributes[0].Length != 0 && lines[^1].Kind != TooltipLineKind.Spacer)
				lines.Add(TooltipLine.Spacer);

			AddAttributes(lines, attributes, ranges);
		}

		// In-game jitter comparison (database -> in-game)
		if (view.HasFlag(ItemTooltipView.InGameRolls) && data.InGameRolls.Length != 0)
		{
			lines.Add(TooltipLine.Spacer);
			lines.Add(TooltipLine.Of(Resources.ItemPropertiesLabelInGameRolls, relic, bold: true));
			lines.AddRange(data.InGameRolls.Select(s => TooltipLine.Of(s, broken)));
		}

		// formula Artifact Details goes after formula attributes
		if (data.FormulaeArtifactAttributes.Length != 0)
		{
			lines.Add(TooltipLine.Spacer);
			lines.Add(TooltipLine.Of(data.FormulaeArtifactName, nameColor, bold: true));
			lines.Add(TooltipLine.Of(data.FormulaeArtifactClass, broken));
			lines.AddRange(data.FormulaeArtifactAttributes.Select(s => TooltipLine.Of(s)));
		}

		// Relic attributes after items attributes with delimiter.
		// Besides real relics and charms, a relic slot may hold any record: God mode in the Forge ignores the rules
		// (e.g. a weapon used as a relic), and what was socketed must still show.
		bool hasSocketedRecord = !item.IsRelicOrCharm && !item.IsArtifact
			&& (item.RelicInfo is not null || item.Relic2Info is not null);
		int relicStart = lines.Count;
		if (item.HasRelicOrCharm || hasSocketedRecord)
		{
			AddSocketed(lines, data.Relic1Attributes, data.RelicInfo1Description, data.RelicInfo1CompletionResolved, data.RelicInfo1CompletionBonusResolved, data.RelicBonus1Attributes);
			AddSocketed(lines, data.Relic2Attributes, data.RelicInfo2Description, data.RelicInfo2CompletionResolved, data.RelicInfo2CompletionBonusResolved, data.RelicBonus2Attributes);
		}
		else if (item.IsArtifact)
			AddBonus(lines, data.ArtifactBonus, data.RelicBonus1Attributes);
		else if (item.IsRelicOrCharm)
			AddBonus(lines, data.RelicBonusTitle, data.RelicBonus1Attributes);

		int relicEnd = lines.Count;

		lines.Add(TooltipLine.Delimiter);

		// Add the item seed
		lines.Add(TooltipLine.Of(data.ItemSeed, broken));

		// The expansion clause: the latest expansion the item needs.
		if (this.ExpansionClause(item) is { } clause)
			lines.Add(TooltipLine.Of(clause, TQColor.Green));

		// The mod the item is linked to, last of the seed section: its name, not the identity key that also holds its
		// source ("name|workshop:id").
		if (ModIdentity.NameOf(item.Mod) is { } mod)
			lines.Add(TooltipLine.Of(string.Format(System.Globalization.CultureInfo.CurrentCulture, Resources.ItemModLine, mod), ModColor));

		// ItemSet
		if (data.ItemSet is { setMembers.Count: > 0 } set)
		{
			lines.Add(TooltipLine.Delimiter);
			foreach (var str in set.Translations)
			{
				// The set name comes first, then one line per piece with its count on the right.
				if (setCounts is not null && str.Key != set.setName
					&& setCounts.TryGetValue(RecordId.Create(str.Key), out var count))
					lines.Add(TooltipLine.WithRange(str.Value, count.ToString()));
				else
					lines.Add(TooltipLine.Of(str.Value));
			}
		}

		// Requirements
		if (data.Requirements.Length != 0)
		{
			lines.Add(TooltipLine.Delimiter);
			lines.AddRange(data.Requirements.Select(s => TooltipLine.Of(s, broken)));
		}

		return new ItemTooltipContent(lines, relicStart, relicEnd);
	}

	/// <summary>The lines of a granted skill: name and level by the icon, then activation, description and effects.</summary>
	public static SkillTooltipContent BuildSkill(GrantedSkill skill)
	{
		var mundane = ItemStyle.Mundane.TQColor();
		var body = new List<TooltipLine>();

		if (skill.Activation is not null)
			body.Add(TooltipLine.Of(skill.Activation, mundane));

		body.AddRange(skill.Description.Select(line => TooltipLine.Of(line, mundane)));

		if (skill.Attributes.Length != 0)
		{
			body.Add(TooltipLine.Spacer);
			body.AddRange(skill.Attributes.Select(line => line.Length == 0 ? TooltipLine.Spacer : TooltipLine.Of(line)));
		}

		return new SkillTooltipContent(
			TooltipLine.Of(skill.Name, ItemStyle.Epic.TQColor(), bold: true),
			TooltipLine.Of(skill.LevelText, ItemStyle.Broken.TQColor()),
			body);
	}

	/// <summary>
	/// Puts blocks of the given heights in columns no taller than <paramref name="maxHeight"/>, in order,
	/// with <paramref name="separator"/> between two blocks of a column. A block taller than that gets a column of its own.
	/// </summary>
	/// <returns>The column of each block, from 0.</returns>
	public static int[] PackColumns(IReadOnlyList<int> heights, int separator, int maxHeight)
	{
		var columns = new int[heights.Count];
		int column = -1;
		int height = 0;
		for (int i = 0; i < heights.Count; i++)
		{
			if (column < 0 || height + separator + heights[i] > maxHeight)
			{
				column++;
				height = -separator;
			}

			columns[i] = column;
			height += separator + heights[i];
		}

		return columns;
	}

	/// <summary>The expansion clause shown under the seed, the latest expansion first; null for a base game item.</summary>
	private string? ExpansionClause(Item item)
	{
		if (item.IsAtlantis)
			return this._translationService.ItemAtlantis;
		if (item.IsEmbers)
			return this._translationService.ItemEmbers;
		if (item.IsRagnarok)
			return this._translationService.ItemRagnarok;
		if (item.IsImmortalThrone)
			return this._translationService.ItemIT;
		return null;
	}

	/// <summary>
	/// The warning of an item naming records the loaded game data lacks, then each of them: its other lines leave out
	/// what those records hold.
	/// </summary>
	private static void AddIncomplete(List<TooltipLine> lines, Item item)
	{
		var missing = item.MissingRecords;
		if (missing.Count == 0)
			return;

		lines.Add(TooltipLine.WarningOf(Resources.ItemIncomplete));
		foreach (var (role, id) in missing)
			lines.Add(TooltipLine.Of($"{RoleLabel(role)} : {id.Raw}", TQColor.DarkGray));
	}

	private static string RoleLabel(ItemRecordRole role) => role switch
	{
		ItemRecordRole.BaseItem => Resources.ItemIncompleteBaseItem,
		ItemRecordRole.Prefix => Resources.ItemIncompletePrefix,
		ItemRecordRole.Suffix => Resources.ItemIncompleteSuffix,
		ItemRecordRole.Relic => Resources.ItemIncompleteRelic,
		_ => Resources.ItemIncompleteRelicBonus,
	};

	/// <summary>The color of the leading tag of <paramref name="text"/>, else the color of the item's rarity.</summary>
	private static TQColor TextColorOrItemColor(Item item, string? text)
		=> string.IsNullOrWhiteSpace(text)
			? item.ItemStyle.TQColor()
			: text.GetColorFromTaggedString() ?? item.ItemStyle.TQColor();

	/// <summary>A section title, "label : description", in the description color or the relic color without one.</summary>
	private static string SectionTitle(string? description, string label)
	{
		var text = (description ?? string.Empty).InsertAfterColorPrefix($"{label} : ");
		return text.HasColorPrefix() ? text : $"{ItemStyle.Relic.TQColor().ColorTag()}{text}";
	}

	/// <summary>A detailed section: spacer, bold title, then the attributes with their ranges when shown.</summary>
	private static void AddSection(List<TooltipLine> lines, string[] attributes, string?[] ranges, string? description, string label, bool showRanges)
	{
		if (attributes.Length == 0)
			return;

		lines.Add(TooltipLine.Spacer);
		lines.Add(TooltipLine.Of(SectionTitle(description, label), bold: true));

		AddAttributes(lines, attributes, showRanges ? ranges : []);
	}

	/// <summary>The attribute lines, each with its range on the right when it has one.</summary>
	private static void AddAttributes(List<TooltipLine> lines, string[] attributes, string?[] ranges)
	{
		for (int i = 0; i < attributes.Length; i++)
		{
			var range = i < ranges.Length ? ranges[i] : null;
			lines.Add(string.IsNullOrEmpty(range) ? TooltipLine.Of(attributes[i]) : TooltipLine.WithRange(attributes[i], range));
		}
	}

	/// <summary><paramref name="ranges"/> cut or filled with nulls to <paramref name="length"/> lines.</summary>
	private static IEnumerable<string?> Padded(string?[] ranges, int length)
		=> Enumerable.Range(0, length).Select(i => i < ranges.Length ? ranges[i] : null);

	/// <summary>A socketed relic or charm: delimiter, name, completion, attributes, then its completion bonus.</summary>
	private static void AddSocketed(List<TooltipLine> lines, string[] attributes, string? name, string? completion, string? bonusTitle, string[] bonusAttributes)
	{
		if (attributes.Length == 0)
			return;

		var relic = ItemStyle.Relic.TQColor();
		lines.Add(TooltipLine.Delimiter);
		lines.Add(TooltipLine.Of(name, relic, bold: true));
		lines.Add(TooltipLine.Of(completion, relic));
		lines.AddRange(attributes.Select(s => TooltipLine.Of(s)));
		lines.Add(TooltipLine.Of(bonusTitle, relic));
		lines.AddRange(bonusAttributes.Select(s => TooltipLine.Of(s)));
	}

	/// <summary>The completion bonus of an artifact, relic or charm: spacer, title, attributes.</summary>
	private static void AddBonus(List<TooltipLine> lines, string? title, string[] bonusAttributes)
	{
		if (bonusAttributes.Length == 0)
			return;

		lines.Add(TooltipLine.Spacer);
		lines.Add(TooltipLine.Of(title, ItemStyle.Relic.TQColor()));
		lines.AddRange(bonusAttributes.Select(s => TooltipLine.Of(s)));
	}
}
