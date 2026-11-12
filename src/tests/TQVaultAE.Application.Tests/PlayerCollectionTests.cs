using AwesomeAssertions;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Application;

public class PlayerCollectionTests
{
	[Fact]
	public void CreateEmptySacks_WithPositiveNumber_CreatesCorrectNumberOfSacks()
	{
		// Arrange
		var playerCollection = new PlayerCollection("TestPlayer", "test_player.chr");

		// Act
		playerCollection.CreateEmptySacks(5);

		// Assert
		playerCollection.Sacks.Should().HaveCount(5);
		playerCollection.NumberOfSacks.Should().Be(5);
		playerCollection.numberOfSacks.Should().Be(5);
	}

	[Fact]
	public void CreateEmptySacks_WithZero_CreatesEmptyArray()
	{
		// Arrange
		var playerCollection = new PlayerCollection("TestPlayer", "test_player.chr");

		// Act
		playerCollection.CreateEmptySacks(0);

		// Assert
		playerCollection.Sacks.Should().BeEmpty();
		playerCollection.NumberOfSacks.Should().Be(0);
	}

	[Fact]
	public void CreateEmptySacks_InitializesSacksAsNotModified()
	{
		// Arrange
		var playerCollection = new PlayerCollection("TestPlayer", "test_player.chr");

		// Act
		playerCollection.CreateEmptySacks(3);

		// Assert
		foreach (var sack in playerCollection.Sacks)
			sack.IsModified.Should().BeFalse();
	}

	[Fact]
	public void GetSack_WithValidIndex_ReturnsSack()
	{
		// Arrange
		var playerCollection = new PlayerCollection("TestPlayer", "test_player.chr");
		playerCollection.CreateEmptySacks(3);

		// Act
		var result = playerCollection.GetSack(1);

		// Assert
		result.Should().NotBeNull();
		result.Should().BeSameAs(playerCollection.Sacks[1]);
	}

	[Fact]
	public void GetSack_WithNegativeIndex_ReturnsNull()
	{
		// Arrange
		var playerCollection = new PlayerCollection("TestPlayer", "test_player.chr");
		playerCollection.CreateEmptySacks(3);

		// Act
		var result = playerCollection.GetSack(-1);

		// Assert
		result.Should().BeNull();
	}

	[Fact]
	public void GetSack_WithOutOfRangeIndex_ReturnsNull()
	{
		// Arrange
		var playerCollection = new PlayerCollection("TestPlayer", "test_player.chr");
		playerCollection.CreateEmptySacks(3);

		// Act
		var result = playerCollection.GetSack(10);

		// Assert
		result.Should().BeNull();
	}

	[Fact]
	public void GetSack_WithNullSacksArray_ReturnsNull()
	{
		// Arrange
		var playerCollection = new PlayerCollection("TestPlayer", "test_player.chr");

		// Act
		var result = playerCollection.GetSack(0);

		// Assert
		result.Should().BeNull();
	}

	[Fact]
	public void MoveSack_WithValidIndices_SwapsSacks()
	{
		// Arrange
		var playerCollection = new PlayerCollection("TestPlayer", "test_player.chr");
		playerCollection.CreateEmptySacks(3);
		var firstSack = playerCollection.Sacks[0];
		var secondSack = playerCollection.Sacks[1];

		// Act
		var result = playerCollection.MoveSack(0, 1);

		// Assert
		result.Should().BeTrue();
		playerCollection.Sacks[0].Should().BeSameAs(secondSack);
		playerCollection.Sacks[1].Should().BeSameAs(firstSack);
		playerCollection.Sacks[0].IsModified.Should().BeTrue();
		playerCollection.Sacks[1].IsModified.Should().BeTrue();
	}

	[Fact]
	public void MoveSack_WithSameSourceAndDestination_ReturnsFalse()
	{
		// Arrange
		var playerCollection = new PlayerCollection("TestPlayer", "test_player.chr");
		playerCollection.CreateEmptySacks(3);

		// Act
		var result = playerCollection.MoveSack(1, 1);

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void MoveSack_WithNegativeIndex_ReturnsFalse()
	{
		// Arrange
		var playerCollection = new PlayerCollection("TestPlayer", "test_player.chr");
		playerCollection.CreateEmptySacks(3);

		// Act
		var result = playerCollection.MoveSack(-1, 1);

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void MoveSack_WithOutOfRangeIndex_ReturnsFalse()
	{
		// Arrange
		var playerCollection = new PlayerCollection("TestPlayer", "test_player.chr");
		playerCollection.CreateEmptySacks(3);

		// Act
		var result = playerCollection.MoveSack(0, 10);

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void MoveSack_WithNullSacksArray_ReturnsFalse()
	{
		// Arrange
		var playerCollection = new PlayerCollection("TestPlayer", "test_player.chr");

		// Act
		var result = playerCollection.MoveSack(0, 1);

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void CopySack_WithValidIndices_CreatesDuplicate()
	{
		// Arrange
		var playerCollection = new PlayerCollection("TestPlayer", "test_player.chr");
		playerCollection.CreateEmptySacks(3);
		var originalSack = playerCollection.Sacks[0];

		// Act
		var result = playerCollection.CopySack(0, 1);

		// Assert
		result.Should().BeTrue();
		playerCollection.Sacks[1].Should().NotBeSameAs(originalSack);
	}

	[Fact]
	public void CopySack_WithSameSourceAndDestination_ReturnsFalse()
	{
		// Arrange
		var playerCollection = new PlayerCollection("TestPlayer", "test_player.chr");
		playerCollection.CreateEmptySacks(3);

		// Act
		var result = playerCollection.CopySack(1, 1);

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void IsModified_WhenNoSacksModified_ReturnsFalse()
	{
		// Arrange
		var playerCollection = new PlayerCollection("TestPlayer", "test_player.chr");
		playerCollection.CreateEmptySacks(3);

		// Act
		var result = playerCollection.IsModified;

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void IsModified_WhenSackModified_ReturnsTrue()
	{
		// Arrange
		var playerCollection = new PlayerCollection("TestPlayer", "test_player.chr");
		playerCollection.CreateEmptySacks(3);
		playerCollection.Sacks[1].IsModified = true;

		// Act
		var result = playerCollection.IsModified;

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public void IsModified_WhenEquipmentSackModified_ReturnsTrue()
	{
		// Arrange
		var playerCollection = new PlayerCollection("TestPlayer", "test_player.chr");
		playerCollection.EquipmentSack = new SackCollection { IsModified = true };

		// Act
		var result = playerCollection.IsModified;

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public void IsModified_WhenPlayerInfoModified_ReturnsTrue()
	{
		// Arrange
		var playerCollection = new PlayerCollection("TestPlayer", "test_player.chr");
		playerCollection.PlayerInfo = new PlayerInfo(new PathIO()) { Modified = true };

		// Act
		var result = playerCollection.IsModified;

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public void Saved_MarksAllSacksAsNotModified()
	{
		// Arrange
		var playerCollection = new PlayerCollection("TestPlayer", "test_player.chr");
		playerCollection.CreateEmptySacks(3);
		playerCollection.Sacks[0].IsModified = true;
		playerCollection.Sacks[1].IsModified = true;
		playerCollection.Sacks[2].IsModified = true;

		// Act
		playerCollection.Saved();

		// Assert
		foreach (var sack in playerCollection.Sacks)
			sack.IsModified.Should().BeFalse();
	}

	[Fact]
	public void Saved_MarksEquipmentSackAsNotModified()
	{
		// Arrange
		var playerCollection = new PlayerCollection("TestPlayer", "test_player.chr");
		playerCollection.EquipmentSack = new SackCollection { IsModified = true };

		// Act
		playerCollection.Saved();

		// Assert
		playerCollection.EquipmentSack.IsModified.Should().BeFalse();
	}

	[Fact]
	public void IsPlayer_WhenPlayerFile_ReturnsTrue()
	{
		// Arrange
		var playerCollection = new PlayerCollection("TestPlayer", "character_player.chr");

		// Act
		var result = playerCollection.IsPlayer;

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public void IsPlayer_WhenNotPlayerFile_ReturnsFalse()
	{
		// Arrange
		var playerCollection = new PlayerCollection("TestPlayer", "character_vault.d6v");

		// Act
		var result = playerCollection.IsPlayer;

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void GetEnumerator_YieldsAllSacks()
	{
		// Arrange
		var playerCollection = new PlayerCollection("TestPlayer", "test_player.chr");
		playerCollection.CreateEmptySacks(3);

		// Act
		var result = playerCollection.ToList();

		// Assert
		result.Should().HaveCount(3);
		result.Should().Contain(playerCollection.Sacks[0]);
		result.Should().Contain(playerCollection.Sacks[1]);
		result.Should().Contain(playerCollection.Sacks[2]);
	}

	[Fact]
	public void GetEnumerator_WithNullSacks_YieldsNothing()
	{
		// Arrange
		var playerCollection = new PlayerCollection("TestPlayer", "test_player.chr");

		// Act
		var result = playerCollection.ToList();

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public void NumberOfSacks_WithNullSacksArray_ReturnsZero()
	{
		// Arrange
		var playerCollection = new PlayerCollection("TestPlayer", "test_player.chr");

		// Act
		var result = playerCollection.NumberOfSacks;

		// Assert
		result.Should().Be(0);
	}
}
