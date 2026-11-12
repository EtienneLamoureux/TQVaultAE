using AwesomeAssertions;
using Moq;
using System.Text.Json;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Results;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

/// <summary>
/// Export flows of <see cref="ItemExchangeService"/>: the single item vs selection choice, files and PasteBin.
/// </summary>
public class ItemExchangeServiceExportTests
{
	private const string PasteUrl = "https://pastebin.com/x";

	private readonly JsonSerializerOptions _jsonOptions = new()
	{
		IncludeFields = true,
		PropertyNameCaseInsensitive = true,
		WriteIndented = true
	};

	private readonly Mock<IPasteBinService> _pasteBin = new();
	private readonly Mock<IFileIO> _fileIO = new();
	private readonly Mock<IItemProvider> _itemProvider = new();
	private readonly ItemExchangeService _service;

	public ItemExchangeServiceExportTests()
	{
		this._pasteBin.Setup(p => p.UploadAsync(It.IsAny<string>(), It.IsAny<string?>())).ReturnsAsync(PasteUrl);
		this._service = new ItemExchangeService(
			this._jsonOptions
			, this._pasteBin.Object
			, itemProvider: this._itemProvider.Object
			, fileIO: this._fileIO.Object);
	}

	private static Item NewItem(int x, int y, string baseItemId = "records/gear/item.dbr")
		=> new() { BaseItemId = baseItemId, PositionX = x, PositionY = y, Width = 1, Height = 1 };

	[Fact]
	public void SerializeSelection_WithNoItem_ReturnsNull()
	{
		this._service.SerializeSelection([]).Should().BeNull();
	}

	[Fact]
	public void SerializeSelection_WithOneItem_ExportsAnItem()
	{
		var item = NewItem(3, 4, "records/gear/ring.dbr");

		var json = this._service.SerializeSelection([item]);

		json.Should().Be(this._service.SerializeItem(item));
		var import = this._service.ImportFromJson(json!);
		import.Scope.Should().Be(ExportScope.Item);
		import.Item!.BaseItemId.Should().Be("records/gear/ring.dbr");
	}

	[Fact]
	public void SerializeSelection_WithSeveralItems_ExportsAMultiSelectInTheirOrder()
	{
		Item[] items = [NewItem(0, 0, "records/gear/a.dbr"), NewItem(1, 0, "records/gear/b.dbr")];

		var json = this._service.SerializeSelection(items);

		json.Should().Be(this._service.SerializeItems(items));
		var import = this._service.ImportFromJson(json!);
		import.Scope.Should().Be(ExportScope.MultiSelect);
		import.Items.Select(i => i.BaseItemId).Should().Equal("records/gear/a.dbr", "records/gear/b.dbr");
	}

	[Fact]
	public void ExportToFile_WritesTheJsonUnchangedThroughFileIO()
	{
		var path = Path.Combine(Path.GetTempPath(), "item_export.json");
		var json = this._service.SerializeItem(NewItem(0, 0));

		this._service.ExportToFile(json, path);

		this._fileIO.Verify(f => f.WriteAllText(path, json), Times.Once);
	}

	[Fact]
	public async Task ExportSelectionToPasteBinAsync_WithSeveralItems_UploadsAMultiSelectNamedMultiItemExport()
	{
		Item[] items = [NewItem(0, 0), NewItem(1, 0)];

		var url = await this._service.ExportSelectionToPasteBinAsync(items);

		url.Should().Be(PasteUrl);
		this._pasteBin.Verify(p => p.UploadAsync(this._service.SerializeItems(items), "Multi-item export"), Times.Once);
	}

	[Fact]
	public async Task ExportSelectionToPasteBinAsync_WithOneItem_UploadsTheItemNamedAfterIt()
	{
		var item = NewItem(0, 0);
		this._itemProvider.Setup(p => p.GetFriendlyNames(item, null, true))
			.Returns(new ToFriendlyNameResult(item) { BaseItemInfoDescription = "Ring of Ages" });

		var url = await this._service.ExportSelectionToPasteBinAsync([item]);

		url.Should().Be(PasteUrl);
		this._pasteBin.Verify(p => p.UploadAsync(this._service.SerializeItem(item), "Ring of Ages"), Times.Once);
	}

	[Theory]
	[InlineData("Rings", "Main Vault/Rings")]
	[InlineData("", "Main Vault/Tab 3")]
	[InlineData("  ", "Main Vault/Tab 3")]
	public void BuildTabPasteName_UsesTheTabLabelElseTheOneBasedTabNumber(string label, string expected)
	{
		var sack = new SackCollection { BagButtonIconInfo = new BagButtonIconInfo { Label = label } };

		this._service.BuildTabPasteName("Main Vault", sack, 2).Should().Be(expected);
	}

	[Fact]
	public void BuildTabPasteName_WithoutIconInfo_UsesTheTabNumber()
	{
		this._service.BuildTabPasteName("Main Vault", new SackCollection(), 0).Should().Be("Main Vault/Tab 1");
	}

	[Fact]
	public async Task ExportTabToPasteBinAsync_UploadsTheTabExportUnderTheTabName()
	{
		var sack = new SackCollection { BagButtonIconInfo = new BagButtonIconInfo { Label = "Rings" } };
		sack.AddItem(NewItem(2, 1));

		var url = await this._service.ExportTabToPasteBinAsync("Main Vault", sack, 4);

		url.Should().Be(PasteUrl);
		this._pasteBin.Verify(p => p.UploadAsync(this._service.SerializeSackCollection(sack, 4), "Main Vault/Rings"), Times.Once);
	}
}
