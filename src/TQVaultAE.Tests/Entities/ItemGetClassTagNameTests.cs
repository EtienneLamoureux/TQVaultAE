using AwesomeAssertions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Entities;

/// <summary>
/// Tests for Item.GetClassTagName method
/// </summary>
public class ItemGetClassTagNameTests
{
	#region Known Item Classes

	[Theory]
	[InlineData(Item.ICLASS_HEAD, "tagCR_Head")]
	[InlineData(Item.ICLASS_FOREARM, "tagCR_Arm")]
	[InlineData(Item.ICLASS_UPPERBODY, "tagCR_Torso")]
	[InlineData(Item.ICLASS_LOWERBODY, "tagCR_Leg")]
	[InlineData(Item.ICLASS_AXE, "tagItemAxe")]
	[InlineData(Item.ICLASS_MACE, "tagItemMace")]
	[InlineData(Item.ICLASS_SWORD, "tagItemWarBlade")]
	[InlineData(Item.ICLASS_SHIELD, "tagItemWarShield")]
	[InlineData(Item.ICLASS_RANGEDONEHAND, "x2tagThrownWeapon")]
	[InlineData(Item.ICLASS_BOW, "tagItemWarBow")]
	[InlineData(Item.ICLASS_SPEAR, "tagItemLance")]
	[InlineData(Item.ICLASS_STAFF, "tagItemBattleStaff")]
	[InlineData(Item.ICLASS_AMULET, "tagItemAmulet")]
	[InlineData(Item.ICLASS_RING, "tagItemRing")]
	[InlineData(Item.ICLASS_ARTIFACT, "tagArtifact")]
	[InlineData(Item.ICLASS_FORMULA, "xtagEnchant02")]
	[InlineData(Item.ICLASS_RELIC, "tagRelic")]
	[InlineData(Item.ICLASS_CHARM, "tagItemCharm")]
	[InlineData(Item.ICLASS_SCROLL, "xtagLogScroll")]
	[InlineData(Item.ICLASS_POTIONMANA, "tagHUDEnergyPotion")]
	[InlineData(Item.ICLASS_POTIONHEALTH, "tagHUDHealthPotion")]
	[InlineData(Item.ICLASS_SCROLL_ETERNAL, "x4tag_PotionReward")]
	[InlineData(Item.ICLASS_QUESTITEM, "tagQuestItem")]
	[InlineData(Item.ICLASS_EQUIPMENT, "xtagArtifactReagentTypeEquipment")]
	[InlineData(Item.ICLASS_DYE, "tagDye")]
	public void GetClassTagName_KnownItemClass_ReturnsCorrectTagName(string itemClass, string expectedTagName)
	{
		// Act
		var result = Item.GetClassTagName(itemClass);

		// Assert
		result.Should().Be(expectedTagName);
	}

	#endregion

	#region Case Insensitivity

	[Theory]
	[InlineData("ARMORPROTECTIVE_HEAD")]
	[InlineData("ArmorProtective_Head")]
	[InlineData("armorprotective_head")]
	[InlineData("ArMoRpRoTeCtIvE_HeAd")]
	public void GetClassTagName_CaseInsensitive_ReturnsCorrectTagName(string itemClass)
	{
		// Act
		var result = Item.GetClassTagName(itemClass);

		// Assert
		result.Should().Be("tagCR_Head");
	}

	[Theory]
	[InlineData("ITEMARTIFACT")]
	[InlineData("ItemArtifact")]
	[InlineData("itemartifact")]
	public void GetClassTagName_ArtifactVariations_CaseInsensitive(string itemClass)
	{
		// Act
		var result = Item.GetClassTagName(itemClass);

		// Assert
		result.Should().Be("tagArtifact");
	}

	#endregion

	#region Unknown Item Classes

	[Fact]
	public void GetClassTagName_UnknownItemClass_ReturnsInputAsFallback()
	{
		// Arrange
		var unknownClass = "UnknownItemClass12345";

		// Act
		var result = Item.GetClassTagName(unknownClass);

		// Assert
		result.Should().Be(unknownClass);
	}

	[Fact]
	public void GetClassTagName_PartialMatch_ReturnsFallback()
	{
		// Arrange - Similar to ICLASS_HEAD but not exact
		var partialClass = "ArmorProtective_Hea"; // Missing 'd'

		// Act
		var result = Item.GetClassTagName(partialClass);

		// Assert - Should return input as fallback since no exact match
		result.Should().Be(partialClass);
	}

	[Fact]
	public void GetClassTagName_EmptyString_ReturnsEmpty()
	{
		// Act
		var result = Item.GetClassTagName(string.Empty);

		// Assert
		result.Should().BeEmpty();
	}

	#endregion
}
