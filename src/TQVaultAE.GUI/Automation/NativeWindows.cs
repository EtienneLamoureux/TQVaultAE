//-----------------------------------------------------------------------
// <copyright file="NativeWindows.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.Runtime.InteropServices;
using System.Text;

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

	public sealed record NativeWindow(IntPtr Handle, string Title, string ClassName, bool Visible, bool Enabled, IntPtr Owner)
	{
		/// <summary>
		/// Gets a value indicating whether this is a standard Win32 dialog (message box, task dialog, common dialog).
		/// </summary>
		public bool IsDialogClass => this.ClassName == "#32770";
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
			PostMessage(GetParent(button.Handle), WM_COMMAND, (IntPtr)((BN_CLICKED << 16) | (button.ControlId & 0xFFFF)), button.Handle);
			return button.Text;
		}

		if (StandardCommandIds.TryGetValue(wanted, out var id))
		{
			PostMessage(dialog, WM_COMMAND, (IntPtr)id, IntPtr.Zero);
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

	/// <summary>
	/// Posts a key press (key down, key up) to a window, through the message loop like a real key: WinForms then runs
	/// its pre-processing (IsInputKey, dialog keys) before the control's key events, and the loop's TranslateMessage
	/// adds the character message.
	/// </summary>
	public static bool PostKey(IntPtr hwnd, Keys key)
	{
		const int WM_KEYDOWN = 0x0100;
		const int WM_KEYUP = 0x0101;
		var virtualKey = (IntPtr)(int)(key & Keys.KeyCode);
		// Repeat count 1; key up also sets the previous state and transition bits.
		return PostMessage(hwnd, WM_KEYDOWN, virtualKey, 1)
			&& PostMessage(hwnd, WM_KEYUP, virtualKey, (IntPtr)unchecked((int)0xC0000001));
	}

	private static string Normalize(string text) => text.Replace("&", string.Empty).Trim();

	private static NativeWindow Describe(IntPtr hwnd)
		=> new(hwnd, GetText(hwnd), GetClass(hwnd), IsWindowVisible(hwnd), IsWindowEnabled(hwnd), GetWindow(hwnd, GW_OWNER));

	private static string GetText(IntPtr hwnd)
	{
		var buffer = new char[GetWindowTextLength(hwnd) + 1];
		int length = GetWindowText(hwnd, buffer, buffer.Length);
		return new string(buffer, 0, length);
	}

	private static string GetClass(IntPtr hwnd)
	{
		var buffer = new char[256];
		int length = GetClassName(hwnd, buffer, buffer.Length);
		return new string(buffer, 0, length);
	}

	private const uint GW_OWNER = 4;

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
	private static partial IntPtr GetWindow(IntPtr hWnd, uint uCmd);

	[LibraryImport("user32.dll")]
	private static partial IntPtr GetParent(IntPtr hWnd);

	[LibraryImport("user32.dll")]
	private static partial int GetDlgCtrlID(IntPtr hWnd);

	[LibraryImport("user32.dll", EntryPoint = "GetWindowTextLengthW")]
	private static partial int GetWindowTextLength(IntPtr hWnd);

	[LibraryImport("user32.dll", EntryPoint = "GetWindowTextW", StringMarshalling = StringMarshalling.Utf16)]
	private static partial int GetWindowText(IntPtr hWnd, [Out] char[] lpString, int nMaxCount);

	[LibraryImport("user32.dll", EntryPoint = "GetClassNameW", StringMarshalling = StringMarshalling.Utf16)]
	private static partial int GetClassName(IntPtr hWnd, [Out] char[] lpClassName, int nMaxCount);

	/// <summary>Sends <paramref name="msg"/> to a window and waits for its window procedure, as Windows does for input.</summary>
	public static IntPtr Send(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam) => SendMessage(hwnd, msg, wParam, lParam);

	[LibraryImport("user32.dll", EntryPoint = "SendMessageW")]
	private static partial IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

	[LibraryImport("user32.dll", EntryPoint = "PostMessageW")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool PostMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

	/// <summary>PrintWindow: the window's content as the desktop composes it, not a WM_PRINT repaint.</summary>
	private const uint PW_RENDERFULLCONTENT = 0x2;

	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, uint flags);

	/// <summary>
	/// The pixels of <paramref name="control"/> as the window shows them on its desktop (the hidden one in headless
	/// mode): its top-level window is copied from the desktop's composition, after its pending paints, then cropped to
	/// the control. Unlike <see cref="Control.DrawToBitmap"/> it keeps what the window really drew, overlaps and
	/// transparent children included.
	/// </summary>
	public static Bitmap RenderAsShown(Control control)
	{
		var top = control.TopLevelControl ?? control;
		top.Update();
		var window = top is Form form ? form.Bounds : top.RectangleToScreen(top.ClientRectangle);
		using var whole = new Bitmap(window.Width, window.Height);
		using (var g = Graphics.FromImage(whole))
		{
			var hdc = g.GetHdc();
			try
			{
				if (!PrintWindow(top.Handle, hdc, PW_RENDERFULLCONTENT))
					throw new InvalidOperationException("PrintWindow failed.");
			}
			finally
			{
				g.ReleaseHdc(hdc);
			}
		}

		var area = control.RectangleToScreen(control.ClientRectangle);
		if (control == top && top is Form)
			area = window;
		area.Offset(-window.X, -window.Y);
		return whole.Clone(Rectangle.Intersect(area, new Rectangle(Point.Empty, whole.Size)), whole.PixelFormat);
	}
}
