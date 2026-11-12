using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using TQVaultAE.Data;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Tests.DbCli;
using static TQVaultAE.Tests.DbCli.DbCliFixtures;

namespace TQVaultAE.Tests.Data;

/// <summary>
/// <see cref="ArzFileProvider"/> maps records of the shared reader to the TQVault model.
/// </summary>
public sealed class ArzFileProviderTests : IDisposable
{
	private const short Int = 0, Float = 1, String = 2, Bool = 3;

	private readonly ArzFileProvider _provider = new(NullLogger<ArzFileProvider>.Instance);
	private readonly List<string> _tempFiles = [];
	private readonly List<ArzFile> _files = [];

	public void Dispose()
	{
		foreach (var file in this._files)
			file.Database?.Dispose();
		foreach (var path in this._tempFiles)
			File.Delete(path);
	}

	private ArzFile Load(params ArzRec[] records) => this.Load([], records);

	/// <param name="strings">Strings placed first in the string table, so their ids are 0, 1, ...</param>
	private ArzFile Load(List<string> strings, params ArzRec[] records)
	{
		var path = WriteTemp(BuildArz(records, strings), ".arz");
		this._tempFiles.Add(path);
		var file = new ArzFile(path);
		this._files.Add(file);
		this._provider.Read(file).Should().BeTrue();
		return file;
	}

	[Fact]
	public void Read_LoadsRecordTableKeyedByNormalizedId()
	{
		var file = this.Load(
			new ArzRec(@"records\item\ring01.dbr", "ArmorJewelry_Ring", 0),
			new ArzRec(@"records/item/amulet01.dbr", "ArmorJewelry_Amulet", 0));

		file.Count.Should().Be(2);
		var info = file.RecordInfo[RecordId.Create(@"RECORDS\ITEM\RING01.DBR")];
		info.RecordType.Should().Be("ArmorJewelry_Ring");
		file.RecordInfo.Should().ContainKey(RecordId.Create(@"records\item\amulet01.dbr"), "'/' and '\\' are the same record path");
	}

	[Fact]
	public void GetItem_KeepsHistoricalValueRepresentation()
	{
		// Booleans stay Int32 and strings are trimmed, as TQVault code compares them that way.
		var file = this.Load(new ArzRec(@"records\item\ring01.dbr", "ArmorJewelry_Ring", 0,
			new ArzVar(Int, "itemLevel", [42]),
			new ArzVar(Float, "characterMana", [BitConverter.SingleToInt32Bits(12.5f)]),
			new ArzVar(Bool, "isQuestItem", [1]),
			new ArzVar(String, "description", [0])));

		var record = this._provider.GetItem(file, RecordId.Create(@"records\item\ring01.dbr"));

		record!.GetInt32("itemLevel", 0).Should().Be(42);
		record.GetSingle("characterMana", 0).Should().Be(12.5f);
		record["isQuestItem"]![0].Should().Be(1);
		record.GetString("description", 0).Should().Be(@"records\item\ring01.dbr", "string id 0 is the record id, trimmed");
	}

	[Fact]
	public void GetItem_WindowsCp1252Strings_DecodeLikeTheGame()
	{
		// The game database references files such as x4tagSQ303_Re’yesh_A1.mp3: byte 0x92 is ’ in cp1252
		// (Latin-1 would give U+0092 and the sound would not be found in the .arc, whose names are cp1252).
		var file = this.Load(["x4tagSQ303_Re’yesh_A1.mp3"], new ArzRec(@"records\sounds\reyesh.dbr", "Sound", 0,
			new ArzVar(String, "soundName", [0])));

		var record = this._provider.GetItem(file, RecordId.Create(@"records\sounds\reyesh.dbr"));

		record!.GetString("soundName", 0).Should().Be("x4tagSQ303_Re’yesh_A1.mp3");
	}

	[Fact]
	public void GetItem_UnknownOrNullId_ReturnsNull()
	{
		var file = this.Load(new ArzRec(@"records\item\ring01.dbr", "ArmorJewelry_Ring", 0));

		this._provider.GetItem(file, RecordId.Create(@"records\item\missing.dbr")).Should().BeNull();
		this._provider.GetItem(file, null!).Should().BeNull();
	}

	[Fact]
	public void Read_NotAnArzFile_ReturnsFalse()
	{
		var path = WriteTemp([1, 2, 3], ".arz");
		this._tempFiles.Add(path);

		this._provider.Read(new ArzFile(path)).Should().BeFalse();
	}
}
