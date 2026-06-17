using AwesomeAssertions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Entities;

public class ExtensionTests
{
	#region GameDlcExtension Tests

	[Theory]
	[InlineData(GameDlc.TitanQuest, "TQ")]
	[InlineData(GameDlc.ImmortalThrone, "IT")]
	[InlineData(GameDlc.Ragnarok, "RAG")]
	[InlineData(GameDlc.Atlantis, "ATL")]
	[InlineData(GameDlc.EternalEmbers, "EEM")]
	public void GameDlcExtension_GetCode_ReturnsExpectedCode(GameDlc dlc, string expectedCode)
	{
		// Act
		var result = dlc.GetCode();

		// Assert
		result.Should().Be(expectedCode);
	}

	[Theory]
	[InlineData(GameDlc.TitanQuest, "tagBackground01")]
	[InlineData(GameDlc.ImmortalThrone, "tagBackground02")]
	[InlineData(GameDlc.Ragnarok, "tagBackground03")]
	[InlineData(GameDlc.Atlantis, "tagBackground04")]
	[InlineData(GameDlc.EternalEmbers, "x4tagBackground05")]
	public void GameDlcExtension_GetTranslationTag_ReturnsExpectedTag(GameDlc dlc, string expectedTag)
	{
		// Act
		var result = dlc.GetTranslationTag();

		// Assert
		result.Should().Be(expectedTag);
	}

	[Theory]
	[InlineData(GameDlc.TitanQuest, "")]
	[InlineData(GameDlc.ImmortalThrone, "(IT)")]
	[InlineData(GameDlc.Ragnarok, "(RAG)")]
	[InlineData(GameDlc.Atlantis, "(ATL)")]
	[InlineData(GameDlc.EternalEmbers, "(EEM)")]
	public void GameDlcExtension_GetSuffix_ReturnsExpectedSuffix(GameDlc dlc, string expectedSuffix)
	{
		// Act
		var result = dlc.GetSuffix();

		// Assert
		result.Should().Be(expectedSuffix);
	}

	#endregion

	#region RarityExtension Tests

	[Theory]
	[InlineData(Rarity.NoGear, null)]
	[InlineData(Rarity.Broken, ItemStyle.Broken)]
	[InlineData(Rarity.Mundane, ItemStyle.Mundane)]
	[InlineData(Rarity.Common, ItemStyle.Common)]
	[InlineData(Rarity.Rare, ItemStyle.Rare)]
	[InlineData(Rarity.Epic, ItemStyle.Epic)]
	[InlineData(Rarity.Legendary, ItemStyle.Legendary)]
	public void RarityExtension_GetItemStyle_ReturnsExpectedStyle(Rarity rarity, ItemStyle? expectedStyle)
	{
		// Act
		var result = rarity.GetItemStyle();

		// Assert
		result.Should().Be(expectedStyle);
	}

	[Theory]
	[InlineData(Rarity.NoGear, null)]
	[InlineData(Rarity.Broken, "tagTutorialTip05TextC")]
	[InlineData(Rarity.Mundane, "tagTutorialTip05TextD")]
	[InlineData(Rarity.Common, "tagTutorialTip05TextD|{0} (+ affix)")]
	[InlineData(Rarity.Rare, "tagTutorialTip05TextF")]
	[InlineData(Rarity.Epic, "tagTutorialTip05TextG")]
	[InlineData(Rarity.Legendary, "tagRDifficultyTitle03")]
	public void RarityExtension_GetTranslationTag_ReturnsExpectedTag(Rarity rarity, string? expectedTag)
	{
		// Act
		var result = rarity.GetTranslationTag();

		// Assert
		result.Should().Be(expectedTag);
	}

	#endregion

	#region ItemStyleExtension Tests

	[Theory]
	[InlineData(ItemStyle.Broken, TQColor.DarkGray)]
	[InlineData(ItemStyle.Mundane, TQColor.White)]
	[InlineData(ItemStyle.Common, TQColor.Yellow)]
	[InlineData(ItemStyle.Rare, TQColor.Green)]
	[InlineData(ItemStyle.Epic, TQColor.Blue)]
	[InlineData(ItemStyle.Legendary, TQColor.Purple)]
	[InlineData(ItemStyle.Quest, TQColor.Purple)]
	[InlineData(ItemStyle.Relic, TQColor.Orange)]
	[InlineData(ItemStyle.Potion, TQColor.Red)]
	[InlineData(ItemStyle.Scroll, TQColor.YellowGreen)]
	[InlineData(ItemStyle.Parchment, TQColor.Blue)]
	[InlineData(ItemStyle.Formulae, TQColor.Turquoise)]
	[InlineData(ItemStyle.Artifact, TQColor.Turquoise)]
	public void ItemStyleExtension_TQColor_ReturnsExpectedColor(ItemStyle style, TQColor expectedColor)
	{
		// Act
		var result = style.TQColor();

		// Assert
		result.Should().Be(expectedColor);
	}

	[Theory]
	[InlineData(ItemStyle.Broken)]
	[InlineData(ItemStyle.Mundane)]
	[InlineData(ItemStyle.Common)]
	[InlineData(ItemStyle.Rare)]
	[InlineData(ItemStyle.Epic)]
	[InlineData(ItemStyle.Legendary)]
	[InlineData(ItemStyle.Quest)]
	[InlineData(ItemStyle.Relic)]
	[InlineData(ItemStyle.Potion)]
	[InlineData(ItemStyle.Scroll)]
	[InlineData(ItemStyle.Parchment)]
	[InlineData(ItemStyle.Formulae)]
	[InlineData(ItemStyle.Artifact)]
	public void ItemStyleExtension_Color_ReturnsNonDefaultColor(ItemStyle style)
	{
		// Act
		var result = style.Color();

		// Assert - Color.Empty has ARGB of 0
		result.ToArgb().Should().NotBe(0);
	}

	#endregion
}
