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

	[Fact]
	public void TQColor_LegendaryAndQuest_BothReturnPurple()
	{
		// Arrange & Act
		var legendaryColor = ItemStyle.Legendary.TQColor();
		var questColor = ItemStyle.Quest.TQColor();

		// Assert
		legendaryColor.Should().Be(TQColor.Purple);
		questColor.Should().Be(TQColor.Purple);
	}

	[Fact]
	public void TQColor_ArtifactAndFormulae_BothReturnTurquoise()
	{
		// Arrange & Act
		var artifactColor = ItemStyle.Artifact.TQColor();
		var formulaeColor = ItemStyle.Formulae.TQColor();

		// Assert
		artifactColor.Should().Be(TQColor.Turquoise);
		formulaeColor.Should().Be(TQColor.Turquoise);
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

	[Theory]
	[InlineData(ItemStyle.Legendary)]
	[InlineData(ItemStyle.Epic)]
	[InlineData(ItemStyle.Rare)]
	public void Color_RarityStyles_ReturnsCorrectRGB(ItemStyle style)
	{
		// Act
		var result = style.Color();

		// Assert - Each color should have distinct RGB values
		result.Should().NotBeNull();
		// Verify colors are different
		var legendaryColor = ItemStyle.Legendary.Color();
		var epicColor = ItemStyle.Epic.Color();
		var rareColor = ItemStyle.Rare.Color();

		legendaryColor.Should().NotBe(epicColor);
		epicColor.Should().NotBe(rareColor);
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

	[Fact]
	public void TQColor_ArtifactAndFormulae_ReturnSameColor()
	{
		// Act
		var artifactColor = ItemStyle.Artifact.TQColor();
		var formulaeColor = ItemStyle.Formulae.TQColor();

		// Assert - Both return Turquoise
		artifactColor.Should().Be(TQColor.Turquoise);
		formulaeColor.Should().Be(TQColor.Turquoise);
		artifactColor.Should().Be(formulaeColor);
	}

	[Fact]
	public void TQColor_LegendaryAndQuest_ReturnSameColor()
	{
		// Act
		var legendaryColor = ItemStyle.Legendary.TQColor();
		var questColor = ItemStyle.Quest.TQColor();

		// Assert - Both return Purple
		legendaryColor.Should().Be(TQColor.Purple);
		questColor.Should().Be(TQColor.Purple);
	}

	[Fact]
	public void TQColor_Relic_ReturnsOrange()
	{
		// Act
		var result = ItemStyle.Relic.TQColor();

		// Assert
		result.Should().Be(TQColor.Orange);
	}

	[Fact]
	public void TQColor_Potion_ReturnsRed()
	{
		// Act
		var result = ItemStyle.Potion.TQColor();

		// Assert
		result.Should().Be(TQColor.Red);
	}

	#endregion

	#region Integration Tests

	[Fact]
	public void FullColorWorkflow_ForLegendaryItem()
	{
		// Arrange
		var style = ItemStyle.Legendary;

		// Act - Get color, generate tag, wrap text
		var tqColor = style.TQColor();
		var tag = tqColor.ColorTag();
		var coloredName = $"{tag}Epic Sword";
		var extractedColor = coloredName.GetColorFromTaggedString();

		// Assert
		extractedColor.Should().Be(TQColor.Purple);
	}

	[Fact]
	public void ItemStyle_And_Rarity_Roundtrip()
	{
		// Arrange
		var rarity = Rarity.Legendary;

		// Act - Get style from rarity, then get color from style
		var style = rarity.GetItemStyle();
		var tqColor = style!.Value.TQColor();

		// Assert
		style.Should().Be(ItemStyle.Legendary);
		tqColor.Should().Be(TQColor.Purple);
	}

	#endregion
}
