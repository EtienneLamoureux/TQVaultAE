using AwesomeAssertions;
using TQVaultAE.Application;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Application;

public class SessionContextTests
{
	[Fact]
	public void SessionContext_InitializesWithEmptyDictionaries()
	{
		// Arrange & Act
		var context = new SessionContext();

		// Assert
		context.Players.Should().NotBeNull();
		context.Vaults.Should().NotBeNull();
		context.Stashes.Should().NotBeNull();
	}

	[Fact]
	public void IconInfoCopied_InitiallyFalse()
	{
		// Arrange & Act
		var context = new SessionContext();

		// Assert
		context.IconInfoCopied.Should().BeFalse();
	}

	[Fact]
	public void IconInfoCopied_AfterSettingIconInfoCopy_True()
	{
		// Arrange
		var context = new SessionContext();
		var iconInfo = new BagButtonIconInfo
		{
			DisplayMode = BagButtonDisplayMode.CustomIcon,
			Label = "Test"
		};

		// Act
		context.IconInfoCopy = iconInfo;

		// Assert
		context.IconInfoCopied.Should().BeTrue();
	}

	[Fact]
	public void IconInfoCopy_ReturnsSetValue()
	{
		// Arrange
		var context = new SessionContext();
		var iconInfo = new BagButtonIconInfo
		{
			DisplayMode = BagButtonDisplayMode.CustomIcon,
			Label = "Test"
		};

		// Act
		context.IconInfoCopy = iconInfo;

		// Assert
		context.IconInfoCopy.Should().BeSameAs(iconInfo);
	}

	[Fact]
	public void IconInfoCopy_CanBeSetToNull()
	{
		// Arrange
		var context = new SessionContext();
		context.IconInfoCopy = new BagButtonIconInfo { Label = "Test" };

		// Act
		context.IconInfoCopy = null!;

		// Assert
		context.IconInfoCopy.Should().BeNull();
		context.IconInfoCopied.Should().BeTrue(); // Still true since flag was set
	}

	[Fact]
	public void CurrentPlayer_CanBeSetAndRetrieved()
	{
		// Arrange
		var context = new SessionContext();
		var player = new PlayerCollection("TestPlayer", "test.chr");

		// Act
		context.CurrentPlayer = player;

		// Assert
		context.CurrentPlayer.Should().BeSameAs(player);
	}

	[Fact]
	public void CurrentPlayer_CanBeNull()
	{
		// Arrange
		var context = new SessionContext();
		context.CurrentPlayer = new PlayerCollection("Test", "test.chr");

		// Act
		context.CurrentPlayer = null!;

		// Assert
		context.CurrentPlayer.Should().BeNull();
	}

	[Fact]
	public void Players_DictionarySupportsAddingEntries()
	{
		// Arrange
		var context = new SessionContext();
		var playerName = "TestPlayer";
		var playerFile = "test.chr";

		// Act - Use AddOrUpdateAtomic which handles Lazy wrapping
		context.Players.AddOrUpdateAtomic("player1", new PlayerCollection(playerName, playerFile));

		// Assert - Verify entry exists and value is correct
		context.Players.TryGetValue("player1", out var retrievedLazy).Should().BeTrue();
		retrievedLazy!.Value.PlayerName.Should().Be(playerName);
		retrievedLazy.Value.PlayerFile.Should().Be(playerFile);
	}

	[Fact]
	public void Vaults_DictionarySupportsAddingEntries()
	{
		// Arrange
		var context = new SessionContext();
		var vaultName = "Vault";
		var vaultFile = "vault.d6v";

		// Act - Use AddOrUpdateAtomic which handles Lazy wrapping
		context.Vaults.AddOrUpdateAtomic("vault1", new PlayerCollection(vaultName, vaultFile));

		// Assert - Verify entry exists and value is correct
		context.Vaults.TryGetValue("vault1", out var retrievedLazy).Should().BeTrue();
		retrievedLazy!.Value.PlayerName.Should().Be(vaultName);
		retrievedLazy.Value.PlayerFile.Should().Be(vaultFile);
	}

	[Fact]
	public void Stashes_DictionarySupportsAddingEntries()
	{
		// Arrange
		var context = new SessionContext();
		var stashPlayer = "Player";
		var stashFile = "stash.d6v";

		// Act - Use AddOrUpdateAtomic which handles Lazy wrapping
		context.Stashes.AddOrUpdateAtomic("stash1", new Stash(stashPlayer, stashFile));

		// Assert - Verify entry exists and value is correct
		// Note: Stash.PlayerName appends " - Immortal Throne" when IsImmortalThrone is true
		context.Stashes.TryGetValue("stash1", out var retrievedLazy).Should().BeTrue();
		retrievedLazy!.Value.PlayerName.Should().Contain(stashPlayer);
		retrievedLazy.Value.StashFile.Should().Be(stashFile);
		retrievedLazy.Value.IsImmortalThrone.Should().BeTrue();
	}
}
