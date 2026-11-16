using System.Drawing;
using AwesomeAssertions;
using Moq;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Services;

/// <summary>
/// <see cref="TQVaultAE.Services.LoadoutService.Empty"/>: the loadout's pieces themselves move to a bag, all or nothing,
/// and the loadout ends empty.
/// </summary>
public sealed partial class LoadoutServiceTests
{
	private static readonly ContainerPlace InventoryPlace = new() { Path = "Player.chr", Name = "HeroA", SackNumber = 0, SackType = SackType.Player };

	[Fact]
	public void Empty_AllFit_MovesThePiecesThemselvesAndEmptiesTheLoadout()
	{
		var set = this.WithLoadoutB(out var helm, out var torso);
		var bag = Bag(new Size(4, 4));

		var result = this._service.Empty(set, "b", bag, new Size(4, 4), InventoryPlace);

		result.Outcome.Should().Be(LoadoutEmptyOutcome.Emptied);
		result.Pieces.Should().Be(2);
		result.Moved.Should().Equal(helm, torso);
		bag.Should().HaveCount(2);
		bag.Should().Contain(helm, "the piece itself, not a copy").And.Contain(torso);
		helm.Place.Name.Should().Be("HeroA", "the search follows the piece into the bag");
		helm.Place.SackType.Should().Be(SackType.Player);
		(helm.PositionX, helm.PositionY).Should().NotBe((torso.PositionX, torso.PositionY));
		bag.IsModified.Should().BeTrue("saved with its file, as after a move by hand");
		var loadout = set.Loadouts.Single();
		loadout.Name.Should().Be("B", "the loadout stays, empty");
		loadout.Items.Should().BeEmpty();
		set.IsModified.Should().BeTrue("the loadouts are written with the other files");
	}

	[Fact]
	public void Empty_TheSearchKeepsThePiecesAtTheBagsPlace()
	{
		var set = this.WithLoadoutB(out var helm, out _);
		this._database.Invocations.Clear();

		this._service.Empty(set, "B", Bag(new Size(4, 4)), new Size(4, 4), InventoryPlace);

		this._database.Verify(d => d.AddLoadoutsToDatabase(set), Times.Once);
		this._database.Verify(d => d.RemoveItemFromDatabase(It.IsAny<Item>()), Times.Never, "the pieces move, none leaves the search");
		helm.Place.Path.Should().Be("Player.chr", "the loadouts file's entries are made again without it");
	}

	[Fact]
	public void Empty_ThenSave_WritesAnEmptyLoadout()
	{
		var set = this.WithLoadoutB(out _, out _);
		this._service.Empty(set, "B", Bag(new Size(4, 4)), new Size(4, 4), InventoryPlace);

		this._service.SaveModified(set);

		var read = this.ReadInNewSession().Loadouts.Single();
		read.Name.Should().Be("B");
		read.Items.Should().BeEmpty();
	}

	[Fact]
	public void Empty_NoRoomForEveryPiece_ChangesNothing()
	{
		var set = this.WithLoadoutB(out var helm, out var torso);
		var size = new Size(4, 4);
		var bag = Bag(size, freeSlots: 1);

		var result = this._service.Empty(set, "B", bag, size, InventoryPlace);

		result.Outcome.Should().Be(LoadoutEmptyOutcome.NoRoom);
		result.Pieces.Should().Be(2);
		result.Moved.Should().BeEmpty();
		bag.Should().NotContain(helm).And.NotContain(torso);
		bag.IsModified.Should().BeFalse();
		set.Loadouts[0].Pieces[EquipmentRules.HeadSlot].Should().BeSameAs(helm);
		set.Loadouts[0].Pieces[EquipmentRules.BodySlot].Should().BeSameAs(torso);
		set.IsModified.Should().BeFalse();
	}

	[Fact]
	public void Empty_ExactRoom_MovesEveryPiece()
	{
		var set = this.WithLoadoutB(out _, out _);
		var size = new Size(4, 4);
		var bag = Bag(size, freeSlots: 2);

		this._service.Empty(set, "B", bag, size, InventoryPlace).Outcome.Should().Be(LoadoutEmptyOutcome.Emptied);

		set.Loadouts[0].Items.Should().BeEmpty();
	}

	[Fact]
	public void Empty_RelicVaultRefusingAPiece_ChangesNothing()
	{
		var set = this.WithLoadoutB(out var helm, out _);
		var relicVault = new SackCollection { SackType = SackType.Stash, StashType = StashType.RelicVaultStash };

		var result = this._service.Empty(set, "B", relicVault, new Size(8, 8), InventoryPlace);

		result.Outcome.Should().Be(LoadoutEmptyOutcome.NoRoom);
		relicVault.Should().BeEmpty();
		set.Loadouts[0].Pieces[EquipmentRules.HeadSlot].Should().BeSameAs(helm);
	}

	[Fact]
	public void Empty_ReadOnlyBag_IsRefusedAndChangesNothing()
	{
		var set = this.WithLoadoutB(out var helm, out _);
		var bag = Bag(new Size(4, 4));
		bag.IsReadOnly = true;

		this._service.Empty(set, "B", bag, new Size(4, 4), InventoryPlace).Outcome.Should().Be(LoadoutEmptyOutcome.Refused);

		bag.Should().BeEmpty();
		set.Loadouts[0].Pieces[EquipmentRules.HeadSlot].Should().BeSameAs(helm);
		set.IsModified.Should().BeFalse();
	}

	[Fact]
	public void Empty_UnknownLoadoutOrUnreadableFile_IsRefused()
	{
		var set = this.WithLoadoutB(out _, out _);
		var bag = Bag(new Size(4, 4));

		this._service.Empty(set, "C", bag, new Size(4, 4), InventoryPlace).Outcome.Should().Be(LoadoutEmptyOutcome.Refused);
		set.ReadFailed = true;
		this._service.Empty(set, "B", bag, new Size(4, 4), InventoryPlace).Outcome.Should().Be(LoadoutEmptyOutcome.Refused);

		bag.Should().BeEmpty();
		set.IsModified.Should().BeFalse();
	}

	[Fact]
	public void Empty_AnEmptyLoadout_ChangesNothing()
	{
		var set = this.Read();
		this._service.Add(set, "Empty");
		var bag = Bag(new Size(4, 4));

		this._service.Empty(set, "Empty", bag, new Size(4, 4), InventoryPlace).Outcome.Should().Be(LoadoutEmptyOutcome.NothingToEmpty);

		bag.IsModified.Should().BeFalse();
		set.IsModified.Should().BeFalse();
	}

	[Fact]
	public void Empty_LeavesTheOtherLoadoutsAlone()
	{
		var set = this.WithLoadoutB(out _, out _);
		this._service.Copy(set, "B", "C").IsSuccess.Should().BeTrue();
		var copyHelm = set.Find("C")!.Pieces[EquipmentRules.HeadSlot];

		this._service.Empty(set, "B", Bag(new Size(4, 4)), new Size(4, 4), InventoryPlace);

		set.Find("B")!.Items.Should().BeEmpty();
		set.Find("C")!.Pieces[EquipmentRules.HeadSlot].Should().BeSameAs(copyHelm);
	}
}
