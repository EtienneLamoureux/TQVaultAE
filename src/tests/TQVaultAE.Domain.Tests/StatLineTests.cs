using AwesomeAssertions;
using TQVaultAE.Domain.Comparison;

namespace TQVaultAE.Tests.Domain;

/// <summary>
/// <see cref="StatLine"/>: a displayed line split into the stat it describes and its numbers.
/// </summary>
public class StatLineTests
{
	private const char S = StatLine.Slot;

	[Fact]
	public void Parse_SignedFlatStat_KeepsTheSignOutOfTheTemplate()
	{
		var line = StatLine.Parse("+42 Strength")!;

		line.Template.Should().Be($"{S} Strength");
		line.Values.Should().Equal(42m);
	}

	[Fact]
	public void Parse_DamageRange_DashIsNotAMinusSign()
	{
		var line = StatLine.Parse("10-20 Fire Damage")!;

		line.Template.Should().Be($"{S}-{S} Fire Damage");
		line.Values.Should().Equal(10m, 20m);
	}

	[Theory]
	[InlineData("-10% Reduced Damage")]
	[InlineData("−10% Reduced Damage")]
	public void Parse_LeadingMinus_IsANegativeValue(string text)
		=> StatLine.Parse(text)!.Values.Should().Equal(-10m);

	[Fact]
	public void Parse_ColorTagsAndLineBreaks_AreNotPartOfTheStat()
	{
		var tagged = StatLine.Parse("{^Y}770 Armor{^N} ")!;
		var plain = StatLine.Parse("700 Armor")!;

		tagged.Key.Should().Be(plain.Key);
		tagged.Values.Should().Equal(770m);
	}

	[Fact]
	public void Parse_Decimals_RecordsEachNumbersPrecision()
	{
		var line = StatLine.Parse("20.0% Chance of 111 Bleeding Damage Retaliation over 3.0 Seconds")!;

		line.Values.Should().Equal(20m, 111m, 3m);
		line.Decimals.Should().Equal(1, 0, 1);
		line.DecimalSeparator.Should().Be('.');
	}

	[Fact]
	public void Parse_CommaDecimalSeparator_IsReadAndKeptForFormatting()
	{
		var line = StatLine.Parse("+1,5 Régénération de santé")!;

		line.Values.Should().Equal(1.5m);
		line.Format([0.5m], [1]).Should().Be("+0,5 Régénération de santé");
	}

	[Fact]
	public void Parse_IndentedLine_BelongsToItsParentStat()
	{
		var parent = StatLine.Parse("25% Chance of:")!;
		var child = StatLine.Parse("    +10% Attack Speed", parent.Template)!;
		var alone = StatLine.Parse("+10% Attack Speed")!;

		child.Indented.Should().BeTrue();
		child.Key.Should().NotBe(alone.Key, "an effect inside a 'Chance of' group is not the item's own stat");
		child.Template.Should().Be(alone.Template);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData("{^Y}")]
	public void Parse_NoText_IsNotAStat(string? text)
		=> StatLine.Parse(text).Should().BeNull();

	[Fact]
	public void Parse_DifferentWhiteSpace_SameStat()
		=> StatLine.Parse("Level:   3")!.Key.Should().Be(StatLine.Parse("Level: 5")!.Key);

	[Theory]
	[InlineData(5, 0, "+5")]
	[InlineData(-2.5, 1, "-2.5")]
	[InlineData(0, 1, "±0.0")]
	[InlineData(1, 1, "+1.0")]
	public void FormatSigned_AlwaysShowsTheSign(decimal value, int decimals, string expected)
		=> StatLine.FormatSigned(value, decimals).Should().Be(expected);

	[Fact]
	public void Format_PutsEachValueInItsSlot()
		=> StatLine.Parse("20.0% Chance of 111 Bleeding Damage over 3.0 Seconds")!
			.Format([5m, -11m, 0m], [1, 0, 1])
			.Should().Be("+5.0% Chance of -11 Bleeding Damage over ±0.0 Seconds");

	[Fact]
	public void FormatDifference_UnchangedNumber_ShowsTheLinesOwnValueWithoutSign()
		=> StatLine.Parse("20.0% Chance of 111 Bleeding Damage over 3.0 Seconds")!
			.FormatDifference([5m, -11m, 0m], [1, 0, 1])
			.Should().Be("+5.0% Chance of -11 Bleeding Damage over 3.0 Seconds");
}
