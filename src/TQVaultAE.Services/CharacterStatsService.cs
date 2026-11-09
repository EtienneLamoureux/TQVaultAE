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

	private readonly IDatabase _database = database;
	private readonly IItemProvider _itemProvider = itemProvider;

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
}
