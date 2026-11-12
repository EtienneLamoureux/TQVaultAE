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
	public void Failed_WithException_IsSuccessIsFalse()
	{
		// Arrange
		var exception = new InvalidOperationException("Test error");

		// Act
		var result = PlayerLoadResult.Failed(exception);

		// Assert
		result.IsSuccess.Should().BeFalse();
	}
}
