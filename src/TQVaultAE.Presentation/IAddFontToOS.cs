using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace TQVaultAE.Presentation
{
	/// <summary>
	/// Abstract OS specific needs for loading a Font from resource file (hot load from memory)
	/// </summary>
	public interface IAddFontToOS
	{
		FontFamily AddFontToOS(string fontName, byte[] fontData);
	}
}
