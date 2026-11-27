using System.Text;
using AwesomeAssertions;
using TQVaultAE.DbCli.Core;
using static TQVaultAE.Tests.DbCli.DbCliFixtures;

namespace TQVaultAE.Tests.DbCli;

public sealed class ArcArchiveTests : IDisposable
{
	private readonly List<string> _files = [];

	public void Dispose()
	{
		foreach (var f in this._files)
			File.Delete(f);
	}

	private IArcArchive Open(IReadOnlyList<ArcSpec> entries, IReadOnlyList<int>? namesOrder = null)
	{
		var path = WriteTemp(BuildArc(entries, namesOrder), ".arc");
		this._files.Add(path);
		return ArcArchiveFactory.Open(path);
	}

	private static byte[] Bytes(int length, int seed)
	{
		var b = new byte[length];
		new Random(seed).NextBytes(b);
		return b;
	}

	[Fact]
	public void Open_UsesNameOffsets_AndSkipsInactiveEntries()
	{
		// Names blob order differs from table order and includes an inactive entry:
		// a sequential name reader would label "b" as "a" and so on.
		var a = Encoding.ASCII.GetBytes("alpha");
		var b = Encoding.ASCII.GetBytes("bravo!");
		var entries = new[] {
			Compressed("dir/b.txt", b),
			new ArcSpec("dir/dead.txt", 3, [], [], Inactive: true),
			Compressed("dir/a.txt", a),
		};
		using var arc = this.Open(entries, namesOrder: [2, 1, 0]);

		arc.Entries.Select(e => e.Name).Should().Equal("dir/b.txt", "dir/a.txt");
		arc.ReadAll(arc.Find("dir/a.txt")!).Should().Equal(a);
		arc.ReadAll(arc.Find("dir/b.txt")!).Should().Equal(b);
		arc.Find("dir/dead.txt").Should().BeNull();
	}

	[Fact]
	public void Names_AreDecodedAsCp1252()
	{
		using var arc = this.Open([Compressed("ui/god’s.tex", [1, 2, 3])]);

		arc.Entries.Single().Name.Should().Be("ui/god’s.tex");
	}

	[Theory]
	[InlineData("Creatures/Monster/x.MSH")]
	[InlineData("""creatures\monster\x.msh""")]
	public void Find_IsCaseInsensitiveAndAcceptsBackslash(string name)
	{
		using var arc = this.Open([Compressed("creatures/monster/x.msh", [9])]);

		arc.Find(name).Should().BeSameAs(arc.Entries[0]);
	}

	[Fact]
	public void ResourceId_UsesArchiveNameAndBackslashes()
	{
		using var arc = this.Open([Compressed("weapons/x.tex", [9])]);

		arc.Entries[0].ResourceId.Should().Be($"""{Path.GetFileNameWithoutExtension(arc.Path)}\weapons\x.tex""");
	}

	[Fact]
	public void ResourceId_OfAnExpansionArchive_StartsWithTheExpansionFolder()
	{
		// The database references Resources\XPack2\Items.arc's entries as XPack2\Items\...
		var root = Path.Combine(Path.GetTempPath(), $"tqdb-layout-{Guid.NewGuid():N}");
		var path = Path.Combine(root, "Resources", "XPack2", "Items.arc");
		Directory.CreateDirectory(Path.GetDirectoryName(path)!);
		File.WriteAllBytes(path, BuildArc([Compressed("weapons/x.tex", [9])]));
		try
		{
			using var arc = ArcArchiveFactory.Open(path);

			arc.ResourcePrefix.Should().Be("""XPack2\Items""");
			arc.Entries[0].ResourceId.Should().Be("""XPack2\Items\weapons\x.tex""");
		}
		finally
		{
			Directory.Delete(root, recursive: true);
		}
	}

	[Fact]
	public void Type3PartWithEqualSizes_IsStillInflated()
	{
		// A compressible part padded so csize == usize: treating it as stored would return zlib bytes.
		var content = Enumerable.Repeat((byte)'A', 1000).ToArray();
		var blob = Zlib(content);
		Array.Resize(ref blob, content.Length);
		var spec = new ArcSpec("x.bin", 3, content, [(blob, content.Length)]);
		using var arc = this.Open([spec]);

		var entry = arc.Entries.Single();
		entry.CompressedSize.Should().Be(entry.Size);
		arc.ReadAll(entry).Should().Equal(content);
	}

	[Fact]
	public void StoredEntry_IsReturnedRaw()
	{
		var content = Bytes(5000, 1);
		using var arc = this.Open([Stored("raw.bin", content)]);

		using var ms = new MemoryStream();
		arc.CopyTo(arc.Entries[0], ms, verify: true);
		ms.ToArray().Should().Equal(content);
		arc.ReadAll(arc.Entries[0]).Should().Equal(content);
	}

	[Fact]
	public void CopyTo_StreamsPartByPart_AndConcatenatesParts()
	{
		var content = Bytes(10_000, 2);
		using var arc = this.Open([Compressed("big.bin", content, partSize: 3000)]);
		var sink = new RecordingStream();

		arc.CopyTo(arc.Entries[0], sink, verify: true);

		sink.ToArray().Should().Equal(content);
		sink.LargestWrite.Should().BeLessThanOrEqualTo(3000);
	}

	[Fact]
	public void CopyTo_Verify_DetectsAdlerMismatch()
	{
		var content = Bytes(100, 3);
		var spec = Compressed("bad.bin", content) with { Adler = Adler(content) ^ 1 };
		using var arc = this.Open([spec]);

		var verified = () => arc.CopyTo(arc.Entries[0], Stream.Null, verify: true);
		var unverified = () => arc.CopyTo(arc.Entries[0], Stream.Null, verify: false);

		verified.Should().Throw<InvalidDataException>().WithMessage("*Adler-32*");
		unverified.Should().NotThrow();
	}

	[Fact]
	public void EmptyArchive_HasNoEntries()
	{
		using var arc = this.Open([]);

		arc.Entries.Should().BeEmpty();
	}

	[Fact]
	public void EntryOfAnotherArchive_IsRejected()
	{
		using var first = this.Open([Compressed("a.bin", [1])]);
		using var second = this.Open([Compressed("a.bin", [1])]);

		var act = () => second.ReadAll(first.Entries[0]);

		act.Should().Throw<ArgumentException>();
	}

	[Theory]
	[InlineData(0)]
	[InlineData(1)]
	[InlineData(5551)]
	[InlineData(5552)]
	[InlineData(100_000)]
	public void Adler32_MatchesReferenceAcrossBlockBoundaries(int length)
	{
		var data = Enumerable.Range(0, length).Select(i => (byte)(255 - (i % 7))).ToArray();
		var incremental = new Adler32();
		incremental.Append(data.AsSpan(0, length / 3));
		incremental.Append(data.AsSpan(length / 3));

		Adler32.Compute(data).Should().Be(Adler(data));
		incremental.Value.Should().Be(Adler(data));
	}

	private sealed class RecordingStream : MemoryStream
	{
		public int LargestWrite { get; private set; }

		public override void Write(ReadOnlySpan<byte> buffer)
		{
			this.LargestWrite = Math.Max(this.LargestWrite, buffer.Length);
			base.Write(buffer);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			this.LargestWrite = Math.Max(this.LargestWrite, count);
			base.Write(buffer, offset, count);
		}
	}
}
