using AwesomeAssertions;
using TQVaultAE.DbCli.Core;
using static TQVaultAE.Tests.DbCli.DbCliFixtures;

namespace TQVaultAE.Tests.DbCli;

public sealed class ArzDatabaseTests : IDisposable
{
	private const string SwordId = """records\item\equipmentweapon\sword\c10_sword01.dbr""";
	private const string RelicId = """records\item\relics\relic01.dbr""";
	private static readonly long SwordTime = new DateTime(2017, 8, 31, 12, 0, 0, DateTimeKind.Utc).ToFileTimeUtc();

	private readonly string _path;
	private readonly IArzDatabase _db;

	public ArzDatabaseTests()
	{
		var strings = new List<string>();
		var records = new List<ArzRec> {
			new(SwordId, "WeaponMelee_Sword", SwordTime,
				new ArzVar(2, "Class", [Str(strings, "WeaponMelee_Sword")]),
				new ArzVar(2, "itemNameTag", [Str(strings, "tagItemHookedBlade")]),
				new ArzVar(0, "itemLevel", [12]),
				new ArzVar(1, "offensivePhysicalMin", [BitConverter.SingleToInt32Bits(10.5f), BitConverter.SingleToInt32Bits(-2.25f)]),
				new ArzVar(3, "cannotPickUp", [0]),
				new ArzVar(0, "bigValue", [123_456_789]),
				new ArzVar(0, "noValues", [])),
			new(RelicId, "ItemRelic", 0,
				new ArzVar(2, "description", [Str(strings, "tagRelic01")]),
				new ArzVar(3, "flag", [1])),
		};
		for (int i = 0; i < 50; i++)
			records.Add(new($"""records\bulk\r{i:D3}.dbr""", "Bulk", 0, new ArzVar(0, "n", [i])));

		this._path = WriteTemp(BuildArz(records, strings), ".arz");
		this._db = ArzDatabaseFactory.Open(this._path);
	}

	private static int Str(List<string> strings, string s)
	{
		strings.Add(s);
		return strings.Count - 1;
	}

	public void Dispose()
	{
		this._db.Dispose();
		File.Delete(this._path);
	}

	[Fact]
	public void Open_IndexesRecordTable_WithAbsoluteOffsetsClassAndTime()
	{
		this._db.Records.Should().HaveCount(52);
		var sword = this._db.Records[0];
		sword.Id.Should().Be(SwordId);
		sword.Class.Should().Be("WeaponMelee_Sword");
		sword.Offset.Should().Be(24, "the first blob starts right after the 24-byte header");
		sword.LastWrite.Should().Be(new DateTime(2017, 8, 31, 12, 0, 0, DateTimeKind.Utc));
		this._db.Records[1].Offset.Should().Be(24 + sword.CompressedSize);
		this._db.Records[51].Index.Should().Be(51);
	}

	[Fact]
	public void Read_DecodesEveryValueTypeAndMultipleValues()
	{
		var rec = this._db.Read(this._db.Records[0]);

		rec.Variables.Select(v => v.Name).Should().Equal("Class", "itemNameTag", "itemLevel", "offensivePhysicalMin", "cannotPickUp", "bigValue", "noValues");
		rec.Get("ITEMNAMETAG")!.Values.Should().Equal("tagItemHookedBlade");
		rec.Get("itemLevel")!.Values.Should().Equal(12);
		rec.Get("offensivePhysicalMin")!.Type.Should().Be(ArzValueType.Float);
		rec.Get("offensivePhysicalMin")!.Values.Should().Equal(10.5f, -2.25f);
		rec.Get("cannotPickUp")!.Values.Should().Equal(false);
		rec.Get("bigValue")!.Values.Should().Equal(123_456_789);
		rec.Get("noValues")!.Values.Should().BeEmpty();
		this._db.Read(this._db.Records[1]).Get("flag")!.Values.Should().Equal(true);
	}

	[Theory]
	[InlineData("""records\item\equipmentweapon\sword\c10_sword01.dbr""")]
	[InlineData("records/item/equipmentweapon/sword/c10_sword01.dbr")]
	[InlineData("""RECORDS\Item\EquipmentWeapon\Sword\C10_SWORD01.DBR""")]
	public void Find_IsCaseInsensitiveAndAcceptsBothSeparators(string id)
		=> this._db.Find(id).Should().BeSameAs(this._db.Records[0]);

	[Fact]
	public void Find_UnknownId_ReturnsNull()
		=> this._db.Find("records/none.dbr").Should().BeNull();

	[Fact]
	public void ReadMany_Ordered_PreservesInputOrderAndAppliesPredicate()
	{
		var result = this._db.ReadMany(this._db.Records, r => r.Info.Class == "Bulk", ordered: true, maxDegreeOfParallelism: 8, cancellationToken: TestContext.Current.CancellationToken).ToList();

		result.Select(r => (int)r.Get("n")!.Values[0]).Should().Equal(Enumerable.Range(0, 50));
	}

	[Fact]
	public void ReadMany_Unordered_ReturnsEveryRecordOnce()
	{
		var ids = this._db.ReadMany(this._db.Records, cancellationToken: TestContext.Current.CancellationToken).Select(r => r.Info.Index).ToList();

		ids.Should().HaveCount(52).And.OnlyHaveUniqueItems();
	}

	[Fact]
	public void ReadMany_Cancelled_Throws()
	{
		using var cts = new CancellationTokenSource();
		cts.Cancel();

		var act = () => this._db.ReadMany(this._db.Records, cancellationToken: cts.Token).ToList();

		act.Should().Throw<OperationCanceledException>();
	}

	[Fact]
	public void Open_WrongHeader_ThrowsInvalidData()
	{
		var bytes = File.ReadAllBytes(this._path);
		bytes[0] = 0x05;
		var path = WriteTemp(bytes, ".arz");
		try {
			var act = () => ArzDatabaseFactory.Open(path);
			act.Should().Throw<InvalidDataException>();
		} finally {
			File.Delete(path);
		}
	}
}
