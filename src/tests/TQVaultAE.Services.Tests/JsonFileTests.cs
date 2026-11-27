using System.Text.Json;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

public class JsonFileTests
{
	private const string Path = "settings.json";
	private static readonly JsonSerializerOptions Options = new() { IncludeFields = true };
	private readonly Mock<IFileIO> _fileIO = new();
	private readonly Mock<ILogger> _log = new();

	public sealed class Settings
	{
		public int value;
	}

	private JsonFileRead<Settings> Read() => JsonFile.Read<Settings>(this._fileIO.Object, Path, Options, this._log.Object, "settings");

	private void HasFile(string content)
	{
		this._fileIO.Setup(f => f.Exists(Path)).Returns(true);
		this._fileIO.Setup(f => f.ReadAllText(Path)).Returns(content);
	}

	[Fact]
	public void Read_MissingFile_GivesNoValueAndNoFailure()
		=> this.Read().Should().Be(new JsonFileRead<Settings>(null, false));

	[Fact]
	public void Read_ValidFile_GivesItsValue()
	{
		this.HasFile("""{ "value": 7 }""");

		var read = this.Read();

		read.Failed.Should().BeFalse();
		read.Value!.value.Should().Be(7);
	}

	[Theory]
	[InlineData("{ not json")]
	[InlineData("null")]
	[InlineData("")]
	public void Read_CorruptOrEmptyFile_FailsWithoutThrowing(string content)
	{
		this.HasFile(content);

		this.Read().Should().Be(new JsonFileRead<Settings>(null, true));
	}

	[Fact]
	public void Read_UnreadableFile_FailsWithoutThrowing()
	{
		this._fileIO.Setup(f => f.Exists(Path)).Returns(true);
		this._fileIO.Setup(f => f.ReadAllText(Path)).Throws<UnauthorizedAccessException>();

		this.Read().Failed.Should().BeTrue();
	}
}
