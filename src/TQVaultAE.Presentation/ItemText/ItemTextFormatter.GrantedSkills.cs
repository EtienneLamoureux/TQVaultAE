using System.Globalization;
using TQVaultAE.Application.Character;
using TQVaultAE.Application.Layout;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Domain.Results;

namespace TQVaultAE.Presentation.ItemText;

public partial class ItemTextFormatter
{
	/// <summary>Width of the word wrapped skill description, in characters.</summary>
	private const int GrantedSkillDescriptionWidth = 40;

	/// <summary>
	/// Gets the skills granted by an item: by its base record, its affixes, its relics and their completion bonuses
	/// (<c>itemSkillName</c> at <c>itemSkillLevel</c>).
	/// </summary>
	/// <remarks>Must run inside <see cref="GetFriendlyNames"/>, which sets <see cref="Item.CurrentFriendlyNameResult"/>.</remarks>
	private GrantedSkill[] GetGrantedSkills(Item itm)
	{
		var skills = new List<GrantedSkill>();
		foreach (var (source, skillId, level) in this.ItemProvider.GetGrantedSkillSources(itm))
		{
			if (this.GetItemGrantedSkill(itm, skillId, level, source.GetString("itemSkillAutoController", 0)) is { } skill)
				skills.Add(skill);
		}

		return skills.ToArray();
	}

	/// <inheritdoc/>
	public bool HasGrantedSkill(Item itm)
	{
		using var modScope = this.ModScope(itm);
		return this.ItemProvider.GetGrantedSkillSources(itm).Any(s => this.DisplayedSkillRecord(s.SkillId) is not null);
	}

	/// <inheritdoc/>
	public IReadOnlyList<string> GetGrantedSkillNames(Item itm)
	{
		using var modScope = this.ModScope(itm);
		var names = new List<string>();
		foreach (var (_, skillId, _) in this.ItemProvider.GetGrantedSkillSources(itm))
		{
			if (this.DisplayedSkillRecord(skillId) is not { } shown)
				continue;

			var name = this.SkillDisplayName(shown, skillId);

			// Plain text for lists: some translations carry formatting tags, e.g. "{^I}".
			name = name.RemoveAllTQTags().Trim();
			if (name.Length != 0 && !names.Contains(name))
				names.Add(name);
		}

		return names;
	}

	/// <summary>
	/// The record that names a granted skill: its buff record (<c>buffSkillName</c>) when it has one, else the skill itself.
	/// </summary>
	/// <returns><see langword="null"/> when the skill record is missing or has no name, like the game's hidden helper skills.</returns>
	private DBRecordCollection? DisplayedSkillRecord(RecordId skillId)
		=> this.ShownSkill(skillId) is { Record: var shown } && !string.IsNullOrEmpty(shown.GetString("skillDisplayName", 0)) ? shown : null;

	/// <summary>
	/// The record that shows a skill, named or not, with its id: its buff record (<c>buffSkillName</c>) when it has one,
	/// else the skill itself. A pet's skills are shown the same way.
	/// </summary>
	/// <returns><see langword="null"/> when the skill record is missing.</returns>
	private (DBRecordCollection Record, RecordId Id)? ShownSkill(RecordId skillId)
	{
		if (Database.GetRecordFromFile(skillId) is not { } skillRecord)
			return null;

		var buffId = skillRecord.GetString("buffSkillName", 0).ToRecordId();
		return !RecordId.IsNullOrEmpty(buffId) && Database.GetRecordFromFile(buffId) is { } buff ? (buff, buffId) : (skillRecord, skillId);
	}

	/// <summary>
	/// The translated name of a skill as shown: its buff's for a buff skill (<see cref="DisplayedSkillRecord"/>).
	/// </summary>
	/// <returns><see langword="null"/> when the record is missing or unnamed, or its name has no translation.</returns>
	private string? TranslatedSkillName(RecordId skillId)
		=> this.DisplayedSkillRecord(skillId) is { } shown && TranslationService.TryTranslateXTag(shown.GetString("skillDisplayName", 0), out var name)
			? name
			: null;

	/// <summary>
	/// What the game's skill window leaves out of a skill's effects: the projectile mechanics (speed, reach, spread), the
	/// time a charge level lasts, the pierce ratio, the "can't be dispelled" and "special animation" flags, the layout of
	/// a ring of projectiles and the skill linking flag.
	/// </summary>
	private static readonly HashSet<string> SkillWindowHiddenVariables = new(StringComparer.OrdinalIgnoreCase)
	{
		"headVelocity", "tailVelocity", "maxDistance", "projectileLaunchRotation", "skillChargeDuration", "notDispelable",
		"offensivePierceRatioMin", "offensivePierceRatioMax", "offensivePierceRatio", "alwaysUseSpecialAnimation",
		"numRings", "spacingAngle", "distanceIncrement", "angleToCaster", "skipSkillLinking", "surviveAfterMasterDeath",
	};

	/// <inheritdoc/>
	public GrantedSkill? GetSkillAtLevel(RecordId skillId, int level, SkillWindowContext? context = null)
		=> this.GetGrantedSkill(new Item(), skillId, Math.Max(1, level), null, context ?? SkillWindowContext.None);

	/// <summary>
	/// The skill window's own hidden variables plus the recharge and energy cost, which
	/// <see cref="SkillWindowCosts"/> writes with the character's modifiers.
	/// </summary>
	private static readonly HashSet<string> SkillWindowOwnHiddenVariables = new(SkillWindowHiddenVariables, StringComparer.OrdinalIgnoreCase)
	{
		"skillCooldownTime", "skillManaCost",
	};

	/// <summary>
	/// The recharge and energy cost lines of the skill window: the skill's values plus what its modifiers with points add,
	/// the recharge lowered by the character's and the modifiers' reduction (<see cref="SkillTooltipLayout.Recharge"/>).
	/// The energy cost is written without the reductions, as the game does.
	/// </summary>
	/// <remarks>A buff skill may keep them in its buff record (Study Prey).</remarks>
	/// <returns>The lines, each with the variable that places it and whether the modifiers changed it.</returns>
	private List<(string Variable, string Text, bool Modified)> SkillWindowCosts(DBRecordCollection skillRecord, DBRecordCollection? buffRecord
		, int level, SkillWindowContext context)
	{
		var lines = new List<(string Variable, string Text, bool Modified)>();
		float Own(string variable)
		{
			var value = ValueAtLevel(skillRecord[variable], level);
			return value != 0 || buffRecord is null ? value : ValueAtLevel(buffRecord[variable], level);
		}

		float cooldown = Own("skillCooldownTime");
		float manaCost = Own("skillManaCost");

		// Like the other effect lines, a leading colour that the tool tip drops, keeping the recharge's own aqua.
		var plain = ItemStyle.Mundane.TQColor().ColorTag();
		float seconds = 0, reduction = 0, addedManaCost = 0;
		foreach (var modifier in context.Modifiers)
		{
			if (Database.GetRecordFromFile(modifier.Id) is not { } record)
				continue;

			seconds += ValueAtLevel(record["skillCooldownTime"], modifier.Level);
			reduction += ValueAtLevel(record["skillCooldownReduction"], modifier.Level);
			addedManaCost += ValueAtLevel(record["skillManaCost"], modifier.Level);
		}

		manaCost += addedManaCost;
		if (cooldown > 0)
		{
			var recharge = SkillTooltipLayout.Recharge(cooldown, seconds, context.CooldownReduction + reduction, context.CooldownReductionCap);
			var format = this.GameFormat("CooldownTime") ?? ItemAttributeProvider.ConvertFormat("^a{%.1f0} Second(s) Recharge");
			lines.Add(("skillCooldownTime", plain + Format(format, recharge), seconds != 0 || reduction != 0));
		}

		if (manaCost > 0)
		{
			var label = TranslationService.TryTranslateXTag("ManaCost", out var text) ? text : "Energy Cost";
			var cost = this.FormatSkillCost(manaCost, label) ?? $"{manaCost.ToString("0", CultureInfo.CurrentCulture)} {label}";
			lines.Add(("skillManaCost", plain + cost, addedManaCost != 0));
		}

		return lines;
	}

	/// <summary>
	/// A skill granted by an item, as the game's item tool tip writes it: the skill window's lines and order (recharge,
	/// "Extra Life Time" of a summon...), without the hit activation chance, which the activation line already gives.
	/// </summary>
	/// <remarks>The game writes the effects in plain white, the recharge keeping its aqua (Brennus' Great Helm).</remarks>
	private GrantedSkill? GetItemGrantedSkill(Item itm, RecordId skillId, int level, string? autoController)
	{
		if (this.GetGrantedSkill(itm, skillId, Math.Max(1, level), autoController, SkillWindowContext.None, itemGranted: true) is not { } skill)
			return null;

		var effect = ItemStyle.Epic.TQColor().ColorTag();
		var plain = ItemStyle.Mundane.TQColor().ColorTag();
		return skill with
		{
			Attributes = [.. skill.Attributes.Select(l => l.StartsWith(effect, StringComparison.Ordinal) ? plain + l[effect.Length..] : l)],
		};
	}

	/// <summary>
	/// Describes one granted skill at a level: name, icon, activation, description and effects.
	/// A buff skill (<c>buffSkillName</c>) takes its name, icon, description and effects from its buff record.
	/// </summary>
	/// <param name="context">
	/// The character's values for the skill window's tool tip; <see langword="null"/> for an item's granted skill. With
	/// it, a summon also shows its own parameters (recharge, energy cost...), the recharge and energy cost take the
	/// modifiers into account, and what the skill window hides (<see cref="SkillWindowHiddenVariables"/>) is left out.
	/// </param>
	/// <returns><see langword="null"/> when the skill record is missing or has no name, like the game's hidden helper skills.</returns>
	private GrantedSkill? GetGrantedSkill(Item itm, RecordId skillId, int level, string? autoController, SkillWindowContext? context = null
		, bool itemGranted = false)
	{
		bool skillWindow = context is not null;
		var hidden = skillWindow ? SkillWindowHiddenVariables : null;
		var ownHidden = skillWindow ? SkillWindowOwnHiddenVariables : null;
		if (Database.GetRecordFromFile(skillId) is not { } skillRecord)
			return null;

		// A pet upgrade (e.g. the Core Dweller's "Inner Fire") only names the skill it gives the pet (petSkillName).
		if (skillRecord.GetString("Class", 0).Equals("SkillSecondary_PetModifier", noCase)
			&& skillRecord.GetString("petSkillName", 0).ToRecordId() is { } petSkillId && !RecordId.IsNullOrEmpty(petSkillId)
			&& Database.GetRecordFromFile(petSkillId) is { } petSkill)
		{
			skillId = petSkillId;
			skillRecord = petSkill;
		}

		var buffId = skillRecord.GetString("buffSkillName", 0).ToRecordId();
		var buffRecord = RecordId.IsNullOrEmpty(buffId) ? null : Database.GetRecordFromFile(buffId);
		var shown = buffRecord ?? skillRecord;

		string nameTag = shown.GetString("skillDisplayName", 0);
		if (string.IsNullOrEmpty(nameTag))
			return null;

		var name = this.SkillDisplayName(shown, skillId);

		string icon = shown.GetString("skillUpBitmapName", 0);
		if (string.IsNullOrEmpty(icon))
			icon = skillRecord.GetString("skillUpBitmapName", 0);

		string[] description = this.GetSkillDescription(shown);

		string levelFormat = this.GameFormat("MenuLevel", "Level:   {0}");

		var activation = this.GetSkillActivationText(autoController);

		// Effects at the granted level: the skill's own parameters (energy cost, cooldown, radius...), then the buff's.
		// The skill window writes the recharge and energy cost itself, with the character's modifiers, and lists every
		// line in its own order (SkillWindowAttributeOrder).
		var attributes = new List<string>();
		var costs = context is null ? [] : this.SkillWindowCosts(skillRecord, buffRecord, level, context);

		// The item tool tip keeps a line's first colour: the recharge's own aqua, not the leading plain one.
		if (itemGranted)
			costs = costs.Select(c => c with { Text = DropLeadingPlainColor(c.Text) }).ToList();
		var ownLines = costs.Select(c => (c.Variable, c.Text)).ToList();
		var order = skillWindow ? new SkillWindowLines(ownLines) : null;

		// "Extra Life Time 12 Seconds" (SpawnObjectsTimeToLive), one of the skill's parameters, which the skill window writes
		// for any skill: a summon (before its attributes) or a modifier that lengthens its parent's summons (Lasting Legacy).
		// The game's text has no value index ("{%.0f}"), unlike the others ("{%.0f0}"). The modifiers with points add
		// theirs (Ancestral Horn 24 + Lasting Legacy 25 = 49), while the summon's own "Life Time" keeps the skill's value.
		float timeToLive = skillWindow ? ValueAtLevel(skillRecord["spawnObjectsTimeToLive"], level) : 0F;
		if (timeToLive > 0)
			timeToLive += context!.Modifiers.Sum(m => Database.GetRecordFromFile(m.Id) is { } modifier
				? ValueAtLevel(modifier["spawnObjectsTimeToLive"], m.Level) : 0F);
		if (timeToLive > 0 && TranslationService.TryTranslateXTag("SpawnObjectsTimeToLive", out var lifeTime))
			ownLines.Add(("spawnObjectsTimeToLive", Format(ItemAttributeProvider.ConvertFormat(lifeTime.Replace("{%.0f}", "{%.0f0}", StringComparison.Ordinal)), timeToLive)));

		// Summons: Skill_SpawnPet, and the projectiles that place one (Skill_AttackProjectileSpawnPet: Lay Trap, Sylvan Nymph).
		if (skillRecord.GetString("Class", 0).EndsWith("SpawnPet", StringComparison.OrdinalIgnoreCase))
		{
			if (skillWindow)
			{
				this.GetAttributesFromRecord(itm, skillRecord, true, skillId, attributes, skillLevelIndex: level - 1, hiddenVariables: ownHidden, skillWindow: order);
			}

			// An item's summon keeps the pet's damage and skills, which the skill window leaves out.
			this.ConvertPetStats(itm, skillRecord, attributes, level, itemGranted ? null : hidden, context?.PetSkillBonus ?? 0);
		}
		else
		{
			// "5% Chance of Activating" of a skill triggered by hits (onHitActivationChance), which item tool tips leave out.
			float activationChance = skillWindow && !itemGranted ? ValueAtLevel(skillRecord["onHitActivationChance"], level) : 0F;
			if (activationChance > 0)
				ownLines.Add(("onHitActivationChance", this.ActivationChanceText(activationChance)));

			this.GetAttributesFromRecord(itm, skillRecord, true, skillId, attributes, skillLevelIndex: level - 1, hiddenVariables: ownHidden, skillWindow: order);
			if (buffRecord is not null)
				this.GetAttributesFromRecord(itm, buffRecord, true, buffId, attributes, skillLevelIndex: level - 1, hiddenVariables: ownHidden
					, skillWindow: skillWindow ? SkillWindowLines.None : null);
		}

		// A skill that only casts another one (skillCastName, e.g. "Potion Blast" casts "Shadow Surge" on a potion):
		// the chance and the cast skill, with its effects at its own level. Its description stands in for a missing one.
		if (this.GetCastSkill(itm, skillRecord, level, levelFormat, attributes, out var cast) is { Length: > 0 } castDescription && description.Length == 0)
			description = castDescription;

		this.GetCastOnDamageType(skillRecord, level, attributes);

		// The lines the character's modifiers changed, which the next level repeats.
		var effects = TrimEmptyLines(attributes);
		var modifiedLines = costs.Where(c => c.Modified)
			.Select(c => Array.FindIndex(effects, a => ReferenceEquals(a, c.Text)))
			.Where(i => i >= 0)
			.ToList();

		return new GrantedSkill(
			skillId,
			name,
			level,
			Format(levelFormat, level),
			string.IsNullOrEmpty(icon) ? null : icon.ToRecordId(),
			string.IsNullOrEmpty(activation) ? null : activation,
			description,
			effects
		)
		{
			ModifiedLines = modifiedLines,
			CastLines = cast is var (chanceLine, levelLine, lastLine, chance)
				? (Array.FindIndex(effects, a => ReferenceEquals(a, chanceLine)), Array.FindIndex(effects, a => ReferenceEquals(a, levelLine)),
					Array.FindIndex(effects, a => ReferenceEquals(a, lastLine)), chance)
				: (-1, -1, -1, 0),
		};
	}

	/// <summary>
	/// The name of a skill (<c>skillDisplayName</c>): its translation (or the text itself when the record writes plain
	/// text, like "Storm Orb"), else the record file name.
	/// </summary>
	private string SkillDisplayName(DBRecordCollection skill, RecordId skillId)
		=> TranslationService.TryTranslateXTag(skill.GetString("skillDisplayName", 0), out var name)
			? name
			: PathIO.GetFileNameWithoutExtension(skillId.Raw);

	/// <summary>The translated description of a skill record (<c>skillBaseDescription</c>), word wrapped.</summary>
	private string[] GetSkillDescription(DBRecordCollection skill)
	{
		string tag = skill.GetString("skillBaseDescription", 0);
		return !string.IsNullOrEmpty(tag) && TranslationService.TryTranslateXTag(tag, out var text)
			? StringHelper.WrapWords(text, GrantedSkillDescriptionWidth).ToArray()
			: [];
	}

	/// <summary>
	/// Adds the skill a granted skill casts (<c>skillCastName</c>, with <c>skillCastChance</c> at <c>skillCastLevel</c>):
	/// a line with the chance and its name, its level, then its effects at that level.
	/// </summary>
	/// <param name="cast">The chance line (null when none is written), the level line, the last line of the effects and the
	/// chance; null without cast skill.</param>
	/// <returns>The description of the cast skill; empty when there is none or no cast skill.</returns>
	private string[] GetCastSkill(Item itm, DBRecordCollection skillRecord, int level, string levelFormat, List<string> attributes,
		out (string? ChanceLine, string LevelLine, string LastLine, float Chance)? cast)
	{
		cast = null;
		var castId = skillRecord.GetString("skillCastName", 0).ToRecordId();
		if (RecordId.IsNullOrEmpty(castId) || Database.GetRecordFromFile(castId) is not { } castRecord)
			return [];

		// A helper skill without a name (e.g. the shield of "Arcane Healing"): its effects only, after "n% Chance of:".
		string? castName = string.IsNullOrWhiteSpace(castRecord.GetString("skillDisplayName", 0))
			? null
			: this.SkillDisplayName(castRecord, castId);

		float chance = ValueAtLevel(skillRecord["skillCastChance"], level);
		int castLevel = Math.Max((int)ValueAtLevel(skillRecord["skillCastLevel"], level), 1);

		var text = ItemStyle.Mundane.TQColor().ColorTag();
		var name = ItemStyle.Epic.TQColor().ColorTag();
		if (attributes.Count != 0)
			attributes.Add(string.Empty);

		string? chanceLine = null;
		if (castName is null)
		{
			if (chance > 0 && this.GameFormat("GlobalPercentChanceOfAllTag") is { } chanceOfAll)
				chanceLine = $"{text}{Format(chanceOfAll, chance)}";
		}
		else if (chance > 0 && this.GameFormat("ChanceOfTag") is { } chanceFormat)
			chanceLine = $"{text}{Format(chanceFormat, chance)} {name}{castName}";
		else
			chanceLine = $"{name}{castName}";

		if (chanceLine is not null)
			attributes.Add(chanceLine);

		var levelLine = $"{ItemStyle.Broken.TQColor().ColorTag()}{Format(levelFormat, castLevel)}";
		attributes.Add(levelLine);

		// On a kill, the cast skill can appear at the slain enemy instead of the player: an area effect lands there.
		if (skillRecord.GetString("Class", 0).Equals("Skill_PassiveOnTargetKilled", noCase)
			&& ValueAtLevel(skillRecord["skillCastOnTargetLocation"], level) > 0
			&& TranslationService.TryTranslateXTag("tqvSkillCastOnTargetLocation", out var onTarget))
			attributes.Add($"{text}{onTarget}");
		this.GetAttributesFromRecord(itm, castRecord, true, castId, attributes, skillLevelIndex: castLevel - 1);
		cast = (chanceLine, levelLine, attributes[^1], chance);
		return this.GetSkillDescription(castRecord);
	}

	/// <summary>
	/// The skills cast on a kill according to the damage type of the killing blow (<c>skillNameCastOnDamage_Fire</c>,
	/// <c>_Cold</c>, <c>_Lightning</c>: Echoes of the Ancestors), as the game's skill window writes them: the chance to be
	/// used, then "Chance for one of the following:" and the fire, cold and lightning damage of those skills at the
	/// level, in whole numbers. Nothing at a level without a chance.
	/// </summary>
	private void GetCastOnDamageType(DBRecordCollection skillRecord, int level, List<string> attributes)
	{
		float chance = ValueAtLevel(skillRecord["skillCastChance"], level);
		if (chance <= 0)
			return;

		var text = ItemStyle.Mundane.TQColor().ColorTag();
		var damages = new List<string>();
		foreach (var type in (string[])["Fire", "Cold", "Lightning"])
		{
			var castId = skillRecord.GetString($"skillNameCastOnDamage_{type}", 0).ToRecordId();
			if (RecordId.IsNullOrEmpty(castId) || Database.GetRecordFromFile(castId)?[$"offensive{type}Min"] is not { } damage)
				continue;

			// The value at the level, none past the last one.
			int value = level <= damage.NumberOfValues ? (int)damage.GetSingle(level - 1) : 0;
			if (value > 0 && TranslationService.TryTranslateXTag($"Damage{type}", out var label))
				damages.Add($"{text}{value.ToString(CultureInfo.CurrentCulture)} {label}");
		}

		if (damages.Count == 0)
			return;

		// A skill that also casts a named skill writes its chance with that skill (GetCastSkill). The skill whose kills
		// trigger it (triggeringSkill) follows the chance: "33.0% Chance to be Used : Shen Pao".
		if (string.IsNullOrEmpty(skillRecord.GetString("skillCastName", 0))
			&& this.GameFormat("SkillChanceWeightFloat") is { } chanceFormat)
		{
			var chanceLine = Format(chanceFormat, chance);
			if (this.TriggeringSkillName(skillRecord) is { } trigger)
				chanceLine += " : " + trigger;
			attributes.Add($"{text}{chanceLine}");
		}

		attributes.Add(string.Empty);
		if (TranslationService.TryTranslateXTag("GlobalChanceOfOneTag", out var oneOf))
			attributes.Add($"{text}{oneOf}");

		attributes.AddRange(damages);
	}

	/// <summary>
	/// The name of the skill whose kills trigger a skill (<c>triggeringSkill</c>: Shen Pao for Echoes of the Ancestors).
	/// </summary>
	/// <returns><see langword="null"/> when the skill names none, or names a missing or unnamed record.</returns>
	private string? TriggeringSkillName(DBRecordCollection skillRecord)
	{
		var triggerId = skillRecord.GetString("triggeringSkill", 0).ToRecordId();
		if (RecordId.IsNullOrEmpty(triggerId) || Database.GetRecordFromFile(triggerId) is not { } trigger
			|| string.IsNullOrWhiteSpace(trigger.GetString("skillDisplayName", 0)))
			return null;

		return this.SkillDisplayName(trigger, triggerId);
	}

	/// <summary>The value of a skill parameter at a level: one value per level, the last one beyond.</summary>
	private static float ValueAtLevel(Variable? variable, int level) => ShownSingle(variable, level - 1);

	/// <summary>A line without its leading plain colour when another colour follows it ("{^W}{^a}6.0 Second(s) Recharge").</summary>
	private static string DropLeadingPlainColor(string line)
	{
		var plain = ItemStyle.Mundane.TQColor().ColorTag();
		return line.StartsWith(plain, StringComparison.Ordinal) && line[plain.Length..].HasColorPrefix() ? line[plain.Length..] : line;
	}

	/// <summary>Drops the blank lines at both ends; the ones between groups stay as separators.</summary>
	private static string[] TrimEmptyLines(List<string> lines)
	{
		int start = lines.FindIndex(l => !string.IsNullOrWhiteSpace(l));
		if (start < 0)
			return [];

		int end = lines.FindLastIndex(l => !string.IsNullOrWhiteSpace(l));
		return lines.GetRange(start, end - start + 1).ToArray();
	}
}
