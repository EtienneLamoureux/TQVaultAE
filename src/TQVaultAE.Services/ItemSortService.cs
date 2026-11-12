using System.Drawing;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Services;

/// <summary>
/// The sort button of a bag: one order per <see cref="SackSortAlgorithm"/>, the optional merge of relic and charm shards
/// through <see cref="IItemStackingService"/>, the packing through <see cref="IItemMovementService.Pack"/>.
/// </summary>
public class ItemSortService(
	IItemMovementService itemMovementService
	, IItemStackingService itemStackingService
	, IItemProvider itemProvider
	, IItemDatabaseService itemDatabaseService
) : IItemSortService
{
	/// <summary>The order of one algorithm, given the required level of an item.</summary>
	private delegate IOrderedEnumerable<Item> SortOrder(IEnumerable<Item> items, Func<Item, int> levelOf);

	/// <summary>
	/// The strategy of each algorithm. Every order ends with the keys of the normal sort, so items of the same kind stay
	/// together and the result does not depend on where the items were.
	/// </summary>
	private static readonly Dictionary<SackSortAlgorithm, SortOrder> Orders = new Dictionary<SackSortAlgorithm, SortOrder>
	{
		[SackSortAlgorithm.Normal] = (items, _) => ThenNormal(items.OrderByDescending(SizeKey)),
		[SackSortAlgorithm.Level] = (items, levelOf) => ThenNormal(items.OrderBy(levelOf)),
		[SackSortAlgorithm.Type] = (items, levelOf) => ThenNormal(items.OrderByDescending(item => item.ItemGroup).ThenBy(levelOf)),
		[SackSortAlgorithm.Rarity] = (items, levelOf) => ThenNormal(items.OrderByDescending(item => item.Rarity).ThenByDescending(item => item.ItemGroup).ThenBy(levelOf)),
	};

	private readonly IItemMovementService _itemMovementService = itemMovementService;
	private readonly IItemStackingService _itemStackingService = itemStackingService;
	private readonly IItemProvider _itemProvider = itemProvider;
	private readonly IItemDatabaseService _itemDatabaseService = itemDatabaseService;

	/// <summary>The largest items first: three times the height plus the width.</summary>
	private static int SizeKey(Item item) => (item.Height * 3) + item.Width;

	/// <summary>The keys of the normal sort: size, group, record, completed relics before shards.</summary>
	private static IOrderedEnumerable<Item> ThenNormal(IOrderedEnumerable<Item> items)
		=> items
			.ThenByDescending(SizeKey)
			.ThenByDescending(item => item.ItemGroup)
			.ThenBy(item => item.BaseItemId)
			.ThenByDescending(item => item.IsRelicComplete);

	/// <summary>
	/// The items in the order of <paramref name="algorithm"/>, the first placed first; an unknown algorithm sorts as
	/// <see cref="SackSortAlgorithm.Normal"/>.
	/// </summary>
	public IReadOnlyList<Item> Order(IEnumerable<Item> items, SackSortAlgorithm algorithm)
	{
		var order = Orders.TryGetValue(algorithm, out var found) ? found : Orders[SackSortAlgorithm.Normal];
		return order(items, this.LevelRequirement).ToList();
	}

	private int LevelRequirement(Item item) => this._itemProvider.GetRequirementInfo(item).Lvl ?? 0;

	/// <inheritdoc/>
	public SackSortResult Sort(SackCollection sack, Size sackSize, bool vertical, SackSortAlgorithm algorithm, bool mergeRelics)
	{
		// A read-only character's bag is never sorted nor merged.
		if (sack.IsReadOnly)
			return SackSortResult.ReadOnlyRefused;

		if (sack.IsEmpty)
			return SackSortResult.SortedOnly;

		// What to put back when the sort fails after a merge: the items in their order and place, the sack as modified as it was.
		var before = sack.Select(item => (Item: item, item.PositionX, item.PositionY)).ToList();
		bool wasModified = sack.IsModified;

		var merge = mergeRelics ? this.MergeShards(sack) : null;

		if (!this._itemMovementService.Pack(sack, this.Order(sack, algorithm), sackSize, vertical))
		{
			// The packing put the merged sack back; the merge is undone too.
			if (merge is { HasChanges: true })
			{
				merge.Undo(this._itemProvider);
				sack.EmptySack();
				foreach (var (item, x, y) in before)
				{
					item.PositionX = x;
					item.PositionY = y;
					sack.AddItem(item);
				}

				sack.IsModified = wasModified;
			}

			return SackSortResult.NotSorted;
		}

		if (merge is not { HasChanges: true })
			return SackSortResult.SortedOnly;

		// The emptied shards leave the search; the others are read again (number, completion bonus).
		foreach (var item in merge.Removed)
			this._itemDatabaseService.RemoveItemFromDatabase(item);

		var changed = merge.Changed(sack);
		this._itemProvider.InvalidateFriendlyNamesCache(changed);
		this._itemDatabaseService.RefreshItems(changed);

		return new SackSortResult(true, changed, merge.Removed);
	}

	/// <summary>
	/// Stacks the incomplete relics and charms of the same record, the fullest first, as when the user drops one on
	/// another: the shards join up to completion (a completed relic rolls its bonus), an emptied one leaves the sack,
	/// the rest of the shards stay where they were.
	/// </summary>
	private ShardMerge MergeShards(SackCollection sack)
	{
		var merge = new ShardMerge();
		var groups = sack
			.Where(item => item.IsRelicOrCharm && !item.IsRelicComplete)
			.GroupBy(item => item.BaseItemId)
			.Where(group => group.Skip(1).Any())
			.Select(group => group.OrderByDescending(item => item.Number).ThenBy(item => item.PositionY).ThenBy(item => item.PositionX).ToList())
			.ToList();

		foreach (var shards in groups)
		{
			foreach (var item in shards)
				merge.Remember(item);

			var target = shards[0];
			foreach (var source in shards.Skip(1))
			{
				if (target.IsRelicComplete)
				{
					target = source;
					continue;
				}

				// The stacking takes the dropped item out of its sack first, like a drag.
				sack.RemoveItem(source);
				var stack = this._itemStackingService.TryStack(source, target, sack, target.Location, disableAutoStacking: false);
				if (stack.Stacked && stack.PickedUpItem is null)
				{
					// Every shard joined.
					merge.Removed.Add(source);
					continue;
				}

				// The target is complete (or did not stack): the rest of the shards stay where they were.
				sack.AddItem(source);
				target = source;
			}
		}

		return merge;
	}

	/// <summary>The shards a merge touched, to read what changed or put them back.</summary>
	private sealed class ShardMerge
	{
		private readonly List<(Item Item, int Var1, RecordId RelicBonusId, bool IsModified)> _before = [];

		/// <summary>The shards emptied into another one.</summary>
		public List<Item> Removed { get; } = [];

		public bool HasChanges => this.Removed.Count > 0 || this._before.Any(b => b.Item.Var1 != b.Var1);

		public void Remember(Item item) => this._before.Add((item, item.Var1, item.RelicBonusId, item.IsModified));

		/// <summary>The shards still in <paramref name="sack"/> with another number than before.</summary>
		public List<Item> Changed(SackCollection sack)
			=> this._before.Where(b => b.Item.Var1 != b.Var1 && sack.IndexOfItem(b.Item) >= 0).Select(b => b.Item).ToList();

		/// <summary>Gives every touched shard its number, bonus and modified flag back.</summary>
		public void Undo(IItemProvider itemProvider)
		{
			foreach (var (item, var1, relicBonusId, isModified) in this._before)
			{
				bool completed = item.IsRelicComplete;
				item.Var1 = var1;
				item.RelicBonusId = relicBonusId;
				item.IsModified = isModified;

				// The completion read the bonus record: read the item again without it.
				if (completed)
					itemProvider.GetDBData(item);
			}
		}
	}
}
