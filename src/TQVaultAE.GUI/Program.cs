//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI
{
	using System;
	using System.Globalization;
	using System.Reflection;
	using System.Resources;
	using System.Security.Permissions;
	using System.Threading;
	using System.Windows.Forms;
	using TQVaultAE.Logs;

	/// <summary>
	/// Main Program class
	/// </summary>
	public static class Program
	{
		private static readonly log4net.ILog Log = Logger.Get(typeof(Program));

		/// <summary>
		/// Right to left reading options for message boxes
		/// </summary>
		private static MessageBoxOptions rightToLeft;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
		public static void Main()
		{
			try
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
			catch (Exception ex)
			{
				Log.ErrorException(ex);
				throw;
			}
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
				Log.Error("UI Thread Exception", t.Exception);
				result = MessageBox.Show(Log.FormatException(t.Exception), "Windows Forms Error", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Stop, MessageBoxDefaultButton.Button1, rightToLeft);
			}
			catch
			{
				try
				{
					Log.Fatal("Fatal Windows Forms Error", t.Exception);
					MessageBox.Show(Log.FormatException(t.Exception), "Fatal Windows Forms Error", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Stop, MessageBoxDefaultButton.Button1, rightToLeft);
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
				Log.Error("An application error occurred.", ex);
			}
			finally
			{
				Application.Exit();
			}
		}



	}
}