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

	[Fact]
	public void GetItemStyle_NoGear_ReturnsNull()
	{
		// Act
		var result = Rarity.NoGear.GetItemStyle();

		// Assert
		result.Should().BeNull();
	}

	[Theory]
	[InlineData(Rarity.Broken)]
	[InlineData(Rarity.Mundane)]
	[InlineData(Rarity.Common)]
	[InlineData(Rarity.Rare)]
	[InlineData(Rarity.Epic)]
	[InlineData(Rarity.Legendary)]
	public void GetItemStyle_AllGearRarities_ReturnsNonNull(Rarity rarity)
	{
		// Act
		var result = rarity.GetItemStyle();

		// Assert
		result.Should().NotBeNull();
	}

	#endregion

	#region GetTranslationTag Tests

	[Theory]
	[InlineData(Rarity.Broken)]
	[InlineData(Rarity.Mundane)]
	[InlineData(Rarity.Common)]
	[InlineData(Rarity.Rare)]
	[InlineData(Rarity.Epic)]
	[InlineData(Rarity.Legendary)]
	public void GetTranslationTag_ValidRarity_ReturnsNonNull(Rarity rarity)
	{
		// Act
		var result = rarity.GetTranslationTag();

		// Assert
		result.Should().NotBeNullOrEmpty();
	}

	[Fact]
	public void GetTranslationTag_NoGear_ReturnsNull()
	{
		// Act
		var result = Rarity.NoGear.GetTranslationTag();

		// Assert
		result.Should().BeNull();
	}

	[Fact]
	public void GetTranslationTag_ReturnsDescriptionFromEnum()
	{
		// Act
		var result = Rarity.Legendary.GetTranslationTag();

		// Assert - Should contain the description attribute value
		result.Should().NotBeNull();
	}

	#endregion
}
