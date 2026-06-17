using System.Drawing;

namespace TQVaultAE.Application.Contracts.Services;

/// <summary>
/// Abstract OS specific needs for loading a Font from resource file (hot load from memory)
/// </summary>
public interface IAddFontToOS
{
	FontFamily AddFontToOS(string fontName, byte[] fontData);
}