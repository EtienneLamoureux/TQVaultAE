using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Presentation;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

/// <summary>
/// The loadouts in the search (<see cref="ItemDatabaseService.AddLoadoutsToDatabase"/>): each piece is a result naming
/// the character and the loadout, apart from the character's own items.
/// </summary>
public class LoadoutSearchTests
{
	private readonly SessionContext _session = new();
	private readonly ItemDatabaseService _database;

	public LoadoutSearchTests()
		=> this._database = new ItemDatabaseService(new Mock<ILogger<ItemDatabaseService>>().Object, new Mock<IItemProvider>().Object, this._session);

	private static Item Piece(string name) => new() { BaseItemId = RecordId.Create($"records/item/{name}.dbr") };

	private static LoadoutSet Set(params Loadout[] loadouts)
		=> new("""C:\Saves\_HeroA\TQVaultAE.Loadouts.json""", "HeroA", null) { Loadouts = loadouts };

	[Fact]
	public void AddLoadoutsToDatabase_EachPieceNamesTheCharacterAndTheLoadout()
	{
		var helm = Piece("helm");
		var ring = Piece("ring");
		var set = Set(new Loadout("Fire", false, [helm, null]), new Loadout("Cold", true, [null, ring]));

		this._database.AddLoadoutsToDatabase(set);

		var results = this._database.ItemDatabase.ToList();
		results.Should().HaveCount(2);
		var helmResult = results.Single(r => r.Item == helm);
		helmResult.SackType.Should().Be(SackType.Loadout);
		helmResult.ContainerName.Should().Be("HeroA");
		helmResult.LoadoutName.Should().Be("Fire");
		results.Single(r => r.Item == ring).LoadoutName.Should().Be("Cold");
		results.Single(r => r.Item == ring).SackNumber.Should().Be(1);
	}

	[Fact]
	public void AddLoadoutsToDatabase_Again_ReplacesTheLoadoutsResultsAndLeavesTheCharactersItems()
	{
		var worn = Piece("worn");
		this._database.AddItemToDatabase(worn, """C:\Saves\_HeroA\Player.chr""", "HeroA", 0, SackType.Equipment);
		this._database.AddLoadoutsToDatabase(Set(new Loadout("Fire", false, [Piece("helm")])));
		var renamed = Piece("helm2");

		this._database.AddLoadoutsToDatabase(Set(new Loadout("Fire 2", false, [renamed])));

		this._database.ItemDatabase.Select(r => r.Item).Should().BeEquivalentTo([worn, renamed]);
		this._database.ItemDatabase.Single(r => r.Item == renamed).LoadoutName.Should().Be("Fire 2");
	}

	[Fact]
	public void RemoveContainerFromDatabase_OfThePlayerFile_KeepsItsLoadouts()
	{
		// A character's reload replaces its own items only.
		var piece = Piece("helm");
		this._database.AddItemToDatabase(Piece("worn"), """C:\Saves\_HeroA\Player.chr""", "HeroA", 0, SackType.Equipment);
		this._database.AddLoadoutsToDatabase(Set(new Loadout("Fire", false, [piece])));

		this._database.RemoveContainerFromDatabase("""C:\Saves\_HeroA\Player.chr""");

		this._database.ItemDatabase.Select(r => r.Item).Should().Equal(piece);
	}

	[Fact]
	public void RebuildItemDatabase_KeepsTheLoadoutsOfTheSession()
	{
		var piece = Piece("helm");
		var set = Set(new Loadout("Fire", false, [piece]));
		this._session.Loadouts[set.FilePath] = set;

		this._database.RebuildItemDatabase();

		this._database.ItemDatabase.Single().LoadoutName.Should().Be("Fire");
	}

	[Fact]
	public void CountItemsByRecord_LeavesTheLoadoutCopiesOut()
	{
		var helm = RecordId.Create("records/item/helm.dbr");
		this._database.AddItemToDatabase(Piece("helm"), """C:\Saves\_HeroA\Player.chr""", "HeroA", 0, SackType.Equipment);
		this._database.AddLoadoutsToDatabase(Set(new Loadout("Fire", false, [Piece("helm")])));

		this._database.CountItemsByRecord([helm])[helm].Should().Be(1, "a loadout piece is a copy of a piece owned");
	}

	[Fact]
	public void SearchResultNavigation_LoadoutPiece_SelectsTheCharacterOnTheEquipmentTab()
	{
		this._database.AddLoadoutsToDatabase(Set(new Loadout("Fire", false, [Piece("helm")])));

		var location = SearchResultNavigation.Resolve(this._database.ItemDatabase.Single());

		location.Panel.Should().Be(ResultPanelKind.Loadout);
		location.NeedsCharacter.Should().BeTrue();
		location.Bag.Should().Be(BagIdConstants.BAGID_EQUIPMENTPANEL);
		location.BagNumber.Should().BeNull();
	}

	[Fact]
	public void ResultTexts_LoadoutPiece_NameTheLoadout()
	{
		this._database.AddLoadoutsToDatabase(Set(new Loadout("Fire", false, [Piece("helm")])));
		var result = this._database.ItemDatabase.Single();

		ItemSummaryText.ContainerTypeLabel(result.SackType, result.StashType).Should().Be(Resources.LoadoutResultsContainer);
		ItemSummaryText.PlaceLabel(result.LoadoutName, SearchResultNavigation.Resolve(result).BagNumber).Should().Be("Fire");
		ItemSummaryText.PlaceLabel(null, 3).Should().Be(ItemSummaryText.BagLabel(3));
	}
}
