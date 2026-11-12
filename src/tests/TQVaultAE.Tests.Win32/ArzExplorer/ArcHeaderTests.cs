using AwesomeAssertions;

namespace TQVaultAE.Tests.Win32.ArzExplorer;

/// <summary>
/// <see cref="global::ArzExplorer.MainForm.HasArcHeader"/> tells an ARC without entries (the game ships
/// XPack4\Underground.arc empty) from a file that is not an ARC, so the empty one is not reported as unreadable.
/// </summary>
public sealed class ArcHeaderTests : IDisposable
{
	private readonly string _path = Path.Combine(Path.GetTempPath(), $"claude-archeader-{Guid.NewGuid():N}.arc");

	public void Dispose()
	{
		if (File.Exists(this._path))
			File.Delete(this._path);
	}

	[Fact]
	public void EmptyArc_HasTheHeader()
	{
		// Same layout as the game's empty Underground.arc: magic, version 1, no entries, padded to 2048 bytes.
		var bytes = new byte[2048];
		"ARC\0"u8.CopyTo(bytes);
		bytes[4] = 1;
		File.WriteAllBytes(this._path, bytes);

		global::ArzExplorer.MainForm.HasArcHeader(this._path).Should().BeTrue();
	}

	[Theory]
	[InlineData(new byte[] { 0x02, 0x00, 0x03, 0x00, 0x10, 0x00 })] // ARZ header
	[InlineData(new byte[] { (byte)'A', (byte)'R' })] // shorter than the magic
	[InlineData(new byte[0])]
	public void OtherFiles_DoNotHaveTheHeader(byte[] content)
	{
		File.WriteAllBytes(this._path, content);

		global::ArzExplorer.MainForm.HasArcHeader(this._path).Should().BeFalse();
	}

	[Fact]
	public void MissingFile_DoesNotHaveTheHeader()
		=> global::ArzExplorer.MainForm.HasArcHeader(this._path).Should().BeFalse();
}
