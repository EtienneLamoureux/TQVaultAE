using System.Diagnostics;
using System.Globalization;
using Microsoft.Extensions.Logging;

namespace TQVaultAE.GUI.Infrastructure;

/// <summary>
/// One rescale of a window (a DPI change, a zoom step) made as one: the window's drawing is frozen until it ends
/// (<see cref="LayoutBatch"/>), then it is drawn once. Each phase is timed, and one Debug line gives them once the
/// window is drawn again (the first time the application is idle after it).
/// </summary>
internal sealed class RescaleScope : IDisposable
{
	private readonly ILogger log;
	private readonly string what;
	private readonly Action ended;
	private readonly LayoutBatch batch;
	private readonly Stopwatch watch = Stopwatch.StartNew();
	private readonly List<(string Name, double Elapsed)> phases = [];
	private double last;

	/// <param name="log">The log the timings go to.</param>
	/// <param name="what">What is rescaled, for the log line ("TQVaultAE: zoom step 6 -> 7").</param>
	/// <param name="window">The window rescaled: its drawing is frozen until the scope ends.</param>
	/// <param name="ended">Called when the scope ends, once the window's drawing is back on.</param>
	public RescaleScope(ILogger log, string what, Control window, Action ended)
	{
		this.log = log;
		this.what = what;
		this.ended = ended;
		this.batch = new LayoutBatch(window);
	}

	/// <summary>Ends the phase named <paramref name="name"/>, started at the end of the one before.</summary>
	public void Phase(string name)
	{
		double now = this.watch.Elapsed.TotalMilliseconds;
		this.phases.Add((name, now - this.last));
		this.last = now;
	}

	/// <summary>
	/// Ends the rescale, even after an exception: the window's drawing comes back on and it is drawn again; the log line
	/// is written once it is.
	/// </summary>
	public void Dispose()
	{
		this.Phase("rest");
		try
		{
			this.batch.Dispose();
		}
		finally
		{
			this.ended();
		}

		double total = this.last;

		void Drawn(object? sender, EventArgs e)
		{
			System.Windows.Forms.Application.Idle -= Drawn;
			double drawn = this.watch.Elapsed.TotalMilliseconds;
			string phases = string.Join(", ", this.phases.Select(p => string.Create(CultureInfo.InvariantCulture, $"{p.Name} {p.Elapsed:F1}")));
			this.log.LogDebug(
				"Rescale {What}: {Phases}; total {Total} ms, drawn {Drawn} ms",
				this.what,
				phases,
				Math.Round(total, 1),
				Math.Round(drawn, 1));
		}

		System.Windows.Forms.Application.Idle += Drawn;
	}
}
