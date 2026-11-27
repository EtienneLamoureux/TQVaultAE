using System.Text;
using AwesomeAssertions;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Tests.Domain;

public class SaveRecordScannerTests
{
	/// <summary>A save string: 32-bit length, then the ASCII text.</summary>
	private static byte[] SaveString(string text)
		=> [.. BitConverter.GetBytes(text.Length), .. Encoding.ASCII.GetBytes(text)];

	private static byte[] Save(params byte[][] parts) => [.. parts.SelectMany(p => p)];

	[Fact]
	public void ReadRecordIds_FindsTheRecordStringsAmongTheOtherData()
	{
		var data = Save(
			SaveString("itemName"),
			SaveString("""Records\XPack\Items\Ring\ring01.dbr"""),
			[1, 0, 0, 0, 0xFF, 0x7F],
			SaveString("relicName"),
			SaveString("records/customitems/sword.DBR"),
			SaveString("""records\XPack\Items\Ring\ring01.dbr"""));

		SaveRecordScanner.ReadRecordIds(data).Should().BeEquivalentTo(
			"""records\xpack\items\ring\ring01.dbr""",
			"""records\customitems\sword.dbr""");
	}

	[Fact]
	public void ReadRecordIds_IgnoresStringsThatAreNotWholeRecordIds()
	{
		var truncated = SaveString("""records\items\ring.dbr""")[..^3];
		var data = Save(
			SaveString("""records\items\ring.tex"""),
			SaveString("""Items\records\ring.dbr"""),
			SaveString("records\\it\u0001ms\\ring.dbr"),
			truncated);

		SaveRecordScanner.ReadRecordIds(data).Should().BeEmpty();
	}
}
