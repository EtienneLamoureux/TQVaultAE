using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Services;

/// <summary>
/// The item edits offered by the sack context menu: affixes, completion bonuses, relics, seed, copies.
/// </summary>
public class ItemEditService(IDatabase database, IItemProvider itemProvider, IItemModService? itemMods = null) : IItemEditService
{
	/// <summary>Shards given by the "complete relic" entry; the item caps it to its completion level.</summary>
	private const int CompleteRelicNumber = 10;

	private readonly IDatabase _database = database;
	private readonly IItemProvider _itemProvider = itemProvider;
	private readonly IItemModService? _itemMods = itemMods;

	/// <summary>The database of the item's mod for the current thread, until disposed.</summary>
	private IDisposable ModScope(Item item) => this._itemMods?.Scope(item) ?? NoScope.Instance;

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
	public ModChoiceOutcome ChangeMod(SackCollection sack, Item item, string? modName, Func<Item, bool>? fits = null)
	{
		string? mod = Normalize(modName);
		if (item.HasOwnMod && SameMod(item.Mod, mod))
			return ModChoiceOutcome.Unchanged;

		// Chosen for the item: its own map from now on, kept wherever it goes and saved in the vault file.
		var previous = (item.Mod, item.ModOrigin);
		item.Mod = mod;
		item.ModOrigin = ModOrigin.Own;

		if (!SameMod(previous.Mod, mod))
		{
			this._itemProvider.GetDBData(item);

			// Read with other data, the item may take another size: without room for it, the choice is refused rather
			// than saving items that overlap. The user moves the item to another tab first.
			if (fits is not null && !fits(item))
			{
				(item.Mod, item.ModOrigin) = previous;
				this._itemProvider.GetDBData(item);
				return ModChoiceOutcome.NoRoom;
			}
		}

		MarkModified(sack, item);
		return ModChoiceOutcome.Changed;
	}

	/// <inheritdoc/>
	public VaultModChange ChangeVaultMod(PlayerCollection vault, string? modName, Func<SackCollection, Item, bool>? fits = null)
	{
		string? mod = Normalize(modName);
		if (SameMod(vault.Mod, mod))
			return VaultModChange.None;

		vault.Mod = mod;
		var changed = new List<Item>();
		var kept = new List<Item>();
		foreach (var sack in vault)
		{
			if (sack is null)
				continue;

			// The items inheriting the vault's map take the new one; own maps are kept. A copy of the list: fits may move
			// an item to another bag.
			foreach (var item in sack.Where(i => !i.HasOwnMod).ToList())
			{
				var previous = item.Mod;
				item.Mod = mod;
				item.ModOrigin = ModOrigin.Inherited;
				item.IsModified = true;
				this._itemProvider.GetDBData(item);

				// Read with other data, the item may take another size: without room for it anywhere, it keeps its map,
				// as its own, rather than being saved over its neighbours.
				if (fits is not null && !fits(sack, item))
				{
					(item.Mod, item.ModOrigin) = (previous, ModOrigin.Own);
					this._itemProvider.GetDBData(item);
					kept.Add(item);
					continue;
				}

				changed.Add(item);
			}

			sack.IsModified = true;
		}

		return new VaultModChange(changed, kept);
	}

	/// <inheritdoc/>
	public SettleResult SettleAll(SackCollection sack, System.Drawing.Size sackSize, IEnumerable<Item> items, bool inVault, string? containerMod, Func<SackCollection, Item, bool>? elsewhere = null)
	{
		var readAgain = new List<Item>();
		var kept = new List<Item>();
		foreach (var item in items.ToList())
		{
			var outcome = this.SettleIn(item, inVault, containerMod
				, i => TQVaultAE.Domain.Actions.SackLayout.FitsOrMoves(sack, sackSize, i) || (elsewhere?.Invoke(sack, i) ?? false));
			if (outcome == SettleOutcome.ReadAgain)
				readAgain.Add(item);
			else if (outcome == SettleOutcome.KeptOwnMap)
				kept.Add(item);
		}

		return new SettleResult(readAgain, kept);
	}

	/// <inheritdoc/>
	public SettleOutcome SettleIn(Item item, bool inVault, string? containerMod, Func<Item, bool> fits)
	{
		bool leavesCharacter = inVault && !item.InVault;
		item.InVault = inVault;

		// An own map goes with the item.
		if (item.HasOwnMod)
			return SettleOutcome.Unchanged;

		// Leaving a character (or a game stash) for a vault makes the map explicit: the item keeps the data the game
		// reads it with.
		if (leavesCharacter && item.ModOrigin == ModOrigin.Inherited)
		{
			item.ModOrigin = ModOrigin.Own;
			item.IsModified = true;
			return SettleOutcome.Unchanged;
		}

		// Else the item inherits the map of its new container.
		string? mod = Normalize(containerMod);
		var previous = (item.Mod, item.ModOrigin);
		item.Mod = mod;
		item.ModOrigin = ModOrigin.Inherited;
		if (previous.ModOrigin != ModOrigin.None && SameMod(previous.Mod, mod))
			return SettleOutcome.Unchanged;

		item.IsModified = true;
		this._itemProvider.GetDBData(item);

		// Read with the new map, the item may take its real size: it must fit where it lies, or at another free spot.
		// Without room for it, it keeps its previous map, as its own.
		if (!fits(item))
		{
			(item.Mod, item.ModOrigin) = (previous.Mod, ModOrigin.Own);
			this._itemProvider.GetDBData(item);
			return SettleOutcome.KeptOwnMap;
		}

		return SettleOutcome.ReadAgain;
	}

	/// <inheritdoc/>
	public bool HasRoomOnceSettled(Item item, bool inVault, string? containerMod, Func<Item, bool> fits)
	{
		// The same rules as SettleIn: an own map goes with the item, and an item leaving a character for a vault keeps
		// its map; only an item taking another map is read with other data.
		string? mod = Normalize(containerMod);
		if (item.HasOwnMod
			|| (inVault && !item.InVault && item.ModOrigin == ModOrigin.Inherited)
			|| (item.ModOrigin != ModOrigin.None && SameMod(item.Mod, mod)))
		{
			return true;
		}

		var previous = (item.Mod, item.ModOrigin);
		(item.Mod, item.ModOrigin) = (mod, ModOrigin.Inherited);
		this._itemProvider.GetDBData(item);
		try
		{
			return fits(item);
		}
		finally
		{
			(item.Mod, item.ModOrigin) = previous;
			this._itemProvider.GetDBData(item);
		}
	}

	private static string? Normalize(string? modName) => string.IsNullOrWhiteSpace(modName) ? null : modName;

	private static bool SameMod(string? a, string? b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

	/// <summary>Rolls a completion bonus from the bonus table of a relic, charm or artifact.</summary>
	private RecordId? RollBonus(Item item)
	{
		float roll = (float)Item.GenerateSeed() / Item.MaxSeed;
		return this._itemProvider.BonusTableRelicOrArtifact(item)?.PickWeighted(roll);
	}

	private static void MarkModified(SackCollection sack, Item item)
	{
		item.IsModified = true;
		sack.IsModified = true;
	}
}
