using System.Runtime.InteropServices;
using TQVaultAE.Application.KeyBinds;
using TQVaultAE.GUI.Interop;

namespace TQVaultAE.GUI.Automation;

/// <summary>Debug input helpers of the developer tools: key and mouse button messages, keys posted to the focused window, the wheel.</summary>
internal static partial class AutomationInput
{
	private const int WM_KEYDOWN = 0x0100;
	private const int WM_KEYUP = 0x0101;
	private const int WM_SYSKEYDOWN = 0x0104;
	private const int WM_SYSKEYUP = 0x0105;
	private const int WM_APP_RESTORE = 0x8000 + 0x4B42;

	/// <summary>The keyboard states to give back once posted keys are processed, oldest first.</summary>
	private static readonly Queue<byte[]> PendingRestores = new();

	private static RestoreWindow? restoreWindow;

	/// <summary>
	/// Presses and releases <paramref name="keyData"/> (a key and its modifiers) like the keyboard: the key messages are
	/// posted to the focused window (else <paramref name="fallback"/>) and handled by the running message loop after the
	/// calling tool returns, so a key that opens a modal window works; the modifiers are released after the key.
	/// Call on the UI thread.
	/// </summary>
	/// <returns>The control the key was sent to.</returns>
	public static Control PostKey(Control fallback, Keys keyData)
	{
		var target = FocusedControl() ?? fallback;
		var previous = new byte[256];
		GetKeyboardState(previous);
		SetKeyboardState(WithModifiers(previous, keyData & Keys.Modifiers));

		var (down, up, wParam, downParam, upParam) = KeyMessages(keyData);
		PostMessage(target.Handle, down, wParam, downParam);
		PostMessage(target.Handle, up, wParam, upParam);

		// Posted after the key: the message loop handles it once the key is processed, a modal window's loop included.
		restoreWindow ??= new RestoreWindow();
		PendingRestores.Enqueue(previous);
		PostMessage(restoreWindow.Handle, WM_APP_RESTORE, 0, 0);
		return target;
	}

	/// <summary>
	/// Turns the mouse wheel over a client point of <paramref name="target"/> (<c>WM_MOUSEWHEEL</c>, the point on the
	/// screen): positive notches away from the user (up), negative towards. The modifiers are held as for
	/// <see cref="SendClick"/>; an unhandled wheel goes on to the parent windows, as for the user.
	/// </summary>
	public static void SendWheel(Control target, Point client, int notches, Keys modifiers)
	{
		const int WM_MOUSEWHEEL = 0x020A, WHEEL_DELTA = 120;
		using var held = HoldModifiers(modifiers);
		var screen = target.PointToScreen(client);
		SendMessage(target.Handle, WM_MOUSEWHEEL, ((notches * WHEEL_DELTA) << 16) | KeyFlags(modifiers), User32.MakeLParam(screen));
	}

	/// <summary>A message-only window that gives back the keyboard state once the posted keys before it are handled.</summary>
	private sealed class RestoreWindow : NativeWindow
	{
		public RestoreWindow() => this.CreateHandle(new CreateParams { Parent = new IntPtr(-3) });

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == WM_APP_RESTORE && PendingRestores.TryDequeue(out var state))
			{
				SetKeyboardState(state);
				return;
			}

			base.WndProc(ref m);
		}
	}

	private static bool PostMessage(nint hWnd, int msg, nint wParam, nint lParam) => NativeWindows.Post(hWnd, msg, wParam, lParam);

	/// <summary>
	/// Presses and releases <paramref name="keyData"/> now, through the steps of the message loop: pre-processing, then
	/// the window procedure of the focused window (else <paramref name="fallback"/>). For keys that open no modal window.
	/// Call on the UI thread.
	/// </summary>
	/// <returns>The control the key was sent to.</returns>
	public static Control SendKey(Control fallback, Keys keyData)
	{
		var target = FocusedControl() ?? fallback;
		using var held = HoldModifiers(keyData & Keys.Modifiers);
		var (down, up, wParam, downParam, upParam) = KeyMessages(keyData);
		Dispatch(target, down, wParam, downParam);
		Dispatch(target, up, wParam, upParam);
		return target;
	}

	/// <summary>
	/// Sends one button message to <paramref name="target"/> at a client point (which may lie outside it, as for the
	/// control that has the mouse capture during a drag): the button down, with its key flag, or up.
	/// </summary>
	public static void SendButton(Control target, Point client, MouseButtons button, bool down)
	{
		var (downMessage, upMessage, _, flag) = ButtonMessages(button);
		SendMessage(target.Handle, down ? downMessage : upMessage, down ? flag : 0, User32.MakeLParam(client));
	}

	/// <summary>The messages of a key: WM_SYSKEY* with Alt held, as Windows sends them, and their parameters.</summary>
	private static (int Down, int Up, nint WParam, nint DownParam, nint UpParam) KeyMessages(Keys keyData)
	{
		int vk = (int)(keyData & Keys.KeyCode);
		bool alt = keyData.HasFlag(Keys.Alt);
		int scan = (int)MapVirtualKey((uint)vk, 0);
		int context = alt ? 1 << 29 : 0;
		nint downParam = 1 | (scan << 16) | context;
		nint upParam = (nint)(1 | (scan << 16) | context | (1 << 30) | unchecked((int)0x80000000));
		return alt || vk == (int)Keys.F10
			? (WM_SYSKEYDOWN, WM_SYSKEYUP, vk, downParam, upParam)
			: (WM_KEYDOWN, WM_KEYUP, vk, downParam, upParam);
	}

	/// <summary>One key message as the message loop handles it: pre-processing, translation, then the window procedure.</summary>
	private static void Dispatch(Control target, int message, nint wParam, nint lParam)
	{
		var msg = Message.Create(target.Handle, message, wParam, lParam);
		if (target.PreProcessControlMessage(ref msg) == PreProcessControlState.MessageProcessed)
			return;

		var native = new NativeMessage { hwnd = msg.HWnd, message = (uint)msg.Msg, wParam = msg.WParam, lParam = msg.LParam };
		TranslateMessage(ref native);
		SendMessage(msg.HWnd, msg.Msg, msg.WParam, msg.LParam);
	}

	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool TranslateMessage(ref NativeMessage msg);

	[LibraryImport("user32.dll", EntryPoint = "MapVirtualKeyW")]
	private static partial uint MapVirtualKey(uint code, uint mapType);

	[StructLayout(LayoutKind.Sequential)]
	private struct NativeMessage
	{
		public nint hwnd;
		public uint message;
		public nint wParam;
		public nint lParam;
		public uint time;
		public int ptX;
		public int ptY;
	}
}
