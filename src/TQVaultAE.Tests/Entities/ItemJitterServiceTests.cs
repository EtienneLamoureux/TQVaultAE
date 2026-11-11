using AwesomeAssertions;
using TQVaultAE.Domain.Jitter;

namespace TQVaultAE.Tests.Entities;

/// <summary>
/// Unit tests of the jitter primitives (issue #552). In-game item checks live in
/// <see cref="JitterVerificationTests"/>; roll order and gating rules are described in issue #552 (https://github.com/EtienneLamoureux/TQVaultAE/issues/552).
/// </summary>
public class ItemJitterServiceTests
{
	[Fact]
	public void GenerateRange_NegativeResults_TruncateTowardZero()
	{
		// The draw truncates (float)d + 0.5f toward zero, so -4 is unreachable in [-4, 4]
		// (a Math.Floor port would produce it).
		var rng = new TitanQuestRNG(1);
		var seen = new HashSet<int>();

		for (int i = 0; i < 5000; i++)
			seen.Add(rng.GenerateRange(-4, 4));

		seen.Should().BeEquivalentTo([-3, -2, -1, 0, 1, 2, 3, 4]);
	}

	[Theory]
	[InlineData(-4, 4, -3, 4)]
	[InlineData(-1, 1, 0, 1)]
	[InlineData(-15, 15, -14, 15)]
	public void GenerateRangeBounds_MatchesEngineTruncation(int min, int max, int expectedMin, int expectedMax)
		=> TitanQuestRNG.GenerateRangeBounds(min, max).Should().Be((expectedMin, expectedMax));

	[Theory]
	[InlineData(20f, 20f, 17f, 24f)]   // k = 4
	[InlineData(10f, 0f, 10f, 11f)]    // k forced to 1
	[InlineData(0.5f, 20f, 0.5f, 1.5f)] // |r| < 1 keeps the original value
	public void JitterBounds_CoverReachableResults(float value, float pct, float expectedMin, float expectedMax)
		=> ItemJitterService.JitterBounds(value, pct).Should().Be((expectedMin, expectedMax));

	[Theory]
	[InlineData(1u)]
	[InlineData(13009u)]
	[InlineData(127773u)]
	[InlineData(2147483646u)]
	public void AdvanceSeed_MatchesParkMillerMinstd(uint seed)
	{
		var rng = new TitanQuestRNG(seed);
		ulong expected = seed;

		for (int i = 0; i < 1000; i++) {
			expected = expected * 16807UL % 2147483647UL;
			rng.AdvanceSeed().Should().Be((int)expected, $"step {i} from seed {seed}");
		}
	}

	[Fact]
	public void RollCharAffix_ZeroJitter_StillConsumesRollAndCanAddOne()
	{
		// An affix roll forces k=1 when (pct/100*v) truncates to 0, so a "fixed" Char affix
		// value still rolls GenerateRange(-1, 1), which yields 0 or +1.
		var rng = new TitanQuestRNG(42);
		var results = new HashSet<float>();

		for (int i = 0; i < 400; i++) {
			var before = rng.CurrentSeed;
			results.Add(ItemJitterService.RollCharAffix(10f, 0f, rng));
			rng.CurrentSeed.Should().NotBe(before);
		}

		results.Should().BeEquivalentTo([10f, 11f]);
	}

	[Theory]
	[InlineData(0f)]
	[InlineData(-5f)]
	public void AddJitter_NonPositivePercent_DoesNotConsumeRng(float pct)
	{
		var rng = new TitanQuestRNG(13009);
		var before = rng.CurrentSeed;
		Span<float> values = [20f, 50f];

		ItemJitterService.AddJitter(values, pct, rng);

		values.ToArray().Should().Equal(20f, 50f);
		rng.CurrentSeed.Should().Be(before);
	}

	[Fact]
	public void AddJitter_PercentAbove50_IsCappedAt50()
	{
		var capped = new TitanQuestRNG(42);
		var at50 = new TitanQuestRNG(42);
		Span<float> a = [100f, 37f];
		Span<float> b = [100f, 37f];

		ItemJitterService.AddJitter(a, 90f, capped);
		ItemJitterService.AddJitter(b, 50f, at50);

		a.ToArray().Should().Equal(b.ToArray());
	}

	[Fact]
	public void Jitter_ResultBelowOneInMagnitude_KeepsOriginalValue()
	{
		// 0.5 with k=1 can roll to 0.5 (+0) or 1.5 (+1); 0.5 itself is kept because |r| < 1.
		var rng = new TitanQuestRNG(7);
		for (int i = 0; i < 200; i++)
			ItemJitterService.Jitter(0.5f, 20f, rng).Should().BeOneOf(0.5f, 1.5f);
	}
}
