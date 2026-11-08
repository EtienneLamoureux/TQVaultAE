using AwesomeAssertions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Domain;

public class PlayerLevelTests
{
	[Theory]
	[InlineData(1, 0)]
	[InlineData(2, 1)]
	[InlineData(7, 1)]
	[InlineData(8, 2)]
	[InlineData(85, 2)]
	public void GetMasteriesAllowed_FollowsTheLevel(int level, int expected)
		=> PlayerLevel.GetMasteriesAllowed(level).Should().Be(expected);

	[Fact]
	public void GetInvestedAttributePoints_AtTheMinimum_IsZero()
		=> PlayerLevel.GetInvestedAttributePoints(50, 50, 50, 300, 300).Should().Be(0);

	[Fact]
	public void GetInvestedAttributePoints_CountsEachAttributeByItsIncrement()
	{
		// 3 points of strength, 2 of dexterity, 1 of intelligence, 2 of health, 1 of mana.
		var points = PlayerLevel.GetInvestedAttributePoints(62, 58, 54, 380, 340);

		points.Should().Be(9);
	}

	[Fact]
	public void GetInvestedAttributePoints_DropsTheRemainderOfEachAttribute()
	{
		// 55 strength is one point and one over, 339 health is no full point.
		var points = PlayerLevel.GetInvestedAttributePoints(55, 50, 50, 339, 300);

		points.Should().Be(1);
	}

	[Theory]
	[InlineData(100, 0, 1, 5000000)]
	[InlineData(100, 1, 2, 7500000)]
	[InlineData(100, 0, 2, 7500000)]
	[InlineData(9000000, 0, 1, 9000000)]
	[InlineData(9000000, 1, 2, 9000000)]
	[InlineData(100, 1, 1, 100)]
	[InlineData(100, 2, 0, 100)]
	public void GetMoneyAfterDifficultyChange_RaisesTheMoneyOnlyWhenADifficultyIsUnlocked(int money, int previous, int difficulty, int expected)
		=> PlayerLevel.GetMoneyAfterDifficultyChange(money, previous, difficulty).Should().Be(expected);

	[Theory]
	[InlineData(10, 12, 4, 6)]
	[InlineData(12, 10, -4, -6)]
	[InlineData(10, 10, 0, 0)]
	public void GetLevelChangePoints_GivesTwoAttributeAndThreeSkillPointsPerLevel(int previous, int level, int attributes, int skills)
		=> PlayerLevel.GetLevelChangePoints(previous, level).Should().Be((attributes, skills));

	[Fact]
	public void TryChangeAttribute_Lowered_GivesThePointsBack()
	{
		var ok = PlayerLevel.TryChangeAttribute(58, 50, PlayerLevel.AtrributeIncrementPerPoint, 0, out var pool);

		ok.Should().BeTrue();
		pool.Should().Be(2);
	}

	[Fact]
	public void TryChangeAttribute_Raised_SpendsThePoints()
	{
		var ok = PlayerLevel.TryChangeAttribute(300, 380, PlayerLevel.HealthAndManaIncrementPerPoint, 5, out var pool);

		ok.Should().BeTrue();
		pool.Should().Be(3);
	}

	[Fact]
	public void TryChangeAttribute_RaisedPastThePool_IsRejectedAndKeepsThePool()
	{
		var ok = PlayerLevel.TryChangeAttribute(50, 62, PlayerLevel.AtrributeIncrementPerPoint, 2, out var pool);

		ok.Should().BeFalse();
		pool.Should().Be(2);
	}

	[Fact]
	public void TryChangeAttribute_SpendingTheWholePool_IsAllowed()
	{
		var ok = PlayerLevel.TryChangeAttribute(50, 58, PlayerLevel.AtrributeIncrementPerPoint, 2, out var pool);

		ok.Should().BeTrue();
		pool.Should().Be(0);
	}

	[Theory]
	[InlineData(1, 1)]
	[InlineData(2, 622)]
	[InlineData(41, 11410987)]
	public void LevelStartXp_IsOnePointAboveTheMinimumOfTheLevel(int level, int expected)
		=> PlayerLevel.LevelStartXp(level).Should().Be(expected);

	[Fact]
	public void LevelStartXp_UnsupportedLevel_Throws()
	{
		var act = () => PlayerLevel.LevelStartXp(86);

		act.Should().Throw<ArgumentOutOfRangeException>();
	}
}
