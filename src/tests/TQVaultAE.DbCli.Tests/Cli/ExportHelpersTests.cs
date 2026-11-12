using System.Text;
using System.Text.Json;
using AwesomeAssertions;
using TQVaultAE.DbCli.Commands;
using TQVaultAE.DbCli.Core;
using TQVaultAE.DbCli.Export;
using TQVaultAE.DbCli.Query;

namespace TQVaultAE.Tests.DbCli.Cli;

public class ExportHelpersTests
{
	[Theory]
	[InlineData("../evil.txt")]
	[InlineData("a/../../evil.txt")]
	[InlineData("/etc/passwd")]
	[InlineData(@"\windows\x")]
	[InlineData("C:/x.txt")]
	public void PathSanitizer_RejectsEscapingOrRootedNames(string name) {
		var act = () => PathSanitizer.Relative(name);

		act.Should().Throw<InvalidDataException>();
	}

	[Fact]
	public void PathSanitizer_ReplacesInvalidChars_KeepsCp1252Letters_GuardsDeviceNames() {
		var rel = PathSanitizer.Relative("textures/./caf\u00e9:x?.tex/con.tex/trail. ");

		rel.Split(Path.DirectorySeparatorChar).Should().Equal("textures", "caf\u00e9_x_.tex", "_con.tex", "trail");
	}

	[Fact]
	public void PathSanitizer_Combine_StaysUnderRoot() {
		var root = Path.Combine(Path.GetTempPath(), "tqdb-root");

		var full = PathSanitizer.Combine(root, @"Resources/Items\weapons/x.tex");

		full.Should().StartWith(Path.GetFullPath(root) + Path.DirectorySeparatorChar);
		full.Should().EndWith(Path.Combine("Resources", "Items", "weapons", "x.tex"));
	}

	[Theory]
	[InlineData(null, "out.json", "Json")]
	[InlineData(null, "out.JSONL", "Jsonl")]
	[InlineData(null, "out.bin", "Tsv")]
	[InlineData(null, null, "Tsv")]
	[InlineData("JSON", "out.tsv", "Json")]
	public void Formats_Resolve_ExplicitWinsElseExtensionElseFallback(string? format, string? outPath, string expected) {
		Formats.Resolve(format, outPath, OutputFormat.Tsv, OutputFormat.Json, OutputFormat.Jsonl, OutputFormat.Tsv).Should().Be(Enum.Parse<OutputFormat>(expected));
	}

	[Theory]
	[InlineData("dbr")]
	[InlineData("xml")]
	[InlineData("1")]
	public void Formats_Resolve_UnsupportedFormat_IsUsageError(string format) {
		var act = () => Formats.Resolve(format, null, OutputFormat.Tsv, OutputFormat.Json, OutputFormat.Tsv);

		act.Should().Throw<UsageException>();
	}

	[Fact]
	public void TableSink_Tsv_EscapesCellsAndFormatsDates() {
		using var ms = new MemoryStream();
		using (var sink = new TableSink(ms, OutputFormat.Tsv, "tag", "text", "when")) {
			sink.Row("tagX", "a\tb\r\nc", new DateTime(2020, 5, 6, 7, 8, 9, DateTimeKind.Utc));
			sink.Complete();
		}

		Encoding.UTF8.GetString(ms.ToArray()).Should().Be("tag\ttext\twhen\ntagX\ta\\tb\\r\\nc\t2020-05-06T07:08:09Z\n");
	}

	[Fact]
	public void TableSink_Json_TypesNumbersAndKeepsAccents() {
		using var ms = new MemoryStream();
		using (var sink = new TableSink(ms, OutputFormat.Json, "name", "size")) {
			sink.Row("\u00e9p\u00e9e.tex", 123L);
			sink.Complete();
		}

		var text = Encoding.UTF8.GetString(ms.ToArray());
		text.Should().Contain("\u00e9p\u00e9e.tex");
		var row = JsonDocument.Parse(text).RootElement.EnumerateArray().Single();
		row.GetProperty("size").GetInt64().Should().Be(123);
	}

	[Fact]
	public void TableSink_Jsonl_WritesIndependentObjectsWithoutSeparators() {
		using var ms = new MemoryStream();
		using (var sink = new TableSink(ms, OutputFormat.Jsonl, "tag")) {
			sink.Row("a");
			sink.Row("b");
			sink.Complete();
		}

		Encoding.UTF8.GetString(ms.ToArray()).Should().Be("{\"tag\":\"a\"}\n{\"tag\":\"b\"}\n");
	}

	[Fact]
	public void TextSearch_KeysAndValuesAreAnded_ResultsSortedByTag() {
		var text = new FakeText(new() {
			["tagZ"] = "Hooked Blade",
			["tagA"] = "Hooked Spear",
			["tagM"] = "Plain Sword",
			["xOther"] = "Hooked Thing",
		});

		var hits = TextCommand.Search(text, [Pattern.Parse("tag*")], [Pattern.Parse("hooked*")]);

		hits.Select(h => h.Key).Should().Equal("tagA", "tagZ");
	}

	[Fact]
	public void ArchiveSelection_MatchesArchiveByKeyFileOrStem_AndEntriesByExtension() {
		var sel = new ArchiveSelection();
		sel.Archives.Add(Pattern.Parse("Items", pathLike: true));
		sel.AddExtensions(["tex", ".MP3"]);

		sel.MatchesArchive("Resources/XPack/Items", @"C:\TQ\Resources\XPack\Items.arc").Should().BeTrue();
		sel.MatchesArchive("Resources/Creatures", @"C:\TQ\Resources\Creatures.arc").Should().BeFalse();
		sel.Extensions.Should().BeEquivalentTo(["tex", "mp3"]);
	}

	[Fact]
	public void InfoHistogram_SortsByCountThenName_AndLabelsEmptyClass() {
		var db = new FakeArzDatabase().Add("a", "B").Add("b", "A").Add("c", "A").Add("d", "");

		var hist = InfoCommand.ClassHistogram(db.Records);

		hist.Select(kv => (kv.Key, kv.Value)).Should().Equal(("A", 2), ("(none)", 1), ("B", 1));
	}

	private sealed class FakeText(Dictionary<string, string> entries) : ITextDatabase
	{
		public string Language => "EN";

		public IReadOnlyDictionary<string, string> Entries { get; } = entries;

		public string? Resolve(string? tag) => tag is not null && entries.TryGetValue(tag, out var t) ? t : null;
	}
}
