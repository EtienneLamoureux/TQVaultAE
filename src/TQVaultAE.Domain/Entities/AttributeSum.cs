namespace TQVaultAE.Domain.Entities;

/// <summary>
/// The numeric record variables of several records added together, by variable name (case-insensitive): what the
/// game's accumulators collect from the equipped items, the item sets and the skills before it computes the character
/// window. The racial bonuses are kept per race, since their race comes from a string variable of the same record.
/// </summary>
public sealed class AttributeSum
{
	private readonly Dictionary<string, double> _values = new(StringComparer.OrdinalIgnoreCase);
	private readonly Dictionary<string, RacialBonus> _races = new(StringComparer.OrdinalIgnoreCase);

	/// <summary>The sum of a variable, 0 when no record has it.</summary>
	public double this[string name] => this._values.GetValueOrDefault(name);

	/// <summary>The variables summed, with their values.</summary>
	public IReadOnlyDictionary<string, double> Values => this._values;

	/// <summary>The racial bonuses, by the race named in the records (<c>racialBonusRace</c>).</summary>
	public IReadOnlyDictionary<string, RacialBonus> Races => this._races;

	/// <summary>Adds a value to a variable.</summary>
	public void Add(string name, double value)
	{
		if (value == 0)
			return;

		this._values[name] = this._values.GetValueOrDefault(name) + value;
	}

	/// <summary>Adds a racial bonus to a race.</summary>
	public void AddRace(string race, RacialBonus bonus)
		=> this._races[race] = this._races.GetValueOrDefault(race).Add(bonus);

	/// <summary>Adds every variable and racial bonus of another sum.</summary>
	public void Add(AttributeSum other)
	{
		foreach (var (name, value) in other._values)
			this.Add(name, value);

		foreach (var (race, bonus) in other._races)
			this.AddRace(race, bonus);
	}
}

/// <summary>
/// The racial bonuses against a race: <c>racialBonusAbsoluteDamage</c>, <c>racialBonusPercentDamage</c> and
/// <c>racialBonusPercentDefense</c>.
/// </summary>
public readonly record struct RacialBonus(double AbsoluteDamage, double PercentDamage, double PercentDefense)
{
	/// <summary>Both bonuses added.</summary>
	public RacialBonus Add(RacialBonus other)
		=> new(this.AbsoluteDamage + other.AbsoluteDamage, this.PercentDamage + other.PercentDamage, this.PercentDefense + other.PercentDefense);
}
