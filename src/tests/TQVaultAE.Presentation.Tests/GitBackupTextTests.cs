using System.ComponentModel;
using System.Globalization;
using AwesomeAssertions;
using TQVaultAE.Application.Results;
using TQVaultAE.Presentation;

namespace TQVaultAE.Tests.Presentation;

/// <summary>
/// <see cref="GitBackupText"/>: the message shown when a git backup step fails.
/// </summary>
public class GitBackupTextTests
{
	private const string Repository = "https://example.com/vaults.git";

	public static TheoryData<GitBackupStep, string> Formats => new()
	{
		{ GitBackupStep.Clone, Resources.GitUnableToClone },
		{ GitBackupStep.Pull, Resources.GitUnableToPull },
		{ GitBackupStep.Fetch, Resources.GitUnableToFetch },
		{ GitBackupStep.Push, Resources.GitUnableToPush },
	};

	[Theory]
	[MemberData(nameof(Formats))]
	public void Message_EachStep_NamesTheRepositoryThenGitsMessage(GitBackupStep step, string format)
		=> GitBackupText.Message(new GitBackupError(step, Repository, new InvalidOperationException("rejected")))
			.Should().Be(string.Format(CultureInfo.CurrentCulture, format, Repository) + Environment.NewLine + "rejected");

	[Fact]
	public void Message_GitNotFound_SaysTheMessageIsGits()
		=> GitBackupText.Message(new GitBackupError(GitBackupStep.Push, Repository, new Win32Exception(2, "not found")))
			.Should().EndWith(Environment.NewLine + "git: not found");
}
