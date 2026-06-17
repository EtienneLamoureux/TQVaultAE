using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Text.Json;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

public class ItemExchangeServiceVaultTests
{
	private readonly JsonSerializerOptions _jsonOptions;
	private readonly Mock<IVaultService> _mockVaultService;
	private readonly Mock<IUIService> _mockUIService;
	private readonly ItemExchangeService _service;

	public ItemExchangeServiceVaultTests()
	{
		_jsonOptions = new JsonSerializerOptions
		{
			IncludeFields = true,
			PropertyNameCaseInsensitive = true,
			WriteIndented = true
		};

		_mockVaultService = new Mock<IVaultService>();
		_mockUIService = new Mock<IUIService>();
		_service = new ItemExchangeService(_jsonOptions);
	}

	[Fact]
	public void SerializePlayerCollection_ShouldProduceValidExportFormat()
	{
		var vault = new PlayerCollection("TestVault", "test.vault") { IsVault = true };
		vault.CreateEmptySacks(12);

		var item = new Item
		{
			BaseItemId = "records/gear/armor/test.dbr",
			Seed = 12345,
			PositionX = 2,
			PositionY = 3,
			Width = 2,
			Height = 3
		};
		vault.Sacks[0].AddItem(item);

		var json = _service.SerializePlayerCollection(vault);

		json.Should().NotBeNullOrEmpty();

		var doc = JsonDocument.Parse(json);
		doc.RootElement.GetProperty("formatVersion").GetInt32().Should().Be(1);
		doc.RootElement.GetProperty("scope").GetString().Should().Be("Vault");

		var data = doc.RootElement.GetProperty("data");
		data.GetProperty("name").GetString().Should().Be("TestVault");

		var sacks = data.GetProperty("sacks");
		sacks.GetArrayLength().Should().Be(1);
		sacks[0].GetProperty("sackNumber").GetInt32().Should().Be(0);
		sacks[0].GetProperty("items").GetArrayLength().Should().Be(1);
	}

	[Fact]
	public void SerializeSackCollection_ShouldProduceValidExportFormat()
	{
		var sack = new SackCollection { SackType = SackType.Vault };
		var item = new Item
		{
			BaseItemId = "records/gear/armor/test.dbr",
			Seed = 54321,
			PositionX = 5,
			PositionY = 7,
			Width = 2,
			Height = 3
		};
		sack.AddItem(item);

		var json = _service.SerializeSackCollection(sack, 3);

		json.Should().NotBeNullOrEmpty();

		var doc = JsonDocument.Parse(json);
		doc.RootElement.GetProperty("formatVersion").GetInt32().Should().Be(1);
		doc.RootElement.GetProperty("scope").GetString().Should().Be("Tab");

		var data = doc.RootElement.GetProperty("data");
		data.GetProperty("sackNumber").GetInt32().Should().Be(3);
		data.GetProperty("items").GetArrayLength().Should().Be(1);
	}

	[Fact]
	public void ImportFromJson_WithVaultScope_ShouldReturnVaultImportResult()
	{
		var vault = new PlayerCollection("ImportedVault", "imported.vault") { IsVault = true };
		vault.CreateEmptySacks(12);

		var item = new Item
		{
			BaseItemId = "records/gear/armor/test.dbr",
			Seed = 12345,
			PositionX = 2,
			PositionY = 3,
			Width = 2,
			Height = 3
		};
		vault.Sacks[0].AddItem(item);

		var json = _service.SerializePlayerCollection(vault);
		var result = _service.ImportFromJson(json);

		result.Success.Should().BeTrue();
		result.Scope.Should().Be(ExportScope.Vault);
		result.VaultName.Should().Be("ImportedVault");
		result.SackItems.Should().NotBeNull();
		result.SackItems.Should().ContainKey(0);
		result.SackItems[0].Should().HaveCount(1);
		result.SackItems[0][0].BaseItemId.Should().Be(item.BaseItemId);
	}

	[Fact]
	public void ImportFromJson_WithTabScope_ShouldReturnTabImportResult()
	{
		var sack = new SackCollection { SackType = SackType.Vault };
		var item = new Item
		{
			BaseItemId = "records/gear/armor/test.dbr",
			Seed = 54321,
			PositionX = 5,
			PositionY = 7,
			Width = 2,
			Height = 3
		};
		sack.AddItem(item);

		var json = _service.SerializeSackCollection(sack, 3);
		var result = _service.ImportFromJson(json);

		result.Success.Should().BeTrue();
		result.Scope.Should().Be(ExportScope.Tab);
		result.SackNumber.Should().Be(3);
		result.Items.Should().HaveCount(1);
		result.Items[0].BaseItemId.Should().Be(item.BaseItemId);
	}

	[Fact]
	public void ImportVaultInto_ShouldClearAllTabsAndImportItems()
	{
		var vault = new PlayerCollection("TargetVault", "target.vault") { IsVault = true };
		vault.CreateEmptySacks(12);

		var existingItem = new Item { BaseItemId = "records/old.dbr", PositionX = 0, PositionY = 0, Width = 1, Height = 1 };
		vault.Sacks[0].AddItem(existingItem);
		vault.Sacks[0].Count.Should().Be(1);// existing

		var importedItems = new Dictionary<int, List<Item>>
		{
			[0] = new()
			{
				new Item { BaseItemId = "records/gear/new1.dbr", PositionX = 0, PositionY = 0, Width = 1, Height = 1 },
				new Item { BaseItemId = "records/gear/new2.dbr", PositionX = 1, PositionY = 0, Width = 1, Height = 1 }
			},
			[1] = new()
			{
				new Item { BaseItemId = "records/gear/new3.dbr", PositionX = 0, PositionY = 0, Width = 1, Height = 1 }
			}
		};

		var importResult = ImportResult.SucceededVault("Imported", importedItems);

		_service.ImportVaultInto(vault, importResult);

		vault.Sacks[0].Count.Should().Be(2);// old cleared, new ones imported
		vault.Sacks[0].items[0].BaseItemId.Should().Be("records/gear/new1.dbr");
		vault.Sacks[0].items[1].BaseItemId.Should().Be("records/gear/new2.dbr");
		vault.Sacks[1].Count.Should().Be(1);
		vault.Sacks[1].items[0].BaseItemId.Should().Be("records/gear/new3.dbr");

		// Other sacks should be empty
		for (int i = 2; i < 12; i++)
			vault.Sacks[i].IsEmpty.Should().BeTrue();
	}

	[Fact]
	public void ImportVaultInto_WithNullVault_ShouldDoNothing()
	{
		var importData = ImportResult.SucceededVault("Test", new Dictionary<int, List<Item>>());
		var act = () => _service.ImportVaultInto(null, importData);
		act.Should().NotThrow();
	}

	[Fact]
	public void ImportVaultInto_WithNullImportData_ShouldDoNothing()
	{
		var vault = new PlayerCollection("Target", "target.vault") { IsVault = true };
		var act = () => _service.ImportVaultInto(vault, null);
		act.Should().NotThrow();
	}

	[Fact]
	public void ImportVaultInto_WithNullSackItems_ShouldDoNothing()
	{
		var vault = new PlayerCollection("Target", "target.vault") { IsVault = true };
		vault.CreateEmptySacks(3);
		var importResult = new ImportResult
		{
			Success = true,
			Scope = ExportScope.Vault,
			SackItems = null,
			SackIconInfo = null
		};
		_service.ImportVaultInto(vault, importResult);
		vault.Sacks[0].IsEmpty.Should().BeTrue();
	}

	[Fact]
	public void ImportVaultInto_WithOutOfRangeSackNumber_ShouldSkip()
	{
		var vault = new PlayerCollection("Target", "target.vault") { IsVault = true };
		vault.CreateEmptySacks(2);
		var importedItems = new Dictionary<int, List<Item>>
		{
			[-1] = new() { new Item { BaseItemId = "neg.dbr", PositionX = 0, PositionY = 0, Width = 1, Height = 1 } },
			[99] = new() { new Item { BaseItemId = "oor.dbr", PositionX = 0, PositionY = 0, Width = 1, Height = 1 } }
		};
		var importResult = ImportResult.SucceededVault("Test", importedItems);

		_service.ImportVaultInto(vault, importResult);

		vault.Sacks[0].IsEmpty.Should().BeTrue();
		vault.Sacks[1].IsEmpty.Should().BeTrue();
	}

	[Fact]
	public void ImportVaultInto_WithBagButtonIconInfo_ShouldPreserveIconInfo()
	{
		var vault = new PlayerCollection("Target", "target.vault") { IsVault = true };
		vault.CreateEmptySacks(3);
		var iconInfo = new BagButtonIconInfo
		{
			Label = "MyTab",
			DisplayMode = BagButtonDisplayMode.Label
		};
		var importedItems = new Dictionary<int, List<Item>>();
		var sackIconInfo = new Dictionary<int, BagButtonIconInfo> { [0] = iconInfo };
		var importResult = ImportResult.SucceededVault("Test", importedItems, sackIconInfo);

		_service.ImportVaultInto(vault, importResult);

		vault.Sacks[0].BagButtonIconInfo.Should().NotBeNull();
		vault.Sacks[0].BagButtonIconInfo.Label.Should().Be("MyTab");
		vault.Sacks[0].BagButtonIconInfo.DisplayMode.Should().Be(BagButtonDisplayMode.Label);
	}

	[Fact]
	public void SerializePlayerCollection_EmptyVault_ShouldSkipEmptySacks()
	{
		var vault = new PlayerCollection("EmptyVault", "empty.vault") { IsVault = true };
		vault.CreateEmptySacks(12);

		var json = _service.SerializePlayerCollection(vault);

		var doc = JsonDocument.Parse(json);
		var sacks = doc.RootElement.GetProperty("data").GetProperty("sacks");
		sacks.GetArrayLength().Should().Be(0);
	}

	[Fact]
	public void ImportFromJson_WithVaultScope_NoSacks_ShouldSucceed()
	{
		var json = """{"formatVersion":1,"scope":"Vault","data":{"name":"Empty","sacks":[]}}""";

		var result = _service.ImportFromJson(json);

		result.Success.Should().BeTrue();
		result.Scope.Should().Be(ExportScope.Vault);
		result.VaultName.Should().Be("Empty");
		result.SackItems.Should().BeEmpty();
	}
}
