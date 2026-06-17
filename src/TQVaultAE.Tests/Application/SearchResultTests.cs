using AwesomeAssertions;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Results;

namespace TQVaultAE.Tests.Application;

public class SearchResultTests
{
	[Fact]
	public void Constructor_WithNullItem_ThrowsArgumentNullException()
	{
		// Act & Assert
		var act = () => new SearchResult(null!, new Lazy<ToFriendlyNameResult>(() => new ToFriendlyNameResult(new Item())));
		act.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public void Constructor_WithNullFnames_ThrowsArgumentNullException()
	{
		// Arrange
		var item = new Item();

		// Act & Assert
		var act = () => new SearchResult(item, null!);
		act.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public void Container_WithNullPlace_ReturnsEmptyString()
	{
		// Arrange
		var item = new Item { Place = null! };
		var searchResult = new SearchResult(item, new Lazy<ToFriendlyNameResult>(() => new ToFriendlyNameResult(item)));

		// Act
		var result = searchResult.Container;

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public void Container_WithValidPlace_ReturnsPath()
	{
		// Arrange
		var item = new Item();
		item.Place.Path = "player_chr/sack1";
		var searchResult = new SearchResult(item, new Lazy<ToFriendlyNameResult>(() => new ToFriendlyNameResult(item)));

		// Act
		var result = searchResult.Container;

		// Assert
		result.Should().Be("player_chr/sack1");
	}

	[Fact]
	public void ContainerName_WithNullPlace_ReturnsEmptyString()
	{
		// Arrange
		var item = new Item { Place = null! };
		var searchResult = new SearchResult(item, new Lazy<ToFriendlyNameResult>(() => new ToFriendlyNameResult(item)));

		// Act
		var result = searchResult.ContainerName;

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public void ContainerName_WithValidPlace_ReturnsName()
	{
		// Arrange
		var item = new Item();
		item.Place.Name = "Test Sack";
		var searchResult = new SearchResult(item, new Lazy<ToFriendlyNameResult>(() => new ToFriendlyNameResult(item)));

		// Act
		var result = searchResult.ContainerName;

		// Assert
		result.Should().Be("Test Sack");
	}

	[Fact]
	public void SackNumber_WithNullPlace_ReturnsZero()
	{
		// Arrange
		var item = new Item { Place = null! };
		var searchResult = new SearchResult(item, new Lazy<ToFriendlyNameResult>(() => new ToFriendlyNameResult(item)));

		// Act
		var result = searchResult.SackNumber;

		// Assert
		result.Should().Be(0);
	}

	[Fact]
	public void SackType_WithNullPlace_ReturnsDefault()
	{
		// Arrange
		var item = new Item { Place = null! };
		var searchResult = new SearchResult(item, new Lazy<ToFriendlyNameResult>(() => new ToFriendlyNameResult(item)));

		// Act
		var result = searchResult.SackType;

		// Assert
		result.Should().Be(default(SackType));
	}

	[Fact]
	public void StashType_WithNullPlace_ReturnsNull()
	{
		// Arrange
		var item = new Item { Place = null! };
		var searchResult = new SearchResult(item, new Lazy<ToFriendlyNameResult>(() => new ToFriendlyNameResult(item)));

		// Act
		var result = searchResult.StashType;

		// Assert
		result.Should().BeNull();
	}

	[Fact]
	public void IdString_CombinesMultipleProperties()
	{
		// Arrange
		var item = new Item();
		item.Place.Path = "path/to/player";
		item.Place.Name = "Player Sack";
		item.Place.SackNumber = 5;
		item.Place.SackType = SackType.Player;
		var searchResult = new SearchResult(item, new Lazy<ToFriendlyNameResult>(() => new ToFriendlyNameResult(item)));

		// Act
		var result = searchResult.IdString;

		// Assert
		result.Should().Contain("path/to/player");
		result.Should().Contain("Player Sack");
		result.Should().Contain("5");
		result.Should().Contain("Player");
	}

	[Fact]
	public void IdString_WithNullPlace_ReturnsDefaultValues()
	{
		// Arrange
		var item = new Item { Place = null! };
		var searchResult = new SearchResult(item, new Lazy<ToFriendlyNameResult>(() => new ToFriendlyNameResult(item)));

		// Act
		var result = searchResult.IdString;

		// Assert
		result.Should().Contain("|"); // Multiple pipe separators for empty values
	}
}
