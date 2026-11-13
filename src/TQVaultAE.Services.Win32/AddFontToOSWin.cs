using System.Drawing.Text;
using System.Runtime.InteropServices;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.Services.Win32;

/// <summary>
/// Windows specific for loading a Font from resource file (hot load from memory)
/// </summary>
public partial class AddFontToOSWin : IAddFontToOS
{
	private static PrivateFontCollection privateFontCollection = new PrivateFontCollection();

	/// <summary>
	/// The AddFontMemResourceEx function adds the font resource from a memory image to the system.
	/// </summary>
	/// <param name="pbFont"></param>
	/// <param name="cbFont"></param>
	/// <param name="pdv"></param>
	/// <param name="pcFonts"></param>
	/// <returns></returns>
	[LibraryImport("gdi32.dll")]
	private static partial IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont, IntPtr pdv, ref uint pcFonts);

	public FontFamily AddFontToOS(string fontName, byte[] fontData)
	{
		uint r = 0;
		unsafe
		{
			fixed (byte* pinptr = fontData)
			{
				IntPtr ptr = (IntPtr)pinptr;
				AddFontMemResourceEx(ptr, (uint)fontData.Length, IntPtr.Zero, ref r);
				privateFontCollection.AddMemoryFont(ptr, fontData.Length);
			}
		}
		return privateFontCollection.Families.First(f => f.Name == fontName);
	}

	/// <inheritdoc/>
	public bool HasEveryGlyph(Font font, string text)
		=> NativeGlyphs.HasEveryGlyph(font, text);
}