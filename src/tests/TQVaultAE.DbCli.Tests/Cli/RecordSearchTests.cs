using AwesomeAssertions;
using TQVaultAE.DbCli.Commands;
using TQVaultAE.DbCli.Query;

namespace TQVaultAE.Tests.DbCli.Cli;

public class RecordSearchTests
{
	private readonly FakeArzDatabase _db = new FakeArzDatabase()
		.Add(@"records\item\sword\a.dbr", "WeaponMelee_Sword", V.Int("itemLevel", 10), V.Str("itemNameTag", "tagA"), V.Str("templateName", @"database\templates\weaponmelee_sword.tpl"), V.Str("lootRef", @"records\item\relic\r.dbr"))
		.Add(@"records\item\sword\b.dbr", "WeaponMelee_Sword", V.Int("itemLevel", 30), V.Str("itemNameTag", "tagB"), V.Str("templateName", @"database\templates\weaponmelee_sword.tpl"))
		.Add(@"records\item\sword\c.dbr", "WeaponMelee_Sword", V.Str("itemNameTag", "tagC"))
		.Add(@"records\item\relic\r.dbr", "ItemRelic", V.Int("itemLevel", 20), V.Str("itemNameTag", "tagRelic"), V.Str("description", "Shard of the Hunt"));

	private readonly FakeNamer _namer = new(new() { ["tagA"] = "Alpha Blade", ["tagB"] = "Beta Blade", ["tagRelic"] = "Hunter Relic" });

	private static RecordQuery Query(params string[] args) {
		var spec = new RecordsCommand().Spec;
		return RecordsCommand.BuildQuery(ArgParser.Parse(["records", .. args], [spec], []));
	}

	private List<RecordView> Run(RecordQuery q, Projection p, SortSpec? sort = null, int limit = 0, IReadOnlyList<string>? follow = null) =>
		RecordSearch.Run(this._db, q, RecordSearch.Plan(q, p, false, sort, limit, follow), this._namer, 1, CancellationToken.None).ToList();

	[Fact]
	public void ClassFilter_RunsBeforeDecompression() {
		var q = Query("--class", "ItemRelic", "--where", "itemLevel>0");

		var hits = this.Run(q, Projection.Parse(["id"]));

		hits.Select(h => h.Info.Id).Should().Equal(@"records\item\relic\r.dbr");
		this._db.Reads.Should().Be(1, "only the record passing the class filter is decoded");
	}

	[Fact]
	public void TableOnlyQuery_WithTableColumns_DecodesNothing() {
		var q = Query("--id", "records/item/sword/*");

		var hits = this.Run(q, Projection.Parse(["id", "class"]));

		hits.Should().HaveCount(3);
		this._db.Reads.Should().Be(0);
	}

	[Fact]
	public void Count_WithoutValueFilters_DecodesNothing() {
		int count = RecordSearch.Count(this._db, Query("--class", "Weapon*"), null, 1, CancellationToken.None);

		count.Should().Be(3);
		this._db.Reads.Should().Be(0);
	}

	[Fact]
	public void LiteralId_UsesIndexedLookup_WithEitherSeparator() {
		var hits = this.Run(Query("--id", "RECORDS/ITEM/SWORD/B.DBR"), Projection.Parse(["id"]));

		hits.Select(h => h.Info.Id).Should().Equal(@"records\item\sword\b.dbr");
	}

	[Fact]
	public void RepeatedIds_AreOred_AndKindsAreAnded() {
		var q = Query("--id", "*/a.dbr", "--id", "*/b.dbr", "--where", "itemLevel>15");

		this.Run(q, Projection.Parse(["id"])).Select(h => h.Info.Id).Should().Equal(@"records\item\sword\b.dbr");
	}

	[Fact]
	public void NameFilter_MatchesResolvedDisplayName_AndProjectsIt() {
		var hits = this.Run(Query("--name", "*blade"), Projection.Parse([]));

		hits.Select(h => h.DisplayName).Should().Equal("Alpha Blade", "Beta Blade");
	}

	[Fact]
	public void TagTemplateAndText_FilterDecodedRecords() {
		this.Run(Query("--tag", "tagR*"), Projection.Parse(["id"])).Should().ContainSingle();
		this.Run(Query("--template", "database/templates/weaponmelee_*"), Projection.Parse(["id"])).Should().HaveCount(2);
		this.Run(Query("--text", "of the hunt"), Projection.Parse(["id"])).Single().Info.Class.Should().Be("ItemRelic");
	}

	[Fact]
	public void Sort_Descending_PutsMissingKeysLast_ThenLimitApplies() {
		var hits = this.Run(Query(), Projection.Parse(["id"]), SortSpec.Parse("itemLevel:desc"), limit: 3);

		hits.Select(h => h.Info.Id).Should().Equal(@"records\item\sword\b.dbr", @"records\item\relic\r.dbr", @"records\item\sword\a.dbr");
	}

	[Fact]
	public void Sort_MissingKey_IsLastInAscendingOrderToo() {
		var hits = this.Run(Query("--class", "Weapon*"), Projection.Parse(["id"]), SortSpec.Parse("itemLevel"));

		hits.Select(h => h.Info.Id).Should().Equal(@"records\item\sword\a.dbr", @"records\item\sword\b.dbr", @"records\item\sword\c.dbr");
	}

	[Fact]
	public void Limit_WithoutSort_StopsDecodingEarly() {
		var hits = this.Run(Query("--where", "itemNameTag?"), Projection.Parse(["id"]), limit: 1);

		hits.Should().ContainSingle();
		this._db.Reads.Should().BeLessThan(4);
	}

	[Fact]
	public void Follow_InlinesReferencedRecordsWithDisplayName() {
		var hits = this.Run(Query("--id", "*/a.dbr"), Projection.Parse(["id"]), follow: ["loot*"]);

		var linked = hits.Single().Linked.Should().ContainSingle().Subject;
		linked.Field.Should().Be("lootRef");
		linked.Records.Single().Info.Id.Should().Be(@"records\item\relic\r.dbr");
		linked.Records.Single().DisplayName.Should().Be("Hunter Relic");
	}

	[Fact]
	public void SortSpec_InvalidDirection_IsUsageError() {
		var act = () => SortSpec.Parse("itemLevel:sideways");

		act.Should().Throw<UsageException>();
	}
}
