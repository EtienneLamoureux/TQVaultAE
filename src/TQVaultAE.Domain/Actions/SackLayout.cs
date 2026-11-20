using System.Drawing;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Domain.Actions;

/// <summary>
/// The cells of a sack: whether an item has room where it lies, and where it would have room.
/// </summary>
/// <remarks>
/// An item read again with other data (a mod's, the base game's) may take another size. These checks tell whether it
/// still lies inside the grid without covering a neighbour, and find it another place when it does not.
/// </remarks>
public static class SackLayout
{
	/// <summary>
	/// True when an item of a sack lies inside its grid without overlapping another item of the sack.
	/// </summary>
	/// <param name="sack">The sack holding the item.</param>
	/// <param name="sackSize">The size of the sack's grid, in cells.</param>
	/// <param name="item">The item, at its position and size.</param>
	/// <param name="leaving">An item of the sack to ignore: the one a swap drop is about to pick up.</param>
	public static bool Fits(SackCollection sack, Size sackSize, Item item, Item? leaving = null)
	{
		var cells = new Rectangle(item.PositionX, item.PositionY, item.Width, item.Height);
		if (cells.X < 0 || cells.Y < 0 || cells.Right > sackSize.Width || cells.Bottom > sackSize.Height)
			return false;

		return !sack.Any(other => !ReferenceEquals(other, item) && !ReferenceEquals(other, leaving)
			&& new Rectangle(other.PositionX, other.PositionY, other.Width, other.Height).IntersectsWith(cells));
	}

	/// <summary>
	/// Moves an item of a sack to the first free spot of its grid (rows first) where it fits.
	/// </summary>
	/// <returns>True when moved; false when there is no room, the item staying where it was.</returns>
	public static bool MoveToFreeSpot(SackCollection sack, Size sackSize, Item item)
	{
		var (x0, y0) = (item.PositionX, item.PositionY);
		for (int y = 0; y + item.Height <= sackSize.Height; y++)
		{
			for (int x = 0; x + item.Width <= sackSize.Width; x++)
			{
				(item.PositionX, item.PositionY) = (x, y);
				if (Fits(sack, sackSize, item))
					return true;
			}
		}

		(item.PositionX, item.PositionY) = (x0, y0);
		return false;
	}

	/// <summary>
	/// True when the item has room in the sack: where it lies, or at the free spot it is then moved to.
	/// </summary>
	public static bool FitsOrMoves(SackCollection sack, Size sackSize, Item item)
		=> Fits(sack, sackSize, item) || MoveToFreeSpot(sack, sackSize, item);

	/// <summary>
	/// True when the item, put at a cell of the sack, would have room: there, or at a free spot of the grid. The item
	/// is left where it was.
	/// </summary>
	/// <param name="sack">The sack the item would go to.</param>
	/// <param name="sackSize">The size of the sack's grid, in cells.</param>
	/// <param name="item">The item, at its size.</param>
	/// <param name="cell">The cell it would be put at.</param>
	/// <param name="leaving">An item of the sack to ignore at that cell: the one a swap drop is about to pick up.</param>
	public static bool HasRoomAt(SackCollection sack, Size sackSize, Item item, Point cell, Item? leaving = null)
	{
		var (x0, y0) = (item.PositionX, item.PositionY);
		(item.PositionX, item.PositionY) = (cell.X, cell.Y);
		bool room = Fits(sack, sackSize, item, leaving) || MoveToFreeSpot(sack, sackSize, item);
		(item.PositionX, item.PositionY) = (x0, y0);
		return room;
	}

	/// <summary>
	/// The size of a grid holding every item of <paramref name="sacks"/> where it lies, at least <paramref name="gameSize"/>:
	/// a save may keep items beyond the size the game data gives (a character played once with an inventory mod).
	/// </summary>
	public static Size Covering(Size gameSize, IEnumerable<SackCollection?> sacks)
	{
		int width = gameSize.Width, height = gameSize.Height;
		foreach (var item in sacks.OfType<SackCollection>().SelectMany(sack => sack))
		{
			if (item.PositionX < 0 || item.PositionY < 0 || RecordId.IsNullOrEmpty(item.BaseItemId))
				continue;

			width = Math.Max(width, item.PositionX + item.Width);
			height = Math.Max(height, item.PositionY + item.Height);
		}

		return new Size(width, height);
	}

	/// <summary>
	/// The size of a vault bag, in cells.
	/// </summary>
	public static readonly Size VaultBagSize = new(18, 20);

	/// <summary>
	/// Shows 1 x 1 (<see cref="Item.ShowNoRoom"/>) the items of a sack whose real size has no room where they lie: out
	/// of the grid, or over an item already kept, in the sack's order. Read again with another map (a link changed since
	/// the file was saved), an item may take a size its place cannot hold. The others get their real size back.
	/// </summary>
	/// <returns>The items now shown 1 x 1.</returns>
	public static IReadOnlyList<Item> ShowItemsWithoutRoom(SackCollection sack, Size sackSize)
	{
		var kept = new List<Rectangle>();
		var noRoom = new List<Item>();
		var grid = new Rectangle(Point.Empty, sackSize);
		foreach (var item in sack)
		{
			item.ClearNoRoom();
			var cells = new Rectangle(item.PositionX, item.PositionY, item.Width, item.Height);
			if (!grid.Contains(cells) || kept.Any(other => other.IntersectsWith(cells)))
			{
				item.ShowNoRoom();
				noRoom.Add(item);
				cells = new Rectangle(item.PositionX, item.PositionY, 1, 1);
			}

			kept.Add(cells);
		}

		return noRoom;
	}
}
