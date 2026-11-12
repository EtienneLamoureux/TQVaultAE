using System.Text;
using AwesomeAssertions;
using TQVaultAE.DbCli.Core;
using static TQVaultAE.Tests.DbCli.DbCliFixtures;

namespace TQVaultAE.Tests.DbCli;

public sealed class TextDatabaseTests
{
	private static Dictionary<string, string> Parse(string text)
	{
		var d = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		TextDatabase.Parse(text, d);
		return d;
	}

	[Fact]
	public void ReadEntries_WithoutBom_DecodesCp1252()
	{
		// Some game files (x2 quests, mod strings) have no BOM and accented cp1252 text.
		byte[] cp1252 = [.. "nordic_questgiver=Hej och v"u8, 0xE4, .. "lkommen\r\n// comment\r\ntagB = a = b"u8];

		var entries = TextDatabaseFactory.ReadEntries(cp1252);

		entries.Should().HaveCount(2);
		entries["NORDIC_QUESTGIVER"].Should().Be("Hej och välkommen");
		entries["tagB"].Should().Be("a = b");
	}

	[Fact]
	public void ReadEntries_WithUtf16Bom_DecodesUnicode()
	{
		var utf16 = Encoding.Unicode.GetPreamble().Concat(Encoding.Unicode.GetBytes("tagRu=Меч")).ToArray();

		TextDatabaseFactory.ReadEntries(utf16)["tagRu"].Should().Be("Меч");
	}

	[Fact]
	public void Parse_SplitsOnFirstEqualsOnly()
	{
		var d = Parse("tagFormula=Damage = {%d0} x 2\r\n");

		d["tagFormula"].Should().Be("Damage = {%d0} x 2");
	}

	[Fact]
	public void Parse_SkipsCommentsBlankAndKeylessLines()
	{
		var d = Parse("""
			// header comment
			  // indented comment

			=orphan value
			no equals here
			tagA=Alpha
			""");

		d.Should().ContainSingle().Which.Should().Be(new KeyValuePair<string, string>("tagA", "Alpha"));
	}

	[Fact]
	public void Parse_KeysAreCaseInsensitiveAndLastDefinitionWins()
	{
		var d = Parse("tagX=first\ntagx=second\nTAGX=third");

		d.Should().ContainSingle();
		d["TagX"].Should().Be("third");
	}

	[Fact]
	public void Parse_KeepsColorCodesAndGenderMarkersRaw()
	{
		var d = Parse("tagStyle=[ms]renforcé[fs]renforcée\ntagRare={^y}Rare");

		d["tagStyle"].Should().Be("[ms]renforcé[fs]renforcée");
		d["tagRare"].Should().Be("{^y}Rare");
	}

	[Fact]
	public void DecodeText_Utf16WithBom()
	{
		var bytes = Encoding.Unicode.GetPreamble().Concat(Encoding.Unicode.GetBytes("tagA=Épée")).ToArray();

		TextDatabase.DecodeText(bytes).Should().Be("tagA=Épée");
	}

	[Fact]
	public void DecodeText_NoBom_IsCp1252()
	{
		// 0x92 is a right single quote in cp1252 (a C1 control in Latin-1).
		byte[] bytes = [.. "tagA=god"u8, 0x92, .. "s"u8];

		TextDatabase.DecodeText(bytes).Should().Be("tagA=god’s");
	}

	[Fact]
	public void Load_ReadsEveryTxtInArchiveOrder_IgnoresOtherFiles()
	{
		var utf16 = Encoding.Unicode.GetPreamble().Concat(Encoding.Unicode.GetBytes("tagShared=from utf16\r\ntagOnlyA=A")).ToArray();
		byte[] cp1252 = [.. "tagShared=from cp1252\r\ntagOnlyB=B"u8, 0x92];
		var arcBytes = BuildArc([
			Compressed("a.txt", utf16),
			Compressed("b.txt", cp1252),
			Compressed("readme.dat", "tagShared=not text"u8.ToArray()),
		]);
		var path = WriteTemp(arcBytes, ".arc");
		try {
			var text = TextDatabaseFactory.Load(path, "EN");

			text.Language.Should().Be("EN");
			text.Entries.Should().HaveCount(3);
			text.Resolve("TAGSHARED").Should().Be("from cp1252");
			text.Resolve("tagOnlyA").Should().Be("A");
			text.Resolve(" tagOnlyB ").Should().Be("B’");
			text.Resolve("tagMissing").Should().BeNull();
			text.Resolve(null).Should().BeNull();
		} finally {
			File.Delete(path);
		}
	}
}
