using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Concurrent;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Config;
using TQVaultAE.Data;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Data;

/// <summary>
/// Testable derived Database class that exposes protected members for testing
/// </summary>
internal class TestableDatabase : Database
{
	public TestableDatabase(
		ILogger<Database> log,
		IArcFileProvider arcFileProvider,
		IArzFileProvider arzFileProvider,
		IItemAttributeProvider itemAttributeProvider,
		IGamePathService gamePathResolver,
		ITQDataService tQData,
		IFileIO fileIO,
		IDirectoryIO directoryIO,
		IPathIO pathIO,
		UserSettings uSettings)
		: base(
			log,
			arcFileProvider,
			arzFileProvider,
			itemAttributeProvider,
			gamePathResolver,
			tQData,
			fileIO,
			directoryIO,
			pathIO,
			uSettings)
	{
	}

	/// <summary>
	/// Exposes the protected textDB dictionary for test setup
	/// </summary>
	public ConcurrentDictionary<string, string> TextDB => textDB;
}

public class DatabaseTests
{
	private readonly TestableDatabase _database;
	private readonly Mock<ILogger<Database>> _mockLog;
	private readonly Mock<IArcFileProvider> _mockArcFileProvider;
	private readonly Mock<IArzFileProvider> _mockArzFileProvider;
	private readonly Mock<IItemAttributeProvider> _mockItemAttributeProvider;
	private readonly Mock<IGamePathService> _mockGamePathService;
	private readonly Mock<ITQDataService> _mockTQDataService;
	private readonly Mock<IFileIO> _mockFileIO;
	private readonly Mock<IDirectoryIO> _mockDirectoryIO;
	private readonly Mock<IPathIO> _mockPathIO;
	private readonly UserSettings _userSettings;

	public DatabaseTests()
	{
		_mockLog = new Mock<ILogger<Database>>();
		_mockArcFileProvider = new Mock<IArcFileProvider>();
		_mockArzFileProvider = new Mock<IArzFileProvider>();
		_mockItemAttributeProvider = new Mock<IItemAttributeProvider>();
		_mockGamePathService = new Mock<IGamePathService>();
		_mockTQDataService = new Mock<ITQDataService>();
		_mockFileIO = new Mock<IFileIO>();
		_mockDirectoryIO = new Mock<IDirectoryIO>();
		_mockPathIO = new Mock<IPathIO>();
		_userSettings = new UserSettings();

		_database = new TestableDatabase(
			_mockLog.Object,
			_mockArcFileProvider.Object,
			_mockArzFileProvider.Object,
			_mockItemAttributeProvider.Object,
			_mockGamePathService.Object,
			_mockTQDataService.Object,
			_mockFileIO.Object,
			_mockDirectoryIO.Object,
			_mockPathIO.Object,
			_userSettings
		);
	}

	#region GetFriendlyName Tests

	[Fact]
	public void GetFriendlyName_WithExistingTag_ReturnsLocalizedText()
	{
		// Arrange
		var tagId = "TAG_HEALTH".ToUpperInvariant();
		var expectedText = "Health";
		_database.TextDB.TryAdd(tagId, expectedText);

		// Act
		var result = _database.GetFriendlyName("TAG_HEALTH");

		// Assert
		result.Should().Be(expectedText);
	}

	[Fact]
	public void GetFriendlyName_IsCaseInsensitive()
	{
		// Arrange
		var tagId = "TAG_STRENGTH".ToUpperInvariant();
		var expectedText = "Strength";
		_database.TextDB.TryAdd(tagId, expectedText);

		// Act & Assert
		_database.GetFriendlyName("TAG_STRENGTH").Should().Be(expectedText);
		_database.GetFriendlyName("tag_strength").Should().Be(expectedText);
		_database.GetFriendlyName("Tag_Strength").Should().Be(expectedText);
	}

	[Fact]
	public void GetFriendlyName_WithNonExistentTag_ReturnsEmptyString()
	{
		// Act
		var result = _database.GetFriendlyName("NON_EXISTENT_TAG");

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public void GetFriendlyName_WithNullTag_ThrowsNullReferenceException()
	{
		// Act & Assert - Database.GetFriendlyName calls tagId.ToUpperInvariant() which throws on null
		var action = () => _database.GetFriendlyName(null!);
		action.Should().Throw<NullReferenceException>();
	}

	[Fact]
	public void GetFriendlyName_WithEmptyTag_ReturnsEmptyString()
	{
		// Act
		var result = _database.GetFriendlyName(string.Empty);

		// Assert
		result.Should().BeEmpty();
	}

	#endregion

	#region GetItemAttributeFriendlyText Tests

	[Fact]
	public void GetItemAttributeFriendlyText_WithValidAttribute_ReturnsLocalizedText()
	{
		// Arrange
		var attributeName = "DAMAGE";
		var attributeData = new ItemAttributesData(
			ItemAttributesEffectType.Offense,
			attributeName,
			"effectTag",
			"+10",
			1
		);
		var friendlyText = "Damage";

		_mockItemAttributeProvider
			.Setup(x => x.GetAttributeData(attributeName))
			.Returns(attributeData);

		_mockItemAttributeProvider
			.Setup(x => x.GetAttributeTextTag(It.IsAny<ItemAttributesData>()))
			.Returns("TAG_DAMAGE");

		_database.TextDB.TryAdd("TAG_DAMAGE", friendlyText);

		// Act
		var result = _database.GetItemAttributeFriendlyText(attributeName);

		// Assert
		result.Should().Contain(friendlyText);
	}

	[Fact]
	public void GetItemAttributeFriendlyText_WithUnknownAttribute_ReturnsErrorFormat()
	{
		// Arrange
		var attributeName = "UNKNOWN_ATTRIBUTE";

		_mockItemAttributeProvider
			.Setup(x => x.GetAttributeData(attributeName))
			.Returns((ItemAttributesData?)null);

		// Act
		var result = _database.GetItemAttributeFriendlyText(attributeName);

		// Assert
		result.Should().Be("?UNKNOWN_ATTRIBUTE?");
	}

	[Fact]
	public void GetItemAttributeFriendlyText_WithUnknownTextTag_ReturnsFallbackFormat()
	{
		// Arrange - when text tag lookup returns empty string, the fallback format is used
		var attributeName = "SOME_ATTRIBUTE";
		var attributeData = new ItemAttributesData(
			ItemAttributesEffectType.Other,
			attributeName,
			"someEffect",
			"+5",
			2
		);

		_mockItemAttributeProvider
			.Setup(x => x.GetAttributeData(attributeName))
			.Returns(attributeData);

		_mockItemAttributeProvider
			.Setup(x => x.GetAttributeTextTag(It.IsAny<ItemAttributesData>()))
			.Returns("NON_EXISTENT_TAG"); // Tag that won't be found in textDB

		// Act
		var result = _database.GetItemAttributeFriendlyText(attributeName);

		// Assert - format is "ATTR<{itemAttribute}> TAG<{textTag}> {variable}"
		// Note: variable (+5) is appended when addVariable (default) is true
		result.Should().Be($"ATTR<{attributeName}> TAG<NON_EXISTENT_TAG> +5");
	}

	[Fact]
	public void GetItemAttributeFriendlyText_WithUnknownTextTagAndNoVariable_DoesNotAppendVariable()
	{
		// Arrange - when text tag lookup returns empty string, the fallback format is used
		var attributeName = "SOME_ATTRIBUTE";
		var attributeData = new ItemAttributesData(
			ItemAttributesEffectType.Other,
			attributeName,
			"someEffect",
			"", // Empty variable
			2
		);

		_mockItemAttributeProvider
			.Setup(x => x.GetAttributeData(attributeName))
			.Returns(attributeData);

		_mockItemAttributeProvider
			.Setup(x => x.GetAttributeTextTag(It.IsAny<ItemAttributesData>()))
			.Returns("NON_EXISTENT_TAG"); // Tag that won't be found in textDB

		// Act
		var result = _database.GetItemAttributeFriendlyText(attributeName);

		// Assert - format is "ATTR<{itemAttribute}> TAG<{textTag}>"
		// No variable appended since it's empty
		result.Should().Be($"ATTR<{attributeName}> TAG<NON_EXISTENT_TAG>");
	}

	[Fact]
	public void GetItemAttributeFriendlyText_AddVariableFalse_DoesNotAppendVariable()
	{
		// Arrange
		var attributeName = "DAMAGE";
		var attributeData = new ItemAttributesData(
			ItemAttributesEffectType.Offense,
			attributeName,
			"effectTag",
			"+10",
			1
		);

		_mockItemAttributeProvider
			.Setup(x => x.GetAttributeData(attributeName))
			.Returns(attributeData);

		_mockItemAttributeProvider
			.Setup(x => x.GetAttributeTextTag(It.IsAny<ItemAttributesData>()))
			.Returns("TAG_DAMAGE");

		_database.TextDB.TryAdd("TAG_DAMAGE", "Damage");

		// Act
		var result = _database.GetItemAttributeFriendlyText(attributeName, addVariable: false);

		// Assert
		result.Should().Be("Damage"); // No "+10" appended
	}

	[Fact]
	public void GetItemAttributeFriendlyText_AddVariableTrue_AppendsVariable()
	{
		// Arrange
		var attributeName = "DAMAGE";
		var attributeData = new ItemAttributesData(
			ItemAttributesEffectType.Offense,
			attributeName,
			"effectTag",
			"+10",
			1
		);

		_mockItemAttributeProvider
			.Setup(x => x.GetAttributeData(attributeName))
			.Returns(attributeData);

		_mockItemAttributeProvider
			.Setup(x => x.GetAttributeTextTag(It.IsAny<ItemAttributesData>()))
			.Returns("TAG_DAMAGE");

		_database.TextDB.TryAdd("TAG_DAMAGE", "Damage");

		// Act
		var result = _database.GetItemAttributeFriendlyText(attributeName, addVariable: true);

		// Assert
		result.Should().Be("Damage +10");
	}

	[Fact]
	public void GetItemAttributeFriendlyText_WithEmptyVariable_DoesNotAppendSpace()
	{
		// Arrange
		var attributeName = "ARMOR";
		var attributeData = new ItemAttributesData(
			ItemAttributesEffectType.Defense,
			attributeName,
			"defEffect",
			"", // Empty variable
			3
		);

		_mockItemAttributeProvider
			.Setup(x => x.GetAttributeData(attributeName))
			.Returns(attributeData);

		_mockItemAttributeProvider
			.Setup(x => x.GetAttributeTextTag(It.IsAny<ItemAttributesData>()))
			.Returns("TAG_ARMOR");

		_database.TextDB.TryAdd("TAG_ARMOR", "Armor");

		// Act
		var result = _database.GetItemAttributeFriendlyText(attributeName, addVariable: true);

		// Assert
		result.Should().Be("Armor"); // No trailing space when variable is empty
	}

	[Fact]
	public void GetItemAttributeFriendlyText_VariableFormatIsPreserved()
	{
		// Arrange
		var attributeName = "HEALPERSEC";
		var attributeData = new ItemAttributesData(
			ItemAttributesEffectType.Other,
			attributeName,
			"someEffect",
			"+{0} per second",
			1
		);

		_mockItemAttributeProvider
			.Setup(x => x.GetAttributeData(attributeName))
			.Returns(attributeData);

		_mockItemAttributeProvider
			.Setup(x => x.GetAttributeTextTag(It.IsAny<ItemAttributesData>()))
			.Returns("TAG_HEAL");

		_database.TextDB.TryAdd("TAG_HEAL", "Health Regeneration");

		// Act
		var result = _database.GetItemAttributeFriendlyText(attributeName, addVariable: true);

		// Assert
		result.Should().Be("Health Regeneration +{0} per second");
	}

	#endregion

	#region VariableToStringNice Tests

	[Fact]
	public void VariableToStringNice_WithValidVariable_ReturnsFormattedString()
	{
		// Arrange
		var variableName = "DAMAGE";
		var variable = new Variable(variableName, VariableDataType.Integer, 1)
		{
			[0] = 100
		};

		var attributeData = new ItemAttributesData(
			ItemAttributesEffectType.Offense,
			variableName,
			"offensiveEffect",
			"",
			1
		);

		_mockItemAttributeProvider
			.Setup(x => x.GetAttributeData(variableName))
			.Returns(attributeData);

		_mockItemAttributeProvider
			.Setup(x => x.GetAttributeTextTag(It.IsAny<ItemAttributesData>()))
			.Returns("TAG_DAMAGE");

		_database.TextDB.TryAdd("TAG_DAMAGE", "Damage");

		// Act - Format is "{friendlyText} {variable}: {value}"
		var result = _database.VariableToStringNice(variable);

		// Assert
		result.Should().Be("Damage: 100");
	}

	[Fact]
	public void VariableToStringNice_WithFloatVariable_ReturnsFormattedString()
	{
		// Arrange
		var variableName = "SPEED";
		var variable = new Variable(variableName, VariableDataType.Float, 1)
		{
			[0] = 1.5f
		};

		var attributeData = new ItemAttributesData(
			ItemAttributesEffectType.Other,
			variableName,
			"otherEffect",
			"",
			1
		);

		_mockItemAttributeProvider
			.Setup(x => x.GetAttributeData(variableName))
			.Returns(attributeData);

		_mockItemAttributeProvider
			.Setup(x => x.GetAttributeTextTag(It.IsAny<ItemAttributesData>()))
			.Returns("TAG_SPEED");

		_database.TextDB.TryAdd("TAG_SPEED", "Speed");

		// Act - Float values are formatted with 6 decimal places
		var result = _database.VariableToStringNice(variable);

		// Assert
		result.Should().Be("Speed: 1.500000");
	}

	[Fact]
	public void VariableToStringNice_WithUnknownAttribute_ReturnsErrorFormat()
	{
		// Arrange
		var variableName = "UNKNOWN_VAR";
		var variable = new Variable(variableName, VariableDataType.Integer, 1)
		{
			[0] = 50
		};

		_mockItemAttributeProvider
			.Setup(x => x.GetAttributeData(variableName))
			.Returns((ItemAttributesData?)null);

		// Act
		var result = _database.VariableToStringNice(variable);

		// Assert - Format is "?{attributeName}?: {value}"
		result.Should().Be("?UNKNOWN_VAR?: 50");
	}

	#endregion

	#region ExtractArcFile Tests

	[Fact]
	public void ExtractArcFile_WithValidArcFileName_ReturnsTrue()
	{
		// Arrange
		var arcFileName = "test.arc";
		var destination = Path.GetTempPath();

		_mockArcFileProvider
			.Setup(x => x.ExtractArcFile(It.IsAny<ArcFile>(), destination))
			.Returns(true);

		// Act
		var result = _database.ExtractArcFile(arcFileName, destination);

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public void ExtractArcFile_WhenArcFileProviderThrows_ReturnsFalse()
	{
		// Arrange
		var arcFileName = "test.arc";
		var destination = Path.GetTempPath();

		_mockArcFileProvider
			.Setup(x => x.ExtractArcFile(It.IsAny<ArcFile>(), destination))
			.Throws(new IOException("Test exception"));

		// Act
		var result = _database.ExtractArcFile(arcFileName, destination);

		// Assert
		result.Should().BeFalse();
	}

	#endregion

	#region GetInfo Tests

	[Fact]
	public void GetInfo_WithNullItemId_ReturnsNull()
	{
		// Act
		var result = _database.GetInfo(null!);

		// Assert
		result.Should().BeNull();
	}

	// Note: GetInfo creates Info object which requires full database setup including:
	// - ArzFile instances set on Database.ArzFile, ArzFileIT, ArzFileMod
	// - ItemAttributeProvider properly configured with attribute mappings
	// - The Info.AssignVariableNames() method requires access to attribute text tags
	// Testing GetInfo fully would require extensive integration test setup
	// See integration tests for full Database testing with game files

	#endregion

	#region GameLanguage Tests

	[Fact]
	public void GameLanguage_WhenAutoDetectIsFalse_ReturnsTQLanguage()
	{
		// Arrange
		_userSettings.AutoDetectLanguage = false;
		_userSettings.TQLanguage = "ENGLISH";

		var db = new TestableDatabase(
			_mockLog.Object,
			_mockArcFileProvider.Object,
			_mockArzFileProvider.Object,
			_mockItemAttributeProvider.Object,
			_mockGamePathService.Object,
			_mockTQDataService.Object,
			_mockFileIO.Object,
			_mockDirectoryIO.Object,
			_mockPathIO.Object,
			_userSettings
		);

		// Act
		var result = db.GameLanguage;

		// Assert
		result.Should().Be("ENGLISH");
	}

	[Fact]
	public void GameLanguage_WhenAutoDetectIsTrueAndNoSettingsFile_ReturnsNull()
	{
		// Arrange
		_userSettings.AutoDetectLanguage = true;
		_userSettings.TQLanguage = "ENGLISH";

		_mockGamePathService.Setup(x => x.SettingsFileTQ).Returns("/non/existent/path");
		_mockGamePathService.Setup(x => x.SettingsFileTQIT).Returns("/non/existent/path2");
		_mockFileIO.Setup(x => x.Exists(It.IsAny<string>())).Returns(false);

		var db = new TestableDatabase(
			_mockLog.Object,
			_mockArcFileProvider.Object,
			_mockArzFileProvider.Object,
			_mockItemAttributeProvider.Object,
			_mockGamePathService.Object,
			_mockTQDataService.Object,
			_mockFileIO.Object,
			_mockDirectoryIO.Object,
			_mockPathIO.Object,
			_userSettings
		);

		// Act
		var result = db.GameLanguage;

		// Assert
		result.Should().BeNull();
	}

	#endregion

	#region AutoDetectLanguage Tests

	[Fact]
	public void AutoDetectLanguage_DefaultsToUserSettingsValue()
	{
		// Arrange
		_userSettings.AutoDetectLanguage = true;

		var db = new TestableDatabase(
			_mockLog.Object,
			_mockArcFileProvider.Object,
			_mockArzFileProvider.Object,
			_mockItemAttributeProvider.Object,
			_mockGamePathService.Object,
			_mockTQDataService.Object,
			_mockFileIO.Object,
			_mockDirectoryIO.Object,
			_mockPathIO.Object,
			_userSettings
		);

		// Assert
		db.AutoDetectLanguage.Should().BeTrue();
	}

	[Fact]
	public void TQLanguage_DefaultsToUserSettingsValue()
	{
		// Arrange
		_userSettings.TQLanguage = "FRENCH";

		var db = new TestableDatabase(
			_mockLog.Object,
			_mockArcFileProvider.Object,
			_mockArzFileProvider.Object,
			_mockItemAttributeProvider.Object,
			_mockGamePathService.Object,
			_mockTQDataService.Object,
			_mockFileIO.Object,
			_mockDirectoryIO.Object,
			_mockPathIO.Object,
			_userSettings
		);

		// Assert
		db.TQLanguage.Should().Be("FRENCH");
	}

	#endregion
}
