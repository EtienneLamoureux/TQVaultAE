//-----------------------------------------------------------------------
// <copyright file="User32.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI.Models
{
	/// <summary>
	/// Class encapsulating the User messages.
	/// </summary>
	internal sealed class User32
	{
		/// <summary>
		/// Prevents a default instance of the User32 class from being created.
		/// </summary>
		private User32()
			: base()
		{
		}

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

		/// <summary>
		/// Takes a Hi Word and Lo Word and combines into a LParam
		/// </summary>
		/// <param name="lowWord">Low word to be combined</param>
		/// <param name="highWord">High word to be combined.</param>
		/// <returns>IntPtr holding the combined lo Word and Hi Word</returns>
		/*private static IntPtr MakeLongParameter(int lowWord, int highWord)
        {
            return (IntPtr)((highWord << 16) | (lowWord & 0xFFFF));
        }*/
	}
}