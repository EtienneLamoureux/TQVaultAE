using System.Collections.ObjectModel;
using AwesomeAssertions;
using Moq;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

public class AffixCatalogServiceTests
{
	private readonly Mock<IItemProvider> _itemProvider = new();
	private readonly Mock<ITranslationService> _translation = new();
	private readonly AffixCatalogService _service;

	public AffixCatalogServiceTests()
	{
		this._translation
			.Setup(t => t.TranslateXTag(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
			.Returns((string tag, bool _, bool _, bool _) => $"T:{tag}");
		this._service = new AffixCatalogService(this._itemProvider.Object, this._translation.Object);
	}

	#region Helpers

	private static Variable Text(string name, string value)
	{
		var v = new Variable(name, VariableDataType.StringVar, 1);
		v[0] = value;
		return v;
	}

	private static Info MakeInfo(string id, string itemClass, string classification = "COMMON", string? description = null)
	{
		var record = new DBRecordCollection(RecordId.Create(id), itemClass);
		record.Set(Text("Class", itemClass));
		record.Set(Text("itemClassification", classification));
		if (description is not null)
			record.Set(Text("description", description));
		return new Info(record);
	}

	private static Item MakeItem(string id, string itemClass, string classification = "COMMON")
		=> new()
		{
			BaseItemId = RecordId.Create(id),
			baseItemInfo = MakeInfo(id, itemClass, classification),
		};

	private static LootTableCollection Table(params (string Id, float Weight, string Translation)[] entries)
	{
		var data = new Dictionary<RecordId, (float Weight, LootRandomizerItem LootRandomizer)>();
		foreach (var (id, weight, translation) in entries)
		{
			var recordId = RecordId.Create(id);
			data.Add(recordId, (weight, LootRandomizerItem.Default(recordId) with { Translation = translation, Tag = "tag" }));
		}

		return new LootTableCollection(RecordId.Create("table"), data);
	}

	private static ReadOnlyDictionary<GameDlc, ReadOnlyCollection<LootTableCollection>> PerDlc(params LootTableCollection[] tables)
		=> new(new Dictionary<GameDlc, ReadOnlyCollection<LootTableCollection>>
		{
			[GameDlc.TitanQuest] = new ReadOnlyCollection<LootTableCollection>(tables),
		});

	private static ReadOnlyDictionary<GameDlc, ReadOnlyCollection<LootTableCollection>> NoTables()
		=> new(new Dictionary<GameDlc, ReadOnlyCollection<LootTableCollection>>());

	#endregion

	#region Set members

	[Fact]
	public void GetSetMemberChoices_ExcludesTheItemAndMissingInfo_SortedByLabel()
	{
		// Arrange
		var item = MakeItem("records/set/helm.dbr", Item.ICLASS_HEAD);
		var members = new Dictionary<string, Info>
		{
			["records/set/helm.dbr"] = MakeInfo("records/set/helm.dbr", "ItemRelic", description: "tagHelm"),
			["records/set/torso.dbr"] = MakeInfo("records/set/torso.dbr", "ItemRelic", description: "tagZTorso"),
			["records/set/greaves.dbr"] = MakeInfo("records/set/greaves.dbr", "ItemRelic", description: "tagAGreaves"),
			["records/set/lost.dbr"] = null!,
		};
		this._itemProvider.Setup(p => p.GetSetItems(item))
			.Returns(new SetItemInfo("set", "tagSet", members, new DBRecordCollection(RecordId.Create("set"), "ItemSet")));

		// Act
		var choices = this._service.GetSetMemberChoices(item);

		// Assert
		choices.Select(c => c.Label).Should().Equal("T:tagAGreaves", "T:tagZTorso");
		choices.Select(c => c.RecordId).Should().Equal("records/set/greaves.dbr", "records/set/torso.dbr");
	}

	[Fact]
	public void GetSetMemberChoices_NotASetItem_ReturnsEmpty()
	{
		var item = MakeItem("records/helm.dbr", Item.ICLASS_HEAD);
		this._itemProvider.Setup(p => p.GetSetItems(item)).Returns((SetItemInfo?)null);

		this._service.GetSetMemberChoices(item).Should().BeEmpty();
	}

	#endregion

	#region Bonuses

	[Fact]
	public void GetCompletionBonusChoices_Artifact_SortedByLabelWithCurrentMarked()
	{
		// Arrange
		var artifact = MakeItem("records/artifact.dbr", Item.ICLASS_ARTIFACT);
		artifact.RelicBonusId = RecordId.Create("records/bonus/b.dbr");
		var table = Table(
			("records/bonus/b.dbr", 1f, "Zeal")
			, ("records/bonus/a.dbr", 3f, "Agility")
		);
		this._itemProvider.Setup(p => p.BonusTableRelicOrArtifact(artifact)).Returns(table);

		// Act
		var choices = this._service.GetCompletionBonusChoices(artifact);

		// Assert
		choices.Should().HaveCount(2);
		choices[0].Label.Should().StartWith("Agility : ").And.Contain($"({0.75f:p2})");
		choices[0].IsCurrent.Should().BeFalse();
		choices[1].Label.Should().StartWith("Zeal : ");
		choices[1].Id.Should().Be(RecordId.Create("records/bonus/b.dbr"));
		choices[1].IsCurrent.Should().BeTrue();
	}

	[Fact]
	public void GetCompletionBonusChoices_IncompleteRelic_ReturnsEmptyWithoutReadingTheTable()
	{
		var record = new DBRecordCollection(RecordId.Create("records/relic.dbr"), Item.ICLASS_RELIC);
		record.Set(Text("Class", Item.ICLASS_RELIC));
		var level = new Variable("completedRelicLevel", VariableDataType.Integer, 1);
		level[0] = 5;
		record.Set(level);
		var relic = new Item
		{
			BaseItemId = RecordId.Create("records/relic.dbr"),
			baseItemInfo = new Info(record),
			Var1 = 1,
		};

		this._service.GetCompletionBonusChoices(relic).Should().BeEmpty();
		this._itemProvider.Verify(p => p.BonusTableRelicOrArtifact(It.IsAny<Item>()), Times.Never);
	}

	[Fact]
	public void GetCompletionBonusChoices_FlagsUnknownAndUntranslatedBonuses()
	{
		var artifact = MakeItem("records/artifact.dbr", Item.ICLASS_ARTIFACT);
		var unknownId = RecordId.Create("records/bonus/unknown.dbr");
		var untranslatedId = RecordId.Create("records/bonus/untranslated.dbr");
		var table = new LootTableCollection(RecordId.Create("table"), new()
		{
			[unknownId] = (1f, LootRandomizerItem.Default(unknownId) with { Tag = "tag", Unknown = true }),
			[untranslatedId] = (1f, LootRandomizerItem.Default(untranslatedId)),
		});
		this._itemProvider.Setup(p => p.BonusTableRelicOrArtifact(artifact)).Returns(table);

		var choices = this._service.GetCompletionBonusChoices(artifact);

		choices.Single(c => c.Id == unknownId).Unknown.Should().BeTrue();
		choices.Single(c => c.Id == untranslatedId).Should().Match<BonusChoice>(c => c.Untranslated && !c.Unknown);
	}

	[Fact]
	public void GetSocketedBonusChoices_MarksTheCurrentBonusOfEachSocket()
	{
		// Arrange
		var item = MakeItem("records/sword.dbr", "WeaponMelee_Sword");
		item.RelicBonusId = RecordId.Create("records/bonus/a.dbr");
		item.RelicBonus2Id = RecordId.Create("records/bonus/b.dbr");
		LootTableCollection? t1 = Table(("records/bonus/a.dbr", 1f, "A"), ("records/bonus/b.dbr", 1f, "B"));
		LootTableCollection? t2 = Table(("records/bonus/a.dbr", 1f, "A"), ("records/bonus/b.dbr", 1f, "B"));
		this._itemProvider.Setup(p => p.BonusTableSocketedRelic(item, out t1, out t2)).Returns(true);

		// Act
		var (relic1, relic2) = this._service.GetSocketedBonusChoices(item);

		// Assert
		relic1.Single(c => c.IsCurrent).Label.Should().StartWith("A : ");
		relic2.Single(c => c.IsCurrent).Label.Should().StartWith("B : ");
	}

	[Fact]
	public void GetSocketedBonusChoices_NoSocketedRelic_ReturnsEmptyLists()
	{
		var item = MakeItem("records/sword.dbr", "WeaponMelee_Sword");
		LootTableCollection? none = null;
		this._itemProvider.Setup(p => p.BonusTableSocketedRelic(item, out none, out none)).Returns(false);

		var (relic1, relic2) = this._service.GetSocketedBonusChoices(item);

		relic1.Should().BeEmpty();
		relic2.Should().BeEmpty();
	}

	#endregion

	#region Affixes

	[Fact]
	public void GetAffixCatalog_NotGear_ReturnsNull()
	{
		var relic = MakeItem("records/relic.dbr", Item.ICLASS_RELIC);

		this._service.GetAffixCatalog(relic, true).Should().BeNull();
		this._itemProvider.Verify(p => p.GetItemAffixes(It.IsAny<RecordId>()), Times.Never);
	}

	[Fact]
	public void GetAffixCatalog_GroupsByTypeAndName_KeepsHighestWeight()
	{
		// Arrange
		var ring = MakeItem("records/ring.dbr", Item.ICLASS_RING);
		var prefixA = Table(
			("records/affix/prefix/fire_01.dbr", 1f, "Of Fire")
			, ("records/affix/prefix/cold_01.dbr", 1f, "Arctic")
		);
		var prefixB = Table(
			("records/affix/prefix/fire_01.dbr", 3f, "Of Fire") // 75% here, 50% in prefixA
			, ("records/affix/prefix/fire_02.dbr", 1f, "Of Fire")
		);
		var suffix = Table(("records/affix/suffix/life_01.dbr", 1f, "Of Life"));
		this._itemProvider.Setup(p => p.GetItemAffixes(ring.BaseItemId))
			.Returns(new ItemAffixes(NoTables(), PerDlc(prefixA, prefixB), PerDlc(suffix)));

		// Act
		var catalog = this._service.GetAffixCatalog(ring, false)!;

		// Assert
		catalog.HasBroken.Should().BeFalse();
		catalog.Broken.Should().BeEmpty();
		catalog.HasPrefix.Should().BeTrue();
		catalog.Prefix.Select(g => g.Key.Translation).Should().Equal("Arctic", "Of Fire");
		var fire = catalog.Prefix[1];
		fire.Select(e => e.AffixId.Raw).Should().Equal("records/affix/prefix/fire_01.dbr", "records/affix/prefix/fire_02.dbr");
		fire.First().WeightPercent.Should().Be(0.75f);
		fire.Should().OnlyContain(e => e.TypeId == 1);
		catalog.Suffix.Single().Single().TypeId.Should().Be(2);
	}

	[Fact]
	public void GetAffixCatalog_EpicWithSettingOn_UsesAllAffixesOfTheGearType()
	{
		var ring = MakeItem("records/ring.dbr", Item.ICLASS_RING, "EPIC");
		this._itemProvider.Setup(p => p.GetAllAvailableAffixes(ring.GearType))
			.Returns(new ItemAffixes(NoTables(), NoTables(), NoTables()));

		var catalog = this._service.GetAffixCatalog(ring, true);

		catalog.Should().NotBeNull();
		this._itemProvider.Verify(p => p.GetAllAvailableAffixes(ring.GearType), Times.Once);
		this._itemProvider.Verify(p => p.GetItemAffixes(It.IsAny<RecordId>()), Times.Never);
	}

	[Fact]
	public void GetAffixCatalog_EpicWithSettingOff_UsesTheItemAffixes()
	{
		var ring = MakeItem("records/ring.dbr", Item.ICLASS_RING, "EPIC");
		this._itemProvider.Setup(p => p.GetItemAffixes(ring.BaseItemId))
			.Returns(new ItemAffixes(NoTables(), NoTables(), NoTables()));

		this._service.GetAffixCatalog(ring, false).Should().NotBeNull();
		this._itemProvider.Verify(p => p.GetAllAvailableAffixes(It.IsAny<GearType>()), Times.Never);
	}

	[Fact]
	public void GroupByEffect_RegroupsAcrossNames_SortedByEffectThenNumber()
	{
		// Arrange: two affix names sharing the "fire" effect, one with "cold"
		var ring = MakeItem("records/ring.dbr", Item.ICLASS_RING);
		var prefix = Table(
			("records/affix/fire_02.dbr", 1f, "Burning")
			, ("records/affix/cold_01.dbr", 1f, "Arctic")
			, ("records/affix/fire_01.dbr", 1f, "Of Fire")
		);
		this._itemProvider.Setup(p => p.GetItemAffixes(ring.BaseItemId))
			.Returns(new ItemAffixes(NoTables(), PerDlc(prefix), NoTables()));
		var byName = this._service.GetAffixCatalog(ring, false)!.Prefix;

		// Act
		var byEffect = this._service.GroupByEffect(byName);

		// Assert
		var fireEffect = RecordId.Create("records/affix/fire_01.dbr").PrettyFileNameExploded.Effect;
		var coldEffect = RecordId.Create("records/affix/cold_01.dbr").PrettyFileNameExploded.Effect;
		byEffect.Select(g => g.Key.Translation).Should().Equal(new[] { coldEffect, fireEffect }.Order());
		var fire = byEffect.Single(g => g.Key.Translation == fireEffect);
		fire.Select(e => e.AffixId.Raw).Should().Equal("records/affix/fire_01.dbr", "records/affix/fire_02.dbr");
		fire.First().FormattedText.Should().StartWith($"{fireEffect} : (").And.Contain("Of Fire");
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void FormatVariantLabel_ShowsTheOtherDimensionThanTheGroup(bool byEffect)
	{
		var id = RecordId.Create("records/affix/fire_02.dbr");
		var entry = new AffixEntry(1, id, GameDlc.TitanQuest, "Burning", 0.5f, string.Empty
			, LootRandomizerItem.Default(id) with { Translation = "Burning" });

		var label = this._service.FormatVariantLabel(entry, byEffect);

		var exploded = id.PrettyFileNameExploded;
		label.Should().StartWith($"({exploded.Number}) {(byEffect ? "Burning" : exploded.Effect)} ({0.5f:p2})");
	}

	#endregion
}
