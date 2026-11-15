using System.Text.RegularExpressions;

namespace TQVaultAE.Services.GitBackup;

/// <summary>
/// Validates the url of the git repository used for the vault backup.
/// </summary>
public static partial class GitRepositoryUrl
{
	[GeneratedRegex(@"((git|ssh|http(s)?)|(git@[\w\.]+))(:(//)?)([\w\.@\:/\-~]+)(\.git)(/)?")]
	private static partial Regex GitUrlRegex();

	/// <summary>
	/// Tells if <paramref name="url"/> looks like a git repository url (https, ssh, git or scp-like "git@host:path.git").
	/// </summary>
	/// <param name="url">The url typed by the user.</param>
	/// <returns><see langword="true"/> when the url is not empty and ends its path with ".git".</returns>
	public static bool IsValid(string? url)
		=> !string.IsNullOrEmpty(url) && GitUrlRegex().IsMatch(url);
}
