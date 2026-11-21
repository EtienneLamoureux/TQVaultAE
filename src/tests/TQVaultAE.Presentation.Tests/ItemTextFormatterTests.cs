using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Config;
using TQVaultAE.Presentation.ItemText;

namespace TQVaultAE.Tests.Presentation;

public class ItemTextFormatterTests
{
	private readonly ItemTextFormatter _formatter = new(
		new Mock<ILogger<ItemTextFormatter>>().Object,
		new Mock<IDatabase>().Object,
		new Mock<IItemProvider>().Object,
		new Mock<IItemAttributeProvider>().Object,
		new Mock<ITranslationService>().Object,
		new Mock<IPathIO>().Object,
		new UserSettings()
	);

	#region Tag Lists Tests

	[Theory]
	[InlineData("offensiveSlowRunSpeed", true)] // camel case, as in the game records
	[InlineData("OFFENSIVETOTALRESISTANCEREDUCTIONABSOLUTE", true)]
	[InlineData("offensivetotaldamagereductionpercent", true)]
	[InlineData("offensiveProjectileFumble", true)] // impaired aim, applied for a time
	[InlineData("retaliationSlowRunSpeed", true)]
	[InlineData("retaliationSlowTotalSpeed", false)] // the game lists it with the damage over time
	[InlineData("offensiveSlowFire", false)] // duration reliant
	[InlineData("OFFENSIVESLOWRUNSPEEDX", false)]
	public void DurationIndependentEffects_IgnoresCase(string effect, bool expected)
		=> ItemTextFormatter.durationIndependentEffects.Contains(effect).Should().Be(expected);

	#endregion

	#region Format Tests

	[Theory]
	[InlineData("Value: {0}", 42, null, null, "Value: 42")]
	[InlineData("{0} + {1} = {2}", 1, 2, null, "1 + 2 = ")] // param3 null leaves {2} empty
	[InlineData("{0}, {1}, {2}", "A", "B", "C", "A, B, C")]
	[InlineData("Value: {0}", null, null, null, "Value: ")] // null param becomes empty
	[InlineData("Damage: {0}", 100, null, null, "Damage: 100")]
	[InlineData("Multiplier: {0:F2}", 1.5, null, null, "Multiplier: 1.50")]
	public void Format_ReturnsCorrectFormattedString(string formatSpec, object? param1, object? param2, object? param3, string expected)
	{
		// Act
		var result = _formatter.Format(formatSpec, param1, param2, param3);

		// Assert
		result.Should().Be(expected);
	}

	[Fact]
	public void Format_WithInvalidFormatSpecifier_ReturnsTheRawLine_InsteadOfThrowing()
	{
		// A malformed translation must only spoil its own line, not the whole item text.
		var result = _formatter.Format("{0} {1} {999}", "test", null);

		result.Should().Be("""FormatErr("{0} {1} {999}", 'test', 'NULL', 'NULL'>""");
	}

	[Fact]
	public void Format_WithGameFormatSyntax_ReturnsTheRawLine_InsteadOfThrowing()
	{
		// An unconverted game format, like "{%.1f0}", is not a valid .NET format.
		var result = _formatter.Format("{%.1f0}% Chance of", 12.5f);

		result.Should().StartWith("""FormatErr("{%.1f0}% Chance of", '""");
	}

	#endregion
}
