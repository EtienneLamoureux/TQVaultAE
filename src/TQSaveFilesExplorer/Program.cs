using System;
using System.Text;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TQ.SaveFilesExplorer.Services;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Config;
using TQVaultAE.Data;
using TQVaultAE.Services;
using TQVaultAE.Services.Win32;

namespace TQ.SaveFilesExplorer;

static class Program
{
	/// <summary>
	/// Point d'entrée principal de l'application.
	/// </summary>
	[STAThread]
	static void Main()
	{
		// The save files use Windows-1252 text, which .NET only provides through the code pages provider.
		Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(false);

		using var services = ConfigureServices().BuildServiceProvider();
		Application.Run(services.GetRequiredService<MainForm>());
	}

	/// <summary>
	/// TQVault's own services, for the save folders. The game path is never resolved: the explorer only reads save files.
	/// </summary>
	internal static IServiceCollection ConfigureServices()
		=> new ServiceCollection()
		// Logs: the explorer keeps none
		.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>))
		// Abstractions
		.AddTransient<IFileIO, FileIO>()
		.AddTransient<IPathIO, PathIO>()
		.AddTransient<IDirectoryIO, DirectoryIO>()
		// Config
		.AddSingleton<UserSettings>(_ => UserSettings.Read())
		// Services
		.AddSingleton<ITQDataService, TQDataService>()
		.AddSingleton<IGamePathService, GamePathServiceWin>()
		.AddSingleton<SaveFolderDetector>()
		// Forms
		.AddSingleton<MainForm>();
}
