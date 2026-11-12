using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Config;
using TQVaultAE.Data;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Domain.Results;

namespace TQVaultAE.Tests.Data;

/// <summary>
/// Tests the attribute lines that <see cref="ItemProvider.GetFriendlyNames"/> builds for item tooltips,
/// with the real <see cref="ItemAttributeProvider"/> and translations in the game's own format.
/// </summary>
public class ItemProviderAttributeDisplayTests
{
	private delegate bool TryTranslate(string tag, out string translation, bool removeAllTQTags, bool doCleanup, bool doCleanupKeepLeadingColorTag);

	private static readonly string Epic = ItemStyle.Epic.TQColor().ColorTag();
	private static readonly string Mundane = ItemStyle.Mundane.TQColor().ColorTag();
	private static readonly string Legendary = ItemStyle.Legendary.TQColor().ColorTag();

	private readonly Dictionary<string, string> _translations = new(StringComparer.OrdinalIgnoreCase)
	{
		["DamageSingleFormat"] = "{%.0f0}",
		["DamageRangeFormat"] = "{%.0f0}-{%.0f1}",
		["DamageSingleFormatTime"] = "over {%.0f0} Seconds",
		["ChanceOfTag"] = "{%.0f0}% Chance of",
		["characterStrength"] = "{%+.0f0} Strength",
		["characterLife"] = "{%+.0f0} Health",
		["DamageBasePhysical"] = "Physical Damage",
		["DamageDurationFire"] = "Burn Damage",
		["DamageStun"] = "Stun",
		["DamageInfluenceSingleFormat"] = "{%.1f0} Seconds of",
		["tagDamageAbsorptionTitle"] = "Protects Against :",
		["formatQualifyingDamage"] = "{%s0}",
		["tagQualifyingDamageFire"] = "Fire",
		["tagQualifyingDamageCold"] = "Cold",
		["MenuLevel"] = "Level: {%d0}",
		["tagItemGrantSkill"] = "Grants Skill :",
		["tagSkillTest"] = "Test Skill",
		["tagSkillTestDescription"] = "Heals you.",
	};

	private readonly Mock<IDatabase> _mockDatabase = new();
	private readonly ItemProvider _provider;

	public ItemProviderAttributeDisplayTests()
	{
		var mockTranslation = new Mock<ITranslationService>();
		mockTranslation.SetupGet(t => t.ItemSeed).Returns("Seed {0} ({1:0.00})");
		mockTranslation.Setup(t => t.TryTranslateXTag(It.IsAny<string>(), out It.Ref<string>.IsAny, It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
			.Returns(new TryTranslate((string tag, out string translation, bool _, bool _, bool _) =>
			{
				translation = this._translations.GetValueOrDefault(tag ?? string.Empty, string.Empty);
				return translation.Length > 0;
			}));

		this._mockDatabase.Setup(d => d.VariableToStringNice(It.IsAny<Variable>())).Returns((Variable v) => v.ToString());

		this._provider = new ItemProvider(
			Mock.Of<ILogger<ItemProvider>>(),
			this._mockDatabase.Object,
			Mock.Of<ILootTableCollectionProvider>(),
			new ItemAttributeProvider(Mock.Of<ILogger<ItemAttributeProvider>>()),
			Mock.Of<ITQDataService>(),
			mockTranslation.Object,
			Mock.Of<IPathIO>(),
			new UserSettings { ShowInGameValues = false }
		);
	}

	[Fact]
	public void StatBonus_UsesTheLabelAsFormat_AndIsMagical()
	{
		var item = this.CreateItem("ArmorJewelry_Ring", Int("characterStrength", 15));

		var lines = this.BaseAttributes(item);

		lines.Should().Equal($"{Epic}+15 Strength");
	}

	[Fact]
	public void NegativeFloatBonus_DropsThePlusSign_InsteadOfShowingPlusMinus()
	{
		// Fix#246: "{0:+#0}" with -10 would print "+-10".
		var item = this.CreateItem("ArmorJewelry_Ring", Float("characterStrength", -10f));

		var lines = this.BaseAttributes(item);

		lines.Should().Equal($"{Epic}-10 Strength");
	}

	[Fact]
	public void ZeroValuedAttribute_IsNotShown()
	{
		var item = this.CreateItem("ArmorJewelry_Ring", Int("characterStrength", 0), Int("characterLife", 40));

		var lines = this.BaseAttributes(item);

		lines.Should().Equal($"{Epic}+40 Health");
	}

	[Fact]
	public void WeaponBaseDamageRange_IsMundane()
	{
		var item = this.CreateItem("WeaponMelee_Sword", Float("offensivePhysicalMin", 10f), Float("offensivePhysicalMax", 20f));

		var lines = this.BaseAttributes(item);

		lines.Should().Equal($"{Mundane}10-20 Physical Damage");
	}

	[Fact]
	public void PhysicalDamageOnJewelry_IsMagical()
	{
		var item = this.CreateItem("ArmorJewelry_Ring", Float("offensivePhysicalMin", 10f), Float("offensivePhysicalMax", 20f));

		var lines = this.BaseAttributes(item);

		lines.Should().Equal($"{Epic}10-20 Physical Damage");
	}

	[Fact]
	public void EqualMinAndMax_ShowsASingleValue()
	{
		var item = this.CreateItem("ArmorJewelry_Ring", Float("offensivePhysicalMin", 12f), Float("offensivePhysicalMax", 12f));

		var lines = this.BaseAttributes(item);

		lines.Should().Equal($"{Epic}12 Physical Damage");
	}

	[Fact]
	public void DamageOverTime_ShowsTheTotal_ValueTimesDuration()
	{
		var item = this.CreateItem("ArmorJewelry_Ring", Float("offensiveSlowFireMin", 5f), Float("offensiveSlowFireDurationMin", 3f));

		var lines = this.BaseAttributes(item);

		lines.Should().Equal($"{Epic}15 Burn Damage over 3 Seconds");
	}

	[Fact]
	public void ChanceOfEffect_PutsTheChanceFirst()
	{
		var item = this.CreateItem("ArmorJewelry_Ring", Float("offensiveStunMin", 2f), Float("offensiveStunChance", 25f));

		var lines = this.BaseAttributes(item);

		lines.Should().Equal($"{Epic}25% Chance of 2.0 Seconds of Stun");
	}

	[Fact]
	public void DamageQualifiers_ShowTheTitleOnce_ThenOneLinePerDamageType()
	{
		var item = this.CreateItem("ArmorProtective_UpperBody", Int("fireDamageQualifier", 1), Int("coldDamageQualifier", 1));

		var lines = this.BaseAttributes(item);

		lines.Should().HaveCount(3);
		lines[0].Should().Be("Protects Against :");
		lines.Skip(1).Should().BeEquivalentTo([$"{Mundane}Fire", $"{Mundane}Cold"]);
	}

	[Fact]
	public void MissingLabelTranslation_ShowsTheTagBetweenQuestionMarks()
	{
		this._translations.Remove("characterLife");
		var item = this.CreateItem("ArmorJewelry_Ring", Int("characterLife", 40));

		var lines = this.BaseAttributes(item);

		lines.Should().Equal($"{Epic}40 {Legendary}?characterLife?");
	}

	[Fact]
	public void UnfilteredRecord_KeepsRequirements_FilteredRecordDropsThem()
	{
		var item = this.CreateItem("ArmorJewelry_Ring", Int("characterLife", 40), Int("levelRequirement", 12));

		var filtered = this.BaseAttributes(item, filterExtra: true);
		var unfiltered = this.BaseAttributes(item, filterExtra: false);

		filtered.Should().Equal($"{Epic}+40 Health");
		unfiltered.Should().Contain($"{Epic}+40 Health");
		unfiltered.Should().Contain(line => line.Contains("levelRequirement", StringComparison.Ordinal));
	}

	[Fact]
	public void MissingRecord_ShowsNoBaseAttributes()
	{
		var item = this.CreateItem("ArmorJewelry_Ring", Int("characterLife", 40));
		this._mockDatabase.Setup(d => d.GetRecordFromFile(It.IsAny<RecordId>())).Returns((DBRecordCollection?)null);

		var lines = this.BaseAttributes(item);

		lines.Should().BeEmpty();
	}

	[Fact]
	public void GrantedBuffSkill_TakesItsNameAndEffectsFromTheBuff_AtTheGrantedLevel()
	{
		this.CreateRecord("""records\test\skill.dbr""", "Skill_BuffRadiusToggled", Str("buffSkillName", """records\test\buff.dbr"""));
		this.CreateRecord("""records\test\buff.dbr""", "SkillBuff_Passive",
			Str("skillDisplayName", "tagSkillTest"),
			Str("skillBaseDescription", "tagSkillTestDescription"),
			Ints("characterLife", 10, 20, 30));
		var item = this.CreateItem("ArmorJewelry_Amulet", Str("itemSkillName", """records\test\skill.dbr"""), Int("itemSkillLevel", 2));

		var skills = this.GrantedSkills(item);

		skills.Should().ContainSingle();
		skills[0].Name.Should().Be("Test Skill");
		skills[0].Level.Should().Be(2);
		skills[0].LevelText.Should().Be("Level: 2");
		skills[0].Description.Should().Equal("Heals you.");
		skills[0].Attributes.Should().Equal($"{Epic}+20 Health");
	}

	[Fact]
	public void GrantedSkillShownApart_IsOnlyNamedInTheItemAttributes()
	{
		this.CreateRecord("""records\test\skill.dbr""", "Skill_BuffSelfDuration",
			Str("skillDisplayName", "tagSkillTest"),
			Str("skillBaseDescription", "tagSkillTestDescription"),
			Ints("characterLife", 10));
		var item = this.CreateItem("ArmorJewelry_Amulet",
			Str("itemSkillName", """records\test\skill.dbr"""),
			Str("itemSkillAutoController", """records\test\controller.dbr"""),
			Int("itemSkillLevel", 1));

		var inline = this._provider.GetFriendlyNames(item, FriendlyNamesExtraScopes.BaseAttributes).BaseAttributes;
		var apart = this._provider.GetFriendlyNames(item, FriendlyNamesExtraScopes.BaseAttributes | FriendlyNamesExtraScopes.GrantedSkills);

		inline.Should().Contain(line => line.Contains("Heals you.", StringComparison.Ordinal));
		apart.BaseAttributes.Should().Contain(line => line.Contains("Test Skill", StringComparison.Ordinal));
		apart.BaseAttributes.Should().NotContain(line => line.Contains("Heals you.", StringComparison.Ordinal));
		apart.GrantedSkills.Should().ContainSingle().Which.Attributes.Should().Equal($"{Epic}+10 Health");
	}

	[Fact]
	public void UnnamedGrantedSkill_IsNotListed()
	{
		this.CreateRecord("""records\test\skill.dbr""", "Skill_Passive", Ints("characterLife", 10));
		var item = this.CreateItem("ArmorJewelry_Amulet", Str("itemSkillName", """records\test\skill.dbr"""), Int("itemSkillLevel", 1));

		this.GrantedSkills(item).Should().BeEmpty();
	}

	[Fact]
	public void HasGrantedSkill_IsTrue_ForANamedSkillGrantedByARelic()
	{
		this.CreateRecord("""records\test\skill.dbr""", "Skill_Passive", Str("skillDisplayName", "tagSkillTest"));
		this.CreateRecord("""records\test\relic.dbr""", "ItemRelic", Str("itemSkillName", """records\test\skill.dbr"""));
		var item = this.CreateItem("ArmorJewelry_Amulet", Int("characterLife", 40));
		item.relicID = RecordId.Create("""records\test\relic.dbr""");

		this._provider.HasGrantedSkill(item).Should().BeTrue();
	}

	[Fact]
	public void HasGrantedSkill_IsTrue_ForABuffSkillNamedByItsBuff()
	{
		this.CreateRecord("""records\test\skill.dbr""", "Skill_BuffRadiusToggled", Str("buffSkillName", """records\test\buff.dbr"""));
		this.CreateRecord("""records\test\buff.dbr""", "SkillBuff_Passive", Str("skillDisplayName", "tagSkillTest"));
		var item = this.CreateItem("ArmorJewelry_Amulet", Str("itemSkillName", """records\test\skill.dbr"""));

		this._provider.HasGrantedSkill(item).Should().BeTrue();
	}

	[Fact]
	public void HasGrantedSkill_IsFalse_WithoutSkill_OrWithAnUnnamedOrMissingOne()
	{
		this.CreateRecord("""records\test\hidden.dbr""", "Skill_Passive", Ints("characterLife", 10));
		var plain = this.CreateItem("ArmorJewelry_Ring", Int("characterLife", 40));
		var hidden = this.CreateItem("ArmorJewelry_Amulet", Str("itemSkillName", """records\test\hidden.dbr"""));
		var missing = this.CreateItem("ArmorJewelry_Bracelet", Str("itemSkillName", """records\test\missing.dbr"""));

		this._provider.HasGrantedSkill(plain).Should().BeFalse();
		this._provider.HasGrantedSkill(hidden).Should().BeFalse();
		this._provider.HasGrantedSkill(missing).Should().BeFalse();
	}

	[Fact]
	public void GetGrantedSkillNames_NamesEachShownSkillOnce_FromTheItemAndItsRelic()
	{
		this._translations["tagSkillOther"] = "{^I}Other Skill";
		this.CreateRecord("""records\test\skill.dbr""", "Skill_Passive", Str("skillDisplayName", "tagSkillTest"));
		this.CreateRecord("""records\test\other.dbr""", "Skill_Passive", Str("skillDisplayName", "tagSkillOther"));
		this.CreateRecord("""records\test\hidden.dbr""", "Skill_Passive", Ints("characterLife", 10));
		this.CreateRecord("""records\test\relic.dbr""", "ItemRelic", Str("itemSkillName", """records\test\other.dbr"""));
		this.CreateRecord("""records\test\bonus.dbr""", "ItemRelic", Str("itemSkillName", """records\test\skill.dbr"""));
		this.CreateRecord("""records\test\prefix.dbr""", "LootRandomizer", Str("itemSkillName", """records\test\hidden.dbr"""));
		var item = this.CreateItem("ArmorJewelry_Amulet", Str("itemSkillName", """records\test\skill.dbr"""));
		item.relicID = RecordId.Create("""records\test\relic.dbr""");
		item.RelicBonusId = RecordId.Create("""records\test\bonus.dbr""");
		item.prefixID = RecordId.Create("""records\test\prefix.dbr""");

		this._provider.GetGrantedSkillNames(item).Should().Equal("Test Skill", "Other Skill");
	}

	private GrantedSkill[] GrantedSkills(Item item)
		=> this._provider.GetFriendlyNames(item, FriendlyNamesExtraScopes.BaseAttributes | FriendlyNamesExtraScopes.GrantedSkills).GrantedSkills;

	private void CreateRecord(string id, string recordClass, params Variable[] variables)
	{
		var recordId = RecordId.Create(id);
		var record = new DBRecordCollection(recordId, recordClass);
		record.Set(Str("Class", recordClass));
		foreach (var variable in variables)
			record.Set(variable);

		this._mockDatabase.Setup(d => d.GetRecordFromFile(recordId)).Returns(record);
	}

	private static Variable Ints(string name, params int[] values)
	{
		var variable = new Variable(name, VariableDataType.Integer, values.Length);
		for (int i = 0; i < values.Length; i++)
			variable[i] = values[i];
		return variable;
	}

	private string[] BaseAttributes(Item item, bool filterExtra = true)
	{
		ToFriendlyNameResult result = this._provider.GetFriendlyNames(item, FriendlyNamesExtraScopes.BaseAttributes, filterExtra);
		return result.BaseAttributes;
	}

	private Item CreateItem(string itemClass, params Variable[] attributes)
	{
		var id = RecordId.Create($"""records\test\{itemClass}.dbr""");
		var record = new DBRecordCollection(id, itemClass);
		record.Set(Str("Class", itemClass));
		foreach (var attribute in attributes)
			record.Set(attribute);

		this._mockDatabase.Setup(d => d.GetRecordFromFile(id)).Returns(record);

		return new Item
		{
			BaseItemId = id,
			baseItemInfo = new Info(record),
		};
	}

	private static Variable Int(string name, int value)
	{
		var variable = new Variable(name, VariableDataType.Integer, 1);
		variable[0] = value;
		return variable;
	}

	private static Variable Float(string name, float value)
	{
		var variable = new Variable(name, VariableDataType.Float, 1);
		variable[0] = value;
		return variable;
	}

	private static Variable Str(string name, string value)
	{
		var variable = new Variable(name, VariableDataType.StringVar, 1);
		variable[0] = value;
		return variable;
	}
}
