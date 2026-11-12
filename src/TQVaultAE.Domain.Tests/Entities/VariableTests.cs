using AwesomeAssertions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Entities;

/// <summary>
/// Unit tests for Variable entity
/// </summary>
public class VariableTests
{
	[Fact]
	public void Variable_Clone_CreatesDeepCopy()
	{
		// Arrange
		var original = new Variable("testVar", VariableDataType.Integer, 2);
		original[0] = 10;
		original[1] = 20;

		// Act
		var clone = original.Clone();

		// Assert
		clone.Should().NotBeSameAs(original);
		clone.Name.Should().Be(original.Name);
		clone[0].Should().Be(10);
		clone[1].Should().Be(20);
	}

	[Fact]
	public void Variable_Clone_ModifyingCloneDoesNotAffectOriginal()
	{
		// Arrange
		var original = new Variable("testVar", VariableDataType.Integer, 2);
		original[0] = 10;
		original[1] = 20;

		// Act
		var clone = original.Clone();
		clone[0] = 999;

		// Assert
		original[0].Should().Be(10);
		clone[0].Should().Be(999);
	}

	[Fact]
	public void Variable_IsValueNonZero_WithNonZeroValue_ReturnsTrue()
	{
		// Arrange
		var variable = new Variable("testVar", VariableDataType.Integer, 1);
		variable[0] = 5;

		// Act
		var result = variable.IsValueNonZero();

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public void Variable_IsValueNonZero_WithZeroValue_ReturnsFalse()
	{
		// Arrange
		var variable = new Variable("testVar", VariableDataType.Integer, 1);
		variable[0] = 0;

		// Act
		var result = variable.IsValueNonZero();

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void Variable_IsValueNonZero_WithNoValues_ReturnsFalse()
	{
		// Arrange
		var variable = new Variable("testVar", VariableDataType.Integer, 0);

		// Act
		var result = variable.IsValueNonZero();

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void Variable_IsValueNonZero_FloatWithNonZero_ReturnsTrue()
	{
		// Arrange
		var variable = new Variable("testVar", VariableDataType.Float, 1);
		variable[0] = 1.5f;

		// Act
		var result = variable.IsValueNonZero();

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public void Variable_IsValueNonZero_FloatWithZero_ReturnsFalse()
	{
		// Arrange
		var variable = new Variable("testVar", VariableDataType.Float, 1);
		variable[0] = 0.0f;

		// Act
		var result = variable.IsValueNonZero();

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void Variable_IsValueNonZero_StringWithValue_ReturnsTrue()
	{
		// Arrange
		var variable = new Variable("testVar", VariableDataType.StringVar, 1);
		variable[0] = "hello";

		// Act
		var result = variable.IsValueNonZero();

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public void Variable_IsValueNonZero_StringWithWhitespace_ReturnsFalse()
	{
		// Arrange
		var variable = new Variable("testVar", VariableDataType.StringVar, 1);
		variable[0] = "   ";

		// Act
		var result = variable.IsValueNonZero();

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void Variable_IsValueNonZero_StringWithEmpty_ReturnsFalse()
	{
		// Arrange
		var variable = new Variable("testVar", VariableDataType.StringVar, 1);
		variable[0] = "";

		// Act
		var result = variable.IsValueNonZero();

		// Assert
		result.Should().BeFalse();
	}

	// Note: Integer and Float tests for IsValueRelevant are duplicates of IsValueNonZero tests
	// (both check for != 0/default). Only Boolean and Unknown have unique behavior.

	[Fact]
	public void Variable_IsValueRelevant_StringWithWhitespace_ReturnsFalse()
	{
		// Arrange
		var variable = new Variable("testVar", VariableDataType.StringVar, 1);
		variable[0] = "   ";

		// Act
		var result = variable.IsValueRelevant;

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void Variable_IsValueRelevant_StringWithValue_ReturnsTrue()
	{
		// Arrange
		var variable = new Variable("testVar", VariableDataType.StringVar, 1);
		variable[0] = "test value";

		// Act
		var result = variable.IsValueRelevant;

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public void Variable_IsValueRelevant_BooleanFalse_ReturnsFalse()
	{
		// Arrange
		var variable = new Variable("testVar", VariableDataType.Boolean, 1);
		variable[0] = false;

		// Act
		var result = variable.IsValueRelevant;

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void Variable_IsValueRelevant_BooleanTrue_ReturnsTrue()
	{
		// Arrange
		var variable = new Variable("testVar", VariableDataType.Boolean, 1);
		variable[0] = true;

		// Act
		var result = variable.IsValueRelevant;

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public void Variable_IsValueRelevant_Unknown_ReturnsTrue()
	{
		// Arrange
		var variable = new Variable("testVar", VariableDataType.Unknown, 1);
		variable[0] = "unknown";

		// Act
		var result = variable.IsValueRelevant;

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public void Variable_IsValueRelevant_MultipleValues_OneNonZero_ReturnsTrue()
	{
		// Arrange
		var variable = new Variable("testVar", VariableDataType.Integer, 3);
		variable[0] = 0;
		variable[1] = 0;
		variable[2] = 1;

		// Act
		var result = variable.IsValueRelevant;

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public void Variable_ToString_FormatsCorrectly()
	{
		// Arrange
		var variable = new Variable("Health", VariableDataType.Integer, 2);
		variable[0] = 100;
		variable[1] = 200;

		// Act
		var result = variable.ToString();

		// Assert
		result.Should().Be("Health,100;200,");
	}

	[Fact]
	public void Variable_ToStringValue_FormatsCorrectly()
	{
		// Arrange
		var variable = new Variable("Health", VariableDataType.Integer, 2);
		variable[0] = 100;
		variable[1] = 200;

		// Act
		var result = variable.ToStringValue();

		// Assert
		result.Should().Be("100, 200");
	}

	[Fact]
	public void Variable_ToString_FloatUsesDecimals()
	{
		// Arrange
		var variable = new Variable("Damage", VariableDataType.Float, 1);
		variable[0] = 1.5f;

		// Act
		var result = variable.ToString();

		// Assert
		result.Should().Contain("1.500000");
	}

	[Fact]
	public void Variable_NumberOfValues_ReturnsCorrectCount()
	{
		// Arrange
		var variable = new Variable("testVar", VariableDataType.Integer, 5);

		// Act & Assert
		variable.NumberOfValues.Should().Be(5);
	}

	[Fact]
	public void Variable_GetInt32_ConvertsCorrectly()
	{
		// Arrange
		var variable = new Variable("testVar", VariableDataType.Integer, 1);
		variable[0] = 42;

		// Act
		var result = variable.GetInt32();

		// Assert
		result.Should().Be(42);
	}

	[Fact]
	public void Variable_GetSingle_ConvertsCorrectly()
	{
		// Arrange
		var variable = new Variable("testVar", VariableDataType.Float, 1);
		variable[0] = 3.14f;

		// Act
		var result = variable.GetSingle();

		// Assert
		result.Should().BeApproximately(3.14f, 0.001f);
	}

	[Fact]
	public void Variable_GetString_ConvertsCorrectly()
	{
		// Arrange
		var variable = new Variable("testVar", VariableDataType.StringVar, 1);
		variable[0] = "hello world";

		// Act
		var result = variable.GetString();

		// Assert
		result.Should().Be("hello world");
	}

	[Fact]
	public void Variable_ToString_CachesResult()
	{
		// Arrange
		var variable = new Variable("testVar", VariableDataType.Integer, 1);
		variable[0] = 100;

		// Act
		var result1 = variable.ToString();
		variable[0] = 999; // Modify after first call
		var result2 = variable.ToString();

		// Assert - Should be cached, so same value
		result1.Should().Be(result2);
		result1.Should().Contain("100");
	}
}
