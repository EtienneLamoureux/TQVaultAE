using AwesomeAssertions;
using System.Text;
using TQ.SaveFilesExplorer.Entities;
using TQ.SaveFilesExplorer.Entities.Players;
using TQ.SaveFilesExplorer.Entities.TransferStash;
using TQ.SaveFilesExplorer.Services;

namespace TQVaultAE.Tests.Win32.SaveFilesExplorer;

/// <summary>
/// <see cref="TQFileService"/> splits a save file into typed records, the way TQ Save Files Explorer shows it.
/// </summary>
public sealed class TQFileTests : IDisposable
{
	static TQFileTests() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

	private static Encoding Cp1252 => Encoding.GetEncoding(1252);

	private readonly TQFileService _service = new();
	private readonly string _folder = Path.Combine(Path.GetTempPath(), $"savefilesexplorer-test-{Guid.NewGuid():N}");

	public TQFileTests() => Directory.CreateDirectory(this._folder);

	public void Dispose() => Directory.Delete(this._folder, true);

	/// <summary>
	/// Writes save file content the way the game does: length-prefixed keys followed by their value.
	/// </summary>
	private sealed class SaveWriter
	{
		private readonly List<byte> _bytes = [];

		public SaveWriter Key(string name)
		{
			this._bytes.AddRange(BitConverter.GetBytes(name.Length));
			this._bytes.AddRange(Cp1252.GetBytes(name));
			return this;
		}

		public SaveWriter Int(string name, int value)
			=> this.Key(name).Raw(BitConverter.GetBytes(value));

		public SaveWriter Text(string name, string value)
			=> this.Key(name).Raw(BitConverter.GetBytes(value.Length)).Raw(Cp1252.GetBytes(value));

		public SaveWriter Utf16(string name, string value)
			=> this.Key(name).Raw(BitConverter.GetBytes(value.Length)).Raw(Encoding.Unicode.GetBytes(value));

		public SaveWriter Raw(params byte[] bytes)
		{
			this._bytes.AddRange(bytes);
			return this;
		}

		public byte[] ToArray() => [.. this._bytes];
	}

	private TQFile Read(string fileName, SaveWriter content)
	{
		var path = Path.Combine(this._folder, fileName);
		File.WriteAllBytes(path, content.ToArray());
		return this._service.ReadFile(path);
	}

	private static SaveWriter Player(int headerVersion = 3)
		=> new SaveWriter()
			.Int("headerVersion", headerVersion)
			.Text("playerCharacterClass", "Warrior");

	[Theory]
	[InlineData(1, TQVersion.TQ)]
	[InlineData(2, TQVersion.TQIT)]
	[InlineData(3, TQVersion.TQAE)]
	public void PlayerFile_HeaderVersion_GivesTheGameVersion(int headerVersion, TQVersion expected)
		=> this.Read("Player.chr", Player(headerVersion)).Version.Should().Be(expected);

	[Fact]
	public void PlayerFile_KnownKeys_AreReadWithTheirType()
	{
		var file = this.Read("Player.chr", Player()
			.Int("playerLevel", 68)
			.Utf16("myPlayerName", "Hélène"));

		file.Records.Select(r => (r.KeyName, r.DataType)).Should().Equal(
			("headerVersion", TQFileDataType.Int),
			("playerCharacterClass", TQFileDataType.String1252),
			("playerLevel", TQFileDataType.Int),
			("myPlayerName", TQFileDataType.StringUTF16));
		file.Records[1].DataAsStr.Should().Be("Warrior");
		file.Records[2].DataAsInt.Should().Be(68);
		file.Records[3].DataAsStr.Should().Be("Hélène");
		file.Records.Should().NotContain(r => r.HasError);
	}

	[Fact]
	public void StringValue_ShapedLikeAKey_IsNotTakenForAKey()
	{
		// "Warrior" is stored as 7,0,0,0,"Warrior": the same shape as a key
		var file = this.Read("Player.chr", Player().Int("playerLevel", 68));

		file.Records.Select(r => r.KeyName).Should().Equal("headerVersion", "playerCharacterClass", "playerLevel");
	}

	[Fact]
	public void UnknownKey_IsKeptWithoutType()
	{
		var file = this.Read("Player.chr", Player().Int("newKeyOfAPatch", 5).Int("playerLevel", 68));

		var unknown = file.Records.Single(r => r.KeyName == "newKeyOfAPatch");
		unknown.IsDataTypeError.Should().BeTrue();
		file.Records.Last().DataAsInt.Should().Be(68, "the next key is still found");
	}

	[Fact]
	public void BytesOutsideAnyKey_BecomeAnUnknownSegment()
	{
		var file = this.Read("Player.chr", Player().Raw(0xDE, 0xAD).Int("playerLevel", 68));

		var segment = file.Records.Single(r => r.IsUnknownSegment);
		segment.DataAsByteArray.Should().Equal(0xDE, 0xAD);
		file.Records.Select(r => r.KeyName).Should().Equal("headerVersion", "playerCharacterClass", TQFileRecord.unknown_segment, "playerLevel");
	}

	[Fact]
	public void Blocks_NestTheirKeys()
	{
		var file = this.Read("Player.chr", Player()
			.Int("begin_block", 1)
			.Int("playerLevel", 68)
			.Int("end_block", 1)
			.Int("isInMainQuest", 1));

		file.Childs.Select(r => r.KeyName).Should().Equal("headerVersion", "playerCharacterClass", "begin_block", "isInMainQuest");
		file.Childs[2].Childs.Select(r => r.KeyName).Should().Equal("playerLevel", "end_block");
		file.Childs[2].Childs[0].Parent.Should().BeSameAs(file.Childs[2]);
	}

	[Fact]
	public void StashFile_FirstFourBytes_AreTheCrc()
	{
		var file = this.Read("winsys.dxb", new SaveWriter()
			.Raw(BitConverter.GetBytes(123456))
			.Int("begin_block", 1)
			.Int("stashVersion", 5)
			.Text("fName", "winsys.dxb")
			.Int("end_block", 1));

		file.Version.Should().Be(TQVersion.TQAE);
		var crc = file.Records[0];
		crc.KeyName.Should().Be(nameof(TQFilePlayerTransferStashKey.CRC));
		crc.DataAsInt.Should().Be(123456);
		(crc.KeyIndex, crc.KeyEnd).Should().Be((0, 3), "the CRC has no key: its range is its value");
		file.Records.Should().NotContain(r => r.HasError);
	}

	[Fact]
	public void PlayerFile_WithoutHeaderVersion_IsRejected()
	{
		var act = () => this.Read("Player.chr", new SaveWriter().Int("playerLevel", 68));

		act.Should().Throw<InvalidDataException>().WithMessage($"*{nameof(TQFilePlayerRecordKey.headerVersion)}*");
	}

	[Theory]
	[InlineData("""C:\saves\_hero\Player.chr""", true)]
	[InlineData("""C:\saves\_hero\Player.CHR""", true)]
	[InlineData("""C:\saves\_hero\winsys.dxb""", true)]
	[InlineData("""C:\saves\_hero\WINSYS.DXG""", true)]
	[InlineData("""C:\saves\_hero\settings.txt""", false)]
	[InlineData("""C:\saves\_hero\Levels_World_World01.map""", false)]
	public void IsSaveFile_MatchesTheSaveExtensionsInAnyCase(string path, bool expected)
		=> TQFile.IsSaveFile(path).Should().Be(expected);
}
