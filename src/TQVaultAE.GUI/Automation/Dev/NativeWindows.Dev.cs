using System.Runtime.InteropServices;
using System.Text;
using TQVaultAE.Automation;

namespace TQVaultAE.GUI.Automation;

/// <summary>Debug window helpers of the developer tools.</summary>
internal static partial class NativeWindows
{
	/// <summary>The bounds of a window on the screen, as Windows gives them; empty when it gives none.</summary>
	public static Rectangle WindowRect(IntPtr hwnd)
		=> GetWindowRect(hwnd, out var r) ? Rectangle.FromLTRB(r.Left, r.Top, r.Right, r.Bottom) : Rectangle.Empty;

	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool GetWindowRect(IntPtr hWnd, out NativeRect rect);

	[StructLayout(LayoutKind.Sequential)]
	private struct NativeRect
	{
		public int Left;
		public int Top;
		public int Right;
		public int Bottom;
	}
}
