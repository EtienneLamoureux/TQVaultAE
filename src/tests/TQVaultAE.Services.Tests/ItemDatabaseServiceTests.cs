using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Results;
using TQVaultAE.Application.Search;
using TQVaultAE.Application.Session;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Domain.Results;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

/// <summary>
/// Unit tests for ItemDatabaseService class
/// </summary>
public class ItemDatabaseServiceTests
{
	private readonly Mock<ILogger<ItemDatabaseService>> _mockLogger;
	private readonly Mock<IItemTextFormatter> _mockItemProvider;
	private readonly ItemDatabaseService _service;

	public ItemDatabaseServiceTests()
	{
		_mockLogger = new Mock<ILogger<ItemDatabaseService>>();
		_mockItemProvider = new Mock<IItemTextFormatter>();
		_service = new ItemDatabaseService(
			_mockLogger.Object,
			_mockItemProvider.Object
		);
	}

	private Item CreateTestItem()
	{
		return new Item
		{
			BaseItemId = RecordId.Create("test/item")
		};
	}

	[Fact]
	public async Task WarmUpAsync_LoadsEveryItem_EvenWhenOneFails()
	{
		// Arrange: one item whose display data cannot be built, two that can
		var broken = CreateTestItem();
		var good = new[] { CreateTestItem(), CreateTestItem() };
		_mockItemProvider.Setup(x => x.GetFriendlyNames(broken, It.IsAny<FriendlyNamesExtraScopes?>(), It.IsAny<bool>()))
			.Throws(new InvalidOperationException("broken record"));
		foreach (var item in good)
			_mockItemProvider.Setup(x => x.GetFriendlyNames(item, It.IsAny<FriendlyNamesExtraScopes?>(), It.IsAny<bool>()))
				.Returns(new ToFriendlyNameResult(item) { RequirementVariables = [] });
		_service.TryAddItemToDatabase(broken);
		foreach (var item in good)
			_service.TryAddItemToDatabase(item);

		// Act
		await _service.WarmUpAsync();

		// Assert
		_service.ItemDatabase.Where(r => r.Item != broken).Should().OnlyContain(r => r.FriendlyNames != null);
		_service.ItemDatabase.Single(r => r.Item == broken).FriendlyNames.Should().BeNull();
	}

	[Fact]
	public void CountItemsByRecord_CountsEachLoadedCopy_WhateverTheRecordPathSpelling()
	{
		// Arrange: two copies of the helm, one of the greaves, and an item of another record
		_service.TryAddItemToDatabase(new Item { BaseItemId = RecordId.Create("""records\xpack2\item\set\helm.dbr""") });
		_service.TryAddItemToDatabase(new Item { BaseItemId = RecordId.Create("""RECORDS/XPACK2/ITEM/SET/HELM.DBR""") });
		_service.TryAddItemToDatabase(new Item { BaseItemId = RecordId.Create("""records\xpack2\item\set\greaves.dbr""") });
		_service.TryAddItemToDatabase(new Item { BaseItemId = RecordId.Create("""records\item\other.dbr""") });
		var helm = RecordId.Create("""Records\XPack2\Item\Set\Helm.dbr""");
		var greaves = RecordId.Create("""records\xpack2\item\set\greaves.dbr""");
		var armband = RecordId.Create("""records\xpack2\item\set\armband.dbr""");

		// Act
		var counts = _service.CountItemsByRecord([helm, greaves, armband]);

		// Assert
		counts.Should().HaveCount(3);
		counts[helm].Should().Be(2);
		counts[greaves].Should().Be(1);
		counts[armband].Should().Be(0);
	}

	[Fact]
	public void TryAddItemToDatabase_WithNullItem_ReturnsFalse()
	{
		// Act
		var result = _service.TryAddItemToDatabase(null!);

		// Assert
		result.Should().BeFalse();
		_service.ItemDatabase.Should().BeEmpty();
	}

	[Fact]
	public void TryAddItemToDatabase_WithValidItem_AddsToDatabase()
	{
		// Arrange
		var item = CreateTestItem();
		_mockItemProvider.Setup(x => x.GetFriendlyNames(item, It.IsAny<FriendlyNamesExtraScopes?>(), It.IsAny<bool>()))
			.Returns(new ToFriendlyNameResult(item));

		// Act
		var result = _service.TryAddItemToDatabase(item);

		// Assert
		result.Should().BeTrue();
		_service.ItemDatabase.Should().HaveCount(1);
		_service.ItemDatabase.First().Item.Should().Be(item);
	}

	[Fact]
	public void TryAddItemToDatabase_WithDuplicateItem_ReturnsFalse()
	{
		// Arrange
		var item = CreateTestItem();
		_mockItemProvider.Setup(x => x.GetFriendlyNames(item, It.IsAny<FriendlyNamesExtraScopes?>(), It.IsAny<bool>()))
			.Returns(new ToFriendlyNameResult(item));

		_service.TryAddItemToDatabase(item);

		// Act
		var result = _service.TryAddItemToDatabase(item);

		// Assert
		result.Should().BeFalse();
		_service.ItemDatabase.Should().HaveCount(1);
	}

	[Fact]
	public void AddItemToDatabase_WithNullItem_DoesNotThrow()
	{
		// Act & Assert
		_service.Invoking(s => s.AddItemToDatabase(null!, "path", "container", 0, SackType.Vault))
			.Should().NotThrow();
		_service.ItemDatabase.Should().BeEmpty();
	}

	[Fact]
	public void AddItemToDatabase_WithValidItem_SetsItemPlaceAndAddsToDatabase()
	{
		// Arrange
		var item = CreateTestItem();
		_mockItemProvider.Setup(x => x.GetFriendlyNames(item, It.IsAny<FriendlyNamesExtraScopes?>(), It.IsAny<bool>()))
			.Returns(new ToFriendlyNameResult(item));

		// Act
		_service.AddItemToDatabase(item, "/vaults/test.vault", "TestVault", 1, SackType.Vault);

		// Assert
		_service.ItemDatabase.Should().HaveCount(1);
		var result = _service.ItemDatabase.First();
		result.Item.Place.Path.Should().Be("/vaults/test.vault");
		result.Item.Place.Name.Should().Be("TestVault");
		result.Item.Place.SackNumber.Should().Be(1);
		result.Item.Place.SackType.Should().Be(SackType.Vault);
		result.Item.Place.StashType.Should().BeNull();
	}

	[Fact]
	public void AddItemToDatabase_WithStashType_SetsStashType()
	{
		// Arrange
		var item = CreateTestItem();
		_mockItemProvider.Setup(x => x.GetFriendlyNames(item, It.IsAny<FriendlyNamesExtraScopes?>(), It.IsAny<bool>()))
			.Returns(new ToFriendlyNameResult(item));

		// Act
		_service.AddItemToDatabase(item, "/stashes/test.stash", "TestStash", 0, SackType.Stash, StashType.TransferStash);

		// Assert
		_service.ItemDatabase.Should().HaveCount(1);
		_service.ItemDatabase.First().Item.Place.StashType.Should().Be(StashType.TransferStash);
	}

	[Fact]
	public void RemoveItemFromDatabase_WithNullItem_DoesNotThrow()
	{
		// Act & Assert
		_service.Invoking(s => s.RemoveItemFromDatabase(null!))
			.Should().NotThrow();
	}

	[Fact]
	public void RemoveItemFromDatabase_WithExistingItem_RemovesFromDatabase()
	{
		// Arrange
		var item1 = CreateTestItem();
		var item2 = CreateTestItem();
		_mockItemProvider.Setup(x => x.GetFriendlyNames(It.IsAny<Item>(), It.IsAny<FriendlyNamesExtraScopes?>(), It.IsAny<bool>()))
			.Returns((Item i, FriendlyNamesExtraScopes? s, bool b) => new ToFriendlyNameResult(i));

		_service.TryAddItemToDatabase(item1);
		_service.TryAddItemToDatabase(item2);

		// Act
		_service.RemoveItemFromDatabase(item1);

		// Assert
		_service.ItemDatabase.Should().HaveCount(1);
		_service.ItemDatabase.First().Item.Should().Be(item2);
	}

	[Fact]
	public void RemoveContainerFromDatabase_RemovesOnlyTheItemsOfThatFile()
	{
		// Arrange: two items in a vault about to be deleted (path case differs), one in another vault
		var gone1 = CreateTestItem();
		var gone2 = CreateTestItem();
		var kept = CreateTestItem();
		_service.AddItemToDatabase(gone1, """C:\Vaults\Gear.vault.json""", "Gear", 0, SackType.Vault);
		_service.AddItemToDatabase(gone2, """C:\VAULTS\gear.vault.json""", "Gear", 3, SackType.Vault);
		_service.AddItemToDatabase(kept, """C:\Vaults\Rings.vault.json""", "Rings", 0, SackType.Vault);

		// Act
		_service.RemoveContainerFromDatabase("""C:\Vaults\Gear.vault.json""");

		// Assert
		_service.ItemDatabase.Select(r => r.Item).Should().Equal(kept);
		// A removed item can be indexed again, for example when the renamed vault is loaded.
		_service.TryAddItemToDatabase(gone1).Should().BeTrue();
	}

	[Fact]
	public void RemoveItemFromDatabase_WithNonExistingItem_DoesNotChangeDatabase()
	{
		// Arrange
		var item1 = CreateTestItem();
		var item2 = CreateTestItem();
		_mockItemProvider.Setup(x => x.GetFriendlyNames(It.IsAny<Item>(), It.IsAny<FriendlyNamesExtraScopes?>(), It.IsAny<bool>()))
			.Returns((Item i, FriendlyNamesExtraScopes? s, bool b) => new ToFriendlyNameResult(i));

		_service.TryAddItemToDatabase(item1);

		// Act
		_service.RemoveItemFromDatabase(item2);

		// Assert
		_service.ItemDatabase.Should().HaveCount(1);
	}

	private void NamesForAll()
		=> _mockItemProvider.Setup(x => x.GetFriendlyNames(It.IsAny<Item>(), It.IsAny<FriendlyNamesExtraScopes?>(), It.IsAny<bool>()))
			.Returns((Item i, FriendlyNamesExtraScopes? s, bool b) => new ToFriendlyNameResult(i));

	[Fact]
	public void IndexVault_NumbersEachItemByItsVaultBag()
	{
		// Arrange: an item in bag 0 and one in bag 2, bag 1 empty
		var vault = new PlayerCollection("TestVault", "/vaults/TestVault.vault") { IsVault = true };
		vault.CreateEmptySacks(3);
		var first = CreateTestItem();
		var third = CreateTestItem();
		vault.Sacks[0].AddItem(first);
		vault.Sacks[2].AddItem(third);
		NamesForAll();

		// Act
		_service.IndexVault(vault, vault.PlayerFile, vault.PlayerName);

		// Assert: the search jump opens the bag that holds each item
		var locations = _service.ItemDatabase.ToDictionary(r => r.Item, SearchResultNavigation.Resolve);
		locations[first].Should().Be(new ResultLocation(ResultPanelKind.Vault, 0, first.Location));
		locations[third].Should().Be(new ResultLocation(ResultPanelKind.Vault, 2, third.Location));
	}

	[Fact]
	public void IndexPlayer_EquipmentAndBags_JumpToTheirPanels()
	{
		// Arrange: one item in the main sack, one in the first bag, one worn
		var player = new PlayerCollection("TestPlayer", "/players/_TestPlayer/Player.chr");
		player.CreateEmptySacks(2);
		var main = CreateTestItem();
		var bagged = CreateTestItem();
		var worn = CreateTestItem();
		player.Sacks[0].AddItem(main);
		player.Sacks[1].AddItem(bagged);
		player.EquipmentSack = new SackCollection();
		player.EquipmentSack.AddItem(worn);
		NamesForAll();

		// Act
		_service.IndexPlayer(player, player.PlayerFile, player.PlayerName);

		// Assert: the equipment opens the equipment tab of the stash panel, as a drop there numbers it
		var locations = _service.ItemDatabase.ToDictionary(r => r.Item, SearchResultNavigation.Resolve);
		locations[main].Panel.Should().Be(ResultPanelKind.PlayerMainSack);
		locations[bagged].Should().Be(new ResultLocation(ResultPanelKind.PlayerBag, 0, bagged.Location));
		locations[worn].Should().Be(new ResultLocation(ResultPanelKind.Equipment, BagIdConstants.BAGID_EQUIPMENTPANEL, worn.Location));
		worn.Place.SackType.Should().Be(SackType.Equipment);
	}

	[Theory]
	[InlineData(StashType.PlayerStash, ResultPanelKind.CharacterStash, BagIdConstants.BAGID_PLAYERSTASH)]
	[InlineData(StashType.TransferStash, ResultPanelKind.SharedStash, BagIdConstants.BAGID_TRANSFERSTASH)]
	[InlineData(StashType.RelicVaultStash, ResultPanelKind.SharedStash, BagIdConstants.BAGID_RELICVAULTSTASH)]
	public void IndexStash_NumbersTheItemsByTheirStashTab(StashType stashType, ResultPanelKind panel, int tab)
	{
		// Arrange
		var stash = new Stash("TestPlayer", "/stashes/TestStash") { Sack = new SackCollection() };
		var item = CreateTestItem();
		stash.Sack.AddItem(item);
		NamesForAll();

		// Act
		_service.IndexStash(stash, stash.StashFile, "Stash name", stashType);

		// Assert
		var result = _service.ItemDatabase.Single();
		result.Item.Place.StashType.Should().Be(stashType);
		result.ContainerName.Should().Be("Stash name");
		SearchResultNavigation.Resolve(result).Should().Be(new ResultLocation(panel, tab, item.Location));
	}

	[Fact]
	public void IndexStash_WithoutSack_AddsNothing()
	{
		_service.IndexStash(new Stash("TestPlayer", "/stashes/Missing"), "/stashes/Missing", "Stash", StashType.PlayerStash);

		_service.ItemDatabase.Should().BeEmpty();
	}

	[Fact]
	public void FullTextSearch_WithNullOrEmptySearchText_ReturnsEmpty()
	{
		// Act
		var resultNull = _service.FullTextSearch(null!);
		var resultEmpty = _service.FullTextSearch("");
		var resultWhitespace = _service.FullTextSearch("   ");

		// Assert
		resultNull.Should().BeEmpty();
		resultEmpty.Should().BeEmpty();
		resultWhitespace.Should().BeEmpty();
	}

	[Fact]
	public void FullTextSearch_WithEmptyDatabase_ReturnsEmpty()
	{
		// Act
		var result = _service.FullTextSearch("test");

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public void ExecuteAdvancedSearch_WithHasSkill_KeepsOnlyTheItemsGrantingASkill()
	{
		// Arrange
		var withSkill = CreateTestItem();
		var withoutSkill = CreateTestItem();
		_mockItemProvider.Setup(x => x.HasGrantedSkill(withSkill)).Returns(true);
		_mockItemProvider.Setup(x => x.HasGrantedSkill(withoutSkill)).Returns(false);
		_service.TryAddItemToDatabase(withSkill);
		_service.TryAddItemToDatabase(withoutSkill);
		var request = new AdvancedSearchRequest
		{
			InitialResults = _service.ItemDatabase.ToList(),
			HasSkill = true,
		};

		// Act
		var result = _service.ExecuteAdvancedSearch(request);

		// Assert
		result.Should().ContainSingle().Which.Item.Should().BeSameAs(withSkill);
	}

	[Fact]
	public void ExecuteAdvancedSearch_WithNullInitialResults_ReturnsEmpty()
	{
		// Arrange
		var request = new AdvancedSearchRequest
		{
			InitialResults = null!
		};

		// Act
		var result = _service.ExecuteAdvancedSearch(request);

		// Assert
		result.Should().BeEmpty();
	}

}
