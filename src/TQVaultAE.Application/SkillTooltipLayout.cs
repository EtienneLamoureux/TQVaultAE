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

	/// <summary>
	/// The effects of the current level: the skill's recharge first, then the others in their order. A summon's own
	/// skills keep their recharge in their block, after the first heading.
	/// </summary>
	public static IReadOnlyList<string> CurrentEffects(IEnumerable<string> effects)
	{
		var lines = effects.ToList();
		var head = lines.FindIndex(IsHeading);
		if (head < 0)
			head = lines.Count;

		return [.. lines.Take(head).OrderBy(line => IsRecharge(line) ? 0 : 1), .. lines.Skip(head)];
	}

	/// <summary>
	/// The effects of the next level: the recharge first, then only what the next level changes. A line that the current
	/// level already shows as is (an energy cost, a flat damage that does not grow...) is left out, as in the game.
	/// From the first heading on (a summon's "Totem Attributes:", "Totem Abilities:"), every line is shown again.
	/// With no current level, every effect is shown.
	/// </summary>
	/// <param name="current">The current level's effects.</param>
	/// <param name="next">The next level's effects.</param>
	/// <param name="modified">
	/// Next level lines the character's modifiers changed (Study Prey's energy cost with Flush Out), shown even when the
	/// current level has the same line.
	/// </param>
	public static IReadOnlyList<string> NextEffects(IReadOnlyCollection<string> current, IEnumerable<string> next
		, IReadOnlyCollection<string>? modified = null)
	{
		var shown = new List<string>();
		bool inBlock = false;
		foreach (var line in CurrentEffects(next))
		{
			inBlock |= IsHeading(line);
			if (inBlock
				? line.Length != 0 || shown.Count != 0
				: IsRecharge(line) || (line.Length != 0 && (!current.Contains(line) || modified?.Contains(line) == true)))
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

	/// <summary>
	/// The recharge the skill window shows (<c>the game</c>): the skill's cooldown plus the seconds its
	/// modifiers add (Rapid Construction -3 s), times one minus the recharge reduction (the character's, plus the
	/// modifiers' such as Resilience), capped at <c>playerCooldownReductionCap</c>; never below 0.
	/// </summary>
	public static double Recharge(double cooldown, double modifierSeconds, double reductionPercent, double reductionCap)
	{
		var reduction = Math.Clamp(reductionPercent, 0, Math.Max(0, reductionCap));
		return Math.Max(0, (cooldown + modifierSeconds) * (1 - reduction / 100));
	}

	/// <summary>True for a heading line, such as "Circle of Power Attributes:".</summary>
	public static bool IsHeading(string line)
		=> line.TrimEnd().EndsWith(':');
}
