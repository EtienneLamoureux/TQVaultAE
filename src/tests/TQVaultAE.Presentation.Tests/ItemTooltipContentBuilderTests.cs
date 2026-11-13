using AwesomeAssertions;
using Moq;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Config;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Results;
using TQVaultAE.Presentation;

namespace TQVaultAE.Tests.Presentation;

public class ItemTooltipContentBuilderTests
{
	private const ItemTooltipView Full = ItemTooltipView.Detailed | ItemTooltipView.InGameRolls | ItemTooltipView.ValueRanges
		| ItemTooltipView.GrantedSkills | ItemTooltipView.SetPieceCounts;

	private readonly ItemTooltipContentBuilder _builder;

	public ItemTooltipContentBuilderTests()
	{
		var translation = new Mock<ITranslationService>();
		translation.SetupGet(t => t.ItemIT).Returns("Immortal Throne Item");
		translation.SetupGet(t => t.ItemRagnarok).Returns("Ragnarok Item");
		translation.SetupGet(t => t.ItemAtlantis).Returns("Atlantis Item");
		translation.SetupGet(t => t.ItemEmbers).Returns("Eternal Embers Item");
		this._builder = new ItemTooltipContentBuilder(translation.Object);
	}

	#region Fixtures

	private static Info InfoOf(string id, string itemClass)
	{
		var record = new DBRecordCollection(RecordId.Create(id), itemClass);
		var cls = new Variable("Class", VariableDataType.StringVar, 1);
		cls[0] = itemClass;
		record.Set(cls);
		return new Info(record);
	}

	private static Item ItemOf(string id, string itemClass)
		=> new() { BaseItemId = RecordId.Create(id), baseItemInfo = InfoOf(id, itemClass) };

	private static ToFriendlyNameResult Sword(string baseId = """records\item\equipmentweapon\sword\u_sword.dbr""")
		=> new(ItemOf(baseId, Item.ICLASS_HEAD))
		{
			BaseItemInfoDescription = "{^G}Sword",
			PrefixInfoDescription = "Sharp",
			SuffixInfoDescription = "{^Y}of Might",
			BaseAttributes = ["base 1", "base 2"],
			PrefixAttributes = ["prefix 1"],
			SuffixAttributes = ["suffix 1"],
			BaseAttributeRanges = [null, "(10 - 20)"],
			ItemSeed = "Seed 42",
			Requirements = ["Level 10"],
		};

	private static IEnumerable<(TooltipLineKind Kind, string Text)> Shape(ItemTooltipContent content)
		=> content.Lines.Select(l => (l.Kind, l.Text));

	#endregion

	#region Build

	[Fact]
	public void Build_IncompleteItem_WarnsUnderTheName_ThenListsTheMissingRecords()
	{
		// Arrange: a suffix and a charm the loaded data lacks
		var data = Sword();
		data.Item.suffixID = RecordId.Create("""records\mod\suffix.dbr""");
		data.Item.relicID = RecordId.Create("""records\mod\charm.dbr""");

		// Act
		var lines = this._builder.Build(data, ItemTooltipView.Classic).Lines;

		// Assert
		lines[0].Text.Should().Be(data.FullName);
		lines[1].Kind.Should().Be(TooltipLineKind.Warning);
		lines[1].Text.Should().Be(TQVaultAE.Presentation.Resources.ItemIncomplete);
		lines[1].Color.Should().Be(ItemTooltipContentBuilder.IncompleteColor);
		lines[2].Text.Should().Be($"{TQVaultAE.Presentation.Resources.ItemIncompleteSuffix} : {data.Item.suffixID.Raw}");
		lines[3].Text.Should().Be($"{TQVaultAE.Presentation.Resources.ItemIncompleteRelic} : {data.Item.relicID.Raw}");
		lines[4].Text.Should().Be("base 1");
	}

	[Fact]
	public void Build_ItemLinkedToAMod_NamesItsModLastInTheSeedSection()
	{
		var data = Sword("""records\xpack4\items\swords\a01_sword.dbr"""); // an Atlantis item: it has an expansion clause
		data.Item.Mod = "shadowchampions";
		data.ItemSeed = "Seed 1";
		data.Requirements = ["Level 10"];

		var lines = this._builder.Build(data, ItemTooltipView.Classic).Lines.ToList();

		int seed = lines.FindIndex(l => l.Text == "Seed 1");
		lines[seed + 1].Color.Should().Be(TQColor.Green);
		lines[seed + 2].Text.Should().Be(string.Format(TQVaultAE.Presentation.Resources.ItemModLine, "shadowchampions"));
		lines[seed + 2].Color.Should().Be(ItemTooltipContentBuilder.ModColor);
		lines[seed + 3].Kind.Should().Be(TooltipLineKind.Delimiter);
		lines[1].Text.Should().NotContain("shadowchampions");
	}

	[Fact]
	public void Build_ItemLinkedToAModWithItsSource_NamesTheModOnly()
	{
		var data = Sword();
		data.Item.Mod = "shadowchampions|workshop:1949340853";

		var lines = this._builder.Build(data, ItemTooltipView.Classic).Lines;

		lines.Should().Contain(l => l.Text == string.Format(TQVaultAE.Presentation.Resources.ItemModLine, "shadowchampions"));
		lines.Should().NotContain(l => l.Text.Contains("workshop", StringComparison.Ordinal));
	}

	[Theory]
	[InlineData(Item.ICLASS_RING)]
	[InlineData(Item.ICLASS_AMULET)]
	public void Build_Classic_Jewellery_BlankLineBetweenNameAndBonuses(string itemClass)
	{
		var data = new ToFriendlyNameResult(ItemOf("""records\item\equipmentring\c01_ring01.dbr""", itemClass))
		{
			BaseItemInfoDescription = "Copper Band",
			ClassicAttributes = ["+3 Dexterity", "10% Fire Resistance"],
		};

		var shape = Shape(this._builder.Build(data, ItemTooltipView.Classic)).Take(4);

		shape.Should().Equal((TooltipLineKind.Text, data.FullName), (TooltipLineKind.Spacer, string.Empty)
			, (TooltipLineKind.Text, "+3 Dexterity"), (TooltipLineKind.Text, "10% Fire Resistance"));
	}

	[Fact]
	public void Build_Classic_Artifact_BlankLineBetweenItsClassAndBonuses()
	{
		var data = new ToFriendlyNameResult(ItemOf("""records\xpack\items\artifacts\a.dbr""", Item.ICLASS_ARTIFACT))
		{
			ArtifactClass = "Divine Artifact",
			ClassicAttributes = ["100% Sleep Resistance", "+1 to all Skills"],
		};

		var shape = Shape(this._builder.Build(data, ItemTooltipView.Classic)).Take(4);

		shape.Should().Equal((TooltipLineKind.Text, data.FullName), (TooltipLineKind.Text, "Divine Artifact")
			, (TooltipLineKind.Spacer, string.Empty), (TooltipLineKind.Text, "100% Sleep Resistance"));
	}

	[Fact]
	public void Build_Classic_NotJewellery_NoBlankLineUnderTheName()
	{
		var data = Sword();
		data.ClassicAttributes = ["base 1", "", "prefix 1"];

		var lines = this._builder.Build(data, ItemTooltipView.Classic).Lines;

		lines[1].Text.Should().Be("base 1");
	}

	[Fact]
	public void Build_BaseGameItem_NamesNoMod()
		=> this._builder.Build(Sword(), Full).Lines.Should().NotContain(l => l.Text == string.Format(TQVaultAE.Presentation.Resources.ItemModLine, string.Empty));

	[Fact]
	public void Build_CompleteItem_HasNoWarning()
		=> this._builder.Build(Sword(), Full).Lines.Should().NotContain(l => l.Kind == TooltipLineKind.Warning);

	[Fact]
	public void Build_Classic_ListsBaseThenPrefixThenSuffix_ThenSeedAndRequirements()
	{
		// Act
		var content = this._builder.Build(Sword(), ItemTooltipView.Classic);

		// Assert
		var data = Sword();
		Shape(content).Should().Equal(
			(TooltipLineKind.Text, data.FullName),
			(TooltipLineKind.Text, "base 1"),
			(TooltipLineKind.Text, "base 2"),
			(TooltipLineKind.Text, "prefix 1"),
			(TooltipLineKind.Text, "suffix 1"),
			(TooltipLineKind.Delimiter, ""),
			(TooltipLineKind.Text, "Seed 42"),
			(TooltipLineKind.Delimiter, ""),
			(TooltipLineKind.Text, "Level 10"));

		// The name is bold, in the color of the base description tag; the seed and requirements are gray.
		content.Lines[0].Should().Be(TooltipLine.Of(data.FullName, TQColor.Green, bold: true));
		content.Lines[6].Color.Should().Be(TQColor.DarkGray);
		content.Lines[8].Color.Should().Be(TQColor.DarkGray);
		content.RelicEnd.Should().Be(content.RelicStart, "no relic lines");
	}

	[Fact]
	public void Build_NameWithoutColorTag_TakesTheRarityColor()
	{
		var data = Sword();
		data.BaseItemInfoDescription = "Sword";

		var content = this._builder.Build(data, ItemTooltipView.Classic);

		content.Lines[0].Color.Should().Be(data.Item.ItemStyle.TQColor());
	}

	[Fact]
	public void Build_Detailed_SectionsPrefixBaseSuffix_WithTitlesAndRanges()
	{
		// Act
		var content = this._builder.Build(Sword(), ItemTooltipView.Detailed | ItemTooltipView.ValueRanges);

		// Assert
		var orange = TQColor.Orange.ColorTag();
		Shape(content).Take(10).Should().Equal(
			(TooltipLineKind.Text, Sword().FullName),
			(TooltipLineKind.Spacer, ""),
			// No color tag in the description: the title takes the relic color
			(TooltipLineKind.Text, $"{orange}{Resources.ItemPropertiesLabelPrefixProperties} : Sharp"),
			(TooltipLineKind.Text, "prefix 1"),
			(TooltipLineKind.Spacer, ""),
			(TooltipLineKind.Text, $"{{^G}}{Resources.ItemPropertiesLabelBaseItemProperties} : Sword"),
			(TooltipLineKind.Text, "base 1"),
			(TooltipLineKind.Range, "base 2"),
			(TooltipLineKind.Spacer, ""),
			(TooltipLineKind.Text, $"{{^Y}}{Resources.ItemPropertiesLabelSuffixProperties} : of Might"));
		content.Lines[2].Bold.Should().BeTrue();
		content.Lines[7].Range.Should().Be("(10 - 20)");
	}

	[Fact]
	public void Build_DetailedWithoutValueRanges_ShowsNoRange()
	{
		var content = this._builder.Build(Sword(), ItemTooltipView.Detailed);

		content.Lines.Should().NotContain(l => l.Kind == TooltipLineKind.Range);
		content.Lines.Should().Contain(TooltipLine.Of("base 2"));
	}

	[Fact]
	public void Build_EmptySection_IsLeftOut()
	{
		var data = Sword();
		data.PrefixAttributes = [];

		var content = this._builder.Build(data, ItemTooltipView.Detailed);

		content.Lines.Should().NotContain(l => l.Text.Contains(Resources.ItemPropertiesLabelPrefixProperties));
	}

	[Theory]
	[InlineData(ItemTooltipView.Classic, false)]
	[InlineData(ItemTooltipView.Detailed, false)]
	[InlineData(ItemTooltipView.Detailed | ItemTooltipView.InGameRolls, true)]
	[InlineData(ItemTooltipView.Classic | ItemTooltipView.InGameRolls, true)]
	public void Build_InGameRolls_OnlyWithTheirFlag(ItemTooltipView view, bool shown)
	{
		var data = Sword();
		data.InGameRolls = ["+10 Strength -> +11 Strength"];

		var content = this._builder.Build(data, view);

		content.Lines.Any(l => l.Text == Resources.ItemPropertiesLabelInGameRolls && l.Bold && l.Color == TQColor.Orange)
			.Should().Be(shown);
		content.Lines.Any(l => l.Text == "+10 Strength -> +11 Strength" && l.Color == TQColor.DarkGray)
			.Should().Be(shown);
	}

	[Fact]
	public void Build_ClassicWithValueRanges_ShowsTheRangeOfEachMergedLine()
	{
		var data = Sword();
		data.ClassicAttributes = ["base 1", "base 2", "", "prefix 1", "suffix 1"];
		data.ClassicAttributeRanges = [null, "(10 - 20)", null, "(1 - 3)", null];

		var content = this._builder.Build(data, ItemTooltipView.Classic | ItemTooltipView.ValueRanges);

		Shape(content).Skip(1).Take(5).Should().Equal(
			(TooltipLineKind.Text, "base 1"),
			(TooltipLineKind.Range, "base 2"),
			(TooltipLineKind.Text, ""),
			(TooltipLineKind.Range, "prefix 1"),
			(TooltipLineKind.Text, "suffix 1"));
		content.Lines[2].Range.Should().Be("(10 - 20)");
		content.Lines[4].Range.Should().Be("(1 - 3)");
		content.Lines.Should().NotContain(l => l.Text.Contains(Resources.ItemPropertiesLabelBaseItemProperties), "the classic view has no sections");
	}

	[Fact]
	public void Build_ClassicWithValueRanges_WithoutMergedLines_TakesTheRangesOfEachRecord()
	{
		// Sword(): base ranges [null, "(10 - 20)"], no prefix or suffix ranges, no merged lines.
		var data = Sword();
		data.SuffixAttributeRanges = ["(5 - 9)"];

		var content = this._builder.Build(data, ItemTooltipView.Classic | ItemTooltipView.ValueRanges);

		Shape(content).Skip(1).Take(4).Should().Equal(
			(TooltipLineKind.Text, "base 1"),
			(TooltipLineKind.Range, "base 2"),
			(TooltipLineKind.Text, "prefix 1"),
			(TooltipLineKind.Range, "suffix 1"));
		content.Lines[4].Range.Should().Be("(5 - 9)");
	}

	[Fact]
	public void Build_ClassicWithoutValueRanges_ShowsNoRange()
	{
		var data = Sword();
		data.ClassicAttributes = ["base 1", "base 2"];
		data.ClassicAttributeRanges = [null, "(10 - 20)"];

		var content = this._builder.Build(data, ItemTooltipView.Classic);

		content.Lines.Should().NotContain(l => l.Kind == TooltipLineKind.Range);
		content.Lines.Should().Contain(TooltipLine.Of("base 2"));
	}

	[Theory]
	[InlineData("""records\item\equipmentweapon\sword\u_sword.dbr""", null)]
	[InlineData("""records\xpack\items\sword.dbr""", "Immortal Throne Item")]
	[InlineData("""records\xpack2\items\sword.dbr""", "Ragnarok Item")]
	[InlineData("""records\xpack3\items\sword.dbr""", "Atlantis Item")]
	[InlineData("""records\xpack4\items\sword.dbr""", "Eternal Embers Item")]
	public void Build_ExpansionClause_FollowsTheSeed(string baseId, string? clause)
	{
		var content = this._builder.Build(Sword(baseId), ItemTooltipView.Classic);

		var seed = content.Lines.ToList().FindIndex(l => l.Text == "Seed 42");
		var next = content.Lines[seed + 1];
		if (clause is null)
			next.Kind.Should().Be(TooltipLineKind.Delimiter);
		else
			next.Should().Be(TooltipLine.Of(clause, TQColor.Green));
	}

	[Fact]
	public void Build_ExpansionClause_LatestExpansionWins()
	{
		// A base of Immortal Throne with an affix of Eternal Embers is an Eternal Embers item.
		var data = Sword("""records\xpack\items\sword.dbr""");
		data.Item.prefixID = RecordId.Create("""records\xpack4\items\lootaffixes\prefix.dbr""");

		var content = this._builder.Build(data, ItemTooltipView.Classic);

		content.Lines.Select(l => l.Text).Should().Contain("Eternal Embers Item")
			.And.NotContain("Immortal Throne Item");
	}

	[Fact]
	public void Build_SocketedRecord_AddsItsBlockAsTheRelicLines()
	{
		// Arrange: anything in the relic slot (God mode) shows its block
		var data = Sword();
		data.Item.relicID = RecordId.Create("""records\item\equipmentweapon\axe\u_axe.dbr""");
		data.Item.RelicInfo = InfoOf("""records\item\equipmentweapon\axe\u_axe.dbr""", Item.ICLASS_HEAD);
		data.RelicInfo1Description = "Axe";
		data.Relic1Attributes = ["relic 1"];
		data.RelicBonus1Attributes = ["bonus 1"];

		// Act
		var content = this._builder.Build(data, ItemTooltipView.Classic);

		// Assert
		var relic = content.Lines.Skip(content.RelicStart).Take(content.RelicEnd - content.RelicStart).ToList();
		relic.Select(l => l.Kind).Should().Equal(
			TooltipLineKind.Delimiter, TooltipLineKind.Text, TooltipLineKind.Text, TooltipLineKind.Text, TooltipLineKind.Text, TooltipLineKind.Text);
		relic[1].Should().Be(TooltipLine.Of("Axe", TQColor.Orange, bold: true));
		relic[2].Should().Be(TooltipLine.Of(data.RelicInfo1CompletionResolved, TQColor.Orange));
		relic[3].Should().Be(TooltipLine.Of("relic 1"));
		relic[4].Should().Be(TooltipLine.Of(data.RelicInfo1CompletionBonusResolved, TQColor.Orange));
		relic[5].Should().Be(TooltipLine.Of("bonus 1"));
		content.Lines[content.RelicEnd].Kind.Should().Be(TooltipLineKind.Delimiter, "the seed section follows");
	}

	[Fact]
	public void Build_Relic_ShowsItsCompletion_ThenItsBonusAsRelicLines()
	{
		// Arrange
		var data = new ToFriendlyNameResult(ItemOf("""records\item\relics\relic.dbr""", Item.ICLASS_RELIC))
		{
			BaseItemInfoDescription = "Relic",
			RelicCompletionFormat = "Completed",
			RelicBonusTitle = "Completion Bonus :",
			RelicBonus1Attributes = ["+5 Strength"],
			ItemSeed = "Seed 1",
		};

		// Act
		var content = this._builder.Build(data, ItemTooltipView.Classic);

		// Assert
		content.Lines[1].Should().Be(TooltipLine.Of("Completed", TQColor.Orange));
		content.Lines.Skip(content.RelicStart).Take(content.RelicEnd - content.RelicStart).Should().Equal(
			TooltipLine.Spacer,
			TooltipLine.Of("Completion Bonus :", TQColor.Orange),
			TooltipLine.Of("+5 Strength"));
	}

	[Fact]
	public void Build_Artifact_ShowsItsClassAndBonus()
	{
		var data = new ToFriendlyNameResult(ItemOf("""records\xpack\items\artifacts\a.dbr""", Item.ICLASS_ARTIFACT))
		{
			ArtifactClass = "Lesser Artifact",
			ArtifactBonus = "Completion Bonus",
			RelicBonus1Attributes = ["+1 Skill"],
		};

		var content = this._builder.Build(data, ItemTooltipView.Classic);

		content.Lines[1].Should().Be(TooltipLine.Of("Lesser Artifact"));
		content.Lines.Skip(content.RelicStart).Take(content.RelicEnd - content.RelicStart).Should().Equal(
			TooltipLine.Spacer,
			TooltipLine.Of("Completion Bonus", TQColor.Orange),
			TooltipLine.Of("+1 Skill"));
	}

	[Fact]
	public void Build_Formula_ShowsRecipe_Reagents_AndTheArtifact()
	{
		var data = new ToFriendlyNameResult(ItemOf("""records\xpack\items\artifacts\arcaneformulae\f.dbr""", Item.ICLASS_FORMULA))
		{
			BaseItemInfoDescription = "Formula",
			ArtifactRecipe = "Recipe",
			FormulaeFormat = "Reagents",
			FormulaeArtifactName = "Artifact",
			FormulaeArtifactClass = "Greater Artifact",
			FormulaeArtifactAttributes = ["+10 Health"],
			ItemSeed = "Seed 1",
		};

		var content = this._builder.Build(data, ItemTooltipView.Classic);

		var nameColor = data.Item.ItemStyle.TQColor();
		content.Lines.Take(9).Should().Equal(
			TooltipLine.Of(data.FullName, nameColor, bold: true),
			TooltipLine.Of("Recipe"),
			TooltipLine.Spacer,
			TooltipLine.Of("Reagents"),
			TooltipLine.Spacer,
			TooltipLine.Of("Artifact", nameColor, bold: true),
			TooltipLine.Of("Greater Artifact", TQColor.DarkGray),
			TooltipLine.Of("+10 Health"),
			TooltipLine.Delimiter);
	}

	[Fact]
	public void Build_FlavorText_IsFollowedByASpacer()
	{
		var data = Sword();
		data.FlavorText = ["An old blade."];

		var content = this._builder.Build(data, ItemTooltipView.Classic);

		content.Lines.Skip(1).Take(2).Should().Equal(TooltipLine.Of("An old blade."), TooltipLine.Spacer);
	}

	private static SetItemInfo Set(ToFriendlyNameResult data)
	{
		var members = new Dictionary<string, Info>
		{
			["""records\set\helm.dbr"""] = InfoOf("""records\set\helm.dbr""", Item.ICLASS_HEAD),
			["""records\set\torso.dbr"""] = InfoOf("""records\set\torso.dbr""", Item.ICLASS_UPPERBODY),
		};
		var set = new SetItemInfo("set", "tagSet", members, new DBRecordCollection(RecordId.Create("set"), "ItemSet"));
		set.Translations["tagSet"] = "Set of the Bear";
		set.Translations["""records\set\helm.dbr"""] = "Bear Helm";
		set.Translations["""records\set\torso.dbr"""] = "Bear Torso";
		data.ItemSet = set;
		return set;
	}

	[Fact]
	public void Build_SetPieceCounts_ShowsTheCountOfEachPieceOnItsRight()
	{
		var data = Sword();
		Set(data);
		var counts = new Dictionary<RecordId, int> { [RecordId.Create("""records\set\helm.dbr""")] = 2 };

		var content = this._builder.Build(data, Full, counts);

		// The set name and a piece without a count stay plain lines.
		content.Lines.SkipWhile(l => l.Text != "Set of the Bear").Take(3).Should().Equal(
			TooltipLine.Of("Set of the Bear"),
			TooltipLine.WithRange("Bear Helm", "2"),
			TooltipLine.Of("Bear Torso"));
	}

	[Fact]
	public void Build_ClassicWithSetPieceCounts_ShowsTheCountOfEachPieceOnItsRight()
	{
		var data = Sword();
		Set(data);
		var counts = new Dictionary<RecordId, int> { [RecordId.Create("""records\set\torso.dbr""")] = 3 };

		var content = this._builder.Build(data, ItemTooltipView.Classic | ItemTooltipView.SetPieceCounts, counts);

		content.Lines.SkipWhile(l => l.Text != "Set of the Bear").Take(3).Should().Equal(
			TooltipLine.Of("Set of the Bear"),
			TooltipLine.Of("Bear Helm"),
			TooltipLine.WithRange("Bear Torso", "3"));
	}

	[Fact]
	public void Build_SetWithoutCounts_ListsThePieces()
	{
		var data = Sword();
		Set(data);

		var content = this._builder.Build(data, ItemTooltipView.Classic);

		var start = content.Lines.ToList().FindIndex(l => l.Text == "Set of the Bear");
		content.Lines[start - 1].Kind.Should().Be(TooltipLineKind.Delimiter);
		content.Lines.Skip(start).Take(3).Select(l => l.Kind).Should().AllBeEquivalentTo(TooltipLineKind.Text);
	}

	#endregion

	#region Skills

	[Fact]
	public void BuildSkill_ActivationDescriptionThenEffects_EmptyEffectLinesAsSpacers()
	{
		var skill = new GrantedSkill(RecordId.Create("skill"), "Thunder", 3, "Level 3", null, "Activated on attack",
			["Calls lightning."], ["10 Damage", "", "2 Second(s)"]);

		var content = ItemTooltipContentBuilder.BuildSkill(skill);

		content.Name.Should().Be(TooltipLine.Of("Thunder", TQColor.Blue, bold: true));
		content.Level.Should().Be(TooltipLine.Of("Level 3", TQColor.DarkGray));
		content.Body.Should().Equal(
			TooltipLine.Of("Activated on attack", TQColor.White),
			TooltipLine.Of("Calls lightning.", TQColor.White),
			TooltipLine.Spacer,
			TooltipLine.Of("10 Damage"),
			TooltipLine.Spacer,
			TooltipLine.Of("2 Second(s)"));
	}

	[Fact]
	public void BuildSkill_WithoutEffects_HasNoTrailingSpacer()
	{
		var skill = new GrantedSkill(RecordId.Create("skill"), "Aura", 1, "Level 1", null, null, ["Passive."], []);

		ItemTooltipContentBuilder.BuildSkill(skill).Body.Should().Equal(TooltipLine.Of("Passive.", TQColor.White));
	}

	[Theory]
	[InlineData(new[] { 100, 100, 100 }, 400, new[] { 0, 0, 0 })]
	[InlineData(new[] { 100, 100, 100 }, 250, new[] { 0, 0, 1 })]
	[InlineData(new[] { 500, 100 }, 250, new[] { 0, 1 })]
	[InlineData(new[] { 100, 500, 100 }, 250, new[] { 0, 1, 2 })]
	public void PackColumns_StartsANewColumnWhenTheNextBlockDoesNotFit(int[] heights, int maxHeight, int[] expected)
		=> ItemTooltipContentBuilder.PackColumns(heights, 10, maxHeight).Should().Equal(expected);

	[Fact]
	public void PackColumns_CountsTheSeparatorBetweenBlocks()
		// 100 + 10 + 100 = 210 fits in 210, a third block does not
		=> ItemTooltipContentBuilder.PackColumns([100, 100, 100], 10, 210).Should().Equal(0, 0, 1);

	#endregion

	#region View

	private static UserSettings NoExtra(bool detailed) => new()
	{
		EnableDetailedTooltipView = detailed,
		TooltipShowInGameRolls = false,
		TooltipShowValueRanges = false,
		TooltipShowGrantedSkills = false,
		TooltipShowSetPieceCounts = false,
	};

	[Fact]
	public void ViewOf_DetailedOff_IsClassicWithTheCheckedExtras()
	{
		var settings = NoExtra(false);
		settings.TooltipShowInGameRolls = true;
		settings.TooltipShowGrantedSkills = true;

		ItemTooltipContentBuilder.ViewOf(settings)
			.Should().Be(ItemTooltipView.Classic | ItemTooltipView.InGameRolls | ItemTooltipView.GrantedSkills);
	}

	[Theory]
	[InlineData(false, ItemTooltipView.Classic)]
	[InlineData(true, ItemTooltipView.Detailed)]
	public void ViewOf_NoExtraChecked_IsTheBareView(bool detailed, ItemTooltipView expected)
		=> ItemTooltipContentBuilder.ViewOf(NoExtra(detailed)).Should().Be(expected);

	[Theory]
	[InlineData(nameof(UserSettings.TooltipShowInGameRolls), ItemTooltipView.InGameRolls)]
	[InlineData(nameof(UserSettings.TooltipShowValueRanges), ItemTooltipView.ValueRanges)]
	[InlineData(nameof(UserSettings.TooltipShowGrantedSkills), ItemTooltipView.GrantedSkills)]
	[InlineData(nameof(UserSettings.TooltipShowSetPieceCounts), ItemTooltipView.SetPieceCounts)]
	public void ViewOf_Classic_EachExtraFollowsItsSetting(string option, ItemTooltipView flag)
	{
		var settings = NoExtra(false);
		typeof(UserSettings).GetProperty(option)!.SetValue(settings, true);

		ItemTooltipContentBuilder.ViewOf(settings).Should().Be(ItemTooltipView.Classic | flag);
	}

	[Fact]
	public void ViewOf_SessionForcesDetailed_ShowsItWithoutTheSetting_ExplicitChoiceWins()
	{
		// The forge forces the detailed display for the session; the setting stays off, so a save cannot keep it.
		var settings = new UserSettings
		{
			EnableDetailedTooltipView = false,
			TooltipShowInGameRolls = false,
			TooltipShowValueRanges = true,
			TooltipShowGrantedSkills = false,
			TooltipShowSetPieceCounts = false,
		};

		ItemTooltipContentBuilder.ViewOf(settings, sessionForcesDetailed: true)
			.Should().Be(ItemTooltipView.Detailed | ItemTooltipView.ValueRanges);
		ItemTooltipContentBuilder.ViewOf(settings, enableDetailedTooltipView: false, sessionForcesDetailed: true)
			.Should().Be(ItemTooltipView.Classic | ItemTooltipView.ValueRanges);
		settings.EnableDetailedTooltipView.Should().BeFalse();
	}

	[Fact]
	public void ViewOf_Detailed_AddsTheCheckedExtras()
	{
		var settings = new UserSettings
		{
			EnableDetailedTooltipView = true,
			TooltipShowInGameRolls = false,
			TooltipShowValueRanges = true,
			TooltipShowGrantedSkills = false,
			TooltipShowSetPieceCounts = true,
		};

		ItemTooltipContentBuilder.ViewOf(settings)
			.Should().Be(ItemTooltipView.Detailed | ItemTooltipView.ValueRanges | ItemTooltipView.SetPieceCounts);
	}

	[Theory]
	// With the default extras: only the item skills.
	[InlineData(true, false, ItemTooltipView.GrantedSkills)]
	[InlineData(false, true, ItemTooltipView.Detailed | ItemTooltipView.GrantedSkills)]
	public void ViewOf_ForcedDisplay_OverridesTheSetting(bool setting, bool forced, ItemTooltipView expected)
		=> ItemTooltipContentBuilder.ViewOf(new UserSettings { EnableDetailedTooltipView = setting }, forced).Should().Be(expected);

	[Theory]
	[InlineData(ItemTooltipView.Classic, FriendlyNamesExtraScopes.ItemFullDisplay)]
	[InlineData(ItemTooltipView.Detailed, FriendlyNamesExtraScopes.ItemFullDisplay)]
	[InlineData(ItemTooltipView.Detailed | ItemTooltipView.GrantedSkills, FriendlyNamesExtraScopes.ItemFullDisplay | FriendlyNamesExtraScopes.GrantedSkills)]
	[InlineData(ItemTooltipView.Classic | ItemTooltipView.GrantedSkills, FriendlyNamesExtraScopes.ItemFullDisplay | FriendlyNamesExtraScopes.GrantedSkills)]
	public void ScopesFor_AsksForTheGrantedSkillsOnlyForTheSkillsPanel(ItemTooltipView view, FriendlyNamesExtraScopes expected)
		=> ItemTooltipContentBuilder.ScopesFor(view).Should().Be(expected);

	#endregion
}
