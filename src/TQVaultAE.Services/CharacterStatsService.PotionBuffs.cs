using TQVaultAE.Application.Character;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Services;

/// <summary>The buffs a health or energy potion triggers (Neidan): sources of their own in the Buffs tab.</summary>
public partial class CharacterStatsService
{
	/// <summary>
	/// The buff a potion triggers for a learned skill of <see cref="SkillStatRules.PotionTriggerClass"/>: the skill cast
	/// (<c>skillCastName</c> at <c>skillCastLevel</c>), with its chance (<c>skillCastChance</c>) and duration
	/// (<c>skillCastDuration</c>) at the skill's level; off. Potent Elixir's resistances, Antidote's, Convergence's potion
	/// buffs, and Inner Equilibrium's maluses, which take its bonuses back. Null for any other skill or a cast that adds
	/// nothing.
	/// </summary>
	private BuffSource? PotionBuffSource(CountedSkill skill)
	{
		var record = skill.Record;
		if (record is null || !record.RecordType.Equals(SkillStatRules.PotionTriggerClass, StringComparison.OrdinalIgnoreCase))
			return null;

		var castName = AtLevel(record, "skillCastName", skill.Level) is { } name ? record["skillCastName"]!.GetString(name) : null;
		if (string.IsNullOrEmpty(castName) || this._database.GetRecordFromFile(castName.ToRecordId()) is not { } cast)
			return null;

		var castId = castName.ToRecordId();
		int castLevel = Math.Max(1, (int)NumberAtLevel(record, "skillCastLevel", skill.Level));
		var sum = this._itemProvider.GetRecordAttributes(castId, castLevel);
		if (sum.Values.Count == 0)
			return null;

		return new BuffSource
		{
			Id = "potion:" + skill.Id.Normalized,
			Group = BuffGroup.Active,
			NameTag = this.NameTag(skill.Id, record),
			Icon = this.Icon(record) ?? this.Icon(cast) ?? FallbackIcon,
			RecordId = castId,
			Level = castLevel,
			PotionTrigger = (NumberAtLevel(record, "skillCastChance", skill.Level), NumberAtLevel(record, "skillCastDuration", skill.Level)),
			Sum = sum,
		};
	}

	/// <summary>
	/// The skill variables that are bonuses of the character (recharge and energy cost reductions, projectile speed,
	/// duration), the game's equipment skill attributes; every other <c>skill...</c> variable is a parameter of the skill
	/// itself (<c>skillCastChance</c>, <c>skillMaxLevel</c>...).
	/// </summary>
	private static readonly HashSet<string> SkillBonusVariables = new(StringComparer.OrdinalIgnoreCase)
	{
		"skillCooldownReduction", "skillCooldownReductionModifier", "skillManaCostReduction", "skillManaCostReductionModifier",
		"skillProjectileSpeedModifier", "skillDurationModifier",
	};

	/// <summary>True when a skill's sum holds a bonus of the character, not only the skill's own parameters.</summary>
	private static bool HasCharacterBonus(AttributeSum sum)
		=> sum.Values.Any(v => v.Value != 0
			&& (!v.Key.StartsWith("skill", StringComparison.OrdinalIgnoreCase) || SkillBonusVariables.Contains(v.Key)));

	/// <summary>The index of a variable's value at a level (its last value past the end), null without the variable.</summary>
	private static int? AtLevel(DBRecordCollection record, string name, int level)
		=> record[name] is { NumberOfValues: > 0 } variable ? Math.Clamp(level, 1, variable.NumberOfValues) - 1 : null;

	/// <summary>A variable's number at a level (its last value past the end), 0 without the variable.</summary>
	private static float NumberAtLevel(DBRecordCollection record, string name, int level)
		=> AtLevel(record, name, level) is { } index
			? Convert.ToSingle(record[name]![index], System.Globalization.CultureInfo.InvariantCulture)
			: 0F;
}
