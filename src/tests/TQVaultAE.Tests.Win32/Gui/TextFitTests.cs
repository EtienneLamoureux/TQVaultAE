using System.Drawing;
using System.Reflection;
using AwesomeAssertions;

namespace TQVaultAE.Tests.Win32.Gui;

/// <summary>
/// The shrink-to-fit search of the drawn buttons (<c>TextFit</c>): the bag buttons had no lower bound, so a long label on
/// a tiny button went down to a font of size 0, which throws.
/// </summary>
public sealed class TextFitTests
{
	private static readonly Type TextFit = typeof(TQVaultAE.GUI.Program).Assembly
		.GetType("TQVaultAE.GUI.Components.Scaling.TextFit", throwOnError: true)!;

	private static float LargestSize(float start, float step, float min, Func<float, bool> fits)
		=> (float)TextFit.GetMethod("LargestSize", BindingFlags.Public | BindingFlags.Static)!.Invoke(null, [start, step, min, fits])!;

	private static Font Shrink(Font font, float step, float min, Func<Font, bool> fits)
		=> (Font)TextFit.GetMethod("Shrink", BindingFlags.Public | BindingFlags.Static)!.Invoke(null, [font, step, min, fits])!;

	[Fact]
	public void LargestSize_NothingFits_StopsAtTheMinimum()
		=> LargestSize(20F, 1F, 1F, _ => false).Should().Be(1F);

	[Fact]
	public void LargestSize_StepsDownToTheFirstSizeThatFits()
		=> LargestSize(20F, 1F, 1F, s => s <= 12.5F).Should().Be(12F);

	[Fact]
	public void LargestSize_FitsAtStart_KeepsTheStart()
		=> LargestSize(9F, 0.25F, 0.5F, _ => true).Should().Be(9F);

	[Fact]
	public void LargestSize_MinimumNotOnAStep_NeverGoesBelowIt()
		=> LargestSize(10F, 3F, 2F, _ => false).Should().Be(4F);

	[Fact]
	public void Shrink_FontFits_ReturnsTheSameFont()
	{
		using var font = new Font(FontFamily.GenericSansSerif, 12F);

		Shrink(font, 0.25F, 0.5F, _ => true).Should().BeSameAs(font);
	}

	[Fact]
	public void Shrink_TooLarge_ReturnsANewFontAtTheFittingSizeAndDisposesTheTrials()
	{
		using var font = new Font(FontFamily.GenericSansSerif, 12F, FontStyle.Bold);
		var tried = new List<Font>();

		using var fitted = Shrink(font, 0.5F, 0.5F, f =>
		{
			tried.Add(f);
			return f.Size <= 10F;
		});

		fitted.Should().NotBeSameAs(font);
		fitted.Size.Should().Be(10F);
		fitted.Style.Should().Be(FontStyle.Bold);
		fitted.FontFamily.Should().Be(font.FontFamily);
		tried.Where(f => !ReferenceEquals(f, font)).Should().AllSatisfy(f => IsDisposed(f).Should().BeTrue());
	}

	/// <summary>A disposed GDI+ font throws on a read of its native data.</summary>
	private static bool IsDisposed(Font font)
	{
		try
		{
			_ = font.GetHeight();
			return false;
		}
		catch (ArgumentException)
		{
			return true;
		}
	}
}
