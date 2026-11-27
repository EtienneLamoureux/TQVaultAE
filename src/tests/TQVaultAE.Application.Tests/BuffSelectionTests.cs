using AwesomeAssertions;
using TQVaultAE.Application.Character;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Tests.Application;

public class BuffSelectionTests
{
	private static BuffSource Source(string id, bool defaultOn, BuffGearRole role = BuffGearRole.None, string variable = "v", double value = 1)
	{
		var sum = new AttributeSum();
		sum.Add(variable, value);
		return new BuffSource { Id = id, Group = role == BuffGearRole.None ? BuffGroup.Active : BuffGroup.Gear, DefaultOn = defaultOn, GearRole = role, Sum = sum };
	}

	[Fact]
	public void IsOn_StartsAtTheDefault_ToggleFlipsIt()
	{
		var selection = new BuffSelection();
		var passive = Source("skill:a", defaultOn: true);
		var buff = Source("skill:b", defaultOn: false);

		selection.IsOn(passive).Should().BeTrue();
		selection.IsOn(buff).Should().BeFalse();

		selection.Toggle(passive);
		selection.Toggle(buff);

		selection.IsOn(passive).Should().BeFalse();
		selection.IsOn(buff).Should().BeTrue();
	}

	[Fact]
	public void Toggle_KeyIgnoresCase_AndSurvivesANewListOfSources()
	{
		var selection = new BuffSelection();
		selection.Toggle(Source("curse:RECORDS\\A.DBR", defaultOn: true));

		selection.IsOn(Source("curse:records\\a.dbr", defaultOn: true)).Should().BeFalse("the sources are read again when the skills change");
	}

	[Fact]
	public void Version_ChangesWithEachToggleAndScroll_NotWithoutChange()
	{
		var selection = new BuffSelection();
		var source = Source("skill:a", defaultOn: false);

		selection.Set(source, false);
		selection.Version.Should().Be(0);

		selection.Toggle(source);
		selection.PickScroll("""records\scroll.dbr""".ToRecordId());
		selection.PickScroll("""records\scroll.dbr""".ToRecordId());

		selection.Version.Should().Be(2);
		selection.Scroll.Should().Be("""records\scroll.dbr""".ToRecordId());
	}

	[Fact]
	public void Sum_SplitsGearAndSkills_LeavesOutTheSourcesOffAndTheOneWithout()
	{
		var item = Source("item:0", true, BuffGearRole.Other, "defensiveFire", 10);
		var skill = Source("skill:a", true, variable: "defensiveFire", value: 5);
		var curse = Source("curse:a", true, variable: "defensiveFire", value: -30);
		var off = Source("shrine:a", false, variable: "defensiveFire", value: 100);
		BuffSource[] sources = [item, skill, curse, off];
		var selection = new BuffSelection();

		var (gear, skills) = BuffSums.Sum(sources, selection.IsOn);
		gear["defensiveFire"].Should().Be(10);
		skills["defensiveFire"].Should().Be(-25);

		var (_, withoutCurse) = BuffSums.Sum(sources, selection.IsOn, curse);
		withoutCurse["defensiveFire"].Should().Be(5);
	}

	[Fact]
	public void Offense_WeaponsApart_OtherGearTogether_ShieldInNeither()
	{
		var sword = Source("item:0", true, BuffGearRole.Weapon, "sword");
		var shield = Source("item:1", true, BuffGearRole.Shield, "shield");
		var ring = Source("item:2", true, BuffGearRole.Other, "ring");
		var set = Source("set:a", true, BuffGearRole.Other, "set");
		var offRing = Source("item:3", false, BuffGearRole.Other, "offRing");

		var gear = BuffSums.Offense([sword, shield, ring, set, offRing], new BuffSelection().IsOn);

		gear.Weapons.Should().ContainSingle().Which["sword"].Should().Be(1);
		gear.Other.Values.Keys.Should().BeEquivalentTo(["ring", "set"]);
	}

	[Theory]
	[InlineData("ArmorJewelry_Ring", false, BuffGearRole.Other)]
	[InlineData("WeaponMelee_Sword", true, BuffGearRole.Weapon)]
	[InlineData("WeaponArmor_Shield", true, BuffGearRole.Shield)]
	[InlineData("", true, BuffGearRole.None)]
	public void GearRoleOf_ByClassAndSlot(string itemClass, bool weaponSlot, BuffGearRole expected)
	{
		var record = new DBRecordCollection("""records\item\x.dbr""".ToRecordId(), itemClass);
		record.Set(new Variable("Class", VariableDataType.StringVar, 1) { [0] = itemClass });
		var item = new Item { PositionX = weaponSlot ? Item.WeaponSlotIndicator : 3, baseItemInfo = itemClass.Length == 0 ? null : new Info(record) };

		BuffSums.GearRoleOf(item).Should().Be(expected);
	}

	[Fact]
	public void ResistanceVariables_FireAddsTheElementalResistance_PierceDoesNot()
	{
		CharacterStatsCalculator.ResistanceVariables(StatResistance.Fire).Should().Equal("defensiveFire", "defensiveFireModifier", "defensiveElementalResistance");
		CharacterStatsCalculator.ResistanceVariables(StatResistance.Pierce).Should().Equal("defensivePierce", "defensivePierceModifier");
	}
}
