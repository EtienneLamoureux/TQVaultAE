using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.GUI.Infrastructure;

namespace TQVaultAE.Tests.Win32.Gui;

/// <summary>
/// Unit tests for ILogExtension class
/// </summary>
public class ILogExtensionTests
{
	private readonly ILogger log = new Mock<ILogger>().Object;

	[Fact]
	public void FormatException_WithValidException_ReturnsTypeAndMessage()
	{
		var result = this.log.FormatException(new InvalidOperationException("Test error message"));

		result.Should().Contain("System.InvalidOperationException");
		result.Should().Contain("Test error message");
	}

	[Fact]
	public void FormatException_WithoutData_IsTheRuntimeText()
	{
		var ex = Thrown(() => throw new InvalidOperationException("Outer error", new ArgumentException("Inner error")));

		this.log.FormatException(ex).Should().Be(ex.ToString());
	}

	[Fact]
	public void FormatException_WithInnerException_IncludesInnerException()
	{
		var ex = new InvalidOperationException("Outer error", new ArgumentException("Inner error"));

		var result = this.log.FormatException(ex);

		result.Should().Contain("---> System.ArgumentException: Inner error");
	}

	[Fact]
	public void FormatException_WithStackTrace_IncludesIt()
	{
		var ex = Thrown(() => throw new InvalidOperationException("Thrown"));

		var result = this.log.FormatException(ex);

		result.Should().Contain(nameof(Thrown));
	}

	[Fact]
	public void FormatException_WithArgumentException_IncludesParamName()
	{
		var result = this.log.FormatException(new ArgumentException("Invalid argument", "paramName"));

		result.Should().Contain("Invalid argument");
		result.Should().Contain("paramName");
	}

	[Fact]
	public void FormatException_WithFileNotFoundException_IncludesFileName()
	{
		var result = this.log.FormatException(new FileNotFoundException("File not found", "test.txt"));

		result.Should().Contain("FileNotFoundException");
		result.Should().Contain("test.txt");
	}

	[Fact]
	public void FormatException_WithAggregateException_FormatsAllInnerExceptions()
	{
		var ex = new AggregateException(new InvalidOperationException("Error 1"), new ArgumentException("Error 2"));

		var result = this.log.FormatException(ex);

		result.Should().Contain("AggregateException");
		result.Should().Contain("Error 1");
		result.Should().Contain("Error 2");
	}

	[Fact]
	public void FormatException_WithData_ListsItsEntries()
	{
		var ex = new InvalidOperationException("With data");
		ex.Data["File"] = "player.chr";
		ex.Data["Offset"] = null;

		var result = this.log.FormatException(ex);

		result.Should().StartWith(ex.ToString());
		result.Should().Contain("Data:");
		result.Should().Contain("File : player.chr");
		result.Should().Contain("Offset : {null}");
	}

	[Fact]
	public void FormatException_DoesNotShowTheMachineName()
	{
		var result = this.log.FormatException(new InvalidOperationException("Test"));

		result.Should().NotContain(Environment.MachineName);
	}

	[Fact]
	public void FormatException_WithNullException_Throws()
	{
		this.Invoking(_ => this.log.FormatException(null!)).Should().Throw<ArgumentNullException>();
	}

	private static Exception Thrown(Action action)
	{
		try
		{
			action();
		}
		catch (Exception ex)
		{
			return ex;
		}

		throw new InvalidOperationException("No exception thrown.");
	}
}
