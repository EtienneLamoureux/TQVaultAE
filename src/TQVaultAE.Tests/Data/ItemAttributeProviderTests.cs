using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Data;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Tests.Data;

public class ItemAttributeProviderTests
{
	private readonly ItemAttributeProvider _provider;

	public ItemAttributeProviderTests()
	{
		_provider = new ItemAttributeProvider(Mock.Of<ILogger<ItemAttributeProvider>>());
	}

	#region SpanHelper Tests

	[Fact]
	public void SpanHelper_ToUpperSpan_ReturnsCorrectUppercase()
	{
		// Arrange
		var input = "defensiveBlockRecovery";

		// Act
		using var pooled = SpanHelper.ToUpperSpan(input);

		// Assert
		pooled.Span.ToString().Should().Be("DEFENSIVEBLOCKRECOVERY");
		pooled.Length.Should().Be(input.Length);
	}

	[Fact]
	public void SpanHelper_ToUpperSpan_HandlesEmptyString()
	{
		// Act
		using var pooled = SpanHelper.ToUpperSpan(string.Empty);

		// Assert
		pooled.Span.Length.Should().Be(0);
	}

	[Fact]
	public void SpanHelper_StartsWithIgnoreCase_Matches()
	{
		// Arrange
		var span = "defensiveSlowPhysical".AsSpan();

		// Act & Assert
		SpanHelper.StartsWithIgnoreCase(span, "DEFENSIVE").Should().BeTrue();
		SpanHelper.StartsWithIgnoreCase(span, "defensive").Should().BeTrue();
		SpanHelper.StartsWithIgnoreCase(span, "Defensive").Should().BeTrue();
		SpanHelper.StartsWithIgnoreCase(span, "ATTACK").Should().BeFalse();
	}

	[Fact]
	public void SpanHelper_EqualsIgnoreCase_Matches()
	{
		// Arrange
		var span1 = "defensiveBlockRecovery".AsSpan();
		var span2 = "DEFENSIVEBLOCKRECOVERY".AsSpan();
		var span3 = "defensiveBlockRecoveryTime".AsSpan();

		// Act & Assert
		SpanHelper.EqualsIgnoreCase(span1, span2).Should().BeTrue();
		SpanHelper.EqualsIgnoreCase(span1, span3).Should().BeFalse();
	}

	[Fact]
	public void SpanHelper_ConcatWithSlice_ConcatenatesCorrectly()
	{
		// Arrange
		var prefix = "Defense".AsSpan();
		var span = "defensiveBlockRecovery".AsSpan();

		// Act
		var result = SpanHelper.ConcatWithSlice(prefix, span, 9);

		// Assert
		result.Should().Be("DefenseBlockRecovery");
	}

	[Fact]
	public void SpanHelper_ConcatWithSlice_WithLength_ConcatenatesCorrectly()
	{
		// Arrange
		var prefix = "DamageModifier".AsSpan();
		var span = "offensiveChaosModifier".AsSpan();

		// Act - skip 9 chars ("offensive"), keep 5 chars ("chaos")
		var result = SpanHelper.ConcatWithSlice(prefix, span, 9, 5);

		// Assert
		result.Should().Be("DamageModifierChaos");
	}

	[Fact]
	public void PooledCharArray_DisposesCorrectly()
	{
		// Arrange
		var pooled = PooledCharArray.Rent(100);

		// Act
		pooled.Dispose();

		// Assert
		pooled.Span.Length.Should().Be(0);
	}

	#endregion

	#region GetAttributeTextTag Tests

	[Fact]
	public void GetAttributeTextTag_ShieldEffect_TransformsCorrectly()
	{
		// Arrange - ShieldEffect type
		var data = new ItemAttributesData(
			ItemAttributesEffectType.ShieldEffect,
			"defensiveBlockRecovery",
			"defensiveBlockRecovery",
			"Min",
			0);

		// Act
		var result = _provider.GetAttributeTextTag(data);

		// Assert
		result.Should().Be("DefenseBlockRecovery");
	}

	[Fact]
	public void GetAttributeTextTag_ShieldEffect_BlockRecovery_ReturnsSpecial()
	{
		// Arrange - Special case: BLOCKRECOVERYTIME
		var data = new ItemAttributesData(
			ItemAttributesEffectType.ShieldEffect,
			"blockRecoveryTime",
			"blockRecoveryTime",
			"Min",
			0);

		// Act
		var result = _provider.GetAttributeTextTag(data);

		// Assert
		result.Should().Be("ShieldBlockRecoveryTime");
	}

	[Fact]
	public void GetAttributeTextTag_Defense_TransformsCorrectly()
	{
		// Arrange - Defense type
		var data = new ItemAttributesData(
			ItemAttributesEffectType.Defense,
			"defensivePhysical",
			"defensivePhysical",
			"Min",
			0);

		// Act
		var result = _provider.GetAttributeTextTag(data);

		// Assert
		result.Should().Be("DefensePhysical");
	}

	[Fact]
	public void GetAttributeTextTag_Defense_OrphanTags_ReturnAsIs()
	{
		// Arrange - Orphan tags that should return as-is
		var data1 = new ItemAttributesData(
			ItemAttributesEffectType.Defense,
			"defensiveTotalSpeedChance",
			"defensiveTotalSpeedChance",
			"Min",
			0);

		var data2 = new ItemAttributesData(
			ItemAttributesEffectType.Defense,
			"defensiveAbsorption",
			"defensiveAbsorption",
			"Min",
			0);

		// Act
		var result1 = _provider.GetAttributeTextTag(data1);
		var result2 = _provider.GetAttributeTextTag(data2);

		// Assert
		result1.Should().Be("defensiveTotalSpeedChance");
		result2.Should().Be("defensiveAbsorption");
	}

	[Fact]
	public void GetAttributeTextTag_Offensive_TransformsCorrectly()
	{
		// Arrange - Offense type - OFFENSIVEPHYSICAL maps to DamageBasePhysical
		var data = new ItemAttributesData(
			ItemAttributesEffectType.Offense,
			"offensivePhysical",
			"offensivePhysical",
			"Min",
			0);

		// Act
		var result = _provider.GetAttributeTextTag(data);

		// Assert
		result.Should().Be("DamageBasePhysical");
	}

	[Fact]
	public void GetAttributeTextTag_OffensiveSlow_TransformsCorrectly()
	{
		// Arrange - OffenseSlow type
		var data = new ItemAttributesData(
			ItemAttributesEffectType.OffenseSlow,
			"offensiveSlowPhysical",
			"offensiveSlowPhysical",
			"Min",
			0);

		// Act
		var result = _provider.GetAttributeTextTag(data);

		// Assert
		result.Should().Be("DamageDurationPhysical");
	}

	[Fact]
	public void GetAttributeTextTag_RetaliationSlow_TransformsCorrectly()
	{
		// Arrange - RetaliationSlow type
		var data = new ItemAttributesData(
			ItemAttributesEffectType.RetaliationSlow,
			"retaliationSlowPhysical",
			"retaliationSlowPhysical",
			"Min",
			0);

		// Act
		var result = _provider.GetAttributeTextTag(data);

		// Assert
		result.Should().Be("RetaliationDurationPhysical");
	}

	[Fact]
	public void GetAttributeTextTag_SkillEffect_StripsSkill()
	{
		// Arrange - SkillEffect type
		var data = new ItemAttributesData(
			ItemAttributesEffectType.SkillEffect,
			"skillFireball",
			"skillFireball",
			"Min",
			0);

		// Act
		var result = _provider.GetAttributeTextTag(data);

		// Assert
		result.Should().Be("Fireball");
	}

	[Fact]
	public void GetAttributeTextTag_SkillEffect_Projectile_StripsProjectile()
	{
		// Arrange - Projectile skill
		var data = new ItemAttributesData(
			ItemAttributesEffectType.SkillEffect,
			"projectileLightning",
			"projectileLightning",
			"Min",
			0);

		// Act
		var result = _provider.GetAttributeTextTag(data);

		// Assert
		result.Should().Be("Lightning");
	}

	[Fact]
	public void GetAttributeTextTag_Other_ReturnsEffectAsIs()
	{
		// Arrange - Other type
		var data = new ItemAttributesData(
			ItemAttributesEffectType.Other,
			"someOtherEffect",
			"someOtherEffect",
			"",
			0);

		// Act
		var result = _provider.GetAttributeTextTag(data);

		// Assert
		result.Should().Be("someOtherEffect");
	}

	[Fact]
	public void GetAttributeTextTag_NullData_ReturnsEmpty()
	{
		// Act
		var result = _provider.GetAttributeTextTag((ItemAttributesData)null);

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public void GetAttributeTextTag_String_CallsCorrectOverload()
	{
		// Arrange - Use an attribute that exists in the dictionary
		var attribute = "defensiveCold";

		// Act
		var result = _provider.GetAttributeTextTag(attribute);

		// Assert
		result.Should().Be("DefenseCold");
	}

	[Fact]
	public void GetAttributeTextTag_String_UnknownAttribute_ReturnsOriginal()
	{
		// Arrange
		var attribute = "unknownAttribute";

		// Act
		var result = _provider.GetAttributeTextTag(attribute);

		// Assert
		result.Should().Be("unknownAttribute");
	}

	#endregion

	#region GetAttributeData Tests

	[Fact]
	public void GetAttributeData_ReturnsCorrectData()
	{
		// Arrange - Use an attribute that exists in the dictionary
		var attribute = "defensiveCold";

		// Act
		var result = _provider.GetAttributeData(attribute);

		// Assert
		result.Should().NotBeNull();
		result!.Effect.Should().Be("defensiveCold");
		result.EffectType.Should().Be(ItemAttributesEffectType.Defense);
	}

	[Fact]
	public void GetAttributeData_EmptyString_ReturnsNull()
	{
		// Act
		var result = _provider.GetAttributeData(string.Empty);

		// Assert
		result.Should().BeNull();
	}

	[Fact]
	public void GetAttributeData_UnknownAttribute_ReturnsNull()
	{
		// Act
		var result = _provider.GetAttributeData("unknownAttributeXYZ");

		// Assert
		result.Should().BeNull();
	}

	#endregion

	#region IsReagent Tests

	[Fact]
	public void IsReagent_KnownReagent_ReturnsTrue()
	{
		// Act - reagent1BaseName is in the reagents array
		var result = _provider.IsReagent("reagent1BaseName");

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public void IsReagent_UnknownReagent_ReturnsFalse()
	{
		// Act
		var result = _provider.IsReagent("notAReagent");

		// Assert
		result.Should().BeFalse();
	}

	#endregion
}
