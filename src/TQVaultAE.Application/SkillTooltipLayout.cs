namespace TQVaultAE.Application;

/// <summary>
/// Which effect lines the game's skill tool tip shows, and in which order (<c>the game</c> for the
/// current level, <c>the game</c> for the next one, in the game).
/// </summary>
public static class SkillTooltipLayout
{
	/// <summary>
	/// True for the recharge line: the game writes it first, in aqua (<c>CooldownTime</c>, "^a{%.1f0} Second(s)
	/// Recharge"), and keeps it in the next level even when it does not change.
	/// </summary>
	public static bool IsRecharge(string line)
		=> line.StartsWith("^a", StringComparison.OrdinalIgnoreCase)
		|| line.StartsWith("{^a}", StringComparison.OrdinalIgnoreCase);

	/// <summary>The effects of the current level: the recharge first, then the others in their order.</summary>
	public static IReadOnlyList<string> CurrentEffects(IEnumerable<string> effects)
		=> [.. effects.OrderBy(line => IsRecharge(line) ? 0 : 1)];

	/// <summary>
	/// The effects of the next level: the recharge first, then only what the next level changes. A line that the current
	/// level already shows as is (an energy cost, a flat damage that does not grow...) is left out, as in the game.
	/// From the first heading on (a summon's "Totem Attributes:", "Totem Abilities:"), every line is shown again.
	/// With no current level, every effect is shown.
	/// </summary>
	public static IReadOnlyList<string> NextEffects(IReadOnlyCollection<string> current, IEnumerable<string> next)
	{
		var shown = new List<string>();
		bool inBlock = false;
		foreach (var line in CurrentEffects(next))
		{
			inBlock |= IsHeading(line);
			if (inBlock ? (line.Length != 0 || shown.Count != 0) : IsRecharge(line) || (line.Length != 0 && !current.Contains(line)))
				shown.Add(line);
		}

		return shown;
	}

	/// <summary>
	/// Where a summon's abilities start: the line after its second heading ("Totem Attributes:", then "Totem Abilities:"),
	/// which the game writes in blue at the current level; the count of lines when there is none.
	/// </summary>
	public static int AbilitiesStart(IReadOnlyList<string> effects)
	{
		int headings = 0;
		for (int i = 0; i < effects.Count; i++)
		{
			if (IsHeading(effects[i]) && ++headings == 2)
				return i + 1;
		}

		return effects.Count;
	}

	/// <summary>True for a heading line, such as "Circle of Power Attributes:".</summary>
	public static bool IsHeading(string line)
		=> line.TrimEnd().EndsWith(':');
}
