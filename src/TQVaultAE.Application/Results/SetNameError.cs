namespace TQVaultAE.Application.Results;

/// <summary>Why the name of a named set (loadout, class set) is refused.</summary>
public enum SetNameError
{
	/// <summary>The name can be used.</summary>
	None,

	/// <summary>No name, or only spaces.</summary>
	Empty,

	/// <summary>Another set of the character has this name (case ignored).</summary>
	AlreadyUsed,
}

/// <summary>The naming rule of a character's named sets (loadouts, class sets): a name, unique among the others, case ignored.</summary>
public static class SetNames
{
	/// <summary>Checks a name against the names already used.</summary>
	/// <param name="names">The names of the character's sets.</param>
	/// <param name="name">The name asked for.</param>
	/// <param name="current">The set being renamed, which may keep its name with another case.</param>
	public static SetNameError Validate(IEnumerable<string> names, string? name, string? current = null)
	{
		if (string.IsNullOrWhiteSpace(name))
			return SetNameError.Empty;

		var same = names.FirstOrDefault(n => string.Equals(n, name.Trim(), StringComparison.OrdinalIgnoreCase));
		return same is null || string.Equals(same, current, StringComparison.OrdinalIgnoreCase)
			? SetNameError.None
			: SetNameError.AlreadyUsed;
	}
}
