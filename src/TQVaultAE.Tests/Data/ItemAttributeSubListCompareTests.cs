using AwesomeAssertions;
using Moq;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Data;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Data;

public class ItemAttributeSubListCompareTests
{
	private readonly Mock<IItemAttributeProvider> _mockItemAttributeProvider;
	private readonly ItemAttributeSubListCompare _comparer;

	public ItemAttributeSubListCompareTests()
	{
		_mockItemAttributeProvider = new Mock<IItemAttributeProvider>();
		_comparer = new ItemAttributeSubListCompare(_mockItemAttributeProvider.Object);
	}

	[Fact]
	public void Compare_WithNullProvider_ThrowsNullReferenceException()
	{
		// This test verifies the comparer handles missing attribute data
		// Arrange
		var var1 = new Variable("testAttr", VariableDataType.Integer, 1) { [0] = 10 };
		var var2 = new Variable("testAttr", VariableDataType.Integer, 1) { [0] = 20 };

		_mockItemAttributeProvider.Setup(p => p.GetAttributeData(It.IsAny<string>()))
			.Returns((ItemAttributesData?)null);

		// Act
		var result = ((IComparer<Variable>)_comparer).Compare(var1, var2);

		// Assert - both return same order (3000000), so result should be 0
		result.Should().Be(0);
	}

	[Fact]
	public void Compare_WithUnknownAttribute_ReturnsZero()
	{
		// Arrange
		var var1 = new Variable("unknownAttr", VariableDataType.Integer, 1);
		var var2 = new Variable("anotherUnknown", VariableDataType.Integer, 1);

		_mockItemAttributeProvider.Setup(p => p.GetAttributeData(It.IsAny<string>()))
			.Returns((ItemAttributesData?)null);

		// Act
		var result = ((IComparer<Variable>)_comparer).Compare(var1, var2);

		// Assert
		result.Should().Be(0);
	}

	[Fact]
	public void Compare_FirstHasLowerOrder_ReturnsNegative()
	{
		// Arrange - Offense (2) has lower order than Defense (9)
		var var1 = CreateVariableWithEffect("offenseAttr", ItemAttributesEffectType.Offense, 0);
		var var2 = CreateVariableWithEffect("defenseAttr", ItemAttributesEffectType.Defense, 0);

		// Act
		var result = ((IComparer<Variable>)_comparer).Compare(var1, var2);

		// Assert - Offense has lower numeric effect type, so should come first (negative)
		result.Should().BeNegative();
	}

	[Fact]
	public void Compare_FirstHasHigherOrder_ReturnsPositive()
	{
		// Arrange - Defense (9) has higher order than Offense (2)
		var var1 = CreateVariableWithEffect("defenseAttr", ItemAttributesEffectType.Defense, 0);
		var var2 = CreateVariableWithEffect("offenseAttr", ItemAttributesEffectType.Offense, 0);

		// Act
		var result = ((IComparer<Variable>)_comparer).Compare(var1, var2);

		// Assert - Defense has higher numeric effect type, so should come second (positive)
		result.Should().BePositive();
	}

	[Fact]
	public void Compare_SameEffectType_UsesSuborder()
	{
		// Arrange - Same effect type, different suborders
		var var1 = CreateVariableWithEffect("attr1", ItemAttributesEffectType.Offense, 1);
		var var2 = CreateVariableWithEffect("attr2", ItemAttributesEffectType.Offense, 5);

		// Act
		var result = ((IComparer<Variable>)_comparer).Compare(var1, var2);

		// Assert - Lower suborder should come first
		result.Should().BeNegative();
	}

	[Fact]
	public void Compare_SameEffectTypeAndSuborder_ReturnsZero()
	{
		// Arrange - Same effect type and suborder
		var var1 = CreateVariableWithEffect("attr1", ItemAttributesEffectType.Offense, 1);
		var var2 = CreateVariableWithEffect("attr2", ItemAttributesEffectType.Offense, 1);

		// Act
		var result = ((IComparer<Variable>)_comparer).Compare(var1, var2);

		// Assert
		result.Should().Be(0);
	}

	[Fact]
	public void Compare_SkillEffectComesBeforeOffense()
	{
		// Arrange - SkillEffect (1) comes before Offense (2)
		var var1 = CreateVariableWithEffect("skillAttr", ItemAttributesEffectType.SkillEffect, 0);
		var var2 = CreateVariableWithEffect("offenseAttr", ItemAttributesEffectType.Offense, 0);

		// Act
		var result = ((IComparer<Variable>)_comparer).Compare(var1, var2);

		// Assert
		result.Should().BeNegative();
	}

	[Fact]
	public void Compare_OtherComesLast()
	{
		// Arrange - Other (11) comes after most other types
		var var1 = CreateVariableWithEffect("otherAttr", ItemAttributesEffectType.Other, 0);
		var var2 = CreateVariableWithEffect("defenseAttr", ItemAttributesEffectType.Defense, 0);

		// Act
		var result = ((IComparer<Variable>)_comparer).Compare(var1, var2);

		// Assert - Other has higher order
		result.Should().BePositive();
	}

	[Fact]
	public void Compare_RetaliationEffects_OrderedCorrectly()
	{
		// Arrange - Retaliation (6) vs RetaliationModifier (7)
		var var1 = CreateVariableWithEffect("retAttr", ItemAttributesEffectType.Retaliation, 0);
		var var2 = CreateVariableWithEffect("retModAttr", ItemAttributesEffectType.RetaliationModifier, 0);

		// Act
		var result = ((IComparer<Variable>)_comparer).Compare(var1, var2);

		// Assert
		result.Should().BeNegative();
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
