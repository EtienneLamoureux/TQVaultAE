using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using TQVaultAE.Config;
using TQVaultAE.Data;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Services;
using static TQVaultAE.Tests.DbCli.DbCliFixtures;

namespace TQVaultAE.Tests.Data;

/// <summary>
/// <see cref="ArcFileProvider"/> exposes archives of the shared reader through the TQVault model.
/// </summary>
public sealed class ArcFileProviderTests : IDisposable
{
	private readonly ArcFileProvider _provider = new(NullLogger<ArcFileProvider>.Instance, new PathIO(), new DirectoryIO(), new UserSettings());
	private readonly List<string> _tempFiles = [];
	private readonly List<ArcFile> _files = [];

	public void Dispose()
	{
		foreach (var file in this._files)
			file.Archive?.Dispose();
		foreach (var path in this._tempFiles)
			if (File.Exists(path))
				File.Delete(path);
			else if (Directory.Exists(path))
				Directory.Delete(path, true);
	}

	private ArcFile Open(params ArcSpec[] entries)
	{
		var path = WriteTemp(BuildArc(entries), ".arc");
		this._tempFiles.Add(path);
		var file = new ArcFile(path);
		this._files.Add(file);
		return file;
	}

	private static byte[] Content(int length, int seed)
	{
		var bytes = new byte[length];
		new Random(seed).NextBytes(bytes);
		return bytes;
	}

	[Fact]
	public void GetData_ResourceIdWithArchivePrefix_ReturnsContent()
	{
		var big = Content(600_000, 1); // spans several zlib parts
		var stored = Content(1000, 2);
		var file = this.Open(Compressed("creatures/monster/skin.tex", big), Stored("ui/frame.tex", stored));
		var prefix = Path.GetFileNameWithoutExtension(file.FileName);

		this._provider.GetData(file, RecordId.Create($@"{prefix}\creatures\monster\skin.tex")).Should().Equal(big);
		this._provider.GetData(file, RecordId.Create($@"{prefix}\UI\FRAME.TEX")).Should().Equal(stored, "lookups ignore case and separators");
	}

	[Fact]
	public void Read_SkipsInactiveEntries()
	{
		var file = this.Open(
			Compressed("kept.txt", Content(100, 3)),
			new ArcSpec("deleted.txt", 3, [], [], Inactive: true));

		this._provider.Read(file).Should().BeTrue();

		file.DirectoryEntries.Keys.Select(k => k.Raw).Should().Equal("kept.txt");
	}

	[Fact]
	public void GetData_UnknownEntry_ReturnsNull()
	{
		var file = this.Open(Stored("a.txt", Content(10, 4)));
		var prefix = Path.GetFileNameWithoutExtension(file.FileName);

		this._provider.GetData(file, RecordId.Create($@"{prefix}\missing.txt")).Should().BeNull();
	}

	[Fact]
	public void Read_EmptyFile_ReportsNoEntries()
	{
		// XPack4\Underground.arc ships empty; it must not break loading.
		var path = WriteTemp([], ".arc");
		this._tempFiles.Add(path);

		this._provider.Read(new ArcFile(path)).Should().BeFalse();
	}

	[Fact]
	public void ExtractArcFile_WritesEveryEntryUnderItsPath()
	{
		var a = Content(50, 5);
		var b = Content(70_000, 6);
		var file = this.Open(Stored("root.txt", a), Compressed("sub/dir/file.bin", b, partSize: 16_384));
		var destination = Path.Combine(Path.GetTempPath(), $"arc-extract-{Guid.NewGuid():N}");
		this._tempFiles.Add(destination);

		this._provider.ExtractArcFile(file, destination).Should().BeTrue();

		File.ReadAllBytes(Path.Combine(destination, "root.txt")).Should().Equal(a);
		File.ReadAllBytes(Path.Combine(destination, "sub", "dir", "file.bin")).Should().Equal(b);
	}
}
