using System.Drawing;

namespace TQVaultAE.Presentation.Win32.Contracts;

/// <summary>
/// Game fonts. The returned fonts are cached and shared: do not dispose them, copy one (<c>new Font(font, style)</c>) to own it.
/// </summary>
public interface IFontService
{
	IAddFontToOS FontLoader { get; }

	Font GetFont(float fontSize, float? scale = null);
	Font GetFont(float fontSize, FontStyle fontStyle, float? scale = null);
	Font GetFont(float fontSize, FontStyle fontStyle, GraphicsUnit unit);
	Font GetFont(float fontSize, FontStyle fontStyle, GraphicsUnit unit, byte b);
	Font GetFont(float fontSize, GraphicsUnit unit);
	Font GetFontLight(float fontSize);
	Font GetFontLight(float fontSize, float? scale = null);
	Font GetFontLight(float fontSize, FontStyle fontStyle, float? scale = null);
	Font GetFontLight(float fontSize, FontStyle fontStyle, GraphicsUnit unit);
	Font GetFontLight(float fontSize, FontStyle fontStyle, GraphicsUnit unit, byte b);
	Font GetFontLight(float fontSize, GraphicsUnit unit);

	/// <summary>The shared copy of <paramref name="font"/> in <paramref name="style"/>, for example its bold version.</summary>
	Font WithStyle(Font font, FontStyle style);

	/// <summary>
	/// Raised by <see cref="Reload"/>: the windows move their fonts to the new family with <see cref="Rebase"/>.
	/// </summary>
	event EventHandler? FontChanged;

	/// <summary>
	/// Reads the base font setting again and makes the next fonts in its family, then raises <see cref="FontChanged"/>.
	/// The fonts given before are dropped from the cache but not disposed: controls may still show them.
	/// </summary>
	void Reload();

	/// <summary>
	/// The font in the base font in use with the size, style and unit of <paramref name="font"/>, if <paramref name="font"/>
	/// is one of this service's fonts in a family used before <see cref="Reload"/> (also grown by a zoom);
	/// <paramref name="font"/> itself otherwise.
	/// </summary>
	Font Rebase(Font font);
}