using System.Drawing;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Character;

/// <summary>
/// The caravan grid sizes in cells a map's data gives (<c>caravanwindow.dbr</c>: its storage, relic vault and transfer
/// windows), null for a size not found.
/// </summary>
/// <param name="Storage">The character's storage: <c>InventoryWidth</c> x the largest <c>InventoryHeightArray</c>.</param>
/// <param name="RelicVault">The relic vault, sized like the storage.</param>
/// <param name="Transfer">The transfer stash: its inventory grid's <c>inventoryXSize</c> x <c>inventoryYSize</c> in pixels, 32 a cell.</param>
public sealed record CaravanSizes(Size? Storage, Size? RelicVault, Size? Transfer)
{
	/// <summary>No size.</summary>
	public static CaravanSizes None { get; } = new(null, null, null);

	/// <summary>
	/// The sizes a mod sets: those of <paramref name="active"/> that differ from the base game's. The base game's own
	/// sizes are not used: a stash file holds its size (the storage grows with the rows bought in the game).
	/// </summary>
	public static CaravanSizes Overrides(CaravanSizes active, CaravanSizes baseGame)
	{
		static Size? Changed(Size? size, Size? baseSize) => size is { } s && s != baseSize ? s : null;

		return new(Changed(active.Storage, baseGame.Storage), Changed(active.RelicVault, baseGame.RelicVault),
			Changed(active.Transfer, baseGame.Transfer));
	}

	/// <summary>The size for a stash panel bag (<see cref="BagIdConstants"/>); null for the equipment or none.</summary>
	public Size? Of(int bagId) => bagId switch
	{
		BagIdConstants.BAGID_TRANSFERSTASH => this.Transfer,
		BagIdConstants.BAGID_PLAYERSTASH => this.Storage,
		BagIdConstants.BAGID_RELICVAULTSTASH => this.RelicVault,
		_ => null,
	};
}
