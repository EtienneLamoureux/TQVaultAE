//-----------------------------------------------------------------------
// <copyright file="Headless.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace TQVaultAE.GUI.Automation;

/// <summary>
/// Debug-only <c>--headless</c> mode: TQVault runs on a hidden Win32 desktop of its own, so no window, taskbar button
/// or focus change ever reaches the user's screen, while every window is still created, shown, laid out and painted
/// as usual (DrawToBitmap captures, tooltips and dialogs work) and is driven through the automation endpoint.
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
/// process started with the switch creates the desktop, starts TQVault again on it (<c>STARTUPINFO.lpDesktop</c>) and
/// leaves once that instance is idle: every thread of the instance, the endpoint's included, lives on the hidden
/// desktop, and so does an instance restarted by <c>Application.Restart</c>, which inherits it.
/// </para>
/// <para>
/// Message boxes are never shown (see <see cref="AutomationHost.TryTakeDialogAnswer"/>), and native dialogs are
/// cancelled as soon as they are activated, since nobody could answer them.
/// </para>
/// </remarks>
internal static partial class Headless
{
	/// <summary>The command line switch.</summary>
	public const string Switch = "--headless";

	/// <summary>Environment variable that gives the started instance the name of its hidden desktop.</summary>
	private const string DesktopVariable = "TQVAULT_HEADLESS_DESKTOP";

	private static IntPtr _cbtHook;
	private static IntPtr _lastCancelled;

	// Kept in a field: the hook outlives any local.
	private static HookProc? _cbtProc;

	private static readonly ConcurrentQueue<string> _cancelled = new();

	/// <summary>Gets a value indicating whether TQVault runs on its hidden desktop.</summary>
	public static bool Enabled { get; private set; }

	/// <summary>Gets the name of the hidden desktop, or null.</summary>
	public static string? DesktopName { get; private set; }

	/// <summary>Gets the native dialogs cancelled because nobody could answer them, oldest first.</summary>
	public static IReadOnlyList<string> CancelledNativeDialogs => [.. _cancelled];

	/// <summary>Whether <paramref name="args"/> holds the switch.</summary>
	public static bool IsRequested(IEnumerable<string> args)
		=> args.Any(a => string.Equals(a, Switch, StringComparison.OrdinalIgnoreCase));

	/// <summary><paramref name="args"/> without the switch, for the regular command line parsing.</summary>
	public static string[] WithoutSwitch(string[] args)
		=> [.. args.Where(a => !string.Equals(a, Switch, StringComparison.OrdinalIgnoreCase))];

	/// <summary>
	/// Whether this process already runs on the hidden desktop it was started on; if so, turns headless mode on.
	/// </summary>
	public static bool TryAttach()
	{
		var expected = Environment.GetEnvironmentVariable(DesktopVariable);
		if (string.IsNullOrEmpty(expected) || !string.Equals(CurrentDesktopName(), expected, StringComparison.OrdinalIgnoreCase))
			return false;

		DesktopName = expected;
		Enabled = true;
		_cbtProc = CbtProc;
		_cbtHook = SetWindowsHookEx(WH_CBT, _cbtProc, IntPtr.Zero, GetCurrentThreadId());
		return true;
	}

	/// <summary>
	/// Creates a hidden desktop and starts this executable again on it, with the same command line, then waits until
	/// the new instance is idle (it then holds the desktop).
	/// </summary>
	/// <returns>An error message, or null when the headless instance runs.</returns>
	public static string? StartOnHiddenDesktop()
	{
		var name = $"TQVaultAE-headless-{Environment.ProcessId}";
		var desktop = CreateDesktop(name, IntPtr.Zero, IntPtr.Zero, 0, DESKTOP_ALL_ACCESS, IntPtr.Zero);
		if (desktop == IntPtr.Zero)
			return $"CreateDesktop failed ({Marshal.GetLastPInvokeError()})";

		try
		{
			Environment.SetEnvironmentVariable(DesktopVariable, name);

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

	/// <summary>Removes the record of the cancelled native dialogs.</summary>
	public static void ClearCancelled() => _cancelled.Clear();

	private static IntPtr CbtProc(int code, IntPtr wParam, IntPtr lParam)
	{
		// A top-level standard dialog (common dialog, task dialog) is about to be activated: cancel it, nobody can see it.
		// Message boxes never get here, they are answered without being shown.
		if (code == HCBT_ACTIVATE && IsTopLevelDialog(wParam))
		{
			// A dialog can be activated more than once before the cancel arrives: record it once.
			if (wParam != _lastCancelled)
			{
				_lastCancelled = wParam;
				var title = GetText(wParam);
				_cancelled.Enqueue($"{DateTime.Now:HH:mm:ss} {title}");
				AutomationHost.Log?.LogWarning("Headless: native dialog \"{Title}\" cancelled", title);
			}
			PostMessage(wParam, WM_COMMAND, IDCANCEL, IntPtr.Zero);
		}

		return CallNextHookEx(_cbtHook, code, wParam, lParam);
	}

	private static bool IsTopLevelDialog(IntPtr hwnd)
	{
		var buffer = new char[16];
		int length = GetClassName(hwnd, buffer, buffer.Length);
		return new string(buffer, 0, length) == "#32770" && (GetWindowLong(hwnd, GWL_STYLE) & WS_CHILD) == 0;
	}

	private static string GetText(IntPtr hwnd)
	{
		var buffer = new char[256];
		int length = GetWindowText(hwnd, buffer, buffer.Length);
		return new string(buffer, 0, length);
	}

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
	private static readonly IntPtr IDCANCEL = 2;

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

	[LibraryImport("user32.dll", EntryPoint = "GetClassNameW", StringMarshalling = StringMarshalling.Utf16)]
	private static partial int GetClassName(IntPtr hWnd, [Out] char[] lpClassName, int nMaxCount);

	[LibraryImport("user32.dll", EntryPoint = "GetWindowTextW", StringMarshalling = StringMarshalling.Utf16)]
	private static partial int GetWindowText(IntPtr hWnd, [Out] char[] lpString, int nMaxCount);

	[LibraryImport("user32.dll", EntryPoint = "GetWindowLongW")]
	private static partial int GetWindowLong(IntPtr hWnd, int index);

	[LibraryImport("user32.dll", EntryPoint = "PostMessageW")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool PostMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
}
