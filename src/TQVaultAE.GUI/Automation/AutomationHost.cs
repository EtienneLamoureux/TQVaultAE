//-----------------------------------------------------------------------
// <copyright file="AutomationHost.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.Globalization;
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


	private static readonly System.Collections.Concurrent.ConcurrentQueue<QueuedAnswer> _dialogAnswers = new();
	private static readonly System.Collections.Concurrent.ConcurrentQueue<string> _dialogAnswered = new();
	private static readonly System.Collections.Concurrent.ConcurrentQueue<string> _dialogUnanswered = new();

	/// <summary>
	/// Queues answers for the next message boxes shown through the message box service.
	/// </summary>
	public static void QueueDialogAnswers(IEnumerable<QueuedAnswer> answers)
	{
		foreach (var answer in answers)
			_dialogAnswers.Enqueue(answer);
	}

	/// <summary>Puts an answer before the ones already queued: the answer of the dialog a tool is about to open.</summary>
	public static void QueueDialogAnswerFirst(QueuedAnswer answer)
	{
		var rest = _dialogAnswers.ToList();
		_dialogAnswers.Clear();
		_dialogAnswers.Enqueue(answer);
		foreach (var queued in rest)
			_dialogAnswers.Enqueue(queued);
	}

	/// <summary>Takes back the first queued answer equal to <paramref name="answer"/> (a click refused after queueing it).</summary>
	public static void WithdrawDialogAnswer(QueuedAnswer answer)
	{
		var rest = _dialogAnswers.ToList();
		rest.Remove(answer);
		_dialogAnswers.Clear();
		foreach (var queued in rest)
			_dialogAnswers.Enqueue(queued);
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
	public static IReadOnlyList<QueuedAnswer> PendingDialogAnswers => [.. _dialogAnswers];

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
	/// <param name="headlessAnswer">The answer to take in headless mode without a queued one, instead of <see cref="HeadlessAnswer"/>.</param>
	public static bool TryTakeDialogAnswer(string caption, string message, MessageBoxButtons buttons, out DialogResult answer, DialogResult? headlessAnswer = null)
	{
		var shortMessage = message.Length > 120 ? message[..120] + "..." : message;
		if (_dialogAnswers.TryPeek(out var next) && next.Text is null && ButtonsOf(buttons).Contains(next.Result) && _dialogAnswers.TryDequeue(out _))
		{
			answer = next.Result;
			_dialogAnswered.Enqueue($"{caption}: {shortMessage} -> {answer}");
			return true;
		}

		answer = DialogResult.None;
		if (!Headless.Enabled)
			return false;

		answer = headlessAnswer ?? HeadlessAnswer(buttons);
		_dialogUnanswered.Enqueue($"{DateTime.Now:HH:mm:ss} {caption} ({buttons}): {shortMessage} -> {answer}");
		_log?.LogWarning("Headless: message box \"{Caption}\" ({Buttons}) had no queued answer, took {Answer}: {Message}", caption, buttons, answer, message);
		return true;
	}

	/// <summary>
	/// Takes the next queued answer for a text input dialog (<see cref="Dialogs.InputDialog"/>) when it is a text
	/// (typed, then OK) or Cancel; any other answer stays for the message boxes. The dialog stays shown otherwise, also in
	/// headless mode, where close_dialog or set_control and click_control answer it.
	/// </summary>
	/// <param name="title">The dialog's title, for the record.</param>
	/// <param name="text">The text to type; null for Cancel.</param>
	/// <returns>Whether the dialog is answered from the queue.</returns>
	public static bool TryTakeInputAnswer(string title, out string? text)
	{
		text = null;
		if (!_dialogAnswers.TryPeek(out var next) || (next.Text is null && next.Result != DialogResult.Cancel) || !_dialogAnswers.TryDequeue(out _))
		{
			if (Headless.Enabled)
				_dialogUnanswered.Enqueue($"{DateTime.Now:HH:mm:ss} {title} (text input): no queued text, left open");
			return false;
		}

		text = next.Text;
		_dialogAnswered.Enqueue($"{title} (text input) -> {next}");
		return true;
	}

	/// <summary>Seconds the endpoint stays up after a fatal error, so that dialog_answers and recent_log can report it.</summary>
	private const int FatalErrorGraceSeconds = 10;

	/// <summary>
	/// Ends the process after the fatal error box of <c>Program.Main</c>: when the endpoint runs, it first stays up
	/// <see cref="FatalErrorGraceSeconds"/> seconds so the error can be read (<c>dialog_answers</c>, <c>recent_log</c>).
	/// Exits even when threads are left, so that nothing lingers.
	/// </summary>
	public static void ExitAfterFatalError()
	{
		if (_app is not null)
		{
			_log?.LogWarning("Fatal error: TQVault exits in {Seconds} s", FatalErrorGraceSeconds);
			Thread.Sleep(TimeSpan.FromSeconds(FatalErrorGraceSeconds));
		}
		Environment.Exit(1);
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
	public static void AttachMainForm(MainForm mainForm)
	{
		_mainForm = mainForm;

		// Called on the UI thread, after the UI language is set.
		_uiCulture = CultureInfo.CurrentUICulture;
		_culture = CultureInfo.CurrentCulture;
	}

	private static CultureInfo? _uiCulture;
	private static CultureInfo? _culture;

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

		// Control.Invoke runs the delegate in the caller's ExecutionContext, and the culture flows with it: without
		// this, forms and texts made by a tool would take the request thread's culture instead of the UI language.
		return (T)form.Invoke(() =>
		{
			if (_uiCulture is not null && _culture is not null)
			{
				CultureInfo.CurrentUICulture = _uiCulture;
				CultureInfo.CurrentCulture = _culture;
			}

			return action(form);
		})!;
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
