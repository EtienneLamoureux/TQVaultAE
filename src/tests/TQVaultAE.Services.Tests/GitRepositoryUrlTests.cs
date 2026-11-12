using AwesomeAssertions;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

public class GitRepositoryUrlTests
{
	[Theory]
	[InlineData("https://github.com/user/vaults.git")]
	[InlineData("http://server/repo.git/")]
	[InlineData("ssh://git@server:2222/user/vaults.git")]
	[InlineData("git://server/vaults.git")]
	[InlineData("git@github.com:user/vaults.git")]
	public void IsValid_GitUrls_ReturnsTrue(string url)
		=> GitRepositoryUrl.IsValid(url).Should().BeTrue();

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("https://github.com/user/vaults")]
	[InlineData("C:\\vaults")]
	[InlineData("github.com/user/vaults.git")]
	public void IsValid_NotAGitUrl_ReturnsFalse(string? url)
		=> GitRepositoryUrl.IsValid(url).Should().BeFalse();
}
