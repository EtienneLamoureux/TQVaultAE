using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Results;
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
	private readonly Mock<IItemProvider> _mockItemProvider;
	private readonly SessionContext _sessionContext;
	private readonly ItemDatabaseService _service;

	public ItemDatabaseServiceTests()
	{
		_mockLogger = new Mock<ILogger<ItemDatabaseService>>();
		_mockItemProvider = new Mock<IItemProvider>();
		_sessionContext = new SessionContext();
		_service = new ItemDatabaseService(
			_mockLogger.Object,
			_mockItemProvider.Object,
			_sessionContext
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

	[Fact]
	public void ClearItemDatabase_ClearsAllItems()
	{
		// Arrange
		var item = CreateTestItem();
		_mockItemProvider.Setup(x => x.GetFriendlyNames(item, It.IsAny<FriendlyNamesExtraScopes?>(), It.IsAny<bool>()))
			.Returns(new ToFriendlyNameResult(item));
		_service.TryAddItemToDatabase(item);

		// Act
		_service.ClearItemDatabase();

		// Assert
		_service.ItemDatabase.Should().BeEmpty();
	}

	[Fact]
	public void RebuildItemDatabase_WithEmptySession_DoesNotThrow()
	{
		// Act
		_service.Invoking(s => s.RebuildItemDatabase())
			.Should().NotThrow();

		// Assert
		_service.ItemDatabase.Should().BeEmpty();
	}

	[Fact]
	public void RebuildItemDatabase_WithVaults_AddsVaultItems()
	{
		// Arrange
		var vaultPath = "/vaults/TestVault.vault";
		var vault = new PlayerCollection("TestVault", vaultPath) { IsVault = true };
		vault.CreateEmptySacks(1);
		vault.Sacks[0].AddItem(CreateTestItem());
		vault.Sacks[0].AddItem(CreateTestItem());
		_sessionContext.Vaults.GetOrAddAtomic(vaultPath, _ => vault);

		_mockItemProvider.Setup(x => x.GetFriendlyNames(It.IsAny<Item>(), It.IsAny<FriendlyNamesExtraScopes?>(), It.IsAny<bool>()))
			.Returns((Item i, FriendlyNamesExtraScopes? s, bool b) => new ToFriendlyNameResult(i));

		// Act
		_service.RebuildItemDatabase();

		// Assert
		_service.ItemDatabase.Should().HaveCount(2);
	}

	[Fact]
	public void RebuildItemDatabase_WithPlayers_AddsPlayerItems()
	{
		// Arrange
		var playerPath = "/players/TestPlayer";
		var player = new PlayerCollection("TestPlayer", playerPath);
		player.CreateEmptySacks(1);
		player.Sacks[0].AddItem(CreateTestItem());
		_sessionContext.Players.GetOrAddAtomic(playerPath, _ => player);

		_mockItemProvider.Setup(x => x.GetFriendlyNames(It.IsAny<Item>(), It.IsAny<FriendlyNamesExtraScopes?>(), It.IsAny<bool>()))
			.Returns((Item i, FriendlyNamesExtraScopes? s, bool b) => new ToFriendlyNameResult(i));

		// Act
		_service.RebuildItemDatabase();

		// Assert
		_service.ItemDatabase.Should().HaveCount(1);
	}

	[Fact]
	public void RebuildItemDatabase_WithEquipmentSack_AddsEquipmentItems()
	{
		// Arrange
		var playerPath = "/players/TestPlayer";
		var player = new PlayerCollection("TestPlayer", playerPath);
		player.EquipmentSack = new SackCollection();
		player.EquipmentSack.AddItem(CreateTestItem());
		_sessionContext.Players.GetOrAddAtomic(playerPath, _ => player);

		_mockItemProvider.Setup(x => x.GetFriendlyNames(It.IsAny<Item>(), It.IsAny<FriendlyNamesExtraScopes?>(), It.IsAny<bool>()))
			.Returns((Item i, FriendlyNamesExtraScopes? s, bool b) => new ToFriendlyNameResult(i));

		// Act
		_service.RebuildItemDatabase();

		// Assert
		_service.ItemDatabase.Should().HaveCount(1);
		_service.ItemDatabase.First().Item.Place.SackType.Should().Be(SackType.Equipment);
	}

	[Fact]
	public void RebuildItemDatabase_WithPlayerStash_AddsStashItems()
	{
		// Arrange
		var stashPath = "/stashes/TestStash";
		var stash = new Stash("TestPlayer", stashPath);
		stash.Sack = new SackCollection();
		stash.Sack.AddItem(CreateTestItem());
		_sessionContext.Stashes.GetOrAddAtomic(stashPath, _ => stash);

		_mockItemProvider.Setup(x => x.GetFriendlyNames(It.IsAny<Item>(), It.IsAny<FriendlyNamesExtraScopes?>(), It.IsAny<bool>()))
			.Returns((Item i, FriendlyNamesExtraScopes? s, bool b) => new ToFriendlyNameResult(i));

		// Act
		_service.RebuildItemDatabase();

		// Assert
		_service.ItemDatabase.Should().HaveCount(1);
		_service.ItemDatabase.First().Item.Place.SackType.Should().Be(SackType.Stash);
	}

	[Fact]
	public void RebuildItemDatabase_WithTransferStash_SetsCorrectStashType()
	{
		// Arrange
		var stashPath = "/stashes/TransferStash";
		var stash = new Stash("TestPlayer", stashPath);
		stash.Sack = new SackCollection();
		stash.Sack.StashType = StashType.TransferStash;
		stash.Sack.AddItem(CreateTestItem());
		_sessionContext.Stashes.GetOrAddAtomic(stashPath, _ => stash);

		_mockItemProvider.Setup(x => x.GetFriendlyNames(It.IsAny<Item>(), It.IsAny<FriendlyNamesExtraScopes?>(), It.IsAny<bool>()))
			.Returns((Item i, FriendlyNamesExtraScopes? s, bool b) => new ToFriendlyNameResult(i));

		// Act
		_service.RebuildItemDatabase();

		// Assert
		_service.ItemDatabase.Should().HaveCount(1);
		_service.ItemDatabase.First().Item.Place.StashType.Should().Be(StashType.TransferStash);
	}

	[Fact]
	public void RebuildItemDatabase_WithRelicVaultStash_SetsCorrectStashType()
	{
		// Arrange
		var stashPath = "/stashes/RelicVaultStash";
		var stash = new Stash("TestPlayer", stashPath);
		stash.Sack = new SackCollection();
		stash.Sack.StashType = StashType.RelicVaultStash;
		stash.Sack.AddItem(CreateTestItem());
		_sessionContext.Stashes.GetOrAddAtomic(stashPath, _ => stash);

		_mockItemProvider.Setup(x => x.GetFriendlyNames(It.IsAny<Item>(), It.IsAny<FriendlyNamesExtraScopes?>(), It.IsAny<bool>()))
			.Returns((Item i, FriendlyNamesExtraScopes? s, bool b) => new ToFriendlyNameResult(i));

		// Act
		_service.RebuildItemDatabase();

		// Assert
		_service.ItemDatabase.Should().HaveCount(1);
		_service.ItemDatabase.First().Item.Place.StashType.Should().Be(StashType.RelicVaultStash);
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
