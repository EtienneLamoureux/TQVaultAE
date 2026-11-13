using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Presentation;

/// <summary>What a line of the skill tool tip is: the window draws each kind in the colour the game uses.</summary>
public enum SkillTipKind
{
	/// <summary>An empty line between the parts.</summary>
	Blank,

	/// <summary>The skill or mastery name.</summary>
	Title,

	/// <summary>The description and the effects of the current level.</summary>
	Text,

	/// <summary>"Current Level : 7", "Next Level : 8".</summary>
	Level,

	/// <summary>The effects of the current level from its first ability (a summon's skills, a granted skill).</summary>
	Ability,

	/// <summary>The effects the next level changes, in grey.</summary>
	Next,

	/// <summary>What stops a point: a requirement, no point left.</summary>
	Refused,

	/// <summary>"Press to add unused skill points."</summary>
	CanAdd,

	/// <summary>The levels the equipment adds.</summary>
	Gear,

	/// <summary>How to click.</summary>
	Hint,
}

/// <summary>A line of the skill tool tip.</summary>
/// <param name="Text">The text, with the game's colour codes inside a line.</param>
/// <param name="Kind">What the line is.</param>
/// <param name="Wrap">True for a text wrapped to the tool tip width (the description, the hint).</param>
public readonly record struct SkillTipLine(string Text, SkillTipKind Kind, bool Wrap = false)
{
	public static readonly SkillTipLine Blank = new(string.Empty, SkillTipKind.Blank);
}

/// <summary>
/// Builds the tool tip of a skill as the game's skill window shows it: name, description, the current level with its
/// effects, the next level with
/// its effects, then what stops a point or the invitation to add one; then the levels the equipment adds and how to click.
/// The mastery button shows the mastery the same way.
/// </summary>
public sealed class SkillTooltipContentBuilder(ICharacterEditService editService, ICharacterStatsService statsService, ITranslationService translation)
{
	private readonly ICharacterEditService _editService = editService;
	private readonly ICharacterStatsService _statsService = statsService;
	private readonly ITranslationService _translation = translation;

	/// <summary>The lines of the tool tip of a skill, or of the mastery when <paramref name="skill"/> is null.</summary>
	/// <param name="skills">The skills as the editor has them now.</param>
	/// <param name="slot">The mastery slot of the pane.</param>
	/// <param name="skill">The skill under the mouse; null for the mastery button.</param>
	/// <param name="augments">The skill bonuses of the equipped items.</param>
	/// <param name="cooldownReduction">The character's recharge reduction, read only for a skill.</param>
	/// <param name="difficulty">The difficulty of the statistics, for the recharge reduction cap.</param>
	public List<SkillTipLine> Build(CharacterSkillEditor skills, int slot, MasterySkill? skill, IReadOnlyList<SkillAugment> augments,
		Func<double> cooldownReduction, int difficulty)
	{
		if (skills.Slots[slot] is not { } tree)
			return [];

		var lines = new List<SkillTipLine>();
		IReadOnlyList<string> currentEffects = [];

		// The item tooltips colour the effects as an epic item; the skill window writes them plain, keeping only the
		// colours inside a line (recharge time in aqua).
		var context = SkillWindowContext.None;
		HashSet<string> modified = [];
		IEnumerable<string> Effects(RecordId id, int level)
		{
			if (this._editService.DescribeSkill(id, level, context) is not { } described)
				return [];

			var effects = described.Attributes.Select(a => a.RemoveLeadingColorTag()).ToList();
			modified = [.. described.ModifiedLines.Where(i => i < effects.Count).Select(i => effects[i])];
			return effects;
		}

		void Level(string tag, string fallback, RecordId id, int level)
		{
			lines.Add(SkillTipLine.Blank);
			lines.Add(new SkillTipLine(this.GameText(tag, fallback, level), SkillTipKind.Level));
			currentEffects = SkillTooltipLayout.CurrentEffects(Effects(id, level));
			var abilities = SkillTooltipLayout.AbilitiesStart(currentEffects);
			lines.AddRange(currentEffects.Select((e, i) => e.Length == 0 ? SkillTipLine.Blank
				: new SkillTipLine(e, i < abilities ? SkillTipKind.Text : SkillTipKind.Ability)));
		}

		// The next level is a line type of its own, in grey, with only what it changes.
		void NextLevel(string tag, string fallback, RecordId id, int level)
		{
			lines.Add(SkillTipLine.Blank);
			lines.Add(new SkillTipLine(this.GameText(tag, fallback, level), SkillTipKind.Level));
			var next = Effects(id, level).ToList();
			lines.AddRange(SkillTooltipLayout.NextEffects([.. currentEffects], next, modified)
				.Select(e => e.Length == 0 ? SkillTipLine.Blank : new SkillTipLine(e, SkillTipKind.Next)));
		}

		var masteryLevel = skills.GetMasteryLevel(slot);
		if (skill is null)
		{
			lines.Add(new SkillTipLine(this._translation.TranslateOr(tree.NameTag, tree.Mastery.ToString()), SkillTipKind.Title));
			lines.AddRange(this.Description(tree.DescriptionTag));
			if (masteryLevel > 0)
				Level("tagCurrentLevel", "Current Level : {%d0}", tree.Id, masteryLevel);
			if (masteryLevel < tree.MaxLevel)
				NextLevel("tagNextLevel", "Next Level : {%d0}", tree.Id, masteryLevel + 1);
			this.Requirements(lines, skills, masteryLevel < tree.MaxLevel, []);
			lines.Add(SkillTipLine.Blank);
			lines.Add(new SkillTipLine(Resources.CESkillPaneHint, SkillTipKind.Hint, Wrap: true));
			return lines;
		}

		var points = skills.GetLevel(skill.Id);
		var bonus = SkillBonus.For(augments, tree, skill.Id);

		// The game counts the levels of the equipment once the skill has a point.
		var current = skill.EffectiveLevel(points, bonus.Total);

		// Every skill shows its next level up to the record's top level, equipment included (War Horn 10 + 1 shows 12; Maul
		// 11 + 8 stops at 16).
		var hasNext = skill.HasNextLevel(points, bonus.Total);

		// A point can be spent while the points are below the maximum and the equipment has not reached the ultimate level.
		var canAddPoint = points < skill.MaxLevel && (skill.UltimateLevel == 0 || current < skill.UltimateLevel);

		// The equipment's levels also raise a summon's own skills (Battle Standard 11/10 shows its level 12 buff), all of
		// them even beyond the ultimate level (Call of the Wild 15 + 6: the wolves dodge as at +6).
		context = this.SkillContext(skills, tree, skill, augments, points > 0 ? bonus.Total : 0, cooldownReduction(), difficulty);
		lines.Add(new SkillTipLine(this._translation.TranslateOr(skill.NameTag, skill.Id.PrettyFileName), SkillTipKind.Title));
		lines.AddRange(this.Description(skill.DescriptionTag));
		if (current > 0)
			Level("tagCurrentLevel", "Current Level : {%d0}", skill.Id, current);
		if (hasNext)
			NextLevel("tagNextLevel", "Next Level : {%d0}", skill.Id, current + 1);

		var refused = new List<string>();
		if (masteryLevel < skill.RequiredMasteryLevel)
		{
			var missing = skill.RequiredMasteryLevel - masteryLevel;
			var mastery = this._translation.TranslateOr(tree.NameTag, tree.Mastery.ToString());
			refused.Add(missing == 1
				? this.GameText("tagSkillReq01Singular", "This skill requires {%d0} additional skill point in {%s1}.", missing, mastery)
				: this.GameText("tagSkillReq01", "This skill requires {%d0} additional skill points in {%s1}.", missing, mastery));
		}

		if (skill.ParentId is { } parentId && skills.GetLevel(parentId) == 0
			&& tree.Skills.FirstOrDefault(s => s.Id == parentId) is { } parent)
			refused.Add(this.GameText("tagSkillReq03", "This skill requires the {%s1} skill.", 1, this._translation.TranslateOr(parent.NameTag, parent.Id.PrettyFileName)));

		this.Requirements(lines, skills, canAddPoint, refused);

		var gear = this.GearBonusLines(tree, skill, bonus).ToList();
		if (gear.Count != 0)
		{
			lines.Add(SkillTipLine.Blank);
			lines.AddRange(gear.Select(g => new SkillTipLine(g, SkillTipKind.Gear)));
		}

		lines.Add(SkillTipLine.Blank);
		lines.Add(new SkillTipLine(Resources.CESkillPaneHint, SkillTipKind.Hint, Wrap: true));
		return lines;
	}

	/// <summary>A game text with its values: <c>{%d0}</c>, <c>{%s1}</c>... (any printf format, by index).</summary>
	private string GameText(string tag, string fallback, params object[] values)
		=> CharacterPanelLayout.FormatGameText(this._translation.TranslateOr(tag, fallback, removeAllTQTags: false), values);

	/// <summary>
	/// A description as the skill window writes it: with its colours, one wrapped line per line of the text, as its line
	/// breaks (<c>{^n}</c>) split it ("...around you.", "{^Y}Can trigger on enemy death."). A colour goes on to the next
	/// line until another one replaces it; two breaks in a row leave an empty line.
	/// </summary>
	private IEnumerable<SkillTipLine> Description(string? tag)
	{
		if (string.IsNullOrEmpty(tag) || this._translation.TranslateXTag(tag) is not { Length: > 0 } text)
		{
			yield return new SkillTipLine(string.Empty, SkillTipKind.Text, Wrap: true);
			yield break;
		}

		var parts = TQVaultAE.GameFiles.TextTags.SplitLines(text).Select(p => p.TrimEnd()).ToList();
		while (parts.Count > 1 && parts[^1].Length == 0)
			parts.RemoveAt(parts.Count - 1);

		char? color = null;
		foreach (var part in parts)
		{
			if (part.Length == 0)
			{
				yield return SkillTipLine.Blank;
				continue;
			}

			var runs = part.SplitColorRuns(bareTags: true);
			var shown = color is { } carried && runs is [{ Code: null }, ..] ? $"{{^{carried}}}{part}" : part;
			yield return new SkillTipLine(shown, SkillTipKind.Text, Wrap: true);
			if (runs is [.., { Code: { } last }])
				color = last;
		}
	}

	/// <summary>
	/// What the character adds to a skill in the skill window: the equipment's levels for a summon's skills, the skill's
	/// modifiers that have points (Resilience lowers Adrenaline's recharge, Flush Out adds to Study Prey's energy cost)
	/// and the character's recharge reduction with its cap, for the difficulty of the statistics.
	/// </summary>
	private SkillWindowContext SkillContext(CharacterSkillEditor skills, MasteryTree tree, MasterySkill skill,
		IReadOnlyList<SkillAugment> augments, int petSkillBonus, double cooldownReduction, int difficulty)
	{
		var modifiers = tree.Skills
			.Where(child => child.ParentId is { } parent && parent.Equals(skill.Id) && skills.GetLevel(child.Id) > 0)
			.Select(child => new SkillModifierLevel(child.Id,
				child.EffectiveLevel(skills.GetLevel(child.Id), SkillBonus.For(augments, tree, child.Id).Total)))
			.ToList();

		var cap = this._statsService.GetGameBalance().EngineValue("playerCooldownReductionCap", difficulty);
		return new SkillWindowContext(petSkillBonus, modifiers, cooldownReduction, cap);
	}

	/// <summary>
	/// The last part of the game's skill tool tip: what stops a point, in red; else "No skill points to add." when none is
	/// left, or "Press to add unused skill points." when a point can be spent.
	/// </summary>
	private void Requirements(List<SkillTipLine> lines, CharacterSkillEditor skills, bool canAddPoint, List<string> refused)
	{
		if (refused.Count == 0 && !canAddPoint)
			return;

		lines.Add(SkillTipLine.Blank);
		if (refused.Count != 0)
			lines.AddRange(refused.Select(r => new SkillTipLine(r, SkillTipKind.Refused)));
		else if (skills.AvailablePoints <= 0)
			lines.Add(new SkillTipLine(this.GameText("NoSkillPointsToAdd", "No skill points to add."), SkillTipKind.Refused));
		else
			lines.Add(new SkillTipLine(this.GameText("PressToAddUnusedSkillPoints", "Press to add unused skill points."), SkillTipKind.CanAdd));
	}

	/// <summary>The gear bonus lines of a skill tooltip: "+N to the skill", "+N to all skills of the mastery", "+N to all skills".</summary>
	private IEnumerable<string> GearBonusLines(MasteryTree tree, MasterySkill skill, SkillBonus bonus)
	{
		var culture = System.Globalization.CultureInfo.CurrentCulture;
		if (bonus.Skill != 0)
			yield return string.Format(culture, Resources.CEGearBonusSkill, bonus.Skill, this._translation.TranslateOr(skill.NameTag, skill.Id.PrettyFileName));
		if (bonus.Mastery != 0)
			yield return string.Format(culture, Resources.CEGearBonusMastery, bonus.Mastery, this._translation.TranslateOr(tree.NameTag, tree.Mastery.ToString()));
		if (bonus.All != 0)
			yield return string.Format(culture, Resources.CEGearBonusAll, bonus.All);
	}
}
