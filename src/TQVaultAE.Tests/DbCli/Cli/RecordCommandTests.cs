using System.Text.Json;
using AwesomeAssertions;

namespace TQVaultAE.Tests.DbCli.Cli;

public sealed class RecordCommandTests : IDisposable
{
	private readonly FakeGameInstall _game = new();

	public void Dispose() => this._game.Dispose();

	private JsonElement RunJson(params string[] args)
	{
		var outFile = this._game.OutPath($"record-{Guid.NewGuid():N}.json");
		var (code, error) = this._game.Run(["record", "--out", outFile, .. args]);
		code.Should().Be(0, error);
		return JsonDocument.Parse(File.ReadAllText(outFile)).RootElement;
	}

	[Fact]
	public void Record_SingleId_IsOneObjectWithEveryFieldAndDisplayName()
	{
		var record = this.RunJson("RECORDS/ITEM/SWORD/A01.DBR");

		record.ValueKind.Should().Be(JsonValueKind.Object);
		record.GetProperty("id").GetString().Should().Be(FakeGameInstall.SwordA);
		record.GetProperty("class").GetString().Should().Be("WeaponMelee_Sword");
		record.GetProperty("displayName").GetString().Should().Be("Alpha Blade");
		var fields = record.GetProperty("fields");
		fields.EnumerateObject().Select(p => p.Name).Should().Equal("Class", "itemNameTag", "itemLevel", "offensivePhysicalMin", "bitmap");
		fields.GetProperty("itemLevel").GetInt32().Should().Be(25);
		fields.GetProperty("offensivePhysicalMin").GetDouble().Should().Be(12.5);
		record.TryGetProperty("resolved", out _).Should().BeFalse();
	}

	[Fact]
	public void Record_SeveralIds_IsAnArrayInArgumentOrder()
	{
		var records = this.RunJson(FakeGameInstall.Relic, FakeGameInstall.SwordA);

		records.EnumerateArray().Select(r => r.GetProperty("id").GetString()).Should().Equal(FakeGameInstall.Relic, FakeGameInstall.SwordA);
	}

	[Fact]
	public void Record_FieldsAndResolve_ProjectsVariablesAndAddsTheirText()
	{
		var record = this.RunJson(FakeGameInstall.SwordA, "--fields", "item*", "--resolve", "--lang", "fr");

		record.GetProperty("displayName").GetString().Should().Be("Lame Alpha");
		record.GetProperty("fields").EnumerateObject().Select(p => p.Name).Should().Equal("itemNameTag", "itemLevel");
		record.GetProperty("resolved").GetProperty("itemNameTag").GetString().Should().Be("Lame Alpha");
	}

	[Fact]
	public void Record_Follow_InlinesTheReferencedRecords()
	{
		var record = this.RunJson(FakeGameInstall.LootTable, "--follow", "lootName*");

		var linked = record.GetProperty("linked");
		linked.GetProperty("lootName1").EnumerateArray().Single().GetProperty("displayName").GetString().Should().Be("Alpha Blade");
		linked.GetProperty("lootName2").EnumerateArray().Single().GetProperty("id").GetString().Should().Be(FakeGameInstall.SwordB);
	}

	[Fact]
	public void Record_UnknownId_ExitsWith3AndSuggestsRecordsWithTheSameFileName()
	{
		var (code, error) = this._game.Run("record", "records/a01.dbr");

		code.Should().Be(3);
		error.Should().Contain("Record 'records/a01.dbr' not found. Did you mean:")
			.And.Contain(FakeGameInstall.SwordA).And.Contain(FakeGameInstall.OldSwordA);
	}

	[Fact]
	public void Record_UnknownFileName_ExitsWith3WithoutSuggestion()
	{
		var (code, error) = this._game.Run("record", FakeGameInstall.SwordA, "records/nothing.dbr");

		code.Should().Be(3);
		error.Trim().Should().Be("tqdb: Record 'records/nothing.dbr' not found.");
	}

	[Fact]
	public void Record_MissingLanguage_WarnsButStillWritesTheRecord()
	{
		var outFile = this._game.OutPath("r.json");

		var (code, error) = this._game.Run("record", FakeGameInstall.SwordA, "--lang", "DE", "--out", outFile);

		code.Should().Be(0);
		error.Should().Contain("warning: No text archive for language 'DE'");
		var record = JsonDocument.Parse(File.ReadAllText(outFile)).RootElement;
		record.GetProperty("displayName").ValueKind.Should().Be(JsonValueKind.Null);
		record.GetProperty("fields").GetProperty("itemLevel").GetInt32().Should().Be(25);
	}

	[Fact]
	public void Record_DbrToFolder_WritesOneFilePerRecordAtItsPath()
	{
		var (code, _) = this._game.Run("record", FakeGameInstall.SwordA, FakeGameInstall.Relic, "--format", "dbr", "--out", this._game.Out);

		code.Should().Be(0);
		File.ReadAllLines(this._game.OutPath(FakeGameInstall.SwordA)).Should().Contain("itemLevel,25,").And.Contain("offensivePhysicalMin,12.500000,");
		File.ReadAllLines(this._game.OutPath(FakeGameInstall.Relic)).Should().Contain("completedRelicLevel,3,");
	}

	[Fact]
	public void Record_DbrToSingleFile_SeparatesRecordsWithTheirId()
	{
		var outFile = this._game.OutPath("both.dbr");

		var (code, _) = this._game.Run("record", FakeGameInstall.SwordA, FakeGameInstall.Relic, "--out", outFile);

		code.Should().Be(0);
		File.ReadAllLines(outFile).Where(l => l.StartsWith("//", StringComparison.Ordinal))
			.Should().Equal($"// {FakeGameInstall.SwordA}", $"// {FakeGameInstall.Relic}");
	}

	[Fact]
	public void Record_Tsv_ListsOneRowPerVariable()
	{
		var outFile = this._game.OutPath("r.tsv");

		var (code, _) = this._game.Run("record", FakeGameInstall.Relic, "--out", outFile);

		code.Should().Be(0);
		File.ReadAllLines(outFile).Should().Contain($"{FakeGameInstall.Relic}\tItemRelic\tRelic of Tests\tcompletedRelicLevel\tint\t3");
	}

	[Fact]
	public void Record_WithoutId_IsUsageError()
	{
		var (code, _) = this._game.Run("record");

		code.Should().Be(2);
	}
}
