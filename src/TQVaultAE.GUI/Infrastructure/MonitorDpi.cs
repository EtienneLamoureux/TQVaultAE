using TQVaultAE.GUI.Interop;

namespace TQVaultAE.GUI.Infrastructure;

/// <summary>
/// The DPI Windows gives a monitor (its scaling setting), which <see cref="Screen"/> does not tell.
/// </summary>
internal static class MonitorDpi
{
	/// <summary>The system DPI: the DPI of the primary monitor when the user signed in, which the themes draw at.</summary>
	public static int SystemDpi => (int)User32.GetDpiForSystem();

	/// <summary>The DPI of <paramref name="screen"/>, or null when Windows does not tell it.</summary>
	public static int? Of(Screen screen)
	{
		var bounds = screen.Bounds;
		var monitor = User32.MonitorFromPoint(new User32.NativePoint(bounds.Left + bounds.Width / 2, bounds.Top + bounds.Height / 2),
			User32.MONITOR_DEFAULTTONEAREST);
		if (monitor == IntPtr.Zero || Shcore.GetDpiForMonitor(monitor, Shcore.MDT_EFFECTIVE_DPI, out uint dpi, out _) != 0)
			return null;

		return (int)dpi;
	}
}
