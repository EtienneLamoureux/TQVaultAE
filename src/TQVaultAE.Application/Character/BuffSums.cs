using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Character;

/// <summary>
/// Adds up the buff sources that are on into what the statistics read: the gear (items and item sets) and the skills
/// (skills, granted skills, curses, shrines...) apart, as <see cref="CharacterStatsCalculator"/> and
/// <see cref="CharacterOffenseCalculator"/> keep them.
/// </summary>
public static class BuffSums
{
	/// <summary>The sums of the sources that count, without <paramref name="without"/> when given.</summary>
	public static (AttributeSum Gear, AttributeSum Skills) Sum(IEnumerable<BuffSource> sources, Func<BuffSource, bool> isOn, BuffSource? without = null)
	{
		ArgumentNullException.ThrowIfNull(sources);
		ArgumentNullException.ThrowIfNull(isOn);
		var gear = new AttributeSum();
		var skills = new AttributeSum();
		foreach (var source in sources.Where(s => !ReferenceEquals(s, without) && isOn(s)))
			(source.IsGear ? gear : skills).Add(source.Sum);

		return (gear, skills);
	}

	/// <summary>
	/// The gear as the attack damage reads it, from the gear sources that count: each weapon in hand on its own, the
	/// armor, jewelry and item sets together (their flat damage adds to every attack); a shield in neither.
	/// </summary>
	public static OffenseGear Offense(IEnumerable<BuffSource> sources, Func<BuffSource, bool> isOn, BuffSource? without = null)
	{
		ArgumentNullException.ThrowIfNull(sources);
		ArgumentNullException.ThrowIfNull(isOn);
		var counted = sources.Where(s => !ReferenceEquals(s, without) && isOn(s)).ToList();
		var other = new AttributeSum();
		foreach (var source in counted.Where(s => s.GearRole == BuffGearRole.Other))
			other.Add(source.Sum);

		return new OffenseGear([.. counted.Where(s => s.GearRole == BuffGearRole.Weapon).Select(s => s.Sum)], other);
	}

	/// <summary>
	/// The levels the sources that count add to every skill (<c>allSkillEnhancement</c>: Battle Standard's "+1 to all
	/// skills"), the standard's own skill included and the masteries aside, as the game counts them.
	/// </summary>
	public static int SkillLevelsAdded(IEnumerable<BuffSource> sources, Func<BuffSource, bool> isOn)
	{
		ArgumentNullException.ThrowIfNull(sources);
		ArgumentNullException.ThrowIfNull(isOn);
		return (int)sources.Where(isOn).Sum(s => s.Sum["allSkillEnhancement"]);
	}

	/// <summary>
	/// What an equipped item is for the attack: a weapon in hand, a shield, or other gear. An empty weapon slot saved as an
	/// item without record is no item at all (<see cref="BuffGearRole.None"/>: bare hands then).
	/// </summary>
	public static BuffGearRole GearRoleOf(Item item)
	{
		ArgumentNullException.ThrowIfNull(item);
		if (!item.IsInWeaponSlot)
			return BuffGearRole.Other;

		if (string.IsNullOrEmpty(item.ItemClass))
			return BuffGearRole.None;

		return item.ItemClass.Equals("WeaponArmor_Shield", StringComparison.OrdinalIgnoreCase) ? BuffGearRole.Shield : BuffGearRole.Weapon;
	}
}
