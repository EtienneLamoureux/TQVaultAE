using AwesomeAssertions;
using Moq;
using System.Collections.ObjectModel;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Data;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Data;

public class ItemAttributeListCompareTests
{
	private readonly Mock<IItemAttributeProvider> _mockItemAttributeProvider;

	public ItemAttributeListCompareTests()
	{
		_mockItemAttributeProvider = new Mock<IItemAttributeProvider>();
	}

	[Fact]
	public void Compare_WithNullAttributeData_ReturnsCorrectOrder()
	{
		// Arrange - non-armor comparer
		var comparer = new ItemAttributeListCompare(false, _mockItemAttributeProvider.Object);
		var list1 = new List<Variable> { new Variable("nullAttr", VariableDataType.Integer, 1) };
		var list2 = new List<Variable> { new Variable("anotherNull", VariableDataType.Integer, 1) };

		_mockItemAttributeProvider.Setup(p => p.GetAttributeData(It.IsAny<string>()))
			.Returns((ItemAttributesData?)null);

		// Act
		var result = ((IComparer<List<Variable>>)comparer).Compare(list1, list2);

		// Assert - both return same order (3000000), so result should be 0
		result.Should().Be(0);
	}

	[Fact]
	public void Compare_ItemSkillName_ReturnsHighestOrder()
	{
		// Arrange - itemSkillName should return 4000000 (highest)
		var comparer = new ItemAttributeListCompare(false, _mockItemAttributeProvider.Object);
		var skillList = new List<Variable> { new Variable("itemSkillName", VariableDataType.StringVar, 1) };
		var normalList = new List<Variable> { CreateVariableWithEffect("normalAttr", ItemAttributesEffectType.Offense, 0) };

		_mockItemAttributeProvider.Setup(p => p.GetAttributeData("normalAttr"))
			.Returns(new ItemAttributesData(ItemAttributesEffectType.Offense, "normalAttr", "effect", "var", 0));

		// Act
		var result = ((IComparer<List<Variable>>)comparer).Compare(skillList, normalList);

		// Assert - skill should come after (positive)
		result.Should().BePositive();
	}

	[Fact]
	public void Compare_NormalAttributeComesBeforeSkill()
	{
		// Arrange
		var comparer = new ItemAttributeListCompare(false, _mockItemAttributeProvider.Object);
		var normalList = new List<Variable> { CreateVariableWithEffect("normalAttr", ItemAttributesEffectType.Offense, 0) };
		var skillList = new List<Variable> { new Variable("itemSkillName", VariableDataType.StringVar, 1) };

		_mockItemAttributeProvider.Setup(p => p.GetAttributeData("normalAttr"))
			.Returns(new ItemAttributesData(ItemAttributesEffectType.Offense, "normalAttr", "effect", "var", 0));

		// Act
		var result = ((IComparer<List<Variable>>)comparer).Compare(normalList, skillList);

		// Assert - normal should come before skill (negative)
		result.Should().BeNegative();
	}

	[Fact]
	public void Compare_WithArmorFlag_DefenseTypeOrderChanges()
	{
		// Arrange - armor comparer reorders Defense to position 1
		var armorComparer = new ItemAttributeListCompare(true, _mockItemAttributeProvider.Object);
		var nonArmorComparer = new ItemAttributeListCompare(false, _mockItemAttributeProvider.Object);

		var defenseList = new List<Variable> { CreateVariableWithEffect("defenseAttr", ItemAttributesEffectType.Defense, 0) };
		var offenseList = new List<Variable> { CreateVariableWithEffect("offenseAttr", ItemAttributesEffectType.Offense, 0) };

		_mockItemAttributeProvider.Setup(p => p.GetAttributeData(It.IsAny<string>()))
			.Returns((string name) => name.Contains("defense")
				? new ItemAttributesData(ItemAttributesEffectType.Defense, name, "effect", "var", 0)
				: new ItemAttributesData(ItemAttributesEffectType.Offense, name, "effect", "var", 0));

		// Act - With armor, Defense should come before Offense
		var armorResult = ((IComparer<List<Variable>>)armorComparer).Compare(defenseList, offenseList);

		// Assert - With armor, Defense (order 1*1000) comes before Offense (order 2*1000)
		armorResult.Should().BeNegative();
	}

	[Fact]
	public void Compare_WithoutArmorFlag_DefenseTypeNormalOrder()
	{
		// Arrange
		var nonArmorComparer = new ItemAttributeListCompare(false, _mockItemAttributeProvider.Object);

		var defenseList = new List<Variable> { CreateVariableWithEffect("defenseAttr", ItemAttributesEffectType.Defense, 0) };
		var offenseList = new List<Variable> { CreateVariableWithEffect("offenseAttr", ItemAttributesEffectType.Offense, 0) };

		_mockItemAttributeProvider.Setup(p => p.GetAttributeData(It.IsAny<string>()))
			.Returns((string name) => name.Contains("defense")
				? new ItemAttributesData(ItemAttributesEffectType.Defense, name, "effect", "var", 0)
				: new ItemAttributesData(ItemAttributesEffectType.Offense, name, "effect", "var", 0));

		// Act - Without armor, Defense (9) should come after Offense (2)
		var result = ((IComparer<List<Variable>>)nonArmorComparer).Compare(defenseList, offenseList);

		// Assert - Defense (9*1000) comes after Offense (2*1000)
		result.Should().BePositive();
	}

	[Fact]
	public void Compare_ShieldEffectAlwaysComesFirst()
	{
		// Arrange - Shield effect should always be position 0 regardless of armor
		var armorComparer = new ItemAttributeListCompare(true, _mockItemAttributeProvider.Object);

		var shieldList = new List<Variable> { CreateVariableWithEffect("shieldAttr", ItemAttributesEffectType.ShieldEffect, 0) };
		var defenseList = new List<Variable> { CreateVariableWithEffect("defenseAttr", ItemAttributesEffectType.Defense, 0) };

		_mockItemAttributeProvider.Setup(p => p.GetAttributeData(It.IsAny<string>()))
			.Returns((string name) => name.Contains("shield")
				? new ItemAttributesData(ItemAttributesEffectType.ShieldEffect, name, "effect", "var", 0)
				: new ItemAttributesData(ItemAttributesEffectType.Defense, name, "effect", "var", 0));

		// Act
		var result = ((IComparer<List<Variable>>)armorComparer).Compare(shieldList, defenseList);

		// Assert - Shield should come first
		result.Should().BeNegative();
	}

	[Fact]
	public void Compare_BaseAttackSpeed_GetsSpecialOrder()
	{
		// Arrange - characterBaseAttackSpeedTag gets special ordering after piercing
		var comparer = new ItemAttributeListCompare(false, _mockItemAttributeProvider.Object);

		var speedList = new List<Variable> { new Variable("characterBaseAttackSpeedTag", VariableDataType.Integer, 1) };
		var piercingList = new List<Variable> { CreateVariableWithEffect("offensivePierceRatioMin", ItemAttributesEffectType.Offense, 0) };

		_mockItemAttributeProvider.Setup(p => p.GetAttributeData("offensivePierceRatioMin"))
			.Returns(new ItemAttributesData(ItemAttributesEffectType.Offense, "offensivePierceRatioMin", "effect", "var", 0));

		// Act
		var result = ((IComparer<List<Variable>>)comparer).Compare(speedList, piercingList);

		// Assert - speed should come right after piercing (positive = speed is higher)
		result.Should().BePositive();
	}

	[Fact]
	public void Compare_RetaliationGlobalChance_GetsSpecialOrder()
	{
		// Arrange - retaliationGlobalChance comes at beginning of retaliation group
		// It gets MakeGlobal(CalcBaseOrder(Retaliation, 0) - 1) which is less than normal retaliation
		var comparer = new ItemAttributeListCompare(false, _mockItemAttributeProvider.Object);

		var retaliationGlobalList = new List<Variable> { new Variable("retaliationGlobalChance", VariableDataType.Integer, 1) };
		var normalRetaliationList = new List<Variable> { CreateVariableWithEffect("retaliationAttr", ItemAttributesEffectType.Retaliation, 0) };

		_mockItemAttributeProvider.Setup(p => p.GetAttributeData("retaliationAttr"))
			.Returns(new ItemAttributesData(ItemAttributesEffectType.Retaliation, "retaliationAttr", "effect", "var", 0));

		// Act
		var result = ((IComparer<List<Variable>>)comparer).Compare(retaliationGlobalList, normalRetaliationList);

		// Assert - retaliationGlobal gets MakeGlobal(order - 1), so it should come before normal retaliation
		// Result is positive means first argument (retaliationGlobal) has higher order
		// Since retaliationGlobal is made global, it gets +10000000, so it comes AFTER
		result.Should().BePositive();
	}

	[Fact]
	public void Compare_OffensiveGlobalChance_GetsSpecialOrder()
	{
		// Arrange - offensiveGlobalChance comes at beginning of offensive group
		var comparer = new ItemAttributeListCompare(false, _mockItemAttributeProvider.Object);

		var offensiveGlobalList = new List<Variable> { new Variable("offensiveGlobalChance", VariableDataType.Integer, 1) };
		var normalOffenseList = new List<Variable> { CreateVariableWithEffect("offenseAttr", ItemAttributesEffectType.Offense, 0) };

		_mockItemAttributeProvider.Setup(p => p.GetAttributeData("offenseAttr"))
			.Returns(new ItemAttributesData(ItemAttributesEffectType.Offense, "offenseAttr", "effect", "var", 0));

		// Act
		var result = ((IComparer<List<Variable>>)comparer).Compare(offensiveGlobalList, normalOffenseList);

		// Assert - offensiveGlobal gets MakeGlobal(order - 1), so it should come before normal offense
		// But since it's made global (+10000000), it actually comes after
		result.Should().BePositive();
	}

	[Fact]
	public void Compare_GlobalAttributes_GetHigherOrder()
	{
		// Arrange - attributes with "Global" group get made global (order + 10000000)
		var comparer = new ItemAttributeListCompare(false, _mockItemAttributeProvider.Object);

		var globalList = new List<Variable> { CreateVariableWithEffect("globalAttr", ItemAttributesEffectType.Offense, 0) };
		var normalList = new List<Variable> { CreateVariableWithEffect("normalAttr", ItemAttributesEffectType.Offense, 0) };

		_mockItemAttributeProvider.Setup(p => p.GetAttributeData("globalAttr"))
			.Returns(new ItemAttributesData(ItemAttributesEffectType.Offense, "globalAttr", "effect", "var", 0));
		_mockItemAttributeProvider.Setup(p => p.GetAttributeData("normalAttr"))
			.Returns(new ItemAttributesData(ItemAttributesEffectType.Offense, "normalAttr", "effect", "var", 0));
		_mockItemAttributeProvider.Setup(p => p.AttributeGroupHas(It.IsAny<Collection<Variable>>(), "Global"))
			.Returns((Collection<Variable> vars, string group) => vars[0].Name.Contains("global"));

		// Act
		var result = ((IComparer<List<Variable>>)comparer).Compare(globalList, normalList);

		// Assert - global should come after (positive) due to +10000000
		result.Should().BePositive();
	}

	[Fact]
	public void Compare_WithArmorFlag_OffensivePhysicalMinGetsSpecialOrder()
	{
		// Arrange - offensivePhysicalMin gets special ordering after blockRecoveryTime when armor
		var comparer = new ItemAttributeListCompare(true, _mockItemAttributeProvider.Object);

		var physicalList = new List<Variable> { new Variable("offensivePhysicalMin", VariableDataType.Integer, 1) };
		var blockRecoveryList = new List<Variable> { CreateVariableWithEffect("blockRecoveryTime", ItemAttributesEffectType.Defense, 0) };

		_mockItemAttributeProvider.Setup(p => p.GetAttributeData("blockRecoveryTime"))
			.Returns(new ItemAttributesData(ItemAttributesEffectType.Defense, "blockRecoveryTime", "effect", "var", 0));

		// Act
		var result = ((IComparer<List<Variable>>)comparer).Compare(physicalList, blockRecoveryList);

		// Assert - physical should come right after blockRecovery
		result.Should().BePositive();
	}

	[Fact]
	public void Compare_UnknownAttribute_GetsOrder3000000()
	{
		// Arrange
		var comparer = new ItemAttributeListCompare(false, _mockItemAttributeProvider.Object);
		var unknownList = new List<Variable> { new Variable("unknownAttr", VariableDataType.Integer, 1) };
		var knownList = new List<Variable> { CreateVariableWithEffect("knownAttr", ItemAttributesEffectType.Other, 0) };

		_mockItemAttributeProvider.Setup(p => p.GetAttributeData("knownAttr"))
			.Returns(new ItemAttributesData(ItemAttributesEffectType.Other, "knownAttr", "effect", "var", 0));
		_mockItemAttributeProvider.Setup(p => p.GetAttributeData("unknownAttr"))
			.Returns((ItemAttributesData?)null);

		// Act
		var result = ((IComparer<List<Variable>>)comparer).Compare(unknownList, knownList);

		// Assert - unknown (3000000) should come after Other (11000), so positive
		result.Should().BePositive();
	}

	[Fact]
	public void Compare_SameOrder_ReturnsZero()
	{
		// Arrange
		var comparer = new ItemAttributeListCompare(false, _mockItemAttributeProvider.Object);
		var list1 = new List<Variable> { CreateVariableWithEffect("attr1", ItemAttributesEffectType.Offense, 0) };
		var list2 = new List<Variable> { CreateVariableWithEffect("attr2", ItemAttributesEffectType.Offense, 0) };

		_mockItemAttributeProvider.Setup(p => p.GetAttributeData(It.IsAny<string>()))
			.Returns((string name) => new ItemAttributesData(ItemAttributesEffectType.Offense, name, "effect", "var", 0));

		// Act
		var result = ((IComparer<List<Variable>>)comparer).Compare(list1, list2);

		// Assert
		result.Should().Be(0);
	}

	private Variable CreateVariableWithEffect(string name, ItemAttributesEffectType effectType, int suborder)
	{
		var variable = new Variable(name, VariableDataType.Integer, 1);
		var attributeData = new ItemAttributesData(effectType, name, "effect", "var", suborder);
		_mockItemAttributeProvider.Setup(p => p.GetAttributeData(name))
			.Returns(attributeData);
		return variable;
	}
}
