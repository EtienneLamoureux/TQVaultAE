using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Services;

/// <inheritdoc/>
public class CharacterStatsService(IDatabase database, IItemProvider itemProvider) : ICharacterStatsService, IGameDataCache
{
	private const string BuffSkillName = "buffSkillName";
	private const string GameEngine = """records\xpack\game\gameengine.dbr""";
	private const string BaseGameEngine = """records\game\gameengine.dbr""";
	private const string HealthManaRegen = """records\game\healthmanaregen.dbr""";
	private const string PlayerTemplate = """records\xpack\creatures\pc\malepc01.dbr""";

	private readonly IDatabase _database = database;
	private readonly IItemProvider _itemProvider = itemProvider;
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
	}

	/// <inheritdoc/>
	public AttributeSum GetGearAttributes(IEnumerable<Item> equipment, bool useAlternate)
	{
		var worn = equipment.Where(item => !item.IsInInactiveWeaponSlot(useAlternate)).ToList();
		var sum = new AttributeSum();
		foreach (var item in worn)
			sum.Add(this._itemProvider.GetAttributeSums(item));

		sum.Add(this._itemProvider.GetSetBonuses(worn));
		return sum;
	}

	/// <inheritdoc/>
	public AttributeSum GetSkillAttributes(IEnumerable<SkillRecord> skills, IReadOnlyList<SkillAugment> augments, IReadOnlyList<MasteryTree> trees, bool includeBuffs)
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
			roles[id] = SkillStatRules.RoleOf(record?.RecordType);
		}

		var sum = new AttributeSum();
		foreach (var (id, savedLevel) in learned)
		{
			var tree = trees.FirstOrDefault(t => t.Contains(id));
			var parentId = tree?.Skills.FirstOrDefault(s => s.Id == id)?.ParentId;
			SkillStatRole? parentRole = parentId is not null && roles.TryGetValue(parentId, out var p) ? p : null;
			var role = SkillStatRules.Resolve(roles[id], parentRole);
			if (!SkillStatRules.Counts(role, includeBuffs))
				continue;

			int level = SkillStatRules.EffectiveLevel(savedLevel, id, tree, augments);

			// A buff skill keeps its bonuses in the buff it casts (buffSkillName), at the skill's level.
			var buff = records[id]?.GetString(BuffSkillName, 0);
			var source = string.IsNullOrEmpty(buff) ? id : buff.ToRecordId();
			sum.Add(this._itemProvider.GetRecordAttributes(source, level));
		}

		return sum;
	}

	/// <inheritdoc/>
	public GameBalance GetGameBalance()
		=> this._balance ??= this.ReadGameBalance();

	private GameBalance ReadGameBalance()
	{
		var defaults = new GameBalance();
		var engine = this.Record(GameEngine) ?? this.Record(BaseGameEngine);
		var combat = this.Record(engine?.GetString("defaultCombatManagerRecord", 0));
		var pak = this.Record(engine?.GetString("playerAttributePak", 0));
		var regen = this.Record(HealthManaRegen);
		var player = this.Record(PlayerTemplate);

		return new GameBalance
		{
			Engine = Numbers(engine),
			Penalties = Numbers(pak),
			OffensiveAbilityEquation = Text(combat, "offensiveAbilityEquation", defaults.OffensiveAbilityEquation),
			DefensiveAbilityEquation = Text(combat, "defensiveAbilityEquation", defaults.DefensiveAbilityEquation),
			LifeRegenEquation = Text(regen, "lifeRegenEquation", defaults.LifeRegenEquation),
			ManaRegenEquation = Text(regen, "manaRegenEquation", defaults.ManaRegenEquation),
			BaseLifeRegen = Value(player, "characterLifeRegen", defaults.BaseLifeRegen),
			BaseManaRegen = Value(player, "characterManaRegen", defaults.BaseManaRegen),
			BaseRunSpeed = Value(player, "characterRunSpeed", defaults.BaseRunSpeed),
			BaseAttackSpeed = Value(player, "characterAttackSpeed", defaults.BaseAttackSpeed),
			HandHitDamageMin = Value(player, "handHitDamageMin", defaults.HandHitDamageMin),
			HandHitDamageMax = Value(player, "handHitDamageMax", defaults.HandHitDamageMax),
			CombatEquations = Equations(combat),
			BaseCastSpeed = Value(player, "characterSpellCastSpeed", defaults.BaseCastSpeed),
		};
	}

	/// <inheritdoc/>
	public OffenseGear GetOffenseGear(IEnumerable<Item> equipment, bool useAlternate)
	{
		var worn = equipment.Where(item => !item.IsInInactiveWeaponSlot(useAlternate)).ToList();
		var weapons = worn
			// An empty weapon slot can be saved as an item without record: it is no weapon (bare hands then).
			.Where(item => item.IsInWeaponSlot && !string.IsNullOrEmpty(item.ItemClass)
				&& !item.ItemClass.Equals("WeaponArmor_Shield", StringComparison.OrdinalIgnoreCase))
			.Select(this._itemProvider.GetAttributeSums)
			.ToList();

		var other = new AttributeSum();
		foreach (var item in worn.Where(item => !item.IsInWeaponSlot))
			other.Add(this._itemProvider.GetAttributeSums(item));

		other.Add(this._itemProvider.GetSetBonuses(worn));
		return new OffenseGear(weapons, other);
	}

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

				animations.Add((header.Frames, header.FrameRate, Value(template, $"{prefix}AttackAnimSpeed{n}", 1)));
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

	private static string Text(DBRecordCollection? record, string name, string fallback)
		=> record?.GetString(name, 0) is { Length: > 0 } text ? text : fallback;

	private static double Value(DBRecordCollection? record, string name, double fallback)
		=> record?[name] is { NumberOfValues: > 0 } variable
			? variable.DataType == VariableDataType.Float ? variable.GetSingle(0) : variable.GetInt32(0)
			: fallback;
}
