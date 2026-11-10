using AwesomeAssertions;
using Moq;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

/// <summary>
/// <see cref="ItemModService"/>: an item linked to a mod is resolved with that mod's database.
/// </summary>
public class ItemModServiceTests
{
	private static readonly InstalledMod Shadow = new("shadowchampions", @"e:\steam\workshop\shadowchampions", ModSource.SteamWorkshop);
	private static readonly RecordId BaseRecord = RecordId.Create(@"records\item\ring.dbr");
	private static readonly RecordId ModRecord = RecordId.Create(@"records\customitems\mi\e_centurionlance.dbr");

	private readonly Mock<IDatabaseMaps> _maps = new();
	private readonly Mock<IDatabase> _database = new();
	private readonly Mock<IModLinkService> _links = new();
	private readonly ItemModService _service;
	private readonly List<string?> _scopes = [];

	public ItemModServiceTests()
	{
		this._links.Setup(l => l.GetInstalledMods(false)).Returns([Shadow]);
		this._maps.Setup(m => m.Use(It.IsAny<string?>())).Returns(Mock.Of<IDisposable>()).Callback<string?>(this._scopes.Add);
		// The base game knows the base record only.
		this._database.Setup(d => d.GetInfo(BaseRecord)).Returns(new Info(new DBRecordCollection(BaseRecord, "ArmorJewelry_Ring")));
		this._service = new ItemModService(this._maps.Object, this._database.Object, this._links.Object);
	}

	private static Item ItemOf(RecordId id, string? mod = null) => new() { BaseItemId = id, Mod = mod };

	[Fact]
	public void Scope_ItemWithoutMod_LeavesTheDatabaseAlone()
	{
		this._service.Scope(ItemOf(BaseRecord)).Should().BeSameAs(NoScope.Instance);
		this._service.Scope(null).Should().BeSameAs(NoScope.Instance);
		this._scopes.Should().BeEmpty();
	}

	[Fact]
	public void Scope_ItemOfAModNotInstalled_LeavesTheDatabaseAlone()
	{
		this._service.Scope(ItemOf(ModRecord, "Lilith")).Should().BeSameAs(NoScope.Instance);
		this._scopes.Should().BeEmpty();
	}

	[Fact]
	public void Scope_VaultItemWithoutMod_UsesTheBaseGameWhateverTheActiveMap()
	{
		var item = ItemOf(ModRecord);
		item.InVault = true;

		using var scope = this._service.Scope(item);

		this._scopes.Should().Equal(string.Empty);
	}

	[Fact]
	public void Scope_VaultItemOfAModNotInstalled_UsesTheBaseGame()
	{
		var item = ItemOf(ModRecord, "Lilith");
		item.InVault = true;

		using var scope = this._service.Scope(item);

		this._scopes.Should().Equal(string.Empty);
	}

	[Fact]
	public void Scope_VaultItemOfAnInstalledMod_UsesThatModsDatabase()
	{
		var item = ItemOf(ModRecord, "shadowchampions");
		item.InVault = true;

		using var scope = this._service.Scope(item);

		this._scopes.Should().Equal(Shadow.Folder);
	}

	[Fact]
	public void Scope_ItemOfAnInstalledMod_UsesThatModsDatabase()
	{
		using var scope = this._service.Scope(ItemOf(ModRecord, "SHADOWCHAMPIONS"));

		this._scopes.Should().Equal(Shadow.Folder);
	}

	[Fact]
	public void IsModOnly_AsksTheBaseGameDatabase()
	{
		this._service.IsModOnly(ItemOf(ModRecord)).Should().BeTrue();
		this._service.IsModOnly(ItemOf(BaseRecord)).Should().BeFalse();
		this._scopes.Should().AllBe(string.Empty);
	}

	[Fact]
	public void AdoptMod_LinksOnlyTheUnlinkedModItems()
	{
		var modItem = ItemOf(ModRecord);
		var baseItem = ItemOf(BaseRecord);
		var otherMod = ItemOf(ModRecord, "Lilith");

		int linked = this._service.AdoptMod([modItem, baseItem, otherMod], Shadow.Name);

		linked.Should().Be(1);
		modItem.Mod.Should().Be(Shadow.Name);
		baseItem.Mod.Should().BeNull();
		otherMod.Mod.Should().Be("Lilith");
	}

	[Fact]
	public void Suggest_AsksAboutEveryRecordTheItemsName()
	{
		IEnumerable<string>? asked = null;
		var suggestion = new ModSuggestion(1, [new ModMatch(Shadow, 1)]);
		this._links.Setup(l => l.SuggestFor(It.IsAny<IEnumerable<string>>()))
			.Callback<IEnumerable<string>>(r => asked = r.ToList()).Returns(suggestion);
		var item = ItemOf(ModRecord);
		item.prefixID = RecordId.Create(@"records\customaffix\p.dbr");

		this._service.Suggest([item, ItemOf(BaseRecord)]).Should().BeSameAs(suggestion);

		asked.Should().Equal(ModRecord.Raw, item.prefixID.Raw, BaseRecord.Raw);
	}

	[Fact]
	public void Clone_KeepsTheMod()
	{
		ItemOf(ModRecord, Shadow.Name).Clone().Mod.Should().Be(Shadow.Name);
	}
}
