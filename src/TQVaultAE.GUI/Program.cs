//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI
{
	using System;
	using System.Drawing;
	using System.Drawing.Text;
	using System.Globalization;
	using System.Reflection;
	using System.Resources;
	using System.Security.Permissions;
	using System.Threading;
	using System.Linq;
	using System.Windows.Forms;
	using TQVaultData;
	using TQVaultAE.GUI.Properties;

	/// <summary>
	/// Main Program class
	/// </summary>
	public static class Program
	{
		/// <summary>
		/// Right to left reading options for message boxes
		/// </summary>
		private static MessageBoxOptions rightToLeft;

		private static PrivateFontCollection privateFontCollection = new PrivateFontCollection();

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
		public static void Main()
		{
			manageCulture();

			// Add the event handler for handling UI thread exceptions to the event.
			Application.ThreadException += new ThreadExceptionEventHandler(MainForm_UIThreadException);

			// Set the unhandled exception mode to force all Windows Forms errors to go through our handler.
			Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

			// Add the event handler for handling non-UI thread exceptions to the event.
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}

		private static void manageCulture()
		{
			if (CultureInfo.CurrentCulture.IsNeutralCulture)
			{
				// Neutral cultures are not supported. Fallback to application's default.
				String assemblyCultureName = ((NeutralResourcesLanguageAttribute)Attribute.GetCustomAttribute(
					Assembly.GetExecutingAssembly(), typeof(NeutralResourcesLanguageAttribute), false))
				   .CultureName;
				Thread.CurrentThread.CurrentCulture = new CultureInfo(assemblyCultureName, true);
			}

			// Set options for Right to Left reading.
			rightToLeft = (MessageBoxOptions)0;
			if (CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft)
			{
				rightToLeft = MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading;
			}
		}

		/// <summary>
		/// Handle the UI exceptions by showing a dialog box, and asking the user whether or not they wish to abort execution.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="t">ThreadExceptionEventArgs data</param>
		private static void MainForm_UIThreadException(object sender, ThreadExceptionEventArgs t)
		{
			DialogResult result = DialogResult.Cancel;
			try
			{
				string errorMsg = string.Format(CultureInfo.InvariantCulture, "Message : {0}\n\nStack Trace:\n{1}", t.Exception.Message, t.Exception.StackTrace);
				TQDebug.DebugEnabled = true;
				TQDebug.DebugWriteLine("UI Thread Exception");
				TQDebug.DebugWriteLine(errorMsg);
				result = MessageBox.Show(errorMsg, "Windows Forms Error", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Stop, MessageBoxDefaultButton.Button1, rightToLeft);
			}
			catch
			{
				try
				{
					TQDebug.DebugEnabled = true;
					TQDebug.DebugWriteLine("Fatal Windows Forms Error");
					TQDebug.DebugWriteLine(string.Format(CultureInfo.InvariantCulture, "Message : {0}\n\nStack Trace:\n{1}", t.Exception.Message, t.Exception.StackTrace));
					MessageBox.Show("Fatal Windows Forms Error", "Fatal Windows Forms Error", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Stop, MessageBoxDefaultButton.Button1, rightToLeft);
				}
				finally
				{
					Application.Exit();
				}
			}

			// Exits the program when the user clicks Abort.
			if (result == DialogResult.Abort)
			{
				Application.Exit();
			}
		}

		/// <summary>
		/// Handle the UI exceptions by showing a dialog box, and asking the user whether or not they wish to abort execution.
		/// </summary>
		/// <remarks>NOTE: This exception cannot be kept from terminating the application - it can only log the event, and inform the user about it.</remarks>
		/// <param name="sender">sender object</param>
		/// <param name="e">UnhandledExceptionEventArgs data</param>
		private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			try
			{
				Exception ex = (Exception)e.ExceptionObject;

				TQDebug.DebugEnabled = true;
				TQDebug.DebugWriteLine("An application error occurred.");
				TQDebug.DebugWriteLine(string.Format(CultureInfo.InvariantCulture, "Message : {0}\n\nStack Trace:\n{1}", ex.Message, ex.StackTrace));
			}
			finally
			{
				Application.Exit();
			}
		}

		#region Font Stuff

		private static FontFamily initCustomFont(byte[] fontData)
		{
			unsafe
			{
				fixed (byte* pinptr = fontData)
				{
					IntPtr ptr = (IntPtr)pinptr;
					privateFontCollection.AddMemoryFont(ptr, fontData.Length);
				}
			}
			return privateFontCollection.Families.Last();
		}

		private static FontFamily _FONT_ALBERTUSMT = null;
		internal static FontFamily FONT_ALBERTUSMT
		{
			get
			{
				if (_FONT_ALBERTUSMT is null) _FONT_ALBERTUSMT = initCustomFont(Properties.Resources.AlbertusMT);
				return _FONT_ALBERTUSMT;
			}
		}

		private static FontFamily _FONT_ALBERTUSMTLIGHT = null;
		internal static FontFamily FONT_ALBERTUSMTLIGHT
		{
			get
			{
				if (_FONT_ALBERTUSMTLIGHT is null) _FONT_ALBERTUSMTLIGHT = initCustomFont(Properties.Resources.AlbertusMTLight);
				return _FONT_ALBERTUSMTLIGHT;
			}
		}

		public static Font GetFontMicrosoftSansSerif(float fontSize, float? scale = null)
		{
			scale = scale ?? 1F;
			return new Font("Microsoft Sans Serif", 8.25F * scale.Value);
		}

		public static Font GetFontAlbertusMT(float fontSize, FontStyle fontStyle, GraphicsUnit unit, byte b)
		{
			return new Font(Program.FONT_ALBERTUSMT, fontSize, fontStyle, unit, b);
		}

		public static Font GetFontAlbertusMT(float fontSize, GraphicsUnit unit)
		{
			return new Font(Program.FONT_ALBERTUSMT, fontSize, unit);
		}

		public static Font GetFontAlbertusMT(float fontSize, float? scale = null)
		{
			scale = scale ?? 1F;
			return new Font(Program.FONT_ALBERTUSMT, fontSize * scale.Value);
		}

		public static Font GetFontAlbertusMTLight(float fontSize, GraphicsUnit unit)
		{
			return new Font(Program.FONT_ALBERTUSMT, fontSize, unit);
		}

		public static Font GetFontAlbertusMTLight(float fontSize, FontStyle fontStyle, GraphicsUnit unit, byte b)
		{
			return new Font(Program.FONT_ALBERTUSMTLIGHT, fontSize, fontStyle, unit, b);
		}

		public static Font GetFontAlbertusMTLight(float fontSize)
		{
			return new Font(Program.FONT_ALBERTUSMTLIGHT, fontSize);
		}

		public static Font GetFontAlbertusMTLight(float fontSize, float? scale = null)
		{
			scale = scale ?? 1F;
			return new Font(Program.FONT_ALBERTUSMTLIGHT, fontSize * scale.Value);
		}

		#endregion

		/// <summary>
		/// Load DB if needed
		/// </summary>
		internal static void LoadDB()
		{
			if (Database.DB is null)
			{
				Database.DB = new Database();
				Database.DB.AutoDetectLanguage = Settings.Default.AutoDetectLanguage;
				Database.DB.TQLanguage = Settings.Default.TQLanguage;
			}
		}

	}
}