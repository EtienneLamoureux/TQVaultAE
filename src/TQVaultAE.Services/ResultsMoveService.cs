using System.Drawing;
using Microsoft.Extensions.Logging;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Results;
using TQVaultAE.Application.Session;
using TQVaultAE.Config;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Services;

/// <inheritdoc cref="IResultsMoveService"/>
public class ResultsMoveService(
	ILogger<ResultsMoveService> log
	, SessionContext session
	, IItemMovementService itemMovementService
	, IItemEditService itemEditService
	, ILoadoutService loadoutService
	, ICharacterReadOnlyService characterReadOnlyService
	, IItemProvider itemProvider
	, IGameMapService gameMapService
	, IPathIO pathIO
	, UserSettings userSettings
) : IResultsMoveService
{
	/// <summary>A size no bag reaches: the cell an item is put back at was its own, inside its bag.</summary>
	private static readonly Size AnyBag = new(1000, 1000);

	private readonly ILogger _log = log;
	private readonly SessionContext _session = session;
	private readonly IItemMovementService _itemMovementService = itemMovementService;
	private readonly IItemEditService _itemEditService = itemEditService;
	private readonly ILoadoutService _loadoutService = loadoutService;
	private readonly ICharacterReadOnlyService _characterReadOnlyService = characterReadOnlyService;
	private readonly IItemProvider _itemProvider = itemProvider;
	private readonly IGameMapService _gameMapService = gameMapService;
	private readonly IPathIO _pathIO = pathIO;
	private readonly UserSettings _userSettings = userSettings;

	/// <inheritdoc/>
	public ResultsMoveTarget? StashTarget(Stash? stash, StashType stashType)
		=> stash?.Sack is { } sack
			? new(sack, new Size(stash.Width, stash.Height), this._itemMovementService.PlaceIn(stash, stashType), this._gameMapService.ActiveCharacterMod)
			: null;

	/// <inheritdoc/>
	public ResultsMoveTarget? VaultTarget(PlayerCollection vault, int bag, Size bagSize)
		=> vault.GetSack(bag) is { } sack
			? new(sack, bagSize, this._itemMovementService.PlaceIn(vault, SackType.Vault, bag), vault.Mod)
			: null;

	/// <inheritdoc/>
	public bool Accepts(IEnumerable<Item> items, StashType stashType)
		=> items.All(item => this._itemMovementService.IsItemValidForStashType(item, stashType));

	/// <inheritdoc/>
	public ResultsMoveOutcome Move(IReadOnlyList<Item> items, ResultsMoveTarget target)
	{
		var moved = new List<Item>();
		var skipped = new List<ResultsMoveSkipped>();
		var origins = new List<ResultsMoveOrigin>();
		var changedSacks = new List<SackCollection> { target.Sack };

		foreach (var item in items.Distinct<Item>(ReferenceEqualityComparer.Instance))
		{
			if (target.Sack.IsReadOnly)
			{
				skipped.Add(new(item, ResultsMoveSkip.TargetReadOnly));
				continue;
			}

			var (origin, refused) = this.Locate(item, target);
			if (origin is null)
			{
				skipped.Add(new(item, refused));
				continue;
			}

			// The item is taken out of its bag (an equipped one leaves an empty item in its slot) once its cells are found.
			Action takeOut = origin.Sack is { } sack ? () => EquipmentRules.TakeOut(sack, item) : () => { };
			if (!this._itemMovementService.TryPlace(item, target.Sack, target.SackSize, target.Place, takeOut))
			{
				skipped.Add(new(item, ResultsMoveSkip.NoRoom));
				continue;
			}

			moved.Add(item);
			origins.Add(origin);
			if (origin.Sack is not null)
				changedSacks.Add(origin.Sack);
		}

		// The loadout pieces leave their loadouts in one write per file; without it, they go back.
		var loadoutsChanged = new List<LoadoutSet>();
		foreach (var group in origins.Where(o => o.Loadouts is not null).GroupBy(o => o.Loadouts!).ToList())
		{
			var change = this._loadoutService.TakePieces(group.Key, [.. group.Select(o => o.Item)]);
			if (change.IsSuccess)
			{
				loadoutsChanged.Add(group.Key);
				continue;
			}

			this._log.LogError(change.Error, "Results move: the loadouts {File} were not written", group.Key.FilePath);
			foreach (var origin in group)
			{
				target.Sack.RemoveItem(origin.Item);
				RestoreState(origin);
				moved.Remove(origin.Item);
				origins.Remove(origin);
				skipped.Add(new(origin.Item, ResultsMoveSkip.LoadoutNotWritten));
			}
		}

		// Like items put there by Move To: they take the map of their new place.
		var settled = this._itemEditService.SettleAll(target.Sack, target.SackSize, moved, target.ContainerMod);

		this._log.LogInformation("Results move: {Moved} item(s) to {Name} sack {SackNumber}, {Skipped} skipped",
			moved.Count, target.Place.Name, target.Place.SackNumber, skipped.Count);

		return new(moved, skipped, settled.ReadAgain, settled.NoRoom, Wearers(origins), loadoutsChanged
			, origins.Count == 0 ? null : new ResultsMoveBatch(target, origins))
		{
			ChangedSacks = [.. changedSacks.Distinct()],
		};
	}

	/// <inheritdoc/>
	public ResultsMoveOutcome Undo(ResultsMoveBatch batch)
	{
		var target = batch.Target.Sack;
		var done = new List<Item>();
		var skipped = new List<ResultsMoveSkipped>();
		var readAgain = new List<Item>();
		var restored = new List<ResultsMoveOrigin>();
		var changedSacks = new List<SackCollection> { target };
		var pieces = new List<ResultsMoveOrigin>();

		// The last moved first: each finds its place as it left it.
		foreach (var origin in batch.Origins.Reverse())
		{
			var item = origin.Item;
			var refused = this.CanPutBack(origin, target);
			if (refused is not null)
			{
				skipped.Add(new(item, refused.Value));
				continue;
			}

			if (origin.Loadouts is not null)
			{
				pieces.Add(origin);
				continue;
			}

			var sack = origin.Sack!;
			target.RemoveItem(item);
			if (origin.EquipmentSlot >= 0)
			{
				sack.RemoveAtItem(origin.EquipmentSlot);
				sack.InsertItem(origin.EquipmentSlot, item);
			}
			else
			{
				sack.AddItem(item);
			}

			if (RestoreState(origin))
				readAgain.Add(item);

			done.Add(item);
			restored.Add(origin);
			changedSacks.Add(sack);
		}

		// Loadout pieces go back in one write per file.
		var loadoutsChanged = new List<LoadoutSet>();
		foreach (var group in pieces.GroupBy(o => o.Loadouts!).ToList())
		{
			var (change, putBack) = this._loadoutService.PutBackPieces(group.Key, [.. group.Select(o => new LoadoutPiecePlacement(o.LoadoutSlot!.Value, o.Item))]);
			if (!change.IsSuccess)
				this._log.LogError(change.Error, "Results undo: the loadouts {File} were not written", group.Key.FilePath);

			var back = new HashSet<Item>(putBack, ReferenceEqualityComparer.Instance);
			foreach (var origin in group)
			{
				if (!back.Contains(origin.Item))
				{
					skipped.Add(new(origin.Item, change.IsSuccess ? ResultsMoveSkip.NoRoom : ResultsMoveSkip.LoadoutNotWritten));
					continue;
				}

				target.RemoveItem(origin.Item);
				if (this.RestoreMap(origin))
					readAgain.Add(origin.Item);
				done.Add(origin.Item);
			}

			if (back.Count > 0)
				loadoutsChanged.Add(group.Key);
		}

		this._log.LogInformation("Results undo: {Done} item(s) put back, {Skipped} left", done.Count, skipped.Count);

		return new(done, skipped, readAgain, [], Wearers(restored), loadoutsChanged, null)
		{
			ChangedSacks = [.. changedSacks.Distinct()],
		};
	}

	/// <summary>
	/// Finds where an item is, as the search placed it, and whether it may leave: its origin, or why it stays.
	/// </summary>
	private (ResultsMoveOrigin? Origin, ResultsMoveSkip Refused) Locate(Item item, ResultsMoveTarget target)
	{
		var place = item.Place;
		if (place.SackType == SackType.Loadout)
			return this.LocatePiece(item);

		var sack = this.SackAt(place);
		if (sack is null || sack.IndexOfItem(item) < 0)
			return (null, ResultsMoveSkip.NotFound);

		if (ReferenceEquals(sack, target.Sack))
			return (null, ResultsMoveSkip.AlreadyThere);

		if (sack.IsReadOnly)
			return (null, ResultsMoveSkip.ReadOnly);

		bool worn = place.SackType == SackType.Equipment;
		if (worn && this._userSettings.PlayerReadonly)
			return (null, ResultsMoveSkip.EquipmentLocked);

		if (!this._itemMovementService.IsItemValidForStashType(item, target.Sack.StashType))
			return (null, ResultsMoveSkip.NotAccepted);

		return (new ResultsMoveOrigin
		{
			Item = item,
			Sack = sack,
			EquipmentSlot = worn ? EquipmentRules.FindSlot(sack, item) : -1,
			Wearer = worn ? this.Container(this._session.Players, place.Path) : null,
			Cell = item.Location,
			Place = Snapshot(place),
			ContainerMod = item.ContainerMod,
			HasContainerMod = item.HasContainerMod,
			LacksRoom = item.LacksRoom,
		}, default);
	}

	/// <summary>Finds a loadout piece in the loadouts of its character.</summary>
	private (ResultsMoveOrigin? Origin, ResultsMoveSkip Refused) LocatePiece(Item item)
	{
		if (!this._session.Loadouts.TryGetValue(item.Place.Path, out var loadouts))
			return (null, ResultsMoveSkip.NotFound);

		for (int index = 0; index < loadouts.Loadouts.Count; index++)
		{
			var loadout = loadouts.Loadouts[index];
			int slot = loadout.Pieces.ToList().FindIndex(p => ReferenceEquals(p, item));
			if (slot < 0)
				continue;

			if (loadouts.ReadFailed || this.CharacterReadOnly(loadouts))
				return (null, ResultsMoveSkip.ReadOnly);

			return (new ResultsMoveOrigin
			{
				Item = item,
				Loadouts = loadouts,
				LoadoutSlot = new LoadoutPieceSlot(loadout.Name, slot),
				Cell = item.Location,
				Place = Snapshot(item.Place),
				ContainerMod = item.ContainerMod,
				HasContainerMod = item.HasContainerMod,
				LacksRoom = item.LacksRoom,
			}, default);
		}

		return (null, ResultsMoveSkip.NotFound);
	}

	/// <summary>Why an item cannot go back where it was, or null when it can.</summary>
	private ResultsMoveSkip? CanPutBack(ResultsMoveOrigin origin, SackCollection target)
	{
		var item = origin.Item;
		if (target.IndexOfItem(item) < 0)
			return ResultsMoveSkip.NotFound;

		if (origin.Loadouts is { } loadouts)
		{
			// The loadouts read again (another session's change) are other objects: the piece has no slot there.
			if (!this._session.Loadouts.TryGetValue(loadouts.FilePath, out var current) || !ReferenceEquals(current, loadouts))
				return ResultsMoveSkip.NotFound;
			if (loadouts.ReadFailed || this.CharacterReadOnly(loadouts))
				return ResultsMoveSkip.ReadOnly;
			return null;
		}

		// The container read again (file watcher) or closed: its bag is another object.
		var sack = origin.Sack!;
		if (!ReferenceEquals(this.SackAt(origin.Place), sack))
			return ResultsMoveSkip.NotFound;
		if (sack.IsReadOnly)
			return ResultsMoveSkip.ReadOnly;

		if (origin.EquipmentSlot >= 0)
		{
			if (this._userSettings.PlayerReadonly)
				return ResultsMoveSkip.EquipmentLocked;
			return origin.EquipmentSlot < sack.Count && ItemKey.IsEmptyItem(sack.GetItem(origin.EquipmentSlot))
				? null
				: ResultsMoveSkip.NoRoom;
		}

		// Its own cells, unless another item took them since.
		var cell = item.Location;
		item.Location = origin.Cell;
		bool free = SackLayout.Fits(sack, AnyBag, item);
		item.Location = cell;
		return free ? null : ResultsMoveSkip.NoRoom;
	}

	/// <summary>
	/// Gives an item back its cell, place and map. Returns whether it was read again with its previous map.
	/// </summary>
	private bool RestoreState(ResultsMoveOrigin origin)
	{
		origin.Item.Location = origin.Cell;
		origin.Item.Place.CopyFrom(origin.Place);
		return this.RestoreMap(origin);
	}

	/// <summary>Gives an item back the map it was read with, and its size; returns whether it was read again.</summary>
	private bool RestoreMap(ResultsMoveOrigin origin)
	{
		var item = origin.Item;
		bool resized = item.LacksRoom != origin.LacksRoom;
		if (origin.LacksRoom)
			item.ShowNoRoom();
		else
			item.ClearNoRoom();

		if (item.HasContainerMod == origin.HasContainerMod && string.Equals(item.ContainerMod, origin.ContainerMod, StringComparison.OrdinalIgnoreCase))
			return resized;

		if (origin.HasContainerMod)
			item.SetContainerMod(origin.ContainerMod);
		else
			item.ClearContainerMod();

		this._itemProvider.GetDBData(item);
		return true;
	}

	/// <summary>
	/// The bag of a place, in the containers of the session: a vault bag, a character's bag or equipment, a stash.
	/// </summary>
	private SackCollection? SackAt(ContainerPlace place)
	{
		if (place.StashType is not null || place.SackType == SackType.Stash)
			return this.Container(this._session.Stashes, place.Path)?.Sack;

		return place.SackType switch
		{
			SackType.Vault => this.Container(this._session.Vaults, place.Path)?.GetSack(place.SackNumber),
			SackType.Player => this.Container(this._session.Players, place.Path)?.GetSack(place.SackNumber),
			SackType.Equipment => this.Container(this._session.Players, place.Path)?.EquipmentSack,
			_ => null,
		};
	}

	/// <summary>A container already loaded in the session; none is loaded here.</summary>
	private T? Container<T>(Domain.Helpers.LazyConcurrentDictionary<string, T> containers, string path)
		where T : class
		=> containers.TryGetValue(path, out var lazy) && lazy.IsValueCreated ? lazy.Value : null;

	/// <summary>Whether the character of a loadouts file is read-only (its files on the disk).</summary>
	private bool CharacterReadOnly(LoadoutSet loadouts)
		=> this._characterReadOnlyService.IsReadOnly(this._pathIO.GetDirectoryName(loadouts.FilePath));

	private static ContainerPlace Snapshot(ContainerPlace place)
	{
		var copy = new ContainerPlace();
		copy.CopyFrom(place);
		return copy;
	}

	private static List<PlayerCollection> Wearers(IEnumerable<ResultsMoveOrigin> origins)
		=> [.. origins.Select(o => o.Wearer).OfType<PlayerCollection>().Distinct()];
}
