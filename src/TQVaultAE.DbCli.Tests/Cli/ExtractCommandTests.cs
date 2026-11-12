using AwesomeAssertions;
using static TQVaultAE.Tests.DbCli.DbCliFixtures;

namespace TQVaultAE.Tests.DbCli.Cli;

public sealed class ExtractCommandTests : IDisposable
{
	/// <summary>Last-write time the fixture archives store for every entry.</summary>
	private static readonly DateTime EntryTime = DateTime.FromFileTimeUtc(0x01D0_0000_0000_0000L);

	private readonly FakeGameInstall _game = new();

	public void Dispose() => this._game.Dispose();

	[Fact]
	public void Extract_RawCopy_MirrorsArchiveKeyAndKeepsBytesAndTimestamp()
	{
		var (code, _) = this._game.Run("extract", "-q", "--archive", "Sounds", "--path", "hit.mp3", "--out", this._game.Out);

		code.Should().Be(0);
		var file = this._game.OutPath("""Audio\Sounds\hit.mp3""");
		File.ReadAllBytes(file).Should().Equal(FakeGameInstall.HitSound);
		File.GetLastWriteTimeUtc(file).Should().Be(EntryTime);
	}

	[Fact]
	public void Extract_WithoutOut_IsUsageError()
	{
		var (code, error) = this._game.Run("extract", "--archive", "Sounds");

		code.Should().Be(2);
		error.Should().Contain("--out <dir> is required").And.Contain("tqdb extract --help");
	}

	[Theory]
	[InlineData("--convert", "gif")]
	[InlineData("--frames", "some")]
	[InlineData("--parallel", "0")]
	public void Extract_InvalidOptionValue_IsUsageError(string option, string value)
	{
		var (code, error) = this._game.Run("extract", "--out", this._game.Out, option, value);

		code.Should().Be(2);
		error.Should().Contain(option);
		Directory.Exists(this._game.Out).Should().BeFalse();
	}

	[Fact]
	public void Extract_NothingMatches_SucceedsWithoutCreatingTheFolder()
	{
		var (code, error) = this._game.Run("extract", "--ext", "xyz", "--out", this._game.Out);

		code.Should().Be(0);
		error.Should().Contain("No entry matched.");
		Directory.Exists(this._game.Out).Should().BeFalse();
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Extract_ConvertPng_WritesPngInPlaceOfTex(bool verify)
	{
		string[] args = ["extract", "-q", "--archive", "Resources/Items", "--path", "sword/a01.tex", "--convert", "png", "--out", this._game.Out];

		var (code, _) = this._game.Run(verify ? [.. args, "--verify"] : args);

		code.Should().Be(0);
		var folder = this._game.OutPath("""Resources\Items\sword""");
		Directory.GetFiles(folder).Select(Path.GetFileName).Should().Equal("a01.png");
		File.ReadAllBytes(Path.Combine(folder, "a01.png")).Take(4).Should().Equal(0x89, (byte)'P', (byte)'N', (byte)'G');
	}

	[Fact]
	public void Extract_ConvertDdsAllFrames_WritesOneFilePerFrame()
	{
		var (code, _) = this._game.Run("extract", "-q", "--path", "sword/anim.tex", "--convert", "dds", "--frames", "all", "--out", this._game.Out);

		code.Should().Be(0);
		var folder = this._game.OutPath("""Resources\Items\sword""");
		Directory.GetFiles(folder).Select(Path.GetFileName).Should().BeEquivalentTo("anim_frame0.dds", "anim_frame1.dds");
		File.ReadAllBytes(Path.Combine(folder, "anim_frame1.dds")).Take(4).Should().Equal("DDS "u8.ToArray());
	}

	[Fact]
	public void Extract_ConvertFirstFrame_WritesSingleFileWithoutFrameSuffix()
	{
		var (code, _) = this._game.Run("extract", "-q", "--path", "sword/anim.tex", "--convert", "dds", "--out", this._game.Out);

		code.Should().Be(0);
		Directory.GetFiles(this._game.OutPath("""Resources\Items\sword""")).Select(Path.GetFileName).Should().Equal("anim.dds");
	}

	[Fact]
	public void Extract_UnconvertibleTexture_WritesRawTexAndWarns()
	{
		var (code, error) = this._game.Run("extract", "--path", "sword/bad.tex", "--convert", "png", "--out", this._game.Out);

		code.Should().Be(0);
		File.ReadAllBytes(this._game.OutPath("""Resources\Items\sword\bad.tex""")).Should().Equal(FakeGameInstall.BrokenTexture);
		error.Should().Contain("""warning: Items\sword\bad.tex: png conversion failed""")
			.And.Contain("1 file(s) written")
			.And.Contain("1 conversion fallback(s), 0 failed");
	}

	[Fact]
	public void Extract_WithoutConvert_CopiesTexturesRaw()
	{
		var (code, _) = this._game.Run("extract", "-q", "--archive", "Resources/Items", "--path", "sword/a01.tex", "--out", this._game.Out);

		code.Should().Be(0);
		File.ReadAllBytes(this._game.OutPath("""Resources\Items\sword\a01.tex""")).Should().Equal(FakeGameInstall.TextureA);
	}

	[Fact]
	public void Extract_VerifyWithBadChecksum_FailsThatEntryOnly_AndLeavesNoPartialFile()
	{
		var (code, error) = this._game.Run("extract", "--archive", "Sounds", "--verify", "--out", this._game.Out);

		code.Should().Be(1);
		error.Should().Contain("error: Audio/Sounds/corrupt.wav:").And.Contain("Adler-32 mismatch").And.Contain("1 file(s) written").And.Contain("1 failed");
		File.Exists(this._game.OutPath("""Audio\Sounds\corrupt.wav""")).Should().BeFalse();
		File.ReadAllBytes(this._game.OutPath("""Audio\Sounds\hit.mp3""")).Should().Equal(FakeGameInstall.HitSound);
	}

	[Fact]
	public void Extract_WithoutVerify_DoesNotCheckChecksums()
	{
		var (code, _) = this._game.Run("extract", "-q", "--archive", "Sounds", "--out", this._game.Out);

		code.Should().Be(0);
		File.ReadAllBytes(this._game.OutPath("""Audio\Sounds\corrupt.wav""")).Should().Equal(FakeGameInstall.HitSound);
	}

	[Theory]
	[InlineData(false, 0, false)] // default: existing files are overwritten
	[InlineData(true, 0, true)] // same size: kept
	[InlineData(true, 1, false)] // other size: replaced
	public void Extract_ExistingFile_IsKeptOnlyWithSkipExistingAndSameSize(bool skipExisting, int sizeDelta, bool kept)
	{
		var target = this._game.OutPath("""Audio\Sounds\hit.mp3""");
		Directory.CreateDirectory(Path.GetDirectoryName(target)!);
		var existing = new byte[FakeGameInstall.HitSound.Length + sizeDelta];
		File.WriteAllBytes(target, existing);
		string[] args = ["extract", "--archive", "Sounds", "--path", "hit.mp3", "--out", this._game.Out];

		var (code, error) = this._game.Run(skipExisting ? [.. args, "--skip-existing"] : args);

		code.Should().Be(0);
		File.ReadAllBytes(target).Should().Equal(kept ? existing : FakeGameInstall.HitSound);
		error.Should().Contain(kept ? "0 file(s) written (0 B), 1 skipped" : "1 file(s) written (500 B), 0 skipped");
	}

	[Fact]
	public void Extract_Flat_DropsTheArchiveKey()
	{
		var (code, _) = this._game.Run("extract", "-q", "--archive", "Sounds", "--path", "hit.mp3", "--flat", "--out", this._game.Out);

		code.Should().Be(0);
		Directory.GetFiles(this._game.Out, "*", SearchOption.AllDirectories).Should().Equal(this._game.OutPath("hit.mp3"));
	}

	[Fact]
	public void Extract_FlatWithTheSameNameInSeveralArchives_WritesOnlyTheLastArchivesEntry()
	{
		// Resources/Items and Resources/XPack/Items both hold sword/a01.tex; parallel writers to one path race.
		var (code, error) = this._game.Run("extract", "--path", "sword/a01.tex", "--flat", "--parallel", "8", "--out", this._game.Out);

		code.Should().Be(0, error);
		File.ReadAllBytes(this._game.OutPath("""sword\a01.tex""")).Should().Equal(FakeGameInstall.TextureAXPack);
		error.Should().Contain("1 file(s) written").And.Contain("--flat: 1 entry replaced by a later archive with the same name.");
	}

	[Theory]
	[InlineData("../evil.mp3", "escapes the output folder")]
	[InlineData("/rooted.mp3", "Rooted entry name")]
	public void Extract_UnsafeEntryName_FailsThatEntryWithoutWritingOutside(string name, string message)
	{
		this._game.WriteArc("""Resources\Evil.arc""", Stored(name, [1, 2, 3]), Stored("fine.mp3", [4, 5]));

		var (code, error) = this._game.Run("extract", "--archive", "Evil", "--flat", "--out", this._game.Out);

		code.Should().Be(1);
		error.Should().Contain(message).And.Contain("1 file(s) written").And.Contain("1 failed");
		Directory.GetFiles(this._game.Root, "*.mp3", SearchOption.AllDirectories).Should().Equal(this._game.OutPath("fine.mp3"));
	}

	[Fact]
	public void Extract_MoreThanTwentyFailures_ReportsTheFirstTwentyAndTheRest()
	{
		var entries = Enumerable.Range(0, 25).Select(i => Stored($"../bad{i:D2}.mp3", [1])).ToArray();
		this._game.WriteArc("""Resources\Evil.arc""", entries);

		var (code, error) = this._game.Run("extract", "--archive", "Evil", "--out", this._game.Out);

		code.Should().Be(1);
		error.Split('\n').Count(l => l.StartsWith("error: Resources/Evil/", StringComparison.Ordinal)).Should().Be(20);
		error.Should().Contain("error: ... and 5 more").And.Contain("25 failed");
	}

	[Theory]
	[InlineData(0, "0 B")]
	[InlineData(1023, "1023 B")]
	[InlineData(1536, "1.5 KB")]
	[InlineData(5L * 1024 * 1024, "5.0 MB")]
	[InlineData(3L * 1024 * 1024 * 1024, "3.00 GB")]
	public void FormatBytes_PicksTheLargestUnitBelow1024(long bytes, string expected)
	{
		TQVaultAE.DbCli.Commands.ExtractCommand.FormatBytes(bytes).Should().Be(expected.Replace(".", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator));
	}
}
