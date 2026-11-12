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
		this._links.Setup(l => l.FindMod(It.IsAny<string?>())).Returns<string?>(key
			=> string.Equals(ModIdentity.NameOf(key), Shadow.Name, StringComparison.OrdinalIgnoreCase) ? Shadow : null);
		this._maps.Setup(m => m.Use(It.IsAny<string?>())).Returns(Mock.Of<IDisposable>()).Callback<string?>(this._scopes.Add);
		// The base game knows the base record only.
		this._database.Setup(d => d.GetInfo(BaseRecord)).Returns(new Info(new DBRecordCollection(BaseRecord, "ArmorJewelry_Ring")));
		this._service = new ItemModService(this._maps.Object, this._database.Object, this._links.Object);
	}

	private static Item ItemOf(RecordId id, string? mod = null, ModOrigin origin = ModOrigin.Inherited)
		=> new() { BaseItemId = id, Mod = mod, ModOrigin = origin };

	[Fact]
	public async Task LoadDataAsync_ModNotLoadedYet_LoadsItsDatabase()
	{
		this._maps.Setup(m => m.LoadedMapFolders).Returns([string.Empty]);

		await this._service.LoadDataAsync("shadowchampions");

		this._maps.Verify(m => m.Preload(Shadow.Folder), Times.Once);
	}

	[Theory]
	[InlineData("shadowchampions")]
	[InlineData(null)]
	[InlineData("Lilith")]
	public void LoadDataAsync_DatabaseLoadedAlready_CompletesAtOnce(string? mod)
	{
		// The base game is loaded at startup; a mod not installed is read with the base game data.
		this._maps.Setup(m => m.LoadedMapFolders).Returns([string.Empty, Shadow.Folder.ToUpperInvariant()]);

		var loading = this._service.LoadDataAsync(mod);

		loading.IsCompletedSuccessfully.Should().BeTrue();
		this._maps.Verify(m => m.Preload(It.IsAny<string?>()), Times.Never);
	}

	[Fact]
	public async Task LoadDataAsync_FailedLoad_IsLeftToTheFirstUse()
	{
		this._maps.Setup(m => m.LoadedMapFolders).Returns([string.Empty]);
		this._maps.Setup(m => m.Preload(Shadow.Folder)).Throws(new IOException("locked"));

		var load = () => this._service.LoadDataAsync("shadowchampions");

		await load.Should().NotThrowAsync();
	}

	[Fact]
	public void Scope_ItemWithoutMapYet_LeavesTheDatabaseAlone()
	{
		this._service.Scope(ItemOf(ModRecord, "shadowchampions", ModOrigin.None)).Should().BeSameAs(NoScope.Instance);
		this._service.Scope(null).Should().BeSameAs(NoScope.Instance);
		this._scopes.Should().BeEmpty();
	}

	[Theory]
	[InlineData(ModOrigin.Inherited)]
	[InlineData(ModOrigin.Own)]
	public void Scope_BaseGameMap_UsesTheBaseGameWhateverTheActiveMap(ModOrigin origin)
	{
		using var scope = this._service.Scope(ItemOf(ModRecord, null, origin));

		this._scopes.Should().Equal(string.Empty);
	}

	[Fact]
	public void Scope_ModNotInstalled_UsesTheBaseGame()
	{
		using var scope = this._service.Scope(ItemOf(ModRecord, "Lilith"));

		this._scopes.Should().Equal(string.Empty);
	}

	[Theory]
	[InlineData(ModOrigin.Inherited)]
	[InlineData(ModOrigin.Own)]
	public void Scope_InstalledMod_UsesThatModsDatabase_InAVaultOrOnACharacter(ModOrigin origin)
	{
		var inVault = ItemOf(ModRecord, "SHADOWCHAMPIONS", origin);
		inVault.InVault = true;

		using (this._service.Scope(inVault))
		using (this._service.Scope(ItemOf(ModRecord, "shadowchampions", origin)))
		{
		}

		this._scopes.Should().Equal(Shadow.Folder, Shadow.Folder);
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
