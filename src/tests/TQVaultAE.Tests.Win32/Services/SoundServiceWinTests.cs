using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Config;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Services.Win32;

namespace TQVaultAE.Tests.Win32.Services;

/// <summary>Random sounds of <see cref="SoundServiceWin"/> when the game sounds could not be loaded.</summary>
public class SoundServiceWinTests
{
	[Fact]
	public void PlayRandom_WithSoundsEnabledButNoneLoaded_PlaysNothing()
	{
		var database = new Mock<IDatabase>();
		database.Setup(d => d.LoadResource(It.IsAny<RecordId>())).Returns((byte[]?)null);
		var settings = new UserSettings { EnableTQVaultSounds = true };
		using var service = new SoundServiceWin(new Mock<ILogger<SoundServiceWin>>().Object, database.Object, settings);

		var play = () =>
		{
			service.PlayRandomCancel();
			service.PlayRandomItemDrop();
			service.PlayRandomMetalHit();
			service.PlayRandomRelicDrop();
			service.PlayRandomVoice();
		};

		play.Should().NotThrow();
	}
}
