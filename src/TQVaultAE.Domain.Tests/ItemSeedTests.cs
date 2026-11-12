using AwesomeAssertions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Domain;

public class ItemSeedTests
{
	[Theory]
	[InlineData(1, true)]
	[InlineData(Item.MaxSeed, true)]
	[InlineData(0, false)]
	[InlineData(Item.MaxSeed + 1, false)]
	[InlineData(-5, false)]
	public void IsValidSeed_AcceptsOneToMaxSeed(int seed, bool expected)
	{
		// The seed dialog promises 1 to 32767: both ends must be accepted.
		Item.IsValidSeed(seed).Should().Be(expected);
	}

	[Fact]
	public void GenerateSeed_IsAlwaysAcceptedBySeedDialog()
	{
		// The Random button of the seed dialog must never propose a seed its OK button refuses.
		for (int i = 0; i < 100_000; i++)
			Item.IsValidSeed(Item.GenerateSeed()).Should().BeTrue();
	}
}
