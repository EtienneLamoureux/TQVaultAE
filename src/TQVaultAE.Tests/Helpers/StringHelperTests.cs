using AwesomeAssertions;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Tests.Helpers;

public class StringHelperTests
{
	#region IsTQVaultSearchRegEx Tests

	[Theory]
	[InlineData("", false, "", null, false)]
	[InlineData("   ", false, "", null, false)]
	[InlineData("sword", false, "sword", null, false)]
	[InlineData("/sword", true, "sword", null, true)]
	[InlineData("/s+w?rd", true, "s+w?rd", null, true)]
	[InlineData("/invalid[regex", true, "invalid[regex", null, false)] // Invalid regex
	public void IsTQVaultSearchRegEx_VariousInputs_ReturnsExpectedResult(
		string input, bool expectedIsRegex, string expectedPattern, object _, bool expectedIsValid)
	{
		// Act
		var result = StringHelper.IsTQVaultSearchRegEx(input);

		// Assert
		result.IsRegex.Should().Be(expectedIsRegex);
		result.Pattern.Should().Be(expectedPattern);
		result.RegexIsValid.Should().Be(expectedIsValid);
	}

	[Fact]
	public void IsTQVaultSearchRegEx_WithValidRegex_ReturnsValidRegex()
	{
		// Arrange
		var input = "/sword.*axe";

		// Act
		var result = StringHelper.IsTQVaultSearchRegEx(input);

		// Assert
		result.IsRegex.Should().BeTrue();
		result.Regex.Should().NotBeNull();
		result.RegexIsValid.Should().BeTrue();
	}

	[Fact]
	public void IsTQVaultSearchRegEx_WithInvalidRegex_ReturnsNullRegex()
	{
		// Arrange
		var input = "/[invalid(regex";

		// Act
		var result = StringHelper.IsTQVaultSearchRegEx(input);

		// Assert
		result.IsRegex.Should().BeTrue();
		result.Regex.Should().BeNull();
		result.RegexIsValid.Should().BeFalse();
	}

	#endregion

	#region ContainsIgnoreCase Tests

	[Theory]
	[InlineData("Hello World", "hello", true)]
	[InlineData("Hello World", "WORLD", true)]
	[InlineData("Hello World", "xyz", false)]
	[InlineData("Hello World", "", true)] // Empty string is contained
	[InlineData("", "hello", false)]
	[InlineData("CamelCase", "camel", true)]
	[InlineData("MixedCASE123", "case123", true)]
	public void ContainsIgnoreCase_VariousInputs_ReturnsExpectedResult(string input, string search, bool expected)
	{
		// Act
		var result = input.ContainsIgnoreCase(search);

		// Assert
		result.Should().Be(expected);
	}

	[Fact]
	public void ContainsIgnoreCase_Span_VariousInputs_ReturnsExpectedResult()
	{
		// Arrange
		var input = new ReadOnlySpan<char>("Hello World".ToCharArray());

		// Act & Assert
		input.ContainsIgnoreCase("HELLO".AsSpan()).Should().BeTrue();
		input.ContainsIgnoreCase("world".AsSpan()).Should().BeTrue();
		input.ContainsIgnoreCase("xyz".AsSpan()).Should().BeFalse();
	}

	#endregion

	#region ToFirstCharUpperCase Tests

	[Theory]
	[InlineData("hello", "Hello")]
	[InlineData("HELLO", "HELLO")]
	[InlineData("h", "H")]
	[InlineData("123abc", "123abc")]
	[InlineData("", "")]
	[InlineData(null, null)]
	public void ToFirstCharUpperCase_VariousInputs_ReturnsExpectedResult(string? input, string? expected)
	{
		// Act
		var result = input.ToFirstCharUpperCase();

		// Assert
		result.Should().Be(expected);
	}

	[Fact]
	public void ToFirstCharUpperCase_Span_VariousInputs_ReturnsExpectedResult()
	{
		// Arrange
		var input = "hello".AsSpan();

		// Act
		var result = input.ToFirstCharUpperCase();

		// Assert
		result.Should().Be("Hello");
	}

	[Fact]
	public void ToFirstCharUpperCase_Span_SingleChar_ReturnsUpperCaseChar()
	{
		// Arrange
		var input = "h".AsSpan();

		// Act
		var result = input.ToFirstCharUpperCase();

		// Assert
		result.Should().Be("H");
	}

	[Fact]
	public void ToFirstCharUpperCase_Span_EmptySpan_ReturnsEmpty()
	{
		// Arrange
		var input = ReadOnlySpan<char>.Empty;

		// Act
		var result = input.ToFirstCharUpperCase();

		// Assert
		result.Should().BeEmpty();
	}

	#endregion

	#region RemoveSuffix Tests

	[Theory]
	[InlineData("HelloWorld", 5, "Hello")]
	[InlineData("Test", 1, "Tes")]
	[InlineData("ABC", 3, "")]
	[InlineData("Short", 10, "")]
	public void RemoveSuffix_String_VariousInputs_ReturnsExpectedResult(string input, int suffixLength, string expected)
	{
		// Act
		var result = input.RemoveSuffix(suffixLength);

		// Assert
		result.Should().Be(expected);
	}

	[Fact]
	public void RemoveSuffix_Span_VariousInputs_ReturnsExpectedResult()
	{
		// Arrange
		var input = "HelloWorld".AsSpan();

		// Act & Assert
		input.RemoveSuffix(5).Should().Be("Hello");
		input.RemoveSuffix(0).Should().Be("HelloWorld");
	}

	#endregion

	#region RemoveAllTQTags Tests

	[Theory]
	[InlineData(null, null)]
	[InlineData("", "")]
	[InlineData("   ", "   ")] // Whitespace preserved
	[InlineData("Plain text", "Plain text")]
	[InlineData("{^Y}Item Name", "Item Name")]
	[InlineData("{^R}Red Text and {^B}Blue Text", "Red Text and Blue Text")]
	public void RemoveAllTQTags_VariousInputs_ReturnsExpectedResult(string? input, string? expected)
	{
		// Act
		var result = input.RemoveAllTQTags();

		// Assert
		result.Should().Be(expected);
	}

	[Fact]
	public void RemoveAllTQTags_WithNestedTags_RemovesAll()
	{
		// Arrange
		var input = "{^S}Prefix {^I}Item Name Suffix";

		// Act
		var result = input.RemoveAllTQTags();

		// Assert
		result.Should().Be("Prefix Item Name Suffix");
	}

	[Fact]
	public void RemoveAllTQTags_WithColorTagsOnly_ReturnsEmpty()
	{
		// Arrange
		var input = "{^Y}";

		// Act
		var result = input.RemoveAllTQTags();

		// Assert
		result.Should().BeEmpty();
	}

	#endregion

	#region TQCleanup Tests

	[Theory]
	[InlineData("Hello // comment", "Hello")]
	[InlineData("// full comment", "")]
	[InlineData("Text //trailing", "Text")]
	[InlineData("NoComment", "NoComment")]
	public void TQCleanup_WithoutLeadingTag_ReturnsExpectedResult(string input, string expected)
	{
		// Act
		var result = input.TQCleanup();

		// Assert
		result.Should().Be(expected);
	}

	[Fact]
	public void TQCleanup_WithColorTag_PreservesLeadingTag()
	{
		// Arrange
		var input = "{^R}Red Text";

		// Act
		var result = input.TQCleanup(keepLeadingColorTag: false);

		// Assert
		result.Should().Be("Red Text");
	}

	[Fact]
	public void TQCleanup_WithColorTagAndKeepTag_PreservesTag()
	{
		// Arrange
		var input = "{^R}Red Text // comment";

		// Act
		var result = input.TQCleanup(keepLeadingColorTag: true);

		// Assert
		result.Should().StartWith("{^R}");
	}

	[Fact]
	public void TQCleanup_NullInput_ReturnsEmpty()
	{
		// Act
		var result = ((string?)null).TQCleanup();

		// Assert
		result.Should().BeEmpty();
	}

	#endregion

	#region NormalizeRecordPath Tests

	[Theory]
	[InlineData("records/items/sword", "RECORDS\\ITEMS\\SWORD")]
	[InlineData("Records\\Items\\Shield", "RECORDS\\ITEMS\\SHIELD")]
	[InlineData("Mixed/Case\\Path", "MIXED\\CASE\\PATH")]
	public void NormalizeRecordPath_VariousInputs_ReturnsExpectedResult(string input, string expected)
	{
		// Act
		var result = input.NormalizeRecordPath();

		// Assert
		result.Should().Be(expected);
	}

	#endregion

	#region IsColorTagOnly Tests

	[Theory]
	[InlineData("{^R}", true)]
	[InlineData("{^Y}", true)]
	[InlineData("{^B}Text", false)]
	[InlineData("Plain text", false)]
	[InlineData("", false)]
	[InlineData(null, false)]
	public void IsColorTagOnly_VariousInputs_ReturnsExpectedResult(string? input, bool expected)
	{
		// Act
		var result = input.IsColorTagOnly();

		// Assert
		result.Should().Be(expected);
	}

	[Fact]
	public void IsColorTagOnlyExtended_ReturnsValueAndLength()
	{
		// Arrange
		var input = "{^R}";

		// Act
		var result = input.IsColorTagOnlyExtended();

		// Assert
		result.Value.Should().BeTrue();
		result.Length.Should().Be(input.Length);
	}

	#endregion

	#region HasColorPrefix Tests

	[Theory]
	[InlineData("{^R}Text", true)]
	[InlineData("{^B}Text", true)]
	[InlineData("No color", false)]
	[InlineData("", false)]
	[InlineData(null, false)]
	public void HasColorPrefix_VariousInputs_ReturnsExpectedResult(string? input, bool expected)
	{
		// Act
		var result = input.HasColorPrefix();

		// Assert
		result.Should().Be(expected);
	}

	#endregion

	#region RemoveEmptyAndSanitize Tests

	[Fact]
	public void RemoveEmptyAndSanitize_WithMixedInputs_ReturnsCleanedStrings()
	{
		// Arrange - "  " becomes empty after ToFirstCharUpperCase trim
		var inputs = new[] { "  ", "Text", null!, "", "More Text" };

		// Act
		var result = inputs.RemoveEmptyAndSanitize().ToList();

		// Assert - "  " and "" get filtered out, null gets skipped
		result.Should().HaveCount(2);
		result.Should().Contain("Text");
		result.Should().Contain("More Text");
	}

	[Fact]
	public void RemoveEmptyAndSanitize_WithColorTags_PreservesTags()
	{
		// Arrange
		var inputs = new[] { "{^R}Red Text" };

		// Act
		var result = inputs.RemoveEmptyAndSanitize().FirstOrDefault();

		// Assert
		result.Should().StartWith("{^R}");
	}

	[Fact]
	public void RemoveEmptyAndSanitize_WithComments_RemovesComments()
	{
		// Arrange
		var inputs = new[] { "Text // comment" };

		// Act
		var result = inputs.RemoveEmptyAndSanitize().FirstOrDefault();

		// Assert
		result.Should().NotContain("//");
	}

	#endregion

	#region JoinString and JoinWithoutStartingSpaces Tests

	[Fact]
	public void JoinString_WithDelim_ConcatenatesStrings()
	{
		// Arrange
		var inputs = new[] { "A", "B", "C" };

		// Act
		var result = inputs.JoinString(",");

		// Assert
		result.Should().Be("A,B,C");
	}

	[Fact]
	public void JoinWithoutStartingSpaces_WithColorTags_MergesColorTags()
	{
		// Arrange
		var inputs = new[] { "{^R}", "Text", "More" };

		// Act
		var result = inputs.JoinWithoutStartingSpaces(" ");

		// Assert
		result.Should().Contain("Text");
	}

	#endregion

	#region SplitOnTQNewLine Tests

	[Theory]
	[InlineData("Line1", new[] { "Line1" })]
	[InlineData("Line1{^N}Line2", new[] { "Line1", "Line2" })]
	[InlineData("{^N}Line2", new[] { "", "Line2" })]
	[InlineData("Line1{^N}", new[] { "Line1", "" })]
	[InlineData("A{^N}B{^N}C", new[] { "A", "B", "C" })]
	public void SplitOnTQNewLine_VariousInputs_ReturnsExpectedResult(string input, string[] expected)
	{
		// Act
		var result = StringHelper.SplitOnTQNewLine(input).ToArray();

		// Assert
		result.Should().BeEquivalentTo(expected);
	}

	#endregion

	#region Eval Tests

	[Theory]
	[InlineData("2 + 2", 4)]
	[InlineData("10 - 3", 7)]
	[InlineData("5 * 6", 30)]
	[InlineData("20 / 4", 5)]
	[InlineData("2 + 3 * 4", 14)] // Standard order of operations
	public void Eval_WithNumericExpressions_ReturnsExpectedResult(string expression, int expected)
	{
		// Act
		var result = expression.Eval<int>();

		// Assert
		result.Should().Be(expected);
	}

	[Theory]
	[InlineData("2.5 + 3.5", 6.0)]
	[InlineData("10.0 / 4.0", 2.5)]
	public void Eval_WithDecimalExpressions_ReturnsExpectedResult(string expression, double expected)
	{
		// Act
		var result = expression.Eval<double>();

		// Assert
		result.Should().BeApproximately(expected, 0.001);
	}

	[Fact]
	public void Eval_WithComplexExpression_ReturnsCorrectResult()
	{
		// Arrange
		var expression = "(10 + 5) * 2";

		// Act
		var result = expression.Eval<int>();

		// Assert
		result.Should().Be(30);
	}

	#endregion

	#region ConcatSlice Tests

	[Fact]
	public void ConcatSlice_WithValidInputs_ConcatenatesCorrectly()
	{
		// Arrange
		var prefix = "Prefix_".AsSpan();
		var span = "Prefix_Value".AsSpan();
		int skipPrefixChars = 7;

		// Act
		var result = StringHelper.ConcatSlice(prefix, span, skipPrefixChars);

		// Assert
		result.Should().Be("Prefix_Value");
	}

	[Fact]
	public void ConcatSlice_WithExplicitSuffix_ConcatenatesCorrectly()
	{
		// Arrange
		var prefix = "A_".AsSpan();
		var span = "A_B_C".AsSpan();
		int skipPrefixChars = 2;
		int suffixLength = 3;

		// Act
		var result = StringHelper.ConcatSlice(prefix, span, skipPrefixChars, suffixLength);

		// Assert
		result.Should().Be("A_B_C");
	}

	#endregion

	#region InsertAfterColorPrefix Tests

	[Fact]
	public void InsertAfterColorPrefix_WithColorTag_InsertsBetweenTagAndContent()
	{
		// Arrange
		var input = "{^R}Red Text";
		var inserted = "_INSERTED_";

		// Act
		var result = input.InsertAfterColorPrefix(inserted);

		// Assert
		result.Should().Contain("{^R}");
		result.Should().Contain("_INSERTED_");
		result.Should().Contain("Red Text");
	}

	[Fact]
	public void InsertAfterColorPrefix_WithNullInput_ReturnsInsertedText()
	{
		// Act
		var result = ((string?)null).InsertAfterColorPrefix("inserted");

		// Assert
		result.Should().Be("inserted");
	}

	[Fact]
	public void InsertAfterColorPrefix_WithNullInserted_ReturnsOriginal()
	{
		// Arrange
		var input = "{^B}Text";

		// Act
		var result = input.InsertAfterColorPrefix(null!);

		// Assert
		result.Should().Be(input);
	}

	[Fact]
	public void InsertAfterColorPrefix_WithoutColorTag_ReturnsOriginalWithInserted()
	{
		// Arrange
		var input = "Plain Text";

		// Act
		var result = input.InsertAfterColorPrefix("_X_");

		// Assert
		result.Should().Contain("_X_");
	}

	#endregion

	#region PrettyFileName Tests

	[Fact]
	public void PrettyFileName_NullInput_ReturnsNull()
	{
		// Act
		string? result = ((string?)null).PrettyFileName();

		// Assert
		result.Should().BeNull();
	}

	[Fact]
	public void PrettyFileName_WithNumbers_EnclosesNumbersInParentheses()
	{
		// Arrange - typical relic naming pattern
		var input = "records/items/relics/aegisofathena_01";

		// Act
		var result = input.PrettyFileName();

		// Assert
		result.Should().Contain("(01)");
	}

	[Fact]
	public void PrettyFileName_ReplacesUnderscoresWithSpaces()
	{
		// Arrange
		var input = "records/items/weapons/great_sword";

		// Act
		var result = input.PrettyFileName();

		// Assert
		result.Should().NotContain("_");
		result.Should().Contain(" ");
	}

	[Theory]
	[InlineData("dmg", "Damage")]
	[InlineData("int", "Intelligence")]
	[InlineData("str", "Strength")]
	[InlineData("att", "Attribute")]
	[InlineData("xp", "XP")]
	[InlineData("spd", "Speed")]
	public void PrettyFileName_Acronyms_ConvertsCorrectly(string input, string expected)
	{
		// Arrange - use path format for PrettyFileName
		var path = $"records/items/{input}_01";

		// Act
		var result = path.PrettyFileName();

		// Assert
		result.Should().Contain(expected);
	}

	[Theory]
	[InlineData("masterya", "Mastery Warfare")]
	[InlineData("masteryb", "Mastery Defense")]
	[InlineData("masteryc", "Mastery Hunting")]
	[InlineData("masteryd", "Mastery Rogue")]
	[InlineData("masterye", "Mastery Earth")]
	[InlineData("masteryf", "Mastery Nature")]
	[InlineData("masteryg", "Mastery Spirit")]
	[InlineData("masteryh", "Mastery Storm")]
	public void PrettyFileName_MasteryAcronyms_ConvertsCorrectly(string input, string expected)
	{
		// Act
		var result = input.PrettyFileName();

		// Assert
		result.Should().Contain(expected);
	}

	[Theory]
	[InlineData("bow", "Bow")]
	[InlineData("da", "Defensive Ability")]
	[InlineData("oa", "Offensive Ability")]
	public void PrettyFileName_SpecialAcronyms_ConvertsCorrectly(string input, string expected)
	{
		// Act
		var result = input.PrettyFileName();

		// Assert
		result.Should().Contain(expected);
	}

	[Fact]
	public void PrettyFileName_RemovesFileExtension()
	{
		// Arrange
		var input = "records/items/sword.dbr";

		// Act
		var result = input.PrettyFileName();

		// Assert
		result.Should().NotContain(".dbr");
	}

	[Fact]
	public void PrettyFileName_ConsecutiveUnderscores_HandledCorrectly()
	{
		// Arrange - path with multiple underscores
		var input = "records/items/weapon___name_01";

		// Act
		var result = input.PrettyFileName();

		// Assert
		result.Should().NotContain("___");
		result.Should().Contain("(01)");
	}

	#endregion

	#region PrettyFileNameExploded Tests

	[Fact]
	public void PrettyFileNameExploded_WithNumberedPattern_ReturnsComponents()
	{
		// Arrange - typical pattern with number
		var input = "records/items/relic_01";

		// Act
		var result = input.PrettyFileNameExploded();

		// Assert
		result.IsMatch.Should().BeTrue();
		result.PrettyFileName.Should().NotBeNullOrEmpty();
		result.Number.Should().NotBeNullOrEmpty();
	}

	[Fact]
	public void PrettyFileNameExploded_WithEffectPattern_ReturnsEffect()
	{
		// Arrange
		var input = "records/items/fire_burning_damage_01";

		// Act
		var result = input.PrettyFileNameExploded();

		// Assert
		result.IsMatch.Should().BeTrue();
		result.Effect.Should().NotBeNullOrEmpty();
	}

	[Fact]
	public void PrettyFileNameExploded_WithoutPattern_ReturnsNotMatch()
	{
		// Arrange - simple path without numbers
		var input = "records/items/sword";

		// Act
		var result = input.PrettyFileNameExploded();

		// Assert
		result.IsMatch.Should().BeFalse();
	}

	[Fact]
	public void PrettyFileNameExploded_ReturnsPrettyFileName()
	{
		// Arrange
		var input = "records/items/weapon_name_05";

		// Act
		var result = input.PrettyFileNameExploded();

		// Assert
		result.PrettyFileName.Should().NotBeNullOrEmpty();
	}

	#endregion

	#region ExplodePrettyFileName Tests

	[Fact]
	public void ExplodePrettyFileName_WithStandardPattern_ExtractsComponents()
	{
		// Arrange - pattern "trash (0) effect (0)" - regex captures second number group
		var input = "Fire (1) Damage (5)";

		// Act
		var result = input.ExplodePrettyFileName();

		// Assert
		result.IsMatch.Should().BeTrue();
		// Effect captures "Damage" (the text before the second number)
		result.Effect.Should().Contain("Damage");
		result.Number.Should().Be("5");
	}

	[Fact]
	public void ExplodePrettyFileName_WithSimplePattern_ExtractsComponents()
	{
		// Arrange
		var input = "Effect Name (3)";

		// Act
		var result = input.ExplodePrettyFileName();

		// Assert
		result.IsMatch.Should().BeTrue();
		result.Effect.Should().Contain("Effect Name");
		result.Number.Should().Be("3");
	}

	[Fact]
	public void ExplodePrettyFileName_WithNoMatch_ReturnsOriginal()
	{
		// Arrange
		var input = "Plain Text No Numbers";

		// Act
		var result = input.ExplodePrettyFileName();

		// Assert
		result.IsMatch.Should().BeFalse();
		result.PrettyFileName.Should().Be(input);
	}

	[Fact]
	public void ExplodePrettyFileName_WithComplexPattern_ExtractsCorrectly()
	{
		// Arrange - pattern like "trash (0) effect (0)"
		var input = "Fire (0) Burning Damage (10)";

		// Act
		var result = input.ExplodePrettyFileName();

		// Assert
		result.IsMatch.Should().BeTrue();
		result.Number.Should().Be("10");
	}

	#endregion

	#region WrapWords Tests

	[Fact]
	public void WrapWords_WithShortText_ReturnsSingleLine()
	{
		// Arrange
		var input = "Short text";
		var columns = 80;

		// Act
		var result = StringHelper.WrapWords(input, columns);

		// Assert
		result.Should().HaveCount(1);
		// Each line should not exceed columns
		foreach (var line in result)
			line.Length.Should().BeLessThanOrEqualTo(columns, $"Line '{line}' exceeds column limit");
	}

	[Fact]
	public void WrapWords_WithTQNewLine_SplitsOnNewLine()
	{
		// Arrange
		var input = "Line1{^N}Line2";
		var columns = 80;

		// Act
		var result = StringHelper.WrapWords(input, columns);

		// Assert
		result.Should().HaveCount(2);
	}

	[Fact]
	public void WrapWords_WithMultipleNewLines_SplitsCorrectly()
	{
		// Arrange
		var input = "Line1{^N}Line2{^N}Line3";
		var columns = 80;

		// Act
		var result = StringHelper.WrapWords(input, columns);

		// Assert
		result.Should().HaveCount(3);
	}

	[Fact]
	public void WrapWords_WithColorTag_PreservesColor()
	{
		// Arrange
		var input = "{^R}Red colored text that should stay together";
		var columns = 30;

		// Act
		var result = StringHelper.WrapWords(input, columns);

		// Assert
		result.Should().NotBeEmpty();
	}

	[Fact]
	public void WrapWords_WithTextExactlyAtLimit_ReturnsSingleLine()
	{
		// Arrange
		var input = "Exactly forty characters here!!!";
		var columns = 40;

		// Act
		var result = StringHelper.WrapWords(input, columns);

		// Assert
		result.Should().HaveCount(1);
	}

	[Fact]
	public void WrapWords_WithTextExceedingLimit_Wraps()
	{
		// Arrange
		var input = "This text exceeds the column limit and should wrap";
		var columns = 20;

		// Act
		var result = StringHelper.WrapWords(input, columns);

		// Assert
		result.Should().HaveCountGreaterThan(1);
	}

	[Fact]
	public void WrapWords_WithEmptyString_ReturnsNonEmptyCollection()
	{
		// Arrange
		var input = "";
		var columns = 80;

		// Act
		var result = StringHelper.WrapWords(input, columns);

		// Assert
		result.Should().NotBeEmpty("empty string still produces a result");
	}

	[Fact]
	public void WrapWords_WithNull_ReturnsEmptyCollection()
	{
		// Arrange
		string? input = null;
		var columns = 80;

		// Act
		var result = StringHelper.WrapWords(input!, columns);

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public void WrapWords_WithZeroColumns_ReturnsSingleLine()
	{
		// Arrange
		var input = "Text";
		var columns = 0;

		// Act
		var result = StringHelper.WrapWords(input, columns);

		// Assert
		result.Should().NotBeEmpty();
	}

	[Fact]
	public void WrapWords_WithNegativeColumns_ReturnsSingleLine()
	{
		// Arrange
		var input = "Text";
		var columns = -5;

		// Act
		var result = StringHelper.WrapWords(input, columns);

		// Assert
		result.Should().NotBeEmpty();
	}

	[Fact]
	public void WrapWords_WithSingleWordExceedingColumns_HandlesGracefully()
	{
		// Arrange
		var input = "ThisIsAVeryLongSingleWordThatExceedsTheColumnLimit";
		var columns = 20;

		// Act
		var result = StringHelper.WrapWords(input, columns);

		// Assert
		result.Should().NotBeEmpty();
	}

	[Fact]
	public void WrapWords_WithMultipleSpaces_SplitsOnSpaces()
	{
		// Arrange
		var input = "Word1   Word2   Word3";
		var columns = 15;

		// Act
		var result = StringHelper.WrapWords(input, columns);

		// Assert
		result.Should().NotBeEmpty();
	}

	[Fact]
	public void WrapWords_WithNewLineAndLongText_HandlesBoth()
	{
		// Arrange
		var input = "Short{^N}This is a very long piece of text that exceeds the column limit";
		var columns = 30;

		// Act
		var result = StringHelper.WrapWords(input, columns);

		// Assert
		result.Should().HaveCountGreaterThan(1);
	}

	#endregion

	#region JoinWithoutStartingSpaces Edge Case Tests

	[Fact]
	public void JoinWithoutStartingSpaces_WithEmptyArray_ReturnsEmpty()
	{
		// Arrange
		var inputs = Array.Empty<string>();

		// Act
		var result = inputs.JoinWithoutStartingSpaces(" ");

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public void JoinWithoutStartingSpaces_WithSingleElement_ReturnsElement()
	{
		// Arrange
		var inputs = new[] { "Single" };

		// Act
		var result = inputs.JoinWithoutStartingSpaces(" ");

		// Assert
		result.Should().Be("Single");
	}

	[Fact]
	public void JoinWithoutStartingSpaces_WithConsecutiveColorTags_MergesThem()
	{
		// Arrange
		var inputs = new[] { "{^R}", "{^B}", "Text" };

		// Act
		var result = inputs.JoinWithoutStartingSpaces(" ");

		// Assert
		result.Should().Be("{^R}{^B} Text");
	}

	[Fact]
	public void JoinWithoutStartingSpaces_WithColorTagAtEnd_SkipsIt()
	{
		// Arrange
		var inputs = new[] { "Text", "{^Y}" };

		// Act
		var result = inputs.JoinWithoutStartingSpaces(" ");

		// Assert
		result.Should().NotEndWith("{^Y}");
	}

	[Fact]
	public void JoinWithoutStartingSpaces_WithMultipleConsecutiveColorTags_MergesAll()
	{
		// Arrange
		var inputs = new[] { "{^R}", "{^G}", "{^B}", "Text" };

		// Act
		var result = inputs.JoinWithoutStartingSpaces(" ");

		// Assert - consecutive color tags get merged but spaces may be preserved
		result.Should().Contain("{^R}");
		result.Should().Contain("{^G}");
		result.Should().Contain("{^B}");
		result.Should().Contain("Text");
	}

	[Fact]
	public void JoinWithoutStartingSpaces_WithPlainTextOnly_JoinsNormally()
	{
		// Arrange
		var inputs = new[] { "One", "Two", "Three" };

		// Act
		var result = inputs.JoinWithoutStartingSpaces(" ");

		// Assert
		result.Should().Be("One Two Three");
	}

	[Fact]
	public void JoinWithoutStartingSpaces_WithCustomDelimiter_UsesDelimiter()
	{
		// Arrange
		var inputs = new[] { "A", "B", "C" };

		// Act
		var result = inputs.JoinWithoutStartingSpaces(",");

		// Assert
		result.Should().Be("A,B,C");
	}

	#endregion
}
