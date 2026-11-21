using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;

namespace TQVaultAE.Tests.DbCli;

/// <summary>Builds synthetic .arz / .arc / .tex files in memory for format tests.</summary>
internal static class DbCliFixtures
{
	static DbCliFixtures() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

	/// <summary>Windows-1252, the encoding of every string in the game files.</summary>
	private static Encoding Cp1252 => Encoding.GetEncoding(1252);

	public static byte[] Zlib(ReadOnlySpan<byte> data)
	{
		using var ms = new MemoryStream();
		using (var z = new ZLibStream(ms, CompressionLevel.Optimal, leaveOpen: true))
			z.Write(data);
		return ms.ToArray();
	}

	public static string WriteTemp(byte[] bytes, string extension)
	{
		var path = Path.Combine(Path.GetTempPath(), $"tqdb-test-{Guid.NewGuid():N}{extension}");
		File.WriteAllBytes(path, bytes);
		return path;
	}

	#region ARZ

	public sealed record ArzVar(short Type, string Name, int[] Raw);

	public sealed record ArzRec(string Id, string Class, long FileTime, params ArzVar[] Variables);

	/// <summary>Builds a database.arz: header, zlib record blobs, record table, string table, 16-byte trailer.</summary>
	public static byte[] BuildArz(IReadOnlyList<ArzRec> records, IList<string> strings)
	{
		int Intern(string s)
		{
			int i = strings.IndexOf(s);
			if (i >= 0)
				return i;
			strings.Add(s);
			return strings.Count - 1;
		}

		var blobs = new List<byte[]>();
		var nameIds = new List<int>();
		foreach (var r in records) {
			nameIds.Add(Intern(r.Id));
			using var raw = new MemoryStream();
			var w = new BinaryWriter(raw);
			foreach (var v in r.Variables) {
				w.Write(v.Type);
				w.Write((short)v.Raw.Length);
				w.Write(Intern(v.Name));
				foreach (var x in v.Raw)
					w.Write(x);
			}
			w.Flush();
			blobs.Add(Zlib(raw.ToArray()));
		}

		using var file = new MemoryStream();
		var fw = new BinaryWriter(file);
		fw.Write(new byte[24]);
		var offsets = new List<int>();
		foreach (var b in blobs) {
			offsets.Add((int)file.Position - 24);
			fw.Write(b);
		}

		int recStart = (int)file.Position;
		for (int i = 0; i < records.Count; i++) {
			fw.Write(nameIds[i]);
			var cls = Cp1252.GetBytes(records[i].Class);
			fw.Write(cls.Length);
			fw.Write(cls);
			fw.Write(offsets[i]);
			fw.Write(blobs[i].Length);
			fw.Write(records[i].FileTime);
		}
		int recSize = (int)file.Position - recStart;

		int strStart = (int)file.Position;
		fw.Write(strings.Count);
		foreach (var s in strings) {
			var b = Cp1252.GetBytes(s);
			fw.Write(b.Length);
			fw.Write(b);
		}
		int strSize = (int)file.Position - strStart;
		fw.Write(new byte[16]);
		fw.Flush();

		var bytes = file.ToArray();
		var h = bytes.AsSpan();
		BinaryPrimitives.WriteInt32LittleEndian(h, 0x00030004);
		BinaryPrimitives.WriteInt32LittleEndian(h[4..], recStart);
		BinaryPrimitives.WriteInt32LittleEndian(h[8..], recSize);
		BinaryPrimitives.WriteInt32LittleEndian(h[12..], records.Count);
		BinaryPrimitives.WriteInt32LittleEndian(h[16..], strStart);
		BinaryPrimitives.WriteInt32LittleEndian(h[20..], strSize);
		return bytes;
	}

	#endregion

	#region ARC

	/// <summary>One ARC file-table entry. <paramref name="Parts"/> are the stored part blobs with their uncompressed sizes.</summary>
	public sealed record ArcSpec(string Name, int StorageType, byte[] Content, IReadOnlyList<(byte[] Blob, int Size)> Parts, uint? Adler = null, bool Inactive = false);

	/// <summary>A normal type-3 entry split into zlib parts of at most <paramref name="partSize"/> bytes.</summary>
	public static ArcSpec Compressed(string name, byte[] content, int partSize = 262_144)
	{
		var parts = new List<(byte[], int)>();
		for (int p = 0; p < content.Length; p += partSize) {
			int n = Math.Min(partSize, content.Length - p);
			parts.Add((Zlib(content.AsSpan(p, n)), n));
		}
		return new ArcSpec(name, 3, content, parts);
	}

	public static ArcSpec Stored(string name, byte[] content) => new(name, 1, content, [(content, content.Length)]);

	/// <summary>
	/// Builds an .arc. <paramref name="namesOrder"/> gives the order of names in the names blob
	/// (indices into <paramref name="entries"/>); default is table order.
	/// </summary>
	public static byte[] BuildArc(IReadOnlyList<ArcSpec> entries, IReadOnlyList<int>? namesOrder = null)
	{
		var enc = Encoding.GetEncoding(1252);
		using var file = new MemoryStream();
		var w = new BinaryWriter(file);
		w.Write(new byte[28]);

		var partTable = new List<(uint Offset, int CSize, int USize)>();
		var entryOffsets = new long[entries.Count];
		var firstParts = new int[entries.Count];
		for (int i = 0; i < entries.Count; i++) {
			var e = entries[i];
			entryOffsets[i] = file.Position;
			firstParts[i] = partTable.Count;
			if (e.Inactive)
				continue;
			foreach (var (blob, size) in e.Parts) {
				if (e.StorageType == 3)
					partTable.Add(((uint)file.Position, blob.Length, size));
				w.Write(blob);
			}
		}

		long toc = file.Position;
		foreach (var p in partTable) {
			w.Write(p.Offset);
			w.Write(p.CSize);
			w.Write(p.USize);
		}

		var order = namesOrder ?? Enumerable.Range(0, entries.Count).ToArray();
		var nameOffsets = new int[entries.Count];
		var nameLengths = new int[entries.Count];
		using var names = new MemoryStream();
		foreach (int i in order) {
			var b = enc.GetBytes(entries[i].Name);
			nameOffsets[i] = (int)names.Length;
			nameLengths[i] = b.Length;
			names.Write(b);
			names.WriteByte(0);
		}
		w.Write(names.ToArray());

		for (int i = 0; i < entries.Count; i++) {
			var e = entries[i];
			int csize = e.Inactive ? 0 : e.Parts.Sum(p => p.Blob.Length);
			int nParts = e.Inactive || e.StorageType != 3 ? 0 : e.Parts.Count;
			w.Write(e.StorageType);
			w.Write((uint)entryOffsets[i]);
			w.Write(csize);
			w.Write(e.Inactive ? 0 : e.Content.Length);
			w.Write(e.Adler ?? Adler(e.Content));
			w.Write(0x01D0_0000_0000_0000L);
			w.Write(nParts);
			w.Write(firstParts[i]);
			w.Write(nameLengths[i]);
			w.Write(nameOffsets[i]);
		}
		w.Flush();

		var bytes = file.ToArray();
		var h = bytes.AsSpan();
		h[0] = (byte)'A';
		h[1] = (byte)'R';
		h[2] = (byte)'C';
		BinaryPrimitives.WriteInt32LittleEndian(h[4..], 1);
		BinaryPrimitives.WriteInt32LittleEndian(h[8..], entries.Count);
		BinaryPrimitives.WriteInt32LittleEndian(h[12..], partTable.Count);
		BinaryPrimitives.WriteInt32LittleEndian(h[16..], partTable.Count * 12);
		BinaryPrimitives.WriteInt32LittleEndian(h[20..], (int)names.Length);
		BinaryPrimitives.WriteUInt32LittleEndian(h[24..], (uint)toc);
		return bytes;
	}

	/// <summary>Reference Adler-32 (plain RFC 1950 loop) to check the production one against.</summary>
	public static uint Adler(ReadOnlySpan<byte> data)
	{
		uint a = 1, b = 0;
		foreach (var x in data) {
			a = (a + x) % 65521;
			b = (b + a) % 65521;
		}
		return (b << 16) | a;
	}

	#endregion

	#region TEX / DDS

	/// <summary>An uncompressed DDS as the game stores it: "DDSR" magic, zero masks, no ALPHAPIXELS, no DDSCAPS_TEXTURE.</summary>
	public static byte[] GameDds(int width, int height, int bitCount, ReadOnlySpan<byte> pixels, string? fourCC = null)
	{
		var dds = new byte[128 + pixels.Length];
		var s = dds.AsSpan();
		"DDSR"u8.CopyTo(s);
		BinaryPrimitives.WriteInt32LittleEndian(s[4..], 124);
		BinaryPrimitives.WriteInt32LittleEndian(s[8..], 0x1 | 0x2 | 0x4 | 0x1000);
		BinaryPrimitives.WriteInt32LittleEndian(s[12..], height);
		BinaryPrimitives.WriteInt32LittleEndian(s[16..], width);
		BinaryPrimitives.WriteInt32LittleEndian(s[20..], width * bitCount / 8);
		BinaryPrimitives.WriteInt32LittleEndian(s[76..], 32);
		if (fourCC is not null) {
			BinaryPrimitives.WriteInt32LittleEndian(s[80..], 0x4);
			Encoding.ASCII.GetBytes(fourCC).CopyTo(s[84..]);
		} else {
			BinaryPrimitives.WriteInt32LittleEndian(s[80..], 0x40);
			BinaryPrimitives.WriteInt32LittleEndian(s[88..], bitCount);
		}
		pixels.CopyTo(s[128..]);
		return dds;
	}

	public static byte[] Tex(int version, params byte[][] frames)
	{
		using var ms = new MemoryStream();
		var w = new BinaryWriter(ms);
		w.Write("TEX"u8);
		w.Write((byte)version);
		w.Write(30);
		if (version == 2)
			w.Write((byte)0);
		foreach (var f in frames) {
			w.Write(f.Length);
			w.Write(f);
		}
		w.Flush();
		return ms.ToArray();
	}

	#endregion
}
