using AwesomeAssertions;
using System.Collections.ObjectModel;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Entities;

/// <summary>
/// Tests for RecordId - meaningful tests only
/// </summary>
public class RecordIdTests
{
	#region DLC Detection Tests

	[Theory]
	[InlineData("records/xpack/ weapon", GameDlc.ImmortalThrone)]
	[InlineData("records\\XPACK\\ weapon", GameDlc.ImmortalThrone)]
	public void Dlc_XPack_ReturnsImmortalThrone(string path, GameDlc expected)
	{
		// Act
		var result = RecordId.Create(path).Dlc;

		// Assert
		result.Should().Be(expected);
	}

	[Theory]
	[InlineData("records\\XPACK2\\ weapon", GameDlc.Ragnarok)]
	public void Dlc_XPack2_ReturnsRagnarok(string path, GameDlc expected)
	{
		// Act
		var result = RecordId.Create(path).Dlc;

		// Assert
		result.Should().Be(expected);
	}

	[Theory]
	[InlineData("records\\XPACK3\\ weapon", GameDlc.Atlantis)]
	public void Dlc_XPack3_ReturnsAtlantis(string path, GameDlc expected)
	{
		// Act
		var result = RecordId.Create(path).Dlc;

		// Assert
		result.Should().Be(expected);
	}

	[Theory]
	[InlineData("records\\XPACK4\\ weapon", GameDlc.EternalEmbers)]
	[InlineData("records\\XPACK4\\ED\\ dungeon", GameDlc.EternalEmbers)] // HardCore dungeon
	public void Dlc_XPack4_ReturnsEternalEmbers(string path, GameDlc expected)
	{
		// Act
		var result = RecordId.Create(path).Dlc;

		// Assert
		result.Should().Be(expected);
	}

	[Theory]
	[InlineData("records/items/sword")]
	[InlineData("records\\items\\sword")]
	public void Dlc_WithoutXPack_ReturnsTitanQuest(string path)
	{
		// Act
		var result = RecordId.Create(path).Dlc;

		// Assert
		result.Should().Be(GameDlc.TitanQuest);
	}

	[Fact]
	public void Dlc_EmptyPath_ReturnsTitanQuest()
	{
		// Act
		var result = RecordId.Create(string.Empty).Dlc;

		// Assert
		result.Should().Be(GameDlc.TitanQuest);
	}

	#endregion

	#region IsOld Detection Tests

	[Theory]
	[InlineData("records\\OLD\\ weapon")]
	[InlineData("records/OLD/weapon")]
	public void IsOld_WithOldPath_ReturnsTrue(string path)
	{
		// Act
		var result = RecordId.Create(path).IsOld;

		// Assert
		result.Should().BeTrue();
	}

	[Theory]
	[InlineData("records/items/sword")]
	[InlineData("records/OLD-items/sword")]
	public void IsOld_WithoutOldPath_ReturnsFalse(string path)
	{
		// Act
		var result = RecordId.Create(path).IsOld;

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void IsOld_EmptyPath_ReturnsFalse()
	{
		// Act
		var result = RecordId.Create(string.Empty).IsOld;

		// Assert
		result.Should().BeFalse();
	}

	#endregion

	#region Normalization Tests

	[Fact]
	public void Normalized_MixedCasePath_UppercasesAndConvertsSlashes()
	{
		// Act
		var result = RecordId.Create("Records/Items/Sword");

		// Assert - Case normalized to uppercase, slashes to backslashes
		result.Normalized.Should().Be("RECORDS\\ITEMS\\SWORD");
	}

	[Fact]
	public void Normalized_ForwardSlashes_ConvertedToBackslashes()
	{
		// Act
		var result = RecordId.Create("records/items/sword");

		// Assert
		result.Normalized.Should().Contain("\\");
	}

	[Fact]
	public void Normalized_EmptyPath_RemainsEmpty()
	{
		// Act
		var result = RecordId.Create(string.Empty);

		// Assert
		result.Normalized.Should().BeEmpty();
	}

	#endregion

	#region Empty/Null Tests

	[Fact]
	public void Empty_IsStaticReadonly_AndEmpty()
	{
		// Assert
		RecordId.Empty.Should().NotBeNull();
		RecordId.Empty.Raw.Should().BeEmpty();
	}

	[Fact]
	public void IsEmpty_EmptyRecord_ReturnsTrue()
	{
		// Act
		var result = RecordId.Create(string.Empty).IsEmpty;

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public void IsEmpty_NonEmptyRecord_ReturnsFalse()
	{
		// Act
		var result = RecordId.Create("records/items").IsEmpty;

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void IsNullOrEmpty_WithNullOrEmpty_ReturnsTrue()
	{
		// Act & Assert
		RecordId.IsNullOrEmpty(null!).Should().BeTrue();
		RecordId.IsNullOrEmpty(RecordId.Empty).Should().BeTrue();
	}

	[Fact]
	public void IsNullOrEmpty_WithValidRecord_ReturnsFalse()
	{
		// Act
		var result = RecordId.IsNullOrEmpty(RecordId.Create("records/items"));

		// Assert
		result.Should().BeFalse();
	}

	#endregion

	#region Create Factory Tests

	[Fact]
	public void Create_TrimsWhitespace()
	{
		// Act
		var result = RecordId.Create("  records/items/sword  ");

		// Assert - Constructor trims whitespace
		result.Raw.Should().Be("records/items/sword");
	}

	[Fact]
	public void Create_EmptyPath_ReturnsEmptyRecord()
	{
		// Act
		var result = RecordId.Create(string.Empty);

		// Assert
		result.Should().NotBeNull();
		result.Raw.Should().BeEmpty();
	}

	#endregion

	#region ToString Tests

	[Fact]
	public void ToString_ReturnsNormalized()
	{
		// Arrange
		var recordId = RecordId.Create("Records/Items/Sword");

		// Act
		var result = recordId.ToString();

		// Assert
		result.Should().Be(recordId.Normalized);
	}

	[Fact]
	public void ToString_EmptyRecord_ReturnsEmpty()
	{
		// Act
		var result = RecordId.Create(string.Empty).ToString();

		// Assert
		result.Should().BeEmpty();
	}

	#endregion

	#region Tokens Tests

	[Fact]
	public void TokensRaw_BackslashPath_SplitsCorrectly()
	{
		// Arrange
		var recordId = RecordId.Create(@"records\items\weapon\sword");

		// Act
		var tokens = recordId.TokensRaw;

		// Assert
		tokens.Should().HaveCount(4);
		tokens.Should().ContainInOrder("records", "items", "weapon", "sword");
	}

	[Fact]
	public void TokensRaw_ForwardSlashPath_SplitsCorrectly()
	{
		// Arrange
		var recordId = RecordId.Create("records/items/weapon/sword");

		// Act
		var tokens = recordId.TokensRaw;

		// Assert
		tokens.Should().HaveCount(4);
		tokens.Should().ContainInOrder("records", "items", "weapon", "sword");
	}

	[Fact]
	public void TokensRaw_MixedSlashes_SplitsCorrectly()
	{
		// Arrange
		var recordId = RecordId.Create("records\\items/weapon\\sword");

		// Act
		var tokens = recordId.TokensRaw;

		// Assert
		tokens.Should().HaveCount(4);
		tokens.Should().ContainInOrder("records", "items", "weapon", "sword");
	}

	[Fact]
	public void TokensRaw_EmptyPath_ReturnsEmptyCollection()
	{
		// Arrange
		var recordId = RecordId.Create(string.Empty);

		// Act
		var tokens = recordId.TokensRaw;

		// Assert
		tokens.Should().BeEmpty();
	}

	[Fact]
	public void TokensRaw_ConsecutiveSlashes_RemovesEmptyEntries()
	{
		// Arrange - Multiple slashes should be treated as single delimiter
		var recordId = RecordId.Create(@"records\\items\\weapon");

		// Act
		var tokens = recordId.TokensRaw;

		// Assert - Should not contain empty strings
		tokens.Should().HaveCount(3);
		tokens.Should().NotContain(string.Empty);
	}

	[Fact]
	public void TokensRaw_SingleToken_ReturnsSingleItemCollection()
	{
		// Arrange
		var recordId = RecordId.Create("sword");

		// Act
		var tokens = recordId.TokensRaw;

		// Assert
		tokens.Should().HaveCount(1);
		tokens.Should().Contain("sword");
	}

	[Fact]
	public void TokensRaw_LeadingSlash_ReturnsWithoutEmptyEntry()
	{
		// Arrange
		var recordId = RecordId.Create(@"\records\items\sword");

		// Act
		var tokens = recordId.TokensRaw;

		// Assert - Should not contain empty first element
		tokens.Should().HaveCount(3);
		tokens.First().Should().Be("records");
	}

	[Fact]
	public void TokensNormalized_UppercasesAllTokens()
	{
		// Arrange
		var recordId = RecordId.Create("records/Items/WeaPon/Sword");

		// Act
		var tokens = recordId.TokensNormalized;

		// Assert
		tokens.Should().HaveCount(4);
		tokens.Should().ContainInOrder("RECORDS", "ITEMS", "WEAPON", "SWORD");
	}

	[Fact]
	public void TokensNormalized_IsCaseInsensitive()
	{
		// Arrange
		var recordId = RecordId.Create("RECORDS\\ITEMS\\SWORD");

		// Act
		var tokens = recordId.TokensNormalized;

		// Assert
		tokens.Should().ContainInOrder("RECORDS", "ITEMS", "SWORD");
	}

	[Fact]
	public void TokensNormalized_EmptyPath_ReturnsEmptyCollection()
	{
		// Arrange
		var recordId = RecordId.Create(string.Empty);

		// Act
		var tokens = recordId.TokensNormalized;

		// Assert
		tokens.Should().BeEmpty();
	}

	[Fact]
	public void TokensNormalized_ReturnsReadOnlyCollection()
	{
		// Arrange
		var recordId = RecordId.Create("records/items/sword");

		// Act
		var tokens = recordId.TokensNormalized;

		// Assert
		tokens.Should().BeAssignableTo<ReadOnlyCollection<string>>();
	}

	[Fact]
	public void TokensNormalized_IsCached_ReturnsSameInstance()
	{
		// Arrange
		var recordId = RecordId.Create("records/items/sword");

		// Act
		var tokens1 = recordId.TokensNormalized;
		var tokens2 = recordId.TokensNormalized;

		// Assert - Should return the same cached instance
		tokens1.Should().BeSameAs(tokens2);
	}

	[Fact]
	public void TokensRaw_IsCached_ReturnsSameInstance()
	{
		// Arrange
		var recordId = RecordId.Create("records/items/sword");

		// Act
		var tokens1 = recordId.TokensRaw;
		var tokens2 = recordId.TokensRaw;

		// Assert - Should return the same cached instance
		tokens1.Should().BeSameAs(tokens2);
	}

	[Fact]
	public void TokensNormalized_AccessingTokensNormalizedFirst_DoesNotThrow()
	{
		// Arrange - Access TokensNormalized before TokensRaw (tests lazy initialization)
		var recordId = RecordId.Create("records/items/sword");

		// Act
		var tokens = recordId.TokensNormalized;

		// Assert
		tokens.Should().HaveCount(3);
		tokens.Should().ContainInOrder("RECORDS", "ITEMS", "SWORD");
	}

	#endregion
}
