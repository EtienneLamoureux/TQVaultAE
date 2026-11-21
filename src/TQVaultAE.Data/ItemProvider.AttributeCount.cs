using System.Collections.Frozen;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Data;

public partial class ItemProvider
{
	/// <summary>
	/// The base values of an item, not counted as attributes: base attack speed, base physical damage, armor, block,
	/// pierce ratio and the global chances.
	/// </summary>
	private static readonly FrozenSet<string> uncountedAttributes = new[]
	{
		"characterBaseAttackSpeedTag",
		"offensivePhysicalMin",
		"offensivePhysicalMax",
		"defensiveProtection",
		"defensiveBlock",
		"blockRecoveryTime",
		"offensiveGlobalChance",
		"retaliationGlobalChance",
		"offensivePierceRatioMin",
	}.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

	/// <summary>
	/// The "totalAttCount" of the item cost equations, as the game counts it: the attributes of the base item, prefix and
	/// suffix records; socketed relics and their bonuses never count. Call it inside the item's mod scope.
	/// </summary>
	internal int GetTotalAttributeCount(Item itm)
		=> CountAttributes(Database.GetRecordFromFile(itm.BaseItemId))
			+ (itm.prefixInfo is null ? 0 : CountAttributes(Database.GetRecordFromFile(itm.prefixID)))
			+ (itm.suffixInfo is null ? 0 : CountAttributes(Database.GetRecordFromFile(itm.suffixID)));

	/// <summary>
	/// Counts one per attribute group of a record (<see cref="ItemAttributesData.EffectGroupOf"/>), and one per skill bonus
	/// whatever its level. Base values, chances and durations alone are not counted.
	/// </summary>
	internal int CountAttributes(DBRecordCollection? record)
	{
		if (record is null)
			return 0;

		HashSet<string> groups = new(StringComparer.Ordinal);
		int skillBonuses = 0;
		foreach (Variable variable in record)
		{
			string name = variable.Name;
			if (FilterValue(variable, false) || FilterKey(name) || FilterRequirements(name))
				continue;

			if (name.StartsWith(AugmentSkillLevel, StringComparison.OrdinalIgnoreCase)
				|| name.StartsWith(AugmentMasteryLevel, StringComparison.OrdinalIgnoreCase)
				|| name.Equals(AugmentAllLevel, StringComparison.OrdinalIgnoreCase))
			{
				skillBonuses++;
				continue;
			}

			if (uncountedAttributes.Contains(name)
				|| name.Contains("Chance", StringComparison.OrdinalIgnoreCase)
				|| name.Contains("Duration", StringComparison.OrdinalIgnoreCase))
				continue;

			groups.Add(ItemAttributesData.EffectGroupOf(ItemAttributeProvider.GetAttributeData(name), name));
		}

		return groups.Count + skillBonuses;
	}
}
