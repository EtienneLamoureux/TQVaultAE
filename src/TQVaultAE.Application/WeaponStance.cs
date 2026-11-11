namespace TQVaultAE.Application;

/// <summary>
/// How the character holds the weapons in hand, as the game decides it: it chooses the attack
/// animations, which set the attack speed shown.
/// </summary>
public enum WeaponStance
{
	Unarmed,
	OneHanded,
	DualWield,
	Bow,
	Spear,
	Staff,
	Thrown,
	DualThrown,
}

/// <summary>The weapon stances: from the weapons in hand, and their animation fields in the player template.</summary>
public static class WeaponStances
{
	/// <summary>The prefix of the stance's fields in the player template (<c>unarmedAttackAnim1</c>, <c>sHandedAttackAnimSpeed1</c>...).</summary>
	public static string RecordPrefix(WeaponStance stance) => stance switch
	{
		WeaponStance.OneHanded => "sHanded",
		WeaponStance.DualWield => "dHanded",
		WeaponStance.Bow => "bow",
		WeaponStance.Spear => "spear",
		WeaponStance.Staff => "staff",
		WeaponStance.Thrown => "rangedOneHand",
		WeaponStance.DualThrown => "dualRanged",
		_ => "unarmed",
	};

	/// <summary>
	/// The stance for the weapons in hand, by their item class: a shield does not count; two melee weapons are dual
	/// wield, two thrown weapons dual thrown; a bow, a staff or a spear their own stance; nothing is unarmed.
	/// </summary>
	public static WeaponStance FromWeapons(IEnumerable<string> weaponClasses)
	{
		var classes = weaponClasses
			.Where(c => !string.IsNullOrEmpty(c) && !c.Equals("WeaponArmor_Shield", StringComparison.OrdinalIgnoreCase))
			.ToList();

		bool Is(string itemClass, string expected) => itemClass.Equals(expected, StringComparison.OrdinalIgnoreCase);
		if (classes.Any(c => Is(c, "WeaponHunting_Bow")))
			return WeaponStance.Bow;
		if (classes.Any(c => Is(c, "WeaponMagical_Staff")))
			return WeaponStance.Staff;
		if (classes.Any(c => Is(c, "WeaponHunting_Spear")))
			return WeaponStance.Spear;

		int thrown = classes.Count(c => Is(c, "WeaponHunting_RangedOneHand"));
		if (thrown > 0)
			return thrown > 1 ? WeaponStance.DualThrown : WeaponStance.Thrown;

		int melee = classes.Count(c => c.StartsWith("WeaponMelee_", StringComparison.OrdinalIgnoreCase));
		return melee switch
		{
			0 => WeaponStance.Unarmed,
			1 => WeaponStance.OneHanded,
			_ => WeaponStance.DualWield,
		};
	}

	/// <summary>
	/// The mean duration of a stance's attack animations, in seconds: each animation's frames / frame rate divided by its
	/// speed in the template.
	/// </summary>
	/// <param name="animations">Each attack animation's frame count, frame rate and template speed.</param>
	public static double MeanDuration(IEnumerable<(int Frames, int FrameRate, double Speed)> animations)
	{
		var durations = animations
			.Where(a => a.FrameRate > 0 && a.Speed > 0)
			.Select(a => a.Frames / (double)a.FrameRate / a.Speed)
			.ToList();
		return durations.Count == 0 ? 0 : durations.Average();
	}

	/// <summary>
	/// Reads the header of an <c>.anm</c> animation: <c>ANM</c>, a version byte, then the bone count, the frame count and
	/// the frame rate (int32 each).
	/// </summary>
	/// <returns><see langword="null"/> when the data is not an animation.</returns>
	public static (int Frames, int FrameRate)? ReadAnimationHeader(ReadOnlySpan<byte> data)
	{
		if (data.Length < 16 || data[0] != (byte)'A' || data[1] != (byte)'N' || data[2] != (byte)'M')
			return null;

		int frames = BitConverter.ToInt32(data[8..12]);
		int frameRate = BitConverter.ToInt32(data[12..16]);
		return frames > 0 && frameRate > 0 ? (frames, frameRate) : null;
	}
}
