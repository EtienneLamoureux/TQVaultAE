using System.Drawing;
using AwesomeAssertions;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Domain;

/// <summary>
/// <see cref="SackLayout"/>: whether an item has room where it lies in a sack, and the free spot it moves to.
/// </summary>
public class SackLayoutTests
{
	private static readonly Size Grid = new(4, 3);

	private static Item Item(int x, int y, int width, int height) => new()
	{
		BaseItemId = RecordId.Create("records/item/x.dbr"),
		PositionX = x,
		PositionY = y,
		Width = width,
		Height = height,
	};

	private static SackCollection Sack(params Item[] items)
	{
		var sack = new SackCollection();
		foreach (var item in items)
			sack.AddItem(item);
		return sack;
	}

	[Fact]
	public void Covering_ItemsInsideTheGameSize_IsTheGameSize()
		=> SackLayout.Covering(Grid, [Sack(Item(3, 2, 1, 1)), null]).Should().Be(Grid);

	[Fact]
	public void Covering_ItemsBeyondTheGameSize_GrowsToHoldThemInEverySack()
		=> SackLayout.Covering(Grid, [Sack(Item(5, 0, 2, 1)), Sack(Item(0, 6, 1, 3))]).Should().Be(new Size(7, 9));

	[Fact]
	public void Covering_NoGameRoom_HoldsTheItemsLeftThere()
		=> SackLayout.Covering(Size.Empty, [Sack(Item(1, 1, 2, 3))]).Should().Be(new Size(3, 4));

	[Fact]
	public void Covering_NoGameRoomAndNoItem_StaysEmpty()
		=> SackLayout.Covering(Size.Empty, [Sack()]).Should().Be(Size.Empty);

	[Theory]
	[InlineData(0, 0, 1, 1, true)]
	[InlineData(3, 2, 1, 1, true)]
	[InlineData(2, 1, 2, 2, true)]
	[InlineData(3, 0, 2, 1, false)]
	[InlineData(0, 2, 1, 2, false)]
	[InlineData(-1, 0, 1, 1, false)]
	public void Fits_AloneInTheSack_FollowsTheGrid(int x, int y, int width, int height, bool expected)
	{
		var item = Item(x, y, width, height);

		SackLayout.Fits(Sack(item), Grid, item).Should().Be(expected);
	}

	[Fact]
	public void Fits_OverANeighbour_IsFalse()
	{
		var item = Item(0, 0, 2, 2);
		var sack = Sack(item, Item(1, 1, 1, 1));

		SackLayout.Fits(sack, Grid, item).Should().BeFalse();
	}

	[Fact]
	public void Fits_NextToANeighbour_IsTrue()
	{
		var item = Item(0, 0, 2, 2);
		var sack = Sack(item, Item(2, 0, 1, 1), Item(0, 2, 1, 1));

		SackLayout.Fits(sack, Grid, item).Should().BeTrue();
	}

	[Fact]
	public void Fits_OverTheItemASwapPicksUp_IsTrue()
	{
		// Arrange: a drop on an item swaps with it; that item is still in the sack when the dropped one is checked
		var dropped = Item(0, 0, 2, 2);
		var pickedUp = Item(1, 1, 1, 1);
		var sack = Sack(dropped, pickedUp);

		SackLayout.Fits(sack, Grid, dropped, leaving: pickedUp).Should().BeTrue();
	}

	[Fact]
	public void MoveToFreeSpot_TakesTheFirstFreeSpotRowsFirst()
	{
		// Arrange: the top left 2x2 is taken, a 2x2 item lies over it
		var item = Item(0, 0, 2, 2);
		var sack = Sack(Item(0, 0, 2, 2), item);

		// Act
		var moved = SackLayout.MoveToFreeSpot(sack, Grid, item);

		// Assert
		moved.Should().BeTrue();
		(item.PositionX, item.PositionY).Should().Be((2, 0));
	}

	[Fact]
	public void MoveToFreeSpot_NoRoom_LeavesTheItemWhereItWas()
	{
		var item = Item(1, 1, 2, 2);
		var sack = Sack(Item(0, 0, 4, 2), item);

		SackLayout.MoveToFreeSpot(sack, Grid, item).Should().BeFalse();

		(item.PositionX, item.PositionY).Should().Be((1, 1));
	}

	[Fact]
	public void FitsOrMoves_ItemThatFits_StaysInPlace()
	{
		var item = Item(2, 1, 1, 1);

		SackLayout.FitsOrMoves(Sack(Item(0, 0, 1, 1), item), Grid, item).Should().BeTrue();

		(item.PositionX, item.PositionY).Should().Be((2, 1));
	}

	[Theory]
	[InlineData(3, 0)]   // room at the cell
	[InlineData(0, 0)]   // over the neighbour: a free spot in column 3
	public void HasRoomAt_RoomAtTheCellOrAFreeSpot_LeavesTheItemWhereItWas(int x, int y)
	{
		// Arrange: an item held elsewhere (not in the sack), 1x3; the sack has a 3x3 block
		var item = Item(9, 9, 1, 3);
		var sack = Sack(Item(0, 0, 3, 3));

		// Act and assert
		SackLayout.HasRoomAt(sack, Grid, item, new Point(x, y)).Should().BeTrue();
		(item.PositionX, item.PositionY).Should().Be((9, 9));
	}

	[Fact]
	public void HasRoomAt_NoRoomInTheSack_IsFalse()
	{
		var item = Item(9, 9, 2, 3);
		var sack = Sack(Item(0, 0, 3, 3));

		SackLayout.HasRoomAt(sack, Grid, item, new Point(2, 0)).Should().BeFalse();
		(item.PositionX, item.PositionY).Should().Be((9, 9));
	}

	[Fact]
	public void HasRoomAt_OverTheItemASwapPicksUp_IsTrue()
	{
		var leaving = Item(0, 0, 4, 3);
		var item = Item(9, 9, 2, 2);

		SackLayout.HasRoomAt(Sack(leaving), Grid, item, new Point(0, 0), leaving).Should().BeTrue();
	}

	[Fact]
	public void ShowItemsWithoutRoom_ItemsOverAKeptOneOrOutOfTheGrid_AreShown1x1_TheOthersKeepTheirSize()
	{
		// Arrange: read with another map, the sword grew over the ring, and the lance out of the grid
		var sword = Item(0, 0, 2, 2);
		var ring = Item(1, 1, 1, 1);
		var lance = Item(3, 1, 1, 5);
		var helm = Item(2, 0, 1, 1);

		// Act
		var noRoom = SackLayout.ShowItemsWithoutRoom(Sack(sword, ring, lance, helm), Grid);

		// Assert: the first in the sack keeps its place; 1x1 the others, where they lie
		noRoom.Should().Equal(ring, lance);
		(sword.LacksRoom, helm.LacksRoom).Should().Be((false, false));
		(ring.Width, ring.Height, lance.Width, lance.Height, lance.RealHeight).Should().Be((1, 1, 1, 1, 5));
	}

	[Fact]
	public void ShowItemsWithoutRoom_ItemThatHasRoomAgain_GetsItsRealSizeBack()
	{
		var lance = Item(3, 0, 1, 3);
		lance.ShowNoRoom();

		SackLayout.ShowItemsWithoutRoom(Sack(lance), Grid).Should().BeEmpty();

		(lance.LacksRoom, lance.Height).Should().Be((false, 3));
	}

	[Fact]
	public void Fits_ItemShown1x1_TakesOneCell()
	{
		// The lance's real 1x3 would cover the ring: shown 1x1, it does not.
		var lance = Item(0, 0, 1, 3);
		lance.ShowNoRoom();
		var ring = Item(0, 1, 1, 1);

		SackLayout.Fits(Sack(lance, ring), Grid, ring).Should().BeTrue();
	}
}
