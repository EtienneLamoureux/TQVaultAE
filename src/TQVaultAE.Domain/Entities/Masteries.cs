namespace TQVaultAE.Domain.Entities;

/// <summary>
/// The game's 11 mastery slots (the panes of its skill window). The record of a slot's mastery comes from the game data
/// (<see cref="MasteryTree"/>), so a mod's masteries take the slots they replace. Class set files save the names.
/// </summary>
[Flags]
public enum Masteries
{
	Defensive = 1 << 0,
	Storm = 1 << 1,
	Earth = 1 << 2,
	Nature = 1 << 3,
	Dream = 1 << 4,
	Rune = 1 << 5,
	Warfare = 1 << 6,
	Hunting = 1 << 7,
	Stealth = 1 << 8,
	Spirit = 1 << 9,
	Neidan = 1 << 10,
}