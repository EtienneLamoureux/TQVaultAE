using AwesomeAssertions;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Application;

public class StashLoadResultTests
{
	[Fact]
	public void IsSuccess_WithNullStash_ReturnsFalse()
	{
		// Arrange
		var result = new StashLoadResult { Stash = null! };

		// Act
		var isSuccess = result.IsSuccess;

		// Assert
		isSuccess.Should().BeFalse();
	}

	[Fact]
	public void IsSuccess_WithStash_ReturnsTrue()
	{
		// Arrange
		var stash = new Stash("Player", "stash.d6v");
		var result = new StashLoadResult { Stash = stash };

		// Act
		var isSuccess = result.IsSuccess;

		// Assert
		isSuccess.Should().BeTrue();
	}

	[Fact]
	public void Succeeded_SetsCorrectValues()
	{
		// Arrange
		var stash = new Stash("Player", "stash.d6v");
		var stashFile = "stash.d6v";

		// Act
		var result = StashLoadResult.Succeeded(stash, stashFile);

		// Assert
		result.Stash.Should().BeSameAs(stash);
		result.StashFile.Should().Be(stashFile);
		result.IsSuccess.Should().BeTrue();
	}

	[Fact]
	public void Empty_ReturnsEmptyResult()
	{
		// Act
		var result = StashLoadResult.Empty();

		// Assert
		result.Stash.Should().BeNull();
		result.StashFile.Should().BeNull();
		result.IsSuccess.Should().BeFalse();
	}

	[Fact]
	public void Properties_CanBeSetAndRetrieved()
	{
		// Arrange
		var result = new StashLoadResult();
		var stash = new Stash("Player", "stash.d6v");

		// Act
		result.Stash = stash;
		result.StashFile = "custom_path.d6v";

		// Assert
		result.Stash.Should().BeSameAs(stash);
		result.StashFile.Should().Be("custom_path.d6v");
	}
}
