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
}
