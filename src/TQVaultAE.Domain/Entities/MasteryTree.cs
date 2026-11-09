namespace TQVaultAE.Domain.Entities;

/// <summary>
/// A mastery as the game's skill window shows it: its record and the skills of its pane.
/// </summary>
/// <param name="Mastery">The mastery.</param>
/// <param name="Id">The mastery record, whose skill level is the mastery level.</param>
/// <param name="NameTag">The tag of its name (<c>skillDisplayName</c>).</param>
/// <param name="DescriptionTag">The tag of its description (<c>skillBaseDescription</c>).</param>
/// <param name="Icon">Its icon (<c>skillUpBitmapName</c>), or an empty string.</param>
/// <param name="MaxLevel">Its highest level (<c>skillMaxLevel</c>).</param>
/// <param name="Skills">The skills of its pane, by tier.</param>
public sealed record MasteryTree(
	Masteries Mastery
	, RecordId Id
	, string NameTag
	, string DescriptionTag
	, string Icon
	, int MaxLevel
	, IReadOnlyList<MasterySkill> Skills)
{
	/// <summary>The pictures of the pane, to draw it as the game does, or <see langword="null"/> when unknown.</summary>
	public SkillPaneArt? Pane { get; init; }

	/// <summary>Tells whether a skill record is this mastery or one of its skills.</summary>
	public bool Contains(RecordId skillId)
		=> this.Id == skillId || this.Skills.Any(s => s.Id == skillId);
}

/// <summary>
/// The pictures of a mastery pane of the game's skill window (<c>SkillPaneCtrl</c> record, PC variant), positions in
/// pixels of the pane.
/// </summary>
/// <param name="Background">The pane background (<c>skillPaneBaseBitmap</c>).</param>
/// <param name="Picture">The mastery picture in the circle (<c>skillPaneMasteryBitmap</c>).</param>
/// <param name="PictureX">Left of the picture.</param>
/// <param name="PictureY">Top of the picture.</param>
/// <param name="Bar">The full mastery bar (<c>masteryBar</c>'s <c>bitmapFullName</c>).</param>
/// <param name="BarX">Left of the bar.</param>
/// <param name="BarY">Top of the bar.</param>
/// <param name="PeekThroughRed">Red of the colour shown through the tier lines reached (<c>peekThroughColorRed</c>), 0 to 255.</param>
/// <param name="PeekThroughGreen">Green of that colour, 0 to 255.</param>
/// <param name="PeekThroughBlue">Blue of that colour, 0 to 255.</param>
/// <param name="DescriptionTag">The tag of the mastery text in the description box (<c>skillPaneDescriptionTag</c>).</param>
/// <param name="MasteryButtonX">Left of the mastery button.</param>
/// <param name="MasteryButtonY">Top of the mastery button.</param>
public sealed record SkillPaneArt(
	string Background
	, string Picture
	, int PictureX
	, int PictureY
	, string Bar
	, int BarX
	, int BarY
	, int PeekThroughRed
	, int PeekThroughGreen
	, int PeekThroughBlue
	, string DescriptionTag
	, int MasteryButtonX
	, int MasteryButtonY);

/// <summary>
/// The button of a skill in its mastery pane (<c>SkillButton</c> record) and the pictures the pane draws for it.
/// </summary>
/// <param name="X">Left of the 40x40 button border in the pane.</param>
/// <param name="Y">Top of the button border.</param>
/// <param name="IsCircular">True for a round border (modifiers), false for a square one.</param>
/// <param name="LockedIcon">The icon shown while the tier is closed (<c>skillDownBitmapName</c>), or an empty string.</param>
/// <param name="ConnectionOn">The connector pieces drawn up the column when the skill has points, from the bottom.</param>
/// <param name="ConnectionOff">The connector pieces drawn when it has none.</param>
public sealed record SkillPaneButton(
	int X
	, int Y
	, bool IsCircular
	, string LockedIcon
	, IReadOnlyList<string> ConnectionOn
	, IReadOnlyList<string> ConnectionOff);

/// <summary>
/// A skill of a mastery pane.
/// </summary>
/// <param name="Id">The skill record, as stored in the save.</param>
/// <param name="NameTag">The tag of its name: the buff or pet skill record names the skills that only cast one.</param>
/// <param name="DescriptionTag">The tag of its description.</param>
/// <param name="Icon">Its icon, or an empty string.</param>
/// <param name="MaxLevel">Its highest level with skill points (<c>skillMaxLevel</c>); items can raise it further.</param>
/// <param name="Tier">Its row in the pane, from 1 (bottom) to 7 (the Atlantis skills).</param>
/// <param name="ParentId">The skill it modifies, which needs a point first, or <see langword="null"/>.</param>
public sealed record MasterySkill(
	RecordId Id
	, string NameTag
	, string DescriptionTag
	, string Icon
	, int MaxLevel
	, int Tier
	, RecordId? ParentId)
{
	/// <summary>The mastery level that unlocks the skill.</summary>
	public int RequiredMasteryLevel => SkillTiers.RequiredMasteryLevel(this.Tier);

	/// <summary>Its button in the pane, or <see langword="null"/> when unknown.</summary>
	public SkillPaneButton? Button { get; init; }
}

/// <summary>
/// The tiers of a mastery pane: the mastery level that unlocks each row of skills.
/// </summary>
public static class SkillTiers
{
	/// <summary>Mastery level that unlocks each tier, from tier 1. Tier 7 came with Atlantis.</summary>
	private static readonly int[] _requiredLevels = [1, 4, 10, 16, 24, 32, 40];

	/// <summary>The mastery level that unlocks each tier, from tier 1.</summary>
	public static IReadOnlyList<int> RequiredLevels => _requiredLevels;

	/// <summary>Vertical position of the buttons of the bottom row (tier 1) in the skill pane.</summary>
	public const int BottomRowY = 403;

	/// <summary>Distance between two rows of buttons in the skill pane.</summary>
	public const int RowSpacing = 62;

	/// <summary>The highest tier.</summary>
	public static int MaxTier => _requiredLevels.Length;

	/// <summary>The mastery level that unlocks a tier; tiers out of range are clamped.</summary>
	public static int RequiredMasteryLevel(int tier)
		=> _requiredLevels[Math.Clamp(tier, 1, MaxTier) - 1];

	/// <summary>
	/// The tier of a skill button from its vertical position in the pane (<c>bitmapPositionY</c>): row 403 is tier 1,
	/// each row above adds one.
	/// </summary>
	public static int FromButtonY(int y)
		=> Math.Clamp((int)Math.Round((BottomRowY - y) / (double)RowSpacing) + 1, 1, MaxTier);
}
