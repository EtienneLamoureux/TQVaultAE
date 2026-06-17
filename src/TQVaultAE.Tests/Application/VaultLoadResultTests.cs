using AwesomeAssertions;
using TQVaultAE.Application;
using TQVaultAE.Application.Results;

namespace TQVaultAE.Tests.Application;

public class VaultLoadResultTests
{
	[Fact]
	public void IsSuccess_WithNullVault_ReturnsFalse()
	{
		// Arrange
		var result = new VaultLoadResult { Vault = null! };

		// Act
		var isSuccess = result.IsSuccess;

		// Assert
		isSuccess.Should().BeFalse();
	}

	[Fact]
	public void IsSuccess_WithVault_ReturnsTrue()
	{
		// Arrange
		var vault = new PlayerCollection("Vault", "vault.d6v");
		var result = new VaultLoadResult { Vault = vault };

		// Act
		var isSuccess = result.IsSuccess;

		// Assert
		isSuccess.Should().BeTrue();
	}

	[Fact]
	public void Succeeded_SetsCorrectValues()
	{
		// Arrange
		var vault = new PlayerCollection("Vault", "vault.d6v");
		var filename = "vault.d6v";

		// Act
		var result = VaultLoadResult.Succeeded(vault, filename);

		// Assert
		result.Vault.Should().BeSameAs(vault);
		result.Filename.Should().Be(filename);
		result.VaultLoaded.Should().BeTrue();
		result.IsSuccess.Should().BeTrue();
	}

	[Fact]
	public void Succeeded_SetsErrorAndArgumentExceptionToNull()
	{
		// Arrange
		var vault = new PlayerCollection("Vault", "vault.d6v");

		// Act
		var result = VaultLoadResult.Succeeded(vault, "vault.d6v");

		// Assert
		result.Error.Should().BeNull();
		result.ArgumentException.Should().BeNull();
	}

	[Fact]
	public void Failed_WithException_SetsCorrectValues()
	{
		// Arrange
		var exception = new InvalidOperationException("Test error");

		// Act
		var result = VaultLoadResult.Failed(exception);

		// Assert
		result.Error.Should().BeSameAs(exception);
		result.VaultLoaded.Should().BeFalse();
		result.IsSuccess.Should().BeFalse();
	}

	[Fact]
	public void Failed_WithException_SetsVaultToNull()
	{
		// Arrange
		var exception = new InvalidOperationException("Test error");

		// Act
		var result = VaultLoadResult.Failed(exception);

		// Assert
		result.Vault.Should().BeNull();
	}

	[Fact]
	public void FailedArgument_SetsCorrectValues()
	{
		// Arrange
		var message = "Invalid format";

		// Act
		var result = VaultLoadResult.FailedArgument(message);

		// Assert
		result.ArgumentException.Should().NotBeNull();
		result.ArgumentException.Message.Should().Be(message);
		result.VaultLoaded.Should().BeFalse();
		result.IsSuccess.Should().BeFalse();
	}

	[Fact]
	public void FailedArgument_SetsErrorToNull()
	{
		// Arrange
		var message = "Invalid format";

		// Act
		var result = VaultLoadResult.FailedArgument(message);

		// Assert
		result.Error.Should().BeNull();
	}
}
