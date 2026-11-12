using System.Drawing;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Config;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Presentation.Win32;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.Tests.Win32.Presentation;

/// <summary>
/// Tests for <see cref="UIService.GetBitmap"/> when an item's record is found but its picture cannot be loaded: the item
/// keeps the size read at load (texture header, else one cell), shows a warning tile of that size, and the failure is
/// logged once per picture.
/// </summary>
public class UIServiceItemPictureTests
{
	private const string Picture = @"XPack2\items\equipmentweapon\bow\uibitmaps\mi_skeletonbow.tex";
	private const string Record = @"records\xpack2\item\equipmentweapons\bow\mi_n_draugr.dbr";

	private readonly Mock<IDatabase> _database = new();
	private readonly Mock<IBitmapService> _bitmaps = new();
	private readonly ListLogger _log = new();

	private UIService NewService() => new(this._log, this._database.Object, this._bitmaps.Object, new UserSettings());

	/// <summary>An item as the load leaves it: record found, size from the texture header (or 0 x 0 if never loaded).</summary>
	private static Item FoundItem(int width, int height, string picture = Picture) => new()
	{
		BaseItemId = Record,
		baseItemInfo = new Info(new DBRecordCollection(Record, "WeaponHunting_Bow")),
		TexImageResourceId = picture,
		Width = width,
		Height = height,
	};

	private int Warnings => this._log.Entries.Count(e => e.Level == LogLevel.Warning);

	[Fact]
	public void MissingPicture_KeepsTheSizeReadAtLoad_AndShowsAPlaceholderOfThatSize()
	{
		var service = this.NewService();
		var item = FoundItem(1, 4);

		var bitmap = service.GetBitmap(item);

		bitmap.Should().BeSameAs(UIService.MissingItemBitmap(1, 4));
		bitmap!.Size.Should().Be(new Size(32, 128));
		(item.Width, item.Height).Should().Be((1, 4));
	}

	[Fact]
	public void UndecodablePicture_ShowsAPlaceholder_AndIsDecodedOnce()
	{
		this._database.Setup(d => d.LoadResource(It.Is<RecordId>(id => id == Picture))).Returns(new byte[64]);
		this._bitmaps.Setup(b => b.LoadFromTexMemory(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
			.Throws(new InvalidDataException("TEX texture length is invalid."));
		var service = this.NewService();
		var item = FoundItem(1, 4);

		var first = service.GetBitmap(item);
		var second = service.GetBitmap(item);

		first.Should().BeSameAs(UIService.MissingItemBitmap(1, 4));
		second.Should().BeSameAs(first);
		(item.Width, item.Height).Should().Be((1, 4));
		this._bitmaps.Verify(b => b.LoadFromTexMemory(It.Is<byte[]>(data => data.Length == 64), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
	}

	[Fact]
	public void PictureTheDecoderRejects_ShowsAPlaceholder()
	{
		this._database.Setup(d => d.LoadResource(It.Is<RecordId>(id => id == Picture))).Returns(new byte[64]);
		this._bitmaps.Setup(b => b.LoadFromTexMemory(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns((Bitmap?)null);
		var service = this.NewService();
		var item = FoundItem(2, 3);

		service.GetBitmap(item).Should().BeSameAs(UIService.MissingItemBitmap(2, 3));
		(item.Width, item.Height).Should().Be((2, 3));
	}

	[Fact]
	public void MissingPicture_WithoutAnyKnownSize_IsOneCell()
	{
		var service = this.NewService();
		var item = FoundItem(0, 0);

		var bitmap = service.GetBitmap(item);

		bitmap!.Size.Should().Be(new Size(32, 32));
		(item.Width, item.Height).Should().Be((1, 1));
	}

	[Fact]
	public void MissingPicture_IsLoggedOncePerPicture_NotAtEveryDraw()
	{
		var service = this.NewService();
		var first = FoundItem(1, 4);
		var sameBow = FoundItem(1, 4);
		var other = FoundItem(1, 1, @"Items\Other\missing.tex");

		for (int i = 0; i < 3; i++)
		{
			service.GetBitmap(first);
			service.GetBitmap(sameBow);
		}
		this.Warnings.Should().Be(1);
		this._log.Entries.Single(e => e.Level == LogLevel.Warning).Message.Should().Contain(Picture).And.Contain(Record).And.Contain("1x4");

		service.GetBitmap(other);
		this.Warnings.Should().Be(2);
	}

	[Fact]
	public void MissingRecord_ShowsTheOneCellPlaceholder_WithoutPictureWarning()
	{
		var service = this.NewService();
		var item = new Item { BaseItemId = @"records\mod\item\unknown.dbr", TexImageResourceId = "DefaultBitmap", Width = 1, Height = 1 };

		service.GetBitmap(item).Should().BeSameAs(UIService.MissingItemBitmap(1, 1));
		this.Warnings.Should().Be(0);
	}

	[Fact]
	public void ItemWithoutRecordId_HasNoPicture()
	{
		var service = this.NewService();

		service.GetBitmap(new Item()).Should().BeNull();
	}

	[Fact]
	public void LoadedPicture_IsShownAndGivesTheSize()
	{
		using var picture = new Bitmap(32, 128);
		this._database.Setup(d => d.LoadResource(It.Is<RecordId>(id => id == Picture))).Returns(new byte[64]);
		this._bitmaps.Setup(b => b.LoadFromTexMemory(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(picture);
		var service = this.NewService();
		var item = FoundItem(0, 0);

		service.GetBitmap(item).Should().BeSameAs(picture);
		(item.Width, item.Height).Should().Be((1, 4));
		this.Warnings.Should().Be(0);
	}

	private sealed class ListLogger : ILogger<UIService>
	{
		public List<(LogLevel Level, string Message)> Entries { get; } = [];

		public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

		public bool IsEnabled(LogLevel logLevel) => true;

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
			=> this.Entries.Add((logLevel, formatter(state, exception)));
	}
}
