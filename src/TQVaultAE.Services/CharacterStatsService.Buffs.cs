using System.Globalization;
using TQVaultAE.Application.Character;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Services;

/// <summary>The sources of the statistics panel's Buffs tab: one sum per item, set, skill, curse, shrine, scroll...</summary>
public partial class CharacterStatsService
{
	private const string SkillDisplayName = "skillDisplayName";
	private const string SkillIcon = "skillUpBitmapName";

	/// <summary>The icon of a source whose records have none (most quest rewards).</summary>
	private const string FallbackIcon = """InGameUI\icons\skills\buffgreenfup01.tex""";

	/// <summary>The fixed-item skill that casts a buff, from a shrine or a quest (<c>skillActiveDuration</c> -1 for the curses).</summary>
	private const string FixedItemBuffClass = "FixedItemSkill_Buff";

	/// <summary>The Tartarus arena's effects on the player: their records sit in this folder.</summary>
	private const string TartarusPlayerFolder = """\tartarusbuffs\playerdebuffs\""";

	private List<RecordId>? _curses;
	private List<(RecordId Buff, string Name, int Difficulty)>? _shrines;
	private List<RecordId>? _tartarus;
	private List<BuffScroll>? _scrolls;

	/// <summary>The pools read from the database are the active map's: a mod can add to them.</summary>
	private void ClearBuffPools()
	{
		this._curses = null;
		this._shrines = null;
		this._tartarus = null;
		this._scrolls = null;
	}

	/// <inheritdoc/>
	public IReadOnlyList<BuffSource> GetBuffSources(BuffSourceRequest request)
	{
		ArgumentNullException.ThrowIfNull(request);
		var sources = new List<BuffSource>(this.GearSources(request.Equipment, request.UseAlternate));

		// A modifier adds to the skill it modifies, which toggles both.
		var learned = this.LearnedSkills(request.Skills, request.Augments, request.Trees);
		foreach (var skill in learned.Where(s => s.ParentId is null))
		{
			var sum = new AttributeSum();
			sum.Add(skill.Sum);
			foreach (var modifier in learned.Where(s => skill.Id.Equals(s.ParentId)))
				sum.Add(modifier.Sum);

			var group = skill.Role == SkillStatRole.Passive ? BuffGroup.Passive : BuffGroup.Active;
			var potionBuff = this.PotionBuffSource(skill);
			// Potent Elixir adds nothing until a potion triggers its buff: only that buff is a source.
			if (potionBuff is null || HasCharacterBonus(sum))
				sources.Add(this.SkillSource("skill:", skill, group, sum));
			if (potionBuff is not null)
				sources.Add(potionBuff);
		}

		foreach (var skill in this.GrantedSkills(request.Equipment, request.UseAlternate))
			sources.Add(this.SkillSource("granted:", skill, skill.Role == SkillStatRole.Passive ? BuffGroup.Passive : BuffGroup.Active, skill.Sum));

		sources.AddRange(this.PetAuraSources(request));

		// The curses the save keeps start on; any other permanent buff it keeps too (a mod's), as a curse.
		var saved = request.SavedBuffs.ToHashSet();
		foreach (var curse in this.Curses().Union(saved))
			sources.Add(this.BuffRecordSource("curse:", curse, BuffGroup.Curse, saved.Contains(curse)));

		foreach (var shrine in this.ShrineBuffs(request.Difficulty))
			sources.Add(this.BuffRecordSource("shrine:", shrine, BuffGroup.Shrine, false));

		foreach (var effect in this.TartarusEffects())
			sources.Add(this.BuffRecordSource("tartarus:", effect, BuffGroup.Tartarus, false));

		if (request.Scroll is { } scroll && this._database.GetRecordFromFile(scroll)?.GetString("skillName", 0) is { Length: > 0 } scrollSkill
			&& this._database.GetRecordFromFile(scrollSkill.ToRecordId()) is { } skillRecord)
		{
			var skillId = scrollSkill.ToRecordId();
			var sum = this.SkillRecordAttributes(skillId, skillRecord, 1);
			sources.Add(new BuffSource
			{
				Id = "scroll:" + scroll.Normalized,
				Group = BuffGroup.Scroll,
				NameTag = this.NameTag(skillId, skillRecord),
				Icon = this.Icon(skillRecord) ?? this._database.GetRecordFromFile(scroll)?.GetString("bitmap", 0) ?? FallbackIcon,
				RecordId = skillId,
				Sum = sum,
			});
		}

		return sources;
	}

	/// <inheritdoc/>
	public IReadOnlyList<BuffScroll> GetBuffScrolls()
		=> this._scrolls ??= [.. this.RecordsOfClass("OneShot_Scroll", "OneShot_Scroll_Eternal")
			.Select(id => (Id: id, Record: this._database.GetRecordFromFile(id)))
			.Where(s => s.Record?.GetString("skillName", 0) is { Length: > 0 } skill
				&& this._database.GetRecordFromFile(skill.ToRecordId()) is { } record
				&& this.RoleOf(record) == SkillStatRole.Buff
				&& this.SkillRecordAttributes(skill.ToRecordId(), record, 1).Values.Count != 0)
			.Select(s => new BuffScroll(s.Id, s.Record!.GetString("description", 0) ?? s.Id.PrettyFileName, s.Record.GetString("bitmap", 0)))
			.OrderBy(s => s.Id.Normalized, StringComparer.Ordinal)];

	/// <summary>
	/// The equipped items with bonuses (not the weapons of the set not in hand, nor an empty slot), then the item sets they
	/// make: one gear source each, on.
	/// </summary>
	private List<BuffSource> GearSources(IEnumerable<Item> equipment, bool useAlternate)
	{
		var worn = equipment.Where(item => !item.IsInInactiveWeaponSlot(useAlternate)).ToList();
		var sources = new List<BuffSource>();
		for (int i = 0; i < worn.Count; i++)
		{
			var item = worn[i];
			var role = BuffSums.GearRoleOf(item);
			if (role == BuffGearRole.None)
				continue;

			sources.Add(new BuffSource
			{
				Id = string.Create(CultureInfo.InvariantCulture, $"item:{i}:{item.BaseItemId?.Normalized}"),
				Group = BuffGroup.Gear,
				GearRole = role,
				Item = item,
				DefaultOn = true,
				Sum = this._itemProvider.GetAttributeSums(item),
			});
		}

		foreach (var set in this._itemProvider.GetSetBonusSources(worn))
			sources.Add(new BuffSource
			{
				Id = "set:" + set.SetId.Normalized,
				Group = BuffGroup.Gear,
				GearRole = BuffGearRole.Other,
				NameTag = set.NameTag,
				RecordId = set.SetId,
				Origins = set.Pieces,
				DefaultOn = true,
				Sum = set.Sum,
			});

		return sources;
	}

	/// <summary>A learned or granted skill as a source: passives start on, the others off.</summary>
	private BuffSource SkillSource(string prefix, CountedSkill skill, BuffGroup group, AttributeSum sum)
	{
		var buff = this.BuffRecord(skill.Record);
		return new BuffSource
		{
			Id = prefix + skill.Id.Normalized,
			Group = group,
			NameTag = this.NameTag(skill.Id, skill.Record),
			Icon = this.Icon(skill.Record) ?? this.Icon(buff) ?? FallbackIcon,
			RecordId = skill.Id,
			Level = skill.Level,
			// A toggled buff an item casts when worn runs as long as it is worn.
			DefaultOn = group == BuffGroup.Passive || skill.OnEquip,
			OnEquip = skill.OnEquip,
			Origins = skill.Origins,
			Sum = sum,
		};
	}

	/// <summary>A buff record (a curse, a shrine's buff, a Tartarus effect) as a source, read at level 1.</summary>
	private BuffSource BuffRecordSource(string prefix, RecordId id, BuffGroup group, bool on)
	{
		var record = this._database.GetRecordFromFile(id);
		return new BuffSource
		{
			Id = prefix + id.Normalized,
			Group = group,
			NameTag = this.NameTag(id, record),
			Icon = this.Icon(record) ?? FallbackIcon,
			RecordId = id,
			DefaultOn = on,
			Sum = this._itemProvider.GetRecordAttributes(id, 1),
		};
	}

	/// <summary>
	/// True when the item casts the skill by itself once worn: the granting record's auto controller
	/// (<c>itemSkillAutoController</c>) fires on equip (<c>triggerType</c> OnEquip). Those skills are toggled buffs: the
	/// item turns them on and they stay on.
	/// </summary>
	private bool CastsOnEquip(Item item, RecordId skillId)
	{
		using var modScope = this._itemMods.ScopeOrNone(item);
		return this._itemProvider.GetGrantedSkillSources(item)
			.Where(s => s.SkillId.Equals(skillId))
			.Select(s => this.Record(s.Source.GetString("itemSkillAutoController", 0))?.GetString("triggerType", 0))
			.Any(trigger => string.Equals(trigger, "OnEquip", StringComparison.OrdinalIgnoreCase));
	}

	/// <summary>The record of the buff a skill casts (<c>buffSkillName</c>), null when it casts none.</summary>
	private DBRecordCollection? BuffRecord(DBRecordCollection? skill)
		=> this.Record(skill?.GetString(BuffSkillName, 0));

	/// <summary>The tag of a skill's name, else the name of its buff, else its file name.</summary>
	private string NameTag(RecordId id, DBRecordCollection? record)
		=> record?.GetString(SkillDisplayName, 0) is { Length: > 0 } tag ? tag
			: this.BuffRecord(record)?.GetString(SkillDisplayName, 0) is { Length: > 0 } buffTag ? buffTag
			: id.PrettyFileName;

	private string? Icon(DBRecordCollection? record)
		=> record?.GetString(SkillIcon, 0) is { Length: > 0 } icon ? icon : null;

	/// <summary>
	/// The temple curses: the buffs of the fixed-item skills that never end (<c>skillActiveDuration</c> -1), the only
	/// permanent ones of the game.
	/// </summary>
	private List<RecordId> Curses()
		=> this._curses ??= [.. this.FixedItemBuffs().Where(b => b.Duration < 0).Select(b => b.Buff).Distinct().Order()];

	/// <summary>
	/// The world shrine buffs and quest blessings for a difficulty: one per buff name, the variant of that difficulty
	/// (records named <c>_epic</c>, <c>_legendary</c>, the others Normal), else of the closest below; buffs that add
	/// nothing left out.
	/// </summary>
	private IEnumerable<RecordId> ShrineBuffs(int difficulty)
	{
		this._shrines ??= [.. this.FixedItemBuffs()
			.Where(b => b.Duration > 0 && !b.Buff.Normalized.Contains("\\sandbox\\", StringComparison.OrdinalIgnoreCase))
			.Select(b => b.Buff)
			.Distinct()
			.Where(id => this._itemProvider.GetRecordAttributes(id, 1).Values.Count != 0)
			.Select(id => (Buff: id, Name: this.NameTag(id, this._database.GetRecordFromFile(id)), Difficulty: ShrineDifficulty(id)))];

		// The variant of the difficulty, else of the closest one below, else the lowest; among variants of one difficulty
		// (regions) the first by record.
		return this._shrines
			.GroupBy(s => s.Name, StringComparer.OrdinalIgnoreCase)
			.Select(g => g
				.OrderBy(s => s.Difficulty <= difficulty ? difficulty - s.Difficulty : 10 + s.Difficulty)
				.ThenBy(s => s.Buff.Normalized, StringComparer.Ordinal)
				.First().Buff)
			.OrderBy(id => id.Normalized, StringComparer.Ordinal);
	}

	private static int ShrineDifficulty(RecordId id)
		=> id.Normalized.Contains("_legendary", StringComparison.OrdinalIgnoreCase) ? 2
			: id.Normalized.Contains("_epic", StringComparison.OrdinalIgnoreCase) ? 1
			: 0;

	/// <summary>The buffs the fixed-item skills cast (shrines, quests, curses), with their duration.</summary>
	private IEnumerable<(RecordId Buff, double Duration)> FixedItemBuffs()
		=> this.RecordsOfClass(FixedItemBuffClass)
			.Select(this._database.GetRecordFromFile)
			.Where(r => r?.GetString(BuffSkillName, 0) is { Length: > 0 })
			.Select(r => (r!.GetString(BuffSkillName, 0)!.ToRecordId(), OptionalValue(r, "skillActiveDuration", 0)));

	/// <summary>The Tartarus arena's effects on the player.</summary>
	private List<RecordId> TartarusEffects()
		=> this._tartarus ??= [.. this.AllRecordIds()
			.Where(id => id.Normalized.Contains(TartarusPlayerFolder, StringComparison.OrdinalIgnoreCase))
			.Where(id => this._itemProvider.GetRecordAttributes(id, 1).Values.Count != 0)
			.Order()];

	/// <summary>The records of the loaded databases (the custom map's, Immortal Throne's, the base game's), each once.</summary>
	private IEnumerable<RecordId> AllRecordIds()
		=> this.ArzFiles().SelectMany(a => a.RecordInfo.Keys).Distinct();

	/// <summary>The records of some classes in the loaded databases, each once.</summary>
	private IEnumerable<RecordId> RecordsOfClass(params string[] classes)
		=> this.ArzFiles()
			.SelectMany(a => a.RecordInfo.Where(r => classes.Contains(r.Value.Class, StringComparer.OrdinalIgnoreCase)).Select(r => r.Key))
			.Distinct();

	private IEnumerable<ArzFile> ArzFiles()
		=> new[] { this._database.ArzFileMod, this._database.ArzFileIT, this._database.ArzFile }.OfType<ArzFile>().Distinct();
}
