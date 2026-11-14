using System.Runtime.InteropServices;

namespace TQVaultAE.GUI.Automation;

/// <summary>
/// Keyboard and mouse input for the automation tools, through the path Windows Forms uses for the real keyboard and
/// mouse: the key messages go to the window that has the keyboard focus and pass the message loop's pre-processing
/// (<c>ProcessCmdKey</c>, <c>IsInputKey</c>, <c>ProcessDialogKey</c>, the form's key preview) before the control's
/// key events; the mouse messages go to the control's window procedure. The modifiers are held in the UI thread's
/// keyboard state, where <see cref="Control.ModifierKeys"/> and the key events read them.
/// </summary>
/// <remarks>
/// The modifier keys themselves send no message: Windows enters the menu mode on a lone Alt from the real keyboard's
/// state only, which posted messages cannot reproduce.
/// </remarks>
internal static partial class AutomationInput
{
	private const int WM_KEYDOWN = 0x0100;
	private const int WM_KEYUP = 0x0101;
	private const int WM_SYSKEYDOWN = 0x0104;
	private const int WM_SYSKEYUP = 0x0105;
	private const int WM_LBUTTONDOWN = 0x0201;
	private const int WM_LBUTTONUP = 0x0202;
	private const int WM_RBUTTONDOWN = 0x0204;
	private const int WM_RBUTTONUP = 0x0205;
	private const int WM_APP_RESTORE = 0x8000 + 0x4B42;
	private const int MK_LBUTTON = 0x0001;
	private const int MK_RBUTTON = 0x0002;
	private const int MK_SHIFT = 0x0004;
	private const int MK_CONTROL = 0x0008;

	private const int VK_SHIFT = 0x10, VK_CONTROL = 0x11, VK_MENU = 0x12;
	private const int VK_LSHIFT = 0xA0, VK_RSHIFT = 0xA1, VK_LCONTROL = 0xA2, VK_RCONTROL = 0xA3, VK_LMENU = 0xA4, VK_RMENU = 0xA5;

	/// <summary>The keyboard states to give back once posted keys are processed, oldest first.</summary>
	private static readonly Queue<byte[]> PendingRestores = new();

	private static RestoreWindow? restoreWindow;

	/// <summary>The control that has the keyboard focus in this application, if any.</summary>
	public static Control? FocusedControl() => Control.FromChildHandle(GetFocus());

	/// <summary>
	/// Holds exactly <paramref name="modifiers"/> (Ctrl, Shift, Alt) in the UI thread's keyboard state until disposed.
	/// Call on the UI thread.
	/// </summary>
	public static IDisposable HoldModifiers(Keys modifiers)
	{
		var previous = new byte[256];
		GetKeyboardState(previous);
		SetKeyboardState(WithModifiers(previous, modifiers));
		return new Restore(previous);
	}

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
	/// Presses and releases a mouse button at a client location of <paramref name="target"/>, with
	/// <paramref name="modifiers"/> held, through its window procedure. Call on the UI thread.
	/// </summary>
	public static void SendClick(Control target, Point client, MouseButtons button, Keys modifiers)
	{
		using var held = HoldModifiers(modifiers);
		var (down, up, buttonFlag) = button == MouseButtons.Right
			? (WM_RBUTTONDOWN, WM_RBUTTONUP, MK_RBUTTON)
			: (WM_LBUTTONDOWN, WM_LBUTTONUP, MK_LBUTTON);
		int keyFlags = (modifiers.HasFlag(Keys.Control) ? MK_CONTROL : 0) | (modifiers.HasFlag(Keys.Shift) ? MK_SHIFT : 0);
		nint position = (client.Y << 16) | (client.X & 0xFFFF);
		SendMessage(target.Handle, down, buttonFlag | keyFlags, position);
		SendMessage(target.Handle, up, keyFlags, position);
	}

	/// <summary>The center of <paramref name="control"/> in its client coordinates.</summary>
	/// <summary>
	/// Right-clicks <paramref name="target"/> at a client point (button down and up), then sends the
	/// <c>WM_CONTEXTMENU</c> Windows sends after it with that point on the screen: the control shows its
	/// <see cref="Control.ContextMenuStrip"/> as for a user, its <c>Opening</c> event and <c>SourceControl</c> included.
	/// </summary>
	public static void SendContextClick(Control target, Point client) => SendMouse(target, client, MouseButtons.Right, 1);

	/// <summary>
	/// Sends what Windows sends for a click at a client point of <paramref name="target"/>: a move, then the button down
	/// and up; for a double click a second down as <c>WM_*BUTTONDBLCLK</c> and up; for the right button, then the
	/// <c>WM_CONTEXTMENU</c> with the point on the screen.
	/// </summary>
	public static void SendMouse(Control target, Point client, MouseButtons button, int clicks)
	{
		const int WM_MOUSEMOVE = 0x0200, WM_LBUTTONDBLCLK = 0x0203, WM_RBUTTONDBLCLK = 0x0206, WM_CONTEXTMENU = 0x007B;
		bool right = button == MouseButtons.Right;
		var (down, up, doubleClick, flag) = right
			? (WM_RBUTTONDOWN, WM_RBUTTONUP, WM_RBUTTONDBLCLK, MK_RBUTTON)
			: (WM_LBUTTONDOWN, WM_LBUTTONUP, WM_LBUTTONDBLCLK, MK_LBUTTON);
		nint position = (client.Y << 16) | (client.X & 0xFFFF);
		nint handle = target.Handle;
		SendMessage(handle, WM_MOUSEMOVE, 0, position);
		SendMessage(handle, down, flag, position);
		SendMessage(handle, up, 0, position);
		if (clicks >= 2 && !target.IsDisposed)
		{
			SendMessage(handle, doubleClick, flag, position);
			SendMessage(handle, up, 0, position);
		}

		if (right && !target.IsDisposed)
		{
			var screen = target.PointToScreen(client);
			SendMessage(handle, WM_CONTEXTMENU, handle, (screen.Y << 16) | (screen.X & 0xFFFF));
		}
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
		int keyFlags = (modifiers.HasFlag(Keys.Control) ? MK_CONTROL : 0) | (modifiers.HasFlag(Keys.Shift) ? MK_SHIFT : 0);
		var screen = target.PointToScreen(client);
		SendMessage(target.Handle, WM_MOUSEWHEEL, ((notches * WHEEL_DELTA) << 16) | keyFlags, (screen.Y << 16) | (screen.X & 0xFFFF));
	}

	public static Point Center(Control control) => new(control.Width / 2, control.Height / 2);

	/// <summary>
	/// The deepest visible child of <paramref name="control"/> under a client point, as Windows finds the window that gets
	/// the mouse, with the point in its client coordinates.
	/// </summary>
	public static (Control Hit, Point Point) ChildAt(Control control, Point client)
	{
		var hit = control;
		while (hit.GetChildAtPoint(client, GetChildAtPointSkip.Invisible) is { } child)
		{
			client = new Point(client.X - child.Left, client.Y - child.Top);
			hit = child;
		}

		return (hit, client);
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

	private static byte[] WithModifiers(byte[] state, Keys modifiers)
	{
		var copy = (byte[])state.Clone();
		static void Set(byte[] s, bool down, params int[] keys)
		{
			foreach (var key in keys)
				s[key] = down ? (byte)0x80 : (byte)0;
		}

		Set(copy, modifiers.HasFlag(Keys.Control), VK_CONTROL, VK_LCONTROL);
		Set(copy, modifiers.HasFlag(Keys.Shift), VK_SHIFT, VK_LSHIFT);
		Set(copy, modifiers.HasFlag(Keys.Alt), VK_MENU, VK_LMENU);
		Set(copy, false, VK_RCONTROL, VK_RSHIFT, VK_RMENU);
		return copy;
	}

	private sealed class Restore(byte[] previous) : IDisposable
	{
		public void Dispose() => SetKeyboardState(previous);
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

	[LibraryImport("user32.dll")]
	private static partial nint GetFocus();

	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool GetKeyboardState([Out] byte[] state);

	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool SetKeyboardState(byte[] state);

	[LibraryImport("user32.dll", EntryPoint = "PostMessageW")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool PostMessage(nint hWnd, int msg, nint wParam, nint lParam);

	[LibraryImport("user32.dll", EntryPoint = "SendMessageW")]
	private static partial nint SendMessage(nint hWnd, int msg, nint wParam, nint lParam);

	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool TranslateMessage(ref NativeMessage msg);

	[LibraryImport("user32.dll", EntryPoint = "MapVirtualKeyW")]
	private static partial uint MapVirtualKey(uint code, uint mapType);
}
