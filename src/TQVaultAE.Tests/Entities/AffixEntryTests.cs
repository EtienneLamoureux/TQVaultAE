using AwesomeAssertions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Entities;

/// <summary>
/// Unit tests for AffixEntry and AffixGroup entities
/// </summary>
public class AffixEntryTests
{
	[Fact]
	public void AffixGroup_WithNullEntries_InitializesEmpty()
	{
		// This test verifies null handling logic in the constructor
		// which is meaningful edge case handling

		// Arrange
		var key = new AffixGroupKey(1, "Test");

		// Act
		var group = new AffixGroup(key, null);

		// Assert - null should be handled gracefully
		group.Count.Should().Be(0);
	}
}
