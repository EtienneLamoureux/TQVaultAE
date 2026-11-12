using AwesomeAssertions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Entities;

public class RarityExtensionTests
{
	#region GetItemStyle Tests

	[Theory]
	[InlineData(Rarity.NoGear, null)]
	[InlineData(Rarity.Broken, ItemStyle.Broken)]
	[InlineData(Rarity.Mundane, ItemStyle.Mundane)]
	[InlineData(Rarity.Common, ItemStyle.Common)]
	[InlineData(Rarity.Rare, ItemStyle.Rare)]
	[InlineData(Rarity.Epic, ItemStyle.Epic)]
	[InlineData(Rarity.Legendary, ItemStyle.Legendary)]
	public void GetItemStyle_ValidRarity_ReturnsExpectedStyle(Rarity rarity, ItemStyle? expected)
	{
		// Act
		var result = rarity.GetItemStyle();

		// Assert
		result.Should().Be(expected);
	}

	#endregion

	#region GetTranslationTag Tests

	[Theory]
	[InlineData(Rarity.NoGear, null)]
	[InlineData(Rarity.Broken, "tagTutorialTip05TextC")]
	[InlineData(Rarity.Mundane, "tagTutorialTip05TextD")]
	[InlineData(Rarity.Common, "tagTutorialTip05TextD|{0} (+ affix)")]
	[InlineData(Rarity.Rare, "tagTutorialTip05TextF")]
	[InlineData(Rarity.Epic, "tagTutorialTip05TextG")]
	[InlineData(Rarity.Legendary, "tagRDifficultyTitle03")]
	public void GetTranslationTag_ReturnsExpectedTag(Rarity rarity, string? expectedTag)
	{
		var result = rarity.GetTranslationTag();

		result.Should().Be(expectedTag);
	}

	#endregion
}
