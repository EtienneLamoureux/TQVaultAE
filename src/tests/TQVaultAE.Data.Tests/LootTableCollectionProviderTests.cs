using System.Collections.ObjectModel;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Config;
using TQVaultAE.Data;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Data;

/// <summary>
/// Unit tests for LootTableCollectionProvider class
/// </summary>
public class LootTableCollectionProviderTests
{
	private readonly Mock<ILogger<LootTableCollectionProvider>> _mockLogger;
	private readonly Mock<IDatabase> _mockDatabase;
	private readonly Mock<ITranslationService> _mockTranslationService;
	private readonly UserSettings _userSettings;
	private readonly LootTableCollectionProvider _provider;

	public LootTableCollectionProviderTests()
	{
		_mockLogger = new Mock<ILogger<LootTableCollectionProvider>>();
		_mockDatabase = new Mock<IDatabase>();
		_mockTranslationService = new Mock<ITranslationService>();
		_userSettings = new UserSettings();

		_provider = new LootTableCollectionProvider(
			_mockLogger.Object,
			_mockDatabase.Object,
			_mockTranslationService.Object,
			_userSettings
		);
	}

	private static DBRecordCollection CreateTestDBRecordCollection(params (string name, string value)[] variables)
	{
		var recordId = RecordId.Create("test/table");
		var drc = new DBRecordCollection(recordId, "LootRandomizerTable");

		foreach (var (name, value) in variables)
		{
			var variable = new Variable(name, VariableDataType.StringVar, 1);
			variable[0] = value;
			drc.Set(variable);
		}

		return drc;
	}

	private static DBRecordCollection CreateTestDBRecordCollectionWithNumericWeight(
		string namePrefix,
		string affixId,
		float weight)
	{
		var recordId = RecordId.Create("test/table");
		var drc = new DBRecordCollection(recordId, "LootRandomizerTable");

		// Add randomizerNameX
		var nameVar = new Variable($"{namePrefix}Name", VariableDataType.StringVar, 1);
		nameVar[0] = affixId;
		drc.Set(nameVar);

		// Add randomizerWeightX
		var weightVar = new Variable($"{namePrefix}Weight", VariableDataType.Float, 1);
		weightVar[0] = weight;
		drc.Set(weightVar);

		return drc;
	}

	private static DBRecordCollection CreateTestDBRecordCollectionWithIntWeight(
		string namePrefix,
		string affixId,
		int weight)
	{
		var recordId = RecordId.Create("test/table");
		var drc = new DBRecordCollection(recordId, "LootRandomizerTable");

		// Add randomizerNameX
		var nameVar = new Variable($"{namePrefix}Name", VariableDataType.StringVar, 1);
		nameVar[0] = affixId;
		drc.Set(nameVar);

		// Add randomizerWeightX
		var weightVar = new Variable($"{namePrefix}Weight", VariableDataType.Integer, 1);
		weightVar[0] = weight;
		drc.Set(weightVar);

		return drc;
	}

	[Fact]
	public void LoadTable_WithNullTableId_ReturnsNull()
	{
		// Act
		var result = _provider.LoadTable(RecordId.Empty);

		// Assert
		result.Should().BeNull();
	}

	[Fact]
	public void LoadTable_WithEmptyTableId_ReturnsNull()
	{
		// Act
		var result = _provider.LoadTable(RecordId.Empty);

		// Assert
		result.Should().BeNull();
	}

	[Fact]
	public void LoadTable_WithUnknownTableId_ReturnsNull()
	{
		// Arrange
		var tableId = RecordId.Create("unknown/table");
		_mockDatabase.Setup(x => x.AllLootRandomizerTable)
			.Returns(new ReadOnlyDictionary<RecordId, DBRecordCollection>(new Dictionary<RecordId, DBRecordCollection>()));

		// Act
		var result = _provider.LoadTable(tableId);

		// Assert
		result.Should().BeNull();
	}

	[Fact]
	public void LoadTable_WithValidTable_CreatesLootTableCollection()
	{
		// Arrange
		var tableId = RecordId.Create("test/table");
		var drc = CreateTestDBRecordCollectionWithIntWeight("randomizer", "affix1", 100);

		var lootTable = new Dictionary<RecordId, DBRecordCollection>
		{
			{ tableId, drc }
		};
		_mockDatabase.Setup(x => x.AllLootRandomizerTable)
			.Returns(new ReadOnlyDictionary<RecordId, DBRecordCollection>(lootTable));

		// Setup loot randomizer lookup
		var affixId = RecordId.Create("affix1");
		var lootRandomizerItem = LootRandomizerItem.Default(affixId);
		var lootRandomizerDict = new Dictionary<RecordId, LootRandomizerItem>
		{
			{ affixId, lootRandomizerItem }
		};
		_mockDatabase.Setup(x => x.AllLootRandomizer)
			.Returns(new ReadOnlyDictionary<RecordId, LootRandomizerItem>(lootRandomizerDict));

		// Setup translation service
		_mockTranslationService.Setup(x => x.TranslateXTag(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
			.Returns("Translated Affix");

		// Act
		var result = _provider.LoadTable(tableId);

		// Assert
		result.Should().NotBeNull();
		result!.TableId.Should().Be(tableId);
		result.Length.Should().Be(1);
	}

	[Fact]
	public void LoadTable_WithDebugEnabled_LogsErrorForUnknownTable()
	{
		// Arrange
		_userSettings.LootTableDebugEnabled = true;
		var tableId = RecordId.Create("unknown/table");
		_mockDatabase.Setup(x => x.AllLootRandomizerTable)
			.Returns(new ReadOnlyDictionary<RecordId, DBRecordCollection>(new Dictionary<RecordId, DBRecordCollection>()));

		// Act
		var result = _provider.LoadTable(tableId);

		// Assert
		result.Should().BeNull();
		_mockLogger.Verify(x => x.Log(
			LogLevel.Error,
			It.IsAny<EventId>(),
			It.IsAny<It.IsAnyType>(),
			It.IsAny<Exception>(),
			It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
	}

	[Fact]
	public void AllLootRandomizerTranslated_WithEmptyTranslationTag_UsesFileDescription()
	{
		// Arrange
		var affixId = RecordId.Create("affix1");
		var lootItem = new LootRandomizerItem(
			affixId,
			string.Empty,
			0,
			0,
			string.Empty,
			"File Description",
			string.Empty
		);

		var lootRandomizerDict = new Dictionary<RecordId, LootRandomizerItem>
		{
			{ affixId, lootItem }
		};
		_mockDatabase.Setup(x => x.AllLootRandomizer)
			.Returns(new ReadOnlyDictionary<RecordId, LootRandomizerItem>(lootRandomizerDict));

		_mockTranslationService.Setup(x => x.TranslateXTag(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
			.Returns(string.Empty);

		// Act
		var result = _provider.AllLootRandomizerTranslated;

		// Assert
		result.Should().NotBeNull();
		result.ContainsKey(affixId).Should().BeTrue();
		result[affixId].Translation.Should().Be("File Description");
	}

	[Fact]
	public void AllLootRandomizerTranslated_WithValidTranslation_UsesTranslation()
	{
		// Arrange
		var affixId = RecordId.Create("affix1");
		var lootItem = new LootRandomizerItem(
			affixId,
			"TAG_ValidTag",
			0,
			0,
			string.Empty,
			string.Empty,
			string.Empty
		);

		var lootRandomizerDict = new Dictionary<RecordId, LootRandomizerItem>
		{
			{ affixId, lootItem }
		};
		_mockDatabase.Setup(x => x.AllLootRandomizer)
			.Returns(new ReadOnlyDictionary<RecordId, LootRandomizerItem>(lootRandomizerDict));

		_mockTranslationService.Setup(x => x.TranslateXTag("TAG_ValidTag", It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
			.Returns("Translated Name");

		// Act
		var result = _provider.AllLootRandomizerTranslated;

		// Assert
		result.Should().NotBeNull();
		result[affixId].Translation.Should().Be("Translated Name");
	}

	[Fact]
	public void AllLootRandomizerTranslated_CachesResult()
	{
		// Arrange
		var affixId = RecordId.Create("affix1");
		var lootItem = new LootRandomizerItem(
			affixId,
			"TAG_Test",
			0,
			0,
			string.Empty,
			string.Empty,
			string.Empty
		);

		var lootRandomizerDict = new Dictionary<RecordId, LootRandomizerItem>
		{
			{ affixId, lootItem }
		};
		_mockDatabase.Setup(x => x.AllLootRandomizer)
			.Returns(new ReadOnlyDictionary<RecordId, LootRandomizerItem>(lootRandomizerDict));

		_mockTranslationService.Setup(x => x.TranslateXTag(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
			.Returns("Translated");

		// Act - Access twice
		var firstResult = _provider.AllLootRandomizerTranslated;
		var secondResult = _provider.AllLootRandomizerTranslated;

		// Assert - Should be the same cached object
		ReferenceEquals(firstResult, secondResult).Should().BeTrue();
	}

	[Fact]
	public void LoadTable_WithEmptyAffixId_SkipsEntry()
	{
		// Arrange
		var tableId = RecordId.Create("test/table");
		var drc = new DBRecordCollection(tableId, "LootRandomizerTable");

		// Add randomizerName with empty value
		var nameVar = new Variable("randomizerName1", VariableDataType.StringVar, 1);
		nameVar[0] = ""; // Empty affix ID
		drc.Set(nameVar);

		// Add randomizerWeight
		var weightVar = new Variable("randomizerWeight1", VariableDataType.Integer, 1);
		weightVar[0] = 100;
		drc.Set(weightVar);

		var lootTable = new Dictionary<RecordId, DBRecordCollection>
		{
			{ tableId, drc }
		};
		_mockDatabase.Setup(x => x.AllLootRandomizerTable)
			.Returns(new ReadOnlyDictionary<RecordId, DBRecordCollection>(lootTable));

		_mockDatabase.Setup(x => x.AllLootRandomizer)
			.Returns(new ReadOnlyDictionary<RecordId, LootRandomizerItem>(new Dictionary<RecordId, LootRandomizerItem>()));

		_mockTranslationService.Setup(x => x.TranslateXTag(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
			.Returns("");

		// Act
		var result = _provider.LoadTable(tableId);

		// Assert - Empty affix should be skipped
		result.Should().NotBeNull();
		result!.Length.Should().Be(0);
	}

	[Fact]
	public void LoadTable_WithZeroWeight_SkipsEntry()
	{
		// Arrange
		var tableId = RecordId.Create("test/table");
		var drc = CreateTestDBRecordCollectionWithIntWeight("randomizer", "affix1", 0);

		var lootTable = new Dictionary<RecordId, DBRecordCollection>
		{
			{ tableId, drc }
		};
		_mockDatabase.Setup(x => x.AllLootRandomizerTable)
			.Returns(new ReadOnlyDictionary<RecordId, DBRecordCollection>(lootTable));

		_mockDatabase.Setup(x => x.AllLootRandomizer)
			.Returns(new ReadOnlyDictionary<RecordId, LootRandomizerItem>(new Dictionary<RecordId, LootRandomizerItem>()));

		_mockTranslationService.Setup(x => x.TranslateXTag(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
			.Returns("");

		// Act
		var result = _provider.LoadTable(tableId);

		// Assert - Zero weight should be skipped
		result.Should().NotBeNull();
		result!.Length.Should().Be(0);
	}

	[Fact]
	public void LoadTable_WithFloatWeight_ProcessesCorrectly()
	{
		// Arrange
		var tableId = RecordId.Create("test/table");
		var drc = CreateTestDBRecordCollectionWithNumericWeight("randomizer", "affix1", 25.5f);

		var lootTable = new Dictionary<RecordId, DBRecordCollection>
		{
			{ tableId, drc }
		};
		_mockDatabase.Setup(x => x.AllLootRandomizerTable)
			.Returns(new ReadOnlyDictionary<RecordId, DBRecordCollection>(lootTable));

		var affixId = RecordId.Create("affix1");
		var lootRandomizerItem = LootRandomizerItem.Default(affixId);
		var lootRandomizerDict = new Dictionary<RecordId, LootRandomizerItem>
		{
			{ affixId, lootRandomizerItem }
		};
		_mockDatabase.Setup(x => x.AllLootRandomizer)
			.Returns(new ReadOnlyDictionary<RecordId, LootRandomizerItem>(lootRandomizerDict));

		_mockTranslationService.Setup(x => x.TranslateXTag(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
			.Returns("");

		// Act
		var result = _provider.LoadTable(tableId);

		// Assert - Float weight should be processed
		result.Should().NotBeNull();
		result!.Length.Should().Be(1);
		result.TotalWeight.Should().Be(25.5f);
	}

	[Fact]
	public void LoadTable_WithDuplicateAffix_CombinesWeights()
	{
		// Arrange - Two entries with same affix should have weights combined
		var tableId = RecordId.Create("test/table");
		var drc = new DBRecordCollection(tableId, "LootRandomizerTable");

		// Entry 1: randomizerName1 = affix1, randomizerWeight1 = 50
		var nameVar1 = new Variable("randomizerName1", VariableDataType.StringVar, 1);
		nameVar1[0] = "affix1";
		drc.Set(nameVar1);

		var weightVar1 = new Variable("randomizerWeight1", VariableDataType.Integer, 1);
		weightVar1[0] = 50;
		drc.Set(weightVar1);

		// Entry 2: randomizerName2 = affix1 (same!), randomizerWeight2 = 30
		var nameVar2 = new Variable("randomizerName2", VariableDataType.StringVar, 1);
		nameVar2[0] = "affix1";
		drc.Set(nameVar2);

		var weightVar2 = new Variable("randomizerWeight2", VariableDataType.Integer, 1);
		weightVar2[0] = 30;
		drc.Set(weightVar2);

		var lootTable = new Dictionary<RecordId, DBRecordCollection>
		{
			{ tableId, drc }
		};
		_mockDatabase.Setup(x => x.AllLootRandomizerTable)
			.Returns(new ReadOnlyDictionary<RecordId, DBRecordCollection>(lootTable));

		var affixId = RecordId.Create("affix1");
		var lootRandomizerItem = LootRandomizerItem.Default(affixId);
		var lootRandomizerDict = new Dictionary<RecordId, LootRandomizerItem>
		{
			{ affixId, lootRandomizerItem }
		};
		_mockDatabase.Setup(x => x.AllLootRandomizer)
			.Returns(new ReadOnlyDictionary<RecordId, LootRandomizerItem>(lootRandomizerDict));

		_mockTranslationService.Setup(x => x.TranslateXTag(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
			.Returns("");

		// Act
		var result = _provider.LoadTable(tableId);

		// Assert - Duplicate affix should have combined weights (50 + 30 = 80)
		result.Should().NotBeNull();
		result!.Length.Should().Be(1);
		result.TotalWeight.Should().Be(80f);
	}

	[Fact]
	public void LoadTable_WithUnknownAffixInAllLootRandomizer_LogsWarningWhenDebugEnabled()
	{
		// Arrange
		_userSettings.LootTableDebugEnabled = true;
		var tableId = RecordId.Create("test/table");
		var drc = CreateTestDBRecordCollectionWithIntWeight("randomizer", "unknown_affix", 100);

		var lootTable = new Dictionary<RecordId, DBRecordCollection>
		{
			{ tableId, drc }
		};
		_mockDatabase.Setup(x => x.AllLootRandomizerTable)
			.Returns(new ReadOnlyDictionary<RecordId, DBRecordCollection>(lootTable));

		// No entry in AllLootRandomizer for the unknown affix
		_mockDatabase.Setup(x => x.AllLootRandomizer)
			.Returns(new ReadOnlyDictionary<RecordId, LootRandomizerItem>(new Dictionary<RecordId, LootRandomizerItem>()));

		_mockTranslationService.Setup(x => x.TranslateXTag(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
			.Returns("");

		// Act
		var result = _provider.LoadTable(tableId);

		// Assert - Should still create entry with Unknown flag
		result.Should().NotBeNull();
		result!.Length.Should().Be(1);

		// Verify error was logged for unknown affix
		_mockLogger.Verify(x => x.Log(
			LogLevel.Error,
			It.IsAny<EventId>(),
			It.IsAny<It.IsAnyType>(),
			It.IsAny<Exception>(),
			It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
	}

	[Fact]
	public void LoadTable_WithUnknownAffix_CreatesDefaultEntry()
	{
		// Arrange
		var tableId = RecordId.Create("test/table");
		var drc = CreateTestDBRecordCollectionWithIntWeight("randomizer", "unknown_affix", 100);

		var lootTable = new Dictionary<RecordId, DBRecordCollection>
		{
			{ tableId, drc }
		};
		_mockDatabase.Setup(x => x.AllLootRandomizerTable)
			.Returns(new ReadOnlyDictionary<RecordId, DBRecordCollection>(lootTable));

		_mockDatabase.Setup(x => x.AllLootRandomizer)
			.Returns(new ReadOnlyDictionary<RecordId, LootRandomizerItem>(new Dictionary<RecordId, LootRandomizerItem>()));

		_mockTranslationService.Setup(x => x.TranslateXTag(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
			.Returns("");

		// Act
		var result = _provider.LoadTable(tableId);

		// Assert - Should still create entry with Unknown flag
		result.Should().NotBeNull();
		result!.Length.Should().Be(1);

		// Verify the entry exists with Unknown flag
		foreach (var kvp in result)
		{
			kvp.Value.LootRandomizer.Unknown.Should().BeTrue();
		}
	}
}
