//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Text.Json;
using TQVaultAE.Data;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Exceptions;
using TQVaultAE.GUI.Dialogs;
using TQVaultAE.GUI.Forms;
using TQVaultAE.Logs;
using TQVaultAE.Presentation.Win32.Contracts;
using TQVaultAE.Presentation.Win32;
using TQVaultAE.Presentation;
using TQVaultAE.Services;
using TQVaultAE.Services.Win32;
using Microsoft.Extensions.Logging;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Results;
using TQVaultAE.Config;
using TQVaultAE.GUI.Infrastructure.Filters;

namespace TQVaultAE.GUI;

/// <summary>
/// Main Program class
/// </summary>
public static class Program
{
	// Set first thing in Main.
	private static ILogger Log = null!;

	/// <summary>
	/// Right to left reading options for message boxes
	/// </summary>
	private static MessageBoxOptions rightToLeft;

	internal static ServiceProvider ServiceProvider { get; private set; } = null!;
	// Registered in the services as ILoggerFactory: keep the interface type.
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1859", Justification = "Registered as ILoggerFactory.")]
	private static ILoggerFactory LoggerFactory = null!;

	/// <summary>
	/// The main entry point for the application.
	/// </summary>
	[STAThread]
	public static void Main()
	{
		var commandLine = Environment.GetCommandLineArgs();

		try
		{
			// Add the event handler for handling UI thread exceptions to the event.
			System.Windows.Forms.Application.ThreadException += new ThreadExceptionEventHandler(MainForm_UIThreadException);

			// Set the unhandled exception mode to force all Windows Forms errors to go through our handler.
			System.Windows.Forms.Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

			// Add the event handler for handling non-UI thread exceptions to the event.
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

			// Sharp on every monitor: the designer layout scales by itself (AutoScaleMode.Dpi), also when a window moves to a
			// monitor of another DPI, and UIService.DpiScale carries the same factor to the panels and drawing built in code
			// (see MainForm.OnDpiChanged).
			System.Windows.Forms.Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
			System.Windows.Forms.Application.EnableVisualStyles();
			System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

			// Setup regular Microsoft.Extensions.Logging abstraction manualy
			LoggerFactory = new LoggerFactory();
			LoggerFactory.AddLog4Net();
			Log = LoggerFactory.CreateLogger(typeof(Program));// Make static level logger

#if DEBUG
			// --headless: TQVault runs on a hidden desktop, driven through the automation endpoint. See documentation/AUTOMATION.md
			if (TQVaultAE.GUI.Automation.Headless.IsRequested(commandLine))
			{
				if (!TQVaultAE.GUI.Automation.Headless.TryAttach())
				{
					// This process only starts the instance that runs on the hidden desktop.
					var headlessError = TQVaultAE.GUI.Automation.Headless.StartOnHiddenDesktop();
					if (headlessError is not null)
					{
						Log.LogError("Headless mode: TQVault could not start on a hidden desktop: {Error}", headlessError);
						Environment.ExitCode = 1;
					}
					return;
				}

				commandLine = TQVaultAE.GUI.Automation.Headless.WithoutSwitch(commandLine);
				Log.LogInformation("Headless mode: running on the hidden desktop {Desktop}", TQVaultAE.GUI.Automation.Headless.DesktopName);
			}

			// In-app MCP automation endpoint, started early so that startup dialogs can be answered. See documentation/AUTOMATION.md
			LoggerFactory.AddProvider(TQVaultAE.GUI.Automation.AutomationHost.RecentLog);
			TQVaultAE.GUI.Automation.AutomationHost.Start(LoggerFactory);
#endif

		restart:
			// Configure DI
			var scol = new ServiceCollection()
				// Logs
				.AddSingleton(LoggerFactory)
				.AddSingleton(typeof(ILogger<>), typeof(Logger<>))
				// Config
				.AddSingleton<UserSettings>(sp => UserSettings.Read())
				.AddSingleton(new JsonSerializerOptions { IncludeFields = true, PropertyNameCaseInsensitive = true, WriteIndented = true })
				// States
				.AddSingleton<SessionContext>()
				// Abstractions
				.AddTransient<IFileIO, FileIO>()
				.AddTransient<IPathIO, PathIO>()
				.AddTransient<IDirectoryIO, DirectoryIO>()
				// Providers
				.AddTransient<IArcFileProvider, ArcFileProvider>()
				.AddTransient<IArzFileProvider, ArzFileProvider>()
				// One database per custom map (mod), the active one answering as IDatabase
				.AddTransient<Database>()
				.AddSingleton<Func<Database>>(sp => () => sp.GetRequiredService<Database>())
				.AddSingleton<DatabaseSelector>()
				.AddSingleton<IDatabase>(sp => sp.GetRequiredService<DatabaseSelector>())
				.AddSingleton<IDatabaseMaps>(sp => sp.GetRequiredService<DatabaseSelector>())
				.AddSingleton<IItemProvider, ItemProvider>()
				.AddSingleton<ILootTableCollectionProvider, LootTableCollectionProvider>()
				.AddTransient<IStashProvider, StashProvider>()
				.AddTransient<IPlayerCollectionProvider, PlayerCollectionProvider>()
				.AddSingleton<ISkillTreeProvider, SkillTreeProvider>()
				.AddTransient<ISackCollectionProvider, SackCollectionProvider>()
				.AddSingleton<IItemAttributeProvider, ItemAttributeProvider>()
				.AddTransient<IDBRecordCollectionProvider, DBRecordCollectionProvider>()
			// Services
			.AddTransient<IAddFontToOS, AddFontToOSWin>()
			.AddSingleton<IGamePathService, GamePathServiceWin>()
			.AddTransient<IPlayerService, PlayerService>()
			.AddTransient<IStashService, StashService>()
			.AddTransient<IVaultService, VaultService>()
			.AddTransient<IFontService, FontService>()
			.AddTransient<ITranslationService, TranslationService>()
			.AddSingleton<IUIService, UIService>()
			.AddSingleton<IUserNotifier>(sp => sp.GetRequiredService<IUIService>())
			.AddSingleton<IIconService, IconService>()
			.AddSingleton<ITQDataService, TQDataService>()
			.AddTransient<IBitmapService, BitmapService>()
#if DEBUG
			// Records the sounds played, for the automation endpoint's recent_sounds tool.
			.AddSingleton<SoundServiceWin>()
			.AddSingleton<ISoundService>(sp => new TQVaultAE.GUI.Automation.RecordingSoundService(
				sp.GetRequiredService<SoundServiceWin>(), sp.GetRequiredService<UserSettings>()))
#else
			.AddSingleton<ISoundService, SoundServiceWin>()
#endif
			.AddTransient<IGameFileService, GameFileService>()
			.AddSingleton<ITagService, TagService>()

		// Application Layer - Core Services
		.AddSingleton<IItemMovementService, ItemMovementService>()
		.AddSingleton<IAffixCatalogService, AffixCatalogService>()
		.AddSingleton<IHighlightService, HighlightService>()
		.AddSingleton<IItemDatabaseService, ItemDatabaseService>()
		.AddSingleton<IItemExchangeService, ItemExchangeService>()
		.AddSingleton<IPasteBinService, PasteBinService>()
		.AddSingleton<IForgeService, ForgeService>()
		.AddSingleton<HttpClient>()
		.AddSingleton<ISettingsPersistenceService, SettingsPersistenceService>()
		.AddSingleton<IStartupService, StartupService>()
		.AddSingleton(new CommandLineArgs(commandLine))
		.AddTransient<ILoadAllService, LoadAllService>()
		.AddSingleton<IItemEditService, ItemEditService>()
		.AddSingleton<IItemStackingService, ItemStackingService>()
		.AddSingleton<IItemSortService, ItemSortService>()
		.AddSingleton<IItemComparisonService, ItemComparisonService>()
		.AddTransient<IItemDropService, ItemDropService>()
		.AddTransient<ISaveOrchestrator, SaveOrchestrator>()
		.AddSingleton<ISearchCategoryService, SearchCategoryService>()
		.AddSingleton<ISearchQueryService, SearchQueryService>()
		.AddSingleton<ICharacterEditService, CharacterEditService>()
		.AddSingleton<ICharacterStatsService, CharacterStatsService>()
		.AddSingleton<IModLinkService, ModLinkService>()
		.AddSingleton<ICharacterReadOnlyService, CharacterReadOnlyService>()
		.AddSingleton<IItemModService, ItemModService>()
		.AddSingleton<IGameMapService, GameMapService>()
		// Caches of game data, emptied when the active custom map changes
		.AddSingleton<IGameDataCache>(sp => (IGameDataCache)sp.GetRequiredService<IItemProvider>())
		.AddSingleton<IGameDataCache>(sp => (IGameDataCache)sp.GetRequiredService<ILootTableCollectionProvider>())
		.AddSingleton<IGameDataCache>(sp => (IGameDataCache)sp.GetRequiredService<ISkillTreeProvider>())
		.AddSingleton<IGameDataCache>(sp => (IGameDataCache)sp.GetRequiredService<IUIService>())
		.AddSingleton<IGameDataCache>(sp => (IGameDataCache)sp.GetRequiredService<ICharacterStatsService>())

				// GUI: services passed to forms and controls, and the only way to create forms
				.AddSingleton<GuiServices>()
				.AddSingleton<IMessageBoxService, MessageBoxService>()
				.AddSingleton<FormFactory>()

				// Forms
				.AddSingleton<MainForm>()
				.AddTransient<AboutBox>()
				.AddSingleton<BagButtonSettings>()
				.AddTransient<CharacterEditDialog>()
				.AddTransient<ItemProperties>()
				.AddTransient<ItemComparisonDialog>()
				.AddTransient<ItemSeedDialog>()
				.AddTransient<ModLinkDialog>()
				.AddTransient<ResultsDialog>()
				.AddTransient<SearchDialogAdvanced>()
				.AddTransient<SettingsDialog>()
				.AddTransient<VaultMaintenanceDialog>()
				.AddTransient<SplashScreenForm>();

			Program.ServiceProvider = scol.BuildServiceProvider();

			var userSettings = ServiceProvider.GetRequiredService<UserSettings>();
			var startupService = ServiceProvider.GetRequiredService<IStartupService>();

			try
			{
				ManageCulture();
				SetUILanguage(startupService.ResolveUICultureName());
				ShowStartupNotice(startupService.SetupGamePaths());
			}
			catch (ExGamePathNotFound ex)
			{
				using (var fbd = new FolderBrowserDialog() { Description = ex.Message, ShowNewFolderButton = false })
				{
					DialogResult result = fbd.ShowDialog();

					if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
					{
						startupService.ApplyChosenGamePath(fbd.SelectedPath);
						userSettings.Save();
						goto restart;
					}
					else goto exit;
				}
			}

			// Initialize the Database AFTER game paths are configured
			var database = ServiceProvider.GetRequiredService<IDatabase>();
			database.LoadDBFile();

			var mainform = Program.ServiceProvider.GetRequiredService<MainForm>();
			var filterMouseWheel = new FormFilterMouseWheelGlobally(mainform);
			var filterMouseButtons = new FormFilterMouseButtonGlobally(mainform);
			System.Windows.Forms.Application.AddMessageFilter(filterMouseWheel);
			System.Windows.Forms.Application.AddMessageFilter(filterMouseButtons);
#if DEBUG
			TQVaultAE.GUI.Automation.AutomationHost.AttachMainForm(mainform);
#endif
			System.Windows.Forms.Application.Run(mainform);
		}
		catch (Exception ex)
		{
			Log.LogError(ex, "TQVault failed to start or stopped on an unhandled error");
#if DEBUG
			// DefaultDesktopOnly boxes are shown by Windows, out of the process' windows: list_windows and close_dialog
			// could not see this one.
			ShowMessageBox(Log.FormatException(ex), Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxService.ReadingOptions);
			TQVaultAE.GUI.Automation.AutomationHost.ExitAfterFatalError();
#else
			ShowMessageBox(Log.FormatException(ex), Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxOptions.DefaultDesktopOnly);
#endif
		}

	exit:;
	}

	#region Init

	/// <summary>
	/// Shows a message box outside the message box service (startup, fatal errors), with its first button as default.
	/// </summary>
	/// <param name="headlessAnswer">Debug: the answer in headless mode when none is queued, instead of the one that changes nothing.</param>
	private static DialogResult ShowMessageBox(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxOptions options, DialogResult? headlessAnswer = null)
	{
#if DEBUG
		// Queued automation answers, and never a box in headless mode.
		if (TQVaultAE.GUI.Automation.AutomationHost.TryTakeDialogAnswer(caption, message, buttons, out var automated, headlessAnswer))
		{
			Log?.LogInformation("Message box \"{Caption}\" answered {Answer} by automation", caption, automated);
			return automated;
		}
#endif
		return MessageBox.Show(message, caption, buttons, icon, MessageBoxDefaultButton.Button1, options);
	}

	/// <summary>
	/// Shows the message about the vault folder chosen while the game paths were set up.
	/// </summary>
	private static void ShowStartupNotice(StartupNotice notice)
	{
		switch (notice)
		{
			case StartupNotice.LegacyVaultShortcut:
				// The vault path is still given by a shortcut: tell to use the configuration UI.
				ShowMessageBox(Resources.DataLinkMsg, Resources.DataLink, MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxService.ReadingOptions);
				break;
			case StartupNotice.DefaultVaultPath:
				ShowMessageBox(Resources.DataDefaultPathMsg, Resources.DataDefaultPath, MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxService.ReadingOptions);
				break;
		}
	}

	/// <summary>
	/// Sets the Current Thread's Culture and UICulture to the language of the settings.
	/// Keeps the OS UI Culture when there is none.
	/// </summary>
	/// <param name="cultureName">The culture found from the settings, or null.</param>
	private static void SetUILanguage(string? cultureName)
	{
		if (string.IsNullOrEmpty(cultureName))
			return;

		try
		{
			// Sets the culture
			Thread.CurrentThread.CurrentCulture = new CultureInfo(cultureName);

			// Sets the UI culture
			Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureName);
		}
		catch (ArgumentNullException e)
		{
			Log.LogError(e, "Argument Null Exception when setting the language");
		}
		catch (NotSupportedException e)
		{
			Log.LogError(e, "Not Supported Exception when setting the language");
		}
	}

	#endregion
	private static void ManageCulture()
	{
		if (CultureInfo.CurrentCulture.IsNeutralCulture)
		{
			// Neutral cultures are not supported. Fallback to application's default.
			String? assemblyCultureName = ((NeutralResourcesLanguageAttribute?)Attribute.GetCustomAttribute(
					Assembly.GetExecutingAssembly(), typeof(NeutralResourcesLanguageAttribute), false))
				?.CultureName ?? "en";
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
	private static void MainForm_UIThreadException(object? sender, ThreadExceptionEventArgs t)
	{
		DialogResult result = DialogResult.Cancel;
		try
		{
			Log.LogError(t.Exception, "UI Thread Exception");
			DialogResult? headlessAnswer = null;
#if DEBUG
			// Headless: an error before the main form has loaded leaves nothing usable, so quit rather than ignore it.
			if (TQVaultAE.GUI.Automation.AutomationHost.MainForm?.AutomationLoadingComplete != true)
				headlessAnswer = DialogResult.Abort;
#endif
			result = ShowMessageBox(Log.FormatException(t.Exception), "Windows Forms Error", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Stop, rightToLeft, headlessAnswer);
		}
		catch
		{
			try
			{
				Log.LogCritical(t.Exception, "Fatal Windows Forms Error");
				ShowMessageBox(Log.FormatException(t.Exception), "Fatal Windows Forms Error", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Stop, rightToLeft);
			}
			finally
			{
				System.Windows.Forms.Application.Exit();
			}
		}

		// Exits the program when the user clicks Abort.
		if (result == DialogResult.Abort)
		{
			System.Windows.Forms.Application.Exit();
		}
	}

	/// <summary>
	/// Handle the UI exceptions by showing a dialog box, and asking the user whether or not they wish to abort execution.
	/// </summary>
	/// <remarks>NOTE: This exception cannot be kept from terminating the application - it can only log the event, and inform the user about it.</remarks>
	/// <param name="sender">sender object</param>
	/// <param name="e">UnhandledExceptionEventArgs data</param>
	private static void CurrentDomain_UnhandledException(object? sender, UnhandledExceptionEventArgs e)
	{
		try
		{
			Exception ex = (Exception)e.ExceptionObject;
			Log.LogError(ex, "An application error occurred.");
		}
		finally
		{
			System.Windows.Forms.Application.Exit();
		}
	}
}