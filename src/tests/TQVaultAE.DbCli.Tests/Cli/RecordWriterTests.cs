using System.Text;
using System.Text.Json;
using AwesomeAssertions;
using TQVaultAE.DbCli.Core;
using TQVaultAE.DbCli.Export;
using TQVaultAE.DbCli.Query;

namespace TQVaultAE.Tests.DbCli.Cli;

public class RecordWriterTests
{
	private static readonly ArzRecord Sword = V.Record(
		@"records\item\sword\a.dbr",
		V.Int("itemLevel", 40),
		V.Float("offensivePhysicalMin", 12.5f),
		V.Int("weights", 5, 50),
		V.Bool("hidePrefixName", true),
		V.Str("itemNameTag", "tagA"),
		V.Str("itemText", "line1\nline2\tend"));

	private static readonly FakeNamer Namer = new(new() { ["tagA"] = "Alpha Blade" });

	private static RecordView View(ArzRecord r, string? displayName = null) => new(r.Info, r) { DisplayName = displayName };

	private static string Json(bool lines, Projection p, IRecordNamer? resolver, params RecordView[] views) {
		using var ms = new MemoryStream();
		using (var sink = new JsonRecordSink(ms, lines, new RecordSinkOptions(p, resolver), indented: false)) {
			foreach (var v in views)
				sink.Write(v);
			sink.Complete();
		}
		return Encoding.UTF8.GetString(ms.ToArray());
	}

	private static string Tsv(Projection p, IRecordNamer? resolver, params RecordView[] views) {
		var sw = new StringWriter();
		var sink = new TsvRecordSink(sw, new RecordSinkOptions(p, resolver));
		foreach (var v in views)
			sink.Write(v);
		sink.Complete();
		return sw.ToString();
	}

	[Fact]
	public void Json_WritesTypedScalarsAndArrays() {
		var json = Json(false, Projection.Parse(["id", "class", "displayName", "*"]), null, View(Sword, "Alpha Blade"));

		using var doc = JsonDocument.Parse(json);
		var rec = doc.RootElement.EnumerateArray().Single();
		rec.GetProperty("id").GetString().Should().Be(@"records\item\sword\a.dbr");
		rec.GetProperty("displayName").GetString().Should().Be("Alpha Blade");
		var fields = rec.GetProperty("fields");
		fields.GetProperty("itemLevel").ValueKind.Should().Be(JsonValueKind.Number);
		fields.GetProperty("itemLevel").GetInt32().Should().Be(40);
		fields.GetProperty("offensivePhysicalMin").GetDouble().Should().Be(12.5);
		fields.GetProperty("hidePrefixName").ValueKind.Should().Be(JsonValueKind.True);
		fields.GetProperty("weights").EnumerateArray().Select(e => e.GetInt32()).Should().Equal(5, 50);
		fields.GetProperty("itemText").GetString().Should().Be("line1\nline2\tend");
	}

	[Fact]
	public void Json_ProjectionLimitsFields_AndMissingDisplayNameIsNull() {
		var json = Json(false, Projection.Parse(["id", "displayName", "itemLevel", "absent"]), null, View(Sword));

		using var doc = JsonDocument.Parse(json);
		var rec = doc.RootElement[0];
		rec.GetProperty("displayName").ValueKind.Should().Be(JsonValueKind.Null);
		rec.GetProperty("fields").EnumerateObject().Select(p => p.Name).Should().Equal("itemLevel");
	}

	[Fact]
	public void Json_NoRecords_IsEmptyArray() {
		Json(false, Projection.Parse([]), null).Trim().Should().Be("[]");
	}

	[Fact]
	public void Json_WithoutDecodedRecord_OmitsFields() {
		var info = Sword.Info;

		var json = Json(false, Projection.Parse(["id", "class"]), null, new RecordView(info, null));

		JsonDocument.Parse(json).RootElement[0].TryGetProperty("fields", out _).Should().BeFalse();
	}

	[Fact]
	public void Jsonl_WritesOneParsableObjectPerLine() {
		var other = V.Record(@"records\b.dbr", V.Int("x", 1));

		var text = Json(true, Projection.Parse(["id", "*"]), null, View(Sword), View(other));

		var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
		lines.Should().HaveCount(2);
		lines.Select(l => JsonDocument.Parse(l).RootElement.GetProperty("id").GetString()).Should().Equal(@"records\item\sword\a.dbr", @"records\b.dbr");
	}

	[Fact]
	public void Json_Resolve_AddsTextOnlyForResolvableTags() {
		var json = Json(false, Projection.Parse(["*"]), Namer, View(Sword));

		var resolved = JsonDocument.Parse(json).RootElement[0].GetProperty("resolved");
		resolved.EnumerateObject().Select(p => (p.Name, p.Value.GetString())).Should().Equal(("itemNameTag", "Alpha Blade"));
	}

	[Fact]
	public void Json_Linked_NestsReferencedRecordsUnderField() {
		var relic = V.Record(@"records\relic.dbr", V.Int("itemLevel", 20));
		var view = View(Sword);
		view.Linked = [("lootRef", [new RecordView(relic.Info, relic) { DisplayName = "Relic" }])];

		var json = Json(false, Projection.Parse(["id"]), null, view);

		var linked = JsonDocument.Parse(json).RootElement[0].GetProperty("linked").GetProperty("lootRef")[0];
		linked.GetProperty("displayName").GetString().Should().Be("Relic");
		linked.GetProperty("fields").GetProperty("itemLevel").GetInt32().Should().Be(20);
	}

	[Fact]
	public void Tsv_Wide_JoinsMultiValues_EscapesControlChars_LeavesMissingEmpty() {
		var tsv = Tsv(Projection.Parse(["id", "weights", "itemText", "absent"]), null, View(Sword));

		var lines = tsv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
		lines[0].Should().Be("id\tweights\titemText\tabsent");
		lines[1].Should().Be(@"records\item\sword\a.dbr" + "\t5;50\tline1\\nline2\\tend\t");
	}

	[Fact]
	public void Tsv_Wide_Resolve_AddsTextColumnPerField() {
		var tsv = Tsv(Projection.Parse(["itemNameTag", "itemLevel"]), Namer, View(Sword));

		var lines = tsv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
		lines[0].Should().Be("itemNameTag\titemNameTag:text\titemLevel\titemLevel:text");
		lines[1].Should().Be("tagA\tAlpha Blade\t40\t");
	}

	[Fact]
	public void Tsv_Star_SwitchesToLongFormat() {
		var tsv = Tsv(Projection.Parse(["id", "item*"]), null, View(Sword));

		var lines = tsv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
		lines[0].Should().Be("id\tclass\tfield\ttype\tvalue");
		lines.Skip(1).Select(l => l.Split('\t')[2]).Should().Equal("itemLevel", "itemNameTag", "itemText");
		lines[1].Should().EndWith("\titemLevel\tint\t40");
	}

	[Fact]
	public void Tsv_NoRecords_StillWritesHeader() {
		Tsv(Projection.Parse([]), null).Should().Be("id\tclass\tdisplayName\n");
	}

	[Fact]
	public void Dbr_WritesArzExplorerLines_FloatsSixDecimals_BoolsAsDigits() {
		var sw = new StringWriter { NewLine = "\n" };
		var sink = DbrRecordSink.ToStream(sw, separators: false);

		sink.Write(View(V.Record("r.dbr", V.Float("f", 1.5f, 2f), V.Bool("b", true), V.Str("s", "x"), V.Int("i", -3))));
		sink.Complete();

		sw.ToString().Should().Be("f,1.500000;2.000000,\nb,1,\ns,x,\ni,-3,\n");
	}

	[Fact]
	public void Dbr_ToStream_WithSeparators_HeadsEachRecordAndSkipsDuplicates() {
		var sw = new StringWriter { NewLine = "\n" };
		var sink = DbrRecordSink.ToStream(sw, separators: true);
		var a = V.Record(@"records\a.dbr", V.Int("x", 1));
		var view = View(a);
		view.Linked = [("self", [View(a)])];

		sink.Write(view);
		sink.Complete();

		sw.ToString().Should().Be("// records\\a.dbr\nx,1,\n");
		sink.Written.Should().Be(1);
	}

	[Fact]
	public void Dbr_ToDirectory_MirrorsRecordPath() {
		var dir = Path.Combine(Path.GetTempPath(), "tqdb-cli-" + Guid.NewGuid().ToString("N"));
		try {
			var sink = DbrRecordSink.ToDirectory(dir);
			sink.Write(View(Sword));

			var file = Path.Combine(dir, "records", "item", "sword", "a.dbr");
			File.Exists(file).Should().BeTrue();
			File.ReadAllLines(file)[0].Should().Be("itemLevel,40,");
		}
		finally {
			if (Directory.Exists(dir))
				Directory.Delete(dir, true);
		}
	}
}
