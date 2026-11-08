//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="VillageIdiot">
//     Copyright (c) Village Idiot. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using ArzExplorer.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Config;
using TQVaultAE.Data;
using TQVaultAE.Domain.Exceptions;
using TQVaultAE.Presentation;
using TQVaultAE.Presentation.Win32;
using TQVaultAE.Presentation.Win32.Contracts;
using TQVaultAE.Services;
using TQVaultAE.Services.Win32;

namespace ArzExplorer;

/// <summary>
/// Holds the main program.
/// </summary>
public static class Program
{
	// Set in Main before any form is created.
	internal static IServiceProvider ServiceProvider = null!;

	/// <summary>
	/// The main entry point for the application.
	/// </summary>
	[STAThread]
	public static void Main()
	{
#if DEBUG
		// --headless: start again on a hidden desktop and leave (see Automation\Headless.cs).
		if (Automation.Headless.IsRequested() && !Automation.Headless.TryAttach())
		{
			var headlessError = Automation.Headless.StartOnHiddenDesktop();
			if (headlessError is not null)
				Console.Error.WriteLine($"Headless mode: ARZExplorer could not start on a hidden desktop: {headlessError}");
			return;
		}
#endif

		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(false);

		// Setup regular Microsoft.Extensions.Logging abstraction manually
		ILoggerFactory loggerFactory = new LoggerFactory();
		loggerFactory.AddLog4Net();
#if DEBUG
		Automation.AutomationHost.Start(loggerFactory);
#endif

		// A game folder picked by the user is saved in the settings, then everything is set up again with it.
		while (StartServices(loggerFactory) is { } gamePathError)
		{
			if (!AskGamePath(gamePathError))
				return;
		}

		var mainform = Program.ServiceProvider.GetRequiredService<MainForm>();
#if DEBUG
		Automation.AutomationHost.AttachMainForm(mainform);
#endif
		Application.Run(mainform);
	}

	/// <summary>
	/// Builds the services, then resolves the game path.
	/// </summary>
	/// <returns>Why the game was not found, or null when it was.</returns>
	private static string? StartServices(ILoggerFactory loggerFactory)
	{
		Program.ServiceProvider = ConfigureServices(loggerFactory).BuildServiceProvider();

		var gamePathResolver = Program.ServiceProvider.GetRequiredService<IGamePathService>();
		try
		{
			gamePathResolver.GamePathTQ = gamePathResolver.ResolveGamePath();
			gamePathResolver.GamePathTQIT = gamePathResolver.ResolveGamePath();
			return null;
		}
		catch (ExGamePathNotFound ex)
		{
			return ex.Message;
		}
	}

	/// <summary>
	/// Asks the user for the game folder and saves it in the settings.
	/// </summary>
	/// <param name="reason">Why the game was not found, shown in the dialog.</param>
	/// <returns>Whether the user picked a folder.</returns>
	private static bool AskGamePath(string reason)
	{
		using var fbd = new FolderBrowserDialog { Description = reason, ShowNewFolderButton = false };
		if (fbd.ShowDialog() != DialogResult.OK || string.IsNullOrWhiteSpace(fbd.SelectedPath))
			return false;

		var userSettings = Program.ServiceProvider.GetRequiredService<UserSettings>();
		userSettings.ForceGamePath = fbd.SelectedPath;
		userSettings.Save();
		return true;
	}

	private static IServiceCollection ConfigureServices(ILoggerFactory loggerFactory)
		=> new ServiceCollection()
		// Logs
		.AddSingleton(loggerFactory)// Register factory
		.AddSingleton(typeof(ILogger<>), typeof(Logger<>))
		// Abstractions
		.AddTransient<IFileIO, FileIO>()
		.AddTransient<IPathIO, PathIO>()
		.AddTransient<IDirectoryIO, DirectoryIO>()
		// Config
		.AddSingleton<UserSettings>(_ => UserSettings.Read())
		// Providers
		.AddTransient<IArcFileProvider, ArcFileProvider>()
		.AddTransient<IArzFileProvider, ArzFileProvider>()
		.AddTransient<IDBRecordCollectionProvider, DBRecordCollectionProvider>()
		// Services
		.AddSingleton<ITQDataService, TQDataService>()
		.AddTransient<IBitmapService, BitmapService>()
		.AddSingleton<IGamePathService, GamePathServiceWin>()
		// Init SoundServiceWin without IDatabase
		.AddSingleton<SoundServiceWin>(sp => new SoundServiceWin(sp.GetRequiredService<ILogger<SoundServiceWin>>(), null, sp.GetRequiredService<UserSettings>()))
		// Forms
		.AddTransient<ArchiveExtractor>()
		.AddSingleton<MainForm>()
		.AddTransient<ExtractProgress>();
}
