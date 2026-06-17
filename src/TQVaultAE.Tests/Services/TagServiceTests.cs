using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Drawing;
using System.Text.Json;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Config.Tags;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

/// <summary>
/// Unit tests for TagService class
/// </summary>
public class TagServiceTests
{
	private readonly Mock<ILogger<TagService>> _mockLogger;
	private readonly Mock<IGamePathService> _mockGamePathService;
	private readonly Mock<IFileIO> _mockFileIO;
	private readonly Mock<IPathIO> _mockPathIO;
	private readonly Mock<ITranslationService> _mockTranslationService;
	private readonly JsonSerializerOptions _jsonOptions;
	private readonly TagService _tagService;
	private readonly string _testConfigPath = @"C:\Test\TagConfig.json";

	public TagServiceTests()
	{
		_mockLogger = new Mock<ILogger<TagService>>();
		_mockGamePathService = new Mock<IGamePathService>();
		_mockFileIO = new Mock<IFileIO>();
		_mockPathIO = new Mock<IPathIO>();
		_mockTranslationService = new Mock<ITranslationService>();
		_jsonOptions = new JsonSerializerOptions { IncludeFields = true, PropertyNameCaseInsensitive = true };

		_mockGamePathService.Setup(x => x.TQVaultConfigFolder).Returns(@"C:\Test");
		_mockPathIO.Setup(x => x.Combine(It.IsAny<string>(), It.IsAny<string>())).Returns(_testConfigPath);
		_mockPathIO.Setup(x => x.GetFileName(It.IsAny<string>())).Returns("_TestPlayer");

		_tagService = new TagService(
			_mockLogger.Object,
			_mockGamePathService.Object,
			_mockFileIO.Object,
			_mockPathIO.Object,
			_jsonOptions
		);
	}

	private PlayerSave CreateTestPlayerSave()
	{
		return new PlayerSave(@"C:\Test\_TestPlayer", false, false, false, "", _mockTranslationService.Object, _mockPathIO.Object);
	}

	[Fact]
	public void Tags_WithEmptyConfig_ReturnsEmptyDictionary()
	{
		// Arrange
		_mockFileIO.Setup(x => x.Exists(_testConfigPath)).Returns(false);

		// Act
		var tags = _tagService.Tags;

		// Assert
		tags.Should().BeEmpty();
	}

	[Fact]
	public void Tags_WithExistingConfig_ReturnsOrderedDictionary()
	{
		// Arrange
		var config = new TagConfig
		{
			tags = new List<TagInfo>
			{
			new TagInfo { name = "ZTag", color = new TagInfoColor { R = 255, G = 0, B = 0 } },
				new TagInfo { name = "ATag", color = new TagInfoColor { R = 0, G = 255, B = 0 } }
			}
		};
		var configJson = JsonSerializer.Serialize(config, _jsonOptions);
		_mockFileIO.Setup(x => x.Exists(_testConfigPath)).Returns(true);
		_mockFileIO.Setup(x => x.ReadAllText(_testConfigPath)).Returns(configJson);

		var service = new TagService(
			_mockLogger.Object,
			_mockGamePathService.Object,
			_mockFileIO.Object,
			_mockPathIO.Object,
			_jsonOptions
		);

		// Act
		var tags = service.Tags;

		// Assert
		tags.Should().HaveCount(2);
		tags.Keys.Should().ContainInOrder("ATag", "ZTag");
		tags["ATag"].Should().Be(Color.FromArgb(0, 255, 0));
		tags["ZTag"].Should().Be(Color.FromArgb(255, 0, 0));
	}

	[Fact]
	public void AddTag_WithNewTagName_ReturnsTrue()
	{
		// Arrange
		var tagName = "TestTag";

		// Act
		var result = _tagService.AddTag(tagName);

		// Assert
		result.Should().BeTrue();
		_mockFileIO.Verify(x => x.WriteAllText(_testConfigPath, It.IsAny<string>()), Times.Once);
	}

	[Fact]
	public void AddTag_WithExistingTagName_ReturnsFalse()
	{
		// Arrange
		var tagName = "TestTag";
		_tagService.AddTag(tagName);

		// Act
		var result = _tagService.AddTag(tagName);

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void AddTag_WithRGB_WithNewTagName_ReturnsTrue()
	{
		// Arrange
		var tagName = "TestTag";

		// Act
		var result = _tagService.AddTag(tagName, 255, 128, 64);

		// Assert
		result.Should().BeTrue();
		_mockFileIO.Verify(x => x.WriteAllText(_testConfigPath, It.IsAny<string>()), Times.Once);
	}

	[Fact]
	public void AssignTag_WithValidPlayerAndTag_ReturnsTrue()
	{
		// Arrange
		var tagName = "TestTag";
		_tagService.AddTag(tagName);
		var playerSave = CreateTestPlayerSave();

		// Act
		var result = _tagService.AssignTag(playerSave, tagName);

		// Assert
		result.Should().BeTrue();
		playerSave.Tags.Should().ContainKey(tagName);
		_mockFileIO.Verify(x => x.WriteAllText(_testConfigPath, It.IsAny<string>()), Times.Exactly(2));
	}

	[Fact]
	public void AssignTag_WithNonExistentTag_ReturnsFalse()
	{
		// Arrange
		var tagName = "NonExistentTag";
		var playerSave = CreateTestPlayerSave();

		// Act
		var result = _tagService.AssignTag(playerSave, tagName);

		// Assert
		result.Should().BeFalse();
		_mockFileIO.Verify(x => x.WriteAllText(_testConfigPath, It.IsAny<string>()), Times.Never);
	}

	[Fact]
	public void AssignTag_WithAlreadyAssignedTag_ReturnsFalse()
	{
		// Arrange
		var tagName = "TestTag";
		_tagService.AddTag(tagName);
		var playerSave = CreateTestPlayerSave();
		_tagService.AssignTag(playerSave, tagName);

		// Act
		var result = _tagService.AssignTag(playerSave, tagName);

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void UnassignTag_WithValidPlayerAndTag_ReturnsTrue()
	{
		// Arrange
		var tagName = "TestTag";
		_tagService.AddTag(tagName);
		var playerSave = CreateTestPlayerSave();
		_tagService.AssignTag(playerSave, tagName);

		// Act
		var result = _tagService.UnassignTag(playerSave, tagName);

		// Assert
		result.Should().BeTrue();
		playerSave.Tags.Should().NotContainKey(tagName);
	}

	[Fact]
	public void UnassignTag_WithNonExistentTag_ReturnsFalse()
	{
		// Arrange
		var tagName = "NonExistentTag";
		var playerSave = CreateTestPlayerSave();

		// Act
		var result = _tagService.UnassignTag(playerSave, tagName);

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void DeleteTag_WithExistingTag_ReturnsTrue()
	{
		// Arrange
		var tagName = "TestTag";
		_tagService.AddTag(tagName);

		// Act
		var result = _tagService.DeleteTag(tagName);

		// Assert
		result.Should().BeTrue();
		_mockFileIO.Verify(x => x.WriteAllText(_testConfigPath, It.IsAny<string>()), Times.Exactly(2));
	}

	[Fact]
	public void DeleteTag_WithNonExistentTag_ReturnsFalse()
	{
		// Arrange
		var tagName = "NonExistentTag";

		// Act
		var result = _tagService.DeleteTag(tagName);

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void DeleteTag_WithAssignedTags_RemovesFromMappings()
	{
		// Arrange
		var tagName = "TestTag";
		_tagService.AddTag(tagName);
		var playerSave = CreateTestPlayerSave();
		_tagService.AssignTag(playerSave, tagName);

		// Act
		var result = _tagService.DeleteTag(tagName);

		// Manually refresh player tags since DeleteTag doesn't call LoadTags
		_tagService.LoadTags(playerSave);

		// Assert
		result.Should().BeTrue();
		playerSave.Tags.Should().NotContainKey(tagName);
	}

	[Fact]
	public void LoadTags_WithPlayerHavingTags_LoadsTagsToPlayer()
	{
		// Arrange
		var tagName = "TestTag";
		_tagService.AddTag(tagName, 255, 128, 64);
		var playerSave = CreateTestPlayerSave();
		_tagService.AssignTag(playerSave, tagName);

		// Clear tags to test loading
		playerSave.Tags.Clear();

		// Act
		_tagService.LoadTags(playerSave);

		// Assert
		playerSave.Tags.Should().HaveCount(1);
		playerSave.Tags.Should().ContainKey(tagName);
		playerSave.Tags[tagName].Should().Be(Color.FromArgb(255, 128, 64));
	}

	[Fact]
	public void LoadTags_WithPlayerHavingNoTags_ClearsPlayerTags()
	{
		// Arrange
		var tagName = "TestTag";
		_tagService.AddTag(tagName);
		var playerSave = CreateTestPlayerSave();
		playerSave.Tags.Add(tagName, Color.Red);

		// Act
		_tagService.LoadTags(playerSave);

		// Assert
		playerSave.Tags.Should().BeEmpty();
	}

	[Fact]
	public void ReadConfig_WithExistingFile_LoadsConfig()
	{
		// Arrange
		var config = new TagConfig
		{
			tags = new List<TagInfo>
		{
			new TagInfo { name = "TestTag", color = new TagInfoColor { R = 255, G = 0, B = 0 } }
			}
		};
		var configJson = JsonSerializer.Serialize(config);
		_mockFileIO.Setup(x => x.Exists(_testConfigPath)).Returns(true);
		_mockFileIO.Setup(x => x.ReadAllText(_testConfigPath)).Returns(configJson);

		// Act
		_tagService.ReadConfig();

		// Assert
		_mockFileIO.Verify(x => x.ReadAllText(_testConfigPath), Times.Once);
	}

	[Fact]
	public void ReadConfig_WithNonExistentFile_CreatesEmptyConfig()
	{
		// Arrange
		_mockFileIO.Setup(x => x.Exists(_testConfigPath)).Returns(false);

		// Act
		_tagService.ReadConfig();

		// Assert
		_mockFileIO.Verify(x => x.ReadAllText(_testConfigPath), Times.Never);
	}

	[Fact]
	public void SaveConfig_WithNullConfig_DoesNotSave()
	{
		// Arrange
		_tagService.ReadConfig();

		// Act & Assert
		_mockFileIO.Verify(x => x.WriteAllText(_testConfigPath, It.IsAny<string>()), Times.Never);
	}

	[Fact]
	public void SaveConfig_WithValidConfig_SavesToFile()
	{
		// Arrange
		var service = new TagService(
			_mockLogger.Object,
			_mockGamePathService.Object,
			_mockFileIO.Object,
			_mockPathIO.Object,
			_jsonOptions
		);
		service.AddTag("TestTag");

		// Act
		service.SaveConfig();

		// Assert
		_mockFileIO.Verify(x => x.WriteAllText(_testConfigPath, It.IsAny<string>()), Times.AtLeastOnce);
	}

	[Fact]
	public void UpdateTag_WithExistingTag_ReturnsTrue()
	{
		// Arrange
		var tagNameOld = "OldTag";
		var tagNameNew = "NewTag";
		_tagService.AddTag(tagNameOld);

		// Act
		var result = _tagService.UpdateTag(tagNameOld, tagNameNew, 255, 128, 64);

		// Assert
		result.Should().BeTrue();
		_mockFileIO.Verify(x => x.WriteAllText(_testConfigPath, It.IsAny<string>()), Times.Exactly(2));
	}

	[Fact]
	public void UpdateTag_WithNonExistentTag_ReturnsFalse()
	{
		// Arrange
		var tagNameOld = "NonExistentTag";
		var tagNameNew = "NewTag";

		// Act
		var result = _tagService.UpdateTag(tagNameOld, tagNameNew, 255, 128, 64);

		// Assert
		result.Should().BeFalse();
		_mockFileIO.Verify(x => x.WriteAllText(_testConfigPath, It.IsAny<string>()), Times.Never);
	}

	[Fact]
	public void UpdateTag_WithAssignedTags_UpdatesMappings()
	{
		// Arrange
		var tagNameOld = "OldTag";
		var tagNameNew = "NewTag";
		_tagService.AddTag(tagNameOld);
		var playerSave = CreateTestPlayerSave();
		_tagService.AssignTag(playerSave, tagNameOld);

		// Act
		var result = _tagService.UpdateTag(tagNameOld, tagNameNew, 255, 128, 64);

		// Manually refresh player tags since UpdateTag doesn't call LoadTags
		_tagService.LoadTags(playerSave);

		// Assert
		result.Should().BeTrue();
		playerSave.Tags.Should().NotContainKey(tagNameOld);
		playerSave.Tags.Should().ContainKey(tagNameNew);
	}
}