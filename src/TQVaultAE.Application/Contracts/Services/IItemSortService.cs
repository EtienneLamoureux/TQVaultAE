using System.Drawing;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Contracts.Services;

/// <summary>
/// The sort button of a bag: puts the items of a sack in the order of a <see cref="SackSortAlgorithm"/>, packed row by row
/// or column by column.
/// </summary>
public interface IItemSortService
{
	/// <summary>
	/// Sorts the items of <paramref name="sack"/>.
	/// </summary>
	/// <remarks>
	/// When an item does not fit in the new order, nothing changes: the same items at their place, the sack as modified
	/// as it was.
	/// </remarks>
	/// <param name="sack">The sack to sort.</param>
	/// <param name="sackSize">Size of the sack in cells.</param>
	/// <param name="vertical"><see langword="true"/> to fill column by column, otherwise row by row.</param>
	/// <param name="algorithm">The order of the items.</param>
	/// <returns><see langword="false"/> when an item did not fit and the sack was left as it was.</returns>
	bool Sort(SackCollection sack, Size sackSize, bool vertical, SackSortAlgorithm algorithm);
}
