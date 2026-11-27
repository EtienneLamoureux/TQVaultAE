using Microsoft.Extensions.Logging;
using Timer = System.Windows.Forms.Timer;

namespace TQVaultAE.GUI.Infrastructure;

/// <summary>
/// Coalesces <see cref="FileSystemWatcher"/> change notifications for game save files and reloads each file once
/// the game has stopped writing it.
/// </summary>
/// <remarks>
/// The game writes several files on one save (player, player stash, caravan stash), and each file may raise several
/// events while it is written. A reload is scheduled per file and pushed back on every new event, so it runs once,
/// after <see cref="QuietPeriod"/> without events and once no other process holds the file open.
/// Watchers stay enabled the whole time, so a write that lands during a reload schedules a new one instead of being lost.
/// Runs on the UI thread (Windows Forms timers), so reloads never overlap and never block the UI with sleeps.
/// </remarks>
internal sealed class FileReloadDebouncer : IDisposable
{
	/// <summary>Time without change events before a file is considered fully written.</summary>
	public static readonly TimeSpan QuietPeriod = TimeSpan.FromMilliseconds(500);

	/// <summary>Reload attempts per file while it stays locked or unreadable, before giving up.</summary>
	public const int MaxAttempts = 10;

	private sealed class Pending
	{
		public required Timer Timer;
		public required Action Reload;
		public int Attempts;

		/// <summary>Bumped on every <see cref="Schedule"/>, to tell whether an event arrived during a reload.</summary>
		public int Version;
	}

	private readonly Dictionary<string, Pending> _pending = new(StringComparer.OrdinalIgnoreCase);
	private readonly ILogger _log;

	public FileReloadDebouncer(ILogger log)
		=> this._log = log;

	/// <summary>
	/// Schedules <paramref name="reload"/> for <paramref name="filePath"/>, replacing any reload already pending for it.
	/// </summary>
	public void Schedule(string filePath, Action reload)
	{
		if (this._pending.TryGetValue(filePath, out var pending)) {
			pending.Reload = reload;
			pending.Attempts = 0;
			pending.Version++;
			pending.Timer.Stop();
			pending.Timer.Start();
			return;
		}

		var timer = new Timer { Interval = (int)QuietPeriod.TotalMilliseconds };
		pending = new Pending { Timer = timer, Reload = reload };
		timer.Tick += (_, _) => this.Run(filePath, pending);
		this._pending.Add(filePath, pending);
		timer.Start();
	}

	private void Run(string filePath, Pending pending)
	{
		pending.Timer.Stop();
		pending.Attempts++;

		if (IsLockedByAnotherProcess(filePath)) {
			this.RetryOrGiveUp(filePath, pending, null);
			return;
		}

		int version = pending.Version;
		try {
			pending.Reload();
		}
		catch (IOException ex) {
			if (pending.Version == version)
				this.RetryOrGiveUp(filePath, pending, ex);
			return;
		}

		// A change event during the reload (e.g. while a message box was open) has already restarted the timer.
		if (pending.Version == version)
			this.Remove(filePath);
	}

	private void RetryOrGiveUp(string filePath, Pending pending, IOException? error)
	{
		if (pending.Attempts < MaxAttempts) {
			this._log.LogDebug(error, "Hot reload of {File} postponed (attempt {Attempt}/{Max})", filePath, pending.Attempts, MaxAttempts);
			pending.Timer.Start();
			return;
		}

		this._log.LogError(error, "Hot reload of {File} abandoned after {Attempts} attempts", filePath, pending.Attempts);
		this.Remove(filePath);
	}

	private void Remove(string filePath)
	{
		if (this._pending.Remove(filePath, out var pending))
			pending.Timer.Dispose();
	}

	/// <summary>
	/// <c>true</c> while another process (the game) still has <paramref name="filePath"/> open.
	/// </summary>
	private static bool IsLockedByAnotherProcess(string filePath)
	{
		try {
			// An exclusive open only succeeds once every other handle is closed.
			using var probe = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
			return false;
		}
		catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException) {
			// Nothing to wait for; the loader reports the missing file itself.
			return false;
		}
		catch (IOException) {
			return true;
		}
		catch (UnauthorizedAccessException) {
			return false;
		}
	}

	public void Dispose()
	{
		foreach (var pending in this._pending.Values)
			pending.Timer.Dispose();
		this._pending.Clear();
	}
}
