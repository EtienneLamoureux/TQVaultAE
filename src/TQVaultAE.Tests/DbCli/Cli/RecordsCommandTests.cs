using System.Text.Json;
using AwesomeAssertions;

namespace TQVaultAE.Tests.DbCli.Cli;

public sealed class RecordsCommandTests : IDisposable
{
	private readonly FakeGameInstall _game = new();

	public void Dispose() => this._game.Dispose();

	private (int Code, string Error, string[] Lines) RunToFile(string extension, params string[] args)
	{
		var outFile = this._game.OutPath($"records-{Guid.NewGuid():N}{extension}");
		var (code, error) = this._game.Run(["records", "--out", outFile, .. args]);
		return (code, error, File.Exists(outFile) ? File.ReadAllLines(outFile) : []);
	}

	[Fact]
	public void Records_Default_IsTsvOfIdClassAndDisplayName()
	{
		var (code, error, lines) = this.RunToFile(".tsv", "--class", "ItemRelic");

		code.Should().Be(0);
		lines.Should().Equal("id\tclass\tdisplayName", $"{FakeGameInstall.Relic}\tItemRelic\tRelic of Tests");
		error.Should().Contain("1 record(s).");
	}

	[Fact]
	public void Records_WhereSortAndLimit_KeepTheHighestMatches()
	{
		var (code, _, lines) = this.RunToFile(".tsv", "--where", "itemLevel>=1", "--sort", "itemLevel:desc", "--limit", "2", "--fields", "id,itemLevel");

		code.Should().Be(0);
		lines.Should().Equal("id\titemLevel", $"{FakeGameInstall.SwordB}\t40", $"{FakeGameInstall.SwordA}\t25");
	}

	[Fact]
	public void Records_Count_PrintsOnlyTheNumber()
	{
		var (code, _, lines) = this.RunToFile(".txt", "--class", "Weapon*", "--where", "itemLevel>10", "--count");

		code.Should().Be(0);
		lines.Should().Equal("2");
	}

	[Fact]
	public void Records_NameFilter_UsesTheRequestedLanguage()
	{
		var (code, _, lines) = this.RunToFile(".jsonl", "--name", "*alpha*", "--lang", "FR");

		code.Should().Be(0);
		lines.Should().ContainSingle();
		JsonDocument.Parse(lines[0]).RootElement.GetProperty("displayName").GetString().Should().Be("Lame Alpha");
	}

	[Fact]
	public void Records_TagFilter_WorksWithoutTextArchive()
	{
		Directory.Delete(Path.Combine(this._game.Root, "Text"), recursive: true);

		var (code, _, lines) = this.RunToFile(".tsv", "--tag", "tagRelic", "--fields", "id,nameTag");

		code.Should().Be(0);
		lines.Should().Equal("id\tnameTag", $"{FakeGameInstall.Relic}\ttagRelic");
	}

	[Fact]
	public void Records_DisplayNameWithoutTextArchive_ExitsWith3()
	{
		Directory.Delete(Path.Combine(this._game.Root, "Text"), recursive: true);

		var (code, error, _) = this.RunToFile(".tsv", "--class", "ItemRelic");

		code.Should().Be(3);
		error.Should().Contain("No text archive for language 'EN' (expected Text\\Text_EN.arc)");
	}

	[Fact]
	public void Records_DbrToFolder_WritesEveryMatch()
	{
		var (code, _) = this._game.Run("records", "--id", "records/item/sword/*", "--format", "dbr", "--out", this._game.Out);

		code.Should().Be(0);
		File.Exists(this._game.OutPath(FakeGameInstall.SwordA)).Should().BeTrue();
		File.Exists(this._game.OutPath(FakeGameInstall.SwordB)).Should().BeTrue();
		File.Exists(this._game.OutPath(FakeGameInstall.OldSwordA)).Should().BeFalse();
	}

	[Theory]
	[InlineData("--where", "=5")]
	[InlineData("--id", "/(/")]
	[InlineData("--format", "xml")]
	[InlineData("--limit", "-1")]
	public void Records_InvalidOption_IsUsageError(string option, string value)
	{
		var (code, error) = this._game.Run("records", option, value, "--out", this._game.OutPath("r.tsv"));

		code.Should().Be(2);
		error.Should().StartWith("tqdb records: ");
	}
}
