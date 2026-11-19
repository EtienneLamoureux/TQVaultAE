using System.Net;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using TQVaultAE.Automation;

namespace ArzExplorer.Automation;

/// <summary>
/// Debug-only MCP server hosted inside ARZExplorer, listening on http://127.0.0.1:{port}/mcp (streamable HTTP).
/// </summary>
/// <remarks>
/// Environment variables:
/// <list type="bullet">
/// <item><c>ARZEXPLORER_MCP_PORT</c>: listening port (default <see cref="DefaultPort"/>).</item>
/// <item><c>ARZEXPLORER_MCP_DISABLED=1</c>: do not start the endpoint.</item>
/// </list>
/// </remarks>
internal static class AutomationHost
{
	// TQVault uses 47017, and 47018 for a second instance (documentation/AUTOMATION.md).
	public const int DefaultPort = 47020;

	private static ILogger? _log;
	private static MainForm? _mainForm;

	/// <summary>Gets the endpoint address, or null when the endpoint is not running.</summary>
	public static string? Address { get; private set; }

	/// <summary>Starts the endpoint in the background.</summary>
	public static void Start(ILoggerFactory loggerFactory)
	{
		_log = loggerFactory.CreateLogger(typeof(AutomationHost));

		if (Environment.GetEnvironmentVariable("ARZEXPLORER_MCP_DISABLED") == "1")
			return;

		var port = int.TryParse(Environment.GetEnvironmentVariable("ARZEXPLORER_MCP_PORT"), out var p) ? p : DefaultPort;
		_ = Task.Run(() => StartServerAsync(port));
	}

	/// <summary>Gives the tools access to the main form; in headless mode, prepares it for the hidden desktop.</summary>
	public static void AttachMainForm(MainForm mainForm)
	{
		_mainForm = mainForm;
		if (HeadlessDesktop.Enabled)
			mainForm.DeferSplitterLayoutUntilShown();
	}

	private static MainForm Form
	{
		get
		{
			var form = _mainForm;
			if (form is null || form.IsDisposed || !form.IsHandleCreated)
				throw new McpException("The ARZExplorer main window is not ready yet.");
			return form;
		}
	}

	/// <summary>
	/// Runs <paramref name="action"/> on the UI thread and returns its result, or throws when the UI thread does not
	/// answer within <paramref name="timeout"/> (the action then still runs once the UI thread is free).
	/// </summary>
	public static T OnUi<T>(Func<MainForm, T> action, TimeSpan? timeout = null)
	{
		try
		{
			return UiThread.Run(Form, action, timeout ?? TimeSpan.FromSeconds(30));
		}
		catch (UiThreadHangException ex)
		{
			throw new McpException(ex.Message, ex);
		}
	}

	private static async Task StartServerAsync(int port)
	{
		try
		{
			await McpEndpoint.StartAsync("arzexplorer", new IPEndPoint(IPAddress.Loopback, port)
				, [typeof(AutomationTools), typeof(FeatureTools), typeof(DiagnosticTools)]).ConfigureAwait(false);
			Address = McpEndpoint.AddressOf(port);
			_log?.LogInformation("Automation MCP endpoint listening on {Address}", Address);
		}
		catch (Exception ex)
		{
			_log?.LogError(ex, "Failed to start the automation MCP endpoint on port {Port}", port);
		}
	}
}
