//-----------------------------------------------------------------------
// <copyright file="AutomationHost.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using TQVaultAE.GUI.Forms;

namespace TQVaultAE.GUI.Automation;

/// <summary>
/// Debug-only MCP server hosted inside TQVault, listening on http://127.0.0.1:{port}/mcp (streamable HTTP).
/// </summary>
/// <remarks>
/// Environment variables:
/// <list type="bullet">
/// <item><c>TQVAULT_MCP_PORT</c>: listening port (default <see cref="DefaultPort"/>).</item>
/// <item><c>TQVAULT_MCP_DISABLED=1</c>: do not start the endpoint.</item>
/// </list>
/// </remarks>
internal static class AutomationHost
{
	public const int DefaultPort = 47017;

	private static ILogger? _log;

	/// <summary>Logger of the automation endpoint, once started.</summary>
	internal static ILogger? Log => _log;
	private static WebApplication? _app;
	private static MainForm? _mainForm;

	/// <summary>
	/// Gets the in-memory log buffer served by <c>recent_log</c>.
	/// </summary>
	public static RecentLogProvider RecentLog { get; } = new();

	/// <summary>
	/// Gets the endpoint address, or null when the endpoint is not running.
	/// </summary>
	public static string? Address { get; private set; }

	/// <summary>
	/// Gets the main form once it has been created.
	/// </summary>
	public static MainForm? MainForm => _mainForm;

	/// <summary>
	/// Starts the endpoint in the background. Safe to call before the main form exists,
	/// so that startup dialogs can already be listed and answered.
	/// </summary>
	public static void Start(ILoggerFactory loggerFactory)
	{
		_log = loggerFactory.CreateLogger(typeof(AutomationHost));

		if (Environment.GetEnvironmentVariable("TQVAULT_MCP_DISABLED") == "1")
		{
			_log.LogInformation("Automation MCP endpoint disabled by TQVAULT_MCP_DISABLED");
			return;
		}

		var port = int.TryParse(Environment.GetEnvironmentVariable("TQVAULT_MCP_PORT"), out var p) ? p : DefaultPort;

		_ = Task.Run(() => StartServerAsync(port));
	}

	/// <summary>
	/// Gives the tools access to the main form.
	/// </summary>
	public static void AttachMainForm(MainForm mainForm) => _mainForm = mainForm;

	/// <summary>
	/// Runs <paramref name="action"/> on the UI thread and returns its result.
	/// </summary>
	public static T OnUi<T>(Func<MainForm, T> action)
	{
		var form = _mainForm;
		if (form is null || form.IsDisposed || !form.IsHandleCreated)
			throw new McpException("The TQVault main window is not ready yet. Use list_windows / close_dialog to answer startup dialogs.");

		if (!form.InvokeRequired)
			return action(form);

		return (T)form.Invoke(() => action(form))!;
	}

	private static async Task StartServerAsync(int port)
	{
		// After Application.Restart (character duplication), the new process starts while the old one still holds
		// the port for a moment: retry for a few seconds before giving up.
		const int attempts = 20;
		for (int attempt = 1; ; attempt++)
		{
			WebApplication? app = null;
			try
			{
				var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions
				{
					ContentRootPath = AppContext.BaseDirectory,
					Args = [],
				});

				builder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, port));
				builder.Logging.ClearProviders();
				builder.Logging.SetMinimumLevel(LogLevel.Warning);
				builder.Logging.AddProvider(RecentLog);

				builder.Services
					.AddMcpServer(options => options.ServerInfo = new() { Name = "tqvault", Version = "1.0.0" })
					.WithHttpTransport(options => options.Stateless = true)
					.WithTools([typeof(AutomationTools)]);

				app = builder.Build();
				app.MapMcp("/mcp");

				await app.StartAsync().ConfigureAwait(false);
				_app = app;
				Address = $"http://127.0.0.1:{port}/mcp";
				_log?.LogInformation("Automation MCP endpoint listening on {Address}", Address);
				return;
			}
			catch (IOException ex) when (ex.InnerException is Microsoft.AspNetCore.Connections.AddressInUseException && attempt < attempts)
			{
				if (app is not null)
					await app.DisposeAsync().ConfigureAwait(false);
				await Task.Delay(500).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				_log?.LogError(ex, "Failed to start the automation MCP endpoint on port {Port}", port);
				return;
			}
		}
	}
}
