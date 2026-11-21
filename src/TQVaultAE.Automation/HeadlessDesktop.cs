using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace TQVaultAE.Automation;

/// <summary>
/// The <c>--headless</c> mode of the apps (TQVaultAE in every build, ARZExplorer in Debug builds): the app runs on a hidden Win32 desktop of its own, so no window,
/// taskbar button or focus change ever reaches the user's screen, while every window is still created, shown, laid out
/// and painted as usual (captures, tooltips and dialogs work) and is driven through the app's automation endpoint.
/// </summary>
/// <remarks>
/// <para>
/// A hidden desktop rather than off-screen windows: the tooltips and dialogs place themselves inside
/// <c>Screen.FromControl(this).WorkingArea</c>, which would pull an off-screen window back onto a monitor; on a desktop
/// of its own every window keeps its normal position, monitor and DPI, and native windows (common dialogs, task dialogs)
/// are covered without knowing them.
/// </para>
/// <para>
/// The main thread cannot move to another desktop (<c>[STAThread]</c> already gave it the hidden COM window), so the
/// process started with the switch creates the desktop, starts the app again on it (<c>STARTUPINFO.lpDesktop</c>) and
/// leaves once that instance is idle: every thread of the instance, the endpoint's included, lives on the hidden
/// desktop, and so does an instance restarted by <c>Application.Restart</c>, which inherits it.
/// </para>
/// <para>
/// The standard dialogs activated on the hidden desktop are cancelled at once, since nobody could answer them, and
/// recorded (<see cref="CancelledDialogs"/>). Each app names its desktop and its environment variable, and says how a
/// cancelled dialog is described.
/// </para>
/// </remarks>
public static partial class HeadlessDesktop
{
	/// <summary>The command line switch.</summary>
	public const string Switch = "--headless";

	private static IntPtr _cbtHook;
	private static IntPtr _lastCancelled;

	// Kept in a field: the hook outlives any local.
	private static HookProc? _cbtProc;

	private static Func<IntPtr, string> _describe = Win32Window.Text;
	private static Action<string>? _onCancelled;

	private static readonly ConcurrentQueue<string> _cancelled = new();

	/// <summary>Gets a value indicating whether the app runs on its hidden desktop.</summary>
	public static bool Enabled { get; private set; }

	/// <summary>Gets the name of the hidden desktop, or null.</summary>
	public static string? DesktopName { get; private set; }

	/// <summary>Gets the dialogs cancelled because nobody could answer them (time and description), oldest first.</summary>
	public static IReadOnlyList<string> CancelledDialogs => [.. _cancelled];

	/// <summary>Removes the record of the cancelled dialogs.</summary>
	public static void ClearCancelled() => _cancelled.Clear();

	/// <summary>Whether <paramref name="args"/> holds the switch.</summary>
	public static bool IsRequested(IEnumerable<string> args)
		=> args.Any(a => string.Equals(a, Switch, StringComparison.OrdinalIgnoreCase));

	/// <summary><paramref name="args"/> without the switch, for the regular command line parsing.</summary>
	public static string[] WithoutSwitch(string[] args)
		=> [.. args.Where(a => !string.Equals(a, Switch, StringComparison.OrdinalIgnoreCase))];

	/// <summary>
	/// Whether this process already runs on the hidden desktop it was started on; if so, turns headless mode on and
	/// cancels from now on the standard dialogs activated on the calling (UI) thread.
	/// </summary>
	/// <param name="desktopVariable">The environment variable <see cref="StartOnHiddenDesktop"/> gave the desktop's name in.</param>
	/// <param name="describe">Describes a cancelled dialog for the record; its title by default.</param>
	/// <param name="onCancelled">Called with the description of each cancelled dialog (to log it).</param>
	public static bool TryAttach(string desktopVariable, Func<IntPtr, string>? describe = null, Action<string>? onCancelled = null)
	{
		var expected = Environment.GetEnvironmentVariable(desktopVariable);
		if (string.IsNullOrEmpty(expected) || !string.Equals(CurrentDesktopName(), expected, StringComparison.OrdinalIgnoreCase))
			return false;

		DesktopName = expected;
		Enabled = true;
		_describe = describe ?? Win32Window.Text;
		_onCancelled = onCancelled;
		_cbtProc = CbtProc;
		_cbtHook = SetWindowsHookEx(WH_CBT, _cbtProc, IntPtr.Zero, GetCurrentThreadId());
		return true;
	}

	/// <summary>
	/// Creates a hidden desktop and starts this executable again on it, with the same command line, then waits until
	/// the new instance is idle (it then holds the desktop).
	/// </summary>
	/// <param name="desktopVariable">The environment variable that gives the started instance the name of its desktop.</param>
	/// <param name="desktopPrefix">The start of the desktop's name, which ends with this process' id.</param>
	/// <returns>An error message, or null when the headless instance runs.</returns>
	public static string? StartOnHiddenDesktop(string desktopVariable, string desktopPrefix)
	{
		var name = $"{desktopPrefix}-headless-{Environment.ProcessId}";
		var desktop = CreateDesktop(name, IntPtr.Zero, IntPtr.Zero, 0, DESKTOP_ALL_ACCESS, IntPtr.Zero);
		if (desktop == IntPtr.Zero)
			return $"CreateDesktop failed ({Marshal.GetLastPInvokeError()})";

		try
		{
			Environment.SetEnvironmentVariable(desktopVariable, name);

			var startup = new STARTUPINFO { cb = Marshal.SizeOf<STARTUPINFO>(), lpDesktop = $@"WinSta0\{name}" };
			var exe = Environment.ProcessPath ?? throw new InvalidOperationException("No process path");
			if (!CreateProcess(exe, Environment.CommandLine, IntPtr.Zero, IntPtr.Zero, false, 0, IntPtr.Zero, null, ref startup, out var process))
				return $"CreateProcess failed ({Marshal.GetLastWin32Error()})";

			// Idle means connected to the desktop: closing this process' handle then leaves the desktop alive.
			var wait = WaitForInputIdle(process.hProcess, 120_000);
			CloseHandle(process.hThread);
			CloseHandle(process.hProcess);
			return wait == WAIT_FAILED ? $"WaitForInputIdle failed ({Marshal.GetLastPInvokeError()})" : null;
		}
		finally
		{
			CloseDesktop(desktop);
		}
	}

	private static IntPtr CbtProc(int code, IntPtr wParam, IntPtr lParam)
	{
		// A top-level standard dialog (message box, common dialog, task dialog) is about to be activated: cancel it,
		// nobody can see it.
		if (code == HCBT_ACTIVATE && IsTopLevelDialog(wParam))
		{
			// A dialog can be activated more than once before the cancel arrives: record it once.
			if (wParam != _lastCancelled)
			{
				_lastCancelled = wParam;
				var description = _describe(wParam);
				_cancelled.Enqueue($"{DateTime.Now:HH:mm:ss} {description}");
				_onCancelled?.Invoke(description);
			}
			Win32Window.Post(wParam, WM_COMMAND, DefaultAnswer(wParam), IntPtr.Zero);
		}

		return CallNextHookEx(_cbtHook, code, wParam, lParam);
	}

	/// <summary>
	/// The command that closes a dialog with its default answer: Cancel, else No (a Yes/No box ignores Cancel and would
	/// stay open), else Ignore (Abort/Retry/Ignore); Cancel for the others (an OK box takes it as OK).
	/// </summary>
	private static IntPtr DefaultAnswer(IntPtr dialog)
	{
		foreach (var id in (ReadOnlySpan<int>)[IDCANCEL, IDNO, IDIGNORE])
		{
			if (Win32Window.DialogItem(dialog, id) != IntPtr.Zero)
				return id;
		}
		return IDCANCEL;
	}

	private static bool IsTopLevelDialog(IntPtr hwnd)
		=> Win32Window.ClassName(hwnd) == Win32Window.DialogClass && (GetWindowLong(hwnd, GWL_STYLE) & WS_CHILD) == 0;

	private static string? CurrentDesktopName()
	{
		var desktop = GetThreadDesktop(GetCurrentThreadId());
		var buffer = new char[256];
		return GetUserObjectInformation(desktop, UOI_NAME, buffer, buffer.Length * 2, out var needed)
			? new string(buffer, 0, Math.Max(0, needed / 2 - 1))
			: null;
	}

	private delegate IntPtr HookProc(int code, IntPtr wParam, IntPtr lParam);

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct STARTUPINFO
	{
		public int cb;
		public string? lpReserved;
		public string? lpDesktop;
		public string? lpTitle;
		public int dwX, dwY, dwXSize, dwYSize, dwXCountChars, dwYCountChars, dwFillAttribute, dwFlags;
		public short wShowWindow, cbReserved2;
		public IntPtr lpReserved2, hStdInput, hStdOutput, hStdError;
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct PROCESS_INFORMATION
	{
		public IntPtr hProcess, hThread;
		public int dwProcessId, dwThreadId;
	}

	private const uint DESKTOP_ALL_ACCESS = 0x01FF | 0x000F0000;
	private const int UOI_NAME = 2;
	private const uint WAIT_FAILED = 0xFFFFFFFF;
	private const int WH_CBT = 5;
	private const int HCBT_ACTIVATE = 5;
	private const int WM_COMMAND = 0x0111;
	private const int GWL_STYLE = -16;
	private const int WS_CHILD = 0x40000000;
	private const int IDCANCEL = 2;
	private const int IDIGNORE = 5;
	private const int IDNO = 7;

	[LibraryImport("user32.dll", EntryPoint = "CreateDesktopW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
	private static partial IntPtr CreateDesktop(string name, IntPtr device, IntPtr devmode, uint flags, uint access, IntPtr attributes);

	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool CloseDesktop(IntPtr desktop);

	[LibraryImport("user32.dll")]
	private static partial IntPtr GetThreadDesktop(uint threadId);

	[LibraryImport("user32.dll", EntryPoint = "GetUserObjectInformationW", StringMarshalling = StringMarshalling.Utf16)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool GetUserObjectInformation(IntPtr obj, int index, [Out] char[] info, int length, out int needed);

	// Stays [DllImport]: the struct holds strings, which source-generated P/Invokes do not marshal in structs.
	[DllImport("kernel32.dll", EntryPoint = "CreateProcessW", SetLastError = true, CharSet = CharSet.Unicode)]
	private static extern bool CreateProcess(string application, string commandLine, IntPtr processAttributes, IntPtr threadAttributes,
		bool inheritHandles, uint flags, IntPtr environment, string? currentDirectory, ref STARTUPINFO startup, out PROCESS_INFORMATION process);

	[LibraryImport("user32.dll", SetLastError = true)]
	private static partial uint WaitForInputIdle(IntPtr process, uint milliseconds);

	[LibraryImport("kernel32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool CloseHandle(IntPtr handle);

	// Stays [DllImport]: source-generated P/Invokes do not marshal delegates.
	[DllImport("user32.dll", EntryPoint = "SetWindowsHookExW")]
	private static extern IntPtr SetWindowsHookEx(int hookId, HookProc proc, IntPtr module, uint threadId);

	[LibraryImport("user32.dll")]
	private static partial IntPtr CallNextHookEx(IntPtr hook, int code, IntPtr wParam, IntPtr lParam);

	[LibraryImport("kernel32.dll")]
	private static partial uint GetCurrentThreadId();

	[LibraryImport("user32.dll", EntryPoint = "GetWindowLongW")]
	private static partial int GetWindowLong(IntPtr hWnd, int index);
}
