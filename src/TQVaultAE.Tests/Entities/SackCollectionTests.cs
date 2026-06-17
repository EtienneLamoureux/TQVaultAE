using System.Drawing;
using AwesomeAssertions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Entities;

/// <summary>
/// Unit tests for SackCollection static methods.
/// These methods handle equipment slot calculations.
/// </summary>
public class SackCollectionTests
{
	#region GetWeaponLocationOffset Tests

	[Theory]
	[InlineData(-1, true)]   // Below range
	[InlineData(0, false)]    // Valid: Weapon1
	[InlineData(1, false)]    // Valid: Shield1
	[InlineData(2, false)]    // Valid: Weapon2
	[InlineData(3, false)]    // Valid: Shield2
	[InlineData(4, true)]     // Above range
	public void GetWeaponLocationOffset_WithVariousSlots_ReturnsExpected(
		int weaponSlot, bool shouldBeEmpty)
	{
		// Act
		var result = SackCollection.GetWeaponLocationOffset(weaponSlot);

		// Assert
		if (shouldBeEmpty)
			result.Should().Be(Point.Empty, "slot is out of range");
		else
			result.Should().NotBe(Point.Empty, "slot is in range");
	}

	[Fact]
	public void GetWeaponLocationOffset_Weapon1_ReturnsValidPoint()
	{
		// Act
		var result = SackCollection.GetWeaponLocationOffset(0);

		// Assert
		result.X.Should().Be(1);
		result.Y.Should().Be(0);
	}

	[Fact]
	public void GetWeaponLocationOffset_Shield1_ReturnsValidPoint()
	{
		// Act
		var result = SackCollection.GetWeaponLocationOffset(1);

		// Assert
		result.X.Should().Be(7);
		result.Y.Should().Be(0);
	}

	[Fact]
	public void GetWeaponLocationOffset_Weapon2_ReturnsValidPoint()
	{
		// Act
		var result = SackCollection.GetWeaponLocationOffset(2);

		// Assert
		result.X.Should().Be(1);
		result.Y.Should().Be(9);
	}

	[Fact]
	public void GetWeaponLocationOffset_Shield2_ReturnsValidPoint()
	{
		// Act
		var result = SackCollection.GetWeaponLocationOffset(3);

		// Assert
		result.X.Should().Be(7);
		result.Y.Should().Be(9);
	}

	#endregion

	#region GetEquipmentLocationSize Tests

	[Theory]
	[InlineData(-1, true)]   // Below range
	[InlineData(12, true)]    // Above range (array has 12 elements, index 0-11)
	[InlineData(0, false)]   // Valid: Head
	[InlineData(1, false)]    // Valid: Neck
	[InlineData(5, false)]    // Valid: Ring1
	public void GetEquipmentLocationSize_WithVariousSlots_ReturnsExpected(
		int equipmentSlot, bool shouldBeEmpty)
	{
		// Act
		var result = SackCollection.GetEquipmentLocationSize(equipmentSlot);

		// Assert
		if (shouldBeEmpty)
			result.Should().Be(Size.Empty, "slot is out of range");
		else
			result.Should().NotBe(Size.Empty, "slot is in range");
	}

	[Fact]
	public void GetEquipmentLocationSize_Head_ReturnsCorrectSize()
	{
		// Act
		var result = SackCollection.GetEquipmentLocationSize(0);

		// Assert
		result.Width.Should().Be(2);
		result.Height.Should().Be(2);
	}

	[Fact]
	public void GetEquipmentLocationSize_Neck_ReturnsCorrectSize()
	{
		// Act
		var result = SackCollection.GetEquipmentLocationSize(1);

		// Assert
		result.Width.Should().Be(2);
		result.Height.Should().Be(1);
	}

	[Fact]
	public void GetEquipmentLocationSize_Body_ReturnsCorrectSize()
	{
		// Act
		var result = SackCollection.GetEquipmentLocationSize(2);

		// Assert
		result.Width.Should().Be(2);
		result.Height.Should().Be(3);
	}

	[Fact]
	public void GetEquipmentLocationSize_Ring_ReturnsCorrectSize()
	{
		// Act
		var result = SackCollection.GetEquipmentLocationSize(5);

		// Assert
		result.Width.Should().Be(1);
		result.Height.Should().Be(1);
	}

	[Fact]
	public void GetEquipmentLocationSize_WeaponSlot_ReturnsCorrectSize()
	{
		// Act
		var result = SackCollection.GetEquipmentLocationSize(7);

		// Assert
		result.Width.Should().Be(2);
		result.Height.Should().Be(5);
	}

	[Fact]
	public void GetEquipmentLocationSize_Artifact_ReturnsCorrectSize()
	{
		// Act
		var result = SackCollection.GetEquipmentLocationSize(11);

		// Assert
		result.Width.Should().Be(2);
		result.Height.Should().Be(2);
	}

	#endregion

	#region GetEquipmentLocationOffset Tests

	[Theory]
	[InlineData(-1, true)]   // Below range
	[InlineData(12, true)]    // Above range (array has 12 elements, index 0-11)
	[InlineData(0, false)]   // Valid: Head
	[InlineData(11, false)]   // Valid: Artifact
	public void GetEquipmentLocationOffset_WithVariousSlots_ReturnsExpected(
		int equipmentSlot, bool shouldBeEmpty)
	{
		// Act
		var result = SackCollection.GetEquipmentLocationOffset(equipmentSlot);

		// Assert
		if (shouldBeEmpty)
			result.Should().Be(Point.Empty, "slot is out of range");
		else
			result.Should().NotBe(Point.Empty, "slot is in range");
	}

	[Fact]
	public void GetEquipmentLocationOffset_Head_ReturnsCorrectPosition()
	{
		// Act
		var result = SackCollection.GetEquipmentLocationOffset(0);

		// Assert
		result.X.Should().Be(4);
		result.Y.Should().Be(0);
	}

	[Fact]
	public void GetEquipmentLocationOffset_Neck_ReturnsCorrectPosition()
	{
		// Act
		var result = SackCollection.GetEquipmentLocationOffset(1);

		// Assert
		result.X.Should().Be(4);
		result.Y.Should().Be(3);
	}

	[Fact]
	public void GetEquipmentLocationOffset_Body_ReturnsCorrectPosition()
	{
		// Act
		var result = SackEquipmentLocationOffset(2);

		// Assert
		result.X.Should().Be(4);
		result.Y.Should().Be(5);
	}

	[Fact]
	public void GetEquipmentLocationOffset_WeaponSlot_ReturnsWeaponIndicator()
	{
		// Act
		var result = SackCollection.GetEquipmentLocationOffset(7);

		// Assert
		result.X.Should().Be(Item.WeaponSlotIndicator);
	}

	[Fact]
	public void GetEquipmentLocationOffset_Artifact_ReturnsCorrectPosition()
	{
		// Act
		var result = SackCollection.GetEquipmentLocationOffset(11);

		// Assert
		result.X.Should().Be(1);
		result.Y.Should().Be(6);
	}

	#endregion

	#region IsWeaponSlot Tests

	[Theory]
	[InlineData(-1, false)]   // Below range
	[InlineData(6, false)]    // Valid: Ring1 (not weapon)
	[InlineData(7, true)]      // Valid: Weapon1
	[InlineData(8, true)]       // Valid: Shield1
	[InlineData(9, true)]       // Valid: Weapon2
	[InlineData(10, true)]     // Valid: Shield2
	[InlineData(11, false)]    // Valid: Artifact (not weapon)
	[InlineData(12, false)]    // Above range
	public void IsWeaponSlot_WithVariousSlots_ReturnsExpected(int equipmentSlot, bool expected)
	{
		// Act
		var result = SackCollection.IsWeaponSlot(equipmentSlot);

		// Assert
		result.Should().Be(expected);
	}

	[Fact]
	public void IsWeaponSlot_HeadSlot_ReturnsFalse()
	{
		// Act
		var result = SackCollection.IsWeaponSlot(0);

		// Assert
		result.Should().BeFalse("Head is not a weapon slot");
	}

	[Fact]
	public void IsWeaponSlot_NeckSlot_ReturnsFalse()
	{
		// Act
		var result = SackCollection.IsWeaponSlot(1);

		// Assert
		result.Should().BeFalse("Neck is not a weapon slot");
	}

	[Fact]
	public void IsWeaponSlot_BodySlot_ReturnsFalse()
	{
		// Act
		var result = SackCollection.IsWeaponSlot(2);

		// Assert
		result.Should().BeFalse("Body is not a weapon slot");
	}

	[Fact]
	public void IsWeaponSlot_WeaponSlots_AllReturnTrue()
	{
		// Weapon1, Shield1, Weapon2, Shield2
		for (int i = 7; i <= 10; i++)
		{
			var result = SackCollection.IsWeaponSlot(i);
			result.Should().BeTrue($"Slot {i} should be a weapon slot");
		}
	}

	[Fact]
	public void IsWeaponSlot_RingSlots_AllReturnFalse()
	{
		// Ring1, Ring2
		SackCollection.IsWeaponSlot(5).Should().BeFalse();
		SackCollection.IsWeaponSlot(6).Should().BeFalse();
	}

	#endregion

	#region Edge Cases

	[Theory]
	[InlineData(int.MinValue)]
	[InlineData(-100)]
	[InlineData(int.MaxValue)]
	[InlineData(100)]
	public void GetWeaponLocationOffset_WithInvalidSlot_ReturnsEmpty(int invalidSlot)
	{
		// Act
		var result = SackCollection.GetWeaponLocationOffset(invalidSlot);

		// Assert
		result.Should().Be(Point.Empty);
	}

	[Theory]
	[InlineData(int.MinValue)]
	[InlineData(-100)]
	[InlineData(int.MaxValue)]
	[InlineData(100)]
	public void GetEquipmentLocationSize_WithInvalidSlot_ReturnsEmpty(int invalidSlot)
	{
		// Act
		var result = SackCollection.GetEquipmentLocationSize(invalidSlot);

		// Assert
		result.Should().Be(Size.Empty);
	}

	[Theory]
	[InlineData(int.MinValue)]
	[InlineData(-100)]
	[InlineData(int.MaxValue)]
	[InlineData(100)]
	public void GetEquipmentLocationOffset_WithInvalidSlot_ReturnsEmpty(int invalidSlot)
	{
		// Act
		var result = SackCollection.GetEquipmentLocationOffset(invalidSlot);

		// Assert
		result.Should().Be(Point.Empty);
	}

	[Theory]
	[InlineData(int.MinValue)]
	[InlineData(-100)]
	[InlineData(int.MaxValue)]
	[InlineData(100)]
	public void IsWeaponSlot_WithInvalidSlot_ReturnsFalse(int invalidSlot)
	{
		// Act
		var result = SackCollection.IsWeaponSlot(invalidSlot);

		// Assert
		result.Should().BeFalse();
	}

	#endregion

	/// <summary>
	/// Helper to avoid conflict with SackCollection.GetEquipmentLocationOffset method name.
	/// </summary>
	private static Point SackEquipmentLocationOffset(int slot) 
		=> SackCollection.GetEquipmentLocationOffset(slot);
}
