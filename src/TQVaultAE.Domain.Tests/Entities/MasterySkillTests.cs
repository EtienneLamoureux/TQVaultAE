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
}
