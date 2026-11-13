using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Results;
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
	private readonly SessionContext _session = new();
	private readonly LoadoutService _service;

	public LoadoutServiceTests()
	{
		Directory.CreateDirectory(this._save);
		this._service = new LoadoutService(new Mock<ILogger<LoadoutService>>().Object, this._itemProvider.Object, this._session, new FileIO(), new PathIO());
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
		Mod = CharacterMod,
		ModOrigin = ModOrigin.Inherited,
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

	[Fact]
	public void Add_ThenReadAgain_GivesIdenticalPiecesSlotBySlotAndTheWeaponSetInHand()
	{
		var helm = Piece("helm", 11);
		var ring = Piece("ring", 22);
		var player = Wearing(true, (0, helm), (EquipmentRules.Ring2Slot, ring));

		this._service.Add(this.Read(), "Fire set", player).IsSuccess.Should().BeTrue();
		var read = this.Read();

		var loadout = read.Loadouts.Should().ContainSingle().Subject;
		loadout.Name.Should().Be("Fire set");
		loadout.UseAlternate.Should().BeTrue();
		ItemIdentity.AreIdentical(loadout.Pieces[0], helm).Should().BeTrue();
		ItemIdentity.AreIdentical(loadout.Pieces[EquipmentRules.Ring2Slot], ring).Should().BeTrue();
		loadout.Items.Should().HaveCount(2);
		loadout.Pieces[0]!.Mod.Should().Be(CharacterMod, "a piece without its own map takes the character's");
		loadout.Pieces[0]!.ModOrigin.Should().Be(ModOrigin.Inherited);
		this._itemProvider.Verify(p => p.GetDBData(It.IsAny<Item>()), Times.Exactly(2));
	}

	[Fact]
	public void Add_KeepsCopies_NotTheWornItems()
	{
		var helm = Piece("helm", 11);
		var player = Wearing(false, (0, helm));
		var set = this.Read();

		this._service.Add(set, "A", player);
		helm.Seed = 99;

		set.Loadouts[0].Pieces[0]!.Seed.Should().Be(11);
	}

	[Fact]
	public void Add_PieceWithItsOwnMap_KeepsItThroughTheFile()
	{
		var relic = Piece("ring", 5);
		relic.Mod = null;
		relic.ModOrigin = ModOrigin.Own;

		this._service.Add(this.Read(), "A", Wearing(false, (EquipmentRules.Ring1Slot, relic)));
		var piece = this.Read().Loadouts[0].Pieces[EquipmentRules.Ring1Slot]!;

		piece.ModOrigin.Should().Be(ModOrigin.Own);
		piece.Mod.Should().BeNull();
	}

	[Theory]
	[InlineData("A")]
	[InlineData("a")]
	[InlineData("  A ")]
	public void Add_NameAlreadyUsedIgnoringCaseAndSpaces_IsRefusedAndTheFileUnchanged(string name)
	{
		var set = this.Read();
		this._service.Add(set, "A", Wearing(false));
		var before = File.ReadAllText(this.LoadoutsFile);

		var result = this._service.Add(set, name, Wearing(false, (0, Piece("helm", 1))));

		result.NameError.Should().Be(LoadoutNameError.AlreadyUsed);
		File.ReadAllText(this.LoadoutsFile).Should().Be(before);
	}

	[Fact]
	public void Add_EmptyName_IsRefused()
		=> this._service.Add(this.Read(), "  ", Wearing(false)).NameError.Should().Be(LoadoutNameError.Empty);

	[Fact]
	public void Update_RecordsTheGearWornNow()
	{
		var set = this.Read();
		this._service.Add(set, "A", Wearing(false, (0, Piece("helm", 1))));

		this._service.Update(set, "a", Wearing(true, (2, Piece("torso", 2)))).IsSuccess.Should().BeTrue();

		var loadout = this.Read().Loadouts.Single();
		loadout.Name.Should().Be("A", "the loadout keeps its name");
		loadout.UseAlternate.Should().BeTrue();
		loadout.Pieces[0].Should().BeNull();
		loadout.Pieces[2]!.Seed.Should().Be(2);
	}

	[Fact]
	public void Rename_ToAnotherCaseOfItsName_IsAllowed_ToAnotherLoadoutsName_IsRefused()
	{
		var set = this.Read();
		this._service.Add(set, "A", Wearing(false));
		this._service.Add(set, "B", Wearing(false));

		this._service.Rename(set, "A", "a").IsSuccess.Should().BeTrue();
		this._service.Rename(set, "a", "b").NameError.Should().Be(LoadoutNameError.AlreadyUsed);

		this.Read().Loadouts.Select(l => l.Name).Should().Equal("a", "B");
	}

	[Fact]
	public void Copy_GivesTheSamePiecesAsOtherItems()
	{
		var set = this.Read();
		this._service.Add(set, "A", Wearing(true, (0, Piece("helm", 1))));

		this._service.Copy(set, "A", "A2").IsSuccess.Should().BeTrue();

		set.Loadouts.Should().HaveCount(2);
		set.Loadouts[1].UseAlternate.Should().BeTrue();
		set.Loadouts[1].Pieces[0].Should().NotBeSameAs(set.Loadouts[0].Pieces[0]);
		ItemIdentity.AreIdentical(set.Loadouts[1].Pieces[0], set.Loadouts[0].Pieces[0]).Should().BeTrue();
		this.Read().Loadouts.Select(l => l.Name).Should().Equal("A", "A2");
	}

	[Fact]
	public void Delete_TheLastLoadout_RemovesTheFile()
	{
		var set = this.Read();
		this._service.Add(set, "A", Wearing(false));

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
		var result = this._service.Add(set, "A", Wearing(false));

		set.ReadFailed.Should().BeTrue();
		result.IsSuccess.Should().BeFalse();
		File.ReadAllText(this.LoadoutsFile).Should().Be("{ not json");
	}

	[Fact]
	public void Add_FileCannotBeWritten_FailsAndKeepsTheLoadoutsAsTheyWere()
	{
		var set = this.Read();
		this._service.Add(set, "A", Wearing(false));
		File.SetAttributes(this.LoadoutsFile, FileAttributes.ReadOnly);

		var result = this._service.Add(set, "B", Wearing(false));

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
