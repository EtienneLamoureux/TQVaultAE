//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI
{
	using log4net.Core;
	using Microsoft.Extensions.DependencyInjection;
	using System;
	using System.Globalization;
	using System.IO;
	using System.Reflection;
	using System.Resources;
	using System.Security.Permissions;
	using System.Threading;
	using System.Windows.Forms;
	using TQVaultAE.Config;
	using TQVaultAE.Data;
	using TQVaultAE.Domain.Contracts.Providers;
	using TQVaultAE.Domain.Contracts.Services;
	using TQVaultAE.Domain.Entities;
	using TQVaultAE.Domain.Helpers;
	using TQVaultAE.Domain.Exceptions;
	using TQVaultAE.Logs;
	using TQVaultAE.Presentation;
	using TQVaultAE.Services;
	using TQVaultAE.Services.Win32;

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

		internal static ServiceProvider ServiceProvider { get; private set; }

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
		public static void Main()
		{
			try
			{
				// Add the event handler for handling UI thread exceptions to the event.
				Application.ThreadException += new ThreadExceptionEventHandler(MainForm_UIThreadException);

				// Set the unhandled exception mode to force all Windows Forms errors to go through our handler.
				Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

				// Add the event handler for handling non-UI thread exceptions to the event.
				AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);

#if DEBUG
				Logger.ChangeRootLogLevel(Level.Debug);
				//TQDebug.DebugEnabled = true;
#endif

			restart:
				// Configure DI
				var scol = new ServiceCollection()
				// Logs
				.AddSingleton(typeof(ILogger<>), typeof(ILoggerImpl<>))
				// States
				.AddSingleton<SessionContext>()
				// Providers
				.AddTransient<IRecordInfoProvider, RecordInfoProvider>()
				.AddTransient<IArcFileProvider, ArcFileProvider>()
				.AddTransient<IArzFileProvider, ArzFileProvider>()
				.AddSingleton<IDatabase, Database>()
				.AddSingleton<IItemProvider, ItemProvider>()
				.AddTransient<ILootTableCollectionProvider, LootTableCollectionProvider>()
				.AddTransient<IStashProvider, StashProvider>()
				.AddTransient<IPlayerCollectionProvider, PlayerCollectionProvider>()
				.AddTransient<ISackCollectionProvider, SackCollectionProvider>()
				.AddSingleton<IItemAttributeProvider, ItemAttributeProvider>()
				.AddTransient<IDBRecordCollectionProvider, DBRecordCollectionProvider>()
				// Services
				.AddTransient<IAddFontToOS, AddFontToOSWin>()
				.AddSingleton<IGamePathService, GamePathServiceWin>()
				.AddTransient<IPlayerService, PlayerService>()
				.AddTransient<IStashService, StashService>()
				.AddTransient<IVaultService, VaultService>()
				.AddTransient<ISearchService, SearchService>()
				.AddTransient<IFontService, FontService>()
				.AddTransient<ITranslationService, TranslationService>()
				.AddSingleton<IUIService, UIService>()
				.AddSingleton<ITQDataService, TQDataService>()
				.AddTransient<IBitmapService, BitmapService>()
				// Forms
				.AddSingleton<MainForm>()
				.AddTransient<AboutBox>()
				.AddTransient<CharacterEditDialog>()
				.AddTransient<ItemProperties>()
				.AddTransient<ItemSeedDialog>()
				.AddTransient<ResultsDialog>()
				.AddTransient<SearchDialog>()
				.AddTransient<SettingsDialog>()
				.AddTransient<VaultMaintenanceDialog>()
				.AddTransient<SplashScreenForm>();

				Program.ServiceProvider = scol.BuildServiceProvider();

				var gamePathResolver = Program.ServiceProvider.GetService<IGamePathService>();

				try
				{
					ManageCulture();
					SetUILanguage();
					SetupGamePaths(gamePathResolver);
					SetupMapName(gamePathResolver);
				}
				catch (ExGamePathNotFound ex)
				{
					using (var fbd = new FolderBrowserDialog() { Description = ex.Message, ShowNewFolderButton = false })
					{
						DialogResult result = fbd.ShowDialog();

						if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
						{
							Config.Settings.Default.ForceGamePath = fbd.SelectedPath;
							Config.Settings.Default.Save();
							goto restart;
						}
						else goto exit;
					}
				}

				var mainform = Program.ServiceProvider.GetService<MainForm>();
				Application.Run(mainform);
			}
			catch (Exception ex)
			{
				Log.ErrorException(ex);
				MessageBox.Show(Log.FormatException(ex), Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
			}

		exit:;
		}


		#region Init

		/// <summary>
		/// Reads the paths from the config files and sets them.
		/// </summary>
		private static void SetupGamePaths(IGamePathService gamePathResolver)
		{
			if (Config.Settings.Default.AutoDetectGamePath)
			{
				gamePathResolver.TQPath = gamePathResolver.ResolveGamePath();
				gamePathResolver.ImmortalThronePath = gamePathResolver.ResolveGamePath();
			}
			else
			{
				gamePathResolver.TQPath = Config.Settings.Default.TQPath;
				gamePathResolver.ImmortalThronePath = Config.Settings.Default.TQITPath;
			}

			// Show a message that the default path is going to be used.
			if (string.IsNullOrEmpty(Config.Settings.Default.VaultPath))
			{
				string folderPath = Path.Combine(gamePathResolver.TQSaveFolder, "TQVaultData");

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

			gamePathResolver.TQVaultSaveFolder = Config.Settings.Default.VaultPath;
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
		private static void SetupMapName(IGamePathService gamePathResolver)
		{
			// Set the map name.  Command line argument can override this setting in LoadResources().
			string mapName = "main";
			if (Config.Settings.Default.ModEnabled)
				mapName = Config.Settings.Default.CustomMap;

			gamePathResolver.MapName = mapName;
		}

		#endregion
		private static void ManageCulture()
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