using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Domain.Results;

namespace TQVaultAE.Data;

public partial class ItemProvider
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
		foreach (var (source, skillId) in this.GrantedSkillSources(itm))
		{
			int level = Math.Max(source.GetInt32("itemSkillLevel", 0), 1);
			if (this.GetGrantedSkill(itm, skillId, level, source.GetString("itemSkillAutoController", 0)) is { } skill)
				skills.Add(skill);
		}

		return skills.ToArray();
	}

	/// <inheritdoc/>
	public bool HasGrantedSkill(Item itm)
		=> this.GrantedSkillSources(itm).Any(s => this.DisplayedSkillRecord(s.SkillId) is not null);

	/// <inheritdoc/>
	public IReadOnlyList<string> GetGrantedSkillNames(Item itm)
	{
		var names = new List<string>();
		foreach (var (_, skillId) in this.GrantedSkillSources(itm))
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
	/// The records of an item that grant a skill (<c>itemSkillName</c>): its base, its affixes, its relics and their
	/// completion bonuses, each once.
	/// </summary>
	private IEnumerable<(DBRecordCollection Source, RecordId SkillId)> GrantedSkillSources(Item itm)
	{
		RecordId?[] sources = [itm.BaseItemId, itm.prefixID, itm.suffixID, itm.relicID, itm.RelicBonusId, itm.relic2ID, itm.RelicBonus2Id];

		var seen = new HashSet<RecordId>();
		foreach (var sourceId in sources)
		{
			if (RecordId.IsNullOrEmpty(sourceId) || !seen.Add(sourceId))
				continue;

			if (Database.GetRecordFromFile(sourceId) is not { } source)
				continue;

			string skillName = source.GetString("itemSkillName", 0);
			if (!string.IsNullOrEmpty(skillName))
				yield return (source, skillName.ToRecordId());
		}
	}

	/// <summary>
	/// The record that names a granted skill: its buff record (<c>buffSkillName</c>) when it has one, else the skill itself.
	/// </summary>
	/// <returns><see langword="null"/> when the skill record is missing or has no name, like the game's hidden helper skills.</returns>
	private DBRecordCollection? DisplayedSkillRecord(RecordId skillId)
	{
		if (Database.GetRecordFromFile(skillId) is not { } skillRecord)
			return null;

		var buffId = skillRecord.GetString("buffSkillName", 0).ToRecordId();
		var shown = (RecordId.IsNullOrEmpty(buffId) ? null : Database.GetRecordFromFile(buffId)) ?? skillRecord;
		return string.IsNullOrEmpty(shown.GetString("skillDisplayName", 0)) ? null : shown;
	}

	/// <inheritdoc/>
	public GrantedSkill? GetSkillAtLevel(RecordId skillId, int level)
		=> this.GetGrantedSkill(new Item(), skillId, Math.Max(1, level), null);

	/// <summary>
	/// Describes one granted skill at a level: name, icon, activation, description and effects.
	/// A buff skill (<c>buffSkillName</c>) takes its name, icon, description and effects from its buff record.
	/// </summary>
	/// <returns><see langword="null"/> when the skill record is missing or has no name, like the game's hidden helper skills.</returns>
	private GrantedSkill? GetGrantedSkill(Item itm, RecordId skillId, int level, string? autoController)
	{
		if (Database.GetRecordFromFile(skillId) is not { } skillRecord)
			return null;

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

		if (!TranslationService.TryTranslateXTag("MenuLevel", out var levelFormat))
			levelFormat = "Level:   {0}";
		else
			levelFormat = ItemAttributeProvider.ConvertFormat(levelFormat);

		var activation = this.GetSkillActivationText(autoController);

		// Effects at the granted level: the skill's own parameters (energy cost, cooldown, radius...), then the buff's.
		var attributes = new List<string>();
		if (skillRecord.GetString("Class", 0).Equals("SKILL_SPAWNPET", noCase))
			this.ConvertPetStats(itm, skillRecord, attributes);
		else
		{
			this.GetAttributesFromRecord(itm, skillRecord, true, skillId, attributes, skillLevelIndex: level - 1);
			if (buffRecord is not null)
				this.GetAttributesFromRecord(itm, buffRecord, true, buffId, attributes, skillLevelIndex: level - 1);
		}

		// A skill that only casts another one (skillCastName, e.g. "Potion Blast" casts "Shadow Surge" on a potion):
		// the chance and the cast skill, with its effects at its own level. Its description stands in for a missing one.
		if (this.GetCastSkill(itm, skillRecord, level, levelFormat, attributes) is { Length: > 0 } castDescription && description.Length == 0)
			description = castDescription;

		return new GrantedSkill(
			skillId,
			name,
			level,
			Format(levelFormat, level),
			string.IsNullOrEmpty(icon) ? null : icon.ToRecordId(),
			string.IsNullOrEmpty(activation) ? null : activation,
			description,
			TrimEmptyLines(attributes)
		);
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
	/// <returns>The description of the cast skill; empty when there is none or no cast skill.</returns>
	private string[] GetCastSkill(Item itm, DBRecordCollection skillRecord, int level, string levelFormat, List<string> attributes)
	{
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

		if (castName is null)
		{
			if (chance > 0 && TranslationService.TryTranslateXTag("GlobalPercentChanceOfAllTag", out var chanceOfAll))
				attributes.Add($"{text}{Format(ItemAttributeProvider.ConvertFormat(chanceOfAll), chance)}");
		}
		else if (chance > 0 && TranslationService.TryTranslateXTag("ChanceOfTag", out var chanceFormat))
			attributes.Add($"{text}{Format(ItemAttributeProvider.ConvertFormat(chanceFormat), chance)}{name}{castName}");
		else
			attributes.Add($"{name}{castName}");

		attributes.Add($"{ItemStyle.Broken.TQColor().ColorTag()}{Format(levelFormat, castLevel)}");

		// On a kill, the cast skill can appear at the slain enemy instead of the player: an area effect lands there.
		if (skillRecord.GetString("Class", 0).Equals("Skill_PassiveOnTargetKilled", noCase)
			&& ValueAtLevel(skillRecord["skillCastOnTargetLocation"], level) > 0
			&& TranslationService.TryTranslateXTag("tqvSkillCastOnTargetLocation", out var onTarget))
			attributes.Add($"{text}{onTarget}");
		this.GetAttributesFromRecord(itm, castRecord, true, castId, attributes, skillLevelIndex: castLevel - 1);
		return this.GetSkillDescription(castRecord);
	}

	/// <summary>The value of a skill parameter at a level: one value per level, the last one beyond.</summary>
	private static float ValueAtLevel(Variable? variable, int level)
		=> variable is null || variable.NumberOfValues == 0
			? 0F
			: variable.GetSingle(Math.Clamp(level - 1, 0, variable.NumberOfValues - 1));

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
