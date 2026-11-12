using AwesomeAssertions;
using Moq;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Forge;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

/// <summary>
/// <see cref="ForgeService"/>: the God mode parts, and what forging changes in the sacks and the search.
/// </summary>
public class ForgeServiceTests
{
	private readonly Mock<IItemProvider> _itemProvider = new();
	private readonly Mock<IItemDatabaseService> _itemDatabase = new();
	private readonly ForgeService _service;

	private readonly SackCollection _baseSack = new();
	private readonly SackCollection _materialSack = new();
	private readonly Dictionary<ForgeSlot, SackCollection?> _sacks = [];

	public ForgeServiceTests()
	{
		this._service = new ForgeService(this._itemProvider.Object, this._itemDatabase.Object);
		this._itemProvider.Setup(x => x.GetRequirementVariables(It.IsAny<Item>())).Returns([]);
	}

	private static Item Sword(string? prefix = null, string? suffix = null) => new()
	{
		BaseItemId = RecordId.Create("""records\item\equipmentweapon\sword\us_n_01.dbr"""),
		prefixID = prefix is null ? RecordId.Empty : RecordId.Create(prefix),
		suffixID = suffix is null ? RecordId.Empty : RecordId.Create(suffix),
	};

	private static ForgeSelection With(Item baseItem, Item? prefix = null, Item? suffix = null)
	{
		var selection = new ForgeSelection();
		selection[ForgeSlot.Base] = baseItem;
		selection[ForgeSlot.Prefix] = prefix;
		selection[ForgeSlot.Suffix] = suffix;
		return selection;
	}

	/// <summary>Places the base item and a prefix material item in their sacks, both saved.</summary>
	private (Item Base, Item Material, ForgeSelection Selection) PrefixForge()
	{
		var baseItem = Sword();
		var material = Sword(prefix: """records\prefix\p01.dbr""");
		this._baseSack.AddItem(baseItem);
		this._materialSack.AddItem(material);
		this._baseSack.IsModified = this._materialSack.IsModified = false;
		this._sacks[ForgeSlot.Base] = this._baseSack;
		this._sacks[ForgeSlot.Prefix] = this._materialSack;
		return (baseItem, material, With(baseItem, prefix: material));
	}

	private ForgeResult Forge(ForgeSelection selection, bool hardcore)
		=> this._service.Forge(this._service.CreateRules(), selection, this._sacks, godMode: false, _ => null, hardcore);

	[Fact]
	public void GodModeParts_ListsTheBaseThenTheAffixesTheItemHas()
	{
		this._service.GodModeParts(Sword()).Should().Equal(ForgePart.Base);
		this._service.GodModeParts(Sword(suffix: """records\suffix\s01.dbr"""))
			.Should().Equal(ForgePart.Base, ForgePart.Suffix);
		this._service.GodModeParts(Sword("""records\prefix\p01.dbr""", """records\suffix\s01.dbr"""))
			.Should().Equal(ForgePart.Base, ForgePart.Prefix, ForgePart.Suffix);
	}

	[Fact]
	public void GodModeParts_ListsTheSocketedRelics()
	{
		var item = Sword();
		item.relicID = RecordId.Create(@"records\item\relics\01_act1_aegisofathena.dbr");
		item.relic2ID = item.relicID;

		this._service.GodModeParts(item).Should().Equal(ForgePart.Base, ForgePart.Relic1, ForgePart.Relic2);
	}

	[Fact]
	public void Forge_NothingToMerge_IsRefusedAndChangesNothing()
	{
		var (baseItem, _, _) = this.PrefixForge();

		var result = this.Forge(With(baseItem), hardcore: true);

		result.Problem.Should().Be(ForgeMessage.MustSelectItemToMerge);
		baseItem.IsModified.Should().BeFalse();
		this._baseSack.IsModified.Should().BeFalse();
		this._itemDatabase.Verify(x => x.TryAddItemToDatabase(It.IsAny<Item>()), Times.Never);
	}

	[Fact]
	public void Forge_MergesTheMaterialAndReplacesTheSearchEntry()
	{
		var (baseItem, material, selection) = this.PrefixForge();

		var result = this.Forge(selection, hardcore: false);

		result.IsSuccess.Should().BeTrue();
		result.Forged.Should().BeSameAs(baseItem);
		baseItem.prefixID.Should().Be(material.prefixID);
		baseItem.IsModified.Should().BeTrue();
		this._baseSack.IsModified.Should().BeTrue();
		this._itemProvider.Verify(x => x.GetDBData(baseItem), Times.Once);
		this._itemDatabase.Verify(x => x.RemoveItemFromDatabase(baseItem), Times.Once);
		this._itemDatabase.Verify(x => x.TryAddItemToDatabase(baseItem), Times.Once);
	}

	[Fact]
	public void Forge_NotHardcore_KeepsTheMaterialItems()
	{
		var (_, material, selection) = this.PrefixForge();

		var result = this.Forge(selection, hardcore: false);

		result.Consumed.Should().BeEmpty();
		this._materialSack.Should().Contain(material);
		this._materialSack.IsModified.Should().BeFalse();
		this._itemDatabase.Verify(x => x.RemoveItemFromDatabase(material), Times.Never);
	}

	[Fact]
	public void Forge_Hardcore_ConsumesTheMaterialItems()
	{
		var (baseItem, material, selection) = this.PrefixForge();

		var result = this.Forge(selection, hardcore: true);

		result.Consumed.Should().Equal(material);
		this._materialSack.Should().NotContain(material);
		this._materialSack.IsModified.Should().BeTrue();
		this._baseSack.Should().Contain(baseItem);
		this._itemDatabase.Verify(x => x.RemoveItemFromDatabase(material), Times.Once);
	}

	[Fact]
	public void Forge_InvalidatesTheFriendlyNamesBeforeReAddingTheForgedItem()
	{
		var (baseItem, _, selection) = this.PrefixForge();
		var calls = new List<string>();
		this._itemProvider.Setup(x => x.InvalidateFriendlyNamesCache(It.IsAny<IEnumerable<Item>>()))
			.Callback(() => calls.Add("invalidate")).Returns(true);
		this._itemDatabase.Setup(x => x.TryAddItemToDatabase(baseItem)).Callback(() => calls.Add("add")).Returns(true);

		this.Forge(selection, hardcore: false);

		calls.Should().Equal("invalidate", "add");
	}

	[Fact]
	public void Preview_WithoutBaseItem_ReturnsNull()
		=> this._service.Preview(new ForgeSelection(), godMode: false, _ => null).Should().BeNull();

	[Fact]
	public void Preview_BaseItemOnly_IsACopyWithoutReadingTheRecordAgain()
	{
		var baseItem = Sword();

		var preview = this._service.Preview(With(baseItem), godMode: false, _ => null);

		preview.Should().NotBeNull().And.NotBeSameAs(baseItem);
		preview!.BaseItemId.Should().Be(baseItem.BaseItemId);
		this._itemProvider.Verify(p => p.GetDBData(It.IsAny<Item>()), Times.Never);
	}

	[Fact]
	public void Preview_WithAPrefixMaterial_MergesItIntoACopyAndReadsItsRecord()
	{
		var baseItem = Sword();
		var material = Sword(prefix: """records\prefix\p01.dbr""");

		var preview = this._service.Preview(With(baseItem, prefix: material), godMode: false, _ => null);

		preview!.prefixID.Should().Be(material.prefixID);
		baseItem.prefixID.Should().Be(RecordId.Empty);
		this._itemProvider.Verify(p => p.GetDBData(preview), Times.Once);
	}
}
