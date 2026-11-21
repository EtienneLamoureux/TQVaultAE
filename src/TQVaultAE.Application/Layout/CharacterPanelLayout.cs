using TQVaultAE.Application.Character;

namespace TQVaultAE.Application.Layout;

/// <summary>
/// How the game's character window shows its statistics (records <c>xpack\ui\character\tab1new..tab3new</c>): attribute
/// totals, the difficulty shown, the panel's size.
/// </summary>
public static class CharacterPanelLayout
{
	/// <summary>Width of the statistics panel of the character window.</summary>
	public const int Width = 251;

	/// <summary>Height of the statistics panel.</summary>
	public const int Height = 390;

	/// <summary>The last difficulty of the game: Normal 0, Epic 1, Legendary 2.</summary>
	public const int Legendary = 2;

	/// <summary>
	/// The difficulty the statistics are computed for (resistance penalty, attack speed): the one picked in the editor,
	/// else the last one the character unlocked; always one of the game's three.
	/// </summary>
	/// <param name="unlocked">The difficulty unlocked, as saved.</param>
	/// <param name="picked">The difficulty picked in the editor, -1 for none.</param>
	public static int ShownDifficulty(int unlocked, int picked = -1)
		=> Math.Clamp(picked >= 0 ? picked : unlocked, 0, Legendary);

	/// <summary>Where the panel's gold frame starts, under the tab strip.</summary>
	public const int FrameTop = 20;

	/// <summary>
	/// An attribute as the game shows it: the base value plus the flat bonuses of gear and skills, times the percentage
	/// bonuses, truncated (58 + 13.5 strength shows 71 in the game).
	/// </summary>
	public static int AttributeTotal(int baseValue, double flatBonus, double percentBonus)
		=> (int)CharacterStatsCalculator.Attribute(baseValue, flatBonus, percentBonus);
}
