using AwesomeAssertions;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Tests.Helpers;

public class SpanHelperTests
{
	#region ConcatWithSlice Tests

	[Fact]
	public void ConcatWithSlice_WithValidPrefixAndSlice_ReturnsConcatenated()
	{
		// Arrange
		var prefix = "Hello ".AsSpan();
		var span = "Hello World".AsSpan();
		var skipPrefixChars = 6;

		// Act
		var result = SpanHelper.ConcatWithSlice(prefix, span, skipPrefixChars);

		// Assert
		result.Should().Be("Hello World");
	}

	[Fact]
	public void ConcatWithSlice_WithSkipPrefixChars_ReturnsWithoutPrefix()
	{
		// Arrange
		var prefix = "PREFIX:".AsSpan();
		var span = "PREFIX:VALUE".AsSpan();
		var skipPrefixChars = 7;

		// Act
		var result = SpanHelper.ConcatWithSlice(prefix, span, skipPrefixChars);

		// Assert
		result.Should().Be("PREFIX:VALUE");
	}

	[Theory]
	[InlineData("Item: ", "Weapon:Sword:Steel", 0, 6, "Item: Weapon")]
	[InlineData("[", "[Name:Value]", 1, 5, "[Name:")]
	public void ConcatWithSlice_WithSuffixLength_ReturnsCorrectSlice(
		string prefix, string span, int skipPrefixChars, int suffixLength, string expected)
	{
		// Act
		var result = SpanHelper.ConcatWithSlice(prefix.AsSpan(), span.AsSpan(), skipPrefixChars, suffixLength);

		// Assert
		result.Should().Be(expected);
	}

	#endregion
}
