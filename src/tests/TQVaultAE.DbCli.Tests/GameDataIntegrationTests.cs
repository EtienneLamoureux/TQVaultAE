using System.Diagnostics;
using AwesomeAssertions;
using TQVaultAE.DbCli.Core;

namespace TQVaultAE.Tests.DbCli;

/// <summary>Checks against the real game files; skipped when the game is not installed.</summary>
public sealed class GameDataIntegrationTests(ITestOutputHelper output)
{
	private const string Skip = "Requires Titan Quest game installation";

	private static GameInstall Game => GameLocator.Locate(TestingConditions.TitanQuestPath);

	[Fact(Skip = Skip, SkipType = typeof(TestingConditions), SkipUnless = nameof(TestingConditions.IsGameInstalled))]
	public void Database_HasExpectedRecordCount_AndDecodesKnownSword()
	{
		using var db = ArzDatabaseFactory.Open(Game.Database);

		db.Records.Should().HaveCount(74_013);
		var info = db.Find("records/item/equipmentweapon/sword/c10_sword01.dbr");
		info.Should().NotBeNull();
		var rec = db.Read(info!);
		rec.Info.Class.Should().Be("WeaponMelee_Sword");
		rec.Get("Class")!.Values.Should().Equal("WeaponMelee_Sword");
		DisplayNames.NameTag(rec).Should().Be("tagItemHookedBlade");

		var text = TextDatabaseFactory.Load(Game.TextArchive("EN")!, "EN");
		DisplayNames.Resolve(rec, text).Should().Be("Hooked Blade");
	}

	[Fact(Skip = Skip, SkipType = typeof(TestingConditions), SkipUnless = nameof(TestingConditions.IsGameInstalled))]
	public void TextEn_LoadsCp1252FilesToo()
	{
		using var arc = ArcArchiveFactory.Open(Game.TextArchive("EN")!);
		var text = TextDatabaseFactory.Load(arc.Path, "EN");

		arc.Entries.Count(e => e.Name.EndsWith(".txt", StringComparison.Ordinal)).Should().Be(57);
		text.Entries.Values.Should().NotContain(v => v.Contains('�'));
		text.Entries.Count.Should().BeGreaterThan(10_000);
	}

	[Fact(Skip = Skip, SkipType = typeof(TestingConditions), SkipUnless = nameof(TestingConditions.IsGameInstalled))]
	public void TutorialsArc_EveryEntryVerifies()
	{
		var path = Game.Archives.Single(a => a.EndsWith("""Toolset\Tutorials.arc""", StringComparison.OrdinalIgnoreCase));
		using var arc = ArcArchiveFactory.Open(path);

		arc.Entries.Should().NotBeEmpty();
		foreach (var e in arc.Entries)
			arc.CopyTo(e, Stream.Null, verify: true);
	}

	[Fact(Skip = Skip, SkipType = typeof(TestingConditions), SkipUnless = nameof(TestingConditions.IsGameInstalled))]
	public void ExpansionResources_ReferencedByTheDatabase_HaveTheirArchiveEntry()
	{
		// Before the expansion folder was part of the resource ids, none of the XPackN\... references matched an entry.
		// The few left are dead references of the game data (removed models, test files).
		var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (var path in Game.Archives.Where(a => a.Contains($"{Path.DirectorySeparatorChar}Resources{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)))
		{
			using var arc = ArcArchiveFactory.Open(path);
			foreach (var e in arc.Entries)
				ids.Add(e.ResourceId);
		}
		var extensions = new HashSet<string>([".tex", ".msh", ".mp3", ".wav"], StringComparer.OrdinalIgnoreCase);
		using var db = ArzDatabaseFactory.Open(Game.Database);
		var references = db.ReadMany(db.Records, cancellationToken: TestContext.Current.CancellationToken)
			.SelectMany(r => r.Variables)
			.Where(v => v.Type == ArzValueType.String)
			.SelectMany(v => v.Values.Cast<string>())
			.Select(s => s.Replace('/', '\\'))
			.Where(s => s.StartsWith("XPack", StringComparison.OrdinalIgnoreCase) && s.Contains('\\') && extensions.Contains(Path.GetExtension(s)))
			.ToHashSet(StringComparer.OrdinalIgnoreCase);

		ids.Should().Contain("""XPack2\Items\equipmentweapon\axe\uibitmaps\m_minerpick.tex""");
		foreach (var expansion in references.GroupBy(s => s[..s.IndexOf('\\', StringComparison.Ordinal)], StringComparer.OrdinalIgnoreCase))
		{
			int found = expansion.Count(ids.Contains);
			output.WriteLine($"{expansion.Key}: {found}/{expansion.Count()}");
			found.Should().BeGreaterThan(expansion.Count() * 98 / 100, expansion.Key);
		}
	}

	[Fact(Skip = Skip, SkipType = typeof(TestingConditions), SkipUnless = nameof(TestingConditions.IsGameInstalled))]
	public void Benchmark_PrintsTimings()
	{
		var sw = Stopwatch.StartNew();
		using var db = ArzDatabaseFactory.Open(Game.Database);
		output.WriteLine($"arz open+index: {sw.ElapsedMilliseconds} ms ({db.Records.Count} records)");

		sw.Restart();
		long variables = db.ReadMany(db.Records, cancellationToken: TestContext.Current.CancellationToken).Sum(r => (long)r.Variables.Count);
		output.WriteLine($"ReadMany all records: {sw.ElapsedMilliseconds} ms ({variables} variables)");

		sw.Restart();
		var text = TextDatabaseFactory.Load(Game.TextArchive("EN")!, "EN");
		output.WriteLine($"Text_EN load: {sw.ElapsedMilliseconds} ms ({text.Entries.Count} tags)");

		sw.Restart();
		var largest = Game.Archives.OrderByDescending(a => new FileInfo(a).Length).First();
		using (var arc = ArcArchiveFactory.Open(largest))
			output.WriteLine($"{Path.GetFileName(largest)} listing: {sw.ElapsedMilliseconds} ms ({arc.Entries.Count} entries)");

		variables.Should().BeGreaterThan(0);
	}
}
