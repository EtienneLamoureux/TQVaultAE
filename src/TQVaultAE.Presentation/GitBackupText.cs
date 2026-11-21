using System.ComponentModel;
using System.Globalization;
using TQVaultAE.Application.Results;

namespace TQVaultAE.Presentation;

/// <summary>
/// Texts of the git backup.
/// </summary>
public static class GitBackupText
{
	/// <summary>
	/// The message of a failed backup step: what could not be done with the repository, then git's own message.
	/// </summary>
	/// <param name="error">The step that failed.</param>
	public static string Message(GitBackupError error)
	{
		var format = error.Step switch
		{
			GitBackupStep.Clone => Resources.GitUnableToClone,
			GitBackupStep.Pull => Resources.GitUnableToPull,
			GitBackupStep.Fetch => Resources.GitUnableToFetch,
			_ => Resources.GitUnableToPush,
		};
		var detail = error.Exception is Win32Exception
			? $"git: {error.Exception.Message}" // git is not installed, or not on the PATH.
			: error.Exception.Message;
		return string.Format(CultureInfo.CurrentCulture, format, error.RepositoryUrl) + Environment.NewLine + detail;
	}
}
