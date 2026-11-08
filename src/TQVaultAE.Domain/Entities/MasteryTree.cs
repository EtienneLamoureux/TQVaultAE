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
	/// <summary>Tells whether a skill record is this mastery or one of its skills.</summary>
	public bool Contains(RecordId skillId)
		=> this.Id == skillId || this.Skills.Any(s => s.Id == skillId);
}

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
}

/// <summary>
/// The tiers of a mastery pane: the mastery level that unlocks each row of skills.
/// </summary>
public static class SkillTiers
{
	/// <summary>Mastery level that unlocks each tier, from tier 1. Tier 7 came with Atlantis.</summary>
	private static readonly int[] _requiredLevels = [1, 4, 10, 16, 24, 32, 40];

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
