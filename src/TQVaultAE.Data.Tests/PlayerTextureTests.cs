using AwesomeAssertions;
using Moq;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;

namespace TQVaultAE.Tests.Data;

/// <summary>The character's body texture, which tells the male or female player template.</summary>
public sealed class PlayerTextureTests : IDisposable
{
	private readonly SaveFixtures _fixtures = new();

	public void Dispose() => this._fixtures.Dispose();

	[Fact]
	public void LoadFile_PlayerTexture_IsRead()
	{
		var info = this._fixtures.LoadPlayer().PlayerInfo!;

		info.PlayerTexture.Should().Be("""Creatures\pc\male\malepc01_white.tex""");
		info.IsFemale.Should().BeFalse();
	}

	[Theory]
	[InlineData("""Creatures\pc\female\femalepc01_tan.tex""", true)]
	[InlineData("""Creatures\pc\male\malepc01_white.tex""", false)]
	[InlineData("", false)]
	public void IsFemale_FollowsTheTextureFolder(string texture, bool expected)
		=> new PlayerInfo(Mock.Of<IPathIO>()) { PlayerTexture = texture }.IsFemale.Should().Be(expected);
}
