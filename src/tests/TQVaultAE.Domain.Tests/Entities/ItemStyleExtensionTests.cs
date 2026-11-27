using AwesomeAssertions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Entities;

public class ItemStyleExtensionTests
{
	#region TQColor Tests

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
	public void TQColor_AllStyles_ReturnsCorrectColor(ItemStyle style, TQColor expected)
	{
		// Act
		var result = style.TQColor();

		// Assert
		result.Should().Be(expected);
	}

	#endregion

	#region Color Tests

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
	public void Color_AllStyles_ReturnsNonNullColor(ItemStyle style)
	{
		// Act
		var result = style.Color();

		// Assert
		result.Should().NotBeNull();
		result.A.Should().Be(255); // Full opacity
	}

	[Fact]
	public void Color_CallsTQColorAndReturnsSystemColor()
	{
		// Arrange
		var style = ItemStyle.Common;

		// Act
		var result = style.Color();
		var expected = style.TQColor().Color();

		// Assert
		result.Should().Be(expected);
	}

	#endregion

	#region Roundtrip Tests

	[Theory]
	[InlineData(ItemStyle.Broken)]
	[InlineData(ItemStyle.Mundane)]
	[InlineData(ItemStyle.Common)]
	[InlineData(ItemStyle.Rare)]
	[InlineData(ItemStyle.Epic)]
	[InlineData(ItemStyle.Legendary)]
	public void TQColor_And_TQColorTag_Roundtrip(ItemStyle style)
	{
		// Arrange - Get TQColor
		var tqColor = style.TQColor();

		// Act - Generate tag and extract back
		var tag = tqColor.ColorTag();
		var extractedColor = tag.GetColorFromTaggedString();

		// Assert
		extractedColor.Should().Be(tqColor);
	}

	#endregion
}
