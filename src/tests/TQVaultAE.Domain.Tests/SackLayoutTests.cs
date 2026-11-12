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
}
