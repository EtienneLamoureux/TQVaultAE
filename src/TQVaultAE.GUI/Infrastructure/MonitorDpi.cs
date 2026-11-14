using System.Runtime.InteropServices;

namespace TQVaultAE.GUI.Infrastructure;

/// <summary>
/// The DPI Windows gives a monitor (its scaling setting), which <see cref="Screen"/> does not tell.
/// </summary>
internal static partial class MonitorDpi
{
	private const uint MONITOR_DEFAULTTONEAREST = 2;
	private const int MDT_EFFECTIVE_DPI = 0;

	/// <summary>The DPI of <paramref name="screen"/>, or null when Windows does not tell it.</summary>
	public static int? Of(Screen screen)
	{
		var bounds = screen.Bounds;
		var monitor = MonitorFromPoint(new NativePoint(bounds.Left + bounds.Width / 2, bounds.Top + bounds.Height / 2), MONITOR_DEFAULTTONEAREST);
		if (monitor == IntPtr.Zero || GetDpiForMonitor(monitor, MDT_EFFECTIVE_DPI, out uint dpi, out _) != 0)
			return null;

		return (int)dpi;
	}

	[StructLayout(LayoutKind.Sequential)]
	private readonly record struct NativePoint(int X, int Y);

	[LibraryImport("user32.dll")]
	private static partial IntPtr MonitorFromPoint(NativePoint point, uint flags);

	[LibraryImport("shcore.dll")]
	private static partial int GetDpiForMonitor(IntPtr monitor, int dpiType, out uint dpiX, out uint dpiY);
}
