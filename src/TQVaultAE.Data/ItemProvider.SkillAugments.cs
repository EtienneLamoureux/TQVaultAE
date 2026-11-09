using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Data;

public partial class ItemProvider
{
	private const string AugmentSkillLevel = "augmentSkillLevel";
	private const string AugmentMasteryLevel = "augmentMasteryLevel";
	private const string AugmentAllLevel = "augmentAllLevel";

	/// <inheritdoc/>
	public IReadOnlyList<SkillAugment> GetSkillAugments(Item item)
	{
		using var modScope = this.ModScope(item);
		var augments = new List<SkillAugment>();

		// The item and its affixes, then each relic or charm at its number of pieces, and its completion bonus.
		(RecordId? Id, int Level)[] sources =
		[
			(item.BaseItemId, 1),
			(item.prefixID, 1),
			(item.suffixID, 1),
			(item.relicID, item.Var1),
			(item.RelicBonusId, 1),
			(item.relic2ID, item.Var2),
			(item.RelicBonus2Id, 1),
		];

		foreach (var (id, level) in sources)
		{
			if (!RecordId.IsNullOrEmpty(id))
				AddSkillAugments(augments, Database.GetRecordFromFile(id), level);
		}

		return augments;
	}

	/// <summary>
	/// Adds the skill bonuses of a record: <c>augmentSkillLevelN</c> with its <c>augmentSkillNameN</c>,
	/// <c>augmentMasteryLevelN</c> with its <c>augmentMasteryNameN</c>, and <c>augmentAllLevel</c>.
	/// </summary>
	/// <param name="level">For a relic, its number of pieces: the value of that rank is used (the last one past it).</param>
	internal static void AddSkillAugments(List<SkillAugment> augments, DBRecordCollection? record, int level)
	{
		if (record is null)
			return;

		foreach (var variable in record)
		{
			var name = variable.Name;
			if (variable.NumberOfValues == 0
				|| variable.DataType is not (VariableDataType.Integer or VariableDataType.Float)
				|| !name.StartsWith("augment", StringComparison.OrdinalIgnoreCase))
				continue;

			int levels = variable.GetInt32(Math.Clamp(level, 1, variable.NumberOfValues) - 1);
			if (levels == 0)
				continue;

			if (name.Equals(AugmentAllLevel, StringComparison.OrdinalIgnoreCase))
				augments.Add(new SkillAugment(SkillAugmentKind.All, null, levels));
			else if (name.StartsWith(AugmentSkillLevel, StringComparison.OrdinalIgnoreCase))
				AddNamed(SkillAugmentKind.Skill, "augmentSkillName" + name[AugmentSkillLevel.Length..]);
			else if (name.StartsWith(AugmentMasteryLevel, StringComparison.OrdinalIgnoreCase))
				// the game drops a mastery bonus whose augmentMasteryNameN is empty (see GetAugmentMasteryLevel).
				AddNamed(SkillAugmentKind.Mastery, "augmentMasteryName" + name[AugmentMasteryLevel.Length..]);

			void AddNamed(SkillAugmentKind kind, string targetVariable)
			{
				var target = record.GetString(targetVariable, 0);
				if (!string.IsNullOrEmpty(target))
					augments.Add(new SkillAugment(kind, target.ToRecordId(), levels));
			}
		}
	}
}
