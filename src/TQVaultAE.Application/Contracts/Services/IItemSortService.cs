using System.Drawing;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Contracts.Services;

/// <summary>
/// The sort button of a bag: puts the items of a sack in the order of a <see cref="SackSortAlgorithm"/>, packed row by row
/// or column by column, and may first stack the shards of the same relic or charm.
/// </summary>
public interface IItemSortService
{
	/// <summary>
	/// Sorts the items of <paramref name="sack"/>.
	/// </summary>
	/// <remarks>
	/// With <paramref name="mergeRelics"/>, the incomplete relics and charms of the same record are first stacked as when
	/// the user drops one on another: up to completion, a completed relic rolling its bonus. The shards that were
	/// emptied leave the sack and the search. When an item does not fit in the new order, nothing changes: the same
	/// items at their place, the shards as they were, the sack as modified as it was.
	/// </remarks>
	/// <param name="sack">The sack to sort.</param>
	/// <param name="sackSize">Size of the sack in cells.</param>
	/// <param name="vertical"><see langword="true"/> to fill column by column, otherwise row by row.</param>
	/// <param name="algorithm">The order of the items.</param>
	/// <param name="mergeRelics">Stack the shards of the same relic or charm first.</param>
	/// <returns>Whether the sack was sorted, and the items the merge changed or removed.</returns>
	SackSortResult Sort(SackCollection sack, Size sackSize, bool vertical, SackSortAlgorithm algorithm, bool mergeRelics);
}
