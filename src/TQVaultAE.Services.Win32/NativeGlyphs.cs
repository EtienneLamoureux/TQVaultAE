using System.Drawing;
using System.Runtime.InteropServices;

namespace TQVaultAE.Services.Win32;

/// <summary>Reads which characters a font has a glyph for, as GDI draws it (a font added from memory included).</summary>
internal static partial class NativeGlyphs
{
	/// <summary>Missing glyphs are marked 0xFFFF rather than given the default glyph.</summary>
	private const uint GGI_MARK_NONEXISTING_GLYPHS = 1;
	private const uint GDI_ERROR = 0xFFFFFFFF;

	/// <summary>Whether <paramref name="font"/> has its own glyph for every character of <paramref name="text"/>.</summary>
	public static bool HasEveryGlyph(Font font, string text)
	{
		if (text.Length == 0)
			return true;

		IntPtr hdc = CreateCompatibleDC(IntPtr.Zero);
		IntPtr hfont = font.ToHfont();
		try
		{
			IntPtr previous = SelectObject(hdc, hfont);
			try
			{
				var glyphs = new ushort[text.Length];
				if (GetGlyphIndicesW(hdc, text, text.Length, glyphs, GGI_MARK_NONEXISTING_GLYPHS) == GDI_ERROR)
					return true; // Unknown: keep the font.
				return !glyphs.Contains((ushort)0xFFFF);
			}
			finally
			{
				SelectObject(hdc, previous);
			}
		}
		finally
		{
			DeleteObject(hfont);
			DeleteDC(hdc);
		}
	}

	[LibraryImport("gdi32.dll")]
	private static partial IntPtr CreateCompatibleDC(IntPtr hdc);

	[LibraryImport("gdi32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool DeleteDC(IntPtr hdc);

	[LibraryImport("gdi32.dll")]
	private static partial IntPtr SelectObject(IntPtr hdc, IntPtr obj);

	[LibraryImport("gdi32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool DeleteObject(IntPtr obj);

	[LibraryImport("gdi32.dll", StringMarshalling = StringMarshalling.Utf16)]
	private static partial uint GetGlyphIndicesW(IntPtr hdc, string text, int count, [Out] ushort[] glyphs, uint flags);
}
