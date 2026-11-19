using System.Drawing;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.Presentation.Win32;

/// <summary>
/// Windows specific for loading a Font from resource file (hot load from memory)
/// </summary>
public partial class AddFontToOSWin : IAddFontToOS
{
	private static readonly PrivateFontCollection privateFontCollection = new PrivateFontCollection();
	private static readonly Lock collectionLock = new();

	/// <summary>
	/// The AddFontMemResourceEx function adds the font resource from a memory image to the system (it keeps its own copy).
	/// </summary>
	[LibraryImport("gdi32.dll")]
	private static partial IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont, IntPtr pdv, ref uint pcFonts);

	/// <inheritdoc/>
	public FontFamily? AddFontToOS(string fontName, byte[] fontData)
	{
		lock (collectionLock)
		{
			// Loaded once: each settings reload asks again.
			FontFamily? family = Find(fontName);
			if (family is not null)
				return family;

			// GDI+ reads the font from this memory for as long as the collection lives (the whole process),
			// so it is unmanaged memory that is never freed, not a pinned array.
			IntPtr memory = Marshal.AllocCoTaskMem(fontData.Length);
			Marshal.Copy(fontData, 0, memory, fontData.Length);
			uint fonts = 0;
			AddFontMemResourceEx(memory, (uint)fontData.Length, IntPtr.Zero, ref fonts);
			privateFontCollection.AddMemoryFont(memory, fontData.Length);
			return Find(fontName);
		}
	}

	private static FontFamily? Find(string fontName)
		=> privateFontCollection.Families.FirstOrDefault(f => f.Name == fontName);

	/// <inheritdoc/>
	public bool HasEveryGlyph(Font font, string text)
		=> NativeGlyphs.HasEveryGlyph(font, text);
}
