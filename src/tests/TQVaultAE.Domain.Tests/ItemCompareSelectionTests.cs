using AwesomeAssertions;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Domain;

/// <summary>
/// <see cref="ItemCompareSelection"/>: the compare selection mode shared by the item panels.
/// </summary>
public class ItemCompareSelectionTests
{
	private readonly ItemCompareSelection _selection = new();
	private readonly Item _first = new();
	private readonly Item _second = new();

	[Fact]
	public void Pick_AnotherItem_GivesThePairInOrderAndEndsTheMode()
	{
		this._selection.Start(this._first);

		this._selection.Pick(this._second).Should().Be((this._first, this._second));
		this._selection.IsActive.Should().BeFalse();
		this._selection.First.Should().BeNull();
	}

	[Fact]
	public void Pick_EmptyCell_KeepsTheModeOn()
	{
		this._selection.Start(this._first);

		this._selection.Pick(null).Should().BeNull();
		this._selection.First.Should().BeSameAs(this._first);
	}

	[Fact]
	public void Pick_TheFirstItemItself_EndsTheModeWithoutComparing()
	{
		this._selection.Start(this._first);

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
