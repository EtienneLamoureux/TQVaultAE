//-----------------------------------------------------------------------
// <copyright file="NativeWindows.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.Runtime.InteropServices;
using System.Text;
using TQVaultAE.Automation;

namespace TQVaultAE.GUI.Automation;

/// <summary>
/// Win32 helpers to find and answer the process' own top-level windows, including native message boxes
/// that are not part of <see cref="System.Windows.Forms.Application.OpenForms"/>.
/// All calls work from any thread.
/// </summary>
internal static partial class NativeWindows
{
	private const int WM_COMMAND = 0x0111;
	private const int BN_CLICKED = 0;

	/// <summary>
	/// Standard dialog command ids (IDOK, IDCANCEL, ...) used as a fallback when a button is not a child window (task dialogs).
	/// </summary>
	private static readonly Dictionary<string, int> StandardCommandIds = new(StringComparer.OrdinalIgnoreCase)
	{
		["OK"] = 1,
		["Cancel"] = 2,
		["Abort"] = 3,
		["Retry"] = 4,
		["Ignore"] = 5,
		["Yes"] = 6,
		["No"] = 7,
		["Close"] = 8,
	};

	/// <summary>The window class of the standard Win32 dialogs (message box, task dialog, common dialog).</summary>
	public const string DialogClass = Win32Window.DialogClass;

	public sealed record NativeWindow(IntPtr Handle, string Title, string ClassName, bool Enabled)
	{
		/// <summary>
		/// Gets a value indicating whether this is a standard Win32 dialog (message box, task dialog, common dialog).
		/// </summary>
		public bool IsDialogClass => this.ClassName == DialogClass;
	}

	public sealed record NativeButton(IntPtr Handle, int ControlId, string Text);

	/// <summary>
	/// Lists the visible top-level windows that belong to the current process.
	/// </summary>
	public static List<NativeWindow> GetProcessTopLevelWindows()
	{
		var pid = (uint)Environment.ProcessId;
		var result = new List<NativeWindow>();
		EnumWindows((hwnd, _) =>
		{
			if (GetWindowThreadProcessId(hwnd, out var windowPid) != 0 && windowPid == pid && IsWindowVisible(hwnd))
				result.Add(Describe(hwnd));
			return true;
		}, IntPtr.Zero);
		return result;
	}

	/// <summary>
	/// Lists the button child windows of a dialog.
	/// </summary>
	public static List<NativeButton> GetButtons(IntPtr dialog)
	{
		var result = new List<NativeButton>();
		EnumChildWindows(dialog, (hwnd, _) =>
		{
			// "Button" for native dialogs, "WindowsForms10.BUTTON.app..." for WinForms buttons.
			if (GetClass(hwnd).Contains("BUTTON", StringComparison.OrdinalIgnoreCase) && IsWindowVisible(hwnd) && IsWindowEnabled(hwnd))
				result.Add(new NativeButton(hwnd, GetDlgCtrlID(hwnd), GetText(hwnd)));
			return true;
		}, IntPtr.Zero);
		return result;
	}

	/// <summary>
	/// Presses a dialog button by its text (accelerator ampersands are ignored).
	/// Falls back to standard command ids (OK, Cancel, Yes, No...) for dialogs without button child windows.
	/// </summary>
	/// <returns>The text of the pressed button, or null when nothing matched.</returns>
	public static string? PressButton(IntPtr dialog, string buttonText)
	{
		var wanted = Normalize(buttonText);
		var button = GetButtons(dialog).FirstOrDefault(b => Normalize(b.Text) == wanted);
		if (button is not null)
		{
			// WM_COMMAND/BN_CLICKED to the parent works even when the dialog is not the foreground window, unlike BM_CLICK.
			// WinForms reflects it to the button, which raises Click.
			Post(GetParent(button.Handle), WM_COMMAND, (IntPtr)((BN_CLICKED << 16) | (button.ControlId & 0xFFFF)), button.Handle);
			return button.Text;
		}

		if (StandardCommandIds.TryGetValue(wanted, out var id))
		{
			Post(dialog, WM_COMMAND, (IntPtr)id, IntPtr.Zero);
			return buttonText;
		}

		return null;
	}

	/// <summary>
	/// Gets the text of the static (label) children of a dialog, e.g. a message box body.
	/// </summary>
	public static List<string> GetStaticTexts(IntPtr dialog)
	{
		var result = new List<string>();
		EnumChildWindows(dialog, (hwnd, _) =>
		{
			if (GetClass(hwnd) == "Static")
			{
				var text = GetText(hwnd);
				if (!string.IsNullOrWhiteSpace(text))
					result.Add(text);
			}
			return true;
		}, IntPtr.Zero);
		return result;
	}

	private static string Normalize(string text) => text.Replace("&", string.Empty).Trim();

	private static NativeWindow Describe(IntPtr hwnd)
		=> new(hwnd, GetText(hwnd), GetClass(hwnd), IsWindowEnabled(hwnd));

	/// <summary>The title or text of a window.</summary>
	public static string GetText(IntPtr hwnd) => Win32Window.Text(hwnd);

	/// <summary>The class name of a window.</summary>
	public static string GetClass(IntPtr hwnd) => Win32Window.ClassName(hwnd);

	private const uint SWP_NOSIZE = 0x0001;
	private const uint SWP_NOZORDER = 0x0004;
	private const uint SWP_NOACTIVATE = 0x0010;

	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint flags);

	/// <summary>Moves a window without resizing it, as Windows does while the user drags it.</summary>
	public static void MoveTo(IntPtr hwnd, int x, int y) => SetWindowPos(hwnd, IntPtr.Zero, x, y, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE);

	private delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr lParam);

	// EnumWindows and EnumChildWindows stay [DllImport]: source-generated P/Invokes do not marshal delegates,
	// and the callbacks are closures over local state.
	[DllImport("user32.dll")]
	private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

	[DllImport("user32.dll")]
	private static extern bool EnumChildWindows(IntPtr hwndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

	[LibraryImport("user32.dll")]
	private static partial uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool IsWindowVisible(IntPtr hWnd);

	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool IsWindowEnabled(IntPtr hWnd);

	[LibraryImport("user32.dll")]
	private static partial IntPtr GetParent(IntPtr hWnd);

	[LibraryImport("user32.dll")]
	private static partial int GetDlgCtrlID(IntPtr hWnd);

	/// <summary>Posts <paramref name="msg"/> to a window's message queue, handled by its thread's message loop.</summary>
	public static bool Post(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam) => Win32Window.Post(hwnd, msg, wParam, lParam);
}
