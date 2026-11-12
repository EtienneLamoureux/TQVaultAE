using System.Drawing;
using AwesomeAssertions;
using TQVaultAE.Presentation.Win32;

namespace TQVaultAE.Tests.Win32.Presentation;

public class TooltipImageCacheTests
{
	/// <summary>10x10 at 32 bpp.</summary>
	private const long ImageBytes = 400;

	private static Bitmap Image() => new(10, 10);

	private static bool IsDisposed(Bitmap image)
	{
		try
		{
			_ = image.Width;
			return false;
		}
		catch (ArgumentException)
		{
			return true;
		}
	}

	[Fact]
	public void Add_OverBudget_DisposesLeastRecentlyUsedFirst()
	{
		using var cache = new TooltipImageCache<string, object?>(maxBytes: 2 * ImageBytes);
		var a = Image();
		var b = Image();
		var c = Image();
		cache.Add("a", a, null);
		cache.Add("b", b, null);
		cache.TryGetCopy("a", out var copy, out _).Should().BeTrue(); // "a" is now the most recently used
		copy.Dispose();

		cache.Add("c", c, null);

		cache.TryGetCopy("b", out _, out _).Should().BeFalse();
		IsDisposed(b).Should().BeTrue("evicted images are freed");
		IsDisposed(a).Should().BeFalse();
		cache.Bytes.Should().Be(2 * ImageBytes);
	}

	[Fact]
	public void TryGetCopy_ReturnsACopyThatOutlivesEviction()
	{
		// A tooltip may still paint its image after the cache evicted it.
		using var cache = new TooltipImageCache<string, string>(maxBytes: ImageBytes);
		cache.Add("a", Image(), "data");

		cache.TryGetCopy("a", out var shown, out var payload).Should().BeTrue();
		cache.Add("b", Image(), "other");

		payload.Should().Be("data");
		IsDisposed(shown).Should().BeFalse();
		shown.Size.Should().Be(new Size(10, 10));
		shown.Dispose();
	}

	[Fact]
	public void RemoveWhere_DisposesMatchingImages()
	{
		using var cache = new TooltipImageCache<(string Item, float Scale), object?>(maxBytes: 10 * ImageBytes);
		var ring = Image();
		var ringScaled = Image();
		var sword = Image();
		cache.Add(("ring", 1f), ring, null);
		cache.Add(("ring", 1.5f), ringScaled, null);
		cache.Add(("sword", 1f), sword, null);

		cache.RemoveWhere(k => k.Item == "ring").Should().Be(2);

		IsDisposed(ring).Should().BeTrue();
		IsDisposed(ringScaled).Should().BeTrue();
		IsDisposed(sword).Should().BeFalse();
		cache.Count.Should().Be(1);
		cache.Bytes.Should().Be(ImageBytes);
	}

	[Fact]
	public void Add_SameKey_DisposesThePreviousImage()
	{
		using var cache = new TooltipImageCache<string, object?>(maxBytes: 10 * ImageBytes);
		var first = Image();
		cache.Add("a", first, null);

		cache.Add("a", Image(), null);

		IsDisposed(first).Should().BeTrue();
		cache.Bytes.Should().Be(ImageBytes);
	}

	[Fact]
	public void Add_ImageLargerThanBudget_IsNotKept()
	{
		using var cache = new TooltipImageCache<string, object?>(maxBytes: ImageBytes - 1);
		var image = Image();

		cache.Add("a", image, null);

		cache.Count.Should().Be(0);
		IsDisposed(image).Should().BeTrue();
	}
}
