using System.Drawing;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Services;

/// <summary>
/// The sort button of a bag: one order per <see cref="SackSortAlgorithm"/>, the packing through
/// <see cref="IItemMovementService.Pack"/>.
/// </summary>
public class ItemSortService(
	IItemMovementService itemMovementService
	, IItemProvider itemProvider
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
	private readonly IItemProvider _itemProvider = itemProvider;

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
	public bool Sort(SackCollection sack, Size sackSize, bool vertical, SackSortAlgorithm algorithm)
		=> sack.IsEmpty || this._itemMovementService.Pack(sack, this.Order(sack, algorithm), sackSize, vertical);
}
