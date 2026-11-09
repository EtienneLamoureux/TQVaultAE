using TQVaultAE.Domain.Comparison;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Results;

namespace TQVaultAE.Application.Contracts.Services;

/// <summary>
/// Two items side by side: what each tooltip shows and how their stats differ.
/// </summary>
/// <param name="First">The texts of the first item, for its classic tooltip display.</param>
/// <param name="Second">The texts of the second item, for its classic tooltip display.</param>
/// <param name="Stats">The stat differences, both ways.</param>
public sealed record ItemComparison(ToFriendlyNameResult First, ToFriendlyNameResult Second, ItemStatComparison Stats);

/// <summary>
/// Compares two items stat by stat, from the values their tooltips display (they follow "Show In-Game Values").
/// </summary>
public interface IItemComparisonService
{
	/// <summary>Compares <paramref name="first"/> with <paramref name="second"/>.</summary>
	ItemComparison Compare(Item first, Item second);
}
