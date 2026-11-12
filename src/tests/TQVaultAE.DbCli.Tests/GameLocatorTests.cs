using AwesomeAssertions;
using TQVaultAE.DbCli.Core;

namespace TQVaultAE.Tests.DbCli;

public sealed class GameLocatorTests : IDisposable
{
	private readonly string _root = Path.Combine(Path.GetTempPath(), $"tqdb-game-{Guid.NewGuid():N}");

	public void Dispose()
	{
		if (Directory.Exists(this._root))
			Directory.Delete(this._root, recursive: true);
	}

	[Fact]
	public void ParseLibraryFolders_ReadsEveryPathAndUnescapesBackslashes()
	{
		var vdf = """
			"libraryfolders"
			{
				"0"
				{
					"path"		"C:\\Program Files (x86)\\Steam"
					"label"		""
					"apps" { "228980" "1" }
				}
				"1"
				{
					"path"		"E:\\GAMES\\Steam"
					"apps" { "475150" "123" }
				}
			}
			""";

		InstallLocations.ParseLibraryFolders(vdf).Should().Equal("""C:\Program Files (x86)\Steam""", """E:\GAMES\Steam""");
	}

	[Fact]
	public void ParseLibraryFolders_OldFormat_ReadsTheNumberedEntries()
	{
		var vdf = """
			"LibraryFolders"
			{
				"TimeNextStatsReport"		"1561832478"
				"ContentStatsID"		"-158337411110787451"
				"1"		"D:\\games\\Steam"
				"2"		"E:\\SteamLibrary"
			}
			""";

		InstallLocations.ParseLibraryFolders(vdf).Should().Equal("""D:\games\Steam""", """E:\SteamLibrary""");
	}

	[Fact]
	public void Locate_ExplicitRoot_ListsDatabaseAndArchivesRecursively()
	{
		Directory.CreateDirectory(Path.Combine(this._root, "Database"));
		Directory.CreateDirectory(Path.Combine(this._root, "Resources", "XPack"));
		Directory.CreateDirectory(Path.Combine(this._root, "Text"));
		File.WriteAllBytes(Path.Combine(this._root, "Database", "database.arz"), [0]);
		File.WriteAllBytes(Path.Combine(this._root, "Resources", "Items.arc"), [0]);
		File.WriteAllBytes(Path.Combine(this._root, "Resources", "XPack", "Items.arc"), [0]);
		File.WriteAllBytes(Path.Combine(this._root, "Text", "Text_EN.arc"), [0]);
		File.WriteAllBytes(Path.Combine(this._root, "Text", "notes.txt"), [0]);

		var game = GameLocator.Locate(this._root);

		game.Database.Should().Be(Path.Combine(this._root, "Database", "database.arz"));
		game.Archives.Should().HaveCount(3).And.OnlyContain(a => a.EndsWith(".arc"));
		game.TextArchive("en").Should().Be(Path.Combine(this._root, "Text", "Text_EN.arc"));
		game.TextArchive("FR").Should().BeNull();
	}

	[Fact]
	public void Locate_ExplicitRootWithoutDatabase_Throws()
	{
		Directory.CreateDirectory(this._root);

		var act = () => GameLocator.Locate(this._root);

		act.Should().Throw<DirectoryNotFoundException>();
	}

	private const string SteamUserKey = """Software\Valve\Steam""";
	private const string Steam64Key = """SOFTWARE\WOW6432Node\Valve\Steam""";
	private const string Steam32Key = """SOFTWARE\Valve\Steam""";
	private const string GogKey = """SOFTWARE\GOG.com\Games\1196955511""";
	private const string GogWow64Key = """SOFTWARE\WOW6432Node\GOG.com\Games\1196955511""";

	/// <summary>Fake registry: (root, key, value name) → value; reads are recorded.</summary>
	private sealed class FakeRegistry(params (InstallLocations.RegistryRoot Root, string Key, string Name, string Value)[] values)
	{
		public List<string> Reads { get; } = [];

		public string? Read(InstallLocations.RegistryRoot root, string subKey, string name)
		{
			this.Reads.Add($"{root}:{subKey}:{name}");
			return values.FirstOrDefault(v => v.Root == root && v.Key == subKey && v.Name == name).Value;
		}
	}

	private static string SteamGame(string library) => Path.Combine(library, "steamapps", "common", "Titan Quest Anniversary Edition");

	[Fact]
	public void SteamCandidates_NoSteamInRegistry_IsEmpty_AndReadsNoFile()
	{
		var registry = new FakeRegistry();
		var files = new List<string>();

		InstallLocations.SteamCandidates(registry.Read, p => { files.Add(p); return null; }).Should().BeEmpty();

		registry.Reads.Should().Equal(
			$"CurrentUser:{SteamUserKey}:SteamPath",
			$"LocalMachine:{Steam64Key}:InstallPath",
			$"LocalMachine:{Steam32Key}:InstallPath");
		files.Should().BeEmpty();
	}

	[Fact]
	public void SteamCandidates_UserSteamPath_WithForwardSlashes_GivesTheSteamFolderThenEveryLibrary()
	{
		var registry = new FakeRegistry((InstallLocations.RegistryRoot.CurrentUser, SteamUserKey, "SteamPath", "c:/program files (x86)/steam"));
		string? vdfPath = null;
		var vdf = """
			"libraryfolders"
			{
				"0" { "path" "C:\\Program Files (x86)\\Steam" }
				"1" { "path" "E:\\GAMES\\Steam" }
			}
			""";

		var candidates = InstallLocations.SteamCandidates(registry.Read, p => { vdfPath = p; return vdf; }).ToList();

		vdfPath.Should().Be("""c:\program files (x86)\steam\steamapps\libraryfolders.vdf""");
		// The library entry naming the Steam folder itself is listed once (case-insensitive).
		candidates.Should().Equal(SteamGame("""c:\program files (x86)\steam"""), SteamGame("""E:\GAMES\Steam"""));
		registry.Reads.Should().ContainSingle("the user's SteamPath wins");
	}

	[Theory]
	[InlineData(Steam64Key)]
	[InlineData(Steam32Key)]
	public void SteamCandidates_MachineInstallPath_UsedWhenNoUserPath_AndMissingVdfKeepsTheSteamFolder(string key)
	{
		var registry = new FakeRegistry((InstallLocations.RegistryRoot.LocalMachine, key, "InstallPath", """D:\Steam"""));

		InstallLocations.SteamCandidates(registry.Read, _ => null).Should().Equal(SteamGame("""D:\Steam"""));
	}

	[Fact]
	public void GogCandidates_ReadsBothRegistryViews_InOrder()
	{
		var registry = new FakeRegistry(
			(InstallLocations.RegistryRoot.LocalMachine, GogKey, "PATH", """C:\GOG Games\TQAE"""),
			(InstallLocations.RegistryRoot.LocalMachine, GogWow64Key, "PATH", """D:\GOG\TQAE"""));

		InstallLocations.GogCandidates(registry.Read).Should().Equal("""C:\GOG Games\TQAE""", """D:\GOG\TQAE""");
	}

	[Fact]
	public void GogCandidates_NotRegistered_IsEmpty()
	{
		var registry = new FakeRegistry((InstallLocations.RegistryRoot.CurrentUser, GogKey, "PATH", "ignored: wrong hive"));

		InstallLocations.GogCandidates(registry.Read).Should().BeEmpty();
		registry.Reads.Should().Equal($"LocalMachine:{GogKey}:PATH", $"LocalMachine:{GogWow64Key}:PATH");
	}

	[Fact]
	public void Candidates_SteamThenGogThenWellKnownFolders()
	{
		var candidates = GameLocator.Candidates(["steam1", "steam2"], ["gog"]).ToList();

		candidates.Take(3).Should().Equal("steam1", "steam2", "gog");
		candidates.Skip(3).Should().NotBeEmpty().And.OnlyContain(c => c.Contains("Titan Quest", StringComparison.Ordinal));
	}

	private string MakeGame(string name)
	{
		var root = Path.Combine(this._root, name);
		Directory.CreateDirectory(Path.Combine(root, "Database"));
		File.WriteAllBytes(Path.Combine(root, "Database", "database.arz"), [0]);
		return root;
	}

	[Fact]
	public void Locate_FirstCandidateWithADatabaseWins()
	{
		var gog = this.MakeGame("gog");
		var steamWithoutGame = Path.Combine(this._root, "steam");

		GameLocator.Locate(null, null, [steamWithoutGame, gog, this.MakeGame("other")]).Root.Should().Be(gog);
	}

	[Fact]
	public void Locate_EnvironmentValueComesBeforeTheCandidates()
	{
		var env = this.MakeGame("env");

		GameLocator.Locate(null, $"\"{env}\"", [this.MakeGame("steam")]).Root.Should().Be(env);
	}

	[Fact]
	public void Locate_InvalidEnvironmentValue_ThrowsWithoutTryingTheCandidates()
	{
		var act = () => GameLocator.Locate(null, Path.Combine(this._root, "missing"), [this.MakeGame("steam")]);

		act.Should().Throw<DirectoryNotFoundException>().WithMessage("*TQ_GAME_PATH*");
	}

	[Fact]
	public void Locate_NoCandidateWithADatabase_Throws()
	{
		var act = () => GameLocator.Locate(null, " ", [Path.Combine(this._root, "none")]);

		act.Should().Throw<DirectoryNotFoundException>().WithMessage("*not found*");
	}
}
