using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Services;

/// <inheritdoc/>
public class CharacterStatsService(IDatabase database, IItemProvider itemProvider) : ICharacterStatsService
{
	private const string BuffSkillName = "buffSkillName";
	private const string GameEngine = """records\xpack\game\gameengine.dbr""";
	private const string BaseGameEngine = """records\game\gameengine.dbr""";
	private const string HealthManaRegen = """records\game\healthmanaregen.dbr""";
	private const string PlayerTemplate = """records\xpack\creatures\pc\malepc01.dbr""";

	private readonly IDatabase _database = database;
	private readonly IItemProvider _itemProvider = itemProvider;
	private GameBalance? _balance;

	/// <inheritdoc/>
	public AttributeSum GetGearAttributes(IEnumerable<Item> equipment)
	{
		var worn = equipment.Where(item => !item.IsInSecondaryWeaponSlot).ToList();
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
			BaseCastSpeed = Value(player, "characterSpellCastSpeed", defaults.BaseCastSpeed),
		};
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

	private static string Text(DBRecordCollection? record, string name, string fallback)
		=> record?.GetString(name, 0) is { Length: > 0 } text ? text : fallback;

	private static double Value(DBRecordCollection? record, string name, double fallback)
		=> record?[name] is { NumberOfValues: > 0 } variable
			? variable.DataType == VariableDataType.Float ? variable.GetSingle(0) : variable.GetInt32(0)
			: fallback;
}
