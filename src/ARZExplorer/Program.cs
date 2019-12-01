//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="VillageIdiot">
//     Copyright (c) Village Idiot. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace ArzExplorer
{
	using Microsoft.Extensions.DependencyInjection;
	using System;
	using System.Windows.Forms;
	using TQVaultAE.Config;
	using TQVaultAE.Data;
	using TQVaultAE.Domain.Contracts.Providers;
	using TQVaultAE.Domain.Contracts.Services;
	using TQVaultAE.Domain.Exceptions;
	using TQVaultAE.Logs;
	using TQVaultAE.Presentation;
	using TQVaultAE.Services.Win32;

	/// <summary>
	/// Holds the main program.
	/// </summary>
	public static class Program
	{
		internal static IServiceProvider ServiceProvider;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		public static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

		restart:;
			// Configure DI
			var scol = new ServiceCollection()
			// Logs
			.AddSingleton(typeof(ILogger<>), typeof(ILoggerImpl<>))
			// Providers
			.AddTransient<IRecordInfoProvider, RecordInfoProvider>()
			.AddTransient<IArcFileProvider, ArcFileProvider>()
			.AddTransient<IArzFileProvider, ArzFileProvider>()
			.AddTransient<IDBRecordCollectionProvider, DBRecordCollectionProvider>()
			// Services
			.AddSingleton<ITQDataService, TQDataService>()
			.AddTransient<IBitmapService, BitmapService>()
			.AddSingleton<IGamePathService, GamePathServiceWin>()
			// Forms
			.AddSingleton<MainForm>()
			.AddTransient<ExtractProgress>();

			Program.ServiceProvider = scol.BuildServiceProvider();

			var gamePathResolver = Program.ServiceProvider.GetService<IGamePathService>();

			try
			{
				gamePathResolver.TQPath = gamePathResolver.ResolveGamePath();
				gamePathResolver.ImmortalThronePath = gamePathResolver.ResolveGamePath();
			}
			catch (ExGamePathNotFound ex)
			{
				using (var fbd = new FolderBrowserDialog() { Description = ex.Message, ShowNewFolderButton = false })
				{
					DialogResult result = fbd.ShowDialog();

					if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
					{
						Settings.Default.ForceGamePath = fbd.SelectedPath;
						Settings.Default.Save();
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
}