using Moq;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Session;

namespace TQVaultAE.Tests.Services;

/// <summary>
/// Map service mocks for the services reading shared stashes.
/// </summary>
internal static class GameMapMocks
{
	/// <summary>
	/// A map service reading each map's shared stash files with that map's data, the base game's with the base game.
	/// </summary>
	public static Mock<IGameMapService> Create()
	{
		var maps = new Mock<IGameMapService>();
		maps.SetupGet(m => m.ActiveSharedStashMap).Returns(string.Empty);
		maps.Setup(m => m.SharedStashDataOf(It.IsAny<string?>())).Returns<string?>(map => new SharedStashData(GameMaps.Normalize(map), null));
		return maps;
	}
}
