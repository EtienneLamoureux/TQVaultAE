using System.Drawing;
using AwesomeAssertions;
using Moq;
using TQVaultAE.Config;
using TQVaultAE.Presentation.Win32;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.Tests.Win32.Presentation;

public class FontServiceTests
{
	// Arial: a system font, so no game font has to be installed.
	private readonly UserSettings _settings = new() { BaseFont = "Arial" };
	private readonly FontService _service;

	public FontServiceTests() => this._service = new(Mock.Of<IAddFontToOS>(), this._settings);

	[Fact]
	public void GetFont_SameArguments_ReturnsTheSharedInstance()
	{
		var first = this._service.GetFont(10F, FontStyle.Bold, 1.5F);

		this._service.GetFont(15F, FontStyle.Bold).Should().BeSameAs(first, "size times scale is the same font");
	}

	[Fact]
	public void GetFont_DifferentStyleUnitOrFamily_ReturnsDistinctFonts()
	{
		var regular = this._service.GetFont(10F, FontStyle.Regular, GraphicsUnit.Pixel);

		this._service.GetFont(10F, FontStyle.Bold, GraphicsUnit.Pixel).Should().NotBeSameAs(regular);
		this._service.GetFont(10F, FontStyle.Regular, GraphicsUnit.Point).Should().NotBeSameAs(regular);
		this._service.GetFontLight(10F, FontStyle.Regular, GraphicsUnit.Pixel).Should().NotBeSameAs(regular);
	}

	[Fact]
	public void GetFont_KeepsTheRequestedSizeStyleAndUnit()
	{
		var font = this._service.GetFontLight(12F, FontStyle.Italic, GraphicsUnit.Pixel);

		font.Size.Should().Be(12F);
		font.Style.Should().Be(FontStyle.Italic);
		font.Unit.Should().Be(GraphicsUnit.Pixel);
		font.FontFamily.Name.Should().Be("Arial");
	}

	[Fact]
	public void WithStyle_ReturnsOneSharedCopyInTheNewStyle_AndTheFontItselfForItsOwnStyle()
	{
		var regular = this._service.GetFontLight(20F, FontStyle.Regular, GraphicsUnit.Pixel);

		var bold = this._service.WithStyle(regular, FontStyle.Bold);

		bold.Style.Should().Be(FontStyle.Bold);
		bold.Size.Should().Be(20F);
		bold.Unit.Should().Be(GraphicsUnit.Pixel);
		this._service.WithStyle(regular, FontStyle.Bold).Should().BeSameAs(bold);
		this._service.WithStyle(regular, FontStyle.Regular).Should().BeSameAs(regular);
	}
	[Fact]
	public void Reload_AfterABaseFontChange_MakesTheNextFontsInTheNewFamily()
	{
		this._service.GetFont(10F).FontFamily.Name.Should().Be("Arial");

		this._settings.BaseFont = "Verdana";
		this._service.Reload();

		this._service.GetFont(10F).FontFamily.Name.Should().Be("Verdana");
		this._service.GetFontLight(10F).FontFamily.Name.Should().Be("Verdana");
	}

	[Fact]
	public void Reload_RaisesFontChangedOnce_WithTheNewFamilyAlreadyInUse()
	{
		string? familyInHandler = null;
		int raised = 0;
		this._service.FontChanged += (_, _) =>
		{
			raised++;
			familyInHandler = this._service.GetFont(10F).FontFamily.Name;
		};

		this._settings.BaseFont = "TimesNewRoman";
		this._service.Reload();

		raised.Should().Be(1);
		familyInHandler.Should().Be("Times New Roman");
	}

	[Fact]
	public void Reload_DoesNotDisposeTheFontsGivenBefore()
	{
		var before = this._service.GetFont(10F, FontStyle.Bold);
		var styled = this._service.WithStyle(before, FontStyle.Italic);

		this._settings.BaseFont = "Verdana";
		this._service.Reload();

		// A disposed font throws on any GDI+ call: a control may still show these until it takes the new ones.
		before.Invoking(f => f.GetHeight()).Should().NotThrow();
		styled.Invoking(f => f.GetHeight()).Should().NotThrow();
		this._service.GetFont(10F, FontStyle.Bold).Should().NotBeSameAs(before);
	}

	[Fact]
	public void Rebase_AFontOfTheFormerFamily_GivesTheSharedFontInTheNewOne_WithItsSizeStyleUnitAndWeight()
	{
		var regular = this._service.GetFont(11F, FontStyle.Bold, GraphicsUnit.Pixel);
		var light = this._service.GetFontLight(12F, FontStyle.Italic);

		this._settings.BaseFont = "Verdana";
		this._service.Reload();

		var rebased = this._service.Rebase(regular);
		rebased.FontFamily.Name.Should().Be("Verdana");
		rebased.Size.Should().Be(11F);
		rebased.Style.Should().Be(FontStyle.Bold);
		rebased.Unit.Should().Be(GraphicsUnit.Pixel);
		rebased.Should().BeSameAs(this._service.GetFont(11F, FontStyle.Bold, GraphicsUnit.Pixel));

		// Light and regular share the family name here: the service remembers which one it gave.
		this._service.Rebase(light).Should().BeSameAs(this._service.GetFontLight(12F, FontStyle.Italic));
	}

	[Fact]
	public void Rebase_AZoomedCopyOfAFormerFont_FollowsTheNewFamily_AndOtherFontsStay()
	{
		using var zoomed = new Font("Arial", 18F, FontStyle.Regular);
		using var unrelated = new Font("Courier New", 10F);
		this._service.GetFont(10F); // Arial in use

		this._settings.BaseFont = "Verdana";
		this._service.Reload();

		var rebased = this._service.Rebase(zoomed);
		rebased.FontFamily.Name.Should().Be("Verdana");
		rebased.Size.Should().Be(18F);
		this._service.Rebase(unrelated).Should().BeSameAs(unrelated);
		this._service.Rebase(rebased).Should().BeSameAs(rebased, "it is already in the base font");
	}

	[Fact]
	public void Rebase_BeforeAnyReload_KeepsTheFont()
	{
		using var arial = new Font("Arial", 10F);

		this._service.Rebase(arial).Should().BeSameAs(arial);
	}

	[Fact]
	public void Reload_BackToTheGameFont_AddsItToTheProcessOnce()
	{
		var loader = new Mock<IAddFontToOS>();
		var settings = new UserSettings { BaseFont = "AlbertusMT" };
		var service = new FontService(loader.Object, settings);
		service.GetFont(10F);
		service.GetFontLight(10F);

		settings.BaseFont = "Arial";
		service.Reload();
		settings.BaseFont = "AlbertusMT";
		service.Reload();
		service.GetFont(10F);
		service.GetFontLight(10F);

		loader.Verify(l => l.AddFontToOS("Albertus MT", It.IsAny<byte[]>()), Times.Once);
		loader.Verify(l => l.AddFontToOS("Albertus MT Light", It.IsAny<byte[]>()), Times.Once);
	}
}