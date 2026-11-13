using System.Drawing;

namespace TQVaultAE.Presentation.Win32.Contracts;

/// <summary>
/// Abstract OS specific needs for loading a Font from resource file (hot load from memory)
/// </summary>
public interface IAddFontToOS
{
	FontFamily AddFontToOS(string fontName, byte[] fontData);

	/// <summary>Whether <paramref name="font"/> has its own glyph for every character of <paramref name="text"/>, as drawn.</summary>
	bool HasEveryGlyph(Font font, string text);
}