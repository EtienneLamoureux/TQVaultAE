//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI
{
	using System;
	using System.Globalization;
	using System.IO;
	using System.Reflection;
	using System.Resources;
	using System.Security.Permissions;
	using System.Threading;
	using System.Windows.Forms;
	using TQVaultAE.Data;
	using TQVaultAE.GUI.Services;
	using TQVaultAE.Logs;
	using TQVaultAE.Presentation;

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

		public static MainForm MainFormInstance { get; private set; }

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

				SetUILanguage();
				SetupGamePaths();
				SetupMapName();
				FontHelper.FontLoader = new AddFontToOSWin();
				MainFormInstance = new MainForm();

				Application.Run(MainFormInstance);
			}
			catch (Exception ex)
			{
				Log.ErrorException(ex);
				throw;
			}
		}


		#region Init

		/// <summary>
		/// Reads the paths from the config files and sets them.
		/// </summary>
		private static void SetupGamePaths()
		{
			TQData.GamePathResolver = new GamePathResolverWin();

			if (!Config.Settings.Default.AutoDetectGamePath)
			{
				TQData.TQPath = Config.Settings.Default.TQPath;
				TQData.ImmortalThronePath = Config.Settings.Default.TQITPath;
			}

			// Show a message that the default path is going to be used.
			if (string.IsNullOrEmpty(Config.Settings.Default.VaultPath))
			{
				string folderPath = Path.Combine(TQData.TQSaveFolder, "TQVaultData");

				// Check to see if we are still using a shortcut to specify the vault path and display a message
				// to use the configuration UI if we are.
				if (!Directory.Exists(folderPath) && File.Exists(Path.ChangeExtension(folderPath, ".lnk")))
				{
					MessageBox.Show(Resources.DataLinkMsg, Resources.DataLink, MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, VaultForm.RightToLeftOptions);
				}
				else
				{
					MessageBox.Show(Resources.DataDefaultPathMsg, Resources.DataDefaultPath, MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, VaultForm.RightToLeftOptions);
				}
			}

			TQData.TQVaultSaveFolder = Config.Settings.Default.VaultPath;
		}

		/// <summary>
		/// Attempts to read the language from the config file and set the Current Thread's Culture and UICulture.
		/// Defaults to the OS UI Culture.
		/// </summary>
		private static void SetUILanguage()
		{
			string settingsCulture = null;
			if (!string.IsNullOrEmpty(Config.Settings.Default.UILanguage))
			{
				settingsCulture = Config.Settings.Default.UILanguage;
			}
			else if (!Config.Settings.Default.AutoDetectLanguage)
			{
				settingsCulture = Config.Settings.Default.TQLanguage;
			}

			if (!string.IsNullOrEmpty(settingsCulture))
			{
				string myCulture = null;
				foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
				{
					if (ci.EnglishName.Equals(settingsCulture, StringComparison.InvariantCultureIgnoreCase))
					{
						myCulture = ci.TextInfo.CultureName;
						break;
					}
				}

				// We found something so we will use it.
				if (!string.IsNullOrEmpty(myCulture))
				{
					try
					{
						// Sets the culture
						Thread.CurrentThread.CurrentCulture = new CultureInfo(myCulture);

						// Sets the UI culture
						Thread.CurrentThread.CurrentUICulture = new CultureInfo(myCulture);
					}
					catch (ArgumentNullException e)
					{
						Log.Error("Argument Null Exception when setting the language", e);
					}
					catch (NotSupportedException e)
					{
						Log.Error("Not Supported Exception when setting the language", e);
					}
				}

				// If not then we just default to the OS UI culture.
			}
		}

		/// <summary>
		/// Sets the name of the game map if a custom map is set in the config file.
		/// Defaults to Main otherwise.
		/// </summary>
		private static void SetupMapName()
		{
			// Set the map name.  Command line argument can override this setting in LoadResources().
			string mapName = "main";
			if (Config.Settings.Default.ModEnabled)
				mapName = Config.Settings.Default.CustomMap;

			TQData.MapName = mapName;
		}

		#endregion
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