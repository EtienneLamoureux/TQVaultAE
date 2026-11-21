using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Results;

/// <summary>
/// What a stash panel bag shows: the equipment panel, or one stash sack with its size in cells.
/// </summary>
/// <param name="BagId">The bag id (see <see cref="BagIdConstants"/>).</param>
/// <param name="Sack">The sack shown; null when there is no character or the stash has no sack.</param>
/// <param name="Width">The sack width in cells (0 for the equipment panel).</param>
/// <param name="Height">The sack height in cells (0 for the equipment panel).</param>
/// <param name="CenteringHeight">The height, in cells, used to center the sack in the panel.</param>
public sealed record StashBag(int BagId, SackCollection? Sack, int Width, int Height, int CenteringHeight)
{
	/// <summary>
	/// Gets whether the bag is the equipment panel.
	/// </summary>
	public bool IsEquipment => this.BagId == BagIdConstants.BAGID_EQUIPMENTPANEL;

	/// <summary>
	/// Gets the free space, in cells, around the sack in a panel of the given size: draw the sack that many half cells
	/// from the panel origin to center it.
	/// </summary>
	/// <param name="maxWidth">The panel width in cells.</param>
	/// <param name="maxHeight">The panel height in cells.</param>
	/// <returns>The free cells on each axis, never negative.</returns>
	public (int X, int Y) CenteringMargin(int maxWidth, int maxHeight)
		=> (Math.Max(0, maxWidth - this.Width), Math.Max(0, maxHeight - this.CenteringHeight));
}
