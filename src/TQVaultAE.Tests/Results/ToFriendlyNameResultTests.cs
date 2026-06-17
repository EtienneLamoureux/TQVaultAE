using System.Text.RegularExpressions;
using AwesomeAssertions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Results;

namespace TQVaultAE.Tests.Results;

/// <summary>
/// Unit tests for ToFriendlyNameResult.FulltextIsMatch methods.
/// Note: Most tests require Item to have baseItemInfo set, which is complex to mock.
/// We test only the guard clauses and edge cases that don't require full Item setup.
/// Full integration tests would need database mocking.
/// </summary>
public class ToFriendlyNameResultTests
{
	/// <summary>
	/// Creates a ToFriendlyNameResult with minimal setup.
	/// Warning: Many methods still depend on Item properties that require baseItemInfo.
	/// We only test guard clauses here.
	/// </summary>
	private ToFriendlyNameResult CreateMinimalResult()
	{
		var item = new Item();
		return new ToFriendlyNameResult(item);
	}

	#region FulltextIsMatch - Guard Clause Tests

	/// <summary>
	/// These tests verify the guard clauses that prevent processing invalid input.
	/// They don't require baseItemInfo to be set.
	/// </summary>

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData("\t")]
	[InlineData("\n")]
	public void FulltextIsMatch_WithInvalidSearch_ReturnsFalse(string search)
	{
		// Arrange
		var result = CreateMinimalResult();

		// Act
		var match = result.FulltextIsMatch(search);

		// Assert
		match.Should().BeFalse("null/empty/whitespace search should return false");
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public void FulltextIsMatchIndexOf_WithInvalidSearch_ReturnsFalse(string search)
	{
		// Arrange
		var result = CreateMinimalResult();

		// Act
		var match = result.FulltextIsMatchIndexOf(search);

		// Assert
		match.Should().BeFalse();
	}

	[Fact]
	public void FulltextIsMatchIndexOf_WithEmptySpan_ReturnsFalse()
	{
		// Arrange
		var result = CreateMinimalResult();
		var emptySpan = ReadOnlySpan<char>.Empty;

		// Act
		var match = result.FulltextIsMatchIndexOf(emptySpan);

		// Assert
		match.Should().BeFalse();
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public void FulltextIsMatchRegex_WithInvalidPattern_ReturnsFalse(string pattern)
	{
		// Arrange
		var result = CreateMinimalResult();

		// Act
		var match = result.FulltextIsMatchRegex(pattern);

		// Assert
		match.Should().BeFalse();
	}

	[Fact]
	public void FulltextIsMatchRegex_WithNullRegexObject_ReturnsFalse()
	{
		// Arrange
		var result = CreateMinimalResult();

		// Act
		var match = result.FulltextIsMatchRegex((Regex)null!);

		// Assert
		match.Should().BeFalse();
	}

	[Fact]
	public void FulltextIsMatchRegex_WithInvalidRegexPattern_ReturnsFalse()
	{
		// Arrange
		var result = CreateMinimalResult();

		// Act
		var match = result.FulltextIsMatchRegex("(invalid["); // Unbalanced bracket

		// Assert
		match.Should().BeFalse("invalid regex should return false, not throw");
	}

	[Fact]
	public void FulltextIsMatchRegex_WithInvalidRegexPattern2_ReturnsFalse()
	{
		// Arrange
		var result = CreateMinimalResult();

		// Act
		var match = result.FulltextIsMatchRegex("*invalid"); // Invalid quantifier

		// Assert
		match.Should().BeFalse();
	}

	[Fact]
	public void FulltextIsMatchRegexRegexObject_WithValidRegex_MatchesEmptyText()
	{
		// Arrange - test with empty text and valid regex
		var result = CreateMinimalResult();
		var regex = new Regex("test", RegexOptions.IgnoreCase);

		// Act
		var match = result.FulltextIsMatchRegex(regex);

		// Assert - should return false when no text matches
		match.Should().BeFalse("no text in fulltext matches");
	}

	[Fact]
	public void FulltextIsMatchRegexRegexObject_WithNullRegex_ReturnsFalse()
	{
		// Arrange
		var result = CreateMinimalResult();

		// Act
		var match = result.FulltextIsMatchRegex((Regex)null!);

		// Assert
		match.Should().BeFalse();
	}

	#endregion

	#region FulltextIsMatch - Guard Clause Coverage

	/// <summary>
	/// Verify that guard clauses prevent NullReferenceException
	/// when Item properties are not fully initialized.
	/// These tests would fail without proper guard clauses.
	/// </summary>

	[Fact]
	public void FulltextIsMatch_DoesNotThrow_WhenItemNotFullyInitialized()
	{
		// Arrange - Item with no baseItemInfo set
		var result = CreateMinimalResult();

		// Act & Assert
		var act = () => result.FulltextIsMatch("test");
		act.Should().NotThrow("guard clauses should prevent NullReferenceException");
	}

	[Fact]
	public void FulltextIsMatchIndexOf_DoesNotThrow_WhenItemNotFullyInitialized()
	{
		// Arrange
		var result = CreateMinimalResult();

		// Act & Assert
		var act = () => result.FulltextIsMatchIndexOf("test");
		act.Should().NotThrow();
	}

	[Fact]
	public void FulltextIsMatchIndexOfSpan_DoesNotThrow_WhenItemNotFullyInitialized()
	{
		// Arrange
		var result = CreateMinimalResult();

		// Act & Assert - pass span directly
		var act = () => result.FulltextIsMatchIndexOf("test".AsSpan());
		act.Should().NotThrow();
	}

	[Fact]
	public void FulltextIsMatchRegex_DoesNotThrow_WhenItemNotFullyInitialized()
	{
		// Arrange
		var result = CreateMinimalResult();

		// Act & Assert
		var act = () => result.FulltextIsMatchRegex("test.*");
		act.Should().NotThrow();
	}

	[Fact]
	public void FulltextIsMatchRegexString_DoesNotThrow_WhenItemNotFullyInitialized()
	{
		// Arrange
		var result = CreateMinimalResult();

		// Act & Assert
		var act = () => result.FulltextIsMatchRegex("test");
		act.Should().NotThrow();
	}

	[Fact]
	public void FulltextIsMatchRegexRegexObject_DoesNotThrow_WhenItemNotFullyInitialized()
	{
		// Arrange
		var result = CreateMinimalResult();
		var regex = new Regex("test");

		// Act & Assert
		var act = () => result.FulltextIsMatchRegex(regex);
		act.Should().NotThrow();
	}

	#endregion
}
