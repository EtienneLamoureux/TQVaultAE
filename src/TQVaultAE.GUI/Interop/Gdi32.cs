using System.Runtime.InteropServices;

namespace TQVaultAE.GUI.Interop;

/// <summary>
/// The gdi32.dll functions the application calls, each declared once.
/// </summary>
internal static partial class Gdi32
{
	/// <summary>Deletes a GDI object (pen, brush, font, bitmap, region) and frees what it holds.</summary>
	[LibraryImport("gdi32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static partial bool DeleteObject(IntPtr hObject);
}
