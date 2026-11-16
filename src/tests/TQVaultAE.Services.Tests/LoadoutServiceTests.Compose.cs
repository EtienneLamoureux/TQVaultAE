using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Session;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

/// <summary>
/// <see cref="LoadoutService.Compose"/> and the save of the loadouts composed in the loadout panel: pieces moved in and
/// out stay in memory, marked modified, until the save or the next change written at once.
/// </summary>
public sealed partial class LoadoutServiceTests
{
	/// <summary>A loadout "A" written to the file, wearing a helm; the loadouts read again.</summary>
	private LoadoutSet WithLoadoutA(out Item helm)
	{
		var set = this.Read();
		this.AddWearing(set, "A", Wearing(false, (0, Piece("helm", 1)))).IsSuccess.Should().BeTrue();
		helm = set.Loadouts[0].Pieces[0]!;
		return set;
	}

	/// <summary>The loadout's pieces with <paramref name="piece"/> in <paramref name="slot"/>, as the panel's slots give them.</summary>
	private static Item?[] With(LoadoutSet set, int slot, Item? piece)
	{
		var pieces = new Item?[12];
		for (int i = 0; i < pieces.Length; i++)
			pieces[i] = i < set.Loadouts[0].Pieces.Count ? set.Loadouts[0].Pieces[i] : null;
		pieces[slot] = piece;
		return pieces;
	}

	/// <summary>A new session reading the file, as at the next start.</summary>
	private LoadoutSet ReadInNewSession()
		=> new LoadoutService(new Mock<ILogger<LoadoutService>>().Object, new Mock<IItemProvider>().Object
			, new ItemMovementService(new Mock<ILogger<ItemMovementService>>().Object), new Mock<IItemDatabaseService>().Object
			, new SessionContext(), new FileIO(), new PathIO()).Read(this._save, "HeroA", CharacterMod);

	[Fact]
	public void Compose_APieceMovedIn_KeepsTheItemItselfMarksModifiedAndLeavesTheFile()
	{
		var set = this.WithLoadoutA(out _);
		var before = File.ReadAllText(this.LoadoutsFile);
		var sword = Piece("sword", 7);

		this._service.Compose(set, "a", With(set, EquipmentRules.Weapon1Slot, sword)).Should().BeTrue();

		set.Loadouts[0].Pieces[EquipmentRules.Weapon1Slot].Should().BeSameAs(sword, "a piece is moved, not copied");
		set.IsModified.Should().BeTrue();
		File.ReadAllText(this.LoadoutsFile).Should().Be(before, "the file is written with the other files");
	}

	[Fact]
	public void Compose_APieceTakenOut_EmptiesItsSlot()
	{
		var set = this.WithLoadoutA(out _);

		this._service.Compose(set, "A", With(set, EquipmentRules.HeadSlot, new Item { BaseItemId = RecordId.Empty })).Should().BeTrue();

		set.Loadouts[0].Pieces[EquipmentRules.HeadSlot].Should().BeNull("an empty item in a slot is an empty slot");
		set.Loadouts[0].Items.Should().BeEmpty();
	}

	[Fact]
	public void Compose_TheSameItems_ChangesNothing()
	{
		var set = this.WithLoadoutA(out _);
		var loadout = set.Loadouts[0];
		this._database.Invocations.Clear();

		this._service.Compose(set, "A", With(set, EquipmentRules.HeadSlot, loadout.Pieces[0])).Should().BeFalse();

		set.IsModified.Should().BeFalse();
		set.Loadouts[0].Should().BeSameAs(loadout);
		this._database.Verify(d => d.AddLoadoutsToDatabase(It.IsAny<LoadoutSet>()), Times.Never);
	}

	[Fact]
	public void Compose_AnIdenticalCopy_IsAChange()
	{
		// Pieces are compared by reference: a copy dropped in place of the piece is another item.
		var set = this.WithLoadoutA(out var helm);

		this._service.Compose(set, "A", With(set, EquipmentRules.HeadSlot, helm.Clone())).Should().BeTrue();
	}

	[Fact]
	public void Compose_UnknownLoadoutOrUnreadableFile_IsRefused()
	{
		var set = this.WithLoadoutA(out _);

		this._service.Compose(set, "B", With(set, 0, Piece("helm", 2))).Should().BeFalse();
		set.ReadFailed = true;
		this._service.Compose(set, "A", With(set, 0, Piece("helm", 2))).Should().BeFalse();
		set.IsModified.Should().BeFalse();
	}

	[Fact]
	public void Compose_TheSearchFollowsThePieces()
	{
		var set = this.WithLoadoutA(out _);
		this._database.Invocations.Clear();

		this._service.Compose(set, "A", With(set, EquipmentRules.Ring1Slot, Piece("ring", 3)));

		this._database.Verify(d => d.AddLoadoutsToDatabase(set), Times.Once);
	}

	[Fact]
	public void GetModified_ListsOnlyTheComposedLoadouts()
	{
		var set = this.WithLoadoutA(out _);
		this._service.GetModified().Should().BeEmpty();

		this._service.Compose(set, "A", With(set, EquipmentRules.Ring1Slot, Piece("ring", 3)));

		this._service.GetModified().Should().ContainSingle().Which.Should().BeSameAs(set);
	}

	[Fact]
	public void SaveModified_WritesTheComposedPiecesAndClearsTheFlag()
	{
		var set = this.WithLoadoutA(out _);
		var sword = Piece("sword", 7);
		this._service.Compose(set, "A", With(set, EquipmentRules.HeadSlot, null));
		this._service.Compose(set, "A", With(set, EquipmentRules.Weapon2Slot, sword));

		this._service.SaveModified(set);

		set.IsModified.Should().BeFalse();
		var read = this.ReadInNewSession().Loadouts.Single();
		read.Pieces[EquipmentRules.HeadSlot].Should().BeNull();
		ItemIdentityComparer.Instance.Equals(read.Pieces[EquipmentRules.Weapon2Slot], sword).Should().BeTrue();
	}

	[Fact]
	public void SaveModified_NotModified_WritesNothing()
	{
		var set = this.WithLoadoutA(out _);
		File.Delete(this.LoadoutsFile);

		this._service.SaveModified(set);

		File.Exists(this.LoadoutsFile).Should().BeFalse();
	}

	[Fact]
	public void SaveModified_ReadOnlyFile_ThrowsForTheSaveToAskAndStaysModified()
	{
		var set = this.WithLoadoutA(out _);
		this._service.Compose(set, "A", With(set, EquipmentRules.Ring1Slot, Piece("ring", 3)));
		File.SetAttributes(this.LoadoutsFile, FileAttributes.ReadOnly);

		var save = () => this._service.SaveModified(set);

		save.Should().Throw<UnauthorizedAccessException>();
		set.IsModified.Should().BeTrue("the save may retry it");
	}

	[Fact]
	public void Rename_WritesTheComposedPiecesToo()
	{
		var set = this.WithLoadoutA(out _);
		var sword = Piece("sword", 7);
		this._service.Compose(set, "A", With(set, EquipmentRules.Weapon1Slot, sword));

		this._service.Rename(set, "A", "B").IsSuccess.Should().BeTrue();

		set.IsModified.Should().BeFalse("the file holds the loadouts as they are now");
		var read = this.ReadInNewSession().Loadouts.Single();
		read.Name.Should().Be("B");
		ItemIdentityComparer.Instance.Equals(read.Pieces[EquipmentRules.Weapon1Slot], sword).Should().BeTrue();
	}

	[Fact]
	public void Read_AgainWithUnsavedPieces_KeepsThem()
	{
		// A reload of the character (its file changed on disk) does not undo the pieces moved in the loadout panel.
		var set = this.WithLoadoutA(out _);
		var sword = Piece("sword", 7);
		this._service.Compose(set, "A", With(set, EquipmentRules.Weapon1Slot, sword));

		var read = this.Read();

		read.Should().BeSameAs(set);
		read.Loadouts[0].Pieces[EquipmentRules.Weapon1Slot].Should().BeSameAs(sword);
	}
}
