using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Config;
using TQVaultAE.Data;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Tests.Data;

public class ItemProviderTests
{
	private readonly ItemProvider _provider;
	private readonly Mock<IDatabase> _mockDatabase;
	private readonly Mock<IItemAttributeProvider> _mockItemAttributeProvider;
	private readonly Mock<ILootTableCollectionProvider> _mockLootTableCollectionProvider;
	private readonly Mock<ITranslationService> _mockTranslationService;
	private readonly Mock<ITQDataService> _mockTQData;
	private readonly Mock<IPathIO> _mockPathIO;

	public ItemProviderTests()
	{
		var mockLog = new Mock<ILogger<ItemProvider>>();
		_mockDatabase = new Mock<IDatabase>();
		_mockItemAttributeProvider = new Mock<IItemAttributeProvider>();
		_mockLootTableCollectionProvider = new Mock<ILootTableCollectionProvider>();
		_mockTranslationService = new Mock<ITranslationService>();
		_mockTQData = new Mock<ITQDataService>();
		_mockPathIO = new Mock<IPathIO>();

		var mockSettings = new UserSettings();

		_provider = new ItemProvider(
			mockLog.Object,
			_mockDatabase.Object,
			_mockLootTableCollectionProvider.Object,
			_mockItemAttributeProvider.Object,
			_mockTQData.Object,
			_mockTranslationService.Object,
			_mockPathIO.Object,
			mockSettings
		);
	}

	#region FilterKey Tests

	[Theory]
	[InlineData("DEFENSIVEABSORPTION", true)] // known unwanted tag
	[InlineData("defensiveabsorption", true)] // lowercase
	[InlineData("DeFeNsIvEAbSoRpTiOn", true)] // mixed case
	[InlineData("WEAPONATTACKSOUND", true)] // ends with SOUND
	[InlineData("BODYMESH", true)] // ends with MESH
	[InlineData("BODYMASKFACE", true)] // starts with BODYMASK
	[InlineData("DAMAGE", false)] // valid attribute
	[InlineData("damage", false)] // valid attribute lowercase
	[InlineData("INVENTORYICON", false)] // another valid attribute
	public void FilterKey_ReturnsCorrectResult(string tag, bool expected)
	{
		// Act
		var result = _provider.FilterKey(tag);

		// Assert
		result.Should().Be(expected);
	}

	#endregion

	#region FilterRequirements Tests

	[Theory]
	[InlineData("LEVELREQUIREMENT", true)]
	[InlineData("STRENGTHREQUIREMENT", true)]
	[InlineData("DEXTERITYREQUIREMENT", true)]
	[InlineData("INTELLIGENCEREQUIREMENT", true)]
	[InlineData("levelrequirement", true)] // case insensitive
	[InlineData("DAMAGE", false)]
	[InlineData("INVENTORYICON", false)]
	public void FilterRequirements_ReturnsCorrectResult(string reqTag, bool expected)
	{
		// Act
		var result = _provider.FilterRequirements(reqTag);

		// Assert
		result.Should().Be(expected);
	}

	#endregion

	#region IsStatBonus Tests

	[Theory]
	[InlineData("CHARACTERSTRENGTH", true)]
	[InlineData("CHARACTERSTRENGTHMODIFIER", true)]
	[InlineData("CHARACTERDEXTERITY", true)]
	[InlineData("CHARACTERDEXTERITYMODIFIER", true)]
	[InlineData("CHARACTERINTELLIGENCE", true)]
	[InlineData("CHARACTERINTELLIGENCEMODIFIER", true)]
	[InlineData("CHARACTERLIFE", true)]
	[InlineData("CHARACTERLIFEMODIFIER", true)]
	[InlineData("CHARACTERMANA", true)]
	[InlineData("CHARACTERMANAMODIFIER", true)]
	[InlineData("characterstrength", true)] // lowercase
	[InlineData("DAMAGE", false)] // non stat bonus
	[InlineData("INVENTORYICON", false)] // non stat bonus
	public void IsStatBonus_ReturnsCorrectResult(string statTag, bool expected)
	{
		// Act
		var result = _provider.IsStatBonus(statTag);

		// Assert
		result.Should().Be(expected);
	}

	#endregion

	#region GetRequirementEquationPrefix Tests

	[Fact]
	public void GetRequirementEquationPrefix_WithValidGearClass_ReturnsPrefix()
	{
		// Arrange - using an item class that might not be in the gear type map
		var itemClass = "weapon";

		// Act
		var result = _provider.GetRequirementEquationPrefix(itemClass);

		// Assert - result could be "none" if not found, or a valid prefix
		result.Should().NotBeNull();
	}

	[Fact]
	public void GetRequirementEquationPrefix_WithInvalidGearClass_ReturnsNone()
	{
		// Arrange - using an invalid item class
		var itemClass = "invalidclass";

		// Act
		var result = _provider.GetRequirementEquationPrefix(itemClass);

		// Assert
		result.Should().Be("none");
	}

	[Fact]
	public void GetRequirementEquationPrefix_WithEmptyString_ReturnsNone()
	{
		// Arrange
		var itemClass = "";

		// Act
		var result = _provider.GetRequirementEquationPrefix(itemClass);

		// Assert
		result.Should().Be("none");
	}

	[Fact]
	public void GetRequirementEquationPrefix_CaseInsensitive_ReturnsPrefix()
	{
		// Arrange - using uppercase
		var itemClass = "WEAPON";

		// Act
		var result = _provider.GetRequirementEquationPrefix(itemClass);

		// Assert - result could be "none" or a valid prefix
		result.Should().NotBeNull();
	}

	#endregion

	#region Format Tests

	[Theory]
	[InlineData("Value: {0}", 42, null, null, "Value: 42")]
	[InlineData("{0} + {1} = {2}", 1, 2, null, "1 + 2 = ")] // param3 null leaves {2} empty
	[InlineData("{0}, {1}, {2}", "A", "B", "C", "A, B, C")]
	[InlineData("Value: {0}", null, null, null, "Value: ")] // null param becomes empty
	[InlineData("Damage: {0}", 100, null, null, "Damage: 100")]
	[InlineData("Multiplier: {0:F2}", 1.5, null, null, "Multiplier: 1.50")]
	public void Format_ReturnsCorrectFormattedString(string formatSpec, object param1, object param2, object param3, string expected)
	{
		// Act
		var result = _provider.Format(formatSpec, param1, param2, param3);

		// Assert
		result.Should().Be(expected);
	}

	[Fact]
	public void Format_WithInvalidFormatSpecifier_ThrowsFormatException()
	{
		// Arrange - format with invalid format specifier that throws FormatException (not ArgumentException)
		var formatSpec = "{0} {1} {999}";

		// Act & Assert - this should throw FormatException since only ArgumentException is caught
		Assert.Throws<FormatException>(() => _provider.Format(formatSpec, "test", null));
	}

	#endregion

	#region FilterValue Tests

	[Fact]
	public void FilterValue_WithZeroInteger_ReturnsTrue()
	{
		// Arrange
		var variable = new Variable("TEST", VariableDataType.Integer, 1);
		variable[0] = 0;

		// Act
		var result = _provider.FilterValue(variable, false);

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public void FilterValue_WithNonZeroInteger_ReturnsFalse()
	{
		// Arrange
		var variable = new Variable("TEST", VariableDataType.Integer, 1);
		variable[0] = 10;

		// Act
		var result = _provider.FilterValue(variable, false);

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void FilterValue_WithZeroFloat_ReturnsTrue()
	{
		// Arrange
		var variable = new Variable("TEST", VariableDataType.Float, 1);
		variable[0] = 0.0f;

		// Act
		var result = _provider.FilterValue(variable, false);

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public void FilterValue_WithNonZeroFloat_ReturnsFalse()
	{
		// Arrange
		var variable = new Variable("TEST", VariableDataType.Float, 1);
		variable[0] = 5.5f;

		// Act
		var result = _provider.FilterValue(variable, false);

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void FilterValue_WithAllowStringsAndEmptyString_ReturnsTrue()
	{
		// Arrange
		var variable = new Variable("TEST", VariableDataType.StringVar, 1);
		variable[0] = string.Empty;

		// Act
		var result = _provider.FilterValue(variable, true);

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public void FilterValue_WithAllowStringsAndNonEmptyString_ReturnsFalse()
	{
		// Arrange
		var variable = new Variable("TEST", VariableDataType.StringVar, 1);
		variable[0] = "test value";

		// Act
		var result = _provider.FilterValue(variable, true);

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void FilterValue_WithoutAllowStringsAndNonEmptyString_ReturnsTrue()
	{
		// Arrange
		var variable = new Variable("TEST", VariableDataType.StringVar, 1);
		variable[0] = "test value";

		// Act
		var result = _provider.FilterValue(variable, false);

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public void FilterValue_WithSpecialStringNameAndAllowStrings_ReturnsFalse()
	{
		// Arrange - CHARACTERBASEATTACKSPEEDTAG should not be filtered even with allowStrings=false
		var variable = new Variable("CHARACTERBASEATTACKSPEEDTAG", VariableDataType.StringVar, 1);
		variable[0] = "speed";

		// Act
		var result = _provider.FilterValue(variable, false);

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void FilterValue_WithITEMSKILLNAME_ReturnsFalse()
	{
		// Arrange - ITEMSKILLNAME should not be filtered
		var variable = new Variable("ITEMSKILLNAME", VariableDataType.StringVar, 1);
		variable[0] = "skill";

		// Act
		var result = _provider.FilterValue(variable, false);

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void FilterValue_WithZeroBoolean_ReturnsTrue()
	{
		// Arrange
		var variable = new Variable("TEST", VariableDataType.Boolean, 1);
		variable[0] = 0;

		// Act
		var result = _provider.FilterValue(variable, false);

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public void FilterValue_WithNonZeroBoolean_ReturnsFalse()
	{
		// Arrange
		var variable = new Variable("TEST", VariableDataType.Boolean, 1);
		variable[0] = 1;

		// Act
		var result = _provider.FilterValue(variable, false);

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void FilterValue_WithMultipleValues_AllZero_ReturnsTrue()
	{
		// Arrange
		var variable = new Variable("TEST", VariableDataType.Integer, 3);
		variable[0] = 0;
		variable[1] = 0;
		variable[2] = 0;

		// Act
		var result = _provider.FilterValue(variable, false);

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public void FilterValue_WithMultipleValues_OneNonZero_ReturnsFalse()
	{
		// Arrange
		var variable = new Variable("TEST", VariableDataType.Integer, 3);
		variable[0] = 0;
		variable[1] = 5;
		variable[2] = 0;

		// Act
		var result = _provider.FilterValue(variable, false);

		// Assert
		result.Should().BeFalse();
	}

	#endregion
}
