using System.Text.Json;
using AwesomeAssertions;

namespace TQVaultAE.Tests.DbCli.Cli;

public sealed class FilesCommandTests : IDisposable
{
	private readonly FakeGameInstall _game = new();

	public void Dispose() => this._game.Dispose();

	private int Count(params string[] selectors)
	{
		var outFile = this._game.OutPath($"count-{Guid.NewGuid():N}.txt");
		var (code, error) = this._game.Run(["files", "--count", "--out", outFile, .. selectors]);
		code.Should().Be(0, error);
		return int.Parse(File.ReadAllText(outFile).Trim());
	}

	[Fact]
	public void Files_Tsv_ListsEveryColumn()
	{
		var outFile = this._game.OutPath("files.tsv");

		var (code, error) = this._game.Run("files", "--archive", "Sounds", "--path", "hit.mp3", "--out", outFile);

		code.Should().Be(0);
		var lines = File.ReadAllLines(outFile);
		lines[0].Should().Be("archive\tname\tresourceId\tsize\tcompressed\tstorage\ttype\tlastWrite");
		var cells = lines[1].Split('\t');
		cells[..4].Should().Equal("Audio/Sounds", "hit.mp3", """Sounds\hit.mp3""", "500");
		int.Parse(cells[4]).Should().BeInRange(1, 499);
		cells[5..].Should().Equal("3", "sound", "2014-11-14T11:41:59Z");
		lines.Should().HaveCount(2);
		error.Should().Contain("1 entry in 1 archive(s).");
	}

	[Fact]
	public void Files_Json_WritesTypedValues()
	{
		var outFile = this._game.OutPath("files.json");

		var (code, _) = this._game.Run("files", "-q", "--archive", "Resources/Items", "--ext", "msh", "--out", outFile);

		code.Should().Be(0);
		var entry = JsonDocument.Parse(File.ReadAllText(outFile)).RootElement.EnumerateArray().Single();
		entry.GetProperty("archive").GetString().Should().Be("Resources/Items");
		entry.GetProperty("size").GetInt64().Should().Be(8);
		entry.GetProperty("compressed").GetInt64().Should().Be(8);
		entry.GetProperty("storage").GetInt32().Should().Be(1);
		entry.GetProperty("type").GetString().Should().Be("mesh");
	}

	[Theory]
	[InlineData("Resources/XPack/Items", 1)] // key
	[InlineData("Items.arc", 5)] // file name: both Items archives
	[InlineData("items", 5)] // stem, any case
	[InlineData("Resources/*", 5)] // glob over the key
	[InlineData("/xpack/", 1)] // regex, unanchored
	[InlineData("Text_*", 2)]
	public void Files_Archive_MatchesKeyFileNameOrStem(string archive, int expected)
	{
		this.Count("--archive", archive).Should().Be(expected);
	}

	[Theory]
	[InlineData(new[] { "--ext", "tex" }, 4)]
	[InlineData(new[] { "--ext", "tex,.msh" }, 5)]
	[InlineData(new[] { "--ext", "tex", "--ext", "mp3" }, 5)]
	[InlineData(new[] { "--path", """sword\a01.*""" }, 3)]
	[InlineData(new[] { "--resource", """items\sword\a01.tex""" }, 2)]
	[InlineData(new[] { "--resource", """Items\sword\*""", "--ext", "msh" }, 1)]
	[InlineData(new[] { "--archive", "Sounds", "--path", "*.wav" }, 1)]
	public void Files_Selectors_AreAndedAcrossKindsAndOredWithinOne(string[] selectors, int expected)
	{
		this.Count(selectors).Should().Be(expected);
	}

	[Fact]
	public void Files_Limit_StopsAcrossArchives()
	{
		var outFile = this._game.OutPath("files.jsonl");

		var (code, _) = this._game.Run("files", "-q", "--ext", "tex", "--limit", "2", "--out", outFile);

		code.Should().Be(0);
		File.ReadAllLines(outFile).Should().HaveCount(2);
	}

	[Theory]
	[InlineData("2", 2)]
	[InlineData("100", 4)]
	public void Files_CountWithLimit_IsCappedByTheLimit(string limit, int expected)
	{
		this.Count("--ext", "tex", "--limit", limit).Should().Be(expected);
	}

	[Fact]
	public void Files_UnreadableArchive_IsReportedAndSkipped()
	{
		File.WriteAllBytes(Path.Combine(this._game.Root, "Resources", "Broken.arc"), [1, 2, 3, 4]);

		var outFile = this._game.OutPath("count.txt");
		var (code, error) = this._game.Run("files", "--archive", "Resources/*", "--count", "--out", outFile);

		code.Should().Be(0);
		error.Should().Contain("warning: cannot open").And.Contain("Broken.arc");
		File.ReadAllText(outFile).Trim().Should().Be("5");
	}

	[Fact]
	public void Files_DbrFormat_IsUsageError()
	{
		var (code, error) = this._game.Run("files", "--format", "dbr");

		code.Should().Be(2);
		error.Should().Contain("--format: 'dbr' is not supported here");
	}

	[Fact]
	public void Files_InvalidRegex_IsUsageError()
	{
		var (code, _) = this._game.Run("files", "--path", "/[/");

		code.Should().Be(2);
	}
}
