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

	[Fact]
	public void GenerateSeed_FromParallelLoads_StaysValidAndVaried()
	{
		// Items are created by parallel loads: a shared Random that is not thread-safe ends up returning 0 forever.
		var seeds = new int[200_000];
		Parallel.For(0, seeds.Length, i => seeds[i] = Item.GenerateSeed());

		seeds.Should().OnlyContain(seed => Item.IsValidSeed(seed));
		seeds.Distinct().Count().Should().BeGreaterThan(1000);
	}
}
