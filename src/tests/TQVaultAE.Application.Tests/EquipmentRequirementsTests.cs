using AwesomeAssertions;
using TQVaultAE.Application.Session;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Application;

public class EquipmentRequirementsTests
{
	private static SortedList<string, Variable> Requirements(int level = 0, int strength = 0, int dexterity = 0, int intelligence = 0)
	{
		var list = new SortedList<string, Variable>();
		void Add(string key, int value)
		{
			if (value != 0)
				list.Add(key, new Variable(key, VariableDataType.Integer, 1) { [0] = value });
		}

		Add(Variable.KEY_LEVELREQ, level);
		Add(Variable.KEY_STRENGTH, strength);
		Add(Variable.KEY_DEXTERITY, dexterity);
		Add(Variable.KEY_INTELLIGENCE, intelligence);
		return list;
	}

	private static RequirementAttributes Have(int level = 50, int strength = 100, int dexterity = 100, int intelligence = 100, params (string Field, double Value)[] bonuses)
	{
		var gear = new AttributeSum();
		foreach (var (field, value) in bonuses)
			gear.Add(field, value);
		return new RequirementAttributes(level, strength, dexterity, intelligence, gear);
	}

	[Theory]
	[InlineData(Item.ICLASS_UPPERBODY, "Armor")]
	[InlineData("ArmorMisc_Vestment", "Armor")]
	[InlineData(Item.ICLASS_RING, "Jewelry")]
	[InlineData(Item.ICLASS_SWORD, "Melee")]
	[InlineData(Item.ICLASS_BOW, "Hunting")]
	[InlineData(Item.ICLASS_RANGEDONEHAND, "Hunting")]
	[InlineData(Item.ICLASS_STAFF, "Staff")]
	[InlineData(Item.ICLASS_SHIELD, "Shield")]
	[InlineData(Item.ICLASS_ARTIFACT, null)]
	[InlineData(null, null)]
	public void TypeOf_NamesTheReductionsOfTheItemsType(string? itemClass, string? type)
		=> EquipmentRequirements.TypeOf(itemClass).Should().Be(type);

	[Theory]
	[InlineData(100, 10, 90)]
	[InlineData(33, 10, 30)] // 29.7, rounded to the nearest
	[InlineData(35, 10, 32)] // 31.5
	[InlineData(0, 50, 0)]
	[InlineData(80, 0, 80)]
	public void Reduced_RoundsToTheNearest(int requirement, double reduction, int reduced)
		=> EquipmentRequirements.Reduced(requirement, reduction).Should().Be(reduced);

	[Fact]
	public void Meets_ArmorRequirement_LoweredByTheArmorAndGlobalReductions()
	{
		var armor = Requirements(strength: 120);

		EquipmentRequirements.Meets(armor, Item.ICLASS_UPPERBODY, Have(strength: 110)).Should().BeFalse();
		EquipmentRequirements.Meets(armor, Item.ICLASS_UPPERBODY, Have(strength: 110, bonuses: ("characterArmorStrengthReqReduction", 10))).Should().BeTrue("120 x 0.9 = 108");
		EquipmentRequirements.Meets(armor, Item.ICLASS_UPPERBODY, Have(strength: 110, bonuses: ("characterGlobalReqReduction", 10))).Should().BeTrue();
		EquipmentRequirements.Meets(armor, Item.ICLASS_UPPERBODY, Have(strength: 110, bonuses: ("characterJewelryStrengthReqReduction", 50))).Should().BeFalse("another type's reduction");
	}

	[Fact]
	public void Meets_Weapon_AddsTheWeaponsReductionToItsTypes()
	{
		var sword = Requirements(dexterity: 200);
		var have = Have(dexterity: 160, bonuses: [("characterMeleeDexterityReqReduction", 10), ("characterWeaponDexterityReqReduction", 10)]);

		EquipmentRequirements.Meets(sword, Item.ICLASS_SWORD, have).Should().BeTrue("200 x 0.8 = 160");
		EquipmentRequirements.Meets(sword, Item.ICLASS_RING, have).Should().BeFalse("no reduction for a ring");
	}

	[Fact]
	public void Reduction_Shield_TheWeaponsAndTheShields()
		=> EquipmentRequirements.Reduction(Item.ICLASS_SHIELD, Variable.KEY_STRENGTH,
			f => f is "characterWeaponStrengthReqReduction" or "characterShieldStrengthReqReduction" ? 5 : 0).Should().Be(10);

	[Fact]
	public void Meets_NegativeTypeReduction_CountsAsNone()
		=> EquipmentRequirements.Meets(Requirements(strength: 100), Item.ICLASS_HEAD, Have(strength: 100, bonuses: ("characterArmorStrengthReqReduction", -20)))
			.Should().BeTrue();

	[Fact]
	public void Meets_Level_LoweredByTheLevelReduction()
	{
		var relic = Requirements(level: 40);

		EquipmentRequirements.Meets(relic, Item.ICLASS_RELIC, Have(level: 36)).Should().BeFalse();
		EquipmentRequirements.Meets(relic, Item.ICLASS_RELIC, Have(level: 36, bonuses: ("characterLevelReqReduction", 10))).Should().BeTrue();
	}

	[Fact]
	public void Bonus_SumsTheGearAndTheSkills()
	{
		var gear = new AttributeSum();
		gear.Add("characterGlobalReqReduction", 5);
		var skills = new AttributeSum();
		skills.Add("characterGlobalReqReduction", 3);

		new RequirementAttributes(1, 1, 1, 1, gear, skills).Bonus("characterGlobalReqReduction").Should().Be(8);
		new RequirementAttributes(1, 1, 1, 1).Bonus("characterGlobalReqReduction").Should().Be(0);
	}

	[Fact]
	public void ReducedValues_GivesEachRequirementTheItemHas_OnceReduced()
	{
		var armor = Requirements(level: 30, strength: 430);
		var have = Have(bonuses: [("characterArmorStrengthReqReduction", 10), (EquipmentRequirements.LevelReduction, 5)]);

		EquipmentRequirements.ReducedValues(armor, Item.ICLASS_UPPERBODY, have).Should().BeEquivalentTo(new Dictionary<string, int>
		{
			[Variable.KEY_LEVELREQ] = 29, // 28.5, rounded to the nearest
			[Variable.KEY_STRENGTH] = 387,
		});
	}
}
