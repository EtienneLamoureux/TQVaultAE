using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Session;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Services;

/// <summary>
/// The item edits offered by the sack context menu: affixes, completion bonuses, relics, seed, copies.
/// </summary>
public class ItemEditService(IDatabase database, IItemProvider itemProvider, IItemModService itemMods) : IItemEditService
{
	/// <summary>Shards given by the "complete relic" entry; the item caps it to its completion level.</summary>
	private const int CompleteRelicNumber = 10;

	private readonly IDatabase _database = database;
	private readonly IItemProvider _itemProvider = itemProvider;
	private readonly IItemModService _itemMods = itemMods;

	/// <summary>The database of the item's mod for the current thread, until disposed.</summary>
	private IDisposable ModScope(Item item) => this._itemMods.ScopeOrNone(item);

	/// <inheritdoc/>
	public bool ChangePrefix(SackCollection sack, Item item, RecordId affixId)
	{
		using var modScope = this.ModScope(item);
		if (affixId == item.prefixID)
			return false;

		item.prefixID = affixId;
		item.prefixInfo = this._database.GetInfo(affixId);
		MarkModified(sack, item);
		return true;
	}

	/// <inheritdoc/>
	public bool ChangeSuffix(SackCollection sack, Item item, RecordId affixId)
	{
		using var modScope = this.ModScope(item);
		if (affixId == item.suffixID)
			return false;

		item.suffixID = affixId;
		item.suffixInfo = this._database.GetInfo(affixId);
		MarkModified(sack, item);
		return true;
	}

	/// <inheritdoc/>
	public void RemovePrefix(SackCollection sack, Item item)
	{
		item.prefixID = RecordId.Empty;
		item.prefixInfo = null;
		MarkModified(sack, item);
	}

	/// <inheritdoc/>
	public void RemoveSuffix(SackCollection sack, Item item)
	{
		item.suffixID = RecordId.Empty;
		item.suffixInfo = null;
		MarkModified(sack, item);
	}

	/// <inheritdoc/>
	public bool ChangeCompletionBonus(SackCollection sack, Item item, RecordId bonusId, bool secondRelic = false)
	{
		using var modScope = this.ModScope(item);
		var currentId = secondRelic ? item.RelicBonus2Id : item.RelicBonusId;
		if (bonusId == currentId)
			return false;

		if (secondRelic)
		{
			item.RelicBonus2Id = bonusId;
			item.RelicBonus2Info = this._database.GetInfo(bonusId);
		}
		else
		{
			item.RelicBonusId = bonusId;
			item.RelicBonusInfo = this._database.GetInfo(bonusId);
		}

		MarkModified(sack, item);
		return true;
	}

	/// <inheritdoc/>
	public void CompleteRelic(SackCollection sack, Item item)
	{
		item.Number = CompleteRelicNumber;

		RecordId? bonusId = this.RollBonus(item);
		if (bonusId is not null)
			item.RelicBonusId = bonusId;

		this._itemProvider.GetDBData(item);

		MarkModified(sack, item);
	}

	/// <inheritdoc/>
	public Item? CraftArtifact(Item formula)
	{
		Item? artifact = this._itemProvider.CraftArtifact(formula);
		if (artifact is null)
			return null;

		RecordId? bonusId = this.RollBonus(artifact);
		if (bonusId is not null)
		{
			using var modScope = this.ModScope(artifact);
			artifact.RelicBonusId = bonusId;
			artifact.RelicBonusInfo = this._database.GetInfo(bonusId);
			artifact.IsModified = true;
		}

		return artifact;
	}

	/// <inheritdoc/>
	public Item? RemoveRelic(SackCollection sack, Item item, bool secondSlot)
	{
		Item? relic = secondSlot
			? this._itemProvider.RemoveRelic2(item)
			: this._itemProvider.RemoveRelic1(item);

		// The item lost its slot content either way.
		sack.IsModified = true;

		// Anything but a relic or charm is gone with the slot.
		return relic is not null && relic.IsRelicOrCharm ? relic : null;
	}

	/// <inheritdoc/>
	public Item? Split(SackCollection sack, Item item)
	{
		Item? rest = item.PopAllButOneItem();
		if (rest is not null)
			sack.IsModified = true;

		return rest;
	}

	/// <inheritdoc/>
	public Item Copy(Item item)
		=> item.Duplicate(false);

	/// <inheritdoc/>
	public Item Duplicate(Item item)
		=> item.Duplicate(true);

	/// <inheritdoc/>
	public Item NewSetMember(Item setItem, RecordId memberId)
	{
		// A piece of the same set comes from the same data: the copy has the set item's map.
		Item member = setItem.MakeEmptyCopy(memberId);
		this._itemProvider.GetDBData(member);
		return member;
	}

	/// <inheritdoc/>
	public bool ForceSeed(SackCollection sack, Item item)
	{
		int originalSeed = item.Seed;
		item.Seed = Item.GenerateSeed();
		item.IsModified = true;

		if (item.Seed == originalSeed)
			return false;

		sack.IsModified = true;
		return true;
	}

	/// <inheritdoc/>
	public bool SetSeed(SackCollection sack, Item item, int seed)
	{
		if (!Item.IsValidSeed(seed) || item.Seed == seed)
			return false;

		item.Seed = seed;
		MarkModified(sack, item);
		return true;
	}

	/// <inheritdoc/>
	public VaultModChange ChangeVaultMod(PlayerCollection vault, string? modName, Func<SackCollection, Item, bool>? fits = null)
	{
		string? mod = Normalize(modName);
		if (SameMod(vault.Mod, mod))
			return VaultModChange.None;

		vault.Mod = mod;
		var changed = new List<Item>();
		var noRoom = new List<Item>();
		foreach (var sack in vault)
		{
			if (sack is null)
				continue;

			// Every item takes the new map. A copy of the list: fits may move an item to another bag.
			foreach (var item in sack.ToList())
			{
				item.SetContainerMod(mod);
				item.ClearNoRoom();
				this._itemProvider.GetDBData(item);

				// Read with other data, the item may take another size: without room for it anywhere, it is shown 1 x 1
				// where it lies rather than over its neighbours, until it is moved where it fits.
				if (fits is not null && !fits(sack, item))
				{
					item.ShowNoRoom();
					noRoom.Add(item);
					continue;
				}

				changed.Add(item);
			}

			sack.IsModified = true;
		}

		return new VaultModChange(changed, noRoom);
	}

	/// <inheritdoc/>
	public SettleResult SettleAll(SackCollection sack, System.Drawing.Size sackSize, IEnumerable<Item> items, string? containerMod, Func<SackCollection, Item, bool>? elsewhere = null)
	{
		var readAgain = new List<Item>();
		var noRoom = new List<Item>();
		bool Fits(Item item) => TQVaultAE.Domain.Actions.SackLayout.FitsOrMoves(sack, sackSize, item) || (elsewhere?.Invoke(sack, item) ?? false);
		foreach (var item in items.ToList())
		{
			bool hadNoRoom = item.LacksRoom;
			item.ClearNoRoom();
			bool newMap = item.SetContainerMod(containerMod);
			if (newMap)
				this._itemProvider.GetDBData(item);

			// Already in the bag, at the size it had: without room for its new size, it is shown 1 x 1 where it lies.
			if (!Fits(item))
			{
				item.ShowNoRoom();
				noRoom.Add(item);
			}
			else if (newMap || hadNoRoom)
				readAgain.Add(item);
		}

		return new SettleResult(readAgain, noRoom);
	}

	/// <inheritdoc/>
	public SettleOutcome SettleIn(Item item, string? containerMod, Func<Item, bool> fits)
	{
		var previous = (item.ContainerMod, item.HasContainerMod);
		bool hadNoRoom = item.LacksRoom;
		item.ClearNoRoom();

		// The item takes the map of its new container; with the same map, it keeps its data.
		if (!item.SetContainerMod(containerMod))
		{
			if (!hadNoRoom)
				return SettleOutcome.Unchanged;

			// Shown 1 x 1, it gets its real size back where that fits; else it stays 1 x 1.
			if (fits(item))
				return SettleOutcome.ReadAgain;

			item.ShowNoRoom();
			return SettleOutcome.Unchanged;
		}

		this._itemProvider.GetDBData(item);

		// Read with the new map, the item may take another size: it must fit where it lies, or at another free spot.
		if (fits(item))
			return SettleOutcome.ReadAgain;

		this.Restore(item, previous, hadNoRoom);
		return SettleOutcome.NoRoom;
	}

	/// <inheritdoc/>
	public bool HasRoomOnceSettled(Item item, string? containerMod, Func<Item, bool> fits)
	{
		// The same rules as SettleIn: only an item taking another map is read with other data; one keeping its map
		// keeps its size there (1 x 1 when it lacks room).
		if (item.HasContainerMod && SameMod(item.ContainerMod, Normalize(containerMod)))
			return true;

		var previous = (item.ContainerMod, item.HasContainerMod);
		bool hadNoRoom = item.LacksRoom;
		item.ClearNoRoom();
		item.SetContainerMod(containerMod);
		this._itemProvider.GetDBData(item);
		try
		{
			return fits(item);
		}
		finally
		{
			this.Restore(item, previous, hadNoRoom);
		}
	}

	/// <summary>Puts an item back on the map it was read with, and its size.</summary>
	private void Restore(Item item, (string? Mod, bool Set) previous, bool lacksRoom)
	{
		if (previous.Set)
			item.SetContainerMod(previous.Mod);
		else
			item.ClearContainerMod();

		this._itemProvider.GetDBData(item);
		if (lacksRoom)
			item.ShowNoRoom();
	}

	private static string? Normalize(string? modName) => string.IsNullOrWhiteSpace(modName) ? null : modName;

	private static bool SameMod(string? a, string? b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

	/// <summary>Rolls a completion bonus from the bonus table of a relic, charm or artifact.</summary>
	private RecordId? RollBonus(Item item) => RelicBonus.Roll(this._itemProvider, item);

	private static void MarkModified(SackCollection sack, Item item)
	{
		item.IsModified = true;
		sack.IsModified = true;
	}
}
