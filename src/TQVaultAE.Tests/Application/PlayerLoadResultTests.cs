using AwesomeAssertions;
using TQVaultAE.Application;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Application;

public class PlayerLoadResultTests
{
	[Fact]
	public void IsSuccess_WithNullPlayer_ReturnsFalse()
	{
		// Arrange
		var result = new PlayerLoadResult { Player = null! };

		// Act
		var isSuccess = result.IsSuccess;

		// Assert
		isSuccess.Should().BeFalse();
	}

	[Fact]
	public void IsSuccess_WithPlayer_ReturnsTrue()
	{
		// Arrange
		var player = new PlayerCollection("TestPlayer", "test.chr");
		var result = new PlayerLoadResult { Player = player };

		// Act
		var isSuccess = result.IsSuccess;

		// Assert
		isSuccess.Should().BeTrue();
	}

	[Fact]
	public void Failed_WithException_SetsError()
	{
		// Arrange
		var exception = new InvalidOperationException("Test error");

		// Act
		var result = PlayerLoadResult.Failed(exception);

		// Assert
		result.Error.Should().BeSameAs(exception);
	}

	[Fact]
	public void Failed_WithException_SetsPlayerToNull()
	{
		// Arrange
		var exception = new InvalidOperationException("Test error");

		// Act
		var result = PlayerLoadResult.Failed(exception);

		// Assert
		result.Player.Should().BeNull();
	}

	[Fact]
	public void Failed_WithException_IsSuccessIsFalse()
	{
		// Arrange
		var exception = new InvalidOperationException("Test error");

		// Act
		var result = PlayerLoadResult.Failed(exception);

		// Assert
		result.IsSuccess.Should().BeFalse();
	}

	[Fact]
	public void Failed_WithNullException_SetsErrorToNull()
	{
		// Act
		var result = PlayerLoadResult.Failed(null!);

		// Assert
		result.Error.Should().BeNull();
	}

	[Fact]
	public void Properties_CanBeSetAndRetrieved()
	{
		// Arrange
		var result = new PlayerLoadResult();
		var player = new PlayerCollection("TestPlayer", "test.chr");
		var playerStash = new Stash("Player", "stash.d6v");
		var transferStash = new Stash("Player", "transfer.d6v");
		var relicVault = new Stash("Player", "relic.d6v");

		// Act
		result.PlayerFile = "player.chr";
		result.Player = player;
		result.PlayerStash = playerStash;
		result.PlayerStashFile = "player.d6v";
		result.TransferStash = transferStash;
		result.RelicVaultStash = relicVault;

		// Assert
		result.PlayerFile.Should().Be("player.chr");
		result.Player.Should().BeSameAs(player);
		result.PlayerStash.Should().BeSameAs(playerStash);
		result.PlayerStashFile.Should().Be("player.d6v");
		result.TransferStash.Should().BeSameAs(transferStash);
		result.RelicVaultStash.Should().BeSameAs(relicVault);
	}
}
