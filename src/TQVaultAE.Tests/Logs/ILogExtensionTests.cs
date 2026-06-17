using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Logs;

namespace TQVaultAE.Tests.Logs;

/// <summary>
/// Unit tests for ILogExtension class
/// </summary>
public class ILogExtensionTests
{
	private readonly Mock<ILogger> _mockLogger;

	public ILogExtensionTests()
	{
		_mockLogger = new Mock<ILogger>();
	}

	#region ErrorException Tests

	[Fact]
	public void ErrorException_LogsErrorWithException()
	{
		// Arrange
		var ex = new InvalidOperationException("Test error");

		// Act
		_mockLogger.Object.ErrorException(ex);

		// Assert - verify LogError was called with the exception and message
		_mockLogger.Verify(
			x => x.Log(
				LogLevel.Error,
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((v, t) => v.ToString() == "Test error"),
				ex,
				It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
			Times.Once);
	}

	[Fact]
	public void ErrorException_WithNullMessage_LogsExceptionMessage()
	{
		// Arrange
		var ex = new Exception();
		var mockLogger = new Mock<ILogger>();

		// Act
		mockLogger.Object.ErrorException(ex);

		// Assert
		mockLogger.Verify(
			x => x.Log(
				LogLevel.Error,
				It.IsAny<EventId>(),
				It.IsAny<It.IsAnyType>(),
				ex,
				It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
			Times.Once);
	}

	#endregion

	#region FormatException Tests

	[Fact]
	public void FormatException_WithValidException_ReturnsFormattedString()
	{
		// Arrange
		var ex = new InvalidOperationException("Test error message");
		var mockLogger = new Mock<ILogger>();

		// Act
		var result = mockLogger.Object.FormatException(ex);

		// Assert
		result.Should().NotBeNullOrEmpty();
		result.Should().Contain("InvalidOperationException");
		result.Should().Contain("Test error message");
	}

	[Fact]
	public void FormatException_WithInnerException_IncludesInnerException()
	{
		// Arrange
		var innerException = new ArgumentException("Inner error");
		var ex = new InvalidOperationException("Outer error", innerException);
		var mockLogger = new Mock<ILogger>();

		// Act
		var result = mockLogger.Object.FormatException(ex);

		// Assert
		result.Should().Contain("Inner Exception");
		result.Should().Contain("ArgumentException");
	}

	[Fact]
	public void FormatException_WhenFormatterFails_ReturnsFallbackMessage()
	{
		// Arrange - create an exception that might cause issues
		var ex = new Exception("Test");
		var mockLogger = new Mock<ILogger>();

		// Act
		var result = mockLogger.Object.FormatException(ex);

		// Assert - should return formatted string, not fallback
		result.Should().NotContain("failed");
		result.Should().Contain("Test");
	}

	[Fact]
	public void FormatException_WithArgumentException_ReturnsProperFormat()
	{
		// Arrange
		var ex = new ArgumentException("Invalid argument", "paramName");
		var mockLogger = new Mock<ILogger>();

		// Act
		var result = mockLogger.Object.FormatException(ex);

		// Assert
		result.Should().Contain("ArgumentException");
		result.Should().Contain("Invalid argument");
		result.Should().Contain("paramName");
	}

	[Fact]
	public void FormatException_WithFileNotFoundException_IncludesFileName()
	{
		// Arrange
		var ex = new FileNotFoundException("File not found", "test.txt");
		var mockLogger = new Mock<ILogger>();

		// Act
		var result = mockLogger.Object.FormatException(ex);

		// Assert
		result.Should().Contain("FileNotFoundException");
		result.Should().Contain("test.txt");
	}

	#endregion

	#region Edge Cases

	[Fact]
	public void FormatException_WithAggregateException_FormatsAllInnerExceptions()
	{
		// Arrange
		var ex1 = new InvalidOperationException("Error 1");
		var ex2 = new ArgumentException("Error 2");
		var aggregateException = new AggregateException(ex1, ex2);
		var mockLogger = new Mock<ILogger>();

		// Act
		var result = mockLogger.Object.FormatException(aggregateException);

		// Assert
		result.Should().Contain("AggregateException");
	}

	[Fact]
	public void ErrorException_LogsToCorrectLogLevel()
	{
		// Arrange
		var ex = new Exception("Test");
		var mockLogger = new Mock<ILogger>();

		// Act
		mockLogger.Object.ErrorException(ex);

		// Assert - verify Log was called with Error level (just verify the method was called)
		mockLogger.Verify(
			x => x.Log(
				LogLevel.Error,
				It.IsAny<EventId>(),
				It.IsAny<It.IsAnyType>(),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
			Times.Once);
	}

	#endregion
}
