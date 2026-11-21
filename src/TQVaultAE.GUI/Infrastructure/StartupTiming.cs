using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace TQVaultAE.GUI.Infrastructure;

/// <summary>
/// Debug: logs the startup steps with the time since the process started, to measure the startup.
/// </summary>
internal static class StartupTiming
{
	/// <summary>
	/// Logs a startup step with the milliseconds since the process started.
	/// </summary>
	[Conditional("DEBUG")]
	public static void Mark(ILogger log, string step)
	{
		using var process = Process.GetCurrentProcess();
		var elapsed = DateTime.Now - process.StartTime;
		log.LogInformation("Startup timing: {Step} at {Elapsed} ms", step, (int)elapsed.TotalMilliseconds);
	}

	/// <summary>
	/// Logs a startup step once the message loop is idle: the messages posted before it, paints included, are done.
	/// </summary>
	[Conditional("DEBUG")]
	public static void MarkWhenIdle(ILogger log, string step)
	{
		EventHandler? idle = null;
		idle = (_, _) =>
		{
			System.Windows.Forms.Application.Idle -= idle;
			Mark(log, step);
		};
		System.Windows.Forms.Application.Idle += idle;
	}
}
