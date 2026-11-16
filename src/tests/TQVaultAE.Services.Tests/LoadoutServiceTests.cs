using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Results;
using TQVaultAE.Application.Session;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

/// <summary>
/// <see cref="LoadoutService"/>: the loadouts file of a character, on real files in a temporary folder.
/// </summary>
public sealed partial class LoadoutServiceTests : IDisposable
{
	private const string CharacterMod = "SomeMod|custommaps";

	private readonly string _save = Path.Combine(Path.GetTempPath(), $"tqvault-loadouts-{Guid.NewGuid():N}", "_HeroA");
	private readonly Mock<IItemProvider> _itemProvider = new();
	private readonly Mock<IItemDatabaseService> _database = new();
	private readonly SessionContext _session = new();
	private readonly LoadoutService _service;

	public LoadoutServiceTests()
	{
		Directory.CreateDirectory(this._save);
		this._service = new LoadoutService(new Mock<ILogger<LoadoutService>>().Object, this._itemProvider.Object
			, new ItemMovementService(new Mock<ILogger<ItemMovementService>>().Object), this._database.Object, this._session, new FileIO(), new PathIO());
	}

	public void Dispose()
	{
		var root = Path.GetDirectoryName(this._save)!;
		if (File.Exists(this.LoadoutsFile))
			File.SetAttributes(this.LoadoutsFile, FileAttributes.Normal);
		if (Directory.Exists(root))
			Directory.Delete(root, true);
	}

	private string LoadoutsFile => Path.Combine(this._save, ILoadoutService.FileName);

	private static Item Piece(string name, int seed) => new()
	{
		BaseItemId = RecordId.Create($"records/item/{name}.dbr"),
		prefixID = RecordId.Create("records/item/lootmagicalaffixes/prefix/fire.dbr"),
		relicID = RecordId.Create("records/item/relics/ember.dbr"),
		RelicBonusId = RecordId.Create("records/item/lootmagicalaffixes/relics/ember_bonus.dbr"),
		Var1 = 3,
		atlantis = true,
		relic2ID = RecordId.Empty,
		RelicBonus2Id = RecordId.Empty,
		Var2 = 2023419968,
		Seed = seed,
		Width = 1,
		Height = 2,
	};

	/// <summary>A character wearing the given pieces (empty items elsewhere, as the save has them).</summary>
	private static PlayerCollection Wearing(bool useAlternate, params (int Slot, Item Item)[] pieces)
	{
		var sack = new SackCollection { SackType = SackType.Equipment, slots = 12 };
		for (int slot = 0; slot < 12; slot++)
		{
			var item = pieces.FirstOrDefault(p => p.Slot == slot).Item ?? new Item { BaseItemId = RecordId.Empty };
			item.Location = SackCollection.GetEquipmentLocationOffset(slot);
			sack.AddItem(item);
		}

		return new PlayerCollection("HeroA", "Player.chr") { EquipmentSack = sack, UseAlternate = useAlternate };
	}

	private LoadoutSet Read() => this._service.Read(this._save, "HeroA", CharacterMod);

	/// <summary>A loadout of the given worn gear: an empty one, then updated with it.</summary>
	private LoadoutChange AddWearing(LoadoutSet set, string name, PlayerCollection player)
		=> this._service.Add(set, name) is { IsSuccess: true } ? this._service.Update(set, name, player) : LoadoutChange.Missing;

	[Fact]
	public void Add_GivesAnEmptyLoadout_WhateverIsWorn()
	{
		this._service.Add(this.Read(), "Fire set").IsSuccess.Should().BeTrue();

		var loadout = this.Read().Loadouts.Should().ContainSingle().Subject;
		loadout.Name.Should().Be("Fire set");
		loadout.UseAlternate.Should().BeFalse();
		loadout.Items.Should().BeEmpty();
	}

	[Fact]
	public void AddThenUpdate_ThenReadAgain_GivesIdenticalPiecesSlotBySlotAndTheWeaponSetInHand()
	{
		var helm = Piece("helm", 11);
		var ring = Piece("ring", 22);
		var player = Wearing(true, (0, helm), (EquipmentRules.Ring2Slot, ring));

		this.AddWearing(this.Read(), "Fire set", player).IsSuccess.Should().BeTrue();
		var read = this.Read();

		var loadout = read.Loadouts.Should().ContainSingle().Subject;
		loadout.UseAlternate.Should().BeTrue();
		ItemIdentityComparer.Instance.Equals(loadout.Pieces[0], helm).Should().BeTrue();
		ItemIdentityComparer.Instance.Equals(loadout.Pieces[EquipmentRules.Ring2Slot], ring).Should().BeTrue();
		loadout.Items.Should().HaveCount(2);
		loadout.Pieces[0]!.ContainerMod.Should().Be(CharacterMod, "a piece is read with the character's map");
	}

	[Theory]
	[InlineData("A")]
	[InlineData("a")]
	[InlineData("  A ")]
	public void Add_NameAlreadyUsedIgnoringCaseAndSpaces_IsRefusedAndTheFileUnchanged(string name)
	{
		var set = this.Read();
		this._service.Add(set, "A");
		var before = File.ReadAllText(this.LoadoutsFile);

		var result = this._service.Add(set, name);

		result.NameError.Should().Be(LoadoutNameError.AlreadyUsed);
		File.ReadAllText(this.LoadoutsFile).Should().Be(before);
	}

	[Fact]
	public void Add_EmptyName_IsRefused()
		=> this._service.Add(this.Read(), "  ").NameError.Should().Be(LoadoutNameError.Empty);

	[Fact]
	public void Update_MergesTheWornPiecesAsCopies_AndKeepsTheLoadoutsPiecesOfTheEmptySlots()
	{
		var set = this.Read();
		this.AddWearing(set, "A", Wearing(false, (0, Piece("helm", 1)), (2, Piece("torso", 2))));
		var wornTorso = Piece("torso", 3);
		var player = Wearing(true, (2, wornTorso), (EquipmentRules.Ring1Slot, Piece("ring", 4)));

		this._service.Update(set, "a", player).IsSuccess.Should().BeTrue();

		set.Loadouts.Single().Pieces[2].Should().NotBeSameAs(wornTorso, "the worn piece stays on: the loadout gets a copy");
		var loadout = this.Read().Loadouts.Single();
		loadout.Name.Should().Be("A", "the loadout keeps its name");
		loadout.UseAlternate.Should().BeTrue("the weapon set in hand is recorded");
		loadout.Pieces[0]!.Seed.Should().Be(1, "the head is empty on the character: the loadout keeps its helm");
		loadout.Pieces[2]!.Seed.Should().Be(3, "the worn torso replaces the loadout's");
		loadout.Pieces[EquipmentRules.Ring1Slot]!.Seed.Should().Be(4);
		player.EquipmentSack!.GetItem(2).Should().BeSameAs(wornTorso);
	}

	[Fact]
	public void Rename_ToAnotherCaseOfItsName_IsAllowed_ToAnotherLoadoutsName_IsRefused()
	{
		var set = this.Read();
		this._service.Add(set, "A");
		this._service.Add(set, "B");

		this._service.Rename(set, "A", "a").IsSuccess.Should().BeTrue();
		this._service.Rename(set, "a", "b").NameError.Should().Be(LoadoutNameError.AlreadyUsed);

		this.Read().Loadouts.Select(l => l.Name).Should().Equal("a", "B");
	}

	[Fact]
	public void Copy_GivesTheSamePiecesAsOtherItems()
	{
		var set = this.Read();
		this.AddWearing(set, "A", Wearing(true, (0, Piece("helm", 1))));

		this._service.Copy(set, "A", "A2").IsSuccess.Should().BeTrue();

		set.Loadouts.Should().HaveCount(2);
		set.Loadouts[1].UseAlternate.Should().BeTrue();
		set.Loadouts[1].Pieces[0].Should().NotBeSameAs(set.Loadouts[0].Pieces[0]);
		ItemIdentityComparer.Instance.Equals(set.Loadouts[1].Pieces[0], set.Loadouts[0].Pieces[0]).Should().BeTrue();
		this.Read().Loadouts.Select(l => l.Name).Should().Equal("A", "A2");
	}

	[Fact]
	public void Delete_TheLastLoadout_RemovesTheFile()
	{
		var set = this.Read();
		this._service.Add(set, "A");

		this._service.Delete(set, "A").IsSuccess.Should().BeTrue();

		File.Exists(this.LoadoutsFile).Should().BeFalse();
		set.Loadouts.Should().BeEmpty();
	}

	[Fact]
	public void Delete_UnknownLoadout_IsMissing()
		=> this._service.Delete(this.Read(), "A").NotFound.Should().BeTrue();

	[Fact]
	public void Read_DamagedFile_WritesNothingOverIt()
	{
		File.WriteAllText(this.LoadoutsFile, "{ not json");

		var set = this.Read();
		var result = this._service.Add(set, "A");

		set.ReadFailed.Should().BeTrue();
		result.IsSuccess.Should().BeFalse();
		File.ReadAllText(this.LoadoutsFile).Should().Be("{ not json");
	}

	[Fact]
	public void Add_FileCannotBeWritten_FailsAndKeepsTheLoadoutsAsTheyWere()
	{
		var set = this.Read();
		this._service.Add(set, "A");
		File.SetAttributes(this.LoadoutsFile, FileAttributes.ReadOnly);

		var result = this._service.Add(set, "B");

		result.Error.Should().NotBeNull();
		set.Loadouts.Select(l => l.Name).Should().Equal("A");
	}

	[Fact]
	public void Read_KeepsTheSetForTheSession()
	{
		var set = this.Read();

		this._service.Find(this._save).Should().BeSameAs(set);
		this._session.Loadouts.Should().ContainKey(this.LoadoutsFile);
	}
}
