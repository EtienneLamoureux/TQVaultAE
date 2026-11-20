using System.Runtime.InteropServices;

namespace TQVaultAE.GUI.Interop;

/// <summary>
/// The uxtheme.dll functions the application calls, each declared once.
/// </summary>
internal static partial class UxTheme
{
	/// <summary>Gives a window another visual style (<c>"DarkMode_Explorer"</c>: dark scroll bars); returns an HRESULT.</summary>
	[LibraryImport("uxtheme.dll", StringMarshalling = StringMarshalling.Utf16)]
	public static partial int SetWindowTheme(IntPtr hwnd, string? subAppName, string? subIdList);
}
