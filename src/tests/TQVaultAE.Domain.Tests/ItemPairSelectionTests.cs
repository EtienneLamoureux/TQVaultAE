using AwesomeAssertions;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Domain;

/// <summary>
/// <see cref="ItemPairSelection"/>: the "pick a second item" mode shared by the item panels (Compare, Insert into item).
/// </summary>
public class ItemPairSelectionTests
{
	private readonly ItemPairSelection _selection = new();
	private readonly Item _first = new();
	private readonly Item _second = new();

	[Fact]
	public void Pick_AnotherItem_GivesThePairInOrderAndEndsTheMode()
	{
		this._selection.Start(this._first);

		var pair = this._selection.Pick(this._second);

		pair.Should().NotBeNull();
		pair!.First.Should().BeSameAs(this._first);
		pair.Second.Should().BeSameAs(this._second);
		pair.Purpose.Should().Be(ItemPairPurpose.Compare);
		this._selection.IsActive.Should().BeFalse();
		this._selection.First.Should().BeNull();
	}

	[Fact]
	public void Pick_KeepsThePurposeAndSackTheModeStartedWith_ThenForgetsThem()
	{
		var sack = new SackCollection();
		this._selection.Start(this._first, ItemPairPurpose.SocketGameRules, sack);

		var pair = this._selection.Pick(this._second);

		pair!.Purpose.Should().Be(ItemPairPurpose.SocketGameRules);
		pair.FirstSack.Should().BeSameAs(sack);
		this._selection.FirstSack.Should().BeNull();
		this._selection.Purpose.Should().Be(ItemPairPurpose.Compare, "a later Compare must not inherit the socket purpose");
	}

	[Fact]
	public void Start_AgainWithAnotherPurpose_ReplacesTheFirstSelection()
	{
		this._selection.Start(this._first, ItemPairPurpose.SocketNoRules, new SackCollection());
		this._selection.Start(this._second);

		this._selection.First.Should().BeSameAs(this._second);
		this._selection.Purpose.Should().Be(ItemPairPurpose.Compare);
		this._selection.FirstSack.Should().BeNull();
	}

	[Fact]
	public void Pick_EmptyCell_KeepsTheModeOn()
	{
		this._selection.Start(this._first);

		this._selection.Pick(null).Should().BeNull();
		this._selection.First.Should().BeSameAs(this._first);
	}

	[Fact]
	public void Pick_TheFirstItemItself_EndsTheModeWithoutAPair()
	{
		this._selection.Start(this._first, ItemPairPurpose.SocketGameRules);

		this._selection.Pick(this._first).Should().BeNull();
		this._selection.IsActive.Should().BeFalse();
	}

	[Fact]
	public void Pick_ModeOff_DoesNothing()
		=> this._selection.Pick(this._second).Should().BeNull();

	[Fact]
	public void Cancel_EndsTheMode_OnlyWhenOn()
	{
		this._selection.Cancel().Should().BeFalse();

		this._selection.Start(this._first);
		this._selection.Cancel().Should().BeTrue();
		this._selection.IsActive.Should().BeFalse();
	}

	[Fact]
	public void Changed_RaisedWhenTheModeStartsAndEnds_NotForAClickOnAnEmptyCell()
	{
		int changes = 0;
		this._selection.Changed += (_, _) => changes++;

		this._selection.Start(this._first);
		this._selection.Pick(null);
		changes.Should().Be(1);

		this._selection.Pick(this._second);
		changes.Should().Be(2);

		this._selection.Cancel();
		changes.Should().Be(2, "the mode was already off");
	}
}
