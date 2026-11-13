using AwesomeAssertions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Tests.Data;

/// <summary>
/// Tests how the amounts, durations, chances and modifiers of an attribute group are formatted into one tooltip line.
/// </summary>
public class ItemProviderAttributeAmountsTests : ItemProviderTextTestBase
{
	[Fact]
	public void InfluenceRange_WithChance_UsesTheSecondsFormat_AfterTheChance()
	{
		var item = this.CreateItem("ArmorJewelry_Ring",
			Float("offensiveStunMin", 1f), Float("offensiveStunMax", 2f), Float("offensiveStunChance", 10f));

		this.BaseAttributes(item).Should().Equal($"{Epic}10.0% Chance of 1.0 ~ 2.0 Stun");
	}

	[Fact]
	public void DamageOverTimeRange_MultipliesBothBoundsByTheDuration()
	{
		var item = this.CreateItem("ArmorJewelry_Ring",
			Float("offensiveSlowFireMin", 5f), Float("offensiveSlowFireMax", 10f), Float("offensiveSlowFireDurationMin", 3f));

		this.BaseAttributes(item).Should().Equal($"{Epic}15 ~ 30 Burn Damage over 3.0 Seconds");
	}

	[Fact]
	public void DurationIndependentEffect_IsNotMultipliedByTheDuration()
	{
		// A slow is a percentage for the whole duration, not a damage per second.
		var item = this.CreateItem("ArmorJewelry_Ring",
			Float("offensiveSlowTotalSpeedMin", 30f), Float("offensiveSlowTotalSpeedDurationMin", 3f));

		this.BaseAttributes(item).Should().Equal($"{Epic}30% Slowed for 3.0 Seconds");
	}

	[Fact]
	public void PercentLabel_IsJoinedToTheAmount_AfterTheChance()
	{
		// The game shows "30% Slowed", not "30 % Slowed".
		var item = this.CreateItem("ArmorJewelry_Ring",
			Float("offensiveSlowTotalSpeedMin", 30f), Float("offensiveSlowTotalSpeedDurationMin", 3f), Float("offensiveSlowTotalSpeedChance", 20f));

		this.BaseAttributes(item).Should().Equal($"{Epic}20.0% Chance of 30% Slowed for 3.0 Seconds");
	}

	[Fact]
	public void PercentLabel_OfAnInstantEffect_IsJoinedToTheAmount()
	{
		this.Translations["DamageLifeLeech"] = "% of Attack damage converted to Health";
		var item = this.CreateItem("ArmorJewelry_Ring", Float("offensiveLifeLeechMin", 15f));

		this.BaseAttributes(item).Should().Equal($"{Epic}15% of Attack damage converted to Health");
	}

	[Fact]
	public void DurationIndependentRange_IsNotMultipliedByTheDuration()
	{
		var item = this.CreateItem("ArmorJewelry_Ring",
			Float("offensiveSlowTotalSpeedMin", 20f), Float("offensiveSlowTotalSpeedMax", 30f), Float("offensiveSlowTotalSpeedDurationMin", 3f));

		this.BaseAttributes(item).Should().Equal($"{Epic}20 ~ 30% Slowed for 3.0 Seconds");
	}

	[Theory]
	[InlineData("offensiveFumble")]
	[InlineData("offensiveProjectileFumble")]
	[InlineData("retaliationSlowRunSpeed")]
	[InlineData("retaliationSlowOffensiveAbility")]
	public void EffectAppliedForATime_IsNotMultiplied_AndLastsFor(string effect)
	{
		// Slam: "70% Chance of Impaired Aim for 3.4 Seconds", not 70 x 4 = 280% "over" the time. The game keeps only the
		// min duration: it loads the max but never uses it.
		var item = this.CreateItem("ArmorJewelry_Ring",
			Float($"{effect}Min", 70f), Float($"{effect}DurationMin", 3.4f), Float($"{effect}DurationMax", 5.1f));

		this.BaseAttributes(item).Should().ContainSingle().Which.Should().StartWith($"{Epic}70").And.EndWith(" for 3.4 Seconds");
	}

	[Fact]
	public void DamageOverTime_LastsOver_AndRetaliationTotalSpeedToo()
	{
		var item = this.CreateItem("ArmorJewelry_Ring",
			Float("retaliationSlowTotalSpeedMin", 10f), Float("retaliationSlowTotalSpeedDurationMin", 2f));

		this.BaseAttributes(item).Should().ContainSingle().Which.Should().EndWith(" over 2.0 Seconds");
	}

	[Fact]
	public void DifferentDurationBounds_ShowOnlyTheMinimumDuration_WhichTheAmountUses()
	{
		var item = this.CreateItem("ArmorJewelry_Ring",
			Float("offensiveSlowFireMin", 5f), Float("offensiveSlowFireDurationMin", 2f), Float("offensiveSlowFireDurationMax", 4f));

		this.BaseAttributes(item).Should().Equal($"{Epic}10 Burn Damage over 2.0 Seconds");
	}

	[Fact]
	public void EqualDurationBounds_ShowASingleDuration()
	{
		var item = this.CreateItem("ArmorJewelry_Ring",
			Float("offensiveSlowFireMin", 5f), Float("offensiveSlowFireDurationMin", 2f), Float("offensiveSlowFireDurationMax", 2f));

		this.BaseAttributes(item).Should().Equal($"{Epic}10 Burn Damage over 2.0 Seconds");
	}

	[Fact]
	public void ItemScalePercent_ScalesFloatAmounts_ButNotIntegers()
	{
		var floatItem = this.CreateItem("ArmorJewelry_Ring", Float("characterStrength", 10f));
		floatItem.itemScalePercent = 1.5f;
		var intItem = this.CreateItem("ArmorJewelry_Amulet", Int("characterStrength", 10));
		intItem.itemScalePercent = 1.5f;

		this.BaseAttributes(floatItem).Should().Equal($"{Epic}+15 Strength");
		this.BaseAttributes(intItem).Should().Equal($"{Epic}+10 Strength");
	}

	[Fact]
	public void ItemScalePercent_ScalesBothBoundsOfARange()
	{
		var item = this.CreateItem("ArmorJewelry_Ring", Float("offensiveFireMin", 10f), Float("offensiveFireMax", 20f));
		item.itemScalePercent = 1.5f;

		this.BaseAttributes(item).Should().Equal($"{Epic}15 ~ 30 Fire Damage");
	}

	[Fact]
	public void AttributeScalePercent_IsHidden_AndRaisesTheItemScale()
	{
		var item = this.CreateItem("ArmorJewelry_Ring", Float("attributeScalePercent", 20f));

		var lines = this.BaseAttributes(item);

		lines.Should().BeEmpty();
		item.itemScalePercent.Should().BeApproximately(1.2f, 0.0001f);
	}

	[Fact]
	public void AttributeScalePercent_IsNotAppliedTwice_NorWithInGameValues()
	{
		var item = this.CreateItem("ArmorJewelry_Ring", Float("attributeScalePercent", 20f));
		item.itemScalePercent = 1.5f; // already scaled by an earlier display

		this.BaseAttributes(item);
		item.itemScalePercent.Should().Be(1.5f);

		this.Settings.ShowInGameValues = true;
		var inGame = this.CreateItem("ArmorJewelry_Amulet", Float("attributeScalePercent", 20f));
		this.BaseAttributes(inGame);
		inGame.itemScalePercent.Should().Be(1.0f, "the roll simulator already applies the scale to the in-game values");
	}

	[Fact]
	public void DrainWithDamageRatio_ShowsTheRatioAfterTheLabel()
	{
		var item = this.CreateItem("ArmorJewelry_Ring",
			Float("offensiveManaBurnDrainMin", 10f), Float("offensiveManaBurnDrainMax", 20f), Float("offensiveManaBurnDamageRatio", 50f));

		this.BaseAttributes(item).Should().Equal($"{Epic}10 ~ 20% Energy Drain (50% of lost Energy as Damage)");
	}

	[Fact]
	public void DamageRatio_WithoutTranslation_ShowsThePercentAndTheAttributeName()
	{
		this.Translations.Remove("DamageManaBurnRatio");
		var item = this.CreateItem("ArmorJewelry_Ring",
			Float("offensiveManaBurnDrainMin", 10f), Float("offensiveManaBurnDamageRatio", 50f));

		this.BaseAttributes(item).Should().Equal($"{Epic}10% Energy Drain {Legendary}50.0% ?offensiveManaBurnDamageRatio?");
	}

	[Fact]
	public void SlowModifiers_MakeOneLine_ChanceThenModifierThenImprovedDuration()
	{
		var item = this.CreateItem("ArmorJewelry_Ring",
			Float("offensiveSlowFireModifier", 20f),
			Float("offensiveSlowFireDurationModifier", 50f),
			Float("offensiveSlowFireModifierChance", 30f));

		this.BaseAttributes(item).Should().Equal($"{Epic}30.0% Chance of +20% Burn Damage with +50% Improved Duration");
	}

	[Fact]
	public void DurationModifierWithoutModifier_IsShownAsARawAttribute()
	{
		var item = this.CreateItem("ArmorJewelry_Ring", Float("offensiveSlowFireDurationModifier", 50f));

		var lines = this.BaseAttributes(item);

		lines.Should().ContainSingle().Which.Should().Contain("DurationModifier");
	}

	[Fact]
	public void SlowModifier_WithoutTranslations_FallsBackToTheTagsBetweenQuestionMarks()
	{
		this.Translations.Remove("DamageDurationModifierFire");
		// Else the modifier reads from the effect's name (ModifierWithoutGameText_ReadsFromTheEffectName).
		this.Translations.Remove("DamageDurationFire");
		this.Translations.Remove("ImprovedTimeFormat");
		this.Translations.Remove("ChanceOfTag");
		var item = this.CreateItem("ArmorJewelry_Ring",
			Float("offensiveSlowFireModifier", 20f),
			Float("offensiveSlowFireDurationModifier", 50f),
			Float("offensiveSlowFireModifierChance", 30f));

		this.BaseAttributes(item).Should().Equal(
			$"{Legendary}?30.0% Chance of? {Legendary}20.0% ?offensiveSlowFireModifier? {Legendary}?with 50% Improved Duration?");
	}

	[Fact]
	public void MissingRangeFormat_FallsBackToDotDot_InTheLegendaryColor()
	{
		this.Translations.Remove("DamageRangeFormat");
		var item = this.CreateItem("ArmorJewelry_Ring", Float("offensiveFireMin", 10f), Float("offensiveFireMax", 20f));

		this.BaseAttributes(item).Should().Equal($"{Legendary}10..20 Fire Damage");
	}

	[Fact]
	public void MissingSingleFormat_FallsBackToTheBareValue_InTheLegendaryColor()
	{
		this.Translations.Remove("DamageSingleFormat");
		var item = this.CreateItem("ArmorJewelry_Ring", Float("offensiveFireMin", 10f));

		this.BaseAttributes(item).Should().Equal($"{Legendary}10 Fire Damage");
	}

	[Fact]
	public void MissingDurationFormats_FallBackToEnglish()
	{
		this.Translations.Remove("DamageSingleFormatTime");
		this.Translations.Remove("DamageRangeFormatTime");
		var single = this.CreateItem("ArmorJewelry_Ring", Float("offensiveSlowFireMin", 5f), Float("offensiveSlowFireDurationMin", 2f));
		var range = this.CreateItem("ArmorJewelry_Amulet",
			Float("offensiveSlowFireMin", 5f), Float("offensiveSlowFireDurationMin", 2f), Float("offensiveSlowFireDurationMax", 4f));

		this.BaseAttributes(single).Should().Equal($"{Epic}10 Burn Damage {Legendary}2");
		this.BaseAttributes(range).Should().Equal($"{Epic}10 Burn Damage {Legendary}2");
	}

	[Fact]
	public void LabelFormatWithMinusSign_DropsTheSign_ForAPositiveValue()
	{
		this.Translations["characterLife"] = "{%-.0f0} Health";
		var item = this.CreateItem("ArmorJewelry_Ring", Float("characterLife", 40f));

		this.BaseAttributes(item).Should().Equal($"{Epic}40 Health");
	}

	[Fact]
	public void ArmorAbsorption_AndItsChance_AreNotShown()
	{
		// The game shows no armor absorption and has no text for it: its chance alone made a raw "ATTR<...>" line.
		var item = this.CreateItem("ArmorProtective_LowerBody",
			Float("defensiveAbsorption", 250f), Float("defensiveAbsorptionChance", 15f));

		this.BaseAttributes(item).Should().BeEmpty();
	}

	[Fact]
	public void ModifierWithoutGameText_ReadsFromTheEffectName()
	{
		this.Translations["DamageDurationDefensiveReduction"] = "Reduced Armor";
		this.Translations.Remove("DamageDurationModifierDefensiveReduction");
		var item = this.CreateItem("WeaponMelee_Sword",
			Float("offensiveSlowDefensiveReductionModifier", 200f), Float("offensiveSlowDefensiveReductionModifierChance", 20f));

		this.BaseAttributes(item).Should().ContainSingle().Which.Should().EndWith("+200% Reduced Armor").And.NotContain("?");
	}

	[Fact]
	public void LabelFormatWithPlusSign_WritesOneSign_ForANegativeValue()
	{
		this.Translations["characterLife"] = "{%+.0f0} Health";
		var item = this.CreateItem("ArmorJewelry_Ring", Float("characterLife", -40f));

		this.BaseAttributes(item).Should().Equal($"{Epic}-40 Health");
	}

	[Fact]
	public void RelicLine_UsesTheValueOfItsNumberOfShards()
	{
		var relic = this.CreateItemWithId("""records\item\relics\test_relic.dbr""", "ItemRelic", Ints("characterStrength", 5, 10, 15), Int("completedRelicLevel", 3));
		relic.Var1 = 2;

		this.BaseAttributes(relic).Should().Equal($"{Epic}+10 Strength");
	}

	[Fact]
	public void SocketedRelic_UsesTheValueOfItsShardCount_InTheItem()
	{
		var item = this.CreateItem("WeaponMelee_Sword");
		this.CreateRecord("""records\item\relics\test_relic.dbr""", "ItemRelic", Ints("characterStrength", 5, 10, 15));
		item.relicID = RecordId.Create("""records\item\relics\test_relic.dbr""");
		item.RelicInfo = this.MockDatabase.Object.GetInfo(item.relicID);
		item.Var1 = 3;

		var relicLines = this.Provider.GetFriendlyNames(item, FriendlyNamesExtraScopes.RelicAttributes).Relic1Attributes;

		relicLines.Should().Equal($"{Epic}+15 Strength");
	}

	[Fact]
	public void ValueIndexPastTheLastValue_UsesTheLastValue()
	{
		var relic = this.CreateItemWithId("""records\item\relics\test_relic.dbr""", "ItemRelic", Ints("characterStrength", 7), Int("completedRelicLevel", 3));
		relic.Var1 = 3;

		this.BaseAttributes(relic).Should().Equal($"{Epic}+7 Strength");
	}
}
