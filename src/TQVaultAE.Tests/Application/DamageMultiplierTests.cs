using AwesomeAssertions;
using TQVaultAE.Application;

namespace TQVaultAE.Tests.Application;

public class DamageMultiplierTests
{
	[Fact]
	public void Bleeding_ScalesWithDexterity()
	{
		var multiplier = DamageMultiplier.Of("bleedingDamageDV * ((dexterityDV / 500) + 1)", "bleedingDamageDV", 300, 230, 100, 40);

		multiplier.Should().NotBeNull();
		multiplier!.Value.Value.Should().BeApproximately(1.46, 1e-9);
		multiplier.Value.Attribute.Should().Be(MultiplierAttribute.Dexterity);
	}

	[Theory]
	[InlineData("(elementalDamageDV * ((intelligenceDV / 650) + 1)) + (intelligenceDV*0.025)", 1.28)] // Cold Damage at 182 Int
	[InlineData("elementalDamageDV * ((intelligenceDV / 500) + 1)", 1.364)] // Frostburn Damage
	public void Elemental_LeavesOutTheFlatPart(string equation, double expected)
		=> DamageMultiplier.Of(equation, "elementalDamageDV", 300, 100, 182, 40)!.Value.Value.Should().BeApproximately(expected, 1e-9);

	[Fact]
	public void LifeLeech_ScalesWithTheLevel()
		=> DamageMultiplier.Of("lifeLeachDamageDV  * (1 + (characterLevelDV / 40))", "lifeLeachDamageDV", 0, 0, 0, 60)
			.Should().Be(new GlobalMultiplier(2.5, MultiplierAttribute.Level));

	[Theory]
	[InlineData("(physicalDamageDV * ((strengthDV/500)+1)) + (strengthDV*0.04)", "physicalDamageDV")]
	[InlineData("(((pierceDamageDV / ((strengthDV/500)+1)) * ((strengthDV/2000)+1)) * ((dexterityDV/400)+1)) + (dexterityDV*0.03)", "pierceDamageDV")]
	[InlineData("0", "physicalDamageDV")]
	public void StrengthOrNoAttribute_HasNone(string equation, string variable)
		=> DamageMultiplier.Of(equation, variable, 300, 230, 182, 40).Should().BeNull();
}
