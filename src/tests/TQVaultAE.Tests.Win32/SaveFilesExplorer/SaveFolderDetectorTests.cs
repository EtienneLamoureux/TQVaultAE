using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using TQ.SaveFilesExplorer.Services;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Config;
using TQVaultAE.Services;
using TQVaultAE.Services.Win32;

namespace TQVaultAE.Tests.Win32.SaveFilesExplorer;

/// <summary>
/// <see cref="SaveFolderDetector"/> finds the save folders TQ Save Files Explorer lists through TQVault's own
/// <see cref="GamePathServiceWin"/>. The file system is mocked through <see cref="IDirectoryIO"/>: nothing is read from
/// the user's saves.
/// </summary>
public class SaveFolderDetectorTests
{
	private static readonly string MyGames = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "My Games");
	private static readonly string SaveDataTQIT = Path.Combine(MyGames, "Titan Quest - Immortal Throne", "SaveData");
	private static readonly string MainTQIT = Path.Combine(SaveDataTQIT, "Main");
	private static readonly string MainTQ = Path.Combine(MyGames, "Titan Quest", "SaveData", "Main");
	private static readonly string UserTQIT = Path.Combine(SaveDataTQIT, "User");
	private static readonly string Sys = Path.Combine(SaveDataTQIT, "Sys");

	private readonly Mock<IDirectoryIO> _directoryIO = new();
	private readonly SaveFolderDetector _detector;

	public SaveFolderDetectorTests()
	{
		var gamePath = new GamePathServiceWin(
			new Mock<ILogger<GamePathServiceWin>>().Object
			, new Mock<ITQDataService>().Object
			, new Mock<IFileIO>().Object
			, new PathIO()
			, this._directoryIO.Object
			, new UserSettings());
		this._detector = new SaveFolderDetector(gamePath, this._directoryIO.Object);
	}

	/// <summary>Makes <paramref name="parent"/> exist with <paramref name="children"/>, filtered like the file system does.</summary>
	private void AddDirectories(string parent, params string[] children)
	{
		this._directoryIO.Setup(d => d.Exists(parent)).Returns(true);
		this._directoryIO.Setup(d => d.GetDirectories(parent))
			.Returns([.. children.Select(c => Path.Combine(parent, c))]);
		this._directoryIO.Setup(d => d.GetDirectories(parent, "_*"))
			.Returns([.. children.Where(c => c.StartsWith('_')).Select(c => Path.Combine(parent, c))]);
	}

	private static DetectedSaveFolder Folder(string label, string parent, string child = "")
		=> new(label, child.Length == 0 ? parent : Path.Combine(parent, child));

	[Fact]
	public void Detect_ListsTheCharactersAndStashesOfTheGameSaveFolders()
	{
		this.AddDirectories(MainTQIT, "_HeroA", "_HeroB", "ArchivedCharacters");
		this.AddDirectories(MainTQ, "_HeroC");
		this.AddDirectories(Sys, "ModA");
		this.AddDirectories(UserTQIT, "_HeroD");

		var groups = this._detector.Detect();

		groups.Should().HaveCount(4);
		groups[0].Should().Equal(
			Folder("TQIT : _HeroA", MainTQIT, "_HeroA"),
			Folder("TQIT : _HeroB", MainTQIT, "_HeroB"),
			Folder("TQ : _HeroC", MainTQ, "_HeroC"));
		groups[1].Should().Equal(Folder("TQIT : Transfer Stash", Sys));
		groups[2].Should().Equal(Folder("ModA : Transfer Stash", Sys, "ModA"));
		groups[3].Should().Equal(Folder("Mod : _HeroD", UserTQIT, "_HeroD"));
	}

	[Fact]
	public void Detect_NoSaveFolder_GivesEmptyGroupsAndNoDefaultFolder()
	{
		var groups = this._detector.Detect();

		groups.Should().HaveCount(4).And.AllSatisfy(g => g.Should().BeEmpty());
		this._detector.DefaultSaveDirectory.Should().BeNull();
		this._directoryIO.Verify(d => d.GetDirectories(It.IsAny<string>()), Times.Never);
		this._directoryIO.Verify(d => d.GetDirectories(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
	}

	[Fact]
	public void DefaultSaveDirectory_IsImmortalThroneFirst()
	{
		this.AddDirectories(MainTQ);
		this._detector.DefaultSaveDirectory.Should().Be(MainTQ);

		this.AddDirectories(MainTQIT);
		this._detector.DefaultSaveDirectory.Should().Be(MainTQIT);
	}

	[Fact]
	public void Services_ResolveTheMainFormDependencies()
	{
		using var services = TQ.SaveFilesExplorer.Program.ConfigureServices()
			.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });

		services.GetRequiredService<SaveFolderDetector>().Should().NotBeNull();
	}
}
