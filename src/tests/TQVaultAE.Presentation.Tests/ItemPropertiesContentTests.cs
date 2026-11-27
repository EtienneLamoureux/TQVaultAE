using System.Drawing;
using AwesomeAssertions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Results;
using TQVaultAE.Presentation;

namespace TQVaultAE.Tests.Presentation;

public class ItemPropertiesContentTests
{
	private const string BaseId = """records\item\equipmentweapon\sword\u_sword.dbr""";
	private const string PrefixId = """records\item\lootmagicalaffixes\prefix\sharp.dbr""";
	private const string SuffixId = """records\item\lootmagicalaffixes\suffix\might.dbr""";

	#region Fixtures

	private static Item SwordItem()
	{
		var record = new DBRecordCollection(RecordId.Create(BaseId), Item.ICLASS_HEAD);
		var cls = new Variable("Class", VariableDataType.StringVar, 1);
		cls[0] = Item.ICLASS_HEAD;
		record.Set(cls);
		return new Item { BaseItemId = RecordId.Create(BaseId), baseItemInfo = new Info(record) };
	}

	private static ToFriendlyNameResult Sword()
		=> new(SwordItem())
		{
			BaseItemInfoDescription = "{^G}Sword",
			BaseItemId = RecordId.Create(BaseId),
			PrefixInfoRecords = new DBRecordCollection(RecordId.Create(PrefixId), "LootRandomizer"),
			SuffixInfoRecords = new DBRecordCollection(RecordId.Create(SuffixId), "LootRandomizer"),
			BaseAttributes = ["base 1", "{^Y}base 2"],
			PrefixAttributes = ["prefix 1"],
			SuffixAttributes = ["suffix 1"],
		};

	private static IEnumerable<(string Text, Color Color)> Rows(ItemPropertySection section)
		=> section.Rows.Select(r => (r.Text, r.Color));

	#endregion

	#region Build

	[Fact]
	public void Build_AlwaysReturnsBasePrefixSuffix_InThatOrder_WithTheirTitles()
	{
		// Act
		var content = ItemPropertiesContent.Build(Sword(), filterExtra: false);

		// Assert
		content.Sections.Select(s => (s.Kind, s.Title)).Should().Equal(
			(ItemPropertySectionKind.Base, Resources.ItemPropertiesLabelBaseItemProperties),
			(ItemPropertySectionKind.Prefix, Resources.ItemPropertiesLabelPrefixProperties),
			(ItemPropertySectionKind.Suffix, Resources.ItemPropertiesLabelSuffixProperties));
	}

	[Fact]
	public void Build_WithExtraInfo_StartsEachSectionWithItsRecordId_InTheRelicColor()
	{
		// Act
		var content = ItemPropertiesContent.Build(Sword(), filterExtra: false);

		// Assert
		var relic = ItemStyle.Relic.Color();
		Rows(content.Sections[0]).Should().Equal(
			(RecordId.Create(BaseId).Normalized, relic),
			("base 1", Color.White),
			("base 2", TQColor.Yellow.Color()));
		Rows(content.Sections[1]).Should().Equal((RecordId.Create(PrefixId).Normalized, relic), ("prefix 1", Color.White));
		Rows(content.Sections[2]).Should().Equal((RecordId.Create(SuffixId).Normalized, relic), ("suffix 1", Color.White));
		content.Sections.Should().OnlyContain(s => s.Visible);
	}

	[Fact]
	public void Build_FilteringExtraInfo_LeavesOutTheRecordIds()
	{
		// Act
		var content = ItemPropertiesContent.Build(Sword(), filterExtra: true);

		// Assert
		content.Sections.Select(s => s.Rows.Select(r => r.Text)).Should().BeEquivalentTo(
			[["base 1", "base 2"], new[] { "prefix 1" }, new[] { "suffix 1" }],
			o => o.WithStrictOrdering());
	}

	[Fact]
	public void Build_SectionWithoutAttributes_IsHidden_EvenWithARecord()
	{
		// Arrange: a prefix record but nothing to show for it.
		var data = Sword();
		data.PrefixAttributes = [];

		// Act
		var content = ItemPropertiesContent.Build(data, filterExtra: false);

		// Assert
		content.Sections[1].Visible.Should().BeFalse();
		content.Sections[1].Rows.Should().BeEmpty("the record id row comes only with attributes");
		content.Sections[0].Visible.Should().BeTrue();
		content.Sections[2].Visible.Should().BeTrue();
	}

	[Fact]
	public void Build_AttributesWithoutAffixRecord_StartWithAnEmptyIdRow()
	{
		// Arrange
		var data = Sword();
		data.SuffixInfoRecords = null;

		// Act
		var content = ItemPropertiesContent.Build(data, filterExtra: false);

		// Assert
		Rows(content.Sections[2]).Should().Equal((string.Empty, ItemStyle.Relic.Color()), ("suffix 1", Color.White));
	}

	[Fact]
	public void Build_Name_IsTheCleanFullName_InTheColorOfTheBaseDescription()
	{
		// Arrange
		var data = Sword();

		// Act
		var content = ItemPropertiesContent.Build(data, filterExtra: true);

		// Assert
		content.Name.Should().Be(data.FullNameClean).And.NotContain("{^");
		content.NameColor.Should().Be(TQColor.Green.Color());
	}

	[Fact]
	public void Build_Name_WithoutColorTag_TakesTheItemStyleColor()
	{
		// Arrange
		var data = Sword();
		data.BaseItemInfoDescription = "Sword";

		// Act
		var content = ItemPropertiesContent.Build(data, filterExtra: true);

		// Assert
		content.NameColor.Should().Be(data.Item.ItemStyle.Color());
	}

	#endregion

	#region Row

	[Fact]
	public void Row_LeadingColorTag_GivesTheColor_AndIsRemoved()
	{
		// Act
		var row = ItemPropertiesContent.Row("{^R}+10% Fire Damage", Color.White);

		// Assert
		row.Should().Be(new ItemPropertyRow("+10% Fire Damage", TQColor.Red.Color()));
	}

	[Fact]
	public void Row_WithoutColorTag_TakesTheDefaultColor()
	{
		// Act
		var row = ItemPropertiesContent.Row("+5 Strength", Color.Orange);

		// Assert
		row.Should().Be(new ItemPropertyRow("+5 Strength", Color.Orange));
	}

	[Fact]
	public void Row_TQNewLineTags_BecomeNewLines_BeforeTheColorTagIsRead()
	{
		// Act
		var row = ItemPropertiesContent.Row("{^Y}Line 1{^N}Line 2", Color.White);

		// Assert
		row.Should().Be(new ItemPropertyRow("Line 1\nLine 2", TQColor.Yellow.Color()));
	}

	#endregion
}
