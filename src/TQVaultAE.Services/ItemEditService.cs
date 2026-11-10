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

		if (relic is null || !relic.IsRelicOrCharm)
		{
			// The slot content is gone with the slot: only the sack records the change.
			sack.IsModified = true;
			return null;
		}

		return relic;
	}

	/// <inheritdoc/>
	public Item? Split(Item item)
		=> item.PopAllButOneItem();

	/// <inheritdoc/>
	public Item Copy(Item item)
		=> item.Duplicate(false);

	/// <inheritdoc/>
	public Item Duplicate(Item item)
		=> item.Duplicate(true);

	/// <inheritdoc/>
	public Item NewSetMember(Item setItem, RecordId memberId)
	{
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
	public bool ChangeMod(SackCollection sack, Item item, string? modName)
	{
		string? mod = string.IsNullOrWhiteSpace(modName) ? null : modName;
		if (string.Equals(item.Mod, mod, StringComparison.OrdinalIgnoreCase) && !item.ModFromVault)
			return false;

		// Chosen for the item: its own link from now on.
		item.Mod = mod;
		item.ModFromVault = false;
		this._itemProvider.GetDBData(item);
		MarkModified(sack, item);
		return true;
	}

	/// <inheritdoc/>
	public IReadOnlyList<Item> ChangeVaultMod(PlayerCollection vault, string? modName)
	{
		string? mod = string.IsNullOrWhiteSpace(modName) ? null : modName;
		string? previous = vault.Mod;
		if (string.Equals(previous, mod, StringComparison.OrdinalIgnoreCase))
			return [];

		vault.Mod = mod;
		var changed = new List<Item>();
		foreach (var sack in vault)
		{
			if (sack is null)
				continue;

			// The items following the vault's link, or without a link, follow the new one; the items having their own
			// link keep it.
			foreach (var item in sack.Where(i => i.Mod is null || i.ModFromVault))
			{
				bool wasIncomplete = item.IsIncomplete;
				item.Mod = mod;
				item.ModFromVault = mod is not null;
				this._itemProvider.GetDBData(item);

				// An item whose missing records the mod all resolves belongs to that mod: it gets its own link, which it
				// keeps when the vault's link changes or when it leaves the vault.
				if (mod is not null && wasIncomplete && !item.IsIncomplete)
					item.ModFromVault = false;

				item.IsModified = true;
				changed.Add(item);
			}

			sack.IsModified = true;
		}

		return changed;
	}

	/// <inheritdoc/>
	public bool AdoptContainerMod(Item item, string? containerMod, Func<Item, bool> fits)
	{
		// Only an item without a link of its own follows the vault's.
		if (string.IsNullOrWhiteSpace(containerMod) || (item.Mod is not null && !item.ModFromVault)
			|| (item.ModFromVault && string.Equals(item.Mod, containerMod, StringComparison.OrdinalIgnoreCase)))
			return false;

		var (previousMod, previousFromVault) = (item.Mod, item.ModFromVault);
		bool wasIncomplete = item.IsIncomplete;

		item.Mod = containerMod;
		item.ModFromVault = true;
		this._itemProvider.GetDBData(item);

		// Read with the mod's data, the item may get its real size: it must still fit where it was dropped.
		if (!fits(item))
		{
			(item.Mod, item.ModFromVault) = (previousMod, previousFromVault);
			this._itemProvider.GetDBData(item);
			return false;
		}

		// The vault's mod resolves all its missing records: the item belongs to that mod.
		if (wasIncomplete && !item.IsIncomplete)
			item.ModFromVault = false;

		item.IsModified = true;
		return true;
	}

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
