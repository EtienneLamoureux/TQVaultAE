using AwesomeAssertions;
using Moq;
using TQVaultAE.Application.Results;
using TQVaultAE.Application.Session;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Services;

/// <summary>
/// <see cref="TQVaultAE.Services.LoadoutService.Swap"/>: the loadout's pieces and the worn ones change places, every item
/// staying one item.
/// </summary>
public sealed partial class LoadoutServiceTests
{
	/// <summary>Loadout "B" recorded from a helm and a torso worn with the second weapon set in hand.</summary>
	private LoadoutSet WithLoadoutB(out Item loadoutHelm, out Item loadoutTorso)
	{
		var set = this.Read();
		this._service.Add(set, "B", Wearing(true, (EquipmentRules.HeadSlot, Piece("helmB", 1)), (EquipmentRules.BodySlot, Piece("torsoB", 2))))
			.IsSuccess.Should().BeTrue();
		loadoutHelm = set.Loadouts[0].Pieces[EquipmentRules.HeadSlot]!;
		loadoutTorso = set.Loadouts[0].Pieces[EquipmentRules.BodySlot]!;
		return set;
	}

	[Fact]
	public void Swap_PutsTheLoadoutsPiecesOnAndTheWornOnesInTheLoadout()
	{
		var set = this.WithLoadoutB(out var loadoutHelm, out var loadoutTorso);
		var wornHelm = Piece("helmA", 3);
		var wornRing = Piece("ringA", 4);
		var player = Wearing(false, (EquipmentRules.HeadSlot, wornHelm), (EquipmentRules.Ring1Slot, wornRing));

		var result = this._service.Swap(set, "b", player);

		result.Should().Be(new LoadoutSwapResult(LoadoutSwapOutcome.Swapped, PutOn: 2, TakenOff: 2));
		var equipment = player.EquipmentSack!;
		equipment.GetItem(EquipmentRules.HeadSlot).Should().BeSameAs(loadoutHelm, "the piece itself, not a copy");
		equipment.GetItem(EquipmentRules.BodySlot).Should().BeSameAs(loadoutTorso);
		loadoutHelm.Location.Should().Be(SackCollection.GetEquipmentLocationOffset(EquipmentRules.HeadSlot));
		var loadout = set.Loadouts.Single();
		loadout.Pieces[EquipmentRules.HeadSlot].Should().BeSameAs(wornHelm);
		loadout.Pieces[EquipmentRules.Ring1Slot].Should().BeSameAs(wornRing);
		loadout.Pieces[EquipmentRules.BodySlot].Should().BeNull("the slot worn empty is empty in the loadout");
		player.UseAlternate.Should().BeTrue("the loadout's weapon set is put in hand");
		loadout.UseAlternate.Should().BeFalse("the loadout keeps the weapon set that was in hand");
		equipment.IsModified.Should().BeTrue();
		set.IsModified.Should().BeTrue("the loadouts are written with the character");
	}

	[Fact]
	public void Swap_AnEmptyLoadoutSlot_TakesTheWornPieceAndEmptiesTheCharactersSlot()
	{
		var set = this.WithLoadoutB(out _, out _);
		var wornRing = Piece("ringA", 4);
		var player = Wearing(true, (EquipmentRules.Ring2Slot, wornRing));

		this._service.Swap(set, "B", player);

		var slot = player.EquipmentSack!.GetItem(EquipmentRules.Ring2Slot);
		ItemKey.IsEmptyItem(slot).Should().BeTrue();
		slot.Location.Should().Be(SackCollection.GetEquipmentLocationOffset(EquipmentRules.Ring2Slot));
		player.EquipmentSack.Should().HaveCount(12, "the equipment keeps one item per slot");
		set.Loadouts[0].Pieces[EquipmentRules.Ring2Slot].Should().BeSameAs(wornRing);
	}

	[Fact]
	public void Swap_Again_SwapsBack()
	{
		var set = this.WithLoadoutB(out var loadoutHelm, out var loadoutTorso);
		var wornHelm = Piece("helmA", 3);
		var player = Wearing(false, (EquipmentRules.HeadSlot, wornHelm));

		this._service.Swap(set, "B", player);
		this._service.Swap(set, "B", player);

		player.EquipmentSack!.GetItem(EquipmentRules.HeadSlot).Should().BeSameAs(wornHelm);
		ItemKey.IsEmptyItem(player.EquipmentSack.GetItem(EquipmentRules.BodySlot)).Should().BeTrue();
		set.Loadouts[0].Pieces[EquipmentRules.HeadSlot].Should().BeSameAs(loadoutHelm);
		set.Loadouts[0].Pieces[EquipmentRules.BodySlot].Should().BeSameAs(loadoutTorso);
		player.UseAlternate.Should().BeFalse();
		set.Loadouts[0].UseAlternate.Should().BeTrue();
	}

	[Fact]
	public void Swap_TheSearchFollowsBothSides()
	{
		var set = this.WithLoadoutB(out var loadoutHelm, out _);
		var player = Wearing(false, (EquipmentRules.HeadSlot, Piece("helmA", 3)));
		this._database.Invocations.Clear();

		this._service.Swap(set, "B", player);

		this._database.Verify(d => d.AddLoadoutsToDatabase(set), Times.Once);
		this._database.Verify(d => d.AddItemToDatabase(loadoutHelm, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), SackType.Equipment, null), Times.Once);
	}

	[Fact]
	public void Swap_ThenSave_WritesTheWornPiecesIntoTheLoadout()
	{
		var set = this.WithLoadoutB(out _, out _);
		var wornRing = Piece("ringA", 4);
		this._service.Swap(set, "B", Wearing(false, (EquipmentRules.Ring1Slot, wornRing)));

		this._service.SaveModified(set);

		var read = this.ReadInNewSession().Loadouts.Single();
		ItemIdentityComparer.Instance.Equals(read.Pieces[EquipmentRules.Ring1Slot], wornRing).Should().BeTrue();
		read.Pieces[EquipmentRules.HeadSlot].Should().BeNull();
	}

	[Fact]
	public void Swap_BothEmptySameWeaponSet_ChangesNothing()
	{
		var set = this.Read();
		this._service.Add(set, "Empty", Wearing(false));
		var player = Wearing(false);
		player.EquipmentSack!.IsModified = false;

		this._service.Swap(set, "Empty", player).Outcome.Should().Be(LoadoutSwapOutcome.NothingToSwap);

		set.IsModified.Should().BeFalse();
		player.EquipmentSack!.IsModified.Should().BeFalse();
	}

	[Fact]
	public void Swap_ReadOnlyCharacter_IsRefusedAndChangesNothing()
	{
		var set = this.WithLoadoutB(out var loadoutHelm, out _);
		var wornHelm = Piece("helmA", 3);
		var player = Wearing(false, (EquipmentRules.HeadSlot, wornHelm));
		player.IsReadOnly = true;

		this._service.Swap(set, "B", player).Outcome.Should().Be(LoadoutSwapOutcome.Refused);

		player.EquipmentSack!.GetItem(EquipmentRules.HeadSlot).Should().BeSameAs(wornHelm);
		set.Loadouts[0].Pieces[EquipmentRules.HeadSlot].Should().BeSameAs(loadoutHelm);
		set.IsModified.Should().BeFalse();
	}

	[Fact]
	public void Swap_UnknownLoadoutOrUnreadableFile_IsRefused()
	{
		var set = this.WithLoadoutB(out _, out _);
		var player = Wearing(false, (EquipmentRules.HeadSlot, Piece("helmA", 3)));

		this._service.Swap(set, "C", player).Outcome.Should().Be(LoadoutSwapOutcome.Refused);
		set.ReadFailed = true;
		this._service.Swap(set, "B", player).Outcome.Should().Be(LoadoutSwapOutcome.Refused);
		set.IsModified.Should().BeFalse();
	}
}
