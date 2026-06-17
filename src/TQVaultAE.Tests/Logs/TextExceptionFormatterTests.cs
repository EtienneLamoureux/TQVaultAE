using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using TQVaultAE.Logs;

namespace TQVaultAE.Tests.Logs;

/// <summary>
/// Unit tests for TextExceptionFormatter class
/// </summary>
public class TextExceptionFormatterTests
{
	#region Constructor Tests

	[Fact]
	public void Constructor_WithNullException_ThrowsArgumentNullException()
	{
		// Act & Assert
		this.Invoking(_ => new TextExceptionFormatter(null!))
			.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public void Constructor_WithValidException_StoresException()
	{
		// Arrange
		var innerException = new InvalidOperationException("test");
		var ex = new Exception("outer", innerException);

		// Act
		var formatter = new TextExceptionFormatter(ex);

		// Assert
		formatter.Exception.Should().BeSameAs(ex);
	}

	#endregion

	#region Format Tests

	[Fact]
	public void Format_WithSimpleException_ReturnsFormattedString()
	{
		// Arrange
		var ex = new InvalidOperationException("Test error message");
		var formatter = new TextExceptionFormatter(ex);

		// Act
		var result = formatter.Format();

		// Assert
		result.Should().NotBeNullOrEmpty();
		result.Should().Contain("InvalidOperationException");
		result.Should().Contain("Test error message");
	}

	[Fact]
	public void Format_WithInnerException_IncludesInnerException()
	{
		// Arrange
		var innerException = new ArgumentException("Inner error");
		var ex = new InvalidOperationException("Outer error", innerException);
		var formatter = new TextExceptionFormatter(ex);

		// Act
		var result = formatter.Format();

		// Assert
		result.Should().Contain("Inner Exception");
		result.Should().Contain("ArgumentException");
		result.Should().Contain("Inner error");
	}

	[Fact]
	public void Format_WithNullStackTrace_ShowsUnavailable()
	{
		// Arrange
		var ex = new Exception("Test");
		var formatter = new TextExceptionFormatter(ex);

		// Act
		var result = formatter.Format();

		// Assert
		result.Should().Contain("StackTrace: Stack Trace Unavailable");
	}

	[Fact]
	public void Format_WithNullHelpLink_ShowsEmpty()
	{
		// Arrange
		var ex = new Exception("Test") { HelpLink = null };
		var formatter = new TextExceptionFormatter(ex);

		// Act
		var result = formatter.Format();

		// Assert
		result.Should().Contain("HelpLink:");
	}

	[Fact]
	public void Format_WithNullSource_ShowsEmpty()
	{
		// Arrange
		var ex = new Exception("Test") { Source = null };
		var formatter = new TextExceptionFormatter(ex);

		// Act
		var result = formatter.Format();

		// Assert
		result.Should().Contain("Source:");
	}

	#endregion

	#region Static FormatException Tests

	[Fact]
	public void FormatException_Static_WithMessageAndException_ReturnsFormattedString()
	{
		// Arrange
		var ex = new InvalidOperationException("Test error");

		// Act
		var result = TextExceptionFormatter.FormatException("Custom Message", ex);

		// Assert
		result.Should().Contain("Custom Message");
		result.Should().Contain("InvalidOperationException");
		result.Should().Contain("Test error");
	}

	[Fact]
	public void FormatException_Static_ExceptionOnly_ReturnsFormattedString()
	{
		// Arrange
		var ex = new ArgumentException("Test argument");

		// Act
		var result = TextExceptionFormatter.FormatException(ex);

		// Assert
		result.Should().NotBeNullOrEmpty();
		result.Should().Contain("ArgumentException");
		result.Should().Contain("Test argument");
	}

	#endregion

	#region AdditionalInfo Tests

	[Fact]
	public void AdditionalInfo_ContainsMachineName()
	{
		// Arrange
		var ex = new Exception("Test");
		var formatter = new TextExceptionFormatter(ex);

		// Act
		var additionalInfo = formatter.AdditionalInfo;

		// Assert
		additionalInfo.Should().NotBeNull();
		additionalInfo.AllKeys.Should().Contain("MachineName");
	}

	[Fact]
	public void AdditionalInfo_ContainsTimeStamp()
	{
		// Arrange
		var ex = new Exception("Test");
		var formatter = new TextExceptionFormatter(ex);

		// Act
		var additionalInfo = formatter.AdditionalInfo;

		// Assert
		additionalInfo.AllKeys.Should().Contain("TimeStamp");
	}

	[Fact]
	public void AdditionalInfo_ContainsFullName()
	{
		// Arrange
		var ex = new Exception("Test");
		var formatter = new TextExceptionFormatter(ex);

		// Act
		var additionalInfo = formatter.AdditionalInfo;

		// Assert
		additionalInfo.AllKeys.Should().Contain("FullName");
	}

	[Fact]
	public void AdditionalInfo_ContainsAppDomainName()
	{
		// Arrange
		var ex = new Exception("Test");
		var formatter = new TextExceptionFormatter(ex);

		// Act
		var additionalInfo = formatter.AdditionalInfo;

		// Assert
		additionalInfo.AllKeys.Should().Contain("AppDomainName");
	}

	[Fact]
	public void AdditionalInfo_ContainsThreadIdentity()
	{
		// Arrange
		var ex = new Exception("Test");
		var formatter = new TextExceptionFormatter(ex);

		// Act
		var additionalInfo = formatter.AdditionalInfo;

		// Assert
		additionalInfo.AllKeys.Should().Contain("ThreadIdentity");
	}

	#endregion

	#region Edge Cases

	[Fact]
	public void Format_WithExceptionHavingDataProperty_IncludesDataPropertyName()
	{
		// Arrange - custom exception with Data dictionary
		var ex = new InvalidOperationException("Test");
		ex.Data["CustomKey"] = "CustomValue";
		var formatter = new TextExceptionFormatter(ex);

		// Act
		var result = formatter.Format();

		// Assert - should include Data property name (actual values depend on ToString of Data)
		result.Should().Contain("Data");
	}

	[Fact]
	public void Format_WithNestedInnerExceptions_FormatsAllLevels()
	{
		// Arrange - three levels of inner exceptions
		var level3 = new Exception("Level 3");
		var level2 = new Exception("Level 2", level3);
		var level1 = new Exception("Level 1", level2);
		var formatter = new TextExceptionFormatter(level1);

		// Act
		var result = formatter.Format();

		// Assert - should contain all three levels
		result.Should().Contain("Level 1");
		result.Should().Contain("Level 2");
		result.Should().Contain("Level 3");
	}

	[Fact]
	public void Format_WithExceptionHavingNoMessage_HandlesEmptyMessage()
	{
		// Arrange
		var ex = new Exception(string.Empty);
		var formatter = new TextExceptionFormatter(ex);

		// Act
		var result = formatter.Format();

		// Assert
		result.Should().NotBeNull();
		result.Should().Contain("Message:");
	}

	#endregion
}
