using System.Runtime.InteropServices;

namespace TQVaultAE.Automation;

/// <summary>
/// What the automation endpoints read from and send to any window by its handle, native dialogs included.
/// All calls work from any thread.
/// </summary>
public static partial class Win32Window
{
	/// <summary>The window class of the standard Win32 dialogs (message box, task dialog, common dialog).</summary>
	public const string DialogClass = "#32770";

	/// <summary>The title or text of a window; empty for no window.</summary>
	public static string Text(IntPtr hwnd)
	{
		var buffer = new char[GetWindowTextLength(hwnd) + 1];
		int length = GetWindowText(hwnd, buffer, buffer.Length);
		return new string(buffer, 0, length);
	}

	/// <summary>The class name of a window.</summary>
	public static string ClassName(IntPtr hwnd)
	{
		var buffer = new char[256];
		int length = GetClassName(hwnd, buffer, buffer.Length);
		return new string(buffer, 0, length);
	}

	/// <summary>The child window of a dialog with control id <paramref name="id"/>, or zero.</summary>
	public static IntPtr DialogItem(IntPtr dialog, int id) => GetDlgItem(dialog, id);

	/// <summary>Posts <paramref name="msg"/> to a window's message queue, handled by its thread's message loop.</summary>
	public static bool Post(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam) => PostMessage(hwnd, msg, wParam, lParam);

	[LibraryImport("user32.dll", EntryPoint = "GetWindowTextLengthW")]
	private static partial int GetWindowTextLength(IntPtr hWnd);

	[LibraryImport("user32.dll", EntryPoint = "GetWindowTextW", StringMarshalling = StringMarshalling.Utf16)]
	private static partial int GetWindowText(IntPtr hWnd, [Out] char[] lpString, int nMaxCount);

	[LibraryImport("user32.dll", EntryPoint = "GetClassNameW", StringMarshalling = StringMarshalling.Utf16)]
	private static partial int GetClassName(IntPtr hWnd, [Out] char[] lpClassName, int nMaxCount);

	[LibraryImport("user32.dll")]
	private static partial IntPtr GetDlgItem(IntPtr dialog, int id);

	[LibraryImport("user32.dll", EntryPoint = "PostMessageW")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool PostMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
}
