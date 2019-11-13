using System;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using TQVaultAE.Domain.Contracts.Services;

namespace TQVaultAE.Services.Win32
{
	/// <summary>
	/// Windows specific for loading a Font from resource file (hot load from memory)
	/// </summary>
	public class AddFontToOSWin : IAddFontToOS
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
		[System.Runtime.InteropServices.DllImport("gdi32.dll")]
		private static extern IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont, IntPtr pdv, [System.Runtime.InteropServices.In] ref uint pcFonts);

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
	}
}
