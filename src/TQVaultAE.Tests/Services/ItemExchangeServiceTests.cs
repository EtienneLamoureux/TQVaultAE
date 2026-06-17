using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Text.Json;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

public class ItemExchangeServiceTests
{
	private readonly JsonSerializerOptions _jsonOptions;
	private readonly ItemExchangeService _service;

	public ItemExchangeServiceTests()
	{
		_jsonOptions = new JsonSerializerOptions
		{
			IncludeFields = true,
			PropertyNameCaseInsensitive = true,
			WriteIndented = true
		};

		_service = new ItemExchangeService(_jsonOptions);
	}

	[Fact]
	public void SerializeItem_ShouldProduceValidExportFormat()
	{
		var item = new Item
		{
			BaseItemId = "records/gear/armor/test.dbr",
			prefixID = "records/affixes/prefix/test_prefix.dbr",
			suffixID = "records/affixes/suffix/test_suffix.dbr",
			Seed = 12345,
			PositionX = 5,
			PositionY = 10,
			StackSize = 1,
			Width = 2,
			Height = 3,
			Var1 = 0,
			Var2 = 2035248,
			itemScalePercent = 1.0F
		};

		var json = _service.SerializeItem(item);

		json.Should().NotBeNullOrEmpty();

		var doc = JsonDocument.Parse(json);
		doc.RootElement.GetProperty("formatVersion").GetInt32().Should().Be(1);
		doc.RootElement.GetProperty("scope").GetString().Should().Be("Item");

		var data = doc.RootElement.GetProperty("data");
		data.GetProperty("baseName").GetString().Should().Be("records/gear/armor/test.dbr");
		data.GetProperty("seed").GetInt32().Should().Be(12345);
		data.GetProperty("pointX").GetInt32().Should().Be(5);
		data.GetProperty("pointY").GetInt32().Should().Be(10);
	}

	[Fact]
	public void ImportFromJson_WithValidItemJson_ShouldReturnImportResult()
	{
		var item = new Item
		{
			BaseItemId = "records/gear/armor/test.dbr",
			prefixID = "records/affixes/prefix/test_prefix.dbr",
			Seed = 12345,
			PositionX = 5,
			PositionY = 10,
			Width = 2,
			Height = 3
		};

		var json = _service.SerializeItem(item);
		var result = _service.ImportFromJson(json);

		result.Should().NotBeNull();
		result.Scope.Should().Be(ExportScope.Item);
		result.Item.Should().NotBeNull();
		result.Item.BaseItemId.Should().Be(item.BaseItemId);
		result.Item.Seed.Should().Be(item.Seed);
	}

	[Fact]
	public void IsPasteBinUrl_WithValidUrl_ShouldReturnTrue()
	{
		_service.IsPasteBinUrl("https://pastebin.com/abc123").Should().BeTrue();
		_service.IsPasteBinUrl("https://pastebin.com/ABC123").Should().BeTrue();
	}

	[Fact]
	public void IsPasteBinUrl_WithInvalidUrl_ShouldReturnFalse()
	{
		_service.IsPasteBinUrl("some random text").Should().BeFalse();
		_service.IsPasteBinUrl("https://google.com").Should().BeFalse();
		_service.IsPasteBinUrl(string.Empty).Should().BeFalse();
	}

	[Fact]
	public void ImportFromJson_WithInvalidJson_ShouldReturnFailure()
	{
		var result = _service.ImportFromJson("not valid json");

		result.Should().NotBeNull();
		result.Success.Should().BeFalse();
	}

	[Fact]
	public void SerializeSackCollection_ShouldProduceValidTabScopeExportFormat()
	{
		var sack = new SackCollection();
		sack.SackType = SackType.Vault;
		sack.AddItem(new Item
		{
			BaseItemId = "records/gear/armor/helm.dbr",
			Seed = 42,
			PositionX = 3,
			PositionY = 7,
			Width = 2,
			Height = 2
		});
		sack.AddItem(new Item
		{
			BaseItemId = "records/gear/weapon/sword.dbr",
			Seed = 99,
			PositionX = 10,
			PositionY = 15,
			Width = 1,
			Height = 3
		});

		var json = _service.SerializeSackCollection(sack, 2);

		json.Should().NotBeNullOrEmpty();

		var doc = JsonDocument.Parse(json);
		doc.RootElement.GetProperty("formatVersion").GetInt32().Should().Be(1);
		doc.RootElement.GetProperty("scope").GetString().Should().Be("Tab");

		var data = doc.RootElement.GetProperty("data");
		data.GetProperty("sackNumber").GetInt32().Should().Be(2);

		var items = data.GetProperty("items");
		items.GetArrayLength().Should().Be(2);
		items[0].GetProperty("baseName").GetString().Should().Be("records/gear/armor/helm.dbr");
		items[1].GetProperty("baseName").GetString().Should().Be("records/gear/weapon/sword.dbr");
	}

	[Fact]
	public void ImportFromJson_WithValidTabJson_ShouldReturnMultipleItems()
	{
		var sack = new SackCollection();
		sack.SackType = SackType.Vault;
		sack.AddItem(new Item
		{
			BaseItemId = "records/gear/armor/helm.dbr",
			Seed = 42,
			PositionX = 3,
			PositionY = 7,
			Width = 2,
			Height = 2
		});
		sack.AddItem(new Item
		{
			BaseItemId = "records/gear/weapon/sword.dbr",
			Seed = 99,
			PositionX = 10,
			PositionY = 15,
			Width = 1,
			Height = 3
		});

		var json = _service.SerializeSackCollection(sack, 2);
		var result = _service.ImportFromJson(json);

		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Scope.Should().Be(ExportScope.Tab);
		result.Items.Should().NotBeNull();
		result.Items.Should().HaveCount(2);
		result.ImportedCount.Should().Be(2);
		result.TotalCount.Should().Be(2);
		result.Items[0].BaseItemId.Should().Be("records/gear/armor/helm.dbr");
		result.Items[0].PositionX.Should().Be(3);
		result.Items[0].PositionY.Should().Be(7);
		result.Items[1].BaseItemId.Should().Be("records/gear/weapon/sword.dbr");
		result.Items[1].PositionX.Should().Be(10);
		result.Items[1].PositionY.Should().Be(15);
	}

[Fact]
	public void ImportResult_TabScope_ShouldHaveCorrectCounts()
	{
		var items = new List<Item>
		{
			new Item { BaseItemId = "a.dbr" },
			new Item { BaseItemId = "b.dbr" },
			new Item { BaseItemId = "c.dbr" }
		};

		var result = ImportResult.SucceededTab(items, 0);

		result.Success.Should().BeTrue();
		result.Scope.Should().Be(ExportScope.Tab);
		result.Items.Should().HaveCount(3);
		result.ImportedCount.Should().Be(3);
		result.TotalCount.Should().Be(3);
	}

	[Fact]
	public void ImportFromJson_WithEmptyTab_ShouldReturnNoItems()
	{
		var sack = new SackCollection();
		sack.SackType = SackType.Vault;

		var json = _service.SerializeSackCollection(sack, 0);
		var result = _service.ImportFromJson(json);

		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Scope.Should().Be(ExportScope.Tab);
		result.Items.Should().NotBeNull();
		result.Items.Should().BeEmpty();
	}

	[Fact]
	public void ImportFromJson_WithUnsupportedScope_ShouldReturnFailure()
	{
		var json = """{"formatVersion":1,"scope":"unknown","data":{}}""";

		var result = _service.ImportFromJson(json);

		result.Should().NotBeNull();
		result.Success.Should().BeFalse();
		result.ErrorMessage.Should().Contain("Unsupported scope");
	}

	[Fact]
	public void SerializeItems_MultiSelectScope_ShouldProduceValidExportFormat()
	{
		var items = new List<Item>
		{
			new Item
			{
				BaseItemId = "records/gear/armor/helm.dbr",
				Seed = 1,
				PositionX = 0,
				PositionY = 0,
				Width = 2,
				Height = 2
			},
			new Item
			{
				BaseItemId = "records/gear/weapon/sword.dbr",
				Seed = 2,
				PositionX = 5,
				PositionY = 5,
				Width = 1,
				Height = 3
			}
		};

		var json = _service.SerializeItems(items);

		json.Should().NotBeNullOrEmpty();

		var doc = JsonDocument.Parse(json);
		doc.RootElement.GetProperty("formatVersion").GetInt32().Should().Be(1);
		doc.RootElement.GetProperty("scope").GetString().Should().Be("MultiSelect");

		var data = doc.RootElement.GetProperty("data");
		data.GetArrayLength().Should().Be(2);
		data[0].GetProperty("baseName").GetString().Should().Be("records/gear/armor/helm.dbr");
		data[1].GetProperty("baseName").GetString().Should().Be("records/gear/weapon/sword.dbr");
	}

	[Fact]
	public void ImportFromJson_WithMultiSelectScope_ShouldReturnMultipleItems()
	{
		var items = new List<Item>
		{
			new Item { BaseItemId = "a.dbr", Seed = 1, PositionX = 0, PositionY = 0, Width = 1, Height = 1 },
			new Item { BaseItemId = "b.dbr", Seed = 2, PositionX = 1, PositionY = 0, Width = 1, Height = 1 }
		};

		var json = _service.SerializeItems(items);
		var result = _service.ImportFromJson(json);

		result.Success.Should().BeTrue();
		result.Scope.Should().Be(ExportScope.MultiSelect);
		result.Items.Should().HaveCount(2);
		result.Items[0].BaseItemId.Should().Be("a.dbr");
		result.Items[1].BaseItemId.Should().Be("b.dbr");
		result.ImportedCount.Should().Be(2);
		result.TotalCount.Should().Be(2);
	}

	[Fact]
	public void ImportFromJson_WithEmptyMultiSelect_ShouldReturnFailure()
	{
		var json = """{"formatVersion":1,"scope":"MultiSelect","data":[]}""";

		var result = _service.ImportFromJson(json);

		result.Success.Should().BeFalse();
		result.ErrorMessage.Should().Contain("Failed to deserialize multi-select");
	}

	[Fact]
	public void SerializeItem_WithDualRelic_ShouldPreserveRelicFields()
	{
		var item = new Item
		{
			BaseItemId = "records/gear/armor/test.dbr",
			Seed = 42,
			PositionX = 0,
			PositionY = 0,
			Width = 2,
			Height = 2,
			StackSize = 1,
			relicID = "records/relics/relic1.dbr",
			RelicBonusId = "records/relics/bonus1.dbr",
			Var1 = 0,
			atlantis = true,
			relic2ID = "records/relics/relic2.dbr",
			RelicBonus2Id = "records/relics/bonus2.dbr",
			Var2 = 2035248
		};

		var json = _service.SerializeItem(item);
		var result = _service.ImportFromJson(json);

		result.Success.Should().BeTrue();
		result.Scope.Should().Be(ExportScope.Item);
		result.Item.Should().NotBeNull();
		result.Item.relicID.Should().Be("records/relics/relic1.dbr");
		result.Item.RelicBonusId.Should().Be("records/relics/bonus1.dbr");
		result.Item.relic2ID.Should().Be("records/relics/relic2.dbr");
		result.Item.RelicBonus2Id.Should().Be("records/relics/bonus2.dbr");
		result.Item.Var2.Should().Be(2035248);
	}

	[Fact]
	public void ImportFromJson_WithMissingFormatVersion_ShouldReturnFailure()
	{
		var json = """{"scope":"Item","data":{}}""";

		var result = _service.ImportFromJson(json);

		result.Success.Should().BeFalse();
		result.ErrorMessage.Should().Contain("Invalid or unsupported export format");
	}

	[Fact]
	public void ImportFromJson_WithWrongFormatVersion_ShouldReturnFailure()
	{
		var json = """{"formatVersion":99,"scope":"Item","data":{}}""";

		var result = _service.ImportFromJson(json);

		result.Success.Should().BeFalse();
		result.ErrorMessage.Should().Contain("Invalid or unsupported export format");
	}

	[Fact]
	public void ImportFromJson_WithMissingScope_ShouldReturnFailure()
	{
		var json = """{"formatVersion":1,"data":{}}""";

		var result = _service.ImportFromJson(json);

		result.Success.Should().BeFalse();
		result.ErrorMessage.Should().Contain("Missing scope");
	}

	[Fact]
	public void ImportFromJson_WithMissingData_ShouldReturnFailure()
	{
		var json = """{"formatVersion":1,"scope":"Item"}""";

		var result = _service.ImportFromJson(json);

		result.Success.Should().BeFalse();
		result.ErrorMessage.Should().Contain("Missing data");
	}

	[Fact]
	public void ImportFromJson_WithEmptyString_ShouldReturnFailure()
	{
		var result = _service.ImportFromJson(string.Empty);

		result.Success.Should().BeFalse();
	}

	[Fact]
	public void ImportFromJson_WithNull_ShouldThrowArgumentNullException()
	{
		var act = () => _service.ImportFromJson(null);

		act.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public void ImportFromJson_WithInvalidItemData_ShouldReturnFailure()
	{
		var json = """{"formatVersion":1,"scope":"Item","data":{"baseName":123}}""";

		var result = _service.ImportFromJson(json);

		result.Success.Should().BeFalse();
	}
}