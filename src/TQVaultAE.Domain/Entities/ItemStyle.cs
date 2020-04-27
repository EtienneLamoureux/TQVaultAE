using System.ComponentModel;

namespace TQVaultAE.Domain.Entities
{

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
}
