using System.Drawing;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Results;
using TQVaultAE.Application.Session;
using TQVaultAE.Config;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

/// <summary>
/// <see cref="ResultsMoveService"/>: the results window moves found items to a stash or a vault bag, skipping and
/// reporting what cannot move, and puts them back.
/// </summary>
public sealed class ResultsMoveServiceTests : IDisposable
{
	private static readonly Size VaultBag = new(4, 4);

	private readonly string _folder = Path.Combine(Path.GetTempPath(), $"tqvault-resultsmove-{Guid.NewGuid():N}", "_HeroA");
	private readonly SessionContext _session = new();
	private readonly UserSettings _settings = new() { PlayerReadonly = false };
	private readonly Mock<IItemEditService> _itemEdit = new();
	private readonly Mock<ICharacterReadOnlyService> _readOnly = new();
	private readonly Mock<IItemProvider> _itemProvider = new();
	private readonly Mock<IGameMapService> _gameMap = new();
	private readonly LoadoutService _loadouts;
	private readonly ResultsMoveService _service;

	public ResultsMoveServiceTests()
	{
		Directory.CreateDirectory(this._folder);
		var movement = new ItemMovementService(new Mock<ILogger<ItemMovementService>>().Object);
		this._itemEdit
			.Setup(e => e.SettleAll(It.IsAny<SackCollection>(), It.IsAny<Size>(), It.IsAny<IEnumerable<Item>>(), It.IsAny<bool>(), It.IsAny<string?>(), It.IsAny<Func<SackCollection, Item, bool>?>()))
			.Returns(new SettleResult([], []));
		this._loadouts = new LoadoutService(new Mock<ILogger<LoadoutService>>().Object, this._itemProvider.Object, movement
			, new Mock<IItemDatabaseService>().Object, this._session, new FileIO(), new PathIO());
		this._service = new ResultsMoveService(new Mock<ILogger<ResultsMoveService>>().Object, this._session, movement
			, this._itemEdit.Object, this._loadouts, this._readOnly.Object, this._itemProvider.Object, this._gameMap.Object
			, new PathIO(), this._settings);
	}

	public void Dispose()
	{
		var root = Path.GetDirectoryName(this._folder)!;
		if (Directory.Exists(root))
			Directory.Delete(root, true);
	}

	private static Item NewItem(string record = "records/item/ring.dbr", int width = 1, int height = 1) => new()
	{
		BaseItemId = RecordId.Create(record),
		Width = width,
		Height = height,
		Mod = null,
		ModOrigin = ModOrigin.Inherited,
	};

	/// <summary>A vault of the session, its items placed as the search places them.</summary>
	private PlayerCollection Vault(string name, int bags = 2)
	{
		var vault = new PlayerCollection(name, $"{name}.vault.json") { IsVault = true };
		vault.CreateEmptySacks(bags);
		this._session.Vaults.GetOrAddAtomic(vault.PlayerFile, _ => vault);
		return vault;
	}

	/// <summary>A character of the session with a main sack and a bag, and empty equipment slots.</summary>
	private PlayerCollection Character()
	{
		var equipment = new SackCollection { SackType = SackType.Equipment, slots = 12 };
		for (int slot = 0; slot < 12; slot++)
			equipment.AddItem(new Item { BaseItemId = RecordId.Empty, Location = SackCollection.GetEquipmentLocationOffset(slot) });

		var player = new PlayerCollection("HeroA", Path.Combine(this._folder, "Player.chr")) { EquipmentSack = equipment };
		player.CreateEmptySacks(2);
		this._session.Players.GetOrAddAtomic(player.PlayerFile, _ => player);
		return player;
	}

	private Stash SharedStash(StashType type)
	{
		var stash = new Stash("Transfer", $"{type}.dxb") { Width = 4, Height = 4 };
		stash.CreateEmptySack();
		stash.Sack.StashType = type;
		this._session.Stashes.GetOrAddAtomic(stash.StashFile, _ => stash);
		return stash;
	}

	private static Item Put(SackCollection sack, Item item, int x, int y, ContainerPlace place)
	{
		item.Location = new Point(x, y);
		item.Place = place;
		sack.AddItem(item);
		sack.IsModified = false;
		return item;
	}

	private static ContainerPlace VaultPlace(PlayerCollection vault, int bag)
		=> new() { Path = vault.PlayerFile, Name = vault.PlayerName, SackNumber = bag, SackType = SackType.Vault };

	private static Item Wear(PlayerCollection player, int slot, Item item)
	{
		var equipment = player.EquipmentSack!;
		equipment.RemoveAtItem(slot);
		item.Location = SackCollection.GetEquipmentLocationOffset(slot);
		item.Place = new ContainerPlace { Path = player.PlayerFile, Name = player.PlayerName, SackType = SackType.Equipment, SackNumber = BagIdConstants.BAGID_EQUIPMENTPANEL };
		equipment.InsertItem(slot, item);
		equipment.IsModified = false;
		return item;
	}

	#region Move

	[Fact]
	public void Move_ToAVaultBag_PlacesEachItemAtTheFirstFreeCells_AndMarksBothBagsModified()
	{
		var source = this.Vault("Source");
		var ring = Put(source.GetSack(0)!, NewItem(), 2, 3, VaultPlace(source, 0));
		var sword = Put(source.GetSack(1)!, NewItem("records/item/sword.dbr", 1, 3), 0, 0, VaultPlace(source, 1));
		var target = this._service.VaultTarget(this.Vault("Target"), 1, VaultBag)!;

		var outcome = this._service.Move([ring, sword], target);

		outcome.Done.Should().Equal(ring, sword);
		outcome.Skipped.Should().BeEmpty();
		target.Sack.Should().Contain([ring, sword]);
		ring.Location.Should().Be(new Point(0, 0));
		sword.Location.Should().Be(new Point(0, 1), "free cells are searched column by column, as the auto-move does");
		source.GetSack(0)!.Should().BeEmpty();
		source.GetSack(0)!.IsModified.Should().BeTrue("the moves are saved like any user move");
		target.Sack.IsModified.Should().BeTrue();
		ring.Place.Name.Should().Be("Target", "the results row shows the new place");
		ring.Place.SackNumber.Should().Be(1);
		outcome.ChangedSacks.Should().Contain([target.Sack, source.GetSack(0)!, source.GetSack(1)!]);
	}

	[Fact]
	public void Move_ItemsTakeTheTargetsMap_AsItemsPutThereByMoveTo()
	{
		var source = this.Vault("Source");
		var ring = Put(source.GetSack(0)!, NewItem(), 0, 0, VaultPlace(source, 0));
		var vault = this.Vault("Target");
		vault.Mod = "SomeMod|custommaps";
		var target = this._service.VaultTarget(vault, 0, VaultBag)!;

		this._service.Move([ring], target);

		this._itemEdit.Verify(e => e.SettleAll(target.Sack, VaultBag, It.Is<IEnumerable<Item>>(i => i.Single() == ring), true, "SomeMod|custommaps", null), Times.Once);
	}

	[Fact]
	public void Move_WithoutRoomForEveryItem_MovesWhatFits_AndReportsTheRest()
	{
		var source = this.Vault("Source");
		var big = Put(source.GetSack(0)!, NewItem("records/item/big.dbr", 4, 4), 0, 0, VaultPlace(source, 0));
		var tooBig = Put(source.GetSack(1)!, NewItem("records/item/big2.dbr", 2, 2), 0, 0, VaultPlace(source, 1));
		var target = this._service.VaultTarget(this.Vault("Target"), 0, VaultBag)!;

		var outcome = this._service.Move([big, tooBig], target);

		outcome.Done.Should().Equal(big);
		outcome.Skipped.Should().Equal(new ResultsMoveSkipped(tooBig, ResultsMoveSkip.NoRoom));
		source.GetSack(1)!.Should().ContainSingle().Which.Should().BeSameAs(tooBig, "an item without room stays where it was");
		tooBig.Place.Name.Should().Be("Source");
		source.GetSack(1)!.IsModified.Should().BeFalse();
	}

	[Fact]
	public void Move_FromAReadOnlyCharacter_SkipsItsItems()
	{
		var player = this.Character();
		player.GetSack(0)!.IsReadOnly = true;
		var ring = Put(player.GetSack(0)!, NewItem(), 0, 0, new ContainerPlace { Path = player.PlayerFile, Name = "HeroA", SackType = SackType.Player });
		var target = this._service.VaultTarget(this.Vault("Target"), 0, VaultBag)!;

		var outcome = this._service.Move([ring], target);

		outcome.Done.Should().BeEmpty();
		outcome.Skipped.Should().Equal(new ResultsMoveSkipped(ring, ResultsMoveSkip.ReadOnly));
		player.GetSack(0)!.Should().Contain(ring);
		outcome.Batch.Should().BeNull("nothing moved, nothing to undo");
	}

	[Fact]
	public void Move_ToTheRelicVault_TakesRelicsOnly()
	{
		var source = this.Vault("Source");
		var relic = Put(source.GetSack(0)!, NewItem("records/item/relics/ember.dbr"), 0, 0, VaultPlace(source, 0));
		var ring = Put(source.GetSack(0)!, NewItem(), 1, 0, VaultPlace(source, 0));
		var target = this._service.StashTarget(this.SharedStash(StashType.RelicVaultStash), StashType.RelicVaultStash)!;

		this._service.Accepts([relic], StashType.RelicVaultStash).Should().BeTrue();
		this._service.Accepts([relic, ring], StashType.RelicVaultStash).Should().BeFalse("the menu entry needs relics or charms only");
		this._service.Accepts([relic, ring], StashType.TransferStash).Should().BeTrue();

		var outcome = this._service.Move([relic, ring], target);

		outcome.Done.Should().Equal(relic);
		outcome.Skipped.Should().Equal(new ResultsMoveSkipped(ring, ResultsMoveSkip.NotAccepted));
		target.Place.StashType.Should().Be(StashType.RelicVaultStash);
	}

	[Fact]
	public void Move_ItemAlreadyInTheTargetOrNoLongerThere_IsSkipped()
	{
		var vault = this.Vault("Target");
		var there = Put(vault.GetSack(0)!, NewItem(), 0, 0, VaultPlace(vault, 0));
		var gone = NewItem();
		gone.Place = VaultPlace(vault, 1);
		var target = this._service.VaultTarget(vault, 0, VaultBag)!;

		var outcome = this._service.Move([there, gone], target);

		outcome.Skipped.Should().Equal(
			new ResultsMoveSkipped(there, ResultsMoveSkip.AlreadyThere),
			new ResultsMoveSkipped(gone, ResultsMoveSkip.NotFound));
	}

	[Fact]
	public void Move_IntoAReadOnlyCharactersStash_MovesNothing()
	{
		var source = this.Vault("Source");
		var ring = Put(source.GetSack(0)!, NewItem(), 0, 0, VaultPlace(source, 0));
		var stash = new Stash("HeroA", "winsys.dxb") { Width = 4, Height = 4 };
		stash.CreateEmptySack();
		stash.IsReadOnly = true;
		var target = this._service.StashTarget(stash, StashType.PlayerStash)!;

		var outcome = this._service.Move([ring], target);

		outcome.Skipped.Should().Equal(new ResultsMoveSkipped(ring, ResultsMoveSkip.TargetReadOnly));
		source.GetSack(0)!.Should().Contain(ring);
	}

	[Fact]
	public void Move_AWornPiece_LeavesAnEmptyItemInItsSlot_AndNamesTheWearer()
	{
		var player = this.Character();
		var helm = Wear(player, 0, NewItem("records/item/helm.dbr", 2, 2));
		var target = this._service.StashTarget(this.SharedStash(StashType.TransferStash), StashType.TransferStash)!;

		var outcome = this._service.Move([helm], target);

		outcome.Done.Should().Equal(helm);
		ItemKey.IsEmptyItem(player.EquipmentSack!.GetItem(0)).Should().BeTrue();
		player.EquipmentSack!.Count.Should().Be(12, "every slot keeps its entry");
		outcome.Wearers.Should().Equal(player);
		target.Sack.Should().Contain(helm);
	}

	[Fact]
	public void Move_AWornPieceWithTheEquipmentLocked_IsSkipped()
	{
		this._settings.PlayerReadonly = true;
		var player = this.Character();
		var helm = Wear(player, 0, NewItem("records/item/helm.dbr", 2, 2));
		var target = this._service.StashTarget(this.SharedStash(StashType.TransferStash), StashType.TransferStash)!;

		var outcome = this._service.Move([helm], target);

		outcome.Skipped.Should().Equal(new ResultsMoveSkipped(helm, ResultsMoveSkip.EquipmentLocked));
		player.EquipmentSack!.GetItem(0).Should().BeSameAs(helm);
	}

	[Fact]
	public void Move_ALoadoutPiece_LeavesItsLoadoutSlotEmpty_InTheFile()
	{
		var (set, ring) = this.LoadoutWithRing();
		var target = this._service.VaultTarget(this.Vault("Target"), 0, VaultBag)!;

		var outcome = this._service.Move([ring], target);

		outcome.Done.Should().Equal(ring);
		outcome.LoadoutsChanged.Should().Equal(set);
		target.Sack.Should().Contain(ring);
		ring.Place.SackType.Should().Be(SackType.Vault);
		ring.Place.Loadout.Should().BeNull("the row no longer names a loadout");
		set.Loadouts[0].Pieces[EquipmentRules.Ring1Slot].Should().BeNull();
		this._loadouts.Read(this._folder, "HeroA", null).Loadouts[0].Items.Should().BeEmpty("the file was written");
	}

	[Fact]
	public void Move_ALoadoutPieceOfAReadOnlyCharacter_IsSkipped()
	{
		var (set, ring) = this.LoadoutWithRing();
		this._readOnly.Setup(r => r.IsReadOnly(this._folder)).Returns(true);
		var target = this._service.VaultTarget(this.Vault("Target"), 0, VaultBag)!;

		var outcome = this._service.Move([ring], target);

		outcome.Skipped.Should().Equal(new ResultsMoveSkipped(ring, ResultsMoveSkip.ReadOnly));
		set.Loadouts[0].Pieces[EquipmentRules.Ring1Slot].Should().BeSameAs(ring);
	}

	/// <summary>A loadout "A" whose ring slot holds a ring, read back from its file as the session has it.</summary>
	private (LoadoutSet Set, Item Ring) LoadoutWithRing()
	{
		var player = this.Character();
		Wear(player, EquipmentRules.Ring1Slot, NewItem());
		this._loadouts.Add(this._loadouts.Read(this._folder, "HeroA", null), "A", player).IsSuccess.Should().BeTrue();
		var set = this._loadouts.Read(this._folder, "HeroA", null);
		var ring = set.Loadouts[0].Pieces[EquipmentRules.Ring1Slot]!;
		// As the search database places it.
		ring.Place = new ContainerPlace { Path = set.FilePath, Name = "HeroA", SackType = SackType.Loadout, Loadout = "A" };
		return (set, ring);
	}

	#endregion

	#region Undo

	[Fact]
	public void Undo_PutsEachItemBackAtItsCellAndPlace()
	{
		var source = this.Vault("Source");
		var ring = Put(source.GetSack(1)!, NewItem(), 3, 2, VaultPlace(source, 1));
		var target = this._service.VaultTarget(this.Vault("Target"), 0, VaultBag)!;
		var batch = this._service.Move([ring], target).Batch!;

		var outcome = this._service.Undo(batch);

		outcome.Done.Should().Equal(ring);
		target.Sack.Should().BeEmpty();
		source.GetSack(1)!.Should().ContainSingle().Which.Should().BeSameAs(ring);
		ring.Location.Should().Be(new Point(3, 2));
		ring.Place.Name.Should().Be("Source");
		ring.Place.SackNumber.Should().Be(1);
	}

	[Fact]
	public void Undo_ItsCellTakenSince_LeavesTheItemInTheTarget()
	{
		var source = this.Vault("Source");
		var ring = Put(source.GetSack(0)!, NewItem(), 0, 0, VaultPlace(source, 0));
		var target = this._service.VaultTarget(this.Vault("Target"), 0, VaultBag)!;
		var batch = this._service.Move([ring], target).Batch!;
		Put(source.GetSack(0)!, NewItem("records/item/other.dbr"), 0, 0, VaultPlace(source, 0));

		var outcome = this._service.Undo(batch);

		outcome.Skipped.Should().Equal(new ResultsMoveSkipped(ring, ResultsMoveSkip.NoRoom));
		target.Sack.Should().Contain(ring);
		ring.Place.Name.Should().Be("Target");
	}

	[Fact]
	public void Undo_AnItemMovedAgainSince_IsLeftWhereItIs()
	{
		var source = this.Vault("Source");
		var ring = Put(source.GetSack(0)!, NewItem(), 0, 0, VaultPlace(source, 0));
		var vault = this.Vault("Target");
		var batch = this._service.Move([ring], this._service.VaultTarget(vault, 0, VaultBag)!).Batch!;
		this._service.Move([ring], this._service.VaultTarget(vault, 1, VaultBag)!);

		var outcome = this._service.Undo(batch);

		outcome.Skipped.Should().Equal(new ResultsMoveSkipped(ring, ResultsMoveSkip.NotFound));
		vault.GetSack(1)!.Should().Contain(ring);
	}

	[Fact]
	public void Undo_AWornPiece_GoesBackInItsSlot()
	{
		var player = this.Character();
		var helm = Wear(player, 0, NewItem("records/item/helm.dbr", 2, 2));
		var target = this._service.StashTarget(this.SharedStash(StashType.TransferStash), StashType.TransferStash)!;
		var batch = this._service.Move([helm], target).Batch!;

		var outcome = this._service.Undo(batch);

		outcome.Done.Should().Equal(helm);
		outcome.Wearers.Should().Equal(player);
		player.EquipmentSack!.GetItem(0).Should().BeSameAs(helm);
		player.EquipmentSack!.Count.Should().Be(12);
		helm.Location.Should().Be(SackCollection.GetEquipmentLocationOffset(0));
		helm.Place.SackType.Should().Be(SackType.Equipment);
		target.Sack.Should().BeEmpty();
	}

	[Fact]
	public void Undo_ALoadoutPiece_GoesBackInItsLoadoutSlot_InTheFile()
	{
		var (set, ring) = this.LoadoutWithRing();
		var target = this._service.VaultTarget(this.Vault("Target"), 0, VaultBag)!;
		var batch = this._service.Move([ring], target).Batch!;

		var outcome = this._service.Undo(batch);

		outcome.Done.Should().Equal(ring);
		outcome.LoadoutsChanged.Should().Equal(set);
		target.Sack.Should().BeEmpty();
		set.Loadouts[0].Pieces[EquipmentRules.Ring1Slot].Should().BeSameAs(ring);
		ring.Location.Should().Be(new Point(-1, -1));
		this._loadouts.Read(this._folder, "HeroA", null).Loadouts[0].Items.Should().ContainSingle("the file was written");
	}

	[Fact]
	public void Undo_AnItemReadWithTheTargetsMap_IsReadAgainWithItsOwn()
	{
		var source = this.Vault("Source");
		var ring = Put(source.GetSack(0)!, NewItem(), 0, 0, VaultPlace(source, 0));
		var target = this._service.VaultTarget(this.Vault("Target"), 0, VaultBag)!;
		var batch = this._service.Move([ring], target).Batch!;
		// What SettleAll does in a modded vault.
		(ring.Mod, ring.ModOrigin) = ("SomeMod|custommaps", ModOrigin.Inherited);

		var outcome = this._service.Undo(batch);

		outcome.ReadAgain.Should().Equal(ring);
		ring.Mod.Should().BeNull();
		this._itemProvider.Verify(p => p.GetDBData(ring), Times.Once);
	}

	#endregion
}
