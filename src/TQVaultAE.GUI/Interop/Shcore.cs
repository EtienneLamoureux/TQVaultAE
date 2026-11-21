using System.Runtime.InteropServices;

namespace TQVaultAE.GUI.Interop;

/// <summary>
/// The shcore.dll functions the application calls, each declared once.
/// </summary>
internal static partial class Shcore
{
	/// <summary>GetDpiForMonitor: the DPI the monitor's scaling setting gives (MDT_EFFECTIVE_DPI).</summary>
	public const int MDT_EFFECTIVE_DPI = 0;

	/// <summary>The DPI of a monitor; returns an HRESULT, 0 on success.</summary>
	[LibraryImport("shcore.dll")]
	public static partial int GetDpiForMonitor(IntPtr monitor, int dpiType, out uint dpiX, out uint dpiY);
}
