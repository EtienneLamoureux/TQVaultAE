using AwesomeAssertions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Entities;

public class BuffSourceTests
{
	[Theory]
	[InlineData(BuffGearRole.None, false)]
	[InlineData(BuffGearRole.Weapon, true)]
	[InlineData(BuffGearRole.Shield, true)]
	[InlineData(BuffGearRole.Other, true)]
	public void IsGear_EveryGearRoleButNone(BuffGearRole role, bool expected)
		=> new BuffSource { Id = "x", Group = BuffGroup.Gear, GearRole = role, Sum = new AttributeSum() }.IsGear.Should().Be(expected);

	[Fact]
	public void Defaults_LevelOne_OffWithoutOrigins()
	{
		var source = new BuffSource { Id = "shrine:x", Group = BuffGroup.Shrine, Sum = new AttributeSum() };

		(source.Level, source.DefaultOn, source.Origins.Count, source.NameTag).Should().Be((1, false, 0, string.Empty));
	}

	[Fact]
	public void BuffGroup_SectionsInTheTabsOrder()
		=> Enum.GetValues<BuffGroup>().Should().Equal(BuffGroup.Gear, BuffGroup.Passive, BuffGroup.Active, BuffGroup.Pet,
			BuffGroup.Curse, BuffGroup.Shrine, BuffGroup.Tartarus, BuffGroup.Scroll);
}
