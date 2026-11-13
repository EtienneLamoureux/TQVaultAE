using System.Diagnostics;
using Microsoft.Extensions.Logging;
using TQVaultAE.Application.Contracts.Services;

namespace TQVaultAE.Services;

/// <inheritdoc cref="IGameProcessService"/>
public class GameProcessService(ILogger<GameProcessService> log) : IGameProcessService
{
	/// <summary>
	/// The process names of the game's executables, without extension: <c>TQ.exe</c> (Anniversary Edition and the
	/// original game) and <c>TQIT.exe</c> (Immortal Throne before the Anniversary Edition).
	/// </summary>
	internal static readonly string[] GameProcessNames = ["TQ", "TQIT"];

	private readonly ILogger Log = log;

	/// <inheritdoc/>
	public bool IsGameRunning()
	{
		try
		{
			return IsGame(Process.GetProcesses().Select(NameOf));
		}
		catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception or NotSupportedException)
		{
			this.Log.LogWarning(ex, "The running processes could not be listed");
			return false;
		}
	}

	/// <summary>True when one of the process names is the game's (case ignored).</summary>
	/// <param name="processNames">Names of running processes, without extension.</param>
	public static bool IsGame(IEnumerable<string> processNames)
		=> processNames.Any(name => GameProcessNames.Contains(name, StringComparer.OrdinalIgnoreCase));

	private static string NameOf(Process process)
	{
		using (process)
		{
			try
			{
				return process.ProcessName;
			}
			catch (InvalidOperationException)
			{
				// The process ended while listed.
				return string.Empty;
			}
		}
	}
}
