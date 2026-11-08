using ArzExplorer.Models;
using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Config;
using TQVaultAE.Data;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Services;
using static TQVaultAE.Tests.DbCli.DbCliFixtures;

namespace TQVaultAE.Tests.Win32.ArzExplorer;

/// <summary>
/// <see cref="ArchiveExtractor"/> writes the content of ARC and ARZ files with the TQVault providers.
/// </summary>
public sealed class ArchiveExtractorTests : IDisposable
{
	private const short Int = 0;

	private readonly ArcFileProvider _arcProvider = new(NullLogger<ArcFileProvider>.Instance, new PathIO(), new DirectoryIO(), new UserSettings());
	private readonly ArzFileProvider _arzProvider = new(NullLogger<ArzFileProvider>.Instance);
	private readonly DBRecordCollectionProvider _recordWriter = new(new DirectoryIO(), new PathIO(), new FileIO());
	private readonly ArchiveExtractor _extractor;
	private readonly string _folder = Path.Combine(Path.GetTempPath(), $"arzexplorer-test-{Guid.NewGuid():N}");
	private readonly List<string> _tempFiles = [];
	private readonly List<ArcFile> _arcFiles = [];

	public ArchiveExtractorTests()
		=> this._extractor = new ArchiveExtractor(this._arcProvider, this._arzProvider, this._recordWriter);

	public void Dispose()
	{
		foreach (var file in this._arcFiles)
			file.Archive?.Dispose();
		foreach (var path in this._tempFiles)
			File.Delete(path);
		if (Directory.Exists(this._folder))
			Directory.Delete(this._folder, true);
	}

	/// <summary>
	/// Opens a synthetic ARC file the way MainForm does.
	/// </summary>
	private TQFileInfo OpenArc(string arcName, params ArcSpec[] entries)
	{
		var directory = Path.Combine(this._folder, "game");
		Directory.CreateDirectory(directory);
		var path = Path.Combine(directory, arcName);
		File.WriteAllBytes(path, BuildArc(entries));

		var arc = new ArcFile(path);
		this._arcFiles.Add(arc);
		this._arcProvider.Read(arc).Should().BeTrue();
		return new TQFileInfo { ARCFile = arc, FileType = CompressedFileType.ArcFile, SourceFile = path };
	}

	private TQFileInfo OpenArz(params ArzRec[] records)
	{
		var path = WriteTemp(BuildArz(records, []), ".arz");
		this._tempFiles.Add(path);
		var arz = new ArzFile(path);
		this._arzProvider.Read(arz).Should().BeTrue();
		return new TQFileInfo { ARZFile = arz, FileType = CompressedFileType.ArzFile, SourceFile = path };
	}

	private static byte[] Bytes(string text) => System.Text.Encoding.ASCII.GetBytes(text);

	private string Output(string relativePath) => Path.Combine(this._folder, "out", relativePath);

	[Fact]
	public void ExtractOne_EntryOfAnArc_WritesItsContent()
	{
		var file = this.OpenArc("Items.arc", Stored("animalrelics/animalpart01a.tex", Bytes("relic")));
		// Tree key of the entry, as MainForm builds it
		var treeKey = RecordId.Create($@"{ArcPaths.TreePrefix(file.SourceFileId)}\animalrelics\animalpart01a.tex");

		this._extractor.ExtractOne(file, treeKey, this.Output("relic.tex"));

		File.ReadAllBytes(this.Output("relic.tex")).Should().Equal(Bytes("relic"));
	}

	[Fact]
	public void ExtractOne_EntryOfAnExpansionArc_WritesItsContent()
	{
		var file = this.OpenArc("Items.arc", Compressed("artifacts/recipe01a.tex", Bytes("recipe")));
		var treeKey = RecordId.Create("""XPack2\Items\artifacts\recipe01a.tex""");

		this._extractor.ExtractOne(file, treeKey, this.Output("recipe.tex"));

		File.ReadAllBytes(this.Output("recipe.tex")).Should().Equal(Bytes("recipe"));
	}

	[Fact]
	public void ExtractOne_RecordOfTheDatabase_WritesItsVariables()
	{
		var file = this.OpenArz(new ArzRec("""records\item\ring01.dbr""", "ArmorJewelry_Ring", 0, new ArzVar(Int, "itemLevel", [42])));

		this._extractor.ExtractOne(file, RecordId.Create("""records\item\ring01.dbr"""), this.Output("ring.dbr"));

		File.ReadAllLines(this.Output("ring.dbr")).Should().Contain("itemLevel,42,");
	}

	[Fact]
	public void ExtractAll_Arc_WritesEveryEntryUnderItsFolders()
	{
		var file = this.OpenArc("UI.arc", Stored("caravan/autosort.tex", Bytes("a")), Compressed("frame.tex", Bytes("b")));
		List<RecordId> reported = [];
		int written = 0;

		var result = this._extractor.ExtractAll(file, this.Output("all"), reported.Add, () => written++, CancellationToken.None);

		result.Should().Be(new ExtractionResult(ExtractionOutcome.Completed, 2));
		reported.Should().HaveCount(2);
		written.Should().Be(2);
		File.ReadAllBytes(this.Output("""all\CARAVAN\AUTOSORT.TEX""")).Should().Equal(Bytes("a"));
		File.ReadAllBytes(this.Output("""all\FRAME.TEX""")).Should().Equal(Bytes("b"));
	}

	[Fact]
	public void ExtractAll_Arz_WritesEveryRecordUnderItsFolders()
	{
		var file = this.OpenArz(
			new ArzRec("""records\item\ring01.dbr""", "ArmorJewelry_Ring", 0, new ArzVar(Int, "itemLevel", [42])),
			new ArzRec("""records\item\amulet01.dbr""", "ArmorJewelry_Amulet", 0, new ArzVar(Int, "itemLevel", [7])));

		var result = this._extractor.ExtractAll(file, this.Output("db"), _ => { }, () => { }, CancellationToken.None);

		result.Outcome.Should().Be(ExtractionOutcome.Completed);
		File.ReadAllLines(this.Output("""db\RECORDS\ITEM\AMULET01.DBR""")).Should().Contain("itemLevel,7,");
		File.ReadAllLines(this.Output("""db\RECORDS\ITEM\RING01.DBR""")).Should().Contain("itemLevel,42,");
	}

	[Fact]
	public void ExtractAll_WhenCancelled_StopsAfterTheEntryBeingWritten()
	{
		var file = this.OpenArc("UI.arc", Stored("a.tex", Bytes("a")), Stored("b.tex", Bytes("b")), Stored("c.tex", Bytes("c")));
		using var cancellation = new CancellationTokenSource();

		var result = this._extractor.ExtractAll(file, this.Output("all"), _ => { }, cancellation.Cancel, cancellation.Token);

		result.Should().Be(new ExtractionResult(ExtractionOutcome.Cancelled, 1));
		File.Exists(this.Output("""all\B.TEX""")).Should().BeFalse();
	}

	[Fact]
	public void ExtractAll_WhenAWriteFails_ReturnsTheErrorInsteadOfThrowing()
	{
		// The extraction runs on a worker thread, where a thrown exception ends the application
		var file = this.OpenArc("UI.arc", Stored("a.tex", Bytes("a")), Stored("b.tex", Bytes("b")));
		var error = new UnauthorizedAccessException("read-only folder");
		var arcProvider = new Mock<IArcFileProvider>();
		arcProvider.Setup(p => p.Write(It.IsAny<ArcFile>(), It.IsAny<string>(), It.IsAny<RecordId>(), It.IsAny<string>())).Throws(error);
		var extractor = new ArchiveExtractor(arcProvider.Object, this._arzProvider, this._recordWriter);

		var result = extractor.ExtractAll(file, this.Output("all"), _ => { }, () => { }, CancellationToken.None);

		result.Should().Be(new ExtractionResult(ExtractionOutcome.Failed, 0, error));
	}
}
