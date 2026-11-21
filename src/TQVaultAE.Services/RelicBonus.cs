using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Services;

/// <summary>
/// The completion bonus a relic, charm or artifact gets when it is completed.
/// </summary>
internal static class RelicBonus
{
	/// <summary>
	/// Rolls a bonus from the item's bonus table, by weight like the game; null when it has none.
	/// </summary>
	public static RecordId? Roll(IItemProvider itemProvider, Item item)
	{
		float roll = (float)Item.GenerateSeed() / Item.MaxSeed;
		return itemProvider.BonusTableRelicOrArtifact(item)?.PickWeighted(roll);
	}
}
