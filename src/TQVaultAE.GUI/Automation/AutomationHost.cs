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


	private static readonly System.Collections.Concurrent.ConcurrentQueue<DialogResult> _dialogAnswers = new();
	private static readonly System.Collections.Concurrent.ConcurrentQueue<string> _dialogAnswered = new();
	private static readonly System.Collections.Concurrent.ConcurrentQueue<string> _dialogUnanswered = new();

	/// <summary>
	/// Queues answers for the next message boxes shown through the message box service.
	/// </summary>
	public static void QueueDialogAnswers(IEnumerable<DialogResult> answers)
	{
		foreach (var answer in answers)
			_dialogAnswers.Enqueue(answer);
	}

	/// <summary>Removes the answers not used yet and the record of the answered boxes.</summary>
	public static void ClearDialogAnswers()
	{
		_dialogAnswers.Clear();
		_dialogAnswered.Clear();
		_dialogUnanswered.Clear();
		Headless.ClearCancelled();
	}

	/// <summary>Gets the answers not used yet.</summary>
	public static IReadOnlyList<DialogResult> PendingDialogAnswers => [.. _dialogAnswers];

	/// <summary>Gets the boxes answered from the queue, oldest first.</summary>
	public static IReadOnlyList<string> AnsweredDialogs => [.. _dialogAnswered];

	/// <summary>Gets the boxes that found no queued answer in headless mode and took their default answer, oldest first.</summary>
	public static IReadOnlyList<string> UnansweredDialogs => [.. _dialogUnanswered];

	/// <summary>
	/// Takes the next queued answer when it is one of <paramref name="buttons"/>; an OK-only box takes only an OK answer,
	/// so an unexpected message does not use up the answer meant for a question.
	/// </summary>
	/// In headless mode a box is never shown: without a queued answer it takes <see cref="HeadlessAnswer"/>, which is recorded
	/// in <see cref="UnansweredDialogs"/>.
	/// </summary>
	/// <returns>Whether the box is answered without being shown.</returns>
	public static bool TryTakeDialogAnswer(string caption, string message, MessageBoxButtons buttons, out DialogResult answer)
	{
		var shortMessage = message.Length > 120 ? message[..120] + "..." : message;
		if (_dialogAnswers.TryPeek(out var next) && ButtonsOf(buttons).Contains(next) && _dialogAnswers.TryDequeue(out answer))
		{
			_dialogAnswered.Enqueue($"{caption}: {shortMessage} -> {answer}");
			return true;
		}

		answer = DialogResult.None;
		if (!Headless.Enabled)
			return false;

		answer = HeadlessAnswer(buttons);
		_dialogUnanswered.Enqueue($"{DateTime.Now:HH:mm:ss} {caption} ({buttons}): {shortMessage} -> {answer}");
		_log?.LogWarning("Headless: message box \"{Caption}\" ({Buttons}) had no queued answer, took {Answer}: {Message}", caption, buttons, answer, message);
		return true;
	}

	/// <summary>
	/// The answer of a box nobody answered in headless mode: the one that changes nothing (Cancel, No, Ignore, OK),
	/// not the default button, which is the first one (Yes) for the service's boxes.
	/// </summary>
	internal static DialogResult HeadlessAnswer(MessageBoxButtons buttons)
	{
		var available = ButtonsOf(buttons);
		foreach (var safe in (DialogResult[])[DialogResult.Cancel, DialogResult.No, DialogResult.Ignore, DialogResult.OK])
		{
			if (available.Contains(safe))
				return safe;
		}
		return available.Length > 0 ? available[0] : DialogResult.Cancel;
	}

	private static DialogResult[] ButtonsOf(MessageBoxButtons buttons) => buttons switch
	{
		MessageBoxButtons.OK => [DialogResult.OK],
		MessageBoxButtons.OKCancel => [DialogResult.OK, DialogResult.Cancel],
		MessageBoxButtons.AbortRetryIgnore => [DialogResult.Abort, DialogResult.Retry, DialogResult.Ignore],
		MessageBoxButtons.YesNoCancel => [DialogResult.Yes, DialogResult.No, DialogResult.Cancel],
		MessageBoxButtons.YesNo => [DialogResult.Yes, DialogResult.No],
		MessageBoxButtons.RetryCancel => [DialogResult.Retry, DialogResult.Cancel],
		MessageBoxButtons.CancelTryContinue => [DialogResult.Cancel, DialogResult.TryAgain, DialogResult.Continue],
		_ => [],
	};

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
