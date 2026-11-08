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
using TQVaultAE.Presentation.Win32.Contracts;
using TQVaultAE.Presentation.Win32;
using TQVaultAE.Presentation;
using TQVaultAE.Services;
using TQVaultAE.Services.Win32;

namespace ArzExplorer;

/// <summary>
/// Holds the main program.
/// </summary>
public static class Program
{
	// Set first thing in Main, before any form is created.
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

		// Setup regular Microsoft.Extensions.Logging abstraction manualy
		ILoggerFactory loggerFactory = new LoggerFactory();
		loggerFactory.AddLog4Net();
#if DEBUG
		Automation.AutomationHost.Start(loggerFactory);
#endif

	restart:;
		// Configure DI
		var scol = new ServiceCollection()
		// Logs
		.AddSingleton(loggerFactory)// Register factory
		.AddSingleton(typeof(ILogger<>), typeof(Logger<>))
		// Abstractions
		.AddTransient<IFileIO, FileIO>()
		.AddTransient<IPathIO, PathIO>()
		.AddTransient<IDirectoryIO, DirectoryIO>()
		// Config
		.AddSingleton<UserSettings>(sp => UserSettings.Read())
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

		Program.ServiceProvider = scol.BuildServiceProvider();

		var gamePathResolver = Program.ServiceProvider.GetRequiredService<IGamePathService>();
		var userSettings = Program.ServiceProvider.GetRequiredService<UserSettings>();

		try
		{
			gamePathResolver.GamePathTQ = gamePathResolver.ResolveGamePath();
			gamePathResolver.GamePathTQIT = gamePathResolver.ResolveGamePath();
		}
		catch (ExGamePathNotFound ex)
		{
			using (var fbd = new FolderBrowserDialog() { Description = ex.Message, ShowNewFolderButton = false })
			{
				DialogResult result = fbd.ShowDialog();

				if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
				{
					userSettings.ForceGamePath = fbd.SelectedPath;
					userSettings.Save();
					goto restart;
				}
				else goto exit;
			}
		}

		var mainform = Program.ServiceProvider.GetRequiredService<MainForm>();
#if DEBUG
		Automation.AutomationHost.AttachMainForm(mainform);
#endif
		Application.Run(mainform);

	exit:;
	}
}