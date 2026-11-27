using System.Drawing;

namespace TQVaultAE.Presentation.Win32.Contracts;

/// <summary>
/// Abstract OS specific needs for loading a Font from resource file (hot load from memory)
/// </summary>
public interface IAddFontToOS
{
	/// <summary>Loads the font <paramref name="fontName"/> from <paramref name="fontData"/> once; null when the data holds no such family.</summary>
	FontFamily? AddFontToOS(string fontName, byte[] fontData);

	/// <summary>Whether <paramref name="font"/> has its own glyph for every character of <paramref name="text"/>, as drawn.</summary>
	bool HasEveryGlyph(Font font, string text);
}