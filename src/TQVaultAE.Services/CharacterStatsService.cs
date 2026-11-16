using Microsoft.Extensions.Logging;
using TQVaultAE.Application.Character;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Services;

/// <inheritdoc/>
public partial class CharacterStatsService(IDatabase database, IItemProvider itemProvider, ILogger<CharacterStatsService> log
	, IItemModService itemMods) : ICharacterStatsService, IGameDataCache
{
	private const string BuffSkillName = "buffSkillName";
	private const string GameEngine = """records\xpack\game\gameengine.dbr""";
	private const string BaseGameEngine = """records\game\gameengine.dbr""";
	private const string HealthManaRegen = """records\game\healthmanaregen.dbr""";
	private const string PlayerTemplate = """records\xpack\creatures\pc\malepc01.dbr""";

	private readonly IDatabase _database = database;
	private readonly IItemProvider _itemProvider = itemProvider;
	private readonly ILogger _log = log;

	/// <summary>Reads a granted skill in the map of the item granting it.</summary>
	private readonly IItemModService _itemMods = itemMods;
	private const string FemalePlayerTemplate = """records\xpack\creatures\pc\femalepc01.dbr""";

	private const string AnimationTableName = "charAnimationTableName";

	/// <summary>The player templates list up to 3 attack animations per stance.</summary>
	private const int MaxAttackAnimations = 3;

	private GameBalance? _balance;
	private Dictionary<WeaponStance, double>? _maleAnimations;
	private Dictionary<WeaponStance, double>? _femaleAnimations;

	/// <summary>The caps, equations and animations are the active map's: a mod can change them.</summary>
	public void ClearGameDataCache()
	{
		this._balance = null;
		this._maleAnimations = null;
		this._femaleAnimations = null;
		this.ClearBuffPools();
	}

	/// <inheritdoc/>
	public AttributeSum GetGearAttributes(IEnumerable<Item> equipment, bool useAlternate)
	{
		var sum = new AttributeSum();
		foreach (var source in this.GearSources(equipment, useAlternate))
			sum.Add(source.Sum);

		return sum;
	}

	/// <inheritdoc/>
	public AttributeSum GetSkillAttributes(IEnumerable<SkillRecord> skills, IReadOnlyList<SkillAugment> augments, IReadOnlyList<MasteryTree> trees, bool includeBuffs)
	{
		var sum = new AttributeSum();
		foreach (var skill in this.LearnedSkills(skills, augments, trees).Where(s => SkillStatRules.Counts(s.Role, includeBuffs)))
			sum.Add(skill.Sum);

		return sum;
	}

	/// <inheritdoc/>
	public AttributeSum GetItemSkillAttributes(IEnumerable<Item> equipment, bool useAlternate, bool includeBuffs)
	{
		var sum = new AttributeSum();
		foreach (var skill in this.GrantedSkills(equipment, useAlternate).Where(s => SkillStatRules.Counts(s.Role, includeBuffs)))
			sum.Add(skill.Sum);

		return sum;
	}

	/// <summary>A skill that adds to the character window: its record, its role resolved, its level and its sum there.</summary>
	/// <param name="ParentId">The skill a modifier modifies, null for any other skill.</param>
	/// <param name="Origins">The items granting it, none for a learned skill.</param>
	private sealed record CountedSkill(RecordId Id, DBRecordCollection? Record, RecordId? ParentId, SkillStatRole Role, int Level, AttributeSum Sum, IReadOnlyList<Item> Origins);

	/// <summary>
	/// The learned skills that add to the character window (passives, masteries, buffs, modifiers of those), each at its
	/// level plus the levels the equipped items add.
	/// </summary>
	private List<CountedSkill> LearnedSkills(IEnumerable<SkillRecord> skills, IReadOnlyList<SkillAugment> augments, IReadOnlyList<MasteryTree> trees)
	{
		var learned = skills
			.Where(s => s.skillLevel > 0 && !string.IsNullOrEmpty(s.skillName))
			.Select(s => (Id: s.skillName.ToRecordId(), s.skillLevel))
			.ToList();

		var records = new Dictionary<RecordId, DBRecordCollection?>();
		var roles = new Dictionary<RecordId, SkillStatRole>();
		foreach (var (id, _) in learned)
		{
			var record = this._database.GetRecordFromFile(id);
			records[id] = record;
			roles[id] = this.RoleOf(record);
		}

		var counted = new List<CountedSkill>();
		foreach (var (id, savedLevel) in learned)
		{
			var tree = trees.FirstOrDefault(t => t.Contains(id));
			var parentId = tree?.Skills.FirstOrDefault(s => s.Id == id)?.ParentId;
			SkillStatRole? parentRole = parentId is not null && roles.TryGetValue(parentId, out var p) ? p : null;
			var role = SkillStatRules.Resolve(roles[id], parentRole);
			if (role is SkillStatRole.None)
				continue;

			int level = SkillStatRules.EffectiveLevel(savedLevel, id, tree, augments);
			counted.Add(new CountedSkill(id, records[id], roles[id] == SkillStatRole.Modifier ? parentId : null, role, level,
				this.SkillRecordAttributes(id, records[id], level), []));
		}

		return counted;
	}

	/// <summary>
	/// The skills the equipped items grant (not the weapons of the set not in hand) that add to the character window,
	/// each once at the level an item grants (<see cref="SkillStatRules.ItemSkills"/>).
	/// </summary>
	private List<CountedSkill> GrantedSkills(IEnumerable<Item> equipment, bool useAlternate)
	{
		var granted = equipment
			.Where(item => !item.IsInInactiveWeaponSlot(useAlternate))
			.SelectMany(item => this._itemProvider.GetGrantedSkillLevels(item).Select(s => (Item: item, s.SkillId, s.Level)))
			.ToList();

		var counted = new List<CountedSkill>();
		foreach (var (id, level) in SkillStatRules.ItemSkills(granted.Select(g => (g.SkillId, g.Level))))
		{
			// The skill and its buff are read in the map of the item granting it at that level, like the item itself: a
			// mod's skill exists only in that mod's data.
			var source = granted.First(g => g.SkillId.Equals(id) && g.Level == level).Item;
			using var modScope = this._itemMods.ScopeOrNone(source);
			var record = this._database.GetRecordFromFile(id);

			// A granted skill modifies nothing of the character's: a modifier never counts.
			var role = SkillStatRules.Resolve(this.RoleOf(record), null);
			if (role is SkillStatRole.None)
				continue;

			var origins = granted.Where(g => g.SkillId.Equals(id)).Select(g => g.Item).Distinct().ToList();
			counted.Add(new CountedSkill(id, record, null, role, level, this.SkillRecordAttributes(id, record, level), origins));
		}

		return counted;
	}

	/// <summary>The role of a skill from its record and the record of the buff it casts (<c>buffSkillName</c>).</summary>
	private SkillStatRole RoleOf(DBRecordCollection? record)
	{
		var buff = record?.GetString(BuffSkillName, 0);
		var buffClass = string.IsNullOrEmpty(buff) ? null : this._database.GetRecordFromFile(buff.ToRecordId())?.RecordType;
		return SkillStatRules.RoleOf(record?.RecordType, buffClass);
	}

	/// <summary>The bonuses of a skill at a level: a buff skill keeps them in the buff it casts (<c>buffSkillName</c>).</summary>
	private AttributeSum SkillRecordAttributes(RecordId id, DBRecordCollection? record, int level)
	{
		var buff = record?.GetString(BuffSkillName, 0);
		var source = string.IsNullOrEmpty(buff) ? id : buff.ToRecordId();
		return this._itemProvider.GetRecordAttributes(source, level);
	}

	/// <inheritdoc/>
	public GameBalance GetGameBalance()
		=> this._balance ??= this.ReadGameBalance();

	private GameBalance ReadGameBalance()
	{
		var defaults = new GameBalance();
		var engine = this.Record(GameEngine) ?? this.Record(BaseGameEngine);
		string? combatId = engine?.GetString("defaultCombatManagerRecord", 0);
		var combat = this.Record(combatId);
		string? pakId = engine?.GetString("playerAttributePak", 0);
		var pak = this.Record(pakId);
		var regen = this.Record(HealthManaRegen);
		var player = this.Record(PlayerTemplate);

		this.WarnIfMissing(engine, GameEngine, "the caps default to 80");
		this.WarnIfMissing(pak, pakId ?? "playerAttributePak", "no difficulty penalty");

		var balance = new GameBalance
		{
			Engine = Numbers(engine),
			Penalties = Numbers(pak),
			OffensiveAbilityEquation = this.Text(combat, combatId, "offensiveAbilityEquation", defaults.OffensiveAbilityEquation),
			DefensiveAbilityEquation = this.Text(combat, combatId, "defensiveAbilityEquation", defaults.DefensiveAbilityEquation),
			LifeRegenEquation = this.Text(regen, HealthManaRegen, "lifeRegenEquation", defaults.LifeRegenEquation),
			ManaRegenEquation = this.Text(regen, HealthManaRegen, "manaRegenEquation", defaults.ManaRegenEquation),
			BaseLifeRegen = this.Value(player, PlayerTemplate, "characterLifeRegen", defaults.BaseLifeRegen),
			BaseManaRegen = this.Value(player, PlayerTemplate, "characterManaRegen", defaults.BaseManaRegen),
			BaseRunSpeed = this.Value(player, PlayerTemplate, "characterRunSpeed", defaults.BaseRunSpeed),
			BaseAttackSpeed = this.Value(player, PlayerTemplate, "characterAttackSpeed", defaults.BaseAttackSpeed),
			HandHitDamageMin = this.Value(player, PlayerTemplate, "handHitDamageMin", defaults.HandHitDamageMin),
			HandHitDamageMax = this.Value(player, PlayerTemplate, "handHitDamageMax", defaults.HandHitDamageMax),
			CombatEquations = Equations(combat),
			BaseCastSpeed = this.Value(player, PlayerTemplate, "characterSpellCastSpeed", defaults.BaseCastSpeed),
		};

		foreach (var name in GameBalance.DamageEquationNames.Where(n => !balance.CombatEquations.TryGetValue(n, out var e) || string.IsNullOrWhiteSpace(e)))
			this.WarnFallback(combatId, name, balance.DamageEquation(name));

		return balance;
	}

	/// <summary>Logs a record the character window needs that the game data lacks.</summary>
	private void WarnIfMissing(DBRecordCollection? record, string id, string consequence)
	{
		if (record is null)
			this._log.LogWarning("Game data: record {Record} not found, {Consequence}: the character window may be wrong for this map", id, consequence);
	}

	/// <summary>Logs a value of the character window taken from the original game because the game data lacks it.</summary>
	private void WarnFallback(string? recordId, string name, object fallback)
		=> this._log.LogWarning("Game data: {Field} not found in {Record}, the original game's value is used ({Fallback}): the character window may be wrong for this map"
			, name, recordId ?? "(no record)", fallback);

	/// <inheritdoc/>
	public OffenseGear GetOffenseGear(IEnumerable<Item> equipment, bool useAlternate)
		// The game adds the flat damage of the armor and jewelry to every attack, and of the weapon in hand to its own; a
		// shield is neither: its flat damage never reaches the attack (its % modifiers do, with every item's).
		=> BuffSums.Offense(this.GearSources(equipment, useAlternate), _ => true);

	/// <inheritdoc/>
	public WeaponStance GetWeaponStance(IEnumerable<Item> equipment, bool useAlternate)
		=> WeaponStances.FromWeapons(equipment
			.Where(item => item.IsInWeaponSlot && !item.IsInInactiveWeaponSlot(useAlternate))
			.Select(item => item.ItemClass));

	/// <inheritdoc/>
	public IReadOnlyDictionary<WeaponStance, double> GetAttackAnimationDurations(bool female)
	{
		var cache = female ? this._femaleAnimations : this._maleAnimations;
		if (cache is not null)
			return cache;

		// The game loads a character's animations from the table its record names (charAnimationTableName, for the
		// players records\creature\pc\anm\anm_*.dbr), whose attack animations and speeds differ from the record's own.
		var template = this.Record(female ? FemalePlayerTemplate : PlayerTemplate);
		if (this.Record(template?.GetString(AnimationTableName, 0)) is { } table)
			template = table;

		var durations = new Dictionary<WeaponStance, double>();
		foreach (var stance in Enum.GetValues<WeaponStance>())
		{
			var prefix = WeaponStances.RecordPrefix(stance);
			var animations = new List<(int Frames, int FrameRate, double Speed)>();
			for (int n = 1; n <= MaxAttackAnimations; n++)
			{
				var file = template?.GetString($"{prefix}AttackAnim{n}", 0);
				if (string.IsNullOrEmpty(file) || this._database.LoadResource(file.ToRecordId()) is not { } data
					|| WeaponStances.ReadAnimationHeader(data) is not { } header)
					continue;

				animations.Add((header.Frames, header.FrameRate, OptionalValue(template, $"{prefix}AttackAnimSpeed{n}", 1)));
			}

			if (WeaponStances.MeanDuration(animations) is var mean and > 0)
				durations[stance] = mean;
		}

		if (female)
			this._femaleAnimations = durations;
		else
			this._maleAnimations = durations;

		return durations;
	}

	private DBRecordCollection? Record(string? id)
		=> string.IsNullOrEmpty(id) ? null : this._database.GetRecordFromFile(id.ToRecordId());

	/// <summary>The numeric variables of a record with all their values.</summary>
	private static Dictionary<string, IReadOnlyList<double>> Numbers(DBRecordCollection? record)
	{
		var numbers = new Dictionary<string, IReadOnlyList<double>>(StringComparer.OrdinalIgnoreCase);
		if (record is null)
			return numbers;

		foreach (var variable in record)
		{
			if (variable.NumberOfValues == 0 || variable.DataType is not (VariableDataType.Integer or VariableDataType.Float))
				continue;

			numbers[variable.Name] = [.. Enumerable.Range(0, variable.NumberOfValues)
				.Select(i => variable.DataType == VariableDataType.Float ? (double)variable.GetSingle(i) : variable.GetInt32(i))];
		}

		return numbers;
	}

	/// <summary>The string fields of a record whose name ends with <c>Equation</c>.</summary>
	private static Dictionary<string, string> Equations(DBRecordCollection? record)
	{
		var equations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		if (record is null)
			return equations;

		foreach (var variable in record)
		{
			if (variable.DataType == VariableDataType.StringVar && variable.NumberOfValues > 0
				&& variable.Name.EndsWith("Equation", StringComparison.OrdinalIgnoreCase))
				equations[variable.Name] = variable.GetString(0);
		}

		return equations;
	}

	/// <summary>A text field of a record, else <paramref name="fallback"/> with a warning.</summary>
	private string Text(DBRecordCollection? record, string? recordId, string name, string fallback)
	{
		if (record?.GetString(name, 0) is { Length: > 0 } text)
			return text;

		this.WarnFallback(recordId, name, fallback);
		return fallback;
	}

	/// <summary>An optional numeric field of a record, else <paramref name="fallback"/>, its usual value: no warning.</summary>
	private static double OptionalValue(DBRecordCollection? record, string name, double fallback)
		=> record?[name] is { NumberOfValues: > 0 } variable
			? variable.DataType == VariableDataType.Float ? variable.GetSingle(0) : variable.GetInt32(0)
			: fallback;

	/// <summary>A numeric field of a record, else <paramref name="fallback"/> with a warning.</summary>
	private double Value(DBRecordCollection? record, string recordId, string name, double fallback)
	{
		if (record?[name] is { NumberOfValues: > 0 } variable)
			return variable.DataType == VariableDataType.Float ? variable.GetSingle(0) : variable.GetInt32(0);

		this.WarnFallback(recordId, name, fallback);
		return fallback;
	}
}
