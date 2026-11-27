using AwesomeAssertions;
using TQVaultAE.Application.Character;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Application;

/// <summary><see cref="ClassSetAttributeRules"/>: the attribute points of a class set (Health, Energy, Strength, Intelligence, Dexterity).</summary>
public class ClassSetAttributeRulesTests
{
	/// <summary>Health 300 + 2 points, Energy 300, Strength 50 + 10 points, Intelligence 50 + 4 points and 2 more (a quest reward), Dexterity 50 + 3 points.</summary>
	private static readonly int[] Values = [380, 300, 90, 68, 62];

	[Fact]
	public void PointsOf_CountsWholePointsOverEachMinimum()
		=> ClassSetAttributeRules.PointsOf(Values).Should().Be(new ClassSetAttributes(2, 0, 10, 4, 3));

	[Fact]
	public void Targets_EmptySet_GivesEveryPointBackAndKeepsWhatIsNotAWholePoint()
	{
		var (targets, complete) = ClassSetAttributeRules.Targets(Values, default, pool: 5);

		complete.Should().BeTrue();
		targets.Should().Equal(300, 300, 50, 52, 50);
		// Every value moves by whole points: the pool gets the 19 points spent (2 + 10 + 4 + 3), none lost.
		Values.Zip(targets).Sum(p => (p.First - p.Second) / (p.First >= 300 ? 40 : 4)).Should().Be(19);
	}

	[Fact]
	public void Targets_SetCapturedFromTheSameValues_GivesTheSameValues()
	{
		var (targets, complete) = ClassSetAttributeRules.Targets(Values, ClassSetAttributeRules.PointsOf(Values), pool: 0);

		complete.Should().BeTrue();
		targets.Should().Equal(Values);
	}

	[Fact]
	public void Targets_NotEnoughPoints_GivesThemInTheGameOrderAndSaysItIsPartial()
	{
		// 19 points spent now and 1 left: 20 for a set asking 30.
		var (targets, complete) = ClassSetAttributeRules.Targets(Values, new ClassSetAttributes(5, 5, 5, 5, 10), pool: 1);

		complete.Should().BeFalse();
		targets.Should().Equal(300 + (5 * 40), 300 + (5 * 40), 50 + (5 * 4), 52 + (5 * 4), 50);
	}
}
