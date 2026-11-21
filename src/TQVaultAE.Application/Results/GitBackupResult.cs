namespace TQVaultAE.Application.Results;

/// <summary>
/// The git command of a backup step that failed.
/// </summary>
public enum GitBackupStep
{
	Clone,
	Pull,
	Fetch,
	Push,
}

/// <summary>
/// A git backup step that failed. The UI tells the user (<c>GitBackupText</c> in Presentation); a backup problem never
/// stops the app.
/// </summary>
/// <param name="Step">The step that failed.</param>
/// <param name="RepositoryUrl">The repository of the backup, from the settings.</param>
/// <param name="Exception">What went wrong.</param>
public sealed record GitBackupError(GitBackupStep Step, string RepositoryUrl, Exception Exception);

/// <summary>
/// The commit and push of the git backup.
/// </summary>
/// <param name="Pushed">Whether a change was pushed.</param>
/// <param name="Error">The step that failed, or null.</param>
public sealed record GitPushResult(bool Pushed, GitBackupError? Error = null);
