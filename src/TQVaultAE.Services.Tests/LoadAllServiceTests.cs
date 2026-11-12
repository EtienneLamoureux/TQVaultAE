using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Results;
using TQVaultAE.Config;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

public class LoadAllServiceTests
{
	/// <summary>
	/// Records the settings saves, with the crash guard flag at each, instead of writing the file.
	/// </summary>
	private sealed class TestLoadAllService(
		UserSettings settings
		, IPlayerService playerService
		, IStashService stashService
		, IVaultService vaultService
	) : LoadAllService(new Mock<ILogger<LoadAllService>>().Object, settings, playerService, stashService, vaultService)
	{
		private readonly UserSettings _settings = settings;

		public List<bool> SavedCompletedFlags { get; } = [];

		protected override void SaveSettings() => this.SavedCompletedFlags.Add(this._settings.LoadAllFilesCompleted);
	}

	private sealed class CountingProgress : IProgress<int>
	{
		private int _total;

		public int Total => this._total;

		public void Report(int value) => Interlocked.Add(ref this._total, value);
	}

	private readonly UserSettings _settings = new() { LoadAllFiles = true, LoadAllFilesCompleted = true };
	private readonly Mock<IPlayerService> _playerService = new();
	private readonly Mock<IStashService> _stashService = new();
	private readonly Mock<IVaultService> _vaultService = new();
	private readonly TestLoadAllService _service;

	public LoadAllServiceTests()
	{
		this._playerService.Setup(p => p.LoadPlayer(It.IsAny<PlayerSave>(), false)).Returns(new PlayerLoadResult());
		this._stashService.Setup(s => s.LoadPlayerStash(It.IsAny<PlayerSave>(), false)).Returns(StashLoadResult.Empty());
		this._vaultService.Setup(v => v.LoadVault(It.IsAny<string>())).Returns(new VaultLoadResult());
		this._service = new TestLoadAllService(this._settings, this._playerService.Object, this._stashService.Object, this._vaultService.Object);
	}

	private static PlayerSave NewCharacter(string name)
	{
		var pathIO = new Mock<IPathIO>();
		pathIO.Setup(p => p.GetFileName(It.IsAny<string>())).Returns<string>(Path.GetFileName);
		return new(Path.Combine(Path.GetTempPath(), "_" + name), false, false, false, string.Empty, new Mock<ITranslationService>().Object, pathIO.Object);
	}

	[Theory]
	[InlineData(0, 0, 0)]
	[InlineData(3, 0, 6)]
	[InlineData(2, 5, 9)]
	public void CountSteps_OnePerCharacterStashAndVault(int characters, int vaults, int expected)
	{
		LoadAllService.CountSteps(characters, vaults).Should().Be(expected);
	}

	[Fact]
	public void LoadAll_ReportsOneStepPerCountedStep()
	{
		var characters = new[] { NewCharacter("A"), NewCharacter("B") };
		var vaults = new[] { "Main Vault", "Rings", "Swords" };
		var progress = new CountingProgress();

		var result = this._service.LoadAll(characters, vaults, progress);

		result.Ran.Should().BeTrue();
		progress.Total.Should().Be(LoadAllService.CountSteps(characters.Length, vaults.Length));
		this._playerService.Verify(p => p.LoadPlayer(It.IsAny<PlayerSave>(), false), Times.Exactly(2));
		this._stashService.Verify(s => s.LoadPlayerStash(It.IsAny<PlayerSave>(), false), Times.Exactly(2));
		this._vaultService.Verify(v => v.LoadVault(It.IsAny<string>()), Times.Exactly(3));
	}

	[Fact]
	public void LoadAll_ReadsTheSharedStashesOfEveryMapGiven()
	{
		// Arrange: the base game and one mod in use
		string[] maps = [string.Empty, @"c:\mods\shadowchampions"];
		this._stashService.Setup(s => s.LoadTransferStash(false, It.IsAny<string?>())).Returns(StashLoadResult.Empty());
		this._stashService.Setup(s => s.LoadRelicVaultStash(false, It.IsAny<string?>())).Returns(StashLoadResult.Empty());
		var progress = new CountingProgress();

		// Act
		this._service.LoadAll([NewCharacter("A")], [], progress, maps);

		// Assert: a transfer stash and a relic vault per map, each counted
		progress.Total.Should().Be(LoadAllService.CountSteps(1, 0, maps.Length));
		foreach (var map in maps)
		{
			this._stashService.Verify(s => s.LoadTransferStash(false, map), Times.Once);
			this._stashService.Verify(s => s.LoadRelicVaultStash(false, map), Times.Once);
		}
	}

	[Fact]
	public void LoadAll_ClearsTheCrashGuardDuringTheLoadAndSetsItAfter()
	{
		this._service.LoadAll([NewCharacter("A")], [], new CountingProgress());

		this._service.SavedCompletedFlags.Should().Equal(false, true);
		this._settings.LoadAllFilesCompleted.Should().BeTrue();
	}

	[Fact]
	public void LoadAll_NothingToLoad_LeavesTheSettingsAlone()
	{
		var result = this._service.LoadAll([], [], new CountingProgress());

		result.Ran.Should().BeFalse();
		this._service.SavedCompletedFlags.Should().BeEmpty();
	}

	[Fact]
	public void LoadAll_WhenALoadThrows_TheCrashGuardStaysCleared()
	{
		this._vaultService.Setup(v => v.LoadVault("Broken")).Throws(new InvalidOperationException("boom"));

		var act = () => this._service.LoadAll([], ["Broken"], new CountingProgress());

		act.Should().Throw<AggregateException>();
		this._service.LastRunFailed.Should().BeTrue();
	}

	[Fact]
	public void LoadAll_CollectsTheReadErrorsOfPlayersStashesAndVaults()
	{
		var player = new PlayerCollection("A", "a.chr") { ArgumentException = new ArgumentException("bad player") };
		this._playerService.Setup(p => p.LoadPlayer(It.IsAny<PlayerSave>(), false))
			.Returns(new PlayerLoadResult { PlayerFile = "a.chr", Player = player });
		var stash = new Stash("A", "a.dxb") { ArgumentException = new ArgumentException("bad stash") };
		this._stashService.Setup(s => s.LoadPlayerStash(It.IsAny<PlayerSave>(), false))
			.Returns(StashLoadResult.Succeeded(stash, "a.dxb"));
		this._vaultService.Setup(v => v.LoadVault("Rings"))
			.Returns(new VaultLoadResult { Filename = "Rings.vault", ArgumentException = new ArgumentException("bad vault") });

		var result = this._service.LoadAll([NewCharacter("A")], ["Main Vault", "Rings"], new CountingProgress());

		result.Errors.Select(e => (e.File, e.Error.Message)).Should().BeEquivalentTo(new (string?, string)[]
		{
			("a.chr", "bad player"),
			("a.dxb", "bad stash"),
			("Rings.vault", "bad vault"),
		});
	}

	[Fact]
	public void DisableLoadAll_TurnsTheLoadOffAndSaves()
	{
		this._settings.LoadAllFilesCompleted = false;
		this._service.LastRunFailed.Should().BeTrue();

		this._service.DisableLoadAll();

		this._settings.LoadAllFiles.Should().BeFalse();
		this._service.LastRunFailed.Should().BeFalse();
		this._service.SavedCompletedFlags.Should().Equal(true);
	}
}
