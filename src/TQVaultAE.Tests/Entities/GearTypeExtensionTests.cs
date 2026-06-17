using AwesomeAssertions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Entities;

public class GearTypeExtensionTests
{
	#region GetItemClass Tests

	[Theory]
	[InlineData(GearType.Head)]
	[InlineData(GearType.Torso)]
	[InlineData(GearType.Arm)]
	[InlineData(GearType.Leg)]
	[InlineData(GearType.Ring)]
	[InlineData(GearType.Amulet)]
	[InlineData(GearType.Spear)]
	[InlineData(GearType.Staff)]
	[InlineData(GearType.Thrown)]
	[InlineData(GearType.Bow)]
	[InlineData(GearType.Sword)]
	[InlineData(GearType.Mace)]
	[InlineData(GearType.Axe)]
	[InlineData(GearType.Shield)]
	public void GetItemClass_BasicGearTypes_ReturnsNonEmpty(GearType type)
	{
		// Act
		var result = type.GetItemClass();

		// Assert
		result.Should().NotBeNullOrEmpty();
	}

	[Theory]
	[InlineData(GearType.Undefined)]
	[InlineData(GearType.MonsterInfrequent)]
	[InlineData(GearType.ForMage)]
	[InlineData(GearType.ForMelee)]
	public void GetItemClass_ModifierTypes_ReturnsEmpty(GearType type)
	{
		// Act
		var result = type.GetItemClass();

		// Assert
		result.Should().BeEmpty();
	}

	[Theory]
	[InlineData(GearType.Jewellery)]
	[InlineData(GearType.AllArmor)]
	[InlineData(GearType.AllWeapons)]
	[InlineData(GearType.AllWearable)]
	public void GetItemClass_FlagCombinations_ReturnsEmpty(GearType type)
	{
		// Act
		var result = type.GetItemClass();

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public void GetItemClass_Ring_ContainsRing()
	{
		// Act
		var result = GearType.Ring.GetItemClass();

		// Assert
		result.Should().Contain("Ring");
	}

	[Fact]
	public void GetItemClass_Head_ContainsHead()
	{
		// Act
		var result = GearType.Head.GetItemClass();

		// Assert
		result.Should().Contain("Head");
	}

	[Fact]
	public void GetItemClass_Artifact_ReturnsArtifactClass()
	{
		// Act
		var result = GearType.Artifact.GetItemClass();

		// Assert
		result.Should().Contain("Artifact");
	}

	#endregion

	#region GetRequirementEquationPrefix Tests

	[Theory]
	[InlineData(GearType.Head)]
	[InlineData(GearType.Torso)]
	[InlineData(GearType.Arm)]
	[InlineData(GearType.Leg)]
	[InlineData(GearType.Ring)]
	[InlineData(GearType.Amulet)]
	[InlineData(GearType.Spear)]
	[InlineData(GearType.Staff)]
	[InlineData(GearType.Thrown)]
	[InlineData(GearType.Bow)]
	[InlineData(GearType.Sword)]
	[InlineData(GearType.Mace)]
	[InlineData(GearType.Axe)]
	[InlineData(GearType.Shield)]
	public void GetRequirementEquationPrefix_BasicGearTypes_ReturnsNonEmpty(GearType type)
	{
		// Act
		var result = type.GetRequirementEquationPrefix();

		// Assert
		result.Should().NotBeNullOrEmpty();
	}

	[Theory]
	[InlineData(GearType.Undefined)]
	[InlineData(GearType.MonsterInfrequent)]
	[InlineData(GearType.ForMage)]
	[InlineData(GearType.ForMelee)]
	public void GetRequirementEquationPrefix_ModifierTypes_ReturnsEmpty(GearType type)
	{
		// Act
		var result = type.GetRequirementEquationPrefix();

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public void GetRequirementEquationPrefix_Ring_ReturnsRing()
	{
		// Act
		var result = GearType.Ring.GetRequirementEquationPrefix();

		// Assert
		result.Should().Be("ring");
	}

	[Fact]
	public void GetRequirementEquationPrefix_Head_ReturnsHead()
	{
		// Act
		var result = GearType.Head.GetRequirementEquationPrefix();

		// Assert
		result.Should().Be("head");
	}

	[Fact]
	public void GetRequirementEquationPrefix_Sword_ReturnsSword()
	{
		// Act
		var result = GearType.Sword.GetRequirementEquationPrefix();

		// Assert
		result.Should().Be("sword");
	}

	[Fact]
	public void GetRequirementEquationPrefix_Shield_ReturnsShield()
	{
		// Act
		var result = GearType.Shield.GetRequirementEquationPrefix();

		// Assert
		result.Should().Be("shield");
	}

	#endregion

	#region GearTypeMap Tests

	[Fact]
	public void GearTypeMap_NotNull()
	{
		// Act
		var result = GearTypeExtension.GearTypeMap;

		// Assert
		result.Should().NotBeNull();
	}

	[Fact]
	public void GearTypeMap_ContainsWeaponTypes()
	{
		// Act
		var map = GearTypeExtension.GearTypeMap;

		// Assert
		map.Should().ContainKey(GearType.Sword);
		map.Should().ContainKey(GearType.Axe);
		map.Should().ContainKey(GearType.Mace);
		map.Should().ContainKey(GearType.Bow);
	}

	[Fact]
	public void GearTypeMap_ContainsArmorTypes()
	{
		// Act
		var map = GearTypeExtension.GearTypeMap;

		// Assert
		map.Should().ContainKey(GearType.Head);
		map.Should().ContainKey(GearType.Torso);
		map.Should().ContainKey(GearType.Shield);
	}

	[Fact]
	public void GearTypeMap_ContainsJewelryTypes()
	{
		// Act
		var map = GearTypeExtension.GearTypeMap;

		// Assert
		map.Should().ContainKey(GearType.Ring);
		map.Should().ContainKey(GearType.Amulet);
	}

	[Fact]
	public void GearTypeMap_DoesNotContainFlagCombinations()
	{
		// Act
		var map = GearTypeExtension.GearTypeMap;

		// Assert
		map.Should().NotContainKey(GearType.Jewellery);
		map.Should().NotContainKey(GearType.AllArmor);
		map.Should().NotContainKey(GearType.AllWeapons);
	}

	[Fact]
	public void GearTypeMap_ContainsArtifact()
	{
		// Act
		var map = GearTypeExtension.GearTypeMap;

		// Assert
		map.Should().ContainKey(GearType.Artifact);
	}

	#endregion

	#region Consistency Tests

	[Fact]
	public void GetItemClass_MatchesMapEntry_ForRing()
	{
		// Act
		var result = GearType.Ring.GetItemClass();
		var map = GearTypeExtension.GearTypeMap;

		// Assert
		if (map.TryGetValue(GearType.Ring, out var attr))
		{
			result.Should().Be(attr.ICLASS);
		}
	}

	[Fact]
	public void GetRequirementEquationPrefix_MatchesMapEntry_ForRing()
	{
		// Act
		var result = GearType.Ring.GetRequirementEquationPrefix();
		var map = GearTypeExtension.GearTypeMap;

		// Assert
		if (map.TryGetValue(GearType.Ring, out var attr))
		{
			result.Should().Be(attr.RequirementEquationPrefix);
		}
	}

	[Theory]
	[InlineData(GearType.Head)]
	[InlineData(GearType.Torso)]
	[InlineData(GearType.Sword)]
	[InlineData(GearType.Mace)]
	public void GetItemClass_MatchesMapEntry_ForVariousTypes(GearType type)
	{
		// Act
		var result = type.GetItemClass();
		var map = GearTypeExtension.GearTypeMap;

		// Assert
		if (map.TryGetValue(type, out var attr))
		{
			result.Should().Be(attr.ICLASS);
		}
	}

	[Theory]
	[InlineData(GearType.Head)]
	[InlineData(GearType.Torso)]
	[InlineData(GearType.Sword)]
	[InlineData(GearType.Mace)]
	public void GetRequirementEquationPrefix_MatchesMapEntry_ForVariousTypes(GearType type)
	{
		// Act
		var result = type.GetRequirementEquationPrefix();
		var map = GearTypeExtension.GearTypeMap;

		// Assert
		if (map.TryGetValue(type, out var attr))
		{
			result.Should().Be(attr.RequirementEquationPrefix);
		}
	}

	#endregion
}
