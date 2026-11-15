//-----------------------------------------------------------------------
// <copyright file="RecentLogProvider.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using Microsoft.Extensions.Logging;

namespace TQVaultAE.GUI.Automation;

/// <summary>
/// Keeps the last log lines in memory so that the automation endpoint can return them.
/// </summary>
internal sealed class RecentLogProvider : ILoggerProvider
{
	private readonly object _sync = new();
	private readonly Queue<string> _lines = new();
	private readonly int _capacity;

	public RecentLogProvider(int capacity = 2000) => this._capacity = capacity;

	/// <summary>
	/// Returns the last <paramref name="count"/> lines, oldest first.
	/// </summary>
	public IReadOnlyList<string> GetLast(int count)
	{
		lock (this._sync)
			return this._lines.Skip(Math.Max(0, this._lines.Count - count)).ToList();
	}

	public void Add(string line)
	{
		lock (this._sync)
		{
			this._lines.Enqueue(line);
			while (this._lines.Count > this._capacity)
				this._lines.Dequeue();
		}
	}

	public ILogger CreateLogger(string categoryName) => new RecentLogger(this, categoryName);

	public void Dispose()
	{
	}

	private sealed class RecentLogger(RecentLogProvider owner, string category) : ILogger
	{
		public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

		public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
		{
			var line = $"{DateTime.Now:HH:mm:ss.fff} {logLevel,-11} {category} - {formatter(state, exception)}";
			if (exception is not null)
				line += Environment.NewLine + exception;

			owner.Add(line);
		}
	}
}
