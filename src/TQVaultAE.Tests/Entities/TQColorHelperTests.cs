using AwesomeAssertions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Entities;

public class TQColorHelperTests
{
	#region GetColorFromTagIdentifier Tests

	[Theory]
	[InlineData('A', TQColor.Aqua)]
	[InlineData('B', TQColor.Blue)]
	[InlineData('C', TQColor.LightCyan)]
	[InlineData('D', TQColor.DarkGray)]
	[InlineData('F', TQColor.Fuschia)]
	[InlineData('G', TQColor.Green)]
	[InlineData('I', TQColor.Indigo)]
	[InlineData('K', TQColor.Khaki)]
	[InlineData('L', TQColor.YellowGreen)]
	[InlineData('M', TQColor.Maroon)]
	[InlineData('O', TQColor.Orange)]
	[InlineData('P', TQColor.Purple)]
	[InlineData('R', TQColor.Red)]
	[InlineData('S', TQColor.Silver)]
	[InlineData('T', TQColor.Turquoise)]
	[InlineData('W', TQColor.White)]
	[InlineData('Y', TQColor.Yellow)]
	public void GetColorFromTagIdentifier_ValidChar_ReturnsCorrectColor(char identifier, TQColor expected)
	{
		// Act
		var result = TQColorHelper.GetColorFromTagIdentifier(identifier);

		// Assert
		result.Should().Be(expected);
	}

	[Theory]
	[InlineData('X')]
	[InlineData('Z')]
	[InlineData('1')]
	[InlineData('a')] // Lowercase returns White as fallback
	public void GetColorFromTagIdentifier_InvalidChar_ReturnsWhite(char identifier)
	{
		// Act
		var result = TQColorHelper.GetColorFromTagIdentifier(identifier);

		// Assert
		result.Should().Be(TQColor.White);
	}

	#endregion

	#region TagIdentifier Tests

	[Theory]
	[InlineData(TQColor.Aqua, 'A')]
	[InlineData(TQColor.Blue, 'B')]
	[InlineData(TQColor.LightCyan, 'C')]
	[InlineData(TQColor.DarkGray, 'D')]
	[InlineData(TQColor.Fuschia, 'F')]
	[InlineData(TQColor.Green, 'G')]
	[InlineData(TQColor.Indigo, 'I')]
	[InlineData(TQColor.Khaki, 'K')]
	[InlineData(TQColor.YellowGreen, 'L')]
	[InlineData(TQColor.Maroon, 'M')]
	[InlineData(TQColor.Orange, 'O')]
	[InlineData(TQColor.Purple, 'P')]
	[InlineData(TQColor.Red, 'R')]
	[InlineData(TQColor.Silver, 'S')]
	[InlineData(TQColor.Turquoise, 'T')]
	[InlineData(TQColor.White, 'W')]
	[InlineData(TQColor.Yellow, 'Y')]
	public void TagIdentifier_ValidColor_ReturnsCorrectChar(TQColor color, char expected)
	{
		// Act
		var result = color.TagIdentifier();

		// Assert
		result.Should().Be(expected);
	}

	#endregion

	#region GetColorFromTaggedString Tests

	[Theory]
	[InlineData("{^A}Aqua text", TQColor.Aqua)]
	[InlineData("{^B}Blue text", TQColor.Blue)]
	[InlineData("{^R}Red text", TQColor.Red)]
	[InlineData("{^G}Green text", TQColor.Green)]
	[InlineData("{^Y}Yellow text", TQColor.Yellow)]
	[InlineData("{^P}Purple text", TQColor.Purple)]
	public void GetColorFromTaggedString_FourCharFormat_ReturnsCorrectColor(string text, TQColor expected)
	{
		// Act
		var result = text.GetColorFromTaggedString();

		// Assert
		result.Should().Be(expected);
	}

	[Theory]
	[InlineData("^A Aqua text", TQColor.Aqua)]
	[InlineData("^R Red text", TQColor.Red)]
	[InlineData("^G Green text", TQColor.Green)]
	public void GetColorFromTaggedString_TwoCharFormat_ReturnsCorrectColor(string text, TQColor expected)
	{
		// Act
		var result = text.GetColorFromTaggedString();

		// Assert
		result.Should().Be(expected);
	}

	[Theory]
	[InlineData("Plain text")]
	[InlineData("")]
	[InlineData(null)]
	[InlineData("   ")]
	public void GetColorFromTaggedString_NoColorTag_ReturnsNull(string? text)
	{
		// Act
		var result = text.GetColorFromTaggedString();

		// Assert
		result.Should().BeNull();
	}

	[Fact]
	public void GetColorFromTaggedString_LowercaseColorId_StillWorks()
	{
		// Arrange - lowercase color identifier should still resolve
		var text = "{^r}lowercase red";

		// Act
		var result = text.GetColorFromTaggedString();

		// Assert
		result.Should().Be(TQColor.Red);
	}

	#endregion

	#region ColorTag Tests

	[Theory]
	[InlineData(TQColor.Red, true, "{^R}")]
	[InlineData(TQColor.Blue, true, "{^B}")]
	[InlineData(TQColor.Green, true, "{^G}")]
	[InlineData(TQColor.Yellow, true, "{^Y}")]
	[InlineData(TQColor.White, true, "{^W}")]
	public void ColorTag_FourCharFormat_ReturnsCorrectFormat(TQColor color, bool fourCharFormat, string expected)
	{
		// Act
		var result = color.ColorTag(fourCharFormat);

		// Assert
		result.Should().Be(expected);
	}

	[Theory]
	[InlineData(TQColor.Red, false, "^R")]
	[InlineData(TQColor.Blue, false, "^B")]
	[InlineData(TQColor.Green, false, "^G")]
	public void ColorTag_TwoCharFormat_ReturnsCorrectFormat(TQColor color, bool fourCharFormat, string expected)
	{
		// Act
		var result = color.ColorTag(fourCharFormat);

		// Assert
		result.Should().Be(expected);
	}

	[Theory]
	[InlineData(TQColor.Aqua)]
	[InlineData(TQColor.LightCyan)]
	[InlineData(TQColor.DarkGray)]
	[InlineData(TQColor.Fuschia)]
	[InlineData(TQColor.Indigo)]
	[InlineData(TQColor.Khaki)]
	[InlineData(TQColor.YellowGreen)]
	[InlineData(TQColor.Maroon)]
	[InlineData(TQColor.Orange)]
	[InlineData(TQColor.Purple)]
	[InlineData(TQColor.Silver)]
	[InlineData(TQColor.Turquoise)]
	public void ColorTag_AllColors_FourCharFormat_GeneratesCorrectFormat(TQColor color)
	{
		// Act
		var result = color.ColorTag(fourCharFormat: true);

		// Assert
		result.Should().StartWith("{^");
		result.Should().EndWith("}");
		result.Length.Should().Be(4);
	}

	[Theory]
	[InlineData(TQColor.Aqua)]
	[InlineData(TQColor.Blue)]
	[InlineData(TQColor.Red)]
	[InlineData(TQColor.Green)]
	[InlineData(TQColor.Yellow)]
	[InlineData(TQColor.White)]
	public void ColorTag_AllColors_TwoCharFormat_GeneratesCorrectFormat(TQColor color)
	{
		// Act
		var result = color.ColorTag(fourCharFormat: false);

		// Assert
		result.Should().StartWith("^");
		result.Length.Should().Be(2);
	}

	#endregion

	#region RemoveLeadingColorTag Tests

	[Theory]
	[InlineData("{^R}Red Text", "Red Text")]
	[InlineData("{^B}Blue Text", "Blue Text")]
	[InlineData("{^G}Green Text", "Green Text")]
	[InlineData("^R Short Form", " Short Form")]
	public void RemoveLeadingColorTag_WithColorTag_RemovesTag(string input, string expected)
	{
		// Act
		var result = input.RemoveLeadingColorTag();

		// Assert
		result.Should().Be(expected);
	}

	[Theory]
	[InlineData("Plain text")]
	[InlineData("No color tag")]
	[InlineData("")]
	public void RemoveLeadingColorTag_NoColorTag_ReturnsOriginal(string input)
	{
		// Act
		var result = input.RemoveLeadingColorTag();

		// Assert
		result.Should().Be(input);
	}

	[Fact]
	public void RemoveLeadingColorTag_NullInput_ReturnsEmpty()
	{
		// Act
		var result = ((string?)null).RemoveLeadingColorTag();

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public void RemoveLeadingColorTag_WhitespaceOnly_ReturnsOriginal()
	{
		// Act
		var result = "   ".RemoveLeadingColorTag();

		// Assert - whitespace returns as-is per implementation
		result.Should().Be("   ");
	}

	[Fact]
	public void RemoveLeadingColorTag_TwoCharFormat_RemovesCorrectly()
	{
		// Arrange
		var input = "^R Red text";

		// Act
		var result = input.RemoveLeadingColorTag();

		// Assert
		result.Should().Be(" Red text");
	}

	#endregion

	#region Color Property Tests

	[Theory]
	[InlineData(TQColor.Red, 255, 0, 0)]
	[InlineData(TQColor.Blue, 0, 163, 255)]
	[InlineData(TQColor.Green, 64, 255, 64)]
	[InlineData(TQColor.Yellow, 255, 245, 43)]
	[InlineData(TQColor.White, 255, 255, 255)]
	[InlineData(TQColor.Aqua, 0, 255, 255)]
	public void Color_ValidColor_ReturnsCorrectRGB(TQColor color, byte expectedR, byte expectedG, byte expectedB)
	{
		// Act
		var result = color.Color();

		// Assert
		result.R.Should().Be(expectedR);
		result.G.Should().Be(expectedG);
		result.B.Should().Be(expectedB);
	}

	[Theory]
	[InlineData(TQColor.Aqua)]
	[InlineData(TQColor.Blue)]
	[InlineData(TQColor.LightCyan)]
	[InlineData(TQColor.DarkGray)]
	[InlineData(TQColor.Fuschia)]
	[InlineData(TQColor.Green)]
	[InlineData(TQColor.Indigo)]
	[InlineData(TQColor.Khaki)]
	[InlineData(TQColor.YellowGreen)]
	[InlineData(TQColor.Maroon)]
	[InlineData(TQColor.Orange)]
	[InlineData(TQColor.Purple)]
	[InlineData(TQColor.Red)]
	[InlineData(TQColor.Silver)]
	[InlineData(TQColor.Turquoise)]
	[InlineData(TQColor.White)]
	[InlineData(TQColor.Yellow)]
	public void Color_AllColors_ReturnsNonEmptyColor(TQColor color)
	{
		// Act
		var result = color.Color();

		// Assert
		result.Should().NotBeNull();
		result.A.Should().Be(255); // Full opacity
	}

	#endregion

	#region Roundtrip Tests

	[Theory]
	[InlineData(TQColor.Red)]
	[InlineData(TQColor.Blue)]
	[InlineData(TQColor.Green)]
	[InlineData(TQColor.Yellow)]
	[InlineData(TQColor.Purple)]
	[InlineData(TQColor.Orange)]
	[InlineData(TQColor.White)]
	public void ColorTag_And_GetColorFromTagIdentifier_Roundtrip(TQColor color)
	{
		// Arrange - Get tag identifier
		var identifier = color.TagIdentifier();

		// Act - Get color back from identifier
		var result = TQColorHelper.GetColorFromTagIdentifier(identifier);

		// Assert
		result.Should().Be(color);
	}

	[Theory]
	[InlineData(TQColor.Red)]
	[InlineData(TQColor.Blue)]
	[InlineData(TQColor.Green)]
	[InlineData(TQColor.Yellow)]
	public void ColorTag_And_GetColorFromTaggedString_Roundtrip(TQColor color)
	{
		// Arrange - Generate tag
		var tag = color.ColorTag();

		// Act - Extract color from tag
		var result = tag.GetColorFromTaggedString();

		// Assert
		result.Should().Be(color);
	}

	[Theory]
	[InlineData(TQColor.Red)]
	[InlineData(TQColor.Blue)]
	[InlineData(TQColor.Green)]
	public void ColorTag_And_Color_And_TagIdentifier_Roundtrip(TQColor color)
	{
		// Arrange
		var tag = color.ColorTag();
		var identifier = color.TagIdentifier();

		// Act
		var colorFromTag = tag.GetColorFromTaggedString();
		var systemColor = color.Color();

		// Assert
		colorFromTag.Should().Be(color);
		identifier.Should().NotBeNull();
	}

	#endregion

	#region Integration Tests

	[Fact]
	public void FullColorWorkflow_GenerateAndParse()
	{
		// Arrange
		var originalColor = TQColor.Purple;

		// Act - Generate tag, wrap in text, parse back
		var tag = originalColor.ColorTag();
		var taggedText = $"{tag}Purple Item";
		var parsedColor = taggedText.GetColorFromTaggedString();

		// Assert
		parsedColor.Should().Be(originalColor);
	}

	[Fact]
	public void RemoveLeadingColorTag_WithColoredText_ProducesCleanText()
	{
		// Arrange
		var originalColor = TQColor.Green;
		var tag = originalColor.ColorTag();
		var coloredText = $"{tag}This is green text";

		// Act
		var cleanText = coloredText.RemoveLeadingColorTag();

		// Assert
		cleanText.Should().Be("This is green text");
	}

	[Fact]
	public void GenerateColoredItemName()
	{
		// Arrange
		var itemName = "Legendary Sword";
		var color = TQColor.Yellow;

		// Act
		var coloredItem = $"{color.ColorTag()}{itemName}";
		var extractedColor = coloredItem.GetColorFromTaggedString();

		// Assert
		extractedColor.Should().Be(color);
		coloredItem.Should().Contain(itemName);
	}

	#endregion
}
