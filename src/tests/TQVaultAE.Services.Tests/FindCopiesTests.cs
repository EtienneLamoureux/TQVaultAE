using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Results;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

/// <summary>
/// The item menu's <i>Search</i> over the search database (<see cref="ItemDatabaseService.FindCopies"/>).
/// </summary>
public class FindCopiesTests
{
	private readonly Mock<IItemTextFormatter> _itemText = new();
	private readonly ItemDatabaseService _database;

	public FindCopiesTests()
	{
		this._database = new ItemDatabaseService(new Mock<ILogger<ItemDatabaseService>>().Object, this._itemText.Object);
		this._itemText.Setup(p => p.GetFriendlyNames(It.IsAny<Item>(), It.IsAny<FriendlyNamesExtraScopes?>(), It.IsAny<bool>()))
			.Returns((Item item, FriendlyNamesExtraScopes? _, bool _) => new ToFriendlyNameResult(item)
			{
				BaseItemInfoDescription = "Sword",
				RequirementVariables = [],
			});
	}

	private static Item Sword(int seed = 1, string suffix = "ofkings", int x = 0, int y = 0)
		=> new()
		{
			BaseItemId = RecordId.Create("""records\item\sword.dbr"""),
			suffixID = RecordId.Create($"""records\item\lootmagicalaffixes\suffix\{suffix}.dbr"""),
			Seed = seed,
			PositionX = x,
			PositionY = y,
		};

	private void Add(Item item, string container, SackType sackType = SackType.Vault, int sack = 0)
		=> this._database.AddItemToDatabase(item, $"""C:\saves\{container}""", container, sack, sackType);

	[Fact]
	public void FindCopies_ListsTheSourceFirst_ThenTheCopiesByContainerAndPlace()
	{
		var source = Sword();
		var inB = Sword(seed: 2);
		var inALater = Sword(seed: 3, y: 5);
		var inAFirst = Sword(seed: 4, y: 1);
		Add(inB, "vault-b");
		Add(inALater, "vault-a");
		Add(source, "vault-z");
		Add(inAFirst, "vault-a");

		var results = this._database.FindCopies(source, CopySearchKind.AllCopies);

		results.Select(r => r.Item).Should().Equal(source, inAFirst, inALater, inB);
		results.Should().OnlyContain(r => r.ItemName == "Sword");
	}

	[Fact]
	public void FindCopies_SameSeed_LeavesTheOtherSeedsOut()
	{
		var source = Sword();
		var samePiece = Sword();
		Add(source, "vault-a");
		Add(samePiece, "vault-b");
		Add(Sword(seed: 2), "vault-b");
		Add(Sword(suffix: "ofbears"), "vault-b");

		this._database.FindCopies(source, CopySearchKind.SameSeed).Select(r => r.Item).Should().Equal(source, samePiece);
	}

	[Fact]
	public void FindCopies_FindsTheLoadoutPieces()
	{
		var source = Sword();
		var piece = Sword();
		Add(source, "vault-a");
		this._database.AddLoadoutsToDatabase(new LoadoutSet("""C:\saves\_Hero\TQVaultAE.Loadouts.json""", "Hero", null)
		{
			Loadouts = [new Loadout("Fire", false, [piece])],
		});

		var results = this._database.FindCopies(source, CopySearchKind.SameSeed);

		results.Select(r => r.Item).Should().Equal(source, piece);
		results[1].LoadoutName.Should().Be("Fire");
	}

	[Fact]
	public void FindCopies_OfAnItemNotInTheDatabase_StillListsItFirst()
	{
		var source = Sword();
		var copy = Sword();
		Add(copy, "vault-a");

		this._database.FindCopies(source, CopySearchKind.SameSeed).Select(r => r.Item).Should().Equal(source, copy);
	}

	[Fact]
	public void FindCopies_LeavesOutTheCopiesThatCannotBeLoaded()
	{
		var source = Sword();
		var broken = Sword();
		Add(source, "vault-a");
		Add(broken, "vault-b");
		this._itemText.Setup(p => p.GetFriendlyNames(broken, It.IsAny<FriendlyNamesExtraScopes?>(), It.IsAny<bool>()))
			.Throws(new InvalidOperationException("broken record"));

		this._database.FindCopies(source, CopySearchKind.SameSeed).Select(r => r.Item).Should().Equal(source);
	}

	[Fact]
	public void FindCopies_AnUnavailableSearch_FindsNothing()
	{
		var plain = new Item { BaseItemId = RecordId.Create("""records\item\sword.dbr""") };
		Add(plain, "vault-a");

		this._database.FindCopies(plain, CopySearchKind.SamePrefix).Should().BeEmpty();
	}
}
