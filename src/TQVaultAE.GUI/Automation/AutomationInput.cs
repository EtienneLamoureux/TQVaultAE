using System.Runtime.InteropServices;
using TQVaultAE.Application.KeyBinds;
using TQVaultAE.GUI.Interop;

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
	private const int WM_LBUTTONDOWN = 0x0201;
	private const int WM_LBUTTONUP = 0x0202;
	private const int WM_RBUTTONDOWN = 0x0204;
	private const int WM_RBUTTONUP = 0x0205;
	private const int MK_LBUTTON = 0x0001;
	private const int MK_RBUTTON = 0x0002;
	private const int MK_SHIFT = 0x0004;
	private const int MK_CONTROL = 0x0008;

	private const int VK_SHIFT = 0x10, VK_CONTROL = 0x11, VK_MENU = 0x12;
	private const int VK_LSHIFT = 0xA0, VK_RSHIFT = 0xA1, VK_LCONTROL = 0xA2, VK_RCONTROL = 0xA3, VK_LMENU = 0xA4, VK_RMENU = 0xA5;

	/// <summary>The control that has the keyboard focus in this application, if any.</summary>
	public static Control? FocusedControl() => Control.FromChildHandle(GetFocus());

	/// <summary>The Windows Forms modifier keys of the key bind modifiers <paramref name="modifiers"/>.</summary>
	public static Keys ModifierKeys(KeyModifiers modifiers)
		=> (modifiers.HasFlag(KeyModifiers.Ctrl) ? Keys.Control : Keys.None)
		| (modifiers.HasFlag(KeyModifiers.Shift) ? Keys.Shift : Keys.None)
		| (modifiers.HasFlag(KeyModifiers.Alt) ? Keys.Alt : Keys.None);

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
	/// Presses and releases a mouse button at a client location of <paramref name="target"/>, with
	/// <paramref name="modifiers"/> held, through its window procedure. Call on the UI thread.
	/// </summary>
	public static void SendClick(Control target, Point client, MouseButtons button, Keys modifiers)
	{
		using var held = HoldModifiers(modifiers);
		var (down, up, _, buttonFlag) = ButtonMessages(button);
		int keyFlags = KeyFlags(modifiers);
		nint position = User32.MakeLParam(client);
		SendMessage(target.Handle, down, buttonFlag | keyFlags, position);
		SendMessage(target.Handle, up, keyFlags, position);
	}

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
		const int WM_MOUSEMOVE = 0x0200, WM_CONTEXTMENU = 0x007B;
		bool right = button == MouseButtons.Right;
		var (down, up, doubleClick, flag) = ButtonMessages(button);
		nint position = User32.MakeLParam(client);
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
			SendMessage(handle, WM_CONTEXTMENU, handle, User32.MakeLParam(screen));
		}
	}

	/// <summary>The center of <paramref name="control"/> in its client coordinates.</summary>
	public static Point Center(Control control) => new(control.Width / 2, control.Height / 2);

	/// <summary>The center of <paramref name="bounds"/>.</summary>
	public static Point Center(Rectangle bounds) => new(bounds.X + (bounds.Width / 2), bounds.Y + (bounds.Height / 2));

	/// <summary>The mouse event of a single <paramref name="button"/> click at the centre of <paramref name="control"/>.</summary>
	public static MouseEventArgs CenterClick(Control control, MouseButtons button) => new(button, 1, control.Width / 2, control.Height / 2, 0);

	/// <summary>The messages of a mouse button (down, up, double click) and its <c>MK_</c> flag.</summary>
	private static (int Down, int Up, int DoubleClick, int Flag) ButtonMessages(MouseButtons button)
	{
		const int WM_LBUTTONDBLCLK = 0x0203, WM_RBUTTONDBLCLK = 0x0206;
		return button == MouseButtons.Right
			? (WM_RBUTTONDOWN, WM_RBUTTONUP, WM_RBUTTONDBLCLK, MK_RBUTTON)
			: (WM_LBUTTONDOWN, WM_LBUTTONUP, WM_LBUTTONDBLCLK, MK_LBUTTON);
	}

	/// <summary>The <c>MK_</c> flags of the modifiers a mouse message carries (Ctrl and Shift; Alt has none).</summary>
	private static int KeyFlags(Keys modifiers)
		=> (modifiers.HasFlag(Keys.Control) ? MK_CONTROL : 0) | (modifiers.HasFlag(Keys.Shift) ? MK_SHIFT : 0);

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

	[LibraryImport("user32.dll")]
	private static partial nint GetFocus();

	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool GetKeyboardState([Out] byte[] state);

	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool SetKeyboardState(byte[] state);

	private static nint SendMessage(nint hWnd, int msg, nint wParam, nint lParam) => User32.SendMessage(hWnd, msg, wParam, lParam);
}
