using AwesomeAssertions;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

/// <summary>
/// <see cref="GameProcessService"/>: which running processes are the game.
/// </summary>
public class GameProcessServiceTests
{
	[Theory]
	[InlineData("TQ")]
	[InlineData("tq")]
	[InlineData("TQIT")]
	public void IsGame_AGameExecutableRuns_IsTrue(string name)
		=> GameProcessService.IsGame(["explorer", name]).Should().BeTrue();

	[Theory]
	[InlineData("TQVaultAE")]
	[InlineData("TQSaveFilesExplorer")]
	[InlineData("ARZExplorer")]
	[InlineData("")]
	public void IsGame_OnlyTheToolsRun_IsFalse(string name)
		=> GameProcessService.IsGame(["explorer", name]).Should().BeFalse("TQVault and its tools are not the game");
}
