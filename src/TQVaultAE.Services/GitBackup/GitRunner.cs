using System.Diagnostics;
using System.Text;

namespace TQVaultAE.Services.GitBackup;

/// <summary>
/// The result of a git command.
/// </summary>
/// <param name="ExitCode">Exit code of git.</param>
/// <param name="Output">Standard output, as git wrote it.</param>
/// <param name="Error">Standard error lines, joined by new lines.</param>
public sealed record GitResult(int ExitCode, string Output, string Error)
{
	public bool Success => this.ExitCode == 0;
}

/// <summary>
/// A git command that failed.
/// </summary>
public sealed class GitCommandException(string command, string error)
	: Exception($"git {command}{Environment.NewLine}{error}")
{
	/// <summary>
	/// The git sub command that failed, such as <c>clone</c>, <c>fetch</c> or <c>push</c>.
	/// </summary>
	public string Command { get; } = command.Split(' ')[0];

	/// <summary>
	/// What git wrote on its standard error.
	/// </summary>
	public string Error { get; } = error;
}

/// <summary>
/// Runs git hidden, with the options the backup needs whatever the user's git configuration:
/// files are stored byte for byte (no line ending conversion, the saves are binary), paths are printed as they are,
/// and git never waits for a password typed in a console nobody sees.
/// </summary>
public sealed class GitRunner(string workingDirectory)
{
	private static readonly string[] FixedOptions =
	[
		"-c", "core.autocrlf=false",
		"-c", "core.safecrlf=false",
		"-c", "core.quotepath=false",
	];

	/// <summary>
	/// Runs git with <paramref name="arguments"/>.
	/// </summary>
	/// <exception cref="System.ComponentModel.Win32Exception">git cannot be started, for example when it is not installed.</exception>
	public GitResult Run(params string[] arguments)
	{
		var startInfo = new ProcessStartInfo("git")
		{
			UseShellExecute = false,
			CreateNoWindow = true,
			RedirectStandardInput = true,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			StandardOutputEncoding = Encoding.UTF8,
			StandardErrorEncoding = Encoding.UTF8,
			WorkingDirectory = workingDirectory,
		};
		startInfo.Environment["GIT_TERMINAL_PROMPT"] = "0";
		foreach (var option in FixedOptions)
			startInfo.ArgumentList.Add(option);
		foreach (var argument in arguments)
			startInfo.ArgumentList.Add(argument);

		using var process = new Process { StartInfo = startInfo };
		var errorLines = new List<string>();
		process.ErrorDataReceived += (_, e) => { if (e.Data is not null) lock (errorLines) errorLines.Add(e.Data); };
		process.Start();
		process.StandardInput.Close();
		process.BeginErrorReadLine();
		var output = process.StandardOutput.ReadToEnd();
		// Without a timeout, WaitForExit also waits for the end of the redirected error stream.
		process.WaitForExit();
		lock (errorLines)
			return new GitResult(process.ExitCode, output, string.Join(Environment.NewLine, errorLines));
	}

	/// <summary>
	/// Runs git with <paramref name="arguments"/> and throws when it fails.
	/// </summary>
	/// <exception cref="GitCommandException">git returned an error.</exception>
	public GitResult RunChecked(params string[] arguments)
	{
		var result = this.Run(arguments);
		if (!result.Success)
			throw new GitCommandException(string.Join(' ', arguments), result.Error);
		return result;
	}
}
