using System.ComponentModel;
using System.Drawing;

namespace TQVaultAE.Domain.Entities;

/// <summary>
/// Item Style types
/// </summary>
public enum ItemStyle
{
	/// <summary>
	/// Broken Item : Have a prefix BROKEN
	/// </summary>
	[Description("tagTutorialTip05TextC")]
	Broken,

	/// <summary>
	/// Mundane Item : normal item without any prefix/suffix
	/// </summary>
	[Description("tagTutorialTip05TextD")]
	Mundane,

	/// <summary>
	/// Common Item : normal item with any prefix or suffix
	/// </summary>
	[Description("tagTutorialTip05TextD|{0} (+ affix)")]
	Common,

	/// <summary>
	/// Rare Item : item with classification "Rare" or <see cref="Common"> item with any "Rare" prefix/suffix
	/// </summary>
	[Description("tagTutorialTip05TextF")]
	Rare,

	/// <summary>
	/// Epic Item : item with classification EPIC
	/// </summary>
	[Description("tagTutorialTip05TextG")]
	Epic,

	/// <summary>
	/// Legendary Item : item with classification LEGENDARY
	/// </summary>
	[Description("tagRDifficultyTitle03")]
	Legendary,

	/// <summary>
	/// Quest Item
	/// </summary>
	[Description("tagQuestItem")]
	Quest,

	/// <summary>
	/// Relic
	/// </summary>
	[Description("tagRelic")]
	Relic,

	/// <summary>
	/// Potion
	/// </summary>
	[Description("tagTutorialTip14Title")]
	Potion,

	/// <summary>
	/// Scroll
	/// </summary>
	[Description("xtagLogScroll")]
	Scroll,

	/// <summary>
	/// Parchment
	/// </summary>
	[Description("x3tagSq04_Letter")]
	Parchment,

	/// <summary>
	/// Formulae
	/// </summary>
	[Description("xtagLogArcaneFormula")]// xtagEnchant02
	Formulae,

	/// <summary>
	/// Artifact
	/// </summary>
	[Description("tagArtifact")]
	Artifact
}

#region Related logic

public static class ItemStyleExtension
{
	/// <summary>
	/// Gets the color for a particular item style
	/// </summary>
	/// <param name="style">ItemStyle enumeration</param>
	/// <returns>System.Drawing.Color for the particular itemstyle</returns>
	public static TQColor TQColor(this ItemStyle style) => style switch
	{
		ItemStyle.Broken => Entities.TQColor.DarkGray,
		ItemStyle.Mundane => Entities.TQColor.White,
		ItemStyle.Common => Entities.TQColor.Yellow,
		ItemStyle.Rare => Entities.TQColor.Green,
		ItemStyle.Epic => Entities.TQColor.Blue,
		ItemStyle.Legendary => Entities.TQColor.Purple,
		ItemStyle.Quest => Entities.TQColor.Purple,
		ItemStyle.Relic => Entities.TQColor.Orange,
		ItemStyle.Potion => Entities.TQColor.Red,
		ItemStyle.Scroll => Entities.TQColor.YellowGreen,
		ItemStyle.Parchment => Entities.TQColor.Blue,
		ItemStyle.Formulae => Entities.TQColor.Turquoise,
		ItemStyle.Artifact => Entities.TQColor.Turquoise,
		_ => Entities.TQColor.White,
	};

	/// <summary>
	/// Gets the color for a particular item style
	/// </summary>
	/// <param name="style">ItemStyle enumeration</param>
	/// <returns>System.Drawing.Color for the particular itemstyle</returns>
	public static Color Color(this ItemStyle style)
		=> style.TQColor().Color();

}

#endregion
