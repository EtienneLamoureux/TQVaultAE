//-----------------------------------------------------------------------
// <copyright file="User32.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.Runtime.InteropServices;

namespace TQVaultAE.GUI.Interop;

/// <summary>
/// The user32.dll functions the application calls, with the window messages and values they take. Each function is
/// declared here once; the declarations used only by the Debug automation endpoint stay in <c>Automation/</c>.
/// </summary>
internal static partial class User32
{
	/// <summary>Turns a window's drawing off (wParam 0) or back on (wParam 1) (WM_SETREDRAW).</summary>
	public const int WM_SETREDRAW = 0x000B;

	/// <summary>Gives a control the font to draw its text with; lParam 1 redraws it (WM_SETFONT).</summary>
	public const int WM_SETFONT = 0x0030;

	/// <summary>
	/// Enumeration of Window Messages that this form handles.
	/// </summary>
	public enum WindowMessage
	{
		/// <summary>
		/// NULL window message
		/// </summary>
		Null = 0x0,

		/// <summary>
		/// Asks the window its maximized size and position, and its minimum and maximum tracking sizes (WM_GETMINMAXINFO).
		/// </summary>
		GetMinMaxInfo = 0x24,

		/// <summary>
		/// Non Client Hit Test Window Message
		/// </summary>
		NonClientHitTest = 0x84,

		/// <summary>
		/// Non Client Activate Window Message
		/// </summary>
		NonClientActivate = 0x86,

		/// <summary>
		/// System Command Window Message
		/// </summary>
		SysCommand = 0x112,

		/// <summary>
		/// Enter Menu Loop Window Message
		/// </summary>
		EnterMenuLoop = 0x211,

		/// <summary>
		/// Exit Menu Loop Window Message
		/// </summary>
		ExitMenuLoop = 0x212,

		/// <summary>
		/// Get System Menu Window Message
		/// </summary>
		/// <remarks>Undocumented message</remarks>
		GetSysMenu = 0x313
	}

	/// <summary>
	/// Enumeration of the system commands
	/// </summary>
	public enum SystemMenuCommand
	{
		/// <summary>
		/// Sizes the window.
		/// </summary>
		Size = 0xF000,

		/// <summary>
		/// Moves the window.
		/// </summary>
		Move = 0xF010,

		/// <summary>
		/// Minimizes the window.
		/// </summary>
		Minimize = 0xF020,

		/// <summary>
		/// Maximizes the window.
		/// </summary>
		Maximize = 0xF030,

		/// <summary>
		/// Moves to the next window.
		/// </summary>
		NextWindow = 0xF040,

		/// <summary>
		/// Moves to the previous window.
		/// </summary>
		PreviousWindow = 0xF050,

		/// <summary>
		/// Closes the window.
		/// </summary>
		Close = 0xF060,

		/// <summary>
		/// Scrolls the window vertically.
		/// </summary>
		VerticalScroll = 0xF070,

		/// <summary>
		/// Scrolls the window horizontally.
		/// </summary>
		HorizontalScroll = 0xF080,

		/// <summary>
		/// Retrieves the window menu as a result of a mouse click.
		/// </summary>
		MouseMenu = 0xF090,

		/// <summary>
		/// Retrieves the window menu as a result of a keystroke.
		/// </summary>
		KeyMenu = 0xF100,

		/// <summary>
		/// Arranges the windows.
		/// </summary>
		Arrange = 0xF110,

		/// <summary>
		/// Restores the window.
		/// </summary>
		Restore = 0xF120,

		/// <summary>
		/// Activates the Start Menu.
		/// </summary>
		TaskList = 0xF130,

		/// <summary>
		/// Activates the screen saver.
		/// </summary>
		ScreenSaver = 0xF140,

		/// <summary>
		/// Activates the window associated with the application-specified hot key.
		/// </summary>
		HotKey = 0xF150,

		/// <summary>
		/// Selects the default item; the user double-clicked the window menu.
		/// </summary>
		Default = 0xF160,

		/// <summary>
		/// Sets the state of the display.
		/// </summary>
		MonitorPower = 0xF170,

		/// <summary>
		/// Changes the cursor to a question mark with a pointer.
		/// </summary>
		ContextHelp = 0xF180,

		/// <summary>
		/// Separator system command.
		/// </summary>
		Separator = 0xF00F
	}

	/// <summary>
	/// Enumerator of the Non Client Hit Test Results.
	/// </summary>
	public enum NonClientHitTestResult
	{
		/// <summary>
		/// Hit Test Error
		/// </summary>
		Error = (-2),

		/// <summary>
		/// Hit Test Transparent
		/// </summary>
		Transparent,

		/// <summary>
		/// Hit Test Nowhere
		/// </summary>
		Nowhere,

		/// <summary>
		/// Hit Test Client
		/// </summary>
		Client,

		/// <summary>
		/// Hit Test Caption
		/// </summary>
		Caption,

		/// <summary>
		/// Hit Test System Menu
		/// </summary>
		SystemMenu,

		/// <summary>
		/// Hit Test Grow Box
		/// </summary>
		GrowBox,

		/// <summary>
		/// Hit Test Menu
		/// </summary>
		Menu,

		/// <summary>
		/// Hit Test Horizontal Scroll Bar
		/// </summary>
		HorizontalScroll,

		/// <summary>
		/// Hit Test Vertical Scroll Bar
		/// </summary>
		VerticalScroll,

		/// <summary>
		/// Hit Test Minimize Button
		/// </summary>
		MinimizeButton,

		/// <summary>
		/// Hit Test Maximize Button
		/// </summary>
		MaximizeButton,

		/// <summary>
		/// Hit Test Left border
		/// </summary>
		LeftBorder,

		/// <summary>
		/// Hit Test Right Border
		/// </summary>
		RightBorder,

		/// <summary>
		/// Hit Test Top Border
		/// </summary>
		TopBorder,

		/// <summary>
		/// Hit Test Top Left Corner
		/// </summary>
		TopLeftCorner,

		/// <summary>
		/// Hit Test Top Right Corner
		/// </summary>
		TopRightCorner,

		/// <summary>
		/// Hit Test Bottom Border
		/// </summary>
		BottomBorder,

		/// <summary>
		/// Hit Test Bottom Left Corner
		/// </summary>
		BottomLeftCorner,

		/// <summary>
		/// Hit Test Bottom Right Corner
		/// </summary>
		BottomRightCorner,

		/// <summary>
		/// Hit Test Border
		/// </summary>
		Border,

		/// <summary>
		/// Hit Test Object
		/// </summary>
		Object,

		/// <summary>
		/// Hit Test Close button
		/// </summary>
		CloseButton,

		/// <summary>
		/// Hit Test Help
		/// </summary>
		Help
	}

	/// <summary>
	/// Enumeration of the non client mouse messages.
	/// </summary>
	public enum NonClientMouseMessage
	{
		/// <summary>
		/// Mouse Move Message
		/// </summary>
		MouseMove = 0xA0,

		/// <summary>
		/// Mouse Left Button Down Message
		/// </summary>
		LeftButtonDown = 0xA1,

		/// <summary>
		/// Mouse Left Button Up Message
		/// </summary>
		LeftButtonUp = 0xA2,

		/// <summary>
		/// Mouse Left Button Double Click Message
		/// </summary>
		LeftButtonDoubleClick = 0xA3,

		/// <summary>
		/// Mouse Right Button Down Message
		/// </summary>
		RightButtonDown = 0xA4,

		/// <summary>
		/// Mouse Right Button Up Message
		/// </summary>
		RightButtonUp = 0xA5,

		/// <summary>
		/// Mouse Right Button Double Click Message
		/// </summary>
		RightButtonDoubleClick = 0xA6,

		/// <summary>
		/// Mouse Middle Button Down Message
		/// </summary>
		MiddleButtonDown = 0xA7,

		/// <summary>
		/// Mouse Middle Button Up Message
		/// </summary>
		MiddleButtonUp = 0xA8,

		/// <summary>
		/// Mouse Middle Button Double Click Message
		/// </summary>
		MiddleButtonDoubleClick = 0xA9,

		/// <summary>
		/// Mouse X Button Down Message
		/// </summary>
		XButtonDown = 0xAB,

		/// <summary>
		/// Mouse X Button Up Message
		/// </summary>
		XButtonUp = 0xAC,

		/// <summary>
		/// Mouse X Button Double Click Message
		/// </summary>
		XButtonDoubleClick = 0xAD
	}

	/// <summary>A point packed in a message parameter as Windows packs the mouse position (MAKELPARAM: x low, y high).</summary>
	public static IntPtr MakeLParam(Point point) => (point.Y << 16) | (point.X & 0xFFFF);

	/// <summary>MonitorFromPoint: the monitor nearest to the point when none contains it.</summary>
	public const uint MONITOR_DEFAULTTONEAREST = 2;

	/// <summary>A point as Windows passes it (POINT).</summary>
	[StructLayout(LayoutKind.Sequential)]
	public readonly record struct NativePoint(int X, int Y);

	/// <summary>
	/// ICONINFO. <c>fIcon</c> is a Win32 BOOL kept as an <see cref="int"/> so the struct stays blittable
	/// for the source-generated P/Invokes.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct IconInfo
	{
		public int fIcon;
		public int xHotspot;
		public int yHotspot;
		public IntPtr hbmMask;
		public IntPtr hbmColor;
	}

	/// <summary>Sends <paramref name="msg"/> to a window and waits for its window procedure.</summary>
	[LibraryImport("user32.dll", EntryPoint = "SendMessageW")]
	public static partial IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

	[LibraryImport("user32.dll")]
	public static partial IntPtr MonitorFromPoint(NativePoint point, uint flags);

	[LibraryImport("user32.dll")]
	public static partial uint GetDpiForSystem();

	[LibraryImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static partial bool GetIconInfo(IntPtr hIcon, out IconInfo pIconInfo);

	[LibraryImport("user32.dll", SetLastError = true)]
	public static partial IntPtr CreateIconIndirect(ref IconInfo icon);

	[LibraryImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static partial bool DestroyIcon(IntPtr hIcon);

	[LibraryImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static partial bool DestroyCursor(IntPtr hCursor);
}