using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

/// <summary>
/// Import flows of <see cref="ItemExchangeService"/>: text dispatch, files, placement in a sack and vault creation.
/// </summary>
public class ItemExchangeServiceImportTests
{
	private const string ItemJson = """{"formatVersion":1,"scope":"Item","data":{"baseName":"records/gear/ring.dbr","pointX":1,"pointY":2}}""";

	private readonly JsonSerializerOptions _jsonOptions = new()
	{
		IncludeFields = true,
		PropertyNameCaseInsensitive = true,
		WriteIndented = true
	};

	private readonly Mock<IPasteBinService> _pasteBin = new();
	private readonly Mock<IFileIO> _fileIO = new();
	private readonly Mock<IVaultService> _vaultService = new();
	private readonly ItemExchangeService _service;

	public ItemExchangeServiceImportTests()
	{
		var movement = new ItemMovementService(new Mock<ILogger<ItemMovementService>>().Object);
		this._service = new ItemExchangeService(
			this._jsonOptions
			, this._pasteBin.Object
			, itemMovementService: movement
			, fileIO: this._fileIO.Object
			, vaultService: this._vaultService.Object);
	}

	private static Item NewItem(int x, int y, int width = 1, int height = 1)
		=> new() { BaseItemId = "records/gear/item.dbr", PositionX = x, PositionY = y, Width = width, Height = height };

	private static SackCollection NewSack(params Item[] items)
	{
		var sack = new SackCollection();
		foreach (var item in items)
			sack.AddItem(item);
		sack.IsModified = false;
		return sack;
	}

	[Fact]
	public async Task ImportFromTextAsync_WithPasteBinUrl_ImportsTheFetchedJson()
	{
		this._pasteBin.Setup(p => p.FetchPasteAsync("https://pastebin.com/abc")).ReturnsAsync(ItemJson);

		var result = await this._service.ImportFromTextAsync("  https://pastebin.com/abc \r\n");

		result.Success.Should().BeTrue();
		result.Scope.Should().Be(ExportScope.Item);
		result.Item!.BaseItemId.Raw.Should().Be("records/gear/ring.dbr");
	}

	[Fact]
	public async Task ImportFromTextAsync_WhenPasteBinFails_ReturnsFailureWithTheReason()
	{
		this._pasteBin.Setup(p => p.FetchPasteAsync(It.IsAny<string>())).ThrowsAsync(new HttpRequestException("404"));

		var result = await this._service.ImportFromTextAsync("https://pastebin.com/gone");

		result.Success.Should().BeFalse();
		result.ErrorMessage.Should().Be("Failed to import from PasteBin: 404");
	}

	[Fact]
	public async Task ImportFromTextAsync_WithJson_DoesNotCallPasteBin()
	{
		var result = await this._service.ImportFromTextAsync(ItemJson);

		result.Success.Should().BeTrue();
		this._pasteBin.Verify(p => p.FetchPasteAsync(It.IsAny<string>()), Times.Never);
	}

	[Fact]
	public void ImportFromFile_ReadsTheFileThroughFileIO()
	{
		var path = Path.Combine(Path.GetTempPath(), "export.json");
		this._fileIO.Setup(f => f.ReadAllText(path)).Returns(ItemJson);

		var result = this._service.ImportFromFile(path);

		result.Success.Should().BeTrue();
		result.Item!.PositionX.Should().Be(1);
	}

	[Fact]
	public void ExportVaultToFile_WritesTheVaultExportJson()
	{
		var vault = new PlayerCollection("MyVault", "my.vault") { IsVault = true };
		vault.CreateEmptySacks(1);
		var path = Path.Combine(Path.GetTempPath(), "MyVault.json");
		string? written = null;
		this._fileIO.Setup(f => f.WriteAllText(path, It.IsAny<string>())).Callback<string, string>((_, text) => written = text);

		this._service.ExportVaultToFile(vault, path);

		written.Should().NotBeNull();
		var import = this._service.ImportFromJson(written!);
		import.Scope.Should().Be(ExportScope.Vault);
		import.VaultName.Should().Be("MyVault");
	}

	[Fact]
	public void PlaceImportedItems_KeepsTheExportedPositionWhenFree()
	{
		var sack = NewSack();
		var item = NewItem(3, 4, 2, 2);

		var placed = this._service.PlaceImportedItems(sack, ImportResult.Succeeded(item), 10, 10);

		placed.Should().Be(1);
		item.Location.Should().Be(new System.Drawing.Point(3, 4));
		sack.Should().Contain(item);
		sack.IsModified.Should().BeTrue();
	}

	[Fact]
	public void PlaceImportedItems_MovesToOpenCellsWhenThePositionIsTaken()
	{
		var sack = NewSack(NewItem(0, 0));
		var item = NewItem(0, 0);

		this._service.PlaceImportedItems(sack, ImportResult.Succeeded(item), 4, 4);

		item.Location.Should().Be(new System.Drawing.Point(0, 1));
	}

	[Fact]
	public void PlaceImportedItems_MovesAnItemThatWouldStickOutOfTheSack()
	{
		var sack = NewSack();
		var item = NewItem(3, 3, 2, 2);

		this._service.PlaceImportedItems(sack, ImportResult.Succeeded(item), 4, 4);

		item.Location.Should().Be(new System.Drawing.Point(0, 0));
	}

	[Fact]
	public void PlaceImportedItems_ClampsNegativePositions()
	{
		var sack = NewSack();
		var item = NewItem(-2, -5);

		this._service.PlaceImportedItems(sack, ImportResult.Succeeded(item), 4, 4);

		item.Location.Should().Be(new System.Drawing.Point(0, 0));
	}

	[Fact]
	public void PlaceImportedItems_SkipsItemsThatDoNotFitAndCountsThePlacedOnes()
	{
		var sack = NewSack(NewItem(0, 0, 2, 1));
		var fits = NewItem(0, 1, 2, 1);
		var tooBig = NewItem(0, 0, 2, 2);

		var placed = this._service.PlaceImportedItems(sack, ImportResult.SucceededMultiSelect([fits, tooBig]), 2, 2);

		placed.Should().Be(1);
		sack.Should().Contain(fits);
		sack.Should().NotContain(tooBig);
	}

	[Fact]
	public void PlaceImportedItems_LaterItemsSeeTheEarlierOnes()
	{
		var sack = NewSack();
		var first = NewItem(0, 0);
		var second = NewItem(0, 0);

		this._service.PlaceImportedItems(sack, ImportResult.SucceededMultiSelect([first, second]), 2, 2);

		second.Location.Should().NotBe(first.Location);
	}

	[Fact]
	public void PlaceImportedItems_TabImportGivesItsIconToTheSack()
	{
		var sack = NewSack();
		var icon = new BagButtonIconInfo { Label = "Rings" };

		this._service.PlaceImportedItems(sack, ImportResult.SucceededTab([NewItem(0, 0)], 2, icon), 4, 4);

		sack.BagButtonIconInfo.Should().BeSameAs(icon);
	}

	[Fact]
	public void PlaceImportedItems_MultiSelectImportKeepsTheSackIcon()
	{
		var icon = new BagButtonIconInfo { Label = "Mine" };
		var sack = NewSack();
		sack.BagButtonIconInfo = icon;

		this._service.PlaceImportedItems(sack, ImportResult.SucceededMultiSelect([NewItem(0, 0)]), 4, 4);

		sack.BagButtonIconInfo.Should().BeSameAs(icon);
	}

	[Fact]
	public void IsVaultEmpty_IsFalseWhenAnySackHoldsAnItem()
	{
		var vault = new PlayerCollection("V", "v.vault") { IsVault = true };
		vault.CreateEmptySacks(3);

		this._service.IsVaultEmpty(vault).Should().BeTrue();

		vault.GetSack(2)!.AddItem(NewItem(0, 0));

		this._service.IsVaultEmpty(vault).Should().BeFalse();
	}

	[Fact]
	public void BuildImportedVaultName_AppendsASortableTimestamp()
	{
		var name = this._service.BuildImportedVaultName("Rings", new DateTime(2026, 3, 4, 5, 6, 7));

		name.Should().Be("Rings_20260304_050607");
	}

	[Fact]
	public void ImportAsNewVault_CreatesFillsAndSavesTheVault()
	{
		var created = new PlayerCollection("Rings_20260304_050607", "x.vault") { IsVault = true };
		created.CreateEmptySacks(2);
		this._vaultService.Setup(v => v.CreateVault("Rings_20260304_050607")).Returns(created);
		var sackItems = new Dictionary<int, List<Item>> { [1] = [NewItem(0, 0)] };

		var name = this._service.ImportAsNewVault(ImportResult.SucceededVault("Rings", sackItems), new DateTime(2026, 3, 4, 5, 6, 7));

		name.Should().Be("Rings_20260304_050607");
		created.GetSack(1)!.Count.Should().Be(1);
		created.GetSack(0)!.IsModified.Should().BeTrue();
		this._vaultService.Verify(v => v.SaveVault(created, "Rings_20260304_050607"), Times.Once);
	}

	[Theory]
	[InlineData("Ring of Ages", "Ring of Ages")]
	[InlineData("", null)]
	public async Task ExportItemToPasteBinAsync_NamesThePasteAfterTheItem(string itemName, string? expectedPasteName)
	{
		var item = NewItem(0, 0);
		var itemProvider = new Mock<TQVaultAE.Application.Contracts.Providers.IItemProvider>();
		itemProvider.Setup(p => p.GetFriendlyNames(item, null, true))
			.Returns(new TQVaultAE.Domain.Results.ToFriendlyNameResult(item) { BaseItemInfoDescription = itemName });
		this._pasteBin.Setup(p => p.UploadAsync(It.IsAny<string>(), It.IsAny<string?>())).ReturnsAsync("https://pastebin.com/x");
		var service = new ItemExchangeService(this._jsonOptions, this._pasteBin.Object, itemProvider: itemProvider.Object);

		var url = await service.ExportItemToPasteBinAsync(item);

		url.Should().Be("https://pastebin.com/x");
		this._pasteBin.Verify(p => p.UploadAsync(service.SerializeItem(item), expectedPasteName), Times.Once);
	}
}
