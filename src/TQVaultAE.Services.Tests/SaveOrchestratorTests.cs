using AwesomeAssertions;
using Moq;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Results;
using TQVaultAE.Config;
using TQVaultAE.Application;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

public class SaveOrchestratorTests
{
	private readonly Mock<IPlayerService> _playerService = new(MockBehavior.Strict);
	private readonly Mock<IVaultService> _vaultService = new(MockBehavior.Strict);
	private readonly Mock<IStashService> _stashService = new(MockBehavior.Strict);
	private readonly Mock<ISettingsPersistenceService> _settingsPersistence = new(MockBehavior.Strict);
	private readonly Mock<IGameFileService> _gameFileService = new(MockBehavior.Strict);
	private readonly UserSettings _settings = new() { GitBackupEnabled = true };
	private readonly SaveOrchestrator _orchestrator;

	/// <summary>
	/// The writes and calls, in order: "player:Hero", "vault:Main Vault", "stash:Hero - Immortal Throne", "configuration", "git".
	/// </summary>
	private readonly List<string> _calls = [];

	private readonly List<SaveError> _asked = [];

	/// <summary>
	/// The write that fails for a file name, with the attempt number (1 based); null when it succeeds.
	/// </summary>
	private readonly Dictionary<string, Func<int, Exception?>> _failures = [];

	private readonly Dictionary<string, int> _attempts = [];

	private static readonly SessionConfiguration Configuration = new("Main Vault", "Hero", 1.5F, true);

	public SaveOrchestratorTests()
	{
		this.ModifiedPlayers("Hero");
		this.ModifiedVaults("Main Vault", "Secondary Vault");
		this.ModifiedStashes("Hero");

		this._playerService
			.Setup(s => s.SaveModifiedPlayer(It.IsAny<string>(), It.IsAny<PlayerCollection>()))
			.Callback<string, PlayerCollection>((_, p) => this.Write("player", p.PlayerName));
		this._vaultService
			.Setup(s => s.SaveModifiedVault(It.IsAny<string>(), It.IsAny<PlayerCollection>()))
			.Callback<string, PlayerCollection>((_, v) => this.Write("vault", v.PlayerName));
		this._stashService
			.Setup(s => s.SaveModifiedStash(It.IsAny<string>(), It.IsAny<Stash>()))
			.Callback<string, Stash>((_, s) => this.Write("stash", s.PlayerName));
		this._settingsPersistence
			.Setup(s => s.SaveConfiguration(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<float>(), It.IsAny<bool>()))
			.Callback(() => this._calls.Add("configuration"))
			.Returns(false);
		this._gameFileService
			.Setup(g => g.GitAddCommitTagAndPush())
			.Callback(() => this._calls.Add("git"))
			.Returns(true);

		this._orchestrator = new SaveOrchestrator(
			this._playerService.Object
			, this._vaultService.Object
			, this._stashService.Object
			, this._settingsPersistence.Object
			, this._gameFileService.Object
			, this._settings);
	}

	private void ModifiedPlayers(params string[] names)
		=> this._playerService.Setup(s => s.GetModifiedPlayers())
			.Returns([.. names.Select(n => ($"{n}.chr", new PlayerCollection(n, $"{n}.chr")))]);

	private void ModifiedVaults(params string[] names)
		=> this._vaultService.Setup(s => s.GetModifiedVaults())
			.Returns([.. names.Select(n => ($"{n}.vault", new PlayerCollection(n, $"{n}.vault")))]);

	private void ModifiedStashes(params string[] names)
		=> this._stashService.Setup(s => s.GetModifiedStashes())
			.Returns([.. names.Select(n => ($"{n}.dxb", new Stash(n, $"{n}.dxb")))]);

	/// <summary>
	/// Records the write of <paramref name="name"/>, then throws when <see cref="_failures"/> says this attempt fails.
	/// </summary>
	private void Write(string kind, string name)
	{
		string call = $"{kind}:{name}";
		this._calls.Add(call);
		int attempt = this._attempts[call] = this._attempts.GetValueOrDefault(call) + 1;
		if (this._failures.TryGetValue(call, out var failure) && failure(attempt) is { } exception)
			throw exception;
	}

	private void Fails(string call, Func<int, Exception?> failure) => this._failures[call] = failure;

	private Func<SaveError, SaveDecision> Answer(params SaveDecision[] decisions)
	{
		var queue = new Queue<SaveDecision>(decisions);
		return error =>
		{
			this._asked.Add(error);
			return queue.Dequeue();
		};
	}

	private static Func<SaveError, SaveDecision> NeverAsked
		=> _ => throw new InvalidOperationException("No file error was expected");

	[Fact]
	public void Save_OnClose_SavesPlayersVaultsStashesThenConfigurationThenGit()
	{
		var result = this._orchestrator.Save(new SaveRequest(true, Configuration, true), NeverAsked);

		this._calls.Should().Equal("player:Hero", "vault:Main Vault", "vault:Secondary Vault", "stash:Hero - Immortal Throne", "configuration", "git");
		result.PlayersSaved.Should().BeTrue();
		result.VaultsSaved.Should().Be(2);
		result.StashesSaved.Should().Be(1);
		result.GitPushed.Should().BeTrue();
		result.Errors.Should().BeEmpty();
	}

	[Fact]
	public void Save_PassesSessionStateToConfigurationAndReturnsItsChangeFlag()
	{
		this._settingsPersistence
			.Setup(s => s.SaveConfiguration("Main Vault", "Hero", 1.5F, true))
			.Returns(true);

		var result = this._orchestrator.Save(new SaveRequest(true, Configuration), NeverAsked);

		result.ConfigChanged.Should().BeTrue();
	}

	[Fact]
	public void Save_FilesOnly_DoesNotTouchConfigurationNorGit()
	{
		var result = this._orchestrator.Save(SaveRequest.FilesOnly, NeverAsked);

		this._calls.Should().NotContain(["configuration", "git"]);
		result.ConfigChanged.Should().BeNull();
		result.GitPushed.Should().BeFalse();
	}

	[Fact]
	public void Save_FilesRefused_StillSavesConfigurationAndGit()
	{
		this._orchestrator.Save(new SaveRequest(false, Configuration, true), NeverAsked);

		this._calls.Should().Equal("configuration", "git");
	}

	[Fact]
	public void Save_GitBackupDisabled_DoesNotPush()
	{
		this._settings.GitBackupEnabled = false;

		var result = this._orchestrator.Save(new SaveRequest(true, Configuration, true), NeverAsked);

		this._gameFileService.Verify(g => g.GitAddCommitTagAndPush(), Times.Never);
		result.GitPushed.Should().BeFalse();
	}

	[Fact]
	public void Save_NothingModified_ReportsNothingSaved()
	{
		this.ModifiedPlayers();
		this.ModifiedVaults();
		this.ModifiedStashes();

		var result = this._orchestrator.Save(SaveRequest.FilesOnly, NeverAsked);

		result.AnythingSaved.Should().BeFalse();
		this._calls.Should().BeEmpty();
	}

	[Fact]
	public void Save_OnlyAStashSaved_ReportsSomethingSavedButNoPlayers()
	{
		this.ModifiedPlayers();
		this.ModifiedVaults();

		var result = this._orchestrator.Save(SaveRequest.FilesOnly, NeverAsked);

		result.AnythingSaved.Should().BeTrue();
		result.PlayersSaved.Should().BeFalse();
	}

	[Fact]
	public void Save_PlayerFileLocked_AsksWithThePlayerNameAndStep()
	{
		var locked = new IOException("locked");
		this.Fails("player:Hero", n => n == 1 ? locked : null);

		this._orchestrator.Save(SaveRequest.FilesOnly, this.Answer(SaveDecision.Retry));

		this._asked.Should().ContainSingle();
		this._asked[0].Step.Should().Be(SaveStep.Players);
		this._asked[0].Name.Should().Be("Hero");
		this._asked[0].Exception.Should().BeSameAs(locked);
	}

	[Fact]
	public void Save_Retry_WritesOnlyTheFailedFileAgain()
	{
		this.Fails("vault:Main Vault", n => n == 1 ? new IOException("locked") : null);

		var result = this._orchestrator.Save(SaveRequest.FilesOnly, this.Answer(SaveDecision.Retry));

		this._calls.Should().Equal("player:Hero", "vault:Main Vault", "vault:Main Vault", "vault:Secondary Vault", "stash:Hero - Immortal Throne");
		result.VaultsSaved.Should().Be(2);
		result.Errors.Should().ContainSingle().Which.Step.Should().Be(SaveStep.Vaults);
	}

	[Fact]
	public void Save_RetryFailingAgain_AsksAgainUntilIgnored()
	{
		this.Fails("player:Hero", _ => new IOException("locked"));

		var result = this._orchestrator.Save(SaveRequest.FilesOnly, this.Answer(SaveDecision.Retry, SaveDecision.Retry, SaveDecision.Ignore));

		this._calls.Should().StartWith(["player:Hero", "player:Hero", "player:Hero", "vault:Main Vault"]);
		result.Errors.Should().HaveCount(3);
	}

	[Fact]
	public void Save_IgnoreAVault_StillSavesTheVaultsAfterIt()
	{
		this.ModifiedVaults("Main Vault", "Secondary Vault", "Third Vault");
		this.Fails("vault:Secondary Vault", _ => new IOException("locked"));

		var result = this._orchestrator.Save(SaveRequest.FilesOnly, this.Answer(SaveDecision.Ignore));

		this._calls.Should().Equal("player:Hero", "vault:Main Vault", "vault:Secondary Vault", "vault:Third Vault", "stash:Hero - Immortal Throne");
		result.VaultsSaved.Should().Be(2);
		this._asked.Should().ContainSingle().Which.Name.Should().Be("Secondary Vault");
	}

	[Fact]
	public void Save_IgnoreTheOnlyPlayer_ReportsNoPlayersSavedAndContinuesWithTheNextSteps()
	{
		this.Fails("player:Hero", _ => new IOException("locked"));

		var result = this._orchestrator.Save(new SaveRequest(true, Configuration, true), this.Answer(SaveDecision.Ignore));

		this._calls.Should().Equal("player:Hero", "vault:Main Vault", "vault:Secondary Vault", "stash:Hero - Immortal Throne", "configuration", "git");
		result.PlayersSaved.Should().BeFalse();
		result.VaultsSaved.Should().Be(2);
		result.AnythingSaved.Should().BeTrue();
	}

	[Fact]
	public void Save_IgnoreOnePlayerOfTwo_StillReportsPlayersSaved()
	{
		this.ModifiedPlayers("Hero", "Mage");
		this.Fails("player:Hero", _ => new IOException("locked"));

		var result = this._orchestrator.Save(SaveRequest.FilesOnly, this.Answer(SaveDecision.Ignore));

		result.PlayersSaved.Should().BeTrue();
		this._calls.Should().Contain("player:Mage");
	}

	[Fact]
	public void Save_Abort_RethrowsTheFileErrorAndWritesNothingAfterIt()
	{
		var locked = new IOException("locked");
		this.Fails("vault:Main Vault", _ => locked);

		var save = () => this._orchestrator.Save(new SaveRequest(true, Configuration, true), this.Answer(SaveDecision.Abort));

		save.Should().Throw<IOException>().Which.Should().BeSameAs(locked);
		this._calls.Should().Equal("player:Hero", "vault:Main Vault");
	}

	[Fact]
	public void Save_AccessDenied_IsAskedLikeAFileError()
	{
		var denied = new UnauthorizedAccessException("read-only");
		this.Fails("vault:Main Vault", n => n == 1 ? denied : null);

		var result = this._orchestrator.Save(SaveRequest.FilesOnly, this.Answer(SaveDecision.Retry));

		this._asked.Should().ContainSingle().Which.Exception.Should().BeSameAs(denied);
		result.VaultsSaved.Should().Be(2);
	}

	[Fact]
	public void Save_AbortAfterAccessDenied_RethrowsIt()
	{
		this.Fails("stash:Hero - Immortal Throne", _ => new UnauthorizedAccessException("read-only"));

		var save = () => this._orchestrator.Save(new SaveRequest(true, Configuration, true), this.Answer(SaveDecision.Abort));

		save.Should().Throw<UnauthorizedAccessException>();
		this._calls.Should().NotContain("configuration");
	}

	[Fact]
	public void Save_StashFileLocked_ReportsTheStashOwnerAndStillPushesGit()
	{
		this.Fails("stash:Hero - Immortal Throne", _ => new IOException("locked"));

		var result = this._orchestrator.Save(new SaveRequest(true, Configuration, true), this.Answer(SaveDecision.Ignore));

		this._asked.Should().ContainSingle();
		this._asked[0].Step.Should().Be(SaveStep.Stashes);
		this._asked[0].Name.Should().Be("Hero - Immortal Throne");
		result.StashesSaved.Should().Be(0);
		this._calls.Should().EndWith("git");
	}

	[Fact]
	public void Save_ErrorOtherThanFileError_IsNotCaught()
	{
		this.Fails("player:Hero", _ => new InvalidOperationException("bug"));

		var save = () => this._orchestrator.Save(SaveRequest.FilesOnly, NeverAsked);

		save.Should().Throw<InvalidOperationException>();
	}
}
