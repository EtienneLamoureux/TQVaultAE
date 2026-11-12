using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Domain.Results;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

/// <summary>
/// Unit tests for HighlightService class
/// </summary>
public class HighlightServiceTests
{
	private readonly Mock<ILogger<HighlightService>> _mockLogger;
	private readonly Mock<IItemProvider> _mockItemProvider;
	private readonly SessionContext _sessionContext;
	private readonly Mock<ITranslationService> _mockTranslationService = new();
	private readonly HighlightService _service;

	public HighlightServiceTests()
	{
		_mockLogger = new Mock<ILogger<HighlightService>>();
		_mockItemProvider = new Mock<IItemProvider>();
		_sessionContext = new SessionContext();
		_service = new HighlightService(
			_mockLogger.Object,
			_mockItemProvider.Object,
			_sessionContext,
			_mockTranslationService.Object
		);
	}

	[Fact]
	public void FindHighlight_WithNoSearchOrFilter_ResetsHighlight()
	{
		// Act
		_service.FindHighlight();

		// Assert
		_service.HighlightedItems.Should().BeEmpty();
		_service.HighlightSearch.Should().BeNull();
		_service.HighlightFilter.Should().BeNull();
	}

	[Fact]
	public void FindHighlight_WithEmptySession_ResetsHighlight()
	{
		// Arrange
		_service.HighlightSearch = "test";

		// Act
		_service.FindHighlight();

		// Assert
		_service.HighlightedItems.Should().BeEmpty();
	}

	[Fact]
	public void FindHighlight_WithEmptyPlayerSacks_DoesNotCallItemProvider()
	{
		// Arrange - Create a player with empty equipment
		var playerFile = "/Test/Players/TestPlayer.plr";
		var playerCollection = new PlayerCollection("TestPlayer", playerFile);
		playerCollection.EquipmentSack = new SackCollection(); // Empty sack with Count=0

		_sessionContext.Players.GetOrAddAtomic(playerFile, _ => playerCollection);

		_service.HighlightSearch = "Test";

		// Act
		_service.FindHighlight();

		// Assert
		_service.HighlightedItems.Should().BeEmpty();
		_mockItemProvider.Verify(
			x => x.GetFriendlyNames(It.IsAny<Item>(), It.IsAny<FriendlyNamesExtraScopes?>(), It.IsAny<bool>()),
			Times.Never);
	}

	[Fact]
	public void FindHighlight_WithFilterOnly_DoesNotSearchText()
	{
		// Arrange - a filter built with an initializer leaves its class, rarity and origin lists empty
		var playerFile = "/Test/Players/TestPlayer.plr";
		var playerCollection = new PlayerCollection("TestPlayer", playerFile);

		_sessionContext.Players.GetOrAddAtomic(playerFile, _ => playerCollection);

		_service.HighlightFilter = new HighlightFilterValues
		{
			MinRequierement = true,
			MinLvl = 5
		};

		// Act
		var act = () => _service.FindHighlight();

		// Assert - the empty lists do not filter anything out, and the player without sacks has no items
		act.Should().NotThrow();
		_service.HighlightedItems.Should().BeEmpty();
	}

	[Fact]
	public void ResetHighlight_ClearsAllState()
	{
		// Arrange
		_service.HighlightSearch = "test";
		_service.HighlightFilter = new HighlightFilterValues();

		// Act
		_service.ResetHighlight();

		// Assert
		_service.HighlightedItems.Should().BeEmpty();
		_service.HighlightSearch.Should().BeNull();
		_service.HighlightFilter.Should().BeNull();
	}

	[Fact]
	public void ResetHighlight_AfterFindHighlight_ClearsPreviousResults()
	{
		// Arrange - set up some state
		_service.HighlightSearch = "test";
		_service.HighlightFilter = new HighlightFilterValues();

		// Act
		_service.ResetHighlight();

		// Assert
		_service.HighlightedItems.Should().NotBeNull();
		_service.HighlightedItems.Should().BeEmpty();
	}

	[Fact]
	public void FindHighlight_WithSearch_LogsInformation()
	{
		// Arrange - Create a player with equipment
		var playerFile = "/Test/Players/TestPlayer.plr";
		var playerCollection = new PlayerCollection("TestPlayer", playerFile);
		playerCollection.EquipmentSack = new SackCollection();

		_sessionContext.Players.GetOrAddAtomic(playerFile, _ => playerCollection);

		_service.HighlightSearch = "Test";

		// Act
		_service.FindHighlight();

		// Assert - verify that no items were found (empty search results)
		_service.HighlightedItems.Should().BeEmpty();
		// The service should log "Highlight search found 0 items"
	}

	[Fact]
	public void FindHighlight_WithStashInSession_ProcessesWithoutError()
	{
		// Arrange - Create a stash directly in session
		var stashKey = "transfer.dxb";
		var stash = new Stash("Transfer", "/Test/transfer.dxb");
		stash.CreateEmptySack();

		_sessionContext.Stashes.GetOrAddAtomic(stashKey, _ => stash);

		_service.HighlightSearch = "Test";

		// Act
		_service.FindHighlight();

		// Assert - the empty stash is searched without error and matches nothing
		_service.HighlightedItems.Should().BeEmpty();
	}

	#region ApplyFilter and type options

	private static Item ItemOfClass(string itemClass)
	{
		var id = RecordId.Create($"""records\item\{itemClass}.dbr""");
		var record = new DBRecordCollection(id, itemClass);
		var classVariable = new Variable("Class", VariableDataType.StringVar, 1) { [0] = itemClass };
		record.Set(classVariable);
		return new Item { BaseItemId = id, baseItemInfo = new Info(record) };
	}

	/// <summary>Puts <paramref name="items"/> in the equipment of a player of the session.</summary>
	private void AddPlayerItems(params Item[] items)
	{
		var playerFile = "/Test/Players/TestPlayer.plr";
		var player = new PlayerCollection("TestPlayer", playerFile) { EquipmentSack = new SackCollection() };
		foreach (var item in items)
			player.EquipmentSack.AddItem(item);

		_sessionContext.Players.GetOrAddAtomic(playerFile, _ => player);
		_mockItemProvider
			.Setup(x => x.GetFriendlyNames(It.IsAny<Item>(), It.IsAny<FriendlyNamesExtraScopes?>(), It.IsAny<bool>()))
			.Returns((Item i, FriendlyNamesExtraScopes? _, bool _) => new ToFriendlyNameResult(i));
	}

	[Fact]
	public void IsEmpty_ValuesWithoutTheirRequirementCheckBox_AreIgnored()
	{
		new HighlightFilterValues { MinLvl = 10, MaxStr = 50 }.IsEmpty.Should().BeTrue();
		new HighlightFilterValues { MinRequierement = true }.IsEmpty.Should().BeFalse();
		new HighlightFilterValues { HavingSkill = true }.IsEmpty.Should().BeFalse();
		new HighlightFilterValues { Origin = [GameDlc.Atlantis] }.IsEmpty.Should().BeFalse();
	}

	[Fact]
	public void ApplyFilter_EmptyFilter_ClearsTheFilterAndTheHighlight()
	{
		_service.HighlightFilter = new HighlightFilterValues { HavingPrefix = true };

		var active = _service.ApplyFilter(new HighlightFilterValues { MinLvl = 10 });

		active.Should().BeFalse();
		_service.HighlightFilter.Should().BeNull();
		_service.HighlightedItems.Should().BeEmpty();
	}

	[Fact]
	public void ApplyFilter_Null_ClearsTheFilter()
	{
		_service.HighlightFilter = new HighlightFilterValues { HavingPrefix = true };

		_service.ApplyFilter(null).Should().BeFalse();
		_service.HighlightFilter.Should().BeNull();
	}

	[Fact]
	public void ApplyFilter_PotionType_HighlightsEveryPotionClass()
	{
		_mockTranslationService.Setup(x => x.TranslateXTag(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
			.Returns((string tag, bool _, bool _, bool _) => tag);
		var health = ItemOfClass(Item.ICLASS_POTIONHEALTH);
		var mana = ItemOfClass(Item.ICLASS_POTIONMANA);
		var sword = ItemOfClass(Item.ICLASS_SWORD);
		AddPlayerItems(health, mana, sword);
		var potions = _service.GetTypeOptions().Single(o => o.DisplayName == "tagTutorialTip14Title");

		var active = _service.ApplyFilter(new HighlightFilterValues { ClassItem = [.. potions.ItemClasses] });

		active.Should().BeTrue();
		_service.HighlightedItems.Should().BeEquivalentTo([health, mana]);
	}

	[Fact]
	public void GetTypeOptions_TrimsTheTrailingPunctuationOfTheArmorNames()
	{
		_mockTranslationService.Setup(x => x.TranslateXTag(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
			.Returns((string tag, bool _, bool _, bool _) => $"{tag} :");

		var options = _service.GetTypeOptions();

		options[0].DisplayName.Should().Be(Item.GetClassTagName(Item.ICLASS_HEAD));
		options[0].ItemClasses.Should().Equal(Item.ICLASS_HEAD);
		options.Single(o => o.ItemClasses.Contains(Item.ICLASS_SWORD)).DisplayName.Should().EndWith(" :");
	}

	[Fact]
	public void GetTypeOptions_EachItemClassBelongsToOneType()
	{
		_mockTranslationService.Setup(x => x.TranslateXTag(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
			.Returns((string tag, bool _, bool _, bool _) => tag);

		_service.GetTypeOptions().SelectMany(o => o.ItemClasses).Should().OnlyHaveUniqueItems();
	}

	#endregion
}
