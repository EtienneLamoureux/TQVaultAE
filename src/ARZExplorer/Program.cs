//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="VillageIdiot">
//     Copyright (c) Village Idiot. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Config;
using TQVaultAE.Data;
using TQVaultAE.Domain.Exceptions;
using TQVaultAE.Presentation;
using TQVaultAE.Services;
using TQVaultAE.Services.Win32;

namespace ArzExplorer;

/// <summary>
/// Holds the main program.
/// </summary>
public static class Program
{
	internal static IServiceProvider ServiceProvider;
	private static ILoggerFactory LoggerFactory;
	private static ILogger Log;
	/// <summary>
	/// The main entry point for the application.
	/// </summary>
	[STAThread]
	public static void Main()
	{
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(false);

		// Setup regular Microsoft.Extensions.Logging abstraction manualy
		LoggerFactory = new LoggerFactory();
		LoggerFactory.AddLog4Net();
		Log = LoggerFactory.CreateLogger(typeof(Program));// Make static level logger

	restart:;
		// Configure DI
		var scol = new ServiceCollection()
		// Logs
		.AddSingleton(LoggerFactory)// Register factory
		.AddSingleton(typeof(ILogger<>), typeof(Logger<>))
		// Abstractions
		.AddTransient<IFileIO, FileIO>()
		.AddTransient<IPathIO, PathIO>()
		.AddTransient<IDirectoryIO, DirectoryIO>()
		// Config
		.AddSingleton<UserSettings>(sp => UserSettings.Read())
		// Providers
		.AddTransient<IRecordInfoProvider, RecordInfoProvider>()
		.AddTransient<IArcFileProvider, ArcFileProvider>()
		.AddTransient<IArzFileProvider, ArzFileProvider>()
		.AddTransient<IDBRecordCollectionProvider, DBRecordCollectionProvider>()
		// Services
		.AddSingleton<ITQDataService, TQDataService>()
		.AddTransient<IBitmapService, BitmapService>()
		.AddSingleton<IGamePathService, GamePathServiceWin>()
		.AddTransient<IDecompressionService, DeflateDecompressionService>()
		.AddSingleton<IFileDataService, MemoryMappedFileService>()
		// Init SoundServiceWin without IDatabase
		.AddSingleton<SoundServiceWin>(sp => new SoundServiceWin(sp.GetService<ILogger<SoundServiceWin>>(), null, sp.GetService<UserSettings>()))
		// Forms
		.AddSingleton<MainForm>()
		.AddTransient<ExtractProgress>();

		Program.ServiceProvider = scol.BuildServiceProvider();

		var gamePathResolver = Program.ServiceProvider.GetService<IGamePathService>();
		var userSettings = Program.ServiceProvider.GetService<UserSettings>();

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

		var mainform = Program.ServiceProvider.GetService<MainForm>();
		Application.Run(mainform);

	exit:;
	}


	/// <summary>
	/// Reads a value from the registry
	/// </summary>
	/// <param name="key">registry hive to be read</param>
	/// <param name="path">path of the value to be read</param>
	/// <returns>string value of the key and value that was read or empty string if there was a problem.</returns>
	internal static string ReadRegistryKey(Microsoft.Win32.RegistryKey key, string[] path)
	{
		// The last item in the path array is the actual value name.
		int valueKey = path.Length - 1;

		// All other values in the path are keys which will need to be navigated.
		int lastSubKey = path.Length - 2;

		for (int i = 0; i <= lastSubKey; ++i)
		{
			key = key.OpenSubKey(path[i]);
			if (key == null)
			{
				return string.Empty;
			}
		}

		return (string)key.GetValue(path[valueKey]);
	}
}