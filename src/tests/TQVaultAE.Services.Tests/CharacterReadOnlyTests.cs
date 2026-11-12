using System.Drawing;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Results;
using TQVaultAE.Config;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

/// <summary>
/// A read-only character (<see cref="CharacterReadOnlyService"/>): the read-only attribute of its save files, and the
/// rules that keep it from changing (moves, sorts, character editor, save).
/// </summary>
public sealed class CharacterReadOnlyTests : IDisposable
{
	private readonly string _folder = Path.Combine(Path.GetTempPath(), "tqvault-readonly-" + Guid.NewGuid().ToString("N"), "_HeroA");
	private readonly Mock<IGamePathService> _gamePath = new();
	private readonly SessionContext _session = new();
	private readonly FileIO _fileIO = new();
	private readonly CharacterReadOnlyService _service;

	public CharacterReadOnlyTests()
	{
		Directory.CreateDirectory(this._folder);
		this._gamePath.Setup(p => p.PlayerSaveFileName).Returns("Player.chr");
		this._gamePath.Setup(p => p.PlayerStashFileNameB).Returns("winsys.dxb");
		this._gamePath.Setup(p => p.PlayerStashFileNameG).Returns("winsys.dxg");
		this._service = new CharacterReadOnlyService(new Mock<ILogger<CharacterReadOnlyService>>().Object, this._session
			, this._gamePath.Object, this._fileIO, new PathIO());
	}

	public void Dispose()
	{
		// Read-only files cannot be deleted.
		var root = Path.GetDirectoryName(this._folder)!;
		foreach (var file in Directory.GetFiles(root, "*", SearchOption.AllDirectories))
			File.SetAttributes(file, FileAttributes.Normal);
		Directory.Delete(root, true);
	}

	private string FileOf(string name) => Path.Combine(this._folder, name);

	private string Write(string name)
	{
		var file = this.FileOf(name);
		File.WriteAllBytes(file, [1, 2, 3]);
		return file;
	}

	private PlayerSave Save() => new(this._folder, true, false, false, string.Empty, new Mock<ITranslationService>().Object, new PathIO());

	private PlayerCollection LoadedPlayer()
	{
		var player = new PlayerCollection("HeroA", this.FileOf("Player.chr")) { EquipmentSack = new SackCollection() };
		player.CreateEmptySacks(2);
		this._session.Players.GetOrAddAtomic(player.PlayerFile, _ => player);
		return player;
	}

	private Stash LoadedStash()
	{
		var stash = new Stash("HeroA", this.FileOf("winsys.dxb"));
		stash.CreateEmptySack();
		this._session.Stashes.GetOrAddAtomic(stash.StashFile, _ => stash);
		return stash;
	}

	#region The rule

	[Fact]
	public void IsReadOnly_NoFileReadOnly_IsFalse()
	{
		this.Write("Player.chr");
		this.Write("winsys.dxb");

		this._service.IsReadOnly(this._folder).Should().BeFalse();
	}

	[Theory]
	[InlineData("Player.chr")]
	[InlineData("winsys.dxb")]
	[InlineData("winsys.dxg")]
	[InlineData(IModLinkService.LinkFileName)]
	public void IsReadOnly_AnyOwnedFileReadOnly_IsTrue(string readOnlyFile)
	{
		foreach (var name in new[] { "Player.chr", "winsys.dxb", "winsys.dxg", IModLinkService.LinkFileName })
			this.Write(name);
		File.SetAttributes(this.FileOf(readOnlyFile), FileAttributes.ReadOnly);

		this._service.IsReadOnly(this._folder).Should().BeTrue();
	}

	[Fact]
	public void IsReadOnly_AnotherFileOfTheFolderReadOnly_IsFalse()
	{
		this.Write("Player.chr");
		File.SetAttributes(this.Write("settings.txt"), FileAttributes.ReadOnly);

		this._service.IsReadOnly(this._folder).Should().BeFalse();
	}

	#endregion

	#region The toggle

	[Fact]
	public void SetReadOnly_SetsTheAttributeOfEveryOwnedFileAndMarksWhatIsLoaded()
	{
		var player = this.Write("Player.chr");
		var stashB = this.Write("winsys.dxb");
		var stashG = this.Write("winsys.dxg");
		var loadedPlayer = this.LoadedPlayer();
		var loadedStash = this.LoadedStash();
		var save = this.Save();

		var result = this._service.SetReadOnly(save, makeReadOnly: true);

		result.Should().Be(new ReadOnlyChangeResult(ReadOnlyChange.Changed, true));
		new[] { player, stashB, stashG }.Should().OnlyContain(f => File.GetAttributes(f).HasFlag(FileAttributes.ReadOnly));
		File.Exists(this.FileOf(IModLinkService.LinkFileName)).Should().BeFalse("a missing owned file is not created");
		save.IsReadOnly.Should().BeTrue();
		loadedPlayer.IsReadOnly.Should().BeTrue();
		loadedPlayer.Sacks!.Append(loadedPlayer.EquipmentSack!).Should().OnlyContain(s => s.IsReadOnly);
		loadedStash.Sack!.IsReadOnly.Should().BeTrue();
	}

	[Fact]
	public void SetReadOnly_Off_ClearsTheAttributeOfEveryOwnedFileAndKeepsTheOthers()
	{
		var player = this.Write("Player.chr");
		var stash = this.Write("winsys.dxb");
		File.SetAttributes(player, FileAttributes.ReadOnly | FileAttributes.Archive);
		File.SetAttributes(stash, FileAttributes.ReadOnly);
		var loadedPlayer = this.LoadedPlayer();
		loadedPlayer.IsReadOnly = true;
		var save = this.Save();
		save.IsReadOnly = true;

		var result = this._service.SetReadOnly(save, makeReadOnly: false);

		result.Change.Should().Be(ReadOnlyChange.Changed);
		result.IsReadOnly.Should().BeFalse();
		File.GetAttributes(player).Should().Be(FileAttributes.Archive);
		File.GetAttributes(stash).HasFlag(FileAttributes.ReadOnly).Should().BeFalse();
		save.IsReadOnly.Should().BeFalse();
		loadedPlayer.Sacks!.Should().OnlyContain(s => !s.IsReadOnly);
	}

	[Theory]
	[InlineData(true, false)]
	[InlineData(false, true)]
	public void SetReadOnly_WithUnsavedChanges_IsRefusedAndChangesNothing(bool playerModified, bool stashModified)
	{
		var player = this.Write("Player.chr");
		this.Write("winsys.dxb");
		var loadedPlayer = this.LoadedPlayer();
		var loadedStash = this.LoadedStash();
		loadedPlayer.Sacks![0].IsModified = playerModified;
		loadedStash.Sack!.IsModified = stashModified;

		var result = this._service.SetReadOnly(this.Save(), makeReadOnly: true);

		result.Change.Should().Be(ReadOnlyChange.UnsavedChanges);
		result.IsReadOnly.Should().BeFalse();
		File.GetAttributes(player).HasFlag(FileAttributes.ReadOnly).Should().BeFalse();
		loadedPlayer.IsReadOnly.Should().BeFalse();
	}

	[Fact]
	public void SetReadOnly_WhenAnAttributeCannotChange_TellsTheStateTheFilesAreIn()
	{
		var fileIO = new Mock<FileIO> { CallBase = true };
		fileIO.Setup(f => f.SetReadOnly(It.Is<string>(p => p.EndsWith("winsys.dxb", StringComparison.Ordinal)), It.IsAny<bool>()))
			.Throws(new UnauthorizedAccessException("denied"));
		var service = new CharacterReadOnlyService(new Mock<ILogger<CharacterReadOnlyService>>().Object, this._session
			, this._gamePath.Object, fileIO.Object, new PathIO());
		this.Write("Player.chr");
		this.Write("winsys.dxb");
		var save = this.Save();

		var result = service.SetReadOnly(save, makeReadOnly: true);

		// Player.chr was changed before the failure: the character is read-only, and says so.
		result.Change.Should().Be(ReadOnlyChange.Failed);
		result.Error.Should().BeOfType<UnauthorizedAccessException>();
		result.IsReadOnly.Should().BeTrue();
		save.IsReadOnly.Should().BeTrue();
	}

	[Fact]
	public void ClearReadOnly_ClearsTheOwnedFilesOnly()
	{
		var player = this.Write("Player.chr");
		var other = this.Write("settings.txt");
		File.SetAttributes(player, FileAttributes.ReadOnly);
		File.SetAttributes(other, FileAttributes.ReadOnly);

		this._service.ClearReadOnly(this._folder);

		File.GetAttributes(player).HasFlag(FileAttributes.ReadOnly).Should().BeFalse();
		File.GetAttributes(other).HasFlag(FileAttributes.ReadOnly).Should().BeTrue();
	}

	#endregion

	#region What a read-only character refuses

	[Fact]
	public void PlayerCollection_IsReadOnly_MarksEverySack()
	{
		var player = new PlayerCollection("HeroA", "Player.chr") { EquipmentSack = new SackCollection() };
		player.CreateEmptySacks(3);

		player.IsReadOnly = true;

		player.Sacks!.Append(player.EquipmentSack).Should().OnlyContain(s => s.IsReadOnly);
	}

	[Fact]
	public void TryPlace_InASackOfAReadOnlyCharacter_IsRefusedAndTheItemStays()
	{
		var movement = new ItemMovementService(new Mock<ILogger<ItemMovementService>>().Object);
		var sack = new SackCollection { IsReadOnly = true };
		var item = new Item { BaseItemId = RecordId.Create("records/item/thing.dbr"), Width = 1, Height = 1 };
		bool taken = false;

		movement.TryPlace(item, sack, new Size(4, 4), new ContainerPlace(), () => taken = true).Should().BeFalse();

		sack.IsEmpty.Should().BeTrue();
		taken.Should().BeFalse("the item is not taken out of its source sack");
	}

	[Fact]
	public void Sort_ASackOfAReadOnlyCharacter_IsRefusedAndLeavesItAsItIs()
	{
		var itemProvider = new Mock<IItemProvider>();
		var sorting = new ItemSortService(new ItemMovementService(new Mock<ILogger<ItemMovementService>>().Object)
			, new ItemStackingService(itemProvider.Object), itemProvider.Object, Mock.Of<IItemDatabaseService>());
		var sack = new SackCollection();
		var item = new Item { BaseItemId = RecordId.Create("records/item/thing.dbr"), Width = 1, Height = 1, PositionX = 3, PositionY = 3 };
		sack.AddItem(item);
		sack.IsModified = false;
		sack.IsReadOnly = true;

		var result = sorting.Sort(sack, new Size(4, 4), vertical: false, SackSortAlgorithm.Normal, mergeRelics: true);

		result.ReadOnly.Should().BeTrue();
		result.Sorted.Should().BeFalse();

		(item.PositionX, item.PositionY).Should().Be((3, 3));
		sack.IsModified.Should().BeFalse();
	}

	[Fact]
	public void CharacterEditApply_OnAReadOnlyCharacter_ChangesNothing()
	{
		var provider = new Mock<IPlayerCollectionProvider>();
		var edit = new CharacterEditService(new PathIO(), provider.Object
			, new Mock<ISkillTreeProvider>().Object, new Mock<IItemProvider>().Object);
		var player = new PlayerCollection("HeroA", "Player.chr") { PlayerInfo = new PlayerInfo(new PathIO()) { CurrentLevel = 10 } };
		player.IsReadOnly = true;
		var request = new CharacterEditRequest(20, "0", null, 0, 0, 50, 50, 50, 300, 300);

		edit.Apply(player, request).Should().BeFalse();

		provider.Verify(p => p.CommitPlayerInfo(It.IsAny<PlayerCollection>(), It.IsAny<PlayerInfo>()), Times.Never);
	}

	[Fact]
	public void GetModifiedPlayers_SkipsAReadOnlyCharacter()
	{
		var player = this.LoadedPlayer();
		player.Sacks![0].IsModified = true;
		player.IsReadOnly = true;
		var service = new PlayerService(new Mock<ILogger<PlayerService>>().Object, this._session, Mock.Of<IItemDatabaseService>()
			, Mock.Of<IPlayerCollectionProvider>(), Mock.Of<IStashService>(), Mock.Of<IGameFileService>(), this._gamePath.Object
			, Mock.Of<ITranslationService>(), Mock.Of<ITQDataService>(), Mock.Of<ITagService>(), this._fileIO, new PathIO()
			, new UserSettings(), Mock.Of<IDirectoryIO>(), Mock.Of<IItemProvider>(), Mock.Of<IDatabase>(), Mock.Of<IGameMapService>()
			, Mock.Of<ICharacterStatsService>(), Mock.Of<ICharacterEditService>());

		service.GetModifiedPlayers().Should().BeEmpty();

		player.IsReadOnly = false;
		service.GetModifiedPlayers().Should().ContainSingle();
	}

	[Fact]
	public void GetModifiedStashes_SkipsTheStashOfAReadOnlyCharacter()
	{
		var stash = this.LoadedStash();
		stash.Sack!.IsModified = true;
		stash.IsReadOnly = true;
		var service = new StashService(new Mock<ILogger<StashService>>().Object, this._session, Mock.Of<IItemDatabaseService>()
			, Mock.Of<IStashProvider>(), this._gamePath.Object, Mock.Of<IGameFileService>(), new UserSettings());

		service.GetModifiedStashes().Should().BeEmpty();

		stash.IsReadOnly = false;
		service.GetModifiedStashes().Should().ContainSingle();
	}

	[Fact]
	public void DuplicateCharacter_OfAReadOnlyCharacter_StartsWritable()
	{
		var copyFolder = Path.Combine(Path.GetDirectoryName(this._folder)!, "_HeroB");
		var gameFiles = new Mock<IGameFileService>();
		gameFiles.Setup(g => g.GetDuplicateCharacterFolder(this._folder, "HeroB")).Returns(copyFolder);
		gameFiles.Setup(g => g.DuplicateCharacterFiles(this._folder, "HeroB")).Returns(() =>
		{
			// Copies keep the read-only attribute of their source.
			Directory.CreateDirectory(copyFolder);
			File.Copy(this.FileOf("Player.chr"), Path.Combine(copyFolder, "Player.chr"));
			return copyFolder;
		});
		this.Write("Player.chr");
		File.SetAttributes(this.FileOf("Player.chr"), FileAttributes.ReadOnly);
		var service = new PlayerService(new Mock<ILogger<PlayerService>>().Object, this._session, Mock.Of<IItemDatabaseService>()
			, Mock.Of<IPlayerCollectionProvider>(), Mock.Of<IStashService>(), gameFiles.Object, this._gamePath.Object
			, Mock.Of<ITranslationService>(), Mock.Of<ITQDataService>(), Mock.Of<ITagService>(), this._fileIO, new PathIO()
			, new UserSettings(), Mock.Of<IDirectoryIO>(), Mock.Of<IItemProvider>(), Mock.Of<IDatabase>(), Mock.Of<IGameMapService>()
			, Mock.Of<ICharacterStatsService>(), Mock.Of<ICharacterEditService>(), characterReadOnlyService: this._service);

		var result = service.DuplicateCharacter(this.Save(), "HeroB");

		result.IsSuccess.Should().BeTrue(result.Error?.Message);
		File.GetAttributes(Path.Combine(copyFolder, "Player.chr")).HasFlag(FileAttributes.ReadOnly).Should().BeFalse();
		File.GetAttributes(this.FileOf("Player.chr")).HasFlag(FileAttributes.ReadOnly).Should().BeTrue("the source stays read-only");
	}

	#endregion
}
