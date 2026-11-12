using AwesomeAssertions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Tests.Entities;

public class MasterySkillTests
{
	private static readonly MasterySkill Maul = new("""Records\Skills\Nature\Maul.dbr""".ToRecordId(), "", "", "", 12, 2, null)
	{
		UltimateLevel = 16,
	};

	[Theory]
	[InlineData(0, 8, 0)]
	[InlineData(5, 0, 5)]
	[InlineData(5, 3, 8)]
	[InlineData(11, 8, 16)]
	public void EffectiveLevel_AddsTheEquipment_UpToTheUltimateLevel(int points, int bonus, int expected)
		=> Maul.EffectiveLevel(points, bonus).Should().Be(expected);

	[Fact]
	public void EffectiveLevel_WithoutUltimateLevel_IsNotCapped()
		=> (Maul with { UltimateLevel = 0 }).EffectiveLevel(11, 8).Should().Be(19);

	[Theory]
	[InlineData(0, 3, 0)]
	[InlineData(0, 0, 0)]
	[InlineData(5, 3, 3)]
	[InlineData(11, 8, 5)]
	[InlineData(16, 2, 0)]
	[InlineData(5, -2, 0)]
	public void ShownItemBonus_OnlyOnceTheSkillHasAPoint_UpToTheUltimateLevel(int points, int bonus, int expected)
		=> Maul.ShownItemBonus(points, bonus).Should().Be(expected);

	[Theory]
	[InlineData(0, 0, true)]
	[InlineData(0, 8, true)]
	[InlineData(7, 0, true)]
	[InlineData(12, 0, true)]
	[InlineData(12, 3, true)]
	[InlineData(12, 4, false)]
	[InlineData(11, 8, false)]
	public void HasNextLevel_BelowTheRecordsTopLevel_EquipmentIncluded(int points, int bonus, bool expected)
		=> Maul.HasNextLevel(points, bonus).Should().Be(expected);

	[Fact]
	public void HasNextLevel_WithoutUltimateLevel_StopsAtTheMaximum()
	{
		var skill = Maul with { UltimateLevel = 0 };

		skill.HasNextLevel(11, 0).Should().BeTrue();
		skill.HasNextLevel(12, 0).Should().BeFalse();
	}
}
