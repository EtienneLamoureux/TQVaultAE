using System.Drawing;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

/// <summary>
/// <see cref="ItemMovementService.Pack"/> and <see cref="ItemMovementService.MoveItems"/>: the cell maths of the
/// sack sort button and of the selection moved with Alt+W/A/S/D.
/// </summary>
public class ItemMovementServiceSortTests
{
	private readonly ItemMovementService _service = new(new Mock<ILogger<ItemMovementService>>().Object);

	private static Item Item(string name, int x, int y, int width, int height) => new()
	{
		BaseItemId = RecordId.Create($"records/item/{name}.dbr"),
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
		sack.IsModified = false;
		return sack;
	}

	private static string Positions(SackCollection sack)
		=> string.Join(" ", sack.Select(i => $"{Name(i)}@{i.PositionX},{i.PositionY}"));

	/// <summary>The name given to <see cref="Item(string, int, int, int, int)"/>: the record file name.</summary>
	private static string Name(Item item)
	{
		var file = item.BaseItemId.Raw.Split('/', '\\')[^1];
		return file[..file.LastIndexOf('.')];
	}

	#region Pack

	[Fact]
	public void Pack_Horizontal_PacksRowByRowInTheGivenOrder()
	{
		// Arrange: a 4x4 sack, items scattered
		var small = Item("small", 3, 3, 1, 1);
		var wide = Item("wide", 0, 3, 2, 1);
		var big = Item("big", 2, 0, 2, 2);
		var sack = Sack(small, wide, big);

		// Act
		var packed = this._service.Pack(sack, [big, wide, small], new Size(4, 4), vertical: false);

		// Assert
		packed.Should().BeTrue();
		Positions(sack).Should().Be("big@0,0 wide@2,0 small@2,1");
		sack.IsModified.Should().BeTrue();
	}

	[Fact]
	public void Pack_Vertical_PacksColumnByColumn()
	{
		var small = Item("small", 3, 3, 1, 1);
		var tall = Item("tall", 0, 2, 1, 2);
		var big = Item("big", 2, 0, 2, 2);
		var sack = Sack(small, tall, big);

		this._service.Pack(sack, [big, tall, small], new Size(4, 4), vertical: true).Should().BeTrue();

		Positions(sack).Should().Be("big@0,0 tall@0,2 small@1,2");
	}

	[Fact]
	public void Pack_ItemDoesNotFit_PutsTheSameItemsBackWhereTheyWere()
	{
		// Arrange: the 2x2 item fills the 2x2 sack, the small one finds no room
		var small = Item("small", 2, 0, 1, 1);
		var big = Item("big", 0, 0, 2, 2);
		var sack = Sack(small, big);

		// Act
		var packed = this._service.Pack(sack, [big, small], new Size(2, 2), vertical: true);

		// Assert
		packed.Should().BeFalse();
		Positions(sack).Should().Be("small@2,0 big@0,0");
		sack.Should().Contain(big, "the search and the panels know these items, not copies");
		sack.Should().Contain(small);
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void Pack_ItemDoesNotFit_KeepsWhetherTheSackHadChangesToSave(bool modified)
	{
		// Arrange: a failed sort must not forget an item dropped in the sack before it, nor invent a change
		var small = Item("small", 2, 0, 1, 1);
		var big = Item("big", 0, 0, 2, 2);
		var sack = Sack(small, big);
		sack.IsModified = modified;

		// Act
		this._service.Pack(sack, [big, small], new Size(2, 2), vertical: true).Should().BeFalse();

		// Assert
		sack.IsModified.Should().Be(modified);
	}

	[Fact]
	public void Pack_EmptySack_DoesNothing()
	{
		var sack = Sack();

		this._service.Pack(sack, [], new Size(4, 4), vertical: false).Should().BeTrue();

		sack.IsModified.Should().BeFalse();
	}

	#endregion

	#region MoveItems

	[Fact]
	public void MoveItems_FreeWay_MovesEverySelectedItem()
	{
		var a = Item("a", 0, 0, 1, 1);
		var b = Item("b", 1, 0, 1, 2);
		var sack = Sack(a, b);

		this._service.MoveItems(sack, [a, b], new Size(4, 4), new Point(1, 1)).Should().BeTrue();

		Positions(sack).Should().Be("a@1,1 b@2,1");
	}

	[Fact]
	public void MoveItems_SelectedItemsMayTakeEachOthersPlace()
	{
		var a = Item("a", 0, 0, 1, 1);
		var b = Item("b", 1, 0, 1, 1);
		var sack = Sack(a, b);

		this._service.MoveItems(sack, [a, b], new Size(3, 1), new Point(1, 0)).Should().BeTrue();

		Positions(sack).Should().Be("a@1,0 b@2,0");
	}

	[Fact]
	public void MoveItems_UnselectedItemInTheWay_MovesNothing()
	{
		var a = Item("a", 0, 0, 1, 1);
		var b = Item("b", 0, 2, 1, 1);
		var blocker = Item("blocker", 1, 2, 1, 1);
		var sack = Sack(a, b, blocker);

		this._service.MoveItems(sack, [a, b], new Size(4, 4), new Point(1, 0)).Should().BeFalse();

		Positions(sack).Should().Be("a@0,0 b@0,2 blocker@1,2");
	}

	[Theory]
	[InlineData(-1, 0)]
	[InlineData(0, -1)]
	[InlineData(1, 0)]
	[InlineData(0, 1)]
	public void MoveItems_LeavingTheSack_MovesNothing(int dx, int dy)
	{
		// A 2x2 item in a 3x2 sack, against the edge it is pushed to.
		var item = Item("item", dx > 0 ? 1 : 0, 0, 2, 2);
		var sack = Sack(item);
		int startX = item.PositionX;

		this._service.MoveItems(sack, [item], new Size(3, 2), new Point(dx, dy)).Should().BeFalse();

		item.PositionX.Should().Be(startX);
		item.PositionY.Should().Be(0);
	}

	#endregion
}
