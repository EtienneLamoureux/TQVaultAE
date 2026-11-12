using System.Text.Json;
using AwesomeAssertions;

namespace TQVaultAE.Tests.DbCli.Cli;

public sealed class TextCommandTests : IDisposable
{
	private readonly FakeGameInstall _game = new();

	public void Dispose() => this._game.Dispose();

	private string[] Lines(params string[] args)
	{
		var outFile = this._game.OutPath($"text-{Guid.NewGuid():N}.tsv");
		var (code, error) = this._game.Run(["text", "-q", "--out", outFile, .. args]);
		code.Should().Be(0, error);
		return File.ReadAllLines(outFile);
	}

	[Fact]
	public void Text_ExactKey_IsLookedUpIgnoringCase()
	{
		// The tag column spells the tag as the game does, like a wildcard search.
		this.Lines("--key", "TAGSWORDA").Should().Equal("tag\ttext", "tagSwordA\tAlpha Blade");
	}

	[Fact]
	public void Text_UnknownExactKey_WritesOnlyTheHeader()
	{
		this.Lines("--key", "tagMissing").Should().Equal("tag\ttext");
	}

	[Fact]
	public void Text_ValueGlob_ReturnsMatchesSortedByTag()
	{
		this.Lines("--value", "*blade").Should().Equal("tag\ttext", "tagSwordA\tAlpha Blade", "tagSwordB\tBeta Blade");
	}

	[Fact]
	public void Text_KeyAndValue_AreAnded()
	{
		this.Lines("--key", "tagSword*", "--value", "/beta/").Should().Equal("tag\ttext", "tagSwordB\tBeta Blade");
	}

	[Fact]
	public void Text_Limit_KeepsTheFirstRows()
	{
		this.Lines("--key", "tag*", "--limit", "1").Should().Equal("tag\ttext", "tagRelic\tRelic of Tests");
	}

	[Fact]
	public void Text_CountWithLimit_IsCappedByTheLimit()
	{
		this.Lines("--key", "tag*", "--count", "--limit", "2").Should().Equal("2");
	}

	[Fact]
	public void Text_Json_UsesTheLanguageArchive()
	{
		var outFile = this._game.OutPath("fr.json");

		var (code, _) = this._game.Run("text", "--lang", "fr", "--value", "*", "--out", outFile);

		code.Should().Be(0);
		var rows = JsonDocument.Parse(File.ReadAllText(outFile)).RootElement.EnumerateArray().ToList();
		rows.Should().ContainSingle();
		rows[0].GetProperty("text").GetString().Should().Be("Lame Alpha");
	}

	[Fact]
	public void Text_UnknownLanguage_ExitsWith3()
	{
		var (code, error) = this._game.Run("text", "--lang", "XX", "--key", "tagSwordA");

		code.Should().Be(3);
		error.Should().Contain("No text archive for language 'XX' (expected Text\\Text_XX.arc).");
	}
}
