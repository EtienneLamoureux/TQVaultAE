using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application;

/// <summary>The border the skill window draws around a skill button, telling what can be done with it.</summary>
public enum SkillButtonState
{
	/// <summary>A point can be spent on it (green).</summary>
	CanRaise,

	/// <summary>It could take a point but none is left (gold).</summary>
	NoPointLeft,

	/// <summary>It has its highest level (blue).</summary>
	Maxed,

	/// <summary>Its tier is not open yet (gray).</summary>
	TierLocked,

	/// <summary>The skill it modifies has no point (red).</summary>
	ParentLocked,
}

/// <summary>A rectangle in pixels of the skill pane.</summary>
public readonly record struct PaneRect(int X, int Y, int Width, int Height)
{
	/// <summary>The bottom edge, excluded.</summary>
	public int Bottom => this.Y + this.Height;

	/// <summary>Tells whether a point of the pane is inside.</summary>
	public bool Contains(double x, double y) => x >= this.X && x < this.X + this.Width && y >= this.Y && y < this.Bottom;
}

/// <summary>A connector piece to draw: its texture and its top-left corner in the pane.</summary>
public readonly record struct PanePiece(string Texture, int X, int Y);

/// <summary>
/// How the game's skill window lays out a mastery pane, read from <c>TQ.exe</c> and the <c>BaseSkillPane</c> record:
/// sizes and positions in pixels of the pane (919x540 in the PC interface), mastery bar, button states and level texts.
/// </summary>
public static class SkillPaneLayout
{
	/// <summary>Width of the pane background (PC interface).</summary>
	public const int Width = 919;

	/// <summary>Height of the pane background.</summary>
	public const int Height = 540;

	/// <summary>Size of a button border.</summary>
	public const int ButtonSize = 40;

	/// <summary>Size of the mastery button, in the square at the bottom-left of the background.</summary>
	public const int MasteryButtonSize = 52;

	/// <summary>Size of a skill icon, drawn inside the border (<c>skillOffsetX/Y</c> = 4).</summary>
	public const int IconSize = 32;

	/// <summary>Offset of the icon in the border.</summary>
	public const int IconOffset = 4;

	/// <summary>Size of the mastery bar (<c>masteryBar</c> texture).</summary>
	public const int BarWidth = 28;

	/// <summary>Height of the mastery bar: 7 steps of 62 pixels, one per tier.</summary>
	public const int BarHeight = 434;

	/// <summary>Height of a connector piece, the distance between two rows of buttons.</summary>
	public const int PieceHeight = 62;

	/// <summary>Offset of the first connector piece from the top-left of its button.</summary>
	public const int PieceOffsetX = -20;

	/// <summary>Offset of the first connector piece from the top-left of its button.</summary>
	public const int PieceOffsetY = -10;

	/// <summary>Texture of a square button border.</summary>
	public const string SquareBorder = """InGameUI\SkillButtonBorder01.tex""";

	/// <summary>Texture of a round button border.</summary>
	public const string RoundBorder = """InGameUI\SkillButtonBorderRound01.tex""";

	/// <summary>The skill level text box, relative to its button: centred on x, top at y (<c>skillLevel</c>).</summary>
	public static (int X, int Y) LevelTextOffset => (21, 41);

	/// <summary>The mastery level text box, relative to the mastery button (<c>masterySkillLevel</c>).</summary>
	public static (int X, int Y) MasteryLevelTextOffset => (30, 62);

	/// <summary>The "points remaining" line (<c>pointsRemaining</c>).</summary>
	public static PaneRect PointsRemaining => new(344, 487, 233, 20);

	/// <summary>The text of the description box under the mastery picture (<c>descriptionTextBox</c>).</summary>
	public static PaneRect Description => new(725, 246, 162, 185);

	/// <summary>The milestone number boxes next to the bar notches, from tier 1 (<c>masteryMilestoneNumber1..7</c>).</summary>
	public static IReadOnlyList<PaneRect> Milestones { get; } =
		[.. new[] { 389, 327, 265, 203, 141, 79, 17 }.Select(y => new PaneRect(27, y, 18, 16))];

	/// <summary>The border of a skill button.</summary>
	public static PaneRect ButtonRect(SkillPaneButton button) => new(button.X, button.Y, ButtonSize, ButtonSize);

	/// <summary>The icon of a skill button, inside its border.</summary>
	public static PaneRect IconRect(SkillPaneButton button) => new(button.X + IconOffset, button.Y + IconOffset, IconSize, IconSize);

	/// <summary>
	/// The share of the mastery bar filled at a mastery level, as the game computes it: one equal step per tier, each
	/// step filled in proportion to the levels between the milestone before and its own (level 1 = 1/7, 4 = 2/7,
	/// 7 = 2.5/7, 40 = 1).
	/// </summary>
	public static double BarFraction(int masteryLevel)
	{
		var tiers = SkillTiers.RequiredLevels;
		double sum = 0;
		int previous = 0;
		foreach (var milestone in tiers)
		{
			if (masteryLevel > milestone)
			{
				sum += 1;
				previous = milestone;
				continue;
			}

			sum += Math.Max(0, masteryLevel - previous) / (double)(milestone - previous);
			break;
		}

		return sum / tiers.Count;
	}

	/// <summary>The height of the mastery bar filled at a mastery level, in pixels.</summary>
	public static int BarFill(int masteryLevel) => (int)Math.Round(BarHeight * BarFraction(masteryLevel));

	/// <summary>
	/// The rectangle painted in the mastery's colour behind the background: as wide as the pane, as high as the filled
	/// bar, ending at the bottom of the bar. The background lets it through its tier lines only, so the lines of the
	/// tiers reached glow.
	/// </summary>
	public static PaneRect PeekThrough(SkillPaneArt art, int masteryLevel)
	{
		var fill = BarFill(masteryLevel);
		return new PaneRect(0, art.BarY + BarHeight - fill - 1, Width, fill);
	}

	/// <summary>The connector pieces of a skill, from the bottom: the "on" pieces when it has points.</summary>
	public static IEnumerable<PanePiece> Connectors(SkillPaneButton button, bool hasPoints)
	{
		var pieces = hasPoints ? button.ConnectionOn : button.ConnectionOff;
		for (int i = 0; i < pieces.Count; i++)
			yield return new PanePiece(pieces[i], button.X + PieceOffsetX, button.Y + PieceOffsetY - i * PieceHeight);
	}

	/// <summary>
	/// The border of a skill button, in the order the game checks: highest level, tier closed, parent skill without a
	/// point, no point left, else a point can be spent.
	/// </summary>
	/// <param name="level">The points spent in the skill.</param>
	/// <param name="maxLevel">Its highest level with points.</param>
	/// <param name="masteryLevel">The level of its mastery.</param>
	/// <param name="requiredMasteryLevel">The mastery level that opens its tier.</param>
	/// <param name="parentHasPoints">False when it modifies a skill that has no point.</param>
	/// <param name="availablePoints">The skill points left.</param>
	public static SkillButtonState State(int level, int maxLevel, int masteryLevel, int requiredMasteryLevel, bool parentHasPoints, int availablePoints)
		=> level >= maxLevel ? SkillButtonState.Maxed
			: masteryLevel < requiredMasteryLevel ? SkillButtonState.TierLocked
			: !parentHasPoints ? SkillButtonState.ParentLocked
			: availablePoints <= 0 ? SkillButtonState.NoPointLeft
			: SkillButtonState.CanRaise;

	/// <summary>The state border texture, square or round.</summary>
	public static string StateBorder(SkillButtonState state, bool round)
	{
		var color = state switch
		{
			SkillButtonState.CanRaise => "Green",
			SkillButtonState.NoPointLeft => "Gold",
			SkillButtonState.Maxed => "Blue",
			SkillButtonState.TierLocked => "GrayOut",
			_ => "Red",
		};
		return $"""InGameUI\SkillButtonBorder{(round ? "Round" : string.Empty)}{color}01.tex""";
	}

	/// <summary>
	/// The level text under a skill button, "level / max" with the levels the items add, or <see langword="null"/>
	/// while its tier is closed: the game hides it.
	/// </summary>
	public static string? LevelText(int level, int itemBonus, int maxLevel, bool tierOpen)
		=> tierOpen ? $"{level + itemBonus} / {maxLevel}" : null;
}
