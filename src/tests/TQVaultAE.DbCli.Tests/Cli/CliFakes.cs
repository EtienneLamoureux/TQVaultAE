using TQVaultAE.DbCli.Core;
using TQVaultAE.DbCli.Query;

namespace TQVaultAE.Tests.DbCli.Cli;

/// <summary>In-memory database for CLI-layer tests; counts decoded records to check pre-decompression filtering.</summary>
internal sealed class FakeArzDatabase : IArzDatabase
{
	private readonly List<ArzRecord> _records = [];
	private int _reads;

	public string Path => "fake.arz";

	public IReadOnlyList<ArzRecordInfo> Records => this._records.ConvertAll(r => r.Info);

	/// <summary>Number of records decoded through <see cref="Read"/> or <see cref="ReadMany"/>.</summary>
	public int Reads => this._reads;

	public FakeArzDatabase Add(string id, string cls, params ArzVariable[] variables) {
		var info = new ArzRecordInfo(this._records.Count, id, cls, 0, 0, new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc));
		var all = new List<ArzVariable> { new("Class", ArzValueType.String, [cls]) };
		all.AddRange(variables);
		this._records.Add(new ArzRecord(info, all));
		return this;
	}

	public ArzRecordInfo? Find(string id) {
		foreach (var r in this._records)
			if (Values.PathInsensitiveEquals(r.Info.Id, id))
				return r.Info;
		return null;
	}

	public ArzRecord Read(ArzRecordInfo info) {
		Interlocked.Increment(ref this._reads);
		return this._records[info.Index];
	}

	public IEnumerable<ArzRecord> ReadMany(IEnumerable<ArzRecordInfo> infos, Func<ArzRecord, bool>? predicate = null, bool ordered = false, int maxDegreeOfParallelism = -1, CancellationToken cancellationToken = default) {
		foreach (var info in infos) {
			var r = this.Read(info);
			if (predicate is null || predicate(r))
				yield return r;
		}
	}

	public void Dispose() {
	}
}

/// <summary>Namer reading the name tag from <c>itemNameTag</c> and resolving through a dictionary.</summary>
internal sealed class FakeNamer(Dictionary<string, string> texts) : IRecordNamer
{
	private readonly Dictionary<string, string> _texts = new(texts, StringComparer.OrdinalIgnoreCase);

	public string? NameTag(ArzRecord record) => record.Get("itemNameTag")?.Values[0] as string;

	public string? DisplayName(ArzRecord record) => this.ResolveTag(this.NameTag(record));

	public string? ResolveTag(string? tag) => tag is not null && this._texts.TryGetValue(tag, out var t) ? t : null;
}

/// <summary>Variable builders.</summary>
internal static class V
{
	public static ArzVariable Int(string name, params int[] values) => new(name, ArzValueType.Int, values.Cast<object>().ToArray());

	public static ArzVariable Float(string name, params float[] values) => new(name, ArzValueType.Float, values.Cast<object>().ToArray());

	public static ArzVariable Str(string name, params string[] values) => new(name, ArzValueType.String, values.Cast<object>().ToArray());

	public static ArzVariable Bool(string name, params bool[] values) => new(name, ArzValueType.Bool, values.Cast<object>().ToArray());

	public static ArzRecord Record(string id, params ArzVariable[] variables) =>
		new(new ArzRecordInfo(0, id, "Test", 0, 0, DateTime.UnixEpoch), variables);
}
