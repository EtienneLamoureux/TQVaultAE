using System.Text.Json;
using AwesomeAssertions;

namespace TQVaultAE.Tests.DbCli.Cli;

public sealed class InfoCommandTests : IDisposable
{
	private readonly FakeGameInstall _game = new();

	public void Dispose() => this._game.Dispose();

	[Fact]
	public void Info_Json_ReportsDatabaseArchivesAndLanguages()
	{
		var outFile = this._game.OutPath("info.json");

		var (code, _) = this._game.Run("info", "--top", "1", "--out", outFile);

		code.Should().Be(0);
		var root = JsonDocument.Parse(File.ReadAllText(outFile)).RootElement;
		root.GetProperty("gameRoot").GetString().Should().Be(this._game.Root);
		root.GetProperty("databaseSize").GetInt64().Should().Be(new FileInfo(Path.Combine(this._game.Root, "Database", "database.arz")).Length);
		root.GetProperty("records").GetInt32().Should().Be(5);
		root.GetProperty("classes").GetInt32().Should().Be(3);
		root.GetProperty("classHistogram").EnumerateObject().Select(p => (p.Name, p.Value.GetInt32())).Should().Equal(("WeaponMelee_Sword", 3));
		root.GetProperty("archiveCount").GetInt32().Should().Be(5);
		root.GetProperty("archiveEntries").GetInt32().Should().Be(9);
		root.GetProperty("archives").EnumerateArray().Select(a => a.GetProperty("key").GetString())
			.Should().Equal("Audio/Sounds", "Resources/Items", "Resources/XPack/Items", "Text/Text_EN", "Text/Text_FR");
		root.GetProperty("languages").EnumerateArray().Select(l => l.GetString()).Should().Equal("EN", "FR");
	}

	[Fact]
	public void Info_TextWithTopZero_ListsEveryClassByCountThenName()
	{
		var outFile = this._game.OutPath("info.txt");

		var (code, _) = this._game.Run("info", "--top", "0", "--format", "text", "--out", outFile);

		code.Should().Be(0);
		var lines = File.ReadAllLines(outFile);
		lines.Should().Contain($"Game root     {this._game.Root}");
		lines.Should().Contain("Records       5 in 3 classes");
		lines.Should().Contain(l => l.StartsWith("Archives      5 (", StringComparison.Ordinal) && l.EndsWith(", 9 entries)", StringComparison.Ordinal));
		lines.Should().Contain("Languages     EN FR");
		lines.SkipWhile(l => l != "Classes:").Skip(1).Select(l => string.Join(' ', l.Split(' ', StringSplitOptions.RemoveEmptyEntries))).Should().Equal(
			"WeaponMelee_Sword 3",
			"ItemRelic 1",
			"LootItemTable_FixedWeight 1");
	}

	[Fact]
	public void Info_WithoutTextFolder_ShowsNoLanguage()
	{
		Directory.Delete(Path.Combine(this._game.Root, "Text"), recursive: true);
		var outFile = this._game.OutPath("info.txt");

		var (code, _) = this._game.Run("info", "--out", outFile);

		code.Should().Be(0);
		File.ReadAllLines(outFile).Should().Contain("Languages     (none)").And.Contain("Top 3 classes:");
	}

	[Fact]
	public void Info_TsvFormat_IsUsageError()
	{
		var (code, error) = this._game.Run("info", "--format", "tsv");

		code.Should().Be(2);
		error.Should().Contain("tqdb info: --format: 'tsv' is not supported here; use one of text, json.");
	}
}
