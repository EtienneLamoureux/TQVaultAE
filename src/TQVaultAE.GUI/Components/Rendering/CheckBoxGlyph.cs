namespace TQVaultAE.GUI.Components.Rendering;

using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CheckBoxState = System.Windows.Forms.VisualStyles.CheckBoxState;

/// <summary>
/// Where the system draws a check box's text, after its glyph: the code that draws over that text (a dimmed or outlined
/// text, a caption outline) must start where the system's text starts.
/// </summary>
internal static partial class CheckBoxGlyph
{
	/// <summary>The gap between the glyph and the text, as the system draws it.</summary>
	private const int TextGap = 3;

	/// <summary>
	/// The left of the text of <paramref name="box"/> in its client area: its padding, the glyph at the DPI of its monitor,
	/// and the gap. The renderer gives the theme's glyph at the system DPI: on a monitor of another DPI the system draws
	/// the glyph at that monitor's DPI (13 px at 96 dpi where the renderer said 20 at 144), and a text repainted from the
	/// renderer's width left the left part of the system's text visible, a thin bar before it.
	/// </summary>
	public static int TextLeft(Graphics graphics, CheckBox box, CheckBoxState state)
	{
		int systemDpi = (int)GetDpiForSystem();
		int glyph = CheckBoxRenderer.GetGlyphSize(graphics, state).Width;
		if (systemDpi > 0 && box.DeviceDpi != systemDpi)
			glyph = (int)Math.Round((double)glyph * box.DeviceDpi / systemDpi);

		return box.Padding.Left + glyph + TextGap;
	}

	/// <summary>
	/// Gets the size <see cref="CheckBoxRenderer"/> draws the glyph at: the theme's glyph at the system DPI, whatever the
	/// DPI of the monitor (20 px at a system DPI of 144). The code that draws its own check boxes with the renderer lays
	/// its rows and text out from this size.
	/// </summary>
	public static Size RendererSize => rendererSize ??= MeasureRendererSize();

	private static Size? rendererSize;

	private static Size MeasureRendererSize()
	{
		using var graphics = Graphics.FromHwnd(IntPtr.Zero);
		return CheckBoxRenderer.GetGlyphSize(graphics, CheckBoxState.UncheckedNormal);
	}

	[LibraryImport("user32.dll")]
	private static partial uint GetDpiForSystem();
}
