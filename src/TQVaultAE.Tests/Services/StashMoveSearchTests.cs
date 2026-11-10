using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Drawing;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Results;
using TQVaultAE.Config;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

/// <summary>
/// An item auto-moved into a stash, then the files reloaded (hot reload): the search must keep exactly one entry for it,
/// at the place the stash load gives.
/// </summary>
/// <remarks>
/// Real <see cref="ItemDatabaseService"/>, <see cref="StashService"/>, <see cref="PlayerService"/> and
/// <see cref="ItemMovementService"/>; the files are a dictionary read by the mocked providers.
/// </remarks>
public sealed class StashMoveSearchTests : IDisposable
{
	private static readonly string PlayerFile = Path.Combine("""C:\Saves\_HeroB""", "Player.chr");
	private static readonly string PlayerStashFile = Path.Combine("""C:\Saves\_HeroB""", "winsys.dxb");
	private const string TransferStashFile = """C:\Saves\Sys\winsys.dxb""";
	private const string RelicVaultStashFile = """C:\Saves\Sys\miscsys.dxb""";

	private static readonly RecordId RelicId = RecordId.Create("records/xpack/items/relics/relic01.dbr");

	/// <summary>The saved files: their items, read again on each load.</summary>
	private readonly Dictionary<string, List<Item>> _files = new(StringComparer.OrdinalIgnoreCase);

	private readonly ItemDatabaseService _database;
	private readonly StashService _stashService;
	private readonly PlayerService _playerService;
	private readonly ItemMovementService _movement = new(new Mock<ILogger<ItemMovementService>>().Object);
	private readonly PlayerSave _save;

	public StashMoveSearchTests()
	{
		var session = new SessionContext();
		this._database = new ItemDatabaseService(new Mock<ILogger<ItemDatabaseService>>().Object, new Mock<IItemProvider>().Object, session);

		var gamePaths = new Mock<IGamePathService>();
		gamePaths.Setup(x => x.PlayerSaveFileName).Returns("Player.chr");
		gamePaths.Setup(x => x.PlayerStashFileNameB).Returns("winsys.dxb");
		gamePaths.Setup(x => x.TransferStashFileFullPath).Returns(TransferStashFile);
		gamePaths.Setup(x => x.RelicVaultStashFileFullPath).Returns(RelicVaultStashFile);

		var stashProvider = new Mock<IStashProvider>();
		stashProvider.Setup(x => x.LoadFile(It.IsAny<Stash>())).Callback<Stash>(stash =>
		{
			stash.CreateEmptySack();
			foreach (Item item in this.ReadFile(stash.StashFile))
				stash.Sack.AddItem(item);
		}).Returns(true);

		var playerProvider = new Mock<IPlayerCollectionProvider>();
		playerProvider.Setup(x => x.LoadFile(It.IsAny<PlayerCollection>(), null)).Callback<PlayerCollection, string?>((player, _) =>
		{
			player.CreateEmptySacks(1);
			foreach (Item item in this.ReadFile(player.PlayerFile))
				player.Sacks[0].AddItem(item);
		});

		this._stashService = new StashService(
			new Mock<ILogger<StashService>>().Object, session, this._database, stashProvider.Object
			, gamePaths.Object, new Mock<IGameFileService>().Object, new UserSettings());

		this._playerService = new PlayerService(
			new Mock<ILogger<PlayerService>>().Object, session, this._database, playerProvider.Object, this._stashService
			, new Mock<IGameFileService>().Object, gamePaths.Object, new Mock<ITranslationService>().Object
			, new Mock<ITQDataService>().Object, new Mock<ITagService>().Object, new Mock<IFileIO>().Object
			, new PathIO(), new UserSettings(), new Mock<IDirectoryIO>().Object
			, new Mock<IItemProvider>().Object, new Mock<IDatabase>().Object, new Mock<IGameMapService>().Object);

		var pathIO = new Mock<IPathIO>();
		pathIO.Setup(x => x.GetFileName("""C:\Saves\_HeroB""")).Returns("_HeroB");
		this._save = new PlayerSave("""C:\Saves\_HeroB""", true, false, false, "", new Mock<ITranslationService>().Object, pathIO.Object);
	}

	public void Dispose() => this._save.Dispose();

	/// <summary>New objects on each read, as a real load.</summary>
	private List<Item> ReadFile(string path)
		=> this._files.TryGetValue(path, out var items) ? items.Select(i => i.Clone()).ToList() : [];

	private static Item Relic() => new()
	{
		BaseItemId = RelicId,
		Width = 1,
		Height = 1,
	};

	private Stash LoadStash(StashType kind, bool reload = false) => (kind switch
	{
		StashType.PlayerStash => this._stashService.LoadPlayerStash(this._save, reload),
		StashType.TransferStash => this._stashService.LoadTransferStash(reload),
		_ => this._stashService.LoadRelicVaultStash(reload),
	}).Stash!;

	private static string FileOf(StashType kind) => kind switch
	{
		StashType.PlayerStash => PlayerStashFile,
		StashType.TransferStash => TransferStashFile,
		_ => RelicVaultStashFile,
	};

	private List<SearchResult> RelicEntries()
		=> this._database.ItemDatabase.Where(r => r.Item.BaseItemId == RelicId).ToList();

	[Theory]
	[InlineData(StashType.PlayerStash)]
	[InlineData(StashType.TransferStash)]
	[InlineData(StashType.RelicVaultStash)]
	public void MoveIntoStash_ThenReloads_KeepOneEntryAtTheStash(StashType kind)
	{
		this._files[PlayerFile] = [Relic()];
		var player = this._playerService.LoadPlayer(this._save).Player!;
		var stash = this.LoadStash(kind);
		var item = player.Sacks![0].Single();

		// The auto-move: the drag takes the item out of the character.
		var place = this._movement.PlaceIn(stash, kind);
		this._movement.TryPlace(item, stash.Sack!, new Size(5, 5), place, () => player.Sacks[0].RemoveItem(item)).Should().BeTrue();

		this.RelicEntries().Should().ContainSingle().Which.Item.Place.Should().Be(place);
		place.Path.Should().Be(FileOf(kind));
		place.Name.Should().Be(stash.Name);

		// Saved, then the game rewrites the character: its reload must not take the item away from the search.
		this._files[PlayerFile] = [];
		this._files[FileOf(kind)] = [item];
		this._playerService.LoadPlayer(this._save, fromFileWatcher: true);

		this.RelicEntries().Should().ContainSingle().Which.Item.Should().BeSameAs(item);

		// Then the stash: its reloaded items replace the old ones, at the same place.
		var reloaded = this.LoadStash(kind, reload: true);

		var entry = this.RelicEntries().Should().ContainSingle().Subject;
		entry.Item.Should().BeSameAs(reloaded.Sack!.Single());
		entry.Item.Place.Should().Be(place, "a moved item gets the place the load gives");
		entry.ContainerName.Should().Be(stash.Name);
	}

	[Fact]
	public void PlayerStashPlace_IsTheCharacterNameAndTheStashFile()
	{
		var stash = this.LoadStash(StashType.PlayerStash);

		var place = this._movement.PlaceIn(stash, StashType.PlayerStash);

		place.Name.Should().Be("HeroB", "the search goes to the character by this name");
		place.Path.Should().Be(PlayerStashFile);
		place.SackNumber.Should().Be(BagIdConstants.BAGID_PLAYERSTASH);
		place.SackType.Should().Be(SackType.Stash);
	}

	[Fact]
	public void StashReload_WithoutMove_DoesNotDuplicateTheSearchEntries()
	{
		this._files[TransferStashFile] = [Relic()];
		this.LoadStash(StashType.TransferStash);

		this.LoadStash(StashType.TransferStash, reload: true);
		this.LoadStash(StashType.TransferStash, reload: true);

		this.RelicEntries().Should().ContainSingle();
	}

	[Fact]
	public void CharacterReload_WithoutMove_DoesNotDuplicateTheSearchEntries()
	{
		this._files[PlayerFile] = [Relic()];
		this._playerService.LoadPlayer(this._save);

		this._playerService.LoadPlayer(this._save, fromFileWatcher: true);

		this.RelicEntries().Should().ContainSingle();
	}
}
