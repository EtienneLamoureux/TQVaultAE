using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

/// <summary><see cref="ClassSetService"/>: the class sets file of a character, on real files in a temporary folder.</summary>
public sealed class ClassSetServiceTests : IDisposable
{
	private readonly string _save = Path.Combine(Path.GetTempPath(), $"tqvault-classsets-{Guid.NewGuid():N}", "_HeroA");
	private readonly ClassSetService _service = new(new Mock<ILogger<ClassSetService>>().Object, new FileIO(), new PathIO());

	public ClassSetServiceTests() => Directory.CreateDirectory(this._save);

	public void Dispose() => Directory.Delete(Path.GetDirectoryName(this._save)!, true);

	private string SetsFile => Path.Combine(this._save, IClassSetService.FileName);

	private static readonly ClassSet Build = new("Storm",
		[
			new ClassSetMastery(Masteries.Storm, 10, [new("""Records\Skills\Storm\IceShard.dbr""", 5)]),
			new ClassSetMastery(Masteries.Earth, 1, []),
		],
		new ClassSetAttributes(1, 2, 3, 4, 5));

	[Fact]
	public void WriteThenRead_GivesTheSameSetsBack()
	{
		var book = this._service.Read(this._save);

		this._service.Write(book, [Build, ClassSet.Empty("Empty")]);
		var read = this._service.Read(this._save);

		read.ReadFailed.Should().BeFalse();
		read.Sets.Select(s => s.Name).Should().Equal("Storm", "Empty");
		read.Sets[0].SameBuild(Build).Should().BeTrue();
		read.Sets[1].Masteries.Should().BeEmpty();
	}

	[Fact]
	public void Write_NoSetLeft_DeletesTheFile()
	{
		var book = this._service.Read(this._save);
		this._service.Write(book, [Build]);

		this._service.Write(book, []);

		File.Exists(this.SetsFile).Should().BeFalse();
	}

	[Fact]
	public void Read_DamagedFile_IsReadFailedAndNeverWrittenOver()
	{
		File.WriteAllText(this.SetsFile, "{ not json");

		var book = this._service.Read(this._save);
		this._service.Write(book, [Build]);

		book.ReadFailed.Should().BeTrue();
		File.ReadAllText(this.SetsFile).Should().Be("{ not json");
	}

	[Fact]
	public void Read_UnknownMastery_IsLeftOutAndTheOthersKept()
	{
		File.WriteAllText(this.SetsFile, """
			{ "version": 1, "classSets": [ { "name": "Mixed", "masteries": [ { "mastery": "NoSuchMastery", "level": 3 }, { "mastery": "storm", "level": 2 } ] } ] }
			""");

		var set = this._service.Read(this._save).Sets.Single();

		set.Masteries.Should().ContainSingle().Which.Mastery.Should().Be(Masteries.Storm);
		set.Attributes.Should().Be(default(ClassSetAttributes));
	}
}
