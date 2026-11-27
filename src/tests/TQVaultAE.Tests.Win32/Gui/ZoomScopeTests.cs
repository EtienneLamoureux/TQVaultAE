using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using AwesomeAssertions;

namespace TQVaultAE.Tests.Win32.Gui;

/// <summary>
/// The fonts a zoom gives the scaling controls (<c>ZoomScope.Give</c>): each zoom made a new font per control and dropped
/// the previous one undisposed, about 600 per zoom step in the main window.
/// </summary>
public sealed class ZoomScopeTests
{
	private static readonly Type ZoomScope = typeof(TQVaultAE.GUI.Program).Assembly
		.GetType("TQVaultAE.GUI.Components.Scaling.ZoomScope", throwOnError: true)!;

	private static IDisposable Enter()
		=> (IDisposable)ZoomScope.GetMethod("Enter", BindingFlags.Public | BindingFlags.Static)!.Invoke(null, null)!;

	private static Font Give(Control owner, Font font)
		=> (Font)ZoomScope.GetMethod("Give", BindingFlags.Public | BindingFlags.Static)!.Invoke(null, [owner, font])!;

	[Fact]
	public void Give_ReplacedFontNoLongerShown_IsDisposedWhenTheZoomEnds()
	{
		using var label = new Label();
		var first = new Font(FontFamily.GenericSansSerif, 9F);
		using var second = new Font(FontFamily.GenericSansSerif, 10F);

		using (Enter())
			label.Font = Give(label, first);

		using (Enter())
		{
			label.Font = Give(label, second);
			IsDisposed(first).Should().BeFalse("the zoom has not ended yet");
		}

		IsDisposed(first).Should().BeTrue();
		IsDisposed(second).Should().BeFalse();
	}

	[Fact]
	public void Give_ReplacedFontStillShown_IsKept()
	{
		using var label = new Label();
		using var first = new Font(FontFamily.GenericSansSerif, 9F);
		using var second = new Font(FontFamily.GenericSansSerif, 10F);

		using (Enter())
		{
			label.Font = Give(label, first);
			Give(label, second);
		}

		IsDisposed(first).Should().BeFalse("the label still shows it");
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
